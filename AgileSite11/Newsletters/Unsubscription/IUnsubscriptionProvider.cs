using System;

using CMS.DataEngine;

namespace CMS.Newsletters
{
    /// <summary>
    /// Handles unsubscriptions.
    /// </summary>
    public interface IUnsubscriptionProvider
    {
        /// <summary>
        /// Returns query of all unsubscribed email addresses that won't receive given newsletter.
        /// </summary>
        /// <param name="newsletterID">Newsletter ID</param>
        ObjectQuery<UnsubscriptionInfo> GetUnsubscriptionsFromSingleNewsletter(int newsletterID);

        /// <summary>
        /// Returns query of all unsubscribed email addresses that won't receive any newsletter.
        /// </summary>
        ObjectQuery<UnsubscriptionInfo> GetUnsubscriptionsFromAllNewsletters();

        /// <summary>
        /// Returns true if specified email address is unsubscribed from newsletter. 
        /// </summary>
        /// <param name="email">Email address</param>
        /// <param name="newsletterID">Newsletter ID</param>
        bool IsUnsubscribedFromSingleNewsletter(string email, int newsletterID);


        /// <summary>
        /// Returns true if specified email address is unsubscribed from the all newsletter.
        /// </summary>
        /// <param name="email">Email address</param>
        /// <exception cref="ArgumentException"><paramref name="email"/> is not specified</exception>
        bool IsUnsubscribedFromAllNewsletters(string email);


        /// <summary>
        /// Unsubscribes email address from given newsletter. Does nothing if specified email is already unsubscribed.
        /// </summary>
        /// <param name="email">Email to unsubscribe</param>
        /// <param name="newsletterID">Newsletter ID</param>
        /// <param name="issueID">ID of issue that visitor used for unsubscription. Use only when issue ID is known.</param>
        void UnsubscribeFromSingleNewsletter(string email, int newsletterID, int? issueID = null);


        /// <summary>
        /// Unsubscribes email address from all newsletters. Does nothing if specified email is already unsubscribed.
        /// </summary>
        /// <param name="email">Email to unsubscribe</param>
        /// <param name="issueID">ID of issue that visitor used for unsubscription. Use only when issue ID is known.</param>
        void UnsubscribeFromAllNewsletters(string email, int? issueID = null);


        /// <summary>
        /// Deletes all unsubscription records that blocks email from getting issues sent from given newsletter.
        /// </summary>
        /// <param name="email">Email address</param>
        /// <param name="newsletterID">Newsletter ID</param>
        void RemoveUnsubscriptionFromSingleNewsletter(string email, int newsletterID);


        /// <summary>
        /// Deletes all "unsubscription from all" records for the given <paramref name="email"/>.
        /// </summary>
        /// <param name="email">Email address</param>
        /// <exception cref="ArgumentException"><paramref name="email"/> has to be specified</exception>
        void RemoveUnsubscriptionsFromAllNewsletters(string email);


        /// <summary>
        /// Clones all unsubscriptions existing for the given <paramref name="email"/> and set them to <paramref name="newEmail"/>.
        /// Final state after execution of the method are two same unsubscriptions, one with the original <paramref name="email"/>, one with the <paramref name="newEmail"/>.
        /// This method should be used whenever subscriber changes their email and already has some unsubscription saved, 
        /// so it can be assumed they want to have unsubscription for the <paramref name="newEmail"/> as well.
        /// </summary>
        /// <param name="email">Email of the existing unsubscriptions</param>
        /// <param name="newEmail">New email set to the cloned unsubscriptions</param>
        [Obsolete("Never intended for public use.")]
        void CloneUnsubscriptionForEmail(string email, string newEmail);
    }
}
