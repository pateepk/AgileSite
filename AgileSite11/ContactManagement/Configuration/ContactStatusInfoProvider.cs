using System;

using CMS.DataEngine;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Class providing ContactStatusInfo management.
    /// </summary>
    public class ContactStatusInfoProvider : AbstractInfoProvider<ContactStatusInfo, ContactStatusInfoProvider>
    {
        #region "Constructor"

        /// <summary>
        /// Constructor using ID and codename Hashtables.
        /// </summary>
        public ContactStatusInfoProvider()
            : base(ContactStatusInfo.TYPEINFO, new HashtableSettings
				{
					ID = true,
					Name = true,
					Load = LoadHashtableEnum.All
				})
        {
        }

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns a query for all the ContactStatusInfo objects.
        /// </summary>
        public static ObjectQuery<ContactStatusInfo> GetContactStatuses()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns contact status with specified ID.
        /// </summary>
        /// <param name="statusId">Contact status ID</param>        
        public static ContactStatusInfo GetContactStatusInfo(int statusId)
        {
            return ProviderObject.GetInfoById(statusId);
        }


        /// <summary>
        /// Returns contact status with specified name.
        /// </summary>
        /// <param name="statusName">Contact status name</param>
        public static ContactStatusInfo GetContactStatusInfo(string statusName)
        {
            return ProviderObject.GetInfoByCodeName(statusName);
        }


        /// <summary>
        /// Sets (updates or inserts) specified contact status.
        /// </summary>
        /// <param name="statusObj">Contact status to be set</param>
        public static void SetContactStatusInfo(ContactStatusInfo statusObj)
        {
            ProviderObject.SetInfo(statusObj);
        }


        /// <summary>
        /// Deletes specified contact status.
        /// </summary>
        /// <param name="statusObj">Contact status to be deleted</param>
        public static void DeleteContactStatusInfo(ContactStatusInfo statusObj)
        {
            ProviderObject.DeleteInfo(statusObj);
        }


        /// <summary>
        /// Deletes contact status with specified ID.
        /// </summary>
        /// <param name="statusId">Contact status ID</param>
        public static void DeleteContactStatusInfo(int statusId)
        {
            ContactStatusInfo statusObj = GetContactStatusInfo(statusId);
            DeleteContactStatusInfo(statusObj);
        }

        #endregion
    }
}