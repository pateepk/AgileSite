using System;
using System.Data;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.Base;

namespace CMS.Reporting
{
    /// <summary>
    /// Class providing ReportTableInfo management.
    /// </summary>
    public class ReportTableInfoProvider : AbstractInfoProvider<ReportTableInfo, ReportTableInfoProvider>
    {
        #region "Public methods"

        /// <summary>
        /// Returns a query for all the <see cref="ReportTableInfo"/> objects.
        /// </summary>
        public static ObjectQuery<ReportTableInfo> GetReportTables()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns object with specified GUID.
        /// </summary>
        /// <param name="guid">Object GUID</param>
        public static ReportTableInfo GetReportTableInfoByGUID(Guid guid)
        {
            return ProviderObject.GetInfoByGuid(guid);
        }


        /// <summary>
        /// Returns the ReportTableInfo structure for the specified report table.
        /// </summary>
        /// <param name="reportTableId">ReportTable ID</param>
        public static ReportTableInfo GetReportTableInfo(int reportTableId)
        {
            return ProviderObject.GetInfoById(reportTableId);
        }


        /// <summary>
        /// Returns the ReportTableInfo structure for the specified parameters.
        /// </summary>
        /// <param name="tableName">Table name</param>
        public static ReportTableInfo GetReportTableInfo(string tableName)
        {
            return ProviderObject.GetReportTableInfoInternal(tableName);
        }


        /// <summary>
        /// Sets (updates or inserts) specified report table.
        /// </summary>
        /// <param name="reportTable">ReportTable to set</param>
        public static void SetReportTableInfo(ReportTableInfo reportTable)
        {
            ProviderObject.SetInfo(reportTable);
        }


        /// <summary>
        /// Deletes specified report table.
        /// </summary>
        /// <param name="reportTableObj">ReportTable object</param>
        public static void DeleteReportTableInfo(ReportTableInfo reportTableObj)
        {
            ProviderObject.DeleteInfo(reportTableObj);
        }


        /// <summary>
        /// Deletes specified report table.
        /// </summary>
        /// <param name="reportTableId">ReportTable ID</param>
        public static void DeleteReportTableInfo(int reportTableId)
        {
            ReportTableInfo reportTableObj = GetReportTableInfo(reportTableId);
            ProviderObject.DeleteInfo(reportTableObj);
        }


        /// <summary>
        /// Uses the query to retrieve the table data from the database and returns the resulting table.
        /// </summary>
        /// <param name="tableObj">ReportTableInfo object</param>
        /// <param name="parameters">Query parameters</param>
        public static DataSet GetTableData(ReportTableInfo tableObj, QueryDataParameters parameters)
        {
            return ProviderObject.GetTableDataInternal(tableObj, parameters);
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Returns the ReportTableInfo structure for the specified parameters.
        /// </summary>
        /// <param name="tableName">Table name</param>
        protected ReportTableInfo GetReportTableInfoInternal(string tableName)
        {
            ReportTableInfo reportTableObj = null;

            string tableReportName = "";

            if (tableName.IndexOfCSafe('.') >= 0)
            {
                tableReportName = tableName.Remove(tableName.LastIndexOfCSafe('.'));
            }

            tableName = tableName.Remove(0, tableName.LastIndexOfCSafe('.') + 1);

            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@ReportTableName", tableName);
            parameters.Add("@ReportName", tableReportName);

            DataSet ds = ConnectionHelper.ExecuteQuery("Reporting.ReportTable.selectByFullName", parameters);

            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                reportTableObj = new ReportTableInfo(ds.Tables[0].Rows[0]);
            }

            return reportTableObj;
        }


        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(ReportTableInfo info)
        {
            if (info != null)
            {
                // Set custom data
                info.SetValue("TableSettings", info.TableSettings.GetData());

                base.SetInfo(info);
            }
            else
            {
                throw new Exception("[ReportTableInfoProvider.SetReportTableInfo]: No ReportTableInfo object set.");
            }
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(ReportTableInfo info)
        {
            if (info != null)
            {
                // Delete table's subscriptions
                ReportSubscriptionInfoProvider.DeleteSubscriptions("ReportSubscriptionTableID=" + info.TableID);

                base.DeleteInfo(info);
            }
        }


        /// <summary>
        /// Uses the query to retrieve the table data from the database and returns the resulting table.
        /// </summary>
        /// <param name="tableObj">ReportTableInfo object</param>
        /// <param name="parameters">Query parameters</param>
        protected DataSet GetTableDataInternal(ReportTableInfo tableObj, QueryDataParameters parameters)
        {
            DataSet ds = ConnectionHelper.ExecuteQuery(tableObj.TableQuery, parameters, (tableObj.TableQueryIsStoredProcedure ? QueryTypeEnum.StoredProcedure : QueryTypeEnum.SQLQuery));
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                return ds;
            }

            return null;
        }

        #endregion
    }
}