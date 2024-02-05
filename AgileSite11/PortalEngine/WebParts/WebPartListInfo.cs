using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.PortalEngine;

[assembly: RegisterObjectType(typeof(WebPartListInfo), "cms.webpartlist")]

namespace CMS.PortalEngine
{
    internal class WebPartListInfo : AbstractInfo<WebPartListInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.webpartlist";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = ObjectTypeInfo.CreateListingObjectTypeInfo(OBJECT_TYPE, WebPartInfo.TYPEINFO);

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty WebPartCategoryListInfo object.
        /// </summary>
        public WebPartListInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new WebPartCategoryListInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public WebPartListInfo(DataRow dr)
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
                "ObjectID",
                "CodeName",
                "DisplayName",
                "ParentID",
                "GUID",
                "LastModified",
                "CategoryImagePath",
                "ObjectPath",
                "ObjectLevel",
                "CategoryChildCount",
                "CategoryWebPartChildCount",
                "CompleteChildCount",
                "WebPartParentID",
                "WebPartFileName",
                "WebPartGUID",
                "WebPartType",
                "WebPartDescription",
                "ObjectType"
                );
        }


        /// <summary>
        /// Gets the data query for this object type
        /// </summary>
        protected override IDataQuery GetDataQueryInternal()
        {
            return new DataQuery("cms.webpartcategory", "selectallview");
        }

        #endregion
    }
}