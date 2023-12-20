using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Reporting;

[assembly: RegisterObjectType(typeof(ReportListInfo), "reporting.reportlist")]

namespace CMS.Reporting
{
    internal class ReportListInfo : AbstractInfo<ReportListInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "reporting.reportlist";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = ObjectTypeInfo.CreateListingObjectTypeInfo(OBJECT_TYPE, ReportInfo.TYPEINFO);

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty ReportCategoryListInfo object.
        /// </summary>
        public ReportListInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new ReportCategoryListInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public ReportListInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "Methods"

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
                "CategoryReportChildCount",
                "CompleteChildCount",
                "ReportLayout",
                "ReportParameters",
                "ReportAccess",
                "ObjectType"
                );
        }


        /// <summary>
        /// Gets the data query for this object type
        /// </summary>
        protected override IDataQuery GetDataQueryInternal()
        {
            return new DataQuery("reporting.reportcategory", "selectcategoryreports");
        }

        #endregion
    }
}