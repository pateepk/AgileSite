using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Search;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Encapsulates functionality that creates <see cref="SearchDocument"/> from given <see cref="TreeNode">document</see> and <see cref="ISearchIndexInfo">index</see>.
    /// </summary>
    internal class SearchDocumentCreator
    {
        private readonly TreeNode mTreeNode;
        private readonly ISearchIndexInfo mIndex;


        /// <summary>
        /// Initializes a new instance of the <see cref="SearchDocumentCreator"/> class.
        /// </summary>
        /// <param name="treeNode">Tree node</param>
        /// <param name="index">Search index info</param>        
        public SearchDocumentCreator(TreeNode treeNode, ISearchIndexInfo index)
        {
            mTreeNode = treeNode;
            mIndex = index;
        }


        /// <summary>
        /// Creates search document.
        /// </summary>
        public SearchDocument Create()
        {
            var doc = CreateDocument();
            var fields = GetSearchFields();

            PrepareFieldsContent(fields, doc);
            InsertFieldsToDocument(fields, doc);

            return doc;
        }


        private static void InsertFieldsToDocument(List<ISearchField> fields, SearchDocument doc)
        {
            fields.ForEach(doc.AddSearchField);
        }


        private void PrepareFieldsContent(List<ISearchField> fields, SearchDocument doc)
        {
            fields.Where(f => f.FieldName == SearchFieldsConstants.CONTENT)
                  .ToList()
                  .ForEach(f => GetFieldContent(doc, f));
        }


        private void GetFieldContent(SearchDocument doc, ISearchField field)
        {
            var e = new DocumentSearchEventArgs
            {
                Node = mTreeNode,
                IndexInfo = mIndex,
                Settings = DocumentSearchHelper.GetIncludedSettings(mTreeNode, mIndex.IndexSettings),
                SearchDocument = doc,
                Content = field.Value.ToString(),
                IsCrawler = mIndex.IndexType == SearchHelper.DOCUMENTS_CRAWLER_INDEX
            };

            DocumentEvents.GetContent.StartEvent(e);

            field.Value = e.Content;
        }


        private List<ISearchField> GetSearchFields()
        {
            var searchFields = Service.Resolve<ISearchFields>();
            searchFields.StoreValues = true;
            return mTreeNode.GetSearchFields(mIndex, searchFields).Items.ToList();
        }


        private SearchDocument CreateDocument()
        {
            var documentParameters = new SearchDocumentParameters
            {
                Index = mIndex,
                Type = mTreeNode.SearchType,
                Id = mTreeNode.GetSearchID(),
                Created = GetCreationDate(),
                SiteName = mTreeNode.NodeSiteName,
                Culture = mTreeNode.DocumentCulture
            };

            return SearchHelper.CreateDocument(documentParameters);
        }


        private DateTime GetCreationDate()
        {
            var dataClass = DataClassInfoProvider.GetDataClassInfo(mTreeNode.NodeClassName);
            if (dataClass == null)
            {
                return DateTimeHelper.ZERO_TIME;
            }

            var creationDateColumnName = dataClass.ClassSearchCreationDateColumn;
            if (string.IsNullOrEmpty(creationDateColumnName) || !mTreeNode.ContainsColumn(creationDateColumnName))
            {
                return DateTimeHelper.ZERO_TIME;
            }

            return ValidationHelper.GetDateTime(mTreeNode.GetValue(creationDateColumnName), DateTimeHelper.ZERO_TIME);
        }
    }
}
