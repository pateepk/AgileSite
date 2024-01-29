using System;
using System.Collections.Generic;

using CMS.Activities;

namespace CMS.ContactManagement
{
    /// <summary>
    /// This class performs tasks which are required to be done when recalculating score points for whole ScoreInfos. This includes marking
    /// score as being recalculated, deleting old points, sending notification emails, etc. 
    /// </summary>
    internal interface IScoreRecalculator
    {
        /// <summary>
        /// Recalculates score points for all rules in one score for all contact.
        /// </summary>
        /// <param name="score">Points for this score will be recalculated</param>
        /// <exception cref="ArgumentNullException"><paramref name="score"/> is null</exception>
        void RecalculateScoreRulesForAllContacts(ScoreInfo score);


        /// <summary>
        /// Recalculates scoring rules for contacts who performed some action. Recalculates only scoring rules which could have been affected by performed activities.
        /// Recalculation is made in batches, so only one operation is made for one scoring rule.
        /// </summary>
        /// <param name="processedActivities">Activities performed by contacts</param>
        /// <param name="contactChanges">Changes of contacts properties</param>
        void RecalculateScoreRulesAfterContactActionsBatch(IList<ActivityInfo> processedActivities, IList<ContactChangeData> contactChanges);
    }
}