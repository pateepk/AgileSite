using System;
using System.Text.RegularExpressions;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.Base;

namespace CMS.Reporting
{
    /// <summary>
    /// Class providing ReportInfo management.
    /// </summary>
    public class ReportInfoProvider : AbstractInfoProvider<ReportInfo, ReportInfoProvider>
    {
        #region "Public properties"

        // Regular expression to match the old GetReportGraph.aspx url
        private static Regex mGetReportGraphUrlRegExp;

        /// <summary>
        /// Old GetReportGraph.aspx url regular expression (backward compatibility).
        /// </summary>
        public static Regex GetReportGraphUrlRegExp
        {
            get
            {
                if (mGetReportGraphUrlRegExp == null)
                {
                    mGetReportGraphUrlRegExp = RegexHelper.GetRegex("(src=\")(.*?)(/.*?CMSPages/GetReportGraph.aspx)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                }
                return mGetReportGraphUrlRegExp;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        public ReportInfoProvider()
            : base(ReportInfo.TYPEINFO, new HashtableSettings
                {
                    ID = true,
                    Name = true,
                    Load = LoadHashtableEnum.All
                })
        {
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns the ReportInfo structure for the specified report.
        /// </summary>
        /// <param name="reportId">Report ID</param>
        public static ReportInfo GetReportInfo(int reportId)
        {
            return ProviderObject.GetInfoById(reportId);
        }


        /// <summary>
        /// Returns the ReportInfo structure for the specified parameter.
        /// </summary>
        /// <param name="reportName">Report name</param>
        public static ReportInfo GetReportInfo(string reportName)
        {
            return ProviderObject.GetInfoByCodeName(reportName);
        }


        /// <summary>
        /// Gets all reports.
        /// </summary>
        public static ObjectQuery<ReportInfo> GetReports()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Converts ReportItemType to string.
        /// </summary>
        /// <param name="itemType">Item type</param>
        public static string ReportItemTypeToString(ReportItemType itemType)
        {
            switch (itemType)
            {
                case ReportItemType.Graph:
                    return "Graph";

                case ReportItemType.Table:
                    return "Table";

                case ReportItemType.Value:
                    return "Value";

                case ReportItemType.HtmlGraph:
                    return "HtmlGraph";
            }
            throw new Exception("[ReportInfoProvider.ReportItemTypeToString] Unknown report item type.");
        }


        /// <summary>
        /// Convert string to report item type.
        /// </summary>
        /// <param name="type">String name of type</param>
        public static ReportItemType StringToReportItemType(string type)
        {
            switch (type.ToLowerCSafe())
            {
                case "graph":
                    return ReportItemType.Graph;

                case "table":
                    return ReportItemType.Table;

                case "value":
                    return ReportItemType.Value;

                case "htmlgraph":
                    return ReportItemType.HtmlGraph;
            }
            throw new Exception("[ReportInfoProvider.StringToReportItemType] Unknown report item type.");
        }


        /// <summary>
        /// Sets (updates or inserts) specified report.
        /// </summary>
        /// <param name="report">Report to set</param>
        public static void SetReportInfo(ReportInfo report)
        {
            if (report != null)
            {
                int oldCategoryId = 0;
                bool isUpdate = (report.ReportID > 0);

                if (isUpdate)
                {
                    // Get old category ID
                    ReportInfo oldReport = ProviderObject.GetInfoByCodeName(report.ReportName, false);
                    if (oldReport != null)
                    {
                        oldCategoryId = oldReport.ReportCategoryID;
                    }
                }

                // Set report
                ProviderObject.SetInfo(report);

                // Update webpart category children count
                ReportCategoryInfoProvider.UpdateReportCategoryChildCount(oldCategoryId, report.ReportCategoryID);
            }
            else
            {
                throw new Exception("[ReportInfoProvider.SetReportInfo]: No ReportInfo object set.");
            }
        }


        /// <summary>
        /// Deletes specified report.
        /// </summary>
        /// <param name="report">Report object</param>
        public static void DeleteReportInfo(ReportInfo report)
        {
            if (report != null)
            {
                int categoryId = report.ReportCategoryID;

                // Delete the object
                ProviderObject.DeleteInfo(report);

                // Update webpart category children count
                ReportCategoryInfoProvider.UpdateReportCategoryChildCount(0, categoryId);
            }
        }


        /// <summary>
        /// Deletes specified report.
        /// </summary>
        /// <param name="reportId">Report id</param>
        public static void DeleteReportInfo(int reportId)
        {
            ReportInfo reportObj = GetReportInfo(reportId);
            DeleteReportInfo(reportObj);
        }


        /// <summary>
        /// Returns true if the report is in Ecommerce category.
        /// </summary>
        /// <param name="reportName">Name of the report</param>
        public static bool IsEcommerceReport(string reportName)
        {
            return ProviderObject.IsEcommerceReportInternal(reportName);
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Returns true if the report is in E-commerce category.
        /// </summary>
        /// <param name="reportName">Name of the report</param>
        protected virtual bool IsEcommerceReportInternal(string reportName)
        {
            ReportInfo ri = GetReportInfo(reportName);
            if (ri != null)
            {
                ReportCategoryInfo rci = ReportCategoryInfoProvider.GetReportCategoryInfo(ri.ReportCategoryID);
                if (rci != null)
                {
                    ReportCategoryInfo ecommerceCat = ReportCategoryInfoProvider.GetReportCategoryInfo("EcommerceReports");
                    if (ecommerceCat != null)
                    {
                        if (rci.CategoryPath.StartsWithCSafe(ecommerceCat.CategoryPath))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        #endregion
    }
}