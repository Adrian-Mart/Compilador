using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Compilador.Calculator;

internal class CodeGenerator
{
    private static string templateNum = @"
    mov dword [a], __float32__(<n1>)
    mov dword [b], __float32__(<n2>)
    
    fld dword [a]
    f<type> dword [b]
    fstp dword [<v1>]
    ";

    private static string templateLeft = @"
    mov eax, dword [<v1>]
    mov dword [a], eax
    mov dword [b], __float32__(<n2>)
    
    fld dword [a]
    f<type> dword [b]
    fstp dword [<v1>]
    ";

    private static string templateRight = @"
    mov dword [a], __float32__(<n1>)
    mov eax, dword [<v1>]
    mov dword [b], eax
    
    fld dword [a]
    f<type> dword [b]
    fstp dword [<v1>]
    ";

    private static string templateBoth = @"
    mov eax, dword [<n1>]
    mov dword [a], eax
    mov eax, dword [<n2>]
    mov dword [b], eax
    
    fld dword [a]
    f<type> dword [b]
    fstp dword [<v1>]
    ";

    private Stack<string> variables = new Stack<string>();
    private Stack<string> usedVariables = new Stack<string>();

    public string GenerateCode(AbstractTree tree)
    {
        variables.Clear();
        usedVariables.Clear();
        StringBuilder sb = new StringBuilder();
        
        foreach (var node in tree.Postorder())
            sb.AppendLine(BuildCode(node));

        return sb.ToString();
    }

    private string BuildCode(Node node)
    {
        if (node.Right is null && node.Left is null)
            return "";

        float n1;
        float n2;

        string v1;
        string v2;
        string code;

        bool left = float.TryParse(node.Left?.Value, out n1);
        bool right = float.TryParse(node.Right?.Value, out n2);

        if (right && left)
        {
            code = templateNum.Replace("<n1>", n1.ToString())
                              .Replace("<n2>", n2.ToString())
                              .Replace("<v1>", GetVariable(out v1));
            usedVariables.Push(v1);
        }
        else if (left)
        {
            v1 = usedVariables.Peek();
            code = templateRight.Replace("<n2>", node.Left?.Variable)
                                .Replace("<n1>", n1.ToString())
                                .Replace("<v1>", v1);
        }
        else if (right)
        {
            v1 = usedVariables.Peek();
            code = templateLeft.Replace("<n2>", n2.ToString())
                               .Replace("<n1>", node.Right?.Variable)
                               .Replace("<v1>", v1);
        }
        else
        {
            usedVariables.Pop();
            v1 = usedVariables.Peek();
            code = templateBoth.Replace("<n1>", node.Left?.Variable)
                               .Replace("<n2>", node.Right?.Variable)
                               .Replace("<v1>", node.Left?.Variable);
        }
        node.Variable = v1;

        return code;
    }

    private string GetVariable(out string v)
    {
        if (variables.Count == usedVariables.Count)
        {
            v = $"v{variables.Count}";
            variables.Push(v);
            return v;
        }
        else
        {
            v = "";
            foreach (var var in variables)
            {
                if (!usedVariables.Contains(var))
                {
                    v = var;
                    return v;
                }
            }
        }

        throw new Exception("ERROR: CodeGenerator.GetVariable()");
    }
}