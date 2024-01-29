using System;

using CMS.Taxonomy;
using CMS.DocumentEngine;
using CMS.SiteProvider;
using CMS.Membership;

namespace APIExamples
{
    /// <summary>
    /// Holds API examples related to page tags.
    /// </summary>
    /// <pageTitle>Tags</pageTitle>
    internal class Tags
    {
        /// <summary>
        /// Holds tag group API examples.
        /// </summary>
        /// <groupHeading>Tag groups</groupHeading>
        private class TagGroups
        {
            /// <heading>Creating a tag group</heading>
            private void CreateTagGroup()
            {
                // Creates a new tag group object
                TagGroupInfo newGroup = new TagGroupInfo();

                // Sets the tag group properties
                newGroup.TagGroupDisplayName = "New tag group";
                newGroup.TagGroupName = "NewTagGroup";
                newGroup.TagGroupDescription = "This tag group was created through the API.";
                newGroup.TagGroupSiteID = SiteContext.CurrentSiteID;
                newGroup.TagGroupIsAdHoc = false;

                // Saves the new tag group to the database
                TagGroupInfoProvider.SetTagGroupInfo(newGroup);
            }


            /// <heading>Updating a tag group</heading>
            private void GetAndUpdateTagGroup()
            {
                // Gets the tag group
                TagGroupInfo updateGroup = TagGroupInfoProvider.GetTagGroupInfo("NewTagGroup", SiteContext.CurrentSiteID);

                if (updateGroup != null)
                {
                    // Updates the tag group properties
                    updateGroup.TagGroupDisplayName = updateGroup.TagGroupDisplayName.ToLower();

                    // Saves the updated tag group to the database
                    TagGroupInfoProvider.SetTagGroupInfo(updateGroup);
                }
            }


            /// <heading>Updating multiple tag groups</heading>
            private void GetAndBulkUpdateTagGroups()
            {
                // Gets all tag groups defined for the current site whose name starts with 'New'
                var tagGroups = TagGroupInfoProvider.GetTagGroups()
                                                    .WhereEquals("TagGroupSiteID", SiteContext.CurrentSiteID)
                                                    .WhereStartsWith("TagGroupName", "New");
                
                // Loops through individual tag groups
                foreach (TagGroupInfo tagGroup in tagGroups)
                {
                    // Updates the tag group properties
                    tagGroup.TagGroupDisplayName = tagGroup.TagGroupDisplayName.ToUpper();

                    // Saves the updated tag group to the database
                    TagGroupInfoProvider.SetTagGroupInfo(tagGroup);
                }
            }


            /// <heading>Deleting a tag group</heading>
            private void DeleteTagGroup()
            {
                // Gets the tag group
                TagGroupInfo deleteGroup = TagGroupInfoProvider.GetTagGroupInfo("NewTagGroup", SiteContext.CurrentSiteID);

                if (deleteGroup != null)
                {
                    // Deletes the tag group
                    TagGroupInfoProvider.DeleteTagGroupInfo(deleteGroup);
                }
            }
        }


        /// <summary>
        /// Holds page tag API examples.
        /// </summary>
        /// <groupHeading>Page tags</groupHeading>
        private class PageTags
        {
            /// <heading>Adding tags to a page</heading>
            private void AddTagToDocument()
            {
                // Prepares a TreeProvider instance
                TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

                // Gets the "/Example" page on the current site
                TreeNode page = tree.SelectNodes()
                    .Path("/Example")
                    .OnCurrentSite()
                    .Culture("en-us")
                    .FirstObject;

                // Gets a tag group
                TagGroupInfo tagGroup = TagGroupInfoProvider.GetTagGroupInfo("NewTagGroup", SiteContext.CurrentSiteID);

                if ((page != null) && (tagGroup != null))
                {
                    // Assigns the tag group to the page
                    page.DocumentTagGroupID = tagGroup.TagGroupID;

                    // Adds the "API test" and "Examples" tags to the page
                    page.DocumentTags = "\"API test\", Examples";

                    // Updates the page in the database
                    page.Update();
                }
            }


            /// <heading>Removing tags from a page</heading>
            private void RemoveTagFromDocument()
            {
                // Prepares a TreeProvider instance
                TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

                // Gets the "/Example" page on the current site
                TreeNode page = tree.SelectNodes()
                    .Path("/Example")
                    .OnCurrentSite()
                    .Culture("en-us")
                    .FirstObject;

                if (page != null)
                {
                    // Checks that the page has tag content
                    if (!String.IsNullOrEmpty(page.DocumentTags))
                    {
                        // Removes all tags from the page
                        page.DocumentTags = "";

                        // Updates the page in the database
                        page.Update();
                    }
                }
            }
        }
    }
}
