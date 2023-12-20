using System;

using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Class providing AccountInfo management.
    /// </summary>
    public class AccountInfoProvider : AbstractInfoProvider<AccountInfo, AccountInfoProvider>
    {
        #region "Variables"

        /// <summary>
        /// Locking object for mass deleting.
        /// </summary>
        private static readonly object deleteLock = new object();

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor which sets weak references for ID caching.
        /// </summary>
        public AccountInfoProvider()
            : base(AccountInfo.TYPEINFO, new HashtableSettings
				{
					ID = true,
                    UseWeakReferences = true
				})
        {
        }

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns query for all the accounts objects.
        /// </summary>
        public static ObjectQuery<AccountInfo> GetAccounts()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns account with specified ID.
        /// </summary>
        /// <param name="accountId">Account ID</param>        
        public static AccountInfo GetAccountInfo(int accountId)
        {
            return ProviderObject.GetInfoById(accountId);
        }


        /// <summary>
        /// Returns account with specified GUID.
        /// </summary>
        /// <param name="accountGuid">Account GUID</param>   
        public static AccountInfo GetAccountInfo(Guid accountGuid)
        {
            return ProviderObject.GetInfoByGuid(accountGuid);
        }


        /// <summary>
        /// Sets (updates or inserts) specified account.
        /// </summary>
        /// <param name="accountObj">Account to be set</param>
        public static void SetAccountInfo(AccountInfo accountObj)
        {
            ProviderObject.SetInfo(accountObj);
        }


        /// <summary>
        /// Deletes specified account.
        /// </summary>
        /// <param name="accountObj">Account to be deleted</param>
        public static void DeleteAccountInfo(AccountInfo accountObj)
        {
            ProviderObject.DeleteInfo(accountObj);
        }


        /// <summary>
        /// Deletes account with specified ID.
        /// </summary>
        /// <param name="accountId">Account ID</param>
        public static void DeleteAccountInfo(int accountId)
        {
            AccountInfo accountObj = GetAccountInfo(accountId);
            DeleteAccountInfo(accountObj);
        }


        /// <summary>
        /// Deletes accounts by specified WHERE condition.
        /// </summary>
        /// <param name="where">SQL WHERE condition</param>
        /// <param name="deleteSubsidiaries">Delete subsidiaries</param>
        public static void DeleteAccountInfos(string where, bool deleteSubsidiaries)
        {
            ProviderObject.DeleteAccountInfosInternal(where, deleteSubsidiaries);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Updates HQ ID of specified accounts.
        /// </summary>
        /// <param name="subsidiaryOfID">HQ ID</param>
        /// <param name="where">Where condition</param>
        public static void UpdateAccountHQ(int subsidiaryOfID, string where)
        {
            ProviderObject.UpdateAccountHQInternal(subsidiaryOfID, where);
        }


        /// <summary>
        /// Updates account status ID of specified accounts.
        /// </summary>
        /// <param name="statusId">Account status ID</param>
        /// <param name="where">Where condition</param>
        public static void UpdateAccountStatus(int statusId, string where)
        {
            ProviderObject.UpdateAccountStatusInternal(statusId, where);
        }


        /// <summary>
        /// Updates primary and secondary contact of all accounts.
        /// </summary>
        /// <param name="oldContactID">ID of contact to be replaced</param>
        /// <param name="newContactID">ID of new contact to replace oldContact</param>
        public static void UpdateAccountPrimaryContact(int oldContactID, int newContactID)
        {
            ProviderObject.UpdateAccountPrimaryContactInternal(oldContactID, newContactID);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(AccountInfo info)
        {
            // When inserting new record set creation time
            if ((info != null) && (info.AccountCreated == DateTimeHelper.ZERO_TIME))
            {
                info.AccountCreated = DateTime.Now;
            }

            base.SetInfo(info);
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(AccountInfo info)
        {
            // Prepare the parameters
            var parameters = new QueryDataParameters();
            parameters.Add("@where", "AccountID = " + info.AccountID);
            ConnectionHelper.ExecuteQuery("OM.ContactGroupMember.removecontactsfromaccount", parameters);
            base.DeleteInfo(info);
        }


        /// <summary>
        /// Deletes accounts by WHERE condition
        /// </summary>
        /// <param name="where">SQL WHERE condition to specify deleted accounts</param>        
        /// <param name="deleteSubsidiaries">Indicates if subsidiaries should be deleted</param>        
        protected virtual void DeleteAccountInfosInternal(string where, bool deleteSubsidiaries)
        {
            // Prepare parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@where", where);
            parameters.Add("@deleteSubsidiaries", deleteSubsidiaries);

            lock (deleteLock)
            {
                // Set long timeout so that mass delete can finish successfully
                using (var cs = new CMSConnectionScope())
                {
                    cs.CommandTimeout = ConnectionHelper.LongRunningCommandTimeout;

                    ConnectionHelper.ExecuteQuery("om.account.massdelete", parameters);
                }
            }
        }


        /// <summary>
        /// Updates primary and secondary contact of all accounts.
        /// </summary>
        /// <param name="oldContactID">ID of contact to be replaced</param>
        /// <param name="newContactID">ID of new contact to replace oldContact</param>
        protected virtual void UpdateAccountPrimaryContactInternal(int oldContactID, int newContactID)
        {
            // Prepare the parameters
            var parameters = new QueryDataParameters();
            parameters.Add("@OldContactID", oldContactID);
            parameters.Add("@NewContactID", newContactID);

            ConnectionHelper.ExecuteQuery("om.account.updateprimarycontact", parameters);

            // Clear hash tables
            ClearHashtables(true);
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Updates HQ ID of specified accounts.
        /// </summary>
        /// <param name="subsidiaryOfID">HQ account ID</param>
        /// <param name="where">Where condition</param>
        protected virtual void UpdateAccountHQInternal(int subsidiaryOfID, string where)
        {
            string updateExp = "AccountSubsidiaryOfID=";
            if (subsidiaryOfID > 0)
            {
                updateExp += subsidiaryOfID.ToString();
            }
            else
            {
                updateExp += "NULL";
            }
            UpdateData(updateExp, null, where);
            ClearHashtables(true);
        }


        /// <summary>
        /// Updates account status ID of specified accounts.
        /// </summary>
        /// <param name="statusId">Account status ID</param>
        /// <param name="where">Where condition</param>
        protected virtual void UpdateAccountStatusInternal(int statusId, string where)
        {
            string updateExp = "AccountStatusID=";
            if (statusId > 0)
            {
                updateExp += statusId.ToString();
            }
            else
            {
                updateExp += "NULL";
            }
            UpdateData(updateExp, null, where);
            ClearHashtables(true);
        }

        #endregion
    }
}