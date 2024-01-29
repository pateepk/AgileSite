using CMS.CMSImportExport;
using CMS.Helpers;

namespace CMS.DeviceProfiles
{
    /// <summary>
    /// Handles special actions during the device profile import process.
    /// </summary>
    internal static class DeviceProfileImport
    {
        #region "Methods"

        /// <summary>
        /// Initializes import handlers
        /// </summary>
        public static void Init()
        {
            ImportExportEvents.ImportObjectType.After += Import_After;
        }


        static void Import_After(object sender, ImportDataEventArgs e)
        {
            var objectType = e.ObjectType;
            if (objectType == DeviceProfileInfo.OBJECT_TYPE)
            {
                var settings = e.Settings;
                var siteObjects = e.SiteObjects;
                var data = e.Data;

                // There is no data for this object type
                if (DataHelper.DataSourceIsEmpty(data) || DataHelper.DataSourceIsEmpty(data.Tables["CMS_DeviceProfile"]))
                {
                    return;
                }

                ImportProvider.AddComponentFiles(objectType, settings, siteObjects, data, "DeviceProfile");
            }
        }

        #endregion
    }
}