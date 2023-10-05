using System.Runtime.Serialization;
using System.Text;
using Compilador.Graph;

namespace Compilador.Processors.Lexer
{
    /// <summary>
    /// Represents the setup for the lexer.
    /// </summary>
    [DataContract, KnownType(typeof(LexerSetup))]
    public class LexerSetup
    {
        /// <summary>
        /// The default setup.
        /// </summary>
        private static LexerSetup defaultSetup = new LexerSetup(' ', '\n', '#', "NEW_LINE", true, '"', "TEXT_DELIMITER");

        /// <summary>
        /// The separator character.
        /// </summary>
        [DataMember()]
        private char separator = ' ';
        /// <summary>
        /// The line break character.
        /// </summary>
        [DataMember()]
        private char lineBreak = '\n';
        /// <summary>
        /// The comment character.
        /// </summary>
        [DataMember()]
        private char comment = '#';
        /// <summary>
        /// A value indicating whether to use text.
        /// </summary>
        [DataMember()]
        private bool useText = true;
        /// <summary>
        /// The text delimiter character.
        /// </summary>
        [DataMember()]
        private char textDelimiter = '"';
        /// <summary>
        /// The text delimiter token.
        /// </summary>
        [DataMember()]
        private string textDelimiterToken = "TEXT_DELIMITER";
        /// <summary>
        /// The line break token.
        /// </summary>
        [DataMember()]
        private string lineBreakToken = "NEW_LINE";

        /// <summary>
        /// Gets the separator character.
        /// </summary>
        public char Separator { get => separator; }
        /// <summary>
        /// Gets the line break character.
        /// </summary>
        public char LineBreak { get => lineBreak; }
        /// <summary>
        /// Gets a value indicating whether to use text.
        /// </summary>
        public bool UseText { get => useText; }
        /// <summary>
        /// Gets the text delimiter character.
        /// </summary>
        public char TextDelimiter { get => textDelimiter; }
        /// <summary>
        /// Gets the text delimiter token.
        /// </summary>
        public string TextDelimiterToken { get => textDelimiterToken; }
        /// <summary>
        /// Gets the comment character.
        /// </summary>
        public char Comment { get => comment; }
        /// <summary>
        /// Gets the line break token.
        /// </summary>
        public string LineBreakToken { get => lineBreakToken; }
        /// <summary>
        /// Gets the default setup.
        /// </summary>
        public static LexerSetup DefaultSetup { get => defaultSetup; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LexerSetup"/> class.
        /// </summary>
        /// <param name="separator">The separator character.</param>
        /// <param name="lineBreak">The line break character.</param>
        /// <param name="comment">The comment character.</param>
        /// <param name="lineBreakToken">The line break token.</param>
        /// <param name="useText">A value indicating whether to use text.</param>
        /// <param name="textDelimiter">The text delimiter character.</param>
        /// <param name="textDelimiterToken">The text delimiter token.</param>
        public LexerSetup(char separator, char lineBreak, char comment, string lineBreakToken, bool useText,
            char textDelimiter, string textDelimiterToken)
        {
            this.separator = separator;
            this.lineBreak = lineBreak;
            this.comment = comment;
            this.lineBreakToken = lineBreakToken;
            this.useText = useText;
            this.textDelimiter = textDelimiter;
            this.textDelimiterToken = textDelimiterToken;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LexerSetup"/> class.
        /// </summary>
        /// <param name="separator">The separator character.</param>
        /// <param name="lineBreak">The line break character.</param>
        /// <param name="comment">The comment character.</param>
        /// <param name="lineBreakToken">The line break token.</param>
        public LexerSetup(char separator, char lineBreak, char comment, string lineBreakToken)
        {
            this.separator = separator;
            this.lineBreak = lineBreak;
            this.comment = comment;
            this.lineBreakToken = lineBreakToken;
            this.useText = false;
            this.textDelimiter = '~';
            this.textDelimiterToken = "";
        }
    }
}