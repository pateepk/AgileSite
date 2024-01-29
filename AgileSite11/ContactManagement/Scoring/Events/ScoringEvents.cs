using System;
using System.Linq;
using System.Text;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Scoring related events.
    /// </summary>
    public class ScoringEvents
    {
        /// <summary>
        /// Fired when recalculation of points for all contacts for one score is performed (e.g. when manually performing recalculation of rules by hitting the "Recalculate" button).
        /// </summary>
        public static RecalculateScoreForAllContactsHandler RecalculateScoreForAllContacts = new RecalculateScoreForAllContactsHandler { Name = "ScoringEvents.RecalculateScoreForAllContacts" };


        /// <summary>
        /// Fired when score is recalculated for contacts batch after their actions (activity, property change, merge or split).
        /// </summary>
        public static RecalculateAfterContactActionsBatchHandler RecalculateAfterContactActionsBatch = new RecalculateAfterContactActionsBatchHandler { Name = "ScoringEvents.RecalculateAfterContactActionsBatch" };
    }
}
