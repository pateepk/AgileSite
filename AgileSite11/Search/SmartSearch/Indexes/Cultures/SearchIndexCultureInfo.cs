using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Localization;
using CMS.Search;

[assembly: RegisterObjectType(typeof(SearchIndexCultureInfo), SearchIndexCultureInfo.OBJECT_TYPE)]

namespace CMS.Search
{
    /// <summary>
    /// Search index culture.
    /// </summary>
    public class SearchIndexCultureInfo : AbstractInfo<SearchIndexCultureInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.searchindexculture";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(SearchIndexCultureInfoProvider), OBJECT_TYPE, "CMS.SearchIndexCulture", null, null, null, null, null, null, null, "IndexID", SearchIndexInfo.OBJECT_TYPE)
        {
            DependsOn = new List<ObjectDependency> { new ObjectDependency("IndexCultureID", CultureInfo.OBJECT_TYPE, ObjectDependencyEnum.Binding) },
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
        /// Culture ID of index.
        /// </summary>
        public virtual int IndexCultureID
        {
            get
            {
                return GetIntegerValue("IndexCultureID", 0);
            }
            set
            {
                SetValue("IndexCultureID", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            SearchIndexCultureInfoProvider.DeleteSearchIndexCultureInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            SearchIndexCultureInfoProvider.SetSearchIndexCultureInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty SearchCultureInfo object.
        /// </summary>
        public SearchIndexCultureInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new SearchCultureInfo object from the given DataRow.
        /// </summary>
        public SearchIndexCultureInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}