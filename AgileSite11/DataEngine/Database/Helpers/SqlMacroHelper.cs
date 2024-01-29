using System;
using System.Text;
using System.Text.RegularExpressions;

using CMS.Base;
using CMS.Helpers;

namespace CMS.DataEngine
{
    /// <summary>
    /// Helper methods for SQL query macros
    /// </summary>
    internal class SqlMacroHelper
    {
        #region "Constants"

        /// <summary>
        /// Replacement for ## in Encode/Decode methods.
        /// </summary>
        private const string SHARP_REPLACEMENT = "{{#}}";

        #endregion


        #region "Variables"

        /// <summary>
        /// Regular expression to match not needed WHERE condition.
        /// </summary>
        public static CMSRegex NoWhereRegExp = GetEmptyWhereRegEx("where");

        /// <summary>
        /// Regular expression to match not needed ORDER BY expression.
        /// </summary>
        public static CMSRegex NoOrderByRegExp = new CMSRegex("(?:\\s+order\\s+by\\s+1)", true);

        /// <summary>
        /// Regular expression to match not needed GROUP BY expression.
        /// </summary>
        public static CMSRegex NoGroupByRegExp = new CMSRegex("(?:\\s+group\\s+by\\s+##groupby##)", true);

        /// <summary>
        /// Regular expression to match not needed HAVING condition.
        /// </summary>
        public static CMSRegex NoHavingRegExp = GetEmptyWhereRegEx("having");

        /// <summary>
        /// Regular expression to match not needed OUTPUT clause.
        /// </summary>
        public static CMSRegex NoOutputRegExp = new CMSRegex("(?:\\s+output\\s+##output##)", true);

        #endregion


        #region "Public methods"

        /// <summary>
        /// Gets a regular expression to match an empty where condition (tautology)
        /// </summary>
        /// <param name="keyWord">Where condition key word</param>
        private static CMSRegex GetEmptyWhereRegEx(string keyWord)
        {
            // Prepare components
            const string commentStart = "\\s*(?:--|/\\*)";
            const string nextExpression = "\\s+(?:order|group)\\s+by";
            const string endBracket = "\\s*\\)";
            const string queryEnd = "\\s*\\z";

            // Next parts of the query that may follow (end current where)
            const string nextParts = "(?=" + nextExpression + "|" + commentStart + "|" + endBracket + "|" + queryEnd + ")";

            // AND operator keyword, if there was OR, we don't want to remove the tautology
            const string andOperator = "\\s+and\\s+";
            const string noNewLineAndOperator = "[^\\S\\n]+and\\s+";

            // General white space as the end of current expression or query
            const string endWhiteSpace = "(?=\\s|\\z)";

            // Tautology that we want to remove and means empty where (1=1)
            const string tautology = "(?:\\(1=1\\)|1=1)";

            // Wrap keyword (typically where or having) into white spaces
            keyWord = $"\\s+{keyWord}\\s+";

            // Build complete pattern using components
            string pattern = "(?:" + keyWord + tautology + nextParts + ")|(?:" + tautology + andOperator + ")|(?:" + noNewLineAndOperator + tautology + endWhiteSpace + ")";

            // Query can contain new lines, matching must be case insensitive
            var options = RegexHelper.DefaultOptions | RegexOptions.Multiline | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase;

            return new CMSRegex(pattern, options);
        }


        /// <summary>
        /// Gets the regular expression for a given macro name
        /// </summary>
        /// <param name="name">Macro name</param>
        public static CMSRegex GetMacroRegEx(string name)
        {
            return new CMSRegex("##" + name + "##", true);
        }


        /// <summary>
        /// Resolves the query macros using the given parameters.
        /// </summary>
        /// <param name="queryText">Query text</param>
        /// <param name="queryParams">Parameters array</param>
        public static string ResolveQueryMacros(string queryText, QueryDataParameters queryParams)
        {
            // Prepare the parameters
            if (queryParams != null)
            {
                queryText = queryParams.GetCompleteQueryText(queryText);
            }

            return queryText;
        }


        /// <summary>
        /// Resolves the query macros.
        /// </summary>
        /// <param name="queryMacros">Query macros</param>
        /// <param name="queryText">Query text</param>
        public static string ResolveQueryMacros(QueryMacros queryMacros, string queryText)
        {
            // Optimized version for general select
            if (queryText == SqlHelper.GENERAL_SELECT)
            {
                queryText = BuildSelectQuery(queryMacros);
            }
            else
            {
                // General version replaces the macros
                queryText = ResolveMacrosInQueryText(queryMacros, queryText);
            }

            // Run the query
            return queryText;
        }


        /// <summary>
        /// Resolves the query macros in the given query text using regular expressions for the query macros and replacements. 
        /// </summary>
        /// <param name="queryMacros">Query macros</param>
        /// <param name="queryText">Query text</param>
        private static string ResolveMacrosInQueryText(QueryMacros queryMacros, string queryText)
        {
            queryText = ReplaceDistinct(queryText, queryMacros.Distinct);
            queryText = ReplaceTopN(queryText, queryMacros.TopN);
            queryText = ReplaceColumns(queryText, EncodeMacroInText(queryMacros.Columns));

            queryText = ReplaceSource(queryText, EncodeMacroInText(queryMacros.Source), EncodeMacroInText(queryMacros.DefaultSource));
            
            queryText = ReplaceWhere(queryText, EncodeMacroInText(queryMacros.Where));
            
            queryText = ReplaceGroupBy(queryText, EncodeMacroInText(queryMacros.GroupBy));
            queryText = ReplaceHaving(queryText, EncodeMacroInText(queryMacros.Having));
            
            queryText = ReplaceOrderBy(queryText, EncodeMacroInText(queryMacros.OrderBy));

            queryText = ReplaceValues(queryText, EncodeMacroInText(queryMacros.Values));
            queryText = ReplaceOutput(queryText, EncodeMacroInText(queryMacros.Output));

            queryText = DecodeMacroInText(queryText);

            return queryText;
        }


        /// <summary>
        /// Replaces the macro within the given text. Returns true, if the replacement was made. Returns false if the the macro wasn't found.
        /// </summary>
        /// <param name="text">Text to replace</param>
        /// <param name="macro">Macro to replace</param>
        /// <param name="replacement">Replacement string</param>
        /// <param name="defaultReplacement">Default replacement in case the replacement is empty</param>
        public static bool ReplaceMacro(ref string text, string macro, string replacement, string defaultReplacement = null)
        {
            // Find first occurrence
            int index = text.IndexOf(macro, StringComparison.InvariantCultureIgnoreCase);
            if (index < 0)
            {
                return false;
            }

            // Apply default replacement
            if (string.IsNullOrEmpty(replacement))
            {
                replacement = defaultReplacement ?? "";
            }

            // Replace all macros
            while (index >= 0)
            {
                text = String.Concat(text.Substring(0, index), replacement, text.Substring(index + macro.Length));

                // Get next location
                index = text.IndexOf(macro, index + replacement.Length, StringComparison.InvariantCultureIgnoreCase);
            }

            return true;
        }


        /// <summary>
        /// Builds the select query using the given macros
        /// </summary>
        /// <param name="queryMacros">Query macros</param>
        private static string BuildSelectQuery(QueryMacros queryMacros)
        {
            var sb = new StringBuilder();

            sb.Append("SELECT");

            // Distinct
            if (queryMacros.Distinct)
            {
                sb.Append(" DISTINCT");
            }

            // Top N
            var topN = queryMacros.TopN;
            if (topN > 0)
            {
                sb.Append(" TOP ");
                sb.Append(topN);
            }

            // Columns
            var columns = GetColumnsString(queryMacros.Columns);
            if (String.IsNullOrEmpty(columns))
            {
                throw new NotSupportedException("[SqlMacroHelper.BuildSelectQuery]: Unable to build select query without columns specified.");
            }

            sb.Append(" ");
            sb.Append(columns);

            // Source
            var source = queryMacros.Source;
            if (String.IsNullOrEmpty(source))
            {
                source = queryMacros.DefaultSource;
                if (String.IsNullOrEmpty(source))
                {
                    throw new NotSupportedException("[SqlMacroHelper.BuildSelectQuery]: Unable to build select query without source or default source specified.");
                }
            }

            sb.Append(Environment.NewLine + "FROM ");
            sb.Append(source);

            // Where condition
            var where = queryMacros.Where;
            if (!String.IsNullOrEmpty(where))
            {
                sb.Append(Environment.NewLine + "WHERE ");
                sb.Append(where);
            }

            // Group by
            var groupBy = queryMacros.GroupBy;

            SqlHelper.HandleEmptyColumns(ref groupBy);

            if (!String.IsNullOrEmpty(groupBy))
            {
                sb.Append(Environment.NewLine + "GROUP BY ");
                sb.Append(groupBy);
            }

            // Having
            var having = queryMacros.Having;
            if (!String.IsNullOrEmpty(having))
            {
                sb.Append(Environment.NewLine + "HAVING ");
                sb.Append(having);
            }

            // Order by
            var orderBy = queryMacros.OrderBy;

            SqlHelper.HandleEmptyColumns(ref orderBy);

            if (!String.IsNullOrEmpty(orderBy))
            {
                sb.Append(Environment.NewLine + "ORDER BY ");
                sb.Append(orderBy);
            }

            return sb.ToString();
        }


        /// <summary>
        /// Replaces the where macro in the query text
        /// </summary>
        /// <param name="queryText">Query text</param>
        /// <param name="groupBy">Where condition</param>
        public static string ReplaceGroupBy(string queryText, string groupBy)
        {
            // Handle the empty columns
            SqlHelper.HandleEmptyColumns(ref groupBy);

            if (ReplaceMacro(ref queryText, QueryMacros.GROUPBY, groupBy, QueryMacros.GROUPBY))
            {
                // Remove the unnecessary group by
                queryText = NoGroupByRegExp.Replace(queryText, "");
            }
            else if (!String.IsNullOrEmpty(groupBy))
            {
                throw new NotSupportedException("[SqlHelper.ReplaceGroupBy]: Missing ##GROUPBY## macro in the query text '" + queryText + "', cannot apply the group by '" + groupBy + "'.");
            }

            return queryText;
        }


        /// <summary>
        /// Replaces the where macro in the query text
        /// </summary>
        /// <param name="queryText">Query text</param>
        /// <param name="having">Where condition</param>
        public static string ReplaceHaving(string queryText, string having)
        {
            // Replace the having macro
            if (ReplaceMacro(ref queryText, QueryMacros.HAVING, having, "1=1"))
            {
                // Remove the unnecessary having
                queryText = NoHavingRegExp.Replace(queryText, "");
            }
            else if (!String.IsNullOrEmpty(having))
            {
                throw new NotSupportedException("[SqlHelper.ReplaceHaving]: Missing ##HAVING## macro in the query text '" + queryText + "', cannot apply the having condition '" + having + "'.");
            }


            return queryText;
        }


        /// <summary>
        /// Replaces the where macro in the query text
        /// </summary>
        /// <param name="queryText">Query text</param>
        /// <param name="where">Where condition</param>
        /// <param name="keepMacro">If true, the where macro is kept in the query</param>
        public static string ReplaceWhere(string queryText, string where, bool keepMacro = false)
        {
            // Ensure that the macro is kept in the expression
            if (keepMacro)
            {
                where = SqlHelper.AddWhereCondition(where, QueryMacros.WHERE);
            }

            // Replace the where macro
            if (ReplaceMacro(ref queryText, QueryMacros.WHERE, where, "1=1"))
            {
                // Remove the unnecessary where if needed
                if (!keepMacro)
                {
                    queryText = NoWhereRegExp.Replace(queryText, "");
                }
            }
            else if (!String.IsNullOrEmpty(where))
            {
                throw new NotSupportedException("[SqlHelper.ReplaceWhere]: Missing ##WHERE## macro in the query text '" + queryText + "', cannot apply the where condition '" + where + "'.");
            }

            return queryText;
        }


        /// <summary>
        /// Replaces the source macro in the query text
        /// </summary>
        /// <param name="queryText">Query text</param>
        /// <param name="source">Query source</param>
        /// <param name="defaultSource">Default source</param>
        public static string ReplaceSource(string queryText, string source, string defaultSource)
        {
            // Replace source
            if (!ReplaceMacro(ref queryText, QueryMacros.SOURCE, source, defaultSource) && !String.IsNullOrEmpty(source))
            {
                throw new NotSupportedException("[SqlHelper.ReplaceSource]: Missing ##SOURCE## macro in the query text '" + queryText + "', cannot apply the source '" + source + "'.");
            }

            return queryText;
        }


        /// <summary>
        /// Replace order by macro in query.
        /// </summary>
        /// <param name="queryText">Query text</param>
        /// <param name="orderBy">Order by expression</param>
        public static string ReplaceOrderBy(string queryText, string orderBy)
        {
            // Handle the empty columns
            SqlHelper.HandleEmptyColumns(ref orderBy);

            if (ReplaceMacro(ref queryText, QueryMacros.ORDERBY, orderBy, "1"))
            {
                // Remove the unnecessary order by
                queryText = NoOrderByRegExp.Replace(queryText, "");
            }
            else if (!String.IsNullOrEmpty(orderBy))
            {
                // Order by macro not present, cannot apply order by
                throw new NotSupportedException("[SqlHelper.ReplaceOrderBy]: Missing ##ORDERBY## macro in the query text '" + queryText + "', cannot apply the specified order by '" + orderBy + "'.");
            }

            return queryText;
        }


        /// <summary>
        /// Removes order by expression from query string.
        /// </summary>
        /// <param name="queryText">Query text</param>
        public static string RemoveOrderBy(string queryText)
        {
            return ReplaceOrderBy(queryText, null);
        }


        /// <summary>
        /// Replaces the TOPN macro in the query text
        /// </summary>
        /// <param name="queryText">Query text</param>
        /// <param name="topN">Top N</param>
        public static string ReplaceTopN(string queryText, int topN)
        {
            // Apply TOPN statement
            string topNstring = (topN > 0) ? "TOP " + topN : "";

            if (!ReplaceMacro(ref queryText, QueryMacros.TOPN, topNstring) && (topN > 0))
            {
                // TopN macro is missing, cannot apply
                throw new NotSupportedException("[SqlHelper.ReplaceTopN]: Missing ##TOPN## macro in the query text '" + queryText + "', cannot apply the specified top N '" + topN + "'.");
            }

            return queryText;
        }


        /// <summary>
        /// Replaces the DISTINCT macro in the query text
        /// </summary>
        /// <param name="queryText">Query text</param>
        /// <param name="distinct">Distinct</param>
        public static string ReplaceDistinct(string queryText, bool distinct)
        {
            var distinctText = distinct ? "DISTINCT" : "";

            if (!ReplaceMacro(ref queryText, QueryMacros.DISTINCT, distinctText) && distinct)
            {
                // Distinct macro is missing, cannot apply
                throw new NotSupportedException("[SqlHelper.ReplaceDistinct]: Missing ##DISTINCT## macro in the query text '" + queryText + "', cannot apply distinct.");
            }

            return queryText;
        }


        /// <summary>
        /// Gets the columns string
        /// </summary>
        /// <param name="columns">Columns</param>
        private static string GetColumnsString(string columns)
        {
            if ((columns == null) || (columns.Trim() == ""))
            {
                // All columns if not specified
                columns = SqlHelper.COLUMNS_ALL;
            }
            else if (columns == SqlHelper.NO_COLUMNS)
            {
                // No columns
                SqlHelper.HandleEmptyColumns(ref columns);
            }

            return columns;
        }


        /// <summary>
        /// Replaces columns macro in the query text
        /// </summary>
        /// <param name="queryText">Query text</param>
        /// <param name="columns">Columns</param>
        public static string ReplaceColumns(string queryText, string columns)
        {
            columns = GetColumnsString(columns);

            // Check if columns by macro is present
            if (!ReplaceMacro(ref queryText, QueryMacros.COLUMNS, columns) && (columns != SqlHelper.COLUMNS_ALL) && (columns != SqlHelper.NO_COLUMNS))
            {
                throw new NotSupportedException("[SqlHelper.ReplaceColumns]: Missing ##COLUMNS## macro in the query text '" + queryText + "', cannot apply the specified columns '" + columns + "'.");
            }

            return queryText;
        }


        /// <summary>
        /// Replaces columns macro in the query text
        /// </summary>
        /// <param name="queryText">Query text</param>
        /// <param name="values">Columns</param>
        public static string ReplaceValues(string queryText, string values)
        {
            if (!ReplaceMacro(ref queryText, QueryMacros.VALUES, values) && !String.IsNullOrEmpty(values))
            {
                throw new NotSupportedException("[SqlHelper.ReplaceValues]: Missing ##VALUES## macro in the query text '" + queryText + "', cannot apply the specified values '" + values + "'.");
            }

            return queryText;
        }


        /// <summary>
        /// Replaces output macro in the query text
        /// </summary>
        /// <param name="queryText">Query text</param>
        /// <param name="output">Output clause</param>
        public static string ReplaceOutput(string queryText, string output)
        {
            if (ReplaceMacro(ref queryText, QueryMacros.OUTPUT, output, QueryMacros.OUTPUT))
            {
                queryText = NoOutputRegExp.Replace(queryText, "");
            }
            else if(!String.IsNullOrEmpty(output))
            {
                throw new NotSupportedException("[SqlHelper.ReplaceOutput]: Missing ##OUTPUT## macro in the query text '" + queryText + "', cannot apply the specified output clause '" + output + "'." );
            }

            return queryText;
        }


        /// <summary>
        /// Encode macros like ##WHERE## in text to {{#}}WHERE{{#}}. 
        /// </summary>
        /// <param name="text">Text to encode.</param>
        public static string EncodeMacroInText(string text)
        {
            return String.IsNullOrEmpty(text) ? text : text.Replace("##", SHARP_REPLACEMENT);
        }


        /// <summary>
        /// Decode macros like ##WHERE## in text to {{#}}WHERE{{#}} 
        /// </summary>
        /// <param name="text">Text to decode.</param>
        public static string DecodeMacroInText(string text)
        {
            return String.IsNullOrEmpty(text) ? text : text.Replace(SHARP_REPLACEMENT, "##");
        }

        #endregion
    }
}