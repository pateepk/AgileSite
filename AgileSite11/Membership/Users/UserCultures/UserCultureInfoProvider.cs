using System;
using System.Data;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.Localization;
using CMS.SiteProvider;

namespace CMS.Membership
{
    /// <summary>
    /// Class providing UserCultureInfo managment.
    /// </summary>
    public class UserCultureInfoProvider : AbstractInfoProvider<UserCultureInfo, UserCultureInfoProvider>
    {
        #region "Public static methods"

        /// <summary>
        /// Returns the query for all relationships between users and cultures.
        /// </summary>   
        public static ObjectQuery<UserCultureInfo> GetUserCultures()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns the UserCultureInfo structure for the specified userCulture.
        /// </summary>
        /// <param name="userId">ID of user</param>
        /// <param name="cultureId">ID of culture</param>
        /// <param name="siteId">ID of site</param>
        public static UserCultureInfo GetUserCultureInfo(int userId, int cultureId, int siteId)
        {
            return ProviderObject.GetUserCultureInfoInternal(userId, cultureId, siteId);
        }


        /// <summary>
        /// Deletes allowed cultures on all sites for given user.
        /// </summary>
        /// <param name="userId">ID of user</param>
        public static void RemoveUserFromAllCultures(int userId)
        {
            ProviderObject.RemoveUserFromAllCulturesInternal(userId);
        }


        /// <summary>
        /// Removes user's allowed cultures for given site.
        /// </summary>
        /// <param name="userId">ID of user</param>
        /// <param name="siteId">ID of site</param>
        public static void RemoveUserFromSite(int userId, int siteId)
        {
            ProviderObject.RemoveUserFromSiteInternal(userId, siteId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified userCulture.
        /// </summary>
        /// <param name="userCulture">UserCulture to set</param>
        public static void SetUserCultureInfo(UserCultureInfo userCulture)
        {
            ProviderObject.SetUserCultureInfoInternal(userCulture);
        }


        /// <summary>
        /// Deletes specified UserCulture.
        /// </summary>
        /// <param name="infoObj">UserCulture object</param>
        public static void DeleteUserCultureInfo(UserCultureInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Removes user from given culture of the given site.
        /// </summary>
        /// <param name="userId">ID of user</param>
        /// <param name="cultureId">ID of culture</param>
        /// <param name="siteId">ID of site</param>
        public static void RemoveUserFromCulture(int userId, int cultureId, int siteId)
        {
            UserCultureInfo infoObj = ProviderObject.GetUserCultureInfoInternal(userId, cultureId, siteId);
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Removes user from given culture of the given site.
        /// </summary>
        /// <param name="user">User info object</param>
        /// <param name="culture">Culture info object</param>
        /// <param name="site">Site info object</param>
        public static void RemoveUserFromCulture(UserInfo user, CultureInfo culture, SiteInfo site)
        {
            UserCultureInfo infoObj = ProviderObject.GetUserCultureInfoInternal(user.UserID, culture.CultureID, site.SiteID);
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Adds user to allowed culture of the site.
        /// </summary>
        /// <param name="userId">ID of user</param>
        /// <param name="cultureId">ID of culture</param>
        /// <param name="siteId">ID of site</param>
        public static void AddUserToCulture(int userId, int cultureId, int siteId)
        {
            ProviderObject.AddUserToCultureInternal(userId, cultureId, siteId);
        }


        /// <summary>
        /// Adds user to allowed culture of the site.
        /// </summary>
        /// <param name="user">User info object</param>
        /// <param name="culture">Culture info object</param>
        /// <param name="site">Site info object</param>
        public static void AddUserToCulture(UserInfo user, CultureInfo culture, SiteInfo site)
        {
            if ((user == null) || (culture == null) || (site == null))
            {
                throw new Exception("[UserCultureInfoProvider.AddUserToCulture]: One or more info objects not set.");
            }
            else
            {
                AddUserToCulture(user.UserID, culture.CultureID, site.SiteID);
            }
        }


        /// <summary>
        /// Gets all allowed cultures for given user and site filtered by where condition and ordered by orderBy expression.
        /// </summary>
        /// <param name="userId">ID of user</param>
        /// <param name="siteId">ID of site</param>
        /// <returns>User's allowed cultures.</returns>
        public static DataTable GetUserCultures(int userId, int siteId)
        {
            return ProviderObject.GetUserCulturesInternal(userId, siteId);
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Returns the UserCultureInfo structure for the specified userCulture.
        /// </summary>
        /// <param name="userId">ID of user</param>
        /// <param name="cultureId">ID of culture</param>
        /// <param name="siteId">ID of site</param>
        protected virtual UserCultureInfo GetUserCultureInfoInternal(int userId, int cultureId, int siteId)
        {
            var where = new WhereCondition()
                                .WhereEquals("UserID", userId)
                                .WhereEquals("CultureID", cultureId)
                                .WhereEquals("SiteID", siteId);

            return GetUserCultures().Where(where).TopN(1).BinaryData(true).FirstObject;
        }


        /// <summary>
        /// Deletes allowed cultures on all sites for given user.
        /// </summary>
        /// <param name="userId">ID of user</param>
        protected virtual void RemoveUserFromAllCulturesInternal(int userId)
        {
            if (userId > 0)
            {
                // Prepare parameters
                QueryDataParameters parameters = new QueryDataParameters();
                parameters.Add("@UserID", userId);

                // Delete allowed cultures
                ConnectionHelper.ExecuteQuery("cms.userculture.deleteforuser", parameters);
            }
        }


        /// <summary>
        /// Removes user's allowed cultures for given site.
        /// </summary>
        /// <param name="userId">ID of user</param>
        /// <param name="siteId">ID of site</param>
        protected virtual void RemoveUserFromSiteInternal(int userId, int siteId)
        {
            if (userId > 0 && siteId > 0)
            {
                // Prepare parameters
                QueryDataParameters parameters = new QueryDataParameters();
                parameters.Add("@UserID", userId);
                parameters.Add("@SiteID", siteId);

                // Remove cultures for user's site
                ConnectionHelper.ExecuteQuery("cms.userculture.removefromsite", parameters);
            }
        }


        /// <summary>
        /// Sets (updates or inserts) specified userCulture.
        /// </summary>
        /// <param name="userCulture">UserCulture to set</param>
        protected virtual void SetUserCultureInfoInternal(UserCultureInfo userCulture)
        {
            if (userCulture != null)
            {
                // Check IDs
                if ((userCulture.UserID <= 0) || (userCulture.CultureID <= 0) || (userCulture.SiteID <= 0))
                {
                    throw new Exception("[UserCultureInfoProvider.SetUserCultureInfo]: Object IDs not set.");
                }

                // Get existing info object
                UserCultureInfo existing = GetUserCultureInfoInternal(userCulture.UserID, userCulture.CultureID, userCulture.SiteID);
                if (existing != null)
                {
                    // Do nothing, item does not carry any data
                }
                else
                {
                    userCulture.Generalized.InsertData();
                }
            }
            else
            {
                throw new Exception("[UserCultureInfoProvider.SetUserCultureInfo]: No UserCultureInfo object set.");
            }
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(UserCultureInfo info)
        {
            if (info != null)
            {
                base.DeleteInfo(info);
            }
        }


        /// <summary>
        /// Gets all allowed cultures for given user and site filtered by where condition and ordered by orderBy expression.
        /// </summary>
        /// <param name="userId">ID of user</param>
        /// <param name="siteId">ID of site, 0 for all sites</param>
        /// <returns>User's allowed cultures.</returns>
        protected virtual DataTable GetUserCulturesInternal(int userId, int siteId)
        {
            var where = new WhereCondition().WhereEquals("UserID", userId);
            if (siteId != 0)
            {
                where = where.WhereEquals("SiteID", siteId);
            }

            DataSet userCultures = GetUserCultures().Where(where).BinaryData(true);

            if (!DataHelper.DataSourceIsEmpty(userCultures))
            {
                return userCultures.Tables[0];
            }
            return null;
        }


        /// <summary>
        /// Deletes allowed culture for all users on given site.
        /// </summary>
        /// <param name="cultureId">ID of culture</param>
        /// <param name="siteId">ID of site</param>
        protected virtual void DeleteSiteAllowedCultureInternal(int cultureId, int siteId)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@CultureID", cultureId);
            parameters.Add("@SiteID", siteId);

            // Delete culture
            ConnectionHelper.ExecuteQuery("cms.userculture.deleteculture", parameters);
        }


        /// <summary>
        /// Adds user to allowed culture of the site.
        /// </summary>
        /// <param name="userId">ID of user</param>
        /// <param name="cultureId">ID of culture</param>
        /// <param name="siteId">ID of site</param>
        protected virtual void AddUserToCultureInternal(int userId, int cultureId, int siteId)
        {
            if (CultureSiteInfoProvider.IsCultureOnSite(cultureId, siteId))
            {
                // Create new culture bindings
                UserCultureInfo infoObj = new UserCultureInfo();

                infoObj.UserID = userId;
                infoObj.CultureID = cultureId;
                infoObj.SiteID = siteId;

                ProviderObject.SetUserCultureInfoInternal(infoObj);
            }
            else
            {
                throw new Exception("[UserCultureInfoProvider.AddUserToCultureInternal]: The culture is not allowed on given site.");
            }
        }

        #endregion
    }
}