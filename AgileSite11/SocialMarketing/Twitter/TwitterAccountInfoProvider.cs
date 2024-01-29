using System;
using System.Linq;

using CMS.DataEngine;

namespace CMS.SocialMarketing
{
    /// <summary>
    /// Provides management of Twitter accounts.
    /// </summary>
    public class TwitterAccountInfoProvider : AbstractInfoProvider<TwitterAccountInfo, TwitterAccountInfoProvider>
    {
        #region "Constructors"

        /// <summary>
        /// Initializes a new instance of the TwitterAccountInfoProvider class.
        /// </summary>
        public TwitterAccountInfoProvider()
            : base(TwitterAccountInfo.TYPEINFO, new HashtableSettings
            {
                ID = true,
                Load = LoadHashtableEnum.All
            })
        {
        }

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns a query for all the TwitterAccountInfo objects.
        /// </summary>
        public static ObjectQuery<TwitterAccountInfo> GetTwitterAccounts()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Retrieves a Twitter account with the specified identifier, and returns it.
        /// </summary>
        /// <param name="accountId">Twitter account identifier.</param>
        /// <returns>A Twitter account with the specified identifier, if found; otherwise, null.</returns>      
        public static TwitterAccountInfo GetTwitterAccountInfo(int accountId)
        {
            return ProviderObject.GetInfoById(accountId);
        }


        /// <summary>
        /// Retrieves a Twitter account with the specified identifier, and returns it.
        /// </summary>
        /// <param name="codeName">Twitter account code name.</param>
        /// <param name="siteId">ID of the site the account is for.</param>
        /// <returns>A Twitter account with the specified identifier, if found; otherwise, null.</returns>      
        public static TwitterAccountInfo GetTwitterAccountInfo(string codeName, SiteInfoIdentifier siteId)
        {
            return ProviderObject.GetInfoByCodeName(codeName, siteId);
        }


        /// <summary>
        /// Returns TwitterAccountInfo with specified GUID.
        /// </summary>
        /// <param name="guid">TwitterAccountInfo GUID</param>                
        public static TwitterAccountInfo GetTwitterAccountInfo(Guid guid)
        {
            return ProviderObject.GetInfoByGuid(guid);
        }


        /// <summary>
        /// Updates or creates the specified Twitter account.
        /// </summary>
        /// <param name="account">Twitter account to be updated or created.</param>
        public static void SetTwitterAccountInfo(TwitterAccountInfo account)
        {
            ProviderObject.SetInfo(account);
        }


        /// <summary>
        /// Deletes the specified Twitter account.
        /// </summary>
        /// <param name="account">Twitter account to be deleted.</param>
        public static void DeleteTwitterAccountInfo(TwitterAccountInfo account)
        {
            ProviderObject.DeleteInfo(account);
        }


        /// <summary>
        /// Deletes the Twitter account with specified identifier.
        /// </summary>
        /// <param name="accountId">Twitter account identifier.</param>
        public static void DeleteTwitterAccountInfo(int accountId)
        {
            TwitterAccountInfo accountObj = GetTwitterAccountInfo(accountId);
            DeleteTwitterAccountInfo(accountObj);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Returns a query for all the TwitterAccountInfo objects of a specified site.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        public static ObjectQuery<TwitterAccountInfo> GetTwitterAccounts(SiteInfoIdentifier siteId)
        {
            return ProviderObject.GetTwitterAccountsInternal(siteId);
        }


        /// <summary>
        /// Gets the default TwitterAccount for given site
        /// </summary>
        /// <param name="siteId">Site for which you want the default TwitterAccount</param>
        /// <returns>Default TwitterAccount for given site, null if no default TwitterAccount exists</returns>
        public static TwitterAccountInfo GetDefaultTwitterAccount(SiteInfoIdentifier siteId)
        {
            return ProviderObject.GetDefaultTwitterAccountInternal(siteId);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(TwitterAccountInfo info)
        {
            using (var scope = BeginTransaction())
            {
                if (!CheckUniqueValues(info, "TwitterAccountAccessToken", "TwitterAccountSiteID"))
                {
                    throw new Exception("[TwitterAccountInfoProvider.SetInfo]: Twitter account object with the specified access token already exists.");
                }

                var defaultAccount = GetDefaultTwitterAccount(info.TwitterAccountSiteID);
                // If account is supposed to be default
                if (defaultAccount != null && info.TwitterAccountIsDefault && defaultAccount.TwitterAccountGUID != info.TwitterAccountGUID)
                {
                    // Make sure only one Account is default for the site
                    QueryDataParameters parameters = new QueryDataParameters();
                    parameters.Add("@SiteID", info.TwitterAccountSiteID);
                    ConnectionHelper.ExecuteQuery("SM.TwitterAccount.MakeAllNonDefault", parameters);
                    ClearHashtables(true);
                }

                base.SetInfo(info);

                scope.Commit();
            }
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(TwitterAccountInfo info)
        {
            ProviderHelper.ClearHashtables(TwitterPostInfo.OBJECT_TYPE, true);
            base.DeleteInfo(info);
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Returns a query for all the TwitterAccountInfo objects of a specified site.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        protected virtual ObjectQuery<TwitterAccountInfo> GetTwitterAccountsInternal(SiteInfoIdentifier siteId)
        {
            return GetObjectQuery().OnSite(siteId);
        }


        /// <summary>
        /// Gets the default TwitterAccount for given site
        /// </summary>
        /// <param name="siteId">Site for which you want the default TwitterAccount</param>
        /// <returns>Default TwitterAccount for given site, null if no default TwitterAccount exists</returns>
        protected virtual TwitterAccountInfo GetDefaultTwitterAccountInternal(SiteInfoIdentifier siteId)
        {
            return GetObjectQuery().Where("TwitterAccountIsDefault", QueryOperator.Equals, true).OnSite(siteId).FirstOrDefault();
        }

        #endregion
    }

}