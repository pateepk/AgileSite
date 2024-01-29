using CMS.DataEngine;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Class providing AccountStatusInfo management.
    /// </summary>
    public class AccountStatusInfoProvider : AbstractInfoProvider<AccountStatusInfo, AccountStatusInfoProvider>
    {
        #region "Constructor"

        /// <summary>
        /// Constructor using ID and codename Hashtables.
        /// </summary>
        public AccountStatusInfoProvider()
            : base(AccountStatusInfo.TYPEINFO, new HashtableSettings
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
        /// Returns a query for all the AccountStatusInfo objects.
        /// </summary>
        public static ObjectQuery<AccountStatusInfo> GetAccountStatuses()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns account status with specified ID.
        /// </summary>
        /// <param name="statusId">Account status ID</param>        
        public static AccountStatusInfo GetAccountStatusInfo(int statusId)
        {
            return ProviderObject.GetInfoById(statusId);
        }


        /// <summary>
        /// Returns account status with specified name.
        /// </summary>
        /// <param name="statusName">Account status name</param>
        public static AccountStatusInfo GetAccountStatusInfo(string statusName)
        {
            return ProviderObject.GetInfoByCodeName(statusName);
        }


        /// <summary>
        /// Sets (updates or inserts) specified account status.
        /// </summary>
        /// <param name="statusObj">Account status to be set</param>
        public static void SetAccountStatusInfo(AccountStatusInfo statusObj)
        {
            ProviderObject.SetInfo(statusObj);
        }


        /// <summary>
        /// Deletes specified account status.
        /// </summary>
        /// <param name="statusObj">Account status to be deleted</param>
        public static void DeleteAccountStatusInfo(AccountStatusInfo statusObj)
        {
            ProviderObject.DeleteInfo(statusObj);
        }


        /// <summary>
        /// Deletes account status with specified ID.
        /// </summary>
        /// <param name="statusId">Account status ID</param>
        public static void DeleteAccountStatusInfo(int statusId)
        {
            AccountStatusInfo statusObj = GetAccountStatusInfo(statusId);
            DeleteAccountStatusInfo(statusObj);
        }

        #endregion
    }
}