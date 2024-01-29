using System;

namespace CMS.Localization
{
    /// <summary>
    /// Throws when trying to create or update culture info without required unique values (culture code or culture alias).
    /// </summary>
    public class CultureNotUniqueException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CultureNotUniqueException" /> class.
        /// </summary>
        public CultureNotUniqueException()
            : base()
        {
        }


        /// <summary>
        /// Initializes a new instance of the  <see cref="CultureNotUniqueException" /> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public CultureNotUniqueException(string message)
            : base(message)
        {
        }


        /// <summary>
        /// Initializes a new instance of the  <see cref="CultureNotUniqueException" /> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        public CultureNotUniqueException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
