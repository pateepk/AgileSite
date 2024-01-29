using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

using CMS.Core;
using CMS.DataEngine;
using CMS.DataEngine.Internal;
using CMS.EventLog;

namespace CMS.Membership
{
    /// <summary>
    /// Database operation support for session and online user processing.
    /// </summary>
    internal sealed class SessionDatabaseUpdater
    {
        /// <summary>
        /// Updates given items as one database operation.
        /// </summary>
        public void UpdateItems(IEnumerable<SessionInfo> items)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            items = items.Where(i => i.IsUpdateRequired).ToArray();

            var dataTableContainer = Service.Resolve<IDataTableProvider>().ConvertToDataTable(items, OnlineUserInfo.OBJECT_TYPE);
            dataTableContainer.DataTable.Columns.Remove(OnlineUserInfo.TYPEINFO.IDColumn);

            var query = QueryInfoProvider.GetQueryInfo(GetFullQueryName("updatesessionbulk"));
            var procedureName = query.QueryText;

            using (var connection = new SqlConnection(ConnectionHelper.ConnectionString))
            {
                connection.Open();

                using (var command = new SqlCommand(procedureName, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    var parameter = command.Parameters.AddWithValue("@Sessions", dataTableContainer.DataTable);
                    parameter.SqlDbType = SqlDbType.Structured;
                    parameter.TypeName = "Type_CMS_SessionTable";

                    try
                    {
                        command.ExecuteNonQuery();

                        UnsetUpdatedFlag(items.ToArray());
                    }
                    catch (SqlException ex)
                    {
                        EventLogProvider.LogException("SessionInfo", "Update", ex);
                    }
                }
            }
        }


        /// <summary>
        /// Updates given item in database.
        /// </summary>
        public void UpdateItem(SessionInfo item)
        {
            UpdateItems(new[] { item });
        }


        /// <summary>
        /// Deletes given item from the database.
        /// </summary>
        public void DeleteItem(SessionInfo item)
        {
            var parameters = GetDeleteParameters(item);
            ConnectionHelper.ExecuteQuery(GetFullQueryName("deletesession"), parameters);
        }


        private static QueryDataParameters GetDeleteParameters(SessionInfo item)
        {
            return new QueryDataParameters { { "@SessionIdentifier", item.SessionIdentifier } };
        }


        private static string GetFullQueryName(string name)
        {
            return ObjectHelper.BuildFullName(UserInfo.OBJECT_TYPE, name);
        }


        private static void UnsetUpdatedFlag(params SessionInfo[] items)
        {
            foreach (var item in items)
            {
                item.UnsetUpdate();
            }
        }
    }
}
