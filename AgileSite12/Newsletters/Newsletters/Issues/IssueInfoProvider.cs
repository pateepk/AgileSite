using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Base;
using CMS.DataEngine;
using CMS.DataEngine.Query;

namespace CMS.Newsletters
{
    /// <summary>
    /// Class providing Issue management.
    /// </summary>
    public class IssueInfoProvider : AbstractInfoProvider<IssueInfo, IssueInfoProvider>
    {
        #region "Properties"

        /// <summary>
        /// Determines whether "link" tag is used instead of "style" tag.
        /// </summary>
        public static bool UseLinkedCSS
        {
            get;
            set;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        public IssueInfoProvider()
            : base(IssueInfo.TYPEINFO, new HashtableSettings
            {
                ID = true
            })
        {
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Returns a query for all the IssueInfo objects.
        /// </summary>
        public static ObjectQuery<IssueInfo> GetIssues()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns the issue with specified ID.
        /// </summary>
        /// <param name="issueId">Issue ID</param>
        public static IssueInfo GetIssueInfo(int issueId)
        {
            return ProviderObject.GetInfoById(issueId);
        }


        /// <summary>
        /// Returns the issue with specified GUID.
        /// </summary>
        /// <param name="issueGuid">Issue GUID</param>
        /// <param name="siteId">Site ID</param>
        public static IssueInfo GetIssueInfo(Guid issueGuid, int siteId)
        {
            return ProviderObject.GetIssueInfoInternal(issueGuid, siteId);
        }


        /// <summary>
        /// Returns original (parent) issue for specified issue ID.
        /// </summary>
        /// <param name="issueId">Issue ID (either variant issue or parent</param>
        public static IssueInfo GetOriginalIssue(int issueId)
        {
            IssueInfo issue = GetIssueInfo(issueId);
            if (issue != null)
            {
                if (issue.IssueVariantOfIssueID > 0)
                {
                    return GetIssueInfo(issue.IssueVariantOfIssueID);
                }
                return issue;
            }
            return null;
        }


        /// <summary>
        /// Sets (updates or inserts) specified issue.
        /// </summary>
        /// <param name="issue">Issue to set</param>
        public static void SetIssueInfo(IssueInfo issue)
        {
            ProviderObject.SetInfo(issue);
        }


        /// <summary>
        /// Deletes issue.
        /// </summary>
        /// <param name="issueObj">Issue</param>
        public static void DeleteIssueInfo(IssueInfo issueObj)
        {
            ProviderObject.DeleteInfo(issueObj);
        }


        /// <summary>
        /// Deletes issue specified by ID.
        /// </summary>
        /// <param name="issueId">Issue ID</param>
        public static void DeleteIssueInfo(int issueId)
        {
            IssueInfo issueObj = GetIssueInfo(issueId);
            DeleteIssueInfo(issueObj);
        }


        /// <summary>
        /// Increases the sent emails count in the issue specified.
        /// </summary>
        /// <param name="issueId">Issue ID</param>
        /// <param name="emails">Number of sent emails to add</param>
        /// <param name="mailoutTime">Time when the e-mails were sent</param>
        public static void AddSentEmails(int issueId, int emails, DateTime mailoutTime)
        {
            ProviderObject.AddSentEmailsInternal(issueId, emails, mailoutTime);
        }


        /// <summary>
        /// Increases the counter for issue unsubscriptions.
        /// </summary>
        /// <param name="issueId">ID of the issue that has been unsubscribed</param>
        public static void IncreaseUnsubscribeCount(int issueId)
        {
            ProviderObject.IncreaseUnsubscribeCountInternal(issueId);
        }


        /// <summary>
        /// Increases the opened emails count int the issue specified.
        /// </summary>
        /// <param name="issueId">ID of the issue</param>
        public static void AddOpenedEmails(int issueId)
        {
            ProviderObject.AddOpenedEmailsInternal(issueId);
        }


        /// <summary>
        /// Invalidates cached newsletter issue on external change.
        /// </summary>
        /// <param name="issueId">ID of the issue to invalidate</param>
        public static void InvalidateIssue(int issueId)
        {
            ProviderObject.TypeInfo.ObjectInvalidated(issueId);
        }


        /// <summary>
        /// Set specified status to the issue.
        /// </summary>
        /// <param name="issueId">Issue ID</param>
        /// <param name="newStatus">New status</param>
        public static void SetIssueStatus(int issueId, IssueStatusEnum newStatus)
        {
            ProviderObject.SetIssueStatusInternal(issueId, newStatus);
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Gets the open rate percentage for this issue (how many e-mails were open out of all sent e-mails).
        /// </summary>
        internal static double OpenRate(IssueInfo issue)
        {
            if (issue == null)
            {
                return 0;
            }

            return (issue.IssueSentEmails == 0 ? 0 : ((double)issue.IssueOpenedEmails / issue.IssueSentEmails) * 100);
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Returns the issue with specified GUID.
        /// </summary>
        /// <param name="issueGuid">Issue GUID</param>
        /// <param name="siteId">Site ID</param>
        protected virtual IssueInfo GetIssueInfoInternal(Guid issueGuid, int siteId)
        {
            if ((issueGuid == Guid.Empty) || (siteId <= 0))
            {
                return null;
            }

            var where = new WhereCondition().WhereEquals("IssueGUID", issueGuid).WhereEquals("IssueSiteID", siteId);

            return GetIssues().Where(where).TopN(1).BinaryData(true).FirstOrDefault();
        }


        /// <summary>
        /// Increases the sent emails count in the issue specified.
        /// </summary>
        /// <param name="issueId">Issue ID</param>
        /// <param name="emails">Number of sent emails to add</param>
        /// <param name="mailoutTime">Time when the e-mails were sent</param>
        protected virtual void AddSentEmailsInternal(int issueId, int emails, DateTime mailoutTime)
        {
            if (emails > 0)
            {
                var updateDictionary = new Dictionary<string, object>
                {
                    {"IssueSentEmails", ("IssueSentEmails + " + emails).AsExpression()},
                    {"IssueMailoutTime", mailoutTime}
                };

                var whereCondition = new WhereCondition()
                    .WhereEquals("IssueID", issueId);

                UpdateData(whereCondition, updateDictionary);
            }
        }


        /// <summary>
        /// Increases the opened emails count in the issue specified.
        /// </summary>
        /// <param name="issueId">ID of the issue</param>
        protected virtual void AddOpenedEmailsInternal(int issueId)
        {
            if (issueId > 0)
            {
                var parameters = new QueryDataParameters();
                parameters.Add("@IssueID", issueId);

                ConnectionHelper.ExecuteQuery("newsletter.issue.addopenedemails", parameters);

                // Issue might be cached, make sure the cache is updated
                InvalidateIssue(issueId);
            }
        }


        /// <summary>
        /// Increases the counter for issue unsubscriptions.
        /// </summary>
        /// <param name="issueId">ID of the issue that has been unsubscribed</param>
        protected virtual void IncreaseUnsubscribeCountInternal(int issueId)
        {
            var parameters = new QueryDataParameters();
            parameters.Add("@IssueID", issueId);

            ConnectionHelper.ExecuteQuery("newsletter.issue.unsubscribe", parameters);

            // Issue might be cached, make sure the cache is updated
            InvalidateIssue(issueId);
        }


        /// <summary>
        /// Sets specified status to the issue.
        /// </summary>
        /// <param name="issueId">Issue ID</param>
        /// <param name="status">New status</param>
        protected virtual void SetIssueStatusInternal(int issueId, IssueStatusEnum status)
        {
            // Get issue
            IssueInfo issue = GetIssueInfo(issueId);
            if (issue != null)
            {
                using (var context = new CMSActionContext())
                {
                    // Do not create new version
                    context.CreateVersion = false;

                    // Do not perform redirect, log license exception
                    context.AllowLicenseRedirect = false;

                    // Change status and save the issue
                    issue.IssueStatus = status;
                    SetIssueInfo(issue);

                    // Remove issue from cache
                    InvalidateIssue(issueId);
                }
            }
        }

        #endregion
    }
}