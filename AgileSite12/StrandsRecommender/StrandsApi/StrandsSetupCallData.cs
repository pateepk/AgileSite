using System.Collections.Generic;

namespace CMS.StrandsRecommender
{
    /// <summary>
    /// Wraps all necessary data required for setup call performing.
    /// </summary>
    public class StrandsSetupCallData
    {
        /// <summary>
        /// Validation token.
        /// </summary>
        public string ValidationToken
        {
            get;
            set;
        }


        /// <summary>
        /// Version sent to Strands Recommender. CMS build version should be used.
        /// </summary>
        public string Version
        {
            get;
            set;
        }


        /// <summary>
        /// True if updating feed is enabled, false otherwise.
        /// </summary>
        public bool FeedActive
        {
            get;
            set;
        }


        /// <summary>
        /// Frequency with which should be feed sending to the Strands Recommender.
        /// </summary>
        /// <remarks>
        /// Has to be in specific format {d|w|h}{\d+}.
        /// d stands for daily update, number has to be from interval 0-23 (eg. d0, d15, d23)
        /// w stands for weekly update, number has to be from interval 0-6 (mondays to sundays, eg. w0, w3, w6)
        /// h stands for hourly update, number has to be from interval 1-12 (every n hour, eg. h1, h7, h12)
        /// </remarks>
        public string FeedFrequency
        {
            get;
            set;
        }


        /// <summary>
        /// Type parameter of setup call, 'kentico' value is required by Strands.
        /// </summary>
        public string Type
        {
            get;
            set;
        }


        /// <summary>
        /// Uri of product catalog feed.
        /// </summary>
        public string FeedUrl
        {
            get;
            set;
        }


        /// <summary>
        /// Events which should be tracked by Strands Recommender.
        /// </summary>
        public List<string> Tracking
        {
            get;
            set;
        }


        /// <summary>
        /// Contains pairs for mapping values from feed to the Strands Recommender.
        /// </summary>
        public Dictionary<string, string> Fields
        {
            get;
            set;
        }


        /// <summary>
        /// Number of products per page.
        /// </summary>
        public int Pagination
        {
            get;
            set;
        }


        /// <summary>
        /// Username required to access catalog feed page.
        /// </summary>
        public string CatalogFeedUsername
        {
            get;
            set;
        }


        /// <summary>
        /// Password required to access catalog feed page.
        /// </summary>
        public string CatalogFeedPassword
        {
            get;
            set;
        }
    }
}