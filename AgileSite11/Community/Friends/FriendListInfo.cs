using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Membership;
using CMS.DataEngine;
using CMS.Community;

[assembly: RegisterObjectType(typeof(FriendListInfo), "community.friendlist")]

namespace CMS.Community
{
    internal class FriendListInfo : AbstractInfo<FriendListInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "community.friendlist";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = ObjectTypeInfo.CreateListingObjectTypeInfo(OBJECT_TYPE, FriendInfo.TYPEINFO);

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty FriendListInfo object.
        /// </summary>
        public FriendListInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new FriendListInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public FriendListInfo(DataRow dr)
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
                string[] viewColumns = new string[] { "AvatarID", "AvatarFileName", "AvatarGUID" };

                // Add columns from base object type
                FriendInfo friendInfo = new FriendInfo();
                UserInfo userInfo = new UserInfo();

                mCombinedColumnNames = CombineColumnNames(viewColumns, friendInfo.ColumnNames, userInfo.ColumnNames);
                mCombinedColumnNames.Remove("UserPassword");
            }
            return mCombinedColumnNames;
        }


        /// <summary>
        /// Gets the data query for this object type
        /// </summary>
        protected override IDataQuery GetDataQueryInternal()
        {
            return new DataQuery("community.friend", "selectfriendships");
        }

        #endregion
    }
}