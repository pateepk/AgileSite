using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

using CMS.Core;
using CMS.DataEngine;
using CMS.DataEngine.CollectionExtensions;
using CMS.DataEngine.Query;
using CMS.DocumentEngine.Search;
using CMS.FormEngine;
using CMS.Helpers;
using CMS.PortalEngine;
using CMS.Search;
using CMS.Search.Azure;

using Microsoft.Azure.Search.Models;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Encapsulates functionality that provides search fields of <see cref="TreeNode"/>.
    /// </summary>
    internal class DocumentSearchFieldsProvider
    {
        private const string FIELD_NODEALIASPATHPREFIXES = "NodeAliasPathPrefixes";

        private static readonly Regex WildcardRegex = RegexHelper.GetRegex("{.+?}", RegexHelper.DefaultOptions);


        private readonly Dictionary<string, Func<TreeNode, string>> mSpecialFields = new Dictionary<string, Func<TreeNode, string>>(StringComparer.InvariantCultureIgnoreCase)
        {
            { "DocumentContent", GetDocumentContent },
            { "DocumentWebParts", GetWidgetsContent }
        };


        private readonly HashSet<string> mRequiredFields = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
        {
            "DocumentID",
            "NodeID",
            "NodeLinkedNodeID",
            "NodeAliasPath"
        };


        private readonly TreeNode mTreeNode;
        private readonly ISearchIndexInfo mIndex;
        private readonly ISearchFields mSearchFields;
        private bool? mIsCrawler;


        private bool IsCrawler
        {
            get
            {
                if (mIsCrawler == null)
                {
                    mIsCrawler = mIndex.IndexType == SearchHelper.DOCUMENTS_CRAWLER_INDEX;
                }

                return mIsCrawler.Value;
            }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentSearchFieldsProvider"/> class.
        /// </summary>
        /// <param name="treeNode">Tree node</param>
        /// <param name="index">Search index</param>
        /// <param name="searchFields">Search fields collection</param>
        public DocumentSearchFieldsProvider(TreeNode treeNode, ISearchIndexInfo index, ISearchFields searchFields)
        {
            mTreeNode = treeNode;
            mIndex = index;
            mSearchFields = searchFields;
        }


        /// <summary>
        /// Returns search fields collection. 
        /// </summary>
        public ISearchFields Get()
        {
            var searchFields = mSearchFields ?? Service.Resolve<ISearchFields>();

            // Although the whole document is excluded from search, some of the columns still need to be added into search fields.
            if (mTreeNode.DocumentSearchExcluded)
            {
                AddFieldsRequiredForExcludedDocument(searchFields);
            }
            else
            {
                EnsureContentField(searchFields);
                AddFields(searchFields);
                AddHTMLContent(searchFields);
                AddCustomFields(searchFields);
                AddExtendedData(searchFields);
            }

            return searchFields;
        }


        /// <summary>
        /// The excluded document needs have some of the columns included in search fields because of the processing 
        /// in <see cref="SearchIndexer.DocumentUpdate"/> (performed when update single document) where on the grounds of 
        /// value of <see cref="TreeNode.DocumentSearchExcluded"/> is document updated/removed in/from Lucene index.
        /// </summary>
        /// <param name="searchFields">Search fields.</param>
        private void AddFieldsRequiredForExcludedDocument(ISearchFields searchFields)
        {
            searchFields.Add(SearchFieldFactory.Instance.Create("DocumentSearchExcluded", typeof(bool), CreateSearchFieldOption.SearchableAndRetrievable), () => mTreeNode.DocumentSearchExcluded);
        }


        private static void EnsureContentField(ISearchFields searchFields)
        {
            searchFields.EnsureContentField();
        }


        private void AddFields(ISearchFields searchFields)
        {
            var dataClass = DataClassInfoProvider.GetDataClassInfo("cms.document");
            var fieldsSettings = dataClass.ClassSearchSettingsInfos;

            AddRequiredFields(fieldsSettings, searchFields, dataClass);
            AddSpecialFields(fieldsSettings, searchFields);
            AddStandardFields(fieldsSettings, searchFields, dataClass);
        }


        /// <summary>
        /// Adds search fields defined in <see cref="mRequiredFields"/> collection to <paramref name="searchFields"/> by turning their searchable flag to true
        /// even though user might have turned it off.
        /// </summary>
        private void AddRequiredFields(SearchSettings settings, ISearchFields searchFields, DataClassInfo dataClass)
        {
            var requiredFieldsSettings = settings.Where(s => mRequiredFields.Contains(s.Name)).ToDictionary(s => s.Name, StringComparer.OrdinalIgnoreCase);

            if (mRequiredFields.Any(requiredField => !requiredFieldsSettings.ContainsKey(requiredField)))
            {
                throw new InvalidOperationException($"Required field(s) '{string.Join(", ", mRequiredFields.Except(requiredFieldsSettings.Keys))}' must be defined in search settings of '{dataClass.ClassName}' data class.");
            }

            foreach (var s in requiredFieldsSettings.Values)
            {
                var dataType = dataClass.GetSearchColumnType(s.Name);

                var field = SearchFieldFactory.Instance.CreateFromSettings(s, dataType);

                // Set the flags the required field would have, even when not configured in search settings
                var systemField = SearchFieldFactory.Instance.Create(s.Name, dataType, CreateSearchFieldOption.SearchableAndRetrievable);
                MergeFieldFlags(systemField, field);

                searchFields.Add(systemField, () => mTreeNode.GetValue(s.Name));
            }

            AddClassNameField(searchFields);
        }


        /// <summary>
        /// Merges all flags from <paramref name="secondary"/> search field to <paramref name="primary"/> search field.
        /// </summary>
        private void MergeFieldFlags(ISearchField primary, ISearchField secondary)
        {
            foreach (var flagName in secondary.FlagNames)
            {
                primary.SetFlag(flagName, primary.GetFlag(flagName) || secondary.GetFlag(flagName));
            }
        }


        private void AddStandardFields(SearchSettings settings, ISearchFields searchFields, DataClassInfo dataClass)
        {
            settings.Where(s => !mSpecialFields.ContainsKey(s.Name) && !mRequiredFields.Contains(s.Name))
                    .ToList()
                    .ForEach(s =>
                    {
                        if (!IsCrawler)
                        {
                            searchFields.AddContentField(mTreeNode, mIndex, s);
                        }

                        searchFields.AddIndexField(mTreeNode, mIndex, s, dataClass.GetSearchColumnType(s.Name));
                    });
        }


        private void AddSpecialFields(SearchSettings settings, ISearchFields searchFields)
        {
            if (IsCrawler)
            {
                return;
            }

            settings.Where(s => mSpecialFields.ContainsKey(s.Name))
                    .ToList()
                    .ForEach(s =>
                {
                    var value = mSpecialFields[s.Name](mTreeNode);
                    searchFields.AddToContentField(() => searchFields.PrepareContentValue(value, true));
                });
        }


        private void AddExtendedData(ISearchFields searchFields)
        {
            if (IsCrawler)
            {
                return;
            }

            AddDocumentAliases(searchFields);
            AddWebPartsContent(searchFields);

            var indexSettings = DocumentSearchHelper.GetIncludedSettings(mTreeNode, mIndex.IndexSettings);

            AddCategories(searchFields, indexSettings.IncludeCategories);
            AddAttachments(searchFields, indexSettings.IncludeAttachments);
        }


        private void AddCategories(ISearchFields searchFields, bool includeCategories)
        {
            var categorySearchFields = new DocumentCategorySearchFieldsProvider(mTreeNode);
            categorySearchFields.AddCategories(searchFields, includeCategories);
        }


        private void AddAttachments(ISearchFields searchFields, bool include)
        {
            var attachmentSearchFields = new DocumentAttachmentSearchFieldsProvider(mTreeNode);
            attachmentSearchFields.AddAttachments(searchFields, include);
        }


        private void AddWebPartsContent(ISearchFields searchFields)
        {
            searchFields.AddToContentField(() => searchFields.PrepareContentValue(GetWebPartsContent(), true));
        }


        private void AddDocumentAliases(ISearchFields searchFields)
        {
            searchFields.AddToContentField(() => searchFields.PrepareContentValue(GetDocumentAliasesContent(), true));
        }


        private void AddHTMLContent(ISearchFields searchFields)
        {
            if (!IsCrawler)
            {
                return;
            }

            searchFields.AddToContentField(() => GetCrawlerContent(mIndex));
        }


        private void AddClassNameField(ISearchFields searchFields)
        {
            searchFields.Add(SearchFieldFactory.Instance.Create("classname", typeof(string), CreateSearchFieldOption.SearchableAndRetrievable), () => mTreeNode.NodeClassName?.ToLowerInvariant());
        }


        private void AddCustomFields(ISearchFields searchFields)
        {
            var customDataClasses = GetCustomDataClasses(searchFields);
            foreach (var customDataClass in customDataClasses)
            {
                if ((customDataClass == null) || !customDataClass.ClassSearchEnabled)
                {
                    continue;
                }

                var fieldsSettings = customDataClass.ClassSearchSettingsInfos;
                if (fieldsSettings == null)
                {
                    continue;
                }
                
                foreach (var setting in fieldsSettings)
                {
                    if (!IsCrawler)
                    {
                        // Strip tags - custom fiedls may content HTML.
                        searchFields.AddContentField(mTreeNode, mIndex, setting, true);
                    }

                    searchFields.AddIndexField(mTreeNode, mIndex, setting, customDataClass.GetSearchColumnType(setting.Name));
                }
            }
        }


        private IEnumerable<DataClassInfo> GetCustomDataClasses(ISearchFields searchFields)
        {
            IEnumerable<DataClassInfo> customDataClasses;

            // When values are not stored in fields collection, get all fields from all document types
            if (searchFields.StoreValues)
            {
                customDataClasses = new[]
                {
                    DataClassInfoProvider.GetDataClassInfo(mTreeNode.NodeClassName)
                };
            }
            else
            {
                customDataClasses = DocumentTypeHelper.GetDocumentTypeClasses();
            }

            return customDataClasses;
        }


        internal static string GetDocumentContent(TreeNode doc)
        {
            var documentContent = ValidationHelper.GetString(doc.GetValue("DocumentContent"), string.Empty);
            if (string.IsNullOrEmpty(documentContent))
            {
                return string.Empty;
            }

            return new SearchEditableContentProvider(documentContent).GetSearchContent();
        }


        /// <summary>
        /// Download and process HTML content and returns parsed content.
        /// </summary>
        private string GetCrawlerContent(ISearchIndexInfo index)
        {
            var crawlerContent = new CrawlerSearchContentProvider(mTreeNode, new SearchCrawler());
            return crawlerContent.GetContent(index);
        }


        /// <summary>
        /// Returns string containing all relevant aliases that should be inserted into search content field.
        /// </summary>
        internal string GetDocumentAliasesContent()
        {
            var cultureWhere = new WhereCondition()
                .WhereEquals("AliasCulture", mTreeNode.DocumentCulture)
                .Or().WhereEquals("AliasCulture".AsColumn().IsNull(string.Empty), string.Empty);

            var documentAliases = DocumentAliasInfoProvider.GetDocumentAliases()
                .Where("AliasNodeID", QueryOperator.Equals, mTreeNode.NodeID)
                .Where(new WhereCondition(cultureWhere))
                .Columns("AliasURLPath");

            var paths = documentAliases.ToList()
                                       .Select(x => x.AliasURLPath)
                                       .Select(x => WildcardRegex.Replace(x, string.Empty))
                                       .ToHashSetCollection();

            return DocumentSearchHelper.GetSearchContent(paths);
        }


        /// <summary>
        /// Processes web parts content and returns string that should be inserted into search content field.
        /// </summary>
        internal string GetWebPartsContent()
        {
            // Get page template
            int templateId = mTreeNode.GetUsedPageTemplateId();

            var content = new StringBuilder(1024);

            GeneralizedInfo pageTemplate = PageTemplateInfoProvider.GetPageTemplateInfo(templateId);
            if (pageTemplate != null)
            {
                // Load Webparts
                string pageTemplateWebparts = ValidationHelper.GetString(pageTemplate.GetValue("PageTemplateWebparts"), String.Empty);

                if (!string.IsNullOrEmpty(pageTemplateWebparts))
                {
                    try
                    {
                        XmlDocument xmlTemplate = new XmlDocument();
                        xmlTemplate.LoadXml(pageTemplateWebparts);
                        XmlNodeList list = xmlTemplate.SelectNodes("//property[@name='text' or @name='contentbefore' or @name='contentafter']");

                        if (list != null)
                        {
                            // Loop thru all properties                            
                            foreach (XmlNode node in list)
                            {
                                // Insert content properties to content
                                content.Append(" ", node.InnerText);
                            }
                        }
                    }
                    // ReSharper disable once EmptyGeneralCatchClause
                    catch
                    {
                        // Do nothing
                    }
                }
            }

            return content.ToString();
        }


        /// <summary>
        /// Processes document web parts content and returns string that should be inserted into search content field.
        /// </summary>
        internal static string GetWidgetsContent(TreeNode node)
        {
            var pageTemplateInstance = GetPageTemplateInstance(node);
            var webParts = pageTemplateInstance.WebPartZones.SelectMany(webPartZone => webPartZone.WebParts);
            return GetDocumentWidgetsTextContent(webParts);
        }


        internal static PageTemplateInstance GetPageTemplateInstance(TreeNode node)
        {
            var documentWebparts = ValidationHelper.GetString(node.GetValue("DocumentWebparts"), String.Empty);
            return new PageTemplateInstance(documentWebparts);
        }


        internal static string GetDocumentWidgetsTextContent(IEnumerable<WebPartInstance> documentWebparts)
        {
            var content = new HashSet<string>();
            foreach (var webpart in documentWebparts)
            {
                var widgetInfo = WidgetInfoProvider.GetWidgetInfo(webpart.WebPartType);
                if (widgetInfo == null)
                {
                    continue;
                }

                var webPartInfo = WebPartInfoProvider.GetWebPartInfo(widgetInfo.WidgetWebPartID);
                if (webPartInfo == null)
                {
                    continue;
                }

                var widgetProperties = FormHelper.MergeFormDefinitions(webPartInfo.WebPartProperties, widgetInfo.WidgetProperties);
                var formInfo = PortalFormHelper.GetWidgetFormInfo(widgetInfo.WidgetName, webpart.ParentZone.WidgetZoneType, widgetProperties, true);
                if (formInfo == null)
                {
                    continue;
                }

                content.AddRangeToSet(GetWidgetPropertiesValues(formInfo, webpart));
            }

            return DocumentSearchHelper.GetSearchContent(content);
        }


        private static IEnumerable<string> GetWidgetPropertiesValues(FormInfo formInfo, WebPartInstance webpart)
        {
            return GetTextFieldNames(formInfo)
                .Where(name => webpart.Properties.ContainsKey(name))
                .Select(name => webpart.Properties[name].ToString())
                .Where(value => !string.IsNullOrEmpty(value));
        }


        private static IEnumerable<string> GetTextFieldNames(FormInfo formInfo)
        {
            return formInfo.GetFields(true, true)
                           .Where(f => f.DataType.Equals(FieldDataType.Text, StringComparison.OrdinalIgnoreCase) || f.DataType.Equals(FieldDataType.LongText, StringComparison.OrdinalIgnoreCase))
                           .Select(f => f.Name);
        }


        /// <summary>
        /// Adds NodeAliasPathPrefixes field for Azure index.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event arguments.</param>
        /// <remarks>
        /// Azure does not support filtering by range (e.g. filter documents located in subtree of a path).
        /// To enable such filtering, all path prefixes are stored in collection, so it is possible to get all documents having a path in the collection
        /// i.e. documents located in subtree of the path.
        /// </remarks>
        internal static void AddNodeAliasPathPrefixesField(object sender, CreateFieldsEventArgs e)
        {
            if (e.Searchable.SearchType.Equals(PredefinedObjectType.DOCUMENT, StringComparison.OrdinalIgnoreCase))
            {
                var collectionOfStringDataType = Microsoft.Azure.Search.Models.DataType.Collection(Microsoft.Azure.Search.Models.DataType.String);
                e.Fields.Add(new Field(NamingHelper.GetValidFieldName(FIELD_NODEALIASPATHPREFIXES), collectionOfStringDataType)
                {
                    IsFilterable = true,
                    IsRetrievable = false
                });
            }
        }


        /// <summary>
        /// Divides NodeAliasPath into NodeAliasPathPrefixes and adds the value into field for Azure index.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event arguments.</param>
        /// <remarks>
        /// Azure does not support filtering by range (e.g. filter documents located in subtree of a path).
        /// To enable such filtering, all path prefixes are stored in collection, so it is possible to get all documents having a path in the collection
        /// i.e. documents located in subtree of the path.
        /// </remarks>
        internal static void DivideNodeAliasPathIntoNodeAliasPathPrefixes(object sender, CreateDocumentEventArgs e)
        {
            if (e.Searchable.SearchType.Equals(PredefinedObjectType.DOCUMENT, StringComparison.OrdinalIgnoreCase))
            {
                var nodeAliasPath = (string)e.SearchDocument.GetValue("NodeAliasPath");
                var nodeAliasPathPrefixes = GetNodeAliasPathPrefixes(nodeAliasPath);

                e.Document.Add(NamingHelper.GetValidFieldName(FIELD_NODEALIASPATHPREFIXES), nodeAliasPathPrefixes);
            }
        }


        /// <summary>
        /// Gets all path prefixes of given <paramref name="nodeAliasPath"/> (incl. own prefix).
        /// </summary>
        /// <param name="nodeAliasPath">Node alias path.</param>
        /// <returns>Enumeration of path prefixes.</returns>
        private static IEnumerable<string> GetNodeAliasPathPrefixes(string nodeAliasPath)
        {
            var nodeAliasPathPrefixes = new List<string>();
            var slashIndex = 0;

            while ((slashIndex = nodeAliasPath.IndexOf("/", slashIndex + 1, StringComparison.Ordinal)) > 0)
            {
                nodeAliasPathPrefixes.Add(nodeAliasPath.Substring(0, slashIndex));
            }

            nodeAliasPathPrefixes.Add(nodeAliasPath);

            return nodeAliasPathPrefixes;
        }
    }
}
