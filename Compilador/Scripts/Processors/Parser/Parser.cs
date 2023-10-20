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
    [DataContract, KnownType(typeof(ParserSetup)), KnownType(typeof(Operator))]
    public class Parser : IProcessor
    {
        /// <summary>
        /// The recursive limit of the parser.
        /// </summary>
        private const int RECURSIVE_LIMIT = 50;

        /// <summary>
        /// The ParserSetup used for parsing.
        /// </summary>
        [DataMember()]
        private ParserSetup setup;

        /// <summary>
        /// The parse table of the grammar.
        /// </summary>
        [DataMember()]
        private Rule[,] parseTable;

        /// <summary>
        /// Initializes a new instance of the Parser class with the specified ParserSetup.
        /// </summary>
        /// <param name="setup">The ParserSetup to use for parsing.</param>
        public Parser(ParserSetup setup)
        {
            this.setup = setup;
            var first = new int[setup.NonTerminalsCount][];
            var follow = new int[setup.NonTerminalsCount][];
            parseTable = new Rule[setup.NonTerminalsCount, setup.TerminalsCount];

            CalculateFirst(first);
            CalculateFollow(first, follow);
            CalculateParseTable(first, follow);

            Console.WriteLine(this);
        }

        /// <summary>
        /// Calculates the parse table of the grammar, based on the first and follow sets.
        /// </summary>
        /// <param name="first">The first set of the grammar.</param>
        /// <param name="follow">The follow set of the grammar.</param>
        /// <exception cref="Exception"></exception>
        private void CalculateParseTable(int[][] first, int[][] follow)
        {
            int empty = setup.GetIndexOfEmpty();
            Rule emptyRule = new Rule(new int[] { empty });
            int[] terminals = setup.GetTerminalIndexes();
            for (int i = 0; i < setup.NonTerminalsCount; i++)
            {
                if (follow[i] == null || first[i] == null)
                    throw new Exception("Grammar is not feasible for LL(1).");
                Rule[] ntRules = setup.GetProductionOf(i).Rules;
                for (int j = 0; j < setup.TerminalsCount; j++)
                {

                    if (terminals[j] == empty && follow[i].Contains(-1))
                    {
                        foreach (var item in ntRules)
                            if (item.Contains(empty))
                            {
                                parseTable[i, j] = emptyRule;
                                break;
                            }
                        continue;
                    }


                    if (follow[i].Contains(terminals[j]))
                    {
                        foreach (var item in ntRules)
                            if (item.Contains(empty))
                            {
                                parseTable[i, j] = emptyRule;
                                break;
                            }
                    }
                    if (first[i].Contains(terminals[j]))
                    {
                        if (parseTable[i, j] != null)
                            throw new Exception("Grammar is not feasible for LL(1).");
                        List<Rule> rules = new List<Rule>();
                        foreach (var rule in setup.GetNonEmptyRules(i))
                        {
                            if (setup.isTerminal(rule[0]) && rule[0] == terminals[j])
                                rules.Add(rule);
                            else if (!setup.isTerminal(rule[0]) && first[rule[0]].Contains(terminals[j]))
                                rules.Add(rule);
                        }
                        if (rules.Count == 1) parseTable[i, j] = rules[0];
                        else throw new Exception("Grammar is not feasible for LL(1).");
                    }
                }
            }
        }

        /// <summary>
        /// Calculates the first of every non terminal of the grammar.
        /// <param name="first">The first set of the grammar.</param>
        /// </summary>
        private void CalculateFirst(int[][] first)
        {
            int[] nonTerminals = setup.GetNonTerminalIndexes();
            foreach (var nonTerminal in nonTerminals.Reverse())
            {
                if (first[nonTerminal] == null)
                    first[nonTerminal] = CalculateFirstOfNT(nonTerminal, 0, first);
            }
        }

        /// <summary>
        /// Calculates the follow of every non terminal of the grammar.
        /// </summary>
        /// <param name="first">The first set of the grammar.</param>
        /// <param name="follow">The follow set of the grammar.</param>
        /// <exception cref="Exception"></exception>
        private void CalculateFollow(int[][] first, int[][] follow)
        {
            int[] nonTerminals = setup.GetNonTerminalIndexes();
            int index = 0;
            foreach (var nonTerminal in nonTerminals)
            {
                if (follow[index] == null)
                    follow[index++] = CalculateFollowOfNT(nonTerminal, 0, first, follow);
            }
        }

        /// <summary>
        /// Calculates the follow of a non-terminal.
        /// </summary>
        /// <param name="nonTerminal">The non-terminal to calculate the follow of.</param>
        /// <param name="recursions">The number of recursions.
        /// Used to prevent undesired recursion.</param>
        /// <param name="first">The first set of the grammar.</param>
        /// <param name="follow">The follow set of the grammar.</param>
        /// <returns>The follow of the non-terminal.</returns>
        private int[] CalculateFollowOfNT(int nonTerminal, int recursions, int[][] first, int[][] follow)
        {
            if (nonTerminal == setup.Start)
                return new int[] { -1 };

            int empty = setup.GetIndexOfEmpty();

            List<int> followList = new List<int>();
            var rulesParticipation = setup.GetRuleParticipation(nonTerminal);
            foreach (var ruleParticipation in rulesParticipation)
            {
                foreach (var rule in ruleParticipation.Value)
                {
                    int indexInRule = rule.IndexOf(nonTerminal);
                    if (indexInRule == -1) continue;

                    if (indexInRule == rule.Lenght - 1 && ruleParticipation.Key != nonTerminal)
                    {
                        followList.AddRange(GetOrCalculateFollowOfNT(ruleParticipation.Key, recursions, first, follow));
                        continue;
                    }

                    bool containsEmpty = false;
                    for (int i = indexInRule + 1; i < rule.Lenght; i++)
                    {
                        if (setup.isTerminal(rule[i]))
                        {
                            if (rule[i] == empty)
                                throw new Exception("Empty terminal found inside rule.");
                            followList.Add(rule[i]);
                            containsEmpty = false;
                            break;
                        }
                        else
                        {
                            int[] firstOfItem = GetOrCalculateFirstOfNT(rule[i], recursions, first);
                            foreach (var firstItem in firstOfItem)
                            {
                                if (firstItem == empty)
                                {
                                    followList.AddRange(GetOrCalculateFollowOfNT(ruleParticipation.Key, recursions, first, follow));
                                    if(followList.Contains(empty))
                                        containsEmpty = true;
                                }
                                else
                                    followList.Add(firstItem);
                            }
                            if (!containsEmpty)
                                break;
                        }
                    }
                    if (containsEmpty)
                        followList.Add(-1);
                }
            }

            followList = followList.Distinct().ToList();
            return followList.ToArray();
        }

        /// <summary>
        /// Gets the follow of a non-terminal. If it's not calculated yet, calculates it.
        /// </summary>
        /// <param name="nonTerminal">The non-terminal to get the follow of.</param>
        /// <param name="recursions">The number of recursions.
        /// Used to prevent undesired recursion.</param>
        /// <param name="first">The first set of the grammar.</param>
        /// <param name="follow">The follow set of the grammar.</param>
        /// <returns>The follow of the non-terminal.</returns>
        /// <exception cref="Exception"></exception>
        private int[] GetOrCalculateFollowOfNT(int nonTerminal, int recursions, int[][] first, int[][] follow)
        {
            if (recursions >= RECURSIVE_LIMIT)
                throw new Exception("Recursive limit reached. Check for undesired recursion in grammar.");
            if (follow[nonTerminal] == null)
                follow[nonTerminal] = CalculateFollowOfNT(nonTerminal, recursions + 1, first, follow);
            return follow[nonTerminal];
        }

        /// <summary>
        /// Calculates the first of a non-terminal.
        /// </summary>
        /// <param name="nonTerminal">The non-terminal to
        /// calculate the first of.</param>
        /// <param name="recursions">The number of recursions.
        /// Used to prevent undesired recursion.</param>
        /// <param name="first">The first set of the grammar.</param>
        /// <returns>The first of the non-terminal.</returns>
        private int[] CalculateFirstOfNT(int nonTerminal, int recursions, int[][] first)
        {
            int empty = setup.GetIndexOfEmpty();

            // Initialize a new list for terminals: first
            List<int> firstList = new List<int>();
            // For each rule of the non-terminal
            foreach (var rule in setup.GetProductionOf(nonTerminal).Rules)
            {
                bool containsEmpty = false;

                // For each item of the rule
                foreach (var item in rule)
                {
                    // If the item is a terminal, add it to first and break
                    if (setup.isTerminal(item))
                    {
                        containsEmpty = false;
                        firstList.Add(item);
                        break;
                    }
                    // If the item is a non-terminal
                    else
                    {
                        containsEmpty = false;
                        // Get the first of the non-terminal
                        int[] firstOfItem = GetOrCalculateFirstOfNT(item, recursions, first);
                        // Add each item of the first of the non-terminal to first, except empty terminal
                        foreach (var firstItem in firstOfItem)
                        {
                            if (firstItem == empty)
                                containsEmpty = true;
                            else
                                firstList.Add(firstItem);
                        }
                        // If the first of the non-terminal doesn't contain empty, break
                        if (!containsEmpty)
                            break;
                    }
                }
                // If the rule contains empty, add empty to first
                if (containsEmpty)
                    firstList.Add(empty);
            }

            // Sort and return first
            firstList.Sort();
            firstList = firstList.Distinct().ToList();
            firstList.Sort();
            return firstList.ToArray();
        }

        /// <summary>
        /// Gets the first of a non-terminal.
        /// </summary>
        /// <param name="nonTerminal">The non-terminal to get the first of.</param>
        /// <param name="recursions">The number of recursions. Used to
        /// prevent undesired recursion.</param>
        /// <param name="first">The first set of the grammar.</param>
        /// <returns>The first of the non-terminal.</returns>
        /// <exception cref="Exception"></exception>
        private int[] GetOrCalculateFirstOfNT(int nonTerminal, int recursions, int[][] first)
        {
            if (recursions >= RECURSIVE_LIMIT)
                throw new Exception("Recursive limit reached. Check for undesired recursion in grammar.");
            if (first[nonTerminal] == null)
                first[nonTerminal] = CalculateFirstOfNT(nonTerminal, recursions + 1, first);
            return first[nonTerminal];
        }

        /// <summary>
        /// Gets the symbols of the input code.
        /// </summary>
        /// <param name="input">The input token stream to parse.</param>
        /// <returns>A list of symbols.</returns>
        private List<int> GetSymbols(TokenStream input)
        {
            List<int> symbols = new List<int>();
            foreach (Token token in input)
            {
                var index = setup.GetIndexOf(token.Type);
                if (index != -1)
                    symbols.Add(index);
                else
                    throw new Exception($"Token not defined in grammar: {token}");
            }
            return symbols;
        }

        /// <summary>
        /// Gets the syntax tree with the tokens instead of the indexes.
        /// </summary>
        /// <param name="tree">The syntax tree to tokenize.</param>
        /// <returns>The syntax tree with the tokens.</returns>
        private string TokenizeTree(SyntaxTree tree)
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
        /// Gets the output string of the input code, the output string is an 
        /// abstract syntax tree of the tokens of the input code.
        /// </summary>
        /// <param name="input">The input code to parse.</param>
        /// <returns>The input AST in string format.</returns>
        public string GetOutputString(object input)
        {
            if (input.GetType() != typeof(TokenStream))
                throw new Exception("Invalid input type for parser. Expected TokenStream, got " + input.GetType().ToString());

            var symbols = GetSymbols((TokenStream)input);
            SyntaxTree tree = ParseAndAbstract(symbols);
            return TokenizeTree(tree);
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

        public override string? ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("\nParse Table\n\t");
            foreach (var terminal in setup.GetTerminalIndexes())
            {
                if (terminal == setup.GetIndexOfEmpty())
                    sb.Append("$\t");
                else
                    sb.Append($"{terminal}\t");
            }
            sb.Append("\n");
            for (int i = 0; i < setup.NonTerminalsCount; i++)
            {
                sb.Append(setup.GetNonTerminalIndexes()[i]).Append("\t");
                for (int j = 0; j < setup.TerminalsCount; j++)
                {
                    if (parseTable[i, j] == null)
                        sb.Append("----\t");
                    else
                    {
                        sb.Append("{");
                        foreach (var rule in parseTable[i, j])
                            sb.Append(rule).Append(",");
                        sb.Remove(sb.Length - 1, 1);
                        sb.Append("}\t");
                    }
                }
                sb.Append("\n");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Parses the input code and generates a syntax tree. If the input
        /// code is not valid, throws an exception. The syntax tree is already
        /// abstracted.
        /// </summary>
        /// <param name="symbols">The input code to parse.</param>
        /// <returns>The abstract syntax tree.</returns>
        private SyntaxTree ParseAndAbstract(List<int> symbols)
        {
            SyntaxTree tree = Parse(symbols);
            tree.AbstractTree(
                setup.HierarchyTerminals,
                setup.OperatorTerminals,
                setup.GetNonTerminalIndexes());
            return tree;
        }

        /// <summary>
        /// Parses the input code and generates a parse tree. If the input
        /// code is not valid, throws an exception. The parse tree is not
        /// abstracted.
        /// </summary>
        /// <param name="symbols">The input code to parse.</param>
        /// <returns>The parse tree.</returns>
        private SyntaxTree Parse(List<int> symbols)
        {
            // Create a new syntax tree with the start symbol as root
            int empty = setup.GetIndexOfEmpty();
            SyntaxTree tree = new SyntaxTree(setup.Start, empty);

            // Create a stack with the start symbol and the end of file symbol
            Stack<int> stack = new Stack<int>();
            stack.Push(-1);
            stack.Push(setup.Start);

            // For each symbol in the input
            for (int i = 0; i <= symbols.Count; i++)
            {
                // Get the symbol
                int symbol;
                // If the symbol is the end of file symbol, set it to -1
                if (i < symbols.Count)
                    symbol = symbols[i];
                else
                    symbol = -1;

                // While the top of the stack is not a terminal different
                // from the empty symbol
                bool match = false;
                while (!match)
                {
                    // If the top of the stack is a terminal
                    if (setup.isTerminal(stack.Peek()))
                    {
                        // If the top of the stack is the end of file symbol
                        if (stack.Peek() == symbol)
                        {
                            // Pop the stack and break
                            match = true;
                            stack.Pop();
                            continue;
                        }
                        // If the top of the stack is the empty symbol
                        else if (stack.Peek() == empty)
                        {
                            // Pop the stack and continue
                            stack.Pop();
                            continue;
                        }
                        // If the top of the stack is a terminal different from the empty symbol
                        else
                            throw new Exception($"Unexpected symbol {symbol} at position {i}. Expected {stack.Peek()}.");
                    }
                    // If the top of the stack is a non-terminal
                    else
                    {
                        // Get the index of the symbol in the parse table
                        int terminalIndex = Array.IndexOf(setup.GetTerminalIndexes(), symbol);
                        // Get the index of the top of the stack in the parse table
                        int nonTerminalIndex = Array.IndexOf(setup.GetNonTerminalIndexes(), stack.Peek());
                        // If the symbol is the end of file symbol, set the index to the last column of the
                        // parse table
                        if (terminalIndex == -1)
                            terminalIndex = Array.IndexOf(setup.GetTerminalIndexes(), empty);
                        // If the top of the stack is not found
                        if (nonTerminalIndex == -1)
                            // and it's the end of file symbol, pop the stack and continue
                            if (i == symbols.Count)
                                break;
                            // else throw an exception
                            else
                                throw new Exception($"Unexpected symbol {symbol} at position {i}. Expected {stack.Peek()}.");
                        // Get the rule from the parse table
                        Rule rule = parseTable[nonTerminalIndex, terminalIndex];
                        // If the rule is null, throw an exception
                        if (rule == null)
                            // Indicate the expected symbols and the position of the unexpected symbol
                            throw new Exception($"Unexpected symbol {symbol} at position {i}. Expected {stack.Peek()}.");
                        else
                        {
                            // Find the leaf in the tree
                            var leaf = tree.FindLeaf(stack.Peek());
                            if (leaf == null)
                                throw new Exception("Leaf not found.");
                            // Pop the stack
                            stack.Pop();
                            // Add the rule to the leaf and push the symbols of the rule to the stack
                            for (int j = rule.Lenght - 1; j >= 0; j--)
                            {
                                tree.AddLeaf(rule[j], leaf);
                                stack.Push(rule[j]);
                            }
                        }
                    }
                }
            }

            return tree;
        }

        public object GetOutputObject(object input)
        {
            if (input.GetType() != typeof(TokenStream))
                throw new Exception(
                    "Invalid input type for parser. Expected TokenStream, got "
                    + input.GetType().ToString());
            return ParseAndAbstract(GetSymbols((TokenStream)input));
        }
    }
}