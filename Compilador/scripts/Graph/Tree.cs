
namespace Compilador.Graph
{
    /// <summary>
    /// Represents a tree.
    /// </summary>
    public class Tree
    {
        /// <summary>
        /// The root of the tree.
        /// </summary>
        private protected SimpleNode root;

        /// <summary>
        /// The list of leafs of the tree.
        /// </summary>
        private protected List<SimpleNode> leafs;

        /// <summary>
        /// The root of the tree.
        /// </summary>
        internal SimpleNode Root { get => root; set => root = value; }

        /// <summary>
        /// The list of leafs of the tree.
        /// </summary>
        internal List<SimpleNode> Leafs { get => leafs; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Tree"/>
        /// class with the specified root value.
        /// </summary>
        public Tree(int rootValue, string data = "")
        {
            root = new SimpleNode(rootValue, data, null) { IsLeaf = true };
            leafs = new List<SimpleNode> { root };
        }

        /// <summary>
        /// Adds a leaf to the tree.
        /// </summary>
        /// <param name="value">The value of the leaf.</param>
        /// <param name="parentValue">The value of the parent of the leaf.</param>
        internal void AddLeaf(int value, string data, SimpleNode parent)
        {
            // Create the leaf
            SimpleNode leaf = new SimpleNode(value, data, null);

            // Add the leaf to the parent
            parent.AddChild(leaf);
            parent.IsLeaf = false;

            // Remove the parent from the leafs list and add the leaf
            leafs.Remove(parent);
            leaf.IsLeaf = true;
            leafs.Add(leaf);
        }

        /// <summary>
        /// Finds the leaf with the specified value.
        /// </summary>
        /// <param name="value">The value of the leaf.</param>
        internal SimpleNode? FindLeaf(int value)
        {
            return leafs.FindLast(node => node.Value == value);
        }

        override public string? ToString()
        {
            return root.ToString();
        }
        
        private void UpdateDepth()
        {
            Queue<SimpleNode> queue = new Queue<SimpleNode>();
            queue.Enqueue(root);
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                current.UpdateDepth();
                foreach (var child in current.Children)
                    queue.Enqueue(child);
            }
        }

        internal List<List<SimpleNode>> GetTreeLevels()
        {
            UpdateDepth();
            List<int> levelsDepth = new List<int>();
            List<List<SimpleNode>> levels = new List<List<SimpleNode>>();
            var postorderTree = CalculatePostorder();
            foreach (var node in postorderTree)
            {
                if (levelsDepth.Contains(node.Depth))
                    levels[levelsDepth.IndexOf(node.Depth)].Add(node);
                else
                {
                    levelsDepth.Add(node.Depth);
                    levels.Add(new List<SimpleNode>() { node });
                }
            }
            return levelsDepth.Select((x, i) => new { Index = x, Value = levels[i] })
                   .OrderBy(x => x.Index)
                   .Select(x => x.Value)
                   .ToList();
        }

        internal List<SimpleNode> CalculatePostorder()
        {
            List<SimpleNode> postorderTree = new List<SimpleNode>();
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
            return postorderTree;
        }
    }
}