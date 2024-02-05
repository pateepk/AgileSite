using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CMS.Base;

namespace CMS.Search
{
    /// <summary>
    /// Base for exceptions thrown during smart search operations.  
    /// </summary>
    public class SearchException : Exception
    {
        /// <summary>
        /// Empty contructor
        /// </summary>
        public SearchException()
        {
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Exception message</param>
        public SearchException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="innerException">Exception that caused the current exception</param>
        public SearchException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
