using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Membership;

[assembly: RegisterObjectType(typeof(UserRoleListInfo), UserRoleListInfo.OBJECT_TYPE)]

namespace CMS.Membership
{
    internal class UserRoleListInfo : AbstractInfo<UserRoleListInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.userrolelist";
        

        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = ObjectTypeInfo.CreateListingObjectTypeInfo(OBJECT_TYPE, UserRoleInfo.TYPEINFO);

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty MembershipUserInfo object.
        /// </summary>
        public UserRoleListInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new MembershipUserInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public UserRoleListInfo(DataRow dr)
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
            return new DataQuery("cms.userrole", "selectuserrole");
        }

        #endregion
    }
}