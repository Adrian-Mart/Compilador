using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Graph
{
    public class TempDFA : Graph
    {
        private char[] alphabet;
        private int[] finalNodes;
        private DFA automata;

        public DFA Automata { get => automata; }

        public TempDFA(int startId, int endId, EdgeInfo[] edges, char[] alphabet) : base(startId, endId, edges)
        {
            this.alphabet = alphabet;
            finalNodes = new int[] { endId };
            ClearEpsilonTransitions();
            ClearDeadEndStates();
            ClearInaccessibleStates();
            DefineAutomata();
            ClearDeadEndStates();
            ClearInaccessibleStates();
            //Console.WriteLine(this.ToString());
            automata = new DFA(this.alphabet, this.ids, this.startId);
        }

        private void ClearEpsilonTransitions()
        {
            int nodesCount = ids.Count;
            int[,][] deltaM = new int[nodesCount, alphabet.Length][];
            Dictionary<int, Node[]> eClousure = new Dictionary<int, Node[]>();
            foreach (var stateId in ids.Keys)
            {
                eClousure.Add(stateId, GetEClousure(ids[stateId]));
            }


            foreach (var stateId in ids.Keys)
            {
                for (int i = 0; i < alphabet.Length; i++)
                {
                    deltaM[stateId, i] = GetTransitions(eClousure, ids[stateId], alphabet[i]);
                }
            }

            EdgeInfo[] info = NFAEdgeInfo(deltaM, nodesCount, alphabet.Length);
            int[] finalStates;
            if (eClousure[startId].Contains(ids[endId]))
                finalStates = new int[] { startId, endId};
            else
                finalStates = new int[] { endId };
            ResetGraphInfo(startId, endId, info, finalStates);

            //Console.WriteLine("");
            //Console.WriteLine("Start: " + startId);
            //Console.WriteLine("End: " + endId);
            //foreach (var stateId in eClousure.Keys)
            //{
            //    Console.Write(stateId);
            //    Console.Write(" :{");
            //    foreach (var set in eClousure[stateId])
            //    {
            //        Console.Write(set.ToString());
            //        Console.Write(" ");
            //    }
            //    Console.WriteLine("}");
            //}

            //foreach (var stateId in eClousure.Keys)
            //{
            //    Console.Write("State q");
            //    Console.WriteLine(stateId);
            //    for (int i = 0; i < alphabet.Length; i++)
            //    {
            //        Console.Write("    S(q");
            //        Console.Write(stateId);
            //        Console.Write(",");
            //        Console.Write(alphabet[i]);
            //        Console.Write("): { ");
            //        foreach (var set in deltaM[stateId, i])
            //        {
            //            Console.Write("q");
            //            Console.Write(set);
            //            Console.Write(" ");
            //        }
            //        Console.WriteLine("}");

            //    }
            //}
        }

        private Node[] GetEClousure(Node n)
        {
            List<Node> nodes = new List<Node>();
            Stack<Node> stack = new Stack<Node>();
            nodes.Add(n);
            stack.Push(n);

            Node temp;
            do
            {
                temp = stack.Pop();
                foreach (var edge in temp.Edges)
                {
                    if (edge.Transition == '~' && !nodes.Contains(edge.End))
                    {
                        nodes.Add(edge.End);
                        stack.Push(edge.End);
                    }
                }
            } while (stack.Count > 0);

            return nodes.ToArray();
        }

        private int[] GetTransitions(Dictionary<int, Node[]> eClousures, Node n, char symbol)
        {
            List<int> nodes = new List<int>();

            foreach (var node in eClousures[n.Id])
            {
                foreach (var edge in node.Edges)
                {
                    if (edge.Transition == symbol)
                    {
                        foreach (var state in eClousures[edge.End.Id])
                        {
                            if (!nodes.Contains(state.Id)) nodes.Add(state.Id);
                        }
                    }
                }
            }

            return nodes.ToArray();
        }

        private int[] GetTransitions(Node n, char symbol)
        {
            List<int> nodes = new List<int>();

            foreach (var edge in n.Edges)
            {
                if (edge.Transition == symbol)
                {
                    if (!nodes.Contains(edge.End.Id))
                        nodes.Add(edge.End.Id);
                }
            }

            return nodes.ToArray();
        }

        private EdgeInfo[] NFAEdgeInfo(int[,][] deltaM, int nodesCount, int alphabetLength)
        {
            List<EdgeInfo> edgeInfo = new List<EdgeInfo>();
            for (int i = 0; i < nodesCount; i++)
            {
                for (int j = 0; j < alphabetLength; j++)
                {
                    foreach (var item in deltaM[i, j])
                    {
                        edgeInfo.Add(new EdgeInfo(i, item, alphabet[j]));
                    }
                }
            }
            return edgeInfo.ToArray();
        }

        private void RenameNodes()
        {
            int index = 0;
            Dictionary<int, Node> newIdsDictionary = new Dictionary<int, Node>();
            Dictionary<int, int> newIds = new Dictionary<int, int>();

            //Console.WriteLine("\tRename: ");
            foreach (var node in ids.Values)
            {
                //Console.WriteLine("\t\tOld: " + node.Id + " New: " + index);
                newIds.Add(node.Id, index);
                newIdsDictionary.Add(index, node);
                index++;
            }

            foreach (var oldName in newIds.Keys)
            {
                ids[oldName].SetId(newIds[oldName]);
            }

            List<int> finalStates = new List<int>();
            foreach (var node in finalNodes)
            {
                if (newIds.ContainsKey(node))
                    finalStates.Add(newIds[node]);
            }

            ids = newIdsDictionary;
            SetFinalStates(finalStates.ToArray());

            startId = newIds[startId];
            endId = newIds[endId];
        }

        private void ResetGraphInfo(int startId, int endId, EdgeInfo[] edges, int[] finalStates)
        {
            Graph temp = new Graph(startId, endId, edges);
            ids = temp.Ids;
            this.startId = startId;
            this.endId = endId;
            SetFinalStates(finalStates);
            RenameNodes();
        }

        private void DefineAutomata()
        {
            Stack<NodeSet> stack = new Stack<NodeSet>();
            List<NodeSet> q = new List<NodeSet>();
            int[,][] transitionTable = new int[ids.Count, alphabet.Length][];
            foreach (var state in ids.Values)
            {
                for (int i = 0; i < alphabet.Length; i++)
                {
                    transitionTable[state.Id, i] = GetTransitions(state, alphabet[i]);
                }
            }

            stack.Push(new NodeSet());
            stack.Peek().AddNode(startId, ids[startId].IsFinal);
            stack.Peek().CalculateTransitions(alphabet.Length, transitionTable, finalNodes);
            q.Add(stack.Peek());

            foreach (var state in ids.Values)
            {
                if (state.Id == startId) continue;
                stack.Push(new NodeSet());
                stack.Peek().AddNode(state.Id, ids[state.Id].IsFinal);
                stack.Peek().CalculateTransitions(alphabet.Length, transitionTable, finalNodes);
                q.Add(stack.Peek());
            }

            while (stack.Count > 0)
            {
                NodeSet set = stack.Pop();
                for (int i = 0; i < alphabet.Length; i++)
                {
                    if (set.Transitions == null) continue;
                    if (!q.Contains(set.Transitions[i]))
                    {
                        stack.Push(set.Transitions[i]);
                        stack.Peek().CalculateTransitions(alphabet.Length, transitionTable, finalNodes);
                        q.Add(stack.Peek());
                    }
                }
            }
            int[] finalStates;
            var edges = DFAEdgeInfo(q, out finalStates);
            ResetGraphInfo(0, finalStates[0], edges, finalStates);
        }

        private EdgeInfo[] DFAEdgeInfo(List<NodeSet> q, out int[] finalStates)
        {
            List<int> fStates = new List<int>();

            List<EdgeInfo> edgeInfo = new List<EdgeInfo>();
            int i = 0;
            foreach (NodeSet set in q)
            {
                if (set.IsFinal) fStates.Add(i);
                edgeInfo.AddRange(set.CalculateEdgeInfo(in q, alphabet));
                i++;
            }

            finalStates = fStates.ToArray();
            return edgeInfo.ToArray();
        }

        private void ClearInaccessibleStates()
        {
            List<int> inaccessibleStates = new List<int>();
            List<int> visitedStates = new List<int>();
            Stack<Node> toVisit = new Stack<Node>();
            toVisit.Push(ids[startId]);

            while (toVisit.Count > 0)
            {
                var state = toVisit.Pop();
                visitedStates.Add(state.Id);
                foreach (var edge in state.Edges)
                {
                    if (visitedStates.Contains(edge.End.Id))
                        continue;
                    toVisit.Push(edge.End);
                }
            }

            foreach (var node in ids.Values)
            {
                if (visitedStates.Contains(node.Id))
                    continue;
                inaccessibleStates.Add(node.Id);
            }

            RemoveStates(inaccessibleStates);
        }

        private void ClearDeadEndStates()
        {
            List<int> deadEndStates = new List<int>();

            foreach (var node in ids.Values)
            {
                if (node.IsStart) continue;
                if (node.IsFinal) continue;
                if ((node.Edges.Count == 0) ||
                    (node.Edges.Count == 1 && node.Edges[0].End.Id == node.Id))
                    deadEndStates.Add(node.Id);
            }

            RemoveStates(deadEndStates);
        }

        private void SetFinalStates(int[] finalStates)
        {
            finalNodes = finalStates;
            foreach (var item in finalNodes)
            {
                if (ids.ContainsKey(item))
                    ids[item].IsFinal = true;
            }
        }

        private void RemoveStates(List<int> keys)
        {
            List<EdgeInfo> info = new List<EdgeInfo>();
            foreach (var node in ids.Values)
            {
                if (keys.Contains(node.Id)) continue;
                foreach (var edge in node.Edges)
                {
                    if (keys.Contains(edge.End.Id))
                        continue;
                    info.Add(new EdgeInfo(edge.Start.Id, edge.End.Id, edge.Transition));
                }
            }

            ResetGraphInfo(startId, endId, info.ToArray(), finalNodes);
        }
    }
}
