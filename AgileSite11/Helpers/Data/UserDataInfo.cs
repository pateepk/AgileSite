using System;
using System.Runtime.Serialization;

namespace CMS.Helpers
{
    /// <summary>
    /// Custom data container.
    /// </summary>    
    [Serializable]
    public class UserDataInfo : XmlData
    {
        /// <summary>
        /// Root element name of custom data.
        /// </summary>
        private const string USERDATAROOTNAME = "info";


        #region "Properties"

        /// <summary>
        /// Gets or sets the IP address.
        /// </summary>
        public string IPAddress
        {
            get
            {
                return (string)this["ip"];
            }
            set
            {
                this["ip"] = value;
            }
        }


        /// <summary>
        /// Gets or sets the agent(browser) information.
        /// </summary>
        public string Agent
        {
            get
            {
                return (string)this["agent"];
            }
            set
            {
                this["agent"] = value;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor for deserialization.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        public UserDataInfo(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }


        /// <summary>
        /// Constructor - creates empty CustomData object.
        /// </summary>
        public UserDataInfo()
            : base(USERDATAROOTNAME)
        {
        }

        #endregion
    }
}