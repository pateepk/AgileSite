using System;

using CMS.Base;
using CMS.ContactManagement.Internal;
using CMS.Core;
using CMS.DataEngine;
using CMS.Membership;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Provides methods for creating relationship between contact and user.
    /// </summary>
    internal class DefaultContactRelationAssigner : IContactRelationAssigner
    {
        private readonly IContactProcessingChecker contactProcessingChecker;


        /// <summary>
        /// Creates an instance of the <see cref="DefaultContactRelationAssigner"/> class.
        /// </summary>
        /// <param name="contactProcessingChecker">Checker to verify that contact processing is allowed.</param>
        public DefaultContactRelationAssigner(IContactProcessingChecker contactProcessingChecker)
        {
            this.contactProcessingChecker = contactProcessingChecker;
        }


        /// <summary>
        /// Creates relationship between given <paramref name="user"/> and <paramref name="contact"/>.
        /// </summary>
        /// <param name="user">Current user to be assigned to the <paramref name="contact"/></param>
        /// <param name="contact">Contact the <paramref name="user"/> should be assigned to</param>
        /// <param name="checker">Checks whether the object should be assigned.</param>
        /// <exception cref="ArgumentNullException"><paramref name="user"/> or <paramref name="contact"/> is null</exception>
        public void Assign(IUserInfo user, ContactInfo contact, IContactDataPropagationChecker checker = null)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (contact == null)
            {
                throw new ArgumentNullException(nameof(contact));
            }

            if (checker != null && !checker.IsAllowed())
            {
                return;
            }

            var userInfo = UserInfoProvider.GetUserInfo(user.UserID);
            Assign(MemberTypeEnum.CmsUser, userInfo, contact);
        }


        /// <summary>
        /// Creates relationship between given <paramref name="contact"/> and given <paramref name="relatedObject"/> of type specified in <paramref name="memberType"/>.
        /// </summary>
        /// <param name="memberType">Type of <paramref name="relatedObject"/></param>
        /// <param name="relatedObject">Related object to be assigned to the given <paramref name="contact"/></param>
        /// <param name="contact">Contact the <paramref name="relatedObject"/> should be assigned to</param>
        /// <param name="checker">Checks whether the object should be assigned.</param>
        /// <exception cref="ArgumentNullException"><paramref name="relatedObject"/> is <c>null</c> -or- <paramref name="contact"/> is <c>null</c></exception>
        public void Assign(MemberTypeEnum memberType, BaseInfo relatedObject, ContactInfo contact, IContactDataPropagationChecker checker = null)
        {
            if (relatedObject == null)
            {
                throw new ArgumentNullException(nameof(relatedObject));
            }

            if (contact == null)
            {
                throw new ArgumentNullException(nameof(contact));
            }

            if (memberType == MemberTypeEnum.EcommerceCustomer && !contactProcessingChecker.CanProcessContactInCurrentContext())
            {
                return;
            }

            if (checker != null && !checker.IsAllowed())
            {
                return;
            }

            // Check that relation doesn't exist
            int relatedObjectId = relatedObject.Generalized.ObjectID;
            if (ContactMembershipInfoProvider.GetMembershipInfo(contact.ContactID, relatedObjectId, memberType) != null)
            {
                return;
            }

            // Create new relation
            var contactMembership = new ContactMembershipInfo
            {
                ContactID = contact.ContactID,
                RelatedID = relatedObjectId,
                MemberType = memberType
            };
            ContactMembershipInfoProvider.SetMembershipInfo(contactMembership);

            // Update contact from related object
            ContactInfoProvider.UpdateContactFromExternalData(relatedObject, false, contact);
        }


        /// <summary>
        /// Creates relationship between for given <paramref name="contactId"/> and related object with given <paramref name="relatedId"/> of type specified in <paramref name="memberType"/>.
        /// </summary>
        /// <param name="relatedId">ID of related object</param>
        /// <param name="memberType">Type of object</param>
        /// <param name="contactId">Contact ID</param>
        /// <param name="checker">Checks whether the object should be assigned.</param>
        public void Assign(int relatedId, MemberTypeEnum memberType, int contactId, IContactDataPropagationChecker checker = null)
        {
            if (contactId == 0)
            {
                return;
            }

            if (memberType == MemberTypeEnum.EcommerceCustomer && !contactProcessingChecker.CanProcessContactInCurrentContext())
            {
                return;
            }

            if (checker != null && !checker.IsAllowed())
            {
                return;
            }

            // Check that relation doesn't exist
            if (ContactMembershipInfoProvider.GetMembershipInfo(contactId, relatedId, memberType) != null)
            {
                return;
            }

            // Create new relation
            var contactMembership = new ContactMembershipInfo
            {
                ContactID = contactId,
                RelatedID = relatedId,
                MemberType = memberType
            };
            ContactMembershipInfoProvider.SetMembershipInfo(contactMembership);

            // Update contact from related object
            var info = GetRelatedObject(relatedId, memberType);
            ContactInfoProvider.UpdateContactFromExternalData(info, false, contactId);
        }


        private static BaseInfo GetRelatedObject(int relatedId, MemberTypeEnum memberType)
        {
            BaseInfo info = null;
            switch (memberType)
            {
                case MemberTypeEnum.CmsUser:
                    info = UserInfoProvider.GetUserInfo(relatedId);
                    break;

                case MemberTypeEnum.EcommerceCustomer:
                    if (ModuleEntryManager.IsModuleLoaded(ModuleName.ECOMMERCE))
                    {
                        info = ProviderHelper.GetInfoById(PredefinedObjectType.CUSTOMER, relatedId);
                    }
                    break;
            }

            return info;
        }
    }
}
