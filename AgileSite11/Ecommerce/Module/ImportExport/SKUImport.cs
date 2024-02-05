using System;

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
            }
        }

        #endregion
    }
}