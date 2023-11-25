using System.Text;

namespace Compilador.Quackier;

internal class CodeGenerator
{
    private Dictionary<int, string> names = new Dictionary<int, string>();
    private Dictionary<string, string> realVariables = new Dictionary<string, string>();
    private Dictionary<string, string> realContent = new Dictionary<string, string>();
    private Dictionary<string, string> stringVariables = new Dictionary<string, string>();
    private Dictionary<string, string> lenVariables = new Dictionary<string, string>();
    private Dictionary<string, string> stringContent = new Dictionary<string, string>();
    private List<string> functions = new List<string>();

    private static string template = @"
section .data
    zero dd 0.0
    format db '%f', 10, 0
<strings>

section .bss
    a: resd 4
    b: resd 4
<vars><reals>

section .text
    global main
    extern printf

main:<dec>
<code>

    ; exit
    xor   eax, eax
    ret

<functions>
    ";


    private static string printTemplate = @"
    ; print
    mov ecx, <var>
    mov edx, <len>
    call print
    ";

    private static string printValTemplate = @"
    ; print
    fld dword [<var>]
    call print_val
    ";

    private static string ifTemplate = @"
if_<name>:
    ; evaluate<eval>
    cmp ecx, 1
    jne else

    ; codigo<code>
    ret
    ";

    private static string whileTemplate = @"
while_<name>:
    ; evaluate<eval>
    cmp ecx, 1
    jne else

    ; codigo<code>
    jmp while_<name>
    ";

    private static string assignNumTemplate = @"
    ; assign data
    mov dword [<var>], __float32__(<n>)
    ";

    private static string assignValTemplate = @"
    ; move value to eax<move>
    mov eax, dword [<v1>]

    ; assign data
    mov dword [<var>], eax
    ";

    private static string boolTemplate = @"
    ; store data<store>
    ; compare
    fld dword [b]
    fld dword [a]
    <action> 
    ";

    private static string realTemplate = @"
    ; store data<store>
    ; operate
    fld dword [a]
    f<type> dword [b]
    fstp dword [<v1>]
    ";

    private static string valTemplateNum = @"
    mov dword [a], __float32__(<n1>)
    mov dword [b], __float32__(<n2>)
    ";

    private static string valTemplateLeft = @"
    mov eax, dword [<v1>]
    mov dword [a], eax
    mov dword [b], __float32__(<n2>)
    ";

    private static string valTemplateRight = @"
    mov dword [a], __float32__(<n1>)
    mov eax, dword [<v1>]
    mov dword [b], eax
    ";

    private static string valTemplateBoth = @"
    mov eax, dword [<n1>]
    mov dword [a], eax
    mov eax, dword [<n2>]
    mov dword [b], eax
    ";
    private static string varTemplate = "    <var>: resd 4";
    private static string stringTemplate = "    <var> db <string>, 10";
    private static string lenTemplate = "    <len> equ $-<str>";
    private static string functionsCode = @"
evalEqual:
    fcomip st1
    fstp st0
    je true
    jne false

evalNotEqual:
    fcomip st1
    fstp st0
    jne true
    je false

evalLess:
    fcomip st1
    fstp st0
    jb true
    jae false

evalGreater:
    fcomip st1
    fstp st0
    ja true
    jbe false

evalAnd:
    fcom dword [zero]
    fstsw ax
    sahf
    jz false

    fxch st1
    fcom dword [zero]
    fstsw ax
    sahf
    jz false

    jmp true

evalOr:
    fcom dword [zero]
    fstsw ax
    sahf
    jnz true

    fxch st1
    fcom dword [zero]
    fstsw ax
    sahf
    jnz true
    
    jmp false

true:
    mov ecx, 1
    ret

false:
    mov ecx, 0
    ret

else:
    ret

print:
    mov eax, 4
    mov ebx, 1
    int 0x80
    ret

print_val:
    sub   esp, 8
    fstp  qword [esp]
    push  format
    call  printf
    add   esp, 12
    extern fflush
    push  0
    call  fflush
    add   esp, 4
    ret
    ";

    private Stack<string> variables = new Stack<string>();
    private Stack<string> usedVariables = new Stack<string>();

    public string GenerateCode(AbstractTree tree, string operation)
    {
        variables.Clear();
        usedVariables.Clear();
        realVariables.Clear();
        stringVariables.Clear();

        functions.Add(functionsCode);

        SetDeclarations(tree);


        foreach (var node in tree.Postorder())
            node.Code = BuildCode(node);

        string code = template.Replace("<code>", tree.Root.Code)
                              .Replace("<vars>", GetVariables())
                              .Replace("<reals>", GetReals())
                              .Replace("<strings>", GetStrings())
                              .Replace("<functions>", GetFunctions())
                              .Replace("<dec>", GetDeclarations());

        return code;
    }

    private string GetFunctions()
    {
        StringBuilder sb = new StringBuilder();
        foreach (var item in functions)
            sb.AppendLine(item);
        return sb.ToString();
    }

    private string GetDeclarations()
    {
        StringBuilder sb = new StringBuilder();
        foreach (var item in realVariables)
            sb.AppendLine(assignNumTemplate.Replace("<var>", item.Value)
                          .Replace("<n>", Format(realContent[item.Value])));
        return sb.ToString();
    }


    private string GetStrings()
    {
        StringBuilder sb = new StringBuilder();
        foreach (var item in stringVariables)
        {
            sb.AppendLine(stringTemplate.Replace("<var>", item.Value)
                                        .Replace("<string>", stringContent[item.Value]));
            sb.AppendLine(lenTemplate.Replace("<len>", lenVariables[item.Value])
                          .Replace("<str>", item.Value));
        }

        return sb.ToString();
    }

    private string GetReals()
    {
        StringBuilder sb = new StringBuilder();
        foreach (var item in realVariables)
            sb.AppendLine(varTemplate.Replace("<var>", item.Value));
        return sb.ToString();
    }

    private string GetVariables()
    {
        StringBuilder sb = new StringBuilder();
        foreach (var item in variables)
            sb.AppendLine(varTemplate.Replace("<var>", item));
        return sb.ToString();
    }

    private void SetDeclarations(AbstractTree tree)
    {
        foreach (var node in tree.Postorder())
        {
            if (node.Value == "DeclareReal")
            {
                var name = $"real{realVariables.Count}";
                realVariables.Add(node.Left?.Value ?? "", name);
                realContent.Add(name, node.Right?.Value ?? "");
            }
            else if (node.Value == "DeclareString")
            {
                var name = $"string{stringVariables.Count}";
                stringVariables.Add(node.Left?.Value ?? "", name);
                lenVariables.Add(name, $"len{lenVariables.Count}");
                stringContent.Add(name, node.Right?.Value ?? "");
            }
        }
    }

    private string BuildCode(Node node)
    {
        if (node.Value == "Sentences")
            return BuildSentences(node);
        else if (node.Right is null && node.Left is null)
            return "";
        else if (node.Value == "If")
            return BuildIf(node);
        else if (node.Value == "While")
            return BuildWhile(node);
        else if (node.Value == "Print")
            return BuildPrint(node);
        else if (node.Value == "DeclareReal" || node.Value == "DeclareString")
            return "";
        else if (node.Value == "Asign")
            return BuildAsign(node);
        else
            return BuildOperation(node);
    }

    private string BuildSentences(Node node)
    {
        string code = "";
        foreach (var child in ((SentencesNode)node).Nodes)
        {
            if (child.Value == "DeclareReal" || child.Value == "DeclareString")
                continue;
            if (child.Value == "If")
                code += $"call if_{names[child.GetHashCode()]}\n";
            else if (child.Value == "While")
                code += $"call while_{names[child.GetHashCode()]}\n";
            else
                code += child.Code;
        }
        
        return code;
    }

    private string BuildOperation(Node node)
    {
        string opType = node.Value;
        bool isBool = opType.StartsWith("eval");

        float n1;
        float n2;

        string? v1;
        string code;

        bool left = float.TryParse(node.Left?.Value, out n1);
        bool right = float.TryParse(node.Right?.Value, out n2);

        if (right && left)
        {
            code = valTemplateNum.Replace("<n1>", Format(n1))
                              .Replace("<n2>", Format(n2))
                              .Replace("<v1>", GetVariable(out v1));
            usedVariables.Push(v1);
        }
        else if (left)
        {
            if (!realVariables.TryGetValue(node.Right?.Value ?? "", out v1))
                v1 = usedVariables.Peek();
            code = valTemplateRight.Replace("<n2>", node.Left?.Variable)
                                .Replace("<n1>", Format(n1))
                                .Replace("<v1>", v1);
        }
        else if (right)
        {
            if (!realVariables.TryGetValue(node.Left?.Value ?? "", out v1))
                v1 = usedVariables.Peek();
            code = valTemplateLeft.Replace("<n2>", Format(n2))
                               .Replace("<n1>", node.Right?.Variable)
                               .Replace("<v1>", v1);
        }
        else if (node.Left?.Left is null && node.Left?.Right is null && node.Right?.Left is null && node.Right?.Right is null)
        {
            code = valTemplateBoth.Replace("<n1>", realVariables[node.Left?.Value ??""])
                               .Replace("<n2>", realVariables[node.Right?.Value ??""])
                               .Replace("<v1>", GetVariable(out v1));
            usedVariables.Push(v1);
        }
        else
        {
            usedVariables.Pop();
            v1 = usedVariables.Peek();
            code = valTemplateBoth.Replace("<n1>", node.Left?.Variable)
                               .Replace("<n2>", node.Right?.Variable)
                               .Replace("<v1>", node.Left?.Variable);
        }

        if (isBool)
        {
            code = boolTemplate.Replace("<store>", code);
            code = code.Replace("<action>", $"call {opType}");
        }
        else
        {
            code = realTemplate.Replace("<store>", code);
            code = code.Replace("<type>", opType);
            code = code.Replace("<v1>", GetVariable(out v1));
            usedVariables.Push(v1);
            node.Variable = v1;
        }

        return code;
    }

    private string BuildAsign(Node node)
    {
        if (node.Right?.Right is null)
        {
            string code = assignNumTemplate.Replace("<var>", realVariables[node.Left?.Value ?? ""])
                                           .Replace("<n>", Format(node.Right?.Value));
            return code;
        }
        else
        {
            string code = assignValTemplate.Replace("<var>", realVariables[node.Left?.Value ?? ""])
                                           .Replace("<move>", node.Right?.Code ?? "")
                                           .Replace("<v1>", node.Right?.Variable ?? "");
            return code;
        }
    }

    private string BuildPrint(Node node)
    {
        string code;
        if(stringVariables.TryGetValue(node.Right?.Value ?? "", out var str))
           code = printTemplate.Replace("<var>", str)
                                .Replace("<len>", lenVariables[str]);
        else
            code = printValTemplate.Replace("<var>", realVariables[node.Right?.Value ?? ""]);

        return code;
    }

    private string BuildWhile(Node node)
    {
        string code = whileTemplate.Replace("<eval>", node.Left?.Code ?? "")
                                .Replace("<code>", node.Right?.Code ?? "")
                                .Replace("<name>", GenerateName(node.GetHashCode()));
        functions.Add(code);
        return code;
    }

    private string BuildIf(Node node)
    {
        string code = ifTemplate.Replace("<eval>", node.Left?.Code ?? "")
                                .Replace("<code>", node.Right?.Code ?? "")
                                .Replace("<name>", GenerateName(node.GetHashCode()));
        functions.Add(code);
        

        return code;
    }

    private string GenerateName(int hash)
    {
        var name = $"name{names.Count}";
        names.Add(hash, name);
        return name;
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

    private string Format(string? real)
    {
        if (real is null)
            throw new Exception("ERROR: CodeGenerator.Format()");
        float number = float.Parse(real);
        return Format(number);
    }

    private string Format(float real)
    {
        if (real == Math.Truncate(real))
            return real.ToString("0.0");
        else
            return real.ToString();
    }
}