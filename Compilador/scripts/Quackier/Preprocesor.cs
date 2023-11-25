
namespace Compilador.Quackier;
internal static class Preprocesor
{

    // Use PEMDAS to determine the precedence of the operators.
    // Put spaces between the operators and the operands.
    // Put spaces between the operands and the parentheses.
    internal static string Preprocess(string code)
    {
        code = SetSpaces(code);
        code = SetPrecedence(code);
        code = code.Replace("{", "(*)>");
        code = code.Replace("}", "<(*)");

        return code;
    }

    private static string SetSpaces(string code)
    {
        // Add the extra spaces.
        code = code.Replace("(*)>", "{");
        code = code.Replace("<(*)", "}");
        code = code.Replace("(", " ( ");
        code = code.Replace(")", " ) ");
        code = code.Replace("+", " + ");
        code = code.Replace("*", " * ");
        code = code.Replace("/", " / ");

        // Remove the extra spaces.
        while (code.Contains("  "))
            code = code.Replace("  ", " ");

        return code;
    }

    private static string SetPrecedence(string code)
    {
        // Create a list of groups.
        List<string> groups = new List<string>();

        // Save the result.
        string result = code;

        while (CreateGroup(groups, result, out result)) ;

        return ReplaceGroups(result, groups);
    }

    private static bool CreateGroup(List<string> group, string code, out string result)
    {
        bool found = false;
        // Split by spaces.
        string[] tokens = code.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        // Scan for left to right for / or *.
        for (int i = 0; i < tokens.Length; i++)
        {
            if (tokens[i] == "/" || tokens[i] == "*")
            {
                group.Add($"( {tokens[i - 1]} {tokens[i]} {tokens[i + 1]} )");
                // Replace the operator with the result of the operation.
                tokens[i] = $"<{group.Count - 1}>";
                // Remove the operands.
                tokens[i - 1] = "";
                tokens[i + 1] = "";

                found = true;

                break;
            }
        }

        if (found)
        {
            result = string.Join(" ", tokens);
            // Remove the extra spaces.
            while (result.Contains("  "))
                result = result.Replace("  ", " ");
        }
        else
            result = code;

        return found;
    }

    private static string ReplaceGroups(string code, List<string> groups)
    {
        // Split by spaces.
        string[] tokens = code.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        while (FirstGroupIndex(code, out int groupIndex, out int tokenIndex))
        {
            tokens[tokenIndex] = groups[groupIndex];
            code = string.Join(" ", tokens);
            tokens = code.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        }

        return string.Join(" ", tokens);
    }

    private static bool FirstGroupIndex(string code, out int groupIndex, out int tokenIndex)
    {
        bool found = false;
        groupIndex = -1;
        tokenIndex = -1;

        // Split by spaces.
        string[] tokens = code.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < tokens.Length; i++)
        {
            if (FoundGroupIndex(tokens[i], out groupIndex))
            {
                found = true;
                tokenIndex = i;
                break;
            }
        }

        return found;
    }

    private static bool FoundGroupIndex(string token, out int index)
    {
        bool found = false;
        index = -1;

        if (token.StartsWith("<") && token.EndsWith(">"))
        {
            found = int.TryParse(token.Substring(1, token.Length - 2), out index);
        }

        return found;
    }
}