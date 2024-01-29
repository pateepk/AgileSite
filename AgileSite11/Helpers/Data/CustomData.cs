using System;
using System.Runtime.Serialization;

namespace CMS.Helpers
{
    /// <summary>
    /// Custom data container.
    /// </summary>    
    [Serializable]
    public class CustomData : XmlData
    {
        /// <summary>
        /// Root element name of custom data.
        /// </summary>
        private const string CUSTOMDATAROOTNAME = "CustomData";


        /// <summary>
        /// Constructor for deserialization.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        public CustomData(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }


        /// <summary>
        /// Constructor - creates empty CustomData object.
        /// </summary>
        public CustomData()
            : base(CUSTOMDATAROOTNAME)
        {
        }
    }
}