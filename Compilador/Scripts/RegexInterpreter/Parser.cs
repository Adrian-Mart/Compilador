using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilador.RegexInterpreter
{
    /// <summary>
    /// Class used for parsing the regular expresion to a reverse polish notation like.
    /// </summary>
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
        /// Contains all the valid operators including ( and ).
        /// </summary>
        private static readonly Dictionary<char, char> operators = new Dictionary<char, char>(){
            {'|', '╔'},
            {'.', '╖'},
            {'*', '╗'},
            {'+', '╘'},
            {'(', '╙'},
            {')', '╚'}
        };

        /// <summary>
        /// Returns the inverted operators dictionary.
        /// </summary>
        private static readonly Dictionary<char, char> operatorsCorrespondency = new Dictionary<char, char>(){
            {'╔','|'},
            {'╖','.'},
            {'╗','*'},
            {'╘','+'},
            {'╙','('},
            {'╚',')'}
        };

        /// <summary>
        /// Returns the operators dictionary. The key is the operator in the
        /// regular expresion and the value is the operator in the reverse 
        /// polish like notation.
        /// </summary>
        public static Dictionary<char, char> Correspondency => operatorsCorrespondency;

        /// <summary>
        /// Returns the expresion parsed in an reverse polish like notation adjusted
        /// for the regular expresion operators. If the expresion contains enclosured
        /// by double quotes operators, they will be replaced by the corresponding
        /// operator in the operators dictionary.
        /// </summary>
        /// <param name="exp">Expresion to be parsed.</param>
        /// <returns>Parsed expresion</returns>
        /// <exception cref="Exception"></exception>
        internal static string Parse(string exp)
        {
            string refactoredExp = exp;
            foreach (var key in operators.Keys)
                refactoredExp = refactoredExp.Replace(
                    string.Format("(<{0}>)", key),
                    operators[key].ToString());

            if (refactoredExp == null) return "";

            operatorStack.Clear();
            outputQueue.Clear();

            foreach (char c in refactoredExp)
            {
                if (c == '.' || c == '(')
                    operatorStack.Push(c);
                else if (c == '|')
                {
                    while (operatorStack.Count > 0 && operatorStack.Peek() == '.')
                    {
                        outputQueue.Enqueue(operatorStack.Pop());
                    }
                    operatorStack.Push(c);
                }
                else if (c == ')')
                {
                    while (operatorStack.Peek() != '(')
                    {
                        if (operatorStack.Count == 0)
                            throw new Exception("Mismatched parentheses");
                        outputQueue.Enqueue(operatorStack.Pop());
                    }
                    operatorStack.Pop();
                }
                else
                    outputQueue.Enqueue(c);
            }

            while (operatorStack.Count > 0)
            {
                outputQueue.Enqueue(operatorStack.Pop());
            }

            return GetOutputString();
        }

        /// <summary>
        /// Warning: this method clears the output queue.
        /// Returns and pops all the characters stored on the output queue.
        /// </summary>
        /// <returns>String of characters stored on the output stack. </returns>
        private static string GetOutputString()
        {
            StringBuilder builder = new StringBuilder();
            while (outputQueue.Count > 0)
            {
                builder.Append(outputQueue.Dequeue());
            }
            return builder.ToString();
        }
    }
}
