using System;
using System.Runtime.Serialization;
using System.Security;

namespace CMS.DataEngine
{
    /// <summary>
    /// Data column definition
    /// </summary>
    [Serializable]
    public struct ColumnDefinition : ISerializable
    {
        /// <summary>
        /// Column name
        /// </summary>
        public string ColumnName
        {
            get;
        }


        /// <summary>
        /// Column type
        /// </summary>
        public Type ColumnType
        {
            get;
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Column name</param>
        /// <param name="type">Column type</param>
        public ColumnDefinition(string name, Type type) : this()
        {
            ColumnName = name;
            ColumnType = type;
        }


        /// <summary>
        /// Constructor for deserialization.
        /// </summary>
        /// <param name="info">Serialization inf</param>
        /// <param name="context">Streaming context</param>
        public ColumnDefinition(SerializationInfo info, StreamingContext context) : this()
        {
            ColumnName = (string)info.GetValue("ColumnName", typeof(string));
            ColumnType = (Type)info.GetValue("ColumnType", typeof(Type));
        }


        /// <summary>
        /// Object serialization.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Context</param>
        [SecurityCritical]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("ColumnName", ColumnName);
            info.AddValue("ColumnType", ColumnType);
        }
    }
}
