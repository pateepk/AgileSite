using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Search;

namespace CMS.Membership
{
    /// <summary>
    /// Search indexer for users index
    /// </summary>
    public class UserSearchIndexer : SearchIndexer
    {
        /// <summary>
        /// Processes the index task
        /// </summary>
        /// <param name="sti">Search task</param>
        protected override void ExecuteProcessTask(SearchTaskInfo sti)
        {
            // Get user index ids
            List<int> indexIds = SearchIndexInfoProvider.GetIndexIDs(new List<string>() { UserInfo.OBJECT_TYPE}, SearchIndexInfo.LUCENE_SEARCH_PROVIDER);
            if (indexIds != null)
            {
                // Loop thru all user indexes
                foreach (int indexId in indexIds)
                {
                    // Get index info
                    SearchIndexInfo indexInfo = SearchIndexInfoProvider.GetSearchIndexInfo(indexId);
                    if (indexInfo != null)
                    {
                        UserInfo user = null;

                        // Generate where condition
                        string whereCondition = UserInfoProvider.GetSearchWhereCondition(indexInfo);
                        if (!String.IsNullOrEmpty(whereCondition))
                        {
                            whereCondition = SqlHelper.AddWhereCondition($"UserID = {sti.SearchTaskValue}", whereCondition);

                            // Check whether exists appropriate user for current conditions
                            user = UserInfoProvider.GetUsersDataWithSettings().Where(new WhereCondition(whereCondition)).TopN(1).FirstOrDefault();
                        }

                        // Try remove user from current index
                        if (user == null)
                        {
                            SearchHelper.Delete(SearchFieldsConstants.ID, sti.SearchTaskValue, indexInfo);
                        }
                        // Update user in current index
                        else
                        {
                            var luceneDocument = LuceneSearchDocumentHelper.ToLuceneSearchDocument(user.GetSearchDocument(indexInfo));
                            SearchHelper.Update(luceneDocument, indexInfo);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Rebuilds the user index
        /// </summary>
        /// <remarks>This method needs to be run in a thread safe way such as smart search task queue.</remarks>
        /// <param name="indexInfo">Search index</param>
        public override void Rebuild(SearchIndexInfo indexInfo)
        {
            // Check whether exist index settings
            if (indexInfo.IndexSettings != null)
            {
                string whereCondition = UserInfoProvider.GetSearchWhereCondition(indexInfo);

                #region "Indexing"

                // Get current index writer
                var iw = indexInfo.Provider.GetWriter(true);

                try
                {
                    if (iw != null)
                    {
                        // Starts first iteration
                        bool start = true;
                        // Last item id
                        int lastId = 0;
                        // Number of batch
                        int batchNumber = 0;
                        // Number of processed items
                        int itemsProcessed = 0;

                        // Documents collection
                        List<SearchDocument> documents = null;

                        // Get user iDocuments
                        while (((documents != null) && (documents.Count == indexInfo.IndexBatchSize)) || start)
                        {
                            start = false;
                            batchNumber++;
                            Logger.LogBatchStart(batchNumber, lastId, UserInfo.TYPEINFO.ObjectClassName);

                            // Get list of documents
                            documents = UserInfoProvider.GetSearchDocuments(indexInfo, whereCondition, lastId);

                            if ((documents != null) && (documents.Count > 0))
                            {
                                // Get last ID  (document id)
                                lastId = ValidationHelper.GetInteger(documents[documents.Count - 1].GetValue(SearchFieldsConstants.ID), 1);

                                // Loop thru all iDocuments
                                foreach (var doc in documents)
                                {
                                    var luceneDocument = LuceneSearchDocumentHelper.ToLuceneSearchDocument(doc);
                                    iw.AddDocument(luceneDocument);
                                }

                                iw.Flush();

                                itemsProcessed += documents.Count;
                            }

                            Logger.LogBatchEnd(itemsProcessed);
                        }
                    }

                    // Optimize index
                    SearchIndexInfoProvider.SetIndexStatus(indexInfo, IndexStatusEnum.OPTIMIZING);
                    iw?.Optimize();
                }
                catch (Exception)
                {
                    // Set statuses to error
                    SearchIndexInfoProvider.SetIndexStatus(indexInfo, IndexStatusEnum.ERROR);
                    throw;
                }
                finally
                {
                    iw?.Close();
                }

                #endregion

                SearchHelper.FinishRebuild(indexInfo);
            }
        }


        /// <summary>
        /// Checks the permissions for the given result document
        /// </summary>
        /// <param name="settings">Check permission settings</param>
        /// <param name="currentDoc">Current result document</param>
        /// <param name="index">Current document index</param>
        public override bool CheckResultPermissions(SearchResults settings, ILuceneSearchDocument currentDoc, int index)
        {
            var inRoles = settings.InRoles;
            var notInRoles = settings.NotInRoles;

            bool allow = true;

            if ((inRoles.Count > 0) || (notInRoles.Count > 0))
            {
                // Indicates whether user should be added to the result collection
                allow = (inRoles.Count == 0);

                // Get user info
                int userId = ValidationHelper.GetInteger(SearchValueConverter.StringToInt(currentDoc.Get("userid")), 0);
                IUserInfo ui = UserInfoProvider.GetUserInfo(userId);
                ui?.FilterSearchResults(inRoles, notInRoles, ref allow);
            }

            return allow;
        }


        /// <summary>
        /// Loads the results to the given result collection
        /// </summary>
        /// <param name="results">Collection of results to load</param>
        /// <param name="result">Dictionary of the loaded results indexed by their key</param>
        public override void LoadResults(IEnumerable<ILuceneSearchDocument> results, SafeDictionary<string, DataRow> result)
        {
            var userIds = new List<int>();

            // Load users
            foreach (var filteredDocument in results)
            {
                userIds.Add(ValidationHelper.GetInteger(filteredDocument.Get(SearchFieldsConstants.ID), 0));
            }

            // Load data for users
            if (userIds.Count > 0)
            {
                DataSet srchData = UserInfoProvider.GetUsersDataWithSettings().WhereIn("UserID", userIds);
                if (!DataHelper.DataSourceIsEmpty(srchData))
                {
                    // Loop thru all results and add all datarows to the result collection (ID_ObjectType)
                    foreach (DataRow dr in srchData.Tables[0].Rows)
                    {
                        result[dr["UserID"] + "_" + UserInfo.OBJECT_TYPE] = dr;
                    }
                }
            }
        }


        /// <summary>
        /// Returns the data class info for the given search document
        /// </summary>
        /// <param name="obj">Document object</param>
        public override DataClassInfo GetDataClassInfo(ILuceneSearchDocument obj)
        {
            return DataClassInfoProvider.GetDataClassInfo("cms.user");
        }


        /// <summary>
        /// Fills the data to the search result data row
        /// </summary>
        /// <param name="dr">Result data row to be filled with postprocessed results</param>
        /// <param name="resultDr">Source data row with raw search results</param>
        /// <param name="doc">Found search document</param>
        public override bool FillSearchResult(DataRow dr, DataRow resultDr, ILuceneSearchDocument doc)
        {
            var result = base.FillSearchResult(dr, resultDr, doc);
            if (result)
            {
                // Ensure title for user object
                if (String.IsNullOrEmpty(Convert.ToString(dr["title"])))
                {
                    dr["title"] = DataHelper.GetStringValue(resultDr, "UserName");
                }

                return true;
            }

            return false;
        }


        /// <summary>
        /// Returns URL to current search result item.
        /// </summary>
        /// <param name="id">ID of the item</param>
        /// <param name="type">Type of the index</param>
        /// <param name="image">Image</param>
        public override string GetSearchImageUrl(string id, string type, string image)
        {
            return AvatarInfoProvider.GetUserAvatarImageUrl(GetSearchValue(id, "UserAvatarID"), GetSearchValue(id, "UserID"), null, 0, 0, 0);
        }


        /// <summary>
        /// Gets the collection of search fields. When no SearchFields colection is provided, new is created.
        /// </summary>
        /// <param name="index">Search index</param>
        /// <param name="searchFields">Search fields collection</param>
        public override ISearchFields GetSearchFields(SearchIndexInfo index, ISearchFields searchFields = null)
        {
            return UserInfo.New().GetSearchFields(index, searchFields);
        }


        /// <summary>
        /// Checks if given <paramref name="className"/> is related to search index settings from <paramref name="indexSettings"/>.
        /// </summary>
        /// <param name="className">Class name</param>
        /// <param name="indexSettings">Search index settings</param>
        public override bool IsClassNameRelevantToIndex(string className, SearchIndexSettings indexSettings)
        {
            // Every user's index use cms.user class
            return className.Equals(UserInfo.OBJECT_TYPE, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
