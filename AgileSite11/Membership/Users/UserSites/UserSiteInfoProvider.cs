using System;
using System.Text;
using System.Linq;

using CMS.Base;
using CMS.DataEngine;
using CMS.LicenseProvider;
using CMS.Search;
using CMS.SiteProvider;

namespace CMS.Membership
{
    /// <summary>
    /// Class providing UserSiteInfo management.
    /// </summary>
    public class UserSiteInfoProvider : AbstractInfoProvider<UserSiteInfo, UserSiteInfoProvider>
    {
        #region "Public static methods"

        /// <summary>
        /// Returns the UserSiteInfo structure for the specified userSite.
        /// </summary>
        /// <param name="userId">UserID</param>
        /// <param name="siteId">SiteID</param>
        public static UserSiteInfo GetUserSiteInfo(int userId, int siteId)
        {
            return ProviderObject.GetUserSiteInfoInternal(userId, siteId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified userSite.
        /// </summary>
        /// <param name="userSite">UserSite to set</param>
        public static void SetUserSiteInfo(UserSiteInfo userSite)
        {
            ProviderObject.SetInfo(userSite);
        }


        /// <summary>
        /// Deletes specified userSite.
        /// </summary>
        /// <param name="infoObj">UserSite object</param>
        public static void DeleteUserSiteInfo(UserSiteInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified userSite.
        /// </summary>
        /// <param name="userId">UserID</param>
        /// <param name="siteId">SiteID</param>
        public static void RemoveUserFromSite(int userId, int siteId)
        {
            // Remove user from site
            UserSiteInfo infoObj = GetUserSiteInfo(userId, siteId);
            DeleteUserSiteInfo(infoObj);
        }


        /// <summary>
        /// Adds specified user to the site.
        /// </summary>
        /// <param name="userId">UserID</param>
        /// <param name="siteId">SiteID</param>
        public static void AddUserToSite(int userId, int siteId)
        {
            // Get infos
            UserInfo ui = UserInfoProvider.GetUserInfo(userId);
            SiteInfo si = SiteInfoProvider.GetSiteInfo(siteId);

            // Set relation
            AddUserToSite(ui, si);
        }


        /// <summary>
        /// Adds specified user to the site.
        /// </summary>
        /// <param name="ui">User info</param>
        /// <param name="si">Site info</param>
        public static void AddUserToSite(UserInfo ui, SiteInfo si)
        {
            if ((si == null) || (ui == null))
            {
                return;
            }

            // Clear license cache
            UserInfoProvider.ClearLicenseValues();

            if (ui.SiteIndependentPrivilegeLevel == UserPrivilegeLevelEnum.Editor)
            {
                if (!UserInfoProvider.LicenseVersionCheck(si.DomainName, FeatureEnum.Editors, ObjectActionEnum.Insert, false))
                {
                    LicenseHelper.GetAllAvailableKeys(FeatureEnum.Editors);

                    // Should happen only if is not redirected in previous step (WindowsAuth)
                    throw new Exception(Encoding.ASCII.GetString(Convert.FromBase64String("W1VzZXIuVmVyc2lvbkNoZWNrXSBSZXF1ZXN0ZWQgYWN0aW9uIGNhbiBub3QgYmUgZXhlY3V0ZWQgZHVlIHRvIGxpY2Vuc2UgbGltaXRhdGlvbnMu")));
                }
            }

            if (!UserInfoProvider.LicenseVersionCheck(si.DomainName, FeatureEnum.SiteMembers, ObjectActionEnum.Insert, false))
            {
                LicenseHelper.GetAllAvailableKeys(FeatureEnum.SiteMembers);

                // Should happen only if is not redirected in previous step (WindowsAuth)
                throw new Exception(Encoding.ASCII.GetString(Convert.FromBase64String("W1VzZXIuVmVyc2lvbkNoZWNrXSBSZXF1ZXN0ZWQgYWN0aW9uIGNhbiBub3QgYmUgZXhlY3V0ZWQgZHVlIHRvIGxpY2Vuc2UgbGltaXRhdGlvbnMu")));
            }


            // Create new binding
            UserSiteInfo infoObj = ProviderObject.CreateInfo();

            infoObj.UserID = ui.UserID;
            infoObj.SiteID = si.SiteID;

            // Save to the database
            SetUserSiteInfo(infoObj);

            // Clear license cache
            UserInfoProvider.ClearLicenseValues();

            // Log search task
            if (SearchIndexInfoProvider.SearchTypeEnabled(UserInfo.OBJECT_TYPE))
            {
                SearchTaskInfoProvider.CreateTask(SearchTaskTypeEnum.Process, UserInfo.OBJECT_TYPE, UserInfo.TYPEINFO.IDColumn, infoObj.UserID.ToString(), infoObj.UserID);
            }
        }


        /// <summary>
        /// Returns the query for all relationships between users and sites.
        /// </summary>   
        public static ObjectQuery<UserSiteInfo> GetUserSites()
        {
            return ProviderObject.GetObjectQuery();
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Returns the UserSiteInfo structure for the specified userSite.
        /// </summary>
        /// <param name="userId">UserID</param>
        /// <param name="siteId">SiteID</param>
        protected virtual UserSiteInfo GetUserSiteInfoInternal(int userId, int siteId)
        {
            var condition = new WhereCondition()
                .WhereEquals("SiteID", siteId)
                .WhereEquals("UserID", userId);

            return GetObjectQuery().TopN(1).Where(condition).FirstOrDefault();
        }


        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(UserSiteInfo info)
        {
            base.SetInfo(info);

            // Invalidate parent user data to ensure reloading user sites/roles
            if (info != null)
            {
                info.Parent.Generalized.Invalidate(false);
            }
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(UserSiteInfo info)
        {
            using (var tr = BeginTransaction())
            {
                UserRoleInfoProvider.RemoveUserFromSiteRoles(info.UserID, info.SiteID);

                // Remove user from site
                base.DeleteInfo(info);

                tr.Commit();
            }

            // Clear the licensing cache
            UserInfoProvider.ClearLicenseValues();

            // Log the search task
            if (SearchIndexInfoProvider.SearchTypeEnabled(UserInfo.OBJECT_TYPE))
            {
                SearchTaskInfoProvider.CreateTask(SearchTaskTypeEnum.Process, UserInfo.OBJECT_TYPE, UserInfo.TYPEINFO.IDColumn, info.UserID.ToString(), info.UserID);
            }
        }

        #endregion
    }
}