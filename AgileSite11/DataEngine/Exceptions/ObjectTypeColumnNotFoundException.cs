using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace CMS.DataEngine
{
    /// <summary>
    /// The ObjectTypeColumnNotFoundException is thrown when an object type is without column definition.
    /// </summary>
    [Serializable]
    public class ObjectTypeColumnNotFoundException : Exception
    {
        /// <summary>
        /// Gets the object type whose column wasn't found.
        /// </summary>
        public string ObjectType
        {
            get;
            protected set;
        }


        /// <summary>
        /// Creates a new instance of ObjectTypeColumnNotFoundException.
        /// </summary>
        public ObjectTypeColumnNotFoundException()
        {
        }


        /// <summary>
        /// Creates a new instance of ObjectTypeColumnNotFoundException.
        /// </summary>
        /// <param name="message">Exception's message.</param>
        /// <param name="objectType">The object type whose column wasn't found.</param>
        public ObjectTypeColumnNotFoundException(string message, string objectType = null)
            : base(message)
        {
            ObjectType = objectType;
        }


        /// <summary>
        /// Creates a new instance of ObjectTypeColumnNotFoundException.
        /// </summary>
        /// <param name="message">Exception's message.</param>
        /// <param name="inner">Inner exception.</param>
        public ObjectTypeColumnNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }


        /// <summary>
        /// Creates a new instance of ObjectTypeColumnNotFoundException.
        /// </summary>
        /// <param name="message">Exception's message.</param>
        /// <param name="objectType">The object type whose column wasn't found.</param>
        /// <param name="inner">Inner exception.</param>
        public ObjectTypeColumnNotFoundException(string message, string objectType, Exception inner)
            : base(message, inner)
        {
            ObjectType = objectType;
        }


        /// <summary>
        /// Creates a new instance of ObjectTypeColumnNotFoundException.
        /// </summary>
        /// <param name="info">SerializationInfo.</param>
        /// <param name="context">StreamingContext.</param>
        protected ObjectTypeColumnNotFoundException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }
    }
}
