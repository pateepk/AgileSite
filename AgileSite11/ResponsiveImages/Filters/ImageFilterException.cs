using System;

namespace CMS.ResponsiveImages
{
    /// <summary>
    /// Represents errors that occur during filter application.
    /// </summary>
    [Serializable]
    public class ImageFilterException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the ImageFilterException class.
        /// </summary>
        public ImageFilterException()
        {
        }


        /// <summary>
        /// Initializes a new instance of the ImageFilterException class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public ImageFilterException(string message)
            : base(message)
        {
        }


        /// <summary>
        /// Initializes a new instance of the ImageFilterException class with a specified error
        ///     message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public ImageFilterException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
