using System;

namespace CMS.ContinuousIntegration
{
    /// <summary>
    /// Represents one record in log.
    /// </summary>
    /// <remarks>
    /// Instance of this class is immutable.
    /// </remarks>
    [Serializable]
    public class LogItem
    {
        #region "Properties"

        /// <summary>
        /// Type of the action which logged the message
        /// </summary>
        public LogItemActionTypeEnum ActionType
        {
            get;
            private set;
        }


        /// <summary>
        /// Type of message.
        /// </summary>
        public LogItemTypeEnum Type
        {
            get;
            private set;
        }


        /// <summary>
        /// Message text.
        /// </summary>
        public string Message
        {
            get;
            private set;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">Message text.</param>
        /// <param name="messageType">Type of message.</param>
        /// <param name="actionType">Type of the action which logged the message.</param>
        public LogItem(string message, LogItemTypeEnum messageType, LogItemActionTypeEnum actionType)
        {
            Message = message;
            Type = messageType;
            ActionType = actionType;
        }


        /// <summary>
        /// Return type and content of message in single string.
        /// </summary>
        public override string ToString()
        {
            return "[" + Type + "-" + ActionType + "] " + Message;
        }

        #endregion
    }
}
