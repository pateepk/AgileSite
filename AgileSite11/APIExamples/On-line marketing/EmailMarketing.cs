using CMS.Newsletters;
using CMS.SiteProvider;
using CMS.ContactManagement;

namespace APIExamples
{
    /// <summary>
    /// Holds email marketing API examples.
    /// </summary>
    /// <pageTitle>Email marketing</pageTitle>
    internal class EmailMarketing
    {
        /// <summary>
        /// Holds email marketing template API examples.
        /// </summary>
        /// <groupHeading>Email marketing templates</groupHeading>
        private class EmailTemplates
        {
            /// <heading>Creating an email template</heading>
            private void CreateEmailTemplate()
            {
                // Creates a new template object
                EmailTemplateInfo newTemplate = new EmailTemplateInfo()
                {
                    // Sets the basic template properties
                    TemplateDisplayName = "New email template",
                    TemplateName = "NewEmailTemplate",
                    TemplateSiteID = SiteContext.CurrentSiteID,

                    // Sets the template type (template for marketing email content in this case)
                    TemplateType = EmailTemplateTypeEnum.Issue,
                    // Other possible template type values: EmailTemplateType.Subscription, EmailTemplateType.Unsubscription, EmailTemplateType.DoubleOptIn

                    // Defines the content of the template
                    TemplateCode = @"<html xmlns=""http://www.w3.org/1999/xhtml"">
                                       <head>
                                         <title>Newsletter</title>
                                         <meta http-equiv=""content-type"" content=""text/html; charset=UTF-8"" />
                                       </head>
                                       <body>
                                         Email template content
                                       </body>
                                     </html>"
                };

                // Saves the new template to the database
                EmailTemplateInfoProvider.SetEmailTemplateInfo(newTemplate);
            }


            /// <heading>Updating an email template</heading>
            private void GetAndUpdateIssueTemplate()
            {
                // Gets the email template
                EmailTemplateInfo updateTemplate = EmailTemplateInfoProvider.GetEmailTemplateInfo("NewEmailTemplate", SiteContext.CurrentSiteID);

                if (updateTemplate != null)
                {
                    // Updates the template properties
                    updateTemplate.TemplateDisplayName = updateTemplate.TemplateDisplayName.ToLower();

                    // Saves the updated template to the database
                    EmailTemplateInfoProvider.SetEmailTemplateInfo(updateTemplate);
                }
            }


            /// <heading>Updating multiple email templates</heading>
            private void GetAndBulkUpdateIssueTemplates()
            {
                // Gets all email marketing templates of the "marketing email" type whose code name starts with 'New'
                var templates = EmailTemplateInfoProvider.GetEmailTemplates()
                                                            .WhereEquals("TemplateType", EmailTemplateTypeEnum.Issue)
                                                            .WhereStartsWith("TemplateName", "New");

                // Loops through individual email templates
                foreach (EmailTemplateInfo template in templates)
                {
                    // Updates the template properties
                    template.TemplateDisplayName = template.TemplateDisplayName.ToUpper();

                    // Saves the updated template to the database
                    EmailTemplateInfoProvider.SetEmailTemplateInfo(template);
                }
            }


            /// <heading>Deleting an email template</heading>
            private void DeleteIssueTemplate()
            {
                // Gets the email template
                EmailTemplateInfo deleteTemplate = EmailTemplateInfoProvider.GetEmailTemplateInfo("NewEmailTemplate", SiteContext.CurrentSiteID);

                if (deleteTemplate != null)
                {
                    // Deletes the email template
                    EmailTemplateInfoProvider.DeleteEmailTemplateInfo(deleteTemplate);
                }
            }
        }


        /// <summary>
        /// Holds email feed API examples.
        /// </summary>
        /// <groupHeading>Email feeds</groupHeading>
        private class EmailFeeds
        {
            /// <heading>Creating a newsletter</heading>
            private void CreateNewsletter()
            {
                // Gets templates for the newsletter's emails
                EmailTemplateInfo subscriptionTemplate = EmailTemplateInfoProvider.GetEmailTemplateInfo("SampleSubscriptionEmailTemplate", SiteContext.CurrentSiteID);
                EmailTemplateInfo unsubscriptionTemplate = EmailTemplateInfoProvider.GetEmailTemplateInfo("SampleUnsubscriptionEmailTemplate", SiteContext.CurrentSiteID);
                EmailTemplateInfo emailTemplate = EmailTemplateInfoProvider.GetEmailTemplateInfo("SampleEmailTemplate", SiteContext.CurrentSiteID);

                if ((subscriptionTemplate != null) && (unsubscriptionTemplate != null) && (emailTemplate != null))
                {
                    // Creates a new email feed object
                    NewsletterInfo newNewsletter = new NewsletterInfo()
                    {
                        // Sets the email feed properties to configure a newsletter
                        NewsletterType = EmailCommunicationTypeEnum.Newsletter,
                        NewsletterSource = NewsletterSource.TemplateBased,
                        NewsletterDisplayName = "New newsletter",
                        NewsletterName = "NewNewsletter",
                        NewsletterSenderName = "Sender name",
                        NewsletterSenderEmail = "sender@localhost.local",
                        NewsletterSiteID = SiteContext.CurrentSiteID,

                        // Assigns email templates to the newsletter
                        NewsletterSubscriptionTemplateID = subscriptionTemplate.TemplateID,
                        NewsletterUnsubscriptionTemplateID = unsubscriptionTemplate.TemplateID,
                    };

                    // Saves the new email feed to the database
                    NewsletterInfoProvider.SetNewsletterInfo(newNewsletter);

                    EmailTemplateNewsletterInfo newEmailTemplateNewsletter = new EmailTemplateNewsletterInfo
                    {
                        NewsletterID = newNewsletter.NewsletterID,
                        TemplateID = emailTemplate.TemplateID
                    };

                    EmailTemplateNewsletterInfoProvider.SetEmailTemplateNewsletterInfo(newEmailTemplateNewsletter);
                }
            }

            
            /// <heading>Creating an email campaign</heading>
            private void CreateEmailCampaign()
            {
                // Gets templates for the email campaign
                EmailTemplateInfo subscriptionTemplate = EmailTemplateInfoProvider.GetEmailTemplateInfo("SampleSubscriptionEmailTemplate", SiteContext.CurrentSiteID);
                EmailTemplateInfo unsubscriptionTemplate = EmailTemplateInfoProvider.GetEmailTemplateInfo("SampleUnsubscriptionEmailTemplate", SiteContext.CurrentSiteID);
                EmailTemplateInfo emailTemplate = EmailTemplateInfoProvider.GetEmailTemplateInfo("SampleEmailTemplate", SiteContext.CurrentSiteID);

                if ((subscriptionTemplate != null) && (unsubscriptionTemplate != null) && (emailTemplate != null))
                {
                    // Creates a new email feed object
                    NewsletterInfo newEmailCampaign = new NewsletterInfo()
                    {
                        // Sets the email feed properties to configure an email campaign
                        NewsletterType = EmailCommunicationTypeEnum.EmailCampaign,
                        NewsletterSource = NewsletterSource.TemplateBased,
                        NewsletterDisplayName = "New email campaign",
                        NewsletterName = "NewEmailCampaign",
                        NewsletterSenderName = "Sender name",
                        NewsletterSenderEmail = "sender@localhost.local",
                        NewsletterSiteID = SiteContext.CurrentSiteID,

                        // Assigns templates to the email campaign
                        NewsletterSubscriptionTemplateID = subscriptionTemplate.TemplateID,
                        NewsletterUnsubscriptionTemplateID = unsubscriptionTemplate.TemplateID,
                    };

                    // Saves the new email feed to the database
                    NewsletterInfoProvider.SetNewsletterInfo(newEmailCampaign);

                    EmailTemplateNewsletterInfo newEmailTemplateCampaign = new EmailTemplateNewsletterInfo
                    {
                        NewsletterID = newEmailCampaign.NewsletterID,
                        TemplateID = emailTemplate.TemplateID
                    };

                    EmailTemplateNewsletterInfoProvider.SetEmailTemplateNewsletterInfo(newEmailTemplateCampaign);
                }
            }


            /// <heading>Updating an email feed (any type)</heading>
            private void GetAndUpdateEmailFeed()
            {
                // Gets the email feed
                NewsletterInfo updateEmailFeed = NewsletterInfoProvider.GetNewsletterInfo("NewNewsletter", SiteContext.CurrentSiteID);

                if (updateEmailFeed != null)
                {
                    // Updates the email feed properties
                    updateEmailFeed.NewsletterDisplayName = updateEmailFeed.NewsletterDisplayName.ToLower();

                    // Saves the updated email feed to the database
                    NewsletterInfoProvider.SetNewsletterInfo(updateEmailFeed);
                }
            }


            /// <heading>Updating multiple email feeds (any type)</heading>
            private void GetAndBulkUpdateEmailFeeds()
            {
                // Gets all email feeds on the current site whose code name starts with 'New'
                var emailFeeds = NewsletterInfoProvider.GetNewsletters()
                                                                .WhereEquals("NewsletterSiteID", SiteContext.CurrentSiteID)
                                                                .WhereStartsWith("NewsletterName", "New");

                // Loops through individual email feeds
                foreach (NewsletterInfo emailFeed in emailFeeds)
                {
                    // Updates the email feed properties
                    emailFeed.NewsletterDisplayName = emailFeed.NewsletterDisplayName.ToUpper();

                    // Saves the updated email feed to the database
                    NewsletterInfoProvider.SetNewsletterInfo(emailFeed);
                }
            }


            /// <heading>Deleting an email feed (any type)</heading>
            private void DeleteEmailFeed()
            {
                // Gets the email feed
                NewsletterInfo deleteEmailFeed = NewsletterInfoProvider.GetNewsletterInfo("NewNewsletter", SiteContext.CurrentSiteID);

                if (deleteEmailFeed != null)
                {
                    // Deletes the email feed
                    NewsletterInfoProvider.DeleteNewsletterInfo(deleteEmailFeed);
                }
            }
        }


        /// <summary>
        /// Holds marketing email API examples.
        /// </summary>
        /// <groupHeading>Marketing emails</groupHeading>
        private class Emails
        {
            /// <heading>Creating marketing emails</heading>
            private void CreateTemplateEmail()
            {
                // Gets the email feed (Newsletter or Email campaign)
                NewsletterInfo emailFeed = NewsletterInfoProvider.GetNewsletterInfo("NewNewsletter", SiteContext.CurrentSiteID);

                EmailTemplateNewsletterInfo assignedTemplate = EmailTemplateNewsletterInfoProvider.GetEmailTemplateNewsletters()
                    .WhereEquals("NewsletterID", emailFeed.NewsletterID)
                    .FirstObject;

                if (emailFeed != null)
                {
                    // Creates a new email object
                    IssueInfo newIssue = new IssueInfo()
                    {
                        // Sets the email properties
                        IssueDisplayName = "New issue name",
                        IssueSubject = "New issue subject",
                        IssueNewsletterID = emailFeed.NewsletterID,
                        IssueSiteID = SiteContext.CurrentSiteID,
                        IssueStatus = IssueStatusEnum.Idle,
                        IssueUnsubscribed = 0,
                        IssueSentEmails = 0,
                        IssueTemplateID = assignedTemplate.TemplateID,
                        IssueUseUTM = false,
                        IssueText = string.Empty,

                        // Defines the email content
                        // This XML is being generated by the Email builder
                        IssueWidgets = "<?xml version=\"1.0\" encoding=\"utf-16\"?>"
                    };

                    // Saves the marketing email to the database
                    IssueInfoProvider.SetIssueInfo(newIssue);
                }
            }
            

            /// <heading>Updating marketing emails</heading>
            private void GetAndUpdateMarketingEmails()
            {
                // Gets the email feed
                NewsletterInfo emailFeed = NewsletterInfoProvider.GetNewsletterInfo("NewNewsletter", SiteContext.CurrentSiteID);

                if (emailFeed != null)
                {
                    // Gets all of the feed's emails that have not been sent yet
                    var issues = IssueInfoProvider.GetIssues()
                                                        .WhereEquals("IssueNewsletterID", emailFeed.NewsletterID)
                                                        .WhereNull("IssueStatus");

                    // Loops through individual emails
                    foreach (IssueInfo issue in issues)
                    {
                        // Updates the email properties
                        issue.IssueSubject = issue.IssueSubject.ToUpper();

                        // Saves the modified email to the database
                        IssueInfoProvider.SetIssueInfo(issue);
                    }
                }
            }


            /// <heading>Deleting marketing emails</heading>
            private void DeleteMarketingEmails()
            {
                // Gets the email feed
                NewsletterInfo emailFeed = NewsletterInfoProvider.GetNewsletterInfo("NewNewsletter", SiteContext.CurrentSiteID);

                if (emailFeed != null)
                {
                    // Gets all of the feed's emails that were already sent
                    var issues = IssueInfoProvider.GetIssues()
                                                        .WhereEquals("IssueNewsletterID", emailFeed.NewsletterID)
                                                        .WhereEquals("IssueStatus", IssueStatusEnum.Finished);

                    // Loops through individual emails
                    foreach (IssueInfo deleteIssue in issues)
                    {
                        // Deletes the email
                        IssueInfoProvider.DeleteIssueInfo(deleteIssue);
                    }
                }
            }
        }


        /// <summary>
        /// Holds email recipient API examples.
        /// </summary>
        /// <groupHeading>Recipients</groupHeading>
        private class Recipients
        {
            /// <heading>Adding newsletter recipients by email address</heading>
            private void AddEmailAddressRecipient()
            {
                // Prepares the email address of the new recipient
                string emailAddress = "subscriber@localhost.local";

                // Gets the newsletter
                NewsletterInfo newsletter = NewsletterInfoProvider.GetNewsletterInfo("NewNewsletter", SiteContext.CurrentSiteID);

                if (newsletter != null)
                {
                    // Prepares instances of the Kentico contact provider and email feed subscription service
                    IContactProvider contactProvider = CMS.Core.Service.Resolve<IContactProvider>();
                    ISubscriptionService subscriptionService = CMS.Core.Service.Resolve<ISubscriptionService>();

                    // Either gets an existing contact by email address or creates a new contact with the given email address
                    ContactInfo contact = contactProvider.GetContactForSubscribing(emailAddress);

                    // Adds the contact as a recipient of the newsletter
                    subscriptionService.Subscribe(contact, newsletter, new SubscribeSettings
                    {
                        AllowOptIn = true, // Allows double opt-in subscription for newsletters that have it enabled
                        SendConfirmationEmail = true, // Allows sending of confirmation emails for the subscription

                        // Removes the email address from the opt-out list for all marketing emails on the given site (if present)
                        RemoveAlsoUnsubscriptionFromAllNewsletters = true,
                    });
                }
            }


            /// <heading>Adding contacts as newsletter recipients</heading>
            private void AddContactRecipient()
            {
                // Gets all contacts whose last name is 'Smith'
                var contacts = ContactInfoProvider.GetContacts()
                                                    .WhereEquals("ContactLastName", "Smith");

                // Gets the newsletter
                NewsletterInfo newsletter = NewsletterInfoProvider.GetNewsletterInfo("NewNewsletter", SiteContext.CurrentSiteID);

                if (newsletter != null)
                {
                    // Prepares an instance of the Kentico email feed subscription service
                    ISubscriptionService subscriptionService = CMS.Core.Service.Resolve<ISubscriptionService>();

                    // Loops through the contacts
                    foreach (ContactInfo contact in contacts)
                    {
                        // Adds the contact as a recipient of the newsletter
                        subscriptionService.Subscribe(contact, newsletter, new SubscribeSettings
                        {
                            AllowOptIn = true, // Allows double opt-in subscription for newsletters that have it enabled
                            SendConfirmationEmail = true, // Allows sending of confirmation emails for the subscription

                            // Removes the contact from the opt-out list for all marketing emails on the given site (if present)
                            RemoveAlsoUnsubscriptionFromAllNewsletters = true,
                        });
                    }
                }
            }


            /// <heading>Adding newsletter recipients via contact groups</heading>
            private void AddContactGroupRecipient()
            {
                // Gets the contact group
                ContactGroupInfo contactGroup = ContactGroupInfoProvider.GetContactGroupInfo("GroupName");

                // Gets the newsletter
                NewsletterInfo newsletter = NewsletterInfoProvider.GetNewsletterInfo("NewNewsletter", SiteContext.CurrentSiteID);

                if ((contactGroup != null) && (newsletter != null))
                {
                    // Prepares an instance of the Kentico email feed subscription service
                    ISubscriptionService subscriptionService = CMS.Core.Service.Resolve<ISubscriptionService>();

                    // Adds the contact group as a recipient of the newsletter
                    subscriptionService.Subscribe(contactGroup, newsletter);
                }
            }


            /// <heading>Unsubscribing an email address from a newsletter</heading>
            private void UnsubscribeRecipient()
            {
                // Prepares the email address that you wish to unsubscribe
                string emailAddress = "subscriber@localhost.local";

                // Gets the newsletter
                NewsletterInfo newsletter = NewsletterInfoProvider.GetNewsletterInfo("NewNewsletter", SiteContext.CurrentSiteID);

                if (newsletter != null)
                {
                    // Prepares an instance of the Kentico email feed subscription service
                    ISubscriptionService subscriptionService = CMS.Core.Service.Resolve<ISubscriptionService>();

                    // Ensures that the email address no longer receives the newsletter's emails
                    // Leaves the email address in the newsletter's list of recipients, but with the "Opted out" status
                    subscriptionService.UnsubscribeFromSingleNewsletter(emailAddress, newsletter.NewsletterID, null, sendConfirmationEmail: true);
                }
            }


            /// <heading>Removing a newsletter recipient</heading>
            private void RemoveRecipient()
            {
                // Gets the subscriber that you wish to remove from the list of recipients (by email address)
                SubscriberInfo subscriber = SubscriberInfoProvider.GetSubscriberByEmail("subscriber@localhost.local", SiteContext.CurrentSiteID);

                // Gets the newsletter
                NewsletterInfo newsletter = NewsletterInfoProvider.GetNewsletterInfo("NewNewsletter", SiteContext.CurrentSiteID);

                if ((subscriber != null) && (newsletter != null))
                {
                    // Prepares an instance of the Kentico email feed subscription service
                    ISubscriptionService subscriptionService = CMS.Core.Service.Resolve<ISubscriptionService>();

                    // Removes the subscriber from the newsletter's recipients
                    subscriptionService.RemoveSubscription(subscriber.SubscriberID, newsletter.NewsletterID, sendConfirmationEmail: true);
                }
            }


            /// <heading>Adding email campaign recipients (contact groups)</heading>
            private void AddEmailCampaignIssueRecipient()
            {
                // Gets the contact group
                ContactGroupInfo contactGroup = ContactGroupInfoProvider.GetContactGroupInfo("GroupName");

                // Gets the email campaign
                NewsletterInfo emailCampaign = NewsletterInfoProvider.GetNewsletterInfo("NewEmailCampaign", SiteContext.CurrentSiteID);

                if ((contactGroup != null) && (emailCampaign != null))
                {
                    // Gets the campaign's email with the subject "New issue"
                    IssueInfo campaignEmail = IssueInfoProvider.GetIssues()
                                                        .WhereEquals("IssueNewsletterID", emailCampaign.NewsletterID)
                                                        .WhereEquals("IssueSubject", "New issue")
                                                        .FirstObject;

                    if (campaignEmail != null)
                    {
                        // Creates an object representing a relationship between the campaign email and contact group
                        var emailContactGroupBinding = new IssueContactGroupInfo
                        {
                            ContactGroupID = contactGroup.ContactGroupID,
                            IssueID = campaignEmail.IssueID
                        };

                        // Assigns the contact group as a recipient for the campaign email
                        IssueContactGroupInfoProvider.SetIssueContactGroupInfo(emailContactGroupBinding);
                    }
                }
            }


            /// <heading>Removing email campaign recipients (contact groups)</heading>
            private void RemoveEmailCampaignIssueRecipient()
            {
                // Gets the contact group
                ContactGroupInfo contactGroup = ContactGroupInfoProvider.GetContactGroupInfo("GroupName");

                // Gets the email campaign
                NewsletterInfo emailCampaign = NewsletterInfoProvider.GetNewsletterInfo("NewEmailCampaign", SiteContext.CurrentSiteID);

                if ((contactGroup != null) && (emailCampaign != null))
                {
                    // Gets the campaign's email with the subject "New issue"
                    IssueInfo campaignEmail = IssueInfoProvider.GetIssues()
                                                        .WhereEquals("IssueNewsletterID", emailCampaign.NewsletterID)
                                                        .WhereEquals("IssueSubject", "New issue")
                                                        .FirstObject;

                    if (campaignEmail != null)
                    {
                        // Gets the object representing the relationship between the campaign email and contact group
                        IssueContactGroupInfo emailContactGroupBinding =
                            IssueContactGroupInfoProvider.GetIssueContactGroupInfo(campaignEmail.IssueID, contactGroup.ContactGroupID);

                        if (emailContactGroupBinding != null)
                        {
                            // Removes the contact group from the campaign email's recipient list
                            IssueContactGroupInfoProvider.DeleteIssueContactGroupInfo(emailContactGroupBinding);
                        }
                    }
                }
            }
        }
    }
}
