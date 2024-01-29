using System;

using CMS.Protection;
using CMS.Helpers;
using CMS.Membership;
using CMS.SiteProvider;
using CMS.Localization;
using System.Data;

namespace APIExamples
{
    /// <summary>
    /// Holds abuse report API examples.
    /// </summary>
    /// <pageTitle>Abuse report</pageTitle>
    internal class AbuseReport
    {
        /// <heading>Creating an abuse report</heading>
        private void CreateAbuseReport()
        {
            // Creates a new abuse report object
            AbuseReportInfo newReport = new AbuseReportInfo();

            // Sets the report properties
            newReport.ReportTitle = "NewReport";
            newReport.ReportComment = "This is an example of an abuse report.";

            newReport.ReportURL = URLHelper.GetAbsoluteUrl(RequestContext.CurrentURL);
            newReport.ReportCulture = LocalizationContext.PreferredCultureCode;
            newReport.ReportSiteID = SiteContext.CurrentSiteID;
            newReport.ReportUserID = MembershipContext.AuthenticatedUser.UserID;
            newReport.ReportWhen = DateTime.Now;
            newReport.ReportStatus = AbuseReportStatusEnum.New;

            // Saves the abuse report to the database
            AbuseReportInfoProvider.SetAbuseReportInfo(newReport);
        }


        /// <heading>Updating an abuse report</heading>
        private void GetAndUpdateAbuseReport()
        {
            // Prepares a condition for loading the 'NewReport' abuse report
            string where = "ReportTitle = N'NewReport'";

            // Gets the report
            DataSet reports = AbuseReportInfoProvider.GetAbuseReports(where, null, 0, null);

            if (!DataHelper.DataSourceIsEmpty(reports))
            {
                // Converts the first DataRow to an abuse report object
                AbuseReportInfo updateReport = new AbuseReportInfo(reports.Tables[0].Rows[0]);

                // Updates the abuse report properties
                updateReport.ReportStatus = AbuseReportStatusEnum.Solved;

                // Saves the changes to the database
                AbuseReportInfoProvider.SetAbuseReportInfo(updateReport);
            }
        }


        /// <heading>Updating multiple abuse reports</heading>
        private void GetAndBulkUpdateAbuseReports()
        {
            // Prepares a where condition for loading all abuse reports whose title starts with 'New'
            string where = "ReportTitle LIKE N'New%'";

            // Gets a DataSet containing the abuse reports that fulfill the condition
            DataSet reports = AbuseReportInfoProvider.GetAbuseReports(where, null, 0, null);

            if (!DataHelper.DataSourceIsEmpty(reports))
            {
                // Loops through individual abuse reports
                foreach (DataRow reportDr in reports.Tables[0].Rows)
                {
                    // Converts the DataRow to an abuse report object
                    AbuseReportInfo modifyReport = new AbuseReportInfo(reportDr);

                    // Updates the abuse report properties
                    modifyReport.ReportStatus = AbuseReportStatusEnum.Rejected;

                    // Saves the changes to the database
                    AbuseReportInfoProvider.SetAbuseReportInfo(modifyReport);
                }
            }
        }


        /// <heading>Deleting an abuse report</heading>
        private void DeleteAbuseReport()
        {
            // Prepares a condition for loading the 'NewReport' abuse report
            string where = "ReportTitle = N'NewReport'";

            // Gets the report
            DataSet reports = AbuseReportInfoProvider.GetAbuseReports(where, null, 0, null);

            if (!DataHelper.DataSourceIsEmpty(reports))
            {
                // Converts the first DataRow to an abuse report object
                AbuseReportInfo deleteReport = new AbuseReportInfo(reports.Tables[0].Rows[0]);

                if (deleteReport != null)
                {
                    // Deletes the abuse report
                    AbuseReportInfoProvider.DeleteAbuseReportInfo(deleteReport);
                }
            }
        }
    }
}
