using System.Text;
using Compilador.Graph;

namespace Compilador.Processors
{
    /// <summary>
    /// The Processor interface is responsible for processing input strings.
    /// This interface is mainly disigned for the Lexer and Parser classes.
    /// </summary>
    public interface IProcessor
    {
        /// <summary>
        /// Gets the output string from the input string.
        /// </summary>
        /// <param name="input">Input text.</param>
        /// <returns></returns>
        public string GetOutputString(string input);
    }
}