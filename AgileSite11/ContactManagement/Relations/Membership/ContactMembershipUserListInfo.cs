using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.ContactManagement;
using CMS.DataEngine;

[assembly: RegisterObjectType(typeof(ContactMembershipUserListInfo), "om.membershipuserlist")]

namespace CMS.ContactManagement
{
    /// <summary>
    /// "Virtual" object for loading contact-user relation data.
    /// </summary>
    public class ContactMembershipUserListInfo : AbstractInfo<ContactMembershipUserListInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "om.membershipuserlist";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = ObjectTypeInfo.CreateListingObjectTypeInfo(OBJECT_TYPE, ContactMembershipInfo.TYPEINFOUSER);

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty ContactMembershipUserListInfo object.
        /// </summary>
        public ContactMembershipUserListInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new ContactMembershipUserListInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public ContactMembershipUserListInfo(DataRow dr)
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
                "MembershipID",
                "ContactID",
                "UserName",
                "FirstName",
                "LastName",
                "Email"
                );
        }


        /// <summary>
        /// Gets the data query for this object type
        /// </summary>
        protected override IDataQuery GetDataQueryInternal()
        {
            return new DataQuery("om.membership", "selectusers");
        }

        #endregion
    }
}