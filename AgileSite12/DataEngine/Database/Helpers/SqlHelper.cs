using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using CMS.Base;
using CMS.Core;
using CMS.Helpers;

using Microsoft.SqlServer.Server;

namespace CMS.DataEngine
{
    /// <summary>
    /// Class to provide common SQL methods.
    /// </summary>
    public static class SqlHelper
    {
        #region "Constants"

        /// <summary>
        /// Constant for no columns.
        /// </summary>
        public const string NO_COLUMNS = "#NONE#";


        /// <summary>
        /// Value for total records input to not get the total amount of data.
        /// </summary>
        public const int NO_TOTALRECORDS = -2;


        /// <summary>
        /// Where condition representing no data.
        /// </summary>
        public const string NO_DATA_WHERE = "0 = 1";


        /// <summary>
        /// Where condition representing all data.
        /// </summary>
        private const string ALL_DATA_WHERE = "1 = 1";


        /// <summary>
        /// Maximum parameter length to log.
        /// </summary>
        public const int MAX_PARAM_LENGTH = 200;


        /// <summary>
        /// Unknown value.
        /// </summary>
        public static object MISSING_VALUE = new MissingValue();


        /// <summary>
        /// Default db schema.
        /// </summary>
        public static string DEFAULT_DB_SCHEMA = "dbo";


        /// <summary>
        /// Suffix for the descending order
        /// </summary>
        public const string ORDERBY_DESC = " DESC";


        /// <summary>
        /// Suffix for the ascending order
        /// </summary>
        public const string ORDERBY_ASC = " ASC";


        /// <summary>
        /// Represents all columns within given query
        /// </summary>
        public const string COLUMNS_ALL = "*";


        /// <summary>
        /// Default inline limit for SQL lists. When the number of values is below this number, the lists on SQL, e.g. IN (x, y, z), are evaluated as inline lists.
        /// </summary>
        public static IntAppSetting DefaultSQLInlineLimit = new IntAppSetting("CMSDefaultSQLInlineLimit", 1000);


        /// <summary>
        /// Format string for the update value expression
        /// </summary>
        internal const string UPDATE_FORMAT = "[{0}] = {1}";


        /// <summary>
        /// Format string for the insert value expression
        /// </summary>
        internal const string INSERT_FORMAT = "{1}";


        /// <summary>
        /// Represents delimiter between object names in SQL syntax
        /// </summary>
        private const char SQL_OBJECT_NAME_DELIMITER = '.';


        /// <summary>
        /// Values in SQL syntax
        /// </summary>
        private const string SQL_VALUES = "VALUES";

        #endregion


        #region "Variables"

        private static Type mIntegerTableType;
        private static Type mOrderedIntegerTableType;

        private static readonly Lazy<IPerformanceCounter> mRunningQueries = new Lazy<IPerformanceCounter>(Service.Resolve<IPerformanceCounter>);

        // Regular expression for removing schema name from DB object name.
        private static Regex mSchemaNameRegEx;

        private static readonly CMSStatic<string> mDbSchema = new CMSStatic<string>();

        /// <summary>
        /// General select SQL statement
        /// </summary>
        public const string GENERAL_SELECT =
@"SELECT ##DISTINCT## ##TOPN## ##COLUMNS## 
FROM ##SOURCE## 
WHERE ##WHERE## 
GROUP BY ##GROUPBY## 
HAVING ##HAVING## ORDER BY ##ORDERBY##";

        /// <summary>
        /// General insert SQL statement
        /// </summary>
        public const string GENERAL_INSERT =
@"INSERT INTO ##SOURCE## (##COLUMNS##) 
VALUES (##VALUES##);

SELECT SCOPE_IDENTITY() AS [ID]";

        /// <summary>
        /// General update SQL statement
        /// </summary>
        public const string GENERAL_UPDATE =
@"UPDATE ##SOURCE## SET ##VALUES## OUTPUT ##OUTPUT##
WHERE ##WHERE##";

        /// <summary>
        /// General upsert (update/insert) SQL statement
        /// </summary>
        public const string GENERAL_UPSERT =
@"IF NOT EXISTS (SELECT * FROM ##SOURCE## WHERE ##WHERE##)
BEGIN
	INSERT INTO ##SOURCE## (##COLUMNS##) VALUES (##VALUES##)
	SELECT SCOPE_IDENTITY() AS [ID]
END ELSE 
	UPDATE ##SOURCE## SET ##VALUES1## WHERE ##WHERE##";

        /// <summary>
        /// General delete SQL statement
        /// </summary>
        public const string GENERAL_DELETE = "DELETE FROM ##SOURCE## WHERE ##WHERE##";


        /// <summary>
        /// Empty select statement
        /// </summary>
        public const string SELECT_NULL = "SELECT NULL";

        #endregion


        #region "Regular expressions"

        /// <summary>
        /// Regex matching comments within a SQL query
        /// </summary>
        public static CMSRegex CommentsRegEx = new CMSRegex(@"(/\*([^*]|[\r\n]|(\*+([^*/]|[\r\n])))*\*+/)|(--.*)");


        /// <summary>
        /// Regex pattern for the column alias
        /// </summary>
        private const string COLUMNS_ALIAS_PATTERN = "[a-z0-9_]+|\\[[^\\]]+\\]|'[^']+'|\"[^\"]+\"";


        /// <summary>
        /// Regex matching column alias within a column expression
        /// </summary>
        /// Groups:                                                            (1: alias first               )                                   (2: alias last                   )
        private static readonly CMSRegex ColumnAliasRegEx = new CMSRegex("^(" + COLUMNS_ALIAS_PATTERN + ")\\s*=|(?:\\bas\\b|(?<=[^=\\s]))\\s+(" + COLUMNS_ALIAS_PATTERN + ")$", true);


        // Simple column regex (used for fast evaluation of columns without aliases)
        private static readonly CMSRegex SimpleColumnRegex = new CMSRegex("^\\w+$");


        /// <summary>
        /// Name of the property to flag a table to be originated from database
        /// </summary>
        internal const string TABLE_IS_FROM_CMS_DB = "IsFromCMSDatabase";


        /// <summary>
        /// Name of the property to flag a table which contains external data
        /// </summary>
        internal const string TABLE_CONTAINS_EXTERNAL_DATA = "CMSHasExternalData";


        /// <summary>
        /// Returns true if the data engine allows partial updates to database (updates only fields which values have changed)
        /// </summary>
        internal static BoolAppSetting AllowPartialUpdates = new BoolAppSetting("CMSAllowPartialUpdates", true);

        #endregion


        #region "Properties"

        /// <summary>
        /// Counter of running queries.
        /// </summary>
        public static IPerformanceCounter RunningQueries => mRunningQueries.Value;


        /// <summary>
        /// Returns type for User-defined integer SQL type
        /// </summary>
        public static Type IntegerTableType => mIntegerTableType ?? (mIntegerTableType = typeof(IEnumerable<int>));


        /// <summary>
        /// Returns type for User-defined integer SQL type
        /// </summary>
        public static Type OrderedIntegerTableType => mOrderedIntegerTableType ?? (mOrderedIntegerTableType = typeof(IOrderedEnumerable<int>));


        /// <summary>
        /// Regular expression for removing schema from DB object name.
        /// </summary>
        private static Regex SchemaNameRegEx => mSchemaNameRegEx ?? (mSchemaNameRegEx = RegexHelper.GetRegex(@"((\[.*?\])|(.*?))\.(?<objectname>\S+)"));

        #endregion


        #region "Query debug methods"

        /// <summary>
        /// Gets the parameters string.
        /// </summary>
        /// <param name="parameters">Query parameters</param>
        public static string GetParamCacheString(QueryDataParameters parameters)
        {
            if (parameters != null)
            {
                StringBuilder sb = new StringBuilder();

                // Process all parameters
                foreach (DataParameter param in parameters)
                {
                    if (!String.IsNullOrEmpty(param.Name))
                    {
                        if (sb.Length > 0)
                        {
                            sb.Append("|");
                        }

                        // Get the parameter value
                        sb.Append(param.Name);
                        sb.Append("=");

                        object value = param.Value;
                        if (value != null)
                        {
                            sb.Append(value.GetHashCode());
                        }
                    }
                }

                return sb.ToString();
            }

            return "";
        }


        /// <summary>
        /// Gets the parameters string.
        /// </summary>
        /// <param name="parameters">Query parameters</param>
        /// <param name="separator">Separator</param>
        /// <param name="size">Size of all objects</param>
        public static string GetParamString(QueryDataParameters parameters, string separator, out int size)
        {
            size = 0;

            if (parameters != null)
            {
                StringBuilder sb = new StringBuilder();

                foreach (DataParameter param in parameters)
                {
                    if (!String.IsNullOrEmpty(param.Name))
                    {
                        if (sb.Length > 0)
                        {
                            sb.Append(separator);
                        }

                        // Get the parameter value
                        object value = param.Value;
                        size += DataHelper.GetObjectSize(value);

                        string paramValue = ValidationHelper.GetString(value, "");
                        if (paramValue.Length > MAX_PARAM_LENGTH)
                        {
                            paramValue = paramValue.Substring(0, MAX_PARAM_LENGTH) + "...";
                        }

                        sb.Append(param.Name);
                        sb.Append(" (");

                        // Handle specific types
                        if (value is string)
                        {
                            sb.Append('\"');
                            sb.Append(paramValue);
                            sb.Append('\"');
                        }
                        else if (value is byte[])
                        {
                            sb.Append(paramValue);
                            sb.Append(" ");
                            sb.Append(DataHelper.GetSizeString(((byte[])value).Length));
                        }
                        else
                        {
                            sb.Append(paramValue);
                        }

                        sb.Append(")");
                    }
                }

                return sb.ToString();
            }

            return "";
        }


        /// <summary>
        /// Gets the results as a string for log.
        /// </summary>
        /// <param name="result">Result object</param>
        /// <param name="totalSize">Total size of the result</param>
        public static string GetResultsString(object result, out int totalSize)
        {
            string results = "";
            totalSize = 0;

            var ds = result as DataSet;
            if (ds != null)
            {
                // Report the DataSet size
                foreach (DataTable dt in ds.Tables)
                {
                    if (dt.Rows.Count > 0)
                    {
                        if (results != "")
                        {
                            results += "\r\n";
                        }

                        int size = DataHelper.GetTableSize(dt);
                        totalSize += size;
                        results += dt.TableName + " (" + dt.Rows.Count + " [" + dt.Columns.Count + "], " + DataHelper.GetSizeString(size) + ")";
                    }
                }
            }
            else
            {
                // Convert to string
                results = result.ToString();
            }

            return results;
        }

        #endregion


        #region "Value methods"

        /// <summary>
        /// Gets the row number expression
        /// </summary>
        /// <param name="orderBy">Order by expression</param>
        /// <param name="partitionBy">Partition by expression</param>
        public static string GetRowNumber(string orderBy, string partitionBy = null)
        {
            // Order by is always used
            var over = String.Format("ORDER BY {0}", orderBy);
            if (!String.IsNullOrEmpty(partitionBy))
            {
                // Partition by only when specified
                over = String.Format("PARTITION BY {0} ", partitionBy) + over;
            }

            return ApplyOver("ROW_NUMBER()", over);
        }


        /// <summary>
        /// Gets the round expression
        /// </summary>
        /// <param name="value">Value expression</param>
        /// <param name="places">Places to round to</param>
        public static string GetRound(string value, int places)
        {
            return String.Format("ROUND({0}, {1})", value, places);
        }


        /// <summary>
        /// Get the CAST expression.
        /// </summary>
        /// <param name="value">Value to be casted</param>
        /// <param name="type">Type the value should be casted to</param>
        /// <example>
        ///     <code>
        ///     GetCast("ActivityValue", "FLOAT")
        ///     </code>
        /// returns "CAST(ActivityValue AS INT)"
        /// </example>
        public static string GetCast(string value, string type)
        {
            return String.Format("CAST({0} AS {1})", value, type);
        }


        /// <summary>
        /// Applies the over clause to the given expression
        /// </summary>
        /// <param name="expression">Expression</param>
        /// <param name="over">Over clause. When null, over clause is not generated at all, otherwise it is generated even when value is an empty string, in such case " OVER ()" is generated</param>
        public static string ApplyOver(string expression, string over)
        {
            if (over == null)
            {
                return expression;
            }

            return String.Format("{0} OVER ({1})", expression, over);
        }


        /// <summary>
        /// Gets the aggregation expression from the given expression
        /// </summary>
        /// <param name="expression">Expression</param>
        /// <param name="type">Aggregation type</param>
        public static string GetAggregation(string expression, AggregationType type)
        {
            string format;

            switch (type)
            {
                case AggregationType.Average:
                    format = "AVG({0})";
                    break;

                case AggregationType.Min:
                    format = "MIN({0})";
                    break;

                case AggregationType.Max:
                    format = "MAX({0})";
                    break;

                case AggregationType.Sum:
                    format = "SUM({0})";
                    break;

                case AggregationType.Count:
                    {
                        // Ensure * for column name if expression not provided
                        if (String.IsNullOrEmpty(expression))
                        {
                            expression = "*";
                        }

                        format = "COUNT({0})";
                    }
                    break;

                default:
                    return expression;
            }

            if (String.IsNullOrEmpty(expression))
            {
                throw new NotSupportedException("[SqlHelper.GetAggregation]: You must specify the expression for the aggregation type '" + type + "'.");
            }

            return String.Format(format, expression);
        }


        /// <summary>
        /// Gets the value expression from the given expression
        /// </summary>
        /// <param name="expression">Expression to wrap</param>
        public static string GetValueExpression(string expression)
        {
            return String.Format("({0})", expression);
        }


        /// <summary>
        /// Gets the parameter name
        /// </summary>
        /// <param name="name">Parameter name</param>
        public static string GetParameterName(string name)
        {
            // Do not process already parametrized names 
            if (name.StartsWith("@", StringComparison.Ordinal))
            {
                return name;
            }

            // Replace dot for multiple identifiers (e.g: table.column)
            name = name.Replace('.', '_');

            // Simple identifier
            if (ValidationHelper.IsIdentifier(name))
            {
                return "@" + name;
            }

            // Try remove square brackets and check identifier format
            var removedBrackets = RemoveSquareBrackets(name);
            if (ValidationHelper.IsIdentifier(removedBrackets))
            {
                return "@" + removedBrackets;
            }

            // Return default value
            return "@Value";
        }

        #endregion


        #region "Query methods"

        /// <summary>
        /// Checks if the given query type supports generating of the query text. If not, throws an exception
        /// </summary>
        /// <param name="type">Query type</param>
        /// <param name="queryName">Query name</param>
        /// <exception cref="NotSupportedException">When query type does not support generating of the query text</exception>
        internal static void CheckQueryTypeForTextGeneration(QueryTypeEnum type, string queryName)
        {
            // Stored procedures are not supported (yet)
            if (type == QueryTypeEnum.StoredProcedure)
            {
                throw new NotSupportedException("[DataQuery.CheckQueryTypeForTextGeneration]: Stored procedure query '" + queryName + "' cannot generate the query text.");
            }
        }


        /// <summary>
        /// Gets the join expression
        /// </summary>
        /// <param name="joinType">Join type</param>
        public static string GetJoinType(JoinTypeEnum joinType)
        {
            switch (joinType)
            {
                case JoinTypeEnum.RightOuter:
                    return "RIGHT OUTER JOIN";

                case JoinTypeEnum.LeftOuter:
                    return "LEFT OUTER JOIN";

                default:
                    return "INNER JOIN";
            }
        }


        /// <summary>
        /// Gets the join expression
        /// </summary>
        /// <param name="left">Left side source</param>
        /// <param name="right">Right side source</param>
        /// <param name="condition">Condition</param>
        /// <param name="joinType">Join type</param>
        public static string GetJoin(string left, string right, string condition, JoinTypeEnum joinType = JoinTypeEnum.Inner)
        {
            var result = String.Format("{0} {1} {2} ON {3}", left, GetJoinType(joinType), right, condition);

            return result;
        }

        /// <summary>
        /// Ensures full name of the given column
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <param name="columnName">Column name</param>
        public static string EnsureFullName(string tableName, string columnName)
        {
            if (!columnName.Contains(".") && !String.IsNullOrEmpty(tableName))
            {
                columnName = String.Format("{0}.{1}", tableName, columnName);
            }
            return columnName;
        }


        /// <summary>
        /// Gets a multiline SQL comment for simple input text (must not contain any open comment sequence)
        /// </summary>
        /// <remarks>
        /// Input text with comment sequence could cause an invalid SQL syntax.
        /// </remarks>
        /// <param name="text">Comment text</param>
        /// <param name="newLine">If true, new line is added before the comment</param>
        public static string GetComment(string text, bool newLine = true)
        {
            return (newLine ? "\r\n" : "") + "/* " + text + " */";
        }


        /// <summary>
        /// Removes single line and simple multiline comments from a SQL query
        /// </summary>
        /// <remarks>
        /// Multiline comments with nested comments won't be removed correctly and could cause an invalid SQL syntax
        /// </remarks>
        /// <param name="query">SQL query</param>
        public static string RemoveComments(string query)
        {
            query = CommentsRegEx.Replace(query, String.Empty);

            return query;
        }


        /// <summary>
        /// Returns true if two queries equal by their content. Ignores extra whitespaces and comments within the comparison
        /// </summary>
        /// <param name="query1">First query</param>
        /// <param name="query2">Second query</param>
        /// <param name="returnDifference">If true, the difference (remainders that don't match) are returned through original values</param>
        /// <exception cref="ArgumentNullException">When query parameter is null</exception>
        public static bool QueriesEqual(ref string query1, ref string query2, bool returnDifference = false)
        {
            if (query1 == null)
            {
                throw new ArgumentNullException("query1");
            }

            if (query2 == null)
            {
                throw new ArgumentNullException("query2");
            }

            query1 = StandardizeQuery(query1);
            query2 = StandardizeQuery(query2);

            return TextHelper.ContentEquals(ref query1, ref query2, returnDifference);
        }


        /// <summary>
        /// Standardizes a query so the spaces are standardized and comments are removed
        /// </summary>
        /// <param name="query">Query to standardize</param>
        /// <exception cref="ArgumentNullException">When query is null</exception>
        private static string StandardizeQuery(string query)
        {
            if (query == null)
            {
                throw new ArgumentNullException("query");
            }

            query = RemoveComments(query);

            var r = RegexHelper.GetRegex(@"\s*([()[\]])\s*", RegexOptions.Compiled);
            query = r.Replace(query, " $1 ");

            return query;
        }


        /// <summary>
        /// Returns the query created as an UNION ALL of given queries.
        /// </summary>
        /// <param name="queries">Queries to union</param>
        public static string UnionQueries(params string[] queries)
        {
            return UnionQueries(queries, true);
        }


        /// <summary>
        /// Returns the query created as an UNION of the given queries.
        /// </summary>
        /// <param name="queries">Queries to union</param>
        /// <param name="unionAll">Union all records (no distinct)</param>
        public static string UnionQueries(string[] queries, bool unionAll)
        {
            var op = unionAll ? SqlOperator.UNION_ALL : SqlOperator.UNION;

            return CombineQueries(queries, new[] { op });
        }


        /// <summary>
        /// Returns the query created as an INTERSECT of the given queries.
        /// </summary>
        /// <param name="queries">Queries to intersect</param>
        public static string IntersectQueries(params string[] queries)
        {
            return CombineQueries(queries, new[] { SqlOperator.INTERSECT });
        }


        /// <summary>
        /// Returns the query created as an EXCEPT of the given queries.
        /// </summary>
        /// <param name="queries">Queries to except</param>
        public static string ExceptQueries(params string[] queries)
        {
            return CombineQueries(queries, new[] { SqlOperator.EXCEPT });
        }


        /// <summary>
        /// Combines the given queries with the operators
        /// </summary>
        /// <param name="queries">Queries to merge</param>
        /// <param name="operators">Operators between queries, for lower number of operators than necessary uses the last operator for the remaining queries. Use one operator in case you want all operators to be the same.</param>
        public static string CombineQueries(IEnumerable<string> queries, IEnumerable<string> operators)
        {
            if (queries == null)
            {
                return "";
            }

            if (operators == null)
            {
                throw new ArgumentNullException(nameof(operators));
            }

            // Create the union from the given queries
            var sb = new StringBuilder();

            int addedQueries = 0;

            string firstQuery = "";

            // Get to the first operator
            var opEnum = operators.GetEnumerator();

            string op = null;

            // Process all queries
            foreach (string query in queries)
            {
                if (!String.IsNullOrEmpty(query))
                {
                    // Add new query
                    if (addedQueries == 0)
                    {
                        firstQuery = query;
                    }
                    else
                    {
                        sb.AppendLine();
                        sb.AppendLine(op);
                    }

                    sb.AppendLine("(");
                    sb.AppendLine(query);
                    sb.AppendLine(")");

                    addedQueries++;
                }

                // Adjust the operator to the next one in list
                if (opEnum.MoveNext())
                {
                    op = opEnum.Current;
                }
            }

            // For zero or no queries get only the first query
            if (addedQueries <= 1)
            {
                return firstQuery;
            }

            return sb.ToString();
        }


        /// <summary>
        /// Combines the given queries with the operator
        /// </summary>
        /// <param name="op">Operator between queries</param>
        /// <param name="query">Original query</param>
        /// <param name="append">Query to append</param>
        public static string AppendQuery(string query, string append, string op)
        {
            if (String.IsNullOrEmpty(append))
            {
                return query;
            }
            if (String.IsNullOrEmpty(query))
            {
                return append;
            }

            return String.Format("{0} {1} ({2})", query, op, append);
        }


        /// <summary>
        /// Preprocesses the give query.
        /// </summary>
        /// <param name="query">Query to preprocess</param>
        public static void PreprocessQuery(QueryParameters query)
        {
            // Handle partial updates
            if ((query.Name != null) && query.Name.EndsWith(".update", StringComparison.InvariantCultureIgnoreCase))
            {
                bool changed = false;
                StringBuilder queryText = null;

                // Process all parameters
                QueryDataParameters queryParams = query.Params;
                if (queryParams != null)
                {
                    foreach (DataParameter param in queryParams)
                    {
                        // Check for missing values
                        if (IsMissing(param.Value))
                        {
                            if (!String.IsNullOrEmpty(param.Name))
                            {
                                int index = query.Text.IndexOf(param.Name, StringComparison.InvariantCultureIgnoreCase);
                                if (index >= 0)
                                {
                                    // Remove parameters for missing values from the query (change to identity)
                                    if (!changed)
                                    {
                                        queryText = new StringBuilder(query.Text);
                                        changed = true;
                                    }

                                    queryText[index] = ' ';
                                }
                            }

                            param.Name = null;
                        }
                    }
                }

                // Assign new query text if changed
                if (changed)
                {
                    query.Text = queryText.ToString();
                }
            }
        }


        /// <summary>
        /// Prepares the query for paging, adds additional system columns and updates the columns list.
        /// </summary>
        /// <param name="queryText">Query text</param>
        /// <param name="macros">Query expressions</param>
        /// <param name="offset">Index of first record to get</param>
        /// <param name="maxRecords">Maximum number of records to get</param>
        /// <param name="getTotal">If true, the query should get the total number of records</param>
        /// <param name="subQuery">If true, the query is used as a sub-query</param>
        public static string PreparePagedQuery(string queryText, QueryMacros macros, int offset, int maxRecords, bool getTotal = true, bool subQuery = false)
        {
            var columns = macros.Columns;

            // Build the query
            if (String.IsNullOrEmpty(columns))
            {
                columns = "*";
            }

            HandleEmptyColumns(ref columns);

            var orderBy = macros.OrderBy;

            HandleEmptyColumns(ref orderBy);

            // Add the system columns for paging
            if (!String.IsNullOrEmpty(orderBy))
            {
                if (macros.TopN <= 0)
                {
                    // Remove Order by if TopN is not specified (cause exceptions)
                    queryText = SqlMacroHelper.RemoveOrderBy(queryText);
                }

                var originalColumns = columns;

                // Append row number
                string rowNumber = new RowNumberColumn(SystemColumns.ROW_NUMBER, orderBy).ToString();
                columns = AddColumns(columns, rowNumber);

                // Build the query text
                var rowsWhere = GetBetween(SystemColumns.ROW_NUMBER, offset + 1, offset + maxRecords);

                var totalColumn = (getTotal ? GetTotalColumn(macros.Total, subQuery) : null);

                if (subQuery)
                {
                    // Get query as sub-query
                    var cols = AddColumns(originalColumns, totalColumn);

                    queryText = PrepareNestedQuery(queryText, rowsWhere, null, 0, cols);
                }
                else
                {
                    // Get query with CTE
                    queryText = PrepareWithQuery(queryText, rowsWhere, SystemColumns.ROW_NUMBER, 0, totalColumn);
                }

                orderBy = null;
            }

            // Update fields in expressions
            macros.OrderBy = orderBy;
            macros.Columns = columns;

            return queryText;
        }


        /// <summary>
        /// Gets the total column expression
        /// </summary>
        /// <param name="totalExpression">Total expression</param>
        /// <param name="subQuery">If true, the query is a sub-query</param>
        private static string GetTotalColumn(string totalExpression, bool subQuery)
        {
            if (String.IsNullOrEmpty(totalExpression))
            {
                // Build the total expression if not provided explicitly
                totalExpression = "SELECT COUNT(*)";

                if (!subQuery)
                {
                    totalExpression += " FROM AllData";
                }
            }

            return AddColumnAlias(string.Format("({0})", totalExpression), SystemColumns.TOTAL_RECORDS);
        }


        /// <summary>
        /// Handles the empty columns constant by replacing it with empty string
        /// </summary>
        /// <param name="columns">Columns to process</param>
        public static void HandleEmptyColumns(ref string columns)
        {
            if (IsNoColumns(columns))
            {
                columns = "";
            }
        }


        /// <summary>
        /// Returns true if the given columns variable represents no columns
        /// </summary>
        /// <param name="columns">Columns</param>
        private static bool IsNoColumns(string columns)
        {
            return (columns == NO_COLUMNS);
        }


        /// <summary>
        /// Prepares a nested query using WITH for further evaluation
        /// </summary>
        /// <param name="queryText">Query text</param>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by expression</param>
        /// <param name="topN">Top N</param>
        /// <param name="extraColumns">Columns to get</param>
        private static string PrepareWithQuery(string queryText, string where, string orderBy, int topN, string extraColumns)
        {
            var result = GetWithQuery("{0}");

            var cols = AddColumns(COLUMNS_ALL, extraColumns);

            // Resolve macros in the query
            result = new QueryMacros
            {
                Where = where,
                OrderBy = orderBy,
                TopN = topN,
                Columns = cols
            }
                .ResolveMacros(result);

            return String.Format(result, queryText);
        }


        /// <summary>
        /// Gets a nested query using WITH for further evaluation
        /// </summary>
        private static string GetWithQuery(string queryText)
        {
            var result = String.Format(
@"
WITH AllData AS 
(
    {0}
) 
SELECT ##TOPN## ##COLUMNS## 
FROM AllData 
WHERE ##WHERE## 
ORDER BY ##ORDERBY##
", queryText);

            return result;
        }


        /// <summary>
        /// Prepares a nested query using WITH for further evaluation
        /// </summary>
        /// <param name="queryText">Query text</param>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by expression</param>
        /// <param name="topN">Top N</param>
        /// <param name="columns">Columns to get</param>
        private static string PrepareNestedQuery(string queryText, string where, string orderBy, int topN, string columns)
        {
            var result = GetNestedQuery("{0}");

            // Resolve macros in the query
            result = new QueryMacros
            {
                Where = where,
                OrderBy = orderBy,
                TopN = topN,
                Columns = columns
            }
                .ResolveMacros(result);

            return String.Format(result, queryText);
        }


        /// <summary>
        /// Gets a nested query using nested SELECT for further evaluation
        /// </summary>
        public static string GetNestedQuery(string queryText, string tableName = "SubData")
        {
            return GetSelectQuery(String.Format(
@"
(
    {0}
) 
AS {1}
"
                , queryText, tableName)
            );
        }


        /// <summary>
        /// Gets the general select query
        /// </summary>
        /// <param name="source">Query source</param>
        /// <param name="where">Where condition</param>
        public static string GetSelectQuery(string source, string where = null)
        {
            var query = SqlMacroHelper.ReplaceSource(GENERAL_SELECT, source, null);

            if (!String.IsNullOrEmpty(where))
            {
                query = SqlMacroHelper.ReplaceWhere(query, where, true);
            }

            return query;
        }


        /// <summary>
        /// Processes the page results - Removes the system columns and gets the total records number.
        /// </summary>
        /// <param name="ds">DataSet with the results</param>
        /// <param name="totalRecords">Returns the total records number</param>
        public static void ProcessPagedResults(DataSet ds, ref int totalRecords)
        {
            totalRecords = 0;

            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                // Get the total count if available
                foreach (DataTable dt in ds.Tables)
                {
                    if (dt.Columns.Contains(SystemColumns.TOTAL_RECORDS))
                    {
                        totalRecords += ValidationHelper.GetInteger(dt.Rows[0][SystemColumns.TOTAL_RECORDS], 0);
                        dt.Columns.Remove(SystemColumns.TOTAL_RECORDS);
                    }
                    else
                    {
                        totalRecords += DataHelper.GetItemsCount(ds);
                    }

                    // Remove the system columns
                    if (dt.Columns.Contains(SystemColumns.ROW_NUMBER))
                    {
                        dt.Columns.Remove(SystemColumns.ROW_NUMBER);
                    }
                }
            }
        }


        /// <summary>
        /// Returns statement for the CASE expressions. 
        /// </summary>
        /// <param name="cases">IEnumerable with KeyValuePair where key is a boolean expression (where condition) and value is result expression</param>
        /// <param name="elseCase">Expression for the else case</param>
        /// <param name="escapeString">Determines whether escape cases' values or not</param>
        public static string GetCase(IEnumerable<KeyValuePair<string, string>> cases, string elseCase = null, bool escapeString = true)
        {
            if (cases == null)
            {
                return null;
            }

            StringBuilder column = new StringBuilder("CASE ");

            // Add all cases
            foreach (var item in cases)
            {
                if (!String.IsNullOrEmpty(item.Key) && !String.IsNullOrEmpty(item.Value))
                {
                    column.AppendFormat("WHEN {0} THEN {1} ", item.Key, escapeString ? GetSafeQueryString(item.Value, false) : item.Value);
                }
            }

            if (!String.IsNullOrEmpty(elseCase))
            {
                column.AppendFormat("ELSE {0} ", elseCase);
            }
            column.Append("END");

            return column.ToString();
        }


        /// <summary>
        /// Gets the convert expression.
        /// </summary>
        /// <param name="value">Value to be converted</param>
        /// <param name="type">Type the value should be converted to</param>
        /// <example>
        ///     <code>
        ///         GetConvert("ActivityValue", "INT")
        ///     </code>
        /// returns CONVERT(INT, ActivityValue)
        /// </example>
        public static string GetConvert(string value, string type)
        {
            return String.Format("CONVERT({0}, {1})", type, value);
        }


        /// <summary>
        /// Gets the ISNUMERIC expression.
        /// </summary>
        /// <example>
        ///     <code>
        ///     GetIsNumeric("ActivityValue")
        ///     </code>
        /// returns ISNUMERIC(ActivityValue)
        /// </example>
        /// <param name="value">Value to be checked</param>
        public static string GetIsNumeric(string value)
        {
            return String.Format("ISNUMERIC({0})", value);
        }


        /// <summary>
        /// Checks if the given query is valid to be used as paged query
        /// </summary>
        /// <param name="query">Query parameters</param>
        internal static void CheckPagedQuery(QueryParameters query)
        {
            if (query.Type == QueryTypeEnum.StoredProcedure)
            {
                throw new NotSupportedException("[SqlHelper.ValidatePagedQuery]: Paging is not supported for stored procedure query '" + query.Name + "'.");
            }
            if ((query.Macros == null) || String.IsNullOrEmpty(query.Macros.OrderBy))
            {
                throw new NotSupportedException("[SqlHelper.ValidatePagedQuery]: The paged query '" + query.Name + "' cannot be executed without the orderBy set.");
            }
        }


        /// <summary>
        /// Returns order by statement based on case boolean expressions. First case has highest priority.
        /// </summary>
        /// <param name="cases">IEnumerable with boolean expressions (where conditions)</param>
        public static string GetCaseOrderBy(params string[] cases)
        {
            return GetCaseOrderBy((IEnumerable<string>)cases);
        }


        /// <summary>
        /// Returns order by statement based on case boolean expressions. First case has highest priority.
        /// </summary>
        /// <param name="cases">IEnumerable with boolean expressions (where conditions)</param>
        public static string GetCaseOrderBy(IEnumerable<string> cases)
        {
            if (cases == null)
            {
                return null;
            }

            int index = 1;

            var casesDictionary = new Dictionary<string, string>();

            foreach (string item in cases)
            {
                if (!String.IsNullOrEmpty(item))
                {
                    casesDictionary.Add(item, (index++).ToString());
                }
            }

            return GetCase(casesDictionary, index.ToString());
        }


        /// <summary>
        /// Gets a table-valued parameter for database calls. Is intended for usage with [Type_CMS_IntegerTable] database type.
        /// </summary>
        /// <remarks>
        /// Returns empty enumeration if there are no values provided. The database doesn't ever expect to get empty table, 
        /// it needs not to add the parameter at all. So when calling this method to add an parameter to a query, 
        /// check this for an empty enumeration at first and do not add the parameter at all in that case.
        /// </remarks>
        public static IEnumerable<SqlDataRecord> BuildIntTable(IEnumerable<int> integers)
        {
            if (integers == null)
            {
                throw new ArgumentNullException(nameof(integers));
            }

            var metaData = new SqlMetaData[1];
            metaData[0] = new SqlMetaData("Value", SqlDbType.Int);

            using (var enumerator = integers.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    var record = new SqlDataRecord(metaData);
                    record.SetInt32(0, enumerator.Current);

                    yield return record;
                }
            }
        }


        /// <summary>
        /// Gets a table-valued parameter for database calls. Is intended for usage with [Type_CMS_OrderedIntegerTable] database type.
        /// </summary>
        /// <remarks>
        /// Returns empty enumeration if there are no values provided. The database doesn't ever expect to get empty table, 
        /// it needs not to add the parameter at all. So when calling this method to add an parameter to a query, 
        /// check this for an empty enumeration at first and do not add the parameter at all in that case.
        /// </remarks>
        public static IEnumerable<SqlDataRecord> BuildOrderedIntTable(IEnumerable<int> integers)
        {
            if (integers == null)
            {
                throw new ArgumentNullException(nameof(integers));
            }

            return BuildIntTable(integers.OrderBy(x => x));
        }


        /// <summary>
        /// Gets a table-valued parameter for database calls. Is intended for usage with [Type_CMS_BigIntTable] database type.
        /// </summary>
        /// <remarks>
        /// Returns empty enumeration if there are no values provided. The database doesn't ever expect to get empty table, 
        /// it needs not to add the parameter at all. So when calling this method to add an parameter to a query, 
        /// check this for an empty enumeration at first and do not add the parameter at all in that case.
        /// </remarks>
        public static IEnumerable<SqlDataRecord> BuildBigIntTable(IEnumerable<long> longs)
        {
            if (longs == null)
            {
                throw new ArgumentNullException(nameof(longs));
            }

            var metaData = new SqlMetaData[1];
            metaData[0] = new SqlMetaData("Value", SqlDbType.BigInt);

            using (var enumerator = longs.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    var record = new SqlDataRecord(metaData);
                    record.SetInt64(0, enumerator.Current);

                    yield return record;
                }
            }
        }


        /// <summary>
        /// Gets a table-valued parameter for database calls. Is intended for usage with [Type_CMS_GuidTable] database type.
        /// </summary>
        /// <remarks>
        /// Returns empty enumeration if there are no values provided. The database doesn't ever expect to get empty table, 
        /// it needs not to add the parameter at all. So when calling this method to add an parameter to a query, 
        /// check this for an empty enumeration at first and do not add the parameter at all in that case.
        /// </remarks>
        public static IEnumerable<SqlDataRecord> BuildGuidTable(IEnumerable<Guid> guids)
        {
            if (guids == null)
            {
                throw new ArgumentNullException(nameof(guids));
            }

            var metaData = new SqlMetaData[1];
            metaData[0] = new SqlMetaData("Value", SqlDbType.UniqueIdentifier);

            using (var enumerator = guids.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    var record = new SqlDataRecord(metaData);
                    record.SetGuid(0, enumerator.Current);

                    yield return record;
                }
            }
        }


        /// <summary>
        /// Gets a table-valued parameter for database calls. Is intended for usage with [Type_CMS_StringTable] database type.
        /// </summary>
        /// <remarks>
        /// Returns empty enumeration if there are no values provided. The database doesn't ever expect to get empty table, 
        /// it needs not to add the parameter at all. So when calling this method to add an parameter to a query, 
        /// check this for an empty enumeration at first and do not add the parameter at all in that case.
        /// </remarks>
        public static IEnumerable<SqlDataRecord> BuildStringTable(IEnumerable<string> strings)
        {
            if (strings == null)
            {
                throw new ArgumentNullException(nameof(strings));
            }

            var metaData = new SqlMetaData[1];
            metaData[0] = new SqlMetaData("Value", SqlDbType.NVarChar, 450);

            using (var enumerator = strings.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    var record = new SqlDataRecord(metaData);
                    record.SetString(0, enumerator.Current);

                    yield return record;
                }
            }
        }

        #endregion


        #region "Where condition methods"

        /// <summary>
        /// Gets the condition matching values of the column between from and to
        /// </summary>
        /// <param name="column">Column name</param>
        /// <param name="from">From value</param>
        /// <param name="to">To value</param>
        public static string GetBetween(string column, int from, int to)
        {
            return String.Format("{0} BETWEEN {1} AND {2}", column, from, to);
        }


        /// <summary>
        /// Adds where condition to the expression.
        /// </summary>
        /// <param name="where">Where condition</param>
        /// <param name="condition">Condition to add</param>
        /// <param name="op">Operator, no spaces required. e.g. "OR"</param>
        public static string AddWhereCondition(string where, string condition, string op)
        {
            // If condition present, add
            if (!String.IsNullOrEmpty(condition))
            {
                // Add and if previous condition not empty
                if (!String.IsNullOrEmpty(where))
                {
                    if (where != condition)
                    {
                        where = "(" + where + ") " + op + " (" + condition + ")";
                    }
                }
                else
                {
                    where = condition;
                }
            }

            return where;
        }


        /// <summary>
        /// Adds where condition to the expression using AND operator.
        /// </summary>
        /// <param name="where">Where condition</param>
        /// <param name="condition">Condition to add</param>
        public static string AddWhereCondition(string where, string condition)
        {
            return AddWhereCondition(where, condition, "AND");
        }


        /// <summary>
        /// Returns where condition in SQL syntax for collection of items.
        /// </summary>
        /// <remarks>The following rules are applied:
        /// <list type="bullet">
        /// <item><description>Duplicate values in collection are ignored</description></item>
        /// <item><description>Duplicate values are compared with case sensitivity</description></item>
        /// <item><description>Null or empty collection generates simplified where condition with dependence on negation parameter ("0 = 1"/"1 = 1")</description></item>
        /// <item><description>Collection with single item results in condition with equality comparer (depend on negation parameter e.g.: "="/"&lt;&gt;" instead of "IN"/"NOT IN")</description></item>
        /// <item><description>Single null value (depends on allowNullValue parameter) results in NULL equality comparer ("IS" or "IS NOT")</description></item>
        /// <item><description>Unicode prefix is automatically added for string values (N'Text')</description></item>
        /// <item><description>This method cannot be used for <see cref="System.Data.DataTable"/> select condition.</description></item>
        /// </list>
        /// </remarks>
        /// <example>
        /// <code>
        /// string whereCondition = GetWhereInCondition("ColumName", new List&lt;int&gt;() { 1, 10 , 50}, true);
        /// </code>
        /// Output:
        /// "ColumnName NOT IN (1, 10, 50)"
        /// </example>
        /// <typeparam name="T">Define value type. Only <see cref="System.Int32"/>, <see cref="System.String"/> or <see cref="System.Guid"/> are supported. Other types are considered as strings and could cause an invalid SQL syntax</typeparam>
        /// <param name="columnName">Column name. The column name is not encapsulated or escaped. Proper format must be ensured outside of this method.</param>
        /// <param name="values">Collection of values. Null or empty values generates simple where condition with dependence on negation parameter (0 = 1 or  1 = 1) </param>
        /// <param name="negation">Indicates whether "NOT IN"/"&lt;&gt;" should be used</param>
        /// <param name="allowNullValue">Indicates whether null values should be considered as database NULL value type</param>
        public static string GetWhereInCondition<T>(string columnName, ICollection<T> values, bool negation, bool allowNullValue)
        {
            return GetWhereConditionInternal(columnName, values, (typeof(T) == typeof(string)), negation, false, allowNullValue);
        }


        /// <summary>
        /// Creates the where condition for the array of values.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="values">Values</param>
        /// <param name="negation">Indicates if NOT IN should be used</param>
        public static string GetWhereCondition(string columnName, IEnumerable<string> values, bool negation = false)
        {
            return GetWhereConditionInternal(columnName, values, true, negation, true);
        }


        /// <summary>
        /// Creates the where condition for the array of values.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="values">Values</param>
        /// <param name="negation">Indicates if NOT IN should be used</param>
        public static string GetWhereCondition(string columnName, IEnumerable<int> values, bool negation = false)
        {
            return GetWhereConditionInternal(columnName, values, true, negation, true);
        }


        /// <summary>
        /// Creates the where condition for the array of values.
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="columnName">Column name</param>
        /// <param name="values">Values</param>
        /// <param name="useUnicode">Indicates if the preposition 'N' should be used for string values</param>
        /// <param name="negation">Indicates if NOT IN should be used</param>
        public static string GetWhereCondition<T>(string columnName, IEnumerable<string> values, bool useUnicode, bool negation = false)
        {
            return GetWhereConditionInternal(columnName, values, useUnicode, negation, true, false, typeof(T));
        }


        /// <summary>
        /// Creates the where condition for the array of values.
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="columnName">Column name</param>
        /// <param name="values">Values</param>
        /// <param name="useUnicode">Indicates if the preposition 'N' should be used for string values</param>
        /// <param name="negation">Indicates if NOT IN should be used</param>
        /// <param name="memoryDataSet">Indicates whether where condition format is for filter or SQL syntax</param>
        /// <param name="allowNullValue">Indicates whether null values should be considered as database NULL value type</param>
        /// <param name="specificType">Allows set other type than defined by generic value (backward compatibility)</param>
        private static string GetWhereConditionInternal<T>(string columnName, IEnumerable<T> values, bool useUnicode, bool negation, bool memoryDataSet, bool allowNullValue = false, Type specificType = null)
        {
            // Null input 
            // => 
            // return simplified result (no-data/all-data) with dependence on condition polarity (negation)    
            if (values == null)
            {
                if (negation)
                {
                    return ALL_DATA_WHERE;
                }
                return NO_DATA_WHERE;
            }

            // Create list of items and remove null values if it's required
            var list = values.Where(c => allowNullValue || (c != null)).ToList();
            using (var enumerator = list.Distinct().GetEnumerator())
            {
                // Empty input 
                // => 
                // return simplified result (no-data/all-data) with dependence on condition polarity (negation)    
                if (!enumerator.MoveNext())
                {
                    if (negation)
                    {
                        return ALL_DATA_WHERE;
                    }
                    return NO_DATA_WHERE;
                }

                var sb = new StringBuilder();
                var hasMany = false;

                // Append first value
                sb.Append(GetWhereConditionStringValue<T>(enumerator.Current, useUnicode, memoryDataSet, specificType));
                while (enumerator.MoveNext())
                {
                    // Add comma to other values - they will be used in IN SQL statement
                    sb.Append(", " + GetWhereConditionStringValue<T>(enumerator.Current, useUnicode, memoryDataSet, specificType));
                    hasMany = true;
                }

                if (hasMany)
                {
                    string op = negation ? "NOT IN" : "IN";
                    return columnName + " " + op + " (" + sb + ")";
                }
                else
                {
                    string op;
                    var value = sb.ToString();

                    //  Ensure operator with dependence on single value
                    if (value == "NULL")
                    {
                        op = negation ? "IS NOT" : "IS";
                    }
                    else
                    {
                        op = negation ? "<>" : "=";
                    }

                    return columnName + " " + op + " " + value;
                }
            }
        }


        /// <summary>
        /// Gets where condition value.
        /// </summary>
        /// <param name="value">Where condition value</param>
        /// <param name="useUnicode">Indicates if the preposition 'N' should be used</param>
        /// <param name="memoryDataSet">Indicates whether where condition format is for filter or SQL syntax</param>
        /// <param name="specificType">Allows set other type than defined by generic value (Backward compatibility)</param>
        private static string GetWhereConditionStringValue<T>(object value, bool useUnicode, bool memoryDataSet, Type specificType = null)
        {
            if (value == null)
            {
                return "NULL";
            }

            string result;
            Type requiredType = specificType ?? typeof(T);

            // Do conversion or enclose value to secure query
            if (requiredType == typeof(int))
            {
                result = ValidationHelper.GetString(ValidationHelper.GetInteger(value, 0), String.Empty);
            }
            else if (requiredType == typeof(long))
            {
                result = ValidationHelper.GetString(ValidationHelper.GetLong(value, 0), String.Empty);
            }
            else if (requiredType == typeof(double))
            {
                result = ValidationHelper.GetString(ValidationHelper.GetDouble(value, 0), String.Empty);
            }
            else if (requiredType == typeof(decimal))
            {
                result = ValidationHelper.GetString(ValidationHelper.GetDecimal(value, 0), String.Empty);
            }
            else if (requiredType == typeof(Guid))
            {
                if (memoryDataSet)
                {
                    result = String.Format("Convert('{0}', 'System.Guid')", ValidationHelper.GetGuid(value, Guid.Empty));
                }
                else
                {
                    result = String.Format("'{0}'", ValidationHelper.GetGuid(value, Guid.Empty));
                }
            }
            else if (requiredType == typeof(bool))
            {
                result = ValidationHelper.GetString((ValidationHelper.GetBoolean(value, false) ? 1 : 0), String.Empty);
            }
            // Safest way - enclose value with apostrophes
            else
            {
                result = GetSafeQueryString(Convert.ToString(value), false);
                result = "'" + result + "'";

                if (useUnicode)
                {
                    result = "N" + result;
                }
            }

            return result;
        }


        /// <summary>
        /// Escapes characters for query which use LIKE pattern.
        /// </summary>
        /// <param name="input">Input text</param>
        /// <param name="escapeUnderScore">Indicates whether underscore character should be escaped</param>
        /// <param name="escapePercentage">Indicates whether percentage character should be escaped</param>
        /// <param name="escapeSquareBrackets">Indicates whether square brackets characters should be escaped</param>
        public static string EscapeLikeQueryPatterns(string input, bool escapeUnderScore = true, bool escapePercentage = true, bool escapeSquareBrackets = true)
        {
            if (!String.IsNullOrEmpty(input))
            {
                // Square brackets
                if (escapeSquareBrackets)
                {
                    input = input.Replace("[", "[[]");
                }

                // Percentage
                if (escapePercentage)
                {
                    input = input.Replace("%", "[%]");
                }

                // Underscore
                if (escapeUnderScore)
                {
                    input = input.Replace("_", "[_]");
                }
            }

            return input;
        }


        /// <summary>
        /// Returns safe sql query string - escapes apostrophes and escapes wildcard characters _, %, [].
        /// </summary>
        /// <param name="input">String to escape</param>
        public static string GetSafeQueryString(string input)
        {
            return GetSafeQueryString(input, true);
        }


        /// <summary>
        /// Returns safe sql query string - escapes apostrophes and optionally escapes wildcard characters _, %, [].
        /// </summary>
        /// <param name="input">String to escape</param>
        /// <param name="escapeWildcards">Determines whether the wildcards characters should be escaped</param>        
        public static string GetSafeQueryString(string input, bool escapeWildcards)
        {
            if (!String.IsNullOrEmpty(input))
            {
                // Replace apostrophe
                input = EscapeQuotes(input);

                // Replace wildcards
                if (escapeWildcards)
                {
                    input = EscapeLikeText(input);
                }
            }
            return input;
        }


        /// <summary>
        /// Gets the value representation for a SQL query text
        /// </summary>
        /// <param name="value">Value</param>
        public static string GetSqlValue(object value)
        {
            var collection = value as IEnumerable<SqlDataRecord>;
            return collection != null
                    ? GetDataRecordSqlValue(collection)
                    : DataTypeManager.GetSqlValue(value);
        }


        /// <summary>
        /// Gets the value representation for a SQL query text retrieved from collection of <see cref="SqlDataRecord"/>
        /// </summary>
        private static string GetDataRecordSqlValue(IEnumerable<SqlDataRecord> collection)
        {
            var values = collection.Select(v => v.GetSqlValue(0)).Select(i => string.Format("({0})", DataTypeManager.GetSqlValue(i)));
            return string.Format("({0} {1}) AS value(val)", SQL_VALUES, string.Join(", ", values));
        }


        /// <summary>
        /// Escapes characters for query which use LIKE pattern (%, _, [, ] and ^).
        /// </summary>
        /// <param name="text">Original input</param>
        /// <returns>The escaped string that can be used as pattern in a LIKE expression</returns>
        public static string EscapeLikeText(string text)
        {
            return EscapeLikeQueryPatterns(text);
        }


        /// <summary>
        /// Escapes single quotes in string value used for SQL query (value's => value''s).
        /// </summary>
        /// <param name="text">Original input text</param>
        /// <returns>The escaped string that can be used as safe string in SQL query</returns>
        public static string EscapeQuotes(string text)
        {
            if (!String.IsNullOrEmpty(text))
            {
                return text.Replace("'", "''");
            }
            return text;
        }

        #endregion


        #region "Columns methods"

        /// <summary>
        /// Adds the column alias to the given expression
        /// </summary>
        /// <param name="expression">Expression</param>
        /// <param name="alias">Alias</param>
        /// <param name="ensureBrackets">If true, the expression is encapsulated in brackets in case it is a complex expression</param>
        public static string AddColumnAlias(string expression, string alias, bool ensureBrackets = true)
        {
            expression = expression.Trim();

            // Ensure brackets for complex expressions
            if (ensureBrackets && (!expression.StartsWith("(", StringComparison.Ordinal) || !expression.EndsWith(")", StringComparison.Ordinal)) && !ValidationHelper.IsCodeName(expression))
            {
                expression = String.Format("({0})", expression);
            }

            return String.Format("{0} AS {1}", expression, AddSquareBrackets(alias));
        }


        /// <summary>
        /// Returns statement for column with case expressions. 
        /// This statement is used for evaluating a set of boolean expressions to determine the result. First case has highest priority.
        /// </summary>
        /// <param name="cases">IEnumerable with KeyValuePair where key is a boolean expression (where condition) and value is result expression</param>
        /// <param name="asColumnName">Specifies column name</param>
        public static string GetCaseColumn(IEnumerable<KeyValuePair<string, string>> cases, string asColumnName = "CaseColumn")
        {
            if (cases == null)
            {
                return null;
            }

            var casesDictionary = new Dictionary<string, string>();
            foreach (var item in cases)
            {
                if (!String.IsNullOrEmpty(item.Key) && !String.IsNullOrEmpty(item.Value))
                {
                    casesDictionary.Add(item.Key, String.Format("'{0}'", GetSafeQueryString(item.Value, false)));
                }
            }

            string result = GetCase(casesDictionary, null, false);

            // Ensure alias for the column
            if (!String.IsNullOrEmpty(asColumnName))
            {
                return AddColumnAlias(result, asColumnName, false);
            }

            return result;
        }


        /// <summary>
        /// Adds the columns.
        /// </summary>
        /// <param name="columns">Original columns</param>
        /// <param name="addColumns">Columns to add</param>
        /// <param name="treatEmptyAsAll">If true, empty source columns are treated as all columns</param>
        public static string AddColumns(string columns, string addColumns, bool treatEmptyAsAll = false)
        {
            if (!String.IsNullOrEmpty(addColumns) && (addColumns != NO_COLUMNS))
            {
                if (!String.IsNullOrEmpty(columns))
                {
                    // Add some columns
                    if (IsNoColumns(columns))
                    {
                        columns = "";
                    }
                    else
                    {
                        columns += ", ";
                    }
                }
                else if (treatEmptyAsAll)
                {
                    columns = "*, ";
                }

                columns += addColumns;
            }

            return columns;
        }


        /// <summary>
        /// Merges the sets of columns and makes sure that each column in the result is present only once.
        /// </summary>
        /// <param name="columns">Original column list</param>
        /// <param name="addColumns">List of columns to add</param>
        /// <param name="uniqueKey">Function which provides unique key for the merging process (if two column keys match, the merging process allows only first column)</param>
        /// <param name="extraColumns">Indicates if columns which are not part of '*' expression are merged to the existing list of columns</param>
        /// <param name="transformation">Column transformation</param>
        /// <returns>Returns the list separated by dashes for use in SQL query</returns>
        public static string MergeColumns(string columns, string addColumns, Func<string, string> uniqueKey = null, bool extraColumns = true, Func<string, string> transformation = null)
        {
            var columnList = ParseColumnList(columns);

            return MergeColumns(columnList, new[] { addColumns }, uniqueKey, extraColumns, transformation);
        }


        /// <summary>
        /// Merges the sets of columns and makes sure that each column in the result is present only once.
        /// </summary>
        /// <param name="columns">Original column list</param>
        /// <param name="addColumns">List of columns to add</param>
        /// <param name="uniqueKey">Function which provides unique key for the merging process (if two column keys match, the merging process allows only first column)</param>
        /// <param name="extraColumns">Indicates if columns which are not part of '*' expression are merged to the existing list of columns</param>
        /// <param name="transformation">Column transformation</param>
        /// <returns>Returns the list separated by dashes for use in SQL query</returns>
        public static string MergeColumns(IEnumerable<string> columns, IEnumerable<string> addColumns = null, Func<string, string> uniqueKey = null, bool extraColumns = true, Func<string, string> transformation = null)
        {
            var result = new List<string>();
            var added = new HashSet<string>();

            var noColumns = false;

            // Add the initial column list
            AddColumnList(result, added, columns, ref noColumns, uniqueKey);

            // Create initial column list
            if ((addColumns != null) && (extraColumns || !added.Contains(COLUMNS_ALL)))
            {
                // Merge all items
                foreach (string addCols in addColumns)
                {
                    if (!String.IsNullOrEmpty(addCols))
                    {
                        // Get list of columns from string
                        var splitCols = ParseColumnList(addCols);

                        // Apply column transformation
                        if (transformation != null)
                        {
                            for (int i = 0; i < splitCols.Count; i++)
                            {
                                splitCols[i] = transformation(splitCols[i]);
                            }
                        }

                        AddColumnList(result, added, splitCols, ref noColumns, uniqueKey);
                    }
                }
            }

            // Return no columns in case the only member was no columns
            if ((result.Count == 0) && noColumns)
            {
                return NO_COLUMNS;
            }

            return JoinColumnList(result);
        }


        /// <summary>
        /// Adds the given list of columns to the existing list
        /// </summary>
        /// <param name="columns">List of columns</param>
        /// <param name="added">Added columns</param>
        /// <param name="addCols">Columns to add</param>
        /// <param name="noColumns">Returns the flag whether some columns were added</param>
        /// <param name="uniqueKey">Unique column key function to identify duplicate columns. If not specified, matches the whole column expression.</param>
        private static void AddColumnList(List<string> columns, HashSet<string> added, IEnumerable<string> addCols, ref bool noColumns, Func<string, string> uniqueKey)
        {
            if (addCols != null)
            {
                // Process all columns
                foreach (string colName in addCols)
                {
                    if (colName != null)
                    {
                        bool isNoColumns = IsNoColumns(colName);

                        if ((colName != ObjectTypeInfo.COLUMN_NAME_UNKNOWN) && !isNoColumns)
                        {
                            // Add only columns which are known
                            var key = GetColumnName(colName).ToLowerInvariant();
                            if (uniqueKey != null)
                            {
                                key = uniqueKey(colName);
                            }
                            // Do not use square brackets within the key
                            else if (IsEncapsulatedBySquareBrackets(key))
                            {
                                key = RemoveSquareBrackets(key);
                            }

                            if (!added.Contains(key))
                            {
                                columns.Add(colName);
                                added.Add(key);
                            }
                        }

                        noColumns |= isNoColumns;
                    }
                }
            }
        }


        /// <summary>
        /// Gets the column name from the given SQL column expression
        /// </summary>
        /// <param name="column">Column to parse</param>
        public static string GetColumnName(string column)
        {
            string expression;
            string alias;

            ParseColumn(column, out expression, out alias);

            return alias ?? expression;
        }


        /// <summary>
        /// Gets the column name from the given SQL column expression
        /// </summary>
        /// <param name="column">Column to parse</param>
        /// <param name="expression">Column expression</param>
        /// <param name="alias">Returns column alias or null in case column does not have any alias</param>
        public static void ParseColumn(string column, out string expression, out string alias)
        {
            if (column != null)
            {
                column = column.Trim();
            }

            expression = column;
            alias = null;

            if (column != null)
            {
                // Skip column alias parsing for simple column values due to  better performance
                if (SimpleColumnRegex.Match(column).Success)
                {
                    return;
                }

                // Find the alias expression
                var match = ColumnAliasRegEx.Match(column);

                if (match.Success)
                {
                    var aliasFirst = match.Groups[1];
                    if (aliasFirst.Success)
                    {
                        // Expression AS Alias
                        alias = aliasFirst.Value.Trim();
                        expression = column.Substring(match.Index + match.Length).Trim();
                    }
                    else
                    {
                        // Alias = Expression
                        var aliasLast = match.Groups[2];
                        if (aliasLast.Success)
                        {
                            alias = aliasLast.Value.Trim();
                            expression = column.Substring(0, match.Index).Trim();
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Joins the given column list. Columns are separated by comma.
        /// </summary>
        /// <param name="columns">Columns to add</param>
        public static string JoinColumnList(IEnumerable<string> columns)
        {
            return columns.Join(", ");
        }


        /// <summary>
        /// Parses the given list of columns to a list (can handle also advanced columns containing functions like ISNULL(A, B) etc.).
        /// If <paramref name="columns"/> contains <see cref="NO_COLUMNS"/>, then <see cref="NO_COLUMNS"/> won't be present in result list.
        /// </summary>
        /// <param name="columns">List of columns separated with commas</param>
        public static List<string> ParseColumnList(string columns)
        {
            return ParseColumnList(columns, false);
        }


        /// <summary>
        /// Parses the given list of columns to a list (can handle also advanced columns containing functions like ISNULL(A, B) etc.).
        /// If <paramref name="columns"/> contains <see cref="NO_COLUMNS"/>, then <see cref="NO_COLUMNS"/> will be present in result list.        
        /// </summary>
        /// <param name="columns">List of columns separated with commas</param>
        internal static List<string> ParseColumnListEvenNoColumnMacro(string columns)
        {
            return ParseColumnList(columns, false, true);
        }


        /// <summary>
        /// Parses the given list of columns to a list (can handle also advanced columns containing functions like ISNULL(A, B) etc.).
        /// </summary>
        /// <param name="columns">List of columns separated with commas</param>
        /// <param name="removeSquareBrackets">Indicates whether square brackets should be removed</param>
        /// <param name="returnNoColumnsMacro">Indicates if the <see cref="NO_COLUMNS"/> in <paramref name="columns"/> will be present in result list, by default no</param>
        internal static List<string> ParseColumnList(string columns, bool removeSquareBrackets, bool returnNoColumnsMacro = false)
        {
            var result = new List<string>();

            if (columns != null)
            {
                var bracketsCount = 0;
                var insideSquareBracket = false;
                var insideApostrophe = false;
                var lastIndex = 0;

                for (int i = 0; i < columns.Length; i++)
                {
                    switch (columns[i])
                    {
                        case '(':
                            // Ignore brackets inside square brackets or apostrophes
                            if (!insideSquareBracket && !insideApostrophe)
                            {
                                bracketsCount++;
                            }
                            break;

                        case ')':
                            // Ignore brackets inside square brackets or apostrophes
                            if (!insideSquareBracket && !insideApostrophe)
                            {
                                bracketsCount--;
                            }
                            break;

                        case '[':
                            // Ignore square brackets inside apostrophes
                            if (!insideApostrophe)
                            {
                                insideSquareBracket = true;
                            }
                            break;

                        case ']':
                            // Ignore square brackets inside apostrophes
                            if (insideApostrophe)
                            {
                                break;
                            }

                            var escaped = (i < columns.Length - 1) && (columns[i + 1] == ']');
                            if (!escaped)
                            {
                                // End of square brackets - not escaped one
                                insideSquareBracket = false;
                            }
                            else
                            {
                                // If escaped, skip the next bracket
                                i++;
                            }
                            break;

                        case '\'':
                            // Ignore apostrophe inside square brackets
                            if (insideSquareBracket)
                            {
                                break;
                            }

                            // We don't have to deal with escaped apostrophes because they use the same opening and closing character.
                            insideApostrophe = !insideApostrophe;
                            break;

                        case ',':
                            if ((bracketsCount == 0) && !insideSquareBracket && !insideApostrophe)
                            {
                                var item = GetColumnListItem(columns.Substring(lastIndex, i - lastIndex), returnNoColumnsMacro);
                                if (!String.IsNullOrEmpty(item))
                                {
                                    // Remove square brackets 
                                    if (removeSquareBrackets)
                                    {
                                        item = RemoveSquareBrackets(item);
                                    }
                                    result.Add(item);
                                }
                                lastIndex = i + 1;
                            }
                            break;
                    }
                }

                // Add the last item which remained
                var lastItem = GetColumnListItem(columns.Substring(lastIndex, columns.Length - lastIndex), returnNoColumnsMacro);
                if (!String.IsNullOrEmpty(lastItem))
                {
                    // Remove square brackets 
                    if (removeSquareBrackets)
                    {
                        lastItem = RemoveSquareBrackets(lastItem);
                    }

                    result.Add(lastItem);
                }
            }

            return result;
        }


        /// <summary>
        /// Returns true if column is encapsulated by square brackets, otherwise return false
        /// </summary>
        /// <param name="column">Column name</param>
        internal static bool IsEncapsulatedBySquareBrackets(string column)
        {
            return column.StartsWith("[", StringComparison.Ordinal) && column.EndsWith("]", StringComparison.Ordinal);
        }


        /// <summary>
        /// Removes square brackets from column name
        /// </summary>
        /// <param name="column">Column name</param>
        internal static string RemoveSquareBrackets(string column)
        {
            if (!String.IsNullOrEmpty(column))
            {
                if (column.Contains(SQL_OBJECT_NAME_DELIMITER))
                {
                    return RemoveMultipleSquareBrackets(column);
                }

                if (IsEncapsulatedBySquareBrackets(column))
                {
                    column = column.Substring(1, column.Length - 2);
                }
            }

            return column;
        }


        /// <summary>
        /// Adds square brackets to column name. Checks whether bracket are not already added
        /// </summary>
        /// <param name="column"></param>
        internal static string AddSquareBrackets(string column)
        {
            if (!String.IsNullOrEmpty(column))
            {
                if (column.Contains(SQL_OBJECT_NAME_DELIMITER))
                {
                    return AddMultipleSquareBrackets(column);
                }

                if (!IsEncapsulatedBySquareBrackets(column))
                {
                    column = String.Format("[{0}]", column.Replace("]", "]]"));
                }
            }

            return column;
        }


        /// <summary>
        /// Removes square brackets in combination of table name and column name.
        /// For example: [DatabaseName].[SchemaName].[TableName].[ColumnName]
        /// </summary>
        /// <returns>
        /// Expression where each part has stripped brackets
        /// For example: DatabaseName.SchemaName.TableName.ColumnName
        /// </returns>
        private static string RemoveMultipleSquareBrackets(string objectExpression)
        {
            var cols = objectExpression.Split(new[] { SQL_OBJECT_NAME_DELIMITER }, StringSplitOptions.RemoveEmptyEntries);
            return cols.Select(RemoveSquareBrackets).Join(SQL_OBJECT_NAME_DELIMITER.ToString(CultureInfo.InvariantCulture));
        }


        /// <summary>
        /// Adds square brackets to combination of table name and column name.
        /// For example: DatabaseName.SchemaName.TableName.ColumnName
        /// </summary>
        /// <returns>
        /// Expression where each part is inside the brackets
        /// For example: [DatabaseName].[SchemaName].[TableName].[ColumnName]
        /// </returns>
        private static string AddMultipleSquareBrackets(string objectExpression)
        {
            var cols = objectExpression.Split(new[] { SQL_OBJECT_NAME_DELIMITER }, StringSplitOptions.RemoveEmptyEntries);
            return cols.Select(AddSquareBrackets).Join(SQL_OBJECT_NAME_DELIMITER.ToString(CultureInfo.InvariantCulture));
        }


        /// <summary>
        /// Transforms the item of the column list
        /// </summary>
        /// <param name="item">Item to translate</param>
        /// <param name="returnNoColumns">Indicates if the <see cref="NO_COLUMNS"/> as <paramref name="item"/> will be returned as column, by default no</param>
        private static string GetColumnListItem(string item, bool returnNoColumns = false)
        {
            if (item == null)
            {
                return null;
            }

            item = item.Trim();

            // Transform no columns constant into an empty column
            if (IsNoColumns(item) && !returnNoColumns)
            {
                return null;
            }

            return item;
        }


        /// <summary>
        /// Ensures the missing columns in the given collection of <see cref="QueryColumnList"/> instances
        /// and removes all occurrences of all columns selector in them
        /// </summary>
        /// <remarks>
        /// This method is useful when doing e.g. UNION on two queries and the same columns are needed
        /// in both queries.
        /// </remarks>
        /// <example>
        /// <para>
        /// Collection A has { A, B as C, D } and collection B has { X, D, C } after calling this method result is
        /// A has { A, B as C, D, NULL AS X } and B has { X, D, C, NULL AS A }.
        /// </para>
        /// <para>
        /// Another example, collection A has { *, A } and B has { X }
        /// result is A has { A , NULL as X } and B has { X, NULL as A }.
        /// </para>
        /// </example>
        /// <param name="columnLists">Input lists of columns</param>
        public static void EnsureMissingColumns(ICollection<QueryColumnList> columnLists)
        {
            var presentColumns = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

            var foundColumns = new List<SafeDictionary<string, IQueryColumn>>();

            // Collect all column names first
            foreach (var columns in columnLists)
            {
                if (columns != null)
                {
                    var foundCols = new SafeDictionary<string, IQueryColumn>(StringComparer.InvariantCultureIgnoreCase);
                    foundColumns.Add(foundCols);

                    // Do not add * to other column lists, it can cause invalid queries when e.g. UNION is used between 2 queries
                    foreach (var column in columns.Where(c => !c.Name.Equals(COLUMNS_ALL, StringComparison.Ordinal)))
                    {
                        var name = column.Name;

                        foundCols[name] = column;

                        // Add to list if not present
                        presentColumns.Add(name);
                    }
                }
            }

            // Build the new lists of columns
            int index = 0;

            foreach (var columns in columnLists)
            {
                if (columns != null)
                {
                    // Rebuild the list of columns
                    columns.Load(NO_COLUMNS);

                    var foundCols = foundColumns[index];

                    foreach (var column in presentColumns)
                    {
                        var found = foundCols[column] ?? new QueryColumn("NULL").As(column);

                        columns.Add(found);
                    }
                }

                index++;
            }
        }


        /// <summary>
        /// Returns true, if the query output is a single column
        /// </summary>
        public static bool IsSingleColumn(string columns)
        {
            bool isSingle = false;

            if (String.IsNullOrEmpty(columns) || (columns == COLUMNS_ALL) || IsNoColumns(columns))
            {
                // Empty, all or none is not a single column
            }
            else
            {
                // Parse the column list
                var cols = ParseColumnList(columns);

                isSingle = (cols.Count == 1);
            }

            return isSingle;
        }

        #endregion


        #region "OrderBy methods"

        /// <summary>
        /// Adds the order by to an existing one.
        /// </summary>
        /// <param name="orderBy">Original order by</param>
        /// <param name="add">Order by to add</param>
        /// <param name="dir">Order direction</param>
        public static string AddOrderBy(string orderBy, string add, OrderDirection dir = OrderDirection.Default)
        {
            Func<string, string> tr = null;

            // Ensure the direction
            if (dir == OrderDirection.Ascending)
            {
                tr = EnsureAscendingOrderByDirection;
            }
            else if (dir == OrderDirection.Descending)
            {
                tr = EnsureDescendingOrderByDirection;
            }

            return MergeColumns(orderBy, add, GetOrderByColumnName, true, tr);
        }


        /// <summary>
        /// Ensures descending order by direction
        /// </summary>
        /// <param name="columnName">Column name</param>
        private static string EnsureDescendingOrderByDirection(string columnName)
        {
            return EnsureOrderByDirection(columnName, OrderDirection.Descending);
        }


        /// <summary>
        /// Ensures ascending order by direction
        /// </summary>
        /// <param name="columnName">Column name</param>
        private static string EnsureAscendingOrderByDirection(string columnName)
        {
            return EnsureOrderByDirection(columnName, OrderDirection.Ascending);
        }


        /// <summary>
        /// Ensures the given order by direction
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="dir">Order direction</param>
        private static string EnsureOrderByDirection(string columnName, OrderDirection dir)
        {
            columnName = GetOrderByColumnName(columnName);

            // Add suffix for descending order
            if (dir == OrderDirection.Descending)
            {
                columnName += ORDERBY_DESC;
            }

            return columnName;
        }


        /// <summary>
        /// Gets the column name from the given SQL column expression
        /// </summary>
        /// <param name="column">Column to parse</param>
        public static string GetOrderByColumnName(string column)
        {
            string suffix;

            return GetOrderByColumnName(column, out suffix);
        }


        /// <summary>
        /// Gets the column name from the given SQL column expression
        /// </summary>
        /// <param name="column">Column to parse</param>
        /// <param name="suffix">Outputs the order by suffix for the column</param>
        public static string GetOrderByColumnName(string column, out string suffix)
        {
            suffix = null;
            column = column.Trim();

            // Do not transform no columns constant
            if (IsNoColumns(column))
            {
                return String.Empty;
            }

            if (column.EndsWith(ORDERBY_ASC, StringComparison.OrdinalIgnoreCase))
            {
                // Remove the ascending suffix
                var index = column.Length - 4;

                suffix = column.Substring(index);
                column = column.Substring(0, index);
            }
            else if (column.EndsWith(ORDERBY_DESC, StringComparison.OrdinalIgnoreCase))
            {
                // Remove the descending suffix
                var index = column.Length - 5;

                suffix = column.Substring(index);
                column = column.Substring(0, index);
            }

            return column.Trim();
        }



        /// <summary>
        /// Returns true if the order by expression contains column sorted the specified way.
        /// </summary>
        /// <param name="orderBy">Order by expression to check</param>
        /// <param name="column">Column to check</param>
        /// <param name="ascending">Direction</param>
        public static bool OrderByContains(string orderBy, string column, bool ascending)
        {
            if (String.IsNullOrEmpty(orderBy) || String.IsNullOrEmpty(column))
            {
                return false;
            }

            column = column.ToLowerInvariant();

            string[] cols = orderBy.Split(',');

            // Process all columns
            foreach (string col in cols)
            {
                string expr = col.Trim();

                string colName = expr;

                bool descending = false;

                // Parse the column name
                if (expr.EndsWith(ORDERBY_DESC, StringComparison.OrdinalIgnoreCase))
                {
                    colName = expr.Substring(0, expr.Length - 5).Trim();
                    descending = true;
                }
                else if (expr.EndsWith(ORDERBY_ASC, StringComparison.OrdinalIgnoreCase))
                {
                    colName = expr.Substring(0, expr.Length - 4).Trim();
                }

                // Check if the column name and the order match
                if ((colName.ToLowerInvariant() == column) && (descending == !ascending))
                {
                    return true;
                }
            }

            return false;
        }


        /// <summary>
        /// Reverses the order by string by toggling between ASC and DESC.
        /// </summary>
        /// <param name="orderBy">Original ORDER by</param>
        public static string ReverseOrderBy(string orderBy)
        {
            if (String.IsNullOrEmpty(orderBy))
            {
                return orderBy;
            }

            string[] cols = orderBy.Split(',');
            string newOrderBy = "";

            // Process all columns
            foreach (string col in cols)
            {
                string expr = col.Trim();

                string order;
                string colName = expr;

                // Parse the column name
                if (expr.EndsWith(ORDERBY_DESC, StringComparison.OrdinalIgnoreCase))
                {
                    // Reverse DESC to ASC
                    colName = expr.Substring(0, expr.Length - 5).Trim();
                    order = ORDERBY_ASC;
                }
                else if (expr.EndsWith(ORDERBY_ASC, StringComparison.OrdinalIgnoreCase))
                {
                    // Reverse ASC to DESC
                    colName = expr.Substring(0, expr.Length - 4).Trim();
                    order = ORDERBY_DESC;
                }
                else
                {
                    // Reverse nothing (ASC to DESC)
                    order = ORDERBY_DESC;
                }

                // Add new column to the result
                if (newOrderBy != "")
                {
                    newOrderBy += ", ";
                }
                newOrderBy += colName + order;
            }

            return newOrderBy;
        }

        #endregion


        #region "Schema methods"

        /// <summary>
        /// Removes the owner from the given object name.
        /// </summary>
        /// <param name="objectName">Object name</param>
        public static string RemoveOwner(string objectName)
        {
            // Remove owner from the viewName if present
            if (objectName != null)
            {
                Match match = SchemaNameRegEx.Match(objectName);
                if (match.Success)
                {
                    objectName = match.Groups["objectname"].Value;
                }
            }

            return objectName;
        }


        /// <summary>
        /// Returns safe string representing DB owner.
        /// </summary>
        /// <param name="owner">DB Owner</param>
        public static string GetSafeOwner(string owner)
        {
            if (owner.ToLowerInvariant() == "dbo" || owner.StartsWith("[", StringComparison.Ordinal))
            {
                return owner;
            }

            return "[" + owner + "]";
        }


        /// <summary>
        /// Returns DB object scheme from settings or default value.
        /// </summary>
        public static string GetDBSchemaOrDefault()
        {
            if (mDbSchema.Value == null)
            {
                var schema = SqlInstallationHelper.GetCurrentDefaultSchema(ConnectionHelper.GetConnection());
                if (String.IsNullOrEmpty(schema))
                {
                    schema = DEFAULT_DB_SCHEMA;
                }

                mDbSchema.Value = schema;
            }

            return mDbSchema.Value;
        }

        #endregion


        #region "Data methods"

        /// <summary>
        /// Returns true if the given value is missing or null.
        /// </summary>
        /// <param name="value">Value to check</param>
        public static bool IsMissing(object value)
        {
            return (value == MISSING_VALUE);
        }


        /// <summary>
        /// Returns true if the given value is missing or null.
        /// </summary>
        /// <param name="value">Value to check</param>
        public static bool IsMissingOrNull(object value)
        {
            return ((value == null) || (value == DBNull.Value) || (value == MISSING_VALUE));
        }


        /// <summary>
        /// Returns true if two objects are equal.
        /// </summary>
        /// <param name="obj1">Object 1</param>
        /// <param name="obj2">Object 2</param>
        public static bool ObjectsEqual(object obj1, object obj2)
        {
            if ((obj1 == null) || (obj1 == DBNull.Value))
            {
                return ((obj2 == null) || (obj2 == DBNull.Value));
            }

            return obj1.Equals(obj2);
        }


        /// <summary>
        /// Indicates whether string matches given SQL 'like' pattern. 
        /// </summary>
        /// <param name="value">Input string value</param>
        /// <param name="pattern">Like search patter</param>
        public static bool MatchLikePattern(string value, string pattern)
        {
            string regexPattern = Regex.Replace(
                pattern,
                @"[%_]|\[[^]]*\]|[^%_[]+",
                match =>
                {
                    if (match.Value == "%")
                    {
                        return ".*";
                    }
                    if (match.Value == "_")
                    {
                        return ".";
                    }
                    if (match.Value.StartsWith("[", StringComparison.Ordinal) && match.Value.EndsWith("]", StringComparison.Ordinal))
                    {
                        return match.Value;
                    }
                    return Regex.Escape(match.Value);
                });

            return Regex.IsMatch(value, string.Format("^{0}$", regexPattern), RegexOptions.IgnoreCase);
        }


        /// <summary>
        /// Creates a scalar table (with one row and one column) from the given value.
        /// </summary>
        /// <param name="value">Value</param>
        internal static DataTable CreateScalarTable<TValue>(TValue value)
        {
            // Get just count
            DataTable newDt = new DataTable();
            newDt.Columns.Add("Value", typeof(TValue));

            var dr = newDt.NewRow();
            dr[0] = value;

            newDt.Rows.Add(dr);
            return newDt;
        }

        #endregion
    }
}