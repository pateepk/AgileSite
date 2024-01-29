using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.IO;
using CMS.Base;
using CMS.Search;

[assembly: RegisterObjectType(typeof(SearchIndexInfo), SearchIndexInfo.OBJECT_TYPE)]

namespace CMS.Search
{
    using RebuildTable = SafeDictionary<int, DateTime>;

    /// <summary>
    /// Search index.
    /// </summary>
    public class SearchIndexInfo : AbstractInfo<SearchIndexInfo>, ISearchIndexInfo
    {
        #region "Constants"

        /// <summary>
        /// Identifies search index that is processed by Azure. Value should be used in <see cref="IndexProvider"/> property.
        /// </summary>
        public const string AZURE_SEARCH_PROVIDER = "Azure";


        /// <summary>
        /// Identifies search index that is processed by Lucene. Value should be used in <see cref="IndexProvider"/> property.
        /// </summary>
        public const string LUCENE_SEARCH_PROVIDER = "Lucene";

        #endregion


        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.searchindex";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(SearchIndexInfoProvider), OBJECT_TYPE, "CMS.SearchIndex", "IndexID", "IndexLastModified", "IndexGUID", "IndexName", "IndexDisplayName", null, null, null, null)
        {
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(GLOBAL, CONFIGURATION)
                },
                ExcludedStagingColumns = new List<string>
                {
                    "IndexLastRebuildTime",
                    "IndexIsOutdated"
                }
            },
            DefaultData = new DefaultDataSettings
            {
                IncludeToWebTemplateDataOnly = true,
                ExcludedColumns = new List<string>
                {
                    "IndexIsOutdated"
                }
            },
            LogEvents = true,
            TouchCacheDependencies = true,
            SupportsVersioning = false,
            AssemblyNameColumn = "IndexCustomAnalyzerAssemblyName",
            ImportExportSettings =
            {
                LogExport = true,
                IsExportable = true,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(GLOBAL, CONFIGURATION)
                },
            },
            SensitiveColumns = new List<string> { nameof(IndexAdminKey), nameof(IndexQueryKey) },
            ContinuousIntegrationSettings =
            {
                Enabled = true
            },
            SerializationSettings =
            {
                ExcludedFieldNames =
                {
                    "IndexLastRebuildTime",
                    "IndexStatus",
                    "IndexIsOutdated"
                },
                StructuredFields = new IStructuredField[]
                {
                    new StructuredField<SearchIndexSettings>("IndexSettings")
                }
            }
        };

        #endregion


        #region "Static variables"

        /// <summary>
        /// Store actual index Rebuild time. [indexId -> DateTime]
        /// </summary>
        private static CMSStatic<RebuildTable> mRebuild = new CMSStatic<RebuildTable>(() => new RebuildTable());


        /// <summary>
        /// Smart search index path prefix.
        /// </summary>
        private static string mIndexPathPrefix;

        #endregion


        #region "Private variables"

        /// <summary>
        /// Last update of index.
        /// </summary>
        private DateTime? mIndexFilesLastUpdate;

        /// <summary>
        /// Contains object with search index settings, initialized by first get of IndexSettings property.
        /// </summary>
        private SearchIndexSettings mIndexSettings;

        /// <summary>
        /// Index status.
        /// </summary>
        private IndexStatusEnum mIndexStatus = IndexStatusEnum.UNKNOWN;
        private IndexStatusEnum mIndexStatusLocal = IndexStatusEnum.UNKNOWN;

        private SearchIndexProvider mProvider;
        private volatile IIndexStatistics statistics;
        private readonly object statisticsLock = new object();

        private readonly IIndexStatistics NO_INDEX_STATISTICS = new NoIndexStatistics();

        #endregion


        #region "Properties"

        /// <summary>
        /// Store actual index Rebuild time. [indexId -> DateTime]
        /// </summary>
        public static RebuildTable Rebuild
        {
            get
            {
                return mRebuild;
            }
        }


        /// <summary>
        /// Gets or sets the batch size for data loading. The default value of 500 is returned when not explicitly set.
        /// </summary>
        public int IndexBatchSize
        {
            get
            {
                return GetIntegerValue("IndexBatchSize", 500);
            }
            set
            {
                SetValue("IndexBatchSize", value);
            }
        }


        /// <summary>
        /// Time when index files were updated. Value is assigned to index files used by current application process.
        /// </summary>
        /// <remarks>
        /// The value of this property is based on index files and is inherently not valid for Azure indexes.
        /// </remarks>
        public DateTime IndexFilesLastUpdate
        {
            get
            {
                // Return cached value
                if (mIndexFilesLastUpdate != null)
                {
                    return mIndexFilesLastUpdate.Value;
                }

                mIndexFilesLastUpdate = Provider.GetIndexLastUpdateTime();

                return mIndexFilesLastUpdate.Value;
            }
            internal set
            {
                mIndexFilesLastUpdate = value;
            }
        }


        /// <summary>
        /// Gets or sets the actual Rebuild time.
        /// </summary>
        public DateTime ActualRebuildTime
        {
            get
            {
                return ValidationHelper.GetDateTime(Rebuild[IndexID], DateTimeHelper.ZERO_TIME);
            }
            set
            {
                Rebuild[IndexID] = value;
            }
        }


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
        /// Index name.
        /// </summary>
        public virtual string IndexName
        {
            get
            {
                return GetStringValue("IndexName", String.Empty);
            }
            set
            {
                SetValue("IndexName", value);
            }
        }


        /// <summary>
        /// Gets or sets the index type (documents, forums, etc).
        /// </summary>
        public virtual string IndexType
        {
            get
            {
                return GetStringValue("IndexType", String.Empty);
            }
            set
            {
                SetValue("IndexType", value);
            }
        }


        /// <summary>
        /// Gets index code name. This property is used in ISearchIndexInfo interface.
        /// </summary>
        public virtual string IndexCodeName
        {
            get
            {
                return OriginalObjectCodeName;
            }
        }


        /// <summary>
        /// Display name.
        /// </summary>
        public virtual string IndexDisplayName
        {
            get
            {
                return GetStringValue("IndexDisplayName", String.Empty);
            }
            set
            {
                SetValue("IndexDisplayName", value);
            }
        }


        /// <summary>
        /// Defines a provider which performs actions on search index (e.g. <see cref="AZURE_SEARCH_PROVIDER"/>, <see cref="LUCENE_SEARCH_PROVIDER"/>).
        /// </summary>
        public virtual string IndexProvider
        {
            get
            {
                return GetStringValue(nameof(IndexProvider), String.Empty);
            }
            set
            {
                SetValue(nameof(IndexProvider), value);
            }
        }


        /// <summary>
        /// Name of the Azure Search service (on Microsoft Azure).
        /// </summary>
        /// <seealso cref="IndexAdminKey"/>
        /// <seealso cref="IndexQueryKey"/>
        public virtual string IndexSearchServiceName
        {
            get
            {
                return GetStringValue(nameof(IndexSearchServiceName), String.Empty);
            }
            set
            {
                SetValue(nameof(IndexSearchServiceName), value);
            }
        }


        /// <summary>
        /// Azure Search admin key for the search service.
        /// </summary>
        /// <seealso cref="IndexSearchServiceName"/>
        /// <seealso cref="IndexQueryKey"/>
        public virtual string IndexAdminKey
        {
            get
            {
                return GetStringValue(nameof(IndexAdminKey), String.Empty);
            }
            set
            {
                SetValue(nameof(IndexAdminKey), value);
            }
        }


        /// <summary>
        /// Azure Search query key for the search service.
        /// </summary>
        /// <seealso cref="IndexSearchServiceName"/>
        /// <seealso cref="IndexAdminKey"/>
        public virtual string IndexQueryKey
        {
            get
            {
                return GetStringValue(nameof(IndexQueryKey), String.Empty);
            }
            set
            {
                SetValue(nameof(IndexQueryKey), value);
            }
        }


        /// <summary>
        /// Domain under which the crawler scans the site.
        /// </summary>
        public virtual string IndexCrawlerDomain
        {
            get
            {
                return GetStringValue("IndexCrawlerDomain", String.Empty);
            }
            set
            {
                SetValue("IndexCrawlerDomain", value);
            }
        }


        /// <summary>
        /// Index user name - credentials for document crawler.
        /// </summary>
        public virtual string IndexCrawlerUserName
        {
            get
            {
                return GetStringValue("IndexCrawlerUserName", String.Empty);
            }
            set
            {
                SetValue("IndexCrawlerUserName", value);
            }
        }


        /// <summary>
        /// Index user name - credentials for document crawler for forms authentication.
        /// </summary>
        public virtual string IndexCrawlerFormsUserName
        {
            get
            {
                return GetStringValue("IndexCrawlerFormsUserName", String.Empty);
            }
            set
            {
                SetValue("IndexCrawlerFormsUserName", value);
            }
        }


        /// <summary>
        /// Index user password - credentials for document crawler.
        /// </summary>
        public virtual string IndexCrawlerUserPassword
        {
            get
            {
                return GetStringValue("IndexCrawlerUserPassword", String.Empty);
            }
            set
            {
                SetValue("IndexCrawlerUserPassword", value);
            }
        }


        /// <summary>
        /// Index analyzer type.
        /// </summary>
        public virtual SearchAnalyzerTypeEnum IndexAnalyzerType
        {
            get
            {
                return ValidationHelper.GetString(GetValue("IndexAnalyzerType"), String.Empty).ToEnum<SearchAnalyzerTypeEnum>();
            }
            set
            {
                SetValue("IndexAnalyzerType", value.ToStringRepresentation());
            }
        }


        /// <summary>
        /// Is community group.
        /// </summary>
        public virtual bool IndexIsCommunityGroup
        {
            get
            {
                return GetBooleanValue("IndexIsCommunityGroup", false);
            }
            set
            {
                SetValue("IndexIsCommunityGroup", value);
            }
        }


        /// <summary>
        /// Indicates if index is rebuilt but outdated
        /// </summary>
        public virtual bool IndexIsOutdated
        {
            get
            {
                return GetBooleanValue("IndexIsOutdated", false);
            }
            set
            {
                SetValue("IndexIsOutdated", value);
            }
        }


        /// <summary>
        /// Index settings.
        /// </summary>
        public virtual SearchIndexSettings IndexSettings
        {
            get
            {
                // Load if not loaded yet
                if (mIndexSettings == null)
                {
                    string settingData = ValidationHelper.GetString(GetValue("IndexSettings"), String.Empty);

                    mIndexSettings = new SearchIndexSettings();
                    mIndexSettings.LoadData(settingData);
                }

                return mIndexSettings;
            }
            set
            {
                mIndexSettings = value;

                if (value != null)
                {
                    SetValue("IndexSettings", value.GetData());
                }
                else
                {
                    SetValue("IndexSettings", null);
                }
            }
        }


        /// <summary>
        /// Index GUID.
        /// </summary>
        public virtual Guid IndexGUID
        {
            get
            {
                return GetGuidValue("IndexGUID", Guid.Empty);
            }
            set
            {
                SetValue("IndexGUID", value);
            }
        }


        /// <summary>
        /// Time of last modification.
        /// </summary>
        public virtual DateTime IndexLastModified
        {
            get
            {
                return GetDateTimeValue("IndexLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("IndexLastModified", value);
            }
        }


        /// <summary>
        /// Time of last Rebuild of index.
        /// </summary>
        public virtual DateTime IndexLastRebuildTime
        {
            get
            {
                return GetDateTimeValue("IndexLastRebuildTime", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("IndexLastRebuildTime", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Gets the index size in bytes. Returns null if size information is not available.
        /// </summary>
        /// <see cref="InvalidateIndexStatistics"/>
        public virtual long? IndexSize
        {
            get
            {               
                return GetStatistics()?.Size;
            }
        }


        /// <summary>
        /// Gets the number of indexed documents. Returns null if document count information is not available.
        /// </summary>
        /// <seealso cref="InvalidateIndexStatistics"/>
        public virtual long? IndexDocumentCount
        {
            get
            {
                return GetStatistics()?.DocumentCount;
            }
        }
        

        /// <summary>
        /// Gets or sets local index status.
        /// </summary>
        /// <remarks>
        /// This property is in-memory only. Use it for indexes whose storage belongs to current application process (e.g. local storage file based index on a web farm server, where each server
        /// maintains their own local index status).
        /// </remarks>
        public IndexStatusEnum IndexStatusLocal
        {
            get
            {
                if (mIndexStatusLocal == IndexStatusEnum.UNKNOWN)
                {
                    // Get index status from the provider
                    mIndexStatusLocal = Provider.GetIndexStatus();
                }

                return mIndexStatusLocal;
            }
            internal set
            {
                mIndexStatusLocal = value;
            }
        }


        /// <summary>
        /// Gets or sets index status.
        /// </summary>
        /// <remarks>
        /// This property is backed by a database column. Use it for indexes whose storage is shared by all web farm servers (e.g. index on shared file system storage or external index - Azure).
        /// </remarks>
        public virtual IndexStatusEnum IndexStatus
        {
            get
            {
                if (mIndexStatus == IndexStatusEnum.UNKNOWN)
                {
                    // Get index status from DB
                    IndexStatusEnum result;

                    if (Enum.TryParse(GetStringValue("IndexStatus", string.Empty), true, out result))
                    {
                        mIndexStatus = result;
                    }
                    else
                    {
                        mIndexStatus = IndexStatusEnum.NEW;
                    }
                }

                return mIndexStatus;
            }
            set
            {
                mIndexStatus = value;

                SetValue("IndexStatus", value.ToStringRepresentation());
            }
        }


        /// <summary>
        /// Gets or sets the custom analyzer assembly name.
        /// </summary>
        public string CustomAnalyzerAssemblyName
        {
            get
            {
                return GetStringValue("IndexCustomAnalyzerAssemblyName", String.Empty);
            }
            set
            {
                SetValue("IndexCustomAnalyzerAssemblyName", value);
            }
        }


        /// <summary>
        /// Gets or sets the custom analyzer class name.
        /// </summary>
        public string CustomAnalyzerClassName
        {
            get
            {
                return GetStringValue("IndexCustomAnalyzerClassName", String.Empty);
            }
            set
            {
                SetValue("IndexCustomAnalyzerClassName", value);
            }
        }


        /// <summary>
        /// Gets or sets the filename which should be used for stop words definition.
        /// </summary>
        public string StopWordsFile
        {
            get
            {
                return GetStringValue("IndexStopWordsFile", String.Empty);
            }
            set
            {
                SetValue("IndexStopWordsFile", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            SearchIndexInfoProvider.DeleteSearchIndexInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            SearchIndexInfoProvider.SetSearchIndexInfo(this);
        }


        /// <summary>
        /// Clones metafile and inserts it to DB as new object.
        /// </summary>
        /// <param name="settings">Cloning settings</param>
        /// <param name="result">Cloning result</param>
        /// <param name="originalObject">Original source BaseInfo (object being cloned)</param>
        protected override void InsertAsCloneInternal(CloneSettings settings, CloneResult result, BaseInfo originalObject)
        {
            IndexLastRebuildTime = DateTimeHelper.ZERO_TIME;
            mIndexFilesLastUpdate = null;

            Insert();
        }


        /// <summary>
        /// Sets the field value.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">New value</param>
        public override bool SetValue(string columnName, object value)
        {
            if (columnName.Equals(nameof(IndexAdminKey), StringComparison.InvariantCultureIgnoreCase) ||
                columnName.Equals(nameof(IndexQueryKey), StringComparison.InvariantCultureIgnoreCase))
            {
                value = EncryptionHelper.EncryptData((string)value);
            }

            return base.SetValue(columnName, value);
        }


        /// <summary>
        /// Gets the field value.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public override object GetValue(string columnName)
        {
            var value = base.GetValue(columnName);

            if (columnName.Equals(nameof(IndexAdminKey), StringComparison.InvariantCultureIgnoreCase) ||
                columnName.Equals(nameof(IndexQueryKey), StringComparison.InvariantCultureIgnoreCase))
            {
                value = EncryptionHelper.DecryptData((string)value);
            }

            return value;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty SearchIndexInfo object.
        /// </summary>
        public SearchIndexInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new SearchIndexInfo object from the given DataRow.
        /// </summary>
        public SearchIndexInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "Index searcher/writer"

        /// <summary>
        /// Gets the index path prefix.
        /// </summary>
        public static string IndexPathPrefix
        {
            get
            {
                if (mIndexPathPrefix == null)
                {
                    string indexPathPrefix = Path.Combine(SystemContext.WebApplicationPhysicalPath, SearchHelper.SearchPath);

                    // Path must end with '\'
                    if (!indexPathPrefix.EndsWithCSafe("\\"))
                    {
                        indexPathPrefix += "\\";
                    }

                    mIndexPathPrefix = indexPathPrefix;
                }

                return mIndexPathPrefix;
            }
        }


        /// <summary>
        /// Returns current index path.
        /// </summary>
        public string CurrentIndexPath
        {
            get
            {
                // Get complete index path prefix
                return IndexPathPrefix + IndexName + "\\";
            }
        }


        /// <summary>
        /// Search index provider
        /// </summary>
        public SearchIndexProvider Provider
        {
            get
            {
                return mProvider ?? (mProvider = new SearchIndexProvider(this));
            }
            internal set
            {
                mProvider = value;
            }
        }


        /// <summary>
        /// Invalidates cached values of index size and document count.
        /// </summary>
        public void InvalidateIndexStatistics()
        {
            statistics = null;
        }


        /// <summary>
        /// Gets index statistics, or null if statistics are not available.
        /// </summary>
        private IIndexStatistics GetStatistics()
        {
            var statisticsLocal = statistics;
            if (statisticsLocal == null)
            {
                statisticsLocal = InitializeAndGetStatistics();
            }

            return statisticsLocal != NO_INDEX_STATISTICS ? statisticsLocal : null;
        }


        /// <summary>
        /// Initializes, publishes and gets new statistics.
        /// </summary>
        private IIndexStatistics InitializeAndGetStatistics()
        {
            lock (statisticsLock)
            {
                // The statistics are initialized from within a lock section, but cleared without locking
                var statisticsLocal = statistics;
                if (statisticsLocal != null)
                {
                    return statisticsLocal;
                }
                var provider = IndexStatisticsProviders.Instance.Get(IndexProvider != "" ? IndexProvider : LUCENE_SEARCH_PROVIDER);
                statisticsLocal = provider.GetStatistics(this) ?? NO_INDEX_STATISTICS;
                statistics = statisticsLocal;

                return statisticsLocal;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns true if the index is optimized
        /// </summary>
        public bool IsOptimized()
        {
            if (SearchIndexInfoProvider.GetIndexStatus(this) == IndexStatusEnum.READY)
            {
                // No items indexed = optimized
                return !IndexDocumentCount.HasValue || (IndexDocumentCount == 0) || Provider.IsOptimized();
            }

            return false;
        }

        #endregion


        #region "Private classes"

        private class NoIndexStatistics : IIndexStatistics
        {
            public long DocumentCount
            {
                get
                {
                    throw new NotImplementedException();
                }
            }


            public long Size
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
        }

        #endregion
    }
}