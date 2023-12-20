using CMS.DataEngine;
using CMS.DocumentEngine;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Handles special actions during the SKU import process.
    /// </summary>
    public static class SKUImport
    {
        #region "Methods"

        /// <summary>
        /// Initializes export handlers
        /// </summary>
        public static void Init()
        {
            ColumnsTranslationEvents.TranslateColumns.Execute += TranslateDocumentColumns_Execute;
        }


        private static void TranslateDocumentColumns_Execute(object sender, ColumnsTranslationEventArgs e)
        {
            if (e.ObjectType == TreeNode.OBJECT_TYPE)
            {
                var node = (TreeNode)e.Data;
                var th = e.TranslationHelper;

                // SKU ID
                node.NodeSKUID = th.GetNewID(SKUInfo.OBJECT_TYPE_SKU, node.NodeSKUID, "SKUGUID", 0, ObjectTypeInfo.COLUMN_NAME_UNKNOWN, ObjectTypeInfo.COLUMN_NAME_UNKNOWN, ObjectTypeInfo.COLUMN_NAME_UNKNOWN);

                // Since SKU property of SKUTreeNode is cached it is not updated when translated NodeSKUID is same as original.
                // During import and staging we need to enforce SKU reload because it can contain untranslated data from source server.
                if (node is SKUTreeNode skuNode)
                {
                    skuNode.ClearSKU();
                }
            }
        }

        #endregion
    }
}