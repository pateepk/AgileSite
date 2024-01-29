using System;
using System.Collections.Generic;
using System.Linq;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.Search.Azure;
using CMS.Taxonomy;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Encapsulates functionality that provides category search fields of <see cref="TreeNode"/>.
    /// </summary>
    internal class DocumentCategorySearchFieldsProvider
    {
        private readonly TreeNode mTreeNode;


        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentCategorySearchFieldsProvider"/> class.
        /// </summary>
        /// <param name="treeNode">Tree node</param>
        public DocumentCategorySearchFieldsProvider(TreeNode treeNode)
        {
            mTreeNode = treeNode;
        }


        /// <summary>
        /// Adds category related search fields into search fields collection.
        /// </summary>
        /// <param name="searchFields">Search fields collection</param>
        /// <param name="includeCategories">Indicates if categories should be inserted to search content field</param>
        internal void AddCategories(ISearchFields searchFields, bool includeCategories)
        {
            HashSet<int> ids = null;
            HashSet<string> names = null;
            
            var documentCategoriesField = SearchFieldFactory.Instance.Create("DocumentCategories", typeof(IEnumerable<string>), CreateSearchFieldOption.SearchableAndRetrievableWithTokenizer);
            documentCategoriesField.Analyzer = SearchAnalyzerTypeEnum.WhiteSpaceAnalyzer;
            searchFields.Add(documentCategoriesField, () =>
            {
                EnsureCategoriesNames(ref names, ref ids);
                return names;
            });

            var documentCategoryIdsField = SearchFieldFactory.Instance.Create(DocumentSearchIndexer.FIELD_DOCUMENTCATEGORYIDS, typeof(IEnumerable<int>), CreateSearchFieldOption.SearchableAndRetrievableWithTokenizer);
            documentCategoryIdsField.Analyzer = SearchAnalyzerTypeEnum.WhiteSpaceAnalyzer;
            searchFields.Add(documentCategoryIdsField, () =>
            {
                EnsureCategoriesIds(ref ids);
                return ids;
            });

            if (includeCategories)
            {
                searchFields.AddToContentField(() =>
                {
                    EnsureCategoriesNames(ref names, ref ids);
                    return DocumentSearchHelper.GetSearchContent(names);
                });
            }
        }


        /// <summary>
        /// Azure Search does not support <c>IEnumerable&lt;int&gt;</c> fields, maps to <c>IEnumerable&lt;string&gt;</c> instead.
        /// </summary>
        internal static void MapAzureSearchDocumentCategories(object sender, CreateFieldEventArgs e)
        {
            if (e.Searchable.SearchType.Equals(PredefinedObjectType.DOCUMENT, StringComparison.OrdinalIgnoreCase))
            {
                if (e.SearchField.FieldName.Equals(DocumentSearchIndexer.FIELD_DOCUMENTCATEGORYIDS, StringComparison.OrdinalIgnoreCase))
                {
                    e.SearchField.DataType = typeof(IEnumerable<string>);
                }
            } 
        }


        /// <summary>
        /// Azure Search does not support <c>IEnumerable&lt;int&gt;</c> document values, converts to <c>IEnumerable&lt;string&gt;</c> instead.
        /// </summary>
        internal static void ConvertAzureSearchDocumentCategories(object sender, AddDocumentValueEventArgs e)
        {
            if (e.Searchable.SearchType.Equals(PredefinedObjectType.DOCUMENT, StringComparison.OrdinalIgnoreCase) && e.Name.Equals(DocumentSearchIndexer.FIELD_DOCUMENTCATEGORYIDS, StringComparison.OrdinalIgnoreCase))
            {
                e.Value = (e.Value as IEnumerable<int>)?.Select(i => i.ToString());
            }
        }


        private void EnsureCategoriesNames(ref HashSet<string> names, ref HashSet<int> ids)
        {
            EnsureCategoriesIds(ref ids);

            if (names == null)
            {
                names = GetCategoriesNames(ids);
            }
        }


        private void EnsureCategoriesIds(ref HashSet<int> ids)
        {
            if (ids == null)
            {
                ids = GetCategoriesIds(mTreeNode);
            }
        }


        /// <summary>
        /// Returns document categories names required to create category related search fields.
        /// </summary>
        /// <param name="ids">Set of category IDs</param>
        internal static HashSet<string> GetCategoriesNames(HashSet<int> ids)
        {
            if (ids.Count <= 0)
            {
                return new HashSet<string>();
            }

            var names = new HashSet<string>();
            var categories = ProviderHelper.GetInfosByIds(PredefinedObjectType.CATEGORY, ids);
            foreach (var obj in categories.Values)
            {
                var category = obj as CategoryInfo;
                if ((category == null) || !category.CategoryEnabled)
                {
                    continue;
                }

                names.Add(category.CategoryDisplayName);
            }

            return names;
        }


        /// <summary>
        /// Returns document category IDs required to create category related search fields.
        /// </summary>
        /// <param name="node">Document</param>
        internal static HashSet<int> GetCategoriesIds(TreeNode node)
        {
            var paths = DocumentCategoryInfoProvider.GetDocumentCategories(node.DocumentID)
                .Columns("CategoryIDPath")
                .WhereTrue("CategoryEnabled")
                .WhereNull("CategoryUserID")
                .GetListResult<string>();

            return GetCategoriesIds(paths);
        }


        private static HashSet<int> GetCategoriesIds(IList<string> paths)
        {
            var ids = new HashSet<int>();
            foreach (var path in paths)
            {
                foreach (int id in GetPathIds(path))
                {
                    if (!ids.Contains(id))
                    {
                        ids.Add(id);
                    }
                }
            }

            return ids;
        }


        private static IEnumerable<int> GetPathIds(string path)
        {
            return path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries)
                       .Select(i => ValidationHelper.GetInteger(i, 0));
        }
    }
}
