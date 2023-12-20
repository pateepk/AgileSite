using System;
using System.Linq;
using System.Text;
using System.Xml.Serialization;


namespace CMS.SocialMarketing
{
    /// <summary>
    /// Represents state of LinkedIn account insights (statistics of a company profile)
    /// </summary>
    public class LinkedInAccountInsightsState
    {
        /// <summary>
        /// Account ID to which the task belongs.
        /// </summary>
        [XmlElement("accountId")]
        public int AccountId
        {
            get;
            set;
        }


        /// <summary>
        /// Represents the last DateTime of collected LinkedIn statistics.
        /// The LinkedIn uses UTC time thus the last collected date time should be a UTC midnight.
        /// </summary>
        [XmlElement("lastCollectedDateTime", IsNullable = true)]
        public DateTime? LastCollectedDateTime
        {
            get;
            set;
        }
    }
}
