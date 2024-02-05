using System;
using System.Linq;
using System.Text;
using System.Xml.Serialization;


namespace CMS.SocialMarketing.LinkedInInternal
{
    /// <summary>
    /// Represents a response body after publishing a new share.
    /// </summary>
    /// <remarks>This class is public for purposes of serialization only. Do not consider it a part of public API. The class' content may change without prior notification.</remarks>
    [XmlRoot("update")]
    public class Update
    {
        /// <summary>
        /// Update key.
        /// </summary>
        [XmlElement("update-key")]
        public string UpdateKey
        {
            get;
            set;
        }


        /// <summary>
        /// Update URL.
        /// </summary>
        [XmlElement("update-url")]
        public string UpdateUrl
        {
            get;
            set;
        }
    }
}
