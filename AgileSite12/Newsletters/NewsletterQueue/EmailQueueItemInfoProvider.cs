using System;
using System.Collections.Generic;

using CMS.ContactManagement;
using CMS.DataEngine;

namespace CMS.Newsletters
{
    /// <summary>
    /// Class providing <see cref="EmailQueueItemInfo"/> management.
    /// </summary>
    public class EmailQueueItemInfoProvider : AbstractInfoProvider<EmailQueueItemInfo, EmailQueueItemInfoProvider>
    {
        #region "Public methods - Basic"

        /// <summary>
        /// Returns a query for all the EmailQueueItem objects.
        /// </summary>
        public static ObjectQuery<EmailQueueItemInfo> GetEmailQueueItems()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns EmailQueueItem with specified ID.
        /// </summary>
        /// <param name="id">EmailQueueItem ID</param>
        public static EmailQueueItemInfo GetEmailQueueItem(int id)
        {
            return ProviderObject.GetInfoById(id);
        }


        /// <summary>
        /// Returns EmailQueueItem with specified GUID.
        /// </summary>
        /// <param name="guid">EmailQueueItem GUID</param>                
        public static EmailQueueItemInfo GetEmailQueueItem(Guid guid)
        {
            return ProviderObject.GetInfoByGuid(guid);
        }


        /// <summary>
        /// Sets (updates or inserts) specified EmailQueueItem.
        /// </summary>
        /// <param name="infoObj">EmailQueueItem to be set</param>
        public static void SetEmailQueueItem(EmailQueueItemInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified EmailQueueItem.
        /// </summary>
        /// <param name="infoObj">EmailQueueItem to be deleted</param>
        public static void DeleteEmailQueueItem(EmailQueueItemInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified EmailQueueItems.
        /// </summary>
        /// <param name="where">Condition to use for deletion.</param>
        public static void DeleteEmailQueueItem(WhereCondition where)
        {
            ProviderObject.DeleteEmailQueueItemInternal(where);
        }


        /// <summary>
        /// Deletes EmailQueueItem with specified site ID.
        /// </summary>
        /// <param name="siteID">EmailQueueItem ID</param>
        public static void DeleteEmailQueueItem(int siteID)
        {
            var where = new WhereCondition()
                .WhereEquals("EmailSiteID", siteID)
                .WhereEqualsOrNull("EmailSending", false);

            DeleteEmailQueueItem(where);
        }


        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Deletes specified EmailQueueItems.
        /// </summary>
        /// <param name="where">Condition to use for deletion</param>
        protected virtual void DeleteEmailQueueItemInternal(WhereCondition where)
        {
            BulkDelete(where);
        }

        #endregion


        /// <summary>
        /// Moves all email is queue assigned to the given <paramref name="sourceContact"/> to the <paramref name="targetContact"/>.
        /// </summary>
        /// <remarks>
        /// This method should be used only in the merging process. Note that there is no consistency check on whether the contacts with given IDs exist or not (nor is the 
        /// foreign key check in DB). Caller of this method should perform all the neccessary checks prior to the method invocation.
        /// </remarks>
        /// <param name="sourceContact">Contact the emails in queue are moved from</param>
        /// <param name="targetContact">Contact the emails in queue are moved to</param>
        internal static void BulkMoveEmailsInQueueToAnotherContact(ContactInfo sourceContact, ContactInfo targetContact)
        {
            var updateDictionary = new Dictionary<string, object>
            {
                {"EmailContactID", targetContact.ContactID},
                {"EmailAddress", targetContact.ContactEmail}
            };

            var whereCondition = new WhereCondition().WhereEquals("EmailContactID", sourceContact.ContactID);

            ProviderObject.UpdateData(whereCondition, updateDictionary);
        }


        /// <summary>
        /// Clear sending status for the given newsletter issue <paramref name="issue" /> so the sending may start.
        /// </summary>
        /// <param name="issue">Newsletter issue</param>
        internal static void ClearSendingStatus(IssueInfo issue)
        {
            int issueId = issue.IssueIsVariant ? issue.IssueVariantOfIssueID : issue.IssueID;

            ProviderObject.UpdateData(
                new WhereCondition().WhereEquals("EmailNewsletterIssueID", issueId),
                new Dictionary<string, object>
                {
                    { "EmailSending", null }
                }
            );
        }


        /// <summary>
        /// Updates specified items in email queue.
        /// </summary>
        /// <param name="where">Condition to use for updating</param>
        /// <param name="updateOptions">Update parameters.</param>
        internal static void BulkUpdateEmailQueueItems(IWhereCondition where, Dictionary<string, object> updateOptions)
        {
            ProviderObject.UpdateData(where, updateOptions);
        }
    }
}
