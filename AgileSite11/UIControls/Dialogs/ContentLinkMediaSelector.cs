using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.SiteProvider;

namespace CMS.UIControls
{
    /// <summary>
    /// Link media selector base class for content module.
    /// </summary>
    public abstract class ContentLinkMediaSelector : LinkMediaSelector
    {
        #region "Constants"

        /// <summary>
        /// List of columns to retrieve for nodes
        /// </summary>
        protected const string NODE_COLUMNS = "ClassDisplayName, NodeSiteID, NodeGUID, DocumentUrlPath, NodeAlias, NodeAliasPath, DocumentName, NodeClassID, DocumentModifiedWhen, NodeACLID, NodeHasChildren, DocumentCheckedOutVersionHistoryID, NodeOwner, DocumentExtensions, ClassName, DocumentType, NodeID, DocumentID, NodeIsContentOnly";

        #endregion


        #region "Properties"

        /// <summary>
        /// Indicates whether the attachments are temporary.
        /// </summary>
        protected bool AttachmentsAreTemporary
        {
            get
            {
                return ((Config.AttachmentFormGUID != Guid.Empty) && (Config.AttachmentDocumentID == 0));
            }
        }


        /// <summary>
        /// Gets or sets GUID of the recently selected attachment.
        /// </summary>
        protected Guid LastAttachmentGuid
        {
            get
            {
                return ValidationHelper.GetGuid(ViewState["LastAttachmentGuid"], Guid.Empty);
            }
            set
            {
                ViewState["LastAttachmentGuid"] = value;
            }
        }


        /// <summary>
        /// Currently selected item.
        /// </summary>
        protected DocumentAttachment CurrentAttachmentInfo
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets ID of the node reflecting new root specified by starting path.
        /// </summary>
        protected int StartingPathNodeID
        {
            get
            {
                return ValidationHelper.GetInteger(ViewState["StartingPathNodeID"], 0);
            }
            set
            {
                ViewState["StartingPathNodeID"] = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns path of the node specified by its ID.
        /// </summary>
        /// <param name="nodeId">ID of the node</param>
        protected static string GetContentPath(int nodeId)
        {
            if (nodeId > 0)
            {
                TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

                // Get node and return its alias path
                using (TreeNode node = tree.SelectSingleNode(nodeId))
                {
                    if (node != null)
                    {
                        return (!node.NodeHasChildren ? TreePathUtils.GetParentPath(node.NodeAliasPath) : node.NodeAliasPath).ToLowerCSafe();
                    }
                }
            }

            return string.Empty;
        }


        /// <summary>
        /// Returns ID of the content node specified by its alias path.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="aliasPath">Alias path of the node</param>
        protected int GetContentNodeId(string siteName, string aliasPath)
        {
            if (!string.IsNullOrEmpty(aliasPath))
            {
                TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);
                using (TreeNode node = tree.SelectSingleNode(siteName, aliasPath, Config.Culture))
                {
                    if (node != null)
                    {
                        // Return node's ID
                        return node.NodeID;
                    }
                }
            }

            return 0;
        }


        /// <summary>
        /// Gets the document by node ID
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="nodeId">Node ID</param>
        protected TreeNode GetDocument(string siteName, int nodeId)
        {
            TreeNode node = null;

            // Content tab
            if (SourceType == MediaSourceEnum.Content)
            {
                if (nodeId <= 0)
                {
                    return null;
                }

                var className = GetNodeClassName(nodeId);
                var query = GetQuery(siteName, className, nodeId);

                node = className.EqualsCSafe("cms.file", true) ? GetFileNode(query) : GetNode(query);
            }
            // Attachments tab
            else if (!AttachmentsAreTemporary)
            {
                TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);
                node = DocumentHelper.GetDocument(Config.AttachmentDocumentID, tree);
            }

            return node;
        }
        

        private DocumentQuery GetQuery(string siteName, string className, int nodeId)
        {
            return DocumentHelper.GetDocuments(className)
                                 .OnSite(siteName)
                                 .WhereEquals("NodeID", nodeId)
                                 .Culture(Config.Culture);
        }


        private static TreeNode GetNode(DocumentQuery query)
        {
            return query
                .CombineWithAnyCulture()
                .FirstOrDefault();
        }

        private static TreeNode GetFileNode(DocumentQuery query)
        {
            var combineFilesWithDefaultCulture = SiteInfoProvider.CombineFilesWithDefaultCulture(query.Properties.SiteName);
            return query
                .CombineWithDefaultCulture(combineFilesWithDefaultCulture)
                .FirstOrDefault();
        }


        private string GetNodeClassName(int nodeId)
        {
            var tree = new TreeProvider();
            return tree.SelectNodes()
                       .All()
                       .Columns("ClassName")
                       .WhereEquals("NodeID", nodeId)
                       .TopN(1)
                       .GetScalarResult<string>();
        }


        /// <summary>
        /// Deletes the attachment
        /// </summary>
        /// <param name="node">Document</param>
        /// <param name="attachmentGuid">Attachment GUID</param>
        protected void DeleteAttachment(TreeNode node, Guid attachmentGuid)
        {
            if (!AttachmentsAreTemporary)
            {
                var workflow = node.GetWorkflow();
                var useAutomaticCheckInOut = (workflow != null) && !workflow.UseCheckInCheckOut(node.NodeSiteName);

                PerformAttachmentAction(node, useAutomaticCheckInOut, () => DocumentHelper.DeleteAttachment(node, attachmentGuid));
            }
            else
            {
                // Delete temporary attachment
                AttachmentInfoProvider.DeleteTemporaryAttachment(attachmentGuid, SiteContext.CurrentSiteName);
            }
        }



        /// <summary>
        /// Gets the corresponding file data for the given item data
        /// </summary>
        /// <param name="data">Item data</param>
        protected IDataContainer GetFileData(IDataContainer data)
        {
            // Get the attachment data
            var documentId = ValidationHelper.GetInteger(data.GetValue("DocumentID"), 0);
            var versionHistoryId = ValidationHelper.GetInteger(data.GetValue("DocumentCheckedOutVersionHistoryID"), 0);

            var fileData = DocumentHelper.GetPrimaryAttachmentsForDocuments(
                new List<int>
                {
                    documentId
                },
                !IsLiveSite,
                new List<int>
                {
                    versionHistoryId
                }
                )[documentId];

            return fileData;
        }


        /// <summary>
        /// Gets the complete tree node with coupled data.
        /// </summary>
        /// <param name="node">Document</param>
        /// <param name="siteName">Site name</param>
        protected TreeNode GetNodeDetails(TreeNode node, string siteName)
        {
            var nodeQuery = DocumentHelper.GetDocuments()
                                          .WhereEquals("NodeGUID", node.NodeGUID)
                                          .OnSite(siteName)
                                          .Culture(Config.Culture)
                                          .CombineWithAnyCulture()
                                          .Type(node.ClassName)
                                          .WithCoupledColumns();

            return (IsLiveSite) ? nodeQuery.PublishedVersion().Published().FirstOrDefault() :
                                  nodeQuery.FirstOrDefault();
        }


        /// <summary>
        /// Checks attachment permissions.
        /// </summary>
        protected string CheckAttachmentPermissions()
        {
            string message = "";

            // For new document
            if (Config.AttachmentFormGUID != Guid.Empty)
            {
                if (Config.AttachmentParentID == 0)
                {
                    message = "Node parent node ID has to be set.";
                }

                if (!RaiseOnCheckPermissions("Create", this))
                {
                    if (!MembershipContext.AuthenticatedUser.IsAuthorizedToCreateNewDocument(Config.AttachmentParentID, SystemDocumentTypes.File))
                    {
                        message = GetString("attach.actiondenied");
                    }
                }
            }
            // For existing document
            else if (Config.AttachmentDocumentID > 0)
            {
                TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);
                // Get document node
                using (TreeNode node = DocumentHelper.GetDocument(Config.AttachmentDocumentID, tree))
                {
                    if (node == null)
                    {
                        message = "Given page doesn't exist!";
                    }
                    if (!RaiseOnCheckPermissions("Modify", this))
                    {
                        if (MembershipContext.AuthenticatedUser.IsAuthorizedPerDocument(node, NodePermissionsEnum.Modify) != AuthorizationResultEnum.Allowed)
                        {
                            message = GetString("attach.actiondenied");
                        }
                    }
                }
            }

            return message;
        }


        /// <summary>
        /// Returns path of the node specified by its ID.
        /// </summary>
        /// <param name="nodeId">ID of the node</param>
        protected static int GetParentNodeID(int nodeId)
        {
            if (nodeId > 0)
            {
                TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);
                // Get node and return its alias path
                using (TreeNode node = tree.SelectSingleNode(nodeId))
                {
                    if (node != null)
                    {
                        return node.NodeParentID;
                    }
                }
            }

            return 0;
        }


        /// <summary>
        /// Performs the attachment action for the node under workflow.
        /// </summary>
        /// <param name="node">Node under which should be performed given action</param>
        /// <param name="useAutomaticCheckInOut">Indicates if automatic check-in and check-out should be used</param>
        /// <param name="action">Action to perform</param>
        protected void PerformAttachmentAction(TreeNode node, bool useAutomaticCheckInOut, Action action)
        {
            VersionManager versionManager = null;
            bool checkin = false;

            if (useAutomaticCheckInOut)
            {
                // Check out the document
                TreeProvider tree = node.TreeProvider;

                versionManager = VersionManager.GetInstance(tree);
                var step = versionManager.CheckOut(node, node.IsPublished, true);

                // Do not check-in document if not under a workflow anymore
                checkin = (step != null);
            }

            // Perform action
            if (action != null)
            {
                action();
            }

            // Check in the document
            if (useAutomaticCheckInOut && checkin && (node.DocumentWorkflowStepID != 0))
            {
                versionManager.CheckIn(node, null);
            }
        }


        /// <summary>
        /// Performs necessary permissions check for the given document
        /// </summary>
        /// <param name="node">Document</param>
        protected void CheckPermissions(TreeNode node)
        {
            // Check 'READ' permission for the specific document if attachments are being created
            if ((SourceType == MediaSourceEnum.DocumentAttachments) && (!AttachmentsAreTemporary))
            {
                if (MembershipContext.AuthenticatedUser.IsAuthorizedPerDocument(node, NodePermissionsEnum.Read) != AuthorizationResultEnum.Allowed)
                {
                    string errMsg = string.Format(GetString("cmsdesk.notauthorizedtoreaddocument"), node.GetDocumentName());

                    // Redirect to access denied page
                    AccessDenied(errMsg);
                }
            }
        }


        /// <summary>
        /// Gets WHERE condition for available sites according field configuration.
        /// </summary>
        protected string GetSiteWhere()
        {
            // First check configuration
            WhereCondition condition = new WhereCondition();

            if (!IsCopyMoveLinkDialog)
            {
                condition.WhereEquals("SiteStatus", SiteStatusEnum.Running.ToStringRepresentation());
            }

            switch (Config.ContentSites)
            {
                case AvailableSitesEnum.OnlySingleSite:
                    condition.WhereEquals("SiteName", Config.ContentSelectedSite);
                    break;

                case AvailableSitesEnum.OnlyCurrentSite:
                    condition.WhereEquals("SiteName", SiteContext.CurrentSiteName);
                    break;
            }

            // Get only current user's sites
            if (!MembershipContext.AuthenticatedUser.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin))
            {
                condition.WhereIn("SiteID", new IDQuery<UserSiteInfo>(UserSiteInfo.TYPEINFO.SiteIDColumn)
                    .WhereEquals("UserID", MembershipContext.AuthenticatedUser.UserID));
            }
            return condition.ToString(true);
        }


        /// <summary>
        /// Ensures content tree is refreshed when new folder is created in Copy/Move dialog.
        /// </summary>
        protected void RefreshContentTree()
        {
            ScriptHelper.RegisterWOpenerScript(Page);
            // Refresh content tree
            ScriptHelper.RegisterStartupScript(Page, typeof(Page), "RefreshContentTree", ScriptHelper.GetScript(@"
if (wopener == null) {
    wopener = opener;
}              
if (wopener.parent != null) {
    if (wopener.parent != null) {
        if (wopener.parent.RefreshTree != null) {
            wopener.parent.RefreshTree();
        }
    }
}"));
        }

        #endregion
    }
}
