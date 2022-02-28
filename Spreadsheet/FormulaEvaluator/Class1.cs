using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace FormulaEvaluator
{
    // Author: Charles Li, Fall 2021
    // University of Utah

    /// <summary>
    /// This class provides the Evaluate methodand helpers for evaluating infix expressions.
    /// ...
    /// </summary>
    public static class Evaluator
    {
        public delegate int Lookup(String v);

        /// <summary>
        /// A method that takes in an infix expression as a string then evaluates it
        /// </summary>
        /// <param name="exp">The expression given as a string</param>
        /// <param name="variableEvaluator">delegate provided to evaluate variables</param>
        /// <returns>The final value of the infix expression </returns>
        public static int Evaluate(String exp, Lookup variableEvaluator)
        {
            Stack<int> values = new Stack<int>();
            Stack<string> operators = new Stack<string>();
            string[] substrings = Regex.Split(exp, "(\\()|(\\))|(-)|(\\+)|(\\*)|(/)");
            int result;

            //removes leading and trailing whitespace for substrings
            for (int i = 0; i < substrings.Length; i++)
            {
                string trim = substrings[i].Trim();
                substrings[i] = trim;
            }



            foreach (string token in substrings)
            {
                if (token.Equals("") || token.Equals(" "))
                {
                    continue;
                }
                //If s in an integer
                if (int.TryParse(token, out result))
                {

                    if (int.Parse(token) < 0)
                    {
                        throw new ArgumentException();
                    }
                    //IsOnTop is an extension
                    if (operators.IsOnTop("*") || operators.IsOnTop("/"))
                    {
                        if (values.Count == 0)
                        {
                            throw new ArgumentException();
                        }
                        //popStack is a helper method
                        result = popStack(int.Parse(token), values.Pop(), operators.Pop());
                        values.Push(result);
                    }
                    else
                    {
                        values.Push(int.Parse(token));
                    }
                }

                //isVar is a helper method determining if a string is a variable
                else if (isVar(token))
                {
                    if (operators.IsOnTop("*") || operators.IsOnTop("/"))
                    {
                        if (values.Count == 0)
                        {
                            throw new ArgumentException();
                        }
                        result = popStack(variableEvaluator(token), values.Pop(), operators.Pop());
                        values.Push(result);
                    }
                    else
                    {
                        //push whatever is returned by the delegate provided
                        values.Push(variableEvaluator(token));
                    }

                }

                //if s is + or -
                else if (token.Equals("+") || token.Equals("-"))
                {
                    if (operators.IsOnTop("+") || operators.IsOnTop("-"))
                    {
                        if (values.Count < 2)
                        {
                            throw new ArgumentException();
                        }
                        result = popStack(values.Pop(), values.Pop(), operators.Pop());
                        values.Push(result);
                        operators.Push(token);
                    }
                    else
                    {
                        operators.Push(token);
                    }
                }

                //if s is * or /
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
                        if (values.Count < 2)
                        {
                            throw new ArgumentException();
                        }
                        result = popStack(values.Pop(), values.Pop(), operators.Pop());
                        values.Push(result);
                    }
                    if (operators.IsOnTop("("))
                    {
                        operators.Pop();
                    }
                    else
                    {
                        throw new ArgumentException();
                    }

                    if (operators.IsOnTop("*") || operators.IsOnTop("/"))
                    {
                        if (values.Count < 2)
                        {
                            throw new ArgumentException();
                        }
                        result = popStack(values.Pop(), values.Pop(), operators.Pop());
                        values.Push(result);

                    }


                }
                else
                {
                    throw new ArgumentException();
                }
            }

            //when the last token has been processed
            if (operators.Count == 0)
            {
                if (values.Count == 1)
                {
                    return result = values.Pop();
                }
                else
                {
                    throw new ArgumentException();
                }

            }
            if (operators.Count == 1)
            {
                if ((operators.IsOnTop("+") || operators.IsOnTop("-")) && (values.Count == 2))
                {
                    result = popStack(values.Pop(), values.Pop(), operators.Pop());
                    return result;
                }
                else
                {
                    throw new ArgumentException();
                }
            }
            return values.Peek();
        }




        /// <summary>
        /// popStack is a helper method that takes in an operator and two integers then applies said operator to integers
        /// </summary>
        /// <param name="value1">an integer</param>
        /// <param name="value2">another integer</param>
        /// <param name="op">the operator to be applied to integers</param>
        /// <returns>returns the final result of the applied operator and two integers</returns>
        private static int popStack(int value1, int value2, string op)
        {
            int result;
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
                //divide by zero exception
                if (value1 == 0)
                {
                    throw new ArgumentException();
                }
                result = value2 / value1;
                return result;
            }
            return 0;
        }

        /// <summary>
        /// A helper method that determines if a string is considered a variable
        /// </summary>
        /// <param name="s">string provided</param>
        /// <returns>true if string is a variable, false otherwise</returns>
        private static bool isVar(string s)
        {


            int currentIndex = 0;
            char[] charArray = s.ToCharArray();

            //if first char is not a letter
            if (!char.IsLetter(charArray[0]))
            {
                return false;
            }


            for (int i = 1; i < charArray.Length; i++)
            {
                // if a char appears after the letter(s), break out of this loop
                if (!char.IsLetter(charArray[i]) && char.IsDigit(charArray[i]))
                {
                    //keeping track of the index in charArray where we broke
                    currentIndex = i;
                    break;
                }
                // if a char appears and it isnt a digit or letter the string is invalid
                if (!char.IsLetter(charArray[i]) && !char.IsDigit(charArray[i]))
                {
                    return false;
                }
                //keeping track of the index in charArray where we broke
                currentIndex = i;
            }
            for (int i = currentIndex; i < charArray.Length; i++)
            {
                //if anything after the first digit isnt a digit the string is not a variable
                if (!char.IsDigit(charArray[i]))
                {
                    return false;
                }

            }
            return true;
        }

    }
    /// <summary>
    /// This class provides the Extension IsOnTop to reduce redundent code found in the Evaluate Method
    /// </summary>
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
