using System;
using System.Linq;

using CMS.DataEngine;

namespace CMS.Membership
{
    /// <summary>
    /// Class providing UserOpenIDInfo management.
    /// </summary>
    public class OpenIDUserInfoProvider : AbstractInfoProvider<OpenIDUserInfo, OpenIDUserInfoProvider>
    {
        #region "Methods"

        /// <summary>
        /// Returns the query for all relationships between OpenIDs and users.
        /// </summary>
        public static ObjectQuery<OpenIDUserInfo> GetOpenIDUsers()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns the OpenIDUserInfo structure for the specified OpenID Claimed Identifier.
        /// </summary>
        /// <param name="openID">OpenID Claimed Identifier</param>
        public static OpenIDUserInfo GetOpenIDUserInfo(string openID)
        {
            return ProviderObject.GetOpenIDUserInfoInternal(openID);
        }


        /// <summary>
        /// Sets (updates or inserts) specified openIDUser object.
        /// </summary>
        /// <param name="openIDUser">OpenIDUserInfo to set</param>
        public static void SetOpenIDUserInfo(OpenIDUserInfo openIDUser)
        {
            ProviderObject.SetOpenIDUserInfoInternal(openIDUser);
        }


        /// <summary>
        /// Deletes specified openIDUser object.
        /// </summary>
        /// <param name="infoObj">OpenIDUserInfo object</param>
        public static void DeleteOpenIDUserInfo(OpenIDUserInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified openIDUser object.
        /// </summary>
        /// <param name="openID">OpenID Claimed Identifier</param>
        public static void DeleteOpenIDUserInfo(string openID)
        {
            OpenIDUserInfo infoObj = GetOpenIDUserInfo(openID);
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Returns UserInfo connected to specified OpenID Claimed Identifier.
        /// </summary>
        /// <param name="openId">OpenID Claimed Identifier</param>
        /// <returns>UserInfo</returns>
        public static UserInfo GetUserInfoByOpenID(string openId)
        {
            return ProviderObject.GetUserInfoByOpenIDInternal(openId);
        }


        /// <summary>
        /// Returns OpenID Claimed Identifier connected with specified UserID.
        /// </summary>
        /// <param name="userId">UserID to look for</param>
        /// <returns>OpenID Claimed Identifier</returns>
        public static string GetOpenIDByUserID(int userId)
        {
            return ProviderObject.GetOpenIDByUserIDInternal(userId);
        }


        /// <summary>
        /// Assigns OpenID Claimed Identifier to the user.
        /// </summary>
        /// <param name="openId">OpenID Claimed Identifier</param>
        /// <param name="openIdProviderUrl">URL of OpenID provider</param>
        /// <param name="userId">User ID</param>
        /// <returns>Returns true if user exists and OpenID Claimed Identifier is not already assigned.</returns>
        public static bool AddOpenIDToUser(string openId, string openIdProviderUrl, int userId)
        {
            return ProviderObject.AddOpenIDToUserInternal(openId, openIdProviderUrl, userId);
        }


        /// <summary>
        /// Deletes OpenIDUserInfo by specified OpenID Claimed Identifier and checks if given User ID is part of this object.
        /// </summary>
        /// <param name="openId">OpenID Claimed Identifier</param>
        /// <param name="userId">UserID for check</param>
        /// <returns>Returns true if OpenIDUserInfo was found by OpenID Claimed Identifier and if User ID matches to this object.</returns>
        public static bool RemoveOpenIDFromUser(string openId, int userId)
        {
            return ProviderObject.RemoveOpenIDFromUserInternal(openId, userId);
        }


        /// <summary>
        /// Updates OpenID Claimed Identifier for associated user.
        /// </summary>
        /// <param name="oldOpenID">Old OpenID Claimed Identifier related to some user</param>
        /// <param name="newOpenID">New OpenID Claimed Identifier</param>
        /// <param name="userID">ID of given user</param>
        public static void UpdateOpenIDUserInfo(string oldOpenID, string newOpenID, int userID)
        {
            // Check if replacing old OpenID
            if (!String.IsNullOrEmpty(oldOpenID))
            {
                OpenIDUserInfo oui = GetOpenIDUserInfo(oldOpenID);
                {
                    // Update object
                    if ((oui != null) && (!String.IsNullOrEmpty(newOpenID)))
                    {
                        oui.OpenID = newOpenID;
                        DeleteOpenIDUserInfo(oldOpenID);
                        ProviderObject.SetOpenIDUserInfoInternal(oui);
                    }
                    // Delete object when new OpenID is empty
                    else if (String.IsNullOrEmpty(newOpenID))
                    {
                        DeleteOpenIDUserInfo(oldOpenID);
                    }
                }
            }
            // Otherwise we are creating new open ID if set
            else if (!String.IsNullOrEmpty(newOpenID))
            {
                // Create new OpenID <-> User record
                OpenIDUserInfo oui = new OpenIDUserInfo();
                oui.OpenID = newOpenID;
                oui.UserID = userID;
                SetOpenIDUserInfo(oui);
            }
        }

        #endregion


        #region "Internal Methods"

        /// <summary>
        /// Returns UserInfo connected to specified OpenID Claimed Identifier.
        /// </summary>
        /// <param name="openId">OpenID Claimed Identifier</param>
        /// <returns>UserInfo</returns>
        protected virtual UserInfo GetUserInfoByOpenIDInternal(string openId)
        {
            if (!String.IsNullOrEmpty(openId))
            {
                var openIdUserInfo = GetObjectQuery().WhereEquals("OpenID", openId).TopN(1).FirstOrDefault();
                if (openIdUserInfo != null)
                {
                    return UserInfoProvider.GetUserInfo(openIdUserInfo.UserID);
                }
            }
            return null;
        }


        /// <summary>
        /// Returns OpenID Claimed Identifier connected with specified UserID.
        /// </summary>
        /// <param name="userId">UserID to look for</param>
        /// <returns>OpenID Claimed Identifier</returns>
        protected virtual string GetOpenIDByUserIDInternal(int userId)
        {
            var item = GetObjectQuery().WhereEquals("UserID", userId).TopN(1).FirstOrDefault();
            return item?.OpenID;
        }


        /// <summary>
        /// Sets (updates or inserts) specified openIDUser object.
        /// </summary>
        /// <param name="openIDUser">OpenIDUserInfo to set</param>
        protected virtual void SetOpenIDUserInfoInternal(OpenIDUserInfo openIDUser)
        {
            if (openIDUser != null)
            {
                // If the Open ID already exists and is owned by another user, do not allow the change
                var existing = GetOpenIDUserInfo(openIDUser.OpenID);
                if ((existing != null) && (existing.UserID != openIDUser.UserID))
                {
                    throw new InvalidOperationException("[OpenIDUserInfoProvider.SetOpenIDUserInfoInternal]: Open ID '" + existing.OpenID + "' is already assigned to another user.");
                }

                SetInfo(openIDUser);
            }
            else
            {
                throw new Exception("[OpenIDUserInfoProvider.SetOpenIDUserInfo]: No OpenIDUserInfo object set.");
            }
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(OpenIDUserInfo info)
        {
            if (info != null)
            {
                base.DeleteInfo(info);
            }
        }


        /// <summary>
        /// Returns the OpenIDUserInfo structure for the specified OpenID Claimed Identifier.
        /// </summary>
        /// <param name="openID">OpenID Claimed Identifier</param>
        protected virtual OpenIDUserInfo GetOpenIDUserInfoInternal(string openID)
        {
            // Empty ID is invalid
            if (String.IsNullOrEmpty(openID))
            {
                return null;
            }

            return GetOpenIDUsers()
                .WhereEquals("OpenID", openID)
                .TopN(1)
                .FirstOrDefault();
        }


        /// <summary>
        /// Assigns OpenID Claimed Identifier to the user.
        /// </summary>
        /// <param name="openId">OpenID Claimed Identifier</param>
        /// <param name="openIdProviderUrl">URL of OpenID provider</param>
        /// <param name="userId">User ID</param>
        /// <returns>Returns true if user exists and OpenID Claimed Identifier is not already assigned.</returns>
        protected virtual bool AddOpenIDToUserInternal(string openId, string openIdProviderUrl, int userId)
        {
            OpenIDUserInfo oui = GetOpenIDUserInfo(openId);
            UserInfo ui = UserInfoProvider.GetUserInfo(userId);

            // Check if specified OpenID Claimed Identifier is not already assigned and given userId exists
            if ((oui == null) && (ui != null))
            {
                oui = new OpenIDUserInfo();
                oui.OpenID = openId;
                oui.OpenIDProviderUrl = openIdProviderUrl;
                oui.UserID = userId;

                oui.Generalized.InsertData();
                return true;
            }

            return false;
        }


        /// <summary>
        /// Deletes OpenIDUserInfo by specified OpenID Claimed Identifier and checks if given User ID is part of this object.
        /// </summary>
        /// <param name="openId">OpenID Claimed Identifier</param>
        /// <param name="userId">UserID for check</param>
        /// <returns>Returns true if OpenIDUserInfo was found by OpenID Claimed Identifier and if User ID matches to this object.</returns>
        protected virtual bool RemoveOpenIDFromUserInternal(string openId, int userId)
        {
            OpenIDUserInfo oui = GetOpenIDUserInfo(openId);

            // Check if OpenIDUserInfo exists and specified user is assigned to this object
            if ((oui != null) && (oui.UserID == userId))
            {
                DeleteInfo(oui);
                return true;
            }

            return false;
        }

        #endregion
    }
}