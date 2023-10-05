using Compilador.Graph;
using Compilador.IO;
using Compilador.RegexInterpreter;

namespace Compilador
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // if (args.Length == 0)
            // {
            //     Console.WriteLine("No arguments specified. Use -what? for help.");
            //     return;
            // }

            // if (args[0] == "-what?")
            // {
            //     Console.WriteLine("Usage: Compilador.exe ([option] [argument])*");
            //     Console.WriteLine("Options:");
            //     Console.WriteLine("\t-what?\t\tShow this help.");
            //     Console.Write("\t-s\t\tReads the lexer from the specified serialized datafile path.");
            //     Console.WriteLine(" For this to work, the option -i must be specified.");
            //     Console.Write("\t-i\t\tInput text for the lexer to tokenize.");
            //     Console.WriteLine(" For this to work, the option -d or -s must be specified.");
            //     Console.Write("\t-d\t\tReads the lexer from the specified lexer datafile path.");
            //     Console.WriteLine(" For this to work, the option -so must be specified.");
            //     Console.Write("\t-so\t\tSpecify the file in which the lexer will be serialized.");
            //     Console.WriteLine(" For this to work, the option -d must be specified.");
            //     Console.WriteLine("\n\nRecommended usage for first use:\ndotnet Compilador.dll -d [lexer datafile path] -so [serialized datafile path]");
            //     Console.WriteLine("\nRecommended usage after the first lexer processing:\ndotnet Compilador.dll -s [serialized datafile path] -i [input code file path]");
            //     return;
            // }


            // var serialIndex = Array.IndexOf(args, "-s");
            // var serialOutIndex = Array.IndexOf(args, "-so");
            // var outIndex = Array.IndexOf(args, "-i");
            // var dataIndex = Array.IndexOf(args, "-d");

            // bool noOutput = outIndex == -1;
            // if(noOutput) Console.WriteLine("No output file specified.");

            // if (!noOutput && serialIndex != -1)
            // {
            //     LexerIO lexerIO = new LexerIO(args[serialIndex + 1]);
            //     lexerIO.WriteFileContent(args[outIndex + 1]);
            // }
            // else if (dataIndex != -1 && serialOutIndex != -1)
            // {
            //     LexerIO lexerIO = new LexerIO(args[dataIndex + 1], args[serialOutIndex + 1]);
            //     if(!noOutput) lexerIO.WriteFileContent(args[outIndex + 1]);
            // }
            // else
            // {
            //     string basePath = AppDomain.CurrentDomain.BaseDirectory;
            //     Console.WriteLine(basePath);
            //     basePath = basePath.Replace("\\bin\\Debug\\net7.0", "");

            //     LexerIO lexerIO = new LexerIO(basePath + "test\\LexerData.lxr", basePath + "test\\LexerDataOut.xml");
            //     lexerIO.WriteFileContent(basePath + "test\\Code.clsr");
            // }

            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            basePath = basePath.Replace("\\bin\\Debug\\net7.0", "");
            ParserIO parserIO = new ParserIO(basePath + "test\\ParserData.prs", basePath + "test\\ParserDataOut.xml");
        }
    }
}