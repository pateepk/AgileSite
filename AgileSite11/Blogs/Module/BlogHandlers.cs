using System.Data;

using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.Search;

namespace CMS.Blogs
{
    /// <summary>
    /// Provides handlers for blogs
    /// </summary>
    internal static class BlogHandlers
    {
        /// <summary>
        /// Initializes the blog handlers
        /// </summary>
        public static void Init()
        {
            DocumentEvents.GetContent.Execute += GetContent_Execute;
        }

        
        /// <summary>
        /// Includes blog content to the document search content
        /// </summary>
        private static void GetContent_Execute(object sender, DocumentSearchEventArgs e)
        {
            if (e.IsCrawler || !e.Settings.IncludeBlogs)
            {
                return;
            }

            // Get all approved comments
            DataSet blogs = BlogCommentInfoProvider.GetApprovedComments(e.Node.DocumentID);
            if (DataHelper.DataSourceIsEmpty(blogs))
            {
                return;
            }

            e.Content += SearchHelper.AddObjectDataToDocument(e.IndexInfo, e.SearchDocument, "blog.comment", blogs);
            blogs.Dispose();
        }
    }
}