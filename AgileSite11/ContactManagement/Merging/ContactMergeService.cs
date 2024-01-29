using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Activities;
using CMS.Automation;
using CMS.Core;
using CMS.DataEngine;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Provides method for merging of a <see cref="ContactInfo"/> into another one.
    /// </summary>
    internal class ContactMergeService : IContactMergeService
    {
        private readonly IContactMergeOnlineUsersUpdater mContactMergeOnlineUsersUpdater;
        internal ILicenseService mLicenseService;


        public ContactMergeService(IContactMergeOnlineUsersUpdater contactMergeOnlineUsersUpdater)
        {
            mContactMergeOnlineUsersUpdater = contactMergeOnlineUsersUpdater;
            mLicenseService = ObjectFactory<ILicenseService>.StaticSingleton();
        }


        /// <summary>
        /// Merges given <paramref name="source"/> contact to the <paramref name="target"/>. Moves subscriptions and copies contact's data. 
        /// In the case the EMS license is available, moves activities, memberships and relations.
        /// </summary>
        /// <remarks>
        /// Does not set the result of merge to the current context or store the GUID in response cookie. If the result should be somehow
        /// persistent during requests, caller of the method has to call <see cref="ICurrentContactProvider.SetCurrentContact"/> with the merged contact.
        /// </remarks>
        /// <param name="source">Source contact to be merged to the <paramref name="target"/></param>
        /// <param name="target">Target contact the <paramref name="source"/> is to be merged to</param>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c> -or <paramref name="target"/> is <c>null</c></exception>
        public void MergeContacts(ContactInfo source, ContactInfo target)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (mLicenseService.IsFeatureAvailable(FeatureEnum.FullContactManagement, ""))
            {
                MoveContactActivities(source, target);
                MoveContactMemberships(source, target);
                MoveAccountRelationships(source, target);
                MoveAccountPrimaryAndSecondaryContacts(source, target);
                MoveContactGroupMemberships(source, target);
                MoveAutomationHistory(source, target);
                AddVisitorToContactMapping(source);
                MoveVisitorToContactData(source, target);
            }

            CopyContactData(source, target);
            mContactMergeOnlineUsersUpdater.Update(source, target);
            
            ContactManagementEvents.ContactMerge.StartEvent(source, target);
            ContactInfoProvider.DeleteContactInfo(source);
            ContactInfoProvider.SetContactInfo(target, true);
        }


        /// <summary>
        /// Tries to merge given <paramref name="contact"/> by <see cref="ContactInfo.ContactEmail"/>. Moves subscriptions and copies contact's data. 
        /// In the case the EMS license is available, moves activities, memberships, relations and copies contact's data. 
        /// Given <paramref name="contact"/> will be used as merge target, i.e. this particular <see cref="ContactInfo"/> will remain, while others with the same <see cref="ContactInfo.ContactEmail"/>
        /// will be merged to the <paramref name="contact"/>.
        /// </summary>
        /// <remarks>
        /// Does not set the result of merge to the current context or store the GUID in response cookie. If the result should be somehow
        /// persistent during requests, caller of the method has to call <see cref="ICurrentContactProvider.SetCurrentContact"/> with the merged contact.
        /// </remarks>
        /// <param name="contact">Contact to be merged with existing one by their emails</param>
        /// <exception cref="ArgumentNullException"><paramref name="contact"/> is <c>null</c></exception>
        /// <exception cref="ArgumentException">Contact's email cannot be empty</exception>
        public void MergeContactByEmail(ContactInfo contact)
        {
            if (contact == null)
            {
                throw new ArgumentNullException(nameof(contact));
            }
            if (string.IsNullOrEmpty(contact.ContactEmail))
            {
                throw new ArgumentException("Contact's email cannot be empty", nameof(contact));
            }

            var sourceContacts = ContactInfoProvider.GetContacts()
                                                    .WhereEquals("ContactEmail", contact.ContactEmail)
                                                    .WhereNotEquals("ContactID", contact.ContactID)
                                                    .ToList();
            
            foreach (var sourceContact in sourceContacts)
            {
                MergeContacts(sourceContact, contact);
            }
        }


        private void AddVisitorToContactMapping(ContactInfo contact)
        {
            if (!MappingExists(contact))
            {
                VisitorToContactInfoProvider.CreateVisitorToContactInfo(contact);
            }
        }


        private bool MappingExists(ContactInfo contact)
        {
            return VisitorToContactInfoProvider.GetVisitorToContacts().WhereEquals("VisitorToContactVisitorGUID", contact.ContactGUID).Count > 0;
        }


        private void MoveVisitorToContactData(ContactInfo source, ContactInfo target)
        {
            VisitorToContactInfoProvider.BulkMoveVisitors(source.ContactID, target.ContactID);
        }


        private void CopyContactData(ContactInfo source, ContactInfo target)
        {
            var contactColumnNames = GetContactColumnsToBeCopied(target);
            foreach (var contactColumn in contactColumnNames)
            {
                if (contactColumn == "ContactCreated")
                {
                    UpdateContactCreatedColumn(source, target);
                }
                else
                {
                    UpdateTargetContactColumn(source, target, contactColumn);
                }
            }
        }


        private static List<string> GetContactColumnsToBeCopied(ContactInfo target)
        {
            var contactColumnNames = target.ColumnNames.ToList();
            contactColumnNames.Remove(ContactInfo.TYPEINFO.GUIDColumn);
            contactColumnNames.Remove(ContactInfo.TYPEINFO.IDColumn);
            return contactColumnNames;
        }


        private void UpdateContactCreatedColumn(ContactInfo contact1, ContactInfo contact2)
        {
            if (contact1.ContactCreated > contact2.ContactCreated)
            {
                UpdateTargetContactColumn(contact2, contact1, "ContactCreated");
            }
            else
            {
                UpdateTargetContactColumn(contact1, contact2, "ContactCreated");
            }
        }


        private static void UpdateTargetContactColumn(ContactInfo source, ContactInfo target, string contactColumn)
        {
            var sourceValue = source.GetValue(contactColumn);
            if (sourceValue == null || IsAnonymousLastName(contactColumn, sourceValue))
            {
                return;
            }

            target.SetValue(contactColumn, sourceValue);
        }


        private static bool IsAnonymousLastName(string contactColumn, object sourceValue)
        {
            return contactColumn == "ContactLastName" && ((string)sourceValue).StartsWith(ContactHelper.ANONYMOUS, StringComparison.OrdinalIgnoreCase);
        }


        private void MoveContactActivities(ContactInfo source, ContactInfo target)
        {
            ActivityInfoProvider.BulkMoveActivitiesToAnotherContact(source.ContactID, target.ContactID);
        }

        
        private void MoveContactMemberships(ContactInfo source, ContactInfo target)
        {
            var memberships = ContactMembershipInfoProvider.GetRelationships()
                                                           .WhereIn("ContactID", new[]
                                                           {
                                                               source.ContactID,
                                                               target.ContactID
                                                           })
                                                           .ToList();

            var sourceMemberships = memberships.Where(relationship => relationship.ContactID == source.ContactID).ToList();
            var targetMemberships = memberships.Where(relationship => relationship.ContactID == target.ContactID).ToList();

            foreach (var sourceMembership in GetMembershipsToBeMerged(sourceMemberships, targetMemberships))
            {
                sourceMembership.ContactID = target.ContactID;
                ContactMembershipInfoProvider.SetMembershipInfo(sourceMembership);
            }
        }


        private IEnumerable<ContactMembershipInfo> GetMembershipsToBeMerged(List<ContactMembershipInfo> sourceMemberships, List<ContactMembershipInfo> targetMemberships)
        {
            return sourceMemberships.Where(relationship => MembershipNotInTargetMemberships(relationship, targetMemberships));
        }


        private bool MembershipNotInTargetMemberships(ContactMembershipInfo sourceMembership, IEnumerable<ContactMembershipInfo> targetMemberships)
        {
            return !targetMemberships.Any(targetMembership =>
                sourceMembership.RelatedID == targetMembership.RelatedID &&
                sourceMembership.MemberType == targetMembership.MemberType
            );
        }

        
        private void MoveAccountRelationships(ContactInfo source, ContactInfo target)
        {
            var relationshipsToBeChanged = AccountContactInfoProvider.GetRelationships()
                                                                     .WhereIn("ContactID", new[]
                                                                     {
                                                                         source.ContactID,
                                                                         target.ContactID
                                                                     })
                                                                     .ToList();

            var sourceRelationships = relationshipsToBeChanged.Where(relationship => relationship.ContactID == source.ContactID).ToList();
            var targetRelationships = relationshipsToBeChanged.Where(relationship => relationship.ContactID == target.ContactID).ToList();

            foreach (var targetRelationship in targetRelationships)
            {
                var sourceRelationship = sourceRelationships.SingleOrDefault(relationship => relationship.AccountID == targetRelationship.AccountID);
                if (sourceRelationship != null && SourceRelationshipDiffersFromTarget(sourceRelationship, targetRelationship))
                {
                    targetRelationship.ContactRoleID = sourceRelationship.ContactRoleID;
                    AccountContactInfoProvider.SetAccountContactInfo(targetRelationship);
                }
            }

            foreach (var sourceRelationship in sourceRelationships.Where(relationship => targetRelationships.All(t => t.AccountID != relationship.AccountID)))
            {
                sourceRelationship.ContactID = target.ContactID;
                AccountContactInfoProvider.SetAccountContactInfo(sourceRelationship);
            }
        }


        private void MoveAccountPrimaryAndSecondaryContacts(ContactInfo source, ContactInfo target)
        {
            // Update primary account contacts
            var primaryContactAccounts = AccountInfoProvider.GetAccounts()
                                                            .WhereEquals("AccountPrimaryContactID", source.ContactID);

            foreach (var primaryContactAccount in primaryContactAccounts)
            {
                primaryContactAccount.AccountPrimaryContactID = target.ContactID;
                primaryContactAccount.Update();
            }

            // Update secondary account contacts
            var secondaryContactAccounts = AccountInfoProvider.GetAccounts()
                                                              .WhereEquals("AccountSecondaryContactID", source.ContactID);

            foreach (var secondaryContactAccount in secondaryContactAccounts)
            {
                secondaryContactAccount.AccountSecondaryContactID = target.ContactID;
                secondaryContactAccount.Update();
            }
        }


        private bool SourceRelationshipDiffersFromTarget(AccountContactInfo sourceRelationship, AccountContactInfo targetRelationship)
        {
            return sourceRelationship.ContactRoleID != 0 &&
                   (sourceRelationship.ContactRoleID != targetRelationship.ContactRoleID);
        }


        private void MoveContactGroupMemberships(ContactInfo source, ContactInfo target)
        {
            var membershipsToBeChanged = ContactGroupMemberInfoProvider.GetRelationships()
                                                                       .WhereIn("ContactGroupMemberRelatedID", new[]
                                                                       {
                                                                           source.ContactID,
                                                                           target.ContactID
                                                                       })
                                                                       .WhereEquals("ContactGroupMemberType", ContactGroupMemberTypeEnum.Contact)
                                                                       .ToList();

            var sourceMemberships = membershipsToBeChanged.Where(relationship => MembershipShouldBeIncluded(source, relationship)).ToList();
            var targetMemberships = membershipsToBeChanged.Where(relationship => relationship.ContactGroupMemberRelatedID == target.ContactID).ToList();

            foreach (var targetMembership in targetMemberships)
            {
                var sourceMembership = sourceMemberships.SingleOrDefault(membership => membership.ContactGroupMemberContactGroupID == targetMembership.ContactGroupMemberContactGroupID);
                if (sourceMembership != null && SourceMembershipDiffersFromTarget(sourceMembership, targetMembership))
                {
                    targetMembership.ContactGroupMemberFromAccount |= sourceMembership.ContactGroupMemberFromAccount;
                    targetMembership.ContactGroupMemberFromManual |= sourceMembership.ContactGroupMemberFromManual;

                    ContactGroupMemberInfoProvider.SetContactGroupMemberInfo(targetMembership);
                }
            }

            foreach (var sourceRelationship in sourceMemberships.Where(relationship => targetMemberships.All(t => t.ContactGroupMemberContactGroupID != relationship.ContactGroupMemberContactGroupID)))
            {
                sourceRelationship.ContactGroupMemberRelatedID = target.ContactID;
                ContactGroupMemberInfoProvider.SetContactGroupMemberInfo(sourceRelationship);
            }
        }


        private bool MembershipShouldBeIncluded(ContactInfo source, ContactGroupMemberInfo contactGroupMember)
        {
            return contactGroupMember.ContactGroupMemberRelatedID == source.ContactID && (
                contactGroupMember.ContactGroupMemberFromManual ||
                contactGroupMember.ContactGroupMemberFromAccount
            );
        }


        private bool SourceMembershipDiffersFromTarget(ContactGroupMemberInfo sourceMembership, ContactGroupMemberInfo targetMembership)
        {
            return sourceMembership.ContactGroupMemberFromAccount != targetMembership.ContactGroupMemberFromAccount ||
                   sourceMembership.ContactGroupMemberFromManual != targetMembership.ContactGroupMemberFromManual;
        }


        private void MoveAutomationHistory(ContactInfo source, ContactInfo target)
        {
            AutomationStateInfoProvider.BulkMoveAutomationStateToTargetContact(source.ContactID, target.ContactID);
        }
    }
}