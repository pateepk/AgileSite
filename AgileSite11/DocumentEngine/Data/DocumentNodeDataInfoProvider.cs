using System.Collections.Generic;

using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.DocumentEngine.Internal
{
    /// <summary>
    /// Class providing document tree nodes management.
    /// </summary>
    /// <remarks>
    /// This class is intended for internal usage only.
    /// </remarks>
    public class DocumentNodeDataInfoProvider : AbstractInfoProvider<DocumentNodeDataInfo, DocumentNodeDataInfoProvider>
    {
        /// <summary>
        /// List of columns automatically synchronized between linked documents and their original.
        /// </summary>
        private static readonly IEnumerable<string> SynchronizedLinkColumns = new[] { "NodeName", "NodeSKUID", "NodeTemplateID", "NodeTemplateForAllCultures" };


        #region "Constructors"

        /// <summary>
        /// Creates an instance of <see cref="DocumentNodeDataInfoProvider"/>.
        /// </summary>
        public DocumentNodeDataInfoProvider()
            : base(DocumentNodeDataInfo.TYPEINFO, new HashtableSettings
            {
                ID = true,
                UseWeakReferences = true
            })
        {
        }

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns document nodes object query.
        /// </summary>
        public static ObjectQuery<DocumentNodeDataInfo> GetDocumentNodes()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns document node with specified ID.
        /// </summary>
        /// <param name="nodeId">Document node ID.</param>        
        public static DocumentNodeDataInfo GetDocumentNodeDataInfo(int nodeId)
        {
            return ProviderObject.GetInfoById(nodeId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified document node.
        /// </summary>
        /// <param name="node">Document node to be set.</param>
        public static void SetDocumentNodeDataInfo(DocumentNodeDataInfo node)
        {
            ProviderObject.SetInfo(node);
        }


        /// <summary>
        /// Deletes specified document node.
        /// </summary>
        /// <param name="node">Document node to be deleted.</param>
        public static void DeleteDocumentNodeDataInfo(DocumentNodeDataInfo node)
        {
            ProviderObject.DeleteInfo(node);
        }


        /// <summary>
        /// Deletes document node with specified ID.
        /// </summary>
        /// <param name="nodeId">Document node ID.</param>
        public static void DeleteDocumentNodeDataInfo(int nodeId)
        {
            DocumentNodeDataInfo node = GetDocumentNodeDataInfo(nodeId);
            DeleteDocumentNodeDataInfo(node);
        }


        #endregion


        #region "Internal methods"

        /// <summary>
        /// Updates the data based on the given where condition using a database query.
        /// </summary>
        /// <param name="where">Where condition.</param>
        /// <param name="values">New values for the data. Dictionary of [columnName] => [value].</param>
        internal static void BulkUpdateData(WhereCondition where, IEnumerable<KeyValuePair<string, object>> values)
        {
            ProviderObject.UpdateData(where, values);
        }


        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(DocumentNodeDataInfo info)
        {
            if (info.NodeID > 0)
            {
                UpdateNodeDataInfoInternal(info);
            }
            else
            {
                InsertNodeDataInfoInternal(info);
            }
        }


        /// <summary>
        /// Inserts new node data.
        /// </summary>
        /// <param name="node">Node data to insert.</param>
        private void InsertNodeDataInfoInternal(DocumentNodeDataInfo node)
        {
            EnsureDefaultData(node);

            // Clear original node ID, value is updated after insert
            node.NodeOriginalNodeID = 0;

            base.SetInfo(node);

            EnsureFieldValues(node.NodeParent, node);
        }


        /// <summary>
        /// Updates node data.
        /// </summary>
        /// <param name="node">Node data to update.</param>
        private void UpdateNodeDataInfoInternal(DocumentNodeDataInfo node)
        {
            bool parentChanged = node.ItemChanged("NodeParentID");
            bool linkedIdChanged = node.ItemChanged("NodeLinkedNodeID");
            int originalNodeId = ValidationHelper.GetInteger(node.GetOriginalValue("NodeLinkedNodeID"), 0);
            int originalParentId = ValidationHelper.GetInteger(node.GetOriginalValue("NodeParentID"), 0);

            // Check whether the link (CMS_Tree) data should be updated
            bool updateLinks = (node.NodeHasLinks || node.IsLink) && node.Generalized.AnyItemChanged(SynchronizedLinkColumns);

            EnsureDefaultData(node);

            // Update original node ID
            node.NodeOriginalNodeID = (node.NodeLinkedNodeID > 0) ? node.NodeLinkedNodeID : node.NodeID;

            base.SetInfo(node);

            // Update the links data
            if (updateLinks)
            {
                UpdateLinks(node);
            }

            if (linkedIdChanged)
            {
                // If linked node ID has changed update original document
                UpdateOriginalNodeHasLinksFlag(originalNodeId);

                // Set flag to new original document
                EnsureOriginalNodeHasLinksFlag(node.NodeLinkedNodeID);
            }

            if (parentChanged)
            {
                UpdateParentNodeHasChildrenFlag(originalParentId);

                var parent = GetDocumentNodeDataInfo(node.NodeParentID);
                EnsureParentNodeHasChildrenFlag(parent);
            }
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(DocumentNodeDataInfo info)
        {
            base.DeleteInfo(info);

            // Update children flag
            UpdateParentNodeHasChildrenFlag(info.NodeParentID);

            // Update links flag
            UpdateOriginalNodeHasLinksFlag(info.NodeLinkedNodeID);
        }


        /// <summary>
        /// Ensures default data for the instance
        /// </summary>
        /// <param name="node">Document node</param>
        private static void EnsureDefaultData(DocumentNodeDataInfo node)
        {
            EnsureContentOnlyFlag(node);
            EnsureACLOwnerFlag(node);
        }


        /// <summary>
        /// Ensures that the NodeIsContentOnly flag is properly set.
        /// </summary>
        /// <param name="node">Document node.</param>
        private static void EnsureContentOnlyFlag(DocumentNodeDataInfo node)
        {
            if (node.GetValue("NodeIsContentOnly") == null)
            {
                var dci = DataClassInfoProvider.GetDataClassInfo(node.NodeClassID);
                node.NodeIsContentOnly = dci.ClassIsContentOnly;
            }
        }


        /// <summary>
        /// Ensures that the NodeIsACLOwner flag is properly set.
        /// </summary>
        /// <param name="node">Document node.</param>
        private static void EnsureACLOwnerFlag(DocumentNodeDataInfo node)
        {
            if (node.GetValue("NodeIsACLOwner") == null)
            {
                node.NodeIsACLOwner = node.NodeClassName.EqualsCSafe(SystemDocumentTypes.Root, true);
            }
        }

        #endregion


        #region "Children flag methods"

        /// <summary>
        /// Updates NodeHasChildren flag only if there is no child node.
        /// </summary>
        /// <param name="parentId">Parent ID.</param>
        private void UpdateParentNodeHasChildrenFlag(int parentId)
        {
            if (parentId <= 0)
            {
                return;
            }

            var parent = GetDocumentNodeDataInfo(parentId);
            if (parent == null)
            {
                return;
            }

            var count = GetDocumentNodes()
                            .WhereID("NodeParentID", parent.NodeID)
                            .Column("NodeID")
                            .Count;

            if (count != 0)
            {
                return;
            }

            parent.NodeHasChildren = false;

            // Force reset changes for parent 
            using (new CMSActionContext { ResetChanges = true })
            {
                parent.Update();
            }
        }


        /// <summary>
        /// Sets NodeHasChildren flag to true, if hasn't been set already.
        /// </summary>
        /// <param name="parent">Parent node.</param>
        private void EnsureParentNodeHasChildrenFlag(DocumentNodeDataInfo parent)
        {
            if (parent == null)
            {
                return;
            }

            if (parent.NodeHasChildren)
            {
                return;
            }

            parent.NodeHasChildren = true;

            // Force reset changes for parent 
            using (new CMSActionContext { ResetChanges = true })
            {
                parent.Update();
            }
        }

        #endregion


        #region "Link methods"

        /// <summary>
        /// Sets NodeHasLinks flag to true, if hasn't been set already.
        /// </summary>
        /// <param name="originalNodeId">Original node ID.</param>
        private void EnsureOriginalNodeHasLinksFlag(int originalNodeId)
        {
            if (originalNodeId <= 0)
            {
                return;
            }

            DocumentNodeDataInfo originalDocument = GetDocumentNodeDataInfo(originalNodeId);
            if (originalDocument == null)
            {
                return;
            }

            if (originalDocument.NodeHasLinks)
            {
                return;
            }

            originalDocument.NodeHasLinks = true;

            // Force reset changes for parent 
            using (new CMSActionContext { ResetChanges = true })
            {
                originalDocument.Update();
            }
        }


        /// <summary>
        /// Updates the data in the linked documents.
        /// Update all nodes which are linked together, except current node which is expected to be already updated.
        /// </summary>
        private void UpdateLinks(DocumentNodeDataInfo node)
        {
            // Prepare values to be synchronized across links
            var values = new Dictionary<string, object>();

            foreach (string columnName in SynchronizedLinkColumns)
            {
                values.Add(columnName, node.GetValue(columnName));
            }

            var where =
                new WhereCondition()
                    // Do not update the node itself, as it is already updated
                    .WhereNotEquals("NodeID", node.NodeID)
                    // Update all nodes linked together
                    .Where(w => w
                        .WhereEquals("NodeID", node.NodeOriginalNodeID)
                        .Or()
                        .WhereEquals("NodeLinkedNodeID", node.NodeOriginalNodeID)
                    );

            // Update links
            BulkUpdateData(where, values);
        }


        /// <summary>
        /// Updates NodeHasLinks flag only if there is no links.
        /// </summary>
        /// <param name="originalNodeId">Original node ID.</param>
        private void UpdateOriginalNodeHasLinksFlag(int originalNodeId)
        {
            if (originalNodeId <= 0)
            {
                return;
            }

            var count = GetDocumentNodes()
                            .WhereID("NodeLinkedNodeID", originalNodeId)
                            .Column("NodeID")
                            .Count;

            if (count != 0)
            {
                return;
            }

            var originalDocument = GetDocumentNodeDataInfo(originalNodeId);
            if (originalDocument == null)
            {
                return;
            }

            originalDocument.NodeHasLinks = false;

            // Force reset changes for parent 
            using (new CMSActionContext { ResetChanges = true })
            {
                originalDocument.Update();
            }
        }

        #endregion


        /// <summary>
        /// Ensures special field values for node data during insertion.
        /// </summary>
        /// <param name="parent">Parent node.</param>
        /// <param name="node">Node.</param>
        internal void EnsureFieldValues(DocumentNodeDataInfo parent, DocumentNodeDataInfo node)
        {
            // Set original node ID
            EnsureNodeOriginalNodeID(node);

            // Ensure children flag
            EnsureParentNodeHasChildrenFlag(parent);

            // Ensure links flag
            EnsureOriginalNodeHasLinksFlag(node.NodeLinkedNodeID);
        }


        /// <summary>
        /// Ensures original node.
        /// </summary>
        internal void EnsureNodeOriginalNodeID(DocumentNodeDataInfo node)
        {
            node.NodeOriginalNodeID = node.NodeLinkedNodeID > 0 ? node.NodeLinkedNodeID : node.NodeID;
            UpdateNodeDataInfoInternal(node);
        }


        /// <summary>
        /// Validates the object code name. Returns true if the code name is valid.
        /// </summary>
        /// <param name="info">Object to check.</param>
        public override bool ValidateCodeName(DocumentNodeDataInfo info)
        {
            // Document has alias path as code name which is populated automatically from node alias, the validation must be ommited
            return true;
        }


        /// <summary>
        /// Checks if the object has unique code name. Returns true if the object has unique code name.
        /// </summary>
        /// <param name="infoObj">Info object to check.</param>
        public override bool CheckUniqueCodeName(DocumentNodeDataInfo infoObj)
        {
            // Document has alias path as code name which is populated automatically from node alias, the validation must be ommited
            return true;
        }
    }
}
