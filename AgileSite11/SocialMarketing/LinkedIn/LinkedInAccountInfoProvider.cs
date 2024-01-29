using System;
using System.Linq;

using CMS.DataEngine;

namespace CMS.SocialMarketing
{
    /// <summary>
    /// Provides management of LinkedIn accounts.
    /// </summary>
    public class LinkedInAccountInfoProvider : AbstractInfoProvider<LinkedInAccountInfo, LinkedInAccountInfoProvider>
    {
        #region "Constructors"

        /// <summary>
        /// Initializes a new instance of the LinkedInAccountInfoProvider class.
        /// </summary>
        public LinkedInAccountInfoProvider()
            : base(LinkedInAccountInfo.TYPEINFO, new HashtableSettings
            {
                ID = true,
                Load = LoadHashtableEnum.All
            })
        {
        }

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns a query for all the LinkedInAccountInfo objects.
        /// </summary>
        public static ObjectQuery<LinkedInAccountInfo> GetLinkedInAccounts()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Retrieves a LinkedIn account with the specified identifier, and returns it.
        /// </summary>
        /// <param name="accountId">LinkedIn account identifier.</param>
        /// <returns>A LinkedIn account with the specified identifier, if found; otherwise, null.</returns>      
        public static LinkedInAccountInfo GetLinkedInAccountInfo(int accountId)
        {
            return ProviderObject.GetInfoById(accountId);
        }


        /// <summary>
        /// Retrieves a LinkedIn account with the specified identifier, and returns it.
        /// </summary>
        /// <param name="codeName">LinkedIn account code name.</param>
        /// <param name="siteId">ID of the site the account is for.</param>
        /// <returns>A LinkedIn account with the specified identifier, if found; otherwise, null.</returns>      
        public static LinkedInAccountInfo GetLinkedInAccountInfo(string codeName, SiteInfoIdentifier siteId)
        {
            return ProviderObject.GetInfoByCodeName(codeName, siteId);
        }


        /// <summary>
        /// Returns LinkedInAccountInfo with specified GUID.
        /// </summary>
        /// <param name="guid">LinkedInAccountInfo GUID</param>                
        public static LinkedInAccountInfo GetLinkedInAccountInfo(Guid guid)
        {
            return ProviderObject.GetInfoByGuid(guid);
        }


        /// <summary>
        /// Updates or creates the specified LinkedIn account.
        /// </summary>
        /// <param name="account">LinkedIn account to be updated or created.</param>
        public static void SetLinkedInAccountInfo(LinkedInAccountInfo account)
        {
            ProviderObject.SetInfo(account);
        }


        /// <summary>
        /// Deletes the specified LinkedIn account.
        /// </summary>
        /// <param name="account">LinkedIn account to be deleted.</param>
        public static void DeleteLinkedInAccountInfo(LinkedInAccountInfo account)
        {
            ProviderObject.DeleteInfo(account);
        }


        /// <summary>
        /// Deletes the LinkedIn account with specified identifier.
        /// </summary>
        /// <param name="accountId">LinkedIn account identifier.</param>
        public static void DeleteLinkedInAccountInfo(int accountId)
        {
            LinkedInAccountInfo accountObj = GetLinkedInAccountInfo(accountId);
            DeleteLinkedInAccountInfo(accountObj);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Returns a query for all the LinkedInAccountInfo objects of a specified site.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        public static ObjectQuery<LinkedInAccountInfo> GetLinkedInAccounts(SiteInfoIdentifier siteId)
        {
            return ProviderObject.GetLinkedInAccountsInternal(siteId);
        }


        /// <summary>
        /// Gets the default LinkedInAccount for given site.
        /// </summary>
        /// <param name="siteId">Site for which you want the default LinkedInAccount</param>
        /// <returns>Default LinkedInAccount for given site, null if no default LinkedInAccount exists</returns>
        public static LinkedInAccountInfo GetDefaultLinkedInAccount(SiteInfoIdentifier siteId)
        {
            return ProviderObject.GetDefaultLinkedInAccountInternal(siteId);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(LinkedInAccountInfo info)
        {
            using (var scope = BeginTransaction())
            {
                if (!CheckUniqueValues(info, "LinkedInAccountProfileID", "LinkedInAccountSiteID"))
                {
                    throw new Exception("[LinkedInAccountInfoProvider.SetInfo]: LinkedIn account object with the specified profile ID already exists.");
                }

                var defaultAccount = GetDefaultLinkedInAccount(info.LinkedInAccountSiteID);
                // If account is supposed to be default
                if (defaultAccount != null && info.LinkedInAccountIsDefault && defaultAccount.LinkedInAccountGUID != info.LinkedInAccountGUID)
                {
                    // Make sure only one Account is default for the site
                    QueryDataParameters parameters = new QueryDataParameters();
                    parameters.Add("@SiteID", info.LinkedInAccountSiteID);
                    ConnectionHelper.ExecuteQuery("SM.LinkedInAccount.MakeAllNonDefault", parameters);

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
        protected override void DeleteInfo(LinkedInAccountInfo info)
        {
            ProviderHelper.ClearHashtables(LinkedInPostInfo.OBJECT_TYPE, true);
            base.DeleteInfo(info);
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Returns a query for all the LinkedInAccountInfo objects of a specified site.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        protected virtual ObjectQuery<LinkedInAccountInfo> GetLinkedInAccountsInternal(SiteInfoIdentifier siteId)
        {
            return GetObjectQuery().OnSite(siteId);
        }


        /// <summary>
        /// Gets the default LinkedInAccount for given site
        /// </summary>
        /// <param name="siteId">Site for which you want the default LinkedInAccount</param>
        /// <returns>Default LinkedInAccount for given site, null if no default LinkedInAccount exists</returns>
        protected virtual LinkedInAccountInfo GetDefaultLinkedInAccountInternal(SiteInfoIdentifier siteId)
        {
            return GetObjectQuery().WhereEquals("LinkedInAccountIsDefault", true).OnSite(siteId).FirstOrDefault();
        }

        #endregion
    }

}