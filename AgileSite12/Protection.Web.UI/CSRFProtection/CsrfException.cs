using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace CMS.Protection.Web.UI
{
    /// <summary>
    /// Exception which is thrown when the CSRF attack is detected.
    /// </summary>
    [Serializable]
    public class CsrfException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CsrfException"/> class.
        /// </summary>
        public CsrfException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CsrfException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">Exception's message</param>
        public CsrfException(string message)
            : base(message)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="CsrfException"/> class with a specified error message and a reference to the inner exception that is the cause of the CSRF exception.
        /// </summary>
        /// <param name="message">Exception's message</param>
        /// <param name="inner">The exception that is the cause of the CSRF exception</param>
        public CsrfException(string message, Exception inner)
            : base(message, inner)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="CsrfException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The SerializationInfo that holds the serialized object data about the CSRF exception.</param>
        /// <param name="context">The StreamingContext that contains contextual information about the source or destination.</param>
        public CsrfException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
