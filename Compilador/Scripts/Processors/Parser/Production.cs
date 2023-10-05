using System.Text;

namespace Compilador.Processors.Parser
{
    /// <summary>
    /// Represents production for multiple rules of a Non Terminal in the parser.
    /// </summary>
    internal class Production
    {
        /// <summary>
        /// The id of the non terminal.
        /// </summary>
        private int nonTerminalId;
        /// <summary>
        /// An index to keep track of the rules adding process.
        /// </summary>
        private int index = 0;
        /// <summary>
        /// The rules of the non terminal.
        /// </summary>
        private Rule[] rules;
        /// <summary>
        /// Gets the rules of the non terminal.
        /// </summary>
        internal Rule[] Rules { get => rules; }
        /// <summary>
        /// Gets the id of the non terminal that defines this production.
        /// </summary>
        public int NonTerminalId { get => nonTerminalId; }
        /// <summary>
        /// Creates a new production for the given non terminal and reserve
        /// space for the given number of rules.
        /// </summary>
        internal Production(int nonTerminalId, int rulesCount)
        {
            this.nonTerminalId = nonTerminalId;
            this.rules = new Rule[rulesCount];
        }
        /// <summary>
        /// Adds a new rule to the production.
        /// </summary>
        /// <param name="rule">The rule to add.</param>
        /// <returns>True if the rule was added, false otherwise.</returns>
        internal bool AddRule(Rule rule)
        {
            if (index > rules.Length)
                return false;
            rules[index] = rule;
            index++;
            return true;
        }
        
        public override string? ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(nonTerminalId).Append(" -> ");
            foreach (var rule in rules)
                builder.Append(rule).Append(" | ");
            builder.Remove(builder.Length - 3, 3);
            return builder.ToString();
        }
    }
}