using System;
using System.IO;
using System.Text;
using Compilador.Graph;
using Compilador.Processors;
using Compilador.RegexInterpreter;

namespace Compilador.IO
{
    public class LexerIO : FileIO
    {
        private List<ITester> automatas = new List<ITester>();
        private List<string> tokens = new List<string>();
        private LexerSetup setup = LexerSetup.DefaultSetup;
        public LexerIO(string fileExtension, string processorPath) : base(fileExtension, processorPath) { }

        public LexerIO(string processorPath) : base(".tks", processorPath) { }

        private string[,] SplitRegexAndTokens(string text, out string? firstLine)
        {
            // Vars to check if first line is setup
            bool firstLineFound = false;
            firstLine = null;

            // Split text by lines
            var inputText = text.Split('\n').Where(x => !string.IsNullOrEmpty(x)).ToArray();

            // Check if first line is setup
            if (inputText[0].Length > 2 && inputText[0][0] == ':' && inputText[0][1] == 'D')
            {
                firstLineFound = true;
                firstLine = inputText[0];
            }

            // Create input array
            string[,] input;
            if (firstLineFound)
                input = new string[inputText.Length - 1, 2];
            else
                input = new string[inputText.Length, 2];

            // Fill input array
            if (firstLineFound)
                for (int i = 0; i < inputText.Length - 1; i++)
                {
                    var parts = inputText[i + 1].Split(new char[] { ' ' });
                    input[i, 0] = string.Join(" ", parts.Take(parts.Length - 1));
                    input[i, 0] = input[i, 0].TrimEnd();
                    input[i, 1] = parts.Last().TrimEnd();
                }
            else
                for (int i = 0; i < inputText.Length; i++)
                {
                    var parts = inputText[i].Split(new char[] { ' ' });
                    input[i, 0] = string.Join(" ", parts.Take(parts.Length - 1));
                    input[i, 0] = input[i, 0].TrimEnd();
                    input[i, 1] = parts.Last().TrimEnd();
                }



            // Return input array
            return input;
        }

        private void AddAutomatas(string regex, string token)
        {
            automatas.Add(Interpreter.Interpret(regex));
            tokens.Add(token);

            if (automatas.Count() != tokens.Count())
                throw new Exception("Automatas and tokens count mismatch.");
        }

        private void SetAutomatas(string[,] input)
        {
            automatas.Clear();
            tokens.Clear();
            for (int i = 0; i < input.GetLength(0); i++)
            {
                if (input[i, 0] != null && input[i, 1] != null)
                    AddAutomatas(input[i, 0], input[i, 1]);
            }
        }

        private protected override IProcessor GetProcessorFromFile(string processorPath)
        {
            string? firstLine;

            // Read file content
            var text = ReadProcessorFileContent(processorPath);
            // Split regex and tokens
            var splittedText = SplitRegexAndTokens(text, out firstLine);
            // Set automatas and tokens
            SetAutomatas(splittedText);
            // Set setup
            SetSetup(firstLine);

            // Return processor
            return new Lexer(automatas, tokens, setup);
        }

        private void SetSetup(string? firstLine)
        {
            if (firstLine == null)
                return;
            firstLine = firstLine.Replace(":D{", "");
            firstLine = firstLine.Replace("}", "");
            firstLine = firstLine.Replace("\\n", "\n");
            firstLine = firstLine.Replace("\\r", "\r");
            firstLine = firstLine.Replace("\\t", "\t");

            var parts = firstLine.Split(',');
            if (parts.Length < 4)
                throw new Exception("Invalid setup.");
            else if (parts.Length == 4 || parts[2] == "N")
                setup = new LexerSetup(parts[0][0], parts[1][0], parts[2][0], parts[3]);
            else if ( parts.Length == 7)
                setup = new LexerSetup(parts[0][0], parts[1][0], parts[2][0], parts[3], true, parts[5][0], parts[6]);
            else
                throw new Exception("Invalid setup.");
        }
    }
}