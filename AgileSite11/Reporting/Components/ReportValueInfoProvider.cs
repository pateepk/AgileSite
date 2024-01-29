using System;
using System.Data;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.Base;

namespace CMS.Reporting
{
    using TypedDataSet = InfoDataSet<ReportValueInfo>;

    /// <summary>
    /// Class providing ReportValueInfo management.
    /// </summary>
    public class ReportValueInfoProvider : AbstractInfoProvider<ReportValueInfo, ReportValueInfoProvider>
    {
        #region "Public methods"

        /// <summary>
        /// Returns a query for all the <see cref="ReportValueInfo"/> objects.
        /// </summary>
        public static ObjectQuery<ReportValueInfo> GetReportValues()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns object with specified GUID.
        /// </summary>
        /// <param name="guid">Object GUID</param>
        public static ReportValueInfo GetReportValueInfoByGUID(Guid guid)
        {
            return ProviderObject.GetInfoByGuid(guid);
        }


        /// <summary>
        /// Returns the ReportValueInfo structure for the specified reportValue.
        /// </summary>
        /// <param name="reportValueId">ReportValue ID</param>
        public static ReportValueInfo GetReportValueInfo(int reportValueId)
        {
            return ProviderObject.GetInfoById(reportValueId);
        }


        /// <summary>
        /// Returns the ReportValueInfo structure for the specified parameters.
        /// </summary>
        /// <param name="valueName">Value name</param>        
        public static ReportValueInfo GetReportValueInfo(string valueName)
        {
            return ProviderObject.GetReportValueInfoInternal(valueName);
        }


        /// <summary>
        /// Sets (updates or inserts) specified report value.
        /// </summary>
        /// <param name="reportValue">ReportValue to set</param>
        public static void SetReportValueInfo(ReportValueInfo reportValue)
        {
            ProviderObject.SetInfo(reportValue);
        }


        /// <summary>
        /// Deletes specified report value.
        /// </summary>
        /// <param name="reportValueObj">ReportValue object</param>
        public static void DeleteReportValueInfo(ReportValueInfo reportValueObj)
        {
            ProviderObject.DeleteInfo(reportValueObj);
        }


        /// <summary>
        /// Deletes specified report value.
        /// </summary>
        /// <param name="reportValueId">ReportValue ID</param>
        public static void DeleteReportValueInfo(int reportValueId)
        {
            ReportValueInfo reportValueObj = GetReportValueInfo(reportValueId);
            ProviderObject.DeleteInfo(reportValueObj);
        }


        /// <summary>
        /// Uses the query to retrieve the value data from the database and returns the resulting scalar value.
        /// </summary>
        /// <param name="valueObj">ReportValueInfo object</param>
        /// <param name="parameters">Parameters array</param>
        public static DataSet GetValueData(ReportValueInfo valueObj, QueryDataParameters parameters)
        {
            return ProviderObject.GetReportValueDataInternal(valueObj, parameters);
        }


        /// <summary>
        /// Returns DataSet of report values.
        /// </summary>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by columns</param>
        [Obsolete("Use method GetReportValues() instead")]
        public static TypedDataSet GetValues(string where, string orderBy)
        {
            return GetValues(where, orderBy, -1, null);
        }


        /// <summary>
        /// Returns DataSet of report values.
        /// </summary>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by columns</param>
        /// <param name="topN">TOP N rows</param>
        /// <param name="columns">Returned columns</param>
        [Obsolete("Use method GetReportValues() instead")]
        public static TypedDataSet GetValues(string where, string orderBy, int topN, string columns)
        {
            return GetReportValues().Where(where).OrderBy(orderBy).TopN(topN).Columns(columns).BinaryData(true).TypedResult;
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Returns the ReportValueInfo structure for the specified parameters.
        /// </summary>
        /// <param name="valueName">Value name</param>  
        protected ReportValueInfo GetReportValueInfoInternal(string valueName)
        {
            string valueReportName = "";

            if (valueName.IndexOfCSafe('.') >= 0)
            {
                valueReportName = valueName.Remove(valueName.LastIndexOfCSafe('.'));
            }

            valueName = valueName.Remove(0, valueName.LastIndexOfCSafe('.') + 1);

            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@ReportValueName", valueName);
            parameters.Add("@ReportName", valueReportName);

            DataSet ds = ConnectionHelper.ExecuteQuery("Reporting.ReportValue.selectByFullName", parameters);

            ReportValueInfo reportValueObj = null;

            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                reportValueObj = new ReportValueInfo(ds.Tables[0].Rows[0]);
            }

            return reportValueObj;
        }


        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(ReportValueInfo info)
        {
            if (info != null)
            {
                info.SetValue("ValueSettings", info.ValueSettings.GetData());
            }
            base.SetInfo(info);
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(ReportValueInfo info)
        {
            if (info != null)
            {
                // Delete value's subscriptions
                ReportSubscriptionInfoProvider.DeleteSubscriptions("ReportSubscriptionValueID=" + info.ValueID);
                base.DeleteInfo(info);
            }
        }


        /// <summary>
        /// Uses the query to retrieve the value data from the database and returns the resulting scalar value.
        /// </summary>
        /// <param name="valueObj">ReportValueInfo object</param>
        /// <param name="parameters">Parameters array</param>
        protected DataSet GetReportValueDataInternal(ReportValueInfo valueObj, QueryDataParameters parameters)
        {
            DataSet ds = ConnectionHelper.ExecuteQuery(valueObj.ValueQuery, parameters,
                                                     (valueObj.ValueQueryIsStoredProcedure ? QueryTypeEnum.StoredProcedure : QueryTypeEnum.SQLQuery));

            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                return ds;
            }

            return null;
        }

        #endregion
    }
}