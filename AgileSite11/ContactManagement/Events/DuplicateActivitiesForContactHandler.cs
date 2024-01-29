using System;
using System.Collections.Generic;

using CMS.Activities;
using CMS.Base;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Event handler for the event fired when we need to duplicate record in Activity table and all related tables if required.
    /// </summary>
    public class DuplicateActivitiesForContactHandler : SimpleHandler<DuplicateActivitiesForContactHandler, DuplicateActivitiesForContactEventArgs>
    {
        /// <summary>
        /// Initiates event handling.
        /// </summary>
        /// <param name="activities">Activities to duplicate</param>
        /// <param name="contact">Contact to duplicate activities for</param>
        public DuplicateActivitiesForContactEventArgs StartEvent(IList<ActivityInfo> activities, ContactInfo contact)
        {
            if (activities == null)
            {
                throw new ArgumentNullException("activities");
            }

            var e = new DuplicateActivitiesForContactEventArgs
            {
                Activities = activities,
                Contact = contact
            };

            return StartEvent(e);
        }
    }
}
