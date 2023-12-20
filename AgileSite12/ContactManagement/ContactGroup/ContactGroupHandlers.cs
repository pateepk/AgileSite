using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Activities;
using CMS.EventLog;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Event handlers of the ContactGroups module.
    /// </summary>
    internal static class ContactGroupHandlers
    {
        /// <summary>
        /// Subscribes to events.
        /// </summary>
        public static void Init()
        {
            ContactManagementEvents.ProcessContactActionsBatch.After += EvaluateContactGroupsForContactActionsBatch;
        }

        
        /// <summary>
        /// Evaluates all dynamic contact groups after batch of contact actions is logged.
        /// </summary>
        private static void EvaluateContactGroupsForContactActionsBatch(object sender, ProcessContactActionsBatchEventArgs e)
        {
            RebuildContactGroups(e.LoggedActivities, e.LoggedContactChanges);
        }


        /// <summary>
        /// Rebuilds dynamic contact groups for given contacts.
        /// </summary>
        /// <param name="loggedActivities">List of performed activities</param>
        /// <param name="loggedContactChanges">List of contact's changes</param>
        private static void RebuildContactGroups(IList<IActivityInfo> loggedActivities, IList<ContactChangeData> loggedContactChanges)
        {
            var activityTypes = new HashSet<string>(loggedActivities.Select(activity => activity.ActivityType));
            var changedColumns = new HashSet<string>(loggedContactChanges.Where(change => change.ChangedColumns != null).SelectMany(change => change.ChangedColumns));
            
            var contactGroupRebuilder = new ContactGroupRebuilder();
            var contactGroupFilter = new AffectedContactGroupsFilter();
            var contactsFilter = new ContactsAffectingContactGroupFilter();

            // Filter contact groups to match only the ones affected by logged activities or by any attribute change
            var dynamicContactGroups = GetDynamicContactGroups().ToList();

            bool newContactWasCreatedOrMerged = loggedContactChanges.Any(change => change.ContactIsNew || change.ContactWasMerged);
            var filteredContactGroups = newContactWasCreatedOrMerged ? dynamicContactGroups : contactGroupFilter.FilterContactGroups(dynamicContactGroups, activityTypes, changedColumns).ToList();
            
            foreach (var contactGroup in filteredContactGroups)
            {
                try
                {
                    var contacts = contactsFilter.FilterContacts(contactGroup, loggedActivities, loggedContactChanges);
                    contactGroupRebuilder.RebuildPartOfContactGroup(contactGroup, contacts);
                }
                catch (Exception ex)
                {
                    EventLogProvider.LogException("ContactGroupHandlers.RebuildContactGroups", "Cannot evaluate contact group", ex, 0, "Unexpected exception occurred while evaluating contact group with ID " + contactGroup.ContactGroupID);
                }
            }
        }


        /// <summary>
        /// Returns dynamic groups.
        /// </summary>
        private static IEnumerable<ContactGroupInfo> GetDynamicContactGroups()
        {
            return ContactGroupInfoProvider.GetContactGroups().WhereNotEmpty("ContactGroupDynamicCondition");
        }
    }
}