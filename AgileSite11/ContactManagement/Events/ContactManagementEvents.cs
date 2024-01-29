using CMS.ContactManagement.Internal;
using CMS.DataEngine;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Contact management events.
    /// </summary>
    public static class ContactManagementEvents
    {
        /// <summary>
        /// Event fires when we need to duplicate record in Activity table and all related tables if required.
        /// </summary>
        public static DuplicateActivitiesForContactHandler DuplicateActivitiesForContact = new DuplicateActivitiesForContactHandler { Name = "ContactManagementEvents.mDuplicateActivitiesForContact" };


        /// <summary>
        /// Event fires when contact actions batch (activities, contact changes) is being processed from queue to DB.
        /// </summary>
        internal static ProcessContactActionsBatchHandler ProcessContactActionsBatch = new ProcessContactActionsBatchHandler { Name = "ContactManagementEvents.ProcessContactActionsBatch" };


        /// <summary>
        /// Events fires when two contacts are being merged.
        /// </summary>
        public static ContactMergeHandler ContactMerge = new ContactMergeHandler { Name = "ContactManagementEvents.ContactManagementEvents" };


        /// <summary>
        /// Event fires when <see cref="ContactInfoProvider.DeleteContactInfos"/> deletes some contacts. Deleted contacts IDs are passed as event parameter.
        /// When single contact is deleted this event is not fired. 
        /// In this case <see cref="TypeInfoEvents.Delete"/> event on <see cref="ContactInfo.TYPEINFO"/> could be used.
        /// </summary>
        public static ContactInfosDeletedHandler ContactInfosDeleted = new ContactInfosDeletedHandler() { Name = "ContactManagementEvents.ContactInfosDeleted" };
    }
}
