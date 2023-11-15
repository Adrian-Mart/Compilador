
namespace Compilador.Graph
{
    /// <summary>
    /// Interface for testing strings.
    /// </summary>
    public interface ITester
    {
        /// <summary>
        /// Tests the given array of IDs. Returns true if the ids
        /// are accepted by the automaton.
        /// </summary>
        /// <param name="ids">The array of IDs to test.</param>
        /// <returns>True if the ids are accepted by the automaton.</returns>
        public bool TestIds(int[] ids);
    }
}