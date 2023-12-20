using System;
using System.Collections.Generic;

using CMS.CMSImportExport;
using CMS.DataEngine;
using CMS.Base;

namespace CMS.MediaLibrary
{
    /// <summary>
    /// Handles special actions during the import process.
    /// </summary>
    internal static class ImportSpecialActions
    {
        /// <summary>
        /// List of removed settings keys code names.
        /// Settings keys in this list won't be imported.
        /// </summary>
        private static readonly HashSet<string> RemovedSettingsKeysCodeNamesInV11 = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            // Unnecessary settings for HTML5 markup
            "CMSAllowedHTML5MediaExtensions",
            "CMSRenderHTML5MediaTags"
        };

        #region "Methods"

        /// <summary>
        /// Initializes export handlers
        /// </summary>
        public static void Init()
        {
            SpecialActionsEvents.GetEmptyObject.Execute += GetImportEmptyObject_Execute;
            ImportExportEvents.ImportObject.Before += ImportObject_Before;
        }


        private static void GetImportEmptyObject_Execute(object sender, ImportGetDataEventArgs e)
        {
            var objectType = e.ObjectType;
            GeneralizedInfo infoObj = null;

            // Get info object
            if (objectType.StartsWithCSafe(ImportExportHelper.MEDIAFILE_PREFIX))
            {
                infoObj = ModuleManager.GetReadOnlyObject(MediaFileInfo.OBJECT_TYPE);
            }

            if (infoObj != null)
            {
                e.Object = infoObj;
            }
        }


        private static void ImportObject_Before(object sender, ImportEventArgs e)
        {
            if (!e.Settings.IsLowerVersion("11.0"))
            {
                return;
            }

            var infoObj = e.Object;

            switch (infoObj.TypeInfo.ObjectType)
            {
                case SettingsKeyInfo.OBJECT_TYPE:
                    if (RemovedSettingsKeysCodeNamesInV11.Contains(infoObj.Generalized.ObjectCodeName))
                    {
                        e.Cancel();
                    }
                    break;
            }
        }

        #endregion
    }
}