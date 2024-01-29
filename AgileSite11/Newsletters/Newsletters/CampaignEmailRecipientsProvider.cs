using System;

using CMS.ContactManagement;
using CMS.DataEngine;
using CMS.SiteProvider;

namespace CMS.Newsletters
{
    internal class CampaignEmailRecipientsProvider : IRecipientsProvider
    {
        private readonly IssueInfo mIssue;

        public CampaignEmailRecipientsProvider(IssueInfo issue)
        {
            if (issue == null)
            {
                throw new ArgumentNullException("issue");
            }

            mIssue = issue;
        }


        /// <summary>
        /// Return all contacts which are subscribed to an issue, includes opted out contacts and bounced.
        /// </summary>
        public ObjectQuery<ContactInfo> GetAllRecipients()
        {
            return ContactInfoProvider.GetContacts()
                                      .WhereIn(
                                          "ContactID",
                                          ContactGroupMemberInfoProvider.GetRelationships()
                                                                        .Column("ContactGroupMemberRelatedID")
                                                                        .WhereEquals("ContactGroupMemberType", (int)ContactGroupMemberTypeEnum.Contact)
                                                                        .WhereIn(
                                                                            "ContactGroupMemberContactGroupID",
                                                                            IssueContactGroupInfoProvider.GetIssueContactGroups()
                                                                                                         .Column("ContactGroupID")
                                                                                                         .WhereEquals("IssueID", mIssue.IssueIsVariant ? mIssue.IssueVariantOfIssueID : mIssue.IssueID)
                                                                        )
                                      )
                                      .WhereNotEmpty("ContactEmail");
        }


        /// <summary>
        /// Return all contacts which are subscribed to the issue, excludes opted out contacts and bounced.
        /// </summary>
        public ObjectQuery<ContactInfo> GetMarketableRecipients()
        {
            var siteName = ((SiteInfo)mIssue.Site).SiteName;
            var bounceLimit = NewsletterHelper.BouncedEmailsLimit(siteName);

            return GetAllRecipients().WithoutUnsubscribed(mIssue.IssueNewsletterID).WithoutBounces(bounceLimit);
        }
    }
}
