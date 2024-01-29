using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using CMS.Helpers;
using CMS.IO;

using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace CMS.DataEngine
{
    /// <summary>
    /// Security methods for SQL queries
    /// </summary>
    public static class SqlSecurityHelper
    {
        private static Regex mWhereRegex;
        private static Regex mOrderByRegex;
        private static Regex mColumnsRegex;
        
        private static readonly string SQLIdentifier = "(?:\\[|@)?[A-Za-z0-9_.]+\\]?";
        private static readonly string SQLNumber = "[+-]?((\\d+(\\.\\d*)?)|(\\.\\d+))";
        internal static readonly string SQLString = "'[^']*'";
        

        /// <summary>
        /// Regular expression to check security of WHERE clause.
        /// </summary>
        public static Regex WhereRegex
        {
            get
            {
                if (mWhereRegex == null)
                {
                    mWhereRegex = RegexHelper.GetRegex(GetWhereRegEx(), true);
                }
                return mWhereRegex;
            }
        }


        /// <summary>
        /// Regular expression to check security of ORDER BY clause.
        /// </summary>
        public static Regex OrderByRegex
        {
            get
            {
                if (mOrderByRegex == null)
                {
                    mOrderByRegex = RegexHelper.GetRegex(GetOrderByRegEx(), true);
                }
                return mOrderByRegex;
            }
        }


        /// <summary>
        /// Regular expression to check security of COLUMNS clause.
        /// </summary>
        public static Regex ColumnsRegex
        {
            get
            {
                if (mColumnsRegex == null)
                {
                    mColumnsRegex = RegexHelper.GetRegex(GetColumnsRegEx(), RegexOptions.None);
                }
                return mColumnsRegex;
            }
        }


        /// <summary>
        /// Gets the regular expression for the safe value of WHERE condition.
        /// </summary>
        public static string GetWhereRegEx()
        {
            // Build the regex
            string str = "(N'|')[^']*'";
            // Identifier, simple number, simple string, number or string in list separated by commas
            string item = String.Format("(?:{0}(?:\\.{0})?|{1}|{2}|({1}|{2})(\\s*,\\s*({1}|{2}))+)", SQLIdentifier, SQLNumber, str);
            string space = "[ ()]";
            string op = "(?:[+*/%=\\-]|(?:NOT )?IN|(?:NOT )?BETWEEN|<>|!=|>=?|<=?|(?:OR|AND|(?:NOT )?LIKE))";
            string unaryOp = "(?:IS (?:NOT )?NULL)";

            return String.Format("^{0}*(?:{1}(?:{0}*((?:{2}{0}*{1})|{3}))*)?{0}*$", space, item, op, unaryOp);
        }


        /// <summary>
        /// Gets the regular expression for the safe value of ORDER BY clause.
        /// </summary>
        public static string GetOrderByRegEx()
        {
            // Build the regex
            string item = String.Format("(?:{0}(?:\\.{0})?(?:\\s+(?:ASC|DESC))?)", SQLIdentifier);
            return String.Format("^\\s*(?:{0}(?:\\s*,\\s*{0})*)?\\s*$", item);
        }


        /// <summary>
        /// Gets the regular expression for the safe value of COLUMNS clause.
        /// </summary>
        public static string GetColumnsRegEx()
        {
            // Build the regex
            string item = String.Format("(?:{0}(?:\\.{0})?(?:\\s+AS\\s*(?:{0}|{1}))?)", SQLIdentifier, SQLString);
            return String.Format("^\\s*(?:{0}(?:\\s*,\\s*{0})*)?\\s*$", item);
        }

        
        /// <summary>
        /// Checks part of a query (or whole query) for malicious code. Returns TRUE if query contains just a SELECT statement.
        /// </summary>
        /// <param name="query">Part of a query or query.</param>
        /// <param name="scope">Defines scope where the specified part of a query is used.</param>
        public static bool CheckQuery(string query, QueryScopeEnum scope)
        {
            if (String.IsNullOrEmpty(query))
            {
                return true;
            }

            string completeQuery = GetCompleteQuery(query, scope);
            if (!String.IsNullOrEmpty(completeQuery))
            {
                TSql100Parser tsqlParser = new TSql100Parser(true);

                IList<ParseError> errors;
                var fragments = tsqlParser.Parse(new StringReader(completeQuery), out errors);

                var sqlScript = fragments as TSqlScript;

                return (errors.Count == 0)
                    && (sqlScript != null)
                    && (sqlScript.Batches.Count == 1)
                    && (sqlScript.Batches[0].Statements.Count == 1)
                    && (sqlScript.Batches[0].Statements[0] is SelectStatement);
            }

            return false;
        }
                 

        private static string GetCompleteQuery(string query, QueryScopeEnum scope)
        {
            string completeQuery;

            switch (scope)
            {
                case QueryScopeEnum.Columns:
                    completeQuery = $"SELECT {query} FROM [NOTEXISTINGTABLE]";
                    break;

                case QueryScopeEnum.OrderBy:
                    completeQuery = $"SELECT * FROM [NOTEXISTINGTABLE] ORDER BY {query}";
                    break;

                case QueryScopeEnum.Where:
                    completeQuery = $"SELECT * FROM [NOTEXISTINGTABLE] WHERE {query}";
                    break;

                case QueryScopeEnum.Query:
                    completeQuery = query;
                    break;

                default:
                    completeQuery = null;
                    break;
            }

            return completeQuery;
        }
    }
}