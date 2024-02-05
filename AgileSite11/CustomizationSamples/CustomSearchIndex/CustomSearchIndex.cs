using System;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.IO;
using CMS.Search;
using CMS.Base;

namespace CMS.CustomSearchIndex
{
    /// <summary>
    /// Sample of custom smart search index. 
    /// In this example all text files from specified directory are indexed.
    /// </summary>
    public class CustomSearchIndex : ICustomSearchIndex
    {
        #region ICustomSearchIndex Members

        /// <summary>
        /// Implementation of rebuild method.
        /// </summary>
        /// <param name="srchInfo">Search index info</param>
        public void Rebuild(SearchIndexInfo srchInfo)
        {
            // Check whether index info object is defined
            if (srchInfo != null)
            {
                // Get index writer for current index
                var iw = srchInfo.Provider.GetWriter(true);
                if (iw != null)
                {
                    try
                    {
                        // Get settings info
                        SearchIndexSettingsInfo sisi = srchInfo.IndexSettings.Items[SearchHelper.CUSTOM_INDEX_DATA];
                        // Get custom data from settings info
                        string path = Convert.ToString(sisi.GetValue("CustomData"));

                        // Check whether path is defined
                        if (!String.IsNullOrEmpty(path))
                        {
                            // Get text files from specified directory
                            string[] files = Directory.GetFiles(path, "*.txt");

                            // Loop through all files
                            foreach (string file in files)
                            {
                                // Get current file info
                                FileInfo fi = FileInfo.New(file);

                                // Get text value
                                string text = fi.OpenText().ReadToEnd();

                                // Check whether file is not empty
                                if (!String.IsNullOrEmpty(text))
                                {
                                    // Convert to lower
                                    text = text.ToLowerCSafe();
                                    // Remove diacritics
                                    text = TextHelper.RemoveDiacritics(text);

                                    // Create new document for current text file
                                    SearchDocumentParameters documentParameters = new SearchDocumentParameters()
                                    {
                                        Index = srchInfo,
                                        Type = SearchHelper.CUSTOM_SEARCH_INDEX,
                                        Id = Guid.NewGuid().ToString(),
                                        Created = fi.CreationTime
                                    };

                                    var doc = LuceneSearchDocumentHelper.ToLuceneSearchDocument(SearchHelper.CreateDocument(documentParameters));

                                    // Add content field. In this field is search processed
                                    doc.AddGeneralField(SearchFieldsConstants.CONTENT, text, SearchHelper.StoreContentField, true);

                                    // Add title field. The value of this field is used for result title
                                    doc.AddGeneralField(SearchFieldsConstants.CUSTOM_TITLE, fi.Name, true, false);

                                    // Add content field. The value of this field is used for result excerpt
                                    doc.AddGeneralField(SearchFieldsConstants.CUSTOM_CONTENT, TextHelper.LimitLength(text, 200), true, false);

                                    // Add date field. The value of this field is used for result date
                                    doc.AddGeneralField(SearchFieldsConstants.CUSTOM_DATE, fi.CreationTime, true, false);

                                    // Add url field. The value of this field is used for result url
                                    doc.AddGeneralField(SearchFieldsConstants.CUSTOM_URL, file, true, false);

                                    // Add title field. The value of this field is used for result image
                                    //SearchHelper.AddField(doc, SearchHelper.CUSTOM_IMAGEURL, "textfile.jpg", true, false);

                                    // Add document to the index
                                    iw.AddDocument(doc);
                                }
                            }

                            // Optimize index
                            iw.Optimize();
                        }
                    }
                    // Log exception
                    catch (Exception ex)
                    {
                        EventLogProvider.LogException("CustomIndex", "Rebuild", ex);
                    }
                    // Always close index writer
                    finally
                    {
                        iw.Close();
                    }
                }
            }
        }

        #endregion
    }
}