using System;
using System.Linq;
using System.Text;

using CMS.Base;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Handler for the event fired when points for all contacts for given score are being recalculated.
    /// </summary>
    public class RecalculateScoreForAllContactsHandler : AdvancedHandler<RecalculateScoreForAllContactsHandler, ScoreEventArgs>
    {
        /// <summary>
        /// Initiates event handling.
        /// </summary>
        /// <param name="recalcuatedScore">Score which is being recalculated</param>
        /// <returns>Event handler</returns>
        public RecalculateScoreForAllContactsHandler StartEvent(ScoreInfo recalcuatedScore)
        {
            var eventArgs = new ScoreEventArgs()
            {
                Score = recalcuatedScore,
            };

            return StartEvent(eventArgs);
        }
    }
}