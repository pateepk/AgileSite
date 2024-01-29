using System.Data;

using CMS.CMSImportExport;
using CMS.Helpers;

namespace CMS.PortalEngine
{
    /// <summary>
    /// Handles special actions during the Page layout import process.
    /// </summary>
    internal static class PageLayoutImport
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
            if (objectType == LayoutInfo.OBJECT_TYPE)
            {
                var settings = e.Settings;
                var siteObjects = e.SiteObjects;
                var data = e.Data;

                // There is no data for this object type
                if (DataHelper.DataSourceIsEmpty(data) || DataHelper.DataSourceIsEmpty(data.Tables["CMS_Layout"]))
                {
                    return;
                }

                ImportProvider.AddComponentFiles(objectType, settings, siteObjects, data, "Layouts");
            }
        }

        #endregion
    }
}