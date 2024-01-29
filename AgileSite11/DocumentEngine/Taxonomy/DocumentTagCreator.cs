using CMS.Helpers;
using CMS.Taxonomy;

namespace CMS.DocumentEngine.Taxonomy
{
    /// <summary>
    /// Class providing functionality creating document tags.
    /// </summary>
    internal class DocumentTagCreator
    {
        private TreeNode Document
        {
            get;
            set;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentTagCreator"/> class.
        /// </summary>
        /// <param name="document">Document for tag creation.</param>
        public DocumentTagCreator(TreeNode document)
        {
            Document = document;
        }


        /// <summary>
        /// Create document tags. Tags are taken from <see cref="TreeNode.DocumentTags"/> field and from specified or inherited tag group <see cref="TreeNode.DocumentTagGroupID"/>
        /// </summary>
        public void CreateTags()
        {
            // Get tag group ID
            var tagGroupId = Document.DocumentTagGroupID;
            if (tagGroupId <= 0)
            {
                tagGroupId = ValidationHelper.GetInteger(Document.GetInheritedValue("DocumentTagGroupID", false), 0);
            }

            // No tag group
            if (tagGroupId <= 0)
            {
                return;
            }

            // Add tags
            DocumentTagInfoProvider.AddTags(tagGroupId, Document.DocumentID, Document.DocumentTags);

            if (Document.NodeHasChildren)
            {
                var originalState = new OriginalStateOfTagsInDocument(Document);
                new DocumentTagUpdater(Document, originalState).UpdateInheritedChildTags(true);
            }

            CacheHelper.TouchKey(TaxonomyCacheHelper.GetTagGroupCacheDependencyKey(tagGroupId));
        }

    }
}
