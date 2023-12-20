using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Base;
using CMS.ContactManagement;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Newsletters;
using CMS.SiteProvider;

namespace CMS.DancingGoat.Samples
{
    /// <summary>
    /// Contains methods for generating sample data for the Newsletters module.
    /// </summary>
    internal class NewslettersDataGenerator
    {
        private readonly NewsletterActivityGenerator mNewsletterActivityGenerator = new NewsletterActivityGenerator();

        /// <summary>
        /// Code name of Colombia coffee promotion newsletter.
        /// </summary>
        public const string NEWSLETTER_COLOMBIA_COFFEE_PROMOTION = "ColombiaCoffeePromotion";

        /// <summary>
        /// Code name of Colombia coffee promotion sample newsletter.
        /// </summary>
        public const string NEWSLETTER_COLOMBIA_COFFEE_PROMOTION_SAMPLE = "ColombiaCoffeeSamplePromotion";

        /// <summary>
        /// Code name of Coffee club membership newsletter.
        /// </summary>
        public const string NEWSLETTER_COFFEE_CLUB_MEMBERSHIP = "CoffeeClubMembership";

        private const string EMAIL_CAMPAIGN_CONTACT_GROUP = "CoffeeClubMembershipRecipients";

        private readonly SiteInfo mSite;
        private readonly Random rand = new Random(DateTime.Now.Millisecond);
        private readonly String[] mSubscriberNames = 
        { 
            "Deneen Fernald", "Antonio Buker", "Marlon Loos", "Nolan Steckler", "Johnetta Tall",
            "Florence Ramsdell", "Modesto Speaker", "Alissa Ferguson", "Calvin Hollier", "Diamond Paik",
            "Mardell Dohrmann", "Dinorah Clower", "Andrea Humbert", "Tyrell Galvan", "Yong Inskeep",
            "Tom Goldschmidt", "Kimbery Rincon", "Genaro Kneeland", "Roselyn Mulvey", "Nancee Jacobson",
            "Jaime Link", "Fonda Belnap", "Muoi Ishmael", "Pearlene Minjarez", "Eustolia Studstill",
            "Marilynn Manos", "Pamila Turnbow", "Lieselotte Satcher", "Sharron Mellon", "Bennett Heatherington",
            "Spring Hessel", "Lashay Blazier", "Veronika Lecuyer", "Mark Spitz", "Peggy Olson",
            "Tyron Bednarczyk", "Terese Betty", "Bibi Kling", "Bruno Spier", "Cristen Bussey",
            "Daine Pridemore", "Gerald Turpen", "Lela Briese", "Sharda Bonnie", "Omar Martin",
            "Marlyn Pettie", "Shiela Cleland", "Marica Granada", "Garland Reagan", "Mora Gillmore",
            "Mariana Rossow", "Betty Pollan", "Analisa Costilla", "Evelyn Mendez", "April Rubino",
            "Zachariah Roberson", "Sheilah Steinhauser", "Araceli Vallance", "Lashawna Weise", "Charline Durante",
            "Melania Nightingale", "Ema Stiltner", "Lynelle Threet", "Dorcas Cully", "Gregg Carranco",
            "Karla Heiner", "Judson Siegmund", "Alyson Oday", "Winston Laxton", "Jarod Turrentine",
            "Israel Shanklin", "Miquel Jorstad", "Brianne Darrow", "Tamara Rulison", "Elliot Rameriz",
            "Gearldine Nova", "Debi Fritts", "Leota Cape", "Tyler Saleem", "Starr Hyden",
            "Loreen Spigner", "Raisa Germain", "Grace Vigue", "Maryann Munsch", "Jason Chon",
            "Gisele Mcquillen", "Juliane Comeaux", "Willette Dodrill", "Sherril Weymouth", "Ashleigh Dearman",
            "Bret Bourne", "Brittney Cron", "Lashell Hampson", "Barbie Dinwiddie", "Ricki Wiener",
            "Bess Pedretti", "Lisha Raley", "Edgar Schuetz", "Jettie Boots", "Jefferson Hinkle" 
        };


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="site">Site the newsletters data will be generated for</param>
        public NewslettersDataGenerator(SiteInfo site)
        {
            mSite = site;
        }


        /// <summary>
        /// Performs newsletters sample data generation.
        /// </summary>
        public void Generate()
        {
            GenerateSubscribersForExistingContacts();
            GenerateNewsletterSubscribers();
            GenerateEmailCampaignSubscribers();
        }


        private void GenerateSubscribersForExistingContacts()
        {
            SubscribeContactWithEmailToNewsletter("monica.king@localhost.local", "DancingGoatNewsletter");
            SubscribeContactWithEmailToNewsletter("monica.king@localhost.local", "Coffee101");
            SubscribeContactWithEmailToNewsletter("Dustin.Evans@localhost.local", "DancingGoatNewsletter");
            SubscribeContactWithEmailToNewsletter("Dustin.Evans@localhost.local", "Coffee101");
            SubscribeContactWithEmailToNewsletter("Todd.Ray@localhost.local", "Coffee101");
            SubscribeContactWithEmailToNewsletter("Stacy.Stewart@localhost.local", "Coffee101");
            SubscribeContactWithEmailToNewsletter("Harold.Larson@localhost.local", "DancingGoatNewsletter");
        }


        private void SubscribeContactWithEmailToNewsletter(string contactEmail, string newsletterCodeName)
        {
            var contact = ContactInfoProvider.GetContactInfo(contactEmail);
            var newsletter = NewsletterInfoProvider.GetNewsletterInfo(newsletterCodeName, SiteContext.CurrentSiteID);
            var fullName = string.Format("{0} {1}", contact.ContactFirstName, contact.ContactLastName);

            var subscriber = CreateSubscriber(contact.ContactEmail, contact.ContactFirstName, contact.ContactLastName, fullName, contact);
            AssignSubscriberToNewsletter(newsletter.NewsletterID, subscriber);
            mNewsletterActivityGenerator.GenerateNewsletterSubscribeActivity(newsletter, subscriber.SubscriberID, contact.ContactID, mSite.SiteID);
        }


        private void GenerateNewsletterSubscribers()
        {
            var coffee101Newsletter = NewsletterInfoProvider.GetNewsletterInfo("Coffee101", mSite.SiteID);
            if (coffee101Newsletter == null)
            {
                return;
            }

            var subscribers = GenerateSubscribers();
            AssignAllSubscribersToNewsletter(subscribers, coffee101Newsletter.NewsletterID);
            AdjustExistingIssues(subscribers);
        }


        private IList<SubscriberInfo> GenerateSubscribers()
        {
            var generatedSubscribers = new List<SubscriberInfo>();

            for (int i = 0; i < mSubscriberNames.Count(); i++)
            {
                var subscriberData = SubscriberData.CreateFromFullName(mSubscriberNames[i]);

                var contact = CreateContact(subscriberData.FirstName, subscriberData.LastName, subscriberData.Email);
                var subscriber = CreateSubscriber(subscriberData.Email, subscriberData.FirstName, subscriberData.LastName, mSubscriberNames[i], contact);
                generatedSubscribers.Add(subscriber);
            }

            return generatedSubscribers;
        }

        
        private SubscriberInfo CreateSubscriber(string email, string firstName, string lastName, string fullName, ContactInfo contact)
        {
            var subscriber = SubscriberInfoProvider.GetSubscriberByEmail(email, mSite.SiteID);
            if (subscriber != null)
            {
                return subscriber;
            }

            subscriber = new SubscriberInfo
            {
                SubscriberEmail = email,
                SubscriberFirstName = firstName,
                SubscriberLastName = lastName,
                SubscriberSiteID = mSite.SiteID,
                SubscriberFullName = fullName,
                SubscriberRelatedID = contact.ContactID,
                SubscriberType = PredefinedObjectType.CONTACT
            };
            SubscriberInfoProvider.SetSubscriberInfo(subscriber);

            return subscriber;
        }


        private ContactInfo CreateContact(string firstName, string lastName, string subscriberEmail)
        {
            var contact = new ContactInfo
            {
                ContactFirstName = firstName,
                ContactLastName = lastName,
                ContactEmail = subscriberEmail
            };
            ContactInfoProvider.SetContactInfo(contact);
            return contact;
        }


        private void AssignAllSubscribersToNewsletter(IEnumerable<SubscriberInfo> subscribers, int newsletterId)
        {
            foreach (var subscriber in subscribers)
            {
                AssignSubscriberToNewsletter(newsletterId, subscriber);
            }
        }


        private void AssignSubscriberToNewsletter(int newsletterId, SubscriberInfo subscriber)
        {
            var subscription = new SubscriberNewsletterInfo
            {
                SubscriberID = subscriber.SubscriberID,
                NewsletterID = newsletterId,
                SubscriptionApproved = true,
                SubscribedWhen = DateTime.Now,
            };

            SubscriberNewsletterInfoProvider.SetSubscriberNewsletterInfo(subscription);
        }


        private void AdjustExistingIssues(IList<SubscriberInfo> subscribers)
        {
            var lesson1 = IssueInfoProvider.GetIssues().OnSite(mSite.SiteID).WhereEquals("IssueSubject", "Coffee 101 - Lesson 1").FirstOrDefault();
            var lesson2 = IssueInfoProvider.GetIssues().OnSite(mSite.SiteID).WhereEquals("IssueSubject", "Coffee 101 - Lesson 2").FirstOrDefault();
            var campaignEmail = IssueInfoProvider.GetIssues().OnSite(mSite.SiteID).WhereEquals("IssueSubject", "Get a free Colombia coffee sample today").FirstOrDefault();

            if ((lesson1 == null) || (lesson2 == null) || campaignEmail == null)
            {
                return;
            }

            SettingsKeyInfoProvider.SetValue("CMSMonitorBouncedEmails", mSite.SiteName, true);

            const int uniqueOpensLesson1 = 26;
            const int uniqueOpensLesson2 = 14;
            const int uniqueOpensCampaignEmail = 42;

            lesson1.IssueSentEmails = 98;
            lesson1.IssueBounces = 2;
            lesson1.IssueOpenedEmails = uniqueOpensLesson1;
            lesson1.IssueUnsubscribed = 5;
            IssueInfoProvider.SetIssueInfo(lesson1);

            lesson2.IssueSentEmails = 98;
            lesson2.IssueBounces = 0;
            lesson2.IssueOpenedEmails = uniqueOpensLesson2;
            lesson2.IssueUnsubscribed = 3;
            IssueInfoProvider.SetIssueInfo(lesson2);

            campaignEmail.IssueSentEmails = 98;
            campaignEmail.IssueBounces = 0;
            campaignEmail.IssueOpenedEmails = uniqueOpensCampaignEmail;
            campaignEmail.IssueUnsubscribed = 3;
            IssueInfoProvider.SetIssueInfo(campaignEmail);

            var subscribersEmails = subscribers.Select(s => s.SubscriberEmail).ToList();

            var lessonLinkTarget = string.Format("http://{0}{1}/Store/Coffee/Ethiopia-Yirgacheffe", RequestContext.CurrentDomain, SystemContext.ApplicationPath);
            var campaignLinkTarget = string.Format("http://{0}{1}/Campaign-assets/Cafe-promotion/Colombia", RequestContext.CurrentDomain, SystemContext.ApplicationPath);

            GenerateClickedLinksToIssue(lesson1.IssueID, lessonLinkTarget, 17, 12, subscribersEmails);
            GenerateClickedLinksToIssue(lesson2.IssueID, lessonLinkTarget, 5, 2, subscribersEmails);
            GenerateClickedLinksToIssue(campaignEmail.IssueID, campaignLinkTarget, 35, 34, subscribersEmails);

            GenerateOpenedEmailToIssue(lesson1.IssueID, uniqueOpensLesson1, subscribersEmails);
            GenerateOpenedEmailToIssue(lesson2.IssueID, uniqueOpensLesson2, subscribersEmails);
            GenerateOpenedEmailToIssue(campaignEmail.IssueID, uniqueOpensCampaignEmail, subscribersEmails);

            GenerateUnsubscriptionsFromIssue(lesson1.IssueID, lesson1.IssueNewsletterID, 5, subscribers);
            GenerateUnsubscriptionsFromIssue(lesson2.IssueID, lesson2.IssueNewsletterID, 3, subscribers);
        }


        /// <summary>
        /// Total clicks value must not be higher then the count of subscribers.
        /// </summary>
        private void GenerateClickedLinksToIssue(int issueId, string linkTarget, int totalClicks, int uniqueClicks, IList<string> subscribersEmails)
        {
            var link = new LinkInfo
            {
                LinkIssueID = issueId,
                LinkTarget = linkTarget,
                LinkDescription = "Try Ethiopian Coffee"
            };
            LinkInfoProvider.SetLinkInfo(link);

            for (var i = 0; i < totalClicks; i++)
            {
                // Simulate non-unique clicks
                var subscriberIndex = (i <= (totalClicks - uniqueClicks)) ? 0 : i;

                var clickedLink = new ClickedLinkInfo
                {
                    ClickedLinkEmail = subscribersEmails[subscriberIndex],
                    ClickedLinkTime = GetRandomDate(DateTime.Now.AddMonths(-1), DateTime.Now.AddDays(-1)),
                    ClickedLinkNewsletterLinkID = link.LinkID,
                };
                ClickedLinkInfoProvider.SetClickedLinkInfo(clickedLink);
            }
        }


        /// <summary>
        /// Unique opens value must not be higher then the count of subscribers.
        /// </summary>
        private void GenerateOpenedEmailToIssue(int issueId, int uniqueOpens, IList<string> subscribersEmails)
        {
            for (var i = 0; i < uniqueOpens; i++)
            {
                var openedEmail = new OpenedEmailInfo
                {
                    OpenedEmailEmail = subscribersEmails[i],
                    OpenedEmailIssueID = issueId,
                    OpenedEmailTime = GetRandomDate(DateTime.Now.AddMonths(-1), DateTime.Now.AddDays(-1)),
                };
                OpenedEmailInfoProvider.SetOpenedEmailInfo(openedEmail);
            }
        }


        /// <summary>
        /// Unsubscribed value must not be higher then the count of subscribers.
        /// </summary>
        private void GenerateUnsubscriptionsFromIssue(int issueId, int newsletterId, int unsubscribed, IList<SubscriberInfo> subscribers)
        {
            for (var i = 0; i < unsubscribed; i++)
            {
                CreateUnsubscription(issueId, newsletterId, subscribers[i].SubscriberEmail);
            }
        }


        private void CreateUnsubscription(int issueId, int newsletterId, string unsubscriptionEmail)
        {
            var unsubscription = new UnsubscriptionInfo
            {
                UnsubscriptionEmail = unsubscriptionEmail,
                UnsubscriptionNewsletterID = newsletterId,
                UnsubscriptionFromIssueID = issueId,
            };
            unsubscription.Insert();
        }


        private DateTime GetRandomDate(DateTime from, DateTime to)
        {
            var range = to - from;
            var randTimeSpan = new TimeSpan((long)(rand.NextDouble() * range.Ticks));
            return from + randTimeSpan;
        }


        private void GenerateEmailCampaignSubscribers()
        {
            var contactGroup = ContactGroupInfoProvider.GetContactGroupInfo(EMAIL_CAMPAIGN_CONTACT_GROUP);
            var newsletter = NewsletterInfoProvider.GetNewsletterInfo("CoffeeClubMembership", mSite.SiteID);
            var campaignIssue = IssueInfoProvider.GetIssues().First(issue => issue.IssueNewsletterID == newsletter.NewsletterID);
            SubscribeContactGroupToIssue(campaignIssue, contactGroup);

            AddContactsToSubscribedContactGroup(contactGroup);
        }

        private void SubscribeContactGroupToIssue(IssueInfo campaignIssue, ContactGroupInfo contactGroup)
        {
            var issueContactGroup = new IssueContactGroupInfo()
            {
                IssueID = campaignIssue.IssueID,
                ContactGroupID = contactGroup.ContactGroupID
            };
            IssueContactGroupInfoProvider.SetIssueContactGroupInfo(issueContactGroup);
        }


        private void AddContactsToSubscribedContactGroup(ContactGroupInfo contactGroup)
        {
            for (int index = 0; index < mSubscriberNames.Length; index++)
            {
                // subscribe just half of contacts
                if (index % 2 == 0)
                {
                    var subscriberData = SubscriberData.CreateFromFullName(mSubscriberNames[index]);
                    var contact = ContactInfoProvider.GetContactInfo(subscriberData.Email);
                    AddContactToContactGroup(contact, contactGroup);

                    // some of them unsubscribe
                    if (index % 10 == 0)
                    {
                        CreateGlobalUnsubscription(subscriberData.Email);
                    }
                }
            }
        }


        private void CreateGlobalUnsubscription(string unsubscriptionEmail)
        {
            var unsubscription = new UnsubscriptionInfo
            {
                UnsubscriptionEmail = unsubscriptionEmail,
            };
            unsubscription.Insert();
        }


        private void AddContactToContactGroup(ContactInfo contact, ContactGroupInfo contactGroup)
        {
            var contactGroupMembership = new ContactGroupMemberInfo();
            contactGroupMembership.ContactGroupMemberContactGroupID = contactGroup.ContactGroupID;
            contactGroupMembership.ContactGroupMemberType = ContactGroupMemberTypeEnum.Contact;
            contactGroupMembership.ContactGroupMemberRelatedID = contact.ContactID;
            ContactGroupMemberInfoProvider.SetContactGroupMemberInfo(contactGroupMembership);
        }


        private class SubscriberData
        {
            public string FirstName
            {
                get; private set;
            }


            public string LastName
            {
                get; private set;
            }


            public string Email
            {
                get;
                private set;
            }


            public static SubscriberData CreateFromFullName(string fullName)
            {
                var words = fullName.Trim().Split(' ');
                var firstName = words[0];
                var lastName = words[1];
                var email = string.Format("{0}@{1}.local", firstName.ToLowerInvariant(), lastName.ToLowerInvariant());

                return new SubscriberData()
                {
                    FirstName = firstName,
                    LastName = lastName,
                    Email = email
                };
            }
        }
    }
}
