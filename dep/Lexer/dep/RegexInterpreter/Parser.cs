using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegexInterpreter
{
    internal static class Parser
    {
        /// <summary>
        /// Stack used for storing the oparators during parsing.
        /// </summary>
        private static Stack<char> operatorStack = new Stack<char>();
        /// <summary>
        /// Queue used for storing the output during parsing.
        /// </summary>
        private static Queue<char> outputQueue = new Queue<char>();

        /// <summary>
        /// Extra symbols that are allowed in any the expresion.
        /// The default symbols are digits and letters.
        /// </summary>
        private static char[] extraAlphabet = new char[] { ' ', '!'};

        /// <summary>
        /// Contains all the valid operators including ( and ).
        /// </summary>
        private static char[] operators = new char[] {'|', '+', '*', '.', '(', ')'};

        public static char[] ExtraAlphabet { get => extraAlphabet; }

        /// <summary>
        /// Returns the expresion parsed in an reverse polish like notation adjusted
        /// for the regular expresion operators.
        /// </summary>
        /// <param name="exp">Expresion to be parsed.</param>
        /// <returns>Parsed expresion</returns>
        /// <exception cref="Exception"></exception>
        internal static string Parse(string exp)
        {
            if (exp == null) return "";
            if (!CheckExp(exp))
                throw new Exception("The expresion contains undefined operators");

            operatorStack.Clear();
            outputQueue.Clear();

            foreach (char c in exp)
            {
                if(char.IsLetterOrDigit(c) || extraAlphabet.Contains(c) || c == '*' || c == '+')
                    outputQueue.Enqueue(c);
                else if (c == '.' || c == '(')
                    operatorStack.Push(c);
                else if (c == '|')
                {
                    while(operatorStack.Count > 0 && operatorStack.Peek() == '.')
                    {
                        outputQueue.Enqueue(operatorStack.Pop());
                    }
                    operatorStack.Push(c);
                }
                else
                {
                    while (operatorStack.Peek() != '(') 
                    {
                        if (operatorStack.Count == 0)
                            throw new Exception("Mismatched parentheses");
                        outputQueue.Enqueue(operatorStack.Pop());
                    }
                    operatorStack.Pop();
                }
            }

            while(operatorStack.Count > 0)
            {
                outputQueue.Enqueue(operatorStack.Pop());
            }

            return GetOutputString();
        }

        /// <summary>
        /// Checks whether exp contains characters that are either
        /// numbers, letters or defined operators.
        /// </summary>
        /// <param name="exp"> string beign cheked </param>
        /// <returns> If the expresion is valid returns true,
        /// else returns false. </returns>
        private static bool CheckExp(string exp)
        {
            foreach (char c in exp)
                if (!operators.Contains(c) && !char.IsLetterOrDigit(c) && !extraAlphabet.Contains(c))
                    return false;
            return true;
        }

        /// <summary>
        /// Warning: this method clears the output queue.
        /// Returns and pops all the characters stored on the output queue.
        /// </summary>
        /// <returns>String of characters stored on the output stack. </returns>
        private static string GetOutputString()
        {
            StringBuilder builder = new StringBuilder();
            while(outputQueue.Count > 0)
            {
                builder.Append(outputQueue.Dequeue());
            }
            return builder.ToString();
        }
    }
}
