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

        /// <summary>
        /// Serializes the processor to a file.
        /// </summary>
        /// <param name="fileName">The name of the file
        /// to serialize to.</param>
        public void Serialize(string fileName);

        /// <summary>
        /// Deserializes the processor from a file.
        /// </summary>
        /// <param name="fileName">The name of the file
        /// to deserialize from.</param>
        /// <returns>The deserialized processor.</returns>
        public static abstract IProcessor? Deserialize(string fileName);
    }
}