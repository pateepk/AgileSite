using CMS.DocumentEngine;
using CMS.Membership;
using CMS.SiteProvider;
using CMS.Search;
using CMS.DataEngine;

namespace APIExamples
{
    /// <summary>
    /// Holds page API examples.
    /// </summary>
    /// <pageTitle>Pages</pageTitle>
    internal class Pages
    {
        /// <summary>
        /// Holds page creation API examples.
        /// </summary>
        /// <groupHeading>Page creation</groupHeading>
        private class PageCreation
        {
            /// <heading>Creating pages in a site's content tree</heading>
            private void CreatePage()
            {
                // Creates a new instance of the Tree provider
                TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

                // Gets the current site's root "/" page, which will serve as the parent page
                TreeNode parentPage = tree.SelectNodes()
                    .Path("/")
                    .OnCurrentSite()
                    .Culture("en-us")
                    .FirstObject;

                if (parentPage != null)
                {
                    // Creates a new page of the "CMS.MenuItem" page type
                    TreeNode newPage = TreeNode.New(SystemDocumentTypes.MenuItem, tree);

                    // Sets the properties of the new page
                    newPage.DocumentName = "Articles";
                    newPage.DocumentCulture = "en-us";

                    // Inserts the new page as a child of the parent page
                    newPage.Insert(parentPage);
                }
            }


            /// <heading>Creating a new culture version of an existing page</heading>
            private void CreateNewCultureVersion()
            {
                // Creates a new instance of the Tree provider
                TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

                // Gets an existing page in the "en-us" culture
                TreeNode page = tree.SelectNodes()
                    .Path("/Articles/Coffee-Beverages-Explained")
                    .OnCurrentSite()
                    .Culture("en-us")
                    .FirstObject;

                if (page != null)
                {
                    // Sets new values in the "DocumentName" and "ArticleTitle" fields of the page
                    page.DocumentName = "Translated article name";
                    page.SetValue("ArticleTitle", "Translated article title");

                    // Inserts a new version of the page in the "de-de" culture
                    // Note that the culture must be assigned to the current site
                    page.InsertAsNewCultureVersion("de-de");
                }
            }


            /// <heading>Creating a linked page</heading>
            private void CreateLinkedPage()
            {
                // Creates an instance of the Tree provider
                TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

                // Gets the page that will be linked
                TreeNode originalPage = tree.SelectNodes()
                    .Path("/Archive/Articles")
                    .OnCurrentSite()
                    .Culture("en-us")
                    .FirstObject;

                // Gets the page that will be used as a parent page for the linked page
                TreeNode parentPage = tree.SelectNodes()
                    .Path("/Articles")
                    .OnCurrentSite()
                    .Culture("en-us")
                    .FirstObject;

                if ((originalPage != null) && (parentPage != null))
                {
                    // Inserts a new linked page under the parent page
                    originalPage.InsertAsLink(parentPage);                
                }
            }
        }


        /// <summary>
        /// Holds page management API examples.
        /// </summary>
        /// <groupHeading>Page management</groupHeading>
        private class PageManagament
        {
            /// <heading>Updating published pages</heading>
            private void UpdatingPublishedPages()
            {
                // Creates an instance of the Tree provider
                TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

                // Gets the published version of pages stored under the "/Articles/" path
                // The pages are retrieved from the Dancing Goat site and in the "en-us" culture
                var pages = tree.SelectNodes()
                    .Path("/Articles/", PathTypeEnum.Children)
                    .WhereStartsWith("DocumentName", "Coffee")
                    .OnSite("DancingGoat")
                    .Culture("en-us");

                // Updates the "DocumentName" and "ArticleTitle" fields of each retrieved page
                foreach (TreeNode page in pages)
                {
                    page.DocumentName = "Updated article name";
                    page.SetValue("ArticleTitle", "Updated article title");

                    // Updates the page in the database
                    page.Update();
                }
            }


            /// <heading>Updating the latest edited version of pages (pages under workflow)</heading>
            private void UpdatingEditedPages()
            {
                // Gets the latest edited version of article pages stored under the '/Articles/' path
                // The pages are retrieved from the Dancing Goat site and in the "en-us" culture                
                var pages = DocumentHelper.GetDocuments()
                    .Types("DancingGoat.Article")
                    .Path("/Articles/", PathTypeEnum.Children)
                    .WhereStartsWith("DocumentName", "Coffee")
                    .OnSite("DancingGoat")
                    .Culture("en-us")
                    .WithCoupledColumns();

                // Note: When updating pages under workflow or versioning, always retrieve pages with all columns to avoid data loss.
                // Either call the 'GetDocuments' method with a class name parameter for a specific page type,
                // or specify the page types using the 'Types' method and then call 'WithCoupledColumns'.

                // Updates the "DocumentName" and "ArticleTitle" fields of each retrieved page
                foreach (TreeNode page in pages)
                {
                    // Allows the system to create a new version of the updated page (required even when not using content locking)
                    // If using content locking, checks out the page
                    page.CheckOut();

                    page.DocumentName = "Updated article name";
                    page.SetValue("ArticleTitle", "Updated article title");

                    // Updates the page in the database
                    page.Update();

                    // Creates a new version of the updated page (required even when not using content locking)
                    // If using content locking, checks in the page
                    page.CheckIn();
                }
            }


            /// <heading>Updating a page and related search indexes</heading>
            private void UpdatingPagesAndUpdatingSearchTasks()
            {
                // Creates a new instance of the Tree provider
                TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

                // Gets an existing page in the "en-us" culture
                TreeNode page = tree.SelectNodes()
                    .Path("/Articles/Coffee-Beverages-Explained")
                    .OnCurrentSite()
                    .Culture("en-us")
                    .FirstObject;


                if (page != null)
                {
                    // Updates the "DocumentName" and "ArticleTitle" fields of the page
                    page.DocumentName = "Updated article name";
                    page.SetValue("ArticleTitle", "Updated article name");

                    // Gets a List of the fields that were updated
                    var changedColumns = page.ChangedColumns();

                    page.Update();

                    // Check if search is enabled for the page and that the modified fields are included in search indexes
                    if (DocumentHelper.IsSearchTaskCreationAllowed(page) && DocumentHelper.SearchFieldChanged(page, changedColumns))
                    {
                        // Creates search tasks for updating the content of the page in related search indexes
                        SearchTaskInfoProvider.CreateTask(SearchTaskTypeEnum.Update, TreeNode.OBJECT_TYPE, SearchFieldsConstants.ID, page.GetSearchID(), page.DocumentID);
                    }
                }
            }


            /// <heading>Copying a page</heading>
            private void CopyingPages()
            {
                // Creates an instance of the Tree provider
                TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

                // Gets the page that will be copied
                TreeNode page = tree.SelectNodes()
                    .Path("/Articles/Coffee-Beverages-Explained")
                    .OnCurrentSite()
                    .Culture("en-us")
                    .FirstObject;

                // Gets the parent page under which the copy will be created
                TreeNode targetPage = tree.SelectNodes()
                    .Path("/Archive")
                    .OnCurrentSite()
                    .Culture("en-us")
                    .FirstObject;

                if ((page != null) && (targetPage != null))
                {
                    // Copies the page to the new location, including any child pages
                    DocumentHelper.CopyDocument(page, targetPage, true, tree);
                }
            }


            /// <heading>Moving a page</heading>
            private void MovingPages()
            {
                // Creates an instance of the Tree provider
                TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

                // Gets the page that will be moved
                TreeNode page = tree.SelectNodes()
                    .Path("/Articles/Coffee-Beverages-Explained")
                    .OnCurrentSite()
                    .Culture("en-us")
                    .FirstObject;

                // Gets the new parent page under which the page will be moved
                TreeNode targetPage = tree.SelectNodes()
                    .Path("/Archive")
                    .OnCurrentSite()
                    .Culture("en-us")
                    .FirstObject;

                if ((page != null) && (targetPage != null))
                {
                    // Moves the page to the new location, including any child pages
                    DocumentHelper.MoveDocument(page, targetPage, tree, true);
                }
            }


            /// <heading>Changing the order of individual pages in the content tree</heading>
            private void MovingPagesUpAndDown()
            {
                // Creates an instance of the Tree provider
                TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

                // Gets the page
                TreeNode page = tree.SelectNodes()
                    .Path("/Articles")
                    .OnCurrentSite()
                    .Culture("en-us")
                    .FirstObject;

                if (page != null)
                {
                    // Moves the page up in the content tree
                    tree.MoveNodeUp(page.DocumentID);

                    // Moves the page down in the content tree
                    tree.MoveNodeDown(page.DocumentID);
                }
            }


            /// <heading>Sorting pages in the content tree</heading>
            private void SortingPages()
            {
                // Creates an instance of the Tree provider
                TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

                // Gets the parent page
                TreeNode page = tree.SelectNodes()
                    .Path("/Articles")
                    .OnCurrentSite()
                    .Culture("en-us")
                    .FirstObject;

                if (page != null)
                {
                    bool orderIsAscending = true;

                    // Sorts all child pages under the specified parent page alphabetically in ascending order                    
                    tree.SortNodesAlphabetically(page.NodeID, SiteContext.CurrentSiteID, orderIsAscending);

                    // Sorts all child pages under the specified parent page ascendingly based on the last modification date
                    tree.SortNodesByDate(page.NodeID, SiteContext.CurrentSiteID, orderIsAscending);
                }
            }
        }


        /// <summary>
        /// Holds page deletion API examples.
        /// </summary>
        /// <groupHeading>Page deletion</groupHeading>
        private class PageDeletion
        {
            /// <heading>Deleting a page in a single culture</heading>
            private void DeletingPages()
            {
                // Creates an instance of the Tree provider
                TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

                // Gets the culture version of the page that will be deleted
                TreeNode page = tree.SelectNodes()
                    .Path("/Articles/On-Roasts")
                    .OnCurrentSite()
                    .Culture("en-us")
                    .FirstObject;


                if (page != null)
                {
                    // Deletes the page and moves it to the recycle bin (only the specified culture version)
                    page.Delete();

                    // Creates search tasks that remove the deleted page from the content of related search indexes
                    if (SearchIndexInfoProvider.SearchEnabled)
                    {
                        SearchTaskInfoProvider.CreateTask(SearchTaskTypeEnum.Delete, TreeNode.OBJECT_TYPE, SearchFieldsConstants.ID, page.GetSearchID(), page.DocumentID);
                    }
                }
            }


            /// <heading>Deleting all culture versions of a page</heading>
            private void DeletingPagesInAllCultures()
            {
                // Creates an instance of the Tree provider
                TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

                // Gets the page that will be deleted
                TreeNode page = tree.SelectNodes()
                    .Path("/Articles/On-Roasts")
                    .OnCurrentSite()
                    .Culture("en-us")
                    .FirstObject;

                if (page != null)
                {
                    // Deletes all culture versions of the page to the recycle bin
                    page.DeleteAllCultures();

                    // Creates search tasks that remove the deleted page from the content of related search indexes
                    if (SearchIndexInfoProvider.SearchEnabled)
                    {
                        SearchTaskInfoProvider.CreateTask(SearchTaskTypeEnum.Delete, TreeNode.OBJECT_TYPE, SearchFieldsConstants.ID, page.GetSearchID(), page.DocumentID);
                    }
                }
            }


            /// <heading>Restoring pages from the recycle bin</heading>
            private void RestorePages()
            {
                // Gets the "/Articles/On-Roasts" page from the recycle bin for the current site
                VersionHistoryInfo pageVersion = VersionHistoryInfoProvider.GetVersionHistories()
                                                                                .WhereEquals("VersionNodeAliasPath", "/Articles/On-Roasts")
                                                                                .WhereEquals("NodeSiteID", SiteContext.CurrentSiteID)
                                                                                .FirstObject;

                // Checks that the deleted page exists in the recycle bin
                if (pageVersion != null)
                {
                    // Creates a new version manager instance and restores the deleted page from the recycle bin
                    VersionManager manager = VersionManager.GetInstance(new TreeProvider(MembershipContext.AuthenticatedUser));
                    manager.RestoreDocument(pageVersion.VersionHistoryID);
                }
            }


            /// <heading>Permanently deleting pages</heading>
            private void DestroyPages()
            {
                // Creates an instance of the Tree provider
                TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

                // Gets the culture version of the page that will be deleted
                TreeNode page = tree.SelectNodes()
                    .Path("/Articles/On-Roasts")
                    .OnCurrentSite()
                    .Culture("en-us")
                    .FirstObject;

                if (page != null)
                {
                    bool deleteAllCultureVersions = true;
                    bool deletePermanently = true;

                    // Permanently deletes all culture versions of the page and its version history
                    page.Delete(deleteAllCultureVersions, deletePermanently);

                    // Creates search tasks that remove the deleted page from the content of related search indexes
                    if (SearchIndexInfoProvider.SearchEnabled)
                    {
                        SearchTaskInfoProvider.CreateTask(SearchTaskTypeEnum.Delete, TreeNode.OBJECT_TYPE, SearchFieldsConstants.ID, page.GetSearchID(), page.DocumentID);
                    }
                }
            }
        }


        /// <summary>
        /// Holds API examples for working with page aliases.
        /// </summary>
        /// <groupHeading>Page aliases</groupHeading>
        private class PageAliases
        {
            /// <heading>Creating a page alias</heading>
            private void CreatingPageAlias()
            {
                // Creates an instance of the Tree provider
                TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

                // Gets the "Articles" page on the current site
                TreeNode page = tree.SelectNodes()
                    .Path("/Articles")
                    .OnCurrentSite()
                    .Culture("en-us")
                    .FirstObject;

                if (page != null)
                {
                    // Creates a new page alias object
                    DocumentAliasInfo newAlias = new DocumentAliasInfo();

                    // Sets the properties of the new alias
                    newAlias.AliasURLPath = "/News";
                    newAlias.AliasNodeID = page.NodeID;
                    newAlias.AliasSiteID = SiteContext.CurrentSiteID;

                    // Saves the page alias to the database
                    DocumentAliasInfoProvider.SetDocumentAliasInfo(newAlias);
                }
            }


            /// <heading>Updating page aliases</heading>
            private void UpdatingPageAlias()
            {
                // Creates an instance of the Tree provider
                TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

                // Gets the "Articles" page on the current site
                TreeNode page = tree.SelectNodes()
                    .Path("/Articles")
                    .OnCurrentSite()
                    .Culture("en-us")
                    .FirstObject;

                // Gets all aliases of the specified page
                var aliases = DocumentAliasInfoProvider.GetDocumentAliases()
                                                            .WhereEquals("AliasNodeID", page.NodeID)
                                                            .WhereEquals("AliasSiteID", SiteContext.CurrentSiteID);

                // Loops through the page's aliases
                foreach (DocumentAliasInfo alias in aliases)
                {
                    // Updates the "AliasExtensions" of the alias to include only the ".html" extension
                    alias.AliasExtensions = ".html";

                    // Saves the updated page alias to the database
                    DocumentAliasInfoProvider.SetDocumentAliasInfo(alias);
                }   
            }


            /// <heading>Deleting page aliases</heading>
            private void DeletePageAlias()
            {
                // Gets all aliases of the '/Articles' page on the current site
                var aliases = DocumentAliasInfoProvider.GetDocumentAliases()
                                                            .WhereEquals("AliasURLPath", "/Articles")
                                                            .WhereEquals("AliasSiteID", SiteContext.CurrentSiteID);

                // Loops through the aliases
                foreach (DocumentAliasInfo deleteAlias in aliases)
                {
                    // Deletes the page alias
                    DocumentAliasInfoProvider.DeleteDocumentAliasInfo(deleteAlias);
                }
            }
        }
    }
}