using System;

using CMS.DataEngine;
using CMS.Base;
using CMS.Helpers;
using CMS.Taxonomy;
using CMS.Relationships;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Functions for content macro methods.
    /// </summary>
    public class ContentTransformationFunctions
    {
        /// <summary>
        /// Returns true if document is in one/all of selected categories.
        /// </summary>
        /// <param name="document">Document to check</param>
        /// <param name="categories">Category names separated with a semicolon</param>
        /// <param name="allCategories">If true, document must be in all of the selected categories.</param>
        public static bool IsInCategories(object document, string categories, bool allCategories)
        {
            TreeNode doc = document as TreeNode;
            if (doc == null)
            {
                return false;
            }

            if (String.IsNullOrEmpty(categories))
            {
                return allCategories;
            }

            var categoryNames = categories.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries);

            var categoryCount = DocumentCategoryInfoProvider.GetDocumentCategories(doc.DocumentID)
                .WhereIn("CategoryName", categoryNames)
                .Count;

            // Return true if all/any categories were found
            return allCategories ? (categoryCount == categoryNames.Length) : (categoryCount > 0);
        }


        /// <summary>
        /// Returns true if document is translated into one of selected cultures.
        /// </summary>
        /// <param name="document">Document to check</param>
        /// <param name="cultures">Culture codes separated with a semicolon</param>
        /// <param name="publishedOnly">If true culture version must be published</param>
        public static bool IsTranslatedTo(object document, string cultures, bool publishedOnly)
        {
            TreeNode doc = document as TreeNode;
            if ((doc != null) && (!String.IsNullOrEmpty(cultures)))
            {
                string[] cultureCodes = cultures.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries);
                string where = new WhereCondition().WhereIn("DocumentCulture", cultureCodes).ToString(true);

                var cultureVersions = doc.TreeProvider.SelectNodes(doc.NodeSiteName, doc.NodeAliasPath, TreeProvider.ALL_CULTURES, false, null, where, null, -1, publishedOnly, 0, DocumentColumnLists.SELECTNODES_REQUIRED_COLUMNS);
                return !DataHelper.DataSourceIsEmpty(cultureVersions);
            }

            return false;
        }


        /// <summary>
        /// Returns true if document has any/all of the specified tags.
        /// </summary>
        /// <param name="document">Document</param>
        /// <param name="tags">Semicolon separated tags</param>
        /// <param name="allTags">Indicates whether all tags must be present or only one of them</param>
        public static bool HasTags(object document, string tags, bool allTags)
        {
            TreeNode doc = document as TreeNode;
            if (doc != null)
            {
                if (!String.IsNullOrEmpty(tags))
                {
                    string[] tagNames = tags.Split(new[] {";"}, StringSplitOptions.RemoveEmptyEntries);

                    string where = new WhereCondition().WhereIn("TagName", tagNames).ToString(true);
                    var tagInfos = TagInfoProvider.GetTags(doc.DocumentID, where);

                    // Return true if all/any tags were found
                    return allTags ? (tagInfos.Count == tagNames.Length) : (tagInfos.Count > 0);
                }

                // No tags were selected
                if (allTags)
                {
                    return true;
                }
            }

            return false;
        }


        /// <summary>
        /// Returns true if document is in specified relationship with with selected document.
        /// </summary>
        /// <param name="document">Document to be checked</param>
        /// <param name="side">Relationship side</param>
        /// <param name="relationship">Relationship name</param>
        /// <param name="relatedDocumentPath">Alias path to selected document</param>
        /// <param name="relatedDocumentSite">Selected document site name</param>
        public static bool IsInRelationship(object document, string side, string relationship, string relatedDocumentPath, string relatedDocumentSite)
        {
            TreeNode doc = document as TreeNode;
            if (doc != null)
            {
                int leftNodeID = 0;
                int rightNodeID = 0;

                // Use site of the checked document when no other is specified
                if (String.IsNullOrEmpty(relatedDocumentSite))
                {
                    relatedDocumentSite = doc.NodeSiteName;
                }

                // Prepare left and right document for relationship
                side = side.ToLowerCSafe();
                if (side == "left")
                {
                    leftNodeID = doc.NodeID;
                    rightNodeID = TreePathUtils.GetNodeIdByAliasPath(relatedDocumentSite, relatedDocumentPath);
                }
                else if (side == "right")
                {
                    leftNodeID = TreePathUtils.GetNodeIdByAliasPath(relatedDocumentSite, relatedDocumentPath);
                    rightNodeID = doc.NodeID;
                }

                // Get relationship ID from relationship name
                RelationshipNameInfo relationshipName = RelationshipNameInfoProvider.GetRelationshipNameInfo(relationship);
                if (relationshipName != null)
                {
                    // Check whether relationship between the two documents exists
                    return (RelationshipInfoProvider.GetRelationshipInfo(leftNodeID, rightNodeID, relationshipName.RelationshipNameId) != null);
                }
            }

            return false;
        }
    }
}