using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// External column settings for the attachment history info
    /// </summary>
    internal class AttachmentHistoryBinarySettings : ExternalColumnSettings<AttachmentHistoryInfo>
    {
        /// <summary>
        /// If true, the attachment history is stored only in database, and does not reflect general settings for storing binary data
        /// </summary>
        private static readonly BoolAppSetting ForceStoreInDatabase = new BoolAppSetting("CMSForceAttachmentHistoryInDatabase");


        /// <summary>
        /// Returns true if the column should be stored in database
        /// </summary>
        /// <param name="objectSiteName">Object site name</param>
        public override bool StoreInDatabase(string objectSiteName)
        {
            if (ForceStoreInDatabase)
            {
                return true;
            }

            return (FileHelper.FilesLocationType(objectSiteName) != FilesLocationTypeEnum.FileSystem);
        }


        /// <summary>
        /// Returns true if the column should be stored in external storage
        /// </summary>
        /// <param name="objectSiteName">Object site name</param>
        public override bool StoreInExternalStorage(string objectSiteName)
        {
            if (ForceStoreInDatabase)
            {
                return false;
            }

            return (FileHelper.FilesLocationType(objectSiteName) != FilesLocationTypeEnum.Database);
        }
    }
}