using Compilador.Graph;

namespace Compilador.Processors.Parser
{
    /// <summary>
    /// Represents a syntax tree.
    /// </summary>
    public class SyntaxTree : Tree
    {
        /// <summary>
        /// The value of the empty symbol.
        /// </summary>
        private int emptyValue;

        /// <summary>
        /// The list of nodes in postorder.
        /// </summary>
        private List<SimpleNode> postorderTree;

        /// <summary>
        /// Initializes a new instance of the <see cref="SyntaxTree"/> class with the specified root value.
        /// </summary>
        /// <param name="rootValue">The value of the root of the tree.</param>
        /// <param name="emptyValue">The index of the empty symbol.</param>
        public SyntaxTree(int rootValue, int emptyValue) : base(rootValue)
        {
            this.emptyValue = emptyValue;
            postorderTree = new List<SimpleNode>();
        }

        public void AbstractTree(int[]? hierarchySymbols, Operator[]? operators, int[] nonTerminals)
        {
            ClearEmptyBranches();
            if (hierarchySymbols != null)
                RemoveHierarchySymbols(hierarchySymbols);
            TrimBranches();
            if (operators != null)
            {
                SetOperators(operators);
                TrimNonTerminals(nonTerminals);
            }
        }

        private void SetOperators(Operator[] operators)
        {
            int[] operatorSymbols = GetOperators(operators);
            RecalculateLeafs();

            // Console.WriteLine($"\nTree:\n{this.ToString()}");
            if (root.IsLeaf) return;

            var visitedNodes = new HashSet<SimpleNode>();
            foreach (var leaf in leafs.Reverse<SimpleNode>())
            {
                if (!operatorSymbols.Contains(leaf.Value))
                    continue;
                var operatorData = GetOperator(leaf, operators);
                if (operatorData == null) continue;

                if (operatorData.IsBinary)
                {
                    var grandParent = leaf.Parent?.Parent;
                    if (grandParent == null)
                        throw new Exception("The grandparent of the leaf is null.");
                    leaf.Parent?.RemoveChild(leaf);
                    leaf.Children = grandParent.Children;
                    grandParent.Parent?.ReplaceChild(grandParent, leaf);

                    if (grandParent == root)
                    {
                        root = leaf;
                        root.Parent = null;
                    }
                }
                else
                {
                    var parent = leaf.Parent;
                    if (parent == null)
                        throw new Exception("The parent of the leaf is null.");
                    leaf.Parent?.RemoveChild(leaf);
                    leaf.Children = parent.Children;
                    parent.Parent?.ReplaceChild(parent, leaf);

                    if (parent == root)
                    {
                        root = leaf;
                        root.Parent = null;
                    }
                }

                // Console.WriteLine($"\nTree:\n{this.ToString()}");
            }
            // Console.WriteLine($"\nTree:\n{this.ToString()}");

            RecalculateLeafs();
        }

        private int[] GetOperators(Operator[] operators)
        {
            return operators.Select(op => op.Symbol).ToArray();
        }

        private Operator? GetOperator(SimpleNode node, Operator[] operators)
        {
            return operators.FirstOrDefault(op => op.Symbol == node.Value);
        }

        private void RemoveHierarchySymbols(int[] hierarchySymbols)
        {
            foreach (var leaf in leafs)
            {
                if (hierarchySymbols.Contains(leaf.Value))
                    leaf.Parent?.RemoveChild(leaf);
            }

            RecalculateLeafs();
        }

        /// <summary>
        /// Removes the nodes that have only one child
        /// and are not the root or a leaf.
        /// </summary>
        private void TrimBranches()
        {
            if (root.Children.Count == 1)
            {
                root = root.Children[0];
                root.Parent = null;
            }
            foreach (var node in CalculatePreorder())
            {
                if (node.Parent != null && node.Parent.Children.Count == 1)
                {
                    var grandParent = node.Parent.Parent;
                    grandParent?.ReplaceChild(node.Parent, node);
                }
            }

            RecalculateLeafs();
        }

        private void TrimNonTerminals(int[] nonTerminals)
        {
            if (root.IsLeaf) return;

            foreach (var node in CalculatePreorder())
            {
                if (!nonTerminals.Contains(node.Value))
                    continue;

                if (node.Parent != null && node.Children.Count == 1)
                {
                    node.Parent?.ReplaceChild(node, node.Children[0]);
                }
                else if (node.Parent == null)
                {
                    root = node.Children[0];
                    root.Parent = null;
                }
                else
                    throw new Exception("A non-terminal node has more than one child.");
            }

            RecalculateLeafs();
        }

        /// <summary>
        /// Removes the empty branches of the tree.
        /// </summary>
        private void ClearEmptyBranches()
        {
            // For each leaf in the tree
            foreach (var leaf in leafs)
            {
                // If the leaf is the empty symbol
                if (leaf.Value == emptyValue)
                {
                    // Remove the leaf and its parent
                    var parent = leaf.Parent;
                    parent?.RemoveChild(leaf);

                    // while the parent is not null and it has no children
                    while (parent != null && parent.Children.Count == 0)
                    {
                        // Remove the parent and set it as the grandparent
                        var grandParent = parent.Parent;
                        grandParent?.RemoveChild(parent);
                        parent = grandParent;
                    }
                }
            }

            RecalculateLeafs();
        }

        private List<SimpleNode> CalculatePreorder()
        {
            List<SimpleNode> preorderTree = new List<SimpleNode>();
            Stack<SimpleNode> stack = new Stack<SimpleNode>();
            stack.Push(root);

            while (stack.Count > 0)
            {
                SimpleNode current = stack.Pop();
                preorderTree.Add(current);

                for (int i = current.Children.Count - 1; i >= 0; i--)
                {
                    stack.Push(current.Children[i]);
                }
            }

            // Console.Write("Preorder: ");
            // Console.WriteLine(string.Join(", ", preorderTree.Select(l => l.Value)));
            return preorderTree;
        }

        private void CalculatePostorder()
        {
            postorderTree.Clear();
            List<SimpleNode> childrenAdded = new List<SimpleNode>();
            Stack<SimpleNode> stack = new Stack<SimpleNode>();
            stack.Push(root);

            while (stack.Count > 0)
            {
                var current = stack.Peek();
                if (current.Children.Count == 0 || childrenAdded.Contains(current))
                {
                    postorderTree.Add(current);
                    stack.Pop();
                }
                else
                {
                    var children = current.Children;
                    for (int i = children.Count - 1; i >= 0; i--)
                        stack.Push(children[i]);
                    childrenAdded.Add(current);
                }
            }

            // Console.Write("Postorder: ");
            // Console.WriteLine(string.Join(", ", postorderTree.Select(l => l.Value)));
        }

        private void RecalculateLeafs()
        {
            CalculatePostorder();
            leafs.Clear();
            foreach (var node in postorderTree)
            {
                if (node.Children.Count == 0)
                {
                    leafs.Add(node);
                    node.IsLeaf = true;
                }
                else node.IsLeaf = false;
            }
            // Console.Write("Leafs: ");
            // Console.WriteLine(string.Join(", ", leafs.Select(l => l.Value)));
        }
    }
}