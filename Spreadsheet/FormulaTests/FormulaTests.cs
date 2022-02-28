using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpreadsheetUtilities;

namespace FormulaTests
{
    [TestClass]
    public class GradingTests
    {

        // Normalizer tests
        [TestMethod(), Timeout(2000)]
        [TestCategory("1")]
        public void TestNormalizerGetVars()
        {
            Formula f = new Formula("2+x1", s => s.ToUpper(), s => true);
            HashSet<string> vars = new HashSet<string>(f.GetVariables());

            Assert.IsTrue(vars.SetEquals(new HashSet<string> { "X1" }));
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("2")]
        public void TestNormalizerEquals()
        {
            Formula f = new Formula("2+x1", s => s.ToUpper(), s => true);
            Formula f2 = new Formula("2+X1", s => s.ToUpper(), s => true);

            Assert.IsTrue(f.Equals(f2));
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("3")]
        public void TestNormalizerToString()
        {
            Formula f = new Formula("2+x1", s => s.ToUpper(), s => true);
            Formula f2 = new Formula(f.ToString());

            Assert.IsTrue(f.Equals(f2));
        }

        // Validator tests
        [TestMethod(), Timeout(2000)]
        [TestCategory("4")]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestValidatorFalse()
        {
            Formula f = new Formula("2+x1", s => s, s => false);
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("5")]
        public void TestValidatorX1()
        {
            Formula f = new Formula("2+x", s => s, s => (s == "x"));
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("6")]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestValidatorX2()
        {
            Formula f = new Formula("2+y1", s => s, s => (s == "x"));
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("7")]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestValidatorX3()
        {
            Formula f = new Formula("2+x1", s => s, s => (s == "x"));
        }


        // Simple tests that return FormulaErrors
        [TestMethod(), Timeout(2000)]
        [TestCategory("8")]
        public void TestUnknownVariable()
        {
            Formula f = new Formula("2+X1");
            Assert.IsInstanceOfType(f.Evaluate(s => { throw new ArgumentException("Unknown variable"); }), typeof(FormulaError));
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("9")]
        public void TestDivideByZero()
        {
            Formula f = new Formula("5/0");
            Assert.IsInstanceOfType(f.Evaluate(s => 0), typeof(FormulaError));
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("10")]
        public void TestDivideByZeroVars()
        {
            Formula f = new Formula("(5 + X1) / (X1 - 3)");
            Assert.IsInstanceOfType(f.Evaluate(s => 3), typeof(FormulaError));
        }


        // Tests of syntax errors detected by the constructor
        [TestMethod(), Timeout(2000)]
        [TestCategory("11")]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestSingleOperator()
        {
            Formula f = new Formula("+");
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("12")]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestExtraOperator()
        {
            Formula f = new Formula("2+5+");
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("13")]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestExtraCloseParen()
        {
            Formula f = new Formula("2+5*7)");
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("14")]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestExtraOpenParen()
        {
            Formula f = new Formula("((3+5*7)");
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("15")]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestNoOperator()
        {
            Formula f = new Formula("5x");
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("16")]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestNoOperator2()
        {
            Formula f = new Formula("5+5x");
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("17")]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestNoOperator3()
        {
            Formula f = new Formula("5+7+(5)8");
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("18")]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestNoOperator4()
        {
            Formula f = new Formula("5 5");
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("19")]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestDoubleOperator()
        {
            Formula f = new Formula("5 + + 3");
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("20")]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestEmpty()
        {
            Formula f = new Formula("");
        }

        // Some more complicated formula evaluations
        [TestMethod(), Timeout(2000)]
        [TestCategory("21")]
        public void TestComplex1()
        {
            Formula f = new Formula("y1*3-8/2+4*(8-9*2)/14*x7");
            Assert.AreEqual(5.14285714285714, (double)f.Evaluate(s => (s == "x7") ? 1 : 4), 1e-9);
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("22")]
        public void TestRightParens()
        {
            Formula f = new Formula("x1+(x2+(x3+(x4+(x5+x6))))");
            Assert.AreEqual(6, (double)f.Evaluate(s => 1), 1e-9);
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("23")]
        public void TestLeftParens()
        {
            Formula f = new Formula("((((x1+x2)+x3)+x4)+x5)+x6");
            Assert.AreEqual(12, (double)f.Evaluate(s => 2), 1e-9);
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("53")]
        public void TestRepeatedVar()
        {
            Formula f = new Formula("a4-a4*a4/a4");
            Assert.AreEqual(0, (double)f.Evaluate(s => 3), 1e-9);
        }

        // Test of the Equals method
        [TestMethod(), Timeout(2000)]
        [TestCategory("24")]
        public void TestEqualsBasic()
        {
            Formula f1 = new Formula("X1+X2");
            Formula f2 = new Formula("X1+X2");
            Assert.IsTrue(f1.Equals(f2));
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("25")]
        public void TestEqualsWhitespace()
        {
            Formula f1 = new Formula("X1+X2");
            Formula f2 = new Formula(" X1  +  X2   ");
            Assert.IsTrue(f1.Equals(f2));
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("26")]
        public void TestEqualsDouble()
        {
            Formula f1 = new Formula("2+X1*3.00");
            Formula f2 = new Formula("2.00+X1*3.0");
            Assert.IsTrue(f1.Equals(f2));
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("27")]
        public void TestEqualsComplex()
        {
            Formula f1 = new Formula("1e-2 + X5 + 17.00 * 19 ");
            Formula f2 = new Formula("   0.0100  +     X5+ 17 * 19.00000 ");
            Assert.IsTrue(f1.Equals(f2));
        }


        [TestMethod(), Timeout(2000)]
        [TestCategory("28")]
        public void TestEqualsNullAndString()
        {
            Formula f = new Formula("2");
            Assert.IsFalse(f.Equals(null));
            Assert.IsFalse(f.Equals(""));
        }


        // Tests of == operator
        [TestMethod(), Timeout(2000)]
        [TestCategory("29")]
        public void TestEq()
        {
            Formula f1 = new Formula("2");
            Formula f2 = new Formula("2");
            Assert.IsTrue(f1 == f2);
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("30")]
        public void TestEqFalse()
        {
            Formula f1 = new Formula("2");
            Formula f2 = new Formula("5");
            Assert.IsFalse(f1 == f2);
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("31")]
        public void TestEqNull()
        {
            Formula f1 = new Formula("2");
            Formula f2 = new Formula("2");
            Assert.IsFalse(null == f1);
            Assert.IsFalse(f1 == null);
            Assert.IsTrue(f1 == f2);
        }


        // Tests of != operator
        [TestMethod(), Timeout(2000)]
        [TestCategory("32")]
        public void TestNotEq()
        {
            Formula f1 = new Formula("2");
            Formula f2 = new Formula("2");
            Assert.IsFalse(f1 != f2);
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("33")]
        public void TestNotEqTrue()
        {
            Formula f1 = new Formula("2");
            Formula f2 = new Formula("5");
            Assert.IsTrue(f1 != f2);
        }


        // Test of ToString method
        [TestMethod(), Timeout(2000)]
        [TestCategory("34")]
        public void TestString()
        {
            Formula f = new Formula("2*5");
            Assert.IsTrue(f.Equals(new Formula(f.ToString())));
        }


        // Tests of GetHashCode method
        [TestMethod(), Timeout(2000)]
        [TestCategory("35")]
        public void TestHashCode()
        {
            Formula f1 = new Formula("2*5");
            Formula f2 = new Formula("2*5");
            Assert.IsTrue(f1.GetHashCode() == f2.GetHashCode());
        }

        // Technically the hashcodes could not be equal and still be valid,
        // extremely unlikely though. Check their implementation if this fails.
        [TestMethod(), Timeout(2000)]
        [TestCategory("36")]
        public void TestHashCodeFalse()
        {
            Formula f1 = new Formula("2*5");
            Formula f2 = new Formula("3/8*2+(7)");
            Assert.IsTrue(f1.GetHashCode() != f2.GetHashCode());
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("37")]
        public void TestHashCodeComplex()
        {
            Formula f1 = new Formula("2 * 5 + 4.00 - _x");
            Formula f2 = new Formula("2*5+4-_x");
            Assert.IsTrue(f1.GetHashCode() == f2.GetHashCode());
        }


        // Tests of GetVariables method
        [TestMethod(), Timeout(2000)]
        [TestCategory("38")]
        public void TestVarsNone()
        {
            Formula f = new Formula("2*5");
            Assert.IsFalse(f.GetVariables().GetEnumerator().MoveNext());
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("39")]
        public void TestVarsSimple()
        {
            Formula f = new Formula("2*X2");
            List<string> actual = new List<string>(f.GetVariables());
            HashSet<string> expected = new HashSet<string>() { "X2" };
            Assert.AreEqual(actual.Count, 1);
            Assert.IsTrue(expected.SetEquals(actual));
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("40")]
        public void TestVarsTwo()
        {
            Formula f = new Formula("2*X2+Y3");
            List<string> actual = new List<string>(f.GetVariables());
            HashSet<string> expected = new HashSet<string>() { "Y3", "X2" };
            Assert.AreEqual(actual.Count, 2);
            Assert.IsTrue(expected.SetEquals(actual));
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("41")]
        public void TestVarsDuplicate()
        {
            Formula f = new Formula("2*X2+X2");
            List<string> actual = new List<string>(f.GetVariables());
            HashSet<string> expected = new HashSet<string>() { "X2" };
            Assert.AreEqual(actual.Count, 1);
            Assert.IsTrue(expected.SetEquals(actual));
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("42")]
        public void TestVarsComplex()
        {
            Formula f = new Formula("X1+Y2*X3*Y2+Z7+X1/Z8");
            List<string> actual = new List<string>(f.GetVariables());
            HashSet<string> expected = new HashSet<string>() { "X1", "Y2", "X3", "Z7", "Z8" };
            Assert.AreEqual(actual.Count, 5);
            Assert.IsTrue(expected.SetEquals(actual));
        }

        // Tests to make sure there can be more than one formula at a time
        [TestMethod(), Timeout(2000)]
        [TestCategory("43")]
        public void TestMultipleFormulae()
        {
            Formula f1 = new Formula("2 + a1");
            Formula f2 = new Formula("3");
            Assert.AreEqual(2.0, f1.Evaluate(x => 0));
            Assert.AreEqual(3.0, f2.Evaluate(x => 0));
            Assert.IsFalse(new Formula(f1.ToString()) == new Formula(f2.ToString()));
            IEnumerator<string> f1Vars = f1.GetVariables().GetEnumerator();
            IEnumerator<string> f2Vars = f2.GetVariables().GetEnumerator();
            Assert.IsFalse(f2Vars.MoveNext());
            Assert.IsTrue(f1Vars.MoveNext());
        }

        // Repeat this test to increase its weight
        [TestMethod(), Timeout(2000)]
        [TestCategory("44")]
        public void TestMultipleFormulaeB()
        {
            TestMultipleFormulae();
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("45")]
        public void TestMultipleFormulaeC()
        {
            TestMultipleFormulae();
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("46")]
        public void TestMultipleFormulaeD()
        {
            TestMultipleFormulae();
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("47")]
        public void TestMultipleFormulaeE()
        {
            TestMultipleFormulae();
        }

        // Stress test for constructor
        [TestMethod(), Timeout(2000)]
        [TestCategory("48")]
        public void TestConstructor()
        {
            Formula f = new Formula("(((((2+3*X1)/(7e-5+X2-X4))*X5+.0005e+92)-8.2)*3.14159) * ((x2+3.1)-.00000000008)");
        }

        // This test is repeated to increase its weight
        [TestMethod(), Timeout(2000)]
        [TestCategory("49")]
        public void TestConstructorB()
        {
            Formula f = new Formula("(((((2+3*X1)/(7e-5+X2-X4))*X5+.0005e+92)-8.2)*3.14159) * ((x2+3.1)-.00000000008)");
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("50")]
        public void TestConstructorC()
        {
            Formula f = new Formula("(((((2+3*X1)/(7e-5+X2-X4))*X5+.0005e+92)-8.2)*3.14159) * ((x2+3.1)-.00000000008)");
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("51")]
        public void TestConstructorD()
        {
            Formula f = new Formula("(((((2+3*X1)/(7e-5+X2-X4))*X5+.0005e+92)-8.2)*3.14159) * ((x2+3.1)-.00000000008)");
        }

        // Stress test for constructor
        [TestMethod(), Timeout(2000)]
        [TestCategory("52")]
        public void TestConstructorE()
        {
            Formula f = new Formula("(((((2+3*X1)/(7e-5+X2-X4))*X5+.0005e+92)-8.2)*3.14159) * ((x2+3.1)-.00000000008)");
        }




        //[TestClass]
        //public class FormulaTests
        //{

        //    static double lookup(string s)
        //    {
        //        return 0;
        //    }

        //    [TestMethod]
        //    [ExpectedException(typeof(FormulaFormatException))]
        //    public void ParsingTestSingleOperator()
        //    {
        //        Formula f = new Formula("+");
        //    }

        //    [TestMethod]
        //    [ExpectedException(typeof(FormulaFormatException))]
        //    public void TestNegativeNumber()
        //    {
        //        Formula f = new Formula("-1");
        //    }

        //    [TestMethod]
        //    [ExpectedException(typeof(FormulaFormatException))]
        //    public void TestNegativeNumberArithmetic()
        //    {
        //        Formula f = new Formula("1 + -1");
        //    }

        //    [TestMethod]
        //    [ExpectedException(typeof(FormulaFormatException))]
        //    public void TestInvalidVar()
        //    {
        //        Formula f = new Formula("x+");
        //    }

        //    [TestMethod]
        //    [ExpectedException(typeof(FormulaFormatException))]
        //    public void TestInvalidVarAddition()
        //    {
        //        Formula f = new Formula("_+ + _/");
        //    }

        //    [TestMethod]
        //    [ExpectedException(typeof(FormulaFormatException))]
        //    public void ParsingTestExtraOperator()
        //    {
        //        Formula f = new Formula("2+5+");
        //    }

        //    [TestMethod]
        //    [ExpectedException(typeof(FormulaFormatException))]
        //    public void ParsingTestExtraParen()
        //    {
        //        Formula f = new Formula("2+5*7)");
        //    }

        //    [TestMethod]
        //    [ExpectedException(typeof(FormulaFormatException))]
        //    public void ParsingTestNoOperator()
        //    {
        //        Formula f = new Formula("5+7+(5)8");
        //    }


        //    [TestMethod]
        //    [ExpectedException(typeof(FormulaFormatException))]
        //    public void ParsingTestException()
        //    {
        //        Formula f = new Formula("%1 + 2");
        //    }

        //    [TestMethod]
        //    [ExpectedException(typeof(FormulaFormatException))]
        //    public void EmptyTokenTestException()
        //    {
        //        Formula f = new Formula("");
        //    }

        //    [TestMethod]
        //    [ExpectedException(typeof(FormulaFormatException))]
        //    public void RightParenthesesTestException()
        //    {
        //        Formula f = new Formula("))");
        //    }

        //    [TestMethod]
        //    [ExpectedException(typeof(FormulaFormatException))]
        //    public void RightParenthesesTestException2()
        //    {
        //        Formula f = new Formula("())");
        //    }

        //    [TestMethod]
        //    [ExpectedException(typeof(FormulaFormatException))]
        //    public void BalancedParenthesesTestException()
        //    {
        //        Formula f = new Formula("((((())))");
        //    }

        //    [TestMethod]
        //    [ExpectedException(typeof(FormulaFormatException))]
        //    public void StartingTokenTestException()
        //    {
        //        Formula f = new Formula("*");
        //    }

        //    [TestMethod]
        //    [ExpectedException(typeof(FormulaFormatException))]
        //    public void StartingTokenTestException2()
        //    {
        //        Formula f = new Formula("/20 * 20");
        //    }

        //    [TestMethod]
        //    [ExpectedException(typeof(FormulaFormatException))]
        //    public void EndingTokenTestException()
        //    {
        //        Formula f = new Formula("20 + 20(");
        //    }

        //    [TestMethod]
        //    [ExpectedException(typeof(FormulaFormatException))]
        //    public void ParenthesesOperatorFollowingTestException()
        //    {
        //        Formula f = new Formula("(+20)");
        //    }

        //    [TestMethod]
        //    [ExpectedException(typeof(FormulaFormatException))]
        //    public void ParenthesesOperatorFollowingTestException2()
        //    {
        //        Formula f = new Formula("(20 + +)");
        //    }

        //    [TestMethod]
        //    [ExpectedException(typeof(FormulaFormatException))]
        //    public void ExtraFollowingTestException()
        //    {
        //        Formula f = new Formula("(20 + 20)5");
        //    }

        //    [TestMethod]

        //    public void BasicFormulaTest()
        //    {
        //        Formula f = new Formula("(20 + 20)");
        //        Assert.AreEqual((double)40, f.Evaluate( s => 0));
        //        IEnumerator<string> e1 = f.GetVariables().GetEnumerator();
        //        Assert.IsFalse(e1.MoveNext());

        //    }

        //    [TestMethod]

        //    public void BasicFormulaTest2()
        //    {
        //        Formula f = new Formula("(20 + 20) * 5");
        //        Assert.AreEqual((double)200, f.Evaluate(s => 0));
        //    }
        //    [TestMethod]
        //    [ExpectedException(typeof(FormulaFormatException))]
        //    public void BasicFormulaNewSpaces()
        //    {
        //        Formula f = new Formula("x 23");

        //    }


        //    [TestMethod]
        //    public void BasicFormulaTestSingleNumber()
        //    {
        //        Formula f = new Formula("5");
        //        Assert.AreEqual((double)5, f.Evaluate(s => 0));
        //    }

        //    [TestMethod]
        //    public void BasicFormulaTestAddition()
        //    {
        //        Formula f = new Formula("5+3");
        //        Assert.AreEqual((double)8, f.Evaluate(s => 0));
        //    }

        //    [TestMethod]
        //    public void BasicFormulaTestSubtraction()
        //    {
        //        Formula f = new Formula("18-10");
        //        Assert.AreEqual((double)8, f.Evaluate(s => 0));
        //    }

        //    [TestMethod]
        //    public void BasicFormulaTestMultiplication()
        //    {
        //        Formula f = new Formula("2*4");
        //        Assert.AreEqual((double)8, f.Evaluate(s => 0));
        //    }

        //    [TestMethod]
        //    public void BasicFormulaTestDivision()
        //    {
        //        Formula f = new Formula("6/3");
        //        Assert.AreEqual((double)2, f.Evaluate(s => 0));
        //    }

        //    [TestMethod]
        //    public void BasicFormulaTestLeftToRight()
        //    {
        //        Formula f = new Formula("2*6+3");
        //        Assert.AreEqual((double)15, f.Evaluate(s => 0));
        //    }

        //    [TestMethod]
        //    public void BasicFormulaTestOrderOperation()
        //    {
        //        Formula f = new Formula("2+6*3");
        //        Assert.AreEqual((double)20, f.Evaluate(s => 0));
        //    }

        //    [TestMethod]
        //    public void BasicFormulaTestParenthesisTimes()
        //    {
        //        Formula f = new Formula("(2+6)*3");
        //        Assert.AreEqual((double)24, f.Evaluate(s => 0));
        //    }

        //    [TestMethod]
        //    public void BasicFormulaTestTimesParenthesis()
        //    {
        //        Formula f = new Formula("2*(3+5)");
        //        Assert.AreEqual((double)16, f.Evaluate(s => 0));
        //    }

        //    [TestMethod]
        //    public void BasicFormulaTestSameTwoHashCodes()
        //    {
        //        Formula f = new Formula("2*(3+5)");
        //        Formula f2 = new Formula("2*(3+5)");
        //        Assert.AreEqual(f.GetHashCode(), f2.GetHashCode());
        //    }

        //    [TestMethod]
        //    public void BasicFormulaTestDifferentHashCodes()
        //    {
        //        Formula f = new Formula("2*(3+5)");
        //        Formula f2 = new Formula("2*(3+55)");
        //        Assert.AreNotEqual(f.GetHashCode(), f2.GetHashCode());
        //    }

        //    [TestMethod]
        //    public void BasicFormulaTestEqualsOp()
        //    {
        //        Formula f = new Formula("2*(3+5)");
        //        Formula f2 = new Formula("2*(3+5)");
        //        Assert.IsTrue(f == f2);
        //    }

        //    [TestMethod]
        //    public void BasicFormulaTestEqualsOpFalse()
        //    {
        //        Formula f = new Formula("2*(3+5)");
        //        Formula f2 = new Formula("2*(3+55)");
        //        Assert.IsFalse(f == f2);
        //    }

        //    [TestMethod]
        //    public void BasicFormulaTestNotEqualsOp()
        //    {
        //        Formula f = new Formula("2*(3+5)");
        //        Formula f2 = new Formula("2*(3+5)");
        //        Assert.IsFalse(f != f2);
        //    }

        //    [TestMethod]
        //    public void BasicFormulaTestNotEqualsOpTrue()
        //    {
        //        Formula f = new Formula("2*(3+5)");
        //        Formula f2 = new Formula("2*(3+5354)");
        //        Assert.IsTrue(f != f2);
        //    }

        //    [TestMethod]
        //    public void BasicFormulaTestEquals()
        //    {
        //        Formula f = new Formula("x1 + y2");
        //        Formula f2 = new Formula("x1+y2");
        //        Assert.IsTrue(f.Equals(f2));
        //    }

        //    [TestMethod]
        //    public void BasicFormulaTestEqualsDifferentObject()
        //    {
        //        Formula f = new Formula("x1 + y2");
        //        String f2 = "x1+y2";
        //        Assert.IsFalse(f.Equals(f2));
        //    }

        //    [TestMethod]
        //    public void BasicFormulaTestEqualsFalse()
        //    {
        //        Formula f = new Formula("x1 + y2");
        //        Formula f2 = new Formula("x1+y22");
        //        Assert.IsFalse(f.Equals(f2));
        //    }

        //    [TestMethod]
        //    [ExpectedException(typeof(FormulaFormatException))]
        //    public void BasicFormulaTestInvalidVar()
        //    {
        //        Formula f = new Formula("x1 + y2", s => s, s=> false);


        //    }

        //    [TestMethod]
        //    [ExpectedException(typeof(FormulaFormatException))]
        //    public void BasicFormulaTestLeftParen()
        //    {
        //        Formula f = new Formula("((x1 + y2");


        //    }

        //    [TestMethod]
        //    public void BasicFormulaTestEqualsDoubles()
        //    {
        //        Formula f = new Formula("x1 + 2.0");
        //        Formula f2 = new Formula("x1+2.0000");
        //        Assert.IsTrue(f.Equals(f2));
        //    }

        //    [TestMethod]
        //    public void BasicFormulaTestToString()
        //    {
        //        Formula f = new Formula("x1 + 2.0");
        //        Formula f2 = new Formula("x1+3");
        //        Assert.IsTrue(f.ToString().GetHashCode() != f2.ToString().GetHashCode());
        //    }


        //    [TestMethod]
        //    public void BasicFormulaTestPlusParenthesis()
        //    {
        //        Formula f = new Formula("(2+6)+3");
        //        Assert.AreEqual((double)11, f.Evaluate(s => 0));
        //    }
        //    [TestMethod]
        //    public void BasicFormulaTestPlusComplex()
        //    {
        //        Formula f = new Formula("2+(3+5*9)");
        //        Assert.AreEqual((double)50, f.Evaluate(s => 0));
        //    }
        //    [TestMethod]
        //    public void BasicFormulaTestOperatorAfterParen()
        //    {
        //        Formula f = new Formula("(1*1)-2/2");
        //        Assert.AreEqual((double)0, f.Evaluate(s => 0));
        //    }
        //    [TestMethod]
        //    public void BasicFormulaTestComplexTimesParentheses()
        //    {
        //        Formula f = new Formula("2+3*(3+5)");
        //        Assert.AreEqual((double)26, f.Evaluate(s => 0));
        //    }
        //    [TestMethod]
        //    public void BasicFormulaTestComplexAndParentheses()
        //    {
        //        Formula f = new Formula("2+3*5+(3+4*8)*5+2");
        //        Assert.AreEqual((double)194, f.Evaluate(s => 0));
        //    }

        //    [TestMethod]
        //    public void BasicFormulaTestNestedParensRight()
        //    {
        //        Formula f = new Formula("x1+(x2+(x3+(x4+(x5+x6))))");
        //        Assert.AreEqual((double)0, f.Evaluate(s => 0));
        //    }
        //    [TestMethod]
        //    public void BasicFormulaTestNestedParensLeft()
        //    {
        //        Formula f = new Formula("((((x1+x2)+x3)+x4)+x5)+x6");
        //        Assert.AreEqual((double)0, f.Evaluate(s => 0));
        //    }

        //    [TestMethod]
        //    public void BasicFormulaTestVar()
        //    {
        //        Formula f = new Formula("x5 + x5");
        //        Assert.AreEqual((double)0, f.Evaluate(s => 0));
        //        IEnumerator<string> e1 = f.GetVariables().GetEnumerator();
        //        Assert.IsTrue(e1.MoveNext());
        //        String s1 = e1.Current;
        //        Assert.IsTrue(s1 == "x5");
        //    }

        //    [TestMethod]
        //    public void BasicFormulaTestTwoVar()
        //    {
        //        Formula f = new Formula("x5 + x4");
        //        Assert.AreEqual((double)0, f.Evaluate(s => 0));
        //        IEnumerator<string> e1 = f.GetVariables().GetEnumerator();
        //        Assert.IsTrue(e1.MoveNext());
        //        String s1 = e1.Current;
        //        Assert.IsTrue(s1 == "x5");
        //        Assert.IsTrue(e1.MoveNext());
        //        s1 = e1.Current;
        //        Assert.IsTrue(s1 == "x4");
        //    }

        //    [TestMethod]
        //    public void BasicFormulaTestSingleVar()
        //    {
        //        Formula f = new Formula("X5");
        //        Assert.AreEqual((double)0, f.Evaluate(s => 0));
        //    }

        //    [TestMethod]
        //    public void BasicFormulaTestVarAddition()
        //    {
        //        Formula f = new Formula("2+x1");
        //        Assert.AreEqual((double)2, f.Evaluate(s => 0));
        //    }

        //    [TestMethod]
        //    public void BasicFormulaTestRepeatedVar()
        //    {
        //        Formula f = new Formula("a4-a4*a4/a4");
        //        Assert.AreEqual((double)0, f.Evaluate(s => 1));
        //        IEnumerator<string> e1 = f.GetVariables().GetEnumerator();
        //        Assert.IsTrue(e1.MoveNext());
        //        String s1 = e1.Current;
        //        Assert.IsTrue(s1 == "a4");

        //    }

        //    [TestMethod]
        //    public void BasicFormulaTestDoubleSubtraction()
        //    {
        //        Formula f = new Formula("(3.3 - 2.2 - 1.1)");
        //        Assert.AreEqual(0.0, (double)f.Evaluate(s => 0), 1e-9);
        //    }

        //    [TestMethod]
        //    public void BasicFormulaTestDivideByZero()
        //    {
        //        Formula f = new Formula("5 / (3.3 - 2.2 - 1.1)");
        //        Assert.IsInstanceOfType(f.Evaluate(x => 0), typeof(FormulaError));
        //    }

        //    [TestMethod]
        //    public void BasicFormulaTestDivideByZero2()
        //    {
        //        Formula f = new Formula("5 / 0");
        //        Assert.IsInstanceOfType(f.Evaluate(x => 0), typeof(FormulaError));
        //    }

        //    [TestMethod]
        //    public void BasicFormulaTestDivideByZeroRightParen()
        //    {
        //        Formula f = new Formula("(5 / 0)");
        //        Assert.IsInstanceOfType(f.Evaluate(x => 0), typeof(FormulaError));
        //    }

        //    [TestMethod]
        //    public void BasicFormulaTestDivideByZeroVar()
        //    {
        //        Formula f = new Formula("5 / a1");
        //        Assert.IsInstanceOfType(f.Evaluate(x => 0), typeof(FormulaError));
        //    }


        //    [TestMethod]
        //    public void BasicFormulaTestUnderscoreVar()
        //    {
        //        Formula f = new Formula("_1 + _1");
        //        Assert.AreEqual((double)2, f.Evaluate(s => 1));
        //    }



        //}
    }
}
