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
        /// The ParserSetup used for parsing.
        /// </summary>
        [DataMember()]
        private ParserSetup setup;

        /// <summary>
        /// Initializes a new instance of the Parser class with the specified ParserSetup.
        /// </summary>
        /// <param name="setup">The ParserSetup to use for parsing.</param>
        public Parser(ParserSetup setup)
        {
            this.setup = setup;
            var table = ParseTableLALR.GenerateTable(setup);
            Console.WriteLine(this);
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
            throw new NotImplementedException();
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