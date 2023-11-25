using System.Text;

namespace Compilador.Quackier;

internal class Node
{
    private protected Node? left;
    private protected Node? right;
    private protected string value;
    private string? variable;
    private string? code;
    internal Node? Left { get => left; set => left = value; }
    internal Node? Right { get => right; set => right = value; }
    internal string Value { get => value; }
    internal string? Variable { get => variable; set => variable = value; }
    internal string? Code { get => code; set => code = value; }

    internal Node(string value)
    {
        this.value = value;
    }

    public override string? ToString()
    {
        return NodeToString("", true, new StringBuilder()).ToString();
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
    public virtual StringBuilder NodeToString(string indent, bool last, StringBuilder sb)
    {
        sb.Append(indent);
        if (last)
        {
            sb.Append("└─");
            indent += "  ";
        }
        else
        {
            sb.Append("|-");
            indent += "| ";
        }
        sb.AppendLine($"{value}");

        sb = Left?.NodeToString(indent, false, sb) ?? sb;
        sb = Right?.NodeToString(indent, true, sb) ?? sb;

        return sb;
    }
}

internal class SentencesNode : Node
{
    private List<Node> nodes = new List<Node>();
    internal List<Node> Nodes { get => nodes; set => nodes = value; }
    internal SentencesNode(string value) : base(value) { }

    public override StringBuilder NodeToString(string indent, bool last, StringBuilder sb)
    {
        sb.Append(indent);
        if (last)
        {
            sb.Append("└─");
            indent += "  ";
        }
        else
        {
            sb.Append("|-");
            indent += "| ";
        }
        sb.AppendLine($"{value}");

        for(int i = 0; i < nodes.Count; i++)
            sb = nodes[i].NodeToString(indent, i == nodes.Count - 1, sb);

        return sb;
    }
}