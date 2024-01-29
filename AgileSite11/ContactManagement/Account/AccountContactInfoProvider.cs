using System;
using System.Linq;

using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Class providing AccountContactInfo management.
    /// </summary>
    public class AccountContactInfoProvider : AbstractInfoProvider<AccountContactInfo, AccountContactInfoProvider>
    {
        #region "Public methods - Basic"

        /// <summary>
        /// Returns query of all relationships between accounts and contacts.
        /// </summary>
        /// <returns></returns>
        public static ObjectQuery<AccountContactInfo> GetRelationships()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns relationship between specified account and contact.
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <param name="contactId">Contact ID</param>
        public static AccountContactInfo GetAccountContactInfo(int accountId, int contactId)
        {
            return ProviderObject.GetAccountContactInfoInternal(accountId, contactId);
        }


        /// <summary>
        /// Returns relationship by ID.
        /// </summary>
        /// <param name="accountContactId">AccountContactID</param>
        public static AccountContactInfo GetAccountContactInfo(int accountContactId)
        {
            return ProviderObject.GetInfoById(accountContactId);
        }


        /// <summary>
        /// Sets relationship between specified account and contact.
        /// </summary>
        /// <param name="infoObj">Account-contact relationship to be set</param>
        public static void SetAccountContactInfo(AccountContactInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes relationship between specified account and contact.
        /// </summary>
        /// <param name="infoObj">Account-contact relationship to be deleted</param>
        public static void DeleteAccountContactInfo(AccountContactInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes relationship between specified account and contact.
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <param name="contactId">Contact ID</param>
        public static void DeleteAccountContactInfo(int accountId, int contactId)
        {
            AccountContactInfo infoObj = GetAccountContactInfo(accountId, contactId);
            DeleteAccountContactInfo(infoObj);
        }


        /// <summary>
        /// Deletes relationship with specific AccountContactID.
        /// </summary>
        /// <param name="accountContactId">AccountContactID</param>
        public static void DeleteAccountContactInfo(int accountContactId)
        {
            DeleteAccountContactInfo(GetAccountContactInfo(accountContactId));
        }


        /// <summary>
        /// Deletes relationships between specified accounts and contacts defined in where condition.
        /// </summary>
        /// <param name="where">Where condition</param>
        public static void DeleteAllAccountContacts(string where)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@where", where);
            ConnectionHelper.ExecuteQuery("OM.ContactGroupMember.removecontactsfromaccount", parameters);

            ProviderObject.DeleteAllAccountContactsInternal(where);
        }


        /// <summary>
        /// Updates contact role of relationships between accounts and contacts.
        /// </summary>
        /// <param name="roleId">New contact role ID</param>
        /// <param name="where">Where condition</param>
        public static void UpdateContactRole(int roleId, string where)
        {
            ProviderObject.UpdateContactRoleInternal(roleId, where);
        }


        /// <summary>
        /// Resets main contact IDs of account(s) to NULL if any of the contacts, specified by where-condition, was set as primary or secondary contact.
        /// </summary>
        /// <param name="accountId">Account ID - required if called from specific account</param>
        /// <param name="contactId">Contact ID - required if called from specific contact</param>
        /// <param name="where">Where condition</param>
        public static void ResetAccountMainContacts(int accountId, int contactId, string where)
        {
            ProviderObject.ResetAccountMainContactsInternal(accountId, contactId, where);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Sets relationship between specified account and contact.
        /// </summary>
        /// <param name="accountID">Account to be set</param>
        /// <param name="where">WHERE condition specifying contacts to be set</param>
        /// <param name="roleID">Account-contact role to be set</param>
        public static void SetContactsIntoAccount(int accountID, string where, int roleID)
        {
            ProviderObject.SetContactsIntoAccountInternal(accountID, where, roleID);
        }


        /// <summary>
        /// Sets relationship between specified account and contact.
        /// </summary>
        /// <param name="contactID">Contact to be set</param>
        /// <param name="where">WHERE condition specifying contacts to be set</param>
        /// <param name="roleID">Account-contact role to be set</param>
        public static void SetAccountsIntoContact(int contactID, string where, int roleID)
        {
            ProviderObject.SetAccountsIntoContactInternal(contactID, where, roleID);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Returns relationship between specified account and contact.
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <param name="contactId">Contact ID</param>
        protected virtual AccountContactInfo GetAccountContactInfoInternal(int accountId, int contactId)
        {
            return GetObjectQuery().TopN(1)
                    .WhereEquals("AccountID", accountId)
                    .WhereEquals("ContactID", contactId)
                    .FirstOrDefault();
        }


        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(AccountContactInfo info)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@accountID", info.AccountID);
            parameters.Add("@contactID", info.ContactID);
            ConnectionHelper.ExecuteQuery("om.contactgroupmember.addcontactintoaccount", parameters);

            base.SetInfo(info);
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(AccountContactInfo info)
        {
            if (info != null)
            {
                // Reset account's main contact IDs if the contact was set as primary or secondary contact
                ResetAccountMainContactsInternal(info.AccountID, info.ContactID, null);

                // Prepare the parameters
                QueryDataParameters parameters = new QueryDataParameters();
                parameters.Add("@where", "AccountID = " + info.AccountID + " AND ContactID = " + info.ContactID);
                ConnectionHelper.ExecuteQuery("OM.ContactGroupMember.removecontactsfromaccount", parameters);

                // Delete the object
                base.DeleteInfo(info);
            }
        }


        /// <summary>
        /// Deletes realtionships between specified accounts and contacts defined in where condition.
        /// </summary>
        /// <param name="where">Where condition</param>
        protected virtual void DeleteAllAccountContactsInternal(string where)
        {
            BulkDelete(new WhereCondition(where));
        }


        /// <summary>
        /// Updates contact role of relationships between accounts and contacts.
        /// </summary>
        /// <param name="roleId">New contact role ID</param>
        /// <param name="where">Where condition</param>
        protected virtual void UpdateContactRoleInternal(int roleId, string where)
        {
            string updateExp = "ContactRoleID=";
            if (roleId > 0)
            {
                updateExp += roleId.ToString();
            }
            else
            {
                updateExp += "NULL";
            }
            UpdateData(updateExp, null, where);
        }


        /// <summary>
        /// Resets main contact IDs of account(s) to NULL if any of the contacts, specified by where-condition, was set as primary or secondary contact.
        /// </summary>
        /// <param name="accountId">Account ID - required if called from specific account</param>
        /// <param name="contactId">Contact ID - required if called from specific contact</param>
        /// <param name="where">Where condition</param>
        protected virtual void ResetAccountMainContactsInternal(int accountId, int contactId, string where)
        {
            if ((accountId <= 0) && (contactId <= 0))
            {
                return;
            }

            if ((accountId > 0) && (contactId > 0))
            {
                where = string.Format("={0} AND [AccountID]={1}", contactId, accountId);
            }
            else if (accountId > 0)
            {
                where = string.Format(" IN (SELECT ContactID FROM OM_AccountContact WHERE {0}) AND [AccountID]={1}", where, accountId);
            }
            else if (contactId > 0)
            {
                where = string.Format("={0} AND [AccountID] IN (SELECT AccountID FROM OM_AccountContact WHERE {1})", contactId, where);
            }
            // Query is in the form: UPDATE OM_Account SET [AccountPrimaryContactID] = NULL WHERE [AccountPrimaryContactID] ##WHERE##;
            //                       UPDATE OM_Account SET [AccountSecondaryContactID] = NULL WHERE [AccountSecondaryContactID] ##WHERE##
            ConnectionHelper.ExecuteQuery("om.accountcontact.resetaccountmaincontacts", null, where, null);
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Sets relationship between specified account and contact.
        /// </summary>
        /// <param name="accountID">Account to be set</param>
        /// <param name="where">WHERE condition specifying contacts to be set</param>
        /// <param name="roleID">Account-contact role to be set</param>
        public void SetContactsIntoAccountInternal(int accountID, string where, int roleID)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@accountID", accountID);
            parameters.Add("@where", where);
            parameters.Add("@roleID", roleID);

            ConnectionHelper.ExecuteQuery("OM.accountcontact.SetContactsIntoAccount", parameters);

            CacheHelper.TouchKey(String.Format("om.account|byid|{0}|children|om.accountcontact", accountID));
        }


        /// <summary>
        /// Sets relationship between specified account and contact.
        /// </summary>
        /// <param name="contactID">Contact to be set</param>
        /// <param name="where">WHERE condition specifying contacts to be set</param>
        /// <param name="roleID">Account-contact role to be set</param>
        public void SetAccountsIntoContactInternal(int contactID, string where, int roleID)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@contactID", contactID);
            parameters.Add("@where", where);
            parameters.Add("@roleID", roleID);

            ConnectionHelper.ExecuteQuery("OM.accountcontact.SetAccountsIntoContact", parameters);

            CacheHelper.TouchKey(String.Format("om.contact|byid|{0}|children|om.accountcontact", contactID));
        }

        #endregion
    }
}