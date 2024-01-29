using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;


namespace CMS.SocialMarketing.LinkedInInternal
{
    /// <summary>
    /// Represents a historical follower statistics element element which is usually returned as an API response to company followers statistics request.
    /// </summary>
    /// <remarks>This class is public for purposes of serialization only. Do not consider it a part of public API. The class' content may change without prior notification.</remarks>
    [XmlRoot("historical-follow-statistics")]
    public class HistoricalFollowStatistics
    {
        /// <summary>
        /// Total count of statistics retrieved.
        /// </summary>
        [XmlAttribute("total")]
        public int Total
        {
            get;
            set;
        }


        /// <summary>
        /// Statistics contained in retrieved response.
        /// </summary>
        [XmlElement("statistic")]
        public List<Statistic> Statistics
        {
            get;
            set;
        }
    }
}
