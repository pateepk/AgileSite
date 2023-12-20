using System;

namespace CMS.Base
{
    /// <summary>
    /// Exception thrown in case the action was cancelled
    /// </summary>
    public class ActionCancelledException : Exception
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Exception message</param>
        public ActionCancelledException(string message)
            : base(message)
        {
        }


        /// <summary>
        /// Constructor
        /// </summary>
        public ActionCancelledException()
            : this("The action was cancelled.")
        {
        }
    }
}