using CMS.DocumentEngine;
using CMS.Membership;
using CMS.SiteProvider;
using CMS.WorkflowEngine;

namespace APIExamples
{
    /// <summary>
    /// Holds workflow and versioning API examples.
    /// </summary>
    /// <pageTitle>Page workflow and versioning</pageTitle>
    internal class PageWorkflow
    {
        /// <summary>
        /// Holds content locking examples.
        /// </summary>
        /// <groupHeading>Content locking</groupHeading>
        private class ContentLocking
        {
            /// <heading>Checking out a page</heading>
            private void CheckOut()
            {
                // Gets a page named 'On Roasts'
                TreeNode page = DocumentHelper.GetDocuments()
                    .WhereEquals("DocumentName", "On Roasts")
                    .OnSite("DancingGoat")
                    .Culture("en-us")
                    .FirstObject;

                    // Checks if the page isn't already checked-out
                    if (!page.IsCheckedOut)
                    {
                        // Checks out the page
                        page.CheckOut();
                    }
            }


            /// <heading>Checking in a page</heading>
            private void CheckIn()
            {
                // Gets a page named 'On Roasts'
                TreeNode page = DocumentHelper.GetDocuments()
                    .WhereEquals("DocumentName", "On Roasts")
                    .OnSite("DancingGoat")
                    .Culture("en-us")
                    .FirstObject;

                // Checks if the page is checked-out
                if (page.IsCheckedOut)
                {
                    // Checks in the page
                    page.CheckIn();
                }
            }

            /// <heading>Undoing a page checkout</heading>
            private void UndoCheckout()
            {
                // Gets a page named 'On Roasts'
                TreeNode page = DocumentHelper.GetDocuments()
                    .WhereEquals("DocumentName", "On Roasts")
                    .OnSite("DancingGoat")
                    .Culture("en-us")
                    .FirstObject;

                // Checks if the page is checked-out
                if (page.IsCheckedOut)
                {
                    // Undoes the checkout
                    page.UndoCheckOut();
                }
            }


            /// <heading>Updating a page with the use of content locking</heading>
            private void UpdatePageUnderWorkflow()
            {
                // Gets an article page named 'On Roasts'
                TreeNode page = DocumentHelper.GetDocuments("DancingGoat.Article")
                    .WhereEquals("DocumentName", "On Roasts")
                    .OnSite("DancingGoat")
                    .Culture("en-us")                    
                    .FirstObject;

                // Note: When updating pages under workflow or versioning, always retrieve pages with all columns to avoid data loss.
                // Either call the 'GetDocuments' method with a class name parameter for a specific page type,
                // or specify the page types using the 'Types' method and then call 'WithCoupledColumns'.

                // Checks whether the page is already checked-out
                if (!page.IsCheckedOut)
                {
                    // Checks out the page
                    page.CheckOut();

                    // Sets new values for the 'DocumentName' and 'ArticleTitle' fields
                    page.DocumentName = "Updated article name";
                    page.SetValue("ArticleTitle", "Updated article title");

                    // Updates the page in the database
                    page.Update();

                    // Checks in the page
                    page.CheckIn();
                }
            }


            /// <heading>Updating multiple pages with the use of content locking</heading>
            private void GetAndUpdatePagesUnderWorkflow()
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
                    // Checks whether the page is already checked-out
                    if (!page.IsCheckedOut)
                    {
                        // Checks out the page
                        page.CheckOut();

                        page.DocumentName = "Updated article name";
                        page.SetValue("ArticleTitle", "Updated article title");

                        // Updates the page in the database
                        page.Update();

                        // Checks in the page
                        page.CheckIn();
                    }                  
                }
            }
        }        


        /// <summary>
        /// Holds examples for managing pages in a workflow.
        /// </summary>
        /// <groupHeading>Page workflow management</groupHeading>
        private class WorkflowStepManagement
        {
            /// <heading>Moving a page to a different step</heading>
            private void MoveToStep()
            {
                // Creates a new instance of the Tree provider
                TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

                // Gets a page named 'On Roasts'
                TreeNode page = DocumentHelper.GetDocuments()
                    .WhereEquals("DocumentName", "On Roasts")
                    .OnSite("DancingGoat")
                    .Culture("en-us")
                    .FirstObject;

                // Gets the page's workflow
                WorkflowManager workflowManager = WorkflowManager.GetInstance(tree);
                WorkflowInfo workflow = workflowManager.GetNodeWorkflow(page);

                // Checks if the page uses workflow
                if (workflow != null)
                {
                    // Checks if the page doesn't use automatic publishing. If it is, the page cannot change workflow steps.
                    if (!workflow.WorkflowAutoPublishChanges)
                    {
                        // Checks if the current user can move the page to the next step.
                        if (workflowManager.CheckStepPermissions(page, WorkflowActionEnum.Approve))
                        {
                            // Based on current workflow step, the page is sent for approval, approved, or published
                            page.MoveToNextStep();
                            // If the page is published, rejects the page
                            page.MoveToPreviousStep();
                            // Moves the page to the first workflow step
                            page.MoveToFirstStep();
                        }
                    }
                }
            }


            /// <heading>Moving a page to the Published step of the workflow</heading>
            private void Publish()
            {
                // Creates a new instance of the Tree provider
                TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

                // Gets a page named 'On Roasts'
                TreeNode page = DocumentHelper.GetDocuments()
                    .WhereEquals("DocumentName", "On Roasts")
                    .OnSite("DancingGoat")
                    .Culture("en-us")
                    .FirstObject;


                // Gets the page's workflow
                WorkflowManager workflowManager = WorkflowManager.GetInstance(tree);
                WorkflowInfo workflow = workflowManager.GetNodeWorkflow(page);

                // Checks if the page uses workflow
                if (workflow != null)
                {
                    // Publishes the page with a comment. There needs to be only one workflow path to the Published step.
                    page.Publish("Review finished, publishing page.");
                }
            }


            /// <heading>Moving a page to the Archived step of the workflow</heading>
            private void Archive()
            {
                // Creates a new instance of the Tree provider
                TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

                // Gets a page named 'On Roasts'
                TreeNode page = DocumentHelper.GetDocuments()
                    .WhereEquals("DocumentName", "On Roasts")
                    .OnSite("DancingGoat")
                    .Culture("en-us")
                    .FirstObject;


                // Gets the page's workflow
                WorkflowManager workflowManager = WorkflowManager.GetInstance(tree);
                WorkflowInfo workflow = workflowManager.GetNodeWorkflow(page);

                // Checks if the page uses workflow
                if (workflow != null)
                {
                    // Archives the page with a comment
                    page.Archive("The page is obsolete, archiving.");
                }
            }
        }

       
        /// <summary>
        /// Holds page versioning API examples.
        /// </summary>
        /// <groupHeading>Page versioning</groupHeading>
        private class PageVersioning
        {
            /// <heading>Rolling back to the oldest version of a page</heading>
            private void RollbackVersion()
            {
                TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

                // Gets a page named 'On Roasts'
                var page = DocumentHelper.GetDocuments()
                    .WhereEquals("DocumentName", "On Roasts")
                    .OnSite("DancingGoat")
                    .Culture("en-us")
                    .FirstObject;

                var versionHistory = VersionHistoryInfoProvider.GetVersionHistories()
                    .WhereEquals("DocumentID", page.DocumentID)
                    .Columns("VersionHistoryID")
                    .OrderByAscending("VersionHistoryID") // Makes sure the oldest version of the page is on the top of the results
                    .Result; // Returns the data in the form of a DataSet

                if (versionHistory != null)
                {
                    // Creates a new version history object
                    VersionHistoryInfo version = new VersionHistoryInfo(versionHistory.Tables[0].Rows[0]);

                    VersionManager versionManager = VersionManager.GetInstance(tree);

                    // Rolls back the specific page version
                    page.VersionManager.RollbackVersion(version.VersionHistoryID);
                }
            }


            /// <heading>Deleting the newest version of a page</heading>
            private void DeleteVersion()
            {
                TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

                // Gets a page named 'On Roasts'
                var page = DocumentHelper.GetDocuments()
                    .WhereEquals("DocumentName", "On Roasts")
                    .OnSite("DancingGoat")
                    .Culture("en-us")
                    .FirstObject;

                var versionHistory = VersionHistoryInfoProvider.GetVersionHistories()
                    .WhereEquals("DocumentID", page.DocumentID)
                    .Columns("VersionHistoryID")
                    .OrderByDescending("VersionHistoryID") // Makes sure the latest version of the page is on the top of the results
                    .Result; // Returns the data in the form of a DataSet

                if (versionHistory != null)
                {
                    // Creates a new version history object
                    VersionHistoryInfo version = new VersionHistoryInfo(versionHistory.Tables[0].Rows[0]);

                    VersionManager versionManager = VersionManager.GetInstance(tree);

                    // Deletes the specific page version
                    page.VersionManager.DestroyDocumentVersion(version.VersionHistoryID);
                }
            }


            /// <heading>Destroying the version history of a specific page</heading>
            private void DestroyHistory()
            {
                TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

                // Gets a page named 'On Roasts'
                var page = DocumentHelper.GetDocuments()
                    .WhereEquals("DocumentName", "On Roasts")
                    .OnSite("DancingGoat")
                    .Culture("en-us")
                    .FirstObject;

                VersionManager versionManager = VersionManager.GetInstance(tree);

                // Destroys the page history
                versionManager.DestroyDocumentHistory(page.DocumentID);
            }
        }


        /// <summary>
        /// Versioning without workflow examples.
        /// </summary>
        /// <groupHeading>Versioning without workflow</groupHeading>
        private class VersioningWithoutWorkflow
        {
            /// <heading>Creating a versioned page without workflow</heading>
            private void CreatePageWithoutWorkflow()
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

                    // Manually publishes the page as the page is not using content locking
                    newPage.MoveToPublishedStep();
                }
            }


            /// <heading>Updating a versioned page without workflow</heading>
            private void UpdatePageWithoutWorkflow()
            {
                // Gets an article page named 'On Roasts'
                TreeNode page = DocumentHelper.GetDocuments("DancingGoat.Article")
                    .WhereEquals("DocumentName", "On Roasts")
                    .OnSite("DancingGoat")
                    .Culture("en-us")
                    .FirstObject;

                // Note: When updating pages under workflow or versioning, always retrieve pages with all columns to avoid data loss.
                // Either call the 'GetDocuments' method with a class name parameter for a specific page type,
                // or specify the page types using the 'Types' method and then call 'WithCoupledColumns'.
                
                // Ensures that a new version of the updated page is created (relevant even when not using content locking)
                page.CheckOut();

                // Sets new value to the 'DocumentName' and 'ArticleTitle'
                page.DocumentName = "Updated article name";
                page.SetValue("ArticleTitle", "Updated article title");

                // Updates the page in the database
                page.Update();

                // Creates a new version of the updated page
                page.CheckIn();
            }
        }
    }
}