using System;
using System.Linq;
using System.Text;
using System.Xml.Serialization;


namespace CMS.SocialMarketing.LinkedInInternal
{
    /// <summary>
    /// Represents an error element which is usually returned as an API response in case of a protocol error.
    /// </summary>
    /// <remarks>This class is public for purposes of serialization only. Do not consider it a part of public API. The class' content may change without prior notification.</remarks>
    [XmlRoot("error")]
    public class Error
    {
        /// <summary>
        /// HTTP status code of the response.
        /// </summary>
        [XmlElement("status")]
        public int Status
        {
            get;
            set;
        }


        /// <summary>
        /// Timestampe of the response.
        /// </summary>
        [XmlElement("timestamp")]
        public long Timestamp
        {
            get;
            set;
        }


        /// <summary>
        /// Request identifier.
        /// </summary>
        [XmlElement("request-id")]
        public string RequestId
        {
            get;
            set;
        }


        /// <summary>
        /// Error code of the response.
        /// </summary>
        [XmlElement("error-code")]
        public int ErrorCode
        {
            get;
            set;
        }


        /// <summary>
        /// Message describing the error condition.
        /// </summary>
        [XmlElement("message")]
        public string Message
        {
            get;
            set;
        }
    }
}
