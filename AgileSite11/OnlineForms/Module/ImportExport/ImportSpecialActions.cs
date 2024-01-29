﻿using System;

using CMS.Base;
using CMS.CMSImportExport;
using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.OnlineForms
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
            SpecialActionsEvents.GetEmptyObject.Execute += GetImportEmptyObject_Execute;
        }


        private static void GetImportEmptyObject_Execute(object sender, ImportGetDataEventArgs e)
        {
            var settings = e.Settings;
            var objectType = e.ObjectType;
            GeneralizedInfo infoObj = null;

            // Get info object
            if (objectType.StartsWithCSafe(ImportExportHelper.BIZFORM_PREFIX))
            {
                string className = objectType.Substring(ImportExportHelper.BIZFORM_PREFIX.Length);
                infoObj = ModuleManager.GetReadOnlyObjectByClassName(className);

                // ## Backward compatibility - for versions prior to 8 (object type consisted of form name, not class name)
                if (infoObj == null)
                {
                    // Get form
                    string bizFormName = objectType.Substring(ImportExportHelper.BIZFORM_PREFIX.Length);
                    var form = BizFormInfoProvider.GetBizFormInfo(bizFormName, settings.SiteId);
                    if (form == null)
                    {
                        throw new Exception("[ImportProvider.LoadObjects]: BizForm '" + bizFormName + "' not found.");
                    }

                    // Get form DataClass
                    DataClassInfo dci = DataClassInfoProvider.GetDataClassInfo(form.FormClassID);
                    if (dci == null)
                    {
                        throw new Exception("[ImportProvider.LoadObjects]: BizForm '" + bizFormName + "' class not found.");
                    }

                    infoObj = ModuleManager.GetReadOnlyObjectByClassName(dci.ClassName);
                }
            }

            if (infoObj != null)
            {
                e.Object = infoObj;
            }
        }


        private static void ImportObject_Before(object sender, ImportEventArgs e)
        {
            var infoObj = e.Object;
            var parameters = e.Parameters;

            if (infoObj.TypeInfo.ObjectType == BizFormInfo.OBJECT_TYPE)
            {
                // Update the class ID
                if (parameters.ObjectProcessType == ProcessObjectEnum.All)
                {
                    if (parameters.UpdateChildObjects)
                    {
                        // Store original class ID for further use
                        parameters.SetValue("formClassId", ValidationHelper.GetInteger(infoObj.GetValue("FormClassID"), 0));
                        infoObj.SetValue("FormClassID", 0);
                    }
                }
            }
        }

        #endregion
    }
}