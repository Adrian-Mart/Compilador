using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilador.Graph
{
    /// <summary>
    /// Represents a node in a graph.
    /// </summary>
    internal class Node
    {
        /// <summary>
        /// The ID of the node.
        /// </summary>
        private int id;
        /// <summary>
        /// The edges of the node.
        /// </summary>
        private List<Edge> edges;
        /// <summary>
        /// A value indicating whether the node is a final state.
        /// </summary>
        private bool isFinal;
        /// <summary>
        /// A value indicating whether the node is a start state.
        /// </summary>
        private bool isStart;

        /// <summary>
        /// Gets the ID of the node.
        /// </summary>
        public int Id { get => id; }

        /// <summary>
        /// Gets the edges of the node.
        /// </summary>
        internal List<Edge> Edges { get => edges; }

        /// <summary>
        /// Gets or sets a value indicating whether the node is a final state.
        /// </summary>
        internal bool IsFinal { get => isFinal; set => isFinal = value; }

        /// <summary>
        /// Gets a value indicating whether the node is a start state.
        /// </summary>
        public bool IsStart { get => isStart; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Node"/> class with the specified ID and final state.
        /// </summary>
        /// <param name="id">The ID of the node.</param>
        /// <param name="isFinal">A value indicating whether the node is a final state.</param>
        internal Node(int id, bool isFinal)
        {
            edges = new List<Edge>();
            this.isFinal = isFinal;
            isStart = false;
            this.id = id;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Node"/> class with the specified ID, final state, and start state.
        /// </summary>
        /// <param name="id">The ID of the node.</param>
        /// <param name="isFinal">A value indicating whether the node is a final state.</param>
        /// <param name="isStart">A value indicating whether the node is a start state.</param>
        internal Node(int id, bool isFinal, bool isStart)
        {
            edges = new List<Edge>();
            this.isFinal = isFinal;
            this.isStart = isStart;
            this.id = id;
        }

        /// <summary>
        /// Adds an edge to the node.
        /// </summary>
        /// <param name="edge">The edge to add.</param>
        internal void AddEdge(Edge edge)
        {
            edges.Add(edge);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string? ToString()
        {
            string result = id.ToString();
            if (isFinal)
                result = result + 'f';
            if (isStart)
                result = 's' + result;

            return result;
        }

        /// <summary>
        /// Sets the ID of the node.
        /// </summary>
        /// <param name="newId">The new ID of the node.</param>
        internal void SetId(int newId)
        {
            id = newId;
        }
    }
}
