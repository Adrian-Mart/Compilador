using System.Collections;
using System.Text;

namespace Compilador.Processors.Parser
{
    /// <summary>
    /// Represents a single production rule in the parser.
    /// </summary>
    internal class Rule : IEnumerable<int>
    {
        /// <summary>
        /// The terminals and/or no terminals of the rule.
        /// </summary>
        private int[] elements;

        /// <summary>
        /// Creates a new rule with the given elements.
        /// </summary>
        /// <param name="elements">The terminals and/or
        /// no terminals of the rule.</param>
        internal Rule(int[] elements)
        {
            this.elements = elements;
        }

        /// <summary>
        /// Gets the element at the given index.
        /// </summary>
        /// <param name="index">The index of the element.</param>
        public int this[int index] { get => elements[index]; }

        /// <summary>
        /// Gets the number of elements in the rule.
        /// </summary>
        public int Lenght { get => elements.Length; }

        /// <summary>
        /// Gets the index of the given element.
        /// </summary>
        /// <param name="element">The element to get the index of.</param>
        /// <returns>The index of the element,-1 if the element is not 
        /// in the rule.</returns>
        internal int IndexOf(int element)
        {
            return System.Array.IndexOf(elements, element);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the elements.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the elements.</returns>
        public IEnumerator<int> GetEnumerator()
        {
            return ((IEnumerable<int>)elements).GetEnumerator();
        }

        public override string? ToString()
        {
            StringBuilder builder = new StringBuilder();
            foreach (var element in elements)
                builder.Append(element).Append(" ");
            builder.Remove(builder.Length - 1, 1);
            return builder.ToString();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the elements.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the elements.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<int>)elements).GetEnumerator();
        }
    }
}