using CMS.DataEngine;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Class providing ContactRoleInfo management.
    /// </summary>
    public class ContactRoleInfoProvider : AbstractInfoProvider<ContactRoleInfo, ContactRoleInfoProvider>
    {
        #region "Constructor"

        /// <summary>
        /// Constructor using ID and codename Hashtables.
        /// </summary>
        public ContactRoleInfoProvider()
            : base(ContactRoleInfo.TYPEINFO, new HashtableSettings
				{
					ID = true,
					Name = true,
					Load = LoadHashtableEnum.None,
				})
        {
        }

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns a query for all the ContactRoleInfo objects.
        /// </summary>
        public static ObjectQuery<ContactRoleInfo> GetContactRoles()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns contact role with specified ID.
        /// </summary>
        /// <param name="roleId">Contact role ID</param>        
        public static ContactRoleInfo GetContactRoleInfo(int roleId)
        {
            return ProviderObject.GetInfoById(roleId);
        }


        /// <summary>
        /// Returns contact role with specified name.
        /// </summary>
        /// <param name="roleName">Contact role name</param>                              
        public static ContactRoleInfo GetContactRoleInfo(string roleName)
        {
            return ProviderObject.GetInfoByCodeName(roleName);
        }


        /// <summary>
        /// Sets (updates or inserts) specified contact role.
        /// </summary>
        /// <param name="roleObj">Contact role to be set</param>
        public static void SetContactRoleInfo(ContactRoleInfo roleObj)
        {
            ProviderObject.SetInfo(roleObj);
        }


        /// <summary>
        /// Deletes specified contact role.
        /// </summary>
        /// <param name="roleObj">Contact role to be deleted</param>
        public static void DeleteContactRoleInfo(ContactRoleInfo roleObj)
        {
            ProviderObject.DeleteInfo(roleObj);
        }


        /// <summary>
        /// Deletes contact role with specified ID.
        /// </summary>
        /// <param name="roleId">Contact role ID</param>
        public static void DeleteContactRoleInfo(int roleId)
        {
            ContactRoleInfo roleObj = GetContactRoleInfo(roleId);
            DeleteContactRoleInfo(roleObj);
        }

        #endregion
    }
}