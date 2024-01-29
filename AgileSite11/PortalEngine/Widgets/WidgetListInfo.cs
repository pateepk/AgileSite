using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.PortalEngine;

[assembly: RegisterObjectType(typeof(WidgetListInfo), "cms.widgetlist")]

namespace CMS.PortalEngine
{
    internal class WidgetListInfo : AbstractInfo<WidgetListInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.widgetlist";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = ObjectTypeInfo.CreateListingObjectTypeInfo(OBJECT_TYPE, WidgetInfo.TYPEINFO);

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty WidgetCategoryListInfo object.
        /// </summary>
        public WidgetListInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new WidgetCategoryListInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public WidgetListInfo(DataRow dr)
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
                "WidgetCategoryImagePath",
                "ObjectPath",
                "ObjectLevel",
                "WidgetCategoryChildCount",
                "WidgetCategoryWidgetChildCount",
                "CompleteChildCount",
                "WidgetWebPartID",
                "WidgetSecurity",
                "WidgetForGroup",
                "WidgetForInline",
                "WidgetForUser",
                "WidgetForEditor",
                "WidgetForDashboard",
                "WidgetGUID",
                "ObjectType"
                );
        }


        /// <summary>
        /// Gets the data query for this object type
        /// </summary>
        protected override IDataQuery GetDataQueryInternal()
        {
            return new DataQuery("cms.widgetcategory", "selectallview");
        }

        #endregion
    }
}