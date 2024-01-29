using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Membership;

[assembly: RegisterObjectType(typeof(MembershipListInfo), "cms.membershiplist")]

namespace CMS.Membership
{
    internal class MembershipListInfo : AbstractInfo<MembershipListInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.membershiplist";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = ObjectTypeInfo.CreateListingObjectTypeInfo(OBJECT_TYPE, MembershipInfo.TYPEINFO);

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty MembershipListInfo object.
        /// </summary>
        public MembershipListInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new MembershipListInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public MembershipListInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Gets the data query for this object type
        /// </summary>
        protected override IDataQuery GetDataQueryInternal()
        {
            return new DataQuery("cms.membershipuser", "selectallwithmembership");
        }

        #endregion
    }
}