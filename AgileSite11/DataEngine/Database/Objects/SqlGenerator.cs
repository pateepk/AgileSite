using System;
using System.Collections.Generic;

using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Generates SQL queries for given table.
    /// </summary>
    public static class SqlGenerator
    {
        #region "Mapping of query names to the query enum"

        /// <summary>
        /// Mapping of query names to query types
        /// </summary>
        private static readonly StringSafeDictionary<SqlOperationTypeEnum> mQueryTypes = new StringSafeDictionary<SqlOperationTypeEnum>().With(d =>
        {
            d[QueryName.GENERALDELETE] = SqlOperationTypeEnum.GeneralDelete;
            d[QueryName.GENERALINSERT] = SqlOperationTypeEnum.GeneralInsert;
            d[QueryName.GENERALUPDATE] = SqlOperationTypeEnum.GeneralUpdate;
            d[QueryName.GENERALUPSERT] = SqlOperationTypeEnum.GeneralUpsert;
            d[QueryName.GENERALSELECT] = SqlOperationTypeEnum.GeneralSelect;

            d[QueryName.DELETE] = SqlOperationTypeEnum.DeleteQuery;
            d[QueryName.DELETEALL] = SqlOperationTypeEnum.DeleteAll;
            d[QueryName.INSERT] = SqlOperationTypeEnum.InsertQuery;
            d[QueryName.INSERTIDENTITY] = SqlOperationTypeEnum.InsertWithIdentity;
            d[QueryName.SELECT] = SqlOperationTypeEnum.SelectQuery;
            d[QueryName.SELECTALL] = SqlOperationTypeEnum.SelectAll;
            d[QueryName.UPDATE] = SqlOperationTypeEnum.UpdateQuery;
            d[QueryName.UPDATEALL] = SqlOperationTypeEnum.UpdateAll;

            d.DefaultValue = SqlOperationTypeEnum.UnknownQuery;
        });


        /// <summary>
        /// Mapping of query names to query types
        /// </summary>
        private static readonly HashSet<string> mSystemQueries = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
        {
            QueryName.GENERALDELETE,
            QueryName.GENERALINSERT,
            QueryName.GENERALUPDATE,
            QueryName.GENERALUPSERT,
            QueryName.GENERALSELECT,

            QueryName.DELETE,
            QueryName.DELETEALL,
            QueryName.INSERT,
            QueryName.INSERTIDENTITY,
            QueryName.SELECT,
            QueryName.SELECTALL,
            QueryName.UPDATE,
            QueryName.UPDATEALL,
        };

        #endregion


        #region "Methods"

        /// <summary>
        /// View name.
        /// </summary>
        /// <param name="viewName">View name</param>
        /// <param name="indexes">Returns extra code for the initialization of the view</param>
        public static string GetSystemViewSqlQuery(string viewName, out string indexes)
        {
            return DataConnectionFactory.GeneratorObject.GetSystemViewSqlQuery(viewName, out indexes);
        }


        /// <summary>
        /// Generates the given type of query for table specified by its className.
        /// </summary>
        /// <param name="className">Class name of the document data</param>
        /// <param name="queryType">Query type</param>
        /// <param name="siteName">Codename of the site</param>
        public static string GetSqlQuery(string className, SqlOperationTypeEnum queryType, string siteName)
        {
            return DataConnectionFactory.GeneratorObject.GetSqlQuery(className, queryType, siteName);
        }


        /// <summary>
        /// Gets an automatically generated query
        /// </summary>
        /// <param name="queryFullName">Query full name</param>
        internal static QueryInfo GetAutomaticQuery(string queryFullName)
        {
            // Parse the query
            string queryName;
            string className;

            if (!ObjectHelper.ParseFullName(queryFullName, out className, out queryName))
            {
                return null;
            }

            if (IsSystemQuery(queryName))
            {
                // System query, generate the query
                string queryText = GetSqlQuery(className, GetQueryType(queryName), null);

                if (!String.IsNullOrEmpty(queryText))
                {
                    // Get data class
                    DataClassInfo dci = null;

                    if (!String.IsNullOrEmpty(className))
                    {
                        dci = DataClassInfoProvider.GetDataClassInfo(className, true);
                    }

                    return CreateVirtualQuery(queryFullName, queryName, dci, queryText);
                }
            }

            return null;
        }


        /// <summary>
        /// Creates a virtual query object
        /// </summary>
        /// <param name="queryFullName">Query full name</param>
        /// <param name="queryName">Query name</param>
        /// <param name="dci">Class info</param>
        /// <param name="queryText">Query text</param>
        internal static QueryInfo CreateVirtualQuery(string queryFullName, string queryName, DataClassInfo dci, string queryText)
        {
            // Create the virtual query object
            var query = new QueryInfo();

            //query.QueryId = Int32.MaxValue;
            query.QueryFullName = queryFullName;
            query.QueryName = queryName;

            if (dci != null)
            {
                query.ClassID = dci.ClassID;
                query.QueryConnectionString = dci.ClassConnectionString;
            }

            query.QueryType = QueryTypeEnum.SQLQuery;
            query.QueryText = queryText;

            query.IsVirtual = true;

            return query;
        }


        /// <summary>
        /// Gets query type due to specified query name.
        /// </summary>
        /// <param name="queryName">Name of the query</param>
        public static SqlOperationTypeEnum GetQueryType(string queryName)
        {
            return mQueryTypes[queryName];
        }


        /// <summary>
        /// Check if the specified query is system query.
        /// </summary>
        /// <param name="queryName">Query name</param>
        public static bool IsSystemQuery(string queryName)
        {
            return mSystemQueries.Contains(queryName);
        }

        #endregion
    }
}