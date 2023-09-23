using Compilador.Graph;
using Compilador.IO;
using Compilador.RegexInterpreter;

namespace Compilador
{
    internal class Program
    {
        static void Main(string[] args)
        {
            /* string e = "a";
            Console.Write("Expression: " + e + " \t Parsed: ");
            Console.Write(Parser.Parse(e));
            Console.Write(" \t Interpreted: ");
            string interpretedExp;
            var automata = Interpreter.Interpret(e, out interpretedExp);
            Console.WriteLine(interpretedExp);

            string? testing = "";
            while (testing != "Exit")
            {
                Console.Write("Testing: ");
                testing = Console.ReadLine();
                if (testing == null) break;
                Console.WriteLine(automata.TestString(testing));
            } */
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            basePath = basePath.Replace("\\bin\\Debug\\net7.0", "");

            LexerIO lexerIO = new LexerIO(basePath + "test\\LexerData.lxr");
            lexerIO.WriteFileContent(basePath + "test\\Code.clsr");
        }
    }
}