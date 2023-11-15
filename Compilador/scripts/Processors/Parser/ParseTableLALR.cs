using System.Text;

namespace Compilador.Processors.Parser
{
    /// <summary>
    /// Generates the LALR parse table for the specified ParserSetup.
    /// </summary>
    public static class ParseTableLALR
    {
        /// <summary>
        /// Generates the LALR parse table for the specified ParserSetup.
        /// </summary>
        /// <param name="setup">The ParserSetup to use for parsing.</param>
        /// <param name="endOfFileIndex">The index of the end of file symbol.</param>
        /// <returns>The LALR parse table.</returns>
        public static Action[][] GenerateTable(ParserSetup setup, out int endOfFileIndex)
        {
            // The index of the end of file symbol
            endOfFileIndex = setup.TerminalsCount + setup.NonTerminalsCount;

            // Calculate the first sets
            var firstSets = CalculateFirst(setup);
            // Get the LR(0) DFA
            var dfa = GetDFA(setup, firstSets);
            // Convert the LR(0) DFA to an LALR DFA
            dfa = CollapseToLALR(dfa);
            // Convert the DFA to a table. Note that
            // this table does not consider errors or
            // the accept action
            var table = TableFromDFA(dfa, setup);
            // Generate the error action
            var error = new Action(ActionType.Error, 0);

            // For each state in the DFA
            for (int i = 0; i < table.GetLength(0); i++)
            {
                // For each symbol in the table
                for (int j = 0; j < table.GetLength(1); j++)
                {
                    // If the action is null
                    if (table[i, j] == null)
                        // Set the action to error
                        table[i, j] = error;
                    // If the action is a shift and the symbol is the start non-terminal in the grammar
                    else if (table[i, j].ActionValue == 0 && table[i, j].Type == ActionType.Reduce)
                        // Set the action to accept
                        table[i, j] = new Action(ActionType.Accept, 0);
                }
            }

            // Return the table
            return Convert2DArrayToJaggedArray(table);
        }

        #region Utilities
        /// <summary>
        /// Calculates the first sets of the grammar.
        /// </summary>
        private static Dictionary<int, HashSet<int>> CalculateFirst(ParserSetup setup)
        {
            // Create the first sets
            var firstSets = new Dictionary<int, HashSet<int>>();

            // For each terminal in the grammar
            foreach (var terminal in setup.GetTerminalIndexes())
            {
                // The first set of the terminal is the terminal itself
                firstSets.Add(terminal, new HashSet<int>() { terminal });
            }

            // For each non-terminal in the grammar
            List<int> nonTerminals = new List<int>();
            foreach (var production in setup.Productions.Values)
            {
                // Create the first set
                var first = new HashSet<int>();
                bool allTerminals = false;

                // For each rule in the production
                for (int i = 0; i < production.Rules.Length; i++)
                {
                    // If the rule is not a terminal, break
                    if (!setup.IsTerminal(production.Rules[i][0]))
                        break;

                    // Add the symbol to the first set
                    first.Add(production.Rules[i][0]);
                    // If the rule is the last, set allTerminals to true
                    if (i == production.Rules.Length - 1)
                        // Set allTerminals to true
                        allTerminals = true;
                }

                // If all the rules start with a terminal
                if (allTerminals)
                    // Set the first set of that non-terminal to the first set
                    firstSets.Add(production.NonTerminalId, first);
                else
                    // Add the non-terminal to the list
                    nonTerminals.Add(production.NonTerminalId);
            }


            // Set a limit to the number of non-terminals in the stack
            const int LIMIT = 100;
            // Create the stack of non-terminals
            Stack<int> nonTerminalsStack = new Stack<int>(nonTerminals);
            // While there are non-terminals in the stack
            while (nonTerminalsStack.Count > 0)
            {
                // Create the first set
                HashSet<int> first;
                // If the count of non-terminals in the stack is greater than the limit
                int count = nonTerminalsStack.Count;
                if (LIMIT < count)
                    // Ask the user to reduce left recursion in the grammar
                    throw new Exception("Reduce left recursion in gramar");

                // If the first set of the non-terminal is already calculated
                first = new HashSet<int>();
                var current = nonTerminalsStack.Peek();
                if (firstSets.ContainsKey(current))
                {
                    // Remove the non-terminal from the stack
                    nonTerminalsStack.Pop();
                }

                // For each rule in the production
                foreach (var rule in setup.Productions[current].Rules)
                {
                    // If the first symbol of the rule first set is already calculated
                    if (firstSets.TryGetValue(rule[0], out var firstSet))
                    {
                        // Add the first set to the first set of the non-terminal
                        foreach (var item in firstSet)
                            first.Add(item);
                    }
                    // Else if the first symbol of the rule is a terminal
                    else
                    {
                        // Add the first symbol to the first set of the non-terminal
                        if (!nonTerminalsStack.Contains(rule[0]))
                            // Add the symbol to stack
                            nonTerminalsStack.Push(rule[0]);
                    }
                }

                // If the count of non-terminals in the stack is not the same as before, continue
                if (count != nonTerminalsStack.Count)
                    continue;
                // If the number of elements in the first set did not change
                // add the first set to the first sets
                firstSets.Add(current, first);
                // Remove the non-terminal from the stack
                nonTerminalsStack.Pop();
            }

            // Return the first sets
            return firstSets;
        }

        /// <summary>
        /// Converts the info on the table to a string.
        /// </summary>
        /// <param name="table">The table to convert.</param>
        /// <param name="endOfFileIndex">The index of the end of file symbol.</param>
        /// <returns>A string representation of the table.</returns>
        public static string TableToString(Action[][] table, int endOfFileIndex)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Parse table\n");
            sb.Append("In|");
            for (int i = 0; i <= endOfFileIndex; i++)
                sb.Append(i.ToString("000")).Append("|");
            sb.Append("\n--|");
            for (int i = 0; i <= endOfFileIndex; i++)
                sb.Append("---").Append("|");
            sb.Append("\n");
            for (int i = 0; i < table.Length; i++)
            {
                sb.Append(i.ToString("00")).Append("|");
                for (int j = 0; j < table[0].Length; j++)
                {
                    sb.Append(table[i][j]).Append("|");
                }
                sb.Append("\n");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Converts a 2D array to a jagged array.
        /// </summary>
        /// <typeparam name="T">The type of the array.</typeparam>
        /// <param name="twoDArray">The 2D array to convert.</param>
        /// <returns>The jagged array.</returns>
        /// <remarks>
        /// This method is used for serialization porpuses.
        /// </remarks>
        private static T[][] Convert2DArrayToJaggedArray<T>(T[,] twoDArray)
        {
            // Get the dimensions of the array
            int rowCount = twoDArray.GetLength(0);
            int colCount = twoDArray.GetLength(1);
            // Create the jagged array
            T[][] jaggedArray = new T[rowCount][];

            // For each row in the array
            for (int i = 0; i < rowCount; i++)
            {
                // Create the row in the jagged array
                jaggedArray[i] = new T[colCount];
                // For each column in the array
                for (int j = 0; j < colCount; j++)
                {
                    // Copy the value from the 2D array
                    // to the jagged array
                    jaggedArray[i][j] = twoDArray[i, j];
                }
            }

            // Return the jagged array
            return jaggedArray;
        }
        #endregion

        #region GenerateLALRTable

        /// <summary>
        /// Converts the DFA to a table.
        /// </summary>
        /// <param name="dfa">The DFA to convert.</param>
        /// <param name="setup">The ParserSetup to use for parsing.</param>
        /// <returns>The DFA in table format</returns>
        private static Action[,] TableFromDFA(List<State> dfa, ParserSetup setup)
        {
            // Get the number of symbols
            int symbols = setup.GetTerminalIndexes().Length + setup.GetNonTerminalIndexes().Length;
            // Create the table
            Action[,] actions = new Action[dfa.Count, symbols + 1];
            // For each state in the DFA
            int stateIndex = 0;
            foreach (var state in dfa)
            {
                // For each configuration in the state
                foreach (var config in state.Configurations)
                {
                    // If the configuration is not set
                    if (config.Action is null)
                        // Throw an exception
                        throw new System.Exception("Configuration without action");

                    // Get the next symbol
                    var next = config.Next();
                    // If the configuration is in its last position
                    if (next == -1)
                    {
                        // For each look ahead in the configuration
                        foreach (var item in GetLookAheadsInConfig(config))
                        {
                            // If the look ahead is the end of file symbol
                            if (item == ParserSetup.EOF)
                                // Set the action to the corresponding action
                                // in the table at the end of file symbol column
                                actions[stateIndex, symbols] = config.Action;
                            else
                                // Set the action to the corresponding action
                                actions[stateIndex, item] = config.Action;
                        }
                    }
                    // If the next symbol is a non-terminal
                    else if (!setup.IsTerminal(next))
                        // Set the action to the an goto action
                        actions[stateIndex, next] = new Action(ActionType.GoTo, config.Action.ActionValue);
                    else
                        // Set the action to the corresponding action
                        actions[stateIndex, next] = config.Action;
                }
                // Increment the state index
                stateIndex++;
            }

            // Return the table
            return actions;
        }

        /// <summary>
        /// Gets the look aheads in the configuration.
        /// </summary>
        /// <param name="config">The configuration to get the look aheads from.</param>
        /// <returns>The look aheads in the configuration.</returns>
        private static List<int> GetLookAheadsInConfig(Configuration config)
        {
            List<int> lookAheads = new List<int>();
            foreach (var lookAhead in config.LookAhead)
                if (!lookAheads.Contains(lookAhead))
                    lookAheads.Add(lookAhead);
            return lookAheads;
        }

        /// <summary>
        /// Converts the LR(0) DFA to an LALR DFA by collapsing states.
        /// </summary>
        /// <param name="dfa">The LR(0) DFA to convert.</param>
        /// <returns>The LALR DFA.</returns>
        private static List<State> CollapseToLALR(List<State> dfa)
        {
            // Get the kernels of the states
            var kernels = GetStateKernels(dfa);

            // Crate the auxiliar structures
            List<State> newDFA = new List<State>();
            Dictionary<int, int> stateRemap = new Dictionary<int, int>();
            Dictionary<int, List<int>[]> LookAheadsRemap = new Dictionary<int, List<int>[]>();
            List<int> skipStates = new List<int>();

            // For each state in the DFA
            for (int i = 0; i < kernels.Count; i++)
            {
                // If the state is already in the skip list
                if (skipStates.Contains(i))
                    continue;

                // Add the state to the remap
                stateRemap.Add(i, i);
                // Add the look aheads to the remap
                LookAheadsRemap.Add(i, new List<int>[kernels[i].Configurations.Count]);

                // For each configuration in the state
                for (int k = 0; k < dfa[i].Configurations.Count; k++)
                    // Add the look aheads to the remap
                    LookAheadsRemap[i][k] = dfa[i].Configurations[k].LookAhead;

                // For each state after the current state
                for (int j = i + 1; j < kernels.Count; j++)
                {
                    // If the kernel of the current state is equal to the kernel of the next state
                    if (kernels[i].Configurations.SequenceEqual(kernels[j].Configurations))
                    {
                        // Add the state to the remap
                        stateRemap.Add(j, i);
                        // Add the look aheads to the remap
                        for (int k = 0; k < kernels[i].Configurations.Count; k++)
                            LookAheadsRemap[i][k].AddRange(dfa[j].Configurations[k].LookAhead);
                        // Add the state to the skip list
                        skipStates.Add(j);
                    }
                }
            }

            // Remap the states indexes to a new DFA
            Dictionary<int, int> stateRemap2 = new Dictionary<int, int>();
            for (int i = 0; i < stateRemap.Values.Distinct().Count(); i++)
                stateRemap2.Add(stateRemap.Values.Distinct().ElementAt(i), i);

            // Create the new DFA
            // For each state in the remap
            foreach (var state in stateRemap.Values.Distinct())
            {
                // Create the new state
                var newState = new State(newDFA.Count);
                int i = 0;

                // For each configuration in the state
                foreach (var config in dfa[state].Configurations)
                {
                    // Set the look aheads
                    config.LookAhead = LookAheadsRemap[state][i].Distinct().ToList();
                    newState.AddConfiguration(config);

                    // Set the action
                    if (config.Action != null)
                        config.Action = new Action(config.Action.Type, stateRemap2[stateRemap[config.Action.ActionValue]]);
                    else
                        throw new Exception("Configuration without action");
                    i++;
                }
                // Add the state to the new DFA
                newDFA.Add(newState);
            }

            // Return the new DFA
            return newDFA;
        }

        /// <summary>
        /// Gets the kernels of the states.
        /// </summary>
        /// <param name="dfa">The DFA to get the kernels from.</param>
        /// <returns>The kernels of the states.</returns>
        private static List<State> GetStateKernels(in List<State> dfa)
        {
            // Create the list of kernels
            List<State> stateKernels = new List<State>();
            // For each state in the DFA
            for (int i = 0; i < dfa.Count; i++)
            {
                // Create the kernel
                var kernel = new State(dfa[i].StateID);
                // Add the configurations to the kernel
                foreach (var config in dfa[i].Configurations)
                    kernel.AddConfiguration(new Configuration(config.Rule, config.Position));
                // Sort the configurations
                kernel.Configurations.Sort();
                // Add the kernel to the list
                stateKernels.Add(kernel);
            }

            // Return the list of kernels
            return stateKernels;
        }

        #endregion

        #region GenerateLR0DFA

        /// <summary>
        /// Gets the LR(0) DFA.
        /// </summary>
        /// <param name="setup">The ParserSetup to use for parsing.</param>
        /// <param name="firstSets">The first sets of the grammar.</param>
        /// <returns>The LR(0) DFA.</returns>
        private static List<State> GetDFA(ParserSetup setup, Dictionary<int, HashSet<int>> firstSets)
        {
            // Get the dfa
            List<State> dfa = GetLRDFA(setup, firstSets);
            // Collapse the states
            CollapseStates(dfa);
            // Return the DFA
            return dfa;
        }

        /// <summary>
        /// Collapses the configurations of the states into a unique configuration
        /// with the look aheads of the configurations.
        /// </summary>
        /// <param name="dfa">The DFA to collapse.</param>
        private static void CollapseStates(List<State> dfa)
        {
            // For each state in the DFA
            foreach (var state in dfa)
                // Set the configurations of the state
                state.SetConfigurations(KernelsInState(state));
        }

        /// <summary>
        /// Gets the configuration kernels of the state.
        /// </summary>
        /// <param name="state">The state to get the kernels from.</param>
        private static List<Configuration> KernelsInState(State state)
        {
            // Create the list of kernels
            List<Configuration> kernels = new List<Configuration>();
            // For each configuration in the state
            foreach (Configuration config in state.Configurations)
            {
                // If the configuration is not a kernel
                bool found = false;
                // For each kernel in the list
                foreach (Configuration kernel in kernels)
                {
                    // Compare the configurations
                    if (config.Rule == kernel.Rule && config.Position == kernel.Position)
                    {
                        // Add the look aheads to the kernel
                        kernel.AddLookAhead(config.LookAhead[0]);
                        // Set found to true
                        found = true;
                        // Break the loop
                        break;
                    }
                }

                // If the configuration is not found in the list
                if (!found)
                {
                    // Create a new kernel
                    var newConfig = new Configuration(config.Rule, config.Position);
                    // Add the look aheads to the kernel
                    newConfig.AddLookAhead(config.LookAhead[0]);
                    // If the configuration has an action
                    if (config.Action != null)
                        // Set the action to the kernel
                        newConfig.SetAction(config.Action);
                    // If not throw an exception
                    else
                        throw new Exception("Configuration without action");

                    // Add the kernel to the list
                    kernels.Add(newConfig);
                }
            }

            // Return the list of kernels
            return kernels;
        }

        /// <summary>
        /// Gets the LR(0) DFA.
        /// </summary>
        /// <param name="setup">The ParserSetup to use for parsing.</param>
        /// <param name="firstSets">The first sets of the grammar.</param>
        /// <returns>The LR(0) DFA.</returns>
        private static List<State> GetLRDFA(ParserSetup setup, Dictionary<int, HashSet<int>> firstSets)
        {
            // Create the list of states
            List<State> dfa = new List<State>();

            // Get the start rules
            var startRules = setup.Productions[setup.Start].Rules;
            // If there are no start rules or more than one, throw an exception
            if (startRules.Count() == 0)
                throw new System.Exception("No rules for start symbol");
            else if (startRules.Count() > 1)
                throw new System.Exception("More than one rule for start symbol");

            // Create the queue of states
            Queue<State> states = new Queue<State>();
            // Add the first state to the queue
            states.Enqueue(new State(0));
            // Create the state map
            List<State> stateMap = new List<State>();

            // Add the start configuration to the first state
            {
                // Create the start configuration
                var startConfig = new Configuration(startRules[0], 0);
                // Add the end of file symbol to the look aheads
                startConfig.AddLookAhead(ParserSetup.EOF);
                // Add the configuration to the state
                states.Peek().AddConfiguration(startConfig);
                // Add the state to the state map
                stateMap.Add(states.Peek());
            }

            // While there are states in the queue
            while (states.Count > 0)
            {
                // Get the next state
                var state = states.Dequeue();
                // Add the state to the DFA
                dfa.Add(state);
                // Set the state configurations
                SetStateConfigs(setup, state, firstSets);
                // Set the state transitions
                foreach (var end in SetTransitions(dfa.Count + states.Count, state, setup, stateMap))
                    // Add the new states to the queue
                    states.Enqueue(end);
            }

            // Return the DFA
            return dfa;
        }

        /// <summary>
        /// Sets the configurations of the state. This method is the implementation of
        /// the generation of the closure of an state.
        /// </summary>
        /// <param name="setup">The ParserSetup to use for parsing.</param>
        /// <param name="state">The state to set the configurations.</param>
        /// <param name="firstSets">The first sets of the grammar.</param>
        private static void SetStateConfigs(ParserSetup setup, State state, Dictionary<int, HashSet<int>> firstSets)
        {
            // Create the queue of configurations
            Queue<Configuration> configurations = new Queue<Configuration>();
            // Add the configurations to the queue
            foreach (var config in state.Configurations)
                configurations.Enqueue(config);

            // Clear the configurations of the state
            state.Configurations.Clear();

            // While there are configurations in the queue
            while (configurations.Count > 0)
            {
                // Get the next configuration
                var config = configurations.Dequeue();

                // Get the next and next next symbols
                var next = config.Next();
                var nextNext = config.NextNext();

                // If the configuration is already in the state, continue
                if (state.Configurations.Contains(config))
                    continue;

                // If there is a next symbol and is a non-terminal
                if (next != -1 && !setup.IsTerminal(next))
                {
                    // For each rule of the non-terminal
                    foreach (var rule in setup.GetProductionOf(next).Rules)
                    {
                        // If there is no next next symbol
                        if (nextNext == -1)
                        {
                            // Create a new configuration
                            var c = new Configuration(rule, 0);
                            // Add the configuration look aheads to the
                            // new configuration
                            c.AddLookAhead(config.LookAhead[0]);
                            // Add the new configuration to the queue
                            configurations.Enqueue(c);
                        }
                        else
                        {
                            // Create a new configuration
                            Configuration c;
                            // If the next next symbol is a terminal
                            if (setup.IsTerminal(nextNext))
                            {
                                // Create a new configuration
                                c = new Configuration(rule, 0);
                                // Add the next next symbol to the look aheads
                                c.AddLookAhead(nextNext);
                                // Add the configuration to the queue
                                configurations.Enqueue(c);
                            }
                            // For each symbol in the first set of the next next symbol
                            foreach (var item in firstSets[nextNext])
                            {
                                // Create a new configuration
                                c = new Configuration(rule, 0);
                                // Add the symbol to the look aheads
                                c.AddLookAhead(item);
                                // Add the configuration to the queue
                                configurations.Enqueue(c);
                            }
                        }
                    }
                }
                // Add the configuration to the state
                state.AddConfiguration(config);
            }
        }

        /// <summary>
        /// Sets the transitions of the state. This method is the implementation of
        /// the generation of the goto of an state.
        /// </summary>
        /// <param name="stateCount">The number of states.</param>
        /// <param name="state">The state to set the transitions.</param>
        /// <param name="setup">The ParserSetup to use for parsing.</param>
        /// <param name="stateMap">The state map.</param>
        /// <returns>The list of new states.</returns>
        private static List<State> SetTransitions(int stateCount, State state, ParserSetup setup, List<State> stateMap)
        {
            // Create the auxiliar structures
            List<int> stateIndexes = new List<int>();
            List<List<Configuration>> ends = new List<List<Configuration>>();
            List<State> returnStates = new List<State>();

            // For each configuration in the state
            foreach (var config in state.Configurations)
            {
                // If there is no next symbol
                if (config.Next() == -1)
                {
                    // Set the action to reduce
                    config.SetAction(new Action(ActionType.Reduce, setup.Rules[config.Rule]));
                    continue;
                }

                // If the configuration has no action
                if (config.Action == null)
                {
                    // Create a new configuration
                    var next = new Configuration(config.Rule, config.Position + 1);
                    // Add the look aheads to the new configuration
                    next.AddLookAhead(config.LookAhead[0]);
                    // Get the index of the next symbol in the state
                    var stateIndex = stateIndexes.IndexOf(config.Next());
                    // If the next symbol is already in the state
                    if (stateIndex != -1)
                    {
                        // Add the configuration to the list of configurations
                        ends[stateIndex].Add(next);
                    }
                    else
                    {
                        // Add the index of the next symbol to the state
                        stateIndexes.Add(config.Next());
                        // Generate a new list of configurations for the next symbol
                        ends.Add(new List<Configuration>() { next });
                    }
                }
            }

            // In this section, the next configurations are
            // grouped by the transition symbol
            Dictionary<int, State> endsMap = new Dictionary<int, State>();
            foreach (var configs in ends)
            {
                // Check if the group of configurations is already in a state
                int stateTarget = -1;
                bool containsAll = true;
                // For each configuration in the group
                foreach (var config in configs)
                {
                    int matches = 0;
                    // For each state in the state map (states already created)
                    for (int i = 0; i < stateMap.Count; i++)
                    {
                        // If the state contains an equal configuration
                        if (stateMap[i].Configurations.Contains(config))
                        {
                            // And is the first state that contains it
                            if (stateTarget == -1)
                            {
                                stateTarget = i;
                                matches++;
                                break;
                            }
                            // And is not the first state that contains it
                            else
                            {
                                // If the state is not the same as 
                                // the first state that contains it
                                if (stateTarget == i)
                                {
                                    matches++;
                                    break;
                                }
                            }
                        }
                    }

                    // Check if the group of configurations is in a state
                    containsAll = matches > 0;
                    // If the group of configurations is not in a state
                    if (!containsAll)
                        break;
                }

                // If the group of configurations is not in a state
                if (!containsAll || stateTarget == -1)
                {
                    // Create a new state
                    var nextState = new State(stateCount++);
                    // Add the configurations to the state
                    foreach (var c in configs)
                        nextState.AddConfiguration(c);
                    // Add the state to the state map
                    stateMap.Add(nextState);
                    // Add the state to the return list
                    returnStates.Add(nextState);
                    // Add the state to the ends map
                    endsMap.TryAdd(configs[0].Previous(), nextState);
                }
                // If the group of configurations is in a state
                else
                    // Add the state to the ends map
                    endsMap.TryAdd(configs[0].Previous(), stateMap[stateTarget]);
            }

            // For each configuration in the state
            foreach (var config in state.Configurations)
            {
                // If the configuration is not reduced
                if (endsMap.TryGetValue(config.Next(), out var nextState))
                    // Set the action to shift to the next state
                    config.SetAction(new Action(ActionType.Shift, endsMap[config.Next()].StateID));
            }

            // Return the list of new states
            return returnStates;
        }

        #endregion

        #region StateClass
        /// <summary>
        /// Represents a state in the DFA.
        /// </summary>
        private class State
        {
            /// <summary>
            /// The ID of the state.
            /// </summary>
            private int stateID;

            /// <summary>
            /// The configurations of the state.
            /// </summary>
            private List<Configuration> configurations;

            /// <summary>
            /// Gets the ID of the state.
            /// </summary>
            public int StateID { get => stateID; }

            /// <summary>
            /// Gets the configurations of the state.
            /// </summary>
            public List<Configuration> Configurations { get => configurations; }

            /// <summary>
            /// Instantiates a new instance of the State class.
            /// </summary>
            /// <param name="stateID"></param>
            public State(int stateID)
            {
                // Set the state ID
                this.stateID = stateID;
                // Create the configurations list
                configurations = new List<Configuration>();
            }

            /// <summary>
            /// Adds a configuration to the state.
            /// </summary>
            /// <param name="configuration">The configuration to add.</param>
            /// <returns>True if the configuration was added, false otherwise.</returns>
            public bool AddConfiguration(Configuration configuration)
            {
                if (configurations.Contains(configuration))
                    return false;
                configurations.Add(configuration);
                return true;
            }

            /// <summary>
            /// Sets the configurations of the state.
            /// </summary>
            /// <param name="configurations">The configurations to set</param>
            public void SetConfigurations(List<Configuration> configurations)
            {
                // Set the configurations
                this.configurations = configurations;
            }

            public override bool Equals(object? obj)
            {
                if (obj == null || GetType() != obj.GetType())
                    return false;
                State other = (State)obj;
                if (!configurations.SequenceEqual(other.configurations))
                    return false;

                return true;
            }

            public override int GetHashCode() => HashCode.Combine(stateID, configurations);

            public override string? ToString()
            {
                // Create the string builder
                StringBuilder sb = new StringBuilder();
                // Append the state ID
                sb.Append(stateID).Append(": {");
                // For each configuration in the state
                foreach (var config in configurations)
                    // Append the configuration
                    sb.Append(config).Append(", ");
                // Remove the last comma
                sb.Remove(sb.Length - 2, 2);
                sb.Append("}");
                // Return the string
                return sb.ToString();
            }
        }

        #endregion

        #region ConfigurationClass
        /// <summary>
        /// Represents a configuration in the DFA.
        /// </summary>
        /// <remarks>
        /// A configuration is a rule with a position in the rule.
        /// </remarks>
        private class Configuration : IComparable
        {
            /// <summary>
            /// The rule of the configuration.
            /// </summary>
            private Rule rule;

            /// <summary>
            /// The position of the configuration in the rule.
            /// </summary>
            private int position;

            /// <summary>
            /// The look aheads of the configuration.
            /// </summary>
            private List<int> lookAhead;

            /// <summary>
            /// The action of the configuration.
            /// </summary>
            private Action? action;

            /// <summary>
            /// Gets the rule of the configuration.
            /// </summary>
            public Rule Rule { get => rule; }

            /// <summary>
            /// Gets the position of the configuration in the rule.
            /// </summary>
            public int Position { get => position; }

            /// <summary>
            /// The list of look aheads of the configuration.
            /// </summary>
            public List<int> LookAhead { get => lookAhead; set => lookAhead = value; }

            /// <summary>
            /// The action of the configuration.
            /// </summary>
            public Action? Action { get => action; set => action = value; }

            /// <summary>
            /// Instantiates a new instance of the Configuration class.
            /// </summary>
            /// <param name="rule">The rule of the configuration.</param>
            /// <param name="position">The position of the configuration in the rule.</param>
            public Configuration(Rule rule, int position)
            {
                this.rule = rule;
                this.position = position;
                this.lookAhead = new List<int>();
            }

            /// <summary>
            /// Adds a look ahead to the configuration.
            /// </summary>
            /// <param name="lookAhead">The look ahead to add.</param>
            public void AddLookAhead(int lookAhead)
            {
                // If the look ahead is not in the list
                if (!this.lookAhead.Contains(lookAhead))
                    // Add the look ahead to the list
                    this.lookAhead.Add(lookAhead);
            }

            /// <summary>
            /// Sets the action of the configuration.
            /// </summary>
            /// <param name="action">The action to set.</param>
            public void SetAction(Action action)
            {
                this.action = action;
            }

            /// <summary>
            /// Gets the previous symbol in the rule. Returns -1 if
            /// the position is 0.
            /// </summary>
            /// <returns>The previous symbol in the rule.</returns>
            public int Previous()
            {
                if (position > 0)
                    return rule[position - 1];
                else
                    return -1;
            }

            /// <summary>
            /// Gets the next symbol in the rule. Returns -1 if
            /// the position is the last.
            /// </summary>
            /// <returns>The next symbol in the rule.</returns>
            public int Next()
            {
                if (position < rule.Count())
                    return rule[position];
                else
                    return -1;
            }

            /// <summary>
            /// Gets the next next symbol in the rule. Returns -1 if
            /// the position is the last or the next symbol is the last.
            public int NextNext()
            {
                if (position + 1 < rule.Count())
                    return rule[position + 1];
                else
                    return -1;
            }

            public override bool Equals(object? obj)
            {
                if (obj == null || GetType() != obj.GetType())
                    return false;

                Configuration other = (Configuration)obj;
                if (rule != other.rule
                    || position != other.position
                    || !lookAhead.SequenceEqual(other.lookAhead))
                    return false;

                return true;
            }

            public override int GetHashCode()
            {
                int lookAheadHash = 0;
                foreach (var symbol in lookAhead)
                    lookAheadHash = HashCode.Combine(lookAheadHash, symbol);
                return HashCode.Combine(rule, position, lookAheadHash);
            }

            public override string? ToString()
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(rule).Append(" : ").Append(position).Append(" [");
                foreach (var symbol in lookAhead)
                    sb.Append(symbol).Append(", ");
                sb.Remove(sb.Length - 2, 2);
                sb.Append("]");
                return sb.ToString();
            }

            public int CompareTo(object? obj)
            {
                if (obj == null || GetType() != obj.GetType())
                    throw new ArgumentException("Object is not a Configuration");

                Configuration other = (Configuration)obj;
                var compare = ToString()?.CompareTo(other.ToString());
                compare ??= 0;
                return (int)compare;
            }
        }

        #endregion
    }
}