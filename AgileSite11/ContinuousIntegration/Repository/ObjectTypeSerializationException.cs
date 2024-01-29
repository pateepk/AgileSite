using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace CMS.ContinuousIntegration
{
    /// <summary>
    /// Thrown when serialization of objects of given object type fails.
    /// </summary>
    [Serializable]
    internal class ObjectTypeSerializationException : ObjectSerializationException
    {
        protected const int NO_INFO_OBJECT_ID = -1;


        /// <summary>
        /// Name of object type which serialization failed.
        /// </summary>
        public String ObjectTypeName
        {
            get;
            private set;
        }


        /// <summary>
        /// Identifier of the object that causes serialization to fail.
        /// </summary>
        public int InfoObjectId
        {
            get;
            private set;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectTypeSerializationException"/> class with default values.
        /// </summary>
        /// <param name="objectTypeName">Name of object type the serialization failed on.</param>
        /// <param name="infoObjectId">Identifier of an instance of the object type the serialization failed on.</param>
        public ObjectTypeSerializationException(string objectTypeName, int infoObjectId = NO_INFO_OBJECT_ID)
        {
            ObjectTypeName = objectTypeName;
            InfoObjectId = infoObjectId;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectTypeSerializationException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="objectTypeName">Name of object type the serialization failed on.</param>
        /// <param name="infoObjectId">Identifier of an instance of the object type the serialization failed on.</param>
        public ObjectTypeSerializationException(string message, string objectTypeName, int infoObjectId = NO_INFO_OBJECT_ID)
            : base(message)
        {
            ObjectTypeName = objectTypeName;
            InfoObjectId = infoObjectId;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectTypeSerializationException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        /// <param name="objectTypeName">Name of object type the serialization failed on.</param>
        /// <param name="infoObjectId">Identifier of an instance of the object type the serialization failed on.</param>
        public ObjectTypeSerializationException(string message, Exception innerException, string objectTypeName, int infoObjectId = NO_INFO_OBJECT_ID)
            : base(message, innerException)
        {
            ObjectTypeName = objectTypeName;
            InfoObjectId = infoObjectId;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectTypeSerializationException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected ObjectTypeSerializationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            ObjectTypeName = info.GetString("ObjectTypeName");
            InfoObjectId = info.GetInt32("InfoObjectId");
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

            info.AddValue("ObjectTypeName", ObjectTypeName);
            info.AddValue("InfoObjectId", InfoObjectId);

            base.GetObjectData(info, context);
        }

    }
}