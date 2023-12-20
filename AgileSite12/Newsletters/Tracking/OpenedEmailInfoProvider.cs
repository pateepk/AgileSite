using System;

using CMS.Base;
using CMS.DataEngine;

namespace CMS.Newsletters
{
    using TypedDataSet = InfoDataSet<OpenedEmailInfo>;

    /// <summary>
    /// Class providing OpenedEmailInfoProvider management.
    /// </summary>
    /// <remarks>
    /// Handles basic CRUD functionality over OpenedEmailInfo objects.
    /// </remarks>
    public class OpenedEmailInfoProvider : AbstractInfoProvider<OpenedEmailInfo, OpenedEmailInfoProvider>
    {
        #region "Constructors"

        /// <summary>
        /// Creates a new instance of OpenedEmailInfoProvider.
        /// </summary>
        public OpenedEmailInfoProvider()
            : base(OpenedEmailInfo.TYPEINFO)
        {
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Returns a query for all the OpenedEmailInfo objects.
        /// </summary>
        public static ObjectQuery<OpenedEmailInfo> GetOpenedEmails()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Retrieves the OpenedEmailInfo object for newsletter issue and subscriber identified by email.
        /// </summary>
        /// <param name="issueId">ID of the newsletter issue</param>
        /// <param name="subscriberEmail">Email of the subscriber</param>
        /// <returns><see cref="OpenedEmailInfo" /> object</returns>
        public static ObjectQuery<OpenedEmailInfo> GetOpenedEmails(int issueId, string subscriberEmail)
        {
            return ProviderObject.GetOpenedEmailsInternal(issueId, subscriberEmail);
        }


        /// <summary>
        /// Gets a dataset with OpenedEmailInfo records of a specified subscriber.
        /// </summary>
        /// <param name="subscriberEmail">Subscriber's email</param>        
        /// <returns>A DataSet containing the OpenedEmailInfo records</returns>
        public static TypedDataSet GetSubscribersOpenedEmails(string subscriberEmail)
        {
            if (String.IsNullOrEmpty(subscriberEmail))
            {
                return null;
            }
            return ProviderObject.GetObjectQuery().WhereEquals("OpenedEmailEmail", subscriberEmail).TypedResult;
        }


        /// <summary>
        /// Gets a dataset with OpenedEmailInfo records of a specified issue.
        /// </summary>
        /// <param name="issueId">Id of the issue</param>
        /// <returns>A DataSet containing the OpenedEmailInfo records</returns>
        public static TypedDataSet GetIssueOpeners(int issueId)
        {
            if (issueId <= 0)
            {
                return null;
            }
            return ProviderObject.GetObjectQuery().WhereEquals("OpenedEmailIssueID", issueId).TypedResult;
        }


        /// <summary>
        /// Saves the OpenedEmailInfo object.
        /// </summary>
        /// <param name="infoObj">An object to save</param>
        public static void SetOpenedEmailInfo(OpenedEmailInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes the OpenedEmailInfo object.
        /// </summary>
        /// <param name="infoObj">An object to delete</param>
        public static void DeleteOpenedEmailInfo(OpenedEmailInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Logs e-mail with newsletter issue as opened by specified subscriber.
        /// </summary>
        /// <param name="email">Subscriber's email</param>
        /// <param name="issueId">Newsletter issue ID</param>
        /// <returns>ID of the opened newsletter issue, 0 if not affected</returns>
        public static int LogOpenedEmail(string email, int issueId)
        {
            return ProviderObject.LogOpenedEmailInternal(email, issueId);
        }


        /// <summary>
        /// Logs e-mail with newsletter issue as opened by specified subscriber.
        /// </summary>
        /// <param name="subscriberGuid">Subscriber's GUID</param>
        /// <param name="issueGuid">GUID of the newsletter issue</param>
        /// <param name="siteId">ID of the site that owns the newsletter</param>
        /// <returns>ID of the opened newsletter issue, 0 if not affected</returns>
        public static int LogOpenedEmail(Guid subscriberGuid, Guid issueGuid, int siteId)
        {
            // Get subscriber
            SubscriberInfo subscriber = SubscriberInfoProvider.GetSubscriberInfo(subscriberGuid, siteId);
            if (subscriber == null)
            {
                return 0;
            }

            // Get issue
            IssueInfo issue = IssueInfoProvider.GetIssueInfo(issueGuid, siteId);
            if (issue == null)
            {
                return 0;
            }
            return ProviderObject.LogOpenedEmailInternal(subscriber.SubscriberEmail, issue.IssueID);
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Retrieves the OpenedEmailInfo object for newsletter issue and subscriber identified by email.
        /// </summary>
        /// <param name="issueId">ID of the newsletter issue</param>
        /// <param name="subscriberEmail">Email of the subscriber</param>
        /// <returns>OpenedEmailInfo object</returns>
        protected virtual ObjectQuery<OpenedEmailInfo> GetOpenedEmailsInternal(int issueId, string subscriberEmail)
        {
            if ((issueId <= 0) || String.IsNullOrEmpty(subscriberEmail))
            {
                return null;
            }
            return GetObjectQuery()
                    .WhereEquals("OpenedEmailIssueID", issueId)
                    .WhereEquals("OpenedEmailEmail", subscriberEmail);
        }


        /// <summary>
        /// Logs e-mail with newsletter issue as opened by specified subscriber.
        /// </summary>
        /// <param name="subscriberEmail">Subscriber's email</param>
        /// <param name="issueId">Newsletter issue ID</param>
        /// <returns>ID of the opened newsletter issue if it is a unique open, 0 if it is not a unique open.</returns>
        protected virtual int LogOpenedEmailInternal(string subscriberEmail, int issueId)
        {
            var openedCount = GetOpenedEmails().WhereEquals("OpenedEmailEmail", subscriberEmail)
                                               .And()
                                               .WhereEquals("OpenedEmailIssueID", issueId).Count;

            if (openedCount == 0)
            {
                // Do not create new version on email open
                using (new CMSActionContext { CreateVersion = false })
                {
                    SetOpenedEmailInfo(new OpenedEmailInfo
                    {
                        OpenedEmailEmail = subscriberEmail,
                        OpenedEmailIssueID = issueId,
                        OpenedEmailTime = DateTime.Now,
                    });
                }

                return issueId;
            }

            return 0;
        }

        #endregion
    }
}