using System;
using System.Collections.Generic;
using System.Linq;

using CMS.DataEngine;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Class providing ContactGroupMemberInfo management.
    /// </summary>
    public class ContactGroupMemberInfoProvider : AbstractInfoProvider<ContactGroupMemberInfo, ContactGroupMemberInfoProvider>
    {
        #region "Public methods - Basic"

        /// <summary>
        /// Returns a query of all relationships among contact groups, related objects of specific types.
        /// </summary>
        public static ObjectQuery<ContactGroupMemberInfo> GetRelationships()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns relationship by ID.
        /// </summary>
        /// <param name="contactGroupMemberId">Contact group member ID</param>
        public static ContactGroupMemberInfo GetContactGroupMemberInfo(int contactGroupMemberId)
        {
            return ProviderObject.GetInfoById(contactGroupMemberId);
        }


        /// <summary>
        /// Returns relationship among specified contact group and related object of specific type.
        /// </summary>
        /// <param name="groupId">Contact group ID</param>
        /// <param name="relatedObjectId">Related object ID</param>
        /// <param name="memberType">Member type</param>
        public static ContactGroupMemberInfo GetContactGroupMemberInfoByData(int groupId, int relatedObjectId, ContactGroupMemberTypeEnum memberType)
        {
            return ProviderObject.GetContactGroupMemberInfoByDataInternal(groupId, relatedObjectId, memberType);
        }


        /// <summary>
        /// Sets relationship among specified contact group and related object of specific type.
        /// </summary>
        /// <param name="infoObj">Contact group-related object-object type relationship to be set</param>
        public static void SetContactGroupMemberInfo(ContactGroupMemberInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Sets relationship among specified contact group and related object of specific type.
        /// </summary>	
        /// <param name="groupId">Contact group ID</param>
        /// <param name="relatedObjectId">Related object ID</param>
        /// <param name="memberType">Member type</param>
        /// <param name="addedHow">Possible types of contact addition into contact group</param>
        public static void SetContactGroupMemberInfo(int groupId, int relatedObjectId, ContactGroupMemberTypeEnum memberType, MemberAddedHowEnum addedHow)
        {
            ContactGroupMemberInfo infoObj = GetContactGroupMemberInfoByData(groupId, relatedObjectId, memberType);
            if (infoObj == null)
            {
                infoObj = ProviderObject.CreateInfo();
                infoObj.ContactGroupMemberContactGroupID = groupId;
                infoObj.ContactGroupMemberRelatedID = relatedObjectId;
                infoObj.ContactGroupMemberType = memberType;
            }

            // Set how was member added
            switch (addedHow)
            {
                case MemberAddedHowEnum.Dynamic:
                    infoObj.ContactGroupMemberFromCondition = true;
                    break;
                case MemberAddedHowEnum.Account:
                    infoObj.ContactGroupMemberFromAccount = true;
                    break;
                case MemberAddedHowEnum.Manual:
                    infoObj.ContactGroupMemberFromManual = true;
                    break;
            }

            SetContactGroupMemberInfo(infoObj);
        }


        


        /// <summary>
        /// Deletes relationship among specified contact group and related object of specific type.
        /// </summary>
        /// <param name="infoObj">Contact group-related object-object type relationship to be deleted</param>
        public static void DeleteContactGroupMemberInfo(ContactGroupMemberInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes relationship specified by ID.
        /// </summary>
        /// <param name="contactGroupMemberId">Contact group member ID</param>
        public static void DeleteContactGroupMemberInfo(int contactGroupMemberId)
        {
            ContactGroupMemberInfo infoObj = GetContactGroupMemberInfo(contactGroupMemberId);
            DeleteContactGroupMemberInfo(infoObj);
        }


        /// <summary>
        /// Deletes relationship among specified contact group and related object of specific type.
        /// </summary>
        /// <param name="groupId">Contact group ID</param>
        /// <param name="relatedObjectId">Related object ID</param>
        /// <param name="memberType">Member type</param>
        public static void DeleteContactGroupMemberInfo(int groupId, int relatedObjectId, ContactGroupMemberTypeEnum memberType)
        {
            ContactGroupMemberInfo infoObj = GetContactGroupMemberInfoByData(groupId, relatedObjectId, memberType);
            DeleteContactGroupMemberInfo(infoObj);
        }


        /// <summary>
        /// Removes specified contact from all groups.
        /// </summary>
        /// <param name="contactId">Contact to be removed from groups</param>
        internal static void DeleteMemberInfosForContact(int contactId)
        {
            GetRelationships().WhereEquals("ContactGroupMemberRelatedID", contactId)
                              .WhereEquals("ContactGroupMemberType", (int)ContactGroupMemberTypeEnum.Contact)
                              .ForEachObject(relationship => relationship.Delete());
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Deletes relationships between specified contact groups and related objects (contact, account) defined in where condition.
        /// </summary>
        /// <param name="where">Where condition</param>
        /// <param name="contactGroupID">Current groupID. Used only when deleting accounts and removing its contacts.</param>
        /// <param name="deletingAccount">Indicates if deleting accounts. If TRUE then also contacts will be removed as well.</param>
        /// <param name="removeAccount">If TRUE then account will be deleted, not only its contacts. Applicable only when deleting accounts.</param>
        public static void DeleteContactGroupMembers(string where, int contactGroupID, bool deletingAccount, bool removeAccount)
        {
            if (deletingAccount)
            {
                // Prepare the parameters
                QueryDataParameters parameters = new QueryDataParameters();
                parameters.Add("@where", where);
                parameters.Add("@contactGroupID", contactGroupID);
                ConnectionHelper.ExecuteQuery("OM.ContactGroupMember.removeaccountcontacts", parameters);
            }

            // If we are deleting only contacts of current account
            if (deletingAccount && !removeAccount)
            {
                // Don't delete account
            }
            // If we are deleting contacts or account and it is required to delete account
            else
            {
                ProviderObject.DeleteContactGroupMembersInternal(where);
            }
        }


       /// <summary>
        /// Returns number of contacts in specified contact group.
        /// </summary>
        /// <param name="contactGroupId">Contact group ID</param>
        public static int GetNumberOfContactsInGroup(int contactGroupId)
        {
            return ProviderObject.GetNumberOfContactsInGroupInternal(contactGroupId);
        }


        /// <summary>
        /// Sets contacts represented by <paramref name="contactIDsToBeAdded"/> as dynamic members of given Contact Group. 
        /// Preserves all existing memberships that are not dynamic (manual or from account).
        /// </summary>
        /// <param name="contactGroup">Contact group in which the contacts will be assigned (and removed)</param>
        /// <param name="contactIDsToBeAdded">Contacts that will be set as dynamic in given contact group</param>
        /// <param name="allContactIDs">
        /// Contacts which are present in this list and are not in <paramref name="contactIDsToBeAdded"/> will be removed from the group by dynamic condition. 
        /// If null, this method will remove all contacts which are not in <paramref name="contactIDsToBeAdded"/> from group.
        /// </param>
        internal static void SetContactsAsDynamic(ContactGroupInfo contactGroup, IEnumerable<int> contactIDsToBeAdded, IEnumerable<int> allContactIDs = null)
        {
            if (contactGroup == null)
            {
                throw new ArgumentNullException(nameof(contactGroup));
            }
            if (contactIDsToBeAdded == null)
            {
                throw new ArgumentNullException(nameof(contactIDsToBeAdded));
            }

            var parameters = new QueryDataParameters();
            parameters.Add("@ContactGroupID", contactGroup.ContactGroupID);

            var contactIDsToBeAddedList = contactIDsToBeAdded.ToList();
            if (contactIDsToBeAddedList.Count > 0)
            {
                var contactIDsIntTable = SqlHelper.BuildOrderedIntTable(contactIDsToBeAddedList);
                parameters.Add("@ContactIDs", contactIDsIntTable, SqlHelper.OrderedIntegerTableType);

            }

            // Optionally, change context of all contacts
            if (allContactIDs != null)
            {
                var allContactIDsList = allContactIDs.ToList();
                if (allContactIDsList.Count > 0)
                {
                    var allContactIDsIntTable = SqlHelper.BuildOrderedIntTable(allContactIDsList);
                    parameters.Add("@AllContactIDs", allContactIDsIntTable, SqlHelper.OrderedIntegerTableType);
                }
            }

            using (var cs = new CMSConnectionScope())
            {
                cs.CommandTimeout = ConnectionHelper.LongRunningCommandTimeout;
                ConnectionHelper.ExecuteQuery("om.contactgroupmember.addcontactstocontactgroupdynamic", parameters);
            }
        }


        /// <summary>
        /// Sets a contact represented by <paramref name="contact"/> as dynamic members of given contact groups represented by <paramref name="contactGroupIDs"/>.
        /// Preserves all existing memberships that are not dynamic (manual or from account).
        /// </summary>
        /// <param name="contact">Contact which will be assigned (and removed) as dynamic member</param>
        /// <param name="contactGroupIDs">Contact groups that will be set as dynamic membership for given contact group</param>
        /// <param name="allContactGroupIDs">Contact groups that are in this list and not in <paramref name="contactGroupIDs"/> will be unassigned from <paramref name="allContactGroupIDs"/></param>
        internal static void SetContactAsDynamic(ContactInfo contact, IEnumerable<int> contactGroupIDs, IEnumerable<int> allContactGroupIDs)
        {
            if (contact == null)
            {
                throw new ArgumentNullException(nameof(contact));
            }
            if (contactGroupIDs == null)
            {
                throw new ArgumentNullException(nameof(contactGroupIDs));
            }
            if (allContactGroupIDs == null)
            {
                throw new ArgumentNullException(nameof(allContactGroupIDs));
            }

            var parameters = new QueryDataParameters();
            parameters.Add("@ContactID", contact.ContactID);

            var contactGroupIDsList = contactGroupIDs.ToList();
            if (contactGroupIDsList.Count > 0)
            {
                var contactGroupIDsParameter = SqlHelper.BuildOrderedIntTable(contactGroupIDsList);
                parameters.Add("@ContactGroupIDs", contactGroupIDsParameter, SqlHelper.OrderedIntegerTableType);
            }

            var allContactGroupIDsList = allContactGroupIDs.ToList();
            if (allContactGroupIDsList.Count > 0)
            {
                var allContactGroupIDsParameter = SqlHelper.BuildOrderedIntTable(allContactGroupIDsList);
                parameters.Add("@AllContactGroupIDs", allContactGroupIDsParameter, SqlHelper.OrderedIntegerTableType);
            }

            using (var cs = new CMSConnectionScope())
            {
                cs.CommandTimeout = ConnectionHelper.LongRunningCommandTimeout;
                ConnectionHelper.ExecuteQuery("om.contactgroupmember.addcontacttocontactgroupsdynamic", parameters);
            }
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Returns relationship among specified contact group and related object of specific type.
        /// </summary>
        /// <param name="groupId">Contact group ID</param>
        /// <param name="relatedObjectId">Related object ID</param>
        /// <param name="memberType">Membership type</param>
        protected virtual ContactGroupMemberInfo GetContactGroupMemberInfoByDataInternal(int groupId, int relatedObjectId, ContactGroupMemberTypeEnum memberType)
        {
            var condition = new WhereCondition()
                .WhereEquals("ContactGroupMemberContactGroupID", groupId)
                .WhereEquals("ContactGroupMemberRelatedID", relatedObjectId)
                .WhereEquals("ContactGroupMemberType", (int)memberType);

            return GetObjectQuery().TopN(1).Where(condition).FirstOrDefault();
        }


        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(ContactGroupMemberInfo info)
        {
            base.SetInfo(info);

            // Add all contacts of the account to the specified contact group
            if (info.ContactGroupMemberType == ContactGroupMemberTypeEnum.Account)
            {
                // Prepare the parameters
                QueryDataParameters parameters = new QueryDataParameters();
                parameters.Add("@accountID", info.ContactGroupMemberRelatedID);
                parameters.Add("@groupID", info.ContactGroupMemberContactGroupID);
                ConnectionHelper.ExecuteQuery("om.contactgroupmember.addaccountcontacts", parameters);
            }
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(ContactGroupMemberInfo info)
        {
            // Remove all contacts of the account from the specified contact group
            if ((info != null) && (info.ContactGroupMemberType == ContactGroupMemberTypeEnum.Account))
            {
                // Prepare the parameters
                QueryDataParameters parameters = new QueryDataParameters();
                parameters.Add("@where", "ContactGroupMemberType = 1 AND ContactGroupMemberRelatedID = " + info.ContactGroupMemberRelatedID);
                parameters.Add("@contactGroupID", info.ContactGroupMemberContactGroupID);
                ConnectionHelper.ExecuteQuery("OM.ContactGroupMember.removeaccountcontacts", parameters);
            }

            base.DeleteInfo(info);
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Deletes relationships between specified contact groups and related objects (contact, account) defined in where condition.
        /// </summary>
        /// <param name="where">Where condition</param>
        protected virtual void DeleteContactGroupMembersInternal(string where)
        {
            var whereCondition = new WhereCondition(where);

            // We need to define object type explicitly to delete all object types
            BulkDelete(whereCondition, new BulkDeleteSettings { ObjectType = ContactGroupMemberInfo.OBJECT_TYPE_CONTACT });
            BulkDelete(whereCondition, new BulkDeleteSettings { ObjectType = ContactGroupMemberInfo.OBJECT_TYPE_ACCOUNT });
        }


        /// <summary>
        /// Returns number of contacts in specified contact group.
        /// </summary>
        /// <param name="contactGroupId">Contact group ID</param>
        protected virtual int GetNumberOfContactsInGroupInternal(int contactGroupId)
        {
            return GetRelationships().WhereEquals("ContactGroupMemberContactGroupID", contactGroupId)
                                     .WhereEquals("ContactGroupMemberType", (int)ContactGroupMemberTypeEnum.Contact)
                                     .Count();
        }

        #endregion
    }
}