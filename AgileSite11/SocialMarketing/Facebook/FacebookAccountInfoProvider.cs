using System;
using System.Linq;

using CMS.DataEngine;

namespace CMS.SocialMarketing
{
    /// <summary>
    /// Provides management of Facebook accounts.
    /// </summary>
    public class FacebookAccountInfoProvider : AbstractInfoProvider<FacebookAccountInfo, FacebookAccountInfoProvider>
    {
        #region "Constructors"

        /// <summary>
        /// Initializes a new instance of the FacebookAccountInfoProvider class.
        /// </summary>
        public FacebookAccountInfoProvider() 
            : base(FacebookAccountInfo.TYPEINFO, new HashtableSettings
            {
                ID = true,
                Load = LoadHashtableEnum.All
            })
        {
        }

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns a query for all the FacebookAccountInfo objects.
        /// </summary>
        /// <returns>Query for all the FacebookAccountInfo objects.</returns>
        public static ObjectQuery<FacebookAccountInfo> GetFacebookAccounts()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Retrieves a Facebook account with the specified identifier, and returns it.
        /// </summary>
        /// <param name="accountId">Facebook account identifier.</param>
        /// <returns>A Facebook account with the specified identifier, if found; otherwise, null.</returns>      
        public static FacebookAccountInfo GetFacebookAccountInfo(int accountId)
        {
            return ProviderObject.GetInfoById(accountId);
        }


        /// <summary>
        /// Returns FacebookAccountInfo with specified GUID.
        /// </summary>
        /// <param name="guid">FacebookAccountInfo GUID</param>                
        public static FacebookAccountInfo GetFacebookAccountInfo(Guid guid)
        {
            return ProviderObject.GetInfoByGuid(guid);
        }


        /// <summary>
        /// Retrieves a FacebookAccountInfo with the specified Facebook page identifier, and returns it.
        /// </summary>
        /// <param name="facebookPageCodeName">Facebook page identifier.</param>
        /// <param name="siteId">Account site identifier.</param>
        /// <returns>A FacebookAccountInfo with the specified Facebook page identifier, if found; otherwise, null.</returns>
        public static FacebookAccountInfo GetFacebookAccountInfo(string facebookPageCodeName, SiteInfoIdentifier siteId)
        {
            return ProviderObject.GetInfoByCodeName(facebookPageCodeName, siteId);
        }


        /// <summary>
        /// Updates or creates the specified Facebook account.
        /// </summary>
        /// <param name="account">Facebook account to be updated or created.</param>
        public static void SetFacebookAccountInfo(FacebookAccountInfo account)
        {
            ProviderObject.SetInfo(account);
        }


        /// <summary>
        /// Deletes the specified Facebook account.
        /// </summary>
        /// <param name="account">Facebook account to be deleted.</param>
        public static void DeleteFacebookAccountInfo(FacebookAccountInfo account)
        {
            ProviderObject.DeleteInfo(account);
        }


        /// <summary>
        /// Deletes the Facebook account with specified identifier.
        /// </summary>
        /// <param name="accountId">Facebook account identifier.</param>
        public static void DeleteFacebookAccountInfo(int accountId)
        {
            FacebookAccountInfo accountObj = GetFacebookAccountInfo(accountId);
            DeleteFacebookAccountInfo(accountObj);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Retrieves a dataset of Facebook accounts for the specified site, and returns it.
        /// </summary>
        /// <param name="siteId">Site identifier.</param>        
        /// <returns>A dataset of Facebook accounts for the specified site.</returns>  
        public static ObjectQuery<FacebookAccountInfo> GetFacebookAccounts(SiteInfoIdentifier siteId)
        {
            return ProviderObject.GetFacebookAccountsInternal(siteId);
        }


        /// <summary>
        /// Gets the default FacebookAccount for given site
        /// </summary>
        /// <param name="siteId">Site for which you want the default FacebookAccount</param>
        /// <returns>Default FacebookAccount for given site, null if no default FacebookAccount exists</returns>
        public static FacebookAccountInfo GetDefaultFacebookAccount(SiteInfoIdentifier siteId)
        {
            return ProviderObject.GetDefaultFacebookAccountInternal(siteId);
        }


        /// <summary>
        /// Retrieves an object query of Facebook accounts belonging to Facebook application with given ID.
        /// </summary>
        /// <param name="applicationId">Facebook application ID.</param>
        /// <returns>Facebook accounts with required application ID.</returns>
        public static ObjectQuery<FacebookAccountInfo> GetFacebookAccountsByApplicationId(int applicationId)
        {
            return ProviderObject.GetFacebookAccountsByApplicationIdInternal(applicationId);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(FacebookAccountInfo info)
        {
            using (var scope = BeginTransaction())
            {
                if (!CheckUniqueValues(info, "FacebookAccountPageID", "FacebookAccountSiteID"))
                {
                    throw new Exception("Facebook account object with the specified page identifier already exists.");
                }

                var defaultAccount = GetDefaultFacebookAccount(info.FacebookAccountSiteID);
                // If account is supposed to be default
                if (defaultAccount != null && info.FacebookAccountIsDefault && defaultAccount.FacebookAccountGUID != info.FacebookAccountGUID)
                {
                    // Make sure only one Account is default for the site
                    QueryDataParameters parameters = new QueryDataParameters();
                    parameters.Add("@SiteID", info.FacebookAccountSiteID);
                    ConnectionHelper.ExecuteQuery("SM.FacebookAccount.MakeAllNonDefault", parameters);

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
        protected override void DeleteInfo(FacebookAccountInfo info)
        {
            ProviderHelper.ClearHashtables(FacebookPostInfo.OBJECT_TYPE, true);
            base.DeleteInfo(info);
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Retrieves a dataset of Facebook accounts for the specified site, and returns it.
        /// </summary>
        /// <param name="siteId">Site identifier.</param>        
        /// <returns>A dataset of Facebook accounts for the specified site.</returns>  
        protected virtual ObjectQuery<FacebookAccountInfo> GetFacebookAccountsInternal(SiteInfoIdentifier siteId)
        {
            return GetObjectQuery().OnSite(siteId);
        }


        /// <summary>
        /// Gets the default FacebookAccount for given site
        /// </summary>
        /// <param name="siteId">Site for which you want the default FacebookAccount</param>
        /// <returns>Default FacebookAccount for given site, null if no default FacebookAccount exists</returns>
        protected virtual FacebookAccountInfo GetDefaultFacebookAccountInternal(SiteInfoIdentifier siteId)
        {
            return GetObjectQuery().Where("FacebookAccountIsDefault", QueryOperator.Equals, true).OnSite(siteId).FirstOrDefault();
        }


        /// <summary>
        /// Retrieves an object query of Facebook accounts belonging to Facebook application with given ID.
        /// </summary>
        /// <param name="applicationId">Facebook application ID.</param>
        /// <returns>Facebook accounts with required application ID.</returns>
        protected virtual ObjectQuery<FacebookAccountInfo> GetFacebookAccountsByApplicationIdInternal(int applicationId)
        {
            return GetObjectQuery().Where("FacebookAccountFacebookApplicationID", QueryOperator.Equals, applicationId);
        }

        #endregion
    }

}