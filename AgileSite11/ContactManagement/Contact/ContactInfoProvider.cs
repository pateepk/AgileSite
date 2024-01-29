using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS.Automation;
using CMS.Base;
using CMS.Core;
using CMS.Core.Internal;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.FormEngine;
using CMS.Globalization;
using CMS.Helpers;
using CMS.Membership;
using CMS.SiteProvider;
using CMS.WebAnalytics;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Class providing ContactInfo management.
    /// </summary>
    public class ContactInfoProvider : AbstractInfoProvider<ContactInfo, ContactInfoProvider>
    {
        #region "Private variables"

        /// <summary>
        /// Delete lock
        /// </summary>
        private static readonly object deleteLock = new object();

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor which enables weak reference caching by ID and GUID.
        /// </summary>
        public ContactInfoProvider()
            : base(ContactInfo.TYPEINFO, new HashtableSettings
                {
                    ID = true,
                    GUID = true,
                    UseWeakReferences = true
                })
        {
        }

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns a query for all the ContactInfo objects.
        /// </summary>
        public static ObjectQuery<ContactInfo> GetContacts()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns contact with specified ID.
        /// </summary>
        /// <param name="contactId">Contact ID</param>        
        public static ContactInfo GetContactInfo(int contactId)
        {
            return ProviderObject.GetContactInfoInternal(contactId);
        }


        /// <summary>
        /// Returns contact with specified GUID.
        /// </summary>
        /// <param name="contactGuid">Contact GUID</param>               
        public static ContactInfo GetContactInfo(Guid contactGuid)
        {
            return ProviderObject.GetContactInfoInternal(contactGuid);
        }


        /// <summary>
        /// Returns contact with specified email.
        /// </summary>
        /// <param name="email">Email to be searched for.</param>          
        /// <returns><see cref="ContactInfo"/> with provided email or <c>null</c> if no contact with given email found.
        /// If more contacts with such email exists most recent one is returned.</returns>
        public static ContactInfo GetContactInfo(string email)
        {
            return ProviderObject.GetContactInfoInternal(email);
        }


        /// <summary>
        /// Sets (updates or inserts) specified contact.
        /// </summary>
        /// <param name="contactObj">Contact to be set</param>
        /// <param name="wasMerged">Determines whether the given contact was merged and therefore the process should propagate the value to the log changes queue</param>  
        public static void SetContactInfo(ContactInfo contactObj, bool wasMerged = false)
        {
            ProviderObject.SetContactInfoInternal(contactObj, wasMerged);
        }
        

        /// <summary>
        /// Deletes specified contact.
        /// </summary>
        /// <param name="contactObj">Contact to be deleted</param>
        public static void DeleteContactInfo(ContactInfo contactObj)
        {
            ProviderObject.DeleteInfo(contactObj);
        }


        /// <summary>
        /// Deletes contact with specified ID.
        /// </summary>
        /// <param name="contactId">Contact ID of the contact to be deleted</param>
        public static void DeleteContactInfo(int contactId)
        {
            ContactInfo contactObj = GetContactInfo(contactId);
            DeleteContactInfo(contactObj);
        }
        

        /// <summary>
        /// Deletes all infos by stored procedure.
        /// </summary>
        /// <param name="whereCondition">Where condition</param>
        /// <param name="batchLimit">Batch limit.</param>
        public static void DeleteContactInfos(string whereCondition, int batchLimit)
        {
            ProviderObject.DeleteContactInfosInternal(whereCondition, batchLimit);
        }

        
        /// <summary>
        /// Updates the specified contact in database with information related to Salesforce lead replication.
        /// </summary>
        /// <param name="contact">The contact to update.</param>
        /// <param name="lastLeadReplicationDateTime">The date and time of contact's last successful replication.</param>
        public static void UpdateLeadReplicationStatus(ContactInfo contact, DateTime lastLeadReplicationDateTime)
        {
            ProviderObject.UpdateLeadReplicationStatusInternal(contact, lastLeadReplicationDateTime);
        }


        /// <summary>
        /// Marks the specified contact as required for lead replication despite its score.
        /// </summary>
        /// <param name="contact">The contact to mark for lead replication.</param>
        public static void RequireLeadReplication(ContactInfo contact)
        {
            ProviderObject.RequireLeadReplicationInternal(contact);
        }


        /// <summary>
        /// Finds contact for given email address.
        /// </summary>
        /// <returns>Returns contact id for given email address, returns 0 if contact is not found.</returns>
        public static int GetContactIDByEmail(string email)
        {
            var contactInfo = GetContactInfo(email);
            if (contactInfo != null)
            {
                return contactInfo.ContactID;
            }
            return 0;
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Returns full contact name consisting of first, middle and last name.
        /// </summary>
        /// <param name="id">Contact info ID</param>
        public static string GetContactFullName(int id)
        {
            return GetContactFullName(GetContactInfo(id));
        }


        /// <summary>
        /// Returns full contact name consisting of first, middle and last name.
        /// </summary>
        /// <param name="ci">Contact info object</param>
        public static string GetContactFullName(ContactInfo ci)
        {
            if (ci == null)
            {
                return null;
            }

            string name = ci.ContactFirstName + " " + ci.ContactMiddleName;
            name = name.Trim() + " " + ci.ContactLastName;

            return name.Trim();
        }


        /// <summary>
        /// Updates contact status ID of specified contacts.
        /// </summary>
        /// <param name="statusId">Contact status ID</param>
        /// <param name="where">Where condition</param>
        public static void UpdateContactStatus(int statusId, string where)
        {
            ProviderObject.UpdateContactStatusInternal(statusId, where);
        }


        /// <summary>
        /// Increments number of bounces by one for specified contact.
        /// </summary>
        /// <param name="contactId">Contact ID</param>
        public static void AddContactBounce(int contactId)
        {
            ProviderObject.AddContactBounceInternal(contactId);
        }


        /// <summary>
        /// Increments number of bounces by one for all contacts specified by e-mail address across all sites.
        /// </summary>
        /// <param name="email">E-mail address</param>
        public static void AddContactBounceByEmail(string email)
        {
            ProviderObject.AddContactBounceByEmailInternal(email);
        }


        /// <summary>
        /// Removes given persona from contacts. Sets column ContactPersonaID to NULL on contacts where this column equals to given persona ID.
        /// </summary>
        /// <param name="personaID">This persona will be removed from contacts</param>
        public static void RemovePersonaFromContacts(int personaID)
        {
            ProviderObject.RemovePersonaFromContactsInternal(personaID);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Returns contact with specified ID.
        /// </summary>
        /// <param name="contactId">Contact ID</param>        
        protected virtual ContactInfo GetContactInfoInternal(int contactId)
        {
            return GetInfoById(contactId);
        }


        /// <summary>
        /// Returns contact with specified GUID.
        /// </summary>
        /// <param name="contactGuid">Contact GUID</param>
        protected virtual ContactInfo GetContactInfoInternal(Guid contactGuid)
        {
            return GetInfoByGuid(contactGuid);
        }


        /// <summary>
        /// Returns contact with specified email.
        /// </summary>
        /// <param name="email">Contact email or <c>null</c> if no contact with given email found.</param>
        protected virtual ContactInfo GetContactInfoInternal(string email)
        {
            return CacheHelper.Cache(cs =>
                {
                    var contact = GetContacts().WhereEquals("ContactEmail", email)
                                               .OrderByAscending("ContactCreated")
                                               .FirstOrDefault();

                    string dependencyKey;
                    if (contact == null)
                    {
                        dependencyKey = ContactInfo.OBJECT_TYPE + "|all";
                    }
                    else
                    {
                        dependencyKey = ContactInfo.OBJECT_TYPE + "|byid|" + contact.ContactID;
                    }
                    cs.CacheDependency = CacheHelper.GetCacheDependency(dependencyKey);

                    return contact;
                }
                , new CacheSettings(10, "ContactInfoProvider", email));
        }


        /// <summary>
        /// Sets (updates or inserts) specified contact.
        /// </summary>
        /// <param name="contactObj">Contact to be set</param>
        /// <param name="wasMerged">Determines whether the given contact was merged and therefore the process should propagate the value to the log changes queue</param>  
        protected virtual void SetContactInfoInternal(ContactInfo contactObj, bool wasMerged)
        {
            if (contactObj == null)
            {
                throw new ArgumentNullException("contactObj");
            }

            // Contact is being created, set init values 
            bool newContact = (contactObj.ContactCreated == DateTimeHelper.ZERO_TIME);
            if (newContact)
            {
                InitNewContact(contactObj);
            }
            else if (contactObj.ItemChanged("ContactEmail"))
            {
                // Reset email bounces count if email address changed so that contact can receive newsletters even if he had invalid email address before
                contactObj.ContactBounces = 0;
            }

            List<string> changedColumns = wasMerged ? contactObj.ColumnNames : contactObj.ChangedColumns();
            
            SetInfo(contactObj);

            // Evaluate dynamic contact groups and lead scoring groups
            LogContactChange(contactObj.ContactID, changedColumns, wasMerged, newContact);
            
            // Update online users table with changed columns
            OnlineUserHelper.UpdateSessions(contactObj, changedColumns, OnlineUserHelper.SessionType.Contact);
        }


        /// <summary>
        /// Deletes all contacts by specified ID. 
        /// </summary>
        /// <param name="where">Where condition</param>
        /// <param name="batchLimit">Batch limit</param>
        protected virtual void DeleteContactInfosInternal(string where, int batchLimit)
        {
            // Fix empty where condition to fit into query
            if (String.IsNullOrEmpty(where))
            {
                where = "1=1";
            }

            // Prepare parameters
            var parameters = new QueryDataParameters();
            parameters.Add("@where", where);
            parameters.Add("@batchLimit", batchLimit > 0 ? (object)batchLimit : null);
            IList<int> deletedContactsIds = new List<int>();
            IList<Guid> deletedContactsGUIDs = new List<Guid>();

            lock (deleteLock)
            {
                DataSet deletedItems;

                using (var cs = new CMSConnectionScope())
                {
                    cs.CommandTimeout = ConnectionHelper.LongRunningCommandTimeout;

                    deletedItems = ConnectionHelper.ExecuteQuery("om.contact.massdelete", parameters);
                }

                // Remove automation history and newsletter dependencies for deleted contacts
                if (!DataHelper.DataSourceIsEmpty(deletedItems))
                {
                    deletedContactsIds = deletedItems.Tables[0].AsEnumerable().Select(i => i.Field<int>("ContactID")).ToList();
                    deletedContactsGUIDs = deletedItems.Tables[0].AsEnumerable().Select(i => i.Field<Guid>("ContactGUID")).ToList();
                    AutomationStateInfoProvider.DeleteAutomationStates(ContactInfo.OBJECT_TYPE, deletedContactsIds);
                }
            }

            ContactInfo.TYPEINFO.InvalidateAllObjects();
            ClearHashtables(true);

            if (deletedContactsIds.Any())
            {
                var eventArgs = new ContactInfosDeletedHandlerEventArgs();
                eventArgs.DeletedContactsIds = deletedContactsIds;
                eventArgs.DeletedContactsGUIDs = deletedContactsGUIDs;
                ContactManagementEvents.ContactInfosDeleted.StartEvent(eventArgs);
            }
        }


        /// <summary>
        /// Updates the specified contact in database with information related to Salesforce lead replication.
        /// </summary>
        /// <param name="contact">The contact to update.</param>
        /// <param name="lastLeadReplicationDateTime">The date and time of contact's last successful replication.</param>
        protected virtual void UpdateLeadReplicationStatusInternal(ContactInfo contact, DateTime lastLeadReplicationDateTime)
        {
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@LeadID", contact.GetValue("ContactSalesForceLeadID"));
            parameters.Add("@LeadReplicationDisabled", contact.GetValue("ContactSalesForceLeadReplicationDisabled"));
            parameters.Add("@LeadReplicationDateTime", contact.GetValue("ContactSalesForceLeadReplicationDateTime"));
            parameters.Add("@LeadReplicationSuspensionDateTime", contact.GetValue("ContactSalesForceLeadReplicationSuspensionDateTime"));
            parameters.Add("@LeadReplicationRequired", contact.GetValue("ContactSalesForceLeadReplicationRequired"));
            parameters.Add("@ContactID", contact.ContactID);
            parameters.Add("@LastLeadReplicationDateTime", lastLeadReplicationDateTime == DateTimeHelper.ZERO_TIME ? new DateTime(1900, 1, 1) : lastLeadReplicationDateTime);

            ConnectionHelper.ExecuteQuery("om.contact.updateleadreplicationstatus", parameters);
        }


        /// <summary>
        /// Marks the specified contact as required for lead replication despite its score.
        /// </summary>
        /// <param name="contact">The contact to mark for lead replication.</param>
        protected virtual void RequireLeadReplicationInternal(ContactInfo contact)
        {
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@ContactID", contact.ContactID);

            ConnectionHelper.ExecuteQuery("om.contact.requireleadreplication", parameters);
        }

        #endregion


        #region "Internal methods - Advanced"
        
        /// <summary>
        /// Updates contact status ID of specified contacts.
        /// </summary>
        /// <param name="statusId">Contact status ID</param>
        /// <param name="where">Where condition</param>
        protected virtual void UpdateContactStatusInternal(int statusId, string where)
        {
            string statusFieldName = "ContactStatusID";
            var changedProperties = new List<string> { statusFieldName };

            var updateExp = String.Format("{0}={1}", statusFieldName, ((statusId > 0) ? statusId.ToString() : "NULL"));
            UpdateData(updateExp, null, where);

            var contactsQuery = GetContacts().Where(where).Columns("ContactId");
            contactsQuery.ForEachPage(pages =>
            {
                foreach (var contact in pages)
                {
                    LogContactChange(contact.ContactID, changedProperties);
                }
            }, 10000);

            ClearHashtables(true);
        }


        /// <summary>
        /// Updates contact information from record submitted through BizForm module - this requires form fields to be mapped to contact.
        /// </summary>
        /// <param name="source">Data class object with source data and mapping definition</param>
        /// <param name="allowOverwrite">If TRUE existing contact data can be overwritten otherwise only empty properties can be filled</param>
        /// <param name="contactID">Contact ID</param>
        public static void UpdateContactFromExternalData(BaseInfo source, bool allowOverwrite, int contactID)
        {
            ContactInfo ci = GetContactInfo(contactID);
            if (ci != null)
            {
                UpdateContactFromExternalData(source, allowOverwrite, ci);
            }
        }


        /// <summary>
        /// Updates contact information from record submitted through BizForm module - this requires form fields to be mapped to contact.
        /// </summary>
        /// <param name="source">Data class object with source data and mapping definition</param>
        /// <param name="allowOverwrite">If TRUE existing contact data can be overwritten otherwise only empty properties can be filled</param>
        /// <param name="contact">Optional. If null then current contact will be updated.</param>
        public static void UpdateContactFromExternalData(BaseInfo source, bool allowOverwrite, ContactInfo contact)
        {
            // Ensure contact
            if (contact == null)
            {
                contact = ContactManagementContext.CurrentContact;
            }

            if ((source == null) || (contact == null))
            {
                return;
            }

            // Get source data class
            DataClassInfo classInfo = DataClassInfoProvider.GetDataClassInfo(source.TypeInfo.ObjectClassName);
            if ((classInfo == null) || (string.IsNullOrEmpty(classInfo.ClassContactMapping)))
            {
                return;
            }

            // Prepare form info based on mapping data
            FormInfo mapInfo = new FormInfo(classInfo.ClassContactMapping);
            if (mapInfo.ItemsList.Count > 0)
            {
                // Get all mapped fields
                var fields = mapInfo.GetFields(true, true);

                try
                {
                    // Name property contains a column of contact object
                    // and MappedToField property contains form field mapped to the contact column
                    foreach (FormFieldInfo ffi in fields)
                    {
                        object value;

                        if (source.TryGetValue(ffi.MappedToField, out value))
                        {
                            // Fill empty properties, overwrite existing values or assign new last name for anonymous contact
                            if (!DataHelper.IsEmpty(value) && ContactFieldCanBeModified(allowOverwrite, contact, ffi))
                            {
                                // Country and state data may be returned either as integer or as string
                                // First and last names may come from Customer object 
                                switch ((ffi.Name ?? "").ToLowerInvariant())
                                {
                                    case "contactcountryid":
                                        value = GetCountryID(value);
                                        break;

                                    case "contactstateid":
                                        value = GetStateID(value);
                                        break;

                                    case "contactlastname":
                                        value = GetTruncatedName(value.ToString());
                                        break;

                                    case "contactfirstname":
                                        value = GetTruncatedName(value.ToString());
                                        break;
                                }

                                // Set new value
                                contact.SetValue(ffi.Name, value);
                            }
                        }
                    }

                    // Update the contact
                    SetContactInfo(contact);
                }
                catch (Exception ex)
                {
                    // Log exception
                    EventLogProvider.LogException("ContactInfoProvider", "UPDATECONTACT", ex);
                }
            }
        }


        /// <summary>
        /// Updates contact information from set of data.
        /// </summary>
        /// <param name="contactData">Set of data</param>
        /// <param name="allowOverwrite">Allows overwrite existing contact information</param>
        /// <param name="contactId">Updated contact ID</param>
        public static void UpdateContactFromExternalSource(Dictionary<string, object> contactData, bool? allowOverwrite, int contactId)
        {
            if ((contactData != null) && (contactData.Count > 0))
            {
                ContactInfo updatedContact = GetContact(contactId);
                if (updatedContact != null)
                {
                    foreach (KeyValuePair<string, object> entry in contactData)
                    {
                        UpdateContact(updatedContact, entry.Key, entry.Value, allowOverwrite);
                    }

                    SetContactInfo(updatedContact);
                }
            }
        }


        /// <summary>
        /// Updates single contact information.
        /// </summary>
        /// <param name="updatedContact">Contact being updated</param>
        /// <param name="key">Property of contact being updated</param>
        /// <param name="value">New value of the contact</param>
        /// <param name="allowOverwrite">Indicates if existing value can be overwritten</param>
        private static void UpdateContact(ContactInfo updatedContact, string key, object value, bool? allowOverwrite)
        {
            bool setData = ValidationHelper.GetBoolean(allowOverwrite, false);
            if (!setData)
            {
                setData = updatedContact.GetValue(key) == null;

                if (key == "ContactLastName")
                {
                    setData = updatedContact.ContactLastName.StartsWith(ContactHelper.ANONYMOUS, StringComparison.OrdinalIgnoreCase);
                }
            }

            if (setData)
            {
                updatedContact.SetValue(key, value);
            }
        }


        /// <summary>
        /// Helper method that returns country ID based on input value.
        /// </summary>
        /// <param name="value">Contains either int or string value (country ID or name)</param>
        private static int? GetCountryID(object value)
        {
            if (value is int)
            {
                var countryID = (int)value;

                // Get country by ID to check its existence
                if (CountryInfoProvider.GetCountryInfo(countryID) != null)
                {
                    return countryID;
                }

                return null;
            }

            if (value is string)
            {
                string countryName = ValidationHelper.GetString(value, string.Empty);
                if (!string.IsNullOrEmpty(countryName) && countryName.Contains(";"))
                {
                    // Get country name if value is in form '<CountryName>;<StateName>'
                    countryName = countryName.Remove(countryName.IndexOf(";", StringComparison.Ordinal));
                }

                // Get country object by code name
                CountryInfo country = CountryInfoProvider.GetCountryInfo(countryName);
                if (country != null)
                {
                    return country.CountryID;
                }
            }

            return null;
        }


        /// <summary>
        /// Helper method that returns state ID based on input value.
        /// </summary>
        /// <param name="value">Contains either int or string value (state ID or name)</param>
        private static int? GetStateID(object value)
        {
            if (value is int)
            {
                var stateID = (int)value;

                // Get state by ID to check its existence
                if (StateInfoProvider.GetStateInfo(stateID) != null)
                {
                    return stateID;
                }

                return null;
            }

            if (value is string)
            {
                string stateName = ValidationHelper.GetString(value, string.Empty);
                if (!string.IsNullOrEmpty(stateName) && stateName.Contains(";"))
                {
                    // Get state name if value is in form '<CountryName>;<StateName>'
                    stateName = stateName.Substring(stateName.IndexOf(";", StringComparison.Ordinal) + 1);
                }

                // Get state object by code name
                StateInfo state = StateInfoProvider.GetStateInfo(stateName);
                if (state != null)
                {
                    return state.StateID;
                }
            }

            return null;
        }


        /// <summary>
        /// Incerements number of bounces by one for specified contact.
        /// </summary>
        /// <param name="contactId">Contact ID</param>
        protected virtual void AddContactBounceInternal(int contactId)
        {
            ContactInfo contact = GetContactInfo(contactId);
            if (contact != null)
            {
                contact.ContactBounces++;
                SetContactInfo(contact);
            }
        }


        /// <summary>
        /// Increments number of bounces by one for all contacts specified by e-mail address.
        /// </summary>
        /// <param name="email">E-mail address</param>
        protected virtual void AddContactBounceByEmailInternal(string email)
        {
            var contacts = GetContacts().WhereEquals("ContactEmail", email)
                                        .Column("ContactID");
            
            foreach (ContactInfo contact in contacts)
            {
                AddContactBounce(contact.ContactID);
            }
        }


        /// <summary>
        /// Removes given persona from contacts. Sets column ContactPersonaID to NULL on contacts where this column equals to given persona ID.
        /// </summary>
        /// <param name="personaID">This persona will be removed from contacts</param>
        protected virtual void RemovePersonaFromContactsInternal(int personaID)
        {
            var parameters = new QueryDataParameters
            {
                { "@PersonaID", personaID }
            };

            UpdateData("ContactPersonaID = NULL", parameters, "ContactPersonaID = @PersonaID");
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Gets contact from context or by ID.
        /// </summary>
        private static ContactInfo GetContact(int contactID)
        {
            if (contactID > 0)
            {
                return GetContactInfo(contactID);
            }
            return ContactManagementContext.GetCurrentContact();
        }


        /// <summary>
        /// Log contact to ContactChangeQueue.
        /// </summary>
        /// <param name="contactID">ID of the contact</param>
        /// <param name="changedProperties">Changes to mark</param>
        /// <param name="wasMerged">Is it merged or spitted contact</param>
        /// <param name="newContact">Is it new contact</param>
        private void LogContactChange(int contactID, List<string> changedProperties, bool wasMerged = false, bool newContact = false)
        {
            var changeData = new ContactChangeData
            {
                ContactID = contactID,
                ChangedColumns = (newContact || wasMerged) ? null : changedProperties,
                ContactIsNew = newContact,
                ContactWasMerged = wasMerged,
            };

            Service.Resolve<IContactChangeRepository>().Save(changeData);
        }


        /// <summary>
        /// Sets campaign, geolocation and creation time to a new contact
        /// </summary>
        private void InitNewContact(ContactInfo contact)
        {
            contact.ContactCreated = Service.Resolve<IDateTimeNowService>().GetDateTimeNow();

            var campaignCode = Service.Resolve<ICampaignService>().CampaignCode;
            if (!string.IsNullOrEmpty(campaignCode))
            {
                var campaign = CampaignInfoProvider.GetCampaignByUTMCode(campaignCode, SiteContext.CurrentSiteName);
                contact.ContactCampaign = (campaign != null) ? campaign.CampaignName : contact.ContactCampaign;
            }

            // Update profile based on geo-location
            if (!contact.ContactCreatedInAdministration)
            {
                var ipHelper = new GeoLocationContactHelper(contact, SiteContext.CurrentSiteName);

                ipHelper.UpdateContactLocation(false);
            }
        }


        /// <summary>
        /// Returns name trimmed to 100 chars. If the string is shorter, returns the same string.
        /// </summary>
        private static string GetTruncatedName(string name)
        {
            return name.Length > 100 ? name.Substring(0, 100) : name;
        }


        /// <summary>
        /// Returns true if contact's field can be modified.
        /// </summary>
        private static bool ContactFieldCanBeModified(bool allowOverwrite, ContactInfo contact, FormFieldInfo ffi)
        {
            return string.IsNullOrEmpty(contact.GetStringValue(ffi.Name, null))
                   || allowOverwrite
                   || (contact.ContactLastName.StartsWith(ContactHelper.ANONYMOUS, StringComparison.OrdinalIgnoreCase) && CMSString.Equals(ffi.Name, "contactlastname", true));
        }
        #endregion
    }
}