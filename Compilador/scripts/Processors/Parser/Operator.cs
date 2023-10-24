using System.Runtime.Serialization;

namespace Compilador.Processors.Parser
{
    /// <summary>
    /// Represents an operator in the grammar.
    /// </summary>
    [DataContract, KnownType(typeof(Operator))]
    public class Operator
    {
        /// <summary>
        /// A value indicating whether the operator is binary.
        /// If false, it is unary.
        /// </summary>
        [DataMember()]
        private bool isBinary = false;

        /// <summary>
        /// The symbol of the operator.
        /// </summary>
        [DataMember()]
        private int symbol;

        /// <summary>
        /// Gets the symbol of the operator.
        /// </summary>
        public int Symbol { get => symbol; }

        /// <summary>
        /// Gets a value indicating whether the operator is binary. If false, it is unary.
        /// </summary>
        public bool IsBinary { get => isBinary; }

        /// <summary>
        /// Initializes a new instance of the <see cref="operatorTerminals"/> class with
        /// the specified symbol.
        /// </summary>
        /// <param name="isBinary">A value indicating whether the operator is binary.</param>
        /// <param name="symbol">The symbol of the operator.</param>
        public Operator(bool isBinary, int symbol)
        {
            this.isBinary = isBinary;
            this.symbol = symbol;
        }
    }
}