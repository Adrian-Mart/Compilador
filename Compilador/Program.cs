using Graph;
using RegexInterpreter;

namespace Compilador // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string e = "(h|H).(o|O).(l|L).(a|A). .(a|b|c|d|e|f|g|h|i|j|k|l|m|n|ñ|o|p|q|r|s|t|u|v|w|x|y|z| )+.!";
            Console.Write("Expression: " + e + " \t Parsed: ");
            Console.Write(Parser.Parse(e));
            Console.Write(" \t Interpreted: ");
            var temp = Interpreter.Interpret(e);
            DFA automata = temp.Automata;
            Console.WriteLine(temp.ToString());

            string? testing = "";
            while (testing != "Exit")
            {
                Console.Write("Testing: ");
                testing = Console.ReadLine();
                if (testing == null) break;
                Console.WriteLine(automata.TestString(testing));
            }
        }
    }
}