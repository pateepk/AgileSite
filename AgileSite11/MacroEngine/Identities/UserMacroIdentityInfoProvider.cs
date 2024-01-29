using System;
using System.Data;

using CMS.Base;
using CMS.DataEngine;

namespace CMS.MacroEngine
{
    /// <summary>
    /// Class providing UserMacroIdentityInfo management.
    /// </summary>
    public class UserMacroIdentityInfoProvider : AbstractInfoProvider<UserMacroIdentityInfo, UserMacroIdentityInfoProvider>
    {
        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        public UserMacroIdentityInfoProvider()
            : base(UserMacroIdentityInfo.TYPEINFO, new HashtableSettings { ID = true, Load = LoadHashtableEnum.None })
        {
        }

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns a query for all the UserMacroIdentityInfo objects.
        /// </summary>
        public static ObjectQuery<UserMacroIdentityInfo> GetUserMacroIdentities()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns UserMacroIdentityInfo with specified ID.
        /// </summary>
        /// <param name="id">UserMacroIdentityInfo ID</param>
        public static UserMacroIdentityInfo GetUserMacroIdentityInfo(int id)
        {
            return ProviderObject.GetInfoById(id);
        }


        /// <summary>
        /// Returns UserMacroIdentityInfo for specified user.
        /// </summary>
        /// <param name="userInfo">User info</param>
        public static UserMacroIdentityInfo GetUserMacroIdentityInfo(IUserInfo userInfo)
        {
            return ProviderObject.GetUserMacroIdentityInfoInternal(userInfo);
        }


        /// <summary>
        /// Sets (updates or inserts) specified UserMacroIdentityInfo.
        /// </summary>
        /// <param name="infoObj">UserMacroIdentityInfo to be set</param>
        public static void SetUserMacroIdentityInfo(UserMacroIdentityInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified UserMacroIdentityInfo.
        /// </summary>
        /// <param name="infoObj">UserMacroIdentityInfo to be deleted</param>
        public static void DeleteUserMacroIdentityInfo(UserMacroIdentityInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes UserMacroIdentityInfo with specified ID.
        /// </summary>
        /// <param name="id">UserMacroIdentityInfo ID</param>
        public static void DeleteUserMacroIdentityInfo(int id)
        {
            UserMacroIdentityInfo infoObj = GetUserMacroIdentityInfo(id);
            DeleteUserMacroIdentityInfo(infoObj);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Returns UserMacroIdentityInfo for specified user.
        /// </summary>
        /// <param name="userInfo">User info</param>
        protected virtual UserMacroIdentityInfo GetUserMacroIdentityInfoInternal(IUserInfo userInfo)
        {
            return GetUserMacroIdentities().WhereEquals("UserMacroIdentityUserID", userInfo.UserID).TopN(1).FirstObject;
        }


        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(UserMacroIdentityInfo info)
        {
            var isNew = (info.UserMacroIdentityID <= 0);
            if (isNew)
            {
                var userInfo = ProviderHelper.GetInfoById(PredefinedObjectType.USER, info.UserMacroIdentityUserID);
                if (userInfo == null)
                {
                    throw new InvalidOperationException($"UserMacroIdenittyInfo '{info}' cannot be set without valid UserMacroIdentityUserID.");
                }
                info.UserMacroIdentityUserGuid = userInfo.Generalized.ObjectGUID;
            }

            base.SetInfo(info);
        }

        #endregion
    }
}