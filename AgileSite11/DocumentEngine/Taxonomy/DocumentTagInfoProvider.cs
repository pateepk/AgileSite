using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.Taxonomy;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Class providing document tag management.
    /// </summary>
    public class DocumentTagInfoProvider : AbstractInfoProvider<DocumentTagInfo, DocumentTagInfoProvider>
    {
        #region "Variables"

        private static Regex mTagsRegex;

        #endregion


        #region "Properties"

        /// <summary>
        /// Regular expression for tags.
        /// </summary>
        private static Regex TagsRegex
        {
            get
            {
                return mTagsRegex ?? (mTagsRegex = RegexHelper.GetRegex(@"^(?:[ ,]*(""[^""]*""|[^"" ,]+))*[ ,]*$"));
            }
        }

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns the DocumentTagInfo structure for the specified documentTag.
        /// </summary>
        /// <param name="documentId">DocumentID</param>
        /// <param name="tagId">TagID</param>
        public static DocumentTagInfo GetDocumentTagInfo(int documentId, int tagId)
        {
            return ProviderObject.GetDocumentTagInfoInternal(documentId, tagId);
        }


        /// <summary>
        /// Returns a query for all the RelationshipInfo objects.
        /// </summary>
        public static ObjectQuery<DocumentTagInfo> GetDocumentTags()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Sets (updates or inserts) specified documentTag.
        /// </summary>
        /// <param name="documentTag">DocumentTag to set</param>
        public static void SetDocumentTagInfo(DocumentTagInfo documentTag)
        {
            ProviderObject.SetInfo(documentTag);
        }


        /// <summary>
        /// Deletes specified documentTag.
        /// </summary>
        /// <param name="documentTag">DocumentTag object</param>
        public static void DeleteDocumentTagInfo(DocumentTagInfo documentTag)
        {
            ProviderObject.DeleteInfo(documentTag);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Removes all the tags from the specified document.
        /// </summary>        
        /// <param name="documentId">ID of the document tags are removed from</param> 
        /// <param name="deleteNotUsedTags">True (default) - All tags which are not assigned to any other document are deleted, False -  Tags always stay in the system even they are not assigned to any other document. </param>
        public static void RemoveTags(int documentId, bool deleteNotUsedTags = true)
        {
            ProviderObject.RemoveTagsInternal(documentId, deleteNotUsedTags);
        }


        /// <summary>
        /// Removes a specified tags from specified group and specified document.
        /// </summary>
        /// <param name="groupId">ID of the group holding tags to remove</param>
        /// <param name="documentId">ID of the document tags are removed from</param>
        /// <param name="tags">Set of tags (separated by commas) to remove</param>
        public static void RemoveTags(int groupId, int documentId, string tags)
        {
            ProviderObject.RemoveTagsInternal(groupId, documentId, tags);
        }


        /// <summary>
        /// Inserts a specified tags of the specified group to the specified document.
        /// </summary>
        /// <param name="groupId">ID of the group inserted tags belongs to</param>
        /// <param name="documentId">ID of the document tags are inserted to</param>
        /// <param name="tags">Set of tags (separated by commas) to insert</param>
        public static void AddTags(int groupId, int documentId, string tags)
        {
            ProviderObject.AddTagsInternal(groupId, documentId, tags);
        }


        /// <summary>
        /// Adds the specific tag to the specified document.
        /// </summary>
        /// <param name="tagId">ID of the tag to add</param>
        /// <param name="documentId">ID of the document the tag is added for</param>
        public static void AddTagToDocument(int tagId, int documentId)
        {
            // Check if both IDs are valid
            if ((tagId > 0) && (documentId > 0))
            {
                // Create new document-tag relationship info
                DocumentTagInfo dti = new DocumentTagInfo();
                dti.DocumentID = documentId;
                dti.TagID = tagId;

                // Save new info
                SetDocumentTagInfo(dti);
            }
        }


        /// <summary>
        /// Removes tag from the specified document.
        /// </summary>
        /// <param name="tagId">ID of the removed tag</param>
        /// <param name="documentId">ID of the document tag is removed from</param>
        public static void RemoveTagFromDocument(int tagId, int documentId)
        {
            DocumentTagInfo infoObj = GetDocumentTagInfo(documentId, tagId);
            DeleteDocumentTagInfo(infoObj);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Returns the DocumentTagInfo structure for the specified documentID and tagID.
        /// </summary>
        /// <param name="documentId">DocumentID</param>
        /// <param name="tagId">TagID</param>
        protected virtual DocumentTagInfo GetDocumentTagInfoInternal(int documentId, int tagId)
        {
            WhereCondition condition = new WhereCondition().WhereEquals("DocumentID", documentId)
                                                           .WhereEquals("TagID", tagId);

            return GetObjectQuery().TopN(1).Where(condition).FirstOrDefault();
        }


        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(DocumentTagInfo info)
        {
            if (info != null)
            {
                // Check IDs
                if ((info.DocumentID <= 0) || (info.TagID <= 0))
                {
                    throw new ArgumentException("Object IDs not set.");
                }

                base.SetInfo(info);
            }
        }


        /// <summary>
        /// Returns <see cref="IWhereCondition"/> object that represents a condition which restricts page selection by single Tag.
        /// </summary>
        /// <param name="tagName">Tag name.</param>
        /// <param name="tagGroupName">Tag Group name.</param>
        internal static IWhereCondition GetDocumentTagWhereCondition(string tagName, string tagGroupName)
        {
            if (string.IsNullOrEmpty(tagName))
            {
                return null;
            }

            ObjectQuery<TagInfo> tagIdentifiers = null;

            if (!string.IsNullOrEmpty(tagGroupName))
            {
                // Filter by a tag in a specific tag group
                tagIdentifiers = TagInfoProvider.GetTags()
                    .Source(s =>
                        s.Join<TagGroupInfo>("TagGroupID", "TagGroupID")
                    )
                    .Columns("TagID")
                    .WhereEquals("TagName", tagName)
                    .WhereEquals("TagGroupName", tagGroupName);
            }
            else
            {
                // Filter by a tag in any tag group
                tagIdentifiers = TagInfoProvider.GetTags()
                    .Columns("TagID")
                    .WhereEquals("TagName", tagName);
            }

            var documentIdentifiers = GetDocumentTags()
                .WhereIn("TagID", tagIdentifiers)
                .Column("DocumentID");

            return new WhereCondition().WhereIn("DocumentID", documentIdentifiers);
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Removes all the tags from the specified document.
        /// </summary>        
        /// <param name="documentId">ID of the document tags are removed from</param> 
        /// <param name="deleteNotUsedTags">True (default) - All tags which are not assigned to any other document are deleted, False -  Tags always stay in the system even they are not assigned to any other document. </param>
        protected virtual void RemoveTagsInternal(int documentId, bool deleteNotUsedTags)
        {
            // Get all tags related to specified document
            var tagsToDecreaseCount = TagInfoProvider.GetTags()
                                                     .WhereIn("TagID", GetDocumentTags().WhereEquals("DocumentID", documentId)
                                                                                        .Column("TagID"));

            using (var transaction = BeginTransaction())
            {
                // Decrease tags count
                foreach (var tag in tagsToDecreaseCount)
                {
                    tag.TagCount = tag.TagCount - 1;

                    TagInfoProvider.SetTagInfo(tag);
                }

                // Delete binding between tags and document
                BulkDelete(new WhereCondition().WhereEquals("DocumentID", documentId));

                // Delete tags with zero count
                if (deleteNotUsedTags)
                {
                    TagInfoProvider.DeleteNotUsedTags();
                }

                transaction.Commit();
            }
        }


        /// <summary>
        /// Removes a specified tags from specified group and specified document.
        /// </summary>
        /// <param name="groupId">ID of the group holding tags to remove</param>
        /// <param name="documentId">ID of the document tags are removed from</param>
        /// <param name="tags">Set of tags (separated by commas) to remove</param>
        protected virtual void RemoveTagsInternal(int groupId, int documentId, string tags)
        {
            // If all the necessary parameters were supplied
            if ((groupId > 0) && (documentId > 0) && (tags != null) && (tags.Trim() != ""))
            {
                // Get tags as an array
                var tagList = GetDistinctTags(tags);

                // Get specified tags in given group
                IEnumerable<TagInfo> existingTags = TagInfoProvider.GetTags()
                                                                   .WhereIn("TagName", tagList)
                                                                   .WhereEquals("TagGroupID", groupId)
                                                                   .TypedResult;

                BulkDelete(
                    new WhereCondition()
                        .WhereEquals("DocumentID", documentId)
                        .WhereIn(
                            "TagID", 
                            existingTags.Select(tag => tag.TagID).ToList())
                         );

                // Go through the removed tags and update count
                foreach (TagInfo tagInfo in existingTags)
                {
                    // Get info on the current tag
                    tagInfo.TagCount -= 1;

                    // If the tag is still used
                    if (tagInfo.TagCount > 0)
                    {
                        // Update the count                                                                                    
                        TagInfoProvider.SetTagInfo(tagInfo);
                    }
                    else
                    {
                        // Remove the tag from the system
                        TagInfoProvider.DeleteTagInfo(tagInfo);
                    }
                }
            }
        }


        /// <summary>
        /// Inserts a specified tags of the specified group to the specified document.
        /// </summary>
        /// <param name="groupId">ID of the group inserted tags belongs to</param>
        /// <param name="documentId">ID of the document tags are inserted to</param>
        /// <param name="tags">Set of tags (separated by commas) to insert</param>
        protected virtual void AddTagsInternal(int groupId, int documentId, string tags)
        {
            // If all the necessary parameters were supplied
            if ((groupId > 0) && (documentId > 0) && !string.IsNullOrWhiteSpace(tags))
            {
                // Get list of tags
                var tagList = GetDistinctTags(tags);

                foreach (var tag in tagList)
                {
                    // Get tag
                    var tagInfo = TagInfoProvider.GetTags()
                                                 .WhereEquals("TagName", tag)
                                                 .WhereEquals("TagGroupID", groupId).FirstObject;

                    // Create tag info if does not exists
                    tagInfo = tagInfo ?? new TagInfo
                    {
                        TagName = tag,
                        TagCount = 0,
                        TagGroupID = groupId
                    };

                    // If tag in given document already exists, then the binding is returned
                    var docTag = GetDocumentTagInfo(documentId, tagInfo.TagID);

                    // If binding does not exists, create new binding and increment count in existing tag
                    if (docTag == null)
                    {
                        tagInfo.TagCount += 1;
                        TagInfoProvider.SetTagInfo(tagInfo);

                        SetDocumentTagInfo(new DocumentTagInfo
                        {
                            DocumentID = documentId,
                            TagID = tagInfo.TagID
                        });
                    }
                }
            }
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Gets list of tags filtering the multiple occurrence of particular tags.
        /// </summary>
        /// <param name="tags">Tags (separated by commas)</param>        
        private ISet<string> GetDistinctTags(string tags)
        {
            var result = new HashSet<string>();

            // Get tags
            var m = TagsRegex.Match(tags);
            var group = m.Groups[1];

            // Store the captures
            foreach (Capture capture in group.Captures)
            {
                string tag = capture.ToString().Trim('"').Trim();

                result.Add(tag);
            }

            return result;
        }


        #endregion
    }
}