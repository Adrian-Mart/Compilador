using System.Text;
using Compilador.Graph;

namespace Compilador.Processors
{
    public class LexerSetup
    {
        private static LexerSetup defaultSetup = new LexerSetup(' ', '\n', '#', "NEW_LINE", true, '"', "TEXT_DELIMITER");

        private char separator = ' ';
        private char lineBreak = '\n';
        private char comment = '#';
        private bool useText = true;
        private char textDelimiter = '"';
        private string textDelimiterToken = "TEXT_DELIMITER";
        private string lineBreakToken = "NEW_LINE";

        public char Separator { get => separator; }
        public char LineBreak { get => lineBreak; }
        public bool UseText { get => useText; }
        public char TextDelimiter { get => textDelimiter; }
        public string TextDelimiterToken { get => textDelimiterToken; }
        public static LexerSetup DefaultSetup { get => defaultSetup; }
        public char Comment { get => comment; }
        public string LineBreakToken { get => lineBreakToken; }

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