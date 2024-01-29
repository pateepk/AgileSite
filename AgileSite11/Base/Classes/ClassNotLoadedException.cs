using System;

namespace CMS.Base
{
    /// <summary>
    /// Exception which is thrown when the custom class is not loaded.
    /// </summary>
    public class ClassNotLoadedException : Exception
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">Message</param>
        public ClassNotLoadedException(string message)
            : base(message)
        {
        }
    }
}
