using System;

using CMS.Core;
using CMS.FormEngine.Web.UI;
using CMS.Helpers;
using CMS.Membership;

namespace CMS.Newsletters.Web.UI
{
    /// <summary>
    /// Helper class for Email Builder.
    /// </summary>
    public static class EmailBuilderHelper
    {
        /// <summary>
        /// Name of UI element that represents the Email builder UI.
        /// </summary>
        public const string EMAIL_BUILDER_UI_ELEMENT = "Newsletter.Issue.Content";


        /// <summary>
        /// Gets the URL for an email issue to be displayed in the Email Builder.
        /// </summary>
        /// <param name="newsletterID">Newsletter identifier.</param>
        /// <param name="issueID">Issue identifier.</param>
        /// <param name="selectedTabIndex">Index of a tab that should be preselected.</param>
        /// <param name="includeSaveMessage">Indicates whether save message parameter should be included in the URL.</param>
        /// <returns></returns>
        public static string GetNavigationUrl(int newsletterID, int issueID, int selectedTabIndex = 0, bool includeSaveMessage = false)
        {
            var navigationUrl = UIContextHelper.GetElementUrl("CMS.Newsletter", EMAIL_BUILDER_UI_ELEMENT);
            navigationUrl = URLHelper.AddParameterToUrl(navigationUrl, "parentobjectid", newsletterID.ToString());
            navigationUrl = URLHelper.AddParameterToUrl(navigationUrl, "objectid", issueID.ToString());
            navigationUrl = URLHelper.AddParameterToUrl(navigationUrl, "selectedtabindex", selectedTabIndex.ToString());

            if (includeSaveMessage)
            {
                navigationUrl = URLHelper.AddParameterToUrl(navigationUrl, "saved", "1");
            }

            return navigationUrl;
        }


        /// <summary>
        /// Returns Email builder URL to the email variant which is considered as 'original'. Original variant is a clone of email which is A/B tested.
        /// </summary>
        /// <param name="issue">The issue.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="issue"/> is null.</exception>
        public static string GetOriginalVariantIssueUrl(IssueInfo issue)
        {
            if (issue == null)
            {
                throw new ArgumentNullException(nameof(issue));
            }

            var abTestService = Service.Resolve<IEmailABTestService>();
            var originalVariant = abTestService.GetOriginalVariant(issue.IssueID);
            string originalVariantUrl = EmailBuilderHelper.GetNavigationUrl(issue.IssueNewsletterID, originalVariant.IssueID);

            return originalVariantUrl;
        }


        /// <summary>
        /// Indicates whether the email builder should display editable UI to given <paramref name="user"/> or UI in a read-only mode. 
        /// Returns false if given <paramref name="issue"/> has different status than <see cref="IssueStatusEnum.Idle"/>.  
        /// </summary>
        /// <param name="issue">Issue info</param>
        /// <param name="user">User info</param>
        /// <param name="siteName">Site name</param>
        public static bool IsIssueEditableByUser(IssueInfo issue, UserInfo user, string siteName)
        {
            return (issue.IssueStatus == IssueStatusEnum.Idle) && user.IsAuthorizedPerResource(ModuleName.NEWSLETTER, "AuthorIssues", siteName);
        }
    }
}
