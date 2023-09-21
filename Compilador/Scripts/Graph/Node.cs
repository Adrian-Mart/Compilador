using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graph
{
    internal class Node
    {
        private int id;
        private List<Edge> edges;
        private bool isFinal;
        private bool isStart;

        public int Id { get => id; }
        internal List<Edge> Edges { get => edges; }
        internal bool IsFinal { get => isFinal; set => isFinal = value; }
        public bool IsStart { get => isStart; }

        internal Node(int id, bool isFinal)
        {
            edges = new List<Edge>();
            this.isFinal = isFinal;
            isStart = false;
            this.id = id;
        }

        internal Node(int id, bool isFinal, bool isStart)
        {
            edges = new List<Edge>();
            this.isFinal = isFinal;
            this.isStart = isStart;
            this.id = id;
        }

        internal void AddEdge(Edge edge)
        {
            edges.Add(edge);
        }

        public override string? ToString()
        {
            string result = id.ToString();
            if(isFinal)
                result = result + 'f';
            if(isStart)
                result = 's' + result;

            return result;
        }

        internal void SetId(int newId)
        {
            id = newId;
        }
    }
}
