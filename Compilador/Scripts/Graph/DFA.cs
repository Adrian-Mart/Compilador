using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Compilador.Graph
{
    /// <summary>
    /// Represents a deterministic finite automaton (DFA).
    /// </summary>
    [DataContract(IsReference = true), KnownType(typeof(DFA)), KnownType(typeof(DFAState))]
    public class DFA : ITester
    {
        /// <summary>
        /// A dictionary that maps state IDs to DFAState objects
        /// </summary>
        [DataMember()]
        private Dictionary<int, DFAState> states;
        /// <summary>
        /// The start state of the DFA
        /// </summary>
        [DataMember()]
        private DFAState startState;
        /// <summary>
        /// The alphabet of the DFA
        /// </summary>
        [DataMember()]
        private int[] alphabet;

        /// <summary>
        /// DFA Constructor for serialization purposes.
        /// </summary>
        internal DFA(Dictionary<int, DFAState> states, DFAState startState, int[] alphabet)
        {
            this.states = states;
            this.startState = startState;
            this.alphabet = alphabet;
        }
        
        internal DFA(int[] alphabet, Dictionary<int, Node> nodes, int startNodeId)
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
        
        public bool TestIds(int[] ids)
        {
            if (ids.Length == 0) return startState.IsFinal;
            if (!IdsInAlphabet(ids))
            {
                return false;
            }

            DFAState? actualState = startState;
            foreach (var c in ids)
            {
                if (actualState == null)
                    return false;
                if (!actualState.TryTransition(c, out actualState))
                    return false;
            }
            if (actualState == null)
                return false;
            return actualState.IsFinal;
        }
        
        private bool IdsInAlphabet(int[] ids)
        {
            foreach (var id in ids)
            {
                if (!alphabet.Contains(id))
                    return false;
            }
            return true;
        }
    }


}
