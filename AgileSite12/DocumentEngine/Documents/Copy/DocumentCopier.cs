using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Base;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.Search;
using CMS.SiteProvider;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Copies published data of a tree node.
    /// </summary>
    internal class DocumentCopier
    {
        protected CopyDocumentSettings Settings
        {
            get;
            set;
        }


        private TreeNode TargetDocument
        {
            get
            {
                return Settings.TargetNode;
            }
        }


        private TreeNode SourceDocument
        {
            get
            {
                return Settings.Node;
            }
        }


        private bool CrossSite
        {
            get;
            set;
        }


        /// <summary>
        /// Collection of original attachment GUID as key and copied attachment GUID as value.
        /// </summary>
        protected IDictionary<Guid, Guid> AttachmentGUIDs
        {
            get;
            set;
        }


        private readonly IDictionary<int, int> processedTemplates = new Dictionary<int, int>();


        /// <summary>
        /// Creates instance of <see cref="DocumentCopier"/>.
        /// </summary>
        /// <param name="settings">Copy document settings.</param>
        public DocumentCopier(CopyDocumentSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            Settings = settings;

            if (SourceDocument == null)
            {
                throw new NullReferenceException("Missing node to copy.");
            }

            if (SourceDocument.IsRoot())
            {
                throw new NotSupportedException("Root node cannot be copied.");
            }

            if (TargetDocument == null)
            {
                throw new NullReferenceException("Missing target node.");
            }

            CrossSite = SourceDocument.NodeSiteID != TargetDocument.NodeSiteID;

            if (Settings.IncludeChildNodes)
            {
                if (SourceDocument.NodeID == TargetDocument.NodeID)
                {
                    throw new NotSupportedException("Cannot copy node to itself.");
                }

                // Check cyclic copying (copying of the node to some of its child nodes)
                if (!CrossSite && TargetDocument.NodeAliasPath.StartsWithCSafe(SourceDocument.NodeAliasPath.TrimEnd('/') + "/", true))
                {
                    throw new NotSupportedException("Cannot copy node to it's child node.");
                }
            }
        }


        /// <summary>
        /// Copies document.
        /// </summary>
        /// <remarks>
        /// Priority of returned culture version of copied document is as follows:
        /// 1. Default culture version of the target site if provided
        /// 2. Default culture version of document site
        /// 3. Document culture
        /// </remarks>
        /// <returns>Returns first copied culture version of the source document.</returns>
        public TreeNode Copy()
        {
            TreeNode firstCopiedCultureNode = null;

            // Do not update document aliases for copy
            using (new DocumentActionContext { GenerateDocumentAliases = false })
            {
                using (var h = DocumentEvents.Copy.StartEvent(SourceDocument, TargetDocument, Settings.IncludeChildNodes, Settings.Tree))
                {
                    h.DontSupportCancel();

                    if (h.CanContinue())
                    {
                        firstCopiedCultureNode = SourceDocument.IsLink ? CopyLink() : CopyCultureVersions();
                        if (firstCopiedCultureNode != null)
                        {
                            if (Settings.IncludeChildNodes)
                            {
                                CopyChildNodes(SourceDocument, firstCopiedCultureNode);
                            }

                            LogToEventLog();
                            LogSearchTask(firstCopiedCultureNode);
                        }
                    }

                    h.FinishEvent();
                }
            }

            return firstCopiedCultureNode;
        }


        private void LogSearchTask(TreeNode copiedNode)
        {
            if (SearchIndexInfoProvider.SearchEnabled && !Settings.ProcessingChildNodes)
            {
                SearchTaskInfoProvider.CreateTask(SearchTaskTypeEnum.Update, TreeNode.OBJECT_TYPE, SearchFieldsConstants.PARTIAL_REBUILD, copiedNode.NodeSiteName + ";" + copiedNode.NodeAliasPath, copiedNode.DocumentID);
            }
        }


        private void LogToEventLog()
        {
            new DocumentEventLogger(SourceDocument).Log("COPYDOC", ResHelper.GetString("tasktitle.copydocument"), false);
        }


        private TreeNode CopyCultureVersions()
        {
            TreeNode firstCopiedCultureNode = null;
            foreach (var sourceCultureNode in Settings.Tree.EnumerateCultureVersions(SourceDocument, TargetDocument.NodeSiteName))
            {
                if (!CultureIsAllowed(sourceCultureNode))
                {
                    continue;
                }

                var copied = CopyCultureVersion(sourceCultureNode, firstCopiedCultureNode);
                if (firstCopiedCultureNode == null)
                {
                    firstCopiedCultureNode = copied;
                }
            }

            return firstCopiedCultureNode;
        }


        protected virtual TreeNode CopyLink()
        {
            var link = SourceDocument.Clone();
            link.InsertAsLink(TargetDocument, Settings.NewDocumentsOwner, Settings.NewDocumentsGroup, false);
            CopyNodePermissions(link);

            return link;
        }


        private void CopyChildNodes(TreeNode sourceNode, TreeNode targetNode)
        {
            var hasChildren = false;
            foreach (var child in Settings.Tree.EnumerateChildren(sourceNode, targetNode.NodeSiteName))
            {
                int nodeClassId = child.GetIntegerValue("NodeClassID", 0);
                if (IsPageTypeAllowedOnSite(nodeClassId, targetNode.NodeSiteID))
                {
                    LogContext.AppendLine(child.NodeAliasPath + " (" + child.DocumentCulture + ")", DocumentHelper.LOGCONTEXT_DOCUMENTS);

                    // Keep order and permission settings of child documents
                    using (new DocumentActionContext { UseAutomaticOrdering = false, PreserveACLHierarchy = true })
                    {
                        var childSettings = GetChildSettings(child, targetNode);
                        CopyNode(childSettings);
                    }

                    hasChildren = true;
                }
                else
                {
                    var message = ResHelper.GetString("contentedit.documentwasskipped");
                    string name = string.Format("{0} ({1})", child.GetDocumentName(), child.DocumentNamePath);
                    LogContext.AppendLine(String.Format(message, name), DocumentHelper.LOGCONTEXT_DOCUMENTS);
                    new DocumentEventLogger(child).Log("COPYDOC", message, false, EventType.WARNING);
                }
            }

            targetNode.NodeHasChildren = hasChildren;
        }


        private static bool IsPageTypeAllowedOnSite(int nodeClassId, int siteId)
        {
            return ClassSiteInfoProvider.GetClassSiteInfo(nodeClassId, siteId) != null;
        }


        protected virtual void CopyNode(CopyDocumentSettings settings)
        {
            new DocumentCopier(settings).Copy();
        }


        private CopyDocumentSettings GetChildSettings(TreeNode child, TreeNode targetNode)
        {
            return new CopyDocumentSettings(Settings, true)
            {
                Node = child,
                TargetNode = targetNode,
                IncludeChildNodes = true
            };
        }


        private static void PropagateNodeData(TreeNode sourcedNode, TreeNode targetCultureNode)
        {
            sourcedNode.CopyDataTo(targetCultureNode, new CopyNodeDataSettings(false, null)
            {
                CopyTreeData = true,
                CopyNonVersionedData = true,
                CopySystemTreeData = true
            });
        }


        private static void InsertNewDocumentCultureVersion(TreeNode targetCultureNode)
        {
            using (new CMSActionContext { LogEvents = false })
            {
                // Culture version uses complete data, there is no need to detect changes
                targetCultureNode.ResetChanges();
                targetCultureNode.InsertAsNewCultureVersion(null, false);
            }
        }


        protected virtual TreeNode CopyCultureVersion(TreeNode sourceCultureNode, TreeNode firstCopiedCultureNode)
        {
            var targetCultureNode = sourceCultureNode.Clone();
            PrepareTargetCultureNode(targetCultureNode);

            // First culture version is copied
            if (firstCopiedCultureNode == null)
            {
                CopyFirstCultureNode(targetCultureNode);
            }
            else
            {
                CopyOtherCultureNode(targetCultureNode, firstCopiedCultureNode);
            }

            CopyRelatedObjects(sourceCultureNode, targetCultureNode);

            return targetCultureNode;
        }


        private void CopyOtherCultureNode(TreeNode targetCultureNode, TreeNode firstCopiedCultureNode)
        {
            PropagateNodeData(firstCopiedCultureNode, targetCultureNode);
            EnsureDocumentTemplate(targetCultureNode);
            InsertNewDocumentCultureVersion(targetCultureNode);
        }


        private void CopyFirstCultureNode(TreeNode targetCultureNode)
        {
            ChangeNodeOwner(targetCultureNode);
            ChangeNodeGroup(targetCultureNode);
            GenerateNodeGUID(targetCultureNode);
            EnsureNodeTemplate(targetCultureNode);
            EnsureDocumentTemplate(targetCultureNode);
            InsertDocument(targetCultureNode);
            CopyNodePermissions(targetCultureNode);
        }


        private void PrepareTargetCultureNode(TreeNode targetCultureNode)
        {
            SetDocumentName(targetCultureNode);
            GenerateDocumentGUIDs(targetCultureNode);
            RemoveWorkflowActionStatus(targetCultureNode);
            targetCultureNode.ResetTranslationFlag();
            TreeProvider.ClearCheckoutInformation(targetCultureNode);
            EnsureDocumentTags(targetCultureNode);
            HandleSKU(targetCultureNode);
        }


        private void EnsureDocumentTags(TreeNode targetCultureNode)
        {
            if (!CrossSite)
            {
                return;
            }

            Settings.Tree.EnsureDocumentTags(targetCultureNode, TargetDocument.NodeSiteID, false, false);
        }


        private static void GenerateDocumentGUIDs(TreeNode targetCultureNode)
        {
            targetCultureNode.DocumentGUID = Guid.NewGuid();
            targetCultureNode.DocumentWorkflowCycleGUID = Guid.NewGuid();
        }


        private static void RemoveWorkflowActionStatus(TreeNode targetCultureNode)
        {
            targetCultureNode.DocumentWorkflowActionStatus = null;
        }


        private void InsertDocument(TreeNode targetCultureNode)
        {
            using (new CMSActionContext { LogEvents = false })
            {
                targetCultureNode.Insert(Settings.TargetNode, false);
            }
        }


        private void CopyNodePermissions(TreeNode targetNode)
        {
            if (Settings.CopyPermissions)
            {
                Settings.Tree.CopyNodePermissions(SourceDocument, targetNode);
            }
        }


        private void EnsureNodeTemplate(TreeNode targetCultureNode)
        {
            Settings.Tree.EnsureTemplate(targetCultureNode, false, TargetDocument.NodeSiteID, processedTemplates, true);
        }

        private void EnsureDocumentTemplate(TreeNode targetCultureNode)
        {
            Settings.Tree.EnsureTemplate(targetCultureNode, true, TargetDocument.NodeSiteID, processedTemplates, true);
        }


        private static void GenerateNodeGUID(TreeNode targetCultureNode)
        {
            targetCultureNode.NodeGUID = Guid.NewGuid();
        }


        private void ChangeNodeOwner(TreeNode targetCultureNode)
        {
            if (Settings.NewDocumentsOwner > 0)
            {
                targetCultureNode.NodeOwner = Settings.NewDocumentsOwner;
            }
        }


        private void ChangeNodeGroup(TreeNode targetCultureNode)
        {
            if (Settings.NewDocumentsGroup > 0)
            {
                targetCultureNode.NodeGroupID = Settings.NewDocumentsGroup;
            }
        }


        private void CopyRelatedObjects(TreeNode sourceCultureNode, TreeNode targetCultureNode)
        {
            // Disable logging of the information about staging task preparation to the asynchronous log
            // Do not log smart search tasks for each related object. Partial rebuild of the index is performed at the end of document copy action.
            using (new CMSActionContext { EnableLogContext = false, CreateSearchTask = false })
            {
                CopyCategories(sourceCultureNode, targetCultureNode);
                if (Settings.CopyAttachments)
                {
                    if (CopyAttachments(sourceCultureNode, targetCultureNode))
                    {
                        using (new DocumentActionContext { LogEvents = false, SendNotifications = false })
                        {
                            targetCultureNode.Update();
                        }
                    }
                }

                var copier = new AlternativeUrlsCopier(sourceCultureNode, targetCultureNode);
                copier.Copy();
            }
        }


        private bool CopyAttachments(TreeNode sourceCultureNode, TreeNode targetCultureNode)
        {
            var attachmentsCopier = new DocumentAttachmentsCopier(sourceCultureNode, targetCultureNode);
            var result = attachmentsCopier.Copy();

            AttachmentGUIDs = attachmentsCopier.AttachmentGUIDs;

            return result;
        }


        private void CopyCategories(TreeNode sourceCulture, TreeNode targetCulture)
        {
            foreach (var category in Settings.Tree.EnumerateCategories(sourceCulture.DocumentID, targetCulture.NodeSiteID).Where(c => c.Item2))
            {
                DocumentCategoryInfoProvider.AddDocumentToCategory(targetCulture.DocumentID, category.Item1);
            }
        }


        private void SetDocumentName(TreeNode targetCulture)
        {
            if (!String.IsNullOrEmpty(Settings.NewDocumentName))
            {
                targetCulture.DocumentName = Settings.NewDocumentName;
            }
        }


        private bool CultureIsAllowed(TreeNode sourceCultureNode)
        {
            return (!Settings.CheckSiteCulture || CultureSiteInfoProvider.IsCultureOnSite(sourceCultureNode.DocumentCulture, TargetDocument.NodeSiteName)) &&
                   Settings.Tree.UserInfo.IsCultureAllowed(sourceCultureNode.DocumentCulture, TargetDocument.NodeSiteName);
        }


        private void HandleSKU(TreeNode cultureNode)
        {
            if (cultureNode.NodeSKUID <= 0)
            {
                return;
            }

            // Get SKU bound to document in case it will be cloned or copied to another site
            GeneralizedInfo sku = null;
            if (Settings.CloneSKU || CrossSite)
            {
                sku = ProviderHelper.GetInfoById(PredefinedObjectType.SKU, cultureNode.NodeSKUID);
            }

            if (sku == null)
            {
                return;
            }

            if (CrossSite)
            {
                // If product is (source) site-specific or target does not allow global products
                if ((sku.GetValue("SKUSiteID") != null) || !SettingsKeyInfoProvider.GetBoolValue("CMSStoreAllowGlobalProducts", TargetDocument.NodeSiteID))
                {
                    // Remove product binding
                    cultureNode.NodeSKUID = 0;
                }
            }

            CloneSKU(cultureNode, sku);
        }


        private void CloneSKU(TreeNode cultureNode, GeneralizedInfo sku)
        {
            if (!Settings.CloneSKU || (cultureNode.NodeSKUID <= 0))
            {
                return;
            }

            var skuCloneSettings = new CloneSettings();
            skuCloneSettings.CloneToSiteID = sku.ObjectSiteID;

            var clonedSKU = sku.InsertAsClone(skuCloneSettings, new CloneResult());
            cultureNode.NodeSKUID = clonedSKU != null ? clonedSKU.Generalized.ObjectID : 0;
        }
    }
}