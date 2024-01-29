using System;

namespace CMS.Automation
{
    /// <summary>
    /// Exception which is thrown when process recurrence check fails and process is not started.
    /// </summary>
    public class ProcessRecurrenceException : Exception
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">Message</param>
        public ProcessRecurrenceException(string message)
            : base(message)
        {
        }
    }
}
