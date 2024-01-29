using CMS.Base;
using CMS.DataEngine;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Document search event arguments
    /// </summary>
    public class DocumentSearchEventArgs : CMSEventArgs
    {
        /// <summary>
        /// Document node
        /// </summary>
        public TreeNode Node
        {
            get;
            set;
        }


        /// <summary>
        /// Document content, used only when it makes sense: GetContent
        /// </summary>
        public string Content
        {
            get;
            set;
        }


        /// <summary>
        /// Search settings for this document
        /// </summary>
        public SearchIndexSettingsInfo Settings
        {
            get;
            set;
        }


        /// <summary>
        /// Search index info
        /// </summary>
        public ISearchIndexInfo IndexInfo
        {
            get;
            set;
        }


        /// <summary>
        /// Search document
        /// </summary>
        public SearchDocument SearchDocument
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the content is indexed for a crawler
        /// </summary>
        public bool IsCrawler
        {
            get;
            set;
        }
    }
}