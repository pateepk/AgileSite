using System;
using System.Linq;
using System.Text;
using System.Xml.Serialization;


namespace CMS.SocialMarketing.LinkedInInternal
{
    /// /// <summary>
    /// Represents a company update statistic.
    /// </summary>
    /// <remarks>This class is public for purposes of serialization only. Do not consider it a part of public API. The class' content may change without prior notification.</remarks>
    public class Statistic
    {
        #region "Common"

        /// <summary>
        /// Time the statistic is related to (UNIX time stamp in milliseconds).
        /// </summary>
        /// <remarks>The LinkedIn returns a timestamp representing a UTC midnight of the day the stat is related to (or midnight of 1st day of month in case of granularity set to month).</remarks>
        [XmlElement("time")]
        public long Time
        {
            get;
            set;
        }

        #endregion


        #region "Historical status update statistics"

        /// <summary>
        /// Count of clicks.
        /// </summary>
        /// <remarks>Shall be used in context of <see cref="HistoricalStatusUpdateStatistics"/> only.</remarks>
        [XmlElement("click-count")]
        public int ClickCount
        {
            get;
            set;
        }


        /// <summary>
        /// Count of comments.
        /// </summary>
        /// <remarks>Shall be used in context of <see cref="HistoricalStatusUpdateStatistics"/> only.</remarks>
        [XmlElement("comment-count")]
        public int CommentCount
        {
            get;
            set;
        }


        /// <summary>
        /// Engagement [%].
        /// </summary>
        /// <remarks>Shall be used in context of <see cref="HistoricalStatusUpdateStatistics"/> only.</remarks>
        [XmlElement("engagement")]
        public double Engagement
        {
            get;
            set;
        }


        /// <summary>
        /// Count of impressions.
        /// </summary>
        /// <remarks>Shall be used in context of <see cref="HistoricalStatusUpdateStatistics"/> only.</remarks>
        [XmlElement("impression-count")]
        public int ImpressionCount
        {
            get;
            set;
        }


        /// <summary>
        /// Count of likes.
        /// </summary>
        /// <remarks>Shall be used in context of <see cref="HistoricalStatusUpdateStatistics"/> only.</remarks>
        [XmlElement("like-count")]
        public int LikeCount
        {
            get;
            set;
        }

        /// <summary>
        /// Count of shares.
        /// </summary>
        /// <remarks>Shall be used in context of <see cref="HistoricalStatusUpdateStatistics"/> only.</remarks>
        [XmlElement("share-count")]
        public int ShareCount
        {
            get;
            set;
        }


        /// <summary>
        /// Count of unique member impressions.
        /// This stat is available for company profiles only. The update level is not supported.
        /// </summary>
        /// <remarks>Shall be used in context of <see cref="HistoricalStatusUpdateStatistics"/> only.</remarks>
        [XmlElement("unique-count")]
        public int UniqueCount
        {
            get;
            set;
        }

        #endregion


        #region "Historical follower statistics"

        /// <summary>
        /// Total count of followers.
        /// </summary>
        /// <remarks>Shall be used in context of <see cref="HistoricalFollowStatistics"/> only.</remarks>
        [XmlElement("total-follower-count")]
        public int TotalFollowerCount
        {
            get;
            set;
        }


        /// <summary>
        /// Count of organic followers.
        /// </summary>
        /// <remarks>Shall be used in context of <see cref="HistoricalFollowStatistics"/> only.</remarks>
        [XmlElement("organic-follower-count")]
        public int OrganicFollowerCount
        {
            get;
            set;
        }


        /// <summary>
        /// Count of paid followers.
        /// </summary>
        /// <remarks>Shall be used in context of <see cref="HistoricalFollowStatistics"/> only.</remarks>
        [XmlElement("paid-follower-count")]
        public int PaidFollowerCount
        {
            get;
            set;
        }

        #endregion
    }
}
