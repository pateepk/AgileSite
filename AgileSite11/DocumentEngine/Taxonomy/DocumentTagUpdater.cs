using CMS.DataEngine;
using CMS.Helpers;
using CMS.Taxonomy;

namespace CMS.DocumentEngine.Taxonomy
{
    /// <summary>
    /// The class provides methods for updating of tags in the specified page.
    /// </summary>
    internal class DocumentTagUpdater
    {
        private TreeNode Document
        {
            get;
            set;
        }


        private OriginalStateOfTagsInDocument OriginalStateOfTags
        {
            get;
            set;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentTagUpdater"/> class.
        /// </summary>
        /// <param name="document">Document which contains processed tags.</param>
        /// <param name="originalState">Original state of settings tags in given page.</param>
        public DocumentTagUpdater(TreeNode document, OriginalStateOfTagsInDocument originalState)
        {
            Document = document;
            OriginalStateOfTags = originalState;
        }


        /// <summary>
        /// Update document tag according to tag group only if necessary.
        /// </summary>
        /// <param name="forceUpdate">Force update of the tags</param>
        public void UpdateTags(bool forceUpdate)
        {
            var tagGroupId = Document.DocumentTagGroupID;
            var originalTagGroup = OriginalStateOfTags.TagGroup;
            var tagGroupChanged = tagGroupId != originalTagGroup;
            var tagsChanged = Document.DocumentTags != OriginalStateOfTags.DocumentTags;

            // Nothing changed and the update is not forced
            if (!tagsChanged && !tagGroupChanged && !forceUpdate)
            {
                return;
            }

            // Ensure inherited tag groups
            if ((tagGroupId <= 0) || (originalTagGroup <= 0))
            {
                var inheritedTagGroup = ValidationHelper.GetInteger(Document.GetInheritedValue("DocumentTagGroupID", false), 0);

                if (tagGroupId <= 0)
                {
                    tagGroupId = inheritedTagGroup;
                }

                if (originalTagGroup <= 0)
                {
                    originalTagGroup = inheritedTagGroup;
                }

                // Update the status if inherited tag group used
                tagGroupChanged = tagGroupId != originalTagGroup;
            }

            // Current or original tag group is set
            var tagGroupIsSet = (originalTagGroup > 0) || (tagGroupId > 0);

            // Nothing changed and the update is not forced (update is not forced if there is nothing to update)
            if (!tagGroupChanged && !tagsChanged && (!forceUpdate || !tagGroupIsSet))
            {
                return;
            }

            // Remove tags from original group (removal is not forced if there is nothing to remove)
            bool removeTags = (forceUpdate || (originalTagGroup > 0));
            if (removeTags)
            {
                DocumentTagInfoProvider.RemoveTags(Document.DocumentID, false);
            }

            // Add tags from current group
            if (forceUpdate || (tagGroupId > 0))
            {
                DocumentTagInfoProvider.AddTags(tagGroupId, Document.DocumentID, Document.DocumentTags);
            }

            // Delete tags with zero count (are not connected to any document)
            if (removeTags)
            {
                TagInfoProvider.DeleteNotUsedTags();
            }

            if (Document.NodeHasChildren)
            {
                UpdateInheritedChildTags(forceUpdate || tagGroupChanged);
            }

            if (tagGroupId > 0)
            {
                CacheHelper.TouchKey(TaxonomyCacheHelper.GetTagGroupCacheDependencyKey(tagGroupId));
            }

            if ((originalTagGroup > 0) && (tagGroupId != originalTagGroup))
            {
                CacheHelper.TouchKey(TaxonomyCacheHelper.GetTagGroupCacheDependencyKey(tagGroupId));
            }
        }


        internal void UpdateInheritedChildTags(bool forceUpdate)
        {
            var treeProvider = Document.TreeProvider; 

            var where = new WhereCondition().WhereNull("DocumentTagGroupID");
            foreach (var descendant in treeProvider.EnumerateDescendants(Document, where))
            {
                if (descendant == null)
                {
                    continue;
                }

                var originalState = new OriginalStateOfTagsInDocument(descendant);
                var tagsManager = new DocumentTagUpdater(descendant, originalState);
                tagsManager.UpdateTags(forceUpdate);
            }
        }
    }
}
