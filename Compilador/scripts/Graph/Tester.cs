
namespace Compilador.Graph
{
    /// <summary>
    /// Interface for testing strings.
    /// </summary>
    public interface ITester
    {
        /// <summary>
        /// Tests a string.
        /// </summary>
        /// <param name="s">The string to test.</param>
        /// <returns>True if the string passes the test, false otherwise.</returns>
        public bool TestString(string s);
    }
}