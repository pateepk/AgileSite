using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.SiteProvider;

[assembly: RegisterObjectType(typeof(SiteListInfo), "cms.sitelist")]

namespace CMS.SiteProvider
{
    /// <summary>
    /// Represents entry for site listing
    /// </summary>
    internal class SiteListInfo : AbstractInfo<SiteListInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.sitelist";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = ObjectTypeInfo.CreateListingObjectTypeInfo(OBJECT_TYPE, SiteInfo.TYPEINFO);

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty SiteListInfo object.
        /// </summary>
        public SiteListInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new SiteListInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public SiteListInfo(DataRow dr)
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
                "SiteID",
                "SiteName",
                "SiteDisplayName",
                "SiteDescription",
                "SiteStatus",
                "SiteDomainName",
                "SiteDefaultStylesheetID",
                "SiteDefaultVisitorCulture",
                "SiteDefaultEditorStylesheet",
                "SiteGUID",
                "SiteLastModified",
                "Documents",
                "SiteIsOffline",
                "SiteIsContentOnly"
                );
        }


        /// <summary>
        /// Gets the data query for this object type
        /// </summary>
        protected override IDataQuery GetDataQueryInternal()
        {
            return new DataQuery("cms.site", "selectsitelist");
        }

        #endregion
    }
}