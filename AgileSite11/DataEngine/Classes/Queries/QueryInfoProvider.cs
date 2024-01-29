using System;

using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Enables access to queries.
    /// </summary>
    public class QueryInfoProvider : AbstractInfoProvider<QueryInfo, QueryInfoProvider>, IFullNameInfoProvider
    {
        #region "Properties"
        
        /// <summary>
        /// If true, the automatic queries are used as the primary source of the queries
        /// </summary>
        public static bool UseAutomaticQueriesAsPrimary
        {
            get;
            set;
        }

        #endregion


        #region "Constructor"

        /// <summary>
        /// Constructor
        /// </summary>
        public QueryInfoProvider()
            : base(QueryInfo.TYPEINFO, new HashtableSettings
                {
                    ID = true, 
                    FullName = true
                })
        {
        }

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns a query for all the QueryInfo objects.
        /// </summary>
        public static ObjectQuery<QueryInfo> GetQueries()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns QueryInfo with specified ID.
        /// </summary>
        /// <param name="id">QueryInfo ID</param>
        public static QueryInfo GetQueryInfo(int id)
        {
            return ProviderObject.GetInfoById(id);
        }


        /// <summary>
        /// Returns QueryInfo with specified name.
        /// </summary>
        /// <param name="name">QueryInfo name</param>
        /// <returns>Query object. Null if the query doesn't exist.</returns>
        public static QueryInfo GetQueryInfoFromDB(string name)
        {
            return ProviderObject.GetInfoByFullName(name, false);
        }



        /// <summary>
        /// Returns QueryInfo with specified name.
        /// </summary>
        /// <param name="name">QueryInfo name</param>
        /// <returns>Query object. Null if the query doesn't exist.</returns>
        public static QueryInfo GetQueryInfo(string name)
        {
            return GetQueryInfo(name, true);
        }


        /// <summary>
        /// Sets (updates or inserts) specified QueryInfo.
        /// </summary>
        /// <param name="infoObj">QueryInfo to be set</param>
        public static void SetQueryInfo(QueryInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified QueryInfo.
        /// </summary>
        /// <param name="infoObj">QueryInfo to be deleted</param>
        public static void DeleteQueryInfo(QueryInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes QueryInfo with specified ID.
        /// </summary>
        /// <param name="id">QueryInfo ID</param>
        public static void DeleteQueryInfo(int id)
        {
            QueryInfo infoObj = GetQueryInfo(id);
            DeleteQueryInfo(infoObj);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Returns QueryInfo with specified name.
        /// </summary>
        /// <param name="queryName">QueryInfo name</param>
        /// <returns>Query object. Null if the query doesn't exist.</returns>
        public virtual QueryInfo GetQueryInfoInternal(string queryName)
        {
            if (String.IsNullOrEmpty(queryName))
            {
                return null;
            }

            LoadInfos();

            QueryInfo result = null;

            var useHashtable = HashtableSettings.FullName;

            // Try to get from the hashtable
            var queryObj = useHashtable ? infos.ByFullName[queryName] : null;
            if (queryObj is NotImplementedInfo)
            {
                // Empty info - does not exist
            }
            else if (queryObj == null)
            {
                bool dbFirst = !UseAutomaticQueriesAsPrimary;
                if (dbFirst)
                {
                    // Was removed or not loaded yet - try to get first from the database
                    result = GetInfoByFullName(queryName, false) ?? SqlGenerator.GetAutomaticQuery(queryName);
                }
                else
                {
                    // Wasn't loaded - try to get first as automatic
                    result = SqlGenerator.GetAutomaticQuery(queryName) ?? GetInfoByFullName(queryName, false);
                }

                if (useHashtable)
                {
                    if (result != null)
                    {
                        infos.ByFullName[queryName] = result;
                    }
                    else
                    {
                        infos.ByFullName[queryName] = InfoHelper.EmptyInfo;
                    }
                }
            }
            else
            {
                // Query found in hashtable
                result = (QueryInfo)queryObj;
            }

            return result;
        }


        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(QueryInfo info)
        {
            base.SetInfo(info);
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(QueryInfo info)
        {
            base.DeleteInfo(info);
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns true if the given query is explicitly defined
        /// </summary>
        /// <param name="className">Class name</param>
        /// <param name="queryName">Query name</param>
        public static bool QueryIsExplicitlyDefined(string className, string queryName)
        {
            // Check if insert query exists
            var qi = GetQueryInfo(className + "." + queryName, false);
            if ((qi != null) && !qi.IsVirtual)
            {
                return true;
            }

            return false;
        }


        /// <summary>
        /// Clears the default queries for the given class name from hashtable
        /// </summary>
        /// <param name="dci">Data class object</param>
        /// <param name="dataClass">If true, removes DataClass queries</param>
        /// <param name="documentType">If true, removes document types queries</param>
        public static void ClearDefaultQueries(DataClassInfo dci, bool dataClass, bool documentType)
        {
            string className = dci.ClassName;

            var byFullName = ProviderObject.infos.ByFullName;

            // Regenerate queries                            
            if (dataClass)
            {
                if (!dci.ClassIsDocumentType || dci.ClassIsCoupledClass)
                {
                    // Standard default queries
                    byFullName.Remove(className + "." + QueryName.SELECT);
                    byFullName.Remove(className + "." + QueryName.DELETE);
                    byFullName.Remove(className + "." + QueryName.INSERT);
                    byFullName.Remove(className + "." + QueryName.UPDATE);
                    byFullName.Remove(className + "." + QueryName.SELECTALL);
                    byFullName.Remove(className + "." + QueryName.INSERTIDENTITY);
                }
            }

            // Custom table queries
            if (!dci.ClassIsDocumentType)
            {
                byFullName.Remove(className + "." + QueryName.DELETEALL);
                byFullName.Remove(className + "." + QueryName.UPDATEALL);
            }
        }


        /// <summary>
        /// Gets the class name from the query name.
        /// </summary>
        /// <param name="queryName">Query name</param>
        public static string GetClassName(string queryName)
        {
            if (queryName == null)
            {
                return null;
            }

            // Parse the query
            int queryNameIndex = queryName.LastIndexOfCSafe('.');
            if (queryNameIndex >= 0)
            {
                return queryName.Remove(queryNameIndex);
            }
            else
            {
                return null;
            }
        }


        /// <summary>
        /// Gets the cache dependencies for the query.
        /// </summary>
        /// <param name="queryName">Query name</param>
        public static string GetQueryCacheDependencies(string queryName)
        {
            string result = null;

            string className = GetClassName(queryName);
            if (!String.IsNullOrEmpty(className))
            {
                // By default, depends on all objects of the given class name
                result = className.ToLowerCSafe() + "|all";
            }

            return result;
        }


        /// <summary>
        /// Gets the query type based on the given type ID
        /// </summary>
        /// <param name="queryTypeId">Query type ID</param>
        public static QueryTypeEnum GetQueryType(int queryTypeId)
        {
            return (queryTypeId == 0) ? QueryTypeEnum.SQLQuery : QueryTypeEnum.StoredProcedure;
        }


        /// <summary>
        /// Gets the query type ID based on the given type
        /// </summary>
        /// <param name="queryType">Query type</param>
        public static int GetQueryTypeID(QueryTypeEnum queryType)
        {
            if (queryType == QueryTypeEnum.SQLQuery)
            {
                return 0;
            }
            else
            {
                return 1;
            }
        }


        /// <summary>
        /// Creates new dictionary for caching the objects by full name
        /// </summary>
        public ProviderInfoDictionary<string> GetFullNameDictionary()
        {
            return new ProviderInfoDictionary<string>(QueryInfo.OBJECT_TYPE, "QueryName;ClassID");
        }


        /// <summary>
        /// Gets the where condition that searches the object based on the given full name
        /// </summary>
        /// <param name="fullName">Object full name</param>
        public string GetFullNameWhereCondition(string fullName)
        {
            string className;
            string queryName;

            if (ObjectHelper.ParseFullName(fullName, out className, out queryName))
            {
                // Get data class
                var dci = DataClassInfoProvider.GetDataClassInfo(className);
                if (dci == null)
                {
                    return null;
                }

                return String.Format("QueryName = N'{0}' AND ClassID = {1}", SqlHelper.GetSafeQueryString(queryName, false), dci.ClassID);
            }

            return null;
        }

        #endregion


        #region "Overridden methods"


        /// <summary>
        /// Returns QueryInfo with specified name.
        /// </summary>
        /// <param name="name">QueryInfo name</param>
        /// <param name="throwException">If true, exception is thrown if query does not exist</param>
        /// <returns>Query object. Null if the query doesn't exist.</returns>
        public static QueryInfo GetQueryInfo(string name, bool throwException)
        {
            var result = ProviderObject.GetQueryInfoInternal(name);

            // Throw exception if required
            if ((result == null) && throwException)
            {
                throw new Exception("[QueryProvider.GetQuery]: Query " + name + " not found!");
            }

            return result;
        }


        /// <summary>
        /// Gets the object query for the provider
        /// </summary>
        protected override ObjectQuery<QueryInfo> GetObjectQueryInternal()
        {
            var q = base.GetObjectQueryInternal();

            // Ensure query source
            q.CustomQueryText = SqlHelper.GENERAL_SELECT;
            q.ConnectionStringName = ConnectionHelper.DEFAULT_CONNECTIONSTRING_NAME;
            q.DefaultQuerySource = "CMS_Query";

            return q;
        }

        #endregion
    }
}