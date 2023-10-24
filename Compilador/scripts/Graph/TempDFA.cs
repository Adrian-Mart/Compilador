using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Compilador.Graph
{
    public class TempDFA : Graph
    {
        /// <summary>
        /// The alphabet of the automata.
        /// </summary>
        private int[] alphabet;
        /// <summary>
        /// The final nodes of the automata.
        /// </summary>
        private int[] finalNodes;
        private int epsilonId = -1;
        /// <summary>
        /// The optimized automata.
        /// </summary>
        private DFA automata;
        /// <summary>
        /// Gets the optimized automata.
        /// </summary>
        public DFA Automata { get => automata; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TempDFA"/> class.
        /// </summary>
        /// <param name="startId">Start node ID.</param>
        /// <param name="endId">End node ID.</param>
        /// <param name="edges">Edges of the graph.</param>
        /// <param name="alphabet">The alphabet of the automata.</param>
        public TempDFA(int startId, int endId, EdgeInfo[] edges, int[] alphabet, int epsilonId) : base(startId, endId, edges)
        {
            this.alphabet = alphabet;
            this.epsilonId = epsilonId;
            finalNodes = new int[] { endId };
            ClearEpsilonTransitions();
            ClearDeadEndStates();
            ClearInaccessibleStates();
            DefineAutomata();
            ClearDeadEndStates();
            ClearInaccessibleStates();
            // Optimiza the automata
            automata = new DFA(this.alphabet, this.ids, this.startId);
        }

        /// <summary>
        /// Converts the NFA-Epsilon to a NFA without epsilon transitions.
        /// </summary>
        private void ClearEpsilonTransitions()
        {
            // Store the number of nodes in the graph
            int nodesCount = ids.Count;

            // Create the deltaM set
            int[,][] deltaM = new int[nodesCount, alphabet.Length][];

            // Create a dictionary for the eClousure set of each node
            Dictionary<int, Node[]> eClousure = new Dictionary<int, Node[]>();
            // Calculate the eClousure set for each node
            foreach (var stateId in ids.Keys)
            {
                eClousure.Add(stateId, GetEClousure(ids[stateId]));
            }

            // Calculate the deltaM set
            foreach (var stateId in ids.Keys)
            {
                for (int i = 0; i < alphabet.Length; i++)
                {
                    deltaM[stateId, i] = GetTransitions(eClousure, ids[stateId], alphabet[i]);
                }
            }

            // Create an edge info array with the new transitions
            EdgeInfo[] info = NFAEdgeInfo(deltaM, nodesCount, alphabet.Length);

            // Define the final states
            int[] finalStates;
            // If the start contains the end node in its eclousure set,
            // then the start is also a final state
            if (eClousure[startId].Contains(ids[endId]))
                finalStates = new int[] { startId, endId};
            else
                finalStates = new int[] { endId };

            // Reset the graph info
            ResetGraphInfo(startId, endId, info, finalStates);
        }

        /// <summary>
        /// Gets the eClousure set of a node.
        /// </summary>
        /// <param name="n">Node to calculate the eClousure set.</param>
        /// <returns>The eClousure set of the node.</returns>
        private Node[] GetEClousure(Node n)
        {
            List<Node> nodes = new List<Node>();
            Stack<Node> stack = new Stack<Node>();
            nodes.Add(n);
            stack.Push(n);

            // Makes a travel through the graph to get the eClousure set
            // The travel is made only through epsilon transitions
            Node temp;
            do
            {
                temp = stack.Pop();
                foreach (var edge in temp.Edges)
                {
                    if (edge.Transition == epsilonId && !nodes.Contains(edge.End))
                    {
                        nodes.Add(edge.End);
                        stack.Push(edge.End);
                    }
                }
            } while (stack.Count > 0);

            return nodes.ToArray();
        }

        /// <summary>
        /// Gets the ids of the nodes from witch the node n can arrive using
        /// only the symbol or epsilon transitions.
        /// </summary>
        /// <param name="eClousures">The eClousure set of every node.</param>
        /// <param name="n">The node to calculate the transitions.</param>
        /// <param name="symbol">The symbol to calculate the transitions.</param>
        /// <returns>An array with the ids of the nodes from witch the node n can arrive.</returns>
        private int[] GetTransitions(Dictionary<int, Node[]> eClousures, Node n, int symbol)
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

        /// <summary>
        /// Gets the ids of the nodes from witch the node n can arrive
        /// using only the symbol transitions.
        /// </summary>
        /// <param name="n">The node to calculate the transitions.</param>
        /// <param name="symbol">The symbol to calculate the transitions.</param>
        /// <returns>An array with the ids of the nodes from witch the node n can arrive.</returns>
        private int[] GetTransitions(Node n, int symbol)
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

        /// <summary>
        /// Gets the edge info of the NFA without epsilon transitions. Uses the deltaM set 
        /// to determine the new transitions.
        /// </summary>
        /// <param name="deltaM">The deltaM set.</param>
        /// <param name="nodesCount">The number of nodes in the graph.</param>
        /// <param name="alphabetLength">The lenght of the alphabet</param>
        /// <returns> An array with the edge info of the NFA without
        /// epsilon transitions.</returns>
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

        /// <summary>
        /// Renames the nodes of the graph to make them sequential.
        /// </summary>
        private void RenameNodes()
        {
            // The index of the new start id node
            int index = 0;
            // Create a new dictionary to store the new ids
            Dictionary<int, Node> newIdsDictionary = new Dictionary<int, Node>();
            // Create a new dictionary to store the new ids in relation to the old ones
            Dictionary<int, int> newIds = new Dictionary<int, int>();
            
            // For each node in the graph, add the new id to the dictionaries
            // and add the node to the new dictionary
            foreach (var node in ids.Values)
            {
                newIds.Add(node.Id, index);
                newIdsDictionary.Add(index, node);
                index++;
            }

            // Renames all of the nodes in the graph
            foreach (var oldName in newIds.Keys)
            {
                ids[oldName].SetId(newIds[oldName]);
            }

            // Stores the old final states with the new ids
            List<int> finalStates = new List<int>();
            foreach (var node in finalNodes)
            {
                if (newIds.ContainsKey(node))
                    finalStates.Add(newIds[node]);
            }

            // Set the new ids dictionary and final states
            ids = newIdsDictionary;
            SetFinalStates(finalStates.ToArray());

            // Set the new start and end ids
            startId = newIds[startId];
            endId = newIds[endId];
        }

        /// <summary>
        /// Resets the graph info.
        /// </summary>
        /// <param name="startId">Start node ID.</param>
        /// <param name="endId">End node ID.</param>
        /// <param name="edges">Edges of the graph.</param>
        /// <param name="finalStates">Array with the final states.</param>
        private void ResetGraphInfo(int startId, int endId, EdgeInfo[] edges, int[] finalStates)
        {
            Graph temp = new Graph(startId, endId, edges);
            ids = temp.Ids;
            this.startId = startId;
            this.endId = endId;
            SetFinalStates(finalStates);
            RenameNodes();
        }

        /// <summary>
        /// Converts the NFA to a DFA.
        /// </summary>
        private void DefineAutomata()
        {
            Stack<NodeSet> stack = new Stack<NodeSet>();
            List<NodeSet> q = new List<NodeSet>();

            // Gets the transition table
            int[,][] transitionTable = new int[ids.Count, alphabet.Length][];
            foreach (var state in ids.Values)
            {
                for (int i = 0; i < alphabet.Length; i++)
                {
                    transitionTable[state.Id, i] = GetTransitions(state, alphabet[i]);
                }
            }

            // Creates the start node set
            stack.Push(new NodeSet());
            stack.Peek().AddNode(startId, ids[startId].IsFinal);
            stack.Peek().CalculateTransitions(alphabet.Length, transitionTable, finalNodes);
            q.Add(stack.Peek());

            // Creates the other node sets
            foreach (var state in ids.Values)
            {
                if (state.Id == startId) continue;
                stack.Push(new NodeSet());
                stack.Peek().AddNode(state.Id, ids[state.Id].IsFinal);
                stack.Peek().CalculateTransitions(alphabet.Length, transitionTable, finalNodes);
                q.Add(stack.Peek());
            }

            // Calculates the transitions for each node set
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

            // Gets the final states and the new edges info
            int[] finalStates;
            var edges = DFAEdgeInfo(q, out finalStates);

            // Resets the graph info
            ResetGraphInfo(0, finalStates[0], edges, finalStates);
        }

        /// <summary>
        /// Calculates the edge information for the DFA.
        /// </summary>
        /// <param name="q">The Q set of the DFA.</param>
        /// <param name="finalStates">An array with the new final states.</param>
        /// <returns>The DFA Edge info </returns>
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

        /// <summary>
        /// Removes the inaccessible states from the graph.
        /// </summary>
        private void ClearInaccessibleStates()
        {
            List<int> inaccessibleStates = new List<int>();
            List<int> visitedStates = new List<int>();
            Stack<Node> toVisit = new Stack<Node>();
            toVisit.Push(ids[startId]);

            // While there are nodes to visit
            while (toVisit.Count > 0)
            {
                // Get the next node to visit
                var state = toVisit.Pop();
                // Add the nodes that can be reached
                // from the current node
                visitedStates.Add(state.Id);
                foreach (var edge in state.Edges)
                {
                    if (visitedStates.Contains(edge.End.Id))
                        continue;
                    toVisit.Push(edge.End);
                }
            }

            // Add the nodes that were not visited
            foreach (var node in ids.Values)
            {
                if (visitedStates.Contains(node.Id))
                    continue;
                inaccessibleStates.Add(node.Id);
            }

            // Remove the inaccessible states
            RemoveStates(inaccessibleStates);
        }

        /// <summary>
        /// Removes the dead end states from the graph.
        /// </summary>
        private void ClearDeadEndStates()
        {
            List<int> deadEndStates = new List<int>();

            // For each node in the graph
            foreach (var node in ids.Values)
            {
                // If the node is the start or final node, continue
                if (node.IsStart) continue;
                if (node.IsFinal) continue;
                // If the node has no edges or only edges that points to itself
                if ((node.Edges.Count == 0) ||
                    (node.Edges.Count == 1 && node.Edges[0].End.Id == node.Id))
                    // Add the node to the dead end states
                    deadEndStates.Add(node.Id);
            }

            // Remove the dead end states
            RemoveStates(deadEndStates);
        }

        
        /// <summary>
        /// Sets the final states of the TempDFA object.
        /// </summary>
        /// <param name="finalStates">An array of integers representing the final states.</param>
        private void SetFinalStates(int[] finalStates)
        {
            finalNodes = finalStates;
            foreach (var item in finalNodes)
            {
                if (ids.ContainsKey(item))
                    ids[item].IsFinal = true;
            }
        }

        /// <summary>
        /// Removes the states with the given keys from the graph and updates the graph info accordingly.
        /// </summary>
        /// <param name="keys">The list of state IDs to be removed.</param>
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
