using CMS.Base;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.Helpers;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Handles special actions during the SKU export process.
    /// </summary>
    internal static class SKUExport
    {
        /// <summary>
        /// Initializes export handlers
        /// </summary>
        internal static void Init()
        {
            ColumnsTranslationEvents.RegisterRecords.Execute += RegisterRecords_Execute;
        }


        private static void RegisterRecords_Execute(object sender, ColumnsTranslationEventArgs e)
        {
            RegisterTranslationRecords(e.Data, e.ObjectType, e.TranslationHelper);
        }


        private static void RegisterTranslationRecords(IDataContainer data, string objectType, TranslationHelper th)
        {
            if (objectType == TreeNode.OBJECT_TYPE)
            {
                int skuId = ValidationHelper.GetInteger(data.GetValue("NodeSKUID"), 0);
                if (skuId > 0)
                {
                    var skuInfo = SKUInfoProvider.GetSKUInfo(skuId);
                    if (skuInfo != null)
                    {
                        th.RegisterRecord(skuInfo);
                    }
                }
            }
        }
    }
}