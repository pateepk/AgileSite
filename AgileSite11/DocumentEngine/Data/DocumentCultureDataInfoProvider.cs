using System.Collections.Generic;

using CMS.DataEngine;

namespace CMS.DocumentEngine.Internal
{
    /// <summary>
    /// Class providing document culture specific data management.
    /// </summary>
    /// <remarks>
    /// This class is intended for internal usage only.
    /// </remarks>
    public class DocumentCultureDataInfoProvider : AbstractInfoProvider<DocumentCultureDataInfo, DocumentCultureDataInfoProvider>
    {
        #region "Public methods"

        /// <summary>
        /// Returns document culture data object query.
        /// </summary>
        public static ObjectQuery<DocumentCultureDataInfo> GetDocumentCultures()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns document culture data with specified ID.
        /// </summary>
        /// <param name="dataId">Document culture ID.</param>        
        public static DocumentCultureDataInfo GetDocumentCultureInfo(int dataId)
        {
            return ProviderObject.GetInfoById(dataId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified document culture data.
        /// </summary>
        /// <param name="data">Document culture data to be set.</param>
        public static void SetDocumentCultureInfo(DocumentCultureDataInfo data)
        {
            ProviderObject.SetInfo(data);
        }


        /// <summary>
        /// Deletes specified document culture data.
        /// </summary>
        /// <param name="data">Document culture data to be deleted.</param>
        public static void DeleteDocumentCultureInfo(DocumentCultureDataInfo data)
        {
            ProviderObject.DeleteInfo(data);
        }


        /// <summary>
        /// Deletes document culture data with specified ID.
        /// </summary>
        /// <param name="dataId">Document culture data ID.</param>
        public static void DeleteDocumentCultureInfo(int dataId)
        {
            var documentCulture = GetDocumentCultureInfo(dataId);
            DeleteDocumentCultureInfo(documentCulture);
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Updates the data based on the given where condition using a database query
        /// </summary>
        /// <param name="where">Where condition</param>
        /// <param name="values">New values for the data. Dictionary of [columnName] => [value]</param>
        internal static void BulkUpdateData(WhereCondition where, IEnumerable<KeyValuePair<string, object>> values)
        {
            ProviderObject.UpdateData(where, values);
        }


        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(DocumentCultureDataInfo info)
        {
            // Update the "can be published" flag for efficient evaluation of publishing status
            info.EnsureDocumentCanBePublishedValue();

            base.SetInfo(info);
        }

        #endregion


        /// <summary>
        /// Validates the object code name. Returns true if the code name is valid.
        /// </summary>
        /// <param name="info">Object to check</param>
        public override bool ValidateCodeName(DocumentCultureDataInfo info)
        {
            // Document has culture as code name which is handled in TreeNode API, the validation must be ommited
            return true;
        }


        /// <summary>
        /// Checks if the object has unique code name. Returns true if the object has unique code name.
        /// </summary>
        /// <param name="infoObj">Info object to check</param>
        public override bool CheckUniqueCodeName(DocumentCultureDataInfo infoObj)
        {
            // Document has culture as code name which is handled in TreeNode API, the validation must be ommited
            return true;
        }
    }
}
