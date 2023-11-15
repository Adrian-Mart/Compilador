using System.Runtime.Serialization;

namespace Compilador.Graph
{
    /// <summary>
    /// Makes a Test that accepts a single character.
    /// </summary>
    [DataContract, KnownType(typeof(UnitaryDFA))]
    public class UnitaryDFA: ITester
    {
        /// <summary>
        /// The transition that the test accepts.
        /// </summary>
        [DataMember()]
        private int transition;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnitaryDFA"/> class.
        /// </summary>
        /// <param name="transition">The transition that the test accepts.</param>
        internal UnitaryDFA(int transition)
        { 
            this.transition = transition;
        }

        public bool TestIds(int[] ids){
            if(ids.Length != 1)
                return false;
            return ids[0] == transition;
        }
    }
}