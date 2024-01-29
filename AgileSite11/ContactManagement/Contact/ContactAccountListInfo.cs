using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.ContactManagement;
using CMS.DataEngine;

[assembly: RegisterObjectType(typeof(ContactAccountListInfo), "om.contactaccountlist")]

namespace CMS.ContactManagement
{
    /// <summary>
    /// Contact-Account relation table.
    /// </summary>
    public class ContactAccountListInfo : AbstractInfo<ContactAccountListInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "om.contactaccountlist";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = ObjectTypeInfo.CreateListingObjectTypeInfo(OBJECT_TYPE, AccountContactInfo.TYPEINFO);

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty ContactAccountListInfo object.
        /// </summary>
        public ContactAccountListInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new ContactAccountListInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public ContactAccountListInfo(DataRow dr)
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
                    "AccountID",
                    "AccountName",
                    "ContactRoleID",
                    "ContactID",
                    "AccountContactID"
                    );
        }


        /// <summary>
        /// Gets the data query for this object type
        /// </summary>
        protected override IDataQuery GetDataQueryInternal()
        {
            return new DataQuery("om.accountcontact", "selectfromaccountview");
        }

        #endregion
    }
}