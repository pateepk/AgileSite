using System;

using CMS.Core;
using CMS.Base;
using CMS.ContactManagement;
using CMS.Membership;

namespace APIExamples
{
    /// <summary>
    /// Holds contact-related API examples.
    /// </summary>
    /// <pageTitle>Contacts</pageTitle>
    internal class ContactsMain
    {
        /// <summary>
        /// Holds contact API examples.
        /// </summary>
        /// <groupHeading>Contacts</groupHeading>
        private class Contacts
        {
            /// <heading>Getting the current contact</heading>
            private void GetCurrentContact()
            {
                // Gets the contact related to the currently processed request
                ContactInfo currentContact = ContactManagementContext.CurrentContact;
            }


            /// <heading>Creating a contact</heading>
            private void CreateContact()
            {
                // Creates a new contact object
                ContactInfo newContact = new ContactInfo()
                {
                    ContactLastName = "Smith",
                    ContactFirstName = "John"                 
                };

                // Saves the contact to the database
                ContactInfoProvider.SetContactInfo(newContact);
            }


            /// <heading>Updating a contact</heading>
            private void GetAndUpdateContact()
            {
                // Gets the first contact whose last name is 'Smith'
                ContactInfo updateContact = ContactInfoProvider.GetContacts()
                                                    .WhereEquals("ContactLastName", "Smith")
                                                    .FirstObject;

                if (updateContact != null)
                {
                    // Updates the contact's properties
                    updateContact.ContactCompanyName = "Company Inc.";

                    // Saves the updated contact to the database
                    ContactInfoProvider.SetContactInfo(updateContact);
                }                
            }


            /// <heading>Updating multiple contacts</heading>
            private void GetAndBulkUpdateContacts()
            {
                // Gets all contacts whose last name is 'Smith'
                var contacts = ContactInfoProvider.GetContacts().WhereEquals("ContactLastName", "Smith");

                // Loops through individual contacts
                foreach (ContactInfo contact in contacts)
                {
                    // Updates the properties of the contact
                    contact.ContactCompanyName = "Company Inc.";

                    // Saves the updated contact to the database
                    ContactInfoProvider.SetContactInfo(contact);
                }
            }


            /// <heading>Linking contacts with user accounts</heading>
            private void AddMembership()
            {
                // Gets the first contact whose last name is 'Smith'
                ContactInfo contact = ContactInfoProvider.GetContacts()
                                                    .WhereEquals("ContactLastName", "Smith")
                                                    .FirstObject;

                if (contact != null)
                {
                    // Links the contact with the current user
                    Service.Resolve<IContactRelationAssigner>().Assign(
                        MembershipContext.AuthenticatedUser.UserID,
                        MemberTypeEnum.CmsUser,
                        contact.ContactID);
                }
            }


            /// <heading>Removing user accounts from contacts</heading>
            private void RemoveMembership()
            {
                // Gets the first contact whose last name is 'Smith'
                ContactInfo contact = ContactInfoProvider.GetContacts()
                                                    .WhereEquals("ContactLastName", "Smith")
                                                    .FirstObject;

                if (contact != null)
                {
                    // Gets the relationship between the contact and the current user
                    ContactMembershipInfo contactMembership = ContactMembershipInfoProvider.GetMembershipInfo(
                        contact.ContactID,
                        MembershipContext.AuthenticatedUser.UserID,
                        MemberTypeEnum.CmsUser);

                    // Deletes the contact-user relationship
                    ContactMembershipInfoProvider.DeleteMembershipInfo(contactMembership.MembershipID);
                }
            }
            

            /// <heading>Deleting a contact</heading>
            private void DeleteContact()
            {
                // Gets the first contact whose last name is 'Smith'
                ContactInfo deleteContact = ContactInfoProvider.GetContacts()
                                                    .WhereEquals("ContactLastName", "Smith")
                                                    .FirstObject;

                if (deleteContact != null)
                {
                    // Deletes the contact
                    ContactInfoProvider.DeleteContactInfo(deleteContact);
                }
            }
        }


        /// <summary>
        /// Holds contact status API examples.
        /// </summary>
        /// <groupHeading>Contact statuses</groupHeading>
        private class ContactStatuses
        {
            /// <heading>Creating a contact status</heading>
            private void CreateContactStatus()
            {
                // Creates a new contact status object
                ContactStatusInfo newStatus = new ContactStatusInfo()
                {
                    ContactStatusDisplayName = "New contact status",
                    ContactStatusName = "NewContactStatus"
                };

                // Saves the contact status to the database
                ContactStatusInfoProvider.SetContactStatusInfo(newStatus);
            }


            /// <heading>Updating a contact status</heading>
            private void GetAndUpdateContactStatus()
            {
                // Gets the contact status
                ContactStatusInfo updateStatus = ContactStatusInfoProvider.GetContactStatusInfo("NewContactStatus");

                if (updateStatus != null)
                {
                    // Updates the contact status properties
                    updateStatus.ContactStatusDisplayName = updateStatus.ContactStatusDisplayName.ToLowerCSafe();

                    // Saves the modified contact status to the database
                    ContactStatusInfoProvider.SetContactStatusInfo(updateStatus);
                }
            }


            /// <heading>Updating multiple contact statuses</heading>
            private void GetAndBulkUpdateContactStatuses()
            {
                // Gets all contact statuses whose code name starts with 'New'
                var statuses = ContactStatusInfoProvider.GetContactStatuses()
                                                                .WhereStartsWith("ContactStatusName","New");

                // Loops through individual contact statuses
                foreach (ContactStatusInfo contactStatus in statuses)
                {
                    // Updates the contact status properties
                    contactStatus.ContactStatusDisplayName = contactStatus.ContactStatusDisplayName.ToUpper();

                    // Saves the modified contact status to the database
                    ContactStatusInfoProvider.SetContactStatusInfo(contactStatus);
                }
            }


            /// <heading>Assigning a status to a contact</heading>
            private void AddContactStatusToContact()
            {
                // Gets the first contact whose last name is 'Smith'
                ContactInfo contact = ContactInfoProvider.GetContacts()
                                                    .WhereEquals("ContactLastName", "Smith")
                                                    .FirstObject;

                // Gets the contact status
                ContactStatusInfo contactStatus = ContactStatusInfoProvider.GetContactStatusInfo("NewContactStatus");

                if ((contact != null) && (contactStatus != null))
                {
                    // Checks that the contact doesn't already have the given status
                    if (contact.ContactStatusID != contactStatus.ContactStatusID)
                    {
                        // Assigns the status to the contact
                        contact.ContactStatusID = contactStatus.ContactStatusID;

                        // Saves the updated contact to the database
                        ContactInfoProvider.SetContactInfo(contact);
                    }
                }
            }


            /// <heading>Removing statuses from contacts</heading>
            private void RemoveContactStatusFromContact()
            {
                // Gets the contact status
                ContactStatusInfo contactStatus = ContactStatusInfoProvider.GetContactStatusInfo("NewContactStatus");

                if (contactStatus != null)
                {
                    // Gets all contacts that have the specified status
                    var contacts = ContactInfoProvider.GetContacts()
                                                        .WhereEquals("ContactStatusID", contactStatus.ContactStatusID);

                    // Loops through the contacts
                    foreach (ContactInfo contact in contacts)
                    {
                        // Sets the status to 'None' for each contact
                        contact.ContactStatusID = 0;

                        // Saves the updated contact to the database
                        ContactInfoProvider.SetContactInfo(contact);
                    }
                }
            }


            /// <heading>Deleting a contact status</heading>
            private void DeleteContactStatus()
            {
                // Gets the contact status
                ContactStatusInfo deleteStatus = ContactStatusInfoProvider.GetContactStatusInfo("NewContactStatus");

                if (deleteStatus != null)
                {
                    // Deletes the contact status
                    ContactStatusInfoProvider.DeleteContactStatusInfo(deleteStatus);
                }
            }
        }
    }
}
