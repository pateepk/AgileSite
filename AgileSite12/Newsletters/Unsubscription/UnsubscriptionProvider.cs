using System;
using System.Linq;

using CMS;
using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Newsletters;

[assembly: RegisterImplementation(typeof(IUnsubscriptionProvider), typeof(UnsubscriptionProvider), Priority = CMS.Core.RegistrationPriority.SystemDefault)]

namespace CMS.Newsletters
{
    /// <summary>
    /// Class that handles unsubscriptions.
    /// </summary>
    internal class UnsubscriptionProvider : IUnsubscriptionProvider
    {
        /// <summary>
        /// Returns query of all unsubscribed email addresses that won't receive given newsletter.
        /// </summary>
        /// <param name="newsletterID">Newsletter ID</param>
        public ObjectQuery<UnsubscriptionInfo> GetUnsubscriptionsFromSingleNewsletter(int newsletterID)
        {
            var unsubscriptions = UnsubscriptionInfoProvider.GetUnsubscriptions()
                                                            .WhereEqualsOrNull("UnsubscriptionNewsletterID", newsletterID);
            return unsubscriptions;
        }


        /// <summary>
        /// Returns query of all unsubscribed email addresses that won't receive any newsletter.
        /// </summary>
        public ObjectQuery<UnsubscriptionInfo> GetUnsubscriptionsFromAllNewsletters()
        {
            var unsubscriptions = UnsubscriptionInfoProvider.GetUnsubscriptions()
                                                            .WhereNull("UnsubscriptionNewsletterID");
            return unsubscriptions;
        }


        /// <summary>
        /// Returns <c>true</c> if specified email address won't receive given newsletter. 
        /// </summary>
        /// <param name="email">Email address</param>
        /// <param name="newsletterID">Newsletter ID</param>
        public bool IsUnsubscribedFromSingleNewsletter(string email, int newsletterID)
        {
            if (String.IsNullOrEmpty(email))
            {
                throw new ArgumentException("Email must be specified", nameof(email));
            }

            return GetUnsubscriptionsFromSingleNewsletter(newsletterID).WhereEquals("UnsubscriptionEmail", email).Any();
        }


        /// <summary>
        /// Returns <c>true</c> if specified email address is unsubscribed from the all newsletters.
        /// </summary>
        /// <param name="email">Email address</param>
        /// <exception cref="ArgumentException"><paramref name="email"/> is not specified</exception>
        public bool IsUnsubscribedFromAllNewsletters(string email)
        {
            if (String.IsNullOrEmpty(email))
            {
                throw new ArgumentException("Email must be specified", nameof(email));
            }

            var unsubscriptions = UnsubscriptionInfoProvider.GetUnsubscriptions()
                                                            .WhereNull("UnsubscriptionNewsletterID")
                                                            .WhereEquals("UnsubscriptionEmail", email);

            return unsubscriptions.Any();
        }


        /// <summary>
        /// Unsubscribes email address from given newsletter. Does nothing if specified email is already unsubscribed.
        /// </summary>
        /// <param name="email">Email to unsubscribe</param>
        /// <param name="newsletterID">Newsletter ID</param>
        /// <param name="issueID">ID of issue that visitor used for unsubscription. Use only when issue ID is known.</param>
        public void UnsubscribeFromSingleNewsletter(string email, int newsletterID, int? issueID = null)
        {
            Unsubscribe(email, newsletterID, issueID);
        }


        /// <summary>
        /// Unsubscribes email address from all newsletters. Does nothing if specified email is already unsubscribed.
        /// </summary>
        /// <param name="email">Email to unsubscribe</param>
        /// <param name="issueID">ID of issue that visitor used for unsubscription. Use only when issue ID is known.</param>
        public void UnsubscribeFromAllNewsletters(string email, int? issueID = null)
        {
            Unsubscribe(email, issueID: issueID);
        }


        /// <summary>
        /// Deletes all unsubscription records that blocks email from getting issues sent from given newsletter.
        /// </summary>
        /// <param name="email">Email address</param>
        /// <param name="newsletterID">Newsletter ID</param>
        public void RemoveUnsubscriptionFromSingleNewsletter(string email, int newsletterID)
        {
            if (String.IsNullOrEmpty(email))
            {
                throw new ArgumentException("Email must be specified", nameof(email));
            }

            GetUnsubscriptionsFromSingleNewsletter(newsletterID)
                .WhereEquals("UnsubscriptionEmail", email)
                .ForEachObject(unsubscription => unsubscription.Delete());
        }


        /// <summary>
        /// Deletes all "unsubscription from all" records for the given <paramref name="email"/>.
        /// </summary>
        /// <param name="email">Email address</param>
        /// <exception cref="ArgumentException"><paramref name="email"/> has to be specified</exception>
        public void RemoveUnsubscriptionsFromAllNewsletters(string email)
        {
            if (String.IsNullOrEmpty(email))
            {
                throw new ArgumentException("Email must be specified", nameof(email));
            }

            var unsubscriptions = UnsubscriptionInfoProvider.GetUnsubscriptions()
                                                            .WhereEquals("UnsubscriptionEmail", email)
                                                            .WhereNull("UnsubscriptionNewsletterID");

            unsubscriptions.ForEachObject(unsubscription => unsubscription.Delete());
        }


        /// <summary>
        /// Unsubscribes email address from given newsletter. Does nothing if specified email is already unsubscribed.
        /// When newsletter is not specified, contact is unsubscribed from all newsletters.
        /// </summary>
        /// <param name="email">Email to unsubscribe</param>
        /// <param name="newsletterID">Specifies newsletter that email is unsubscribed from. Null means that contact is unsubscribed from all newsletters.</param>
        /// <param name="issueID">ID of issue that visitor used for unsubscription. Use only when issue ID is known.</param>
        private void Unsubscribe(string email, int? newsletterID = null, int? issueID = null)
        {
            if (String.IsNullOrEmpty(email))
            {
                throw new ArgumentException("Email must be specified", nameof(email));
            }

            email = email.ToLowerCSafe();

            if (!ValidationHelper.IsEmail(email))
            {
                throw new ArgumentException("Email is in incorrect format", nameof(email));
            }

            var existingUnsubscriptions = UnsubscriptionInfoProvider.GetUnsubscriptions()
                                                                    .WhereEquals("UnsubscriptionNewsletterID", newsletterID)
                                                                    .WhereEquals("UnsubscriptionEmail", email);

            if (existingUnsubscriptions.Any())
            {
                return;
            }

            var unsubscription = new UnsubscriptionInfo
            {
                UnsubscriptionEmail = email,
                UnsubscriptionNewsletterID = newsletterID,
                UnsubscriptionFromIssueID = issueID
            };
            unsubscription.Insert();
        }
    }
}