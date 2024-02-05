using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Watches for score points changes after score is recalculated and sends notification email if score points for the contact exceeds limit specified in <see cref="ScoreInfo.ScoreEmailAtScore"/>.
    /// </summary>
    /// <remarks>
    /// In order to work correctly, method <see cref="SaveScoresSnapshot"/> has to be called before recalculation and method <see cref="SendNotifications"/> has to be called after recalculation.
    /// </remarks>
    internal class BatchContactScoreNotificationsChecker
    {
        private readonly IList<int> mContactIds;
        private ContactWithScoreValueCollection mOverLimitContactsWithScores;


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="contactIds">Collection of Contact IDs to be checked</param>
        /// <exception cref="ArgumentNullException"><paramref name="contactIds"/> is null</exception>
        public BatchContactScoreNotificationsChecker(ISet<int> contactIds)
        {
            if (contactIds == null)
            {
                throw new ArgumentNullException("contactIds");
            }

            mContactIds = contactIds.ToList();
        }


        /// <summary>
        /// This method has to be called before recalculation. 
        /// It saves current score points so notification emails will be send only for contact and scores which will exceed limit after recalculation.
        /// </summary>
        public void SaveScoresSnapshot()
        {
            mOverLimitContactsWithScores = ScoreInfoProvider.GetScoresWhereContactsExceededLimit(mContactIds);
        }


        /// <summary>
        /// Compares current score points with score points saved before recalculation (by method <see cref="SaveScoresSnapshot"/>) and sends notification emails for those scores whose
        /// points now exceeds limit, but didn't before recalculation.
        /// </summary>
        /// <exception cref="InvalidOperationException">Method <see cref="SaveScoresSnapshot"/> wasn't called before recalculation</exception>
        public void SendNotifications()
        {
            if (mOverLimitContactsWithScores == null)
            {
                throw new InvalidOperationException("[BatchContactScoreNotificationsChecker.SendNotifications]: Method SaveScoresSnapshot has to be called before checking for score changes");
            }

            var contactWithScoresExceedingLimit = ScoreInfoProvider.GetScoresWhereContactsExceededLimit(mContactIds);
            
            // Remove scores where limit was exceeded already before recalculation 
            contactWithScoresExceedingLimit.RelativeComplement(mOverLimitContactsWithScores);
            
            foreach (var contactWithScoreValue in contactWithScoresExceedingLimit)
            {
                var scoreNotificationsSender = new ScoreNotificationsSender();
                var score = ScoreInfoProvider.GetScoreInfo(contactWithScoreValue.ScoreID);
                var contact = ContactInfoProvider.GetContactInfo(contactWithScoreValue.ContactID);

                scoreNotificationsSender.SendNotification(contact, score, contactWithScoreValue.ScoreValue);
            }
        }
    }
}
