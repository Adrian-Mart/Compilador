using System.Diagnostics;
using System.Runtime.Serialization;
using System.Text;
using Compilador.Graph;
using Compilador.Processors.Lexer;

namespace Compilador.Processors.Parser
{
    /// <summary>
    /// The Parser class is responsible for parsing the input code and generating a parse tree.
    /// </summary>
    [DataContract, KnownType(typeof(ParserSetup)), KnownType(typeof(Operator)), KnownType(typeof(Action))]
    public class Parser : IProcessor
    {
        #region Properties
        /// <summary>
        /// The ParserSetup used for parsing.
        /// </summary>
        [DataMember()]
        private ParserSetup setup;

        /// <summary>
        /// The LALR parse table.
        /// </summary>
        [DataMember()]
        private Action[][] table;

        /// <summary>
        /// The index of the end of file symbol.
        /// </summary>
        [DataMember()]
        private int endOfFileIndex;
        #endregion

        /// <summary>
        /// Initializes a new instance of the Parser class with the specified ParserSetup.
        /// </summary>
        /// <param name="setup">The ParserSetup to use for parsing.</param>
        public Parser(ParserSetup setup)
        {
            // Set the setup
            this.setup = setup;
            // Generate the LALR table
            table = ParseTableLALR.GenerateTable(setup, out endOfFileIndex);
            //Console.WriteLine(ParseTableLALR.TableToString(table, endOfFileIndex));
            Console.WriteLine("Parser table created");
        }

        #region Processor
        /// <summary>
        /// Gets the symbols of the input code.
        /// </summary>
        /// <param name="input">The input token stream to parse.</param>
        /// <returns>A list of symbols.</returns>
        private (List<int>, List<string>) GetSymbols(TokenStream input)
        {
            List<int> symbols = new List<int>();
            List<string> values = new List<string>();

            foreach (Token token in input)
            {
                var index = setup.GetIndexOf(token.Type);
                if (index != -1)
                    symbols.Add(index);
                else
                    throw new Exception($"Token not defined in grammar: {token}");

                values.Add(token.Data);
            }
            return (symbols, values);
        }

        /// <summary>
        /// Gets the parse tree with the tokens instead of the indexes.
        /// </summary>
        /// <param name="tree">The parse tree to tokenize.</param>
        /// <returns>The parse tree with the tokens.</returns>
        private string TokenizeTree(Tree tree)
        {
            StringBuilder sb = new StringBuilder();
            string num = "";
            foreach (var character in tree.ToString()?.ToArray() ?? new char[0])
            {
                int index;
                if (int.TryParse(character.ToString(), out index))
                    num += character.ToString();
                else
                {
                    if (num == "")
                        sb.Append(character);
                    else
                    {
                        sb.Append(setup.GetTokenOf(int.Parse(num)));
                        num = "";
                    }
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Gets the output string of the input code, the output string is
        /// a parse tree of the tokens of the input code.
        /// </summary>
        /// <param name="input">The input code to parse.</param>
        /// <returns>The input parse tree in string format.</returns>
        public string GetOutputString(object input)
        {
            if (input.GetType() != typeof(TokenStream))
                throw new Exception("Invalid input type for parser. Expected TokenStream, got " + input.GetType().ToString());

            var tokens = GetSymbols((TokenStream)input);
            return TokenizeTree(Parse(tokens.Item1, tokens.Item2));
        }

        public void Serialize(string fileName)
        {
            using (StreamWriter writer = new StreamWriter(fileName))
            {
                DataContractSerializerSettings settings = new DataContractSerializerSettings
                {
                    PreserveObjectReferences = true
                };
                DataContractSerializer obj = new DataContractSerializer(typeof(Parser), settings);
                obj.WriteObject(writer.BaseStream, this);
            }
        }

        public static IProcessor? Deserialize(string fileName)
        {
            Parser? parser = null;
            using (StreamReader writer = new StreamReader(fileName))
            {
                DataContractSerializer obj =
                    new DataContractSerializer(typeof(Parser));
                parser = (Parser?)obj.ReadObject(writer.BaseStream);
            }
            return parser;
        }

        public object GetOutputObject(object input)
        {
            if (input.GetType() != typeof(TokenStream))
                throw new Exception(
                    "Invalid input type for parser. Expected TokenStream, got "
                    + input.GetType().ToString());

            var tokens = GetSymbols((TokenStream)input);
            return (Parse(tokens.Item1, tokens.Item2), setup);
        }
        #endregion

        /// <summary>
        /// Parses the input code and generates a parse tree. If the input
        /// code is not valid, throws an exception. The parse tree is not
        /// abstracted.
        /// </summary>
        /// <param name="symbols">The input code to parse.</param>
        /// <returns>The parse tree.</returns>
        private Tree Parse(List<int> symbols, List<string> values)
        {
            // The stack of symbols
            Stack<int> stack = new Stack<int>();
            // The stack of nodes
            Stack<SimpleNode> nodes = new Stack<SimpleNode>();
            // Push the initial state
            stack.Push(0);
            int index = 0;
            // Create the tree
            Tree tree = new Tree(0);

            bool accepted = false;
            while (!accepted)
            {
                // Get the current state
                int state = stack.Peek();
                int symbol;

                // If the index is the end of the file
                if (index == symbols.Count)
                    // Set the symbol to the end of file symbol
                    symbol = endOfFileIndex;
                else
                    // Set the symbol to the current symbol
                    symbol = symbols[index];

                // Get the action from the LALR table
                Action action = table[state][symbol];



                // Print the parse stack
                Console.WriteLine("Stack     : " + string.Join(" ", stack.Reverse().ToArray()));
                // Print the parse current symbol and action
                Console.WriteLine($"Symbols[{index}]: {symbol}");
                Console.WriteLine($"Action    : {action}\n");


                switch (action.Type)
                {
                    // If the action is an error
                    case ActionType.Error:
                        int line;
                        string text = GetLine(values, index, symbols, out line);
                        // Throw an exception
                        throw new Exception($"Error at symbol {values[index]}, line[{line}]: {text}");
                    // If the action is a shift
                    case ActionType.Shift:
                        // Push the symbol and the action value to the stack
                        stack.Push(symbol);
                        stack.Push(action.ActionValue);
                        nodes.Push(new SimpleNode(symbol, values[index], null));
                        index++;
                        break;
                    // If the action is a reduce
                    case ActionType.Reduce:
                        // Get the rule
                        var rule = setup.GetRules()[action.ActionValue];
                        SimpleNode node = new SimpleNode(rule.Production.NonTerminalId, "", null);

                        // Pop n * 2 symbols from the stack, where n is the length of the rule
                        // This is because the stack contains the symbol and the state
                        for (int i = 0; i < rule.Lenght * 2; i++)
                            stack.Pop();
                        for (int i = 0; i < rule.Lenght; i++)
                            node.AddChild(nodes.Pop());

                        // Get the new state
                        int newState = stack.Peek();
                        // Push the non terminal and the new state to the stack
                        stack.Push(rule.Production.NonTerminalId);
                        stack.Push(table[newState][rule.Production.NonTerminalId].ActionValue);
                        nodes.Push(node);
                        break;
                    // If the action is an accept
                    case ActionType.Accept:
                        // Set the accepted flag to true
                        accepted = true;
                        break;
                    default:
                        break;
                }
            }
            // Add the rest of the tree to the root node
            tree.FindLeaf(0)?.AddChild(nodes.Pop());
            return tree;
        }

        /// <summary>
        /// Gets the line of the input code where the error occurred.
        /// </summary>
        /// <param name="index">The index of the error.</param>
        /// <param name="input">The input code.</param>
        /// <param name="line">The line of the error.</param>
        private string GetLine(List<string> values, int index, List<int> input, out int line)
        {
            line = 0;
            var lines = input.Select((x, i) => new { Index = i, Value = x })
                .Where(x => x.Value == setup.NewLineIndex)
                .Select(x => x.Index)
                .ToList();

            int count = 0;
            for (int i = 0; i < lines.Count - 1; i++)
            {
                if (count > index)
                    break;
                count += lines[i];
                line++;
            }

            return string.Join(" ",
                values.GetRange(lines[line] + 1, lines[line + 1] - lines[line] - 1));
        }
    }
}