using Compilador.Graph;
using Compilador.Processors.Parser;

namespace Compilador.Quackier;
internal class AbstractTree
{
    private Node root;

    public Node Root { get => root; }

    internal AbstractTree(Tree tree, ParserSetup setup)
    {
        TrimTree(tree, setup);
        JoinSenteces(tree, setup);
        SetDeclaration(tree, setup);
        SetFlow(tree, setup);
        SetValue(tree, setup);
        SetAsignation(tree, setup);
        root = new Node(GetType(tree.Root, setup));
        CopyTree(tree, setup);
    }

    private void SetAsignation(Tree tree, ParserSetup setup)
    {
        int asignationIndex = setup.GetIndexOf("ASSIGNMENT");
        foreach (var node in tree.CalculatePostorder())
        {
            if (node.Value == asignationIndex)
            {
                node.Data = node.Children[1].Data;
                node.Children.RemoveAt(1);
            }
        }
    }

    private void SetValue(Tree tree, ParserSetup setup)
    {
        int expIndex = setup.GetIndexOf("EXPRESSION");
        int termIndex = setup.GetIndexOf("TERM");
        int valueIndex = setup.GetIndexOf("VALUE");

        bool changes = true;
        while (changes)
        {
            changes = false;
            foreach (var node in tree.CalculatePostorder())
            {
                if (node.Value == expIndex || node.Value == termIndex)
                {
                    node.Parent?.Children.Remove(node);
                    node.Parent?.Children.AddRange(node.Children);
                    changes = true;
                    break;
                }
            }
        }

        foreach (var node in tree.CalculatePostorder())
        {
            if (node.Value == valueIndex)
            {
                node.Data = node.Children[0].Data;
                node.Children.RemoveAt(0);
            }
        }
    }

    private void SetFlow(Tree tree, ParserSetup setup)
    {
        int flowIndex = setup.GetIndexOf("FLOW");
        foreach (var node in tree.CalculatePostorder())
        {
            if (node.Value == flowIndex)
            {
                node.Data = node.Children[0].Children[0].Data;
                node.AddChild(node.Children[0].Children[1]);
                node.Children.RemoveAt(1);
            }
        }
    }

    private void SetDeclaration(Tree tree, ParserSetup setup)
    {
        int declarationIndex = setup.GetIndexOf("DECLARATION");
        foreach (var node in tree.CalculatePostorder())
        {
            if (node.Value == declarationIndex)
            {
                node.Data = node.Children[0].Data;
                node.Children.RemoveAt(0);
                node.Children.RemoveAt(1);
            }
        }
    }

    private void JoinSenteces(Tree tree, ParserSetup setup)
    {
        int sentencesIndex = setup.GetIndexOf("SENTENCES"); bool changes = true;
        while (changes)
        {
            changes = false;
            foreach (var node in tree.CalculatePostorder())
            {
                if (node.Value == sentencesIndex && node.Parent?.Value == sentencesIndex)
                {
                    changes = true;
                    node.Parent.ReplaceChild(node, node.Children);
                    break;
                }
            }
        }
    }

    private void TrimTree(Tree tree, ParserSetup setup)
    {
        int expIndex = setup.GetIndexOf("EXPRESSION");
        int termIndex = setup.GetIndexOf("TERM");
        int sentenceIndex = setup.GetIndexOf("SENTENCE");
        int newlineIndex = setup.GetIndexOf("new_line");
        int openIndex = setup.GetIndexOf("{");
        int closeIndex = setup.GetIndexOf("}");

        int endIndex = tree.Root.Children.Count - 1;
        tree.Root = tree.Root.Children[0];

        bool changes = true;
        while (changes)
        {
            changes = false;
            foreach (var node in tree.CalculatePostorder())
            {
                if (node.Value == newlineIndex ||
                    node.Value == openIndex ||
                    node.Value == closeIndex ||
                    node.Data == "(" || node.Data == ")")
                {
                    changes = true;
                    node.Parent?.Children.Remove(node);
                    break;
                }
                else if (node.Data == "/" || node.Data == "*" ||
                    node.Data == "+" || node.Data == "-" || node.Data == "=="
                    || node.Data == "!=" || node.Data == "<" || node.Data == ">")
                {
                    changes = true;
                    if (node.Parent == null)
                        break;

                    node.Parent.Children.Remove(node);
                    node.Parent.Data = $"Op:{node.Data}";
                    break;
                }
                else if (node.Value == sentenceIndex)
                {
                    changes = true;
                    node.Parent?.Children.Remove(node);
                    node.Parent?.AddChildren(node.Children);
                    break;
                }
                // else if (node.Value == expIndex || node.Value == termIndex)
                // {
                //     node.Parent?.Children.Remove(node);
                //     node.Parent?.Children.AddRange(node.Children);
                //     chages = true;
                //     break;
                // }
            }
        }
    }
    override public string? ToString()
    {
        return root.ToString();
    }

    private void CopyTree(Tree tree, ParserSetup setup)
    {
        root = AsignChilds(root, tree, tree.Root, setup);
    }

    private Node AsignChilds(Node n, Tree tree, SimpleNode node, ParserSetup setup)
    {
        if (node.Children.Count == 0)
            return n;

        if (node.Value == setup.GetIndexOf("SENTENCES"))
        {
            SentencesNode sentences = new SentencesNode("Sentences");
            foreach (var child in node.Children)
            {
                var nChild = AsignChilds(new Node(GetType(child, setup)), tree, child, setup);
                sentences.Nodes.Add(nChild);
            }
            return sentences;
        }

        n.Left = AsignChilds(new Node(GetType(node.Children[0], setup)), tree, node.Children[0], setup);
        n.Right = AsignChilds(new Node(GetType(node.Children[1], setup)), tree, node.Children[1], setup);
        return n;
    }

    private string GetType(SimpleNode n, ParserSetup setup)
    {   
        if (n.Value == setup.GetIndexOf("print"))
            return "print";
        else if (n.Value == setup.GetIndexOf("PRINT"))
            return "Print";
        else if (n.Value == setup.GetIndexOf("ASSIGNMENT"))
            return "Asign";
        else if (n.Data == "Op:+")
            return "add";
        else if (n.Data == "Op:-")
            return "sub";
        else if (n.Data == "Op:*")
            return "mul";
        else if (n.Data == "Op:/")
            return "div";
        else if (n.Data == "Op:==")
            return "evalEqual";
        else if (n.Data == "Op:!=")
            return "evalNotEqual";
        else if (n.Data == "Op:<")
            return "evalLess";
        else if (n.Data == "Op:>")
            return "evalGreater";
        else if (n.Data == "Op:and")
            return "evalAnd";
        else if (n.Data == "Op:or")
            return "evalOr";
        else if (n.Data == "quack_if")
            return "If";
        else if (n.Data == "quack_while")
            return "While";
        else if (n.Data == "real_quack")
            return "DeclareReal";
        else if (n.Data == "string_quack")
            return "DeclareString";

        return n.Data;
    }

    internal List<Node> Postorder()
        {
            List<Node> postorderTree = new List<Node>();
            List<Node> childrenAdded = new List<Node>();
            Stack<Node> stack = new Stack<Node>();
            stack.Push(root);

            while (stack.Count > 0)
            {
                var current = stack.Peek();
                List<Node> children;
                if (current is SentencesNode)
                    children = ((SentencesNode)current).Nodes;
                else
                {
                    children = new List<Node>();
                    if (current.Left != null)
                        children.Add(current.Left);
                    if (current.Right != null)
                        children.Add(current.Right);
                }

                if (children.Count == 0 || childrenAdded.Contains(current))
                {
                    postorderTree.Add(current);
                    stack.Pop();
                }
                else
                {
                    for (int i = children.Count - 1; i >= 0; i--)
                        stack.Push(children[i]);
                    childrenAdded.Add(current);
                }
            }

            return postorderTree;
        }
}