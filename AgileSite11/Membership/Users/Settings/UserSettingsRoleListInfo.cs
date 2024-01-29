using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Membership;

[assembly: RegisterObjectType(typeof(UserSettingsRoleListInfo), "cms.usersettingsrolelist")]

namespace CMS.Membership
{
    internal class UserSettingsRoleListInfo : AbstractInfo<UserSettingsRoleListInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.usersettingsrolelist";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = GetTypeInfo();

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty UserSettingsRoleListInfo object.
        /// </summary>
        public UserSettingsRoleListInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new UserSettingsRoleListInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public UserSettingsRoleListInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "Methods"

        private static ObjectTypeInfo GetTypeInfo()
        {
            var typeInfo = ObjectTypeInfo.CreateListingObjectTypeInfo(OBJECT_TYPE, UserRoleInfo.TYPEINFO);
            typeInfo.ModuleName = "cms.users";

            return typeInfo;
        }


        /// <summary>
        /// Gets the default list of column names for this class
        /// </summary>
        protected override List<string> GetColumnNames()
        {
            return CombineColumnNames(
                "UserID",
                "RoleID",
                "UserName",
                "FullName",
                "Email",
                "RoleName",
                "RoleDisplayName",
                "RoleDescription",
                "SiteID",
                "SiteName"
                );
        }


        /// <summary>
        /// Gets the data query for this object type
        /// </summary>
        protected override IDataQuery GetDataQueryInternal()
        {
            return new DataQuery("cms.user", "selectallsettingsroles");
        }

        #endregion
    }
}