using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.IO;
using CMS.MacroEngine;
using CMS.SiteProvider;

namespace CMS.Search
{
    /// <summary>
    /// Provides the smart search methods.
    /// </summary>
    public static class SearchHelper
    {
        #region "Constants"

        /// <summary>
        /// General search index constant.
        /// </summary>
        public const string GENERALINDEX = "smartsearch.general";

        /// <summary>
        /// On-line forms search index constant.
        /// </summary>
        public const string ONLINEFORMINDEX = "cms.form";

        /// <summary>
        /// Guid value of the simple search setting item.
        /// </summary>
        public static Guid SIMPLE_ITEM_ID = new Guid("11111111-1111-1111-1111-111111111111");

        /// <summary>
        /// Constant for custom index data.
        /// </summary>
        public static Guid CUSTOM_INDEX_DATA = SIMPLE_ITEM_ID;

        /// <summary>
        /// Invariant field value.
        /// </summary>
        public const string INVARIANT_FIELD_VALUE = "invariantifieldivaluei";

        /// <summary>
        /// Max allowed length of path for place where indexes are stored.
        /// 260 is Windows limit, -1 for NULL, -16 for index file name, -1 for index name, -1 for slash 
        /// </summary>
        public const int MAX_INDEX_PATH = 241;


        /// <summary>
        /// Highlighting's starting tag marker.
        /// </summary>
        private const string STARTING_MARKER = "##HIGHL_START##";

        /// <summary>
        /// Highlighting's ending tag marker.
        /// </summary>
        private const string ENDING_MARKER = "##HIGHL_END##";

        /// <summary>
        /// Constant for custom search index.
        /// </summary>
        public const string CUSTOM_SEARCH_INDEX = "CUSTOM_SEARCH_INDEX";

        /// <summary>
        /// Constant for custom search index.
        /// </summary>
        public const string DOCUMENTS_CRAWLER_INDEX = "DOCUMENTS_CRAWLER_INDEX";

        /// <summary>
        /// Constant for default class search title column.
        /// </summary>
        public const string DEFAULT_SEARCH_TITLE_COLUMN = "DocumentName";

        /// <summary>
        /// Constant for default class search content column.
        /// </summary>
        public const string DEFAULT_SEARCH_CONTENT_COLUMN = "DocumentContent";

        /// <summary>
        /// Constant for default class search creation date column.
        /// </summary>
        public const string DEFAULT_SEARCH_CREATION_DATE_COLUMN = "DocumentCreatedWhen";

        #endregion


        #region "Variables"

        /// <summary>
        /// Finds only ":" special character - basic search.
        /// </summary>
        private static Regex mBasicEscaperRegEx;

        /// <summary>
        /// Regular expression for Image content.
        /// </summary>
        private static Regex mImageContentReplacer;

        /// <summary>
        /// Gets the regular expression for word splitting used in SubsetAnalyzer.
        /// </summary>
        private static Regex mWordRegex;

        /// <summary>
        /// XPath function to specify which webparts fields should be added to the search document content.
        /// </summary>
        private static string mSearchContentXpathValue;

        /// <summary>
        /// List of excluded fields from conversion, semicolon is used as separator.
        /// </summary>
        private static string mExcludedFieldsFromConversion;

        /// <summary>
        /// If true, performs the search only when the content part of the search expression is present.
        /// </summary>
        private static bool? mSearchOnlyWhenContentPresent;

        /// <summary>
        /// Custom search path.
        /// </summary>
        private static readonly StringAppSetting mCustomSearchPath = new StringAppSetting("CMSSearchIndexPath", null);

        /// <summary>
        /// Maximal field length.
        /// </summary>
        private static int? mMaxFieldLength;

        /// <summary>
        /// Maximal number of returned results.
        /// </summary>
        private static int? mMaxResults;

        /// <summary>
        /// Gets the maximum size of an attachment (in kB) which are processed with content extractors. 0 means unlimited size (default).
        /// If a positive number is set, attachments the size of which exceeds the value are not processed and their content is not indexed.
        /// </summary>
        private static int? mMaxAttachmentSize;

        /// <summary>
        /// Indicates whether content field should be stored in the index
        /// </summary>
        private static bool? mStoreContentField;

        /// <summary>
        /// Indicates whether diacritics should be removed for index field
        /// </summary>
        private static bool? mRemoveDiacriticsForIndexField;

        /// <summary>
        /// Collection of index Searchers. [key -> IndexSearcher]
        /// </summary>
        private static readonly CMSStatic<SafeDictionary<string, IIndexSearcher>> mSearchers = new CMSStatic<SafeDictionary<string, IIndexSearcher>>(() => new SafeDictionary<string, IIndexSearcher>());

        /// <summary>
        /// Gets the weight (from interval 0 - 1.0f) which is given to synonyms if the synonym expansion is used. 1.0f means the synonyms are equally important as original words.
        /// </summary>
        public static AppSetting<double> SynonymsWeight = new AppSetting<double>("CMSSearchSynonymsWeight", 0.9f, ValidationHelper.GetDoubleSystem);

        /// <summary>
        /// Indicates if the search indexes are stored on external storage.
        /// </summary>
        private static bool? mIndexesInSharedStorage;

        #endregion


        #region "Events"

        /// <summary>
        /// An event raised upon <see cref="CreatingDefaultSearchSettings"/> execution. Custom search field flags can be added to <see cref="CreateDefaultSearchSettingsEventArgs.SearchSettings"/>
        /// being created.
        /// </summary>
        public static CreateDefaultSearchSettingsHandler CreatingDefaultSearchSettings = new CreateDefaultSearchSettingsHandler { Name = nameof(SearchHelper) + "." + nameof(CreatingDefaultSearchSettings) };

        #endregion


        #region "Properties"

        /// <summary>
        /// Collection of index Searchers. [key -> IndexSearcher]
        /// </summary>
        public static SafeDictionary<string, IIndexSearcher> Searchers
        {
            get
            {
                return mSearchers;
            }
        }


        /// <summary>
        /// Gets the maximum size of an attachment (in kB) which are processed with content extractors. 0 means unlimited size (default).
        /// If a positive number is set, attachments the size of which exceeds the value are not processed and their content is not indexed.
        /// </summary>
        public static int MaxAttachmentSize
        {
            get
            {
                if (mMaxAttachmentSize == null)
                {
                    mMaxAttachmentSize = ValidationHelper.GetInteger(SettingsHelper.AppSettings["CMSSearchMaxAttachmentSize"], 0);
                }

                return mMaxAttachmentSize.Value;
            }
        }


        /// <summary>
        /// Gets the value that is used for maximal number of records returned from search index.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This option doesn't affect information about number of available results.
        /// </para>
        /// <para>
        /// Default value is set to 1000 items per search request. This value can be changed by config key CMSSearchMaxResultsNumber but always consider impact on search performance.
        /// </para>
        /// </remarks>
        public static int MaxResults
        {
            get
            {
                if (mMaxResults == null)
                {
                    mMaxResults = ValidationHelper.GetInteger(SettingsHelper.AppSettings["CMSSearchMaxResultsNumber"], 1000);
                }

                return mMaxResults.Value;
            }
        }


        /// <summary>
        /// Gets the value that indicates whether diacritics should be removed for index field
        /// </summary>
        public static bool RemoveDiacriticsForIndexField
        {
            get
            {
                if (mRemoveDiacriticsForIndexField == null)
                {
                    mRemoveDiacriticsForIndexField = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSRemoveDiacriticsForIndexField"], true);
                }

                return mRemoveDiacriticsForIndexField.Value;
            }
        }


        /// <summary>
        /// Gets the value that indicates whether content field should be stored in the index
        /// </summary>
        public static bool StoreContentField
        {
            get
            {
                if (mStoreContentField == null)
                {
                    mStoreContentField = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSSearchStoreContentField"], false);
                }

                return mStoreContentField.Value;
            }
        }


        /// <summary>
        /// Gets the regular expression for word splitting used in SubsetAnalyzer.
        /// </summary>
        public static Regex SubsetAnalyzerWordRegex
        {
            get
            {
                if (mWordRegex == null)
                {
                    var wordRegex = CoreServices.AppSettings["CMSSubsetAnalyzerWordRegex"];
                    if (!String.IsNullOrEmpty(wordRegex))
                    {
                        try
                        {
                            mWordRegex = new CMSRegex(wordRegex);
                        }
                        catch
                        {
                            mWordRegex = new CMSRegex("(\\w|@|\\.)+");
                        }
                    }
                    else
                    {
                        mWordRegex = new CMSRegex("(\\w|@|\\.)+");
                    }
                }
                return mWordRegex;
            }
        }


        /// <summary>
        /// Gets or sets the fields separated by semicolon, this field will be excluded from int/double conversion
        /// </summary>
        public static string ExcludedFieldsFromConversion
        {
            get
            {
                // Default excluded columns
                return mExcludedFieldsFromConversion ?? (mExcludedFieldsFromConversion = ";documentid;nodeid;_position;_score;nodelinkednodeid;nodeaclid;");
            }
            set
            {
                mExcludedFieldsFromConversion = value;

                // Check whether fields string is defined
                if (mExcludedFieldsFromConversion != null)
                {
                    // Add helper semicolons and convert it to lower
                    mExcludedFieldsFromConversion = ";" + mExcludedFieldsFromConversion.ToLowerInvariant() + ";";
                }
            }
        }


        /// <summary>
        /// Gets the regular expression for basic search replacement.
        /// </summary>
        public static Regex BasicSearchReplacer
        {
            get
            {
                return mBasicEscaperRegEx ?? (mBasicEscaperRegEx = RegexHelper.GetRegex("[\\[\\:\\]\\{\\}]"));
            }
        }


        /// <summary>
        /// Gets the regular expression for image content tag.
        /// </summary>
        public static Regex ImageContentReplacer
        {
            get
            {
                return mImageContentReplacer ?? (mImageContentReplacer = RegexHelper.GetRegex("<image>.*</image>"));
            }
        }


        /// <summary>
        /// XPath function to specify which web parts fields should be added to the search document content.
        /// </summary>
        public static string SearchContentXpathValue
        {
            get
            {
                // Check if property is already initialized
                if (String.IsNullOrEmpty(mSearchContentXpathValue))
                {
                    // Try to load it from web.config
                    mSearchContentXpathValue = ValidationHelper.GetString(SettingsHelper.AppSettings["CMSSearchContentXpathValue"], "");

                    // Load it manually
                    if (String.IsNullOrEmpty(mSearchContentXpathValue))
                    {
                        mSearchContentXpathValue = "//property[@name='text' or @name='contentbefore' or @name='contentafter']";
                    }
                }

                return mSearchContentXpathValue;
            }
        }


        /// <summary>
        /// If true, performs the search only when the content part of the search expression is present.
        /// </summary>
        public static bool SearchOnlyWhenContentPresent
        {
            get
            {
                if (mSearchOnlyWhenContentPresent == null)
                {
                    mSearchOnlyWhenContentPresent = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSSearchOnlyWhenContentPresent"], true);
                }
                return mSearchOnlyWhenContentPresent.Value;
            }
            set
            {
                mSearchOnlyWhenContentPresent = value;
            }
        }


        /// <summary>
        /// Custom search path.
        /// </summary>
        public static string CustomSearchPath
        {
            get
            {
                return mCustomSearchPath;
            }
            set
            {
                mCustomSearchPath.Value = value;
            }
        }


        /// <summary>
        /// Search path.
        /// File system location where Smart Search stores the indexes.
        /// </summary>
        public static string SearchPath
        {
            get
            {
                return CustomSearchPath ?? "App_Data\\CMSModules\\SmartSearch\\";
            }
        }


        /// <summary>
        /// Indicates whether Smart Search indexes are shared through external storage.
        /// Tasks for shared index needs to be executed only once by one instance.
        /// </summary>
        internal static bool IndexesInSharedStorage
        {
            get
            {
                if (mIndexesInSharedStorage == null)
                {
                    mIndexesInSharedStorage = StorageHelper.IsSharedStorage(SearchPath);
                }
                return mIndexesInSharedStorage.Value;
            }
            set
            {
                mIndexesInSharedStorage = value;
            }
        }


        /// <summary>
        /// Maximal field length.
        /// </summary>
        public static int MaxFieldLength
        {
            get
            {
                if (mMaxFieldLength == null)
                {
                    mMaxFieldLength = ValidationHelper.GetInteger(SettingsHelper.AppSettings["CMSSearchMaxFieldLength"], Int32.MaxValue);
                }
                return mMaxFieldLength.Value;
            }
            set
            {
                mMaxFieldLength = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Creates empty search document.
        /// </summary>
        public static SearchDocument CreateEmptyDocument()
        {
            return new SearchDocument();
        }


        /// <summary>
        /// Creates search document based on given parameters
        /// </summary>
        /// <param name="parameters">Document initialization parameters</param>
        public static SearchDocument CreateDocument(SearchDocumentParameters parameters)
        {
            // Create empty document
            var doc = CreateEmptyDocument();

            doc.Initialize(parameters);

            return doc;
        }


        /// <summary>
        /// Adds a general field do the search document.
        /// </summary>
        /// <param name="document">Document to which should be field added</param>
        /// <param name="name">Name of new field</param>
        /// <param name="value">Value of field</param>
        /// <param name="store">Should be value stored</param>
        /// <param name="tokenize">Should be value tokenized</param>
        /// <param name="valueToLower">If true, the value of the field is converted to lower case before adding to the index</param>
        public static void AddGeneralField(ILuceneSearchDocument document, string name, object value, bool store, bool tokenize, bool valueToLower = true)
        {
            // Must have correct values
            if ((document == null) || String.IsNullOrEmpty(name))
            {
                return;
            }

            // Convert filed name to lower
            name = name.ToLowerInvariant();

            string stringValue = null;

            // String value
            if (value is string)
            {
                stringValue = Convert.ToString(value);
            }
            // Check whether field should be excluded from conversion
            else if (!ExcludedFieldsFromConversion.Contains(";" + name + ";"))
            {
                stringValue = SearchValueConverter.ConvertToString(value);
            }

            // If string value is not defined use default conversion
            if (stringValue == null)
            {
                stringValue = Convert.ToString(value);
            }

            // Convert value to lower and remove diacritics if required
            if (!String.IsNullOrEmpty(stringValue))
            {
                if (valueToLower)
                {
                    stringValue = stringValue.ToLowerInvariant();
                }
                if (RemoveDiacriticsForIndexField)
                {
                    stringValue = TextHelper.RemoveDiacritics(stringValue);
                }
            }

            // Add field to document
            document.Add(name, stringValue, store, tokenize);
        }


        /// <summary>
        /// Prepares value to add to content field.
        /// </summary>
        /// <param name="value">Value to add</param> 
        /// <param name="stripTags">Indicates whether tags should be stripped</param>
        public static string PrepareContentValue(object value, bool stripTags)
        {
            string contentValue = ValidationHelper.GetString(value, String.Empty);
            if (!String.IsNullOrEmpty(contentValue))
            {
                if (stripTags)
                {
                    contentValue = HTMLHelper.StripTags(contentValue, false, true, " ", "@", "");
                }

                // Remove macros
                contentValue = MacroProcessor.RemoveMacros(contentValue, " ");

                // Decode html characters
                contentValue = HttpUtility.HtmlDecode(contentValue);

                // Remove diacritics
                contentValue = TextHelper.RemoveDiacritics(contentValue);
            }

            return contentValue;
        }


        /// <summary>
        ///  Adds object data to iDocument, data for content field prepares and returns.
        /// </summary>
        /// <param name="index">Index for which the content is to be collected.</param>
        /// <param name="doc">Search document</param>
        /// <param name="classname">Class name</param>
        /// <param name="ds">Dataset with data</param>
        public static string AddObjectDataToDocument(ISearchIndexInfo index, SearchDocument doc, string classname, DataSet ds)
        {
            StringBuilder sb = new StringBuilder();

            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                DataClassInfo dci = DataClassInfoProvider.GetDataClassInfo(classname);
                if (dci != null)
                {
                    // Load XML schema 
                    SearchSettings ss = new SearchSettings();
                    ss.LoadData(dci.ClassSearchSettings);

                    foreach (SearchSettingsInfo ssi in ss)
                    {
                        if ((ssi != null) && (SearchFieldsHelper.Instance.IsContentField(index, ssi)))
                        {
                            // Prepare for content
                            foreach (DataRow row in ds.Tables[0].Rows)
                            {
                                if (ds.Tables[0].Columns.Contains(ssi.Name))
                                {
                                    // Append content
                                    var content = PrepareContentValue(row[ssi.Name], true);
                                    if (!String.IsNullOrEmpty(content))
                                    {
                                        sb.Append(" ");
                                        sb.Append(content);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return sb.ToString();
        }


        /// <summary>
        /// Creates rebuild task if search settings are not empty, for user index is task created always.
        /// </summary>
        /// <param name="indexID">Index ID</param>
        /// <returns>True if search task was created, false if not</returns>
        public static bool CreateRebuildTask(int indexID)
        {
            // Get search index info
            SearchIndexInfo sii = SearchIndexInfoProvider.GetSearchIndexInfo(indexID);
            if (sii != null)
            {
                if (!DataHelper.DataSourceIsEmpty(sii.IndexSettings.GetAll()) || (sii.IndexType.ToLowerInvariant() == PredefinedObjectType.USER))
                {
                    // Rebuild search index info
                    SearchTaskInfoProvider.CreateTask(SearchTaskTypeEnum.Rebuild, null, null, sii.IndexName, sii.IndexID);
                    return true;
                }
                else
                {
                    // Index isn't ready to rebuild
                    return false;
                }
            }
            return false;
        }


        /// <summary>
        /// Returns true if given class allows search.
        /// </summary>
        /// <param name="className">Code name of the class</param>
        public static bool IsClassSearchEnabled(string className)
        {
            var classInfo = DataClassInfoProvider.GetDataClassInfo(className);
            return (classInfo != null) && classInfo.ClassSearchEnabled;
        }

        #endregion


        #region "Index update/remove/rebuild/optimize"

        /// <summary>
        /// Optimize specified index.
        /// </summary>
        /// <remarks>This method needs to be run in a thread safe way such as smart search task queue.</remarks>
        /// <param name="indexInfo">Search index</param>
        public static void Optimize(SearchIndexInfo indexInfo)
        {
            // Check whether index info object is defined
            if (indexInfo == null)
            {
                return;
            }

            // If no indexed items, no optimize
            if (!indexInfo.IndexDocumentCount.HasValue || indexInfo.IndexDocumentCount == 0)
            {
                return;
            }

            // Set status to optimizing
            SearchIndexInfoProvider.SetIndexStatus(indexInfo, IndexStatusEnum.OPTIMIZING);

            IIndexWriter indexWriter;

            try
            {
                // Get writer
                indexWriter = indexInfo.Provider.GetWriter(false);
            }
            catch (Exception)
            {
                // Set status to error
                SearchIndexInfoProvider.SetIndexStatus(indexInfo, IndexStatusEnum.ERROR);
                throw;
            }


            // Check whether writer exists
            if (indexWriter != null)
            {
                try
                {
                    // Optimize index
                    indexWriter.Optimize();
                }
                catch (Exception)
                {
                    // Set status to error
                    SearchIndexInfoProvider.SetIndexStatus(indexInfo, IndexStatusEnum.ERROR);
                    throw;
                }
                finally
                {
                    // Close index
                    indexWriter.Close();
                }
            }

            // Invalidate searcher
            InvalidateSearcher(indexInfo.IndexGUID);

            // Set status to ready
            if (SearchIndexInfoProvider.GetIndexStatus(indexInfo) != IndexStatusEnum.ERROR)
            {
                SearchIndexInfoProvider.SetIndexStatus(indexInfo, IndexStatusEnum.READY);
                SearchIndexInfoProvider.SetIndexFilesLastUpdateTime(indexInfo, DateTime.Now);
            }
        }


        /// <summary>
        /// Rebuild part of the index with dependence on type and values.
        /// </summary>
        /// <param name="taskInfo">Search task</param>
        public static void PartialRebuild(SearchTaskInfo taskInfo)
        {
            SearchIndexers.GetIndexer(taskInfo.SearchTaskObjectType).PartialRebuild(taskInfo);
        }


        /// <summary>
        /// Rebuild specified index.
        /// </summary>
        /// <param name="indexInfo">Search index</param>
        public static void Rebuild(SearchIndexInfo indexInfo)
        {
            // Check whether index info object is defined
            if (indexInfo == null)
            {
                return;
            }

            // Set index status to rebuilding
            SearchIndexInfoProvider.SetIndexStatus(indexInfo, IndexStatusEnum.REBUILDING);

            try
            {
                // Try unlock index
                // Index shouldn't be locked but app. crash or search crash
                // could cause constantly locked file
                indexInfo.Provider.Unlock();

                SearchIndexers.GetIndexer(indexInfo.IndexType).Rebuild(indexInfo);
            }
            catch (Exception)
            {
                SearchIndexInfoProvider.SetIndexStatus(indexInfo, IndexStatusEnum.ERROR);
                throw;
            }

            if (SearchIndexInfoProvider.GetIndexStatus(indexInfo) != IndexStatusEnum.ERROR)
            {
                // Set index status to ready
                SearchIndexInfoProvider.SetIndexStatus(indexInfo, IndexStatusEnum.READY);
                SearchIndexInfoProvider.SetIndexFilesLastUpdateTime(indexInfo, DateTime.Now);
            }

            // Invalidate infos about index size
            indexInfo.InvalidateIndexStatistics();
        }


        /// <summary>
        /// Finishes the rebuild of the given index by marking it as updated
        /// </summary>
        /// <param name="srchInfo">Search index info</param>
        public static void FinishRebuild(SearchIndexInfo srchInfo)
        {
            // Invalidate searcher                            
            InvalidateSearcher(srchInfo.IndexGUID);

            // Set actual rebuild time
            srchInfo.ActualRebuildTime = DateTime.Now;

            // Set last rebuild time
            srchInfo.IndexLastRebuildTime = DateTime.Now;

            if (SearchIndexInfoProvider.GetIndexStatus(srchInfo) != IndexStatusEnum.ERROR)
            {
                // Clear 'Outdated' flag if rebuild finished without error
                srchInfo.IndexIsOutdated = false;
            }

            SearchIndexInfoProvider.SetSearchIndexInfo(srchInfo);
        }


        /// <summary>
        /// Insert or update document for specified index.
        /// </summary>
        /// <remarks>This method needs to be run in a thread safe way such as smart search task queue.</remarks>
        /// <param name="iDoc">Document</param>
        /// <param name="indexInfo">Search index</param>
        public static void Update(ILuceneSearchDocument iDoc, SearchIndexInfo indexInfo)
        {
            // Check whether document and search index info object exist
            if ((iDoc == null) || (indexInfo == null))
            {
                return;
            }

            string id = iDoc.Get(SearchFieldsConstants.ID);

            if (!String.IsNullOrEmpty(id))
            {
                Delete(SearchFieldsConstants.ID, id, indexInfo);
            }

            // Get current index writer 
            var iw = indexInfo.Provider.GetWriter(false);
            if (iw != null)
            {
                try
                {
                    // Add document to the index
                    iw.AddDocument(iDoc);
                }
                finally
                {
                    // Close writer
                    iw.Close();
                }
            }

            SearchIndexInfoProvider.SetIndexFilesLastUpdateTime(indexInfo, DateTime.Now);

            // Invalidate searcher            
            InvalidateSearcher(indexInfo.IndexGUID);

            // Invalidate infos about index size
            indexInfo.InvalidateIndexStatistics();
        }


        /// <summary>
        /// Remove document(s) with dependence on field name and field value collection for specified index.
        /// </summary>
        /// <param name="fieldName">Field name</param>
        /// <param name="values">Collection of field values</param>
        /// <param name="indexInfo">Search index</param>
        public static void Delete(string fieldName, ICollection values, SearchIndexInfo indexInfo)
        {
            // Check whether index info object is defined
            if (indexInfo == null)
            {
                return;
            }

            // Invalidate infos about index size
            indexInfo.InvalidateIndexStatistics();

            // Get searcher object
            var isrch = indexInfo.Provider.GetSearcher();
            if (isrch != null)
            {
                try
                {
                    foreach (string value in values)
                    {
                        isrch.Delete(fieldName, value);
                    }

                    isrch.Commit();
                }
                finally
                {
                    SearchIndexInfoProvider.SetIndexFilesLastUpdateTime(indexInfo, DateTime.Now);

                    // Invalidate searcher                
                    InvalidateSearcher(indexInfo.IndexGUID);

                    isrch.Close();
                }
            }
        }


        /// <summary>
        /// Remove document(s) with dependence on field name and field value for specified index.
        /// </summary>
        /// <param name="fieldName">Field name</param>
        /// <param name="value">Field value</param>
        /// <param name="indexInfo">Search index</param>
        public static void Delete(string fieldName, string value, SearchIndexInfo indexInfo)
        {
            // Check whether index info object is defined
            if (indexInfo == null)
            {
                return;
            }

            // Invalidate infos about index size
            indexInfo.InvalidateIndexStatistics();

            // Get searcher object
            var isrch = indexInfo.Provider.GetSearcher();
            if (isrch != null)
            {
                try
                {
                    isrch.Delete(fieldName, value);
                    isrch.Commit();
                }
                finally
                {
                    SearchIndexInfoProvider.SetIndexFilesLastUpdateTime(indexInfo, DateTime.Now);

                    // Invalidate searcher
                    InvalidateSearcher(indexInfo.IndexGUID);

                    isrch.Close();
                }
            }
        }

        #endregion


        #region "Highlight methods"

        /// <summary>
        /// Highlighting match evaluator.
        /// </summary>
        /// <param name="m">Represents the results from a single regular expression match</param>
        private static string HighlightMatch(Match m)
        {
            // Get highlighting object array
            object[] highl = SearchContext.CurrentHighlight;

            // Check whether highlighting object array is defined
            if (highl != null)
            {
                // Get text to highlight
                string high = highl[0] as string;
                int currentLength = Convert.ToInt32(highl[1]);

                // Check whether text to highlight is specified
                if (!String.IsNullOrEmpty(high))
                {
                    // Add starting marker
                    high = high.Insert(m.Groups["selected"].Index + currentLength, STARTING_MARKER);

                    // Add ending marker
                    high = high.Insert(m.Groups["selected"].Index + m.Groups["selected"].Length + STARTING_MARKER.Length + currentLength, ENDING_MARKER);

                    // Set current offset
                    currentLength += STARTING_MARKER.Length + ENDING_MARKER.Length;

                    // Save current values in the 
                    highl[0] = high;
                    highl[1] = currentLength;

                    // Save current highlighting object
                    SearchContext.CurrentHighlight = highl;
                }
            }

            // Return nothing
            return null;
        }


        /// <summary>
        /// Highlight input text with dependence on current search key words.
        /// </summary>
        /// <param name="text">Input text</param>
        /// <param name="startTag">Starting HTML tag</param>
        /// <param name="endTag">Ending HTML tag</param>
        public static string Highlight(string text, string startTag, string endTag)
        {
            // Get regexs collection
            Regex regex = SearchContext.HighlightRegex;

            // Check whether collection is defined
            if ((regex != null) && (!String.IsNullOrEmpty(text)))
            {
                // Remove whitespaces
                text = TextHelper.ReduceWhiteSpaces(text, " ");

                // Decode input text
                text = HttpUtility.HtmlDecode(text);

                // Create object array with text and highlight offset
                object[] highl = new object[2];
                highl[0] = text;
                highl[1] = 0;

                // Save object array
                SearchContext.CurrentHighlight = highl;

                var textWithoutDiacritics = TextHelper.RemoveDiacritics(text);

                // Use input text without diacritics if it's length isn't different
                // (if the length is not same, the highlight would not match the original text)
                if (textWithoutDiacritics.Length == text.Length)
                {
                    text = textWithoutDiacritics;
                }

                // Keywords highlighting
                regex.Replace(text, HighlightMatch);

                // Get processed text
                highl = SearchContext.CurrentHighlight;

                // Encode text
                text = HTMLHelper.HTMLEncode(highl[0] as string);

                // Check whether text is defined
                if (text != null)
                {
                    // Replace highlighting macros
                    text = text.Replace(STARTING_MARKER, startTag);
                    text = text.Replace(ENDING_MARKER, endTag);
                }
            }

            // Return highlighted text
            return text;
        }

        #endregion


        #region "Custom search index"

        /// <summary>
        /// Returns ICusromSearchIndex for specified index info.
        /// </summary>
        /// <param name="indexInfo">Search index</param>
        public static ICustomSearchIndex GetCustomSearchIndex(SearchIndexInfo indexInfo)
        {
            // Get the settings info
            SearchIndexSettingsInfo sisi = indexInfo.IndexSettings.GetSearchIndexSettingsInfo(CUSTOM_INDEX_DATA);
            if (sisi != null)
            {
                try
                {
                    // Get assembly name
                    string assemblyName = ValidationHelper.GetString(sisi.GetValue("AssemblyName"), String.Empty);
                    string className = ValidationHelper.GetString(sisi.GetValue("ClassName"), String.Empty);
                    if (!String.IsNullOrEmpty(assemblyName) && !String.IsNullOrEmpty(className))
                    {
                        // Create instance of ICustomSearchIndex
                        ICustomSearchIndex customSearch = ClassHelper.GetClass(assemblyName, className) as ICustomSearchIndex;
                        if (customSearch == null)
                        {
                            throw new Exception("[SearchHelper.GetCustomSearchIndex] Classname '" + className + "' could not be found.");
                        }
                        return customSearch;
                    }
                    else
                    {
                        throw new Exception("[SearchHelper.GetCustomSearchIndex] Assembly name or class name is not defined.");
                    }
                }
                catch (Exception)
                {
                    SearchIndexInfoProvider.SetIndexStatus(indexInfo, IndexStatusEnum.ERROR);
                    throw;
                }
            }
            return null;
        }

        #endregion


        #region "Search methods"

        /// <summary>
        /// Returns dataset with search results, if search is used for non-document index, path and class name values are ignored (can be null).
        /// </summary>
        /// <param name="parameters">Search parameters</param>
        /// <returns>DataSet with search results</returns>
        /// <remarks>
        /// This method does not constrain the search by <see cref="SearchParameters.ClassNames"/>.
        /// To constrain the search by class names, set <see cref="SearchCondition.DocumentCondition"/> of a <see cref="SearchCondition"/>.
        /// Use <see cref="SearchSyntaxHelper.CombineSearchCondition"/> to obtain the resulting <see cref="SearchParameters.SearchFor"/> expression.
        /// </remarks>
        public static DataSet Search(SearchParameters parameters)
        {
            // Get the results
            var filteredResults = SearchInternal(parameters);

            // Check if some results were found
            if ((filteredResults == null) || (filteredResults.Count == 0))
            {
                parameters.NumberOfResults = 0;
                return null;
            }

            // Get the result range (from and to index)
            int fromIndex;
            int toIndex;

            GetResultRange(parameters, filteredResults, out fromIndex, out toIndex);

            // Load result data
            var resultData = LoadResultData(filteredResults, fromIndex, toIndex);

            // List of smart search documents
            var docs = new SafeDictionary<string, ILuceneSearchDocument>();

            // Create DataSet with results
            var resultDs = CreateResultDataSet(filteredResults, fromIndex, toIndex, resultData, docs, parameters.MaxScore);

            // Store current results to the request storage
            SearchContext.CurrentSearchDocuments = docs;
            SearchContext.CurrentSearchResults = resultData;

            return resultDs;
        }


        /// <summary>
        /// Returns list with search results, if search is used for non-document index, path and class name values are ignored (can be null).
        /// </summary>
        /// <param name="parameters">Search parameters</param>
        private static List<ILuceneSearchDocument> SearchInternal(SearchParameters parameters)
        {
            // Prepare results
            var results = PrepareSearchResults(parameters);
            if (results == null)
            {
                return null;
            }

            // Fire search handler
            using (var h = SearchEvents.Search.StartEvent(parameters, results))
            {
                if (h.CanContinue())
                {
                    // Add default results
                    SearchManager.AddResults(parameters, results);
                }

                h.FinishEvent();
            }

            return results.Results;
        }


        /// <summary>
        /// Adds the attachment results to the search results
        /// </summary>
        /// <param name="parameters">Search parameters</param>
        private static SearchResults PrepareSearchResults(SearchParameters parameters)
        {
            // Set default number of results
            parameters.NumberOfResults = 0;
            SearchContext.LastError = null;

            // Check whether indexes are defined
            if (String.IsNullOrEmpty(parameters.SearchIndexes))
            {
                return null;
            }

            string[] indexNames = parameters.SearchIndexes.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            List<SearchIndexInfo> indexes = new List<SearchIndexInfo>();

            List<int> indexIDs = new List<int>(indexNames.Length);

            // Lists of (not/in) roles
            List<string> userInRoles = null;
            List<string> userNotInRoles = null;

            // Loop through all indexes and create searchables
            foreach (string indexName in indexNames)
            {
                // Get current index
                SearchIndexInfo sii = SearchIndexInfoProvider.GetLocalizedSearchIndexInfo(indexName, parameters.CurrentCulture);
                if (sii != null)
                {
                    indexes.Add(sii);

                    indexIDs.Add(sii.IndexID);

                    // Keep role list for user index type
                    if (sii.IndexType == PredefinedObjectType.USER)
                    {
                        // Check whether index settings are defined
                        if ((sii.IndexSettings.Items != null) && (sii.IndexSettings.Items.Count > 0))
                        {
                            // Get settings info
                            SearchIndexSettingsInfo sisi = sii.IndexSettings.Items[SIMPLE_ITEM_ID];
                            if (sisi != null)
                            {
                                // Initialize collections
                                if (userInRoles == null)
                                {
                                    userInRoles = new List<string>();
                                    userNotInRoles = new List<string>();
                                }

                                // Try get in roles
                                string inRoles = ValidationHelper.GetString(sisi.GetValue("UserInRoles"), String.Empty);

                                // Check whether in roles are defined
                                if (!String.IsNullOrEmpty(inRoles))
                                {
                                    string[] tmpInRoles = inRoles.ToLowerInvariant().Split(';');
                                    foreach (string role in tmpInRoles)
                                    {
                                        if (!String.IsNullOrEmpty(role) && !userInRoles.Contains(role))
                                        {
                                            userInRoles.Add(role);
                                        }
                                    }
                                }

                                // Try get not in roles
                                string notInRoles = ValidationHelper.GetString(sisi.GetValue("UserNotInRoles"), String.Empty);

                                // Check whether not in roles are defined
                                if (!String.IsNullOrEmpty(notInRoles))
                                {
                                    string[] tmpNotInRoles = notInRoles.ToLowerInvariant().Split(';');
                                    foreach (string role in tmpNotInRoles)
                                    {
                                        if (!String.IsNullOrEmpty(role) && !userNotInRoles.Contains(role))
                                        {
                                            userNotInRoles.Add(role);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // Check whether exists at least one search index
            if (indexes.Count == 0)
            {
                return null;
            }

            List<int> siteIds = CollectSites(parameters, indexIDs);

            // Prepare parameters for document filter
            var results = new SearchResults(parameters.User, userInRoles, userNotInRoles, siteIds, indexes)
            {
                DocumentParameters = new DocumentFilterSearchResultsParameters(parameters.User, parameters.CheckPermissions, parameters.CurrentCulture, parameters.CombineWithDefaultCulture)
                {
                    DefaultCulture = parameters.DefaultCulture
                }
            };

            return results;
        }


        /// <summary>
        /// Collects the list of sites to search
        /// </summary>
        /// <param name="parameters">Search parameters</param>
        /// <param name="indexIDs">List of index IDs</param>
        private static List<int> CollectSites(SearchParameters parameters, List<int> indexIDs)
        {
            List<int> siteIds = new List<int>(indexIDs.Count);

            // Get sites for all indexes
            var sites = SearchIndexSiteInfoProvider.GetIndexSites(indexIDs.ToArray())
                .Columns("SiteID", "SiteName", "SiteStatus");

            string sitesCondition = String.Empty;

            // Loop through all sites
            foreach (SiteInfo site in sites)
            {
                // Exclude stopped sites
                if (site.Status == SiteStatusEnum.Stopped)
                {
                    sitesCondition = SearchSyntaxHelper.AddSearchCondition(sitesCondition, SearchSyntaxHelper.GetFieldCondition(SearchFieldsConstants.SITE, site.SiteName, site.SiteName, false));
                }
                // If search in attachments is enabled and current site is running add site to the sites collection
                else
                {
                    siteIds.Add(site.SiteID);
                }
            }

            // Add sites condition
            parameters.SearchFor = SearchSyntaxHelper.AddSearchCondition(parameters.SearchFor, sitesCondition);

            return siteIds;
        }


        /// <summary>
        /// Gets the result range indexes
        /// </summary>
        /// <param name="parameters">Search parameters</param>
        /// <param name="filteredResults">Filtered results</param>
        /// <param name="fromIndex">Returns starting index</param>
        /// <param name="toIndex">Returns ending index</param>
        private static void GetResultRange(SearchParameters parameters, List<ILuceneSearchDocument> filteredResults, out int fromIndex, out int toIndex)
        {
            // Ensures non-negative starting position
            if (parameters.StartingPosition < 0)
            {
                parameters.StartingPosition = 0;
            }

            // Re-compute starting position if it is necessary
            if ((filteredResults.Count < parameters.StartingPosition) && (parameters.DisplayResults > 0))
            {
                parameters.StartingPosition = (filteredResults.Count / parameters.DisplayResults) * parameters.DisplayResults;
            }

            // Define the range of processing
            fromIndex = parameters.StartingPosition;
            toIndex = fromIndex + parameters.DisplayResults - 1;
            if (toIndex >= filteredResults.Count)
            {
                toIndex = filteredResults.Count - 1;
            }
        }


        /// <summary>
        /// Loads the result data for the search results
        /// </summary>
        /// <param name="filteredResults">Filtered search results</param>
        /// <param name="fromIndex">Starting index</param>
        /// <param name="toIndex">Ending index</param>
        private static SafeDictionary<string, DataRow> LoadResultData(List<ILuceneSearchDocument> filteredResults, int fromIndex, int toIndex)
        {
            // Contains result data - key is <ID>_<ObjectType>
            var resultData = new SafeDictionary<string, DataRow>();

            // Get result types
            var types = GetResultTypes(filteredResults, fromIndex, toIndex);

            // Load the specifically indexed types
            foreach (var type in types)
            {
                var typeCopy = type;
                var results = GetResultsToProcess(filteredResults, fromIndex, toIndex, t => string.Equals(t, typeCopy, StringComparison.InvariantCultureIgnoreCase));

                SearchIndexers.GetIndexer(type).LoadResults(results, resultData);
            }

            return resultData;
        }


        /// <summary>
        /// Creates the result DataSet from the search results
        /// </summary>
        /// <param name="filteredResults">List of filtered results</param>
        /// <param name="fromIndex">Starting index for the results</param>
        /// <param name="toIndex">End index for the results</param>
        /// <param name="resultData">Result data</param>
        /// <param name="docs">Search documents for particular items</param>
        /// <param name="maxScore">Maximum score value encountered within the search hits</param>
        private static DataSet CreateResultDataSet(List<ILuceneSearchDocument> filteredResults, int fromIndex, int toIndex, SafeDictionary<string, DataRow> resultData, SafeDictionary<string, ILuceneSearchDocument> docs, float maxScore)
        {
            var dt = NewResultsTable();

            // Loop through results which should be displayed - create fake dataset
            foreach (var obj in GetResultsToProcess(filteredResults, fromIndex, toIndex, t => true))
            {
                // Get type for current result
                string type = ValidationHelper.GetString(obj.Get(SearchFieldsConstants.TYPE), String.Empty);

                var row = dt.NewRow();
                var add = false;

                var id = ValidationHelper.GetString(obj.Get(SearchFieldsConstants.ID), String.Empty) + "_" + type;
                row["id"] = id;
                row["type"] = type;

                // Score value (since luc v3 the absolute score is returned, not the relative one
                // The normalized relative has to be computed
                row["score"] = Math.Min(1, ValidationHelper.GetDouble(obj.Get(SearchFieldsConstants.SCORE), 0) / maxScore);

                row["position"] = ValidationHelper.GetString(obj.Get(SearchFieldsConstants.POSITION), String.Empty);
                row["absscore"] = ValidationHelper.GetString(obj.Get(SearchFieldsConstants.SCORE), String.Empty);
                row["maxscore"] = maxScore;
                row["index"] = ValidationHelper.GetString(obj.Get(SearchFieldsConstants.INDEX), String.Empty);

                switch (type.ToUpperInvariant())
                {
                    case CUSTOM_SEARCH_INDEX:
                        {
                            // Get the custom search index result
                            var dr = GetCustomSearchIndexResult(row, obj);
                            if (dr != null)
                            {
                                resultData[id] = dr;
                                docs[id] = obj;

                                add = true;
                            }
                        }
                        break;

                    default:
                        {
                            // Fill search result
                            var dr = resultData[id];
                            if (SearchIndexers.GetIndexer(type).FillSearchResult(row, dr, obj))
                            {
                                docs[id] = obj;

                                add = true;
                            }
                        }
                        break;
                }

                if (add)
                {
                    // Add current row to the fake dataset
                    dt.Rows.Add(row);
                }
            }

            // Create dataset
            DataSet ds = new DataSet();
            ds.Tables.Add(dt);

            return ds;
        }


        /// <summary>
        /// Fills custom search index search result, returns data row that replaces the reference to search result in result data
        /// </summary>
        /// <param name="resultDr">Result data row</param>
        /// <param name="doc">Results search document</param>
        private static DataRow GetCustomSearchIndexResult(DataRow resultDr, ILuceneSearchDocument doc)
        {
            // Custom search index
            resultDr["title"] = ValidationHelper.GetString(doc.Get(SearchFieldsConstants.CUSTOM_TITLE), String.Empty);
            resultDr["content"] = ValidationHelper.GetString(doc.Get(SearchFieldsConstants.CUSTOM_CONTENT), String.Empty);

            // Date
            string customDate = ValidationHelper.GetString(doc.Get(SearchFieldsConstants.CUSTOM_DATE), String.Empty);
            if (!String.IsNullOrEmpty(customDate))
            {
                resultDr["created"] = SearchValueConverter.StringToDate(customDate);
            }

            resultDr["image"] = ValidationHelper.GetString(doc.Get(SearchFieldsConstants.CUSTOM_IMAGEURL), String.Empty);

            // Create data row for custom url
            var urlDt = new DataTable();

            urlDt.Columns.Add(SearchFieldsConstants.CUSTOM_URL);
            urlDt.Rows.Add(new object[] { ValidationHelper.GetString(doc.Get(SearchFieldsConstants.CUSTOM_URL), String.Empty) });

            return urlDt.Rows[0];
        }


        /// <summary>
        /// Creates new search results data table
        /// </summary>
        private static DataTable NewResultsTable()
        {
            var dt = new DataTable();

            dt.TableName = "results";

            dt.Columns.Add("id");
            dt.Columns.Add("type");
            dt.Columns.Add("score");
            dt.Columns.Add("position");

            dt.Columns.Add("title");
            dt.Columns.Add("content");
            dt.Columns.Add("created");
            dt.Columns.Add("image");
            dt.Columns.Add("documentExtensions");

            dt.Columns.Add("absscore");
            dt.Columns.Add("maxscore");

            dt.Columns.Add("index");

            return dt;
        }


        /// <summary>
        /// Filters the search results, returns number of resulting items
        /// </summary>
        /// <param name="hits">Hits collection with search results</param>
        /// <param name="parameters">Search result parameters</param>
        /// <param name="searchParams">Search parameters</param>
        public static int FilterResults(ISearchHits hits, SearchResults parameters, SearchParameters searchParams)
        {
            if (hits == null)
            {
                return 0;
            }

            // Number of denied documents
            int denied = 0;
            int i;

            var filteredResults = parameters.Results;

            float maxScore = 0;

            var existingResults = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

            for (i = 0; ((i - denied) < searchParams.NumberOfProcessedResults) && (i < hits.Length()); i++)
            {
                // Get current document
                var currentDoc = hits.Doc(i);

                string type = currentDoc.Get(SearchFieldsConstants.TYPE);
                string id = currentDoc.Get(SearchFieldsConstants.ID);

                // Skip null objects
                if (id == null)
                {
                    continue;
                }

                // Check if the result was already added
                string key = String.Format("{0}|{1}", type, id);

                bool allow = !existingResults.Contains(key);

                // Check permissions
                if (allow)
                {
                    allow = SearchIndexers.GetIndexer(type).CheckResultPermissions(parameters, currentDoc, i - denied);
                }

                // Add to results
                if (allow)
                {
                    // Add temp fields with position and score
                    currentDoc.AddGeneralField(SearchFieldsConstants.POSITION, i, true, false);
                    currentDoc.AddGeneralField(SearchFieldsConstants.SCORE, hits.Score(i), true, false);

                    filteredResults.Add(currentDoc);

                    maxScore = Math.Max(maxScore, hits.Score(i));

                    // Mark existing result
                    existingResults.Add(key);
                }
                else
                {
                    denied++;
                }
            }

            // Remove all null results. Results may be set to null by indexer while checking result permissions.
            // When checking permissions and adding new result, indexer may decide to remove previous result from collection.
            // Result can't be removed directly in order to keep index values accurate. Thats why null value is used.
            for (int j = 0; j < filteredResults.Count; j++)
            {
                if (filteredResults[j] == null)
                {
                    filteredResults.RemoveAt(j);
                    j--;
                }
            }

            if (maxScore > 0)
            {
                searchParams.MaxScore = maxScore;
            }

            // Check whether exists at least one result and get last position of result in result's collection
            if (filteredResults.Count > 0)
            {
                var lastDoc = filteredResults[filteredResults.Count - 1];

                // Add real last position for the last document if exists at least one denied document
                if (denied > 0)
                {
                    lastDoc.RemoveField(SearchFieldsConstants.POSITION);
                    lastDoc.AddGeneralField(SearchFieldsConstants.POSITION, i - 1, true, false);
                }

                return ValidationHelper.GetInteger(lastDoc.Get(SearchFieldsConstants.POSITION), 0) + 1;
            }

            return 0;
        }


        /// <summary>
        /// Gets the list of all types present in the given results
        /// </summary>
        /// <param name="results">List of the results</param>
        /// <param name="from">Index from which process the results</param>
        /// <param name="to">Index to process the results</param>
        public static IList<string> GetResultTypes(IList<ILuceneSearchDocument> results, int from, int to)
        {
            var types = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

            for (int i = from; i <= to; i++)
            {
                var doc = results[i];

                var type = doc.Get(SearchFieldsConstants.TYPE);
                if (!types.Contains(type))
                {
                    types.Add(type);
                }
            }

            return types.ToList();
        }


        /// <summary>
        /// Processes the results from the given range matching the required type
        /// </summary>
        /// <param name="results">List of the results</param>
        /// <param name="from">Index from which process the results</param>
        /// <param name="to">Index to process the results</param>
        /// <param name="typeLambda">Function that checks the type of the results to process</param>
        public static IEnumerable<ILuceneSearchDocument> GetResultsToProcess(IList<ILuceneSearchDocument> results, int from, int to, Func<string, bool> typeLambda)
        {
            for (int i = from; i <= to; i++)
            {
                var doc = results[i];

                if (typeLambda(doc.Get(SearchFieldsConstants.TYPE)))
                {
                    yield return doc;
                }
            }
        }


        /// <summary>
        /// Invalidates searcher for the given index.
        /// </summary>
        /// <param name="id">Searcher id</param>
        public static void InvalidateSearcher(Guid id)
        {
            lock (Searchers)
            {
                Searchers[id.ToString()] = null;
                Searchers[id + "_readonly"] = null;
            }

            if (IndexesInSharedStorage && CMSActionContext.CurrentLogWebFarmTasks)
            {
                WebFarmHelper.CreateTask(SearchTaskType.InvalidateSearcher, "InvalidateSearcher", id.ToString());
            }
        }


        /// <summary>
        /// Returns FileInfo object of stop words file for the given path
        /// </summary>
        /// <param name="path">Path to the stop words file</param>
        public static FileInfo GetStopWordsFileInfo(string path)
        {
            if (!String.IsNullOrEmpty(path))
            {
                string stopWordFile = SearchIndexInfo.IndexPathPrefix + "_StopWords\\" + path + ".txt";
                if (File.Exists(stopWordFile))
                {
                    return FileInfo.New(stopWordFile);
                }

                // Log warning that stop word file wasn't found
                LogContext.LogEventToCurrent(EventType.WARNING, "Smart search", "GETSTOPWORDFILE", "Stop words file '" + stopWordFile + "' was not found.", null, 0, null, 0, null, null, 0, null, null, null, DateTime.Now);
            }

            return null;
        }


        /// <summary>
        /// Returns analyzer object for specified analyzer name.
        /// </summary>
        /// <param name="assemblyName">Assembly name</param>
        /// <param name="className">Class name</param>
        public static TAnalyzerType LoadCustomAnalyzer<TAnalyzerType>(string assemblyName, string className)
            where TAnalyzerType : class
        {
            // Check whether assembly and class name are defined
            if (!String.IsNullOrEmpty(assemblyName) && !String.IsNullOrEmpty(className))
            {
                // Try to get analyzer
                TAnalyzerType analyzer = ClassHelper.GetClass(assemblyName, className) as TAnalyzerType;
                if (analyzer != null)
                {
                    return analyzer;
                }
            }

            throw new Exception("[SearchHelper.LoadCustomAnalyzer] Custom analyzer definition was not found or is not well formed for analyzer assembly '" + assemblyName + "' or class name '" + className + "'.");
        }

        #endregion


        #region "Class search fields methods"

        /// <summary>
        /// Returns true if any field included in the search changed (checks fields defined in Class Search Settings).
        /// </summary>
        /// <param name="info">Info object</param>
        /// <param name="changedColumns">List of changed columns, if not provided, retrieves the list from the object</param>
        /// <param name="checkSpecialFields">If true, the special fields like (ClassSearchTitleColumn, etc.) are checked for changes</param>
        public static bool SearchFieldChanged(BaseInfo info, List<string> changedColumns = null, bool checkSpecialFields = true)
        {
            // Ensure changed columns
            if (changedColumns == null)
            {
                changedColumns = info.ChangedColumns();
            }

            // Search settings of SKU class are, for some reason (see JT-86), stored in cms.document class
            string className = string.Equals(info.TypeInfo.ObjectClassName, PredefinedObjectType.SKU, StringComparison.OrdinalIgnoreCase)
                ? PredefinedObjectType.DOCUMENT
                : info.TypeInfo.ObjectClassName;

            // Check whether the columns which are part of search changed
            DataClassInfo dataClassInfo = DataClassInfoProvider.GetDataClassInfo(className);

            return dataClassInfo.SearchFieldChanged(changedColumns, checkSpecialFields);
        }


        /// <summary>
        /// Returns true if the search task for given node should be created.
        /// Returns true if 
        ///  - search is allowed on general level AND 
        ///  - search is allowed for class of the object
        /// </summary>
        /// <param name="className">Class to check</param>
        public static bool IsSearchTaskCreationAllowed(string className)
        {
            // Check static settings
            return SearchIndexInfoProvider.SearchEnabled && SearchEnabledForClass(className);
        }


        /// <summary>
        /// Returns true if search is enabled for a given class (document type). Returns false if class not found.
        /// </summary>
        /// <param name="className">Name of the class</param>
        public static bool SearchEnabledForClass(string className)
        {
            if (!String.IsNullOrEmpty(className))
            {
                DataClassInfo dc = DataClassInfoProvider.GetDataClassInfo(className);
                if (dc != null)
                {
                    return dc.ClassSearchEnabled;
                }
            }
            return false;
        }


        /// <summary>
        /// Gets flag value which should be preselected for a column based on its data type. Handles local index flags only.
        /// </summary>
        /// <param name="flagName">Name of flag for which to get the default.</param>
        /// <param name="formFieldDataTypeEnum">Type of field</param>
        /// <returns>Returns default flag value.</returns>
        /// <remarks>
        /// When determining default search field flags values, <see cref="CreateDefaultSearchSettings"/> should be used
        /// as it collects information for flags of all search index providers.
        /// </remarks>
        /// <seealso cref="CreateDefaultSearchSettings"/>
        public static bool GetSearchFieldDefaultValue(string flagName, Type formFieldDataTypeEnum)
        {
            switch (flagName)
            {
                case SearchSettings.CONTENT:
                    return (formFieldDataTypeEnum == typeof(String));

                case SearchSettings.SEARCHABLE:

                    return ((formFieldDataTypeEnum != typeof(Guid)) && (formFieldDataTypeEnum != typeof(String)));

                case SearchSettings.TOKENIZED:
                    return (formFieldDataTypeEnum == typeof(String));

                default:
                    return false;
            }
        }


        /// <summary>
        /// Creates new <see cref="SearchSettingsInfo"/> for a field from given parameters.
        /// </summary>
        /// <param name="name">Name of the search field.</param>
        /// <param name="flags">A dictionary of flag names and their values.</param>
        /// <param name="fieldname">Custom field name.</param>
        /// <returns><see cref="SearchSettingsInfo"/> initialized from given parameters.</returns>
        public static SearchSettingsInfo CreateSearchSettings(string name, IDictionary<string, bool> flags, string fieldname)
        {
            bool fieldChanged;
            return CreateSearchSettings(name, flags, fieldname, null, out fieldChanged);
        }

        /// <summary>
        /// Creates new <see cref="SearchSettingsInfo"/> for a field from given parameters. The out parameter <paramref name="fieldChanged"/> indicates, whether the field's definition has changed
        /// when compared to <paramref name="ssiOld"/>.
        /// </summary>
        /// <param name="name">Name of the search field.</param>
        /// <param name="flags">A dictionary of flag names and their values.</param>
        /// <param name="fieldname">Custom field name.</param>
        /// <param name="ssiOld">Original search settings to detect changes against. Changes are not detected if null is passed.</param>
        /// <param name="fieldChanged">After the method returns, indicates whether the field's definition has changed when compared to <paramref name="ssiOld"/>. Set to false if <paramref name="ssiOld"/> is null.</param>
        /// <returns><see cref="SearchSettingsInfo"/> initialized from given parameters.</returns>
        public static SearchSettingsInfo CreateSearchSettings(string name, IDictionary<string, bool> flags, string fieldname, SearchSettingsInfo ssiOld, out bool fieldChanged)
        {
            var ssi = new SearchSettingsInfo
            {
                ID = (ssiOld != null) ? ssiOld.ID : Guid.NewGuid(),
                Name = name,
            };

            var flagNames = flags.Keys;
            foreach (string flagName in flagNames)
            {
                ssi.SetFlag(flagName, flags[flagName]);
            }

            fieldname = ValidationHelper.GetCodeName(fieldname.Trim(), maxLength: 200);
            if (!String.IsNullOrEmpty(fieldname))
            {
                ssi.FieldName = fieldname;
            }

            fieldChanged = ssiOld != null && (ssiOld.FieldName != fieldname || flagNames.Any(flagName => ssiOld.GetFlag(flagName) != flags[flagName]));

            return ssi;
        }


        /// <summary>
        /// Returns the default search settings of the class.
        /// </summary>
        /// <param name="className">Name of the class</param>
        public static string GetDefaultSearchSettings(string className)
        {
            return GetDefaultSearchSettings(DataClassInfoProvider.GetDataClassInfo(className));
        }


        /// <summary>
        /// Returns the default search settings of the class.
        /// </summary>
        /// <param name="dci">Class to process</param>
        public static string GetDefaultSearchSettings(DataClassInfo dci)
        {
            // Store values to DB
            if (dci != null)
            {
                var fields = new SearchSettings();
                var attributes = dci.GetSearchIndexColumns();

                foreach (ColumnDefinition column in attributes)
                {
                    var ssi = CreateDefaultSearchSettings(column.ColumnName, column.ColumnType);

                    fields.SetSettingsInfo(ssi);
                }

                return fields.GetData();
            }

            return null;
        }


        /// <summary>
        /// Creates the default search settings for given column name and its data type.
        /// </summary>
        /// <param name="name">Name of column the <see cref="SearchSettings"/> is being created for.</param>
        /// <param name="dataType">Data type of the column.</param>
        /// <returns>Search settings for given column.</returns>
        /// <remarks>
        /// The process of default search settings creation can be customized via <see cref="CreatingDefaultSearchSettings"/> event.
        /// </remarks>
        /// <seealso cref="CreatingDefaultSearchSettings"/>
        public static SearchSettingsInfo CreateDefaultSearchSettings(string name, Type dataType)
        {
            var content = GetSearchFieldDefaultValue(SearchSettings.CONTENT, dataType);
            var searchable = GetSearchFieldDefaultValue(SearchSettings.SEARCHABLE, dataType);
            var tokenized = GetSearchFieldDefaultValue(SearchSettings.TOKENIZED, dataType);
            var flags = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase)
                    {
                        { SearchSettings.CONTENT, content },
                        { SearchSettings.SEARCHABLE, searchable },
                        { SearchSettings.TOKENIZED, tokenized }
                    };

            var ssi = CreateSearchSettings(name, flags, "");
            var eventArgs = new CreateDefaultSearchSettingsEventArgs
            {
                SearchSettings = ssi,
                Name = name,
                DataType = dataType
            };

            using (CreatingDefaultSearchSettings.StartEvent(eventArgs))
            {
                return ssi;
            }
        }


        /// <summary>
        /// Sets default class search columns.
        /// </summary>
        /// <param name="dci">DataClass to process</param>
        public static void SetDefaultClassSearchColumns(DataClassInfo dci)
        {
            if (dci == null)
            {
                return;
            }

            dci.ClassSearchTitleColumn = DEFAULT_SEARCH_TITLE_COLUMN;
            dci.ClassSearchContentColumn = DEFAULT_SEARCH_CONTENT_COLUMN;
            dci.ClassSearchCreationDateColumn = DEFAULT_SEARCH_CREATION_DATE_COLUMN;
        }

        #endregion


        #region "Search content extraction methods"

        /// <summary>
        /// Extracts the search information from the info. Returns null by default. Should be overridden by specific info.
        /// </summary>
        /// <param name="info">Object to extract search content from</param>
        /// <param name="context">Extraction context passed to the extractor in case the value is not cached</param>
        /// <param name="cachedValueUsed">Indicates whether the cached value was used or the extractor has to be called</param>
        public static XmlData GetBinaryDataSearchContent(BaseInfo info, ExtractionContext context, out bool cachedValueUsed)
        {
            cachedValueUsed = false;

            var ti = info.TypeInfo;

            var extension = info.GetStringValue(ti.ExtensionColumn, "");
            if (!IsSearchAllowedExtension(extension, info.Generalized.ObjectSiteID))
            {
                return null;
            }

            var searchContentXml = info.GetStringValue(ti.SearchContentColumn, "");
            var searchContent = new XmlData();

            if (!String.IsNullOrEmpty(searchContentXml))
            {
                // Load existing data, extraction is not needed
                searchContent.LoadData(searchContentXml);

                // No extraction processed, cached value used
                cachedValueUsed = true;
            }
            else
            {
                // Try to extract search content from the binary data
                try
                {
                    int fileSize = -1;
                    if (ti.SizeColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                    {
                        // Get the file size
                        fileSize = info.GetIntegerValue(ti.SizeColumn, -1);
                    }
                    if ((MaxAttachmentSize <= 0) || (fileSize < MaxAttachmentSize))
                    {
                        // Extract only when MaxAttachmentSize is not defined or the file size is lower than the bound
                        var binaryData = info.Generalized.GetBinaryData();
                        if (binaryData != null)
                        {
                            // Extract the search content
                            using (binaryData.Stream)
                            {
                                searchContent = SearchTextExtractorManager.ExtractData(extension, binaryData, context);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    CoreServices.EventLog.LogException("Indexing " + extension + " file", "GetBinaryDataSearchContent", ex);
                }
            }

            return searchContent;
        }


        /// <summary>
        /// Check if file extension is allowed for indexing such attachment in search index.
        /// </summary>
        /// <param name="ext">Attachment extension</param>
        /// <param name="siteId">ID of the node site</param>
        /// <returns>True if extension is allowed otherwise false</returns>
        public static bool IsSearchAllowedExtension(string ext, int siteId)
        {
            var allowed = SettingsKeyInfoProvider.GetValue("CMSSearchAllowedFileTypes", siteId);
            if (String.IsNullOrEmpty(allowed))
            {
                // Empty setting means all extensions are allowed
                return true;
            }

            string allowedExt = ";" + allowed.Trim(';').ToLowerInvariant() + ";";
            try
            {
                return allowedExt.Contains(";" + ext.ToLowerInvariant().TrimStart('.') + ";");
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }
}