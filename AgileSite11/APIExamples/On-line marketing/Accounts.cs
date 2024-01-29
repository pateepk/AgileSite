using CMS.Base;
using CMS.ContactManagement;

namespace APIExamples
{
    /// <summary>
    /// Holds account API examples.
    /// </summary>
    /// <pageTitle>Accounts</pageTitle>
    internal class AccountsMain
    {
        /// <summary>
        /// Holds account API examples.
        /// </summary>
        /// <groupHeading>Accounts</groupHeading>
        private class Accounts
        {
            /// <heading>Creating an account</heading>
            private void CreateAccount()
            {
                // Creates a new account object
                AccountInfo newAccount = new AccountInfo()
                {
                    AccountName = "New Account"
                };

                // Saves the new account to the database
                AccountInfoProvider.SetAccountInfo(newAccount);
            }


            /// <heading>Updating accounts</heading>
            private void GetAndUpdateAccount()
            {
                // Gets all accounts whose name starts with 'New'
                var accounts = AccountInfoProvider.GetAccounts().WhereStartsWith("AccountName", "New");

                // Loops through individual accounts
                foreach (AccountInfo account in accounts)
                {
                    // Updates the account's properties
                    account.AccountName = account.AccountName.ToLowerCSafe();

                    // Saves the modified account into the database
                    AccountInfoProvider.SetAccountInfo(account);
                }
            }


            /// <heading>Adding contacts to an account</heading>
            private void AddContactToAccount()
            {
                // Gets the 'New Account' account
                AccountInfo account = AccountInfoProvider.GetAccounts()
                                                            .WhereEquals("AccountName", "New Account")
                                                            .FirstObject;
                
                if (account != null)
                {
                    // Gets all contacts whose last name is 'Smith'
                    var contacts = ContactInfoProvider.GetContacts().WhereEquals("ContactLastName", "Smith");

                    // Loops through individual contacts
                    foreach (ContactInfo contact in contacts)
                    {
                        // Creates an object representing the relationship between the account and the contact
                        AccountContactInfo accountContact = new AccountContactInfo()
                        {
                            AccountID = account.AccountID,
                            ContactID = contact.ContactID
                        };

                        // Saves the account-contact relationship to the database
                        AccountContactInfoProvider.SetAccountContactInfo(accountContact);
                    }
                }
            }


            /// <heading>Removing contacts from an account</heading>
            private void RemoveContactFromAccount()
            {
                // Gets the 'New Account' account
                AccountInfo account = AccountInfoProvider.GetAccounts()
                                                            .WhereEquals("AccountName", "New Account")
                                                            .FirstObject;

                if (account != null)
                {
                    // Gets the first contact whose last name is 'Smith'
                    ContactInfo contact = ContactInfoProvider.GetContacts()
                                                        .WhereEquals("ContactLastName", "Smith")
                                                        .FirstObject;

                    // Gets the relationship between the contact and the account
                    AccountContactInfo accountContact = AccountContactInfoProvider.GetAccountContactInfo(account.AccountID, contact.ContactID);

                    if (accountContact != null)
                    {
                        // Removes the contact from the account
                        AccountContactInfoProvider.DeleteAccountContactInfo(accountContact);
                    }
                }
            }


            /// <heading>Deleting an account</heading>
            private void DeleteAccount()
            {
                // Gets the 'New Account'
                AccountInfo deleteAccount = AccountInfoProvider.GetAccounts()
                                                            .WhereEquals("AccountName", "New Account")
                                                            .FirstObject;

                if (deleteAccount != null)
                {
                    // Deletes the account
                    AccountInfoProvider.DeleteAccountInfo(deleteAccount);                    
                }
            }
        }


        /// <summary>
        /// Holds account status API examples.
        /// </summary>
        /// <groupHeading>Account statuses</groupHeading>
        private class AccountStatuses
        {
            /// <heading>Creating an account status</heading>
            private void CreateAccountStatus()
            {
                // Creates a new account status object
                AccountStatusInfo newStatus = new AccountStatusInfo()
                {
                    AccountStatusDisplayName = "New account status",
                    AccountStatusName = "NewAccountStatus",
                };

                // Saves the account status to the database
                AccountStatusInfoProvider.SetAccountStatusInfo(newStatus);
            }


            /// <heading>Updating an account status</heading>
            private void GetAndUpdateAccountStatus()
            {
                // Gets the account status
                AccountStatusInfo updateStatus = AccountStatusInfoProvider.GetAccountStatusInfo("NewAccountStatus");

                if (updateStatus != null)
                {
                    // Updates the account status properties
                    updateStatus.AccountStatusDisplayName = updateStatus.AccountStatusDisplayName.ToLowerCSafe();

                    // Saves the modified account status to the database
                    AccountStatusInfoProvider.SetAccountStatusInfo(updateStatus);
                }
            }


            /// <heading>Updating multiple account statuses</heading>
            private void GetAndBulkUpdateAccountStatuses()
            {
                // Gets all account statuses whose code name starts with 'New'
                var statuses = AccountStatusInfoProvider.GetAccountStatuses()
                                                            .WhereStartsWith("AccountStatusName","New");
                
                // Loops through individual account statuses
                foreach (AccountStatusInfo accountStatus in statuses)
                {
                    // Updates the account status properties
                    accountStatus.AccountStatusDisplayName = accountStatus.AccountStatusDisplayName.ToUpper();

                    // Saves the modified account status to the database
                    AccountStatusInfoProvider.SetAccountStatusInfo(accountStatus);
                }
            }


            /// <heading>Assigning a status to an account</heading>
            private void AddAccountStatusToAccount()
            {
                // Gets the 'New Account' account
                AccountInfo account = AccountInfoProvider.GetAccounts()
                                                            .WhereEquals("AccountName", "New Account")
                                                            .FirstObject;

                // Gets the account status
                AccountStatusInfo accountStatus = AccountStatusInfoProvider.GetAccountStatusInfo("NewAccountStatus");

                if ((account != null) && (accountStatus != null))
                {
                    // Checks that the account doesn't already have the given status
                    if (account.AccountStatusID != accountStatus.AccountStatusID)
                    {
                        // Assigns the status to the account
                        account.AccountStatusID = accountStatus.AccountStatusID;

                        // Saves the updated account to the database
                        AccountInfoProvider.SetAccountInfo(account);
                    }
                }
            }


            /// <heading>Removing statuses from accounts</heading>
            private void RemoveAccountStatusFromAccount()
            {
                // Gets the account status
                AccountStatusInfo accountStatus = AccountStatusInfoProvider.GetAccountStatusInfo("NewAccountStatus");

                if (accountStatus != null)
                {
                    // Gets all accounts that have the specified status
                    var accounts = AccountInfoProvider.GetAccounts().WhereEquals("AccountStatusID", accountStatus.AccountStatusID);

                    // Loops through the accounts
                    foreach (AccountInfo account in accounts)
                    {
                        // Sets the status to 'None' for each account
                        account.AccountStatusID = 0;

                        // Saves the updated account to the database
                        AccountInfoProvider.SetAccountInfo(account);
                    }
                }
            }


            /// <heading>Deleting an account status</heading>
            private void DeleteAccountStatus()
            {
                // Gets the account status
                AccountStatusInfo deleteStatus = AccountStatusInfoProvider.GetAccountStatusInfo("NewAccountStatus");

                if (deleteStatus != null)
                {
                    // Deletes the account status
                    AccountStatusInfoProvider.DeleteAccountStatusInfo(deleteStatus);
                }
            }
        }


        /// <summary>
        /// Holds contact role API examples.
        /// </summary>
        /// <groupHeading>Contact roles for accounts</groupHeading>
        private class ContactRoles
        {
            /// <heading>Creating a contact role</heading>
            private void CreateContactRole()
            {
                // Creates a new contact role object
                ContactRoleInfo newRole = new ContactRoleInfo()
                {
                    ContactRoleDisplayName = "New role",
                    ContactRoleName = "NewContactRole",
                };

                // Saves the contact role to the database
                ContactRoleInfoProvider.SetContactRoleInfo(newRole);
            }


            /// <heading>Updating a contact role</heading>
            private void GetAndUpdateContactRole()
            {
                // Gets the contact role
                ContactRoleInfo updateRole = ContactRoleInfoProvider.GetContactRoleInfo("NewContactRole");

                if (updateRole != null)
                {
                    // Updates the contact role properties
                    updateRole.ContactRoleDisplayName = updateRole.ContactRoleDisplayName.ToLowerCSafe();

                    // Saves the modified contact role to the database
                    ContactRoleInfoProvider.SetContactRoleInfo(updateRole);
                }
            }


            /// <heading>Updating multiple contact roles</heading>
            private void GetAndBulkUpdateContactRoles()
            {
                // Gets all contact roles whose code name starts with 'New'
                var roles = ContactRoleInfoProvider.GetContactRoles().WhereStartsWith("ContactRoleName", "New");

                // Loops through individual contact roles
                foreach (ContactRoleInfo role in roles)
                {
                    // Updates the contact role properties
                    role.ContactRoleDisplayName = role.ContactRoleDisplayName.ToUpper();

                    // Saves the updated contact role to the database
                    ContactRoleInfoProvider.SetContactRoleInfo(role);
                }
            }


            /// <heading>Assigning a role to contacts in an account</heading>
            private void AssignRoleToContacts()
            {
                // Gets the 'New Account' account
                AccountInfo account = AccountInfoProvider.GetAccounts()
                                                            .WhereEquals("AccountName", "New Account")
                                                            .FirstObject;

                // Gets the contact role
                ContactRoleInfo contactRole = ContactRoleInfoProvider.GetContactRoleInfo("NewContactRole");

                if ((account != null) && (contactRole != null))
                {
                    // Gets the relationships between the account and all of its contacts
                    var accountContactRelationships = AccountContactInfoProvider.GetRelationships().WhereEquals("AccountID", account.AccountID);

                    // Loops through the contact-account relationships
                    foreach (AccountContactInfo accountContact in accountContactRelationships)
                    {
                        // Assigns the role to the contact for the specified account
                        accountContact.ContactRoleID = contactRole.ContactRoleID;

                        // Saves the account-contact relationship to the database
                        AccountContactInfoProvider.SetAccountContactInfo(accountContact);
                    }
                }
            }


            /// <heading>Deleting a contact role</heading>
            private void DeleteContactRole()
            {
                // Gets the contact role
                ContactRoleInfo deleteRole = ContactRoleInfoProvider.GetContactRoleInfo("NewContactRole");

                if (deleteRole != null)
                {
                    // Deletes the contact role
                    ContactRoleInfoProvider.DeleteContactRoleInfo(deleteRole);
                }
            }
        }
    }
}
