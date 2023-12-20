using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.ContactManagement;
using CMS.DataEngine;

[assembly: RegisterObjectType(typeof(ContactMembershipCustomerListInfo), "om.membershipcustomerlist")]

namespace CMS.ContactManagement
{
    /// <summary>
    /// "Virtual" object for loading contact-customer relation data.
    /// </summary>
    public class ContactMembershipCustomerListInfo : AbstractInfo<ContactMembershipCustomerListInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "om.membershipcustomerlist";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = ObjectTypeInfo.CreateListingObjectTypeInfo(OBJECT_TYPE, ContactMembershipInfo.TYPEINFOCUSTOMER);

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty ContactMembershipCustomerListInfo object.
        /// </summary>
        public ContactMembershipCustomerListInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new ContactMembershipCustomerListInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public ContactMembershipCustomerListInfo(DataRow dr)
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
                    "CustomerFirstName",
                    "CustomerLastName",
                    "CustomerEmail"
                    );
        }


        /// <summary>
        /// Gets the data query for this object type
        /// </summary>
        protected override IDataQuery GetDataQueryInternal()
        {
            return new DataQuery("om.membership", "selectcustomers");
        }

        #endregion
    }
}