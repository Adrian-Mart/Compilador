using System.Collections;
using System.Text;

namespace Compilador.Processors.Lexer
{
    public class TokenStream : IEnumerable<Token>
    {
        /// <summary>
        /// The stream of tokens.
        /// </summary>
        private Queue<Token> stream = new Queue<Token>();

        /// <summary>
        /// Initialize a new instance of the TokenStream class with a queue of tokens from
        /// the tokens types and tokens data.
        /// </summary>
        /// <param name="tokensTypes">The list of tokens types.</param>
        /// <param name="tokensData">The list of tokens data.</param>
        /// <exception cref="Exception"></exception>
        public TokenStream(List<string> tokensTypes, List<string> tokensData)
        {
            if (tokensTypes.Count != tokensData.Count)
                throw new Exception("The number of tokens types and tokens data must be the same.");
            for (int i = 0; i < tokensTypes.Count; i++)
            {
                stream.Enqueue(new Token(tokensTypes[i], tokensData[i]));
            }
        }

        /// <summary>
        /// Initialize a new instance of the TokenStream class from a queue of tokens.
        /// <summary>
        /// <param name="stream">The queue of tokens.</param>
        private TokenStream(Queue<Token> stream)
        {
            this.stream = new Queue<Token>(stream);
        }

        /// <summary>
        /// Peek the next token in the stream.
        /// </summary>
        public Token Peek()
        {
            return stream.Peek();
        }

        /// <summary>
        /// Get the next token in the stream.
        /// </summary>
        public Token Next()
        {
            return stream.Dequeue();
        }

        /// <summary>
        /// Copy the current state of this token stream.
        /// </summary>
        /// <returns>A copy of this token stream.</returns>
        public TokenStream Copy()
        {
            return new TokenStream(stream);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (Token token in stream)
            {
                sb.Append(token.ToString());
                sb.Append(" ");
            }
            sb.Remove(sb.Length - 1, 1);
            return sb.ToString();
        }

        public IEnumerator<Token> GetEnumerator()
        {
            return stream.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}