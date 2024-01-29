using System;
using System.Linq;
using System.Text;
using System.Xml.Serialization;


namespace CMS.SocialMarketing.LinkedInInternal
{
    /// <summary>
    /// Represents a request body for publishing a new share.
    /// </summary>
    /// <remarks>This class is public for purposes of serialization only. Do not consider it a part of public API. The class' content may change without prior notification.</remarks>
    [XmlRoot("share")]
    public class Share
    {
        /// <summary>
        /// Share's comment.
        /// </summary>
        [XmlElement("comment")]
        public string Comment
        {
            get;
            set;
        }


        /// <summary>
        /// Share's visibility.
        /// </summary>
        [XmlElement("visibility")]
        public Visibility Visibility
        {
            get;
            set;
        }
    }
}
