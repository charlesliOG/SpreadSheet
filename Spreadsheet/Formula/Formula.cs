// Skeleton written by Joe Zachary for CS 3500, September 2013
// Read the entire skeleton carefully and completely before you
// do anything else!

// Version 1.1 (9/22/13 11:45 a.m.)

// Change log:
//  (Version 1.1) Repaired mistake in GetTokens
//  (Version 1.1) Changed specification of second constructor to
//                clarify description of how validation works

// (Daniel Kopta) 
// Version 1.2 (9/10/17) 

// Change log:
//  (Version 1.2) Changed the definition of equality with regards
//                to numeric tokens


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SpreadsheetUtilities
{
    //Author: Charles Li, Fall 2021
    //University of Utah

    /// <summary>
    /// Represents formulas written in standard infix notation using standard precedence
    /// rules.  The allowed symbols are non-negative numbers written using double-precision 
    /// floating-point syntax (without unary preceeding '-' or '+'); 
    /// variables that consist of a letter or underscore followed by 
    /// zero or more letters, underscores, or digits; parentheses; and the four operator 
    /// symbols +, -, *, and /.  
    /// 
    /// Spaces are significant only insofar that they delimit tokens.  For example, "xy" is
    /// a single variable, "x y" consists of two variables "x" and y; "x23" is a single variable; 
    /// and "x 23" consists of a variable "x" and a number "23".
    /// 
    /// Associated with every formula are two delegates:  a normalizer and a validator.  The
    /// normalizer is used to convert variables into a canonical form, and the validator is used
    /// to add extra restrictions on the validity of a variable (beyond the standard requirement 
    /// that it consist of a letter or underscore followed by zero or more letters, underscores,
    /// or digits.)  Their use is described in detail in the constructor and method comments.
    /// </summary>
    public class Formula
    {

        private string formulaString;
        private List<string> normalizedVars;

        /// <summary>
        /// Creates a Formula from a string that consists of an infix expression written as
        /// described in the class comment.  If the expression is syntactically invalid,
        /// throws a FormulaFormatException with an explanatory Message.
        /// 
        /// The associated normalizer is the identity function, and the associated validator
        /// maps every string to true.  
        /// </summary>
        public Formula(String formula) :
            this(formula, s => s, s => true)
        {

        }

        /// <summary>
        /// Creates a Formula from a string that consists of an infix expression written as
        /// described in the class comment.  If the expression is syntactically incorrect,
        /// throws a FormulaFormatException with an explanatory Message.
        /// 
        /// The associated normalizer and validator are the second and third parameters,
        /// respectively.  
        /// 
        /// If the formula contains a variable v such that normalize(v) is not a legal variable, 
        /// throws a FormulaFormatException with an explanatory message. 
        /// 
        /// If the formula contains a variable v such that isValid(normalize(v)) is false,
        /// throws a FormulaFormatException with an explanatory message.
        /// 
        /// Suppose that N is a method that converts all the letters in a string to upper case, and
        /// that V is a method that returns true only if a string consists of one letter followed
        /// by one digit.  Then:
        /// 
        /// new Formula("x2+y3", N, V) should succeed
        /// new Formula("x+y3", N, V) should throw an exception, since V(N("x")) is false
        /// new Formula("2x+y3", N, V) should throw an exception, since "2x+y3" is syntactically incorrect.
        /// </summary>
        public Formula(String formula, Func<string, string> normalize, Func<string, bool> isValid)
        {
            //validSyntax is a helper method that sees if the formula is syntactically correct
            if (validSyntax(formula, normalize, isValid))
            {
                //normalized string is a helper that returns the normalized version of the original string
                formulaString = normalizedString(formula, normalize);
                //getNormalizedVars is a helper that returns all normalized variables in the original string
                normalizedVars = (List<string>)getNormalizedVars(formulaString);
            }

        }

        /// <summary>
        /// Evaluates this Formula, using the lookup delegate to determine the values of
        /// variables.  When a variable symbol v needs to be determined, it should be looked up
        /// via lookup(normalize(v)). (Here, normalize is the normalizer that was passed to 
        /// the constructor.)
        /// 
        /// For example, if L("x") is 2, L("X") is 4, and N is a method that converts all the letters 
        /// in a string to upper case:
        /// 
        /// new Formula("x+7", N, s => true).Evaluate(L) is 11
        /// new Formula("x+7").Evaluate(L) is 9
        /// 
        /// Given a variable symbol as its parameter, lookup returns the variable's value 
        /// (if it has one) or throws an ArgumentException (otherwise).
        /// 
        /// If no undefined variables or divisions by zero are encountered when evaluating 
        /// this Formula, the value is returned.  Otherwise, a FormulaError is returned.  
        /// The Reason property of the FormulaError should have a meaningful explanation.
        ///
        /// This method should never throw an exception.
        /// </summary>
        public object Evaluate(Func<string, double> lookup)
        {
            Stack<double> values = new Stack<double>();
            Stack<string> operators = new Stack<string>();
            string[] substrings = Regex.Split(formulaString, "(\\()|(\\))|(-)|(\\+)|(\\*)|(/)");
            double result;
            
            try
            { 
                foreach (string token in substrings)
                {
                    if (token.Equals("") || token.Equals(" "))
                    {
                        continue;
                    }
                    //If token is a number
                    if (Double.TryParse(token, out result))
                    {
                        //IsOnTop is an extension
                        if (operators.IsOnTop("*") || operators.IsOnTop("/"))
                        {
                            if (Double.Parse(token) == 0 && operators.Peek() == "/")
                            {
                                return new FormulaError("cannot divide by zero");
                            }
                            result = popStack(Double.Parse(token), values.Pop(), operators.Pop());
                            values.Push(result);
                        }
                        else
                        {
                            values.Push(Double.Parse(token));
                        }
                    }
                    //if token is a variable

                    else if (validVarEvaluate(token))
                    {
                        if (operators.IsOnTop("*") || operators.IsOnTop("/"))
                        {
                            if ((lookup(token) - 0.0 < 1e-9) && operators.Peek() == "/")
                            {
                                return new FormulaError("cannot divide by zero");
                            }
                            result = popStack(lookup(token), values.Pop(), operators.Pop());
                            values.Push(result);
                        }
                        else
                        {
                            //push whatever is returned by the delegate provided
                            values.Push(lookup(token));
                        }
                    }

                    //if token is + or -
                    else if (token.Equals("+") || token.Equals("-"))
                    {
                        if (operators.IsOnTop("+") || operators.IsOnTop("-"))
                        {
                            result = popStack(values.Pop(), values.Pop(), operators.Pop());
                            values.Push(result);
                            operators.Push(token);
                        }
                        else
                        {
                            operators.Push(token);
                        }
                    }

                    //if token is * or /
                    else if (token.Equals("*") || token.Equals("/"))
                    {
                        operators.Push(token);
                    }

                    //if s is left parenthesis
                    else if (token.Equals("("))
                    {
                        operators.Push(token);
                    }

                    //if s is right parenthesis
                    else if (token.Equals(")"))
                    {
                        if (operators.IsOnTop("+") || operators.IsOnTop("-"))
                        {
                            result = popStack(values.Pop(), values.Pop(), operators.Pop());
                            values.Push(result);
                        }
                        if (operators.IsOnTop("("))
                        {
                            operators.Pop();
                        }

                        if (operators.IsOnTop("*") || operators.IsOnTop("/"))
                        {
                            if ((values.Peek() - 0.0 < 1e-9) && operators.Peek() == "/")
                            {
                                return new FormulaError("cannot divide by zero");
                            }
                            result = popStack(values.Pop(), values.Pop(), operators.Pop());
                            values.Push(result);
                        }
                    }
                }
                //when the last token has been processed
                if (operators.Count == 0)
                {
                    if (values.Count == 1)
                    {
                        return result = values.Pop();
                    }
                }
                if (operators.Count == 1)
                {
                    if ((operators.IsOnTop("+") || operators.IsOnTop("-")) && (values.Count == 2))
                    {
                        result = popStack(values.Pop(), values.Pop(), operators.Pop());
                        return result;
                    }
                }
                return values.Peek();
            }
            //if an undefined variables is found, it will throw an ArgumentException that will be caught here returning a FormulaError
             catch (ArgumentException e)
            {
                return new FormulaError("undefined variable");
            }

        }

        /// <summary>
        /// Enumerates the normalized versions of all of the variables that occur in this 
        /// formula.  No normalization may appear more than once in the enumeration, even 
        /// if it appears more than once in this Formula.
        /// 
        /// For example, if N is a method that converts all the letters in a string to upper case:
        /// 
        /// new Formula("x+y*z", N, s => true).GetVariables() should enumerate "X", "Y", and "Z"
        /// new Formula("x+X*z", N, s => true).GetVariables() should enumerate "X" and "Z".
        /// new Formula("x+X*z").GetVariables() should enumerate "x", "X", and "z".
        /// </summary>
        public IEnumerable<String> GetVariables()
        {
            return normalizedVars;
        }

        /// <summary>
        /// Returns a string containing no spaces which, if passed to the Formula
        /// constructor, will produce a Formula f such that this.Equals(f).  All of the
        /// variables in the string should be normalized.
        /// 
        /// For example, if N is a method that converts all the letters in a string to upper case:
        /// 
        /// new Formula("x + y", N, s => true).ToString() should return "X+Y"
        /// new Formula("x + Y").ToString() should return "x+Y"
        /// </summary>
        public override string ToString()
        {
            return formulaString;
        }

        /// <summary>
        /// If obj is null or obj is not a Formula, returns false.  Otherwise, reports
        /// whether or not this Formula and obj are equal.
        /// 
        /// Two Formulae are considered equal if they consist of the same tokens in the
        /// same order.  To determine token equality, all tokens are compared as strings 
        /// except for numeric tokens and variable tokens.
        /// Numeric tokens are considered equal if they are equal after being "normalized" 
        /// by C#'s standard conversion from string to double, then back to string. This 
        /// eliminates any inconsistencies due to limited floating point precision.
        /// Variable tokens are considered equal if their normalized forms are equal, as 
        /// defined by the provided normalizer.
        /// 
        /// For example, if N is a method that converts all the letters in a string to upper case:
        ///  
        /// new Formula("x1+y2", N, s => true).Equals(new Formula("X1  +  Y2")) is true
        /// new Formula("x1+y2").Equals(new Formula("X1+Y2")) is false
        /// new Formula("x1+y2").Equals(new Formula("y2+x1")) is false
        /// new Formula("2.0 + x7").Equals(new Formula("2.000 + x7")) is true
        /// </summary>
        public override bool Equals(object obj)
        {
            if (!(obj is Formula))
            {
                return false;
            }
            return obj.ToString().GetHashCode() == formulaString.GetHashCode();
        }

        /// <summary>
        /// Reports whether f1 == f2, using the notion of equality from the Equals method.
        /// Note that if both f1 and f2 are null, this method should return true.  If one is
        /// null and one is not, this method should return false.
        /// </summary>
        public static bool operator ==(Formula f1, Formula f2)
        {
            if (ReferenceEquals(f1, null))
            {
                return ReferenceEquals(f2, null);
            }
            return f1.Equals(f2);
        }

        /// <summary>
        /// Reports whether f1 != f2, using the notion of equality from the Equals method.
        /// Note that if both f1 and f2 are null, this method should return false.  If one is
        /// null and one is not, this method should return true.
        /// </summary>
        public static bool operator !=(Formula f1, Formula f2)
        {
            return !(f1 == f2);
        }

        /// <summary>
        /// Returns a hash code for this Formula.  If f1.Equals(f2), then it must be the
        /// case that f1.GetHashCode() == f2.GetHashCode().  Ideally, the probability that two 
        /// randomly-generated unequal Formulae have the same hash code should be extremely small.
        /// </summary>
        public override int GetHashCode()
        {
            return formulaString.GetHashCode();
        }

        /// <summary>
        /// Takes a string and returns a normalized version of it
        /// </summary>
        /// <param name="formula"></param>
        /// <param name="normalize"></param>
        /// <returns>a new string created by using the delegate normalize on formula</returns>
        private static string normalizedString(string formula, Func<string, string> normalize)
        {

            string tempString = "";
            //normalizes the whole formula, then adds all into a tempstring, numbers are converted to
            //doubles then to string before being added
            foreach (string token in GetTokens(normalize(formula)))
            {
                if (Double.TryParse(token, out double result))
                {
                    
                    tempString += Double.Parse(token).ToString();
                }
                else
                {
                    tempString += token;
                }
            }

            return tempString;

        }
        /// <summary>
        /// Gets all the normalized variables of a string into an IEnumerable without duplicates
        /// </summary>
        /// <param name="normalized"></param>
        /// <returns>An IEnumerable containing all normalized variables (no duplicates)</returns>
        private static IEnumerable<String> getNormalizedVars(string normalized)
        {
            List<String> varList = new List<string>();
            foreach (string token in GetTokens(normalized))
            {
                //validVarEvaluate is a helper method that sees if a string is a token
                if (validVarEvaluate(token))
                {
                    //dont add to list if it already exists(duplicates)
                    if (!varList.Contains(token))
                    {
                        varList.Add(token);
                    }
                }
            }
            return varList;

        }
        /// <summary>
        /// Sees if a string is a variable after validator delegate
        /// </summary>
        /// <param name="variable"></param>
        /// <param name="isValid"></param>
        /// <returns>true if passes validator and has default structure of variable</returns>
        private static bool validVar(string variable, Func<string, bool> isValid)
        {
            string varPattern = "^[a-zA-Z_][a-zA-Z0-9_]*$";
            if (!Regex.IsMatch(variable, varPattern))
            {
                return false;
            }
            if (isValid(variable))
            {
                return true;
            }
            else { return false; }


        }

        /// <summary>
        /// Sees if a string has the basic necessities of a variable
        /// </summary>
        /// <param name="variable"></param>
        /// <returns>passes default requirments of a variable</returns>
        private static bool validVarEvaluate(string variable)
        {
            string varPattern = "^[a-zA-Z_][a-zA-Z0-9_]*$";

            return Regex.IsMatch(variable, varPattern);

        }

        /// <summary>
        /// takes in a formula, normalizer and validator
        /// checks if the normalized formula is syntactically correct and passes validator
        /// </summary>
        /// <param name="formula"></param>
        /// <param name="normalize"></param>
        /// <param name="isValid"></param>
        /// <returns>returns true if the formula is valid, false otherwise</returns>
        private static bool validSyntax(string formula, Func<string, string> normalize, Func<string, bool> isValid)
        {
            int index = 0;
            int leftparenCount = 0;
            int rightparenCount = 0;
            string prevToken = "";

            //One Token Rule
            if (GetTokens(normalize(formula)).Count() == 0)
            {
                throw new FormulaFormatException("must be at least one token");
            }
            foreach (string token in GetTokens(normalize(formula)))
            {

                //Parsing
                if (!(token == "(" || token == ")" || token == "+" || token == "-" || token == "*" || token == "/"))
                {
                    if (!Double.TryParse(token, out double result))
                    {
                        //if not any other valid token see if its a valid variable
                        if (!validVar(token, isValid))
                        {
                            throw new FormulaFormatException("invalid token in formula");
                        }
                    }

                }

                index++;


                //Parenthesis/Operator Following Rule 
                if (prevToken == "(" || prevToken == "+" || prevToken == "-" || prevToken == "*" || prevToken == "/")
                {
                    
                    if (validVar(token, isValid) || token == "(" || Double.TryParse(token, out double result))
                    {

                    }
                    else
                    {

                        throw new FormulaFormatException("Any token that immediately follows an opening parenthesis or and operator must be either a number, variable, or an opening parenthesis");
                    }
                }

                //Extra Following Rule
                if (prevToken != "")
                {
                    if (prevToken == ")" || Double.TryParse(prevToken, out double prevResult) || validVar(prevToken, isValid))
                    {
                        if (token == ")" || token == "+" || token == "-" || token == "*" || token == "/")
                        {

                        }
                        else
                        {
                            throw new FormulaFormatException("Any token that immediately follows a number, a variable, or a closing parenthesis must be either an operator or a closing parenthesis");
                        }
                    }
                }

                //Right Parentheses Rule
                if (token == "(")
                {
                    leftparenCount++;
                }
                if (token == ")")
                {
                    rightparenCount++;
                    if (rightparenCount > leftparenCount)
                    {
                        throw new FormulaFormatException("When reading left to right there should be no point where there are more closing parentheses than opening ones");
                    }
                }

                //starting token rule
                if (index == 1)
                {
                    if (token == "(" || Double.TryParse(token, out double result) || validVar(token, isValid))
                    {

                    }
                    else
                    {
                        throw new FormulaFormatException("The first token of the expression is not a number, variable or opening parenthesis");
                    }
                }

                //Ending token rule
                if (index == GetTokens(formula).Count())
                {
                    if (Double.TryParse(token, out double result) || token == ")" || validVar(token, isValid))
                    {

                    }
                    else
                    {
                        throw new FormulaFormatException("The last token of the expression is not a number, variable or opening parenthesis");
                    }
                }
                prevToken = token;
            }

            //out of for loop
            //Balanced Parentheses Rule
            if (rightparenCount != leftparenCount)
            {
                throw new FormulaFormatException("the total number of opening parentheses must be equal to the number of closing ones");
            }

            return true;

        }

        /// <summary>
        /// popStack is a helper method that takes in an operator and two integers then applies said operator to integers
        /// </summary>
        /// <param name="value1">an integer</param>
        /// <param name="value2">another integer</param>
        /// <param name="op">the operator to be applied to integers</param>
        /// <returns>returns the final result of the applied operator and two integers</returns>
        private static double popStack(double value1, double value2, string op)
        {

            double result;
            if (op.Equals("+"))
            {
                result = value2 + value1;

                return result;
            }
            if (op.Equals("-"))
            {
                result = value2 - value1;

                return result;
            }
            if (op.Equals("*"))
            {
                result = value2 * value1;
                return result;
            }
            if (op.Equals("/"))
            {
                result = value2 / value1;
                return result;
            }
            return 0;
        }

        /// <summary>
        /// Given an expression, enumerates the tokens that compose it.  Tokens are left paren;
        /// right paren; one of the four operator symbols; a string consisting of a letter or underscore
        /// followed by zero or more letters, digits, or underscores; a double literal; and anything that doesn't
        /// match one of those patterns.  There are no empty tokens, and no token contains white space.
        /// </summary>
        private static IEnumerable<string> GetTokens(String formula)
        {
            // Patterns for individual tokens
            String lpPattern = @"\(";
            String rpPattern = @"\)";
            String opPattern = @"[\+\-*/]";
            String varPattern = @"[a-zA-Z_](?: [a-zA-Z_]|\d)*";
            String doublePattern = @"(?: \d+\.\d* | \d*\.\d+ | \d+ ) (?: [eE][\+-]?\d+)?";
            String spacePattern = @"\s+";

            // Overall pattern
            String pattern = String.Format("({0}) | ({1}) | ({2}) | ({3}) | ({4}) | ({5})",
                                            lpPattern, rpPattern, opPattern, varPattern, doublePattern, spacePattern);

            // Enumerate matching tokens that don't consist solely of white space.
            foreach (String s in Regex.Split(formula, pattern, RegexOptions.IgnorePatternWhitespace))
            {
                if (!Regex.IsMatch(s, @"^\s*$", RegexOptions.Singleline))
                {
                    yield return s;
                }
            }

        }
    }


    /// <summary>
    /// Used to report syntactic errors in the argument to the Formula constructor.
    /// </summary>
    public class FormulaFormatException : Exception
    {
        /// <summary>
        /// Constructs a FormulaFormatException containing the explanatory message.
        /// </summary>
        public FormulaFormatException(String message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Used as a possible return value of the Formula.Evaluate method.
    /// </summary>
    public struct FormulaError
    {
        /// <summary>
        /// Constructs a FormulaError containing the explanatory reason.
        /// </summary>
        /// <param name="reason"></param>
        public FormulaError(String reason)
            : this()
        {
            Reason = reason;
        }

        /// <summary>
        ///  The reason why this FormulaError was created.
        /// </summary>
        public string Reason { get; private set; }
    }
    static class StackExtensions
    {
        /// <summary>
        /// Sees if the value on top of this stack matches the given string
        /// </summary>
        /// <param name="stack">stack to be peeked</param>
        /// <param name="targetString">string to be matched bu peeked stack</param>
        /// <returns></returns>
        public static bool IsOnTop(this Stack<string> stack, string targetString)
        {
            return stack.Count > 0 && stack.Peek() == targetString;
        }
    }
}

