using CMS.Base;
using CMS.ContactManagement;

namespace APIExamples
{
    /// <summary>
    /// Holds contact group API examples.
    /// </summary>
    /// <pageTitle>Contact groups</pageTitle>
    internal class ContactGroups
    {
        /// <heading>Creating a contact group</heading>
        private void CreateContactGroup()
        {
            // Creates a new contact group object
            ContactGroupInfo newGroup = new ContactGroupInfo()
            {
                ContactGroupDisplayName = "New group",
                ContactGroupName = "NewGroup",
                ContactGroupEnabled = true,
                ContactGroupDynamicCondition = "{%Contact.ContactLastName.Contains(\"Smith\")%}"
            };

            // Saves the contact group to the database
            ContactGroupInfoProvider.SetContactGroupInfo(newGroup);
        }


        /// <heading>Updating a contact group</heading>
        private void GetAndUpdateContactGroup()
        {
            // Gets the contact group
            ContactGroupInfo updateGroup = ContactGroupInfoProvider.GetContactGroupInfo("NewGroup");

            if (updateGroup != null)
            {
                // Updates the contact group's properties
                updateGroup.ContactGroupDisplayName = updateGroup.ContactGroupDisplayName.ToLowerCSafe();

                // Saves the modified contact group to the database
                ContactGroupInfoProvider.SetContactGroupInfo(updateGroup);
            }
        }


        /// <heading>Updating multiple contact groups</heading>
        private void GetAndBulkUpdateContactGroups()
        {
            // Gets all contact groups whose name starts with 'New'
            var groups = ContactGroupInfoProvider.GetContactGroups().WhereStartsWith("ContactGroupName", "New");

            // Loops through individual contact groups
            foreach (ContactGroupInfo group in groups)
            {
                // Updates the contact group properties
                group.ContactGroupDisplayName = group.ContactGroupDisplayName.ToUpper();

                // Saves the modified contact group to the database
                ContactGroupInfoProvider.SetContactGroupInfo(group);
            }
        }

        
        /// <heading>Adding contacts to a contact group</heading>
        private void AddContactToGroup()
        {
            // Gets the contact group
            ContactGroupInfo group = ContactGroupInfoProvider.GetContactGroupInfo("NewGroup");

            if (group != null)
            {
                // Gets all contacts whose last name is 'Smith'
                var contacts = ContactInfoProvider.GetContacts().WhereEquals("ContactLastName", "Smith");

                // Loops through individual contacts
                foreach (ContactInfo contact in contacts)
                {
                    // Creates an object representing the relationship between the contact and the group
                    ContactGroupMemberInfo newContactGroupMember = new ContactGroupMemberInfo()
                    {
                        ContactGroupMemberContactGroupID = group.ContactGroupID,
                        ContactGroupMemberType = ContactGroupMemberTypeEnum.Contact,
                        ContactGroupMemberRelatedID = contact.ContactID,
                        ContactGroupMemberFromManual = true
                    };

                    // Saves the contact-group relationship to the database
                    ContactGroupMemberInfoProvider.SetContactGroupMemberInfo(newContactGroupMember);
                }
            }
        }


        /// <heading>Removing contacts from a contact group</heading>
        private void RemoveContactFromGroup()
        {
            // Gets the contact group
            ContactGroupInfo group = ContactGroupInfoProvider.GetContactGroupInfo("NewGroup");

            if (group != null)
            {
                // Gets the first contact whose last name is 'Smith'
                ContactInfo contact = ContactInfoProvider.GetContacts()
                                                    .WhereEquals("ContactLastName", "Smith")
                                                    .FirstObject;

                // Gets the relationship between the contact and the contact group
                ContactGroupMemberInfo contactGroupMember = 
                    ContactGroupMemberInfoProvider.GetContactGroupMemberInfoByData(group.ContactGroupID, contact.ContactID, ContactGroupMemberTypeEnum.Contact);

                if (contactGroupMember != null)
                {
                    // Removes the contact from the contact group
                    ContactGroupMemberInfoProvider.DeleteContactGroupMemberInfo(contactGroupMember);
                }
            }
        }


        /// <heading>Adding accounts to a contact group</heading>
        private void AddAccountToGroup()
        {
            // Gets the contact group
            ContactGroupInfo group = ContactGroupInfoProvider.GetContactGroupInfo("NewGroup");

            if (group != null)
            {
                // Gets the first the account whose name starts with 'New"
                AccountInfo account = AccountInfoProvider.GetAccounts()
                                                    .WhereStartsWith("AccountName", "New")
                                                    .FirstObject;

                // Creates an object representing the relationship between the account and the contact group
                ContactGroupMemberInfo newContactGroupAccountMember = new ContactGroupMemberInfo()
                {
                    ContactGroupMemberContactGroupID = group.ContactGroupID,
                    ContactGroupMemberType = ContactGroupMemberTypeEnum.Account,
                    ContactGroupMemberRelatedID = account.AccountID
                };

                // Saves the account-contact group relationship to the database
                ContactGroupMemberInfoProvider.SetContactGroupMemberInfo(newContactGroupAccountMember);
            }
        }


        /// <heading>Removing accounts from a contact group</heading>
        private void RemoveAccountFromGroup()
        {
            // Gets the contact group
            ContactGroupInfo group = ContactGroupInfoProvider.GetContactGroupInfo("NewGroup");

            if (group != null)
            {
                // Gets the first the account whose name starts with 'New"
                AccountInfo account = AccountInfoProvider.GetAccounts()
                                                    .WhereStartsWith("AccountName", "New")
                                                    .FirstObject;

                // Gets the relationship between the account and the contact group
                ContactGroupMemberInfo contactGroupAccountMember =
                    ContactGroupMemberInfoProvider.GetContactGroupMemberInfoByData(group.ContactGroupID, account.AccountID, ContactGroupMemberTypeEnum.Account);

                if (contactGroupAccountMember != null)
                {
                    // Removes the account from the contact group
                    ContactGroupMemberInfoProvider.DeleteContactGroupMemberInfo(contactGroupAccountMember);
                }
            }
        }



        /// <heading>Deleting a contact group</heading>
        private void DeleteContactGroup()
        {
            // Gets the contact group
            ContactGroupInfo deleteGroup = ContactGroupInfoProvider.GetContactGroupInfo("NewGroup");

            if (deleteGroup != null)
            {
                // Deletes the contact group
                ContactGroupInfoProvider.DeleteContactGroupInfo(deleteGroup);
            }
        }
    }
}
