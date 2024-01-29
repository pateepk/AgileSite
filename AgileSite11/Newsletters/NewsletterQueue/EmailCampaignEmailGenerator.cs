using System;
using System.Collections.Generic;
using System.Data;

using CMS.ContactManagement;

namespace CMS.Newsletters
{
    /// <summary>
    /// Provides method for generating emails for given <see cref="IssueInfo"/>.
    /// </summary>
    internal class EmailCampaignEmailGenerator : IEmailCampaignEmailGenerator
    {
        internal int mBatchSize = 10000;


        /// <summary>
        /// Loads all <see cref="ContactInfo"/> subscribed via <see cref="ContactGroupInfo"/> to given <paramref name="issue"/> and fills the given <paramref name="dataTable"/>.
        /// </summary>
        /// <remarks>
        /// It removes email duplicities - e.g. when the <see cref="ContactInfo"/> is subscribed via more <see cref="ContactGroupInfo"/>.
        /// </remarks>
        /// <param name="dataTable">Container to be filled with the generated emails</param>
        /// <param name="issue">Instance of <see cref="IssueInfo"/> the emails are supposed to be generated for</param>
        public void GenerateEmailsForIssue(DataTable dataTable, IssueInfo issue)
        {
            var recipientsProvider = new CampaignEmailRecipientsProvider(issue);
            var contacts = recipientsProvider.GetMarketableRecipients().Columns("ContactID", "ContactEmail");
            var generatedEmails = new HashSet<string>();
            int issueId = issue.IssueIsVariant ? issue.IssueVariantOfIssueID : issue.IssueID;

            contacts.ForEachPage(pagedContacts =>
            {
                foreach (var contact in pagedContacts)
                {
                    if (generatedEmails.Contains(contact.ContactEmail))
                    {
                        continue;
                    }

                    DataRow tableRow = dataTable.NewRow();
                    tableRow["IssueID"] = issueId;
                    tableRow["SiteID"] = issue.IssueSiteID;
                    tableRow["Sending"] = false;
                    tableRow["GUID"] = Guid.NewGuid();
                    tableRow["ContactID"] = contact.ContactID;
                    tableRow["Email"] = contact.ContactEmail;
                    dataTable.Rows.Add(tableRow);

                    generatedEmails.Add(contact.ContactEmail);
                }
            }, mBatchSize);
        }
    }
}
