
using System.Runtime.Serialization;
using System.Text;

namespace Compilador.Processors.Parser
{
    /// <summary>
    /// Stores all the grammar related data of the parser.
    /// </summary>
    [DataContract, KnownType(typeof(ParserSetup)), KnownType(typeof(Operator)), KnownType(typeof(Production)), KnownType(typeof(Rule))]
    public class ParserSetup
    {
        /// <summary>
        /// The index of the start symbol.
        /// </summary>
        [DataMember()]
        private int start;

        /// <summary>
        /// An index to keep track of the non terminals and terminals.
        /// </summary>
        [DataMember()]
        private int index = 0;

        /// <summary>
        /// A dictionary to store the non terminals and their indexes.
        /// </summary>
        [DataMember()]
        private Dictionary<string, int> nonTerminals;

        /// <summary>
        /// A dictionary to store the terminals and their indexes.
        /// </summary>
        [DataMember()]
        private Dictionary<string, int> terminals;

        /// <summary>
        /// A dictionary to store the productions and their nonterminal indexes.
        /// </summary>
        [DataMember()]
        private Dictionary<int, Production> productions;

        /// <summary>
        /// The rules of the grammar.
        /// </summary>
        [DataMember()]
        private Dictionary<Rule, int> rules;

        /// <summary>
        /// The hierarchy terminals.
        /// </summary>
        [DataMember()]
        private int[]? hierarchyTerminals;

        /// <summary>
        /// The operator terminals and their types.
        /// </summary>
        [DataMember()]
        private Operator[]? operatorTerminals;

        
        [DataMember()]
        private int newLineIndex;

        /// <summary>
        /// Gets the number of non terminals.
        /// </summary>
        public int NonTerminalsCount { get => nonTerminals.Count; }
        /// <summary>
        /// Gets the number of terminals.
        /// </summary>
        public int TerminalsCount { get => terminals.Count; }
        /// <summary>
        /// Gets the start symbol index.
        /// </summary>
        public int Start { get => start; }
        /// <summary>
        /// Gets the hierarchy terminals.
        /// </summary>
        public int[]? HierarchyTerminals { get => hierarchyTerminals; }
        /// <summary>
        /// Gets the operator terminals and their types.
        /// </summary>
        public Operator[]? OperatorTerminals { get => operatorTerminals; }
        /// <summary>
        /// Gets the productions.
        /// </summary>
        /// <value>The productions.</value>
        /// <remarks>The key is the non terminal index and the value is the production.</remarks>
        internal Dictionary<int, Production> Productions { get => productions; }

        /// <summary>
        /// The index of the EOF terminal.
        /// </summary>
        private const int eof = -1;
        /// <summary>
        /// Gets the index of the EOF terminal.
        /// </summary>
        public static int EOF { get => eof; }
        /// <summary>
        /// Gets the dictionary of rules.
        /// </summary>
        internal Dictionary<Rule, int> Rules { get => rules; }
        /// <summary>
        /// Gets the index of the new line terminal.
        /// </summary>
        public int NewLineIndex { get => newLineIndex;  }

        /// <summary>
        /// Constructs a new parser setup with the given start symbol, non terminals and terminals.
        /// </summary>
        /// <param name="start">The start symbol.</param>
        /// <param name="nonTerminals">The non terminals.</param>
        /// <param name="terminals">The terminals.</param>
        /// <exception cref="ArgumentException"></exception>
        internal ParserSetup(string start, List<string> nonTerminals,
            List<string> terminals, List<string>? hierarchy, List<(string, bool)>? operators, string newLine)
        {
            this.nonTerminals = new Dictionary<string, int>();
            this.terminals = new Dictionary<string, int>();
            productions = new Dictionary<int, Production>();
            rules = new Dictionary<Rule, int>();

            foreach (var nonTerminal in nonTerminals)
                AddNonTerminal(nonTerminal);
            foreach (var terminal in terminals)
                AddTerminal(terminal);

            if (!this.nonTerminals.TryGetValue(start, out this.start))
                throw new ArgumentException("Start symbol not found in production rules.");
            if (!this.terminals.TryGetValue(newLine, out newLineIndex))
                throw new ArgumentException("New line symbol not found in terminals.");

            if (hierarchy != null)
                hierarchyTerminals = hierarchy.Select(GetTerminalIndex).ToArray();
            if (operators != null)
            {
                operatorTerminals = new Operator[operators.Count];
                for (int i = 0; i < operators.Count; i++)
                {
                    operatorTerminals[i] = new Operator(operators[i].Item2, GetTerminalIndex(operators[i].Item1));
                }
            }
        }

        /// <summary>
        /// Adds a new non terminal to the setup.
        /// </summary>
        /// <param name="name">The name of the non terminal.</param>
        internal void AddNonTerminal(string name)
        {
            if (nonTerminals.ContainsKey(name))
                return;
            nonTerminals.Add(name, index);
            index++;
        }

        /// <summary>
        /// Adds a new terminal to the setup.
        /// </summary>
        /// <param name="name">The name of the terminal.</param>
        internal void AddTerminal(string name)
        {
            if (terminals.ContainsKey(name))
                return;
            terminals.Add(name, index);
            index++;
        }

        /// <summary>
        /// Adds a new production to the setup.
        /// </summary>
        /// <param name="name">The name of the non terminal.</param>
        /// <param name="production">The production to add.</param>
        internal void AddProduction(string name, Production production)
        {
            int id = GetNoTerminalIndex(name);
            if (id == -1)
                throw new ArgumentException("Non-terminal not found in production rules.");
            productions.Add(id, production);

            foreach (var rule in production.Rules)
            {
                if (!rules.ContainsKey(rule))
                    rules.Add(rule, rules.Count);
            }
        }

        /// <summary>
        /// Gets the index of the given non terminal.
        /// </summary>
        /// <param name="name">The name of the non terminal.</param>
        internal int GetNoTerminalIndex(string name)
        {
            int index;
            if (nonTerminals.TryGetValue(name, out index))
                return index;
            return -1;
        }

        /// <summary>
        /// Gets the index of the given terminal.
        /// </summary>
        /// <param name="name">The name of the terminal.</param>
        internal int GetTerminalIndex(string name)
        {
            int index;
            if (terminals.TryGetValue(name, out index))
                return index;
            return -1;
        }

        /// <summary>
        /// Gets the index of the given non terminal or terminal.
        /// Returns -1 if not found.
        /// </summary>
        /// <param name="v">The name of the non terminal or terminal.</param>
        internal int GetIndexOf(string v)
        {
            int index;
            if (nonTerminals.TryGetValue(v, out index))
                return index;
            if (terminals.TryGetValue(v, out index))
                return index;
            return -1;
        }

        /// <summary>
        /// Gets the index of the empty terminal.
        /// </summary>
        /// <returns>The index of the empty terminal.</returns>
        internal int GetIndexOfEmpty()
        {
            return GetTerminalIndex("~");
        }

        /// <summary>
        /// Gets the indexes of the terminals in the grammar.
        /// </summary>
        /// <returns>The indexes of the terminals.</returns>
        internal int[] GetTerminalIndexes()
        {
            return terminals.Values.ToArray();
        }

        /// <summary>
        /// Gets the indexes of the non terminals in the grammar.
        /// </summary>
        /// <returns>The indexes of the non terminals.</returns>
        internal int[] GetNonTerminalIndexes()
        {
            return nonTerminals.Values.ToArray();
        }

        public override string? ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("Start: ").Append(start).Append("\n");
            builder.Append("Non-Terminals: ").Append("\n");
            foreach (var nonTerminal in nonTerminals)
                builder.Append("\t").Append(nonTerminal.Key).Append(": ").Append(nonTerminal.Value).Append("\n");
            builder.Append("Terminals: ").Append("\n");
            foreach (var terminal in terminals)
                builder.Append("\t").Append(terminal.Key).Append(": ").Append(terminal.Value).Append("\n");
            builder.Append("Productions: ").Append("\n");
            foreach (var production in productions)
                builder.Append("\t").Append(production.Key).Append(": ").Append(production.Value.ToString()).Append("\n");
             builder.Append("Rules: ").Append("\n");
            foreach (var rule in rules)
                builder.Append("\t").Append(rule.Value).Append(") : ").Append(rule.Key.ToString()).Append("\n");
            return builder.ToString();
        }

        /// <summary>
        /// Gets the production of the given non terminal.
        /// </summary>
        /// <param name="nonTerminal">The non terminal.</param>
        /// <returns>The production of the given non terminal.</returns>
        internal Production GetProductionOf(int nonTerminal)
        {
            Production? production;
            if (productions.TryGetValue(nonTerminal, out production))
                return production;
            throw new ArgumentException("Non-terminal not found in production rules.");
        }

        /// <summary>
        /// Gets the rules of the given non terminal, excluding the empty rule.
        /// </summary>
        /// <param name="nonTerminal">The non terminal.</param>
        internal Rule[] GetNonEmptyRules(int nonTerminal)
        {
            int empty = GetIndexOfEmpty();
            List<Rule> rules = new List<Rule>();
            foreach (var rule in productions[nonTerminal].Rules)
            {
                if (!rule.Contains(empty))
                    rules.Add(rule);
            }
            return rules.ToArray();
        }

        /// <summary>
        /// Gets the rules where the given non terminal participates.
        /// </summary>
        /// <param name="nonTerminal">The non terminal.</param>
        /// <returns>A dictionary with the non terminals and the rules
        /// where they participate. The key is the non terminal index and
        /// the value is a list of rules.</returns>
        internal Dictionary<int, List<Rule>> GetRuleParticipation(int nonTerminal)
        {
            Dictionary<int, List<Rule>> participation = new Dictionary<int, List<Rule>>();

            // For each production
            foreach (var production in productions.Values)
            {
                // For each rule in production
                foreach (var rule in production.Rules)
                {
                    // If rule contains the non-terminal
                    if (rule.Contains(nonTerminal))
                    {
                        // Add the rule to the participation list
                        if (!participation.ContainsKey(production.NonTerminalId))
                            participation.Add(production.NonTerminalId, new List<Rule>());
                        participation[production.NonTerminalId].Add(rule);
                    }
                }
            }
            return participation;
        }

        /// <summary>
        /// Determines if the given index is a non terminal.
        /// </summary>
        /// <param name="index">The index of the symbol to check.</param>
        /// <returns>True if the symbol is a non terminal, false otherwise.</returns>
        internal bool IsTerminal(int index)
        {
            return terminals.ContainsValue(index);
        }

        internal string GetTokenOf(int index)
        {
            string token = terminals.FirstOrDefault(x => x.Value == index).Key;
            if (token == null)
                token = nonTerminals.FirstOrDefault(x => x.Value == index).Key;
            if (token == null)
                throw new ArgumentException("Index is either a non-terminal or a terminal.");
            return token;
        }

        internal Rule[] GetRules()
        {
            List<Rule> rules = new List<Rule>();
            foreach (var production in productions)
            {
                foreach (var rule in production.Value.Rules)
                {
                    if(rule != null && !rules.Contains(rule))
                        rules.Add(rule);
                }
            }
            return rules.ToArray();
        }
    }
}
