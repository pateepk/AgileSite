using System;
using System.Collections.Generic;

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Representation of the one record (one row) from the analytics log file.
    /// </summary>
    public class LogRecord
    {
        /// <summary>
        /// Time when the logged record occurred.
        /// </summary>
        public DateTime LogTime
        {
            get;
            set;
        }

        
        /// <summary>
        /// Code name of the log.
        /// </summary>
        public string CodeName
        {
            get;
            set;
        }


        /// <summary>
        /// Number of hits.
        /// </summary>
        public int Hits
        {
            get;
            set;
        }


        /// <summary>
        /// Total value.
        /// </summary>
        public double Value
        {
            get;
            set;
        }


        /// <summary>
        /// List of hits values.
        /// </summary>
        public List<double> ValuesSet
        {
            get;
            set;
        }


        /// <summary>
        /// Conversion name.
        /// </summary>
        public string ObjectName
        {
            get;
            set;
        }


        /// <summary>
        /// Object ID.
        /// </summary>
        public int ObjectId
        {
            get;
            set;
        }

        /// <summary>
        /// Site name where conversion occurred.
        /// </summary>
        public string SiteName
        {
            get;
            set;
        }


        /// <summary>
        /// Culture.
        /// </summary>
        public string Culture
        {
            get;
            set;
        }
    }
}