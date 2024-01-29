using System;
using CMS.CMSImportExport;

namespace CMS.WebFarmSync
{
    /// <summary>
    /// Handles special actions during the import process.
    /// </summary>
    internal static class ImportSpecialActions
    {
        #region "Methods"

        /// <summary>
        /// Initializes export handlers
        /// </summary>
        public static void Init()
        {
            ImportExportEvents.ImportObject.Before += ImportObject_Before;
            SpecialActionsEvents.ImportLoadDefaultSelection.Execute += LoadDefaultSelection_Execute;
        }


        private static void LoadDefaultSelection_Execute(object sender, ImportLoadSelectionArgs e)
        {
            var objectType = e.ObjectType;
            var settings = e.Settings;
            var siteObject = e.SiteObject;

            switch (objectType)
            {
                case WebFarmServerInfo.OBJECT_TYPE:
                    DefaultSelectionParameters parameters = new DefaultSelectionParameters()
                    {
                        ObjectType = objectType,
                        SiteObjects = siteObject
                    };
                    settings.LoadDefaultSelection(parameters);

                    // Cancel default selection
                    e.Select = false;
                    break;
            }
        }


        public static void ImportObject_Before(object sender, ImportEventArgs e)
        {
            var infoObj = e.Object;
            if ((infoObj.TypeInfo.ObjectType == "cms.scheduledtask") && (infoObj.Generalized.ObjectCodeName == "SynchronizeWebFarmChanges"))
            {
                // Do not import the object
                e.Cancel();
            }
        }

        #endregion
    }
}