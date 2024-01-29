using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.ContactManagement;
using CMS.DataEngine;

[assembly: RegisterObjectType(typeof(ContactGroupContactListInfo), "om.contactgroupcontactlist")]

namespace CMS.ContactManagement
{
    /// <summary>
    /// Contact group contacts list
    /// </summary>
    public class ContactGroupContactListInfo : AbstractInfo<ContactGroupContactListInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "om.contactgroupcontactlist";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = ObjectTypeInfo.CreateListingObjectTypeInfo(OBJECT_TYPE, ContactGroupMemberInfo.TYPEINFOCONTACT);

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty ContactGroupContactListInfo object.
        /// </summary>
        public ContactGroupContactListInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new ContactGroupContactListInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public ContactGroupContactListInfo(DataRow dr)
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
            return CombineColumnNames(
                    "ContactID",
                    "ContactFirstName",
                    "ContactMiddleName",
                    "ContactLastName",
                    "ContactJobTitle",
                    "ContactAddress1",
                    "ContactCity",
                    "ContactZIP",
                    "ContactMobilePhone",
                    "ContactBusinessPhone",
                    "ContactEmail",
                    "ContactBirthday",
                    "ContactGender",
                    "ContactNotes",
                    "ContactLastModified",
                    "ContactCreated",
                    "ContactGroupMemberContactGroupID",
                    "ContactGroupMemberFromCondition",
                    "ContactGroupMemberFromAccount",
                    "ContactGroupMemberFromManual"
                    );
        }


        /// <summary>
        /// Gets the data query for this object type
        /// </summary>
        protected override IDataQuery GetDataQueryInternal()
        {
            return new DataQuery("om.contactgroupmember", "selectfromcontactview");
        }

        #endregion
    }
}