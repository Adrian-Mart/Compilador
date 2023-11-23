using Compilador.Graph;
using Compilador.Processors.Parser;

namespace Compilador.Calculator;
internal class AbstractTree
{
    private Node root;

    internal AbstractTree(Tree tree, ParserSetup setup)
    {
        TrimTree(tree, setup);
        root = new Node(GetType(tree.Root, setup));
        CopyTree(tree, setup);
    }

    private void TrimTree(Tree tree, ParserSetup setup)
    {
        int expIndex = setup.GetIndexOf("EXPRESSION");
        int termIndex = setup.GetIndexOf("TERM");

        int endIndex = tree.Root.Children.Count - 1;


        tree.Root = tree.Root.Children[0].Children[0].Children[0];
        bool chages = true;
        while (chages)
        {
            chages = false;
            foreach (var node in tree.CalculatePostorder())
            {
                if (node.Data == "/" || node.Data == "*" ||
                    node.Data == "+" || node.Data == "-" ||
                    node.Data == "(" || node.Data == ")")
                {
                    node.Parent?.Children.Remove(node);
                    chages = true;
                    break;
                }
                else if (node.Value == expIndex || node.Value == termIndex)
                {
                    node.Parent?.Children.Remove(node);

                    if (node.Children.Count != 1)
                        throw new Exception("Invalid tree.");

                    node.Parent?.Children.Add(node.Children[0]);
                    chages = true;
                    break;
                }
            }
        }
    }

    private void CopyTree(Tree tree, ParserSetup setup)
    {
        AsignChilds(root, tree, tree.Root, setup);
    }

    private void AsignChilds(Node n, Tree tree, SimpleNode node, ParserSetup setup)
    {
        if (node.Children.Count == 0)
            return;

        n.Left = new Node(GetType(node.Children[0], setup));
        n.Right = new Node(GetType(node.Children[1], setup));
        AsignChilds(n.Left, tree, node.Children[0], setup);
        AsignChilds(n.Right, tree, node.Children[1], setup);
    }

    private string GetType(SimpleNode n, ParserSetup setup)
    {
        if (n.Value == setup.GetIndexOf("ADD"))
            return "add";
        else if (n.Value == setup.GetIndexOf("SUB"))
            return "sub";
        else if (n.Value == setup.GetIndexOf("MUL"))
            return "mul";
        else if (n.Value == setup.GetIndexOf("DIV"))
            return "div";
        return n.Data;
    }

    public List<Node> Postorder()
    {
        List<Node> list = new List<Node>();
        Postorder(root, list);
        return list;
    }

    private void Postorder(Node? node, List<Node> list)
    {
        if (node == null)
            return;

        Postorder(node.Left, list);
        Postorder(node.Right, list);
        list.Add(node);
    }
}