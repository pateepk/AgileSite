using System;

namespace CMS.StrandsRecommender
{
    /// <summary>
    /// Represents a general Strands exception.
    /// </summary>
    public class StrandsException : Exception
    {
        private string mUIMessage;


        /// <summary>
        /// Message displayed in the UI to the user
        /// </summary>
        public string UIMessage
        {
            get
            {
                return mUIMessage ?? Message;
            }
            set
            {
                mUIMessage = value;
            }
        }


        /// <summary>
        /// Initializes a new instance of the Strands exception.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public StrandsException(string message)
            : this(message, uiMessage: null)
        {
        }


        /// <summary>
        /// Initializes a new instance of the Strands exception.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="uiMessage">The exception localized message.</param>
        public StrandsException(string message, string uiMessage)
            : base(message)
        {
            UIMessage = uiMessage;
        }


        /// <summary>
        /// Initializes a new instance of the Strands exception.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="innerException">The inner exception.</param>
        public StrandsException(string message, Exception innerException)
            : this(message, null, innerException)
        {
        }


        /// <summary>
        /// Initializes a new instance of the Strands exception.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="innerException">The inner exception.</param>
        /// <param name="uiMessage">The exception localized message.</param>
        public StrandsException(string message, string uiMessage, Exception innerException)
            : base(message, innerException)
        {
            UIMessage = uiMessage;
        }
    }
}
