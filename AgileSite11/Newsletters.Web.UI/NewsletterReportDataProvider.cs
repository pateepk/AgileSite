using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS.DataEngine;
using CMS.DataEngine.Query;
using CMS.Helpers;
using CMS.SiteProvider;

namespace CMS.Newsletters.Web.UI
{
    /// <summary>
    /// Provides methods to serve data for newsletter report.
    /// </summary>
    internal class NewsletterReportDataProvider : INewsletterReportDataProvider
    {
        /// <summary>
        /// Represents association between main issue (key) and all its variants (value).
        /// </summary>
        private Dictionary<int, ICollection<int>> issueVariantsByParentId;

        /// <summary>
        /// Collection of all issue IDs related to the newsletter.
        /// </summary>
        private IList<int> issueIDs;

        /// <summary>
        /// Collection IDs that represents main issues (ie. not variant issues).
        /// </summary>
        private IList<int> onlyParentIssueIDs;

        /// <summary>
        /// Represents association between main issue ID and sum of 'opens' of its variants.
        /// </summary>
        private Dictionary<int, int> variantOpens;

        /// <summary>
        /// Name of site the newsletter belongs to.
        /// </summary>
        private string siteName = String.Empty;


        private class ClicksModel
        {
            public int UniqueClicks;
            public int EmailId;
        }


        /// <summary>
        /// Returns statistical data of email in the newsletter given by <paramref name="newsletterId"/>.
        /// </summary>
        /// <param name="newsletterId">ID of the email feed where the email belong.</param>
        public IEnumerable<NewsletterEmailsDataViewModel> GetEmailsData(int newsletterId)
        {
            var newsletter = NewsletterInfoProvider.GetNewsletterInfo(newsletterId);

            if (newsletter == null)
            {
                throw new ArgumentException("Given parameter represents an invalid object.", nameof(newsletterId));
            }

            siteName = SiteInfoProvider.GetSiteName(newsletter.NewsletterSiteID);
            LoadIssuesAndVariantsData(newsletterId);

            if ((issueIDs == null) || !issueIDs.Any())
            {
                return Enumerable.Empty<NewsletterEmailsDataViewModel>();
            }

            var mainEmails = GetMainEmails();
            var result = new List<NewsletterEmailsDataViewModel>();

            if (mainEmails.Any())
            {
                // Get 'Clicks' using separate query because of possibility of separated database.
                var clicks = GetClicksForEmails();
                var clicksDictionary = clicks.ToDictionary(c => c.EmailId);

                foreach (var issue in mainEmails)
                {
                    int issueId = issue.IssueID;
                    int sentOrDeliveredIssues = GetDeliveredCount(issue);

                    var model = GetModelFromIssue(issue);

                    model.Clicks = GetClicksForIssue(clicksDictionary, issueId);
                    model.ClickRate = (sentOrDeliveredIssues > 0) ? GetDecimalRate(model.Clicks, sentOrDeliveredIssues) : 0.00m;

                    model.Opens = GetOpensForIssue(issue);
                    model.OpenRate = (sentOrDeliveredIssues > 0) ? GetDecimalRate(model.Opens, sentOrDeliveredIssues) : 0.00m; ;

                    model.UnsubscribedRate = (sentOrDeliveredIssues > 0) ? GetDecimalRate(issue.IssueUnsubscribed, sentOrDeliveredIssues) : 0.00m;

                    result.Add(model);
                }
            }

            return result;
        }


        private int GetOpensForIssue(IssueInfo issue)
        {
            int issueId = issue.IssueID;
            int issueOpens = issue.IssueOpenedEmails;

            if (variantOpens.ContainsKey(issueId))
            {
                issueOpens += variantOpens[issueId];
            }

            return issueOpens;
        }


        private int GetClicksForIssue(Dictionary<int, ClicksModel> clicksDictionary, int issueId)
        {
            int clicksCount = 0;

            if (clicksDictionary.ContainsKey(issueId))
            {
                clicksCount = clicksDictionary[issueId].UniqueClicks;
            }

            // Get sum of variant clicks
            if (issueVariantsByParentId.ContainsKey(issueId))
            {
                int variantClicks = issueVariantsByParentId[issueId].Sum(i =>
                {
                    if (clicksDictionary.ContainsKey(i))
                    {
                        return clicksDictionary[i].UniqueClicks;
                    }

                    return 0;
                });

                clicksCount += variantClicks;
            }

            return clicksCount;
        }


        private DataSet GetAllIssuesForNewsletter(int newsletterId)
        {
            var issuesQuery = IssueInfoProvider
                .GetIssues()
                .Columns("IssueID", "IssueVariantOfIssueID")
                .WhereEquals("IssueNewsletterID", newsletterId)
                .WhereNotNull("IssueMailoutTime");

            return issuesQuery.Result;
        }


        private void LoadDataForVariants(int newsletterId)
        {
            // Returns number of opens for all variants for each issue
            DataSet variantIssueSummaries = IssueInfoProvider.GetIssues()
                .Columns(new QueryColumn("IssueVariantOfIssueID"),
                         new AggregatedColumn(AggregationType.Sum, "IssueOpenedEmails").As("OpenedEmailsSum"))
                .WhereEquals("IssueNewsletterID", newsletterId)
                .WhereNotNull("IssueOpenedEmails")
                .GroupBy("IssueVariantOfIssueID")
                .Having("IssueVariantOfIssueID IS NOT NULL");

            variantOpens = new Dictionary<int, int>();

            foreach (DataRow row in variantIssueSummaries.Tables[0].Rows)
            {
                int issueId = row.Field<int>("IssueVariantOfIssueID");
                int opensSummary = row.Field<int>("OpenedEmailsSum");
                variantOpens.Add(issueId, opensSummary);
            }
        }


        private void LoadIssuesAndVariantsData(int newsletterId)
        {
            var issuesDataset = GetAllIssuesForNewsletter(newsletterId);

            // Don't continue if newsletter has no issues
            if (DataHelper.DataSourceIsEmpty(issuesDataset))
            {
                return;
            }

            LoadDataForIssuesAndHelperCollections(issuesDataset);
            LoadDataForVariants(newsletterId);
        }


        private void LoadDataForIssuesAndHelperCollections(DataSet issuesDataset)
        {
            issueVariantsByParentId = new Dictionary<int, ICollection<int>>();
            onlyParentIssueIDs = new List<int>();
            var issueIDsHash = new HashSet<int>();

            foreach (DataRow row in issuesDataset.Tables[0].Rows)
            {
                int currentIssueId = DataHelper.GetIntValue(row, "IssueID");
                issueIDsHash.Add(currentIssueId);
                int parentId = DataHelper.GetIntValue(row, "IssueVariantOfIssueID");

                if (parentId > 0)
                {
                    if (issueVariantsByParentId.ContainsKey(parentId))
                    {
                        ICollection<int> innerCollection = issueVariantsByParentId[parentId];
                        innerCollection.Add(currentIssueId);
                    }
                    else
                    {
                        var innerList = new List<int>();
                        innerList.Add(currentIssueId);
                        issueVariantsByParentId.Add(parentId, innerList);
                    }
                }
                else
                {
                    onlyParentIssueIDs.Add(currentIssueId);
                }
            }

            issueIDs = issueIDsHash.ToList();
        }


        private decimal GetDecimalRate(int numerator, int denominator)
        {
            decimal rate = (decimal)numerator / denominator * 100;
            return Convert.ToDecimal(rate.ToString("0.00"));
        }


        private int GetDeliveredCount(IssueInfo issue)
        {
            if (issue == null)
            {
                return 0;
            }

            if (!NewsletterHelper.MonitorBouncedEmails(siteName))
            {
                return issue.IssueSentEmails;
            }

            return issue.IssueSentEmails - issue.IssueBounces;
        }


        /// <summary>
        /// Returns a collection of main emails.
        /// </summary>
        private IEnumerable<IssueInfo> GetMainEmails()
        {
            var mainIssuesQuery = IssueInfoProvider
                .GetIssues()
                .Columns("IssueID", "IssueMailoutTime", "IssueDisplayName", "IssueSentEmails", "IssueUnsubscribed", "IssueOpenedEmails", "IssueBounces")
                .WhereIn("IssueID", onlyParentIssueIDs)
                .OrderByAscending("IssueMailoutTime");

            return mainIssuesQuery.ToList();
        }


        private IEnumerable<ClicksModel> GetClicksForEmails()
        {
            // Involve all issues of the newsletter to obtain also data for AB variants
            var clicksQuery = ClickedLinkInfoProvider
                .GetClickedLinks()
                .Source(s => s.Join<LinkInfo>("ClickedLinkNewsletterLinkID", "LinkID"))
                .Columns(
                    new AggregatedColumn(AggregationType.Count, "DISTINCT(ClickedLinkEmail)").As("UniqueClicks"),
                    "LinkIssueID".AsColumn().As("IssueID")
                )
                .WhereIn("LinkIssueID", issueIDs)
                .GroupBy("LinkIssueID");

            return clicksQuery.Select(GetClicksModel);
        }


        private NewsletterEmailsDataViewModel GetModelFromIssue(IssueInfo email)
        {
            var model = new NewsletterEmailsDataViewModel
            {
                Date = email.IssueMailoutTime,
                Name = email.IssueDisplayName,
                Opens = email.IssueOpenedEmails,
                Sent = email.IssueSentEmails,
                Unsubscribed = email.IssueUnsubscribed
            };

            return model;
        }


        private ClicksModel GetClicksModel(DataRow row)
        {
            var model = new ClicksModel();

            if (ContainsColumn(row, "UniqueClicks"))
            {
                model.UniqueClicks = row.Field<int>("UniqueClicks");
            }

            if (ContainsColumn(row, "IssueID"))
            {
                model.EmailId = row.Field<int>("IssueID");
            }

            return model;
        }


        private static bool ContainsColumn(DataRow row, string columnName)
        {
            return (row.Table.Columns.IndexOf(columnName) >= 0);
        }
    }
}