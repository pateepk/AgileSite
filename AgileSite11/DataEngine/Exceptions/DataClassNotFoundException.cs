using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace CMS.DataEngine
{
    /// <summary>
    /// This exception is thrown when data class is not found.
    /// </summary>
    [Serializable]
    public class DataClassNotFoundException : Exception
    {
        /// <summary>
        /// Gets name of the class that wasn't found.
        /// </summary>
        public string ClassName
        {
            get;
            protected set;
        }


        /// <summary>
        /// Gets the object type whose class wasn't found.
        /// </summary>
        public string ObjectType
        {
            get;
            protected set;
        }


        /// <summary>
        /// Creates a new instance of DataClassNotFoundException.
        /// </summary>
        public DataClassNotFoundException()
        {
        }


        /// <summary>
        /// Creates a new instance of DataClassNotFoundException.
        /// </summary>
        /// <param name="message">Exception's message.</param>
        /// <param name="className">Name of the class that wasn't found.</param>
        /// <param name="objectType">The object type whose class wasn't found.</param>
        public DataClassNotFoundException(string message, string className, string objectType = null)
            : base(message)
        {
            ClassName = className;
            ObjectType = objectType;
        }


        /// <summary>
        /// Creates a new instance of DataClassNotFoundException.
        /// </summary>
        /// <param name="message">Exception's message.</param>
        /// <param name="className">Name of the class that wasn't found.</param>
        /// <param name="inner">Inner exception.</param>
        public DataClassNotFoundException(string message, string className, Exception inner)
            : base(message, inner)
        {
            ClassName = className;
        }


        /// <summary>
        /// Creates a new instance of DataClassNotFoundException.
        /// </summary>
        /// <param name="message">Exception's message.</param>
        /// <param name="className">Name of the class that wasn't found.</param>
        /// <param name="objectType">The object type whose class wasn't found.</param>
        /// <param name="inner">Inner exception.</param>
        public DataClassNotFoundException(string message, string className, string objectType, Exception inner)
            : base(message, inner)
        {
            ClassName = className;
            ObjectType = objectType;
        }


        /// <summary>
        /// Creates a new instance of DataClassNotFoundException.
        /// </summary>
        /// <param name="info">SerializationInfo.</param>
        /// <param name="context">StreamingContext.</param>
        protected DataClassNotFoundException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }
    }
}
