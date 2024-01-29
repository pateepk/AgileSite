using System;
using System.Collections.Generic;
using System.Text;

using CMS.Base;
using CMS.Helpers;

namespace CMS.DataEngine
{
    /// <summary>
    /// Class encapsulating parameters for GeneralizedInfo serialization (export).
    /// </summary>
    public class SynchronizationObjectSettings
    {
        #region "Variables"

        private OperationTypeEnum mOperation = OperationTypeEnum.Synchronization;

        private SafeDictionary<string, HashSet<string>> mBindingDuplicities;

        private bool mEnsureBinaryData = true;
        private bool mCreateHierarchy;
        private bool mHandleBoundObjects = true;
        private bool mIncludeChildren = true;
        private bool mIncludeBindings = true;
        private bool mIncludeOtherBindings = true;
        private bool mIncludeSiteBindings = true;
        private bool mIncludeTranslations = true;
        private bool mProcessTranslations = true;
        private bool mIncludeCategories = true;
        private bool mIncludeMetafiles = true;
        private bool mIncludeRelationships = true;
        private bool mIncludeScheduledTasks = true;
        private bool mIncludeProcesses = true;

        private bool mDisableCollectionPaging = true;

        private int mMaxRelativeLevel = -1;

        private bool mDocLinkedDocuments;
        private List<string> mDocConnectedObjects;

        private string mRootName = "NewDataSet";
        private bool mBinary;
        private bool mIncludeObjectData = true;

        private WhereCondition mWhereCondition;

        private string mColumns;
        private string mOrderBy;
        private int mTopN;
        private int mOffset;
        private int mMaxRecords;

        private string mDocSiteName;
        private string mDocAliasPath;
        private string mDocCultureCode;
        private string mDocClassNames;
        private string mDocVersion;

        private bool mDocCombineWithDefaultCulture;
        private bool mDocSelectOnlyPublished = true;
        private bool mDocCoupledData = true;
        private bool mDocSingleDocument;

        private Uri mExportItemURI;

        private string mReguestStockKey;

        #endregion


        #region "RequestStock Properties"

        /// <summary>
        /// Returns key for request stock helper caching.
        /// </summary>
        public virtual string RequestStockKey
        {
            get
            {
                if (mReguestStockKey == null)
                {
                    var sb = new StringBuilder(String.Join("|", 
                        Convert.ToInt32(CreateHierarchy),
                        Convert.ToInt32(IncludeBindings),
                        Convert.ToInt32(IncludeOtherBindings),
                        Convert.ToInt32(IncludeSiteBindings),
                        Convert.ToInt32(IncludeCategories),
                        Convert.ToInt32(IncludeChildren),
                        Convert.ToInt32(IncludeMetafiles),
                        Convert.ToInt32(IncludeRelationships),
                        Convert.ToInt32(IncludeTranslations),
                        Convert.ToInt32(ProcessTranslations),
                        Convert.ToInt32(IncludeChildren),
                        Convert.ToInt32(DocLinkedDocuments),
                        Convert.ToInt32(Binary),
                        Convert.ToInt32(DocCombineWithDefaultCulture),
                        Convert.ToInt32(DocSelectOnlyPublished),
                        Convert.ToInt32(IncludeObjectData),
                        RootName,
                        WhereCondition.ToString(true),
                        Columns,
                        OrderBy,
                        TopN,
                        Offset,
                        MaxRecords,
                        DocSiteName,
                        DocAliasPath,
                        DocCultureCode,
                        DocClassNames,
                        DocVersion,
                        ExportItemURI
                    ));

                    foreach (string item in DocConnectedObjects)
                    {
                        sb.Append(",");
                        sb.Append(item);
                    }

                    sb.Append(
                        "|", MaxRelativeLevel,
                        "|", Operation
                    );

                    mReguestStockKey = sb.ToString();
                }
                return mReguestStockKey;
            }
        }

        #endregion


        #region "General Properties"

        /// <summary>
        /// Gets duplicities for object bindings
        /// </summary>
        public SafeDictionary<string, HashSet<string>> BindingDuplicities 
        {
            get
            {
                return mBindingDuplicities ?? (mBindingDuplicities = new SafeDictionary<string, HashSet<string>>());
            }
        }


        /// <summary>
        /// Translation helper to pass to the callback methods.
        /// </summary>
        public TranslationHelper TranslationHelper
        {
            get;
            set;
        }


        /// <summary>
        /// Operation type (Export / Synchronization / etc.). According to this parameter export determines which child / binding objects to include in the export.
        /// </summary>
        public OperationTypeEnum Operation
        {
            get
            {
                return mOperation;
            }
            set
            {
                mOperation = value;
                mReguestStockKey = null;
            }
        }


        /// <summary>
        /// If true, result will be hierarchical (children, bindings in lower level than parent etc.).
        /// </summary>
        public bool CreateHierarchy
        {
            get
            {
                return mCreateHierarchy;
            }
            set
            {
                mCreateHierarchy = value;
                mReguestStockKey = null;
            }
        }


        /// <summary>
        /// If false, all bound objects (such as children or all types of bindings) are not being handled.
        /// If true, handling of these objects is determined by more specific settings (such as IncludeChildren, IncludeBindings, etc.)
        /// </summary>
        public bool HandleBoundObjects
        {
            get
            {
                return mHandleBoundObjects;
            }
            set
            {
                mHandleBoundObjects = value;
                mReguestStockKey = null;
            }
        }


        /// <summary>
        /// If true, child objects are included in the result.
        /// </summary>
        public bool IncludeChildren
        {
            get
            {
                return mIncludeChildren && HandleBoundObjects;
            }
            set
            {
                mIncludeChildren = value;
                mReguestStockKey = null;
            }
        }


        /// <summary>
        /// If true, binding objects are included in the result.
        /// </summary>
        public bool IncludeBindings
        {
            get
            {
                return mIncludeBindings && HandleBoundObjects;
            }
            set
            {
                mIncludeBindings = value;
                mReguestStockKey = null;
            }
        }


        /// <summary>
        /// If true, other binding objects are included in the result.
        /// </summary>
        public bool IncludeOtherBindings
        {
            get
            {
                return mIncludeOtherBindings && HandleBoundObjects;
            }
            set
            {
                mIncludeOtherBindings = value;
                mReguestStockKey = null;
            }
        }


        /// <summary>
        /// If true, site binding objects are included in the result. This setting is applied only when IncludeBindings is true.
        /// </summary>
        public bool IncludeSiteBindings
        {
            get
            {
                return mIncludeSiteBindings && HandleBoundObjects;
            }
            set
            {
                mIncludeSiteBindings = value;
                mReguestStockKey = null;
            }
        }


        /// <summary>
        /// If true, translation table is exported within the result as well.
        /// </summary>
        public bool IncludeTranslations
        {
            get
            {
                return mIncludeTranslations;
            }
            set
            {
                mIncludeTranslations = value;
                mReguestStockKey = null;
            }
        }


        /// <summary>
        /// If true, translation table is filled within ProcessTranslations method.
        /// </summary>
        public bool ProcessTranslations
        {
            get
            {
                return mProcessTranslations;
            }
            set
            {
                mProcessTranslations = value;
                mReguestStockKey = null;
            }
        }


        /// <summary>
        /// If true, parent category hierarchy of the object is included in the result as well.
        /// </summary>
        public bool IncludeCategories
        {
            get
            {
                return mIncludeCategories && HandleBoundObjects;
            }
            set
            {
                mIncludeCategories = value;
                mReguestStockKey = null;
            }
        }


        /// <summary>
        /// If true, metafiles of the object are included in the result as well.
        /// </summary>
        public bool IncludeMetafiles
        {
            get
            {
                return mIncludeMetafiles && HandleBoundObjects;
            }
            set
            {
                mIncludeMetafiles = value;
                mReguestStockKey = null;
            }
        }


        /// <summary>
        /// If true, relationships of the object are included in the result as well.
        /// </summary>
        public bool IncludeRelationships
        {
            get
            {
                return mIncludeRelationships && HandleBoundObjects;
            }
            set
            {
                mIncludeRelationships = value;
                mReguestStockKey = null;
            }
        }


        /// <summary>
        /// If true, scheduled tasks of the object are included in the result as well.
        /// </summary>
        public bool IncludeScheduledTasks
        {
            get
            {
                return mIncludeScheduledTasks && HandleBoundObjects;
            }
            set
            {
                mIncludeScheduledTasks = value;
                mReguestStockKey = null;
            }
        }


        /// <summary>
        /// If true, automation processes of the object are included in the result as well.
        /// </summary>
        public bool IncludeProcesses
        {
            get
            {
                return mIncludeProcesses && HandleBoundObjects;
            }
            set
            {
                mIncludeProcesses = value;
                mReguestStockKey = null;
            }
        }


        /// <summary>
        /// Determines maximal level of the relationship (parent-child). -1 means all levels, 0 means no child objects, 1 means first level of children, etc.
        /// </summary>
        public int MaxRelativeLevel
        {
            get
            {
                return mMaxRelativeLevel;
            }
            set
            {
                mMaxRelativeLevel = value;
                mReguestStockKey = null;
            }
        }


        /// <summary>
        /// If true, the traversal process sets AllowPaging to false to all the collections it goes through.
        /// </summary>
        public bool DisableCollectionPaging
        {
            get
            {
                return mDisableCollectionPaging;
            }
            set
            {
                mDisableCollectionPaging = value;
            }
        }


        /// <summary>
        /// If true, all the collections are forced to load binary data.
        /// </summary>
        public bool EnsureBinaryData
        {
            get
            {
                return mEnsureBinaryData;
            }
            set
            {
                mEnsureBinaryData = value;
            }
        }

        #endregion


        #region "Document properties"

        /// <summary>
        /// If true, linked documents are included in the result.
        /// </summary>
        public bool DocLinkedDocuments
        {
            get
            {
                return mDocLinkedDocuments;
            }
            set
            {
                mDocLinkedDocuments = value;
                mReguestStockKey = null;
            }
        }


        /// <summary>
        /// Gets or sets the list of collections from ConnectedObjects collection which will go into the result.
        /// </summary>
        public List<string> DocConnectedObjects
        {
            get
            {
                if (mDocConnectedObjects == null)
                {
                    mDocConnectedObjects = new List<string> {
                        "allattachments", 
                        "versionhistory", 
                        "attachmenthistory", 
                        "aliases", 
                        "acls",
                        "messageboards",
                        "categories",
                        "relationships",
                        "pagetemplate",
                        "sku",
                        "group",
                        "taggroup"
                    };
                }
                return mDocConnectedObjects;
            }
            set
            {
                mDocConnectedObjects = value;
                mReguestStockKey = null;
            }
        }


        /// <summary>
        /// Handles FK ID (can be used to fill translation helper for example).
        /// </summary>
        /// <param name="settings">Export settings object</param>
        /// <param name="obj">Object (TreeNode / Info object) to process</param>
        /// <param name="columnName">Column name of the dependency</param>
        /// <param name="objectType">Object type of the dependency</param>
        /// <param name="required">Determines whether the dependency is required (reflects required flag from TypeInfo).</param>
        public delegate void OnProcessID(SynchronizationObjectSettings settings, ICMSObject obj, string columnName, string objectType, bool required);


        /// <summary>
        /// Method which is called to process any FK ID.
        /// </summary>
        public OnProcessID ProcessIDCallback
        {
            get;
            set;
        }

        #endregion


        #region "General Properties"

        /// <summary>
        /// Name of the root element of the resulting export (root of xml).
        /// </summary>
        public string RootName
        {
            get
            {
                return mRootName;
            }
            set
            {
                mRootName = value;
                mReguestStockKey = null;
            }
        }


        /// <summary>
        /// If true, binary data is exported as well.
        /// </summary>
        public bool Binary
        {
            get
            {
                return mBinary;
            }
            set
            {
                mBinary = value;
                mReguestStockKey = null;
            }
        }


        /// <summary>
        /// If false, object data are not exported (true by default).
        /// </summary>
        public bool IncludeObjectData
        {
            get
            {
                return mIncludeObjectData;
            }
            set
            {
                mIncludeObjectData = value;
                mReguestStockKey = null;
            }
        }

        #endregion


        #region "Multiple objects export properties"

        /// <summary>
        /// Where condition.
        /// </summary>
        public WhereCondition WhereCondition
        {
            get
            {
                return mWhereCondition ?? (mWhereCondition = new WhereCondition());
            }
            set
            {
                mWhereCondition = value;
                mReguestStockKey = null;
            }
        }


        /// <summary>
        /// Gets or sets ORDER BY clause.
        /// </summary>
        public string OrderBy
        {
            get
            {
                return mOrderBy;
            }
            set
            {
                mOrderBy = value;
                mReguestStockKey = null;
            }
        }


        /// <summary>
        /// Gets or sets COLUMNS clause.
        /// </summary>
        public string Columns
        {
            get
            {
                return mColumns;
            }
            set
            {
                mColumns = value;
                mReguestStockKey = null;
            }
        }


        /// <summary>
        /// Gets or sets TOP N clause.
        /// </summary>
        public int TopN
        {
            get
            {
                return mTopN;
            }
            set
            {
                mTopN = value;
                mReguestStockKey = null;
            }
        }


        /// <summary>
        /// Gets or sets offset of the records.
        /// </summary>
        public int Offset
        {
            get
            {
                return mOffset;
            }
            set
            {
                mOffset = value;
                mReguestStockKey = null;
            }
        }


        /// <summary>
        /// Gets or sets maximum number of records.
        /// </summary>
        public int MaxRecords
        {
            get
            {
                return mMaxRecords;
            }
            set
            {
                mMaxRecords = value;
                mReguestStockKey = null;
            }
        }

        #endregion


        #region "Document export properties"

        /// <summary>
        /// Nodes site name.
        /// </summary>
        public string DocSiteName
        {
            get
            {
                return mDocSiteName;
            }
            set
            {
                mDocSiteName = value;
                mReguestStockKey = null;
            }
        }


        /// <summary>
        /// Path. It may contain % and _ as wild card characters for any number of unknown characters or one unknown character respectively (for MS SQL).
        /// </summary>
        public string DocAliasPath
        {
            get
            {
                return mDocAliasPath;
            }
            set
            {
                mDocAliasPath = value;
                mReguestStockKey = null;
            }
        }


        /// <summary>
        /// Nodes culture code.
        /// </summary>
        public string DocCultureCode
        {
            get
            {
                return mDocCultureCode;
            }
            set
            {
                mDocCultureCode = value;
                mReguestStockKey = null;
            }
        }


        /// <summary>
        /// List of classNames to select separated by semicolon (e.g.: "cms.article;cms.product").
        /// </summary>
        public string DocClassNames
        {
            get
            {
                return mDocClassNames;
            }
            set
            {
                mDocClassNames = value;
                mReguestStockKey = null;
            }
        }


        /// <summary>
        /// Version of the document to return.
        /// </summary>
        public string DocVersion
        {
            get
            {
                return mDocVersion;
            }
            set
            {
                mDocVersion = value;
                mReguestStockKey = null;
            }
        }


        /// <summary>
        /// Specifies if return the default culture document when specified culture not found.
        /// </summary>
        public bool DocCombineWithDefaultCulture
        {
            get
            {
                return mDocCombineWithDefaultCulture;
            }
            set
            {
                mDocCombineWithDefaultCulture = value;
                mReguestStockKey = null;
            }
        }


        /// <summary>
        /// Select only published nodes.
        /// </summary>
        public bool DocSelectOnlyPublished
        {
            get
            {
                return mDocSelectOnlyPublished;
            }
            set
            {
                mDocSelectOnlyPublished = value;
                mReguestStockKey = null;
            }
        }


        /// <summary>
        /// If false, coupled data are not included in the result.
        /// </summary>
        public bool DocCoupledData
        {
            get
            {
                return mDocCoupledData;
            }
            set
            {
                mDocCoupledData = value;
                mReguestStockKey = null;
            }
        }


        /// <summary>
        /// If true, only single document is selected
        /// </summary>
        public bool DocSingleDocument
        {
            get
            {
                return mDocSingleDocument;
            }
            set
            {
                mDocSingleDocument = value;
                mReguestStockKey = null;
            }
        }


        /// <summary>
        /// If true, all cultures are deleted when document is being deleted.
        /// </summary>
        public bool DocDeleteAllCultures
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the history is deleted when the document is being deleted.
        /// </summary>
        public bool DocDestroyHistory
        {
            get;
            set;
        }

        #endregion


        #region "ODATA Properties"

        /// <summary>
        /// Gets or sets URI of the exported item (needed for ODATA export).
        /// </summary>
        public Uri ExportItemURI
        {
            get
            {
                return mExportItemURI;
            }
            set
            {
                mExportItemURI = value;
                mReguestStockKey = null;
            }
        }

        #endregion


        #region "REST-specific Properties"

        /// <summary>
        /// Gets or sets the callback JS code used a JSONP.
        /// </summary>
        public string JSONCallback
        {
            get;
            set;
        }

        #endregion
    }
}