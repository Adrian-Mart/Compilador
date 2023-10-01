using System;
using System.IO;
using System.Text;
using Compilador.Graph;
using Compilador.Processors;
using Compilador.RegexInterpreter;

namespace Compilador.IO
{
    /// <summary>
    /// LexerIO class that reads a processor file and creates a Lexer 
    /// object based on the automatas and tokens defined in the file.
    /// </summary>
    /// <remarks>
    /// This class inherits from the ProcessorIO class and overrides the 
    /// GetProcessorFromFile method to create a Lexer object.
    /// </remarks>
    public class LexerIO : FileIO
    {
        /// <summary>
        /// The DFA of the lexer.
        /// </summary>
        private List<ITester> automatas = new List<ITester>();
        /// <summary>
        /// The tokens of the lexer.
        /// </summary>
        private List<string> tokens = new List<string>();
        /// <summary>
        /// The setup of the lexer.
        /// </summary>
        private LexerSetup setup = LexerSetup.DefaultSetup;

        public LexerIO(string processorPath, string saveToFilePath) : base(".tks", processorPath, saveToFilePath) { }

        public LexerIO(string serialDataPath) : base(".tks", serialDataPath) { }


        /// <summary>
        /// Splits the text by lines and then by separators.
        /// </summary>
        /// <param name="text">
        /// The text to split.
        /// </param>
        /// <param name="firstLine">
        /// Null if first line is not setup, otherwise contains the first line.
        /// </param>
        /// <returns>
        /// A 2D array containing the regex and token for each line.
        /// </returns>
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

        /// <summary>
        /// Adds an automata and its token to the lexer.
        /// </summary>
        /// <param name="regex">
        /// A string containing the regex for this token.
        /// </param>
        /// <param name="token">
        /// A string containing the token of this regex.
        /// </param>
        /// <exception cref="Exception"></exception>
        private void AddAutomatas(string regex, string token)
        {
            automatas.Add(Interpreter.Interpret(regex));
            tokens.Add(token);

            if (automatas.Count() != tokens.Count())
            {
                Console.WriteLine("Automatas and tokens count mismatch."
                + " Have mercy on them, they have no partner, just like me.");
                throw new Exception("Automatas and tokens count mismatch.");
            }
        }

        /// <summary>
        /// Sets the DFA from each regex for each token.
        /// </summary>
        /// <param name="input">
        /// The input array containing the regex and token for each line.
        /// </param>
        private void SetAutomatas(string[,] input)
        {
            float progress = 0;
            Console.WriteLine("Creating automatas...");
            automatas.Clear();
            tokens.Clear();
            for (int i = 0; i < input.GetLength(0); i++)
            {
                if (input[i, 0] != null && input[i, 1] != null)
                    AddAutomatas(input[i, 0], input[i, 1]);
                progress = (float)i / input.GetLength(0) * 100;
                Console.WriteLine(string.Concat("  Progress: ", progress, "%, Regex to DFA: ", input[i, 0], " READY"));
            }
            Console.WriteLine("  Progress: 100%");
        }

        /// <summary>
        /// Gets a processor (<seealso cref="Lexer"/>) from the specified file path.
        /// </summary>
        /// <param name="processorPath">
        /// The path of the file containing the processor data.
        /// </param>
        /// <returns>
        /// A processor (<seealso cref="Lexer"/>) from the specified file path.
        /// </returns>
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

        /// <summary>
        /// Sets the setup of the lexer.
        /// </summary>
        /// <param name="firstLine">The first line containing the setup</param>
        /// <exception cref="Exception"></exception>
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

            if (parts.Length == 4 || parts[2] == "N")
                setup = new LexerSetup(parts[0][0], parts[1][0], parts[2][0], parts[3]);
            else if (parts.Length == 7)
                setup = new LexerSetup(parts[0][0], parts[1][0], parts[2][0], parts[3], true, parts[5][0], parts[6]);
            else
            {
                Console.WriteLine("The Lexer configuration is incorrect. Please read the documentation " +
                "carefully. I put a lot of effort into it.");
                throw new Exception("Invalid setup.");
            }
        }

        private protected override IProcessor GetProcessorFromFile(string processorPath, string saveToFilePath)
        {
            Lexer lexer = (Lexer)GetProcessorFromFile(processorPath);
            lexer.Serialize(saveToFilePath);
            return lexer;
        }

        private protected override IProcessor? GetProcessorFromSerialFile(string processorPath)
        {
            Lexer? lexer = (Lexer?)Lexer.Deserialize(processorPath);
            return lexer;
        }
    }
}