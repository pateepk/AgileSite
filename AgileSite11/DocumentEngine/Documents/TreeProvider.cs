using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS.Base;
using CMS.ContinuousIntegration;
using CMS.DataEngine;
using CMS.DataEngine.Query;
using CMS.DocumentEngine.Compatibility;
using CMS.DocumentEngine.Internal;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.Membership;
using CMS.PortalEngine;
using CMS.Relationships;
using CMS.Search;
using CMS.SiteProvider;
using CMS.Taxonomy;
using CMS.SearchProviderSQL;

namespace CMS.DocumentEngine
{
    using TypedDataSet = InfoDataSet<TreeNode>;

    /// <summary>
    /// Provides methods for management of the tree structure.
    /// </summary>
    public class TreeProvider
    {
        #region "Constants"

        /// <summary>
        /// Size of the batch when processing multiple documents
        /// </summary>
        internal const int PROCESSING_BATCH = 500;

        /// <summary>
        /// Supplementary constant to specify all document cultures.
        /// </summary>
        public const string ALL_CULTURES = "##ALL##";

        /// <summary>
        /// Supplementary constant to specify all sites.
        /// </summary>
        public const string ALL_SITES = "##ALL##";

        /// <summary>
        /// Supplementary constant to specify all class names.
        /// </summary>
        public const string ALL_CLASSNAMES = DataClassInfoProvider.ALL_CLASSNAMES;

        /// <summary>
        /// Supplementary constant to specify all documents.
        /// </summary>
        public const string ALL_DOCUMENTS = "/%";

        /// <summary>
        /// All relative levels.
        /// </summary>
        public const int ALL_LEVELS = -1;

        #endregion


        #region "Variables"

        private ISearchProvider mProviderObject;
        private UserInfo mUserInfo;
        private bool? mCombineWithDefaultCulture;
        private string mPreferredCultureCode;
        private bool mSortMergedResults = true;
        private bool? mUseAutomaticOrdering;
        private bool mSynchronizeFieldValues = true;
        private bool mUseCustomHandlers = true;
        private bool mGenerateNewGuid = true;
        private bool mCheckUniqueNames = true;
        private bool mCheckLinkConsistency = true;
        private bool mAutomaticallyUpdateDocumentAlias = true;
        private bool mEnsureSafeNodeAlias = true;
        private bool mCheckUniqueAttachmentNames = true;
        private bool mUpdateTimeStamps = true;
        private bool mUpdateUser = true;
        private bool mUpdatePaths = true;
        private bool mUpdateSKUColumns = true;
        private bool mUpdateDocumentContent = true;
        private bool mLogEvents = true;
        private bool mLogSynchronization = true;
        private bool mLogIntegration = true;
        private bool mTouchCacheDependencies = true;
        private bool mLogWebFarmTasks = true;
        private bool mSelectAllData = true;
        private bool mEnableNotifications = true;
        private bool mEnableRating = true;
        private bool mAllowAsyncActions = true;
        private bool mEnableDocumentAliases = true;
        private bool mHandleACLs = true;
        private bool? mUseParentNodeGroupID;
        private bool mUpdateNodeName = true;
        private bool mUpdateUrlPath = true;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Current user info.
        /// </summary>
        public virtual UserInfo UserInfo
        {
            get
            {
                // Get the user from context if not found
                if (mUserInfo == null)
                {
                    return (UserInfo)CMSActionContext.CurrentUser;
                }

                return mUserInfo;
            }
            set
            {
                mUserInfo = value;
            }
        }


        /// <summary>
        /// Indicates if returned nodes should be combined with appropriate nodes of default culture in case they are not localized. It applies only if you're using multilingual support. The default value is false.
        /// </summary>
        public virtual bool CombineWithDefaultCulture
        {
            get
            {
                if (mCombineWithDefaultCulture == null)
                {
                    // Get the default combine settings
                    return SettingsKeyInfoProvider.GetBoolValue("CMSCombineWithDefaultCulture");
                }

                return mCombineWithDefaultCulture.Value;
            }
            set
            {
                mCombineWithDefaultCulture = value;
            }
        }


        /// <summary>
        /// Preferred culture code to use when none set.
        /// </summary>
        public virtual string PreferredCultureCode
        {
            get
            {
                if (mPreferredCultureCode == null)
                {
                    // Get the preferred culture for user
                    string culture = CultureHelper.GetPreferredCulture();
                    mPreferredCultureCode = String.IsNullOrEmpty(culture) ? CultureHelper.GetDefaultCultureCode(null) : culture;
                }

                return mPreferredCultureCode;
            }
            set
            {
                mPreferredCultureCode = value;
            }
        }


        /// <summary>
        /// Base query name to use for the document selection (for the enhanced selection options only).
        /// </summary>
        public virtual string SelectQueryName
        {
            get;
            set;
        }


        /// <summary>
        /// If true, tables from result DataSet are merged into a single table.
        /// </summary>
        public bool MergeResults
        {
            get;
            set;
        }


        /// <summary>
        /// If true, merged results are sorted.
        /// </summary>
        public bool SortMergedResults
        {
            get
            {
                return mSortMergedResults;
            }
            set
            {
                mSortMergedResults = value;
            }
        }


        /// <summary>
        /// Indicates whether field values should be synchronized on set for the document (SKU mappings etc.)
        /// </summary>
        internal bool SynchronizeFieldValues
        {
            get
            {
                return mSynchronizeFieldValues && DocumentActionContext.CurrentUseAutomaticOrdering;
            }
            set
            {
                mSynchronizeFieldValues = value;
            }
        }


        /// <summary>
        /// If true, automatic ordering is used for new nodes.
        /// </summary>
        public bool UseAutomaticOrdering
        {
            get
            {
                if (mUseAutomaticOrdering == null)
                {
                    mUseAutomaticOrdering = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSUseAutomaticNodeOrdering"], true);
                }

                return mUseAutomaticOrdering.Value && DocumentActionContext.CurrentUseAutomaticOrdering;
            }
            set
            {
                mUseAutomaticOrdering = value;
            }
        }


        /// <summary>
        /// If true, duplicate (linked) items are filtered.
        /// </summary>
        public bool FilterOutDuplicates
        {
            get;
            set;
        }


        /// <summary>
        /// If true, custom handlers are used with Document operations.
        /// </summary>
        public bool UseCustomHandlers
        {
            get
            {
                return mUseCustomHandlers;
            }
            set
            {
                mUseCustomHandlers = value;
            }
        }


        /// <summary>
        /// If true, new GUID is generated for the inserted nodes where required.
        /// </summary>
        public bool GenerateNewGuid
        {
            get
            {
                return mGenerateNewGuid;
            }
            set
            {
                mGenerateNewGuid = value;
            }
        }


        /// <summary>
        /// If true, unique node names, document names and aliases are checked against the database. Turn off only when you perform the validation by yourself.
        /// </summary>
        public bool CheckUniqueNames
        {
            get
            {
                return mCheckUniqueNames;
            }
            set
            {
                mCheckUniqueNames = value;
            }
        }


        /// <summary>
        /// If true, the linked document checks if the reference to its original is preserved.
        /// </summary>
        public bool CheckLinkConsistency
        {
            get
            {
                return mCheckLinkConsistency;
            }
            set
            {
                mCheckLinkConsistency = value;
            }
        }


        /// <summary>
        /// If true, the document alias should be automatically updated upon document name change.
        /// </summary>
        public bool AutomaticallyUpdateDocumentAlias
        {
            get
            {
                return mAutomaticallyUpdateDocumentAlias;
            }
            set
            {
                mAutomaticallyUpdateDocumentAlias = value;
            }
        }


        /// <summary>
        /// If true, node alias will include only allowed characters (turn off only when you perform the validation by yourself or the source data are already valdiated).
        /// </summary>
        public bool EnsureSafeNodeAlias
        {
            get
            {
                return mEnsureSafeNodeAlias;
            }
            set
            {
                mEnsureSafeNodeAlias = value;
            }
        }


        /// <summary>
        /// If true, unique attachment names within one document are checked against the database. Turn off only when you perform the validation by yourself.
        /// </summary>
        public bool CheckUniqueAttachmentNames
        {
            get
            {
                return mCheckUniqueAttachmentNames;
            }
            set
            {
                mCheckUniqueAttachmentNames = value;
            }
        }


        /// <summary>
        /// If true, time stamps of the document are updated.
        /// </summary>
        public bool UpdateTimeStamps
        {
            get
            {
                return mUpdateTimeStamps && CMSActionContext.CurrentUpdateTimeStamp;
            }
            set
            {
                mUpdateTimeStamps = value;
            }
        }


        /// <summary>
        /// If true, user IDs of the document (creator, modifier) are updated.
        /// </summary>
        public bool UpdateUser
        {
            get
            {
                return mUpdateUser && CMSActionContext.CurrentUpdateSystemFields;
            }
            set
            {
                mUpdateUser = value;
            }
        }


        /// <summary>
        /// Indicates if document name and URL paths should be updated.
        /// </summary>
        public bool UpdatePaths
        {
            get
            {
                return mUpdatePaths;
            }
            set
            {
                mUpdatePaths = value;
            }
        }


        /// <summary>
        /// If true, SKU columns of the document are updated.
        /// </summary>
        public bool UpdateSKUColumns
        {
            get
            {
                return mUpdateSKUColumns;
            }
            set
            {
                mUpdateSKUColumns = value;
            }
        }


        /// <summary>
        /// If true, document content is updated from internal objects.
        /// </summary>
        public bool UpdateDocumentContent
        {
            get
            {
                return mUpdateDocumentContent;
            }
            set
            {
                mUpdateDocumentContent = value;
            }
        }


        /// <summary>
        /// If true, events are log on document update.
        /// </summary>
        public bool LogEvents
        {
            get
            {
                return mLogEvents && CMSActionContext.CurrentLogEvents;
            }
            set
            {
                mLogEvents = value;
            }
        }


        /// <summary>
        /// If true, synchronization tasks are logged on document update.
        /// </summary>
        public bool LogSynchronization
        {
            get
            {
                return mLogSynchronization && CMSActionContext.CurrentLogSynchronization;
            }
            set
            {
                mLogSynchronization = value;
            }
        }


        /// <summary>
        /// If true, integration tasks are logged on document update.
        /// </summary>
        public bool LogIntegration
        {
            get
            {
                return mLogIntegration && CMSActionContext.CurrentLogIntegration;
            }
            set
            {
                mLogIntegration = value;
            }
        }


        /// <summary>
        /// If true, cache dependencies are touched on document update.
        /// </summary>
        public bool TouchCacheDependencies
        {
            get
            {
                return mTouchCacheDependencies && CMSActionContext.CurrentTouchCacheDependencies;
            }
            set
            {
                mTouchCacheDependencies = value;
            }
        }


        /// <summary>
        /// If true, web farm tasks are logged on the object update.
        /// </summary>
        public bool LogWebFarmTasks
        {
            get
            {
                return mLogWebFarmTasks && CMSActionContext.CurrentLogWebFarmTasks;
            }
            set
            {
                mLogWebFarmTasks = value;
            }
        }


        /// <summary>
        /// If true, all data are selected. Otherwise only base document data are selected.
        /// </summary>
        public bool SelectAllData
        {
            get
            {
                return mSelectAllData;
            }
            set
            {
                mSelectAllData = value;
            }
        }


        /// <summary>
        /// Indicates whether notifications are sent when content changes occur (document updated/inserted/deleted,...). By default it is set to TRUE.
        /// </summary>
        public bool EnableNotifications
        {
            get
            {
                return mEnableNotifications && CMSActionContext.CurrentSendNotifications;
            }
            set
            {
                mEnableNotifications = value;
            }
        }


        /// <summary>
        /// Indicates whether rating of a document should be updated. By default it is set to TRUE.
        /// </summary>
        public bool EnableRating
        {
            get
            {
                return mEnableRating && CMSActionContext.CurrentUpdateRating;
            }
            set
            {
                mEnableRating = value;
            }
        }


        /// <summary>
        /// Indicates whether asynchronous actions should be allowed (log synchronization tasks). By default it is set to TRUE.
        /// </summary>
        public bool AllowAsyncActions
        {
            get
            {
                return mAllowAsyncActions && CMSActionContext.CurrentAllowAsyncActions;
            }
            set
            {
                mAllowAsyncActions = value;
            }
        }


        /// <summary>
        /// Indicates if the document aliases should be generated.
        /// </summary>
        public virtual bool EnableDocumentAliases
        {
            get
            {
                return mEnableDocumentAliases && DocumentActionContext.CurrentGenerateDocumentAliases;
            }
            set
            {
                mEnableDocumentAliases = value;
            }
        }


        /// <summary>
        /// Indicates whether the ACL operations should be performed during the document operations.
        /// </summary>
        internal bool HandleACLs
        {
            get
            {
                return mHandleACLs && DocumentActionContext.CurrentHandleACLs;
            }
            set
            {
                mHandleACLs = value;
            }
        }


        /// <summary>
        /// Indicates whether NodeGroupID property should be set according to the parent value.
        /// </summary>
        public virtual bool UseParentNodeGroupID
        {
            get
            {
                if (mUseParentNodeGroupID == null)
                {
                    // Get the default settings
                    return SettingsKeyInfoProvider.GetBoolValue("CMSUseParentGroupIDForNewDocuments");
                }
                return mUseParentNodeGroupID.Value;
            }
            set
            {
                mUseParentNodeGroupID = value;
            }
        }


        /// <summary>
        /// If true, the document stays checked in on the insert operation
        /// </summary>
        public bool KeepCheckedInOnInsert
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if URL path should be updated.
        /// </summary>
        internal bool UpdateUrlPath
        {
            get
            {
                return mUpdateUrlPath;
            }
            set
            {
                mUpdateUrlPath = value;
            }
        }

        #endregion


        #region "Protected properties"


        /// <summary>
        /// Returns search provider object.
        /// </summary>
        protected virtual ISearchProvider ProviderObject
        {
            get
            {
                if (mProviderObject == null)
                {
                    var providerAssemblyName = SettingsHelper.AppSettings["CMSSearchProviderAssembly"];

                    // Use built-in provider if custom search provider is not specified
                    if (String.IsNullOrEmpty(providerAssemblyName) || providerAssemblyName.Equals("CMS.SearchProviderSQL", StringComparison.OrdinalIgnoreCase))
                    {
                        mProviderObject = new SearchProvider();
                    }
                    else
                    {
                        var fullName = providerAssemblyName + ".SearchProvider";
                        mProviderObject = ClassHelper.GetClass<ISearchProvider>(providerAssemblyName, fullName);
                        if (mProviderObject == null)
                        {
                            throw new InvalidOperationException($"The class {fullName} couldn't be loaded.");
                        }
                    }
                }
                return mProviderObject;
            }
        }


        /// <summary>
        /// If true, node name is automatically updated from the document name when needed
        /// </summary>
        internal bool UpdateNodeName
        {
            get
            {
                return mUpdateNodeName;
            }
            set
            {
                mUpdateNodeName = value;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>    
        public TreeProvider()
        {
            FilterOutDuplicates = false;
            MergeResults = false;
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="userInfo">Current user info object</param>
        public TreeProvider(IUserInfo userInfo)
        {
            FilterOutDuplicates = false;
            MergeResults = false;
            mUserInfo = (UserInfo)userInfo;
        }

        #endregion


        #region "Filtering"

        /// <summary>
        /// Removes nodes of default culture that are translated to required language.
        /// </summary>
        /// <param name="ds">Original DataSet object containing both default and required language versions</param>
        /// <param name="siteName">Documents site name</param>
        /// <param name="preferredCulture">Required culture code</param>
        /// <param name="contextCulture">Culture code that allows to override currently preferred culture</param>
        public static void FilterOutLocalizedVersions(DataSet ds, string siteName, string preferredCulture, string contextCulture = null)
        {
            // No data to filter, do not process
            if (DataHelper.DataSourceIsEmpty(ds))
            {
                return;
            }

            var defaultCulture = CultureHelper.GetDefaultCultureCode(siteName);

            // If default culture requested, do nothing
            if (string.Equals(defaultCulture, preferredCulture, StringComparison.InvariantCultureIgnoreCase))
            {
                return;
            }

            bool allCultures = preferredCulture == ALL_CULTURES;
            if (allCultures)
            {
                // Filter out preferred culture first (top priority documents)
                preferredCulture = !String.IsNullOrEmpty(contextCulture) ? contextCulture : CultureHelper.GetPreferredCulture();
            }

            // Go through all the tables
            foreach (DataTable dt in ds.Tables)
            {
                // If not all cultures, process the regular way
                if (!allCultures)
                {
                    FilterOutLocalizedRows(dt, preferredCulture);
                }
                else
                {
                    // Get the list of cultures
                    var cultures = !AllSites(siteName) ? CultureSiteInfoProvider.GetSiteCultureCodes(siteName) : DataHelper.GetUniqueValues(dt, "DocumentCulture", false);

                    FilterOutLocalizedRows(dt, preferredCulture);

                    // Filter out default culture (second priority documents)
                    if (!string.Equals(preferredCulture, defaultCulture, StringComparison.InvariantCultureIgnoreCase))
                    {
                        FilterOutLocalizedRows(dt, defaultCulture);
                    }

                    // Filter the other cultures (the rest - any existing culture)
                    if (cultures != null)
                    {
                        foreach (string culture in cultures)
                        {
                            // If not preferred or default, filter
                            if (!string.Equals(culture, preferredCulture, StringComparison.InvariantCultureIgnoreCase) && !string.Equals(culture, defaultCulture, StringComparison.InvariantCultureIgnoreCase))
                            {
                                FilterOutLocalizedRows(dt, culture);
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Removes nodes of any culture that are translated to required language.
        /// </summary>
        /// <param name="dt">DataTable to filter</param>
        /// <param name="preferredCulture">Required culture code</param>
        public static void FilterOutLocalizedRows(DataTable dt, string preferredCulture)
        {
            if (preferredCulture == null)
            {
                return;
            }

            var localizedRows = dt.Select("DocumentCulture = '" + SqlHelper.EscapeQuotes(preferredCulture) + "'");

            // For each localized row delete the corresponding default culture row                
            if (localizedRows.Length <= 0)
            {
                return;
            }

            // Create table of localized nodes
            Hashtable localized = new Hashtable();
            int nodeIdIndex = dt.Columns.IndexOf("NodeID");
            if (nodeIdIndex < 0)
            {
                throw new Exception("[TreeProvider.FilterOutLocalizedVersions]: NodeID column not found.");
            }

            foreach (DataRow localizedRow in localizedRows)
            {
                int nodeId = (int)localizedRow[nodeIdIndex];
                localized[nodeId] = localizedRow;
            }

            // Create list of rows to delete
            foreach (DataRow dr in dt.Rows)
            {
                int nodeId = (int)dr[nodeIdIndex];
                DataRow localizedRow = (DataRow)localized[nodeId];
                // If localized version exists, delete the other cultures
                if ((localizedRow != null) && (dr != localizedRow))
                {
                    dr.Delete();
                }
            }

            dt.AcceptChanges();
        }


        /// <summary>
        /// Filters out duplicate documents (linked ones).
        /// </summary>
        /// <param name="ds">Dataset containing the documents</param>
        public static void FilterOutDuplicateDocuments(DataSet ds)
        {
            if (ds == null)
            {
                return;
            }
            foreach (DataTable dt in ds.Tables)
            {
                // Get all links
                DataRow[] linkedRows = dt.Select("NodeLinkedNodeID IS NOT NULL");
                foreach (DataRow linkedRow in linkedRows)
                {
                    // Check whether row is still present in table
                    if ((linkedRow.RowState != DataRowState.Deleted) && (linkedRow.RowState != DataRowState.Detached))
                    {
                        // Get the matching rows
                        int nodeId = ValidationHelper.GetInteger(linkedRow["NodeLinkedNodeID"], 0);
                        string culture = ValidationHelper.GetString(linkedRow["DocumentCulture"], "");
                        DataRow[] originalRows = dt.Select("(NodeID = " + nodeId + " OR NodeLinkedNodeID = " + nodeId + ") AND DocumentCulture = '" + SqlHelper.EscapeQuotes(culture) + "'");

                        // If duplicate found, delete the duplicate items
                        if (originalRows.Length > 1)
                        {
                            DataRow keepRow = linkedRow;
                            foreach (DataRow duplicateRow in originalRows)
                            {
                                // Keep first not linked (original) node
                                if (duplicateRow["NodeLinkedNodeID"] == DBNull.Value)
                                {
                                    keepRow = duplicateRow;
                                    break;
                                }
                            }

                            // Delete the duplicate items
                            foreach (DataRow duplicateRow in originalRows)
                            {
                                if (duplicateRow != keepRow)
                                {
                                    dt.Rows.Remove(duplicateRow);
                                }
                            }
                        }
                    }
                }
            }

            // Accept DataSet changes
            ds.AcceptChanges();
        }

        #endregion


        #region "Search"

        /// <summary>
        /// Searches data and returns results.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="searchNodePath">Path of the starting node</param>
        /// <param name="cultureCode">Culture code</param>
        /// <param name="searchExpression">Search expression</param>
        /// <param name="searchMode">Search mode</param>
        /// <param name="searchChildNodes">Indicates if child nodes should be searched</param>
        /// <param name="classNames">List of class names to be searched in format application.class separated by semicolon.</param>
        /// <param name="filterResultsByReadPermission">Indicates if search results should be filtered by read permission for the current user. The default value is false</param>
        /// <param name="searchOnlyPublished">Indicates if only published documents should be searched</param>
        /// <param name="whereCondition">Where condition</param>
        /// <param name="orderBy">Order by clause</param>
        public DataSet Search(string siteName, string searchNodePath, string cultureCode, string searchExpression, SearchModeEnum searchMode, bool searchChildNodes = true, string classNames = null, bool filterResultsByReadPermission = false, bool searchOnlyPublished = false, string whereCondition = null, string orderBy = null)
        {
            return Search(siteName, searchNodePath, cultureCode, searchExpression, searchMode, searchChildNodes, classNames, filterResultsByReadPermission, searchOnlyPublished, whereCondition, orderBy, SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSCombineWithDefaultCulture"));
        }


        /// <summary>
        /// Searches specified node and returns results.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="searchNodePath">Path of the starting node</param>
        /// <param name="cultureCode">Culture code</param>
        /// <param name="searchExpression">Search expression</param>
        /// <param name="searchMode">Search mode</param>
        /// <param name="searchChildNodes">Indicates if child nodes should be searched</param>
        /// <param name="classNames">List of class names to be searched in format application.class separated by semicolon.</param>
        /// <param name="filterResultsByReadPermission">Indicates if search results should be filtered by read permission for the current user. The default value is false</param>
        /// <param name="searchOnlyPublished">Indicates if only published documents should be searched</param>
        /// <param name="whereCondition">Where condition</param>
        /// <param name="orderBy">Order by clause</param>
        /// <param name="combineWithDefaultCulture">Indicates if results will be combined with default culture</param>
        public DataSet Search(string siteName, string searchNodePath, string cultureCode, string searchExpression, SearchModeEnum searchMode, bool searchChildNodes, string classNames, bool filterResultsByReadPermission, bool searchOnlyPublished, string whereCondition, string orderBy, bool combineWithDefaultCulture)
        {
            // Log on site keywords
            if (PortalContext.ViewMode.IsLiveSite())
            {
                ModuleCommands.LogOnSiteKeywords(siteName, cultureCode, searchExpression);
            }

            // Remove "/%" from search node path
            if (searchNodePath.EndsWith("%", StringComparison.Ordinal))
            {
                searchNodePath = searchNodePath.Substring(0, searchNodePath.Length - 1);
            }
            if (searchNodePath.EndsWith("/", StringComparison.Ordinal) & (searchNodePath != "/"))
            {
                searchNodePath = searchNodePath.Substring(0, searchNodePath.Length - 1);
            }

            // Get starting node
            var startingNode = SelectSingleNode(siteName, searchNodePath, ALL_CULTURES, false, null, false);
            if (startingNode == null)
            {
                throw new Exception("[TreeProvider.Search]: The specified document with alias path '" + searchNodePath + "' does not exist.");
            }

            var searchAliasPath = startingNode.NodeAliasPath;
            if (searchChildNodes)
            {
                searchAliasPath = searchAliasPath.TrimEnd('/') + "/%";
            }

            // Search provider
            var results = ProviderObject.Search(siteName, searchAliasPath, cultureCode, searchExpression, searchMode, searchChildNodes, classNames, filterResultsByReadPermission, searchOnlyPublished, whereCondition, orderBy, combineWithDefaultCulture);

            // replace rows the user is not allowed to read (if it's required and if userID value is specified)
            if (filterResultsByReadPermission)
            {
                results = TreeSecurityProvider.FilterDataSetByPermissions(results, NodePermissionsEnum.Read, UserInfo);
            }

            // Get Data in current culture and combine with default culture if is set
            if (combineWithDefaultCulture)
            {
                FilterOutLocalizedVersions(results, siteName, cultureCode, PreferredCultureCode);
            }

            // Exclude all nodes from result, that are specified in CMSExcludeDocumentsFromSearch
            if (!String.IsNullOrEmpty(SettingsKeyInfoProvider.GetValue(siteName + ".CMSExcludeDocumentsFromSearch")))
            {
                RemoveCMSExcludeDocumentsFromSearch(ref results, siteName);
            }

            return results;
        }


        /// <summary>
        /// Searches attachment binary data for search expression.
        /// </summary>
        /// <param name="parameters">Search parameters</param>
        /// <param name="siteIds">Site Ids</param>
        /// <returns>DataSet with result</returns>
        /// <remarks>This method allows constraining the search to given <see cref="SearchParameters.ClassNames"/>.</remarks>
        public DataSet AttachmentSearch(SearchParameters parameters, List<int> siteIds)
        {
            // Get SQL clauses
            SearchQueryClauses clauses = SearchManager.GetQueryClauses(parameters.SearchFor);
            string sqlQuery = String.Empty;

            // Check if clauses exists
            if (clauses != null)
            {
                // Get SQL query
                sqlQuery = clauses.GetQuery();

                // Get highlighted words
                SearchContext.Highlights = clauses.HighlightedWords;
            }

            // Check whether search words are defined
            if (String.IsNullOrEmpty(sqlQuery))
            {
                return null;
            }

            DataSet result = null;

            // Trim search expression
            sqlQuery = sqlQuery.Trim();

            if (!String.IsNullOrEmpty(sqlQuery))
            {
                #region "Where condition"

                // Process class names
                var className = parameters.ClassNames;
                string classes = String.Empty;
                if (!String.IsNullOrEmpty(className) && (className != "##ALL##"))
                {
                    string[] classArr = className.Split(';');
                    foreach (string currentClass in classArr)
                    {
                        classes += "'" + currentClass + "',";
                    }

                    classes = classes.TrimEnd(',');
                    if (!String.IsNullOrEmpty(classes))
                    {
                        classes = "ClassName IN(" + classes + ")";
                    }
                }

                // Trim order by
                var orderBy = ValidationHelper.GetString(parameters.AttachmentOrderBy, String.Empty).Trim();

                // Trim where condition
                var where = ValidationHelper.GetString(parameters.AttachmentWhere, String.Empty).Trim();

                if (!String.IsNullOrEmpty(where))
                {
                    // Add bracket envelope to where condition
                    where = "(" + where + ")";
                }

                // Class names
                if (!String.IsNullOrEmpty(classes))
                {
                    if (!String.IsNullOrEmpty(where))
                    {
                        where += " AND ";
                    }

                    where += classes;
                }

                // Exclude attachments of documents which are excluded from search
                if (!String.IsNullOrEmpty(where))
                {
                    where += " AND ";
                }

                where += "((" + SystemViewNames.View_CMS_Tree_Joined + ".DocumentSearchExcluded IS NULL) OR (" + SystemViewNames.View_CMS_Tree_Joined + ".DocumentSearchExcluded = 0))";

                // Path
                var path = parameters.Path;
                if (!String.IsNullOrEmpty(path))
                {
                    if (!String.IsNullOrEmpty(where))
                    {
                        where += " AND ";
                    }

                    where += "(" + SystemViewNames.View_CMS_Tree_Joined + ".NodeAliasPath LIKE N'" + SqlHelper.EscapeQuotes(path) + "')";
                }

                #endregion


                #region "Cultures"

                var culture = parameters.CurrentCulture;
                var combineWithDefaultCulture = parameters.CombineWithDefaultCulture;

                if (culture != ALL_CULTURES)
                {
                    if (!String.IsNullOrEmpty(where))
                    {
                        where += " AND ";
                    }

                    // Get result for combining with default culture
                    var defaultCulture = parameters.DefaultCulture;
                    if (combineWithDefaultCulture && (culture != defaultCulture))
                    {
                        where += "((DocumentCulture = N'" + culture + "') OR (DocumentCulture = N'" + defaultCulture + "'))";
                    }
                    // Get result for only one culture
                    else
                    {
                        where += "(DocumentCulture =  N'" + culture + "')";
                    }
                }

                #endregion


                #region "Sites where"

                // Add sites where condition
                if ((siteIds != null) && (siteIds.Count > 0))
                {
                    if (!String.IsNullOrEmpty(where))
                    {
                        where += " AND ";
                    }

                    where += "(NodeSiteID IN (";

                    // Modify WHERE condition
                    foreach (int siteId in siteIds)
                    {
                        where += siteId + ",";
                    }

                    where = where.TrimEnd(',');
                    where += ")) ";
                }

                #endregion


                #region "Search"

                // Set default order by (score)
                if (String.IsNullOrEmpty(orderBy))
                {
                    orderBy = "Score DESC";
                }

                // Prepare parameters
                var q =
                    AttachmentInfoProvider.GetAttachments()
                        // Search only main attachments to make sure search results do not include direct reference to variant
                        .ExceptVariants()
                        .Columns("DocumentID", "NodeID", "NodeAliasPath", "DocumentCulture", "NodeClassID", "ClassName", "NodeACLID", "NodeSiteID", "NodeLinkedNodeID", "NodeOwner")
                        .AddColumn(new AggregatedColumn(AggregationType.Sum, "RANK").As("Score"))
                        .Source(s => s
                            .InnerJoin("CONTAINSTABLE(CMS_Attachment, AttachmentBinary, " + sqlQuery + ") AS KEY_TBL", "CMS_Attachment.AttachmentID = KEY_TBL.[KEY]")
                            .InnerJoin(SystemViewNames.View_CMS_Tree_Joined, "View_CMS_Tree_Joined.DocumentID = CMS_Attachment.AttachmentDocumentID")
                        )
                        .Where(GetPublishedWhereCondition())
                        .Where(where)
                        .GroupBy("DocumentID", "NodeID", "NodeAliasPath", "DocumentCulture", "NodeClassID", "ClassName", "NodeACLID", "NodeSiteID", "NodeLinkedNodeID", "NodeOwner")
                        .OrderBy(orderBy);

                // Execute query and get results
                result = q.Result;

                // Filter result by permissions if required 
                if (parameters.CheckPermissions)
                {
                    result = TreeSecurityProvider.FilterDataSetByPermissions(result, NodePermissionsEnum.Read, (UserInfo)parameters.User);
                }

                // Filter result by culture
                if (combineWithDefaultCulture)
                {
                    FilterOutLocalizedVersions(result, ALL_SITES, culture, PreferredCultureCode);
                }

                #endregion
            }

            return result;
        }


        /// <summary>
        /// Filters results with AliasPath in CMSExcludeDocumentsFromSearch key.
        /// </summary>
        /// <param name="searchResultDS">Search results to filter</param>
        /// <param name="siteName">Site name</param>
        protected virtual void RemoveCMSExcludeDocumentsFromSearch(ref DataSet searchResultDS, string siteName)
        {
            if (siteName == null)
            {
                siteName = "";
            }

            // Get all document's aliaspaths to exclude
            var excludedAliasPaths = SettingsKeyInfoProvider.GetValue(siteName + ".CMSExcludeDocumentsFromSearch").Split(';');
            var rowsToRemove = new ArrayList();

            if (DataHelper.DataSourceIsEmpty(searchResultDS))
            {
                return;
            }

            // Process all results
            foreach (DataRow resultRow in searchResultDS.Tables[0].Rows)
            {
                var resultAliasPath = Convert.ToString(resultRow["NodeAliasPath"]).TrimEnd('/') + "/";

                foreach (string excludedPath in excludedAliasPaths)
                {
                    if (String.IsNullOrEmpty(excludedPath))
                    {
                        continue;
                    }

                    var excludedAliasPath = excludedPath;

                    // If excluded path contains %, it represents more than one node
                    if (excludedAliasPath.IndexOf("%", StringComparison.Ordinal) >= 0)
                    {
                        excludedAliasPath = excludedAliasPath.Substring(0, excludedAliasPath.Length - 1);
                        if (resultAliasPath.StartsWith(excludedAliasPath, StringComparison.InvariantCultureIgnoreCase))
                        {
                            rowsToRemove.Add(resultRow);
                        }
                    }
                    else
                    {
                        excludedAliasPath = excludedAliasPath.TrimEnd('/') + "/";
                        if (resultAliasPath.StartsWith(excludedAliasPath, StringComparison.InvariantCultureIgnoreCase))
                        {
                            rowsToRemove.Add(resultRow);
                        }
                    }
                }
            }

            foreach (DataRow row in rowsToRemove)
            {
                if (row.RowState != DataRowState.Deleted && row.RowState != DataRowState.Detached)
                {
                    searchResultDS.Tables[0].Rows.Remove(row);
                }
            }
            searchResultDS.Tables[0].AcceptChanges();
        }

        #endregion


        #region "Select single node (Document)"

        /// <summary>
        /// Returns single node specified by node GUID, culture and site name. Does not include the coupled data.
        /// </summary>
        /// <param name="nodeGuid">Node GUID</param>
        /// <param name="cultureCode">Culture code</param>
        /// <param name="siteName">Site name</param>
        public virtual TreeNode SelectSingleNode(Guid nodeGuid, string cultureCode, string siteName)
        {
            return SelectSingleNode<TreeNode>(nodeGuid, cultureCode, siteName, GetCombineWithDefaultCulture(siteName));
        }


        /// <summary>
        /// Returns single node specified by node GUID, culture and site name. Does not include the coupled data.
        /// </summary>
        /// <param name="nodeGuid">Node GUID</param>
        /// <param name="cultureCode">Culture code</param>
        /// <param name="siteName">Site name</param>
        /// <param name="combineWithDefaultCulture">Indicates whether node in default culture should be returned if node in specified culture was not found.</param>
        public virtual TreeNode SelectSingleNode(Guid nodeGuid, string cultureCode, string siteName, bool combineWithDefaultCulture)
        {
            return SelectSingleNode<TreeNode>(nodeGuid, cultureCode, siteName, combineWithDefaultCulture);
        }


        /// <summary>
        /// Returns single node specified by node ID and culture.
        /// </summary>
        /// <param name="nodeId">Node ID</param>
        /// <param name="cultureCode">Culture code</param>
        public virtual TreeNode SelectSingleNode(int nodeId, string cultureCode = null)
        {
            return SelectSingleNode(nodeId, cultureCode, CombineWithDefaultCulture);
        }


        /// <summary>
        /// Returns single node specified by node ID and culture, optionally combined with default culture.
        /// </summary>
        /// <param name="nodeId">Node ID to select</param>
        /// <param name="cultureCode">Document culture to select</param>
        /// <param name="combineWithDefaultCulture">Indicates whether node in default culture should be returned if node in specified culture was not found.</param>
        /// <param name="coupledData">If true, coupled data are also returned</param>
        public virtual TreeNode SelectSingleNode(int nodeId, string cultureCode, bool combineWithDefaultCulture, bool coupledData = true)
        {
            return SelectSingleNode<TreeNode>(nodeId, cultureCode, combineWithDefaultCulture, coupledData);
        }


        /// <summary>
        /// Returns single node specified by specified node ID, culture and class name. Most efficient way of getting the document. Result contains coupled data only if classNames are specified.
        /// </summary>
        /// <param name="nodeId">Node ID</param>
        /// <param name="culture">Culture code</param>
        /// <param name="classNames">List of class names separated by semicolon (e.g.: "cms.article;cms.product")</param>
        public virtual TreeNode SelectSingleNode(int nodeId, string culture, string classNames)
        {
            return SelectSingleNode(ALL_SITES, null, culture, false, classNames, "NodeID = " + nodeId, null, -1, false);
        }


        /// <summary>
        /// Returns single node specified by document ID. If the result is a link to another document original document is returned instead.
        /// </summary>
        /// <param name="documentId">Document ID</param>
        /// <param name="coupledData">If true, coupled data are also returned</param>
        /// <param name="columns">Columns to be selected</param>
        public virtual TreeNode SelectSingleDocument(int documentId, bool coupledData = true, string columns = null)
        {
            return SelectSingleDocument<TreeNode>(documentId, coupledData, columns);
        }


        /// <summary>
        /// Returns single node specified by alias path, culture and site name.
        /// </summary>
        /// <param name="siteName">Node site name</param>
        /// <param name="aliasPath">Node alias path</param>
        /// <param name="cultureCode">Culture code</param>
        public virtual TreeNode SelectSingleNode(string siteName, string aliasPath, string cultureCode)
        {
            return SelectSingleNode(siteName, aliasPath, cultureCode, GetCombineWithDefaultCulture(siteName));
        }


        /// <summary>
        /// Returns single node specified by alias path, culture and site name matching the provided parameters.
        /// </summary>
        /// <param name="siteName">Node site name</param>
        /// <param name="aliasPath">Node alias path</param>
        /// <param name="cultureCode">Node culture</param>
        /// <param name="combineWithDefaultCulture">Indicates whether node in default culture should be returned if node in specified culture was not found.</param>
        /// <param name="className">Node class name (e.g.: "cms.article")</param>
        /// <param name="selectOnlyPublished">Select only published nodes.</param>
        /// <param name="checkPermissions">Indicates whether permissions should be checked.</param>
        /// <param name="selectCoupledData">Indicates whether coupled data should be selected.</param>
        public virtual TreeNode SelectSingleNode(string siteName, string aliasPath, string cultureCode, bool combineWithDefaultCulture, string className = null, bool selectOnlyPublished = true, bool checkPermissions = false, bool selectCoupledData = true)
        {
            return SelectSingleNode<TreeNode>(siteName, aliasPath, cultureCode, combineWithDefaultCulture, className, selectOnlyPublished, checkPermissions, selectCoupledData);
        }


        /// <summary>
        /// Returns single node specified by alias path, culture and site name matching the provided parameters. Result contains coupled data only if classNames are specified.
        /// </summary>
        /// <param name="siteName">Nodes site name</param>
        /// <param name="aliasPath">Node alias path - may contain % and _ as wild card characters for any number of unknown characters or one unknown character respectively (for MS SQL).</param>
        /// <param name="cultureCode">Nodes culture code</param>
        /// <param name="combineWithDefaultCulture">Indicates whether node in default culture should be returned if node in specified culture was not found.</param>
        /// <param name="classNames">List of class names separated by semicolon (e.g.: "cms.article;cms.product")</param>
        /// <param name="where">Where condition to use for the data selection</param>
        /// <param name="orderBy">Order by clause to use for the data selection</param>
        /// <param name="maxRelativeLevel">Maximum child level of the selected nodes</param>
        /// <param name="selectOnlyPublished">Select only published nodes.</param>
        /// <param name="columns">Columns to be selected. Columns definition must contain mandatory columns (NodeID, NodeLinkedNodeID, DocumentCulture)</param>
        public virtual TreeNode SelectSingleNode(string siteName, string aliasPath, string cultureCode, bool combineWithDefaultCulture, string classNames, string where, string orderBy, int maxRelativeLevel, bool selectOnlyPublished, string columns = null)
        {
            return SelectSingleNode<TreeNode>(siteName, aliasPath, cultureCode, combineWithDefaultCulture, classNames, where, orderBy, maxRelativeLevel, selectOnlyPublished, null);
        }


        /// <summary>
        /// Returns single node matching the provided parameters.
        /// </summary>
        /// <param name="parameters">Parameters for the node selection</param>
        public virtual TreeNode SelectSingleNode(NodeSelectionParameters parameters)
        {
            return SelectSingleNode<TreeNode>(parameters);
        }

        #endregion


        #region "Select single node (generic)"

        /// <summary>
        /// Returns single node specified by node GUID, culture and site name. Does not include the coupled data.
        /// </summary>
        /// <param name="nodeGuid">Node GUID</param>
        /// <param name="cultureCode">Culture code</param>
        /// <param name="siteName">Site name</param>
        /// <param name="combineWithDefaultCulture">Indicates whether node in default culture should be returned if node in specified culture was not found.</param>
        protected virtual NodeType SelectSingleNode<NodeType>(Guid nodeGuid, string cultureCode, string siteName, bool combineWithDefaultCulture) where NodeType : TreeNode, new()
        {
            // Empty NodeGUID or empty culture code without combine with default culture are not allowed 
            if ((nodeGuid == Guid.Empty) || (String.IsNullOrEmpty(cultureCode) && !combineWithDefaultCulture))
            {
                return null;
            }

            // Prepare query
            var query = SelectNodes<NodeType>()
                        .WhereEquals(DocumentNodeDataInfo.TYPEINFO.GUIDColumn, nodeGuid)
                        .Published(false);

            // Use custom query name if set
            if (!String.IsNullOrEmpty(SelectQueryName))
            {
                query.QueryName = SelectQueryName;
            }

            // Add site condition
            if (!AllSites(siteName))
            {
                query.OnSite(siteName);
            }

            // Add culture condition
            if (cultureCode != ALL_CULTURES)
            {
                query.Culture(cultureCode)
                     .CombineWithDefaultCulture(combineWithDefaultCulture);
            }
            else
            {
                query.AllCultures();
            }

            // Return first node
            return query.FirstOrDefault();
        }


        /// <summary>
        /// Returns single node specified by node ID and culture.
        /// </summary>
        /// <param name="nodeId">Node ID</param>
        /// <param name="cultureCode">Culture code</param>
        /// <param name="combineWithDefaultCulture">Indicates whether node in default culture should be returned if node in specified culture was not found.</param>
        /// <param name="coupledData">Indicates whether coupled data should be contained in the result.</param>
        protected virtual NodeType SelectSingleNode<NodeType>(int nodeId, string cultureCode, bool combineWithDefaultCulture, bool coupledData) where NodeType : TreeNode, new()
        {
            if (nodeId <= 0)
            {
                return null;
            }

            // If cultureCode not set, use any culture
            if (cultureCode == null)
            {
                cultureCode = ALL_CULTURES;
                combineWithDefaultCulture = true;
            }

            NodeType result = null;

            // Get the node properties by selecting the base node record
            var nodeData = DocumentNodeDataInfoProvider.GetDocumentNodeDataInfo(nodeId);
            if (nodeData != null)
            {
                // Select the node
                result = SelectSingleNode<NodeType>(nodeData.NodeSiteName, nodeData.NodeAliasPath, cultureCode, combineWithDefaultCulture, nodeData.NodeClassName, false, false, coupledData);
            }

            // Return result    
            if (result != null)
            {
                result.TreeProvider = this;
            }

            return result;
        }


        /// <summary>
        /// Returns single node specified by document ID. If the result is a link to another document original document is returned instead.
        /// </summary>
        /// <param name="documentId">Document ID</param>
        /// <param name="coupledData">Indicates whether coupled data should be contained in the result.</param>
        /// <param name="columns">Columns to be selected</param>
        protected virtual NodeType SelectSingleDocument<NodeType>(int documentId, bool coupledData, string columns) where NodeType : TreeNode, new()
        {
            if (documentId <= 0)
            {
                return null;
            }

            // Prepare query
            var data = SelectNodes()
                .All()
                .TopN(1)
                .WhereID("DocumentID", documentId)
                .WhereNull("NodeLinkedNodeID");

            // Don't use no expand to efficiently leverage base table index for selection by DocumentID
            data.Properties.SourceSettings.UseNoExpand = false;

            // Set up query type if coupled data is requested
            if (coupledData)
            {
                var type = TreePathUtils.GetClassNameByDocumentID(documentId);
                if (type != null)
                {
                    // Request document type
                    data.ClassName = type;
                }
            }

            if (columns != null)
            {
                // Class name needs to be always included to initialize node instance correctly
                var cols = SqlHelper.AddColumns(columns, "ClassName");
                data.Columns(cols);
            }

            // No data found
            if (DataHelper.DataSourceIsEmpty(data))
            {
                return null;
            }

            // Create new instance
            var row = data.Tables[0].Rows[0];
            var className = row["ClassName"].ToString();
            return TreeNode.New<NodeType>(className, row, this);
        }


        /// <summary>
        /// Returns single node specified by alias path, culture and site name.
        /// </summary>
        /// <param name="siteName">Node site name</param>
        /// <param name="aliasPath">Node alias path</param>
        /// <param name="cultureCode">Culture code</param>
        /// <param name="combineWithDefaultCulture">Indicates whether node in default culture should be returned if node in specified culture was not found.</param>
        /// <param name="className">Node class name (e.g.: "cms.article")</param>
        /// <param name="selectOnlyPublished">Select only published nodes.</param>
        /// <param name="checkPermissions">Indicates whether permissions should be checked.</param>
        /// <param name="selectCoupledData">Indicates whether coupled data should be contained in the result.</param>
        protected virtual NodeType SelectSingleNode<NodeType>(string siteName, string aliasPath, string cultureCode, bool combineWithDefaultCulture, string className, bool selectOnlyPublished, bool checkPermissions, bool selectCoupledData) where NodeType : TreeNode, new()
        {
            if (!LicenseHelper.LicenseVersionCheck(RequestContext.CurrentDomain, FeatureEnum.Blogs, ObjectActionEnum.Edit))
            {
                LicenseHelper.GetAllAvailableKeys(FeatureEnum.Blogs);
            }

            if (!LicenseHelper.LicenseVersionCheck(RequestContext.CurrentDomain, FeatureEnum.Documents, ObjectActionEnum.Edit))
            {
                LicenseHelper.GetAllAvailableKeys(FeatureEnum.Documents);
            }

            NodeType result = null;


            DataSet nodeDS = null;
            aliasPath = SqlHelper.EscapeLikeQueryPatterns(aliasPath);

            // Get node class name if not specified
            if (String.IsNullOrEmpty(className))
            {
                // If root, use root class name
                if (aliasPath == "/")
                {
                    className = SystemDocumentTypes.Root;

                    if (!selectCoupledData)
                    {
                        // Get base node to retrieve the className
                        nodeDS = SelectNodes(siteName, aliasPath, cultureCode, combineWithDefaultCulture, null, null, null, -1, selectOnlyPublished, 1);
                    }
                }
                else
                {
                    // Get base node to retrieve the className
                    nodeDS = SelectNodes(siteName, aliasPath, cultureCode, combineWithDefaultCulture, null, null, null, -1, selectOnlyPublished, 1);
                    if (!DataHelper.DataSourceIsEmpty(nodeDS))
                    {
                        className = (string)nodeDS.Tables[0].Rows[0]["ClassName"];
                    }
                }
            }

            // Get the data
            if (!String.IsNullOrEmpty(className))
            {
                // Select the full record data if required
                if (selectCoupledData)
                {
                    nodeDS = SelectNodes(siteName, aliasPath, cultureCode, combineWithDefaultCulture, className, null, null, -1, selectOnlyPublished, 1);
                }
                else if (nodeDS == null)
                {
                    nodeDS = SelectNodes(siteName, aliasPath, cultureCode, combineWithDefaultCulture, null, GetClassNamesWhereCondition(className), null, -1, selectOnlyPublished, 1);
                }

                if (!DataHelper.DataSourceIsEmpty(nodeDS))
                {
                    if (checkPermissions)
                    {
                        nodeDS = TreeSecurityProvider.FilterDataSetByPermissions(nodeDS, NodePermissionsEnum.Read, UserInfo);
                    }

                    if (!DataHelper.DataSourceIsEmpty(nodeDS))
                    {
                        DataTable dt = nodeDS.Tables[0];
                        DataRow nodeRow = dt.Rows[0];

                        // If combined and more rows selected, get the culture requested
                        if (combineWithDefaultCulture && (dt.Rows.Count > 1))
                        {
                            string bestCulture = cultureCode;
                            if (bestCulture == ALL_CULTURES)
                            {
                                bestCulture = PreferredCultureCode;
                            }

                            // Go through the table and find the default culture document
                            foreach (DataRow dr in dt.Rows)
                            {
                                if (string.Equals(dr["DocumentCulture"].ToString(), bestCulture, StringComparison.InvariantCultureIgnoreCase))
                                {
                                    nodeRow = dr;
                                    break;
                                }
                            }
                        }

                        // Create the node
                        result = TreeNode.New<NodeType>(className, nodeRow, this);
                    }
                }
            }


            // Return result    
            if (result != null)
            {
                result.TreeProvider = this;
            }

            return result;
        }


        /// <summary>
        /// Returns single node specified by alias path, culture and site name matching the provided parameters. Result contains coupled data only if classNames are specified.
        /// </summary>
        /// <param name="siteName">Node site name</param>
        /// <param name="aliasPath">Node alias path - may contain % and _ as wild card characters for any number of unknown characters or one unknown character respectively (for MS SQL).</param>
        /// <param name="cultureCode">Culture code</param>
        /// <param name="combineWithDefaultCulture">Indicates whether node in default culture should be returned if node in specified culture was not found.</param>
        /// <param name="classNames">List of class names separated by semicolon (e.g.: "cms.article;cms.product")</param>
        /// <param name="where">Where condition for the data selection</param>
        /// <param name="orderBy">Order by clause for the data selection</param>
        /// <param name="maxRelativeLevel">Maximum child level of the selected nodes</param>
        /// <param name="selectOnlyPublished">Select only published nodes.</param>
        /// <param name="columns">Columns to be selected</param>
        protected virtual NodeType SelectSingleNode<NodeType>(string siteName, string aliasPath, string cultureCode, bool combineWithDefaultCulture, string classNames, string where, string orderBy, int maxRelativeLevel, bool selectOnlyPublished, string columns) where NodeType : TreeNode, new()
        {
            // Prepare the parameters
            var parameters = new NodeSelectionParameters
            {
                SiteName = siteName,
                AliasPath = aliasPath,
                CultureCode = cultureCode,
                CombineWithDefaultCulture = combineWithDefaultCulture,
                ClassNames = classNames,
                Where = where,
                OrderBy = orderBy,
                MaxRelativeLevel = maxRelativeLevel,
                SelectOnlyPublished = selectOnlyPublished,
                Columns = columns,
                SelectSingleNode = true
            };

            return SelectSingleNode<NodeType>(parameters);
        }


        /// <summary>
        /// Returns single node matching the provided parameters. Result contains coupled data only if classNames are specified.
        /// </summary>
        /// <param name="parameters">Parameters for the node selection</param>
        protected virtual NodeType SelectSingleNode<NodeType>(NodeSelectionParameters parameters) where NodeType : TreeNode, new()
        {
            parameters.SelectSingleNode = true;

            // Get the data
            DataSet ds = SelectNodes(parameters);
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                // Create the node
                DataRow dr = ds.Tables[0].Rows[0];
                string className = ValidationHelper.GetString(dr["ClassName"], "");

                return TreeNode.New<NodeType>(className, dr, this);
            }
            return null;
        }

        #endregion


        #region "Select nodes"

        /// <summary>
        /// Gets the query for all published documents
        /// </summary>
        public virtual MultiDocumentQuery SelectNodes()
        {
            var query = TreeNodeProvider.GetDocuments()
                .Published()
                .PublishedVersion();
            query.Properties.TreeProvider = this;

            return query;
        }


        /// <summary>
        /// Gets the query for all published documents of specific type
        /// </summary>
        /// <param name="className">Class name representing document type</param>
        public virtual DocumentQuery SelectNodes(string className)
        {
            var query = TreeNodeProvider.GetDocuments(className)
                .Published()
                .PublishedVersion();
            query.Properties.TreeProvider = this;

            return query;
        }


        /// <summary>
        /// Gets the query for all published documents of specific type
        /// </summary>
        /// <typeparam name="NodeType">Type of the instances returned by the query.</typeparam>
        public virtual DocumentQuery<NodeType> SelectNodes<NodeType>()
            where NodeType : TreeNode, new()
        {
            var query = TreeNodeProvider.GetDocuments<NodeType>()
                .Published()
                .PublishedVersion();

            query.Properties.TreeProvider = this;

            return query;
        }


        /// <summary>
        /// Returns nodes specified by node ID, culture and site name. Multiple nodes are returned if ALL_CULTURES passed in cultureCode parameter. Does not select coupled data.
        /// </summary>
        /// <param name="nodeId">Node ID</param>
        /// <param name="cultureCode">Document culture - if null or empty string passed, ALL_CULTURES constant is used instead.</param>
        /// <param name="siteName">Site name</param>
        public virtual TypedDataSet SelectNodes(int nodeId, string cultureCode, string siteName)
        {
            if (nodeId <= 0)
            {
                return null;
            }

            bool combineWithDefaultCulture = GetCombineWithDefaultCulture(siteName);

            // If cultureCode not set, use any culture
            if (String.IsNullOrEmpty(cultureCode))
            {
                cultureCode = ALL_CULTURES;
                combineWithDefaultCulture = true;
            }

            // Prepare the parameters
            var parameters = new NodeSelectionParameters
            {
                SiteName = siteName,
                CultureCode = cultureCode,
                CombineWithDefaultCulture = combineWithDefaultCulture,
                Where = "(NodeID = " + nodeId + ")",
                SelectOnlyPublished = false
            };

            return SelectNodes(parameters);
        }


        /// <summary>
        /// Returns nodes specified by node alias path, culture and site name, optionally combined with default culture. Result contains coupled data only if classNames are specified.
        /// </summary>
        /// <param name="siteName">Nodes site name</param>
        /// <param name="aliasPath">Nodes alias path - may contain % and _ as wild card characters for any number of unknown characters or one unknown character respectively (for MS SQL).</param>
        /// <param name="cultureCode">Nodes culture code</param>
        /// <param name="combineWithDefaultCulture">Indicates whether node in default culture should be returned if node in specified culture was not found.</param>
        /// <param name="classNames">List of class names separated by semicolon (e.g.: "cms.article;cms.product")</param>
        /// <param name="where">Where condition for the data selection</param>
        /// <param name="orderBy">Order by clause for the data selection</param>
        /// <param name="maxRelativeLevel">Maximum child level of the selected nodes</param>
        /// <param name="selectOnlyPublished">Select only published nodes.</param>
        /// <param name="topN">Limits the number of returned items.</param>
        /// <param name="columns">Columns to be selected. Columns definition must contain mandatory columns (NodeID, NodeLinkedNodeID, DocumentCulture)</param>
        public virtual TypedDataSet SelectNodes(string siteName, string aliasPath, string cultureCode, bool combineWithDefaultCulture, string classNames = null, string where = null, string orderBy = null, int maxRelativeLevel = -1, bool selectOnlyPublished = true, int topN = 0, string columns = null)
        {
            // Prepare the parameters
            var parameters = new NodeSelectionParameters
            {
                SiteName = siteName,
                AliasPath = aliasPath,
                CultureCode = cultureCode,
                CombineWithDefaultCulture = combineWithDefaultCulture,
                ClassNames = classNames,
                Where = where,
                OrderBy = orderBy,
                MaxRelativeLevel = maxRelativeLevel,
                SelectOnlyPublished = selectOnlyPublished,
                TopN = topN,
                Columns = columns
            };

            return SelectNodes(parameters);
        }


        /// <summary>
        /// Returns nodes without coupled data specified by node alias path, culture and site name, optionally combined with default culture.
        /// </summary>
        /// <param name="siteName">Nodes site name</param>
        /// <param name="aliasPath">Nodes alias path - may contain % and _ as wild card characters for any number of unknown characters or one unknown character respectively (for MS SQL).</param>
        /// <param name="cultureCode">Nodes culture code</param>
        /// <param name="combineWithDefaultCulture">Indicates whether node in default culture should be returned if node in specified culture was not found.</param>
        /// <param name="classNames">List of class names separated by semicolon (e.g.: "cms.article;cms.product")</param>
        /// <param name="where">Where condition for the data selection</param>
        /// <param name="orderBy">Order by clause for the data selection</param>
        /// <param name="maxRelativeLevel">Maximum child level of the selected nodes</param>
        /// <param name="selectOnlyPublished">Select only published nodes.</param>
        /// <param name="relationshipWithNodeGuid">Select nodes that are related to node with this GUID.</param>
        /// <param name="relationshipName">Relationship name</param>
        /// <param name="relatedNodeIsOnTheLeftSide">Indicates whether the related node is located on the left side of the relationship</param>
        /// <param name="topN">Limits the number of returned items.</param>
        /// <param name="columns">Columns to be selected. Columns definition must contain mandatory columns (NodeID, NodeLinkedNodeID, DocumentCulture)</param>
        public virtual TypedDataSet SelectNodes(string siteName, string aliasPath, string cultureCode, bool combineWithDefaultCulture, string classNames, string where, string orderBy, int maxRelativeLevel, bool selectOnlyPublished, Guid relationshipWithNodeGuid, string relationshipName, bool relatedNodeIsOnTheLeftSide, int topN = 0, string columns = null)
        {
            // Prepare the parameters
            var parameters = new NodeSelectionParameters
            {
                SiteName = siteName,
                AliasPath = aliasPath,
                CultureCode = cultureCode,
                CombineWithDefaultCulture = combineWithDefaultCulture,
                ClassNames = classNames,
                Where = where,
                OrderBy = orderBy,
                MaxRelativeLevel = maxRelativeLevel,
                SelectOnlyPublished = selectOnlyPublished,
                TopN = topN,
                Columns = columns,
                RelationshipName = relationshipName,
                RelationshipNodeGUID = relationshipWithNodeGuid,
                RelationshipSide = relatedNodeIsOnTheLeftSide ? RelationshipSideEnum.Left : RelationshipSideEnum.Right
            };

            return SelectNodes(parameters);
        }


        /// <summary>
        /// Returns nodes matching the provided parameters. Result contains coupled data only if classNames are specified.
        /// </summary>
        /// <param name="parameters">Selection parameters</param>
        public virtual TypedDataSet SelectNodes(NodeSelectionParameters parameters)
        {
            if (parameters.CheckLicense)
            {
                if (!LicenseHelper.LicenseVersionCheck(RequestContext.CurrentDomain, FeatureEnum.Blogs, ObjectActionEnum.Edit))
                {
                    LicenseHelper.GetAllAvailableKeys(FeatureEnum.Blogs);
                }

                if (!LicenseHelper.LicenseVersionCheck(RequestContext.CurrentDomain, FeatureEnum.Documents, ObjectActionEnum.Edit))
                {
                    LicenseHelper.GetAllAvailableKeys(FeatureEnum.Documents);
                }
            }

            var nodesDataProvider = new NodesDataProvider(parameters)
            {
                SelectQueryName = SelectQueryName,
                MergeResults = MergeResults,
                SortMergedResults = SortMergedResults,
                SelectAllData = SelectAllData,
                PreferredCultureCode = PreferredCultureCode
            };

            return nodesDataProvider.GetDataSet();
        }


        /// <summary>
        /// Returns nodes count matching the provided parameters.
        /// </summary>
        /// <param name="siteName">Nodes site name</param>
        /// <param name="aliasPath">Nodes alias path - may contain % and _ as wild card characters for any number of unknown characters or one unknown character respectively (for MS SQL).</param>
        /// <param name="cultureCode">Nodes culture code</param>
        /// <param name="combineWithDefaultCulture">Indicates whether node in default culture should be counted in if node in specified culture was not found.</param>
        /// <param name="classNames">List of class names separated by semicolon (e.g.: "cms.article;cms.product")</param>
        /// <param name="where">Where condition for the data count</param>
        /// <param name="orderBy">Order by clause for the data counting</param>
        /// <param name="maxRelativeLevel">Maximum child level of the counted nodes</param>
        /// <param name="selectOnlyPublished">Count only published nodes.</param>
        public virtual int SelectNodesCount(string siteName, string aliasPath, string cultureCode, bool combineWithDefaultCulture, string classNames, string where, string orderBy, int maxRelativeLevel, bool selectOnlyPublished)
        {
            // Get the data
            DataSet ds = SelectNodes(siteName, aliasPath, cultureCode, combineWithDefaultCulture, classNames, where, orderBy, maxRelativeLevel, selectOnlyPublished, 0, DocumentColumnLists.SELECTNODES_REQUIRED_COLUMNS);
            return DataHelper.GetItemsCount(ds);
        }

        #endregion


        #region "Other methods"

        /// <summary>
        /// Returns true if the given classNames value represents all classes
        /// </summary>
        /// <param name="classNames">Class names</param>
        internal static bool AllClasses(string classNames)
        {
            return (classNames == ALL_CLASSNAMES);
        }


        /// <summary>
        /// Returns true if the given site name value represents all sites
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static bool AllSites(string siteName)
        {
            return String.IsNullOrEmpty(siteName) || (siteName == ALL_SITES);
        }


        /// <summary>
        /// Returns where condition for given culture.
        /// </summary>
        /// <param name="siteName">Nodes site name</param>
        /// <param name="cultureCode">Nodes culture code</param>
        /// <param name="combineWithDefaultCulture">Indicates whether node in default culture should be returned if node in specified culture was not found.</param>
        private static WhereCondition GetCultureWhereCondition(string siteName, string cultureCode, bool combineWithDefaultCulture)
        {
            var condition = new WhereCondition();
            // No culture condition for all cultures
            if (cultureCode == ALL_CULTURES)
            {
                return condition;
            }

            // Do not query all sites, take the global default culture
            if (AllSites(siteName))
            {
                siteName = null;
            }

            string defaultCulture = CultureHelper.GetDefaultCultureCode(siteName);

            condition = new WhereCondition().WhereEquals("DocumentCulture", cultureCode);

            if (combineWithDefaultCulture && (!String.Equals(cultureCode, defaultCulture, StringComparison.InvariantCultureIgnoreCase)))
            {
                condition.Or().WhereEquals("DocumentCulture", defaultCulture);
            }

            return condition;
        }


        /// <summary>
        /// Returns the complete where condition based on the given parameters.
        /// </summary> 
        /// <param name="siteName">Nodes site name</param>
        /// <param name="aliasPath">Node alias path - may contain % and _ as wild card characters for any number of unknown characters or one unknown character respectively (for MS SQL).</param>
        /// <param name="cultureCode">Nodes culture code</param>
        /// <param name="combineWithDefaultCulture">Indicates whether node in default culture should be returned if node in specified culture was not found.</param>
        /// <param name="where">Where condition for the data selection</param>
        /// <param name="maxRelativeLevel">Maximum child level of the selected nodes</param>
        /// <param name="selectOnlyPublished">Select only published nodes.</param>
        public static string GetCompleteWhereCondition(string siteName, string aliasPath, string cultureCode, bool combineWithDefaultCulture, string where, bool selectOnlyPublished, int maxRelativeLevel)
        {
            string oldWhere = where;
            where = "";

            // Add site condition
            if (!AllSites(siteName))
            {
                where = SqlHelper.AddWhereCondition(where, "NodeSiteID = " + SiteInfoProvider.GetSiteID(siteName));
            }

            // Add published condition
            if (selectOnlyPublished)
            {
                where = SqlHelper.AddWhereCondition(where, GetPublishedWhereCondition().ToString(true));
            }

            // Add culture condition
            if (cultureCode != ALL_CULTURES)
            {
                where = SqlHelper.AddWhereCondition(where, GetCultureWhereCondition(siteName, cultureCode, combineWithDefaultCulture).ToString(true));
            }

            // Add node level condition (only when selecting more nodes)
            if (aliasPath == null)
            {
                aliasPath = ALL_DOCUMENTS;
            }

            if (aliasPath.IndexOfAny(new[] { '%', '_', '[' }) >= 0)
            {
                int baseLevel = aliasPath.Split('/').GetUpperBound(0);
                if (maxRelativeLevel >= 0)
                {
                    int tmpNodeLevel = baseLevel + maxRelativeLevel - 1;
                    where = SqlHelper.AddWhereCondition(where, "NodeLevel <= " + tmpNodeLevel);
                }
            }

            // Add alias path condition
            where = SqlHelper.AddWhereCondition(where, TreePathUtils.GetAliasPathCondition(aliasPath).ToString(true));

            // Add the additional where condition
            where = SqlHelper.AddWhereCondition(where, oldWhere);

            // Return the result
            return where;
        }


        /// <summary>
        /// Gets the where condition for published documents
        /// </summary>
        public static WhereCondition GetPublishedWhereCondition()
        {
            var where = new WhereCondition();

            // Add the now parameter
            var now = new DynamicDataParameter("Now", () => DateTime.Now);

            where

                // AsLiteral - SQL server query passing performance optimization
                .WhereEquals("DocumentCanBePublished", true.AsLiteral())
                .And()
                .Where(w => w
                    .WhereNull("DocumentPublishFrom")
                    .Or()
                    .WhereLessOrEquals("DocumentPublishFrom", now)
                )
                .And()
                .Where(w => w
                    .WhereNull("DocumentPublishTo")
                    .Or()
                    .WhereGreaterOrEquals("DocumentPublishTo", now)
                );

            return where;
        }


        /// <summary>
        /// Returns where condition for given class names.
        /// </summary>
        /// <param name="classNames">List of class names separated by semicolon.</param>
        public static string GetClassNamesWhereCondition(string classNames)
        {
            if (String.IsNullOrEmpty(classNames))
            {
                return null;
            }

            // Create the where condition
            return new WhereCondition().WhereIn("ClassName", classNames.Split(';')).ToString(true);
        }


        /// <summary>
        /// Returns where condition for the given relationship or null if the relationship was not found.
        /// </summary>
        /// <param name="nodeId">ID of a node in the relationship</param>
        /// <param name="name">Collection name</param>
        /// <param name="documentIsOnLeftSide">If true, the document can be on left side of the relationship</param>
        /// <param name="documentIsOnRightSide">If true, the document can be on right side of the relationship</param>
        public static string GetRelationshipWhereCondition(int nodeId, string name, bool documentIsOnLeftSide, bool documentIsOnRightSide)
        {
            // Prepare where condition on relationship name
            string nameWhere = null;

            if (!String.Equals(name, "Any", StringComparison.InvariantCultureIgnoreCase))
            {
                // Get relationships for specified relationship name
                RelationshipNameInfo ri = RelationshipNameInfoProvider.GetRelationshipNameInfo(name);
                if (ri != null)
                {
                    // Documents related only within the given relationship
                    nameWhere = "RelationshipNameID = " + ri.RelationshipNameId;
                }
                else
                {
                    // Name not found (return no results)
                    return null;
                }
            }

            string leftSideWhere = null;

            // Left side where
            if (documentIsOnLeftSide)
            {
                leftSideWhere = SqlHelper.AddWhereCondition("LeftNodeID = " + nodeId, nameWhere);
                leftSideWhere = "NodeID IN (SELECT RightNodeID FROM CMS_Relationship WHERE " + leftSideWhere + ")";
            }

            string rightSideWhere = null;

            // Right side where
            if (documentIsOnRightSide)
            {
                rightSideWhere = SqlHelper.AddWhereCondition("RightNodeID = " + nodeId, nameWhere);
                rightSideWhere = "NodeID IN (SELECT LeftNodeID FROM CMS_Relationship WHERE " + rightSideWhere + ")";
            }


            return SqlHelper.AddWhereCondition(leftSideWhere, rightSideWhere, "OR");
        }


        /// <summary>
        /// Initializes the node order under specified parent node.
        /// </summary>
        /// <param name="parentId">Parent node ID</param>
        /// <param name="siteId">Site ID</param>
        public virtual void InitNodeOrders(int parentId, int siteId)
        {
            // Init the node orders to the proper sequence
            TreeNode node = TreeNode.New();

            node.NodeParentID = parentId;
            node.NodeSiteID = siteId;

            node.Generalized.InitObjectsOrder();
        }


        /// <summary>
        /// Moves specified node up in the order sequence (up = smaller NodeOrder = closer to beginning in the navigation).
        /// </summary>
        /// <param name="node">Node</param>
        public virtual void MoveNodeUp(TreeNode node)
        {
            if (node == null)
            {
                return;
            }

            var e = new DocumentChangeOrderEventArgs(node, -1, true);

            // Handle the event
            using (var h = DocumentEvents.ChangeOrder.StartEvent(e))
            {
                if (h.CanContinue())
                {
                    node.Generalized.MoveObjectUp();
                }

                // Finish the event
                h.FinishEvent();
            }
        }


        /// <summary>
        /// Moves specified node up in the order sequence (up = smaller NodeOrder = closer to beginning in the navigation).
        /// </summary>
        /// <param name="nodeId">Node ID</param>
        public virtual TreeNode MoveNodeUp(int nodeId)
        {
            var node = SelectSingleNode(nodeId, ALL_CULTURES);

            MoveNodeUp(node);

            return node;
        }


        /// <summary>
        /// Moves specified node down in the order sequence (down = larger NodeOrder = further in the navigation).
        /// </summary>
        /// <param name="node">Node</param>
        public virtual void MoveNodeDown(TreeNode node)
        {
            if (node == null)
            {
                return;
            }

            var e = new DocumentChangeOrderEventArgs(node, 1, true);

            // Handle the event
            using (var h = DocumentEvents.ChangeOrder.StartEvent(e))
            {
                if (h.CanContinue())
                {
                    node.Generalized.MoveObjectDown();
                }

                // Finish the event
                h.FinishEvent();
            }
        }


        /// <summary>
        /// Moves specified node down in the order sequence (down = larger NodeOrder = further in the navigation).
        /// </summary>
        /// <param name="nodeId">Node ID</param>
        public virtual TreeNode MoveNodeDown(int nodeId)
        {
            var node = SelectSingleNode(nodeId, ALL_CULTURES);
            MoveNodeDown(node);

            return node;
        }


        /// <summary>
        /// Sets the specified node order.
        /// </summary>
        /// <param name="node">Node</param>
        /// <param name="newOrder">New node order</param>
        /// <returns>Returns new node order.</returns>
        public virtual void SetNodeOrder(TreeNode node, int newOrder)
        {
            if (node == null)
            {
                return;
            }

            var e = new DocumentChangeOrderEventArgs(node, newOrder);

            // Handle the event
            using (var h = DocumentEvents.ChangeOrder.StartEvent(e))
            {
                if (h.CanContinue())
                {
                    node.Generalized.SetObjectOrder(newOrder);
                }

                // Finish the event
                h.FinishEvent();
            }
        }


        /// <summary>
        /// Sets the specified node order.
        /// </summary>
        /// <param name="nodeId">Node ID</param>
        /// <param name="newOrder">New node order</param>
        /// <returns>Returns new node order.</returns>
        public virtual void SetNodeOrder(int nodeId, int newOrder)
        {
            // Set node order
            var node = SelectSingleNode(nodeId, ALL_CULTURES);
            SetNodeOrder(node, newOrder);
        }


        /// <summary>
        /// Sets the specified node order.
        /// </summary>
        /// <param name="node">Node</param>
        /// <param name="newOrder">New node order</param>
        /// <returns>Returns new node order.</returns>
        public virtual void SetNodeOrder(TreeNode node, DocumentOrderEnum newOrder = DocumentOrderEnum.Unknown)
        {
            // Do not change node order when restoring from the CI repository
            if ((node == null) || RepositoryActionContext.CurrentIsRestoreOperationRunning)
            {
                return;
            }

            // Ensure new order type if not provided
            if (newOrder == DocumentOrderEnum.Unknown)
            {
                newOrder = TreePathUtils.NewDocumentOrder(node.NodeSiteName);
            }

            var e = new DocumentChangeOrderEventArgs(node, newOrder);

            // Handle the event
            using (var h = DocumentEvents.ChangeOrder.StartEvent(e))
            {
                if (h.CanContinue())
                {
                    // Set the node order
                    switch (newOrder)
                    {
                        case DocumentOrderEnum.First:
                            node.Generalized.SetObjectOrder(1);
                            break;

                        case DocumentOrderEnum.Last:
                            node.Generalized.SetObjectOrder(node.Generalized.GetLastObjectOrder());
                            break;

                        case DocumentOrderEnum.Alphabetical:
                            node.Generalized.SetObjectAlphabeticalOrder();
                            break;
                    }
                }

                // Finish the event
                h.FinishEvent();
            }
        }


        /// <summary>
        /// Sets the specified node order.
        /// </summary>
        /// <param name="nodeId">Node ID</param>
        /// <param name="newOrder">New node order</param>
        /// <returns>Returns new node order.</returns>
        public virtual void SetNodeOrder(int nodeId, DocumentOrderEnum newOrder)
        {
            // Init node orders under the parent
            var node = SelectSingleNode(nodeId, ALL_CULTURES);

            SetNodeOrder(node, newOrder);
        }


        /// <summary>
        /// Sort child nodes alphabetically by document name.
        /// </summary>
        /// <param name="parentNodeId">Parent node ID</param>
        /// <param name="siteId">Node site ID</param>
        /// <param name="ascending">Indicates whether the alphabetical order should be ascending or descending.</param>
        public virtual void SortNodesAlphabetically(int parentNodeId, int siteId, bool ascending)
        {
            var e = new DocumentSortEventArgs(parentNodeId, ascending, DocumentSortEnum.Alphabetically);

            // Handle the event
            using (var h = DocumentEvents.Sort.StartEvent(e))
            {
                if (h.CanContinue())
                {
                    TreeNode node = TreeNode.New();

                    node.NodeParentID = parentNodeId;
                    node.NodeSiteID = siteId;

                    node.Generalized.SortAlphabetically(ascending, "NodeOrder", "DocumentName");
                }

                // Finish the event
                h.FinishEvent();
            }
        }


        /// <summary>
        /// Sort child nodes by the date of last modification.
        /// </summary>
        /// <param name="parentNodeId">Parent node ID</param>
        /// <param name="siteId">Node site ID</param>
        /// <param name="ascending">Indicates whether the sort should be from older to newer or vice versa.</param>
        public virtual void SortNodesByDate(int parentNodeId, int siteId, bool ascending)
        {
            var e = new DocumentSortEventArgs(parentNodeId, ascending, DocumentSortEnum.ByDate);

            // Handle the event
            using (var h = DocumentEvents.Sort.StartEvent(e))
            {
                if (h.CanContinue())
                {
                    TreeNode node = TreeNode.New();

                    node.NodeParentID = parentNodeId;
                    node.NodeSiteID = siteId;

                    node.Generalized.SortAlphabetically(ascending, "NodeOrder", "DocumentModifiedWhen");
                }

                // Finish the event
                h.FinishEvent();
            }
        }


        /// <summary>
        /// Deletes site tree root. For purposes of site deletion.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public virtual void DeleteSiteTree(string siteName)
        {
            DocumentHelper.DeleteSiteTree(siteName, this);
        }


        /// <summary>
        /// Creates site tree root. For purposes of site creation.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <returns>Root node</returns>
        public virtual TreeNode CreateSiteRoot(string siteName)
        {
            return CreateSiteRoot(new SiteRootCreationSettings(siteName));
        }


        /// <summary>
        /// Creates site tree root. For purposes of site creation.
        /// </summary>
        /// <param name="settings">Settings to be used for site root creation</param>
        /// <returns>Root node</returns>
        internal TreeNode CreateSiteRoot(SiteRootCreationSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            // Get the site
            var siteName = settings.SiteName;
            var si = SiteInfoProvider.GetSiteInfo(siteName);
            if (si == null)
            {
                throw new Exception("[TreeProvider.CreateSiteRoot]: Site '" + siteName + "' not found.");
            }

            // Get root class info
            var rootClass = DataClassInfoProviderBase<DataClassInfoProvider>.GetDataClassInfo(SystemDocumentTypes.Root);
            if (rootClass == null)
            {
                throw new Exception("[TreeProvider.CreateSiteRoot]: Class for root document type not found.");
            }

            ClassSiteInfoProvider.AddClassToSite(rootClass.ClassID, si.SiteID);
            CultureSiteInfoProvider.AddCultureToSite(settings.Culture, siteName);

            var tree = new TreeProvider(MembershipContext.AuthenticatedUser);
            var rootNode = TreeNode.New(SystemDocumentTypes.Root, tree);

            rootNode.NodeSiteID = si.SiteID;
            rootNode.DocumentCulture = settings.Culture;

            // NodeGUID is stable across web sites to keep a stable root reference for synchronization
            rootNode.NodeGUID = settings.NodeGUID;

            // DocumentGUID is stable across web sites to keep a stable root culture version reference for synchronization
            rootNode.DocumentGUID = settings.DocumentGUID;

            rootNode.Insert(null);

            // Ensure new ACL for the root
            if (HandleACLs)
            {
                var acl = AclInfoProvider.CreateNewAcl(si.SiteID);

                // ACL GUID is stable across web sites to keep a stable root ACL reference for synchronization
                acl.ACLGUID = settings.ACLGUID;
                acl.Insert();

                rootNode.NodeACLID = acl.ACLID;
                rootNode.NodeIsACLOwner = true;

                // Do not log events
                using (new CMSActionContext { LogEvents = false })
                {
                    rootNode.Update();
                }
            }

            // Return created root node
            return rootNode;
        }


        /// <summary>
        /// Changes default culture of the site.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="newCultureCode">New default culture code</param>
        public virtual void ChangeSiteDefaultCulture(string siteName, string newCultureCode)
        {
            // Get the old culture
            string oldCultureCode = SettingsKeyInfoProvider.GetValue(siteName + ".CMSDefaultCultureCode");

            ChangeSiteDefaultCulture(siteName, newCultureCode, oldCultureCode);
        }


        /// <summary>
        /// Changes default culture of the site. Removes old culture from the site and adds the new one. Changes culture of the documents in the old culture to the new one. 
        /// Doesn't handle collisions of existing documents in new culture.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="newCultureCode">New default culture code</param>
        /// <param name="oldCultureCode">Old default culture code</param>
        public virtual void ChangeSiteDefaultCulture(string siteName, string newCultureCode, string oldCultureCode)
        {
            // Set default culture for the new site
            SettingsKeyInfoProvider.SetValue("CMSDefaultCultureCode", siteName, newCultureCode);

            // Add new culture to site and remove the old one
            try
            {
                CultureSiteInfoProvider.RemoveCultureFromSite(oldCultureCode, siteName);
            }
            catch
            {
            }

            if (!CultureSiteInfoProvider.IsCultureOnSite(newCultureCode, siteName))
            {
                CultureSiteInfoProvider.AddCultureToSite(newCultureCode, siteName);
            }

            // Change DocumentCulture of the documents that used the old one and do not exist in the new default culture
            ChangeCulture(siteName, oldCultureCode, newCultureCode);
        }


        /// <summary>
        /// Changes culture of the documents in old culture to the new one.
        /// If there is an existing document in new culture, the old culture version is not changed to the new one.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="currentCulture">Current culture</param>
        /// <param name="newCulture">New culture</param>
        public virtual void ChangeCulture(string siteName, string currentCulture, string newCulture)
        {
            DocumentCultureDataInfoProvider.BulkUpdateData(
                new WhereCondition()
                    // Include all documents which exist on the target
                    .WhereEquals("DocumentCulture", currentCulture)
                    .WhereIn(
                        "DocumentNodeID",
                        DocumentNodeDataInfoProvider.GetDocumentNodes().OnSite(siteName)
                    )
                    // Do not include documents which already exist in the target culture
                    .WhereNotIn(
                        "DocumentNodeID",
                        DocumentHelper.GetDocuments()
                            .Column("DocumentNodeID")
                            .All()
                            .OnSite(siteName)
                            .WhereEquals("DocumentCulture", newCulture)
                    ),
                new Dictionary<string, object>
                {
                    // Change culture to the new culture
                    { "DocumentCulture", newCulture }
                }
            );
        }


        /// <summary>
        /// Deletes all links including child documents to the specified node.
        /// </summary>
        /// <param name="node">Document</param>
        public virtual void DeleteLinks(TreeNode node)
        {
            var parentLink = false;

            foreach (var link in EnumerateLinks(node))
            {
                // Skip already deleted link
                if (link == null)
                {
                    continue;
                }

                var aliasPath = link.NodeAliasPath;
                var siteId = link.NodeSiteID;
                if (node.NodeAliasPath.StartsWith(aliasPath + "/", StringComparison.InvariantCultureIgnoreCase) && node.NodeSiteID == siteId)
                {
                    parentLink = true;
                }
                else
                {
                    link.Delete();
                }
            }

            if (parentLink)
            {
                throw new Exception("[TreeProvider.DeleteLinks]: Cannot delete the page '" + HTMLHelper.HTMLEncode(node.DocumentNamePath) + "' which is child of its own link.");
            }
        }


        /// <summary>
        /// Returns the original node for given link node in specified culture.
        /// </summary>
        /// <param name="node">Tree node</param>
        /// <param name="cultureCode">Culture code</param>
        public virtual TreeNode GetOriginalNode(TreeNode node, string cultureCode = null)
        {
            if (node == null)
            {
                return null;
            }

            if (!node.IsLink)
            {
                return node;
            }

            if (cultureCode == null)
            {
                cultureCode = node.DocumentCulture;
            }

            // Get original node if linked
            int linkedNodeId = ValidationHelper.GetInteger(node.GetValue("NodeLinkedNodeID"), 0);
            var link = SelectSingleNode(linkedNodeId, cultureCode, true);
            if (link == null)
            {
                throw new NullReferenceException("[TreeProvider.GetOriginalNode]: The original node for a linked document doesn't exist.");
            }

            return link;
        }


        /// <summary>
        /// Creates document from given data.
        /// </summary>
        /// <param name="data">Node data</param>
        /// <param name="tree">Tree provider for the new document</param>
        public static TreeNode GetDocument(IDataContainer data, TreeProvider tree)
        {
            return GetDocument<TreeNode>(data, tree);
        }


        /// <summary>
        /// Creates document from given data.
        /// </summary>
        /// <param name="data">Node data</param>
        /// <param name="tree">Tree provider for the new document</param>
        public static NodeType GetDocument<NodeType>(IDataContainer data, TreeProvider tree) where NodeType : TreeNode, new()
        {
            if (data is NodeType)
            {
                return (NodeType)data;
            }

            // Create new node
            string className = ValidationHelper.GetString(data.GetValue("ClassName"), "");
            return TreeNode.New<NodeType>(className, data, tree);
        }


        ///<summary>
        /// Sets owner group of specified nodes
        ///</summary>
        ///<param name="nodeAliasPath">Alias path of node</param>
        ///<param name="nodeGroupId">ID of the owner group</param>
        ///<param name="siteId">Site ID</param>
        ///<param name="inheritOwnerGroup">Indicates whether owner group settings should be inherited by child nodes.</param>
        public virtual void ChangeCommunityGroup(string nodeAliasPath, int nodeGroupId, int siteId, bool inheritOwnerGroup)
        {
            if (String.IsNullOrEmpty(nodeAliasPath))
            {
                return;
            }

            // Prepare documents where condition
            var where =
                new WhereCondition()
                    .WhereEquals("NodeSiteID", siteId)
                    .Where(
                        TreePathUtils.GetPathWhereCondition(nodeAliasPath, inheritOwnerGroup ? PathTypeEnum.Section : PathTypeEnum.Single)
                    );

            // Set owner group for documents
            DocumentNodeDataInfoProvider.BulkUpdateData(
                where,
                new Dictionary<string, object> {
                    { "NodeGroupID", (nodeGroupId > 0) ? (object)nodeGroupId : null }
                }
            );
        }


        /// <summary>
        /// Clears the workflow information from the given tree node.
        /// </summary>
        /// <param name="node">Tree node</param>
        public static void ClearWorkflowInformation(TreeNode node)
        {
            if (node == null)
            {
                throw new Exception("[TreeProvider.ClearWorkflowInformation]: Missing TreeNode node.");
            }

            // Clear workflow information
            ClearCheckoutInformation(node);

            node.DocumentCheckedOutVersionHistoryID = 0;
            node.DocumentPublishedVersionHistoryID = 0;
            node.DocumentWorkflowStepID = 0;
            node.DocumentIsArchived = false;
            node.DocumentLastVersionNumber = null;
            node.DocumentLastPublished = DateTime.MinValue;
        }


        /// <summary>
        /// Clears the checkout information from the given tree node.
        /// </summary>
        /// <param name="node">Tree node</param>
        public static void ClearCheckoutInformation(TreeNode node)
        {
            if (node == null)
            {
                throw new Exception("[TreeProvider.ClearCheckoutInformation]: Missing TreeNode node.");
            }

            // Clear checkout information
            node.DocumentCheckedOutByUserID = 0;
            node.DocumentCheckedOutWhen = DateTime.MinValue;
            node.DocumentCheckedOutAutomatically = true;
        }


        /// <summary>
        /// Returns whether NodeGroupID property should be set according to the parent value.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public bool GetUseParentNodeGroupID(string siteName)
        {
            if (mUseParentNodeGroupID == null)
            {
                return SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSUseParentGroupIDForNewDocuments");
            }
            return mUseParentNodeGroupID.Value;
        }


        /// <summary>
        /// Returns whether nodes should be combined with appropriate nodes of default culture if not localized. This applies only when using multilingual support. The default value is false.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public virtual bool GetCombineWithDefaultCulture(string siteName)
        {
            if (mCombineWithDefaultCulture == null)
            {
                // Get the default combine settings
                return SiteInfoProvider.CombineWithDefaultCulture(siteName);
            }

            return mCombineWithDefaultCulture.Value;
        }


        /// <summary>
        /// Returns whether document permissions should be checked in the content management UI.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public bool CheckDocumentUIPermissions(string siteName)
        {
            return SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSCheckDocumentPermissions");
        }


        /// <summary>
        /// Ensures columns required for selection in given column list. Returns the adjusted list
        /// </summary>
        /// <param name="columns">List of columns</param>
        public static string EnsureRequiredColumns(string columns)
        {
            if (!String.IsNullOrEmpty(columns))
            {
                columns = SqlHelper.MergeColumns(columns, DocumentColumnLists.SELECTNODES_REQUIRED_COLUMNS);
            }

            return columns;
        }

        #endregion


        #region "Rating methods"

        /// <summary>
        /// Sets document new ratings.
        /// </summary>
        /// <param name="node">Document</param>
        /// <param name="newRating">New rating value</param>
        /// <param name="newNumOfRatings">Number of ratings</param>
        public static void SetRating(TreeNode node, double newRating, int newNumOfRatings)
        {
            // Document is missing
            if (node == null)
            {
                return;
            }

            // Do not update rating
            if (!node.TreeProvider.EnableRating)
            {
                return;
            }

            // Set rating values to the document
            node.DocumentRatings = newNumOfRatings;
            node.DocumentRatingValue = newRating;

            var changedColumns = node.ChangedColumns();

            // Update document data - do not modify user info and do not log events
            using (new DocumentActionContext { LogEvents = false, UpdateSystemFields = false })
            {
                node.Update(false);
            }

            // Update search index for document
            if (DocumentHelper.IsSearchTaskCreationAllowed(node) && DocumentHelper.SearchFieldChanged(node, changedColumns))
            {
                SearchTaskInfoProvider.CreateTask(SearchTaskTypeEnum.Update, TreeNode.OBJECT_TYPE, SearchFieldsConstants.ID, node.GetSearchID(), node.DocumentID);
            }
        }


        /// <summary>
        /// Resets rating values of given document.
        /// </summary>
        /// <param name="node">Document</param>
        public static void ResetRating(TreeNode node)
        {
            SetRating(node, 0, 0);

            // Raise event for rating reset
            if (DocumentEvents.ResetRating.IsBound)
            {
                DocumentEvents.ResetRating.StartEvent(node);
            }
        }


        /// <summary>
        /// Updates rating value of given document.
        /// </summary>
        /// <param name="node">Document</param>
        /// <param name="rating">Rating value</param>
        /// <param name="rememberRating">If true, rating is stored into cookie</param>
        public static void AddRating(TreeNode node, double rating, bool rememberRating)
        {
            // Document is missing
            if (node == null)
            {
                return;
            }

            // Do not update rating
            if (!node.TreeProvider.EnableRating)
            {
                return;
            }

            SetRating(node, node.DocumentRatingValue + rating, node.DocumentRatings + 1);

            // Store rated document ID into cookie if required
            if (rememberRating)
            {
                RememberRating(node);
            }
        }


        /// <summary>
        /// Remember that user has rated the document.
        /// </summary>
        /// <param name="node">Document</param>
        public static void RememberRating(TreeNode node)
        {
            // Get the value from the cookie
            string docs = CookieHelper.GetValue(CookieName.RatedDocuments) ?? "|";

            if (!docs.Contains("|" + node.DocumentID + "|"))
            {
                docs += node.DocumentID + "|";

                // Actualize the cookie
                CookieHelper.SetValue(CookieName.RatedDocuments, docs, DateTime.Now.AddYears(1));
            }
        }


        /// <summary>
        /// Sets a flag indicating that user rated the document.
        /// </summary>
        /// <param name="node">Document</param>
        public static bool HasRated(TreeNode node)
        {
            if (node == null)
            {
                return false;
            }

            // Check the content of the cookie
            string docs = CookieHelper.GetValue(CookieName.RatedDocuments);

            if ((docs != null) && (docs.Trim() != String.Empty))
            {
                return docs.Contains("|" + node.DocumentID + "|");
            }

            return false;
        }

        #endregion


        #region "Helper methods"

        /// <summary>
        /// Sets the query cultures to include proper combination of cultures for backward compatibility.
        /// Use only in case when your code uses string culture parameter, which may have TreeProvider.ALL_CULTURES value
        /// </summary>
        /// <param name="query">Query</param>
        /// <param name="cultureCode">Culture code</param>
        /// <param name="combineWithDefaultCulture">Combine with default culture</param>
        public static void SetQueryCultures(MultiDocumentQuery query, string cultureCode, bool combineWithDefaultCulture)
        {
            // Add culture condition
            if (cultureCode != ALL_CULTURES)
            {
                query.Culture(cultureCode)
                    .CombineWithDefaultCulture(combineWithDefaultCulture);
            }
            else
            {
                if (!combineWithDefaultCulture)
                {
                    query.AllCultures();
                }
                else
                {
                    query.CombineWithAnyCulture();
                }
            }
        }


        /// <summary>
        /// Handles related object of a document and its descendants when moving a document to a different site
        /// </summary>
        /// <param name="node">Moved document</param>
        /// <param name="targetSiteId">Target site ID</param>
        internal void ChangeRelatedObjectsSite(TreeNode node, int targetSiteId)
        {
            if (node == null)
            {
                return;
            }

            // Disable logging of the information about staging task preparation to the asynchronous log
            // Do not log smart search tasks for each related object. Partial rebuild of the index is performed at the end of document move action.
            using (new CMSActionContext { EnableLogContext = false, CreateSearchTask = false })
            {
                // Handle current document
                ChangeDocumentRelatedObjectsSite(node, targetSiteId);

                // Get the moved documents in all culture versions
                var documents = SelectNodes()
                    .All()
                    .OnSite(targetSiteId)
                    .Path(node.NodeAliasPath, PathTypeEnum.Section)
                    .OrderBy("NodeLevel");

                foreach (var doc in documents)
                {
                    // Skip already processed culture version of the current node
                    if ((doc.NodeID == node.NodeID) && doc.DocumentCulture.Equals(node.DocumentCulture, StringComparison.InvariantCultureIgnoreCase))
                    {
                        continue;
                    }

                    ChangeDocumentRelatedObjectsSite(doc, targetSiteId);
                }
            }
        }


        /// <summary>
        /// Handles related objects of a single document when moved to different site
        /// </summary>
        /// <param name="node">Document</param>
        /// <param name="targetSiteId">Target site ID</param>
        private void ChangeDocumentRelatedObjectsSite(TreeNode node, int targetSiteId)
        {
            // Perfrom actions only for standard document
            if (node.IsLink)
            {
                return;
            }

            int documentId = node.DocumentID;

            EnsureTemplate(node, false, targetSiteId);
            EnsureTemplate(node, true, targetSiteId);
            MoveAttachments(documentId, targetSiteId);
            RemoveCategories(documentId, targetSiteId);
            EnsureDocumentTags(node, targetSiteId);
        }


        /// <summary>
        /// Removes document invalid categories from the target site
        /// </summary>
        /// <param name="documentId">Document ID</param>
        /// <param name="targetSiteId">Target site ID</param>
        private void RemoveCategories(int documentId, int targetSiteId)
        {
            foreach (var category in EnumerateCategories(documentId, targetSiteId).Where(c => !c.Item2))
            {
                // Unlink category from the document
                DocumentCategoryInfoProvider.RemoveDocumentFromCategory(documentId, category.Item1);
            }
        }


        /// <summary>
        /// Moves attachments across the sites
        /// </summary>
        /// <param name="documentId">Document ID</param>
        /// <param name="targetSiteId">Target site ID</param>
        private void MoveAttachments(int documentId, int targetSiteId)
        {
            AttachmentInfoProvider.GetAttachments(documentId, true)
                                  .ForEachObject(a => AttachmentInfoProvider.MoveAttachment(a, targetSiteId));
        }


        /// <summary>
        /// Ensures the move action target site contains all of the tag groups and tags related to the document.
        /// </summary>
        /// <param name="node">Node to move</param>
        /// <param name="targetSiteId">Target site ID</param>
        /// <param name="handleTags">Indicates if tags should be handled (count updated etc.)</param>
        /// <param name="update">Indicates if the document should be updated if necessary</param>        
        internal void EnsureDocumentTags(TreeNode node, int targetSiteId, bool handleTags = true, bool update = true)
        {
            if (node == null)
            {
                return;
            }

            // Try to get node tag group ID, if document inherits tag group do nothing, otherwise ensure tag group on the target site
            int documentGroupId = node.DocumentTagGroupID;
            if (documentGroupId <= 0)
            {
                return;
            }

            var group = TagGroupInfoProvider.GetTagGroupInfo(documentGroupId);
            if (group == null)
            {
                return;
            }

            // Remove tags from current group
            if (handleTags)
            {
                DocumentTagInfoProvider.RemoveTags(documentGroupId, node.DocumentID, node.DocumentTags);
            }

            // Try to get tag group ID from the new site
            var targetGroup = TagGroupInfoProvider.GetTagGroupInfo(group.TagGroupName, targetSiteId);
            if (targetGroup != null)
            {
                // Set new tag group id if found
                node.DocumentTagGroupID = targetGroup.TagGroupID;
            }
            else
            {
                // Create new tag group
                var newTagGroup = new TagGroupInfo
                {

                    TagGroupDisplayName = group.TagGroupDisplayName,
                    TagGroupName = group.TagGroupName,
                    TagGroupDescription = group.TagGroupDescription,
                    TagGroupSiteID = targetSiteId,
                    TagGroupIsAdHoc = group.TagGroupIsAdHoc
                };

                TagGroupInfoProvider.SetTagGroupInfo(newTagGroup);

                // Set new tag group ID
                node.DocumentTagGroupID = newTagGroup.TagGroupID;
            }

            // Update the tag group
            if (update)
            {
                node.CultureData.Update();
            }

            // Add tags to new group
            if (handleTags)
            {
                DocumentTagInfoProvider.AddTags(node.DocumentTagGroupID, node.DocumentID, node.DocumentTags);
            }
        }


        /// <summary>
        /// Ensures correct page template for given document on the target site.
        /// </summary>
        /// <param name="node">Document</param>
        /// <param name="cultureSpecific">Indicates if culture specific template should be handled</param>
        /// <param name="targetSiteId">Target site ID</param>
        /// <param name="processedTemplates">Set of already processed template IDs</param>
        /// <param name="createCopyForAdhoc">Indicates if copy for ad-hoc template should be created</param>
        internal void EnsureTemplate(TreeNode node, bool cultureSpecific, int targetSiteId, IDictionary<int, int> processedTemplates = null, bool createCopyForAdhoc = false)
        {
            if (node == null)
            {
                return;
            }

            var columnName = cultureSpecific ? "DocumentPageTemplateID" : "NodeTemplateID";
            var pageTemplateId = node.GetIntegerValue(columnName, 0);
            if (pageTemplateId <= 0)
            {
                return;
            }

            var template = PageTemplateInfoProvider.GetPageTemplateInfo(pageTemplateId);
            if (template == null)
            {
                return;
            }

            // Disable unnecessary actions
            using (new CMSActionContext { EnableLogContext = false, TouchCacheDependencies = TouchCacheDependencies, LogWebFarmTasks = LogWebFarmTasks, CreateVersion = false, LogSynchronization = LogSynchronization })
            {
                // Ensure page template correction
                if (!template.IsReusable)
                {
                    if (createCopyForAdhoc)
                    {
                        int newTemplateId;
                        var processed = (processedTemplates != null) && processedTemplates.ContainsKey(pageTemplateId);
                        if (!processed)
                        {
                            // Clone template as ad-hoc to create new ad-hoc template
                            var templateCopy = PageTemplateInfoProvider.CloneTemplateAsAdHoc(template, template.DisplayName, targetSiteId, node.NodeGUID);
                            newTemplateId = templateCopy.PageTemplateId;

                            if (processedTemplates != null)
                            {
                                processedTemplates.Add(pageTemplateId, templateCopy.PageTemplateId);
                            }
                        }
                        else
                        {
                            // Use already processed template ID
                            newTemplateId = processedTemplates[template.PageTemplateId];
                        }

                        // Update the template ID
                        node.SetIntegerValue(columnName, newTemplateId, false);
                    }
                    else
                    {
                        // Template already processed
                        if (template.PageTemplateSiteID == targetSiteId)
                        {
                            return;
                        }

                        // Change site assignment of the ad-hoc page template
                        template.PageTemplateSiteID = targetSiteId;
                        template.PageTemplateGUID = Guid.NewGuid();

                        PageTemplateInfoProvider.SetPageTemplateInfo(template);
                    }
                }
                // Allow reusable page template for target site
                else
                {
                    PageTemplateSiteInfoProvider.AddPageTemplateToSite(pageTemplateId, targetSiteId);
                }
            }
        }


        /// <summary>
        /// Enumerates child documents of given document in best matching culture version based on these priorities:
        /// 1. Default culture version of the target site if provided
        /// 2. Default culture version of document site
        /// 3. document culture
        /// </summary>
        /// <param name="node">Document</param>
        /// <param name="targetSiteName">Target site name</param>
        /// <returns>Best matching culture version of each child document</returns>
        internal IEnumerable<TreeNode> EnumerateChildren(TreeNode node, string targetSiteName = null)
        {
            // Prioritize the culture versions to process only the best matching one
            var culturePriorities = GetCulturePriorities(node, targetSiteName);

            // Prepare base query
            var baseQuery = SelectNodes()
                .All()
                .AllCultures(false)
                .Culture(culturePriorities.ToArray())
                .CombineWithAnyCulture()
                .WhereEquals("NodeParentID", node.NodeID);

            var classNames = GetClassNames(baseQuery);
            if (classNames.Count == 0)
            {
                yield break;
            }

            var children = baseQuery
                .Types(classNames.ToArray())
                .WithCoupledColumns();

            children.PagedBy(PROCESSING_BATCH);
            do
            {
                foreach (var child in children)
                {
                    yield return child;
                }

                // Check if the next page is available
                if (children.NextPageAvailable)
                {
                    children = children.NextPage();
                }
                else
                {
                    // End iteration
                    break;
                }
            } while (true);
        }


        /// <summary>
        /// Gets list of document types in the result set of the query.
        /// </summary>
        /// <param name="query">Multi-document query</param>
        private static IList<string> GetClassNames(MultiDocumentQuery query)
        {
            return query
                    .Clone()
                    .Distinct()
                    .Column("ClassName")
                    .GetListResult<string>();
        }


        /// <summary>
        /// Enumerates descendants of given document in the same culture version
        /// </summary>
        /// <param name="node">Document</param>
        /// <param name="where">Where condition to limit the set of descendants</param>
        /// <returns>Culture versions of each descendant matching the culture of the given node</returns>
        internal IEnumerable<TreeNode> EnumerateDescendants(TreeNode node, WhereCondition where = null)
        {
            var descendants = SelectNodes()
                .All()
                .AllCultures(false)
                .CombineWithDefaultCulture(false)
                .Culture(node.DocumentCulture)
                .OnSite(node.NodeSiteID)
                .Path(node.NodeAliasPath, PathTypeEnum.Children)
                .Where(where)
                .OrderBy("NodeLevel");

            descendants.PagedBy(PROCESSING_BATCH);
            do
            {
                foreach (var descendant in descendants)
                {
                    yield return descendant;
                }

                // Check if the next page is available
                if (descendants.NextPageAvailable)
                {
                    descendants = descendants.NextPage();
                }
                else
                {
                    // End iteration
                    break;
                }
            } while (true);
        }


        /// <summary>
        /// Enumerates culture versions of given document in this specific order:
        /// 1. Default culture version of the target site if provided
        /// 2. Default culture version of document site
        /// 3. Document culture
        /// </summary>
        /// <param name="node">Document</param>
        /// <param name="targetSiteName">Target site name</param>
        /// <param name="reverse">Reverse the priorities</param>
        /// <returns>Culture versions of given document</returns>
        internal IEnumerable<TreeNode> EnumerateCultureVersions(TreeNode node, string targetSiteName = null, bool reverse = false)
        {
            // Prioritize the culture versions to process only the best matching one
            var culturePriorities = GetCulturePriorities(node, targetSiteName);

            // Get all culture versions - sorted by cultures
            var cultures = SelectNodes(node.NodeClassName)
                .All()
                .Where(GetCultureVersionsWhereCondition(node.NodeID))
                .OrderBy(reverse ? OrderDirection.Ascending : OrderDirection.Descending, DocumentQueryColumnBuilder.GetCulturePriorityColumn("DocumentCulture", culturePriorities).ToString())
                .OrderByAscending("DocumentCulture");

            foreach (var culture in cultures)
            {
                yield return culture;
            }
        }


        /// <summary>
        /// Gets where condition for all culture versions of a node.
        /// </summary>
        /// <param name="nodeId">Node ID</param>
        internal static WhereCondition GetCultureVersionsWhereCondition(int nodeId)
        {
            return new WhereCondition().WhereEquals("NodeID", nodeId);
        }


        /// <summary>
        /// Enumerates links of given document
        /// </summary>
        /// <param name="node">Document</param>
        /// <returns>Links of given document</returns>
        internal IEnumerable<TreeNode> EnumerateLinks(TreeNode node)
        {
            // Get all links
            var links = SelectNodes(node.NodeClassName)
                .All()
                .WhereEquals("NodeLinkedNodeID", node.NodeID);

            foreach (var link in links)
            {
                yield return link;
            }
        }


        /// <summary>
        /// Gets list of cultures prioritized by following priorities:
        /// 1. Default culture version of the target site if provided
        /// 2. Default culture version of document site
        /// 3. Document culture
        /// </summary>
        /// <param name="node">Document</param>
        /// <param name="targetSiteName">Target site name</param>
        private static List<string> GetCulturePriorities(TreeNode node, string targetSiteName)
        {
            var culturePriorities = new List<string>();
            if (!string.IsNullOrEmpty(targetSiteName))
            {
                culturePriorities.Add(CultureHelper.GetDefaultCultureCode(targetSiteName));
            }

            culturePriorities.Add(CultureHelper.GetDefaultCultureCode(node.NodeSiteName));
            culturePriorities.Add(node.DocumentCulture);
            return culturePriorities.Distinct(StringComparer.InvariantCultureIgnoreCase).ToList();
        }


        /// <summary>
        /// Copy node permissions
        /// </summary>
        /// <param name="sourceNode">Source node</param>
        /// <param name="targetNode">Target node</param>
        internal void CopyNodePermissions(TreeNode sourceNode, TreeNode targetNode)
        {
            if (!HandleACLs)
            {
                return;
            }

            bool ensureAclHierarchy = DocumentActionContext.CurrentPreserveACLHierarchy || (targetNode.NodeParentID == sourceNode.NodeParentID);
            AclInfoProvider.CopyAcl(sourceNode, targetNode, ensureAclHierarchy);
        }


        /// <summary>
        /// Handles document permission within move action
        /// </summary>
        /// <param name="node">Document node</param>
        internal void HandleMoveNodePermissions(TreeNode node)
        {
            if (!HandleACLs)
            {
                return;
            }

            // Break inheritance if permissions should be preserved
            if (DocumentActionContext.CurrentPreserveACLHierarchy)
            {
                AclInfoProvider.BreakInheritance(node, true);
            }
            else
            {
                var parent = node.Parent;
                if (parent == null)
                {
                    return;
                }

                var parentAclId = parent.NodeACLID;
                AclInfoProvider.ChangeAclId(node.NodeSiteID, node.NodeAliasPath, parentAclId);
                SetInheritedACL(node, parentAclId);
            }
        }


        /// <summary>
        /// Ensures given ACL is inherited by the given node
        /// </summary>
        /// <param name="node">Document node</param>
        /// <param name="aclId">ACL ID to be inherited by the node</param>
        internal void SetInheritedACL(TreeNode node, int aclId)
        {
            if (!HandleACLs)
            {
                return;
            }

            node.NodeIsACLOwner = false;
            node.NodeACLID = aclId;
        }


        /// <summary>
        /// Enumerates document categories
        /// </summary>
        /// <param name="documentId">Document ID</param>
        /// <param name="targetSiteId">Target site ID</param>
        /// <returns>Tuple where the first item is category ID and the second item indicates if the category is valid on target site.</returns>
        internal IEnumerable<Tuple<int, bool>> EnumerateCategories(int documentId, int targetSiteId)
        {
            var categories = DocumentCategoryInfoProvider.GetDocumentCategories(documentId)
                .Columns("CategoryID, CategorySiteID, CategoryUserID")
                .TypedResult;

            if (DataHelper.DataSourceIsEmpty(categories))
            {
                yield break;
            }

            // Check if destination site is using global categories
            bool globalCategoriesAllowed = SettingsKeyInfoProvider.GetBoolValue("cmsallowglobalcategories", targetSiteId);

            // For each category save new document category
            foreach (DataRow row in categories.Tables[0].Rows)
            {
                bool isValid;
                var categoryId = DataHelper.GetDataRowValue(row, "CategoryID").ToInteger(0);
                var siteId = DataHelper.GetDataRowValue(row, "CategorySiteID").ToInteger(0);
                var userId = DataHelper.GetDataRowValue(row, "CategoryUserID").ToInteger(0);

                // User category 
                if (userId > 0)
                {
                    // Check if user is assigned to destination site
                    isValid = UserSiteInfoProvider.GetUserSiteInfo(userId, targetSiteId) != null;
                }
                // Category is global
                else if (siteId == 0)
                {
                    // Check if global categories are allowed on destination site
                    isValid = globalCategoriesAllowed;
                }
                // Site category 
                else
                {
                    // Check if category is on destination site too
                    isValid = siteId == targetSiteId;
                }

                yield return new Tuple<int, bool>(categoryId, isValid);
            }
        }

        #endregion
    }
}