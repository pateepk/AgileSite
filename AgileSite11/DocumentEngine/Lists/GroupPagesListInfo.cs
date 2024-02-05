using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.PortalEngine;

[assembly: RegisterObjectType(typeof(GroupPagesListInfo), GroupPagesListInfo.OBJECT_TYPE)]

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Defines the listing info for the group pages.
    /// </summary>
    internal class GroupPagesListInfo : AbstractInfo<GroupPagesListInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = WidgetInfo.GROUPPAGESLIST;


        /// <summary>
        /// Type information
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = GetTypeInfo();

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty UserDocumentsListInfo object.
        /// </summary>
        public GroupPagesListInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new UserDocumentsListInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public GroupPagesListInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "Methods"

        private static ObjectTypeInfo GetTypeInfo()
        {
            var typeInfo = ObjectTypeInfo.CreateListingObjectTypeInfo(OBJECT_TYPE, TreeNode.TYPEINFO);
            typeInfo.TypeCondition = new TypeCondition().WhereIsNotNull("NodeGroupID");

            return typeInfo;
        }


        /// <summary>
        /// Gets the data query for this object type
        /// </summary>
        protected override IDataQuery GetDataQueryInternal()
        {
            return new DataQuery().From(SystemViewNames.View_CMS_Tree_Joined);
        }

        #endregion
    }
}
