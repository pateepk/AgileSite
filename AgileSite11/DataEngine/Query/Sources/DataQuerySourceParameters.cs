using System;
using System.Linq;

namespace CMS.DataEngine
{
    /// <summary>
    /// Parameters for the data query source
    /// </summary>
    public class DataQuerySourceParameters
    {
        #region "Properties"

        /// <summary>
        /// Executing query
        /// </summary>
        public IDataQuery Query
        {
            get;
            set;
        }


        /// <summary>
        /// Query parameters
        /// </summary>
        public DataQuerySettings Settings
        {
            get;
            set;
        }


        /// <summary>
        /// Offset of the first record to retrieve
        /// </summary>
        public int Offset
        {
            get;
            set;
        }


        /// <summary>
        /// Max records to retrieve
        /// </summary>
        public int MaxRecords
        {
            get;
            set;
        }


        /// <summary>
        /// Returns total records
        /// </summary>
        public int TotalRecords
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="query">Data query</param>
        /// <param name="settings">Query parameters</param>
        /// <param name="offset">Offset of the first record to retrieve</param>
        /// <param name="maxRecords">Max records to retrieve</param>
        public DataQuerySourceParameters(IDataQuery query, DataQuerySettings settings, int offset, int maxRecords)
        {
            Query = query;
            Settings = settings;
            Offset = offset;
            MaxRecords = maxRecords;
        }


        /// <summary>
        /// Gets the string representation of the where condition
        /// </summary>
        public override string ToString()
        {
            var parts = new[]
            {
                GetPart("SETTINGS", Environment.NewLine + Settings + Environment.NewLine),
                GetPart("OFFSET", Offset),
                GetPart("MAXRECORDS", MaxRecords)
            }
            .Where(p => !String.IsNullOrEmpty(p));

            return String.Join(
                Environment.NewLine,
                parts
            );
        }


        /// <summary>
        /// Gets the part of the string representation
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="value">Value</param>
        private string GetPart(string name, object value)
        {
            if (value == null)
            {
                return null;
            }

            return String.Format("{0}: {1},", name, value.ToString().Trim());
        }

        #endregion
    }
}