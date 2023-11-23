namespace Compilador.Calculator;

internal class Node
{
    private protected Node? left;
    private protected Node? right;
    private protected string value;
    private string? variable;
    internal Node? Left { get => left; set => left = value;}
    internal Node? Right { get => right; set => right = value;}
    internal string Value { get => value; }
    internal string? Variable { get => variable; set => variable = value; }

    internal Node(string value)
    {
        this.value = value;
    }

    internal Node(string value, Node left, Node right)
    {
        this.value = value;
        this.left = left;
        this.right = right;
    }
}