using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS.Base;
using CMS.Chat;
using CMS.Core;
using CMS.DataEngine;
using CMS.DeviceProfiles;
using CMS.DocumentEngine;
using CMS.Ecommerce;
using CMS.Membership;
using CMS.ModuleUsageTracking;
using CMS.PortalEngine;
using CMS.Reporting;
using CMS.SiteProvider;

[assembly: RegisterModuleUsageDataSource(typeof(ObjectCountDataSource))]

namespace CMS.ModuleUsageTracking
{
    /// <summary>
    /// Class for retrieving row counts of specified objects.
    /// </summary>
    internal class ObjectCountDataSource : IModuleUsageDataSource
    {
        /// <summary>
        /// Types of objects which count will be tracked. 
        /// </summary>
        protected List<string> mTrackedObjects = new List<string>
        {
            TreeNode.OBJECT_TYPE,
            SiteInfo.OBJECT_TYPE,
            DeviceProfileInfo.OBJECT_TYPE,
            WebPartInfo.OBJECT_TYPE,
            ChatRoomInfo.OBJECT_TYPE,
            OrderInfo.OBJECT_TYPE,
            CustomerInfo.OBJECT_TYPE,
            UserInfo.OBJECT_TYPE,
            ReportInfo.OBJECT_TYPE
        };


        /// <summary>
        /// Name of the object count data source.
        /// </summary>
        public string Name
        {
            get
            {
                return "CMS.ObjectCount";
            }
        }


        /// <summary>
        /// Get counts of objects in the database.
        /// </summary>
        public IModuleUsageDataCollection GetData()
        {
            // Create dictionary [TableName] => [ObjectType]
            var objectTables = GetTrackedObjectsTables();

            // Get dataset with records [TableName] => [RowCount]
            var rowCounts = GetRowCounts(objectTables.Keys.ToList());
            rowCounts.Execute();

            var result = ObjectFactory<IModuleUsageDataCollection>.New();

            if (rowCounts.TotalRecords > 0)
            {
                // Insert record [ObjectType] => [RowCount]
                foreach (DataRow row in rowCounts.Result.Tables[0].Rows)
                {
                    result.Add(objectTables[Convert.ToString(row["TableName"])].Replace('.', '_'), Convert.ToInt64(row["RowCount"]));
                }
            }

            return result;
        }


        /// <summary>
        /// Gets query that returns data set containing row count number for each provided table.
        /// The data set contains two columns: TableName and RowCount.
        /// </summary>
        /// <param name="tableNames">Names of database tables</param>
        protected DataQuery GetRowCounts(IList<string> tableNames)
        {
            var countQuery = new DataQuery()
                .From("sys.tables AS T JOIN sys.dm_db_partition_stats AS P ON T.object_id = P.object_id")
                .Columns("T.name AS TableName, P.row_count AS RowCount")
                .WhereLessThan("P.index_id", 2)
                .WhereIn("T.name", tableNames);

            return countQuery;
        }


        /// <summary>
        /// Returns name of database table in which given type of object is stored.
        /// </summary>
        /// <param name="objectType">Object type string</param>
        protected string GetTableName(string objectType)
        {
            var typeInfo = ObjectTypeManager.GetTypeInfo(objectType);          
            var className = typeInfo == null ? null : typeInfo.ObjectClassName;

            if (className != null)
            {
                var classInfo = DataClassInfoProvider.GetDataClassInfo(className);
                return classInfo == null ? null : classInfo.ClassTableName;
            }

            return null;
        }


        /// <summary>
        /// Returns dictionary containing table name (key) for each tracked object type (value).
        /// </summary>
        /// <remarks>
        /// Unknown object types or types without table are left out.
        /// </remarks>
        private IDictionary<string, string> GetTrackedObjectsTables()
        {
            var result = new Dictionary<string, string>(mTrackedObjects.Count);

            foreach (var objectType in mTrackedObjects)
            {
                var tableName = GetTableName(objectType);

                // Skip object types without table name
                if (!String.IsNullOrEmpty(tableName))
                {
                    result.Add(tableName, objectType);
                }
            }

            return result;
        }
    }
}
