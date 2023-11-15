using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilador.Graph
{
    /// <summary>
    /// Represents a set of nodes in a graph.
    /// </summary>
    internal class NodeSet
    {
        /// <summary>
        /// The reachable nodes from each of the nodes in the set.
        /// </summary>
        private NodeSet[]? transitions;
        /// <summary>
        /// An empty set of nodes.
        /// </summary>
        private static NodeSet emptySet = new NodeSet();
        /// <summary>
        /// A value indicating whether the set is a final state.
        /// </summary>
        private bool isFinal;

        /// <summary>
        /// The IDs of the nodes in the set.
        /// </summary>
        private List<int> nodesIds;
        /// <summary>
        /// Gets the number of nodes in the set.
        /// </summary>
        internal int Count { get => nodesIds.Count; }
        /// <summary>
        /// Gets the reachable nodes from each of the nodes in the set.
        /// </summary>
        internal NodeSet[]? Transitions { get => transitions; }
        public bool IsFinal { get => isFinal; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeSet"/> class.
        /// </summary>
        internal NodeSet()
        {
            nodesIds = new List<int>();
            transitions = null;
            isFinal = false;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if(obj.GetType() != this.GetType()) return false;
            var objNode = (NodeSet)obj;
            if (objNode.Count != nodesIds.Count) return false;
            for (int i = 0; i < nodesIds.Count; i++)
            {
                if (nodesIds[i] != objNode.nodesIds[i]) return false;
            }
            return true;
        }

        /// <summary>
        /// Adds a node to the set.
        /// </summary>
        /// <param name="n">The ID of the node to add.</param>
        /// <param name="isFinal">Whether the node is a final state.</param>
        internal void AddNode(int n, bool isFinal)
        {
            if (nodesIds.Contains(n))
                return;
            nodesIds.Add(n);
            nodesIds.Sort();
            this.isFinal = isFinal;
        }

        /// <summary>
        /// Adds a set of nodes to the set.
        /// </summary>
        /// <param name="nodes">
        /// An array of IDs of the nodes to add.
        /// </param>
        /// <param name="hasFinals">
        /// Whether at least on of the nodes is a final state.
        /// </param>
        private void AddNode(int[] nodes, bool hasFinals)
        {
            foreach (var n in nodes)
            {
                AddNode(n, hasFinals);
            }
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Gets the transition for the specified symbol.
        /// </summary>
        /// <param name="symbolIndex">
        /// The index of the symbol in the alphabet.
        /// </param>
        /// <param name="transitionTable">
        /// The transition table.
        /// </param>
        /// <param name="finalStates">
        /// The final states.
        /// </param>
        /// <returns>
        /// The transition for the specified symbol.
        /// </returns>
        private NodeSet GetTransition(int symbolIndex, int[,][] transitionTable, int[] finalStates)
        {
            if (nodesIds.Count == 0) return emptySet;

            NodeSet set = new NodeSet();

            foreach (var n in nodesIds)
            {
                bool hasFinalStates = false;
                foreach (var item in transitionTable[n, symbolIndex])
                {
                    if (finalStates.Contains(item))
                    {
                        hasFinalStates = true;
                        break;
                    }
                }
                set.AddNode(transitionTable[n, symbolIndex], hasFinalStates);
            }
            return set;
        }

        /// <summary>
        /// Calculates the transitions for the set.
        /// </summary>
        /// <param name="alphabetLength">The length of the alphabet.</param>
        /// <param name="transitionTable">The transition table.</param>
        /// <param name="finalStates">The final states.</param>
        internal void CalculateTransitions(int alphabetLength, int[,][] transitionTable, int[] finalStates)
        {
            transitions = new NodeSet[alphabetLength];
            for (int i = 0; i < transitions.Length; i++)
            {
                transitions[i] = GetTransition(i, transitionTable, finalStates);
            }
        }

        /// <summary>
        /// Calculates the edge information for the set.
        /// </summary>
        /// <param name="q">The Q set.</param>
        /// <param name="alphabet">The alphabet of the language defined by the regex.</param>
        /// <returns>The edge information for the set.</returns>
        internal EdgeInfo[] CalculateEdgeInfo(in List<NodeSet> q, int[] alphabet)
        {
            int index = q.IndexOf(this);
            List<EdgeInfo> info = new List<EdgeInfo>();
            if (transitions == null)
                return new EdgeInfo[0];
            for (int i = 0; i < transitions.Length; i++)
            {
                if (!transitions[i].Equals(emptySet))
                    info.Add(new EdgeInfo(index, q.IndexOf(transitions[i]), alphabet[i]));
            }
            return info.ToArray();
        }
        
        public override string? ToString()
        {
            if (this == emptySet) return " - ";
            StringBuilder sb = new StringBuilder();
            if (isFinal) sb.Append("F");
            foreach (var item in nodesIds)
            {
                sb.Append(item.ToString());
            }
            sb.Append(" -> { ");
            if (transitions == null)
                sb.Append(" - }");
            else
            {
                foreach (var item in transitions)
                {
                    sb.Append(item.Name());
                    sb.Append(", ");
                }
                sb.Remove(sb.Length - 2, 2);
                sb.Append("}");
            }
            return sb.ToString();
        }

        /// <summary>
        /// Gets the name of the set.
        /// </summary>
        /// <returns>A String that represents the name of the set.</returns>
        internal string Name()
        {
            if (nodesIds.Count == 0) return "E";
            StringBuilder sb = new StringBuilder();
            if (isFinal) sb.Append("F");
            foreach (var item in nodesIds)
            {
                sb.Append(item.ToString());
            }
            return sb.ToString();
        }
    }
}
