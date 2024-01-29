using System;

using CMS.Base;

namespace CMS.DocumentEngine.Taxonomy
{
    /// <summary>
    /// Encapsulates original state of tags settings in a page.
    /// </summary>
    internal class OriginalStateOfTagsInDocument
    {
        public int TagGroup { get; set; }


        public string DocumentTags { get; set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="OriginalStateOfTagsInDocument"/> class.
        /// </summary>
        /// <param name="document">Document which contains original settings of tags.</param>
        public OriginalStateOfTagsInDocument(TreeNode document)
        {
            if (document == null)
            {
                throw new ArgumentException("Parameter document can not be null");
            }

            DocumentTags = document.GetOriginalValue("DocumentTags").ToString(String.Empty);
            TagGroup = document.GetOriginalValue("DocumentTagGroupID").ToInteger(0);
        }       
    }
}
