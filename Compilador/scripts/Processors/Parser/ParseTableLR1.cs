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
            throw new System.NotImplementedException();
        }

        private static List<State> GetDFA(ParserSetup setup)
        {
            List<State> dfa = GetLRDFA(setup);


            throw new System.NotImplementedException();
        }

        private static List<State> GetLRDFA(ParserSetup setup)
        {
            List<State> dfa = new List<State>();
            var startRules = setup.Productions[setup.Start].Rules;
            if (startRules.Count() == 0)
                throw new System.Exception("No rules for start symbol");
            else if (startRules.Count() > 1)
                throw new System.Exception("More than one rule for start symbol");

            Stack<State> states = new Stack<State>();
            states.Push(new State(0));
            List<Configuration> initialConfigurations = new List<Configuration>();
            Dictionary<Configuration, State> stateMap = new Dictionary<Configuration, State>();

            {
                var startConfig = new Configuration(startRules[0], 0);
                startConfig.AddLookAhead(ParserSetup.EOF);
                initialConfigurations.Add(startConfig);
            }

            while (states.Count > 0)
            {
                var state = states.Pop();
                dfa.Add(state);
                SetStateConfigs(initialConfigurations, setup, state);
                foreach (var end in SetTransitions(state, setup, stateMap))
                    states.Push(end);
            }

            throw new System.NotImplementedException();
        }

        private static void SetStateConfigs(List<Configuration> initialConfigurations, ParserSetup setup, State state)
        {
            Queue<Configuration> configurations = new Queue<Configuration>();
            foreach (var config in initialConfigurations)
                configurations.Enqueue(config);

            while (configurations.Count > 0)
            {
                var config = configurations.Dequeue();
                var next = config.Next();
                // TODO: Restrict grammar to not have empty rules, or 2 consecutive non-terminals
                if (state.Configurations.Contains(config))
                    continue;
                if (next != -1 && !setup.IsTerminal(next))
                    foreach (var rule in setup.GetProductionOf(next).Rules)
                    {
                        var c = new Configuration(rule, 0);
                        c.AddLookAhead(config.LookAhead[0]);
                        if (!state.Configurations.Contains(c))
                        {
                            c.RemoveLookAhead(config.LookAhead[0]);
                            c.AddLookAhead(config.NextNext());
                        }
                        configurations.Enqueue(c);
                    }
                state.AddConfiguration(config);
            }
        }

        private static List<State> SetTransitions(State state, ParserSetup setup, Dictionary<Configuration, State> stateMap)
        {
            throw new System.NotImplementedException();
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
        }

        private class Configuration
        {
            private Rule rule;
            private int position;
            private List<int> lookAhead;
            private Action? action;

            public Rule Rule { get => rule; }
            public int Position { get => position; }
            public List<int> LookAhead { get => lookAhead; }

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

            public override int GetHashCode() => HashCode.Combine(rule, position, lookAhead);

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