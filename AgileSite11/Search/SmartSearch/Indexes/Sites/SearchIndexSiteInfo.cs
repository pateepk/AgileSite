using System;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Search;

[assembly: RegisterObjectType(typeof(SearchIndexSiteInfo), SearchIndexSiteInfo.OBJECT_TYPE)]

namespace CMS.Search
{
    /// <summary>
    /// Search index site.
    /// </summary>
    public class SearchIndexSiteInfo : AbstractInfo<SearchIndexSiteInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.searchindexsite";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(SearchIndexSiteInfoProvider), OBJECT_TYPE, "CMS.SearchIndexSite", null, null, null, null, null, null, "IndexSiteID", "IndexID", SearchIndexInfo.OBJECT_TYPE)
        {
            TouchCacheDependencies = true,
            SupportsVersioning = false,
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Index ID.
        /// </summary>
        public virtual int IndexID
        {
            get
            {
                return GetIntegerValue("IndexID", 0);
            }
            set
            {
                SetValue("IndexID", value);
            }
        }


        /// <summary>
        /// Site ID of index.
        /// </summary>
        public virtual int IndexSiteID
        {
            get
            {
                return GetIntegerValue("IndexSiteID", 0);
            }
            set
            {
                SetValue("IndexSiteID", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            SearchIndexSiteInfoProvider.DeleteSearchIndexSiteInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            SearchIndexSiteInfoProvider.SetSearchIndexSiteInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty SearchIndexSiteInfo object.
        /// </summary>
        public SearchIndexSiteInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new SearchIndexSiteInfo object from the given DataRow.
        /// </summary>
        public SearchIndexSiteInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}