using System;
using System.Collections.Generic;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.Search;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Indexer for document category
    /// </summary>
    public class DocumentCategorySearchIndexer : SearchIndexer
    {
        /// <summary>
        /// Indexes the document category
        /// </summary>
        /// <param name="sti">Search task</param>
        protected override void ExecuteProcessTask(SearchTaskInfo sti)
        {
            // Get document index ids
            List<int> indexIds = SearchIndexInfoProvider.GetIndexIDs(new List<string> { TreeNode.OBJECT_TYPE }, SearchIndexInfo.LUCENE_SEARCH_PROVIDER);
            if (indexIds == null)
            {
                return;
            }

            // Loop thru all IDs
            foreach (int indexId in indexIds)
            {
                // Get index info
                SearchIndexInfo csii = SearchIndexInfoProvider.GetSearchIndexInfo(indexId);
                if (csii == null)
                {
                    continue;
                }

                // Get searcher object
                IIndexSearcher cis = csii.Provider.GetSearcher(true);
                if ((cis == null) || (cis.NumberOfDocuments() <= 0))
                {
                    continue;
                }

                // Create query to get all documents with required category ID
                string query = SearchSyntaxHelper.GetFieldCondition(DocumentSearchIndexer.FIELD_DOCUMENTCATEGORYIDS, sti.SearchTaskValue);

                // Search documents
                var hits = cis.Search(query, csii.Provider.GetAnalyzer(true));

                // Check whether exists at least one record
                if ((hits == null) || (hits.Length() <= 0))
                {
                    continue;
                }

                // Loop thru all document
                for (int i = 0; i < hits.Length(); i++)
                {
                    // Get document from collection of results
                    var cd = hits.Doc(i);
                    if (cd == null)
                    {
                        continue;
                    }

                    string id = cd.Get(SearchFieldsConstants.ID);
                    if (String.IsNullOrEmpty(id))
                    {
                        continue;
                    }

                    int docId = ValidationHelper.GetInteger(id.Split(';')[0], 0);
                    if (docId > 0)
                    {
                        // Update document in the index for specified document id
                        DocumentUpdate(docId, sti);
                    }
                }
            }
        }
    }
}
