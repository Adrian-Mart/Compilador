using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Compilador.Graph
{
    /// <summary>
    /// Represents a state in a deterministic finite automaton (DFA).
    /// </summary>
    [DataContract, KnownType(typeof(DFAState))]
    internal class DFAState
    {
        /// <summary>
        /// The ID of the state.
        /// </summary>
        [DataMember()]
        private int id;

        /// <summary>
        /// A value indicating whether the state is a final state.
        /// </summary>
        [DataMember()]
        private bool isFinal;

        /// <summary>
        /// The transitions of the state.
        /// </summary>
        [DataMember()]
        private Dictionary<int, DFAState> transitions;
            
        /// <summary>
        /// Gets the ID of the state.
        /// </summary>
        public int Id { get => id; }
            
        /// <summary>
        /// Gets a value indicating whether the state is a final state.
        /// </summary>
        public bool IsFinal { get => isFinal; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DFAState"/> class.
        /// </summary>
        /// <param name="node">The node to create the state from.</param>
        internal DFAState(Node node)
        {
            id = node.Id;
            transitions = new Dictionary<int, DFAState>();
            isFinal = node.IsFinal;
        }

        /// <summary>
        /// Adds the transitions of the given node to the state.
        /// </summary>
        /// <param name="node">The node to add the transitions from.</param>
        /// <param name="nodes">The dictionary of nodes in the DFA.</param>
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
        
        internal bool TryTransition(int transition, out DFAState? nextState)
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
