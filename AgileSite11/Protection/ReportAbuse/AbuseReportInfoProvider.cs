using System;
using System.Data;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.Base;

namespace CMS.Protection
{
    /// <summary>
    /// Class providing AbuseReportInfo management.
    /// </summary>
    public class AbuseReportInfoProvider : AbstractInfoProvider<AbuseReportInfo, AbuseReportInfoProvider>
    {
        #region "Constants"

        /// <summary>
        /// Object types which are supported to be displayed in details.
        /// </summary>
        private const string SUPPORTED_OBJECT_TYPES = PredefinedObjectType.BLOGCOMMENT + ";" + PredefinedObjectType.BOARDMESSAGE + ";" + PredefinedObjectType.FORUMPOST + ";";

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns the AbuseReportInfo structure for the specified abuseReport.
        /// </summary>
        /// <param name="abuseReportId">AbuseReport id</param>
        public static AbuseReportInfo GetAbuseReportInfo(int abuseReportId)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@ID", abuseReportId);

            // Get the data
            DataSet ds = ConnectionHelper.ExecuteQuery("CMS.AbuseReport.select", parameters);
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                return new AbuseReportInfo(ds.Tables[0].Rows[0]);
            }

            return null;
        }


        /// <summary>
        /// Sets (updates or inserts) specified abuseReport.
        /// </summary>
        /// <param name="abuseReport">AbuseReport to set</param>
        public static void SetAbuseReportInfo(AbuseReportInfo abuseReport)
        {
            if (abuseReport != null)
            {
                if (abuseReport.ReportID > 0)
                {
                    abuseReport.Generalized.UpdateData();
                }
                else
                {
                    abuseReport.Generalized.InsertData();
                }
            }
            else
            {
                throw new Exception("[AbuseReportInfoProvider.SetAbuseReportInfo]: No AbuseReportInfo object set.");
            }
        }


        /// <summary>
        /// Deletes specified abuseReport.
        /// </summary>
        /// <param name="infoObj">AbuseReport object</param>
        public static void DeleteAbuseReportInfo(AbuseReportInfo infoObj)
        {
            if (infoObj != null)
            {
                infoObj.Generalized.DeleteData();
            }
        }


        /// <summary>
        /// Deletes specified abuseReport.
        /// </summary>
        /// <param name="abuseReportId">AbuseReport id</param>
        public static void DeleteAbuseReportInfo(int abuseReportId)
        {
            AbuseReportInfo infoObj = GetAbuseReportInfo(abuseReportId);
            DeleteAbuseReportInfo(infoObj);
        }


        /// <summary>
        /// Returns Dataset with AbuseReportInfo.
        /// </summary>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by expression</param>
        /// <param name="topN">Restricts maximum number of rows, for all rows use 0</param>
        /// <param name="columns">Select only specified columns</param>
        public static DataSet GetAbuseReports(string where, string orderBy, int topN, string columns)
        {
            return ConnectionHelper.ExecuteQuery("CMS.AbuseReport.selectall", null, where, orderBy, topN, columns);
        }


        /// <summary>
        /// Indicates if given obejct type is supproted to show details.
        /// </summary>
        /// <param name="objectType">Object type</param>
        public static bool IsObjectTypeSupported(string objectType)
        {
            if (!string.IsNullOrEmpty(objectType))
            {
                return SUPPORTED_OBJECT_TYPES.ToLowerCSafe().Contains(objectType.ToLowerCSafe() + ";");
            }
            return false;
        }

        #endregion
    }
}