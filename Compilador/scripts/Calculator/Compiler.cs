using Compilador.Graph;
using Compilador.Processors.Lexer;
using Compilador.Processors.Parser;

namespace Compilador.Calculator;
public static class Compiler
{
    public static string Compile(string code, Lexer lexer, Parser parser)
    {
        // Preprocess the code.
        code = Preprocesor.Preprocess(code);
        // Tokenize the code.
        TokenStream? tokens = lexer.GetOutputObject(code) as TokenStream;
        if (tokens == null)
            throw new Exception("Lexer output is null.");
        // Parse the code.
        (Tree, ParserSetup)? parserOutput = parser.GetOutputObject(tokens) as (Tree, ParserSetup)?;
        if (parserOutput == null)
            throw new Exception("Parser output is null.");
        // Generate the AST.
        AbstractTree tree = new AbstractTree(parserOutput.Value.Item1, parserOutput.Value.Item2);
        // Generate the code.
        CodeGenerator generator = new CodeGenerator();
        string generatedCode = generator.GenerateCode(tree);
        // Return the generated code.
        Console.WriteLine(generatedCode);
        return generatedCode;
    }
}