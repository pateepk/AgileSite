using System;
using System.Collections.Generic;
using System.Linq;

using CMS.DataEngine;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Class providing VisitorToContactInfo management.
    /// </summary>
    public class VisitorToContactInfoProvider : AbstractInfoProvider<VisitorToContactInfo, VisitorToContactInfoProvider>
    {
        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        public VisitorToContactInfoProvider() : base(VisitorToContactInfo.TYPEINFO)
        {
        }

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns a query for all the VisitorToContactInfo objects.
        /// </summary>
        public static ObjectQuery<VisitorToContactInfo> GetVisitorToContacts()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns VisitorToContactInfo with specified ID.
        /// </summary>
        /// <param name="id">VisitorToContactInfo ID</param>
        public static VisitorToContactInfo GetVisitorToContactInfo(int id)
        {
            return ProviderObject.GetInfoById(id);
        }


        /// <summary>
        /// Sets (updates or inserts) specified VisitorToContactInfo.
        /// </summary>
        /// <param name="infoObj">VisitorToContactInfo to be set</param>
        public static void SetVisitorToContactInfo(VisitorToContactInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified VisitorToContactInfo.
        /// </summary>
        /// <param name="infoObj">VisitorToContactInfo to be deleted</param>
        public static void DeleteVisitorToContactInfo(VisitorToContactInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes VisitorToContactInfo with specified ID.
        /// </summary>
        /// <param name="id">VisitorToContactInfo ID</param>
        public static void DeleteVisitorToContactInfo(int id)
        {
            VisitorToContactInfo infoObj = GetVisitorToContactInfo(id);
            DeleteVisitorToContactInfo(infoObj);
        }


        /// <summary>
        /// Returns a <see cref="ContactInfo"/> for given <param name="visitor"></param> <see cref="Guid"/>.
        /// </summary>
        public static ContactInfo GetContactForVisitor(Guid visitor)
        {
            if (visitor == Guid.Empty)
            {
                return null;
            }

            return ContactInfoProvider.GetContacts()
                                      .WhereIn("ContactID", GetVisitorToContacts().WhereEquals("VisitorToContactVisitorGUID", visitor)
                                                                                  .Column("VisitorToContactContactID")
                                                                                  .TopN(1))
                                      .TopN(1)
                                      .FirstOrDefault();
        }
        

        #endregion


        #region "Bulk methods"

        /// <summary>
        /// Moves all relations between visitor and the contact from the contact identified by given <paramref name="sourceContactID"/> to the contact identified by <paramref name="targetContactID"/>.
        /// </summary>
        /// <remarks>
        /// This method should be used only in the merging process. Note that there is no consistency check on whether the contacts with given IDs exist or not (nor is the 
        /// foreign key check in DB). Caller of this method should perform all the necessary checks prior to the method invocation.
        /// </remarks>
        /// <param name="sourceContactID">Identifier of the contact the activities are moved from</param>
        /// <param name="targetContactID">Identifier of the contact the activities are moved to</param>
        public static void BulkMoveVisitors(int sourceContactID, int targetContactID)
        {
            var updateDictionary = new Dictionary<string, object>
            {
                {"VisitorToContactContactID", targetContactID}
            };

            var whereCondition = new WhereCondition().WhereEquals("VisitorToContactContactID", sourceContactID);

            ProviderObject.UpdateData(whereCondition, updateDictionary);
        }

        #endregion

        /// <summary>
        /// Creates and sets record for given contact.
        /// </summary>
        public static void CreateVisitorToContactInfo(ContactInfo contact)
        {
            if (contact == null)
            {
                throw new ArgumentNullException(nameof(contact));
            }

            SetVisitorToContactInfo(new VisitorToContactInfo
            {
                VisitorToContactContactID = contact.ContactID,
                VisitorToContactVisitorGUID = contact.ContactGUID,
            });
        }
    }
}