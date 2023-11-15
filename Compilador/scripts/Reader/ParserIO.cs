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

        private protected override Parser GetProcessorFromFile(string processorPath)
        {
            ParserSetup setup = SetSetup(processorPath);
            Console.WriteLine(setup.ToString());
            return new Parser(setup);
        }

        private protected override Parser GetProcessorFromFile(string processorPath, string saveToFilePath)
        {
            processor = GetProcessorFromFile(processorPath);
            processor.Serialize(saveToFilePath);
            return (Parser) processor;
        }

        private protected override Parser? GetProcessorFromSerialFile(string processorPath)
        {
            var parser = (Parser?)Parser.Deserialize(processorPath);
            if (parser == null)
                throw new Exception("Invalid parser serial data.");
            processor = parser;
            return parser;
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
            string newLine = "";
            List<string>? productionsLines = new List<string>();
            List<string>? nonTerminals = null;
            List<string>? terminals = null;
            List<string>? hierarchy = null;
            List<(string, bool)>? operators = null;

            // For each line in the file
            foreach (var line in text.Split('\n'))
            {
                // If the line contains the start symbol
                if (line.StartsWith("& Start: ", StringComparison.OrdinalIgnoreCase))
                {
                    string[] setupData = line.Split(' ');
                    if (setupData.Length != 3)
                        throw new Exception("Invalid start setup. Already off to a bad start.");
                    start = setupData[2];
                }
                // If the line contains the start symbol
                else if (line.StartsWith("& New line: ", StringComparison.OrdinalIgnoreCase))
                {
                    string[] setupData = line.Split(' ');
                    if (setupData.Length != 4)
                        throw new Exception("Invalid start setup. Already off to a bad start.");
                    newLine = setupData[3];
                }
                // If the line contains the non terminals
                else if (line.StartsWith("& Non terminals: [", StringComparison.OrdinalIgnoreCase))
                {
                    nonTerminals = ProcessListFromLine(line, "& Non terminals");
                }
                // If the line contains the terminals
                else if (line.StartsWith("& Terminals: [", StringComparison.OrdinalIgnoreCase))
                {
                    terminals = ProcessListFromLine(line, "& Terminals");
                }
                // If the line contains the hierarchy
                else if (line.StartsWith("& Hierarchy: [", StringComparison.OrdinalIgnoreCase))
                {
                    hierarchy = ProcessListFromLine(line, "& Hierarchy");
                }
                // If the line contains the operators
                else if (line.StartsWith("& Operators: [", StringComparison.OrdinalIgnoreCase))
                {
                    operators = ProcessOperators(line);
                }
                // If the line is a production
                else
                    productionsLines.Add(line);
            }

            // If any of the data is null or empty, throw an exception
            if (string.IsNullOrEmpty(start) || productionsLines.Count == 0 || nonTerminals == null || terminals == null)
                throw new Exception("Invalid setup. Please read the documentation carefully. I put a lot of effort into it.");

            // Create the setup
            setup = new ParserSetup(start, nonTerminals, terminals, hierarchy, operators, newLine);

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
                throw new Exception($"Invalid production rule: {line}. Please read the documentation carefully. I put a lot of effort into it.");

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

                production.AddRule(new Rule(ruleIndexes, production));
            }

            return production;
        }

        /// <summary>
        /// Processes the list from a line in the file
        /// </summary>
        /// <param name="line">The line</param>
        /// <param name="start">The start of the line</param>
        /// <returns>The list as a string list</returns>
        private List<string> ProcessListFromLine(string line, string start)
        {
            List<string> list = new List<string>();

            var data = line.Replace(start, "", StringComparison.OrdinalIgnoreCase);
            data = data.Replace(": [", "", StringComparison.OrdinalIgnoreCase);
            data = data.Replace("]", "", StringComparison.OrdinalIgnoreCase);

            foreach (var nonTerminal in data.Split(' '))
            {
                if (string.IsNullOrEmpty(nonTerminal)) continue;
                list.Add(nonTerminal);
            }

            return list;
        }

        private List<(string, bool)> ProcessOperators(string line)
        {
            List<(string, bool)> operators = new List<(string, bool)>();

            var data = line.Replace("& Operators: [", "", StringComparison.OrdinalIgnoreCase);
            data = data.Replace("]", "", StringComparison.OrdinalIgnoreCase);

            foreach (var op in data.Split(' '))
            {
                if (string.IsNullOrEmpty(op)) continue;
                var opData = op.Split(':');
                if (opData.Length != 2)
                    throw new Exception("Invalid operator. Please read the documentation carefully. I put a lot of effort into it.");
                operators.Add((opData[0], string.Compare(opData[1], "binary_op", StringComparison.OrdinalIgnoreCase) == 0));
            }
            return operators;
        }
    }
}