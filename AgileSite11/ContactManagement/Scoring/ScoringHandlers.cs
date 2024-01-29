using System;
using System.Linq;
using System.Text;

using CMS.Activities.Internal;
using CMS.Core;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Event handlers of the Scoring module.
    /// </summary>
    internal static class ScoringHandlers
    {
        /// <summary>
        /// Subscribes to events.
        /// </summary>
        public static void Init()
        {
            ContactManagementEvents.ProcessContactActionsBatch.After += RecalculateScoresBatch;

            ScoringEvents.RecalculateAfterContactActionsBatch.Before += SaveScoresSnapshotBeforeBatchRecalculation;

            ScoringEvents.RecalculateAfterContactActionsBatch.After += PerformActionsAfterRecalculationToBatchGroup;
        }


        /// <summary>
        /// Recalculates scores after batch of contact activities was processed.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event args</param>
        private static void RecalculateScoresBatch(object sender, ProcessContactActionsBatchEventArgs e)
        {
            if (ScoreInfoProvider.GetScores().Count == 0)
            {
                return;
            }

            var scoreCalculationManager = Service.Resolve<IScoreRecalculator>();
            scoreCalculationManager.RecalculateScoreRulesAfterContactActionsBatch(e.LoggedActivities.Cast<ActivityDto>().Select(activity => activity.ToActivityInfo()).ToList(), e.LoggedContactChanges);
        }


        /// <summary>
        /// Saves current score values before score is recalculated, so email notifications can be send later and corresponding marketing automation triggers can be fired.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event arguments containing collection of affected contacts</param>
        private static void SaveScoresSnapshotBeforeBatchRecalculation(object sender, RecalculateAfterContactActionsBatchEventArgs e)
        {
            var notificationsWatcher = new BatchContactScoreNotificationsChecker(e.ContactIDs);
            notificationsWatcher.SaveScoresSnapshot();

            e.CustomArguments["NotificationsWatcher"] = notificationsWatcher;

            var triggerProcessor = new BatchScoreTriggersRunner(e.ContactIDs);
            triggerProcessor.SaveScoresSnapshot();

            e.CustomArguments["TriggerProcessor"] = triggerProcessor;
        }


        /// <summary>
        /// Performs actions after score calculation.
        /// Sends notification emails when contact exceeded points limit (saved by <see cref="SaveScoresSnapshotBeforeBatchRecalculation"/> method)
        /// and processes marketing automation score triggers.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event arguments containing collection of affected contacts</param>
        private static void PerformActionsAfterRecalculationToBatchGroup(object sender, RecalculateAfterContactActionsBatchEventArgs e)
        {
            var notificationsWatcher = (BatchContactScoreNotificationsChecker)e.CustomArguments["NotificationsWatcher"];

            notificationsWatcher.SendNotifications();

            var triggerProcessor = (BatchScoreTriggersRunner)e.CustomArguments["TriggerProcessor"];
            
            triggerProcessor.ProcessTriggers();
        }
    }
}
