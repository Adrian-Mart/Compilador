
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
        /// Initializes a new instance of the <see cref="Tree"/>
        /// class with the specified root value.
        /// </summary>
        public Tree(int rootValue)
        {
            root = new SimpleNode(rootValue, null) { IsLeaf = true };
            leafs = new List<SimpleNode> { root };
        }

        /// <summary>
        /// Adds a leaf to the tree.
        /// </summary>
        /// <param name="value">The value of the leaf.</param>
        /// <param name="parentValue">The value of the parent of the leaf.</param>
        internal void AddLeaf(int value, SimpleNode parent)
        {
            // Create the leaf
            SimpleNode leaf = new SimpleNode(value, null);

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
    }
}