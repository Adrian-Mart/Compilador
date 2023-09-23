using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Represents an edge in a graph, containing information about its start and end nodes, as well as the transition character.
/// </summary>
namespace Compilador.Graph
{
    public class EdgeInfo
    {
        /// <summary>
        /// The start node of the edge.
        /// </summary>
        private int start;
        /// <summary>
        /// The end node of the edge.
        /// </summary>
        private int end;
        /// <summary>
        /// The transition character of the edge.
        /// </summary>
        private char transition;

        /// <summary>
        /// Gets the start node of the edge.
        /// </summary>
        public int Start { get => start; }
        /// <summary>
        /// Gets the end node of the edge.
        /// </summary>
        public int End { get => end; }
        /// <summary>
        /// Gets the transition character of the edge.
        /// </summary>
        public char Transition { get => transition; }

        /// <summary>
        /// Initializes a new instance of the EdgeInfo class with the specified start and end nodes, and transition character.
        /// </summary>
        /// <param name="start">The start node of the edge.</param>
        /// <param name="end">The end node of the edge.</param>
        /// <param name="transition">The transition character of the edge.</param>
        public EdgeInfo(int start, int end, char transition)
        {
            this.start = start;
            this.end = end;
            this.transition = transition;
        }

        /// <summary>
        /// Returns a string representation of the edge in the format "start-transition->end".
        /// </summary>
        /// <returns>A string representation of the edge.</returns>
        public override string? ToString()
        {
            return string.Format("{0}-{1}->{2}", start, transition, end);
        }
    }
}
