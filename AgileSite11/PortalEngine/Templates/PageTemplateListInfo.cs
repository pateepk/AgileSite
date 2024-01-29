using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.PortalEngine;

[assembly: RegisterObjectType(typeof(PageTemplateListInfo), "cms.pagetemplatelist")]

namespace CMS.PortalEngine
{
    internal class PageTemplateListInfo : AbstractInfo<PageTemplateListInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.pagetemplatelist";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = ObjectTypeInfo.CreateListingObjectTypeInfo(OBJECT_TYPE, PageTemplateInfo.TYPEINFO);

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty PageTemplateCategoryListInfo object.
        /// </summary>
        public PageTemplateListInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new PageTemplateCategoryListInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public PageTemplateListInfo(DataRow dr)
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
                "CategoryTemplateChildCount",
                "CompleteChildCount",
                "ObjectType",
                "Parameter",
                "PageTemplateForAllPages",
                "PageTemplateType"
                );
        }


        /// <summary>
        /// Gets the data query for this object type
        /// </summary>
        protected override IDataQuery GetDataQueryInternal()
        {
            return new DataQuery("cms.pagetemplatecategory", "selectallview");
        }

        #endregion
    }
}