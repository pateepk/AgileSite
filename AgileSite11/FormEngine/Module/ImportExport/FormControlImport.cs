using System;
using System.Data;
using System.Xml;

using CMS.IO;
using CMS.Helpers;
using CMS.CMSImportExport;

namespace CMS.FormEngine
{
    /// <summary>
    /// Handles special actions during the Form control import process.
    /// </summary>
    public static class FormControlImport
    {
        #region "Methods"

        /// <summary>
        /// Initializes import handlers
        /// </summary>
        public static void Init()
        {
            ImportExportEvents.ImportObjectType.After += Import_After;

            SpecialActionsEvents.ProcessMainObject.Before += ProcessMainObject_Before;
        }


        private static void ProcessMainObject_Before(object sender, ImportEventArgs e)
        {
            var settings = e.Settings;
            var control = e.Object as FormUserControlInfo;

            if (e.Settings.Version.StartsWith("11", StringComparison.Ordinal) && ValidationHelper.GetInteger(settings.HotfixVersion, 0) < 4)
            {
                FixHtml5InputControl(control);
            }
        }


        private static void Import_After(object sender, ImportDataEventArgs e)
        {
            var objectType = e.ObjectType;
            if (objectType == FormUserControlInfo.OBJECT_TYPE)
            {
                var settings = e.Settings;
                var siteObjects = e.SiteObjects;
                var data = e.Data;

                // There is no data for this object type
                if (DataHelper.DataSourceIsEmpty(data) || DataHelper.DataSourceIsEmpty(data.Tables["CMS_FormUserControl"]))
                {
                    return;
                }

                string safeObjectType = ImportExportHelper.GetSafeObjectTypeName(objectType);

                string sourcePath = DirectoryHelper.CombinePath(settings.TemporaryFilesPath, ImportExportHelper.DATA_FOLDER, ImportExportHelper.FILES_FOLDER) + "\\" + safeObjectType + "\\";
                string targetPath = ImportProvider.GetTargetPath(settings, settings.WebsitePath);

                var table = data.Tables["CMS_FormUserControl"];
                foreach (DataRow dr in table.Rows)
                {
                    // Import process canceled
                    if (settings.ProcessCanceled)
                    {
                        ImportProvider.ImportCanceled();
                    }

                    // Prepare the data
                    string objectName = dr["UserControlCodeName"].ToString();
                    string objectFileName = dr["UserControlFileName"].ToString();

                    // Copy files if object is processed
                    if (!String.IsNullOrEmpty(objectFileName) && !"inherited".Equals(objectFileName, StringComparison.OrdinalIgnoreCase) && settings.IsProcessed(FormUserControlInfo.OBJECT_TYPE, objectName, siteObjects))
                    {
                        string sourceObjectPath = FileHelper.GetFullPhysicalPath(objectFileName, FormUserControlInfoProvider.FormUserControlsDirectory, sourcePath);
                        string targetObjectPath = FileHelper.GetFullPhysicalPath(objectFileName, FormUserControlInfoProvider.FormUserControlsDirectory, targetPath);

                        // Source files
                        ImportProvider.AddSourceFiles(FormUserControlInfo.OBJECT_TYPE, settings, sourceObjectPath, targetObjectPath);
                    }
                }
            }
        }


        /// <summary>
        /// Fix Html5Input form control typos.
        /// </summary>
        internal static void FixHtml5InputControl(FormUserControlInfo control)
        {
            if (!control?.UserControlCodeName?.Equals("Html5Input", StringComparison.OrdinalIgnoreCase) ?? true)
            {
                return;
            }

            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.LoadXml(control.UserControlParameters);
            }
            catch (XmlException)
            {
                return;
            }

            var maxLenght = xmlDoc.SelectSingleNode("form/field[@guid = '30ca5076-c48f-44c3-b359-be02b63ae81a' and @column = 'Maxlenght']") as XmlElement;
            var minLenght = xmlDoc.SelectSingleNode("form/field[@guid = '53002493-c0a1-456b-8507-921e74051993' and @column = 'Minlenght']") as XmlElement;

            var maxLenghtFieldCaption = xmlDoc.SelectSingleNode("form/field[@guid = '30ca5076-c48f-44c3-b359-be02b63ae81a' and @column = 'Maxlenght']/properties/fieldcaption");
            var minLenghtFieldCaption = xmlDoc.SelectSingleNode("form/field[@guid = '53002493-c0a1-456b-8507-921e74051993' and @column = 'Minlenght']/properties/fieldcaption");

            if (maxLenghtFieldCaption != null)
            {
                maxLenghtFieldCaption.InnerXml = "Maxlength";
            }

            if (minLenghtFieldCaption != null)
            {
                minLenghtFieldCaption.InnerXml = "Minlength";
            }

            if (maxLenght != null)
            {
                maxLenght.SetAttribute("column", "Maxlength");
            }

            if (minLenght != null)
            {
                minLenght.SetAttribute("column", "Minlength");
            }

            if (minLenght != null || maxLenght != null || minLenghtFieldCaption != null || maxLenghtFieldCaption != null)
            {
                control.UserControlParameters = xmlDoc.ToFormattedXmlString(true);
            }
        }

        #endregion
    }
}