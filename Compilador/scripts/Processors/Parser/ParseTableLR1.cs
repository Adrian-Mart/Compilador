using System.Diagnostics;
using System.Reflection;
using System.Text;
using Compilador.Graph;
using Compilador.Processors.Lexer;

namespace Compilador.Processors.Parser
{
    public static class ParseTableLALR
    {
        public static Action[,] GenerateTable(ParserSetup setup)
        {
            var dfa = GetDFA(setup);
            CollapseToLALR(dfa);
            //TODO: Check if the DFA is correct
            //TODO: Generate the parse table
            //TODO: Adjust reduce actions to use the correct rule
            throw new System.NotImplementedException();
        }

        private static List<State> CollapseToLALR(List<State> dfa)
        {
            var kernels = GetStateKernels(dfa);
            List<State> newDFA = new List<State>();
            Dictionary<int, int> stateRemap = new Dictionary<int, int>();
            Dictionary<int, List<int>[]> LookAheadsRemap = new Dictionary<int, List<int>[]>();
            List<int> skipStates = new List<int>();
            for (int i = 0; i < kernels.Count; i++)
            {
                if (skipStates.Contains(i))
                    continue;
                stateRemap.Add(i, i);
                LookAheadsRemap.Add(i, new List<int>[kernels[i].Configurations.Count]);
                for (int k = 0; k < dfa[i].Configurations.Count; k++)
                    LookAheadsRemap[i][k] = dfa[i].Configurations[k].LookAhead;
                for (int j = i + 1; j < kernels.Count; j++)
                {
                    if (kernels[i].Configurations.SequenceEqual(kernels[j].Configurations))
                    {
                        stateRemap.Add(j, i);
                        for (int k = 0; k < kernels[i].Configurations.Count; k++)
                            LookAheadsRemap[i][k].AddRange(dfa[j].Configurations[k].LookAhead);
                        skipStates.Add(j);
                    }
                }
            }

            foreach (var state in stateRemap.Values.Distinct())
            {

                var newState = new State(newDFA.Count);
                int i = 0;
                foreach (var config in dfa[state].Configurations)
                {
                    config.LookAhead = LookAheadsRemap[state][i].Distinct().ToList();
                    newState.AddConfiguration(config);
                    
                    if (config.Action != null)
                        config.Action = new Action(config.Action.Type, stateRemap[config.Action.ActionValue]);
                    else
                        throw new System.Exception("Configuration without action");
                    i++;
                }
                newDFA.Add(newState);
            }

            return newDFA;
        }

        private static List<State> GetStateKernels(in List<State> dfa)
        {
            List<State> stateKernels = new List<State>();
            for (int i = 0; i < dfa.Count; i++)
            {
                var kernel = new State(dfa[i].StateID);
                foreach (var config in dfa[i].Configurations)
                    kernel.AddConfiguration(new Configuration(config.Rule, config.Position));
                kernel.Configurations.Sort();
                stateKernels.Add(kernel);
            }
            return stateKernels;
        }

        private static List<State> GetDFA(ParserSetup setup)
        {
            List<State> dfa = GetLRDFA(setup);
            CollapseStates(dfa);
            return dfa;
        }

        private static void CollapseStates(List<State> dfa)
        {
            foreach (var state in dfa)
                state.SetConfigurations(KernelsInState(state));
        }

        private static List<Configuration> KernelsInState(State state)
        {
            List<Configuration> kernels = new List<Configuration>();
            foreach (Configuration config in state.Configurations)
            {
                bool found = false;
                foreach (Configuration kernel in kernels)
                {
                    if (config.Rule == kernel.Rule && config.Position == kernel.Position)
                    {
                        kernel.AddLookAhead(config.LookAhead[0]);
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    var newConfig = new Configuration(config.Rule, config.Position);
                    newConfig.AddLookAhead(config.LookAhead[0]);
                    if (config.Action != null)
                        newConfig.SetAction(config.Action);
                    else
                        throw new System.Exception("Configuration without action");
                    kernels.Add(newConfig);
                }
            }
            return kernels;
        }

        private static List<State> GetLRDFA(ParserSetup setup)
        {
            List<State> dfa = new List<State>();
            var startRules = setup.Productions[setup.Start].Rules;
            if (startRules.Count() == 0)
                throw new System.Exception("No rules for start symbol");
            else if (startRules.Count() > 1)
                throw new System.Exception("More than one rule for start symbol");

            Queue<State> states = new Queue<State>();
            states.Enqueue(new State(0));
            List<State> stateMap = new List<State>();

            {
                var startConfig = new Configuration(startRules[0], 0);
                startConfig.AddLookAhead(ParserSetup.EOF);
                states.Peek().AddConfiguration(startConfig);
                stateMap.Add(states.Peek());
            }

            while (states.Count > 0)
            {
                var state = states.Dequeue();
                dfa.Add(state);
                SetStateConfigs(setup, state);
                foreach (var end in SetTransitions(dfa.Count + states.Count, state, setup, stateMap))
                    states.Enqueue(end);
            }

            return dfa;
        }

        private static void SetStateConfigs(ParserSetup setup, State state)
        {
            Queue<Configuration> configurations = new Queue<Configuration>();
            foreach (var config in state.Configurations)
                configurations.Enqueue(config);

            state.Configurations.Clear();
            // TODO: Revisar que para una config 2 -> 5 . 1 6, al agregar los de 1, su lookAhead sea 6

            while (configurations.Count > 0)
            {
                var config = configurations.Dequeue();
                var next = config.Next();
                var nextNext = config.NextNext();
                // TODO: Restrict grammar to not have empty rules, or 2 consecutive non-terminals
                if (state.Configurations.Contains(config))
                    continue;
                if (next != -1 && !setup.IsTerminal(next))
                    foreach (var rule in setup.GetProductionOf(next).Rules)
                    {
                        var c = new Configuration(rule, 0);
                        if (nextNext == -1)
                            c.AddLookAhead(config.LookAhead[0]);
                        else
                            c.AddLookAhead(nextNext);
                        configurations.Enqueue(c);
                    }
                state.AddConfiguration(config);
            }
        }

        private static List<State> SetTransitions(int stateCount, State state, ParserSetup setup, List<State> stateMap)
        {
            List<int> stateIndexes = new List<int>();
            List<List<Configuration>> ends = new List<List<Configuration>>();
            List<State> returnStates = new List<State>();
            foreach (var config in state.Configurations)
            {
                if (config.Next() == -1)
                {
                    config.SetAction(new Action(ActionType.Reduce, config.Rule.Production.NonTerminalId));
                    continue;
                }

                if (config.Action == null)
                {
                    var next = new Configuration(config.Rule, config.Position + 1);
                    next.AddLookAhead(config.LookAhead[0]);
                    var stateIndex = stateIndexes.IndexOf(config.Next());
                    if (stateIndex != -1)
                        ends[stateIndex].Add(next);
                    else
                    {
                        stateIndexes.Add(config.Next());
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
                    containsAll = matches > 0;
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

        private class State
        {
            private int stateID;
            private List<Configuration> configurations;

            public int StateID { get => stateID; }
            public List<Configuration> Configurations { get => configurations; }

            public State(int stateID)
            {
                this.stateID = stateID;
                configurations = new List<Configuration>();
            }

            public bool AddConfiguration(Configuration configuration)
            {
                if (configurations.Contains(configuration))
                    return false;
                configurations.Add(configuration);
                return true;
            }

            public void SetConfigurations(List<Configuration> configurations)
            {
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
                StringBuilder sb = new StringBuilder();
                sb.Append(stateID).Append(": {");
                foreach (var config in configurations)
                    sb.Append(config).Append(", ");
                sb.Remove(sb.Length - 2, 2);
                sb.Append("}");
                return sb.ToString();
            }
        }

        private class Configuration : IComparable
        {
            private Rule rule;
            private int position;
            private List<int> lookAhead;
            private Action? action;

            public Rule Rule { get => rule; }
            public int Position { get => position; }
            public List<int> LookAhead { get => lookAhead; set => lookAhead = value;}
            public Action? Action { get => action; set => action = value; }

            public Configuration(Rule rule, int position)
            {
                this.rule = rule;
                this.position = position;
                this.lookAhead = new List<int>();
            }

            public void AddLookAhead(int lookAhead)
            {
                if (!this.lookAhead.Contains(lookAhead))
                    this.lookAhead.Add(lookAhead);
            }

            public void RemoveLookAhead(int lookAhead)
            {
                this.lookAhead.Remove(lookAhead);
            }

            public void SetAction(Action action)
            {
                this.action = action;
            }

            public int Previous()
            {
                if (position > 0)
                    return rule[position - 1];
                else
                    return -1;
            }

            public int Next()
            {
                if (position < rule.Count())
                    return rule[position];
                else
                    return -1;
            }

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
    }
    public class Action
    {
        private ActionType type;
        private int actionValue;

        public ActionType Type { get => type; }
        public int ActionValue { get => actionValue; }

        public Action(ActionType type, int actionValue)
        {
            this.type = type;
            this.actionValue = actionValue;
        }
    }

    public enum ActionType
    {
        Shift,
        Reduce,
        Accept,
        Error,
        GoTo
    }
}