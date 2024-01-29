using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Membership;

[assembly: RegisterObjectType(typeof(UserListInfo), "cms.userlist")]

namespace CMS.Membership
{
    internal class UserListInfo : AbstractInfo<UserListInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.userlist";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = ObjectTypeInfo.CreateListingObjectTypeInfo(OBJECT_TYPE, UserInfo.TYPEINFO);

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty UserListInfo object.
        /// </summary>
        public UserListInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new UserListInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public UserListInfo(DataRow dr)
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
            if ((mCombinedColumnNames == null) || TypeInfo.ColumnsInvalidated)
            {
                // View-specific columns
                string[] viewColumns = { "AvatarID", "AvatarFileName", "AvatarGUID" };

                UserInfo userInfo = new UserInfo();

                var names = CombineColumnNames(viewColumns, userInfo.ColumnNames);

                names.Remove("UserPassword");

                mCombinedColumnNames = names;
            }

            return mCombinedColumnNames;
        }


        /// <summary>
        /// Gets the data query for this object type
        /// </summary>
        protected override IDataQuery GetDataQueryInternal()
        {
            return new DataQuery("cms.user", QueryName.GENERALSELECT).From("View_CMS_User");
        }

        #endregion
    }
}