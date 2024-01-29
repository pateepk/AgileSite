using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CMS.CMSImportExport;
using CMS.DataEngine;
using CMS.FormEngine;

namespace CMS.TranslationServices
{
    /// <summary>
    /// Handles special actions during the import process.
    /// </summary>
    internal static class ImportSpecialActions
    {
        /// <summary>
        /// List of removed form control GUIDs.
        /// Form controls in this list won't be imported.
        /// </summary>
        private static readonly HashSet<Guid> RemovedFormControlGUIDsInV11 = new HashSet<Guid>
        {
            new Guid("560f8ef9-f5ff-445f-b751-0ecbb759a262"), // Get Microsoft Translator Key
            new Guid("c73ff6fe-6f73-4648-bfaf-635ba8673568")  // Get Microsoft Translator Secret
        };


        /// <summary>
        /// Initializes export handlers
        /// </summary>
        public static void Init()
        {
            ImportExportEvents.ImportObject.Before += ImportObject_Before;
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
                    if (infoObj.Generalized.ObjectCodeName.Equals("CMSMSTranslatorClientID", StringComparison.OrdinalIgnoreCase)
                        || infoObj.Generalized.ObjectCodeName.Equals("CMSMSTranslatorClientSecret", StringComparison.OrdinalIgnoreCase))
                    {
                        e.Cancel();
                    }
                    break;

                case FormUserControlInfo.OBJECT_TYPE:
                    if (RemovedFormControlGUIDsInV11.Contains(infoObj.Generalized.ObjectGUID))
                    {
                        e.Cancel();
                    }
                    break;
            }
        }
    }
}
