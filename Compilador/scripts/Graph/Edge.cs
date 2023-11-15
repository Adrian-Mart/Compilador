using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilador.Graph
{
    /// <summary>
    /// Edge class. Represents an edge in the graph and contains the
    /// end node to which the edge goes. Considers a transition
    /// character from a node to the end node.
    /// </summary>
    internal class Edge
    {
        /// <summary>
        /// Node start from which the edge starts
        /// </summary>
        private Node start;
        /// <summary>
        /// Node end which the edge goes
        /// </summary>
        private Node end;
        /// <summary>
        /// Transition id from the start node 
        /// to the end node
        /// </summary>
        private int transition;

        /// <summary>
        /// Transition id from the start node 
        /// to the end node
        /// </summary>
        internal int Transition { get => transition; }
        /// <summary>
        /// Node end which the edge goes
        /// </summary>
        internal Node End { get => end; }
        /// <summary>
        /// Node start from which the edge starts
        /// </summary>
        internal Node Start { get => start; }

        /// <summary>
        /// Constructor for the edge
        /// </summary>
        /// <param name="start"> Node start from which the edge
        /// starts </param>
        /// <param name="end"> Node to which the edge goes </param>
        /// <param name="transition"> Transition id from the 
        /// "From" node "To" the end node </param>
        internal Edge(Node start, Node end, int transition)
        {
            this.start = start;
            this.end = end;
            this.transition = transition;
        }

        /// <summary>
        /// Returns a string representation of the edge.
        /// </summary>
        /// <returns> A string with the format [start]-[transition]>[end].</returns>
        public override string? ToString()
        {
            return string.Format("{0}-{1}>{2}", start, transition, end);
        }
    }
}
