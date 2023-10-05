using System.Text;

namespace Compilador.Processors.Parser
{
    /// <summary>
    /// The Parser class is responsible for parsing the input code and generating a parse tree.
    /// </summary>
    public class Parser : IProcessor
    {
        /// <summary>
        /// The recursive limit of the parser.
        /// </summary>
        private const int RECURSIVE_LIMIT = 50;
        /// <summary>
        /// The ParserSetup used for parsing.
        /// </summary>
        private ParserSetup setup;
        /// <summary>
        /// The first set of every non-terminal of the grammar.
        /// </summary>
        private int[][] first;
        /// <summary>
        /// The follow set of every non-terminal of the grammar.
        /// </summary>
        private int[][] follow;
        /// <summary>
        /// The parse table of the grammar.
        /// </summary>
        private Rule[,][] parseTable;

        /// <summary>
        /// Initializes a new instance of the Parser class with the specified ParserSetup.
        /// </summary>
        /// <param name="setup">The ParserSetup to use for parsing.</param>
        public Parser(ParserSetup setup)
        {
            this.setup = setup;
            first = new int[setup.NonTerminalsCount][];
            follow = new int[setup.NonTerminalsCount][];
            parseTable = new Rule[setup.NonTerminalsCount, setup.TerminalsCount][];

            CalculateFirst();
            CalculateFollow();
            CalculateParseTable();

            Console.WriteLine("\n___________________________\n");
            Console.WriteLine(this);
        }

        /// <summary>
        /// Calculates the parse table of the grammar, based on the first and follow sets.
        /// </summary>
        /// <exception cref="Exception"></exception>
        private void CalculateParseTable()
        {
            int empty = setup.GetIndexOfEmpty();
            Rule[] emptyRule = new Rule[] { new Rule(new int[] { empty }) };
            int[] terminals = setup.GetTerminalIndexes();
            for (int i = 0; i < setup.NonTerminalsCount; i++)
            {
                Rule[] ntRules = setup.GetProductionOf(i).Rules;
                for (int j = 0; j < setup.TerminalsCount; j++)
                {
                    if(terminals[j] == empty && follow[i].Contains(-1))
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
                        if (rules.Count != 0) parseTable[i, j] = rules.ToArray();
                    }
                }
            }
        }

        /// <summary>
        /// Calculates the first of every non terminal of the grammar.
        /// </summary>
        private void CalculateFirst()
        {
            int[] nonTerminals = setup.GetNonTerminalIndexes();
            int index = 0;
            foreach (var nonTerminal in nonTerminals)
            {
                if (first[index] == null)
                    first[index++] = CalculateFirstOfNT(nonTerminal, 0);
            }
        }

        /// <summary>
        /// Calculates the follow of every non terminal of the grammar.
        /// </summary>
        /// <exception cref="Exception"></exception>
        private void CalculateFollow()
        {
            int[] nonTerminals = setup.GetNonTerminalIndexes();
            int index = 0;
            foreach (var nonTerminal in nonTerminals)
            {
                if (follow[index] == null)
                    follow[index++] = CalculateFollowOfNT(nonTerminal, 0);
            }
        }

        /// <summary>
        /// Calculates the follow of a non-terminal.
        /// </summary>
        /// <param name="nonTerminal">The non-terminal to calculate the follow of.</param>
        /// <param name="recursions">The number of recursions.
        /// Used to prevent undesired recursion.</param>
        /// <returns>The follow of the non-terminal.</returns>
        private int[] CalculateFollowOfNT(int nonTerminal, int recursions)
        {
            if (nonTerminal == setup.Start)
                return new int[] { -1 };

            int empty = setup.GetIndexOfEmpty();

            List<int> follow = new List<int>();
            var rulesParticipation = setup.GetRuleParticipation(nonTerminal);
            foreach (var ruleParticipation in rulesParticipation)
            {
                if (ruleParticipation.Key == nonTerminal)
                    continue;
                foreach (var rule in ruleParticipation.Value)
                {
                    int indexInRule = rule.IndexOf(nonTerminal);
                    if (indexInRule == -1) continue;

                    if (indexInRule == rule.Lenght - 1)
                    {
                        follow.AddRange(GetOrCalculateFollowOfNT(ruleParticipation.Key, recursions));
                        continue;
                    }

                    bool containsEmpty = false;
                    for (int i = indexInRule + 1; i < rule.Lenght; i++)
                    {
                        if (setup.isTerminal(rule[i]))
                        {
                            if (rule[i] == empty)
                                throw new Exception("Empty terminal found inside rule.");
                            follow.Add(rule[i]);
                            break;
                        }
                        else
                        {
                            int[] firstOfItem = GetOrCalculateFirstOfNT(rule[i], recursions);
                            foreach (var firstItem in firstOfItem)
                            {
                                if (firstItem == empty)
                                {
                                    containsEmpty = true;
                                    follow.AddRange(GetOrCalculateFollowOfNT(ruleParticipation.Key, recursions));
                                }
                                else
                                    follow.Add(firstItem);
                            }
                            if (!containsEmpty)
                                break;
                        }
                        if (containsEmpty)
                            follow.Add(-1);
                    }
                }
            }

            follow = follow.Distinct().ToList();
            return follow.ToArray();
        }

        /// <summary>
        /// Gets the follow of a non-terminal. If it's not calculated yet, calculates it.
        /// </summary>
        /// <param name="nonTerminal">The non-terminal to get the follow of.</param>
        /// <param name="recursions">The number of recursions.
        /// Used to prevent undesired recursion.</param>
        /// <returns>The follow of the non-terminal.</returns>
        /// <exception cref="Exception"></exception>
        private int[] GetOrCalculateFollowOfNT(int nonTerminal, int recursions)
        {
            if (recursions >= RECURSIVE_LIMIT)
                throw new Exception("Recursive limit reached. Check for undesired recursion in grammar.");
            if (follow[nonTerminal] == null)
                follow[nonTerminal] = CalculateFollowOfNT(nonTerminal, recursions + 1);
            return follow[nonTerminal];
        }

        /// <summary>
        /// Calculates the first of a non-terminal.
        /// </summary>
        /// <param name="nonTerminal">The non-terminal to
        /// calculate the first of.</param>
        /// <param name="recursions">The number of recursions.
        /// Used to prevent undesired recursion.</param>
        /// <returns>The first of the non-terminal.</returns>
        private int[] CalculateFirstOfNT(int nonTerminal, int recursions)
        {
            int empty = setup.GetIndexOfEmpty();

            // Initialize a new list for terminals: first
            List<int> first = new List<int>();
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
                        first.Add(item);
                        break;
                    }
                    // If the item is a non-terminal
                    else
                    {
                        containsEmpty = false;
                        // Get the first of the non-terminal
                        int[] firstOfItem = GetOrCalculateFirstOfNT(item, recursions);
                        // Add each item of the first of the non-terminal to first, except empty terminal
                        foreach (var firstItem in firstOfItem)
                        {
                            if (firstItem == empty)
                                containsEmpty = true;
                            else
                                first.Add(firstItem);
                        }
                        // If the first of the non-terminal doesn't contain empty, break
                        if (!containsEmpty)
                            break;
                    }
                }
                // If the rule contains empty, add empty to first
                if (containsEmpty)
                    first.Add(empty);
            }

            // Sort and return first
            first.Sort();
            first = first.Distinct().ToList();
            first.Sort();
            return first.ToArray();
        }

        /// <summary>
        /// Gets the first of a non-terminal.
        /// </summary>
        /// <param name="nonTerminal">The non-terminal to get the first of.</param>
        /// <param name="recursions">The number of recursions. Used to prevent undesired recursion.</param>
        /// <returns>The first of the non-terminal.</returns>
        /// <exception cref="Exception"></exception>
        private int[] GetOrCalculateFirstOfNT(int nonTerminal, int recursions)
        {
            if (recursions >= RECURSIVE_LIMIT)
                throw new Exception("Recursive limit reached. Check for undesired recursion in grammar.");
            if (first[nonTerminal] == null)
                first[nonTerminal] = CalculateFirstOfNT(nonTerminal, recursions + 1);
            return first[nonTerminal];
        }

        public string GetOutputString(string input)
        {
            throw new NotImplementedException();
        }

        public void Serialize(string fileName)
        {
            throw new NotImplementedException();
        }

        public static IProcessor? Deserialize(string fileName)
        {
            throw new NotImplementedException();
        }

        public override string? ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("NonTerminal\tFirst\t\t\tFollow\n");

            for (int i = 0; i < setup.NonTerminalsCount; i++)
            {
                int[] firstSet = first[i];
                int[] followSet = follow[i];

                sb.Append($"{setup.GetNonTerminalIndexes()[i]}\t\t{{ {string.Join(", ", firstSet)} }}\t\t{{ {string.Join(", ", followSet)} }}\n");
            }

            sb.Append("\n\nParse Table\n\t");
            foreach (var terminal in setup.GetTerminalIndexes())
            {
                if(terminal == setup.GetIndexOfEmpty())
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
    }
}