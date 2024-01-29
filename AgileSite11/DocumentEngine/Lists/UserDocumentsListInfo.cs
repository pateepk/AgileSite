using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.DocumentEngine;

[assembly: RegisterObjectType(typeof(UserDocumentsListInfo), "cms.userdocumentslist")]

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Defines the listing info for user documents
    /// </summary>
    public class UserDocumentsListInfo : AbstractInfo<UserDocumentsListInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.userdocumentslist";


        /// <summary>
        /// Type information (Not complete - some information is missing).
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = ObjectTypeInfo.CreateListingObjectTypeInfo(OBJECT_TYPE, TreeNode.TYPEINFO);

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty UserDocumentsListInfo object.
        /// </summary>
        public UserDocumentsListInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new UserDocumentsListInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public UserDocumentsListInfo(DataRow dr)
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
                "DocumentName",
                "NodeSiteID",
                "NodeID",
                "ClassName",
                "ClassDisplayName",
                "DocumentNamePath",
                "DocumentModifiedWhen",
                "DocumentCulture",
                "CultureName",
                "UserID1",
                "UserID2",
                "UserID3",
                "DocumentWorkflowStepID",
                "NodeAliasPath",
                "Type"
                );
        }


        /// <summary>
        /// Gets the data query for this object type
        /// </summary>
        protected override IDataQuery GetDataQueryInternal()
        {
            return new DataQuery("cms.document", "selectalluserdocuments");
        }

        #endregion
    }
}