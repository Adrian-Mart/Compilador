using Compilador.Processors;
using Compilador.Processors.Parser;

namespace Compilador.IO
{
    /// <summary>
    /// The parser IO, used to read and write parser files. Can also be used
    /// to serialize and deserialize parsers.
    /// </summary>
    public class ParserIO : FileIO
    {
        /// <summary>
        /// Creates a new parser IO from the serialized data in the given file.
        /// </summary>
        /// <param name="serialDataPath">The path to the serialized data file.</param>
        public ParserIO(string serialDataPath) : base(".psr", serialDataPath) { }

        /// <summary>
        /// Creates a new parser IO from a parser data file and saves the serialized data to the given file.
        /// </summary>
        /// <param name="processorPath">The path to the parser data file.</param>
        /// <param name="saveToFilePath">The path to the file to save the serialized data.</param>
        public ParserIO(string processorPath, string saveToFilePath) : base(".psr", processorPath, saveToFilePath) { }

        private protected override IProcessor GetProcessorFromFile(string processorPath)
        {
            throw new NotImplementedException();
        }

        private protected override IProcessor GetProcessorFromFile(string processorPath, string saveToFilePath)
        {
            ParserSetup setup = SetSetup(processorPath);
            Console.WriteLine(setup.ToString());
            Parser parser = new Parser(setup);
            // parser.Serialize(saveToFilePath);
            return parser;
        }

        private protected override IProcessor? GetProcessorFromSerialFile(string processorPath)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sets up the parser with the data from the file
        /// </summary>
        /// <param name="filePath">The path to the file</param>
        /// <returns>The parser setup</returns>
        /// <exception cref="Exception"></exception>
        private ParserSetup SetSetup(string filePath)
        {
            // The setup to return
            ParserSetup setup;
            string text = ReadProcessorFileContent(filePath);

            // The start symbol and other data
            string start = "";
            List<string>? productionsLines = new List<string>();
            List<string>? nonTerminals = null;
            List<string>? terminals = null;

            // For each line in the file
            foreach (var line in text.Split('\n'))
            {
                // If the line contains the start symbol
                if (line.StartsWith("& Start", StringComparison.OrdinalIgnoreCase))
                {
                    string[] setupData = line.Split(' ');
                    if (setupData.Length != 3)
                        throw new Exception("Invalid start setup. Already off to a bad start.");
                    start = setupData[2];
                }
                // If the line contains the non terminals
                else if (line.StartsWith("& Non terminals: [", StringComparison.OrdinalIgnoreCase))
                {
                    nonTerminals = ProcessNonTerminals(line);
                }
                // If the line contains the terminals
                else if (line.StartsWith("& Terminals: [", StringComparison.OrdinalIgnoreCase))
                {
                    terminals = ProcessTerminals(line);
                }
                // If the line is a production
                else
                    productionsLines.Add(line);
            }

            // If any of the data is null or empty, throw an exception
            if (string.IsNullOrEmpty(start) || productionsLines.Count == 0 || nonTerminals == null || terminals == null)
                throw new Exception("Invalid setup. Please read the documentation carefully. I put a lot of effort into it.");

            // Create the setup
            setup = new ParserSetup(start, nonTerminals, terminals);

            // Add the productions to the setup
            foreach (var line in productionsLines)
            {
                string nonTerminalName;
                var production = GetProductionFromLine(setup, line, out nonTerminalName);
                setup.AddProduction(nonTerminalName, production);
            }

            // Return the setup
            return setup;
        }

        /// <summary>
        /// Gets a production from a line in the file
        /// </summary>
        /// <param name="setup">The parser setup</param>
        /// <param name="line">The line</param>
        /// <param name="nonTerminalName">The name of the non terminal</param>
        private Production GetProductionFromLine(ParserSetup setup, string line, out string nonTerminalName)
        {
            // Split the line into the non terminal name and the rules
            string[] data = line.Split("->");
            if (data.Length != 2)
                throw new Exception("Invalid production rule. Please read the documentation carefully. I put a lot of effort into it.");

            // Remove spaces from the non terminal name
            nonTerminalName = data[0].Replace(" ", "");
            // Split the rules into an array
            var rulesInfo = data[1].Split("|");
            // Get the index of the non terminal
            var nonTerminalIndex = setup.GetNoTerminalIndex(nonTerminalName);
            // Create the production
            Production production = new Production(nonTerminalIndex, rulesInfo.Length);

            // Add the rules to the production
            foreach (var ruleInfo in rulesInfo)
            {
                var ruleIndex = ruleInfo.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                int[] ruleIndexes = new int[ruleIndex.Length];
                for (int i = 0; i < ruleIndexes.Length; i++)
                {
                    ruleIndexes[i] = setup.GetIndexOf(ruleIndex[i]);
                }

                production.AddRule(new Rule(ruleIndexes));
            }

            return production;
        }

        /// <summary>
        /// Processes the non terminals from a line in the file
        /// </summary>
        /// <param name="line">The line</param>
        /// <returns>The non terminals as a string list</returns>
        private List<string> ProcessNonTerminals(string line)
        {
            List<string> nonTerminals = new List<string>();

            var data = line.Replace("& Non terminals: [", "", StringComparison.OrdinalIgnoreCase);
            data = data.Replace("]", "", StringComparison.OrdinalIgnoreCase);

            foreach (var nonTerminal in data.Split(' '))
            {
                if (string.IsNullOrEmpty(nonTerminal)) continue;
                nonTerminals.Add(nonTerminal);
            }

            return nonTerminals;
        }

        /// <summary>
        /// Processes the terminals from a line in the file
        /// </summary>
        /// <param name="line">The line</param>
        /// <returns>The terminals as a string list</returns>
        private List<string> ProcessTerminals(string line)
        {
            List<string>? terminals = new List<string>();

            var data = line.Replace("& Terminals: [", "", StringComparison.OrdinalIgnoreCase);
            data = data.Replace("]", "", StringComparison.OrdinalIgnoreCase);

            foreach (var terminal in data.Split(' '))
            {
                if (string.IsNullOrEmpty(terminal)) continue;
                terminals.Add(terminal);
            }

            return terminals;
        }
    }
}