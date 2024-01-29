using System;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.LicenseProvider;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Helper class for merging, splitting and deleting accounts.
    /// </summary>
    public class AccountHelper
    {
        #region "Public relation methods"

        /// <summary>
        /// Creates new Account - Contact relation.
        /// </summary>
        public static void CreateAccountContactRelation(int accountID, int contactID, int contactRoleID)
        {
            if (AccountContactInfoProvider.GetAccountContactInfo(accountID, contactID) != null)
            {
                return;
            }

            AccountContactInfo aci = new AccountContactInfo();
            aci.ContactID = contactID;
            aci.AccountID = accountID;
            aci.ContactRoleID = contactRoleID;
            AccountContactInfoProvider.SetAccountContactInfo(aci);
        }

        #endregion


        #region "Public delete methods"

        /// <summary>
        /// Deletes account.
        /// </summary>
        /// <param name="account">Account to be deleted</param>
        /// <param name="deleteSubsidiaries">Indicates if subsidiary accounts should be deleted as well.</param>
        public static void Delete(AccountInfo account, bool deleteSubsidiaries)
        {
            if (account != null)
            {
                if (deleteSubsidiaries)
                {
                    var subsidiaries = AccountInfoProvider.GetAccounts()
                                                          .WhereEquals("AccountSubsidiaryOfID", account.AccountID)
                                                          .OrderBy("AccountID");

                    foreach (var subsidiary in subsidiaries)
                    {
                        Delete(subsidiary, deleteSubsidiaries);
                    }
                }

                AccountInfoProvider.DeleteAccountInfo(account);
            }
        }

        #endregion


        #region "Public general methods"

        /// <summary>
        /// Creates and returns new  account.
        /// </summary>
        /// <returns>Returns new AccountInfo</returns>
        public static AccountInfo GetNewAccount(string namePrefix)
        {
            // Check license
            LicenseHelper.RequestFeature(RequestContext.CurrentDomain, FeatureEnum.FullContactManagement);

            var account = new AccountInfo();
            account.AccountName = GetName(namePrefix);
            AccountInfoProvider.SetAccountInfo(account);
            return account;
        }


        /// <summary>
        /// Returns name for newly created account
        /// </summary>
        private static string GetName(string namePrefix)
        {
            return namePrefix + DateTime.Now.ToString("yyyy-MM-dd_HH:mm:ss");
        }

        #endregion
    }
}