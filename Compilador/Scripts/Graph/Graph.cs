using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilador.Graph
{
    public class Graph
    {
        private protected Dictionary<int, Node> ids;
        private protected int startId;
        private protected int endId;

        internal Dictionary<int, Node> Ids { get => ids; }

        public Graph(int startId, int endId, EdgeInfo[] edges)
        {
            this.startId = startId;
            this.endId = endId;

            int nodesCount = NodesInDescription(edges);
            if (nodesCount < 2)
                throw new Exception("Node number is not valid. The minimum number of nodes is 2.");
            ids = new Dictionary<int, Node>();
            bool startIsEnd = startId == endId;

            if (startIsEnd)
                ids.Add(startId, new Node(startId, true, true));
            else
            {
                ids.Add(startId, new Node(startId, false, true));
                ids.TryAdd(endId, new Node(endId, true));
            }

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
            if(stringBuilder.Length > 2) stringBuilder.Remove(stringBuilder.Length - 2, 2);
            return stringBuilder.ToString();
        }

    }
}
