using CMS;
using CMS.Base;
using CMS.ContactManagement;
using CMS.DataEngine;

[assembly: RegisterImplementation(typeof(IContactRelationAssigner), typeof(DefaultContactRelationAssigner), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.ContactManagement
{
    /// <summary>
    /// Provides method for creating relationship between <see cref="ContactInfo"/> and other objects.
    /// </summary>
    public interface IContactRelationAssigner
    {
        /// <summary>
        /// Creates relationship between given <paramref name="user"/> and <paramref name="contact"/>.
        /// </summary>
        /// <remarks>
        /// Creates new <see cref="ContactMembershipInfo"/> record with given <paramref name="user"/> and <paramref name="contact"/>.
        /// </remarks>
        /// <param name="user">User to be assigned to the <paramref name="contact"/></param>
        /// <param name="contact">Contact the <paramref name="user"/> should be assigned to</param>
        /// <param name="checker">Checks whether the object should be assigned.</param>
        /// <example>
        /// Following example shows how to use method <see cref="Assign(IUserInfo, ContactInfo, IContactDataPropagationChecker)"/>.
        /// <code>
        /// ...
        /// IContactRelationAssigner contactRelationAssigner = someImplementation;
        /// var user = someUser;
        /// var contact = someContact;
        /// 
        /// // Will create new ContactMembershipInfo record of type User linking to the given contact and user
        /// contactRelationAssigner.Assign(user, contact);
        /// ...
        /// </code>
        /// </example>
        void Assign(IUserInfo user, ContactInfo contact, IContactDataPropagationChecker checker = null);


        /// <summary>
        /// Adds specified relationship among specified contact, related object and object type.
        /// </summary>
        /// <param name="memberType">Type of object</param>
        /// <param name="info">Base info of related object</param>
        /// <param name="contact">Contact info</param>
        /// <param name="checker">Checks whether the object should be assigned.</param>
        /// <example>
        /// Following example shows how to use method <see cref="Assign(MemberTypeEnum, BaseInfo, ContactInfo, IContactDataPropagationChecker)"/>.
        /// <code>
        /// ...
        /// IContactRelationAssigner contactRelationAssigner = someImplementation;
        /// var subscriber = subscriber;
        /// var contact = someContact;
        /// 
        /// // Will create new ContactMembershipInfo record of type Subscriber linking to the given contact and subscriber
        /// contactRelationAssigner.Assign(MemberTypeEnum.NewsletterSubscriber, subscriber, contact);
        /// ...
        /// </code>
        /// </example>
        void Assign(MemberTypeEnum memberType, BaseInfo info, ContactInfo contact, IContactDataPropagationChecker checker = null);


        /// <summary>
        /// Creates relationship between for given <paramref name="contactId"/> and related object with given <paramref name="relatedId"/> of type specified in <paramref name="memberType"/>.
        /// </summary>
        /// <param name="relatedId">ID of related object</param>
        /// <param name="memberType">Type of object</param>
        /// <param name="contactId">Contact ID</param>
        /// <param name="checker">Checks whether the object should be assigned.</param>
        /// <example>
        /// Following example shows how to use method <see cref="Assign(int, MemberTypeEnum, int, IContactDataPropagationChecker)"/>.
        /// <code>
        /// ...
        /// IContactRelationAssigner contactRelationAssigner = someImplementation;
        /// var subscriber = subscriber;
        /// var contactI = someContactId;
        /// 
        /// // Will create new ContactMembershipInfo record of type Subscriber linking to the given contact and subscriber
        /// contactRelationAssigner.Assign(subscriber.SubscriberID, MemberTypeEnum.NewsletterSubscriber, someContactId);
        /// ...
        /// </code>
        /// </example>
        void Assign(int relatedId, MemberTypeEnum memberType, int contactId, IContactDataPropagationChecker checker = null);
    }
}