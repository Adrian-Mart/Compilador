using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graph
{
    public class EdgeInfo
    {
        private int start;
        private int end;
        private char transition;

        public int Start { get => start; }
        public int End { get => end; }
        public char Transition { get => transition; }

        public EdgeInfo(int start, int end, char transition)
        {
            this.start = start;
            this.end = end;
            this.transition = transition;
        }

        public override string? ToString()
        {
            return string.Format("{0}-{1}->{2}", start, transition, end);
        }
    }
}
