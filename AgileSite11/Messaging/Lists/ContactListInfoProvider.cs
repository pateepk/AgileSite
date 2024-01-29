using System;
using System.Data;

using CMS.DataEngine;
using CMS.DataEngine.Query;
using CMS.Helpers;
using CMS.LicenseProvider;

namespace CMS.Messaging
{
    using TypedDataSet = InfoDataSet<ContactListInfo>;

    /// <summary>
    /// Class providing ContactListInfo management.
    /// </summary>
    public class ContactListInfoProvider : AbstractInfoProvider<ContactListInfo, ContactListInfoProvider>
    {
        #region "Public methods"

        /// <summary>
        /// Returns the ContactListInfo structure for the specified contactList.
        /// </summary>
        /// <param name="ownerId">ID of user</param>
        /// <param name="contactId">ID of user in contactlist</param>
        public static ContactListInfo GetContactListInfo(int ownerId, int contactId)
        {
            return ProviderObject.GetContactListInfoInternal(ownerId, contactId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified contactList item.
        /// </summary>
        /// <param name="contact">ContactList info object to set</param>
        public static void SetContactListInfo(ContactListInfo contact)
        {
            // Check license for messaging
            LicenseHelper.CheckFeatureAndRedirect(RequestContext.CurrentDomain, FeatureEnum.Messaging);

            ProviderObject.SetContactListInfoInternal(contact);
        }


        /// <summary>
        /// Deletes specified contact from contactlist.
        /// </summary>
        /// <param name="contact">ContactList object</param>
        public static void DeleteContactListInfo(ContactListInfo contact)
        {
            ProviderObject.DeleteInfo(contact);
        }


        /// <summary>
        /// Adds user to contactlist.
        /// </summary>
        /// <param name="ownerId">ID of contactlist owner</param>
        /// <param name="contactId">ID of user to add to contactlist</param>
        public static void AddToContactList(int ownerId, int contactId)
        {
            ProviderObject.AddToContactListInternal(ownerId, contactId);
        }


        /// <summary>
        /// Removes user from contactlist.
        /// </summary>
        /// <param name="contactId">ID of user to be removed from contactlist</param>
        /// <param name="ownerId">ID of contactlist owner</param>
        public static void RemoveFromContactList(int ownerId, int contactId)
        {
            ProviderObject.RemoveFromContactListInternal(ownerId, contactId);
        }


        /// <summary>
        /// Returns contactlist based on conditions.
        /// </summary>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by clause</param>
        [Obsolete("Use CMS.DataEngine.ObjectQuery<ContactListInfo> instead")]
        public static TypedDataSet GetContactList(string where, string orderBy)
        {
            // Check license for messaging
            LicenseHelper.CheckFeatureAndRedirect(RequestContext.CurrentDomain, FeatureEnum.Messaging);

            return ProviderObject.GetContactListInternal(where, orderBy);
        }


        /// <summary>
        /// Returns all users from user's contactlist.
        /// </summary>
        /// <param name="ownerId">User ID</param>
        public static TypedDataSet GetContactList(int ownerId)
        {
            // Check license for messaging
            LicenseHelper.CheckFeatureAndRedirect(RequestContext.CurrentDomain, FeatureEnum.Messaging);

            return ProviderObject.GetContactListInternal(ownerId, null, null, -1, null);
        }


        /// <summary>
        /// Returns all users from user's contactlist.
        /// </summary>
        /// <param name="ownerId">User ID</param>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by clause</param>
        /// <param name="topN">Select only top N columns</param>
        /// <param name="columns">Specifies columns to be selected</param>
        public static TypedDataSet GetContactList(int ownerId, string where, string orderBy, int topN, string columns)
        {
            // Check license for messaging
            LicenseHelper.CheckFeatureAndRedirect(RequestContext.CurrentDomain, FeatureEnum.Messaging);

            return ProviderObject.GetContactListInternal(ownerId, where, orderBy, topN, columns);
        }


        /// <summary>
        /// Returns all users from user's contactlist.
        /// </summary>
        /// <param name="ownerId">User ID</param>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by clause</param>
        /// <param name="topN">Select only top N columns</param>
        /// <param name="columns">Specifies columns to be selected</param>
        /// <param name="offset">Index of first record to get</param>
        /// <param name="maxRecords">Maximum number of records to get. If maxRecords is zero or less, all records are returned (no paging is used)</param>
        /// <param name="totalRecords">Returns total records</param>
        public static TypedDataSet GetContactList(int ownerId, string where, string orderBy, int topN, string columns, int offset, int maxRecords, ref int totalRecords)
        {
            // Check license for messaging
            LicenseHelper.CheckFeatureAndRedirect(RequestContext.CurrentDomain, FeatureEnum.Messaging);

            return ProviderObject.GetContactListInternal(ownerId, where, orderBy, topN, columns, offset, maxRecords, ref totalRecords);
        }


        /// <summary>
        /// Returns true if given user is in owner's contactlist.
        /// </summary>
        /// <param name="ownerId">ID of contactlist owner</param>
        /// <param name="contactId">ID of user to check</param>
        public static bool IsInContactList(int ownerId, int contactId)
        {
            return ProviderObject.IsInContactListInternal(ownerId, contactId);
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Returns the ContactListInfo structure for the specified contactList.
        /// </summary>
        /// <param name="ownerId">ID of contact list owner</param>
        /// <param name="contactId">ID of user in contactlist</param>
        protected virtual ContactListInfo GetContactListInfoInternal(int ownerId, int contactId)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@ContactListContactUserID", contactId);
            parameters.Add("@ContactListUserID", ownerId);

            // Get the data
            DataSet ds = ConnectionHelper.ExecuteQuery("messaging.contactlist.select", parameters);
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                return new ContactListInfo(ds.Tables[0].Rows[0]);
            }

            return null;
        }


        /// <summary>
        /// Sets (updates or inserts) specified contactList item.
        /// </summary>
        /// <param name="contact">ContactList info object to set</param>
        protected virtual void SetContactListInfoInternal(ContactListInfo contact)
        {
            if (contact != null)
            {
                // Check IDs
                if ((contact.ContactListContactUserID <= 0) || (contact.ContactListUserID <= 0))
                {
                    throw new Exception("[ContactListInfoProvider.SetContactListInfo]: Object IDs not set.");
                }

                // Get existing
                ContactListInfo existing = GetContactListInfoInternal(contact.ContactListUserID, contact.ContactListContactUserID);
                if (existing != null)
                {
                    contact.Generalized.UpdateData();
                }
                else
                {
                    contact.Generalized.InsertData();
                }
            }
            else
            {
                throw new Exception("[ContactListInfoProvider.SetContactListInfo]: No ContactListInfo object set.");
            }
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(ContactListInfo info)
        {
            if (info != null)
            {
                base.DeleteInfo(info);
            }
        }


        /// <summary>
        /// Adds user to contactlist.
        /// </summary>
        /// <param name="ownerId">ID of contactlist owner</param>
        /// <param name="contactId">ID of user to add to contactlist</param>
        protected virtual void AddToContactListInternal(int ownerId, int contactId)
        {
            // Add userto the contact list
            if (!IsInContactListInternal(ownerId, contactId))
            {
                ContactListInfo cli = new ContactListInfo();
                cli.ContactListUserID = ownerId;
                cli.ContactListContactUserID = contactId;
                SetContactListInfoInternal(cli);
            }

            // Remove user from ignore list
            IgnoreListInfoProvider.RemoveFromIgnoreList(ownerId, contactId);
        }


        /// <summary>
        /// Removes user from contactlist.
        /// </summary>
        /// <param name="contactId">ID of user to be removed from contactlist</param>
        /// <param name="ownerId">ID of contactlist owner</param>
        protected virtual void RemoveFromContactListInternal(int ownerId, int contactId)
        {
            ContactListInfo infoObj = GetContactListInfoInternal(ownerId, contactId);
            DeleteInfo(infoObj);
        }


        /// <summary>
        /// Returns all users from user's contactlist.
        /// </summary>
        /// <param name="ownerId">User ID</param>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by clause</param>
        /// <param name="topN">Select only top N columns</param>
        /// <param name="columns">Specifies columns to be selected</param>
        protected virtual TypedDataSet GetContactListInternal(int ownerId, string where, string orderBy, int topN, string columns)
        {
            int totalRecords = 0;
            return GetContactListInternal(ownerId, where, orderBy, topN, columns, 0, 0, ref totalRecords);
        }


        /// <summary>
        /// Returns all users from user's contactlist.
        /// </summary>
        /// <param name="ownerId">User ID</param>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by clause</param>
        /// <param name="topN">Select only top N columns</param>
        /// <param name="columns">Specifies columns to be selected</param>
        /// <param name="offset">Index of first record to get</param>
        /// <param name="maxRecords">Maximum number of records to get. If maxRecords is zero or less, all records are returned (no paging is used)</param>
        /// <param name="totalRecords">Returns total records</param>
        protected virtual TypedDataSet GetContactListInternal(int ownerId, string where, string orderBy, int topN, string columns, int offset, int maxRecords, ref int totalRecords)
        {
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@UserID ", ownerId);
            parameters.EnsureDataSet<ContactListInfo>();

            return ConnectionHelper.ExecuteQuery("messaging.contactlist.selectcontactlist", parameters, where, orderBy, topN, columns, offset, maxRecords, ref totalRecords).As<ContactListInfo>();
        }


        /// <summary>
        /// Returns contactlist based on conditions.
        /// </summary>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by clause</param>
        [Obsolete("Use CMS.DataEngine.ObjectQuery<ContactListInfo> instead")]
        protected virtual TypedDataSet GetContactListInternal(string where, string orderBy)
        {
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.EnsureDataSet<ContactListInfo>();

            return ConnectionHelper.ExecuteQuery("messaging.contactlist.selectall", parameters, where, orderBy).As<ContactListInfo>();
        }


        /// <summary>
        /// Returns true if given user is in owner's contactlist.
        /// </summary>
        /// <param name="ownerId">ID of contactlist owner</param>
        /// <param name="contactId">ID of user to check</param>
        protected virtual bool IsInContactListInternal(int ownerId, int contactId)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@ContactListContactUserID", contactId);
            parameters.Add("@ContactListUserID", ownerId);

            DataSet result = ConnectionHelper.ExecuteQuery("messaging.contactlist.select", parameters);

            return (!DataHelper.DataSourceIsEmpty(result));
        }

        #endregion
    }
}