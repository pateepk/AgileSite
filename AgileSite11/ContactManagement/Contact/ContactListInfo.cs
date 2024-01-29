using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Base;
using CMS.ContactManagement;
using CMS.DataEngine;

[assembly: RegisterObjectType(typeof(ContactListInfo), "om.contactlist")]

namespace CMS.ContactManagement
{
    /// <summary>
    /// "Virtual" object for loading data from contact view.
    /// </summary>
    public class ContactListInfo : AbstractInfo<ContactListInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "om.contactlist";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = ObjectTypeInfo.CreateListingObjectTypeInfo(OBJECT_TYPE, ContactInfo.TYPEINFO);

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty ContactListInfo object.
        /// </summary>
        public ContactListInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new ContactListInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public ContactListInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Gets the default list of column names for this class
        /// </summary>
        protected override List<string> GetColumnNames()
        {
            if (mCombinedColumnNames == null)
            {
                mCombinedColumnNames = new List<string>
                {
                    "ContactID",
                    "ContactFirstName",
                    "ContactMiddleName",
                    "ContactLastName",
                    "ContactJobTitle",
                    "ContactAddress1",
                    "ContactCity",
                    "ContactZIP",
                    "ContactStateID",
                    "ContactCountryID",
                    "ContactMobilePhone",
                    "ContactBusinessPhone",
                    "ContactEmail",
                    "ContactBirthday",
                    "ContactGender",
                    "ContactStatusID",
                    "ContactNotes",
                    "ContactOwnerUserID",
                    "ContactMonitored",
                    "ContactGUID",
                    "ContactLastModified",
                    "ContactCreated",
                    "ContactBounces",
                    "ContactFullNameJoined",
                    "ContactFullAddressJoined"
                };
            }
            return mCombinedColumnNames;
        }


        /// <summary>
        /// Gets the data query for this object type
        /// </summary>
        protected override IDataQuery GetDataQueryInternal()
        {
            return ContactInfoProvider.GetContacts()
                                      .AddColumn(new QueryColumn("ISNULL(OM_Contact.ContactFirstName, '') + " +
                                                                 "CASE WHEN (NULLIF(OM_Contact.ContactFirstName,'') IS NULL) THEN '' ELSE ' ' END + " +
                                                                 "ISNULL(OM_Contact.ContactMiddleName, '') + " +
                                                                 "CASE WHEN(NULLIF(OM_Contact.ContactMiddleName, '') IS NULL) THEN '' ELSE ' ' END + " +
                                                                 "ISNULL(OM_Contact.ContactLastName, '')").As("ContactFullNameJoined"))
                                      .AddColumn(new QueryColumn("ISNULL(OM_Contact.ContactAddress1,'') + " +
                                                                 "CASE WHEN (NULLIF(OM_Contact.ContactAddress1,'') IS NULL) THEN '' ELSE ', ' END + " +
                                                                 "ISNULL(OM_Contact.ContactCity, '')").As("ContactFullAddressJoined"))
                                      .AsNested();

        }

        #endregion
    }
}