namespace Compilador.Graph
{
    /// <summary>
    /// Makes a Test that accepts a single character.
    /// </summary>
    public class UnitaryDFA: ITester
    {
        /// <summary>
        /// The transition that the test accepts.
        /// </summary>
        private char transition;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnitaryDFA"/> class.
        /// </summary>
        /// <param name="transition">The transition that the test accepts.</param>
        internal UnitaryDFA(char transition)
        { 
            this.transition = transition;
        }

        public bool TestString(string s){
            if(s.Length != 1)
                return false;
            return s[0] == transition;
        }
    }
}