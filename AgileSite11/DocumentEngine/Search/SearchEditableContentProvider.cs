using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Base;
using CMS.DataEngine;
using CMS.PortalEngine;

namespace CMS.DocumentEngine.Search
{
    /// <summary>
    /// Provides filtered content from document content field.
    /// </summary>
    internal class SearchEditableContentProvider
    {
        private const string EDITABLE_IMAGE_WEBPART_TAG = "<image>";


        private string Content
        {
            get;
            set;
        }


        /// <summary>
        /// Creates an instance of the <see cref="SearchEditableContentProvider"/> class.
        /// </summary>
        /// <param name="content">XML with editable webparts.</param>
        public SearchEditableContentProvider(string content)
        {
            Content = content;
        }


        /// <summary>
        /// Returns filtered content from DocumentContent field. This method filters EditableImage 
        /// webparts from content. If processed content does not contain XML with editable webparts, 
        /// the method returns Content without modifications.
        /// </summary>
        public string GetSearchContent()
        {
            if (string.IsNullOrEmpty(Content))
            {
                return string.Empty;
            }
            
            var items = new EditableItems();
            items.LoadContentXml(Content);

            var content = GetEditableContent(items.EditableWebParts);
            content.AddRange(GetEditableContent(items.EditableRegions));

            return DocumentSearchHelper.GetSearchContent(content);
        }


        private static HashSet<string> GetEditableContent(MultiKeyDictionary<string> editableItems)
        {
            return editableItems.TypedValues
                                .Where(item => !IsEditableImage(item))
                                .ToHashSet();
        }


        private static bool IsEditableImage(string itemContent)
        {
            return itemContent != null && itemContent.StartsWith(EDITABLE_IMAGE_WEBPART_TAG, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
