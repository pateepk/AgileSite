using System;
using System.Linq;

namespace CMS.DataEngine
{
    /// <summary>
    /// Query expressions
    /// </summary>
    public class QueryMacros
    {
        #region "Constants"

        /// <summary>
        /// Values query macro
        /// </summary>
        public const string VALUES = "##VALUES##";

        /// <summary>
        /// Output from non query statement. Allows to return data from inserted and deleted rows.
        /// </summary>
        public const string OUTPUT = "##OUTPUT##";

        /// <summary>
        /// Columns query macro
        /// </summary>
        public const string COLUMNS = "##COLUMNS##";

        /// <summary>
        /// Distinct query macro
        /// </summary>
        public const string DISTINCT = "##DISTINCT##";

        /// <summary>
        /// Top N query macro
        /// </summary>
        public const string TOPN = "##TOPN##";

        /// <summary>
        /// Order by query macro
        /// </summary>
        public const string ORDERBY = "##ORDERBY##";

        /// <summary>
        /// Source query macro
        /// </summary>
        public const string SOURCE = "##SOURCE##";

        /// <summary>
        /// Where query macro
        /// </summary>
        public const string WHERE = "##WHERE##";

        /// <summary>
        /// Having query macro
        /// </summary>
        public const string HAVING = "##HAVING##";
        
        /// <summary>
        /// Group by query macro
        /// </summary>   
        public const string GROUPBY = "##GROUPBY##";

        #endregion

        
        #region "Properties"

        /// <summary>
        /// Where condition
        /// </summary>
        public string Where
        {
            get;
            set;
        }


        /// <summary>
        /// Order by
        /// </summary>
        public string OrderBy
        {
            get;
            set;
        }


        /// <summary>
        /// Top N records
        /// </summary>
        public int TopN
        {
            get;
            set;
        }


        /// <summary>
        /// List of columns to select
        /// </summary>
        public string Columns
        {
            get;
            set;
        }


        /// <summary>
        /// Query source
        /// </summary>
        public string Source
        {
            get;
            set;
        }


        /// <summary>
        /// Default query source
        /// </summary>
        public string DefaultSource
        {
            get;
            set;
        }


        /// <summary>
        /// If set to true, returns only distinct (different) values.
        /// </summary>
        public bool Distinct
        {
            get;
            set;
        }


        /// <summary>
        /// List of columns to group by
        /// </summary>
        public string GroupBy
        {
            get;
            set;
        }


        /// <summary>
        /// Where condition for the GroupBy clause
        /// </summary>
        public string Having
        {
            get;
            set;
        }


        /// <summary>
        /// List of values for the update / insert query
        /// </summary>
        public string Values
        {
            get;
            set;
        }


        /// <summary>
        /// Output from non query statement. Allows to return data from inserted and deleted rows.
        /// </summary>
        public string Output
        {
            get;
            set;
        }


        /// <summary>
        /// Total items expression
        /// </summary>
        public string Total
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Resolves the given macros within a query
        /// </summary>
        /// <param name="queryText">Query text</param>
        public string ResolveMacros(string queryText)
        {
            return SqlMacroHelper.ResolveQueryMacros(this, queryText);
        }


        /// <summary>
        /// Gets the string representation of the where condition
        /// </summary>
        public override string ToString()
        {
            var parts = new []
            {
                GetPart("WHERE", Where),
                GetPart("ORDERBY", OrderBy),
                GetPart("TOPN", TopN),
                GetPart("COLUMNS", Columns),
                GetPart("SOURCE", Source ?? DefaultSource),
                GetPart("GROUPBY", GroupBy),
                GetPart("HAVING", Having),
                GetPart("VALUES", Values),
                GetPart("OUTPUT", Output),
                GetPart("TOTAL", Total)
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