namespace Compilador.Processors.Lexer
{
    /// <summary>
    /// The Token class represents a token.
    /// It contains the token's type and data in string format.
    /// </summary>
    public class Token
    {
        /// <summary>
        /// The token's type.
        /// </summary>
        private string type;
        /// <summary>
        /// The token's data.
        /// </summary>
        private string data;

        /// <summary>
        /// Gets the token's type.
        /// </summary>
        public string Type { get => type; }
        /// <summary>
        /// Gets the token's data.
        /// </summary>
        public string Data { get => data; }

        /// <summary>
        /// Initializes a new instance of the Token class with a type and data.
        /// </summary>
        /// <param name="type">The token's type.</param>
        /// <param name="data">The token's data.</param>
        public Token(string type, string data)
        {
            this.type = type;
            this.data = data;
        }

        public override string? ToString()
        {
            return $"<{type}:{data}>";
        }
    }
}