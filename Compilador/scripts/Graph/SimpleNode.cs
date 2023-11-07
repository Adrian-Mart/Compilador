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
    internal class SimpleNode
    {
        /// <summary>
        /// The value of the node.
        /// </summary>
        private int value;

        private string data;

        /// <summary>
        /// The depth of the node in the graph.
        /// </summary>
        private int depth;

        /// <summary>
        /// A value indicating whether the node is a leaf.
        /// </summary>
        private bool isLeaf = false;

        /// <summary>
        /// The parent of the node. If null, the node is the root node.
        /// </summary>
        private SimpleNode? parent = null;

        /// <summary>
        /// The list of children of the node.
        /// </summary>
        private List<SimpleNode> children;

        /// <summary>
        /// Gets the value of the node.
        /// </summary>
        public int Value { get => value; }

        /// <summary>
        /// Gets the value of the node.
        /// </summary>
        public string Data { get => data; }

        /// <summary>
        /// Gets the list of children of the node.
        /// </summary>
        internal List<SimpleNode> Children { get => children; set => children = value; }

        /// <summary>
        /// Gets a value indicating whether the node is a leaf.
        /// </summary>
        public bool IsLeaf { get => isLeaf; set => isLeaf = value;}

        /// <summary>
        /// Gets the parent of the node. If null, the node is the root node.
        /// </summary>
        internal SimpleNode? Parent { get => parent; }

        /// <summary>
        /// Gets the depth of the node in the graph.
        /// </summary>
        public int Depth { get => depth; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleNode"/> class with the specified value.
        /// </summary>
        /// <param name="value">The value of the node.</param>
        /// <param name="parent">The parent of the node.</param>
        internal SimpleNode(int value, string data, SimpleNode? parent)
        {
            this.value = value;
            this.parent = parent;
            this.data = data;
            children = new List<SimpleNode>();

            depth = parent == null ? 0 : parent.depth + 1;
        }

        internal void UpdateDepth()
        {
            depth = parent == null ? 0 : parent.depth + 1;
        }

        internal void AddChild(SimpleNode node)
        {
            children.Insert(0, node);
            node.parent = this;
            node.depth = depth + 1;
        }

        /// <summary>
        /// Removes the specified child from the list of children of the node.
        /// </summary>
        /// <param name="value">The value of the child to remove.</param>
        /// <returns>A value indicating whether the child was removed.</returns>
        internal bool RemoveChild(int value)
        {
            var child = children.Find(x => x.value == value);
            if (child == null)
                return false;

            children.Remove(child);
            return true;
        }

        /// <summary>
        /// Removes the specified child from the list of children of the node.
        /// </summary>
        /// <param name="child">The child to remove.</param>
        /// <returns>A value indicating whether the child was removed.</returns>
        internal bool RemoveChild(SimpleNode child)
        {
            return children.Remove(child);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string? ToString()
        {
            return NodeToString("", true, new StringBuilder()).ToString();
        }

        public void ReplaceChild(SimpleNode oldChild, SimpleNode newChild)
        {
            int index = children.IndexOf(oldChild);
            children[index] = newChild;
            newChild.parent = this;
        }
        
        /// <summary>
        /// Stores in an <see cref="StringBuilder"/> the children of the node
        /// in a tree-like fashion. Based on the answer by
        /// <see href="https://stackoverflow.com/users/15721/will">Will</see>
        /// (answered Oct 30, 2009 at 11:12), edited by
        /// <see href="https://stackoverflow.com/users/69809/vgru">vgru</see>
        /// (edited Oct 30, 2009 at 11:35) in
        /// <see href="https://stackoverflow.com/a/1649027/12347616">this</see>
        /// StackOverflow question.
        /// </summary>
        /// <param name="indent">The string to use as the indent for the
        /// current node.</param>
        /// <param name="last">A value indicating whether the current node is the
        /// last child of its parent.</param>
        public StringBuilder NodeToString(string indent, bool last, StringBuilder sb)
        {
            sb.Append(indent);
            if(parent == null)
            {
                sb.Append("->");
                indent += "  ";
            }
            else if (last)
            {
                sb.Append("└─");
                indent += "  ";
            }
            else
            {
                sb.Append("|-");
                indent += "| ";
            }
            sb.AppendLine($"<{value} : {data}>");

            for (int i = 0; i < Children.Count; i++)
                sb = Children[i].NodeToString(indent, i == Children.Count - 1, sb);

            return sb;
        }

        internal void AddChildren(List<SimpleNode> children)
        {
            foreach (var child in children.Reverse<SimpleNode>())
            {
                this.children.Insert(0, child);
                child.parent = this;
            }
        }
    }
}
