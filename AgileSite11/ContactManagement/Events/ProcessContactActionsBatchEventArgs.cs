using System.Collections.Generic;

using CMS.Base;
using CMS.Activities;
using CMS.Activities.Internal;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Arguments for the ProcessActivitiesHandler event handler.
    /// </summary>
    internal class ProcessContactActionsBatchEventArgs : CMSEventArgs
    {
        /// <summary>
        /// Activities which were inserted to the DB. 
        /// </summary>
        public IList<IActivityInfo> LoggedActivities
        {
            get;
            internal set;
        }


        /// <summary>
        /// Contact changes which took place in the last interval. Those changes are already written in the database even on the Before event. 
        /// This list serves as an information for online marketing calculators (contact groups, scoring, etc.), so they know which groups or scoring rules should be recalculated.
        /// </summary>
        internal IList<ContactChangeData> LoggedContactChanges
        {
            get;
            set;
        }
    }
}
