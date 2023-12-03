using Compilador.Graph;
using Compilador.Processors.Lexer;
using Compilador.Processors.Parser;

namespace Compilador.Quackier;
public static class Compiler
{
    public static string Compile(string code, Lexer lexer, Parser parser, string filePath)
    {
        // Preprocess the code.
        code = Preprocesor.Preprocess(code);
        // Tokenize the code.
        TokenStream? tokens = lexer.GetOutputObject(code) as TokenStream;
        if (tokens == null)
            throw new Exception("Lexer output is null.");
        WriteFileContent(lexer.GetOutputString(code), filePath, ".tkn");
        // Parse the code.
        (Tree, ParserSetup)? parserOutput = parser.GetOutputObject(tokens) as (Tree, ParserSetup)?;
        if (parserOutput == null)
            throw new Exception("Parser output is null.");
        WriteFileContent(parser.GetOutputString(tokens), filePath, ".prs");
        TypeChecker typeChecker = new TypeChecker(parserOutput.Value.Item1, parserOutput.Value.Item2);
        if(!typeChecker.CheckTypes())
            throw new Exception("Type checking failed.");
        // Generate the AST.
        AbstractTree tree = new AbstractTree(parserOutput.Value.Item1, parserOutput.Value.Item2);
        // Generate the code.
        CodeGenerator generator = new CodeGenerator();
        string generatedCode = generator.GenerateCode(tree, code);
        // Return the generated code.
        //Console.WriteLine(generatedCode);
        WriteFileContent(generatedCode, filePath);
        return generatedCode;
    }

    private static void WriteFileContent(string input, string filePath, string extension = ".asm")
    {
        using (StreamWriter writer = new StreamWriter(filePath + extension))
        {
            writer.Write(input);
        }
    }
}