using System;
using System.Collections.Generic;
using System.Linq;

using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Class providing ContactMembershipInfo management.
    /// </summary>
    public class ContactMembershipInfoProvider : AbstractInfoProvider<ContactMembershipInfo, ContactMembershipInfoProvider>
    {
        #region "Public methods - Basic"

        /// <summary>
        /// Returns a query for all the ContactMembershipInfo objects.
        /// </summary>
        public static ObjectQuery<ContactMembershipInfo> GetRelationships()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns list of IDs of all related objects for specified contact and membership type.
        /// </summary>
        /// <param name="contactId">Contact ID</param>
        /// <param name="membershipType">Membership type</param>
        public static List<int> GetRelationships(int contactId, MemberTypeEnum membershipType)
        {
            return GetRelationships()
                .WhereEquals("ContactID", contactId)
                .WhereEquals("MemberType", membershipType)
                .Column("RelatedID")
                .Select(x => x.RelatedID)
                .ToList();
        }


        /// <summary>
        /// Check whether given contact has relationship.
        /// </summary>
        /// <param name="contactId">Contact ID</param>
        public static bool HasRelationship(int contactId)
        {
            return ProviderObject.GetObjectQuery().WhereEquals("ContactID", contactId).Any();
        }


        /// <summary>
        /// Returns relationship among specified contact, related object and object type.
        /// </summary>
        /// <param name="membershipID">ID of membership record</param>
        public static ContactMembershipInfo GetMembershipInfo(int membershipID)
        {
            return ProviderObject.GetInfoById(membershipID);
        }


        /// <summary>
        /// Returns relationship among specified contact, related object and object type.
        /// </summary>
        /// <param name="contactId">Contact ID</param>
        /// <param name="relatedObjectId">Related object ID</param>
        /// <param name="memberType">Member type</param>
        public static ContactMembershipInfo GetMembershipInfo(int contactId, int relatedObjectId, MemberTypeEnum memberType)
        {
            return ProviderObject.GetMembershipInfoInternal(contactId, relatedObjectId, memberType);
        }


        /// <summary>
        /// Sets relationship among specified contact, related object and object type.
        /// </summary>
        /// <param name="infoObj">Contact-related object-object type relationship to be set</param>
        public static void SetMembershipInfo(ContactMembershipInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes relationship among specified contact, related object and object type.
        /// </summary>
        /// <param name="infoObj">Contact-related object-object type relationship to be deleted</param>
        public static void DeleteMembershipInfo(ContactMembershipInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes relationship among specified contact, related object and object type.
        /// </summary>
        /// <param name="membershipID">ID of record</param>
        public static void DeleteMembershipInfo(int membershipID)
        {
            ContactMembershipInfo infoObj = GetMembershipInfo(membershipID);
            DeleteMembershipInfo(infoObj);
        }


        /// <summary>
        /// Deletes relationship among specified contact, related object and object type.
        /// </summary>
        /// <param name="contactId">Contact ID</param>
        /// <param name="relatedObjectId">Related object ID</param>
        /// <param name="memberType">Member type</param>
        public static void DeleteMembershipInfo(int contactId, int relatedObjectId, MemberTypeEnum memberType)
        {
            ContactMembershipInfo infoObj = GetMembershipInfo(contactId, relatedObjectId, memberType);
            DeleteMembershipInfo(infoObj);
        }


        /// <summary>
        /// Gets contact ID from membership relations.
        /// </summary>
        /// <param name="relatedObjectId">Related ID </param>
        /// <param name="memberType">Member type (user, subscriber, customer)</param>
        public static int GetContactIDByMembership(int relatedObjectId, MemberTypeEnum memberType)
        {
            return GetRelationships().WhereEquals("RelatedID", relatedObjectId)
                                     .WhereEquals("MemberType", memberType)
                                     .Column("ContactID")
                                     .GetScalarResult(0);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Returns relationship among specified contact, related object and object type.
        /// </summary>
        /// <param name="contactId">Contact ID</param>
        /// <param name="relatedObjectId">Related object ID</param>
        /// <param name="memberType">Member type</param>
        protected virtual ContactMembershipInfo GetMembershipInfoInternal(int contactId, int relatedObjectId, MemberTypeEnum memberType)
        {
            return GetObjectQuery().TopN(1).Where(new WhereCondition()
                .WhereEquals("ContactID", contactId)
                .WhereEquals("RelatedID", relatedObjectId)
                .WhereEquals("MemberType", (int)memberType))
                .FirstOrDefault();
        }


        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(ContactMembershipInfo info)
        {
            if ((info != null) && (info.MembershipCreated == DateTimeHelper.ZERO_TIME))
            {
                info.MembershipCreated = DateTime.Now;
            }
            base.SetInfo(info);
        }

        #endregion
    }
}