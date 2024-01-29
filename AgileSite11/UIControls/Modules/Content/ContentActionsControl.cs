using System;

using CMS.Base;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.SiteProvider;

namespace CMS.UIControls
{
    /// <summary>
    /// Control for handling global content actions.
    /// </summary>
    public abstract class ContentActionsControl : CMSUserControl
    {
        #region "Variables"

        private TreeProvider mTreeProvider;

        #endregion


        #region "Properties"

        /// <summary>
        /// Tree provider.
        /// </summary>
        public TreeProvider TreeProvider
        {
            get
            {
                return mTreeProvider ?? (mTreeProvider = new TreeProvider());
            }
        }


        /// <summary>
        /// Determines whether action is a dialog action.
        /// </summary>
        public bool IsDialogAction
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Processes the given action.
        /// </summary>
        /// <param name="node">Node to process</param>
        /// <param name="targetNode">Target node</param>
        /// <param name="action">Action to process</param>
        /// <param name="childNodes">Process also child nodes</param>
        /// <param name="throwExceptions">If true, the exceptions are thrown</param>
        /// <param name="copyPermissions">Indicates if permissions should be copied</param>
        protected TreeNode ProcessAction(TreeNode node, TreeNode targetNode, string action, bool childNodes, bool throwExceptions, bool copyPermissions)
        {
            int nodeId = node.NodeID;
            int targetId = targetNode.NodeID;

            action = action.ToLowerCSafe();
            bool first = action.EndsWithCSafe("first");

            // Perform the action
            switch (action)
            {
                case "linknodefirst":
                case "linknodeposition":
                    {
                        try
                        {
                            // Set the parent ID
                            int newPosition = 1;

                            if (!targetNode.IsRoot() && !first)
                            {
                                // Init the node orders in parent
                                TreeProvider.InitNodeOrders(targetNode.NodeParentID, targetNode.NodeSiteID);
                                targetNode = TreeProvider.SelectSingleNode(targetId);

                                // Get the target order
                                newPosition = targetNode.NodeOrder + 1;

                                // Get real parent node
                                int newTargetId = targetNode.NodeParentID;
                                targetNode = TreeProvider.SelectSingleNode(newTargetId);
                            }

                            // Link the node
                            TreeNode newNode = LinkNode(node, targetNode, TreeProvider, copyPermissions, childNodes);
                            if (newNode != null)
                            {
                                // Reposition the node
                                if (newPosition == 1)
                                {
                                    // First position
                                    TreeProvider.SetNodeOrder(newNode, DocumentOrderEnum.First);
                                }
                                else
                                {
                                    // After the target
                                    TreeProvider.SetNodeOrder(newNode, newPosition);
                                }
                            }

                            return newNode;
                        }
                        catch (Exception ex)
                        {
                            EventLogProvider.LogException("Content", "LINKDOC", ex);

                            AddError(GetString("ContentRequest.LinkFailed"));
                            if (throwExceptions)
                            {
                                throw;
                            }
                            return null;
                        }
                    }

                case "linknode":
                    {
                        try
                        {
                            // Link the node
                            TreeNode newNode = LinkNode(node, targetNode, TreeProvider, copyPermissions, childNodes);
                            if (newNode != null)
                            {
                                // Set default position
                                TreeProvider.SetNodeOrder(newNode, DocumentOrderEnum.First);
                            }
                            return newNode;
                        }
                        catch (Exception ex)
                        {
                            EventLogProvider.LogException("Content", "LINKDOC", ex);

                            AddError(GetString("ContentRequest.LinkFailed"));
                            if (throwExceptions)
                            {
                                throw;
                            }
                            return null;
                        }
                    }

                case "copynodefirst":
                case "copynodeposition":
                    {
                        try
                        {
                            // Set the parent ID
                            int newPosition = 1;

                            if (!targetNode.IsRoot() && !first)
                            {
                                // Init the node orders in parent
                                TreeProvider.InitNodeOrders(targetNode.NodeParentID, targetNode.NodeSiteID);
                                targetNode = TreeProvider.SelectSingleNode(targetId);

                                // Get the target order
                                newPosition = targetNode.NodeOrder + 1;

                                // Get real parent node
                                int newTargetId = targetNode.NodeParentID;
                                targetNode = TreeProvider.SelectSingleNode(newTargetId);
                            }

                            // Copy the node
                            TreeNode newNode = CopyNode(node, targetNode, childNodes, TreeProvider, copyPermissions);
                            if (newNode != null)
                            {
                                // Reposition the node
                                if (newPosition == 1)
                                {
                                    // First position
                                    TreeProvider.SetNodeOrder(newNode, DocumentOrderEnum.First);
                                }
                                else
                                {
                                    // After the target
                                    TreeProvider.SetNodeOrder(newNode, newPosition);
                                }

                                return newNode;
                            }
                        }
                        catch (Exception ex)
                        {
                            EventLogProvider.LogException("Content", "COPYDOC", ex);

                            AddError(GetString("ContentRequest.CopyFailed"));
                            if (throwExceptions)
                            {
                                throw;
                            }
                            return null;
                        }
                    }
                    break;

                case "copynode":
                    {
                        try
                        {
                            // Copy the node
                            TreeNode newNode = CopyNode(node, targetNode, childNodes, TreeProvider, copyPermissions);
                            if (newNode != null)
                            {
                                // Set default position
                                TreeProvider.SetNodeOrder(newNode, DocumentOrderEnum.First);
                            }
                            return newNode;
                        }
                        catch (Exception ex)
                        {
                            EventLogProvider.LogException("Content", "COPYDOC", ex);

                            AddError(GetString("ContentRequest.CopyFailed"));
                            if (throwExceptions)
                            {
                                throw;
                            }
                            return null;
                        }
                    }

                case "movenodefirst":
                case "movenodeposition":
                    {
                        try
                        {
                            // Check the permissions for document
                            if (CurrentUser.IsAuthorizedPerDocument(node, NodePermissionsEnum.Modify) == AuthorizationResultEnum.Allowed)
                            {
                                // Set the parent ID
                                int newPosition = 1;

                                if (!targetNode.IsRoot() && !first)
                                {
                                    // Init the node orders in parent
                                    TreeProvider.InitNodeOrders(targetNode.NodeParentID, targetNode.NodeSiteID);
                                    targetNode = TreeProvider.SelectSingleNode(targetId);

                                    // Get the target order
                                    newPosition = targetNode.NodeOrder;
                                    if ((node.NodeParentID != targetNode.NodeParentID) || (node.NodeOrder > targetNode.NodeOrder))
                                    {
                                        // For moving up in the same level or between levels, place node behind the target node
                                        newPosition += 1;
                                    }

                                    // Get real parent node
                                    int newTargetId = targetNode.NodeParentID;
                                    targetNode = TreeProvider.SelectSingleNode(newTargetId);
                                }

                                // Move the node under the correct parent
                                if (((node.NodeOrder != newPosition) || (node.NodeParentID != targetNode.NodeID)) && MoveNode(node, targetNode, TreeProvider, copyPermissions))
                                {
                                    if (targetNode.NodeID == nodeId)
                                    {
                                        return null;
                                    }

                                    // Reposition the node
                                    if (newPosition == 1)
                                    {
                                        // First position
                                        TreeProvider.SetNodeOrder(node, DocumentOrderEnum.First);
                                    }
                                    else
                                    {
                                        // After the target
                                        TreeProvider.SetNodeOrder(node, newPosition);
                                    }

                                    return node;
                                }
                            }
                            else
                            {
                                string encodedAliasPath = " (" + HTMLHelper.HTMLEncode(node.NodeAliasPath) + ")";
                                AddError(GetString("ContentRequest.NotAllowedToMove") + encodedAliasPath);
                            }
                        }
                        catch (Exception ex)
                        {
                            EventLogProvider.LogException("Content", "MOVEDOC", ex);

                            AddError(GetString("ContentRequest.MoveFailed"));
                            if (throwExceptions)
                            {
                                throw;
                            }
                            return null;
                        }
                    }
                    break;

                case "movenode":
                    {
                        try
                        {
                            // Move the node
                            if (MoveNode(node, targetNode, TreeProvider, copyPermissions))
                            {
                                // Set default position
                                TreeProvider.SetNodeOrder(node, DocumentOrderEnum.First);
                                return node;
                            }
                        }
                        catch (Exception ex)
                        {
                            EventLogProvider.LogException("Content", "MOVEDOC", ex);

                            AddError(GetString("ContentRequest.MoveFailed"));
                            if (throwExceptions)
                            {
                                throw;
                            }
                            return null;
                        }
                    }
                    break;
            }

            return null;
        }


        /// <summary>
        /// Links the node to the specified target.
        /// </summary>
        /// <param name="node">Node to move</param>
        /// <param name="targetNode">Target node</param>
        /// <param name="tree">Tree provider</param>
        /// <param name="copyPermissions">Indicates if node permissions should be copied</param>
        /// <param name="childNodes">Indicates whether to link also child nodes</param>
        protected TreeNode LinkNode(TreeNode node, TreeNode targetNode, TreeProvider tree, bool copyPermissions, bool childNodes)
        {
            string encodedAliasPath = " (" + HTMLHelper.HTMLEncode(node.NodeAliasPath) + ")";

            // Check create permission
            if (!IsUserAuthorizedToCopyOrLink(node, targetNode, node.NodeClassName))
            {
                AddError(GetString("ContentRequest.NotAllowedToLink") + encodedAliasPath);
                return null;
            }

            // Check allowed child class
            int nodeClassId = ValidationHelper.GetInteger(node.GetValue("NodeClassID"), 0);
            if (!DocumentHelper.IsDocumentTypeAllowed(targetNode, nodeClassId) || (ClassSiteInfoProvider.GetClassSiteInfo(nodeClassId, targetNode.NodeSiteID) == null))
            {
                AddError(String.Format(GetString("ContentRequest.ErrorDocumentTypeNotAllowed"), node.NodeAliasPath, GetClassDisplayName(node.NodeClassName)));
                return null;
            }

            // Determine whether any child nodes are present
            bool includeChildNodes = node.NodeHasChildren && childNodes;

            // Document can't be copied under itself if child nodes are present
            if ((node.NodeID == targetNode.NodeID) && includeChildNodes)
            {
                AddError(GetString("ContentRequest.CannotLinkToItself") + encodedAliasPath);
                return null;
            }

            string domainToCheck;
            if (targetNode.NodeSiteID == node.NodeSiteID)
            {
                domainToCheck = RequestContext.CurrentDomain;
            }
            else
            {
                SiteInfo targetSite = SiteInfoProvider.GetSiteInfo(targetNode.NodeSiteID);
                domainToCheck = targetSite.DomainName;
            }

            // Check the license limitations
            if ((node.NodeClassName.ToLowerCSafe() == "cms.blog") && !LicenseHelper.LicenseVersionCheck(domainToCheck, FeatureEnum.Blogs, ObjectActionEnum.Insert))
            {
                AddError(GetString("cmsdesk.bloglicenselimits"));
                return null;
            }

            // Check license limitations related to child documents
            if (childNodes && !CheckBlogLicenseLimitations(node, domainToCheck))
            {
                AddError(GetString("ContentRequest.BlogLicenseLimitations"));
                return null;
            }

            // Check cyclic linking (linking of the node to some of its child nodes)
            if ((targetNode.NodeSiteID == node.NodeSiteID) && (targetNode.NodeAliasPath.TrimEnd('/') + "/").StartsWithCSafe(node.NodeAliasPath + "/", true) && includeChildNodes)
            {
                AddError(GetString("ContentRequest.CannotLinkToChild"));
                return null;
            }

            if (NodeHasScopePreventsLinksCreation(targetNode))
            {
                AddError(GetString("contentrequest.cantcreatelink"));
                return null;
            }

            AddLog(HTMLHelper.HTMLEncode(node.NodeAliasPath));

            DocumentHelper.InsertDocumentAsLink(node, targetNode, tree, includeChildNodes, copyPermissions);

            SetExpandedNode(node.NodeParentID);
            return node;
        }


        /// <summary>
        /// Copies the node to the specified target.
        /// </summary>
        /// <param name="node">Node to copy</param>
        /// <param name="targetNode">Target node</param>
        /// <param name="tree">Tree provider</param>
        /// <param name="childNodes">Copy also child nodes</param>
        /// <param name="copyPermissions">Indicates if node permissions should be copied</param>
        /// <param name="newDocumentName">New document name</param>
        protected TreeNode CopyNode(TreeNode node, TreeNode targetNode, bool childNodes, TreeProvider tree, bool copyPermissions, string newDocumentName = null)
        {
            string encodedAliasPath = " (" + HTMLHelper.HTMLEncode(node.NodeAliasPath) + ")";

            // Do not copy child nodes in case of no child nodes
            childNodes = childNodes && node.NodeHasChildren;

            // Get the document to copy
            int nodeId = node.NodeID;
            if ((nodeId == targetNode.NodeID) && childNodes)
            {
                AddError(GetString("ContentRequest.CannotCopyToItself") + encodedAliasPath);
                return null;
            }

            // Check move permission
            if (!IsUserAuthorizedToCopyOrLink(node, targetNode, node.NodeClassName))
            {
                AddError(GetString("ContentRequest.NotAllowedToCopy") + encodedAliasPath);
                return null;
            }

            // Check cyclic copying (copying of the node to some of its child nodes)
            if (childNodes && (targetNode.NodeSiteID == node.NodeSiteID) && targetNode.NodeAliasPath.StartsWithCSafe(node.NodeAliasPath + "/", true))
            {
                AddError(GetString("ContentRequest.CannotCopyToChild"));
                return null;
            }

            string domainToCheck;
            if (targetNode.NodeSiteID == node.NodeSiteID)
            {
                domainToCheck = RequestContext.CurrentDomain;
            }
            else
            {
                SiteInfo targetSite = SiteInfoProvider.GetSiteInfo(targetNode.NodeSiteID);
                domainToCheck = targetSite.DomainName;
            }

            // Check the license limitations for current document
            if ((node.NodeClassName.ToLowerCSafe() == "cms.blog") && !LicenseHelper.LicenseVersionCheck(domainToCheck, FeatureEnum.Blogs, ObjectActionEnum.Insert))
            {
                AddError(GetString("cmsdesk.bloglicenselimits"));
                return null;
            }

            // Check license limitations related to child documents
            if (childNodes && !CheckBlogLicenseLimitations(node, domainToCheck))
            {
                AddError(GetString("ContentRequest.BlogLicenseLimitations"));
                return null;
            }

            // Check allowed child class
            int nodeClassId = node.GetValue("NodeClassID", 0);
            if (!DocumentHelper.IsDocumentTypeAllowed(targetNode, nodeClassId) || (ClassSiteInfoProvider.GetClassSiteInfo(nodeClassId, targetNode.NodeSiteID) == null))
            {
                AddError(String.Format(GetString("ContentRequest.ErrorDocumentTypeNotAllowed"), node.NodeAliasPath, GetClassDisplayName(node.NodeClassName)));
                return null;
            }

            // Copy the document
            AddLog(HTMLHelper.HTMLEncode(node.NodeAliasPath + " (" + node.DocumentCulture + ")"));

            var settings = new CopyDocumentSettings(node, targetNode, tree)
            {
                IncludeChildNodes = childNodes,
                CopyPermissions = copyPermissions,
                NewDocumentName = newDocumentName,
                CheckSiteCulture = true,
                CloneSKU = true
            };
            node = DocumentHelper.CopyDocument(settings);

            if (node == null)
            {
                AddError(GetString("ContentRequest.AllCultureVersionsSkipped"));
                return null;
            }

            SetExpandedNode(node.NodeParentID);

            return node;
        }


        /// <summary>
        /// Moves the node to the specified target.
        /// </summary>
        /// <param name="node">Node to move</param>
        /// <param name="targetNode">Target node</param>
        /// <param name="tree">Tree provider</param>
        /// <param name="preservePermissions">Indicates if node permissions should be preserved</param>
        /// <returns>Whether to set new order</returns>
        protected bool MoveNode(TreeNode node, TreeNode targetNode, TreeProvider tree, bool preservePermissions)
        {
            string encodedAliasPath = " (" + HTMLHelper.HTMLEncode(node.NodeAliasPath) + ")";

            // If node parent ID is already the target ID, do not move it
            if (targetNode.NodeID == node.NodeParentID)
            {
                if (IsDialogAction)
                {
                    // Impossible to move document to same location
                    AddError(GetString("contentrequest.cannotmovetosameloc") + encodedAliasPath);
                }
                return true;
            }

            // Check move permission
            if (!IsUserAuthorizedToMove(node, targetNode, node.NodeClassName))
            {
                AddError(GetString("ContentRequest.NotAllowedToMove") + encodedAliasPath);
                return false;
            }

            // Get the document to copy
            int nodeId = node.NodeID;
            if (nodeId == targetNode.NodeID)
            {
                AddError(GetString("ContentRequest.CannotMoveToItself") + encodedAliasPath);
                return false;
            }

            // Check cyclic movement (movement of the node to some of its child nodes)
            if ((targetNode.NodeSiteID == node.NodeSiteID) && targetNode.NodeAliasPath.StartsWithCSafe(node.NodeAliasPath + "/", true))
            {
                AddError(GetString("ContentRequest.CannotMoveToChild"));
                return false;
            }

            if (targetNode.NodeSiteID != node.NodeSiteID)
            {
                SiteInfo targetSite = SiteInfoProvider.GetSiteInfo(targetNode.NodeSiteID);
                string domainToCheck = targetSite.DomainName;

                // Check the license limitations
                if ((node.NodeClassName.ToLowerCSafe() == "cms.blog") && !LicenseHelper.LicenseVersionCheck(domainToCheck, FeatureEnum.Blogs, ObjectActionEnum.Insert))
                {
                    AddError(GetString("cmsdesk.bloglicenselimits"));
                    return false;
                }

                // Check license limitations related to child documents
                if (!CheckBlogLicenseLimitations(node, domainToCheck))
                {
                    AddError(GetString("ContentRequest.BlogLicenseLimitations"));
                    return false;
                }
            }

            // Check allowed child classes
            int nodeClassId = node.GetValue("NodeClassID", 0);
            if (!DocumentHelper.IsDocumentTypeAllowed(targetNode, nodeClassId) || (ClassSiteInfoProvider.GetClassSiteInfo(nodeClassId, targetNode.NodeSiteID) == null))
            {
                AddError(String.Format(GetString("ContentRequest.ErrorDocumentTypeNotAllowed"), node.NodeAliasPath, GetClassDisplayName(node.NodeClassName)));
                return false;
            }

            // Move the document
            AddLog(string.Format(ResHelper.GetString("content.ui.pageallsubpages"), HTMLHelper.HTMLEncode(node.NodeAliasPath + " (" + node.DocumentCulture + ")")));

            DocumentHelper.MoveDocument(node, targetNode, tree, preservePermissions);
            SetExpandedNode(node.NodeParentID);

            return true;
        }


        /// <summary>
        /// Gets user friendly document type identifier.
        /// </summary>
        /// <param name="className">Document type class name</param>
        private string GetClassDisplayName(string className)
        {
            var classInfo = DataClassInfoProvider.GetDataClassInfo(className);
            if (classInfo != null)
            {
                return ResHelper.LocalizeString(classInfo.ClassDisplayName);
            }

            return className;
        }


        private bool NodeHasScopePreventsLinksCreation(TreeNode treeNode)
        {
            var scopeInfo = DocumentTypeScopeInfoProvider.GetScopeInfo(treeNode);

            if (scopeInfo != null)
            {
                return !scopeInfo.ScopeAllowLinks;
            }
            
            return false;
        }

        #endregion


        #region "Virtual members"

        /// <summary>
        /// Handles error message.
        /// </summary>
        /// <param name="errorMessage">Message</param>
        protected virtual void AddError(string errorMessage)
        {
        }


        /// <summary>
        /// Adds the log information.
        /// </summary>
        /// <param name="newLog">New log information</param>
        protected virtual void AddLog(string newLog)
        {
        }


        /// <summary>
        /// Sets the expanded node ID.
        /// </summary>
        /// <param name="nodeId">Node ID to set</param>
        protected virtual void SetExpandedNode(int nodeId)
        {
        }

        #endregion


        #region "Permission checking"

        /// <summary>
        /// Determines whether current user is authorized to move document.
        /// </summary>
        /// <param name="sourceNode">Source node</param>
        /// <param name="targetNode">Target node</param>
        /// <param name="sourceNodeClassName">Source node class name</param>
        /// <returns>True if authorized</returns>
        protected bool IsUserAuthorizedToMove(TreeNode sourceNode, TreeNode targetNode, string sourceNodeClassName)
        {
            bool isAuthorized = false;

            // Check 'modify permission' to source document
            if (CurrentUser.IsAuthorizedPerDocument(sourceNode, NodePermissionsEnum.Modify) == AuthorizationResultEnum.Allowed)
            {
                // Check 'create permission'
                if (CurrentUser.IsAuthorizedToCreateNewDocument(targetNode, sourceNodeClassName))
                {
                    isAuthorized = true;
                }
            }

            return isAuthorized;
        }


        /// <summary>
        /// Determines whether current user is authorized to copy document.
        /// </summary>
        /// <param name="sourceNode">Source node</param>
        /// <param name="targetNode">Target node</param>
        /// <param name="sourceNodeClassName">Source node class name</param>
        /// <returns>True if authorized</returns>
        protected bool IsUserAuthorizedToCopyOrLink(TreeNode sourceNode, TreeNode targetNode, string sourceNodeClassName)
        {
            bool isAuthorized = false;

            // Check 'read permission' to source document
            if (CurrentUser.IsAuthorizedPerDocument(sourceNode, NodePermissionsEnum.Read) == AuthorizationResultEnum.Allowed)
            {
                // Check 'create permission'
                if (CurrentUser.IsAuthorizedToCreateNewDocument(targetNode, sourceNodeClassName))
                {
                    isAuthorized = true;
                }
            }

            return isAuthorized;
        }

        #endregion


        #region "License checking"

        /// <summary>
        /// Checks license limitations for blogs module
        /// </summary>
        /// <param name="node">Document</param>
        /// <param name="domainToCheck">Domain to check the license for</param>
        private bool CheckBlogLicenseLimitations(TreeNode node, string domainToCheck)
        {
            var blog = DataClassInfoProvider.GetDataClassInfo("cms.blog");
            if (blog == null)
            {
                return true;
            }

            // If there is a license limitation
            var limitations = LicenseKeyInfoProvider.VersionLimitations(domainToCheck, FeatureEnum.Blogs, false);
            if (limitations <= 0)
            {
                return true;
            }

            // Get number of current blogs
            var allData = TreeProvider.SelectNodes()
                                      .All()
                                      .Column(new CountColumn().As("Count"))
                                      .WhereEquals("NodeClassID", blog.ClassID)
                                      .OnSite(LicenseHelper.GetSiteIDbyDomain(domainToCheck));
            var count = DataHelper.DataSourceIsEmpty(allData) ? 0 : ValidationHelper.GetInteger(allData.Tables[0].Rows[0]["Count"], 0);

            // Get number of new blogs
            var newData = TreeProvider.SelectNodes()
                                       .All()
                                       .Column(new CountColumn().As("Count"))
                                       .WhereEquals("NodeClassID", blog.ClassID)
                                       .OnSite(node.NodeSiteID)
                                       .Path(node.NodeAliasPath, PathTypeEnum.Children);
            var newCount = DataHelper.DataSourceIsEmpty(newData) ? 0 : ValidationHelper.GetInteger(newData.Tables[0].Rows[0]["Count"], 0);

            // Check the numbers
            var finalCount = count + newCount;
            if (finalCount <= limitations)
            {
                return true;
            }

            return false;
        }

        #endregion
    }
}