using System.Collections.Generic;
using System.Linq;

using CMS.Activities;
using CMS.Core;
using CMS.OnlineMarketing.Internal;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Provides method for processing all the contact changes and logged activities. Triggers recalculations of dynamic 
    /// segments and marketing automation processes related to the contact actions.
    /// </summary>
    internal sealed class ContactActionsQueueProcessor
    {
        private readonly IContactChangeQueueRecalculationProvider mContactChangeQueueRecalculationProvider;
        private readonly IActivityQueueRecalculationProvider mActivityQueueRecalculationProvider;


        /// <summary>
        /// Instantiates the class.
        /// </summary>
        public ContactActionsQueueProcessor() : this(Service.Resolve<IContactChangeQueueRecalculationProvider>(), Service.Resolve<IActivityQueueRecalculationProvider>())
        {
        }


        /// <summary>
        /// Instantiates the class with all dependencies.
        /// </summary>
        internal ContactActionsQueueProcessor(IContactChangeQueueRecalculationProvider contactChangeQueueRecalculationProvider, IActivityQueueRecalculationProvider activityQueueRecalculationProvider)
        {
            mContactChangeQueueRecalculationProvider = contactChangeQueueRecalculationProvider;
            mActivityQueueRecalculationProvider = activityQueueRecalculationProvider;
        }


        /// <summary>
        /// Processes items from queue. It processes logged activities and contact changes. 
        /// </summary>
        /// <remarks>At the end the event <see cref="ContactManagementEvents.ProcessContactActionsBatch"/> with filtered activities and obtained contact changes is raised</remarks>
        internal void ProcessAllContactActions()
        {
            bool fetch = true;

            while (fetch)
            {
                var activities = mActivityQueueRecalculationProvider.Dequeue();
                var changeBatch = mContactChangeQueueRecalculationProvider.Dequeue().ToList();

                if (activities.Any() || changeBatch.Any())
                {
                    // Process the activities
                    var processContactActionEventArgs = CreateProcessContactActionEventArgs(activities, changeBatch);

                    using (var h = ContactManagementEvents.ProcessContactActionsBatch.StartEvent(processContactActionEventArgs))
                    {
                        h.FinishEvent();
                    }
                }
                else
                {
                    fetch = false;
                }
            }
        }


        private static ProcessContactActionsBatchEventArgs CreateProcessContactActionEventArgs(IList<IActivityInfo> activities, IList<ContactChangeData> changeBatch)
        {
            return new ProcessContactActionsBatchEventArgs
            {
                LoggedActivities = activities,
                LoggedContactChanges = changeBatch
            };
        }
    }
}
