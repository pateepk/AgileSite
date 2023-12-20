﻿using System;
using System.Runtime.Serialization;

namespace CMS.OnlineForms
{
    /// <summary>
    /// Thrown when biz form operation fails.
    /// </summary>
    [Serializable]
    public class BizFormException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BizFormException"/> class with default values.
        /// </summary>
        public BizFormException()
          : base()
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="BizFormException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public BizFormException(string message)
          : base(message)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="BizFormException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="inner">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        public BizFormException(string message, Exception inner)
          : base(message, inner)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="BizFormException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        protected BizFormException(SerializationInfo info, StreamingContext context)
          : base(info, context)
        {
        }
    }
}
