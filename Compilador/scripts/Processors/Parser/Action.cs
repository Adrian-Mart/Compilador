using System.Runtime.Serialization;
using System.Text;

namespace Compilador.Processors.Parser
{
    /// <summary>
    /// The Action class represents an action in the LALR parse table.
    /// </summary>
    [DataContract, KnownType(typeof(Action))]
    public class Action
    {
        /// <summary>
        /// The type of the action.
        /// </summary>
        [DataMember()]
        private ActionType type;

        /// <summary>
        /// The value of the action.
        /// </summary>
        [DataMember()]
        private int actionValue;

        /// <summary>
        /// Gets the type of the action.
        /// </summary>
        public ActionType Type { get => type; }

        /// <summary>
        /// Gets the value of the action.
        /// </summary>
        public int ActionValue { get => actionValue; }

        /// <summary>
        /// Initializes a new instance of the Action class with
        /// the specified type and value.
        /// </summary>
        /// <param name="type">The type of the action.</param>
        /// <param name="actionValue">The value of the action.</param>
        public Action(ActionType type, int actionValue)
        {
            this.type = type;
            this.actionValue = actionValue;
        }

        public override string ToString()
        {
            if (type is ActionType.Error)
                return "   ";
            else if (type is ActionType.Accept)
                return "ACC";
            StringBuilder sb = new StringBuilder();
            switch (type)
            {
                case ActionType.Shift:
                    sb.Append("S");
                    break;
                case ActionType.Reduce:
                    sb.Append("R");
                    break;
                case ActionType.GoTo:
                    sb.Append("G");
                    break;
                default:
                    break;
            }
            sb.Append(actionValue.ToString("00"));
            return sb.ToString();
        }
    }

    /// <summary>
    /// The ActionType enum represents the type of an action.
    /// </summary>
    public enum ActionType
    {
        /// <summary>
        /// The action is a shift. The value is the
        /// index of the state to shift to.
        /// </summary>
        Shift,
        /// <summary>
        /// The action is a reduce. The value is the
        /// index of the production to reduce.
        /// </summary>
        Reduce,
        /// <summary>
        /// The action is an accept.
        /// </summary>
        Accept,
        /// <summary>
        /// The action is an error.
        /// </summary>
        Error,
        /// <summary>
        /// The action is a goto. The value is the
        /// index of the state to go to.
        /// </summary>
        GoTo
    }
}