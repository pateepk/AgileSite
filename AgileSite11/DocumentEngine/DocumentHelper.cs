using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

using CMS.Base;
using CMS.DataEngine;
using CMS.DocumentEngine.Internal;
using CMS.EventLog;
using CMS.FormEngine;
using CMS.Helpers;
using CMS.IO;
using CMS.Membership;
using CMS.PortalEngine;
using CMS.Search;
using CMS.SiteProvider;
using CMS.Synchronization;


namespace CMS.DocumentEngine
{
    using TypedDataSet = InfoDataSet<TreeNode>;

    /// <summary>
    /// Class providing document management methods.
    /// </summary>
    public static class DocumentHelper
    {
        #region "Constants & variables"

        /// <summary>
        /// Log context name for document actions
        /// </summary>
        public const string LOGCONTEXT_DOCUMENTS = "Documents";


        /// <summary>
        /// Document prefix for object type.
        /// </summary>
        public const string DOCUMENT_PREFIX = TreeNode.OBJECT_TYPE + ".";

        #endregion


        #region "Documents management"

        /// <summary>
        /// Returns current document version. If versioning is used, gets the current VersionHistory record, otherwise gets the record directly from the database. Result contains coupled data only if classNames are specified.
        /// </summary>
        /// <param name="siteName">Nodes site name</param>
        /// <param name="aliasPath">Path. It may contain % and _ as wild card characters for any number of unknown characters or one unknown character respectively (for MS SQL).</param>
        /// <param name="cultureCode">Nodes culture code</param>
        /// <param name="combineWithDefaultCulture">Indicates whether node in default culture should be returned if node in specified culture was not found.</param>
        /// <param name="classNames">List of class names separated by semicolon (e.g.: "cms.article;cms.product")</param>
        /// <param name="where">Where condition for the data selection</param>
        /// <param name="orderBy">Order by clause for the data selection</param>
        /// <param name="maxRelativeLevel">Maximum child level of the selected nodes</param>
        /// <param name="selectOnlyPublished">Select only published nodes.</param>
        /// <param name="columns">Columns to be selected</param>
        /// <param name="tree">TreeProvider to use</param>
        public static TreeNode GetDocument(string siteName, string aliasPath, string cultureCode, bool combineWithDefaultCulture, string classNames, string where, string orderBy, int maxRelativeLevel, bool selectOnlyPublished, string columns, TreeProvider tree)
        {
            // Prepare the parameters
            var parameters = new NodeSelectionParameters
            {
                SiteName = siteName,
                AliasPath = aliasPath,
                CultureCode = cultureCode,
                CombineWithDefaultCulture = combineWithDefaultCulture,
                ClassNames = classNames,
                Where = where,
                OrderBy = orderBy,
                MaxRelativeLevel = maxRelativeLevel,
                SelectOnlyPublished = selectOnlyPublished,
                Columns = columns,
                SelectSingleNode = true
            };

            return GetDocument(parameters, tree);
        }


        /// <summary>
        /// Returns current document version. If versioning used, gets the current VersionHistory record, otherwise gets the record directly from the database.
        /// </summary>
        /// <param name="parameters">Parameters for the node selection</param>
        /// <param name="tree">TreeProvider to use</param>
        public static TreeNode GetDocument(NodeSelectionParameters parameters, TreeProvider tree)
        {
            // Ensure provider instance
            if (tree == null)
            {
                tree = new TreeProvider();
            }

            // Get the document
            var node = tree.SelectSingleNode(parameters);

            // Apply latest version
            ApplyLatestVersion(node, tree);

            return node;
        }


        /// <summary>
        /// Returns current document version. If versioning is used, gets the current VersionHistory record, otherwise gets the record directly from the database.
        /// </summary>
        /// <param name="documentId">Document ID to retrieve</param>
        /// <param name="tree">TreeProvider to use for the DB access</param>
        public static TreeNode GetDocument(int documentId, TreeProvider tree)
        {
            // Ensure provider instance
            if (tree == null)
            {
                tree = new TreeProvider();
            }

            // Get the document
            var node = tree.SelectSingleDocument(documentId);

            // Apply latest version
            ApplyLatestVersion(node, tree);

            return node;
        }


        /// <summary>
        /// Returns current document version. If versioning used, gets the current VersionHistory record, otherwise gets the record directly from the database.
        /// </summary>
        /// <param name="nodeId">Node ID to retrieve</param>
        /// <param name="cultureCode">Nodes culture code</param>
        /// <param name="tree">TreeProvider to use</param>
        public static TreeNode GetDocument(int nodeId, string cultureCode, TreeProvider tree)
        {
            return GetDocument(nodeId, cultureCode, false, tree);
        }


        /// <summary>
        /// Returns current document version. If versioning used, gets the current VersionHistory record, if not, gets the record directly from the database.
        /// </summary>
        /// <param name="nodeId">Node ID to retrieve</param>
        /// <param name="cultureCode">Nodes culture code</param>
        /// <param name="combineWithDefaultCulture">Indicates whether node in default culture should be returned if node in specified culture was not found.</param>
        /// <param name="tree">TreeProvider to use</param>
        public static TreeNode GetDocument(int nodeId, string cultureCode, bool combineWithDefaultCulture, TreeProvider tree)
        {
            // Ensure provider instance
            if (tree == null)
            {
                tree = new TreeProvider();
            }

            // Get the document
            var node = tree.SelectSingleNode(nodeId, cultureCode, combineWithDefaultCulture);

            // Apply latest version
            ApplyLatestVersion(node, tree);

            return node;
        }


        /// <summary>
        /// Returns the latest version of the document if versioning is used or the document itself otherwise.
        /// </summary>
        /// <param name="node">Document node</param>
        /// <param name="tree">TreeProvider to use</param>
        public static TreeNode GetDocument(TreeNode node, TreeProvider tree)
        {
            // Ensure provider instance
            if (tree == null)
            {
                tree = new TreeProvider();
            }

            // Apply latest version
            ApplyLatestVersion(node, tree);

            return node;
        }


        /// <summary>
        /// Returns latest version documents data in a dataset. Result contains coupled data only if classNames are specified.
        /// </summary>
        /// <param name="siteName">Nodes site name</param>
        /// <param name="aliasPath">Path. It may contain % and _ as wild card characters for any number of unknown characters or one unknown character respectively (for MS SQL).</param>
        /// <param name="cultureCode">Nodes culture code</param>
        /// <param name="combineWithDefaultCulture">Indicates whether node in default culture should be returned if node in specified culture was not found.</param>
        /// <param name="classNames">List of class names separated by semicolon (e.g.: "cms.article;cms.product")</param>
        /// <param name="where">Where condition for the data selection</param>
        /// <param name="orderBy">Order by clause for the data selection</param>
        /// <param name="maxRelativeLevel">Maximum child level of the selected nodes</param>
        /// <param name="selectOnlyPublished">Select only published nodes</param>
        /// <param name="tree">Tree provider to use</param>
        public static TypedDataSet GetDocuments(string siteName, string aliasPath, string cultureCode, bool combineWithDefaultCulture, string classNames, string where, string orderBy, int maxRelativeLevel, bool selectOnlyPublished, TreeProvider tree)
        {
            return GetDocuments(siteName, aliasPath, cultureCode, combineWithDefaultCulture, classNames, where, orderBy, maxRelativeLevel, selectOnlyPublished, 0, null, tree);
        }


        /// <summary>
        /// Returns latest version documents data in a dataset. Result contains coupled data only if classNames are specified.
        /// </summary>
        /// <param name="siteName">Nodes site name</param>
        /// <param name="aliasPath">Path. It may contain % and _ as wild card characters for any number of unknown characters or one unknown character respectively (for MS SQL).</param>
        /// <param name="cultureCode">Nodes culture code</param>
        /// <param name="combineWithDefaultCulture">Indicates whether node in default culture should be returned if node in specified culture was not found.</param>
        /// <param name="classNames">List of class names separated by semicolon (e.g.: "cms.article;cms.product")</param>
        /// <param name="where">Where condition for the data selection</param>
        /// <param name="orderBy">Order by clause for the data selection</param>
        /// <param name="maxRelativeLevel">Maximum child level of the selected nodes</param>
        /// <param name="selectOnlyPublished">Select only published nodes</param>
        /// <param name="topN">Limits the number of returned items.</param>
        /// <param name="tree">Tree provider to use</param>
        public static TypedDataSet GetDocuments(string siteName, string aliasPath, string cultureCode, bool combineWithDefaultCulture, string classNames, string where, string orderBy, int maxRelativeLevel, bool selectOnlyPublished, int topN, TreeProvider tree)
        {
            return GetDocuments(siteName, aliasPath, cultureCode, combineWithDefaultCulture, classNames, where, orderBy, maxRelativeLevel, selectOnlyPublished, topN, null, tree);
        }


        /// <summary>
        /// Returns latest version documents data in a dataset. Result contains coupled data only if classNames are specified.
        /// </summary>
        /// <param name="siteName">Nodes site name</param>
        /// <param name="aliasPath">Path. It may contain % and _ as wild card characters for any number of unknown characters or one unknown character respectively (for MS SQL)</param>
        /// <param name="cultureCode">Nodes culture code</param>
        /// <param name="combineWithDefaultCulture">Indicates whether node in default culture should be returned if node in specified culture was not found.</param>
        /// <param name="classNames">List of class names separated by semicolon (e.g.: "cms.article;cms.product")</param>
        /// <param name="where">Where condition for the data selection</param>
        /// <param name="orderBy">Order by clause for the data selection</param>
        /// <param name="maxRelativeLevel">Maximum child level of the selected nodes</param>
        /// <param name="selectOnlyPublished">Select only published nodes</param>
        /// <param name="topN">Limits the number of returned items.</param>
        /// <param name="tree">Tree provider to use</param>
        /// <param name="columns">Columns to be selected. Must contain mandatory columns (NodeID, NodeLinkedNodeID, DocumentCulture).</param>
        public static TypedDataSet GetDocuments(string siteName, string aliasPath, string cultureCode, bool combineWithDefaultCulture, string classNames, string where, string orderBy, int maxRelativeLevel, bool selectOnlyPublished, int topN, string columns, TreeProvider tree)
        {
            // Prepare the parameters
            var parameters = new NodeSelectionParameters
            {
                SiteName = siteName,
                AliasPath = aliasPath,
                CultureCode = cultureCode,
                CombineWithDefaultCulture = combineWithDefaultCulture,
                ClassNames = classNames,
                Where = where,
                OrderBy = orderBy,
                MaxRelativeLevel = maxRelativeLevel,
                SelectOnlyPublished = selectOnlyPublished,
                TopN = topN,
                Columns = columns
            };

            return GetDocuments(parameters, tree);
        }


        /// <summary>
        /// Returns latest version documents data in a dataset. Result contains coupled data only if classNames are specified.
        /// </summary>
        /// <param name="siteName">Nodes site name</param>
        /// <param name="aliasPath">Path. It may contain % and _ as wild card characters for any number of unknown characters or one unknown character respectively (for MS SQL).</param>
        /// <param name="cultureCode">Nodes culture code</param>
        /// <param name="combineWithDefaultCulture">Indicates whether node in default culture should be returned if node in specified culture was not found.</param>
        /// <param name="classNames">List of class names separated by semicolon (e.g.: "cms.article;cms.product")</param>
        /// <param name="where">Where condition for the data selection</param>
        /// <param name="orderBy">Order by clause for the data selection</param>
        /// <param name="maxRelativeLevel">Maximum child level of the selected nodes</param>
        /// <param name="selectOnlyPublished">Select only published nodes</param>
        /// <param name="relationshipWithNodeGuid">Select nodes that are related to node with this GUID.</param>
        /// <param name="relationshipName">Relationship name</param>
        /// <param name="relatedNodeIsOnTheLeftSide">Indicates whether the related node is located on the left side of the relationship.</param>
        /// <param name="topN">Limits the number of returned items.</param>
        /// <param name="columns">Columns to be selected.</param>
        /// <param name="tree">Tree provider to use</param>
        public static TypedDataSet GetDocuments(string siteName, string aliasPath, string cultureCode, bool combineWithDefaultCulture, string classNames, string where, string orderBy, int maxRelativeLevel, bool selectOnlyPublished, Guid relationshipWithNodeGuid, string relationshipName, bool relatedNodeIsOnTheLeftSide, int topN, string columns, TreeProvider tree)
        {
            // Prepare the parameters
            var parameters = new NodeSelectionParameters
            {
                SiteName = siteName,
                AliasPath = aliasPath,
                CultureCode = cultureCode,
                CombineWithDefaultCulture = combineWithDefaultCulture,
                ClassNames = classNames,
                Where = where,
                OrderBy = orderBy,
                MaxRelativeLevel = maxRelativeLevel,
                SelectOnlyPublished = selectOnlyPublished,
                TopN = topN,
                Columns = columns,
                RelationshipNodeGUID = relationshipWithNodeGuid,
                RelationshipName = relationshipName,
                RelationshipSide = relatedNodeIsOnTheLeftSide ? RelationshipSideEnum.Left : RelationshipSideEnum.Right
            };

            return GetDocuments(parameters, tree);
        }


        /// <summary>
        /// Returns latest version documents data in a dataset.
        /// </summary>
        /// <param name="parameters">Parameters for the node selection</param>
        /// <param name="tree">Tree provider to use</param>
        public static TypedDataSet GetDocuments(NodeSelectionParameters parameters, TreeProvider tree)
        {
            // Ensure provider instance
            if (tree == null)
            {
                tree = new TreeProvider();
            }

            // Ensure latest version of documents is selected
            parameters.SelectLatestVersion = true;

            // Get the documents data
            return tree.SelectNodes(parameters);
        }


        /// <summary>
        /// Returns latest version documents data in a dataset. Result contains coupled data only if classNames are specified.
        /// </summary>
        /// <param name="siteName">Nodes site name</param>
        /// <param name="aliasPath">Path. It may contain % and _ as wild card characters for any number of unknown characters or one unknown character respectively (for MS SQL).</param>
        /// <param name="cultureCode">Nodes culture code</param>
        /// <param name="combineWithDefaultCulture">Indicates whether node in default culture should be returned if node in specified culture was not found.</param>
        /// <param name="classNames">List of class names separated by semicolon (e.g.: "cms.article;cms.product")</param>
        /// <param name="where">Where condition for the data selection</param>
        /// <param name="orderBy">Order by clause for the data selection</param>
        /// <param name="maxRelativeLevel">Maximum child level of the selected nodes</param>
        /// <param name="selectOnlyPublished">Select only published nodes</param>
        /// <param name="relationshipWithNodeGuid">Select nodes that are related to node with this GUID.</param>
        /// <param name="relationshipName">Relationship name</param>
        /// <param name="relatedNodeIsOnTheLeftSide">Indicates whether the related node is located on the left side of the relationship.</param>
        /// <param name="tree">Tree provider to use</param>
        public static TypedDataSet GetDocuments(string siteName, string aliasPath, string cultureCode, bool combineWithDefaultCulture, string classNames, string where, string orderBy, int maxRelativeLevel, bool selectOnlyPublished, Guid relationshipWithNodeGuid, string relationshipName, bool relatedNodeIsOnTheLeftSide, TreeProvider tree)
        {
            return GetDocuments(siteName, aliasPath, cultureCode, combineWithDefaultCulture, classNames, where, orderBy, maxRelativeLevel, selectOnlyPublished, relationshipWithNodeGuid, relationshipName, relatedNodeIsOnTheLeftSide, 0, null, tree);
        }


        /// <summary>
        /// Gets the query for all documents (latest versions)
        /// </summary>
        public static MultiDocumentQuery GetDocuments()
        {
            return TreeNodeProvider.GetDocuments().LatestVersion();
        }


        /// <summary>
        /// Gets the query for all documents (latest versions) of specific type
        /// </summary>
        /// <param name="className">Class name representing document type</param>
        public static DocumentQuery GetDocuments(string className)
        {
            return TreeNodeProvider.GetDocuments(className).LatestVersion();
        }


        /// <summary>
        /// Gets the query for all documents (latest versions) of specific type
        /// </summary>
        /// <typeparam name="TDocument">Type of the instances returned by the query.</typeparam>
        public static DocumentQuery<TDocument> GetDocuments<TDocument>()
            where TDocument : TreeNode, new()
        {
            return TreeNodeProvider.GetDocuments<TDocument>().LatestVersion();
        }


        /// <summary>
        /// Updates the current version of the document within the database. If versioning is used, updates the last version within the VersionHistory, otherwise updates directly the database document record.
        /// </summary>
        /// <param name="node">Document to update</param>
        /// <param name="tree">TreeProvider to use</param>
        /// <param name="updateColumns">List of columns which should be updated explicitly (separated by ';')</param>
        public static void UpdateDocument(TreeNode node, TreeProvider tree = null, string updateColumns = null)
        {
            // Check parameters
            CheckParameters(node);

            // Ensure tree provider
            tree = EnsureTreeProvider(node, tree);

            // Get the node workflow
            var workflow = node.GetWorkflow();
            var usesWorkflow = workflow != null;

            // Document uses workflow
            if (usesWorkflow)
            {
                // Save the node version
                node.VersionManager.SaveVersion(node, null, null, updateColumns, workflow);
            }
            else
            {
                // Remove remaining workflow information (node does not use workflow any more)
                ClearWorkflowInformation(node);

                var changedColumns = node.ChangedColumns();

                // Do not process additional actions
                using (new DocumentActionContext { SendNotifications = false })
                {
                    // Update node
                    node.Update(false);
                    node.IsLastVersion = true;
                }

                // Log synchronization
                LogDocumentChange(node, TaskTypeEnum.UpdateDocument, tree);

                // Send notification
                node.SendNotifications("UPDATEDOC");

                // Update search index
                if (SearchFieldChanged(node, changedColumns))
                {
                    UpdateSearchIndexIfAllowed(node);
                }
            }

            // Reset the changes            
            node.ResetChanges();
        }


        /// <summary>
        /// Inserts a new document.
        /// </summary>
        /// <param name="node">Document node</param>
        /// <param name="parentNode">Parent node</param>
        /// <param name="tree">TreeProvider to use</param>
        /// <param name="allowCheckOut">If true, document check out is allowed on the insert action.</param>
        public static void InsertDocument(TreeNode node, TreeNode parentNode, TreeProvider tree = null, bool allowCheckOut = true)
        {
            // Check parameters
            CheckParameters(node, parentNode);

            // Ensure tree provider
            tree = EnsureTreeProvider(node, tree);

            // Do not process additional actions
            using (new DocumentActionContext { SendNotifications = false })
            {
                node.Insert(parentNode, false);
                node.IsLastVersion = true;
            }

            // Get the node workflow
            var workflow = node.GetWorkflow();
            var usesWorkflow = workflow != null;
            var isBlogPost = string.Equals(node.NodeClassName, "cms.blogpost", StringComparison.InvariantCultureIgnoreCase);

            // Do not process additional actions
            using (new DocumentActionContext { SendNotifications = false })
            {
                // If workflow defined, create a version and check out the document
                if (usesWorkflow)
                {
                    // Check out for the next editing if using Check In/Out as unpublished document
                    if (!tree.KeepCheckedInOnInsert && allowCheckOut && workflow.UseCheckInCheckOut(node.NodeSiteName))
                    {
                        node.VersionManager.CheckOut(node, false);
                    }

                    // Make sure document version is created
                    var step = node.DocumentWorkflowStepID > 0 ? node.WorkflowStep : null;
                    var published = (step != null) && step.StepIsPublished;
                    node.VersionManager.EnsureVersion(node, published);
                }
                else
                {
                    if (isBlogPost && node.PublishedVersionExists)
                    {
                        // Add activity point when blog post is published
                        BadgeInfoProvider.UpdateActivityPointsToUser(ActivityPointsEnum.BlogPosts, node.NodeOwner, node.NodeSiteName, true);
                    }
                }
            }

            // Log synchronization task
            LogDocumentChange(node, TaskTypeEnum.CreateDocument, tree);

            if (!usesWorkflow)
            {
                // Send notification
                node.SendNotifications("CREATEDOC");
            }

            // Update search index
            UpdateSearchIndexIfAllowed(node);

            // Reset the changes
            node.ResetChanges();
        }


        /// <summary>
        /// Inserts a new document culture version.
        /// </summary>
        /// <param name="node">Document node</param>
        /// <param name="tree">TreeProvider to use</param>
        /// <param name="cultureCode">Culture code of new culture version (If not specified, node.DocumentCulture property is used.)</param>
        /// <param name="createVersion">Create a new version for document under workflow.</param>
        /// <param name="allowCheckOut">If true, document check out is allowed on the insert action.</param>
        public static void InsertNewCultureVersion(TreeNode node, TreeProvider tree, string cultureCode, bool createVersion = true, bool allowCheckOut = true)
        {
            var settings = new NewCultureDocumentSettings(node, cultureCode, tree)
            {
                CreateVersion = createVersion,
                AllowCheckOut = allowCheckOut
            };

            InsertNewCultureVersion(settings);
        }


        /// <summary>
        /// Inserts a new document culture version.
        /// </summary>
        /// <param name="settings">New culture version settings</param>
        public static void InsertNewCultureVersion(NewCultureDocumentSettings settings)
        {
            TreeProvider tree = settings.Tree;
            TreeNode node = settings.Node;
            TreeNode sourceNode = null;
            bool createVersion = settings.CreateVersion;

            // Check parameters
            CheckParameters(node);

            // Ensure tree provider
            tree = EnsureTreeProvider(node, tree);

            // Do process additional objects if source document not available
            bool copyAttachments = settings.CopyAttachments && (node.DocumentID > 0);
            bool copyCategories = settings.CopyCategories && (node.DocumentID > 0);
            bool clearAttachmentFields = settings.ClearAttachmentFields && (node.DocumentID > 0);

            // Keep original node to copy attachments
            if (copyAttachments || copyCategories)
            {
                sourceNode = node.Clone();
            }

            // Clear workflow information if new version should be created
            if (createVersion)
            {
                // Make sure there is no workflow information for new language version
                ClearWorkflowInformation(node);
            }

            // Do not send notifications
            // Reset changes because node might get updated and can trigger CI which needs to compare changes between inserted and updated node data
            using (new DocumentActionContext { SendNotifications = false })
            {
                node.InsertAsNewCultureVersion(settings.CultureCode, false);
                node.IsLastVersion = true;
            }

            // Get the node workflow
            var workflow = node.GetWorkflow();
            var usesWorkflow = workflow != null;

            // Do not process additional actions
            using (new DocumentActionContext { SendNotifications = false })
            {
                // If workflow defined and new version should be created, create a version
                if (usesWorkflow)
                {
                    if (createVersion)
                    {
                        // Check out for the next editing if allowed and using Check In/Out
                        if (!tree.KeepCheckedInOnInsert && settings.AllowCheckOut && workflow.UseCheckInCheckOut(node.NodeSiteName))
                        {
                            node.VersionManager.CheckOut(node, false);
                        }

                        // Make sure document version is created
                        node.VersionManager.EnsureVersion(node, false);
                    }
                }

                // Ensure correct attachments binding with document or clear their field values if don't copy attachments
                bool update = false;
                if (clearAttachmentFields)
                {
                    update = ClearFieldAttachments(node);
                }
                else if (copyAttachments)
                {
                    // Copy all attachments to a new culture version if required
                    update = new NewCultureVersionAttachmentsCopier(sourceNode, node).Copy();
                }

                if (update)
                {
                    if (usesWorkflow)
                    {
                        // Update version data (field attachments)
                        node.VersionManager.SaveVersion(node);
                    }
                    else
                    {
                        // Update document data
                        node.Update(false);
                    }
                }

                // Copy categories
                if (copyCategories)
                {
                    CopyDocumentCategories(sourceNode.DocumentID, node.DocumentID);
                }
            }

            // Log synchronization task
            LogDocumentChange(node, TaskTypeEnum.CreateDocument, tree);

            if (!usesWorkflow)
            {
                // Send notification
                node.SendNotifications("CREATEDOC");
            }

            // Update search index
            UpdateSearchIndexIfAllowed(node);

            // Reset the changes
            node.ResetChanges();
        }


        /// <summary>
        /// Inserts a new linked document.
        /// </summary>
        /// <param name="node">Document node</param>
        /// <param name="parent">Parent node</param>
        /// <param name="tree">TreeProvider to use</param>
        /// <param name="includeChildNodes">Link also the node child nodes.</param>
        /// <param name="copyPermissions">Indicates if the document permissions should be copied.</param>
        /// <param name="newDocumentsOwner">ID of the new document owner</param>
        /// <param name="newDocumentsGroup">ID of the new document group</param>
        public static void InsertDocumentAsLink(TreeNode node, TreeNode parent, TreeProvider tree = null, bool includeChildNodes = false, bool copyPermissions = false, int newDocumentsOwner = 0, int newDocumentsGroup = 0)
        {
            // Check parameters
            CheckParameters(node, parent);

            // Ensure tree provider
            tree = EnsureTreeProvider(node, tree);

            int originalNodeId = node.NodeID;
            if ((originalNodeId == parent.NodeID) && includeChildNodes)
            {
                throw new Exception("[DocumentHelper.InsertDocumentAsLink]: Page linking is causing application loop, the page '" + node.NodeAliasPath + "' cannot be linked.");
            }

            TreeNode original = null;

            using (new DocumentActionContext { SendNotifications = false })
            {
                node.InsertAsLink(parent, newDocumentsOwner, newDocumentsGroup, false);
                node.IsLastVersion = true;

                if (copyPermissions)
                {
                    original = tree.GetOriginalNode(node);
                    tree.CopyNodePermissions(original, node);
                }
            }

            // Log synchronization task
            LogDocumentChange(node, TaskTypeEnum.CreateDocument, tree);

            // Send notification
            node.SendNotifications("CREATEDOC");

            // Update search index for node
            UpdateSearchIndexIfAllowed(node);

            // Reset the changes
            node.ResetChanges();

            // Process child documents
            if (includeChildNodes && (node.NodeID > 0))
            {
                if (original == null)
                {
                    original = tree.GetOriginalNode(node);
                }

                using (new DocumentActionContext { UseAutomaticOrdering = false, PreserveACLHierarchy = true })
                {
                    // Get child nodes
                    foreach (var child in tree.EnumerateChildren(original))
                    {
                        // Skip already deleted children
                        if (child == null)
                        {
                            continue;
                        }

                        int nodeClassId = ValidationHelper.GetInteger(child.GetValue("NodeClassID"), 0);
                        if (ClassSiteInfoProvider.GetClassSiteInfo(nodeClassId, node.NodeSiteID) != null)
                        {
                            LogContext.AppendLine(HTMLHelper.HTMLEncode(child.NodeAliasPath), LOGCONTEXT_DOCUMENTS);
                            InsertDocumentAsLink(child, node, tree, true, copyPermissions, newDocumentsOwner, newDocumentsGroup);
                        }
                        else
                        {
                            var message = ResHelper.GetString("contentedit.documentwasskipped");
                            string name = string.Format("{0} ({1})", child.GetDocumentName(), child.DocumentNamePath);
                            LogContext.AppendLine(String.Format(message, name), LOGCONTEXT_DOCUMENTS);
                            new DocumentEventLogger(child).Log("LINKDOC", message, false, EventType.WARNING);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Deletes the given document.
        /// </summary>
        /// <param name="node">Document node to delete</param>
        /// <param name="tree">TreeProvider to use</param>
        /// <param name="deleteAllCultures">Delete all culture version of the specified document?</param>
        /// <param name="destroyHistory">Destroy the document history?</param>
        /// <returns>Returns true if last culture (Tree record) has been deleted.</returns>
        public static bool DeleteDocument(TreeNode node, TreeProvider tree = null, bool deleteAllCultures = false, bool destroyHistory = false)
        {
            // Ensure tree provider
            tree = EnsureTreeProvider(node, tree);

            // Prepare settings for deleting document
            var settings = new DeleteDocumentSettings(node, deleteAllCultures, destroyHistory, tree);

            // Delete document
            return DeleteDocument(settings);
        }


        /// <summary>
        /// Deletes document according to supplied settings.
        /// </summary>
        /// <param name="settings">Document deletion settings</param>
        /// <returns>Returns true if last culture (Tree record) has been deleted.</returns>
        public static bool DeleteDocument(DeleteDocumentSettings settings)
        {
            var node = settings.Node;
            var tree = settings.Tree;

            // Check parameters
            CheckParameters(node);

            // Ensure tree provider
            tree = EnsureTreeProvider(node, tree);

            // Check node IDs (cycling check)
            if (settings.DeletedNodeIDs.Contains(node.NodeID))
            {
                throw new Exception("[DocumentHelper.DeleteDocument]: Page deletion is causing application loop, the page '" + node.NodeAliasPath + "' cannot be deleted.");
            }
            settings.DeletedNodeIDs.Add(node.NodeID);

            var deleteCallback = settings.DeleteCallback;

            // If link, directly delete the node
            if (node.IsLink)
            {
                // Delete the child nodes
                DeleteDocumentChildNodes(settings);

                // Add document to the log
                LogContext.AppendLine(node.NodeAliasPath + " (" + node.DocumentCulture + ")", LOGCONTEXT_DOCUMENTS);

                // Delete the node
                var nodeSettings = new DeleteDocumentSettings(settings);
                nodeSettings.DeleteChildNodes = false;
                node.DeleteInternal(nodeSettings);

                settings.DeletedNodeIDs.Remove(node.NodeID);

                // Delete search index
                if (SearchIndexInfoProvider.SearchEnabled && SearchHelper.SearchEnabledForClass(node.NodeClassName))
                {
                    SearchTaskInfoProvider.CreateTask(SearchTaskTypeEnum.Delete, TreeNode.OBJECT_TYPE, SearchFieldsConstants.ID, node.GetSearchID(), node.DocumentID);
                }

                // Log synchronization
                LogDocumentChange(node, TaskTypeEnum.DeleteDocument, tree);

                // Raise callback
                if (deleteCallback != null)
                {
                    deleteCallback(node);
                }

                return true;
            }

            var lastCulture = node.GetTranslatedCultureData().Count == 1;
            if (settings.DeleteAllCultures || lastCulture)
            {
                // Delete the linked documents. Links always lead to the original document, so there is no need to delete links for link.
                DeleteDocumentLinks(settings);

                // Delete child nodes
                DeleteDocumentChildNodes(settings);
            }

            var deleteNodes = new List<TreeNode>();
            var deletedDocs = new HashSet<int>();

            // Delete the document node
            if (settings.DeleteAllCultures)
            {
                // Prepare the nodes to delete
                foreach (var culture in tree.EnumerateCultureVersions(node, null, true))
                {
                    deleteNodes.Add(culture);
                }
            }
            else
            {
                deleteNodes.Add(node);
            }

            bool lastCultureVersionDeleted = true;
            DataSet cultureData = null;

            // Delete the nodes
            for (int i = 0; i < deleteNodes.Count; ++i)
            {
                // Get node to delete
                var deleteNode = deleteNodes[i];

                // Check if the node is root
                bool isRoot = deleteNode.IsRoot();
                // Check if last culture version is deleted
                if ((i != 0) && (i == (deleteNodes.Count - 1)))
                {
                    lastCulture = true;
                }

                // If node is root, do not allow to delete
                if (!isRoot || !lastCulture || settings.AllowRootDeletion)
                {
                    // Add document to the log
                    LogContext.AppendLine(deleteNode.NodeAliasPath + " (" + deleteNode.DocumentCulture + ")", LOGCONTEXT_DOCUMENTS);

                    // Keep original value of publishedVersionExists
                    bool publishedVersionExists = deleteNode.PublishedVersionExists;

                    // Send delete notification
                    deleteNode.SendNotifications("DELETEDOC");

                    // Copy paths to the alternating document if is specified
                    EnsureAlternatingDocument(deleteNode, settings, lastCulture);

                    if (!settings.DestroyHistory && !DocumentActionContext.CurrentForceDestroyHistory)
                    {
                        int versionHistoryId = deleteNode.DocumentCheckedOutVersionHistoryID;
                        if (versionHistoryId > 0)
                        {
                            // Set version to current user (to be in user's recycle bin)
                            var version = VersionHistoryInfoProvider.GetVersionHistoryInfo(versionHistoryId);
                            if (version != null)
                            {
                                version.ModifiedByUserID = tree.UserInfo.UserID;
                                version.ModifiedWhen = DateTime.Now;
                                version.DeletedByUserID = tree.UserInfo.UserID;
                                version.DeletedWhen = DateTime.Now;
                                version.VersionNodeAliasPath = deleteNode.NodeAliasPath;
                                version.DocumentNamePath = deleteNode.DocumentNamePath;
                                VersionHistoryInfoProvider.SetVersionHistoryInfo(version);
                            }

                            // Update published version
                            versionHistoryId = deleteNode.DocumentPublishedVersionHistoryID;
                            if (versionHistoryId > 0)
                            {
                                version = VersionHistoryInfoProvider.GetVersionHistoryInfo(versionHistoryId);
                                if (version != null)
                                {
                                    if (version.WasPublishedFrom != DateTimeHelper.ZERO_TIME)
                                    {
                                        version.WasPublishedTo = DateTime.Now;
                                    }
                                    VersionHistoryInfoProvider.SetVersionHistoryInfo(version);
                                }
                            }
                        }
                        else
                        {
                            // Ensure the document version for recycle bin
                            var manager = VersionManager.GetInstance(tree);
                            manager.EnsureVersion(deleteNode, false, true);
                        }
                    }
                    else
                    {
                        var manager = VersionManager.GetInstance(tree);
                        manager.DestroyDocumentHistory(deleteNode.DocumentID);
                    }

                    // Do not process additional actions
                    using (new DocumentActionContext { SendNotifications = false })
                    {
                        // Delete the node
                        lastCultureVersionDeleted = deleteNode.DeleteInternal(settings);
                    }

                    // Log synchronization
                    LogDocumentChange(deleteNode, TaskTypeEnum.DeleteDocument, tree);

                    // Delete ad-hoc template from culture version
                    int templateId = deleteNode.DocumentPageTemplateID;
                    if (IsAdhocTemplate(templateId) && templateId != deleteNode.NodeTemplateID)
                    {
                        bool delete = true;

                        // Get the culture data for the first request
                        if (cultureData == null)
                        {
                            cultureData = deleteNode.GetTranslatedCultureData()
                                              .Columns("DocumentID", "DocumentPageTemplateID")
                                              .Result;
                        }

                        // Check page templates of other language versions
                        foreach (DataRow row in cultureData.Tables[0].Rows)
                        {
                            var documentId = row["DocumentID"].ToInteger(0);
                            var pageTemplateId = row["DocumentPageTemplateID"].ToInteger(0);

                            if (!deletedDocs.Contains(documentId) && (deleteNode.DocumentID != documentId) && (pageTemplateId == templateId))
                            {
                                delete = false;
                                break;
                            }
                        }

                        // Delete ad-hoc page template
                        if (delete)
                        {
                            DeleteAdhocPageTemplate(tree, templateId);
                        }
                    }

                    // Delete node templates if last culture version deleted
                    if (lastCultureVersionDeleted)
                    {
                        DeleteAdhocPageTemplate(tree, deleteNode.NodeTemplateID);

                        // Delete all ad-hoc templates which are assigned through node GUID
                        PageTemplateInfoProvider.DeleteAdHocTemplates(node.NodeGUID, node.NodeSiteID);
                    }

                    // Keep deleted document ID
                    deletedDocs.Add(deleteNode.DocumentID);

                    // Delete search index for not published document
                    if (SearchIndexInfoProvider.SearchEnabled && SearchHelper.SearchEnabledForClass(deleteNode.NodeClassName))
                    {
                        SearchTaskInfoProvider.CreateTask(SearchTaskTypeEnum.Delete, TreeNode.OBJECT_TYPE, SearchFieldsConstants.ID, deleteNode.GetSearchID(), deleteNode.DocumentID);
                    }

                    // Update activity points
                    if ((CMSString.Compare(deleteNode.NodeClassName, "cms.blogpost", true) == 0) && publishedVersionExists)
                    {
                        BadgeInfoProvider.UpdateActivityPointsToUser(ActivityPointsEnum.BlogPosts, deleteNode.NodeOwner, deleteNode.NodeSiteName, false);

                        // Update counts of blog comments
                        UserInfoProvider.UpdateUserCounts(ActivityPointsEnum.BlogCommentPost, deleteNode.NodeOwner, 0);
                    }

                    // Raise callback
                    if (deleteCallback != null)
                    {
                        deleteCallback(deleteNode);
                    }
                }
                else
                {
                    lastCultureVersionDeleted = false;
                }
            }

            return lastCultureVersionDeleted;
        }


        private static bool IsAdhocTemplate(int templateId)
        {
            var template = PageTemplateInfoProvider.GetPageTemplateInfo(templateId);
            return template != null && !template.IsReusable;
        }


        /// <summary>
        /// If synchronization logging is enabled, disables log context and logs the document change.
        /// </summary>
        /// <param name="node">Node to log</param>
        /// <param name="taskTypeEnum">Task type to log</param>
        /// <param name="tree">Tree provider to use</param>
        public static void LogDocumentChange(TreeNode node, TaskTypeEnum taskTypeEnum, TreeProvider tree)
        {
            if (!tree.LogSynchronization)
            {
                return;
            }

            using (new CMSActionContext { EnableLogContext = false })
            {
                DocumentSynchronizationHelper.LogDocumentChange(node, taskTypeEnum, tree);
            }
        }


        /// <summary>
        /// Clears the workflow information from the given document node.
        /// </summary>
        /// <param name="node">Document node</param>
        public static void ClearWorkflowInformation(TreeNode node)
        {
            TreeProvider.ClearWorkflowInformation(node);
        }


        /// <summary>
        /// Clears the checkout information from the given document node.
        /// </summary>
        /// <param name="node">Document node</param>
        public static void ClearCheckoutInformation(TreeNode node)
        {
            TreeProvider.ClearCheckoutInformation(node);
        }


        /// <summary>
        /// Moves the specified document to the new location.
        /// </summary>
        /// <param name="node">Document node to move</param>
        /// <param name="target">Target node</param>
        /// <param name="tree">TreeProvider to use</param>
        /// <param name="keepPermissions">Indicates if node permissions should be preserved.</param>
        public static void MoveDocument(TreeNode node, TreeNode target, TreeProvider tree = null, bool keepPermissions = false)
        {
            var settings = new MoveDocumentSettings(node, target, tree)
            {
                KeepPermissions = keepPermissions
            };

            MoveDocument(settings);
        }


        /// <summary>
        /// Moves the node according to the given settings.
        /// </summary>
        /// <param name="settings">Document copy settings</param>
        public static void MoveDocument(MoveDocumentSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            var node = settings.Node;
            var target = settings.TargetNode;
            var tree = settings.Tree;
            var keepPermissions = settings.KeepPermissions;

            // Check parameters
            CheckParameters(node, target);

            if (node.IsRoot())
            {
                throw new NotSupportedException("[DocumentHelper.MoveDocument]: Root node cannot be moved.");
            }

            // Check consistency
            if (node.NodeID == target.NodeID)
            {
                throw new NotSupportedException("[DocumentHelper.MoveDocument]: Node cannot be moved under itself.");
            }

            // Ensure tree provider
            tree = EnsureTreeProvider(node, tree);

            // Move to different site
            var originalAliasPath = node.NodeAliasPath;
            var originalSiteId = node.NodeSiteID;
            var targetSiteId = target.NodeSiteID;
            var moveAcrossSite = originalSiteId != targetSiteId;

            // Check cyclic moving (moving of the node to some of its child nodes) on the same site
            if (!moveAcrossSite && target.NodeAliasPath.StartsWith(node.NodeAliasPath + "/", StringComparison.InvariantCultureIgnoreCase))
            {
                throw new NotSupportedException("[DocumentHelper.MoveDocument]: Cannot move node to it's child node.");
            }

            // Log delete all task if moved to another site
            if (moveAcrossSite)
            {
                LogDocumentChange(node, TaskTypeEnum.DeleteAllCultures, tree);
            }

            // Handle the event
            using (var h = DocumentEvents.Move.StartEvent(node, target, tree))
            {
                h.DontSupportCancel();

                if (h.CanContinue())
                {
                    // Set the parent ID and update the node - Document itself takes care of the changing the parent dependent values
                    node.NodeParentID = target.NodeID;

                    // Update children flag since the document was moved under the target node
                    target.NodeHasChildren = true;

                    using (new DocumentActionContext { LogEvents = false, PreserveACLHierarchy = settings.KeepPermissions, CreateSearchTask = false, LogSynchronization = false })
                    {
                        node.Update();
                    }
                }

                // Finish the event
                h.FinishEvent();
            }

            new DocumentEventLogger(node).Log("MOVEDOC", ResHelper.GetString("tasktitle.movedocument"), false);

            if (moveAcrossSite)
            {
                MoveHistories(node.NodeAliasPath, targetSiteId, tree);
            }

            // Log synchronization
            if (tree.LogSynchronization)
            {
                // Create task parameters
                TaskParameters taskParams = null;

                if (keepPermissions)
                {
                    taskParams = new TaskParameters();
                    taskParams.SetParameter("copyPermissions", true);
                }

                using (new CMSActionContext { EnableLogContext = false })
                {
                    if (moveAcrossSite)
                    {
                        DocumentSynchronizationHelper.LogDocumentChange(node, TaskTypeEnum.UpdateDocument, tree, SynchronizationInfoProvider.ENABLED_SERVERS, taskParams, tree.AllowAsyncActions);
                        DocumentSynchronizationHelper.LogDocumentChange(target.NodeSiteName, node.NodeAliasPath + "/%", TaskTypeEnum.UpdateDocument, true, true, tree, SynchronizationInfoProvider.ENABLED_SERVERS, true, taskParams, tree.AllowAsyncActions, null);
                    }
                    else
                    {
                        DocumentSynchronizationHelper.LogDocumentChange(node, TaskTypeEnum.MoveDocument, tree, SynchronizationInfoProvider.ENABLED_SERVERS, taskParams, tree.AllowAsyncActions);
                    }
                }
            }

            LogMoveSearchTasks(node, originalSiteId, originalAliasPath);
        }


        /// <summary>
        /// Logs the search tasks for a move operation of the document
        /// </summary>
        /// <param name="node">Document node</param>
        /// <param name="originalSiteId">Original site ID</param>
        /// <param name="originalAliasPath">Original alias path</param>
        private static void LogMoveSearchTasks(TreeNode node, int originalSiteId, string originalAliasPath)
        {
            if (!SearchIndexInfoProvider.SearchEnabled)
            {
                return;
            }

            // Log partial rebuild task to propagate document change including child documents
            var site = SiteInfoProvider.GetSiteInfo(originalSiteId);
            if (site == null)
            {
                return;
            }

            // Delete moved documents from indexes
            SearchTaskInfoProvider.CreateTask(SearchTaskTypeEnum.Delete, TreeNode.OBJECT_TYPE, SearchFieldsConstants.PARTIAL_REBUILD, site.SiteName + ";" + originalAliasPath, node.DocumentID, false);

            // Use partial rebuild and update new location
            SearchTaskInfoProvider.CreateTask(SearchTaskTypeEnum.Update, TreeNode.OBJECT_TYPE, SearchFieldsConstants.PARTIAL_REBUILD, node.NodeSiteName + ";" + node.NodeAliasPath, node.DocumentID);
        }


        /// <summary>
        /// Copies the node to the specified location.
        /// </summary>
        /// <param name="node">Document node to copy</param>
        /// <param name="target">Target node</param>
        /// <param name="includeChildNodes">Copy also the node child nodes.</param>
        /// <param name="tree">TreeProvider to use</param>
        public static TreeNode CopyDocument(TreeNode node, TreeNode target, bool includeChildNodes = false, TreeProvider tree = null)
        {
            var settings = new CopyDocumentSettings(node, target, tree)
            {
                IncludeChildNodes = includeChildNodes
            };

            return CopyDocument(settings);
        }


        /// <summary>
        /// Copies the node according to the given settings.
        /// </summary>
        /// <param name="settings">Document copy settings</param>
        public static TreeNode CopyDocument(CopyDocumentSettings settings)
        {
            return new DocumentWithVersionsCopier(settings).Copy();
        }


        /// <summary>
        /// Copies categories from one document to another.
        /// </summary>
        /// <param name="originalDocumentId">Original document ID</param>
        /// <param name="newDocumentId">New document ID</param>
        public static void CopyDocumentCategories(int originalDocumentId, int newDocumentId)
        {
            if ((originalDocumentId <= 0) || (newDocumentId <= 0))
            {
                return;
            }

            DataSet ds = DocumentCategoryInfoProvider.GetDocumentCategories(originalDocumentId)
                .Column("CategoryID")
                .TypedResult;

            if (DataHelper.DataSourceIsEmpty(ds))
            {
                return;
            }

            // Add new document to categories
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                DocumentCategoryInfoProvider.AddDocumentToCategory(newDocumentId, Convert.ToInt32(dr["CategoryID"]));
            }
        }


        /// <summary>
        /// Changes the document so it is link to another document.
        /// </summary>
        /// <param name="node">Document node</param>
        /// <param name="linkedNodeId">Node ID to link</param>
        /// <param name="tree">TreeProvider to use</param>
        internal static void ChangeDocumentToLink(TreeNode node, int linkedNodeId, TreeProvider tree)
        {
            CheckParameters(node);
            tree = EnsureTreeProvider(node, tree);
            var documentIds = node.GetTranslatedCultureData()
                                  .Column("DocumentID")
                                  .GetListResult<int>();

            // No culture versions to process
            if (documentIds.Count == 0)
            {
                return;
            }

            var manager = VersionManager.GetInstance(tree);
            using (var tr = new CMSTransactionScope())
            {
                documentIds
                    .ToList()
                    .ForEach(id => manager.DestroyDocumentHistory(id));

                node.ChangeToLink(linkedNodeId);

                tr.Commit();
            }
        }


        /// <summary>
        /// Ensures that the parent document of the blog post is blog month.
        /// </summary>
        /// <param name="node">Blog post document</param>
        /// <param name="parent">Current parent document</param>
        /// <param name="tree">TreeProvider to use</param>
        /// <returns>New parent (blog month document if CMS.BlogMonth document type exists)</returns>
        public static TreeNode EnsureBlogPostHierarchy(TreeNode node, TreeNode parent, TreeProvider tree)
        {
            // Check parameters
            CheckParameters(node, parent);

            // Ensure tree provider
            tree = EnsureTreeProvider(node, tree);

            // Missing parent node
            if (parent == null)
            {
                return null;
            }

            // Get the BlogMonth class to set the default page template
            var monthClass = DataClassInfoProvider.GetDataClassInfo("CMS.BlogMonth");
            if (monthClass == null)
            {
                return parent;
            }

            CultureInfo cultureInfo = CultureHelper.GetCultureInfo(node.DocumentCulture);
            DateTime startDate = ValidationHelper.GetDateTime(node.GetValue("BlogPostDate"), DateTimeHelper.ZERO_TIME);

            // Get the month name and set the first letter to upper case
            string monthName = startDate.ToString("MMMM", cultureInfo.DateTimeFormat) + " " + startDate.Year;
            monthName = TextHelper.ToTitleCase(monthName);
            startDate = new DateTime(startDate.Year, startDate.Month, 1);

            // Check if site exists
            SiteInfo si = SiteInfoProvider.GetSiteInfo(parent.NodeSiteID);
            if (si == null)
            {
                return parent;
            }

            // Move current node one level up when creating under blog month node
            if (string.Equals(parent.NodeClassName, "cms.blogmonth", StringComparison.InvariantCultureIgnoreCase))
            {
                // Try get parent in combination with any culture
                var parentParent = tree.SelectNodes()
                                        .TopN(1)
                                        .WhereEquals("NodeID", parent.NodeParentID)
                                        .CombineWithAnyCulture()
                                        .Published(false)
                                        .FirstOrDefault();

                if (parentParent != null)
                {
                    parent = parentParent;
                }
            }

            var data = GetDocuments("cms.blogmonth")
                .OnSite(si.SiteName)
                .Path(parent.NodeAliasPath, PathTypeEnum.Children)
                .AllCultures()
                .CombineWithDefaultCulture(false)
                .WhereEquals("BlogMonthStartingDate", startDate)
                .Published(false)
                .Columns("NodeID", "DocumentCulture");

            bool createCultureVersion = false;
            if (!DataHelper.DataSourceIsEmpty(data))
            {
                // Check current culture                    
                DataRow[] cultureRow = data.Tables[0].Select("DocumentCulture = '" + node.DocumentCulture + "'");
                createCultureVersion = (cultureRow.Length == 0);

                if (!createCultureVersion)
                {
                    // Get existing
                    var nodeId = ValidationHelper.GetInteger(data.Tables[0].Rows[0]["NodeID"], 0);
                    return GetDocument(nodeId, TreeProvider.ALL_CULTURES, tree);
                }
            }

            // Create the month node
            TreeNode monthNode;

            // Culture version should be created get node from another culture
            if (createCultureVersion)
            {
                var row = data.Tables[0].Rows[0];
                monthNode = GetDocument((int)row["NodeID"], (string)row["DocumentCulture"], tree);
            }
            else
            {
                monthNode = TreeNode.New("CMS.BlogMonth", tree);
                monthNode.DocumentCulture = node.DocumentCulture;
            }

            // Set document page template
            monthNode.SetDefaultPageTemplateID(monthClass.ClassDefaultPageTemplateID);

            // Insert the month
            monthNode.SetValue("BlogMonthName", monthName);
            monthNode.SetValue("BlogMonthStartingDate", startDate);
            monthNode.DocumentName = monthName;

            // Create culture version
            if (createCultureVersion)
            {
                InsertNewCultureVersion(monthNode, tree, node.DocumentCulture);
            }
            // Create new document
            else
            {
                InsertDocument(monthNode, parent, tree);
            }

            // Get workflow info
            var workflow = monthNode.GetWorkflow();

            // Check if auto publish changes is allowed
            if ((workflow != null) && workflow.WorkflowAutoPublishChanges && !workflow.UseCheckInCheckOut(monthNode.NodeSiteName))
            {
                // Automatically publish document
                monthNode.MoveToPublishedStep();
            }

            return monthNode;
        }


        /// <summary>
        /// Returns true if child class is allowed within given parent class and alias path.
        /// </summary>
        /// <param name="node">Node under which is created a new document</param>
        /// <param name="childClassId">Child class ID</param>
        public static bool IsDocumentTypeAllowed(TreeNode node, int childClassId)
        {
            return AllowedChildClassInfoProvider.IsChildClassAllowed(node.GetValue("NodeClassID", 0), childClassId) && DocumentTypeScopeInfoProvider.IsDocumentTypeAllowed(node, childClassId);
        }

        #endregion


        #region "Attachments management"

        /// <summary>
        /// Returns the latest version of the specified attachment.
        /// Returns only an attachment which is not a variant. For an attachment variant returns <c>null</c>.
        /// </summary>
        /// <param name="attachmentGuid">Attachment GUID</param>
        /// <param name="siteName">Site name</param>
        /// <param name="includeBinary">Indicates whether binary data should be included.</param>
        public static DocumentAttachment GetAttachment(Guid attachmentGuid, string siteName, bool includeBinary = true)
        {
            TreeNode node;
            return GetAttachment(attachmentGuid, siteName, includeBinary, out node);
        }


        /// <summary>
        /// Returns the latest version of the specified attachment.
        /// Returns only an attachment which is not a variant. For an attachment variant returns <c>null</c>.
        /// </summary>
        /// <param name="attachmentGuid">Attachment GUID</param>
        /// <param name="siteName">Site name</param>
        /// <param name="includeBinary">Indicates whether binary data should be included.</param>
        /// <param name="node">Returning the latest version of the document node</param>
        public static DocumentAttachment GetAttachment(Guid attachmentGuid, string siteName, bool includeBinary, out TreeNode node)
        {
            node = null;

            var tree = new TreeProvider();

            // Get the attachment
            var attachment = (DocumentAttachment)AttachmentInfoProvider.GetAttachmentInfoWithoutBinary(attachmentGuid, siteName);
            if (attachment == null)
            {
                var vm = VersionManager.GetInstance(tree);
                attachment = (DocumentAttachment)vm.GetLatestAttachmentVersion(attachmentGuid, siteName, includeBinary);

                // Not found, return null
                if (attachment == null)
                {
                    return null;
                }
            }

            if (attachment.IsVariant())
            {
                return null;
            }

            if (attachment.AttachmentFormGUID == Guid.Empty)
            {
                // Get the attachment document
                node = GetDocument(attachment.AttachmentDocumentID, tree);
                return (node != null) ? GetAttachment(node, attachmentGuid, includeBinary) : null;
            }

            // If temporary attachment and requires binary data, get also the binary data
            if (includeBinary)
            {
                attachment = (DocumentAttachment)AttachmentInfoProvider.GetAttachmentInfo(attachmentGuid, siteName);
            }

            return attachment;
        }


        /// <summary>
        /// Returns given version of the specified attachment.
        /// Returns only an attachment which is not a variant. For an attachment variant returns <c>null</c>.
        /// </summary>
        /// <param name="attachmentGuid">Attachment GUID</param>
        /// <param name="versionHistoryId">Required version history ID</param>
        /// <param name="includeBinary">Indicates whether binary column should be included.</param>
        public static DocumentAttachment GetAttachment(Guid attachmentGuid, int versionHistoryId, bool includeBinary = true)
        {
            var attachmentVersion = GetAttachmentInternal(attachmentGuid, versionHistoryId, includeBinary);

            if ((attachmentVersion != null) && attachmentVersion.IsVariant())
            {
                return null;
            }

            return attachmentVersion;
        }


        /// <summary>
        /// Returns given version of the specified attachment.
        /// Searches in main and variant attachments.
        /// </summary>
        private static DocumentAttachment GetAttachmentInternal(Guid attachmentGuid, int versionHistoryId, bool includeBinary = true)
        {
            var attachmentVersion = GetAttachmentVersion(attachmentGuid, versionHistoryId, includeBinary);
            if (attachmentVersion == null)
            {
                return null;
            }

            var attachment = (DocumentAttachment)attachmentVersion;

            // Get binary data from version explicitly. It may be stored in FS
            if (includeBinary)
            {
                attachment.AttachmentBinary = attachmentVersion.AttachmentBinary;
            }

            attachment.AttachmentVersionHistoryID = versionHistoryId;

            // Attachment from database should not have changes
            attachment.ResetChanges();

            return attachment;
        }


        private static AttachmentHistoryInfo GetAttachmentVersion(Guid attachmentGuid, int versionHistoryId, bool includeBinary)
        {
            // Use new TreeProvider if none given
            var tree = new TreeProvider();

            // Get the attachment version
            var vm = VersionManager.GetInstance(tree);

            // Get the attachment version
            return vm.GetAttachmentVersion(versionHistoryId, attachmentGuid, includeBinary);
        }


        /// <summary>
        /// Returns given version of the specified attachment.
        /// Returns only an attachment which is not a variant. For an attachment variant returns <c>null</c>.
        /// </summary>
        /// <param name="documentId">Document ID</param>
        /// <param name="fileName">Attachment file name</param>
        /// <param name="versionHistoryId">Required version history ID</param>
        /// <param name="includeBinary">Indicates whether binary column should be included.</param>
        public static DocumentAttachment GetAttachment(int documentId, string fileName, int versionHistoryId, bool includeBinary = true)
        {
            var tree = new TreeProvider();
            var vm = VersionManager.GetInstance(tree);

            // Get the attachment version
            var attachment = (DocumentAttachment)vm.GetAttachmentVersion(versionHistoryId, fileName, includeBinary);

            if (attachment == null)
            {
                return null;
            }

            attachment.AttachmentVersionHistoryID = versionHistoryId;

            // Attachment from database should not have changes
            attachment.ResetChanges();

            return attachment;
        }


        /// <summary>
        /// Returns specified attachment for the given node.
        /// Returns only an attachment which is not a variant. For an attachment variant returns <c>null</c>.
        /// </summary>
        /// <param name="node">Document node</param>
        /// <param name="attachmentGuid">Attachment GUID</param>
        /// <param name="includeBinary">Indicates whether binary data should be included.</param>
        public static DocumentAttachment GetAttachment(TreeNode node, Guid attachmentGuid, bool includeBinary = true)
        {
            if (node == null)
            {
                throw new Exception("[DocumentHelper.GetAttachment]: Missing page node.");
            }

            DocumentAttachment currentAttachment = null;

            // Get the node workflow
            var wm = WorkflowManager.GetInstance(node.TreeProvider);
            var wi = wm.GetNodeWorkflow(node);
            var getCurrent = true;

            // If workflow defined, use attachment version
            if (wi != null)
            {
                // Get the attachment from the version history
                int versionHistoryId = node.DocumentCheckedOutVersionHistoryID;
                if (versionHistoryId > 0)
                {
                    getCurrent = false;
                    currentAttachment = GetAttachmentInternal(attachmentGuid, versionHistoryId, includeBinary);
                    if (currentAttachment == null)
                    {
                        throw new Exception("[DocumentHelper.GetAttachment]: Attachment with given GUID not found in page version.");
                    }
                }
            }

            // Get the attachment directly from the database
            if (getCurrent)
            {
                // Get original node
                node = node.TreeProvider.GetOriginalNode(node);
                if (node != null)
                {
                    // Get the site
                    var si = SiteInfoProvider.GetSiteInfo(node.NodeSiteID);
                    if (si == null)
                    {
                        throw new Exception("[DocumentHelper.GetAttachment]: Attachment site not found.");
                    }

                    // Get current attachment from the database
                    var atInfo =
                        includeBinary ?
                        AttachmentInfoProvider.GetAttachmentInfo(attachmentGuid, si.SiteName) :
                        AttachmentInfoProvider.GetAttachmentInfoWithoutBinary(attachmentGuid, si.SiteName);

                    currentAttachment = (DocumentAttachment)atInfo;
                }
            }

            if ((currentAttachment != null) && currentAttachment.IsVariant())
            {
                return null;
            }

            // Return the result
            return currentAttachment;
        }


        /// <summary>
        /// Returns attachment for the given node specified by file name.
        /// Returns only an attachment which is not a variant. For an attachment variant returns <c>null</c>.
        /// </summary>
        /// <param name="node">Document node</param>
        /// <param name="fileName">Attachment file name</param>
        /// <param name="includeBinary">Indicates whether binary data should be included.</param>
        public static DocumentAttachment GetAttachment(TreeNode node, string fileName, bool includeBinary = true)
        {
            if (node == null)
            {
                throw new Exception("[DocumentHelper.GetAttachment]: Missing page node.");
            }

            DocumentAttachment attachment = null;

            // Get the node workflow
            var wm = WorkflowManager.GetInstance(node.TreeProvider);
            var wi = wm.GetNodeWorkflow(node);

            bool getCurrent = true;

            // If workflow defined, use attachment version
            if (wi != null)
            {
                // Get the attachment from the version history
                int versionHistoryId = node.DocumentCheckedOutVersionHistoryID;
                if (versionHistoryId > 0)
                {
                    getCurrent = false;
                    attachment = GetAttachment(node.DocumentID, fileName, versionHistoryId, includeBinary);
                }
            }

            // Get the attachment directly from the database
            if (getCurrent)
            {
                // Get original node
                node = node.TreeProvider.GetOriginalNode(node);
                if (node != null)
                {
                    // Get the site
                    var si = SiteInfoProvider.GetSiteInfo(node.NodeSiteID);
                    if (si == null)
                    {
                        throw new Exception("[DocumentHelper.GetAttachment]: Attachment site not found.");
                    }

                    // Get current attachment from the database
                    attachment = (DocumentAttachment)AttachmentInfoProvider.GetAttachmentInfo(node.DocumentID, fileName, includeBinary);
                }
            }

            // Return the result
            return attachment;
        }


        /// <summary>
        /// Gets attachments (<see cref="AttachmentInfo"/>) or attachments versions (<see cref="AttachmentHistoryInfo"/>) for given document.
        /// Result contains objects of type <see cref="AttachmentHistoryInfo"/> if given document is under workflow. Otherwise result contains <see cref="AttachmentInfo"/>. 
        /// Returns only main attachments, not attachment variants.
        /// </summary>
        /// <param name="node">Document which contains requested attachments.</param>
        /// <param name="getBinary">Indicates if binary data should be included.</param>
        public static IObjectQuery GetAttachments(TreeNode node, bool getBinary)
        {
            CheckParameters(node);

            var wi = node.GetWorkflow();
            int versionHistoryId = node.DocumentCheckedOutVersionHistoryID;

            IObjectQuery query;

            if ((wi != null) && (versionHistoryId > 0))
            {
                query = AttachmentHistoryInfoProvider.GetAttachmentHistories().InVersionExceptVariants(versionHistoryId);
            }
            else
            {
                query = AttachmentInfoProvider.GetAttachments().ExceptVariants().WhereEquals("AttachmentDocumentID", node.DocumentID);
            }

            query.IncludeBinaryData = getBinary;

            return query;
        }


        /// <summary>
        /// Deletes attachments bound to the specified field (file field or group attachments field)
        /// </summary>
        /// <param name="className">Document class name</param>
        /// <param name="field">Form field object which contains information to remove its attachments</param>
        public static void DeleteDocumentAttachments(string className, FormFieldInfo field)
        {
            if (field == null)
            {
                return;
            }

            // Get base query
            var pages = GetDocuments(className)
                .All()
                .Published();

            switch (field.DataType)
            {
                case FieldDataType.File:
                    {
                        if (field.IsDummyField)
                        {
                            // Dummy field has no database representation so no file GUID is stored there
                            return;
                        }

                        var columnName = field.Name;
                        pages.Columns("NodeSiteID", columnName);

                        pages.ForEachRow(row =>
                        {
                            // Get attachment GUID and site ID
                            var guid = row[columnName].ToGuid(Guid.Empty);
                            var siteId = row["NodeSiteID"].ToInteger(0);

                            if ((guid == Guid.Empty) || (siteId <= 0))
                            {
                                return;
                            }

                            // Get attachment
                            var attachment = AttachmentInfoProvider.GetAttachmentInfoWithoutBinary(guid, SiteInfoProvider.GetSiteName(siteId));
                            if (attachment == null)
                            {
                                return;
                            }

                            // Delete specified attachment
                            AttachmentInfoProvider.DeleteAttachmentInfo(attachment);
                        });
                    }
                    break;

                case DocumentFieldDataType.DocAttachments:
                    {
                        // Add column
                        pages.Column("DocumentID");

                        var condition = new WhereCondition()
                            .WhereEquals("AttachmentGroupGuid", field.Guid)
                            .WhereIn("AttachmentDocumentID", pages);

                        AttachmentInfoProvider.DeleteAttachments(condition);
                    }
                    break;
            }
        }


        /// <summary>
        /// Gets the primary attachments for the given document IDs. Used internally to display thumbnails in dialogs
        /// </summary>
        /// <param name="documentIds">Document IDs</param>
        /// <param name="getHistories">If true, the process is allowed to retrieve unpublished attachments from version history</param>
        /// <param name="versionHistoryIds">Version history IDs when getting version histories is allowed</param>
        public static SafeDictionary<int, IDataContainer> GetPrimaryAttachmentsForDocuments(IList<int> documentIds, bool getHistories, IList<int> versionHistoryIds)
        {
            // Document IDs must be provided
            if (documentIds == null)
            {
                throw new ArgumentNullException("documentIds");
            }

            // If get histories is enabled, must provide version history IDs
            if (getHistories && (versionHistoryIds == null))
            {
                throw new ArgumentNullException("versionHistoryIds");
            }

            var remainingDocumentIds = new HashSet<int>(documentIds);

            var fileData = new SafeDictionary<int, IDataContainer>();

            // Prepare image priority column
            var imageCondition = "AttachmentExtension IN ('" + String.Join("', '", ImageHelper.ImageExtensions.Select(ext => SqlHelper.GetSafeQueryString("." + ext))) + "')";
            var imagePriority = SqlHelper.GetCaseOrderBy(imageCondition) + ", ";

            // Prepare priority order by, the preferred order is following

            // Single attachment field - e.g. CMS.File attachment or Article teaser
            // - Image - e.g. Product teaser
            // - Non-image - e.g. Product manual

            // Multiple attachments field - e.g. Product images
            // - First attachment in field

            // Unsorted attachments - e.g. Article attachments
            // - First attachment in unsorted attachments

            var priorityOrderBy = "AttachmentIsUnsorted, AttachmentGroupGUID, AttachmentOrder," + imagePriority;

            // Get published attachments first
            IDataQuery files =
                AttachmentInfoProvider.GetAttachments()
                    // Only one attachment is taken for each document and main attachments have priority, it is pointless to query variants
                    .ExceptVariants()
                    .BinaryData(false)
                    .WhereIn("AttachmentDocumentID", documentIds) // Only for the given document IDs
                    .AddColumn(
                        new RowNumberColumn("AttachmentPriority", priorityOrderBy + "AttachmentName, AttachmentLastModified DESC") // Define priority order
                        {
                            PartitionBy = "AttachmentDocumentID"
                        }
                    )
                    .AsNested()
                    .WhereEquals("AttachmentPriority", 1);

            // Fill the lookup table with attachment data
            FillFileDataDictionary(files, fileData, remainingDocumentIds);

            // If some document IDs are not yet covered, get files from attachment history
            if (getHistories && (remainingDocumentIds.Count > 0))
            {
                files =
                    AttachmentHistoryInfoProvider.GetAttachmentHistories()
                        .BinaryData(false)
                        .WhereIn("AttachmentDocumentID", documentIds) // Only for the given document IDs
                        .InVersionsExceptVariants(versionHistoryIds.ToArray())
                        .AddColumn(
                            new RowNumberColumn("AttachmentPriority", priorityOrderBy + "AttachmentLastModified DESC") // Define priority order
                            {
                                PartitionBy = "AttachmentDocumentID"
                            }
                        )
                        .AsNested()
                        .WhereEquals("AttachmentPriority", 1);

                // Fill the lookup table with attachment history data
                FillFileDataDictionary(files, fileData, remainingDocumentIds);
            }

            return fileData;
        }


        /// <summary>
        /// Fills the dictionary of the document ID - file data mapping with data from the given query
        /// </summary>
        /// <param name="fileData">Query with the data</param>
        /// <param name="data">Data dictionary</param>
        /// <param name="remainingDocumentIds">Collection of document IDs for which the data wasn't provided yet. Document IDs for which the data is provided are removed from this collection.</param>
        private static void FillFileDataDictionary(IDataQuery fileData, SafeDictionary<int, IDataContainer> data, HashSet<int> remainingDocumentIds)
        {
            if (fileData == null)
            {
                return;
            }

            fileData.ForEachRow(dr =>
            {
                var documentId = ValidationHelper.GetInteger(dr["AttachmentDocumentID"], 0);
                if (documentId > 0)
                {
                    data[documentId] = new DataRowContainer(dr);
                }

                remainingDocumentIds.Remove(documentId);
            });
        }


        /// <summary>
        /// Deletes attachment from the given document node, including versioning consideration.
        /// </summary>
        /// <param name="node">Document node</param>
        /// <param name="guidColumnName">Column containing the Attachment GUID</param>
        /// <exception cref="InvalidOperationException">Deleting of attachment variants is not supported. Use AttachmentInfoProvider or AttachmentHistoryInfoProvider to handle attachment variants.</exception>

        public static void DeleteAttachment(TreeNode node, string guidColumnName)
        {
            DeleteAttachment(node, guidColumnName, Guid.Empty);
        }


        /// <summary>
        /// Deletes attachment from the given document node, including versioning consideration.
        /// </summary>
        /// <param name="node">Document node</param>
        /// <param name="attachmentGuid">Attachment GUID</param>
        /// <exception cref="InvalidOperationException">Deleting of attachment variants is not supported. Use AttachmentInfoProvider or AttachmentHistoryInfoProvider to handle attachment variants.</exception>
        public static void DeleteAttachment(TreeNode node, Guid attachmentGuid)
        {
            DeleteAttachment(node, null, attachmentGuid);
        }


        /// <summary>
        /// Deletes attachment specified by either GUID or GUID column name from the given document node, including versioning consideration.
        /// </summary>
        /// <param name="node">Document node</param>
        /// <param name="guidColumnName">Column containing the Attachment GUID (optional)</param>
        /// <param name="attachmentGuid">Attachment GUID (optional)</param>
        private static void DeleteAttachment(TreeNode node, string guidColumnName, Guid attachmentGuid)
        {
            if (node == null)
            {
                throw new Exception("[DocumentHelper.DeleteAttachment]: Missing page node.");
            }
            if ((guidColumnName != null) && (node.GetValue(guidColumnName) == null) && (attachmentGuid == Guid.Empty))
            {
                return;
            }

            // Get current attachment
            if ((guidColumnName != null) && (attachmentGuid == Guid.Empty))
            {
                attachmentGuid = ValidationHelper.GetGuid(node.GetValue(guidColumnName), Guid.Empty);
            }

            var si = SiteInfoProvider.GetSiteInfo(node.NodeSiteID);
            if (si == null)
            {
                throw new Exception("[DocumentHelper.DeleteAttachment]: Site with identifier '" + node.NodeSiteID + "' not found.");
            }

            // Get the node workflow
            var vm = VersionManager.GetInstance(node.TreeProvider);
            var wm = vm.WorkflowManager;
            var wi = wm.GetNodeWorkflow(node);

            // If workflow defined, use versioning for remove
            if (wi != null)
            {
                int versionHistoryId = vm.EnsureVersion(node, node.IsPublished);

                DeleteAttachmentVersion(node, attachmentGuid, versionHistoryId);
            }
            else
            {
                DeletePublishedAttachment(node, attachmentGuid);
            }

            // For field attachment
            if (guidColumnName != null)
            {
                // Update the document value
                node.SetValue(guidColumnName, null);

                // Update the document type
                if (node.IsFile() && guidColumnName.Equals("fileattachment", StringComparison.InvariantCultureIgnoreCase))
                {
                    node.DocumentType = null;
                }
            }

            // Update search index for given document
            UpdateSearchIndexIfAllowed(node);
        }


        private static void DeleteAttachmentVersion(TreeNode node, Guid attachmentGuid, int versionHistoryId)
        {
            var attachment = GetAttachmentInternal(attachmentGuid, versionHistoryId, false);

            if (attachment != null)
            {
                if (attachment.IsVariant())
                {
                    throw new InvalidOperationException("Deleting of attachment variants is not supported. Use AttachmentHistoryInfoProvider to handle attachment variants.");
                }

                // Handle the event
                var tree = new TreeProvider();

                using (var h = WorkflowEvents.RemoveAttachmentVersion.StartEvent(node, attachment, tree))
                {
                    h.DontSupportCancel();

                    if (h.CanContinue())
                    {
                        // Remove the AttachmentHistory
                        var vm = VersionManager.GetInstance(tree);
                        vm.RemoveAttachmentVersion(versionHistoryId, attachmentGuid);
                    }

                    // Finalize the event
                    h.FinishEvent();
                }
            }
        }

        private static void DeletePublishedAttachment(TreeNode node, Guid attachmentGuid)
        {
            var attachment = (DocumentAttachment)AttachmentInfoProvider.GetAttachmentInfoWithoutBinary(attachmentGuid, node.NodeSiteName);
            if (attachment != null)
            {
                if (attachment.IsVariant())
                {
                    throw new InvalidOperationException("Deleting of attachment variants is not supported. Use AttachmentInfoProvider to handle attachment variants.");
                }

                // Handle the event
                using (var h = DocumentEvents.DeleteAttachment.StartEvent(node, attachment, node.TreeProvider))
                {
                    h.DontSupportCancel();

                    if (h.CanContinue())
                    {
                        // Delete the attachment from the database and disk
                        AttachmentInfoProvider.DeleteAttachmentInfo(attachment.AttachmentID);
                    }

                    // Finalize the event
                    h.FinishEvent();
                }
            }
        }


        /// <summary>
        /// Adds given attachment to the document. Does not update the document to the database, updates the attachment if currently present.
        /// </summary>
        /// <param name="node">Document node</param>
        /// <param name="guidColumnName">Column containing the Attachment GUID</param>
        /// <param name="file">Attachment file</param>
        /// <param name="width">New width of the image attachment</param>
        /// <param name="height">New height of the image attachment</param>
        /// <param name="maxSideSize">Maximum side size of the image attachment</param>
        public static DocumentAttachment AddAttachment(TreeNode node, string guidColumnName, HttpPostedFile file, int width = ImageHelper.AUTOSIZE, int height = ImageHelper.AUTOSIZE, int maxSideSize = ImageHelper.AUTOSIZE)
        {
            return AddAttachment(node, guidColumnName, Guid.Empty, Guid.Empty, file, width, height, maxSideSize);
        }


        /// <summary>
        /// Adds specified attachment to the document. Does not update the document to the database, updates the attachment if currently present.
        /// </summary>
        /// <param name="node">Document node</param>
        /// <param name="guidColumnName">Column containing the Attachment GUID</param>
        /// <param name="file">Attachment file path</param>
        /// <param name="width">New width of the image attachment</param>
        /// <param name="height">New height of the image attachment</param>
        /// <param name="maxSideSize">Maximum side size of the image attachment</param>
        public static DocumentAttachment AddAttachment(TreeNode node, string guidColumnName, string file, int width = ImageHelper.AUTOSIZE, int height = ImageHelper.AUTOSIZE, int maxSideSize = ImageHelper.AUTOSIZE)
        {
            return AddAttachment(node, guidColumnName, Guid.Empty, Guid.Empty, file, width, height, maxSideSize);
        }


        /// <summary>
        /// Adds specified group attachment to the document or updates the attachment if already present.
        /// </summary>
        /// <param name="node">Document node</param>
        /// <param name="attachmentGuid">Attachment GUID</param>
        /// <param name="groupGuid">GUID of the attachment group</param>
        /// <param name="file">Attachment file path</param>
        /// <param name="width">New width of the image attachment</param>
        /// <param name="height">New height of the image attachment</param>
        /// <param name="maxSideSize">Maximum side size of the image attachment</param>
        public static DocumentAttachment AddGroupedAttachment(TreeNode node, Guid attachmentGuid, Guid groupGuid, string file, int width = ImageHelper.AUTOSIZE, int height = ImageHelper.AUTOSIZE, int maxSideSize = ImageHelper.AUTOSIZE)
        {
            return AddAttachment(node, null, attachmentGuid, groupGuid, file, width, height, maxSideSize);
        }


        /// <summary>
        /// Adds given group attachment to the document or updates the attachment if already present.
        /// </summary>
        /// <param name="node">Document node</param>
        /// <param name="attachmentGuid">Attachment GUID</param>
        /// <param name="groupGuid">GUID of the attachment group</param>
        /// <param name="file">Attachment file path</param>
        /// <param name="width">New width of the image attachment</param>
        /// <param name="height">New height of the image attachment</param>
        /// <param name="maxSideSize">Maximum side size of the image attachment</param>
        public static DocumentAttachment AddGroupedAttachment(TreeNode node, Guid attachmentGuid, Guid groupGuid, HttpPostedFile file, int width = ImageHelper.AUTOSIZE, int height = ImageHelper.AUTOSIZE, int maxSideSize = ImageHelper.AUTOSIZE)
        {
            if (groupGuid == Guid.Empty)
            {
                throw new Exception("[DocumentHelper.AddGroupedAttachment]: Missing attachment group GUID.");
            }

            return AddAttachment(node, null, attachmentGuid, groupGuid, file, width, height, maxSideSize);
        }


        /// <summary>
        /// Adds specified unsorted attachment to the document or updates the attachment if already present.
        /// </summary>
        /// <param name="node">Document node</param>
        /// <param name="attachmentGuid">Attachment GUID</param>
        /// <param name="file">Attachment file path</param>
        /// <param name="width">New width of the image attachment</param>
        /// <param name="height">New height of the image attachment</param>
        /// <param name="maxSideSize">Maximum side size of the image attachment</param>
        public static DocumentAttachment AddUnsortedAttachment(TreeNode node, Guid attachmentGuid, string file, int width = ImageHelper.AUTOSIZE, int height = ImageHelper.AUTOSIZE, int maxSideSize = ImageHelper.AUTOSIZE)
        {
            return AddAttachment(node, null, attachmentGuid, Guid.Empty, file, width, height, maxSideSize);
        }


        /// <summary>
        /// Adds given unsorted attachment to the document or updates the attachment if already present.
        /// </summary>
        /// <param name="node">Document node</param>
        /// <param name="attachmentGuid">Attachment GUID</param>
        /// <param name="file">Attachment file path</param>
        /// <param name="width">New width of the image attachment</param>
        /// <param name="height">New height of the image attachment</param>
        /// <param name="maxSideSize">Maximum side size of the image attachment</param>
        public static DocumentAttachment AddUnsortedAttachment(TreeNode node, Guid attachmentGuid, HttpPostedFile file, int width = ImageHelper.AUTOSIZE, int height = ImageHelper.AUTOSIZE, int maxSideSize = ImageHelper.AUTOSIZE)
        {
            return AddAttachment(node, null, attachmentGuid, Guid.Empty, file, width, height, maxSideSize);
        }


        /// <summary>
        /// Adds an attachment to the document. Does not update the document to the database, updates the attachment if currently present.
        /// </summary>
        /// <param name="node">Document node</param>
        /// <param name="guidColumnName">Column containing the Attachment GUID (optional)</param>
        /// <param name="attachmentGuid">GUID of the attachment (optional)</param>
        /// <param name="groupGuid">GUID of the attachment group (optional)</param>
        /// <param name="source">Attachment source (HttpPostedFile, AttachmentInfo or file path)</param>
        /// <param name="width">New width of the image attachment</param>
        /// <param name="height">New height of the image attachment</param>
        /// <param name="maxSideSize">Maximum side size of the image attachment</param>
        public static DocumentAttachment AddAttachment(TreeNode node, string guidColumnName, Guid attachmentGuid, Guid groupGuid, AttachmentSource source, int width = ImageHelper.AUTOSIZE, int height = ImageHelper.AUTOSIZE, int maxSideSize = ImageHelper.AUTOSIZE)
        {
            Guid newGuid;
            DocumentAttachment attachment;

            if (node == null)
            {
                throw new Exception("[DocumentHelper.AddAttachment]: Missing page node.");
            }

            // Get the node workflow
            var versionManager = VersionManager.GetInstance(node.TreeProvider);
            var workflowManager = versionManager.WorkflowManager;
            var workflowInfo = workflowManager.GetNodeWorkflow(node);
            if (workflowInfo != null)
            {
                // Ensure the document version
                versionManager.EnsureVersion(node, node.IsPublished);
            }

            int versionHistoryId = node.DocumentCheckedOutVersionHistoryID;
            bool usesWorkflow = (workflowInfo != null) || (versionHistoryId > 0);

            // Get document GUID column name value
            if ((guidColumnName != null) && (node.GetValue(guidColumnName) != null))
            {
                attachmentGuid = ValidationHelper.GetGuid(node.GetValue(guidColumnName), Guid.Empty);
            }

            var siteName = GetSiteName(node);

            // Get existing attachment
            var currentAttachment = GetExistingAttachment(attachmentGuid, usesWorkflow, versionHistoryId, siteName);
            if (currentAttachment == null)
            {
                newGuid = attachmentGuid != Guid.Empty ? attachmentGuid : Guid.NewGuid();
                attachment = AttachmentInfoProvider.CreateNewAttachment(source, newGuid, node.DocumentID, siteName);

                if (attachment.IsVariant())
                {
                    throw new InvalidOperationException("Inserting or updating of attachment variants is not supported. Use AttachmentInfoProvider or AttachmentHistoryInfoProvider to handle attachment variants data.");
                }

                // Ensure unique attachment file name
                if (node.TreeProvider.CheckUniqueAttachmentNames)
                {
                    attachment.AttachmentName = GetUniqueAttachmentFileName(node, attachment);
                }
            }
            else
            {
                newGuid = currentAttachment.AttachmentGUID;

                // Takes attachment from posted data
                attachment = AttachmentInfoProvider.CreateNewAttachment(source, newGuid, node.DocumentID, siteName);

                if (attachment.IsVariant())
                {
                    throw new InvalidOperationException("Inserting or updating of attachment variants is not supported. Use AttachmentInfoProvider or AttachmentHistoryInfoProvider to handle attachment variants data.");
                }

                // Sets instance of existing attachment by new posted attachment
                var nameChanged = currentAttachment.AttachmentName != attachment.AttachmentName;

                AttachmentInfoProvider.CopyAttachmentProperties(attachment, currentAttachment);

                // From this point we will work with updated existing attachment
                attachment = currentAttachment;

                // Ensure unique attachment file name
                if (node.TreeProvider.CheckUniqueAttachmentNames && nameChanged)
                {
                    attachment.AttachmentName = GetUniqueAttachmentFileName(node, attachment);
                }
            }

            // Set attachment grouped attachment flag
            attachment.AttachmentGroupGUID = groupGuid;

            // Set attachment unsorted attachment flag
            attachment.AttachmentIsUnsorted = String.IsNullOrEmpty(guidColumnName) && groupGuid == Guid.Empty;

            // Ensure image attachment resize
            if (ImageHelper.IsImage(attachment.AttachmentExtension))
            {
                AttachmentBinaryHelper.ResizeImageAttachment(attachment, width, height, maxSideSize);
            }

            // Save attachment - if workflow defined, use versioning for save
            if (usesWorkflow)
            {
                versionManager.SaveAttachmentVersion(attachment, versionHistoryId);
            }
            else
            {
                SaveAttachment(attachment, node);
            }

            GenerateVariantsForAttachment(attachment, node);

            // Set the GUID of the attachment to the document field if field attachment is processed
            if (guidColumnName != null)
            {
                // Update the document
                node.SetValue(guidColumnName, newGuid);
            }

            // Set default extension
            SetDefaultExtension(node, guidColumnName, attachment);

            // Update search index for given document
            UpdateSearchIndexIfAllowed(node);

            // Return current attachment version
            return attachment;
        }


        /// <summary>
        /// Updates the document attachment
        /// </summary>
        /// <param name="node">Document node</param>
        /// <param name="attachment">Attachment to update</param>
        /// <exception cref="ArgumentNullException"><paramref name="node"/> or <paramref name="attachment"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Updating of attachment variants is not supported. Use AttachmentInfoProvider or AttachmentHistoryInfoProvider to handle attachment variants data.</exception>
        public static void UpdateAttachment(TreeNode node, DocumentAttachment attachment)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node), "Document node is not provided.");
            }

            if (attachment == null)
            {
                throw new ArgumentNullException(nameof(attachment), "Attachment is not provided.");
            }

            if (attachment.IsVariant())
            {
                throw new InvalidOperationException("Updating of attachment variants is not supported. Use AttachmentInfoProvider or AttachmentHistoryInfoProvider to handle attachment variants data.");
            }

            // Get the node workflow
            var vm = VersionManager.GetInstance(node.TreeProvider);
            var wm = vm.WorkflowManager;

            var wi = wm.GetNodeWorkflow(node);
            if (wi != null)
            {
                // Ensure the document version
                vm.EnsureVersion(node, node.IsPublished);
            }

            // Document uses workflow and document is already saved (attachment is not temporary)
            var versionHistoryId = node.DocumentCheckedOutVersionHistoryID;
            var usesWorkflow = versionHistoryId > 0;
            var generateVariants = attachment.ItemChanged("AttachmentBinary");

            attachment.AllowPartialUpdate = true;

            if (usesWorkflow)
            {
                var saveContext = new AttachmentHistorySetterContext(attachment, versionHistoryId);
                if (saveContext.NewAttachmentVersionToBeCreated)
                {
                    generateVariants = true;
                }

                vm.SaveAttachmentVersion(saveContext);               
            }
            else
            {
                SaveAttachment(attachment, node);
            }

            if (generateVariants)
            {
                GenerateVariantsForAttachment(attachment, node);
            }

            if (!usesWorkflow)
            {
                // Update search index for given document
                UpdateSearchIndexIfAllowed(node);

                DocumentSynchronizationHelper.LogDocumentChange(node, TaskTypeEnum.UpdateDocument, node.TreeProvider);
            }
        }


        /// <summary>
        /// Returns unique attachment file name in the document scope.
        /// </summary>
        /// <param name="node">Document node</param>
        /// <param name="attachment">Attachment for which the name should be unified</param>
        public static string GetUniqueAttachmentFileName(TreeNode node, DocumentAttachment attachment)
        {
            if (node == null)
            {
                throw new Exception("[DocumentHelper.GetUniqueAttachmentFileName]: Missing page node.");
            }

            // Get site name
            var siteName = SiteInfoProvider.GetSiteInfo(node.NodeSiteID).SiteName;

            var fileName = attachment.AttachmentName;
            int currentAttachmentId;

            // Get extension and remove it from file name
            var indexOfExtension = fileName.LastIndexOf(".", StringComparison.Ordinal);
            var extension = String.Empty;
            if (indexOfExtension > 0)
            {
                extension = fileName.Substring(indexOfExtension).TrimStart('.');
                extension = '.' + URLHelper.GetSafeFileName(extension, siteName);
                fileName = fileName.Remove(indexOfExtension);
            }

            // Get safe file name
            fileName = URLHelper.GetSafeFileName(fileName, siteName);
            var originalName = fileName;

            // Append extension to safe file name
            fileName += extension;

            // Remove unique index
            if (originalName.EndsWith(")", StringComparison.Ordinal))
            {
                originalName = Regex.Replace(originalName, "[ " + URLHelper.ForbiddenCharactersReplacement(siteName) + "](\\(\\d+\\))$", ""); //.TrimEnd(new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '(', ')', ' ', ForbiddenCharactersReplacement(siteName) });
                if (originalName == String.Empty)
                {
                    originalName = fileName;
                }
            }

            // Get all which may possibly match 
            string searchFileName = TextHelper.LimitLength(originalName, AttachmentInfoProvider.MAXATTACHMENTNAMELENGTH - 6, String.Empty);

            // Create where condition
            var condition = new WhereCondition()
                .WhereEquals("AttachmentDocumentID", node.DocumentID)
                .WhereStartsWith("AttachmentName", searchFileName);

            // Get the node workflow
            DataSet ds;

            var vm = VersionManager.GetInstance(node.TreeProvider);
            var wm = vm.WorkflowManager;
            var wi = wm.GetNodeWorkflow(node);
            string idColumn;

            if (wi != null)
            {
                idColumn = "AttachmentHistoryID";
                currentAttachmentId = attachment.AttachmentHistoryID;

                // Get document version
                int versionHistoryId = node.DocumentCheckedOutVersionHistoryID;

                // Get attachments for given version
                ds = AttachmentHistoryInfoProvider.GetAttachmentHistories()
                                                  .InVersionExceptVariants(versionHistoryId)
                                                  .Columns("AttachmentHistoryID, AttachmentName")
                                                  .Where(condition)
                                                  .BinaryData(true);
            }
            else
            {
                idColumn = "AttachmentID";
                currentAttachmentId = attachment.AttachmentID;

                // Get attachments
                ds =
                    AttachmentInfoProvider.GetAttachments()
                        // Only main attachments have unique file names
                        .ExceptVariants()
                        .Columns("AttachmentID, AttachmentName")
                        .Where(condition)
                        .BinaryData(true);
            }

            // Check collisions
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                var dt = ds.Tables[0];

                bool unique = false;
                int uniqueIndex = 0;

                do
                {
                    // Get matching rows
                    var match = dt.Select("AttachmentName = '" + SqlHelper.EscapeQuotes(fileName) + "' AND " + idColumn + "<>" + currentAttachmentId);
                    if (match.Length == 0)
                    {
                        // If not match, consider as unique
                        unique = true;
                    }
                    else
                    {
                        // If match (duplicity found), create new name
                        uniqueIndex++;

                        string uniqueString = " (" + uniqueIndex + ")";

                        int originalLength = AttachmentInfoProvider.MAXATTACHMENTNAMELENGTH - uniqueString.Length;
                        if (originalName.Length > originalLength)
                        {
                            fileName = originalName.Substring(0, originalLength) + uniqueString;
                        }
                        else
                        {
                            fileName = originalName + uniqueString;
                        }

                        // Get safe file name and append extension
                        fileName = URLHelper.GetSafeFileName(fileName, siteName) + extension;
                    }
                }
                while (!unique);
            }

            return fileName;
        }


        ///<summary>
        /// Finds out whether given attachment name is unique.
        ///</summary>
        /// <param name="node">Document node</param>
        /// <param name="attachment">Attachment to check</param>
        /// <returns>True if attachment name is unique</returns>
        public static bool AttachmentHasUniqueName(TreeNode node, DocumentAttachment attachment)
        {
            var checkVersionHistory = (node.DocumentCheckedOutVersionHistoryID > 0);
            if (checkVersionHistory)
            {
                return AttachmentHistoryInfoProvider.IsUniqueAttachmentName(node, attachment.AttachmentName, attachment.AttachmentExtension, attachment.AttachmentHistoryID);
            }

            return AttachmentInfoProvider.IsUniqueAttachmentName(node.DocumentID, attachment.AttachmentName, attachment.AttachmentExtension, attachment.AttachmentID);
        }


        /// <summary>
        /// Saves temporary attachments for specified form.
        /// </summary>
        /// <param name="node">Document node</param>
        /// <param name="formGuid">Form GUID</param>
        /// <param name="siteName">Site name</param>
        /// <returns>True if node was changed, false otherwise</returns>
        public static bool SaveTemporaryAttachments(TreeNode node, Guid formGuid, string siteName)
        {
            bool nodeChanged = false;

            if (node == null)
            {
                throw new Exception("[DocumentHelper.SaveTemporaryAttachments]: Missing page node.");
            }

            // Get the node workflow
            var vm = VersionManager.GetInstance(node.TreeProvider);
            var wm = vm.WorkflowManager;
            var wi = wm.GetNodeWorkflow(node);
            int versionHistoryId = 0;
            if (wi != null)
            {
                // Ensure the document version
                versionHistoryId = vm.EnsureVersion(node, node.IsPublished);
            }

            // Get temporary attachments
            var attachments =
                AttachmentInfoProvider.GetAttachments()
                    .WhereEquals("AttachmentFormGUID", formGuid);

            foreach (var ai in attachments)
            {
                ai.AttachmentFormGUID = Guid.Empty;
                ai.AttachmentDocumentID = node.DocumentID;
                ai.AttachmentSiteID = node.NodeSiteID;

                // Ensure attachment binary data
                if (ai.AttachmentBinary == null)
                {
                    string filePath = AttachmentBinaryHelper.GetFilePhysicalPath(siteName, ai.AttachmentGUID.ToString(), ai.AttachmentExtension);

                    if (File.Exists(filePath))
                    {
                        ai.AttachmentBinary = File.ReadAllBytes(filePath);
                    }
                }

                // Ensure document extension for file
                if (!ai.AttachmentIsUnsorted && (ai.AttachmentGroupGUID == Guid.Empty) && node.IsFile())
                {
                    // Update document extensions if no custom are used
                    if (!node.DocumentUseCustomExtensions)
                    {
                        node.DocumentExtensions = ai.AttachmentExtension;
                    }
                    node.DocumentType = ai.AttachmentExtension;
                }

                var attachment = new DocumentAttachment(ai);
                if (wi == null)
                {
                    // Make the attachment not temporary
                    SaveAttachment(attachment, node);
                }
                else
                {
                    // Delete temporary attachment and attach attachment to the given version of the document
                    AttachmentInfoProvider.DeleteAttachmentInfo(ai);
                    vm.SaveAttachmentVersion(attachment, versionHistoryId);
                }

                var context = new AttachmentVariantContext(node);
                attachment.GenerateAllVariants(context);

                nodeChanged = true;
            }

            return nodeChanged;
        }


        /// <summary>
        /// Returns URL to the given attachment.
        /// </summary>
        /// <param name="attachment">Attachment info</param>
        /// <param name="versionHistoryId">Version history ID</param>
        public static string GetAttachmentUrl(IAttachment attachment, int versionHistoryId)
        {
            return GetAttachmentUrl(attachment.AttachmentGUID, versionHistoryId);
        }


        /// <summary>
        /// Returns URL to the specified attachment GUID.
        /// </summary>
        /// <param name="attGuid">Attachment GUID</param>
        /// <param name="versionHistoryId">Version history ID</param>
        public static string GetAttachmentUrl(Guid attGuid, int versionHistoryId)
        {
            string fileUrl = "~/CMSPages/GetFile.aspx?guid=" + attGuid;
            if (versionHistoryId > 0)
            {
                fileUrl += "&versionhistoryid=" + versionHistoryId;
            }
            return fileUrl;
        }


        /// <summary>
        /// Moves attachment up within node attachments. Supports unordered, grouped and temporary attachments.
        /// </summary>
        /// <param name="attachmentGuid">GUID of attachment or attachment history</param>
        /// <param name="node">TreeNode under which attachments will be sorted.</param>
        /// <exception cref="ArgumentNullException"><paramref name="node"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Moving attachment variants is not supported.</exception>
        public static void MoveAttachmentUp(Guid attachmentGuid, TreeNode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }

            var attachment = GetAttachmentObject(attachmentGuid, node);
            if (attachment != null)
            {
                if (attachment.IsVariant())
                {
                    throw new InvalidOperationException("Moving attachment variants is not supported.");
                }

                attachment.Generalized.MoveObjectUp();
            }
        }


        private static IAttachment GetAttachmentObject(Guid attachmentGuid, TreeNode node)
        {
            IAttachment attachment;

            if (node.DocumentCheckedOutVersionHistoryID > 0)
            {
                // Under workflow sort attachment history
                attachment = GetAttachmentVersion(attachmentGuid, node.DocumentCheckedOutVersionHistoryID, false);
            }
            else
            {
                // Without workflow sort published attachments
                attachment = AttachmentInfoProvider.GetAttachmentInfoWithoutBinary(attachmentGuid, node.NodeSiteName);
            }

            return attachment;
        }


        /// <summary>
        /// Moves attachment down within node attachments. Supports unordered, grouped and temporary attachments.
        /// </summary>
        /// <param name="attachmentGuid">GUID of attachment or attachment history</param>
        /// <param name="node">TreeNode under which attachments will be sorted</param>
        /// <exception cref="ArgumentNullException"><paramref name="node"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Moving attachment variants is not supported.</exception>
        public static void MoveAttachmentDown(Guid attachmentGuid, TreeNode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }

            var attachment = GetAttachmentObject(attachmentGuid, node);
            if (attachment != null)
            {
                if (attachment.IsVariant())
                {
                    throw new InvalidOperationException("Moving attachment variants is not supported.");
                }

                attachment.Generalized.MoveObjectDown();
            }
        }


        /// <summary>
        /// Copies unsorted and grouped attachments of the given document as temporary attachments for specified form.
        /// </summary>
        /// <param name="node">Original document node</param>
        /// <param name="formGuid">Form GUID</param>
        /// <param name="siteName">Name of the site</param>
        /// <remarks>This method is intended to be used only during new culture version of the document creation.</remarks>
        public static void CopyAttachmentsAsTemporary(TreeNode node, Guid formGuid, string siteName)
        {
            var site = SiteInfoProvider.GetSiteInfo(siteName);
            if (site == null)
            {
                throw new NullReferenceException("Node site not found.");
            }

            var attachmentGuids =
                GetAttachments(node, true)
                    .ApplySettings(settings => settings
                        .Columns("AttachmentGUID")
                        .Where(where => where
                            .WhereTrue("AttachmentIsUnsorted")
                            .Or()
                            .WhereNotNull("AttachmentGroupGUID")
                        )
                    )
                    .GetListResult<Guid>();

            foreach (Guid attachmentGuid in attachmentGuids)
            {
                var attachment = GetAttachmentToCopyAsTemporary(node, attachmentGuid);
                if (attachment == null)
                {
                    continue;
                }

                // Set temporary attachment properties
                attachment.AttachmentID = 0;
                attachment.AttachmentGUID = Guid.NewGuid();
                attachment.AttachmentDocumentID = 0;
                attachment.AttachmentFormGUID = formGuid;
                attachment.AttachmentSiteID = site.SiteID;

                AttachmentInfoProvider.SetAttachmentInfo(attachment);
            }
        }


        private static AttachmentInfo GetAttachmentToCopyAsTemporary(TreeNode node, Guid attachmentGuid)
        {
            // Get attachment history of latest version
            int versionHistoryId = node.DocumentCheckedOutVersionHistoryID;
            if (versionHistoryId > 0)
            {
                var history = node.VersionManager.GetLatestAttachmentVersion(attachmentGuid, node.NodeSiteName);
                return history != null ? history.ConvertToAttachment() : null;
            }

            // Get published attachment
            var attachment = AttachmentInfoProvider.GetAttachmentInfo(attachmentGuid, node.NodeSiteName);
            if (attachment != null)
            {
                // Get the binary if not present
                if (attachment.AttachmentBinary == null)
                {
                    attachment.AttachmentBinary = AttachmentBinaryHelper.GetAttachmentBinary(attachmentGuid, node.NodeSiteName);
                }
            }

            return attachment;
        }

        #endregion


        #region "Data transfer methods"

        /// <summary>
        /// Copies the data from the source node to the destination node according to the settings.
        /// </summary>
        /// <param name="sourceNode">Source node</param>
        /// <param name="destNode">Destination node</param>
        /// <param name="settings">Copy node data settings</param>
        public static void CopyNodeData(TreeNode sourceNode, TreeNode destNode, CopyNodeDataSettings settings)
        {
            sourceNode.CopyDataTo(destNode, settings);
        }


        /// <summary>
        /// Updates culture data field value for all documents given by where condition.
        /// </summary>
        /// <param name="fieldName">Field name</param>
        /// <param name="value">Value</param>
        /// <param name="where">Where condition</param>
        public static void ChangeDocumentCultureDataField(string fieldName, object value, WhereCondition where)
        {
            DocumentCultureDataInfoProvider.BulkUpdateData(where, new Dictionary<string, object>
            {
                { fieldName, value }
            });
        }

        #endregion


        #region "Tree management"

        /// <summary>
        /// Deletes complete site tree including the version history.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="tree">Tree provider to use</param>
        /// <param name="deleteCallback">Callback function raised when document has been deleted</param>
        public static void DeleteSiteTree(string siteName, TreeProvider tree, OnAfterDocumentDeletedEventHandler deleteCallback = null)
        {
            if (tree == null)
            {
                tree = new TreeProvider();
            }

            // Get root document in any culture
            var root = tree.SelectNodes()
                           .All()
                           .AllCultures(false)
                           .CombineWithAnyCulture()
                           .OnSite(siteName)
                           .Path("/")
                           .FirstOrDefault();

            // Process within transaction
            if (root != null)
            {
                using (var tr = new CMSTransactionScope())
                {
                    // Delete the document including root document
                    var settings = new DeleteDocumentSettings(root, true, true, tree);
                    settings.AllowRootDeletion = true;
                    settings.DeleteCallback = deleteCallback;
                    DeleteDocument(settings);

                    // Commit the transaction
                    tr.Commit();
                }
            }

            int siteId = SiteInfoProvider.GetSiteID(siteName);

            // Delete documents recycle bin versions
            VersionManager.GetInstance(tree).DeleteRecycleBinAndAllRelatedOlderVersions(siteId);

            // Delete site temporary attachments
            AttachmentInfoProvider.DeleteTemporaryAttachments(siteId);

            // Clear hashtables
            ProviderHelper.ClearHashtables(DocumentNodeDataInfo.OBJECT_TYPE, true);
        }


        /// <summary>
        /// Occurs after document is deleted.
        /// </summary>
        /// <param name="node">Document node</param>
        public delegate void OnAfterDocumentDeletedEventHandler(TreeNode node);

        #endregion


        #region "General methods"

        /// <summary>
        /// Gets the published state of the document from the given data container
        /// </summary>
        /// <param name="dc">Data container</param>
        public static bool GetPublished(IDataContainer dc)
        {
            if (dc.ContainsColumn("DocumentCanBePublished"))
            {
                // Check through common column
                if (!ValidationHelper.GetBoolean(dc.GetValue("DocumentCanBePublished"), false))
                {
                    return false;
                }
            }
            else
            {
                // Validate columns presence
                foreach (var col in DocumentColumnLists.CANBEPUBLISHED_REQUIRED_COLUMNS)
                {
                    if (!dc.ContainsColumn(col))
                    {
                        throw new Exception("[DocumentHelper.GetPublished]: There must be 'DocumentCanBePublished' or '" + col + "' column present in order to evaluate the published status of the document.");
                    }
                }

                // Check if the document is archived, if so, do not consider published
                if ((dc.GetValue("DocumentWorkflowStepID") == null) ||
                    ValidationHelper.GetBoolean(dc.GetValue("DocumentIsArchived"), false))
                {
                    return false;
                }

                // Check published / checked out version
                bool checkedOutVersionHistoryExists = dc.GetValue("DocumentCheckedOutVersionHistoryID") != null;
                bool publishedVersionHistoryExists = dc.GetValue("DocumentPublishedVersionHistoryID") != null;

                if (checkedOutVersionHistoryExists != publishedVersionHistoryExists)
                {
                    return false;
                }
            }

            // Validate publish from / to columns
            if (!dc.ContainsColumn("DocumentPublishFrom"))
            {
                throw new Exception("[DocumentHelper.GetPublished]: There must be 'DocumentPublishFrom' column present in order to evaluate the published status of the document.");
            }

            if (!dc.ContainsColumn("DocumentPublishFrom"))
            {
                throw new Exception("[DocumentHelper.GetPublished]: There must be 'DocumentPublishTo' column present in order to evaluate the published status of the document.");
            }

            // Check publishing times
            return (
                (DateTime.Now >= ValidationHelper.GetDateTime(dc.GetValue("DocumentPublishFrom"), DateTime.MinValue)) &&
                (DateTime.Now <= ValidationHelper.GetDateTime(dc.GetValue("DocumentPublishTo"), DateTime.MaxValue))
            );
        }


        /// <summary>
        /// Indicates if given object type represents document.
        /// </summary>
        /// <param name="objectType">Object type</param>
        public static bool IsDocumentObjectType(string objectType)
        {
            return TreeNodeProvider.IsDocumentObjectType(objectType);
        }


        /// <summary>
        /// Checks whether specified column exists in main document view.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public static bool ColumnExistsInDocumentView(string columnName)
        {
            var ds = new DataQuery().TopN(1)
                                    .From(SystemViewNames.View_CMS_Tree_Joined)
                                    .Result;

            return (ds.Tables.Count > 0) && ds.Tables[0].Columns.Contains(columnName);
        }


        /// <summary>
        /// Gets the new DataSet for the given node with specific class name.
        /// </summary>
        /// <param name="className">Class name</param>
        /// <param name="isCoupled">Is coupled class</param>
        /// <param name="hasSku">Has SKU information</param>
        public static DataSet GetTreeNodeDataSet(string className, bool isCoupled, bool hasSku)
        {
            // Build source table
            var data = new DataSet();
            var table = new DataTable(TranslationHelper.GetSafeClassName(className));
            data.Tables.Add(table);

            // Add the Tree class columns
            var treeInfo = ClassStructureInfo.GetClassInfo("CMS.Tree");
            int count = treeInfo.ColumnsCount;
            for (int i = 0; i < count; i++)
            {
                // Add the column
                var column = treeInfo.ColumnDefinitions[i];
                table.Columns.Add(new DataColumn(column.ColumnName, column.ColumnType));
            }

            // Add the document columns
            var docInfo = ClassStructureInfo.GetClassInfo("CMS.Document");
            count = docInfo.ColumnsCount;
            for (int i = 0; i < count; i++)
            {
                // Add the column
                var column = docInfo.ColumnDefinitions[i];
                table.Columns.Add(new DataColumn(column.ColumnName, column.ColumnType));
            }

            // Add the coupled class columns
            if (isCoupled)
            {
                if (className == null)
                {
                    throw new Exception("[ClassStructureInfo.GetTreeNodeDataSet]: Class name is not specified.");
                }

                var coupledInfo = ClassStructureInfo.GetClassInfo(className);
                count = coupledInfo.ColumnsCount;
                for (int i = 0; i < count; i++)
                {
                    // Add the column
                    var column = coupledInfo.ColumnDefinitions[i];
                    table.Columns.Add(new DataColumn(column.ColumnName, column.ColumnType));
                }
            }

            // Add the SKU class columns
            if (hasSku)
            {
                var skuInfo = ClassStructureInfo.GetClassInfo("ECommerce.SKU");
                count = skuInfo.ColumnsCount;
                for (int i = 0; i < count; i++)
                {
                    // Add the column
                    var column = skuInfo.ColumnDefinitions[i];
                    table.Columns.Add(new DataColumn(column.ColumnName, column.ColumnType));
                }
            }

            return data;
        }

        #endregion


        #region "Document search"

        /// <summary>
        /// Returns true if any field included in the search changed (checks fields defined in Class Search Settings).
        /// </summary>
        /// <param name="node">Document node</param>
        /// <param name="changedColumns">List of changed columns</param>
        public static bool SearchFieldChanged(TreeNode node, List<string> changedColumns)
        {
            // Check base class for documents
            if (SearchHelper.SearchFieldChanged(node, changedColumns, false))
            {
                return true;
            }

            // Check document type columns which are part of search
            DataClassInfo dc = DataClassInfoProviderBase<DataClassInfoProvider>.GetDataClassInfo(node.NodeClassName);
            return dc.SearchFieldChanged(changedColumns);
        }


        /// <summary>
        /// Returns true if the search task for given node should be created.
        /// Returns true if 
        ///  - search is allowed on general level AND 
        ///  - search is allowed for document type of given node
        ///  - published version for given node exists AND
        /// </summary>
        /// <param name="node">Node to check</param>
        public static bool IsSearchTaskCreationAllowed(TreeNode node)
        {
            // Check static settings
            return SearchHelper.IsSearchTaskCreationAllowed(node.NodeClassName) && node.PublishedVersionExists;
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Deletes ad-hoc page template used by the document.
        /// </summary>
        /// <param name="tree">Tree provider</param>
        /// <param name="templateId">Template ID</param>
        private static void DeleteAdhocPageTemplate(TreeProvider tree, int templateId)
        {
            if (templateId <= 0)
            {
                return;
            }

            // Get the page template
            var template = PageTemplateInfoProvider.GetPageTemplateInfo(templateId);
            if ((template == null) || template.IsReusable)
            {
                return;
            }

            // Do not log to the event log
            // Propagate tree provider settings
            using (new CMSActionContext { EnableLogContext = false, LogEvents = false, LogWebFarmTasks = tree.LogWebFarmTasks, TouchCacheDependencies = tree.TouchCacheDependencies, LogSynchronization = tree.LogSynchronization })
            {
                PageTemplateInfoProvider.DeletePageTemplate(template);
            }
        }


        /// <summary>
        /// Applies latest version to given document
        /// </summary>
        /// <param name="node">Document</param>
        /// <param name="tree">Tree provider</param>
        internal static void ApplyLatestVersion(TreeNode node, TreeProvider tree)
        {
            if (node == null)
            {
                return;
            }

            if (tree == null)
            {
                throw new NullReferenceException("[DocumentHelper.ApplyLatestVersion]: Tree provider instance not initialized.");
            }

            if (!node.IsComplete)
            {
                node.MakeComplete(true);
            }

            var versionId = node.DocumentCheckedOutVersionHistoryID;
            if (versionId > 0)
            {
                // Apply version data
                var version = VersionHistoryInfoProvider.GetVersionHistoryInfo(versionId);
                var manager = VersionManager.GetInstance(tree);
                manager.ApplyVersion(node, version);
            }

            // Make sure document acts like last edited version to prevent published data modification
            node.IsLastVersion = true;
        }


        /// <summary>
        /// Ensures paths for alternating document for the currently deleted document.
        /// </summary>
        /// <param name="deletedDocument">Deleted document</param>
        /// <param name="settings">Document deletion settings</param>
        /// <param name="lastCulture">Indicates whether document is last culture version.</param>
        private static void EnsureAlternatingDocument(TreeNode deletedDocument, DeleteDocumentSettings settings, bool lastCulture)
        {
            // Check whether alternating document is defined and exists published version
            if ((settings.AlternatingDocument == null) || ((settings.AlternatingDocumentMaxLevel >= 0) && (deletedDocument.NodeLevel > settings.AlternatingDocumentMaxLevel)) || !deletedDocument.PublishedVersionExists)
            {
                return;
            }

            // List of paths
            var paths = new List<string>();

            // NodeAliasPath, URLPath, Document aliases
            if (settings.AlternatingDocumentCopyAllPaths)
            {
                // Add NodeAliasPath and URLPath to the path collection
                paths.Add(deletedDocument.NodeAliasPath);
                if (!String.IsNullOrEmpty(deletedDocument.DocumentUrlPath))
                {
                    paths.Add(deletedDocument.DocumentUrlPath);
                }

                string culture = deletedDocument.DocumentCulture;
                // Move all aliases if all cultures should be deleted or current lang version is the last
                if (settings.DeleteAllCultures || lastCulture)
                {
                    culture = String.Empty;
                }

                // Move document aliases to the alternating document
                DocumentAliasInfoProvider.MoveAliases(deletedDocument.NodeID, settings.AlternatingDocument.NodeID, culture);
            }
            // URLPath or NodeAlias path
            else
            {
                // Use URL path for alternating document
                paths.Add(!String.IsNullOrEmpty(deletedDocument.DocumentUrlPath) ? deletedDocument.DocumentUrlPath : deletedDocument.NodeAliasPath);
            }

            // Loop thru path collection
            foreach (string path in paths)
            {
                // Check whether document alias is unique
                if (!DocumentAliasInfoProvider.IsUnique(path, 0, deletedDocument.DocumentCulture, deletedDocument.DocumentExtensions, deletedDocument.NodeSiteName, false, settings.AlternatingDocument.NodeID))
                {
                    continue;
                }

                // Create new document alias
                var alias = new DocumentAliasInfo
                {
                    AliasCulture = deletedDocument.DocumentCulture,
                    AliasExtensions = deletedDocument.DocumentExtensions,
                    AliasURLPath = path,
                    AliasNodeID = settings.AlternatingDocument.NodeID,
                    AliasSiteID = deletedDocument.NodeSiteID
                };

                DocumentAliasInfoProvider.SetDocumentAliasInfo(alias);
            }

            // Log synchronization task to update aliases
            LogDocumentChange(settings.AlternatingDocument, TaskTypeEnum.UpdateDocument, settings.AlternatingDocument.TreeProvider);
        }


        /// <summary>
        /// Deletes the child nodes of all culture versions under the specified parent node.
        /// </summary>
        /// <param name="settings">Settings used when deleting child nodes.</param>
        private static void DeleteDocumentChildNodes(DeleteDocumentSettings settings)
        {
            var node = settings.Node;
            var tree = settings.Tree;

            // Check parameters
            CheckParameters(node);

            // Ensure tree provider
            tree = EnsureTreeProvider(node, tree);

            foreach (var child in tree.EnumerateChildren(node))
            {
                // Skip already deleted children
                if (child == null)
                {
                    continue;
                }

                // Prepare settings for child node
                var childNodeSettings = new DeleteDocumentSettings(settings)
                {
                    Node = child,
                    Tree = tree,
                    DeleteAllCultures = true
                };

                // Delete the child document
                DeleteDocument(childNodeSettings);
            }
        }


        /// <summary>
        /// Deletes document linked documents
        /// </summary>
        /// <param name="settings">Delete settings</param>
        private static void DeleteDocumentLinks(DeleteDocumentSettings settings)
        {
            var node = settings.Node;
            var tree = settings.Tree;
            var parentLink = false;

            // Ensure tree provider
            tree = EnsureTreeProvider(node, tree);

            foreach (var link in tree.EnumerateLinks(node))
            {
                // Skip already deleted link
                if (link == null)
                {
                    continue;
                }

                var aliasPath = link.NodeAliasPath;
                var siteId = link.NodeSiteID;
                if (node.NodeAliasPath.StartsWith(aliasPath + "/", StringComparison.InvariantCultureIgnoreCase) && (node.NodeSiteID == siteId))
                {
                    parentLink = true;
                }
                else
                {
                    // Prepare settings for deleting of linked document
                    var linkedNodeSettings = new DeleteDocumentSettings(settings)
                    {
                        Node = link,
                        DeleteAllCultures = false
                    };

                    // Delete document
                    DeleteDocument(linkedNodeSettings);
                }
            }

            if (parentLink)
            {
                throw new Exception("[DocumentHelper.DeleteDocumentLinks]: Cannot delete the page '" + HTMLHelper.HTMLEncode(node.DocumentNamePath) + "' which is child of its own link.");
            }
        }


        /// <summary>
        /// Moves histories of document section to the target site
        /// </summary>
        /// <param name="nodeAliasPath">Node alias path of the moved document</param>
        /// <param name="targetSiteId">Target site ID</param>
        /// <param name="tree">Tree provider</param>
        private static void MoveHistories(string nodeAliasPath, int targetSiteId, TreeProvider tree)
        {
            // Get the moved document IDs
            var data = tree.SelectNodes()
                           .All()
                           .Columns("DocumentID")
                           .OnSite(targetSiteId)
                           .WhereNull("NodeLinkedNodeID")
                           .Path(nodeAliasPath, PathTypeEnum.Section)
                           .GetListResult<int>();

            foreach (int documentId in data)
            {
                // Move version histories
                VersionHistoryInfoProvider.MoveHistories(documentId, targetSiteId);

                // Move attachment histories
                AttachmentHistoryInfoProvider.MoveHistories(documentId, targetSiteId);
            }
        }


        /// <summary>
        /// Clears field attachments GUID values.
        /// </summary>
        private static bool ClearFieldAttachments(TreeNode node)
        {
            var ci = DataClassInfoProviderBase<DataClassInfoProvider>.GetDataClassInfo(node.NodeClassName);
            if (ci == null)
            {
                throw new DocumentTypeNotExistsException("Page type with '" + node.NodeClassName + "' class name not found.");
            }

            if (!ci.ClassIsCoupledClass)
            {
                return false;
            }

            var fi = FormHelper.GetFormInfo(ci.ClassName, false);
            if (fi == null)
            {
                return false;
            }

            // Get attachment fields with a value
            var attachmentFields = fi.GetFields(FieldDataType.File)
                                     .Select(f => f.Name)
                                     .Where(name => node.GetValue(name, Guid.Empty) != Guid.Empty)
                                     .ToList();

            // Clear values
            attachmentFields.ForEach(name => node.SetValue(name, null));

            return attachmentFields.Count > 0;
        }


        /// <summary>
        /// Checks method prerequisites
        /// </summary>
        /// <param name="node">Document node</param>
        private static void CheckParameters(TreeNode node)
        {
            // Check node
            if (node == null)
            {
                throw new NullReferenceException("[DocumentHelper.CheckParameters]: Missing document node.");
            }

            // Check node site
            if (String.IsNullOrEmpty(node.NodeSiteName))
            {
                throw new Exception("[DocumentHelper.CheckParameters]: Node site not found.");
            }
        }


        /// <summary>
        /// Checks method prerequisites
        /// </summary>
        /// <param name="node">Document node</param>
        /// <param name="parent">Parent document node</param>
        private static void CheckParameters(TreeNode node, TreeNode parent)
        {
            // Check node
            if (node == null)
            {
                throw new NullReferenceException("[DocumentHelper.CheckParameters]: Missing document node.");
            }

            if (!node.IsRoot())
            {
                // Check parent
                if (parent == null)
                {
                    throw new NullReferenceException("[DocumentHelper.CheckParameters]: Missing document parent node.");
                }

                // Check parent node site
                if (String.IsNullOrEmpty(parent.NodeSiteName))
                {
                    throw new Exception("[DocumentHelper.CheckParameters]: Parent node site not found.");
                }
            }
        }


        /// <summary>
        /// Ensures tree provider instance
        /// </summary>
        /// <param name="node">Document node</param>
        /// <param name="tree">Tree provider</param>
        private static TreeProvider EnsureTreeProvider(TreeNode node, TreeProvider tree)
        {
            // Ensure tree provider
            if (tree == null)
            {
                tree = new TreeProvider();
            }

            // Propagate tree provider
            node.TreeProvider = tree;

            return tree;
        }


        /// <summary>
        /// Returns existing attachment - if <paramref name="usesWorkflow"/> set,
        /// returns given <paramref name="versionHistoryId"/> of the specified attachment.
        /// </summary>
        /// <param name="attachmentGuid">Attachment GUID</param>
        /// <param name="usesWorkflow">Indicates if attachment under workflow should be returned.</param>
        /// <param name="versionHistoryId">Specifics history version</param>
        /// <param name="siteName">Name of the site attachment should belong to.</param>
        private static DocumentAttachment GetExistingAttachment(Guid attachmentGuid, bool usesWorkflow, int versionHistoryId, string siteName)
        {
            if (attachmentGuid == Guid.Empty)
            {
                return null;
            }

            if (usesWorkflow)
            {
                var latestAttachment = GetAttachment(attachmentGuid, versionHistoryId, false);
                if (latestAttachment != null)
                {
                    return latestAttachment;
                }
            }

            // If not found, try to get from published version
            return (DocumentAttachment)AttachmentInfoProvider.GetAttachmentInfoWithoutBinary(attachmentGuid, siteName);
        }


        /// <summary>
        /// Return name of site that is assigned to the node.
        /// </summary>
        private static string GetSiteName(TreeNode node)
        {
            SiteInfo si = SiteInfoProvider.GetSiteInfo(node.NodeSiteID);
            if (si == null)
            {
                throw new Exception("Site not found.");
            }
            string siteName = si.SiteName;
            return siteName;
        }


        /// <summary>
        /// Updates search index if search task for given node should be created.
        /// </summary>
        /// <param name="node">Node to create search task for</param>
        internal static void UpdateSearchIndexIfAllowed(TreeNode node)
        {
            if (IsSearchTaskCreationAllowed(node))
            {
                SearchTaskInfoProvider.CreateTask(SearchTaskTypeEnum.Update, TreeNode.OBJECT_TYPE, SearchFieldsConstants.ID, node.GetSearchID(), node.DocumentID);
            }
        }


        /// <summary>
        /// Updates node document extensions and document type if node is file.
        /// </summary>
        /// <param name="node">Node to update</param>
        /// <param name="guidColumnName">Column containing the Attachment GUID</param>
        /// <param name="attachment">Posted attachment</param>
        private static void SetDefaultExtension(TreeNode node, string guidColumnName, IAttachment attachment)
        {
            if (!node.IsFile() || guidColumnName == null || !guidColumnName.Equals("fileattachment", StringComparison.InvariantCultureIgnoreCase))
            {
                return;
            }

            // Update document extensions if no custom extensions are used
            if (!node.DocumentUseCustomExtensions)
            {
                node.DocumentExtensions = attachment.AttachmentExtension;
            }
            node.DocumentType = attachment.AttachmentExtension;
        }


        /// <summary>
        /// Saves attachment as <see cref="AttachmentInfo"/>.
        /// </summary>
        /// <remarks>Raises the <see cref="DocumentEvents.SaveAttachment"/> event for saving the attachment.</remarks>
        internal static void SaveAttachment(DocumentAttachment attachment, TreeNode node)
        {
            // Handle the event
            using (var handler = DocumentEvents.SaveAttachment.StartEvent(node, attachment, node.TreeProvider))
            {
                handler.DontSupportCancel();

                if (handler.CanContinue())
                {
                    // Save the attachment
                    var attachmentInfo = attachment.GetAttachmentInfo();
                    AttachmentInfoProvider.SetAttachmentInfo(attachmentInfo);
                    attachment.Load(attachmentInfo);

                }

                // Finalize the event
                handler.FinishEvent();
            }
        }


        private static void GenerateVariantsForAttachment(DocumentAttachment attachment, TreeNode node)
        {
            var context = new AttachmentVariantContext(node);
            attachment.GenerateAllVariants(context);
        }

        #endregion
    }
}