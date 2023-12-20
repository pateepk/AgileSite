using CMS.DataEngine;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Class providing ContactGroupInfo management.
    /// </summary>
    public class ContactGroupInfoProvider : AbstractInfoProvider<ContactGroupInfo, ContactGroupInfoProvider>
    {
        #region "Constructor"

        /// <summary>
        /// Constructor using ID and codename Hashtables.
        /// </summary>
        public ContactGroupInfoProvider()
            : base(ContactGroupInfo.TYPEINFO, new HashtableSettings
                {
                    ID = true,
                    Name = true,
                    Load = LoadHashtableEnum.None,
                    UseWeakReferences = true,
                })
        {
        }

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns a query for all the ContactGroupInfo objects.
        /// </summary>
        public static ObjectQuery<ContactGroupInfo> GetContactGroups()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns contact group with specified ID.
        /// </summary>
        /// <param name="groupId">Contact group ID</param>        
        public static ContactGroupInfo GetContactGroupInfo(int groupId)
        {
            return ProviderObject.GetInfoById(groupId);
        }


        /// <summary>
        /// Returns contact group with specified name.
        /// </summary>
        /// <param name="groupName">Contact group name</param>         
        public static ContactGroupInfo GetContactGroupInfo(string groupName)
        {
            return ProviderObject.GetInfoByCodeName(groupName);
        }


        /// <summary>
        /// Sets (updates or inserts) specified contact group.
        /// </summary>
        /// <param name="groupObj">Contact group to be set</param>
        public static void SetContactGroupInfo(ContactGroupInfo groupObj)
        {
            ProviderObject.SetInfo(groupObj);
        }


        /// <summary>
        /// Deletes specified contact group.
        /// </summary>
        /// <param name="groupObj">Contact group to be deleted</param>
        public static void DeleteContactGroupInfo(ContactGroupInfo groupObj)
        {
            ProviderObject.DeleteInfo(groupObj);
        }


        /// <summary>
        /// Deletes contact group with specified ID.
        /// </summary>
        /// <param name="groupId">Contact group ID</param>
        public static void DeleteContactGroupInfo(int groupId)
        {
            ContactGroupInfo groupObj = GetContactGroupInfo(groupId);
            DeleteContactGroupInfo(groupObj);
        }

        #endregion
    }
}