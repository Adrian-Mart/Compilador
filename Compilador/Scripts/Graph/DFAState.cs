using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graph
{
    internal class DFAState
    {
        private int id;
        private bool isFinal;
        private Dictionary<char, DFAState> transitions;
        public int Id { get => id; }
        public bool IsFinal { get => isFinal; }

        internal DFAState(Node node)
        {
            id = node.Id;
            transitions = new Dictionary<char, DFAState>();
            isFinal = node.IsFinal;
        }

        internal void AddTransitions(Node node, Dictionary<int, DFAState> nodes)
        {
            transitions.Clear();
            foreach (var edge in node.Edges)
            {
                try { transitions.Add(edge.Transition, nodes[edge.End.Id]); }
                catch (ArgumentException argException)
                {
                    Console.WriteLine("Error: The state has ambiguous transitions.");
                    throw argException;
                }
            }
        }

        /// <summary>
        /// Try to move the DFA to the next state through the transition.
        /// </summary>
        /// <param name="transition"> Transition selected to go to the next state </param>
        /// <param name="nextState"> The arrival DFAState through the transition.
        /// Null if ther is no available transition. </param>
        /// <returns></returns>
        internal bool TryTransition(char transition, out DFAState? nextState)
        {
            if (transitions.ContainsKey(transition))
            {
                nextState = transitions[transition];
                return true;
            }
            else
            {
                nextState = null;
                return false;
            }
        }
    }
}
