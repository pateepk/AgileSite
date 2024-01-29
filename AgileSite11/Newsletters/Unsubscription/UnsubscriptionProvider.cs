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
    /// If false, having null in the newsletter ID column means unsubscribed from all newsletters.
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
        /// Returns true if specified email address is unsubscribed from newsletter.
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
        /// Returns true if specified email address is unsubscribed from the all newsletters.
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
        /// Clones all unsubscriptions existing for the given <paramref name="email"/> and set them to <paramref name="newEmail"/>.
        /// Final state after execution of the method are two same unsubscriptions, one with the original <paramref name="email"/>, one with the <paramref name="newEmail"/>.
        /// This method should be used whenever subscriber changes their email and already has some unsubscription saved, 
        /// so it can be assumed they want to have unsubscription for the <paramref name="newEmail"/> as well.
        /// </summary>
        /// <param name="email">Email of the existing unsubscriptions</param>
        /// <param name="newEmail">New email set to the cloned unsubscriptions</param>
        /// <exception cref="ArgumentNullException"><paramref name="email"/> or <paramref name="newEmail"/> is not specified</exception>
        [Obsolete("Never intended for public use.")]
        public void CloneUnsubscriptionForEmail(string email, string newEmail)
        {
            if (String.IsNullOrEmpty(email))
            {
                throw new ArgumentException("Email must be specified", nameof(email));
            }

            if (String.IsNullOrEmpty(newEmail))
            {
                throw new ArgumentException("Email must be specified", nameof(newEmail));
            }
            
            if (!ValidationHelper.IsEmail(newEmail))
            {
                throw new ArgumentException("Given email is not in valid format", nameof(newEmail));    
            }

            if (email.EqualsCSafe(newEmail, true))
            {
                return;
            }

            var oldMailUnsubscriptions = UnsubscriptionInfoProvider.GetUnsubscriptions()
                                                                   .WhereEquals("UnsubscriptionEmail", email);
            
            foreach (var unsubscription in oldMailUnsubscriptions)
            {
                Unsubscribe(newEmail, unsubscription.UnsubscriptionNewsletterID);
            }
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