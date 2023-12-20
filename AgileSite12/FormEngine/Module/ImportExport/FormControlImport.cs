using System;
using System.Data;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

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
            if (e.Object is FormUserControlInfo formControl)
            {
                if (e.Settings.Version.StartsWith("11", StringComparison.Ordinal) && ValidationHelper.GetInteger(settings.HotfixVersion, 0) < 4)
                {
                    FixHtml5InputControl(formControl);
                }

                if (settings.IsLowerVersion("12.0"))
                {
                    if (String.Equals("ReCAPTCHA", formControl.UserControlCodeName, StringComparison.OrdinalIgnoreCase))
                    {
                        ChangeRecaptchaAssemblyAndParameters(formControl);
                    }

                    if (String.Equals("ConsentAgreement", formControl.UserControlCodeName, StringComparison.OrdinalIgnoreCase))
                    {
                        DisallowNoneOptionInConsentAgreement(formControl);
                    }
                }
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

            XDocument doc = null;
            try
            {
                doc = XDocument.Parse(control.UserControlParameters);
            }
            catch (XmlException)
            {
                return;
            }

            var maxLenght = doc.XPathSelectElement("form/field[@guid = '30ca5076-c48f-44c3-b359-be02b63ae81a' and @column = 'Maxlenght']");
            var minLenght = doc.XPathSelectElement("form/field[@guid = '53002493-c0a1-456b-8507-921e74051993' and @column = 'Minlenght']");

            var maxLenghtFieldCaption = doc.XPathSelectElement("form/field[@guid = '30ca5076-c48f-44c3-b359-be02b63ae81a' and @column = 'Maxlenght']/properties/fieldcaption");
            var minLenghtFieldCaption = doc.XPathSelectElement("form/field[@guid = '53002493-c0a1-456b-8507-921e74051993' and @column = 'Minlenght']/properties/fieldcaption");

            maxLenghtFieldCaption?.SetValue("Maxlength");
            minLenghtFieldCaption?.SetValue("Minlength");
            maxLenght?.SetAttributeValue("column", "Maxlength");
            minLenght?.SetAttributeValue("column", "Minlength");

            if (minLenght != null || maxLenght != null || minLenghtFieldCaption != null || maxLenghtFieldCaption != null)
            {
                control.UserControlParameters = doc.ToString();
            }
        }


        private static void ChangeRecaptchaAssemblyAndParameters(FormUserControlInfo formControl)
        {        
                formControl.UserControlFileName = String.Empty;
                formControl.UserControlAssemblyName = "CMS.FormEngine.Web.UI";
                formControl.UserControlClassName = "CMS.FormEngine.Web.UI.RecaptchaControl";
                formControl.UserControlParameters = @"<form version=""2""><category name=""General""><properties><visible>true</visible></properties></category><field allowempty=""true"" column=""Theme"" columnsize=""10"" columntype=""text"" guid=""f51ccf2d-a926-4eb9-8863-8687658c4559"" publicfield=""false"" resolvedefaultvalue=""False"" visible=""true""><properties><defaultvalue>light</defaultvalue><fieldcaption>Control theme</fieldcaption><fielddescription>Choose a design theme for the control.</fielddescription></properties><settings><controlname>DropDownListControl</controlname><DisplayActualValueAsItem>False</DisplayActualValueAsItem><EditText>False</EditText><Options>light;Light
dark;Dark</Options><SortItems>False</SortItems></settings></field><field allowempty=""true"" column=""Type"" columnsize=""10"" columntype=""text"" guid=""172e6dd5-d22b-4447-8655-fa1572371807"" publicfield=""false"" resolvedefaultvalue=""False"" visible=""true""><properties><defaultvalue>image</defaultvalue><fieldcaption>Type</fieldcaption><fielddescription>The type of CAPTCHA to serve.</fielddescription></properties><settings><controlname>DropDownListControl</controlname><DisplayActualValueAsItem>False</DisplayActualValueAsItem><EditText>False</EditText><Options>image;Image
audio;Audio</Options><SortItems>False</SortItems></settings></field><field allowempty=""true"" column=""Size"" columnsize=""10"" columntype=""text"" guid=""a60fce38-8179-40f0-8334-af4c55379cd7"" publicfield=""false"" resolvedefaultvalue=""False"" visible=""true""><properties><defaultvalue>normal</defaultvalue><fieldcaption>Size</fieldcaption><fielddescription>The size of the reCAPTCHA widget.</fielddescription></properties><settings><controlname>DropDownListControl</controlname><DisplayActualValueAsItem>False</DisplayActualValueAsItem><EditText>False</EditText><Options>normal;Normal
compact;Compact</Options><SortItems>False</SortItems></settings></field></form>";
        }


        private static void DisallowNoneOptionInConsentAgreement(FormUserControlInfo formControl)
        {
            formControl.UserControlParameters = @"<form version=""2""><field column=""Consent"" columnsize=""200"" columntype=""text"" guid=""f361f020-4bb4-40b4-b383-c70324c987ca"" publicfield=""false"" resolvedefaultvalue=""False"" visible=""true""><properties><fieldcaption>Consent</fieldcaption></properties><settings><AddGlobalObjectNamePrefix>False</AddGlobalObjectNamePrefix><AddGlobalObjectSuffix>False</AddGlobalObjectSuffix><AllowAll>False</AllowAll><AllowDefault>False</AllowDefault><AllowEditTextBox>False</AllowEditTextBox><AllowEmpty>False</AllowEmpty><controlname>Uni_selector</controlname><DialogWindowName>SelectionDialog</DialogWindowName><EditDialogWindowHeight>700</EditDialogWindowHeight><EditDialogWindowWidth>1000</EditDialogWindowWidth><EditWindowName>EditWindow</EditWindowName><EncodeOutput>True</EncodeOutput><GlobalObjectSuffix ismacro=""true"">{$general.global$}</GlobalObjectSuffix><ItemsPerPage>25</ItemsPerPage><LocalizeItems>True</LocalizeItems><MaxDisplayedItems>25</MaxDisplayedItems><MaxDisplayedTotalItems>50</MaxDisplayedTotalItems><ObjectType>cms.consent</ObjectType><RemoveMultipleCommas>False</RemoveMultipleCommas><ReturnColumnName>ConsentName</ReturnColumnName><ReturnColumnType>id</ReturnColumnType><SpecialFields>;{$general.selectnone$}</SpecialFields><UseDefaultNameFilter>True</UseDefaultNameFilter><ValuesSeparator>;</ValuesSeparator></settings></field><field allowempty=""true"" column=""ConsentReferenceMarkup"" columntype=""longtext"" guid=""ca8a7a6c-70b7-48cd-8f0b-eb0622cda63c"" publicfield=""false"" resolvedefaultvalue=""False"" visible=""true""><properties><fieldcaption>Consent reference markup</fieldcaption><fielddescription>HTML content placed after the short text version of the consent. Can be used to display a pop-up window or a link to a page with the full text version of the consent.</fielddescription></properties><settings><controlname>LargeTextArea</controlname></settings></field></form>";
        }

        #endregion
    }
}