using System;
using System.Linq;
using System.Text;
using System.Xml.Serialization;


namespace CMS.SocialMarketing.LinkedInInternal
{
    /// <summary>
    /// Represents a LinkedIn company.
    /// </summary>
    /// <remarks>This class is public for purposes of serialization only. Do not consider it a part of public API. The class' content may change without prior notification.</remarks>
    public class Company
    {
        /// <summary>
        /// Company ID.
        /// </summary>
        [XmlElement("id")]
        public string ID
        {
            get;
            set;
        }


        /// <summary>
        /// Company name.
        /// </summary>
        [XmlElement("name")]
        public string Name
        {
            get;
            set;
        }
    }
}
