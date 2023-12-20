using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.WebAnalytics;

[assembly: RegisterObjectType(typeof(SearchEngineInfo), SearchEngineInfo.OBJECT_TYPE)]

namespace CMS.WebAnalytics
{
    /// <summary>
    /// SearchEngineInfo data container class.
    /// </summary>
    public class SearchEngineInfo : AbstractInfo<SearchEngineInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.searchengine";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(SearchEngineInfoProvider), OBJECT_TYPE, "CMS.SearchEngine", "SearchEngineID", "SearchEngineLastModified", "SearchEngineGUID", "SearchEngineName", "SearchEngineDisplayName", null, null, null, null)
        {
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(GLOBAL, CONFIGURATION),
                }
            },

            LogEvents = true,
            TouchCacheDependencies = true,
            ImportExportSettings =
            {
                IncludeToWebTemplateExport = ObjectRangeEnum.Site,
                LogExport = true,
                IsExportable = true,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(GLOBAL, CONFIGURATION)
                },
            },
            DefaultData = new DefaultDataSettings(),
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Search engine object keyword parameter.
        /// </summary>
        [DatabaseField]
        public virtual string SearchEngineKeywordParameter
        {
            get
            {
                return GetStringValue("SearchEngineKeywordParameter", "");
            }
            set
            {
                SetValue("SearchEngineKeywordParameter", value);
            }
        }


        /// <summary>
        /// Date of last modification of search engine object.
        /// </summary>
        [DatabaseField]
        public virtual DateTime SearchEngineLastModified
        {
            get
            {
                return GetDateTimeValue("SearchEngineLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("SearchEngineLastModified", value);
            }
        }


        /// <summary>
        /// Search engine object domain role.
        /// </summary>
        [DatabaseField]
        public virtual string SearchEngineDomainRule
        {
            get
            {
                return GetStringValue("SearchEngineDomainRule", "");
            }
            set
            {
                SetValue("SearchEngineDomainRule", value);
            }
        }


        /// <summary>
        /// Search engine object display name.
        /// </summary>
        [DatabaseField]
        public virtual string SearchEngineDisplayName
        {
            get
            {
                return GetStringValue("SearchEngineDisplayName", "");
            }
            set
            {
                SetValue("SearchEngineDisplayName", value);
            }
        }


        /// <summary>
        /// Search engine object code name.
        /// </summary>
        [DatabaseField]
        public virtual string SearchEngineName
        {
            get
            {
                return GetStringValue("SearchEngineName", "");
            }
            set
            {
                SetValue("SearchEngineName", value);
            }
        }


        /// <summary>
        /// Search engine object ID.
        /// </summary>
        [DatabaseField]
        public virtual int SearchEngineID
        {
            get
            {
                return GetIntegerValue("SearchEngineID", 0);
            }
            set
            {
                SetValue("SearchEngineID", value);
            }
        }


        /// <summary>
        /// Search engine object GUID.
        /// </summary>
        [DatabaseField]
        public virtual Guid SearchEngineGUID
        {
            get
            {
                return GetGuidValue("SearchEngineGUID", Guid.Empty);
            }
            set
            {
                SetValue("SearchEngineGUID", value);
            }
        }


        /// <summary>
        /// Search engine crawler.
        /// </summary>
        [DatabaseField]
        public virtual string SearchEngineCrawler
        {
            get
            {
                return GetStringValue("SearchEngineCrawler", string.Empty);
            }
            set
            {
                SetValue("SearchEngineCrawler", value);
            }
        }

        #endregion


        #region "GeneralizedInfo properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            SearchEngineInfoProvider.DeleteSearchEngineInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            SearchEngineInfoProvider.SetSearchEngineInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty SearchEngineInfo object.
        /// </summary>
        public SearchEngineInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new SearchEngineInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public SearchEngineInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}