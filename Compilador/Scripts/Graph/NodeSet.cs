using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilador.Graph
{
    internal class NodeSet
    {
        private NodeSet[]? transitions;
        private static NodeSet emptySet = new NodeSet();
        private bool isFinal;

        private List<int> nodesIds;
        internal int Count { get => nodesIds.Count; }
        internal NodeSet[]? Transitions { get => transitions; }
        public bool IsFinal { get => isFinal; }

        internal NodeSet()
        {
            nodesIds = new List<int>();
            transitions = null;
            isFinal = false;
        }

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
        internal void AddNode(int n, bool isFinal)
        {
            if (nodesIds.Contains(n))
                return;
            nodesIds.Add(n);
            nodesIds.Sort();
            this.isFinal = isFinal;
        }

        private void AddNode(int[] nodes, bool hasFinals)
        {
            foreach (var n in nodes)
            {
                AddNode(n, hasFinals);
            }
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

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

        internal void CalculateTransitions(int alphabetLength, int[,][] transitionTable, int[] finalStates)
        {
            transitions = new NodeSet[alphabetLength];
            for (int i = 0; i < transitions.Length; i++)
            {
                transitions[i] = GetTransition(i, transitionTable, finalStates);
            }
        }

        internal EdgeInfo[] CalculateEdgeInfo(in List<NodeSet> q, char[] alphabet)
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
