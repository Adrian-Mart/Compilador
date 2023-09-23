using Compilador.Graph;

namespace Compilador.RegexInterpreter
{
    /// <summary>
    /// Class that interprets a regular expression and returns a DFA.
    /// </summary>
    internal static class Interpreter
    {

        /// <summary>
        /// Interprets a regular expression and returns a DFA.
        /// </summary>
        /// <param name="exp">
        /// Expression to interpret.
        /// </param>
        /// <returns>
        /// ITester object that represents the DFA and can 
        /// test whether a string belongs to the language 
        /// defined by the expression.
        /// </returns>
        internal static ITester Interpret(string exp)
        {
            string interpretedExp;
            return Interpret(exp, out interpretedExp);
        }

        /// <summary>
        /// Interprets a regular expression and returns a DFA.
        /// </summary>
        /// <param name="exp">Expression to interpret.</param>
        /// <param name="interpretedExp">A string containing 
        /// the interpreted expression. Mainly for debugging purposes.</param>
        /// <returns>
        /// ITester object that represents the DFA and can 
        /// test whether a string belongs to the language 
        /// defined by the expression.
        /// </returns>
        /// <exception cref="System.Exception"></exception>
        /// <exception cref="Exception"></exception>
        internal static ITester Interpret(string exp, out string interpretedExp)
        {
            // If the expression is empty, throw an exception.
            if (exp == null || exp.Length == 0) throw new System.Exception("Empty expression.");
            // If the expression is a single character, return a UnitaryDFA.
            else if (exp.Length == 1)
            {
                interpretedExp = exp;
                return new UnitaryDFA(exp[0]);
            }

            // If the expression is not empty, interpret it.
            List<char> alphabet = new List<char>();
            List<EdgeInfo> edgeInfo = new List<EdgeInfo>();
            Stack<string> values = new Stack<string>();
            Stack<int> nodeIds = new Stack<int>();
            int nodeIndex = -1;

            // Parse the expression.
            string parsedExp = Parser.Parse(exp);

            // For each character in the parsed expression,
            foreach (char c in parsedExp)
            {
                // Add the corresponding transition to the alphabet and
                // append the corresponding subgraph to the edgeInfo list.
                if (c == '|')
                    nodeIndex = OrEdgeInfo(edgeInfo, nodeIds, values, nodeIndex);
                else if (c == '.')
                    nodeIndex = JoinEdgeInfo(edgeInfo, nodeIds, values, nodeIndex);
                else if (c == '+')
                    nodeIndex = PlusEdgeInfo(edgeInfo, nodeIds, values, nodeIndex);
                else if (c == '*')
                    nodeIndex = StarEdgeInfo(edgeInfo, nodeIds, values, nodeIndex);
                else
                {
                    var transition = c;
                    if (Parser.Correspondency.ContainsKey(c))
                        transition = Parser.Correspondency[c];

                    values.Push(transition.ToString());
                    if (!alphabet.Contains(transition))
                        alphabet.Add(transition);
                }
            }

            // If the expression was not parsed correctly, throw an exception.
            if (values.Count > 1) throw new Exception(
                string.Format("The parsed expression \"{1}\" {0}",
                "has more operators or values than expected", parsedExp));

            // If the expression was parsed correctly, return the DFA.
            int end = nodeIds.Pop();
            int start = nodeIds.Pop();
            alphabet.Sort();
            interpretedExp = values.Pop();
            var temp = new TempDFA(start, end, edgeInfo.ToArray(), alphabet.ToArray());
            return temp.Automata;
        }

        
        /// <summary>
        /// Adds edges and nodes to the given edgeInfo list, representing the '|' operator
        /// between two elements.
        /// </summary>
        /// <param name="edgeInfo">The list of EdgeInfo objects to add the edges to.</param>
        /// <param name="nodeIds">The stack of node IDs to use for creating new nodes.</param>
        /// <param name="values">The stack of string values to use for creating new nodes.</param>
        /// <param name="nodeIndex">The index of the current node.</param>
        /// <returns>The index of the last node added to the edgeInfo list.</returns>
        private static int OrEdgeInfo(List<EdgeInfo> edgeInfo, Stack<int> nodeIds, Stack<string> values, int nodeIndex)
        {
            string b = values.Pop();
            string a = values.Pop();

            int lettersOrNumbers = 2;
            if (a.Length == 1) lettersOrNumbers--;
            if (b.Length == 1) lettersOrNumbers--;

            // a|b ->  1-e>2, 3-e>6, 1-e>4, 5-e>6, 2-a>3, 4-b>5
            edgeInfo.Add(new EdgeInfo(nodeIndex + 1, nodeIndex + 2, '~'));
            edgeInfo.Add(new EdgeInfo(nodeIndex + 3, nodeIndex + 6, '~'));
            edgeInfo.Add(new EdgeInfo(nodeIndex + 1, nodeIndex + 4, '~'));
            edgeInfo.Add(new EdgeInfo(nodeIndex + 5, nodeIndex + 6, '~'));

            if (lettersOrNumbers == 0)
            {
                edgeInfo.Add(new EdgeInfo(nodeIndex + 4, nodeIndex + 5, a[0]));
                edgeInfo.Add(new EdgeInfo(nodeIndex + 2, nodeIndex + 3, b[0]));
            }
            else if (lettersOrNumbers == 1)
            {
                if (b.Length != 1)
                {
                    int bNodeEnd = nodeIds.Pop();
                    int bNodeStart = nodeIds.Pop();
                    edgeInfo.Add(new EdgeInfo(nodeIndex + 4, bNodeStart, '~'));
                    edgeInfo.Add(new EdgeInfo(bNodeEnd, nodeIndex + 5, '~'));
                    edgeInfo.Add(new EdgeInfo(nodeIndex + 2, nodeIndex + 3, a[0]));
                }
                else
                {
                    int aNodeEnd = nodeIds.Pop();
                    int aNodeStart = nodeIds.Pop();
                    edgeInfo.Add(new EdgeInfo(nodeIndex + 4, nodeIndex + 5, b[0]));
                    edgeInfo.Add(new EdgeInfo(nodeIndex + 2, aNodeStart, '~'));
                    edgeInfo.Add(new EdgeInfo(aNodeEnd, nodeIndex + 3, '~'));
                }
            }
            else
            {
                int bNodeEnd = nodeIds.Pop();
                int bNodeStart = nodeIds.Pop();
                int aNodeEnd = nodeIds.Pop();
                int aNodeStart = nodeIds.Pop();
                edgeInfo.Add(new EdgeInfo(nodeIndex + 4, bNodeStart, '~'));
                edgeInfo.Add(new EdgeInfo(bNodeEnd, nodeIndex + 5, '~'));
                edgeInfo.Add(new EdgeInfo(nodeIndex + 2, aNodeStart, '~'));
                edgeInfo.Add(new EdgeInfo(aNodeEnd, nodeIndex + 3, '~'));
            }

            nodeIds.Push(nodeIndex + 1);
            nodeIds.Push(nodeIndex + 6);

            values.Push(string.Format("|({0},{1})", a, b));
            return nodeIndex + 6;
        }

        /// <summary>
        /// Adds edges and nodes to the given edgeInfo list, representing the '.' operator between two elements.
        /// </summary>
        /// <param name="edgeInfo">The list of EdgeInfo objects to add the edges to.</param>
        /// <param name="nodeIds">The stack of node IDs to use for creating new nodes.</param>
        /// <param name="values">The stack of string values to use for creating new nodes.</param>
        /// <param name="nodeIndex">The index of the current node.</param>
        /// <returns>The index of the last node added to the edgeInfo list.</returns>
        private static int JoinEdgeInfo(List<EdgeInfo> edgeInfo, Stack<int> nodeIds, Stack<string> values, int nodeIndex)
        {
            // a.b ->  1-a>2, 2-b>3
            string b = values.Pop();
            string a = values.Pop();

            int lettersOrNumbers = 2;
            if (a.Length == 1) lettersOrNumbers--;
            if (b.Length == 1) lettersOrNumbers--;

            if (lettersOrNumbers == 0)
            {
                edgeInfo.Add(new EdgeInfo(nodeIndex + 1, nodeIndex + 2, a[0]));
                edgeInfo.Add(new EdgeInfo(nodeIndex + 2, nodeIndex + 3, b[0]));
            }
            else if (lettersOrNumbers == 1)
            {
                if (b.Length != 1)
                {
                    int bNodeEnd = nodeIds.Pop();
                    int bNodeStart = nodeIds.Pop();
                    edgeInfo.Add(new EdgeInfo(nodeIndex + 2, bNodeStart, '~'));
                    edgeInfo.Add(new EdgeInfo(bNodeEnd, nodeIndex + 3, '~'));
                    edgeInfo.Add(new EdgeInfo(nodeIndex + 1, nodeIndex + 2, a[0]));
                }
                else
                {
                    int aNodeEnd = nodeIds.Pop();
                    int aNodeStart = nodeIds.Pop();
                    edgeInfo.Add(new EdgeInfo(nodeIndex + 2, nodeIndex + 3, b[0]));
                    edgeInfo.Add(new EdgeInfo(nodeIndex + 1, aNodeStart, '~'));
                    edgeInfo.Add(new EdgeInfo(aNodeEnd, nodeIndex + 2, '~'));
                }
            }
            else
            {
                int bNodeEnd = nodeIds.Pop();
                int bNodeStart = nodeIds.Pop();
                int aNodeEnd = nodeIds.Pop();
                int aNodeStart = nodeIds.Pop();
                edgeInfo.Add(new EdgeInfo(nodeIndex + 2, bNodeStart, '~'));
                edgeInfo.Add(new EdgeInfo(bNodeEnd, nodeIndex + 3, '~'));
                edgeInfo.Add(new EdgeInfo(nodeIndex + 1, aNodeStart, '~'));
                edgeInfo.Add(new EdgeInfo(aNodeEnd, nodeIndex + 2, '~'));
            }

            nodeIds.Push(nodeIndex + 1);
            nodeIds.Push(nodeIndex + 3);

            values.Push(string.Format(".({0},{1})", a, b));
            return nodeIndex + 3;
        }

        /// <summary>
        /// Adds edges and nodes to the given edgeInfo list, representing the '*' operator.
        /// </summary>
        /// <param name="edgeInfo">The list of EdgeInfo objects to add the edges to.</param>
        /// <param name="nodeIds">The stack of node IDs to use for creating new nodes.</param>
        /// <param name="values">The stack of string values to use for creating new nodes.</param>
        /// <param name="nodeIndex">The index of the current node.</param>
        /// <returns>The index of the last node added to the edgeInfo list.</returns>
        private static int StarEdgeInfo(List<EdgeInfo> edgeInfo, Stack<int> nodeIds, Stack<string> values, int nodeIndex)
        {
            // a*  ->  1-e>2, 2-a>3, 3-e>4, 3-e>2, 1-e>4
            string a = values.Pop();

            edgeInfo.Add(new EdgeInfo(nodeIndex + 1, nodeIndex + 2, '~'));
            edgeInfo.Add(new EdgeInfo(nodeIndex + 3, nodeIndex + 4, '~'));
            edgeInfo.Add(new EdgeInfo(nodeIndex + 3, nodeIndex + 2, '~'));
            edgeInfo.Add(new EdgeInfo(nodeIndex + 1, nodeIndex + 4, '~'));

            if (a.Length == 1)
            {
                edgeInfo.Add(new EdgeInfo(nodeIndex + 2, nodeIndex + 3, a[0]));
            }
            else
            {
                int aNodeEnd = nodeIds.Pop();
                int aNodeStart = nodeIds.Pop();
                edgeInfo.Add(new EdgeInfo(nodeIndex + 2, aNodeStart, '~'));
                edgeInfo.Add(new EdgeInfo(aNodeEnd, nodeIndex + 3, '~'));
            }

            nodeIds.Push(nodeIndex + 1);
            nodeIds.Push(nodeIndex + 4);

            values.Push(string.Format("*({0})", a));
            return nodeIndex + 4;
        }

        /// <summary>
        /// Adds edges and nodes to the given edgeInfo list, representing the '+' operator.
        /// </summary>
        /// <param name="edgeInfo">The list of EdgeInfo objects to add the edges to.</param>
        /// <param name="nodeIds">The stack of node IDs to use for creating new nodes.</param>
        /// <param name="values">The stack of string values to use for creating new nodes.</param>
        /// <param name="nodeIndex">The index of the current node.</param>
        /// <returns>The index of the last node added to the edgeInfo list.</returns>
        private static int PlusEdgeInfo(List<EdgeInfo> edgeInfo, Stack<int> nodeIds, Stack<string> values, int nodeIndex)
        {
            // a+  ->  1-e>2, 2-a>3, 3-e>4, 3-e>2
            string a = values.Pop();

            edgeInfo.Add(new EdgeInfo(nodeIndex + 1, nodeIndex + 2, '~'));
            edgeInfo.Add(new EdgeInfo(nodeIndex + 3, nodeIndex + 4, '~'));
            edgeInfo.Add(new EdgeInfo(nodeIndex + 3, nodeIndex + 2, '~'));

            if (a.Length == 1)
            {
                edgeInfo.Add(new EdgeInfo(nodeIndex + 2, nodeIndex + 3, a[0]));
            }
            else
            {
                int aNodeEnd = nodeIds.Pop();
                int aNodeStart = nodeIds.Pop();
                edgeInfo.Add(new EdgeInfo(nodeIndex + 2, aNodeStart, '~'));
                edgeInfo.Add(new EdgeInfo(aNodeEnd, nodeIndex + 3, '~'));
            }

            nodeIds.Push(nodeIndex + 1);
            nodeIds.Push(nodeIndex + 4);

            values.Push(string.Format("+({0})", a));
            return nodeIndex + 4;
        }
    }
}
