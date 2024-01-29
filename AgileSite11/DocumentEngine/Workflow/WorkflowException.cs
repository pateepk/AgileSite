using System;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Special workflow exception.
    /// </summary>
    public class WorkflowException : Exception
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public WorkflowException()
            : base()
        {
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">Exception message</param>
        public WorkflowException(string message)
            : base(message)
        {
        }
    }
}