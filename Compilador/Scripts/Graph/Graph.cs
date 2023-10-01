using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Compilador.Graph
{
    /// <summary>
    /// Represents a graph data structure.
    /// </summary>
    /// <remarks>
    /// This class is used to create a graph with nodes and edges.
    /// Designed to provide methods to manipulate and traverse the graph.
    /// </remarks>
    public class Graph
    {
        /// <summary>
        /// The dictionary of nodes in the graph. The key is the node ID.
        /// </summary>
        private protected Dictionary<int, Node> ids;
        /// <summary>
        /// The start node ID.
        /// </summary>
        private protected int startId;
        /// <summary>
        /// The main end node ID.
        /// </summary>
        private protected int endId;
        /// <summary>
        /// Gets the start node ID.
        /// </summary>
        internal Dictionary<int, Node> Ids { get => ids; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="Graph"/> class with the
        /// specified start and end node IDs, and an array of edges.
        /// </summary>
        /// <param name="startId">The ID of the start node.</param>
        /// <param name="endId">The ID of the end node.</param>
        /// <param name="edges">An array of <see cref="EdgeInfo"/>
        /// objects representing the edges in the graph.</param>
        public Graph(int startId, int endId, EdgeInfo[] edges)
        {
            this.startId = startId;
            this.endId = endId;

            // Check if the number of nodes is valid
            int nodesCount = NodesInDescription(edges);
            if (nodesCount < 2)
            {
                Console.WriteLine("DFA definition: Node number is not valid. The minimum number of nodes is 2.");
                throw new Exception("Node number is not valid. The minimum number of nodes is 2.");
            }
                

            // Inicialize the dictionary of nodes
            ids = new Dictionary<int, Node>();

            // Determine if the start node is the end node
            bool startIsEnd = startId == endId;

            // Add the start and end nodes to the dictionary
            if (startIsEnd)
                ids.Add(startId, new Node(startId, true, true));
            else
            {
                ids.Add(startId, new Node(startId, false, true));
                ids.TryAdd(endId, new Node(endId, true));
            }

            // Add the rest of the nodes to the dictionary
            foreach (EdgeInfo edgeInfo in edges)
            {
                if (!ids.ContainsKey(edgeInfo.Start))
                {
                    ids.Add(edgeInfo.Start, new Node(edgeInfo.Start, false));
                }
                if (!ids.ContainsKey(edgeInfo.End))
                {
                    ids.Add(edgeInfo.End, new Node(edgeInfo.End, false));
                }

                Edge localEdge = new Edge(ids[edgeInfo.Start], ids[edgeInfo.End], edgeInfo.Transition);
                ids[edgeInfo.Start].AddEdge(localEdge);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Graph"/> class with the
        /// </summary>
        /// <param name="edges">
        /// An array of <see cref="EdgeInfo"/> objects representing the edges in the graph.
        /// </param>
        /// <returns>
        /// An int representing the number of nodes in the graph.
        /// </returns>
        private protected int NodesInDescription(EdgeInfo[] edges)
        {
            List<int> nodes = new List<int>();
            foreach (EdgeInfo edge in edges)
            {
                if (!nodes.Contains(edge.Start)) nodes.Add(edge.Start);
                if (!nodes.Contains(edge.End)) nodes.Add(edge.End);
            }
            return nodes.Count;
        }

        public override string? ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var node in ids.Values)
            {
                foreach (var edge in node.Edges)
                {
                    stringBuilder.Append(edge.ToString()).Append(", ");
                }
            }
            if (stringBuilder.Length > 2) stringBuilder.Remove(stringBuilder.Length - 2, 2);
            return stringBuilder.ToString();
        }

    }
}
