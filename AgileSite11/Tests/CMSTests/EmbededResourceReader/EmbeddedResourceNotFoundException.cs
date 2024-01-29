using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace CMS.Tests
{
    /// <summary>
    /// Exception thrown where it is not possible to find or extract an embedded resource.
    /// </summary>
    [Serializable]
    public class EmbeddedResourceNotFoundException : Exception
    {
        /// <summary>
        /// Name of the resource, usually only partial (without name-space prefix).
        /// </summary>
        public string EmbeddedResourceName
        {
            get;
            private set;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="EmbeddedResourceNotFoundException"/> class with default values.
        /// </summary>
        /// <param name="embeddedResourceName">Name of the missing resource (see <see cref="EmbeddedResourceName"/>).</param>
        /// <remarks>Default error message (including value of <paramref name="embeddedResourceName"/>) is used.</remarks>
        public EmbeddedResourceNotFoundException(string embeddedResourceName)
            : base(GetErrorMessage(embeddedResourceName))
        {
            EmbeddedResourceName = embeddedResourceName;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="EmbeddedResourceNotFoundException"/> class with a specified error message.
        /// </summary>
        /// <param name="embeddedResourceName">Name of the missing resource (see <see cref="EmbeddedResourceName"/>).</param>
        /// <param name="message">The message that describes the error.</param>
        public EmbeddedResourceNotFoundException(string embeddedResourceName, string message)
            : base(message)
        {
            EmbeddedResourceName = embeddedResourceName;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="EmbeddedResourceNotFoundException"/> class with default values.
        /// </summary>
        /// <param name="embeddedResourceName">Name of the missing resource (see <see cref="EmbeddedResourceName"/>).</param>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        public EmbeddedResourceNotFoundException(string embeddedResourceName, string message, Exception innerException)
            : base(message, innerException)
        {
            EmbeddedResourceName = embeddedResourceName;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="EmbeddedResourceNotFoundException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected EmbeddedResourceNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            EmbeddedResourceName = info.GetString("EmbeddedResourceName");
        }


        /// <summary>
        /// Sets <paramref name="info"/>  with information about the exception.
        /// </summary>
        /// <param name="info">Stores all the data needed to serialize or deserialize an object.</param>
        /// <param name="context">Describes the source and destination of a given serialized stream, and provides an additional caller-defined context.</param>
        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }

            info.AddValue("EmbeddedResourceName", EmbeddedResourceName);

            base.GetObjectData(info, context);
        }


        /// <summary>
        /// Returns error message with embedded resource name incorporated within.
        /// </summary>
        /// <param name="resourceName">Name or partial name of the resource.</param>
        /// <remarks>Message is always in English.</remarks>
        protected static string GetErrorMessage(string resourceName)
        {
            return String.Format("Embedded resource with name {0} not found!", resourceName);
        }
    }
}
