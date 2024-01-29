using System.Collections.Generic;

using CMS.Base;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Handler for event fired when score is recalculated for contacts batch after their actions (activity, property change, merge or split).
    /// </summary>
    public class RecalculateAfterContactActionsBatchHandler : AdvancedHandler<RecalculateAfterContactActionsBatchHandler, RecalculateAfterContactActionsBatchEventArgs>
    {
        /// <summary>
        /// Initiates event handling.
        /// </summary>
        /// <param name="contactIDs">Contacts whose score is being recalculated</param>
        /// <returns>Handler</returns>
        public RecalculateAfterContactActionsBatchHandler StartEvent(ISet<int> contactIDs)
        {
            return StartEvent(new RecalculateAfterContactActionsBatchEventArgs
            {
                ContactIDs = contactIDs,
            });
        }
    }
}