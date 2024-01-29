using System;
using System.Data;
using System.Linq;
using System.Text;

using CMS.Helpers;

namespace CMS.ContactManagement
{
    /// <summary>
    /// This class handles sending notification emails for contacts who exceeded score limit after recalculating score points for the whole ScoreInfo.
    /// </summary>
    internal class AllContactsScoreNotificationsChecker
    {
        /// <summary>
        /// Sends notification emails to address specified in the ScoreInfo for contacts who exceeded score limit.
        /// </summary>
        /// <param name="score">Score info object</param>
        /// <exception cref="ArgumentNullException"><paramref name="score"/> is null</exception>
        internal void SendAllNotifications(ScoreInfo score)
        {
            if (score == null)
            {
                throw new ArgumentNullException("score");
            }

            if ((score.ScoreEmailAtScore <= 0) || !ValidationHelper.AreEmails(score.ScoreNotificationEmail))
            {
                return;
            }
            
            // Get IDs of contacts who exceeded score limit for specific scoring
            DataSet ds = ScoreContactRuleInfoProvider.GetContactsWithScore(score.ScoreEmailAtScore).WhereEquals("ScoreID", score.ScoreID);
            if (DataHelper.DataSourceIsEmpty(ds))
            {
                return;
            }

            var scoreNotificationsSender = new ScoreNotificationsSender();
            foreach (DataRow row in ds.Tables[0].Rows)
            {
                var contact = ContactInfoProvider.GetContactInfo(ValidationHelper.GetInteger(row["ContactID"], 0));
                int scoreValue = ValidationHelper.GetInteger(row["Score"], 0);

                scoreNotificationsSender.SendNotification(contact, score, scoreValue);
            }
        }
    }
}
