using System;

namespace CMS.SalesForce
{

    /// <summary>
    /// Represents a general SalesForce exception.
    /// </summary>
    public class SalesForceException : Exception
    {

        #region "Constructors"

        /// <summary>
        /// Initializes a new instance of the SalesForce exception.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public SalesForceException(string message) : base(message)
        {

        }

        /// <summary>
        /// Initializes a new instance of the SalesForce exception.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="innerException">The inner exception.</param>
        public SalesForceException(string message, Exception innerException) : base(message, innerException)
        {

        }

        #endregion

    }

}