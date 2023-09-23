
namespace Compilador.Graph
{
    public class UnitaryDFA: ITester
    {
        private char transition;

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