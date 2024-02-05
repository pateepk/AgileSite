using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.ContactManagement;
using CMS.DataEngine;

[assembly: RegisterObjectType(typeof(AccountContactListInfo), "om.accountcontactlist")]

namespace CMS.ContactManagement
{
    /// <summary>
    /// Account-Contact relation class used for listing.
    /// </summary>
    public class AccountContactListInfo : AbstractInfo<AccountContactListInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "om.accountcontactlist";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = ObjectTypeInfo.CreateListingObjectTypeInfo(OBJECT_TYPE, AccountContactInfo.TYPEINFO);

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty AccountContactListInfo object.
        /// </summary>
        public AccountContactListInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new AccountContactListInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public AccountContactListInfo(DataRow dr)
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
                "ContactEmail",
                "ContactRoleID",
                "AccountID",
                "AccountContactID"
                );
        }


        /// <summary>
        /// Gets the data query for this object type
        /// </summary>
        protected override IDataQuery GetDataQueryInternal()
        {
            return new DataQuery("om.accountcontact", "selectfromcontactview");
        }

        #endregion
    }
}