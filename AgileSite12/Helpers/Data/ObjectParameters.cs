using System;
using System.Collections;
using System.Runtime.Serialization;

namespace CMS.Helpers
{
    /// <summary>
    /// Object parameters container.
    /// </summary>    
    [Serializable]
    public class ObjectParameters : XmlData
    {
        #region "Constants"

        /// <summary>
        /// Root element name of custom data.
        /// </summary>
        private const string PARAMETERSROOTNAME = "Parameters";

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor for deserialization.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        public ObjectParameters(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            AllowMacros = true;
        }


        /// <summary>
        /// Constructor - creates empty ObjectParameters object.
        /// </summary>
        public ObjectParameters()
            : base(PARAMETERSROOTNAME)
        {
            AllowMacros = true;
        }

        #endregion
    }
}