using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.SiteProvider;
using CMS.Core;

namespace CMS.CustomTables
{
    /// <summary>
    /// Provides methods for creating search documents for custom tables
    /// </summary>
    public class CustomTableHelper
    {
        /// <summary>
        /// Returns collection of search index documents filtered by input criteria.
        /// </summary>
        /// <param name="indexInfo">Search index info object</param>
        /// <param name="customTableClassName">Custom table class name</param>
        /// <param name="whereCondition">Where condition</param>
        /// <param name="lastItemId">Last item id</param>
        internal static List<SearchDocument> GetSearchDocuments(ISearchIndexInfo indexInfo, string customTableClassName, string whereCondition, int lastItemId)
        {
            // Get type info
            var ti = CustomTableItemProvider.GetTypeInfo(customTableClassName);
            if (ti == null)
            {
                throw new Exception("[CustomTableItemProvider.GetSearchDocuments]: Custom table '" + customTableClassName + "' type info not found.");
            }

            // Get data from db
            string where = SqlHelper.AddWhereCondition(ti.IDColumn + " > " + lastItemId, whereCondition);

            var ds = ConnectionHelper.ExecuteQuery(customTableClassName + ".selectall", null, @where, ti.IDColumn, indexInfo.IndexBatchSize);

            // Check whether data exist
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                var documents = new List<SearchDocument>();
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    CustomTableItem cti = CustomTableItem.New(customTableClassName, dr);
                    documents.Add(cti.GetSearchDocument(indexInfo));
                }

                return documents;
            }

            return null;
        }


        /// <summary>
        /// Returns the dataset containing custom tables classes.
        /// </summary>        
        public static ObjectQuery<DataClassInfo> GetCustomTableClasses()
        {
            return DataClassInfoProvider.GetClasses().WithObjectType(CustomTableInfo.OBJECT_TYPE_CUSTOMTABLE);
        }


        /// <summary>
        /// Returns the dataset containing custom tables classes allowed in site.
        /// </summary>     
        /// <param name="siteId">Site id</param>
        public static ObjectQuery<DataClassInfo> GetCustomTableClasses(int siteId)
        {
            return GetCustomTableClasses().OnSite(siteId);
        }


        /// <summary>
        /// Returns DataSet with custom tables which current user can read.
        /// </summary>
        /// <param name="where">Complete where condition</param>
        /// <param name="orderBy">Current order by clause</param>
        /// <param name="topN">Current top N value</param>
        /// <param name="columns">Currently selected columns</param>
        public static DataSet GetFilteredTablesByPermission(string where, string orderBy, int topN, string columns)
        {
            var data = GetCustomTableClasses(SiteContext.CurrentSiteID)
                                          .Where(where)
                                          .OrderBy(orderBy)
                                          .TopN(topN)
                                          .Columns(SqlHelper.AddColumns(columns, "ClassID"));

            // Check permission to each custom table if user is not authorized to read all (from module)
            if (DataHelper.DataSourceIsEmpty(data) || MembershipContext.AuthenticatedUser.IsAuthorizedPerResource(ModuleName.CUSTOMTABLES, "Read"))
            {
                return data;
            }

            return FilterTablesByPermission(data);
        }


        /// <summary>
        /// Filters custom tables by 'read' permission
        /// </summary>
        /// <param name="data">Custom tables data query</param>
        internal static DataSet FilterTablesByPermission(ObjectQuery<DataClassInfo> data)
        {
            ArrayList toDelete = new ArrayList();
            DataRowCollection rows = data.Tables[0].Rows;

            foreach (DataRow row in rows)
            {
                int classId = ValidationHelper.GetInteger(row["ClassID"], 0);
                string className = DataClassInfoProvider.GetClassName(classId);
                if (String.IsNullOrEmpty(className) || !MembershipContext.AuthenticatedUser.IsAuthorizedPerClassName(className, "Read"))
                {
                    toDelete.Add(row);
                }
            }

            // Delete from DataSet
            foreach (DataRow row in toDelete)
            {
                rows.Remove(row);
            }

            return data;
        }
    }
}
