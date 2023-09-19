using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Graph
{
    public class DFA
    {
        private Dictionary<int, DFAState> states;
        private DFAState startState;
        private char[] alphabet;

        internal DFA(char[] alphabet, Dictionary<int, Node> nodes, int startNodeId)
        {
            this.alphabet = alphabet;
            states = new Dictionary<int, DFAState>();
            startState = new DFAState(nodes[startNodeId]);
            states.Add(startState.Id, startState);
            foreach (var item in nodes.Values)
            {
                if (item == nodes[startNodeId])
                    continue;
                var state = new DFAState(item);
                states.Add(state.Id, state);
            }

            foreach (var state in states.Values)
            {
                state.AddTransitions(nodes[state.Id], states);
            }
        }

        public bool TestString(string s)
        {
            int errorIndex = 0;
            if (s == "") return startState.IsFinal;
            if (!StringInAlphabet(s, out errorIndex))
            {
                Console.WriteLine(string.Format("Undefined char. Error in char {0} of {1}: {2}",
                    errorIndex, s, s[errorIndex]));
                return false;
            }

            DFAState? actualState = startState;
            errorIndex = 0;
            foreach (var c in s)
            {
                if (actualState == null)
                    return false;
                if (!actualState.TryTransition(c, out actualState))
                {
                    Console.WriteLine(string.Format("Misplaced char. Error in char {0} of {1}: {2}",
                        errorIndex, s, c));
                    return false;
                }
                errorIndex++;
            }
            if (!actualState.IsFinal)
                Console.WriteLine(string.Format("Incomplete expresion. Error in char {0} of {1}: {2}",
                    errorIndex - 1, s, s[errorIndex - 1]));
            return actualState.IsFinal;
        }

        /// <summary>
        /// Test whether all the characters in s are contained in the alphabet.
        /// </summary>
        /// <param name="s"> String being tested </param>
        /// <param name="errorIndex"> Index of the first character not contained
        /// in the alphabet</param>
        /// <returns>True if all the characters in s are contained in the alphabet,
        /// in any other case false. </returns>
        private bool StringInAlphabet(string s, out int errorIndex)
        {
            errorIndex = 0;
            foreach (var c in s)
            {
                if (!alphabet.Contains(c))
                    return false;
                errorIndex++;
            }
            return true;
        }
    }
}
