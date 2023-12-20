using System;
using System.Collections.Generic;
using System.Data;

using CMS.ContactManagement;
using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.Newsletters
{
    /// <summary>
    /// Provides methods to get contacts for whom the newsletter should be send.
    /// </summary>
    internal class EmailQueueRecipientCandidatesRetriever : IEmailQueueRecipientCandidatesRetriever
    {
        private readonly IEmailAddressBlocker emailAddressBlocker;
        private readonly IssueInfo issue;
        private readonly bool monitorBounces;
        private readonly int bounceLimit;


        /// <summary>
        /// Creates instance of <see cref="EmailQueueRecipientCandidatesRetriever"/>.
        /// </summary>
        /// <param name="emailAddressBlocker">Check if email address is not blocked.</param>
        /// <param name="issue">Issue for which the contacts should be retrieved.</param>
        /// <param name="monitorBounces">Indicates if bounces should be monitored.</param>
        /// <param name="bounceLimit">Bounces threshold.</param>
        public EmailQueueRecipientCandidatesRetriever(IEmailAddressBlocker emailAddressBlocker, 
                                                      IssueInfo issue, 
                                                      bool monitorBounces, 
                                                      int bounceLimit)
        {
            this.emailAddressBlocker = emailAddressBlocker;
            this.issue = issue;
            this.monitorBounces = monitorBounces;
            this.bounceLimit = bounceLimit;
        }


        /// <summary>
        /// Fills given table with contact data from subscribed contacts and contact groups.
        /// </summary>
        /// <param name="table">Data table.</param>
        /// <param name="siteId">Site ID.</param>
        /// <param name="generatedEmails">E-mail addresses that are to be ignored. The method also fills this set with generated e-mail addresses.</param>
        public void FillContactTable(DataTable table, int siteId, HashSet<string> generatedEmails)
        {
            DataSet members = GetSubscribers(PredefinedObjectType.CONTACT, issue.IssueNewsletterID)
                                  .NotBounced("SubscriberBounces", monitorBounces, bounceLimit)
                                  .Columns("Newsletter_Subscriber.SubscriberID", "SubscriberRelatedID AS ContactID");

            if (!DataHelper.DataSourceIsEmpty(members))
            {
                // Set "ContactID" column as primary key
                members.Tables[0].PrimaryKey = new[] { members.Tables[0].Columns[1] };

                // Get list of IDs of subscribed contacts
                var contactIds = DataHelper.GetUniqueValues(members.Tables[0], "ContactID", false);

                var maxCount = 10000;

                do
                {
                    // Get number of items to work with
                    maxCount = (contactIds.Count > maxCount) ? maxCount : contactIds.Count;

                    // Get maximally 10000 contact IDs from the collect
                    var tempIds = contactIds.GetRange(0, maxCount);
                    contactIds.RemoveRange(0, maxCount);

                    // Get emails of subscribed contacts
                    var contacts = ContactInfoProvider.GetContacts()
                        .Columns("ContactID", "ContactEmail")
                        .WhereIn("ContactID", tempIds)
                        .Where(w => w.WhereNot(new WhereCondition(w).WhereNull("ContactEmail").Or().WhereEquals("ContactEmail", "")))
                        .NotBounced("ContactBounces", monitorBounces, bounceLimit)
                        .TypedResult;

                    if (!DataHelper.DataSourceIsEmpty(contacts))
                    {
                        // Merge data from both tables
                        var reader = new DataTableReader(contacts.Tables[0]);
                        members.Tables[0].Load(reader);
                        members.AcceptChanges();
                    }
                }
                while (contactIds.Count > 0);

                var issueId = issue.IssueIsVariant ? issue.IssueVariantOfIssueID : issue.IssueID;
                
                if (members.Tables[0].Columns.Contains("ContactEmail"))
                { 
                    // Fill the table
                    foreach (DataRow contact in members.Tables[0].Rows)
                    {
                        // Get subscriber ID, contact ID and email address
                        var subscriberId = ValidationHelper.GetInteger(contact["SubscriberID"], 0);
                        var contactId = ValidationHelper.GetInteger(contact["ContactID"], 0);
                        var email = ValidationHelper.GetString(contact["ContactEmail"], string.Empty);
                        
                        if (string.IsNullOrEmpty(email) || generatedEmails.Contains(email) || emailAddressBlocker.IsBlocked(email))
                        {
                            continue;
                        }

                        // Create new row in the table
                        var tableRow = table.NewRow();
                        tableRow["IssueID"] = issueId;
                        tableRow["SubscriberID"] = subscriberId;
                        tableRow["SiteID"] = siteId;
                        tableRow["Sending"] = true;
                        tableRow["GUID"] = Guid.NewGuid();
                        tableRow["ContactID"] = contactId;
                        tableRow["Email"] = email;
                        table.Rows.Add(tableRow);

                        // Store current email to check duplicates
                        generatedEmails.Add(email);
                    }
                }
            }
            
            // Get IDs of all subscribed contact groups
            members = GetSubscribers(PredefinedObjectType.CONTACTGROUP, issue.IssueNewsletterID)
                          .NotBounced("SubscriberBounces", monitorBounces, bounceLimit)
                          .Columns("Newsletter_Subscriber.SubscriberID", "SubscriberRelatedID");

            if (DataHelper.DataSourceIsEmpty(members))
            {
                return;
            }

            foreach (DataRow group in members.Tables[0].Rows)
            {
                // Get subscriber and contact group IDs
                var subscriberId = ValidationHelper.GetInteger(group[0], 0);
                var contactGroupId = ValidationHelper.GetInteger(group[1], 0);

                FillContactGroupTable(table, contactGroupId, subscriberId, generatedEmails, true);
            }
        }


        /// <summary>
        /// Fills given table with contact data from specified contact group.
        /// </summary>
        /// <param name="table">Data table.</param>
        /// <param name="contactGroupId">Contact group ID.</param>
        /// <param name="subscriberId">Subscriber ID.</param>
        /// <param name="currentEmails">Hash table with emails that have been already added.</param>
        /// <param name="setSending">Indicate if new records should be set with 'sending' status.</param>
        public void FillContactGroupTable(DataTable table, int contactGroupId, int subscriberId, HashSet<string> currentEmails, bool setSending)
        {
            if (currentEmails == null)
            {
                currentEmails = new HashSet<string>();
            }

            var siteId = issue.IssueSiteID;
            var issueId = issue.IssueIsVariant ? issue.IssueVariantOfIssueID : issue.IssueID;

            // Get IDs and emails of all valid contact group members
            DataSet contacts = ContactGroupMemberInfoProvider.GetContactGroupMembers(contactGroupId, monitorBounces, bounceLimit)
                                                             .Columns("ContactID", "ContactEmail");

            if (DataHelper.DataSourceIsEmpty(contacts))
            {
                return;
            }

            foreach (DataRow contact in contacts.Tables[0].Rows)
            {
                // Get contact ID and email address
                var contactId = ValidationHelper.GetInteger(contact[0], 0);
                var email = ValidationHelper.GetString(contact[1], string.Empty);

                // Check the email's uniqueness in the table
                if (!string.IsNullOrEmpty(email) && (!currentEmails.Contains(email)) && !emailAddressBlocker.IsBlocked(email))
                {
                    // Create new row in the table
                    var tableRow = table.NewRow();
                    tableRow["IssueID"] = issueId;
                    tableRow["SubscriberID"] = subscriberId;
                    tableRow["SiteID"] = siteId;
                    tableRow["Sending"] = setSending;
                    tableRow["GUID"] = Guid.NewGuid();
                    tableRow["ContactID"] = contactId;
                    tableRow["Email"] = email;
                    table.Rows.Add(tableRow);

                    // Store current email to check duplicates
                    currentEmails.Add(email);
                }
            }
        }


        private static ObjectQuery<SubscriberInfo> GetSubscribers(string subscriberType, int newsletterId)
        {
            return SubscriberInfoProvider.GetSubscribers()
                                         .Source(s => s.InnerJoin<SubscriberNewsletterInfo>("Newsletter_Subscriber.SubscriberID", "SubscriberID"))
                                         .WhereEquals("NewsletterID", newsletterId)
                                         .Where(w => w.WhereTrue("SubscriptionApproved")
                                                      .Or()
                                                      .WhereNull("SubscriptionApproved"))
                                         .WhereEquals("SubscriberType", subscriberType);
        }
    }
}
