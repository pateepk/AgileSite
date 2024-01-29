using System;
using System.Linq;
using System.Text;
using System.Xml.Serialization;


namespace CMS.SocialMarketing.LinkedInInternal
{
    /// <summary>
    /// Represents visibility setting used i.e. by shares.
    /// </summary>
    /// <remarks>This class is public for purposes of serialization only. Do not consider it a part of public API. The class' content may change without prior notification.</remarks>
    public class Visibility
    {
        /// <summary>
        /// Visibility code.
        /// </summary>
        [XmlElement("code")]
        public string Code
        {
            get;
            set;
        }
    }
}
