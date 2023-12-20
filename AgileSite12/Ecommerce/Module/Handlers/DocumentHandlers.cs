using CMS.DocumentEngine;
using CMS.DocumentEngine.Internal;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Provides handlers for special SKUs actions within document actions
    /// </summary>
    internal static class DocumentHandlers
    {
        /// <summary>
        /// Initializes the document handlers
        /// </summary>
        public static void Init()
        {
            DocumentEvents.Move.Before += EnsureProductSiteConsistency;
            DocumentEvents.Delete.Before += DeleteMultiBuyDiscountTree;
            DocumentSynchronizationEvents.LogDocumentChangeClone.Execute += LogDocumentChangeClone;
        }


        /// <summary>
        /// Delete bindings between given document and MultiBuyDiscountInfo.
        /// </summary>
        private static void DeleteMultiBuyDiscountTree(object sender, DocumentEventArgs documentEventArgs)
        {
            var nodeId = documentEventArgs.Node.NodeID;
            var multiBuyDiscountTreeInfos = MultiBuyDiscountTreeInfoProvider.GetMultiBuyDiscountTrees().WhereEquals("NodeID", nodeId).TypedResult;

            foreach (var multiBuyDiscountTreeInfo in multiBuyDiscountTreeInfos)
            {
                MultiBuyDiscountTreeInfoProvider.DeleteMultiBuyDiscountTreeInfo(multiBuyDiscountTreeInfo);
            }
        }


        /// <summary>
        /// Removes sku from document when document is moved to another site. 
        /// SKU is removed only if is not accessible on target site.
        /// </summary>
        private static void EnsureProductSiteConsistency(object sender, DocumentEventArgs e)
        {
            var node = e.Node;
            var originalSiteId = node.NodeSiteID;
            var target = e.TargetParentNode;
            var targetSiteId = target.NodeSiteID;

            // Document is moved within the site
            if (originalSiteId == targetSiteId)
            {
                return;
            }

            // Skip documents without SKU
            if (!node.HasSKU)
            {
                return;
            }

            // Remove product binding if target site does not allow global (shared) SKUs
            if(!ECommerceSettings.AllowGlobalProducts(targetSiteId))
            {
                node.NodeSKUID = 0;
                return;
            }

            // Remove product binding if SKU is site-specific (can not exits on target site)
            var product = SKUInfoProvider.GetSKUInfo(node.NodeSKUID);
            if ((product != null) && !product.IsGlobal)
            {
                node.NodeSKUID = 0;
            }
        }


        /// <summary>
        /// Fixes cloned SKUTreeNode when synchronization task of the document with SKU is being prepared.
        /// </summary>
        private static void LogDocumentChangeClone(object sender, LogDocumentChangeCloneEventArgs e)
        {
            var original = e.Original;
            var clone = e.Clone;

            if (original == null || clone == null)
            {
                return;
            }

            if (!original.HasSKU || !clone.HasSKU)
            {
                return;
            }

            if (clone is SKUTreeNode cloneSKUTreeNode && original is SKUTreeNode originalSKUTreeNode)
            {
                cloneSKUTreeNode.SKU = originalSKUTreeNode.SKU.Clone();
            }
        }
    }
}