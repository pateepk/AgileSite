using System;

namespace CMS.Automation
{
    /// <summary>
    /// Exception which is thrown when process cannot be started because of its disabled state.
    /// </summary>
    public class ProcessDisabledException : Exception
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">Message</param>
        public ProcessDisabledException(string message)
            : base(message)
        {
        }
    }
}
