using System;
using System.Collections.Generic;

using CMS.DataEngine;
using CMS.Search;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Encapsulates functionality that provides attachment search fields.
    /// </summary>
    internal class DocumentAttachmentSearchFieldsProvider
    {
        private readonly TreeNode mTreeNode;


        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentAttachmentSearchFieldsProvider"/> class.
        /// </summary>
        /// <param name="treeNode">Tree node</param>
        public DocumentAttachmentSearchFieldsProvider(TreeNode treeNode)
        {
            mTreeNode = treeNode;
        }


        /// <summary>
        /// Adds attachment metadata and content into search fields collection.
        /// </summary>
        /// <param name="searchFields">Search fields collection.</param>
        /// <param name="includeAttachments">Indicates if attachments should be inserted to search content field.</param>
        public void AddAttachments(ISearchFields searchFields, bool includeAttachments)
        {
            if (!mTreeNode.PublishedVersionExists || !includeAttachments)
            {
                return;
            }

            searchFields.AddToContentField(() => GetAttachments(searchFields));
        }


        private string GetAttachments(ISearchFields searchFields)
        {
            var attachments = new HashSet<string>();
            foreach (var attachment in mTreeNode.AllAttachments)
            {
                attachments.Add(GetAttachmentContent(searchFields, attachment));
            }

            return DocumentSearchHelper.GetSearchContent(attachments);
        }


        private string GetAttachmentContent(ISearchFields searchFields, DocumentAttachment attachment)
        {
            var context = new ExtractionContext
            {
                Culture = mTreeNode.DocumentCulture
            };

            try
            {
                return attachment.EnsureSearchContent(context, searchFields) ?? string.Empty;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to get search content for attachment '{attachment.AttachmentName}' (ID: {attachment.ID}). See the inner exception for details.", ex);
            }
        }
    }
}