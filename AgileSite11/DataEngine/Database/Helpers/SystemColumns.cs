using System;

namespace CMS.DataEngine
{
    /// <summary>
    /// System column names
    /// </summary>
    public static class SystemColumns
    {
        /// <summary>
        /// Prefix for all system columns.
        /// </summary>
        private const string COLUMN_PREFIX = "CMS_";

        /// <summary>
        /// Order row number
        /// </summary>
        public const string ORDER_ROW_NUMBER = COLUMN_PREFIX + "ORN";

        /// <summary>
        /// Order column
        /// </summary>
        public const string ORDER = COLUMN_PREFIX + "O";

        /// <summary>
        /// Source row number column name
        /// </summary>
        public const string SOURCE_ROW_NUMBER = COLUMN_PREFIX + "SRN";

        /// <summary>
        /// Source number column name
        /// </summary>
        public const string SOURCE_NUMBER = COLUMN_PREFIX + "SN";

        /// <summary>
        /// Source type column name
        /// </summary>
        public const string SOURCE_TYPE = COLUMN_PREFIX + "T";

        /// <summary>
        /// Row number
        /// </summary>
        public const string ROW_NUMBER = COLUMN_PREFIX + "RN";

        /// <summary>
        /// Total number of found records
        /// </summary>
        public const string TOTAL_RECORDS = COLUMN_PREFIX + "TOT";

        /// <summary>
        /// Total number of found records for a sub-query in multi-query
        /// </summary>
        public const string SUB_TOTAL_RECORDS = COLUMN_PREFIX + "STOT";


        /// <summary>
        /// Aggregated total for multi-query
        /// </summary>
        internal static readonly string AGGREGATED_TOTAL = String.Format(
            "SELECT (SUM({0})) AS [{1}] FROM (SELECT [CMS_SN], (MAX({0})) AS [{0}] FROM AllData GROUP BY CMS_SN) AS SubData", 
            SUB_TOTAL_RECORDS, 
            TOTAL_RECORDS
        );


        /// <summary>
        /// Column name with priorities of duplicates
        /// </summary>
        public const string DUPLICATE_PRIORITY = COLUMN_PREFIX + "DUP";


        /// <summary>
        /// Checks if column is one of the system columns.
        /// </summary>
        /// <param name="columnName">Name of column</param>
        public static bool IsSystemColumn(string columnName)
        {
            return columnName.StartsWith(COLUMN_PREFIX, StringComparison.Ordinal);
        }
    }
}