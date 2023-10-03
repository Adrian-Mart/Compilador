using System.Runtime.Serialization;
using System.Text;
using Compilador.Graph;

namespace Compilador.Processors
{
    /// <summary>
    /// The Lexer class is responsible for tokenizing input strings using a list of DFAs (Deterministic Finite Automata) and a list of tokens.
    /// </summary>
    [DataContract, KnownType(typeof(LexerSetup)), KnownType(typeof(DFA)), KnownType(typeof(UnitaryDFA))]
    public class Lexer : IProcessor
    {
        /// <summary>
        /// The setup for the lexer.
        /// </summary>
        [DataMember()]
        LexerSetup setup;

        /// <summary>
        /// The list of DFAs to use for tokenization.
        /// </summary>
        [DataMember()]
        List<ITester> automatas = new List<ITester>();
        /// <summary>
        /// The list of tokens to use for tokenization.
        /// </summary>
        [DataMember()]
        List<string> tokens = new List<string>();

        /// <summary>
        /// Initializes a new instance of the Lexer class with a list of DFAs and a list of tokens.
        /// </summary>
        /// <param name="automatas">The list of DFAs to use for tokenization.</param>
        /// <param name="tokens">The list of tokens to use for tokenization.</param>
        /// <param name="setup">The setup for the lexer.</param>
        public Lexer(List<ITester> automatas, List<string> tokens, LexerSetup setup)
        {
            this.automatas = automatas;
            this.tokens = tokens;
            this.setup = setup;
        }

        /// <summary>
        /// Tokenizes the input string using the list of DFAs and tokens.
        /// </summary>
        /// <param name="input">The input string to tokenize.</param>
        /// <returns>A list of tokens.</returns>
        private List<string> Tokenize(string input)
        {
            List<string> tokenStream = new List<string>();
            if (setup.UseText)
            {
                input = ParseStrings(input);
            }

            // Clear the input string
            input = ClearInput(input);

            int lineCount = 0;
            // Split the input string into lines
            foreach (var line in input.Split(setup.LineBreak))
            {
                lineCount++;
                // Split the line into lexemes
                foreach (var lexeme in line.Split(setup.Separator))
                {
                    if (lexeme == "")
                        continue;
                    // Identify the token of the lexeme
                    var token = IdentifyTokens(lexeme);
                    if (token != null && token.Count > 0)
                        tokenStream.AddRange(token);
                    else
                    {
                        // If the lexeme is not a token, throw an exception.
                        Console.WriteLine(
                            string.Format(
                                "Lexical error in line {0}: {1} has no matching token. Is it lonely like me?",
                                lineCount, lexeme)
                        );
                        throw new Exception("No token was found for <" + lexeme + ">, just as lonely as I am.");
                    }

                }

                tokenStream.Add(setup.LineBreakToken);
            }

            // Return the list of tokens.
            return tokenStream;
        }

        /// <summary>
        /// Returns a string of tokens for the input string using the list of DFAs and tokens.
        /// </summary>
        /// <param name="input">The input string to tokenize.</param>
        /// <returns>A string of tokens.</returns>
        public string GetOutputString(string input)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var token in Tokenize(input))
            {
                sb.Append(token);
                if (token == setup.LineBreakToken)
                    sb.Append("\n");
                else
                    sb.Append(" ");
            }
            sb.Remove(sb.Length - 1, 1);
            return sb.ToString();
        }

        /// <summary>
        /// Identifies the token of a string.
        /// </summary>
        /// <param name="text">Text being indentified.</param>
        /// <returns>Token of a string and null if there is no match for this text.</returns>
        private string? IdentifyToken(string text)
        {
            if (text == "╔") return setup.TextDelimiterToken;
            foreach (var automata in automatas)
            {
                if (automata.TestString(text))
                    return tokens[automatas.IndexOf(automata)];
            }
            return null;
        }

        private List<string> IdentifyTokens(string text)
        {
            List<string> tokens = new List<string>();
            return IdentifyTokens(tokens, text);
        }

        private List<string> IdentifyTokens(List<string> tokens, string text)
        {
            if(string.IsNullOrEmpty(text))
                return tokens;

            StringBuilder mainSb = new StringBuilder();
            string auxSb = "";
            mainSb.Append(text);
            for (int i = text.Length - 1; i >= 0; i--)
            {
                string? token = IdentifyToken(mainSb.ToString());
                
                if(token == null)
                {
                    auxSb = string.Concat(mainSb.ToString().Last(), auxSb);
                    mainSb.Remove(i, 1);
                }
                else
                {
                    tokens.Add(token);
                    break;
                }
            }
            
            return IdentifyTokens(tokens, auxSb);
        }

        private string ParseStrings(string input)
        {
            var tempText = input.Split(setup.TextDelimiter);
            // Check if the text delimiter count is even
            if (tempText.Length % 2 == 0)
            {
                Console.WriteLine("Lexical error: You started a string but didn't finish it. How frustrating.");
                throw new Exception("Invalid text delimiter count.");
            }
            // Replace the text delimiter with a special character
            StringBuilder sb = new StringBuilder();
            int index = 0;
            foreach (var item in tempText)
            {
                if (index % 2 == 1)
                    sb.Append('╔');
                else
                    sb.Append(item);
                index++;
            }

            return sb.ToString();
        }

        /// <summary>
        /// Clears the input string from comments, empty lines and spaces.
        /// </summary>
        /// <param name="input">Input text</param>
        /// <returns>A string with the input text without comments, empty
        /// lines and spaces.</returns>
        private string ClearInput(string input)
        {
            StringBuilder sb = new StringBuilder();

            // Split the input string into lines
            foreach (var line in input.Split(setup.LineBreak))
            {
                if (line.Length == 0) continue;
                if (line[0] == setup.Comment) continue;
                if (line[0] == setup.LineBreak) continue;
                for (int i = 0; i < line.Length; i++)
                {
                    if (line[i] != setup.Separator || line[i] != setup.LineBreak)
                        break;
                }
                sb.Append(line).Append(setup.LineBreak);
            }
            // Remove the last line break
            sb.Remove(sb.Length - 1, 1);
            // Return the list of tokens.
            return sb.ToString();
        }

        public void Serialize(string fileName)
        {
            using (StreamWriter writer = new StreamWriter(fileName))
            {
                DataContractSerializerSettings settings = new DataContractSerializerSettings
                {
                    PreserveObjectReferences = true
                };
                DataContractSerializer obj = new DataContractSerializer(typeof(Lexer), settings);
                obj.WriteObject(writer.BaseStream, this);
            }
        }

        public static IProcessor? Deserialize(string fileName)
        {
            Lexer? lexer = null;
            using (StreamReader writer = new StreamReader(fileName))
            {
                DataContractSerializer obj =
                    new DataContractSerializer(typeof(Lexer));
                lexer = (Lexer?)obj.ReadObject(writer.BaseStream);
            }
            return lexer;
        }
    }
}