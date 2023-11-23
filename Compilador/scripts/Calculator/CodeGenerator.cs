using System.Text;

namespace Compilador.Calculator;

internal class CodeGenerator
{
    private static string template = @"
section .data
    format: db <format>,10,0

section .bss
    a: resd 4
    b: resd 4
<vars>
    
section .text
    global main
    extern printf

main:
<code>
    
    ; print result
    fld dword [v0]
    sub   esp, 8
    fstp  qword [esp]
    push  format
    call  printf
    add   esp, 12
    
    ; exit
    xor   eax, eax
    ret
    ";

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

    public string GenerateCode(AbstractTree tree, string operation)
    {
        variables.Clear();
        usedVariables.Clear();
        StringBuilder sb = new StringBuilder();
        
        foreach (var node in tree.Postorder())
            sb.Append(BuildCode(node));
        string code = sb.ToString();
        sb.Clear();

        foreach (var var in variables)
            sb.Append($"    {var}: resd 4\n");
        string vars = sb.ToString();

        string format = $"\"{operation} = %5f\"";
        return template.Replace("<code>", code)
                          .Replace("<vars>", vars)
                          .Replace("<format>", format);
    }

    private string BuildCode(Node node)
    {
        if (node.Right is null && node.Left is null)
            return "";

        string opType = node.Value;

        float n1;
        float n2;

        string v1;
        string code;

        bool left = float.TryParse(node.Left?.Value, out n1);
        bool right = float.TryParse(node.Right?.Value, out n2);

        if (right && left)
        {
            code = templateNum.Replace("<n1>", n1.ToString("0.000000"))
                              .Replace("<n2>", n2.ToString("0.000000"))
                              .Replace("<v1>", GetVariable(out v1));
            usedVariables.Push(v1);
        }
        else if (left)
        {
            v1 = usedVariables.Peek();
            code = templateRight.Replace("<n2>", node.Left?.Variable)
                                .Replace("<n1>", n1.ToString("0.000000"))
                                .Replace("<v1>", v1);
        }
        else if (right)
        {
            v1 = usedVariables.Peek();
            code = templateLeft.Replace("<n2>", n2.ToString("0.000000"))
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
        code = code.Replace("<type>", opType);
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