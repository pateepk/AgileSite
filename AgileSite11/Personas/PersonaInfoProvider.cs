using System;
using System.Collections.Generic;
using System.Linq;

using CMS.ContactManagement;
using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.Personas
{
    /// <summary>
    /// Class providing PersonaInfo management.
    /// </summary>
    public class PersonaInfoProvider : AbstractInfoProvider<PersonaInfo, PersonaInfoProvider>
    {
        #region "Constructor"

        /// <summary>
        /// Constructor which enables caching by ID.
        /// </summary>
        public PersonaInfoProvider()
            : base(PersonaInfo.TYPEINFO, new HashtableSettings()
            {
                ID = true,
                GUID = true,
                Load = LoadHashtableEnum.All,
            })
        {
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Returns a query for all the PersonaInfo objects.
        /// </summary>
        public static ObjectQuery<PersonaInfo> GetPersonas()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Sets (updates or inserts) specified PersonaInfo.
        /// </summary>
        /// <param name="infoObj">PersonaInfo to be set</param>
        public static void SetPersonaInfo(PersonaInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified PersonaInfo.
        /// </summary>
        /// <param name="infoObj">PersonaInfo to be deleted</param>
        public static void DeletePersonaInfo(PersonaInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Returns PersonaInfo with specified ID.
        /// </summary>
        /// <param name="id">PersonaInfo ID</param>
        public static PersonaInfo GetPersonaInfoById(int id)
        {
            return ProviderObject.GetInfoById(id);
        }


        /// <summary>
        /// Returns PersonaInfo with specified code name.
        /// </summary>
        /// <param name="codeName">Persona code name</param>
        public static PersonaInfo GetPersonaInfoByCodeName(string codeName)
        {
            return ProviderObject.GetInfoByCodeName(codeName);
        }


        /// <summary>
        /// Returns PersonaInfo by specifying underlying score ID.
        /// </summary>
        /// <param name="id">Score ID</param>
        public static PersonaInfo GetPersonaInfoByScoreId(int id)
        {
            return ProviderObject.GetPersonaInfoByScoreIdInternal(id);
        }


        #region "Reevaluation methods"

        /// <summary>
        /// Reevaluates contact, returning ID of persona contact should be assigned to. 
        /// </summary>
        /// <remarks>
        /// This method shouldn't be called directly - only from the ContactPersonaEvaluator.
        /// </remarks>
        /// <param name="contact">Contact being reevaluated</param>
        /// <exception cref="ArgumentNullException"><paramref name="contact"/> is null</exception>
        /// <returns>ID of persona the contact should ve assigned to, null if contact isn't assigned to any persona.</returns>
        public static int? ReevaluateContact(ContactInfo contact)
        {
            return ProviderObject.ReevaluateContactInternal(contact);
        }


        /// <summary>
        /// Reevaluates given contacts.
        /// </summary>
        /// <remarks>
        /// This method shouldn't be called directly - only from the ContactPersonaEvaluator.
        /// </remarks>
        /// <param name="contactIds">Contacts being reevaluated</param>
        /// <exception cref="ArgumentNullException"><paramref name="contactIds"/> is null</exception>
        public static void ReevaluateContacts(IEnumerable<int> contactIds)
        {
            ProviderObject.ReevaluateContactsInternal(contactIds);
        }


        /// <summary>
        /// Reevaluates persona for all contacts. 
        /// </summary>
        /// <remarks>
        /// This method shouldn't be called directly - only from the ContactPersonaEvaluator.
        /// </remarks>
        public static void ReevaluateAllContacts()
        {
            ProviderObject.ReevaluateAllContactsInternal();
        }

        #endregion

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(PersonaInfo info)
        {
            // When setting new persona, create new scoring for it. When new persona was created by cloning scoreID is already set.
            if ((info.PersonaID == 0) && (info.PersonaScoreID == 0))
            {
                PersonasFactory.GetPersonaChangesPropagator().PropagatePersonaCreation(info);
            }

            // Get changed columns and original value of persona points limit in advance before object is actually updated
            List<string> changedColumns = null;
            int originalValue = ValidationHelper.GetInteger(info.GetOriginalValue("PersonaPointsThreshold"), 0);

            if ((info.PersonaID) != 0 && (info.PersonaScoreID != 0))
            {
                changedColumns = info.ChangedColumns();
            }

            base.SetInfo(info);

            if (changedColumns != null)
            {
                PersonasFactory.GetPersonaChangesPropagator().PropagatePersonaChanges(changedColumns, originalValue, info);
            }
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(PersonaInfo info)
        {
            base.DeleteInfo(info);

            PersonasFactory.GetPersonaChangesPropagator().PropagatePersonaDeletion(info);
        }


        /// <summary>
        /// Returns PersonaInfo by specifying underlying score ID.
        /// </summary>
        /// <param name="id">Score ID</param>
        protected virtual PersonaInfo GetPersonaInfoByScoreIdInternal(int id)
        {
            return GetPersonas().WhereEquals("PersonaScoreID", id).FirstObject;
        }


        /// <summary>
        /// Reevaluates contact, returning ID of persona contact should be assigned to. 
        /// </summary>
        /// <remarks>
        /// This method shouldn't be called directly - only from the ContactPersonaEvaluator.
        /// </remarks>
        /// <param name="contact">Contact being reevaluated</param>
        /// <exception cref="ArgumentNullException"><paramref name="contact"/> is null</exception>
        /// <returns>ID of persona the contact should be assigned to, null if contact isn't assigned to any persona</returns>
        protected virtual int? ReevaluateContactInternal(ContactInfo contact)
        {
            if (contact == null)
            {
                throw new ArgumentNullException("contact");
            }

            var query = ScoreContactRuleInfoProvider
                        .GetScoreContactRules()
                        .Columns("PersonaID", "PersonaPointsThreshold")
                        .AddColumn(new AggregatedColumn(AggregationType.Sum, "Value").As("Sum"))
                        .Source(s => s.LeftJoin<PersonaInfo>("ScoreID", "PersonaScoreID"))
                        .WhereEquals("PersonaEnabled", 1)
                        .WhereEquals("ContactID", contact.ContactID)
                        .Where(w => w.WhereNull("Expiration")
                            .Or()
                        .Where("Expiration", QueryOperator.LessOrEquals, DateTime.Now))
                        .GroupBy("PersonaID", "PersonaPointsThreshold")
                        .AsNested()
                            .Columns("PersonaID")
                            .AddColumn(new QueryColumn("1.0 * Sum / PersonaPointsThreshold").As("Quotient"))
                            .AsNested()
                                .TopN(1)
                                .Columns("PersonaID")
                                .Where("Quotient", QueryOperator.LargerOrEquals, 1)
                                .OrderByDescending("Quotient");

            var personaID = query.GetScalarResult<int>();
            if (personaID == 0)
            {
                return null;
            }
            return personaID;
        }


        /// <summary>
        /// Reevaluates given contacts.
        /// </summary>
        /// <remarks>
        /// This method shouldn't be called directly - only from the ContactPersonaEvaluator.
        /// </remarks>
        /// <param name="contactIds">Contacts being reevaluated</param>
        /// <exception cref="ArgumentNullException"><paramref name="contactIds"/> is null</exception>
        protected virtual void ReevaluateContactsInternal(IEnumerable<int> contactIds)
        {
            if (contactIds == null)
            {
                throw new ArgumentNullException(nameof(contactIds));
            }

            var contactIdsList = contactIds.ToList();

            var param = new QueryDataParameters();

            if (contactIdsList.Count > 0)
            {
                var contactIDsIntTable = SqlHelper.BuildOrderedIntTable(contactIdsList);
                param.Add("@ContactIDs", contactIDsIntTable, SqlHelper.OrderedIntegerTableType);
            }

            ConnectionHelper.ExecuteNonQuery("Personas.Persona.ReevaluateAllContacts", param);

            RefreshCacheForContactsWithChangedPersona(contactIdsList);
        }


        /// <summary>
        /// Reevaluates persona for all contacts. 
        /// </summary>
        /// <remarks>
        /// This method shouldn't be called directly - only from the ContactPersonaEvaluator.
        /// </remarks>
        protected virtual void ReevaluateAllContactsInternal()
        {
            ConnectionHelper.ExecuteNonQuery("Personas.Persona.ReevaluateAllContacts");
            ProviderHelper.ClearHashtables(ContactInfo.OBJECT_TYPE, true);
        }


        /// <summary>
        /// Invalidates all reevaluated contacts and refreshes cache for them.
        /// </summary>
        /// <param name="contactIdsList">List of affected contacts</param>
        private static void RefreshCacheForContactsWithChangedPersona(IEnumerable<int> contactIdsList)
        {
            foreach (var contactId in contactIdsList)
            {
                ContactInfoProvider.ProviderObject.TypeInfo.ObjectInvalidated(contactId);
            }
        }

        #endregion
    }
}