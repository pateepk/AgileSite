using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.MacroEngine;
using CMS.Modules;

using Newtonsoft.Json;

namespace CMS.Newsletters
{
    /// <summary>
    /// Issue helper class.
    /// </summary>
    public class IssueHelper : AbstractHelper<IssueHelper>
    {
        #region "Public methods"

        /// <summary>
        /// Returns issue status friendly name.
        /// </summary>
        /// <param name="status">Issue status</param>
        /// <param name="resPrefix">Resource string prefix (if NULL default prefix will be used)</param>
        public static string GetStatusFriendlyName(IssueStatusEnum status, string resPrefix)
        {
            if (String.IsNullOrEmpty(resPrefix))
            {
                resPrefix = "newsletterissuestatus.";
            }

            return ResHelper.GetString(resPrefix + status);
        }


        #endregion


        #region "A/B test methods"

        /// <summary>
        /// Initializes number of e-mails that should be sent for each variant.
        /// </summary>
        /// <param name="parentId">ID of A/B test parent issue</param>
        internal static void InitABTestNumbers(int parentId)
        {
            // Get A/B test info for given issue
            ABTestInfo abi = ABTestInfoProvider.GetABTestInfoForIssue(parentId);
            if (abi != null)
            {
                abi.TestNumberPerVariantEmails = GetNumberPerVariantEmails(abi);
                ABTestInfoProvider.SetABTestInfo(abi);
            }
        }


        /// <summary>
        /// Returns number of e-mails to be sent to a variant of specified A/B test.
        /// </summary>
        /// <param name="abi">A/B test info</param>
        private static int GetNumberPerVariantEmails(ABTestInfo abi)
        {
            if ((abi == null) || (abi.TestSizePercentage <= 0))
            {
                return 0;
            }

            // Get number of all variants
            List<IssueABVariantItem> variants = GetIssueVariants(abi.TestIssueID, null);
            if ((variants != null) && (variants.Count > 0))
            {
                int variantsCount = variants.Count;
                // Get number of items in newsletter queue for the specified issue
                int totalCount = EmailQueueItemInfoProvider.GetEmailQueueItems().Where("EmailNewsletterIssueID", QueryOperator.Equals, abi.TestIssueID).TotalRecords;

                // Number of e-mails for entire test group
                double allEmails = ((double)abi.TestSizePercentage / 100) * totalCount;
                // Number of e-mails to sent according to number of variants
                int emailsToSent = (int)Math.Floor(allEmails / variantsCount);

                if (emailsToSent > totalCount)
                {
                    emailsToSent = totalCount;
                }
                return emailsToSent;
            }
            return 0;
        }


        /// <summary>
        /// Returns A/B test winner.
        /// </summary>
        /// <param name="parentIssue">Parent issue</param>
        /// <param name="abi">A/B test info</param>
        public static IssueInfo GetWinnerIssue(IssueInfo parentIssue, ABTestInfo abi)
        {
            if ((parentIssue == null) || (abi == null))
            {
                return null;
            }

            if (abi.TestWinnerOption == ABTestWinnerSelectionEnum.Manual)
            {
                return null;
            }

            // Get the A/B test variants
            List<IssueABVariantItem> variants = GetIssueVariants(parentIssue, null);
            int winnerVariantId = 0;
            bool equalResults = true;

            switch (abi.TestWinnerOption)
            {
                // Select winner according to number of opened emails
                case ABTestWinnerSelectionEnum.OpenRate:
                    {
                        int winnerOpens = 0;

                        foreach (IssueABVariantItem item in variants)
                        {
                            // Get variant issue and compare number of opened emails
                            IssueInfo variant = IssueInfoProvider.GetIssueInfo(item.IssueID);
                            if (variant != null)
                            {
                                // Get number of opened emails of the variant
                                int variantOpens = variant.IssueOpenedEmails;

                                if (winnerOpens < variantOpens)
                                {
                                    // Set new winner variant
                                    winnerOpens = variantOpens;
                                    winnerVariantId = item.IssueID;
                                    equalResults = false;
                                }
                                else if (winnerOpens == variantOpens)
                                {
                                    // More variants have equal results
                                    equalResults = true;
                                }
                            }
                        }
                    }
                    break;
                // Select winner according to number of total unique clicks
                case ABTestWinnerSelectionEnum.TotalUniqueClicks:
                    {
                        int winnerClicks = 0;

                        foreach (IssueABVariantItem item in variants)
                        {
                            // Get number of unique clicks of all variant's links and compare the values
                            int variantClicks = GetIssueTotalUniqueClicks(item.IssueID);

                            if (winnerClicks < variantClicks)
                            {
                                // Set new winner variant
                                winnerClicks = variantClicks;
                                winnerVariantId = item.IssueID;
                                equalResults = false;
                            }
                            else if (winnerClicks == variantClicks)
                            {
                                // More variants have equal results
                                equalResults = true;
                            }
                        }
                    }
                    break;
            }

            IssueInfo winner = null;
            if (!equalResults)
            {
                // Get winner variant issue; if all variants have equal results -> winner selection will be postponed by an hour
                winner = IssueInfoProvider.GetIssueInfo(winnerVariantId);
            }

            return winner;
        }


        /// <summary>
        /// Returns number of unique clicks of all issue's links.
        /// </summary>
        /// <param name="issueId">Issue ID</param>
        public static int GetIssueTotalUniqueClicks(int issueId)
        {
            var clickedLinksSummary = ClickedLinkInfoProvider.GetClickedLinks()
            .Column(new AggregatedColumn(AggregationType.Count, "Distinct(ClickedLinkEmail)"))
            .Source(s => s.Join<LinkInfo>("ClickedLinkNewsletterLinkID", "LinkID"))
            .WhereEquals("LinkIssueID", issueId)
            .GroupBy("ClickedLinkEmail")
            .GetListResult<int>();

            return clickedLinksSummary.Sum();
        }


        /// <summary>
        /// Copies properties of the <paramref name="winningVariantIssue"/> to the <paramref name="targetIssue"/>.
        /// </summary>
        /// <param name="winningVariantIssue">Winning variant issue</param>
        /// <param name="targetIssue">Target issue</param>
        public static void CopyWinningVariantIssueProperties(IssueInfo winningVariantIssue, IssueInfo targetIssue)
        {
            if (winningVariantIssue == null || targetIssue == null)
            {
                return;
            }

            targetIssue.IssueSubject = winningVariantIssue.IssueSubject;
            targetIssue.IssueText = winningVariantIssue.IssueText;
            targetIssue.IssueWidgets = winningVariantIssue.IssueWidgets;
            targetIssue.IssueTemplateID = winningVariantIssue.IssueTemplateID;
            targetIssue.IssueSenderName = winningVariantIssue.IssueSenderName;
            targetIssue.IssueSenderEmail = winningVariantIssue.IssueSenderEmail;
        }


        /// <summary>
        /// Returns issue variants for specified issue ID if any.
        /// </summary>
        /// <param name="issueId">Issue ID</param>
        /// <param name="additionalWhereCondition">Additional WHERE condition</param>
        /// <remarks>
        /// The first item in the list is always the original variant.
        /// </remarks>
        public static List<IssueABVariantItem> GetIssueVariants(int issueId, string additionalWhereCondition)
        {
            return GetIssueVariants(IssueInfoProvider.GetIssueInfo(issueId), additionalWhereCondition);
        }


        /// <summary>
        /// Prepares JSON object to be inserted to the breadcrumbs.
        /// On some pages the breadcrumbs needs to be hard-coded in order to be able to access single email via link and ensure consistency of breadcrumbs.
        /// </summary>
        /// <param name="issue">Issue</param>
        /// <param name="newsletter">Newsletter</param>
        /// <returns>Serialized list of objects where every object represents single breadcrumb.</returns>
        public static string GetBreadcrumbsData(IssueInfo issue, NewsletterInfo newsletter = null)
        {
            var breadcrumbsList = new List<dynamic>();
            newsletter = newsletter ?? NewsletterInfoProvider.GetNewsletterInfo(issue.IssueNewsletterID);

            // Root application
            var rootUIElement = UIElementInfoProvider.GetUIElementInfo("CMS.Newsletter", "Newsletter");
            string rootRedirectUrl = URLHelper.ResolveUrl(ApplicationUrlHelper.GetApplicationUrl(rootUIElement)) + "&ignorehash=1";

            breadcrumbsList.Add(new
            {
                text = MacroResolver.Resolve(rootUIElement.ElementDisplayName),
                redirectUrl = rootRedirectUrl,
                isRoot = true
            });

            // Email campaign listing
            var listingUIElement = UIElementInfoProvider.GetUIElementInfo("CMS.Newsletter", "EditNewsletterProperties");
            string listingUrl = issue == null ? "" : string.Format("{0}&elementguid={1}&objectid={2}", rootRedirectUrl, listingUIElement.ElementGUID, newsletter.NewsletterID);
            string listingSuffix = issue == null ? newsletter.GetNiceName() : "";

            var listingBreadcrumb = new
            {
                text = HTMLHelper.HTMLEncode(newsletter.NewsletterDisplayName),
                redirectUrl = listingUrl,
                suffix = HTMLHelper.HTMLEncode(listingSuffix)
            };

            breadcrumbsList.Add(listingBreadcrumb);

            if (issue != null)
            {
                // Email
                string displayName = issue.IssueDisplayName;
                breadcrumbsList.Add(new
                {
                    text = HTMLHelper.HTMLEncode(displayName),
                    suffix = ResHelper.GetString("objecttype.newsletter_issue")
                });
            }

            return JsonConvert.SerializeObject(new { data = breadcrumbsList }, new JsonSerializerSettings { StringEscapeHandling = StringEscapeHandling.EscapeHtml });
        }


        /// <summary>
        /// Returns issue variants for specified issue if any.
        /// </summary>
        /// <param name="issue">Issue</param>
        /// <param name="additionalWhereCondition">Additional WHERE condition</param>
        /// <remarks>
        /// The first item in the list is always the original variant.
        /// </remarks>
        public static List<IssueABVariantItem> GetIssueVariants(IssueInfo issue, string additionalWhereCondition)
        {
            var result = new List<IssueABVariantItem>();

            if (issue == null)
            {
                return result;
            }

            if (!issue.IssueIsABTest)
            {
                return result;
            }

            int issueId = issue.IssueID;
            if (issue.IssueVariantOfIssueID > 0)
            {
                issueId = issue.IssueVariantOfIssueID;
            }
            string where = "IssueVariantOfIssueID=" + issueId;
            if (!String.IsNullOrEmpty(additionalWhereCondition))
            {
                where = SqlHelper.AddWhereCondition(where, additionalWhereCondition);
            }

            // Get A/B test and retrieve winner issue ID
            int winnerIssueId = 0;
            ABTestInfo abi = ABTestInfoProvider.GetABTestInfoForIssue(issueId);
            if (abi != null)
            {
                winnerIssueId = abi.TestWinnerIssueID;
            }

            var issues = IssueInfoProvider.GetIssues()
                                          .Columns("IssueID", "IssueVariantName", "IssueStatus", "IssueVariantOfIssueID", "IssueIsABTest")
                                          .Where(where)
                                          .OrderBy("IssueVariantName", "IssueID");
            if (issues.Any())
            {
                foreach (var issueObj in issues)
                {
                    int issueVariantId = issueObj.IssueID;
                    string variantName = issueObj.GetVariantName();
                    IssueStatusEnum issueStatus = issueObj.IssueStatus;
                    result.Add(new IssueABVariantItem(issueVariantId, variantName, issueVariantId == winnerIssueId, issueStatus));
                }
            }
            return result;
        }

        #endregion
    }
}