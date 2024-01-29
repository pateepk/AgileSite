using System;

namespace CMS.DataEngine
{
    /// <summary>
    /// Exception raised when unsupported SQL type is encountered.
    /// </summary>
    public class MissingSQLTypeException : Exception
    {
        /// <summary>
        /// Gets or sets table column name.
        /// </summary>
        public string ColumnName
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets unsupported SQL type.
        /// </summary>
        public string UnsupportedType
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets supported SQL type.
        /// </summary>
        public string RecommendedType
        {
            get;
            set;
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Message text</param>
        /// <param name="columnName">Column name</param>
        /// <param name="currentSqlType">Unsupported type</param>
        /// <param name="recommendedSqlType">Supported type</param>
        public MissingSQLTypeException(string message, string columnName, string currentSqlType, string recommendedSqlType)
            : base(message)
        {
            ColumnName = columnName;
            UnsupportedType = currentSqlType;
            RecommendedType = recommendedSqlType;
        }
    }
}
