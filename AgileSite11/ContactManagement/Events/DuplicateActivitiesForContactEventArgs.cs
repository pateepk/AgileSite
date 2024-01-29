using System.Collections.Generic;

using CMS.Activities;
using CMS.Base;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Event arguments for the event fired when we need to duplicate record in Activity table and all related tables if required.
    /// </summary>
    public class DuplicateActivitiesForContactEventArgs : CMSEventArgs
    {
        /// <summary>
        /// Activities to duplicate. 
        /// </summary>
        public IList<ActivityInfo> Activities
        {
            get;
            internal set;
        }


        /// <summary>
        /// Contact to duplicate activities for.
        /// </summary>
        public ContactInfo Contact
        {
            get;
            internal set;
        }
    }
}
