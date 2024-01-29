using System;
using System.Collections.Generic;
using System.Data;
using System.Collections;
using System.Linq;

using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.Search;
using CMS.Base;

namespace CMS.SearchProviderSQL
{
    /// <summary>
    /// Class providing searching.
    /// </summary>
    public class SearchProvider : ISearchProvider
    {
        /// <summary>
        /// Supplementary constant to specify all document cultures.
        /// </summary>
        public const string ALL_CULTURES = "##ALL##";

        /// <summary>
        /// Supplementary constant to specify all sites.
        /// </summary>
        public const string ALL_SITES = "##ALL##";


        /// <summary>
        /// Searches data and returns results.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="searchNodePath">Search node path</param>
        /// <param name="cultureCode">Culture code</param>
        /// <param name="searchExpression">Search expression</param>
        /// <param name="searchMode">Search mode</param>
        /// <param name="searchChildNodes">Search child nodes</param>
        /// <param name="classNames">Class names</param>
        /// <param name="filterResultsByReadPermission">Filter results by read permission?</param>
        /// <param name="searchOnlyPublished">Search only published?</param>
        /// <param name="whereCondition">Where condition</param>
        /// <param name="orderBy">Order by clause</param>
        /// <param name="combineWithDefaultCulture">Specifies if return the default culture document when specified culture not found</param>
        public DataSet Search(string siteName, string searchNodePath, string cultureCode, string searchExpression, SearchModeEnum searchMode, bool searchChildNodes, string classNames, bool filterResultsByReadPermission, bool searchOnlyPublished, string whereCondition, string orderBy, bool combineWithDefaultCulture)
        {
            // Prepare the where condition
            whereCondition = GetCompleteWhereCondition(siteName, searchNodePath, cultureCode, combineWithDefaultCulture, whereCondition, searchOnlyPublished, -1);

            DataSet searchResultDS = null;

            // Get classnames if not defined
            IEnumerable<string> classes;

            if (String.IsNullOrEmpty(classNames))
            {
                // Get the result classes
                var existingClasses = DocumentTypeHelper.GetClassNames(new WhereCondition(whereCondition)).ToList();
                if (existingClasses.Count == 0)
                {
                    return null;
                }

                classes = existingClasses;
            }
            else
            {
                classes = classNames.Split(';');
            }

            // Exclude classNames, which are in CMSExcludeDocumentTypesFromSearch key in config file
            var excludedClassNames = SettingsKeyInfoProvider.GetValue(siteName + ".CMSExcludeDocumentTypesFromSearch").Trim().Split(';');
            if (excludedClassNames.Length > 0)
            {
                classes = classes.Except(excludedClassNames, StringComparer.InvariantCultureIgnoreCase);
            }

            // Prepare search words
            string[] searchWords = GetSearchWords(searchExpression, searchMode);

            // Search each class 
            foreach (string className in classes)
            {
                if (String.IsNullOrEmpty(className))
                {
                    continue;
                }

                DataSet classSearchResultDS = null;

                // Search each word
                foreach (string word in searchWords)
                {
                    if (String.IsNullOrEmpty(word))
                    {
                        continue;
                    }

                    // Prepare the parameters
                    var parameters = new QueryDataParameters();
                    parameters.Add("@Expression", word);

                    // Backward compatibility
                    parameters.Add("@NodeAliasPath", searchNodePath);
                    parameters.Add("@SiteName", siteName);
                    parameters.Add("@DocumentCulture", cultureCode);
                    parameters.Add("@DefaultCulture", CultureHelper.GetDefaultCultureCode(siteName));
                    parameters.Add("@ClassName", className);

                    var wordSearchResultDS = GetWordSearchResult(className, word, whereCondition, orderBy, parameters);

                    // Document fields
                    DataSet documentSearchResultDS = null;
                    var docQuery = QueryInfoProvider.GetQueryInfo("CMS.Root.SearchDocuments", false);
                    if (docQuery != null)
                    {
                        var where = SqlHelper.AddWhereCondition("ClassName = N'" + SqlHelper.EscapeQuotes(className) + "'", whereCondition);
                        documentSearchResultDS = ConnectionHelper.ExecuteQuery(docQuery.QueryFullName, parameters, where, orderBy);
                    }

                    // Attachments
                    DataSet attachmentSearchResultDS = null;
                    var attQuery = QueryInfoProvider.GetQueryInfo("CMS.Root.SearchAttachments", false);
                    if (attQuery != null)
                    {
                        var where = SqlHelper.AddWhereCondition("ClassName = N'" + SqlHelper.EscapeQuotes(className) + "'", whereCondition);
                        attachmentSearchResultDS = ConnectionHelper.ExecuteQuery(attQuery.QueryFullName, parameters, where, orderBy);
                    }

                    if (!DataHelper.DataSourceIsEmpty(wordSearchResultDS) || !DataHelper.DataSourceIsEmpty(attachmentSearchResultDS) || !DataHelper.DataSourceIsEmpty(documentSearchResultDS))
                    {
                        // First result
                        if (classSearchResultDS == null)
                        {
                            // Add current results
                            AddResultItems(ref classSearchResultDS, wordSearchResultDS);
                            AddResultItems(ref classSearchResultDS, attachmentSearchResultDS);
                            AddResultItems(ref classSearchResultDS, documentSearchResultDS);

                            classSearchResultDS.AcceptChanges();
                        }
                        else if (searchMode == SearchModeEnum.AllWords)
                        {
                            // Intersect the results
                            IntersectResultItems(ref classSearchResultDS, new[] { wordSearchResultDS, documentSearchResultDS, attachmentSearchResultDS });

                            // Accept intersection
                            classSearchResultDS.AcceptChanges();
                        }
                        else
                        {
                            // Other mode, add new items
                            AddResultItems(ref classSearchResultDS, wordSearchResultDS);
                            AddResultItems(ref classSearchResultDS, attachmentSearchResultDS);
                            AddResultItems(ref classSearchResultDS, documentSearchResultDS);

                            classSearchResultDS.AcceptChanges();
                        }
                    }
                    else if (searchMode == SearchModeEnum.AllWords)
                    {
                        // If all words and result empty, result for class is empty
                        classSearchResultDS = null;
                        break;
                    }
                }

                // If some results found, add to the resulting DataSet
                if (!DataHelper.DataSourceIsEmpty(classSearchResultDS))
                {
                    if (searchResultDS == null)
                    {
                        searchResultDS = classSearchResultDS;
                    }
                    else
                    {
                        searchResultDS.Merge(classSearchResultDS);
                    }
                }
            }

            // Return the result
            return searchResultDS;
        }


        /// <summary>
        /// Gets search results based on document text fields
        /// </summary>
        /// <param name="className">Class name</param>
        /// <param name="word">Word to search</param>
        /// <param name="whereCondition">Where condition</param>
        /// <param name="orderBy">Order by expression</param>
        /// <param name="queryParams">Query parameters for backward compatibility</param>
        private static DataSet GetWordSearchResult(string className, string word, string whereCondition, string orderBy, QueryDataParameters queryParams)
        {
            var classInfo = DataClassInfoProvider.GetDataClassInfo(className);
            if (classInfo == null)
            {
                throw new NullReferenceException("[SearchProvider.GetWordSearchResult]: Type '" + className + "' not found.");
            }

            // Backward compatibility
            var searchQuery = QueryInfoProvider.GetQueryInfo(className + ".SearchTree", false);
            if (searchQuery != null)
            {
                return ConnectionHelper.ExecuteQuery(searchQuery.QueryFullName, queryParams, whereCondition, orderBy);
            }

            // Get base query
            var tree = new TreeProvider();
            var query = tree.SelectNodes(className)
                            .All()
                            .AddColumn(new QueryColumn("DocumentName").As("SearchResultName"))
                            .Where(whereCondition)
                            .OrderBy(orderBy);

            // Prepare words condition
            var wordsWhere = new WhereCondition();
            if (!classInfo.ClassIsCoupledClass)
            {
                wordsWhere.WhereContains("DocumentName", word).Or().WhereContains("DocumentContent", word);
            }
            else
            {
                // Get all text fields for the type
                var type = ClassStructureInfo.GetClassInfo(className);
                var columns = type.GetColumns(typeof(string));
                foreach (var column in columns)
                {
                    wordsWhere.Or().WhereContains(column, word);
                }
            }
            query.Where(wordsWhere);

            return query.Result;
        }


        /// <summary>
        /// Intersects the result items.
        /// </summary>
        /// <param name="target">Target data</param>
        /// <param name="newData">New data (to intersect)</param>
        public static void IntersectResultItems(ref DataSet target, DataSet[] newData)
        {
            ArrayList deleteRows = new ArrayList();

            // All words, intersect the results
            foreach (DataRow dr in target.Tables[0].Rows)
            {
                int documentId = ValidationHelper.GetInteger(dr["DocumentID"], 0);
                bool present = false;

                // Check if present in new data
                foreach (DataSet ds in newData)
                {
                    if (!DataHelper.DataSourceIsEmpty(ds))
                    {
                        // Check word presence
                        DataRow[] rows = ds.Tables[0].Select("DocumentID = " + documentId.ToString());
                        if (rows.Length > 0)
                        {
                            present = true;
                            break;
                        }
                    }
                }

                // Remove the document if not found
                if (!present)
                {
                    deleteRows.Add(dr);
                }
            }

            // Delete the rows
            foreach (DataRow dr in deleteRows)
            {
                target.Tables[0].Rows.Remove(dr);
            }
        }


        /// <summary>
        /// Adds the result items to the given DataSet.
        /// </summary>
        /// <param name="target">Target DataSet</param>
        /// <param name="newData">New data</param>
        public static void AddResultItems(ref DataSet target, DataSet newData)
        {
            // If no new records, keep the result
            if (DataHelper.DataSourceIsEmpty(newData))
            {
                return;
            }

            // If target not yet exists, assign
            if (target == null)
            {
                if (newData.Tables[0].Rows.Count == 1)
                {
                    target = newData;
                    return;
                }
                else
                {
                    target = new DataSet();
                    target.Tables.Add(newData.Tables[0].Clone());
                }
            }

            // Add new documents
            foreach (DataRow dr in newData.Tables[0].Rows)
            {
                int documentId = ValidationHelper.GetInteger(dr["DocumentID"], 0);
                DataRow[] rows = target.Tables[0].Select("DocumentID = " + documentId.ToString());
                // If not found, add
                if (rows.Length <= 0)
                {
                    target.Tables[0].ImportRow(dr);
                }
            }
        }


        /// <summary>
        /// Returns the complete where condition based on the given parameters.
        /// </summary> 
        /// <param name="siteName">Nodes site name</param>
        /// <param name="aliasPath">Path. It may contain % and _ as wild card characters for any number of unknown characters or one unknown character respectively (for MS SQL)</param>
        /// <param name="cultureCode">Nodes culture code</param>
        /// <param name="combineWithDefaultCulture">Specifies if return the default culture document when specified culture not found</param>
        /// <param name="where">Where condition to use for the data selection</param>
        /// <param name="maxRelativeLevel">Maximal child level of the selected nodes</param>
        /// <param name="selectOnlyPublished">Select only published nodes</param>
        public static string GetCompleteWhereCondition(string siteName, string aliasPath, string cultureCode, bool combineWithDefaultCulture, string where, bool selectOnlyPublished, int maxRelativeLevel)
        {
            string oldWhere = where;
            where = "";

            // Add site condition
            if (siteName != ALL_SITES)
            {
                var siteInfo = new SiteInfoIdentifier(siteName);
                where = SqlHelper.AddWhereCondition(where, "NodeSiteID = " + siteInfo.ObjectID);
            }

            // Add published condition
            if (selectOnlyPublished)
            {
                where = SqlHelper.AddWhereCondition(where, TreeProvider.GetPublishedWhereCondition().ToString(true));
            }

            // Add culture condition
            if (cultureCode != ALL_CULTURES)
            {
                string defaultCulture = CultureHelper.GetDefaultCultureCode(siteName);

                if (combineWithDefaultCulture && (cultureCode.ToLowerCSafe() != defaultCulture.ToLowerCSafe()))
                {
                    where = SqlHelper.AddWhereCondition(where, "(DocumentCulture = N'" + SqlHelper.EscapeQuotes(cultureCode) + "') OR (DocumentCulture = N'" + SqlHelper.EscapeQuotes(defaultCulture) + "')");
                }
                else
                {
                    where = SqlHelper.AddWhereCondition(where, "DocumentCulture = N'" + SqlHelper.EscapeQuotes(cultureCode) + "'");
                }
            }

            // Add node level condition
            int baseLevel = aliasPath.Split('/').GetUpperBound(0);
            if (maxRelativeLevel >= 0)
            {
                int tmpNodeLevel = baseLevel + maxRelativeLevel - 1;
                where = SqlHelper.AddWhereCondition(where, "NodeLevel <= " + tmpNodeLevel);
            }

            // Add the condition to filter out documents excluded from search
            where = SqlHelper.AddWhereCondition(where, "(DocumentSearchExcluded IS NULL) OR (DocumentSearchExcluded = 0)");

            // Add alias path condition
            where = SqlHelper.AddWhereCondition(where, TreePathUtils.GetAliasPathCondition(aliasPath).ToString(true));

            // Add the additional where condition
            where = SqlHelper.AddWhereCondition(where, oldWhere);

            // Return the result
            return where;
        }


        /// <summary>
        /// Converts search expression to array of searched words according to search mode.
        /// </summary>
        /// <param name="searchExpression">Search expression</param>
        /// <param name="searchMode">Search mode</param>
        protected virtual string[] GetSearchWords(string searchExpression, SearchModeEnum searchMode)
        {
            string[] result = new string[1];

            switch (searchMode)
            {
                case SearchModeEnum.AllWords:
                case SearchModeEnum.AnyWord:
                    string[] expressions = searchExpression.Split('"');
                    int quoteNumber = expressions.GetUpperBound(0);
                    // if the expression is correctly quoted process the quotes
                    if (quoteNumber % 2 == 0)
                    {
                        int expressionIndex;
                        for (expressionIndex = expressions.GetLowerBound(0); expressionIndex <= expressions.GetUpperBound(0); expressionIndex++)
                        {
                            if (expressionIndex % 2 == 0)
                            {
                                // outside quotes - parse words and add them to the array one by one
                                string[] words = expressions[expressionIndex].Split(' ');
                                int wordIndex;
                                for (wordIndex = words.GetLowerBound(0); wordIndex <= words.GetUpperBound(0); wordIndex++)
                                {
                                    if ((words[wordIndex] != null) && (words[wordIndex].Trim() != ""))
                                    {
                                        string[] transTemp1 = new string[result.Length + 1];
                                        Array.Copy(result, transTemp1, Math.Min(result.Length, transTemp1.Length));
                                        result = transTemp1;
                                        result[result.GetUpperBound(0)] = words[wordIndex];
                                    }
                                }
                                string[] transTemp2 = new string[result.Length + 1];
                                Array.Copy(result, transTemp2, Math.Min(result.Length, transTemp2.Length));
                                result = transTemp2;
                            }
                            else
                            {
                                string[] transTemp3 = new string[result.Length + 1];
                                Array.Copy(result, transTemp3, Math.Min(result.Length, transTemp3.Length));
                                result = transTemp3;
                                result[result.GetUpperBound(0)] = expressions[expressionIndex];
                            }
                        }
                    }
                    else
                    {
                        // not correctly quoted - ignore quotes
                        result = searchExpression.Replace(@"""", "").Split(' ');
                    }
                    break;

                case SearchModeEnum.ExactPhrase:
                    result = new string[1];
                    result[0] = searchExpression;
                    break;
            }

            return result;
        }
    }
}