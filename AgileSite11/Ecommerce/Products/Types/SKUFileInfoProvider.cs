using System;

using CMS.DataEngine;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Class providing SKUFileInfo management.
    /// </summary>
    public class SKUFileInfoProvider : AbstractInfoProvider<SKUFileInfo, SKUFileInfoProvider>
    {
        #region "Public methods - Basic"

        /// <summary>
        /// Returns the query for all SKU files.
        /// </summary>        
        public static ObjectQuery<SKUFileInfo> GetSKUFiles()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns SKU file with specified ID.
        /// </summary>
        /// <param name="fileId">Sku file ID.</param>        
        public static SKUFileInfo GetSKUFileInfo(int fileId)
        {
            return ProviderObject.GetInfoById(fileId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified SKU file.
        /// </summary>
        /// <param name="fileObj">Sku file to be set.</param>
        public static void SetSKUFileInfo(SKUFileInfo fileObj)
        {
            ProviderObject.SetInfo(fileObj);
        }


        /// <summary>
        /// Deletes specified SKU file.
        /// </summary>
        /// <param name="fileObj">Sku file to be deleted.</param>
        public static void DeleteSKUFileInfo(SKUFileInfo fileObj)
        {
            ProviderObject.DeleteInfo(fileObj);
        }


        /// <summary>
        /// Deletes SKU file with specified ID.
        /// </summary>
        /// <param name="fileId">Sku file ID.</param>
        public static void DeleteSKUFileInfo(int fileId)
        {
            var fileObj = GetSKUFileInfo(fileId);
            DeleteSKUFileInfo(fileObj);
        }

        #endregion
    }
}