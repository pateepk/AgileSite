using System;

namespace CMS.SiteProvider
{
    /// <summary>
    /// Running site exception.
    /// </summary>
    public class RunningSiteException : Exception
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public RunningSiteException()
        {
        }


        /// <summary>
        /// Constructor with message.
        /// </summary>
        public RunningSiteException(string message)
            : base(message)
        {
        }
    }
}
