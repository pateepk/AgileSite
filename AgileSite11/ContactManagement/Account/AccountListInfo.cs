using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.ContactManagement;
using CMS.DataEngine;

[assembly: RegisterObjectType(typeof(AccountListInfo), "om.accountlist")]

namespace CMS.ContactManagement
{
    /// <summary>
    /// "Virtual" object for loading data from contact view.
    /// </summary>
    public class AccountListInfo : AbstractInfo<AccountListInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "om.accountlist";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = ObjectTypeInfo.CreateListingObjectTypeInfo(OBJECT_TYPE, AccountInfo.TYPEINFO);

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty AccountListInfo object.
        /// </summary>
        public AccountListInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new AccountListInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public AccountListInfo(DataRow dr)
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
                    "AccountAddress1",
                    "AccountAddress2",
                    "AccountCity",
                    "AccountZIP",
                    "AccountStateID",
                    "AccountCountryID",
                    "AccountWebSite",
                    "AccountPhone",
                    "AccountEmail",
                    "AccountFax",
                    "AccountPrimaryContactID",
                    "AccountSecondaryContactID",
                    "AccountStatusID",
                    "AccountNotes",
                    "AccountOwnerUserID",
                    "AccountSubsidiaryOfID",
                    "AccountGUID",
                    "AccountLastModified",
                    "AccountCreated",
                    "PrimaryContactFirstName",
                    "PrimaryContactMiddleName",
                    "PrimaryContactLastName",
                    "SecondaryContactFirstName",
                    "SecondaryContactMiddleName",
                    "SecondaryContactLastName",
                    "SubsidiaryOfName",
                    "PrimaryContactFullName",
                    "SecondaryContactFullName",
                    "AccountFullAddress"
                    );
        }


        /// <summary>
        /// Gets the data query for this object type
        /// </summary>
        protected override IDataQuery GetDataQueryInternal()
        {
            return new DataQuery("om.account", "selectfromview");
        }

        #endregion
    }
}