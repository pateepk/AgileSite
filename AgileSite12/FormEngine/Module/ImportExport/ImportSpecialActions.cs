using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

using CMS.CMSImportExport;
using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.FormEngine
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
            SpecialActionsEvents.ProcessMainObject.Before += ProcessMainObject_Before;

            SpecialActionsEvents.ImportLoadDefaultSelection.Execute += LoadDefaultSelection_Execute;
        }


        /// <summary>
        /// Provides additional actions when importing objects
        /// </summary>
        private static void ProcessMainObject_Before(object sender, ImportEventArgs e)
        {
            var settings = e.Settings;
            var infoObj = e.Object;
            var parameters = e.Parameters;
            var existing = parameters.ExistingObject;

            using (new ImportSpecialCaseContext(settings))
            {
                if (parameters.SkipObjectUpdate || (parameters.ObjectProcessType != ProcessObjectEnum.All))
                {
                    return;
                }

                switch (infoObj.TypeInfo.ObjectType)
                {
                    // Update BizForm class 
                    case PredefinedObjectType.BIZFORM:
                        if (parameters.UpdateChildObjects)
                        {
                            ImportFormClass(e);
                        }
                        break;
                }

                // Update form definition of classes
                if (infoObj is DataClassInfo dci)
                {
                    UpdateDataClass(settings, dci, existing);

                    // Has to run after the form definition merging in UpdateSystemTable method
                    FixHtml5InputControlTyposInDataClass(settings, dci);
                }

                // Update existing alt.forms
                if (infoObj is AlternativeFormInfo afi)
                {
                    if (existing != null)
                    {
                        UpdateAltForm(settings, afi, existing);
                    }

                    // Has to run after all form definition merging
                    FixHtml5InputControlTyposInAlternativeForm(settings, afi);
                }
            }
        }


        /// <summary>
        /// Imports the form data class
        /// </summary>
        private static void ImportFormClass(ImportEventArgs e)
        {
            var settings = e.Settings;
            var infoObj = e.Object;
            var th = e.TranslationHelper;
            var parameters = e.Parameters;
            var existing = parameters.ExistingObject;

            // Update the class
            var classDT = parameters.Data.Tables["CMS_Class"];

            if (!DataHelper.DataSourceIsEmpty(classDT))
            {
                // Get the data
                int formClassId = parameters.GetValue("formClassId", 0);

                var classRow = classDT.Select("ClassID = " + formClassId);
                if (classRow.Length > 0)
                {
                    // Update class object
                    var ci = DataClassInfo.New(classRow[0]);
                    var className = ci.ClassName;

                    if (infoObj.Generalized.ObjectID == 0)
                    {
                        var tm = new TableManager(ci.ClassConnectionString);

                        // New - create unique class
                        ci.ClassGUID = Guid.NewGuid();
                        ci.ClassTableName = tm.GetUniqueTableName(ci.ClassTableName);
                        ci.ClassName = DataClassInfoProvider.GetUniqueClassName(className);
                    }
                    else if (existing != null)
                    {
                        // Get data from existing class
                        var existingClass = ci.Generalized.GetExisting() as DataClassInfo;
                        if (existingClass != null)
                        {
                            ci.ClassGUID = existingClass.ClassGUID;
                            ci.ClassTableName = existingClass.ClassTableName;
                            ci.ClassName = existingClass.ClassName;
                        }
                    }

                    // Change class translation
                    th.ChangeCodeName(new TranslationParameters(infoObj.TypeInfo) { CodeName = className }, ci.ClassName);

                    var importParameters = new ImportParameters
                    {
                        TranslationHelper = parameters.TranslationHelper,
                        SiteObject = false,
                        Data = parameters.Data,
                        UpdateChildObjects = true,
                        ObjectProcessType = ProcessObjectEnum.All,
                        PostProcessList = parameters.PostProcessList,
                        ImportedObjects = parameters.ImportedObjects,
                        PostProcessing = parameters.PostProcessing,
                        ParentObject = infoObj
                    };

                    var result = ImportProvider.ImportObject(settings, ci, importParameters, null);

                    var existingClassInfo = result.SomeDataImported
                        ? DataClassInfoProvider.GetDataClassInfo(ci.ClassGUID)
                        : (DataClassInfo)ci.Generalized.GetExisting();

                    // Set new class ID
                    infoObj.SetValue("FormClassID", existingClassInfo.ClassID);
                }
            }
        }


        /// <summary>
        /// Updates the DataClass within the import process
        /// </summary>
        /// <param name="settings">Import settings</param>
        /// <param name="dci">Imported class</param>
        /// <param name="existing">Existing class</param>
        /// <exception cref="DataClassTableUpdateException">Thrown when import would require updating a table, but the table is not associated with the provided <paramref name="dci"/>.</exception>
        private static void UpdateDataClass(SiteImportSettings settings, DataClassInfo dci, BaseInfo existing)
        {
            if (existing != null)
            {
                var existingClass = (DataClassInfo)existing;

                // Special handling for system table (only import custom fields)
                if (existingClass.ClassShowAsSystemTable)
                {
                    UpdateSystemTable(settings, dci, existingClass);
                }
            }

            // Update table
            if (dci.ClassIsCoupledClass && (dci.ClassFormDefinition == ""))
            {
                // Cannot be neither system nor custom table if form definition not present
                dci.ClassShowAsSystemTable = false;
                dci.ClassIsCustomTable = false;
            }
        }


        /// <summary>
        /// Updates the imported system table class
        /// </summary>
        /// <param name="settings">Import settings</param>
        /// <param name="dci">Imported class</param>
        /// <param name="existingClass">Existing class</param>
        private static void UpdateSystemTable(SiteImportSettings settings, DataClassInfo dci, DataClassInfo existingClass)
        {
            // Keep all existing properties except for form definition and customized columns (other properties are system ones)
            if (settings.IsOlderVersion)
            {
                CopySystemColumns(dci, existingClass, new[] { "ClassFormDefinition" });
            }

            if (dci.ClassFormDefinition != "")
            {
                // Get the definitions to combine
                var oldFi = FormHelper.GetFormInfo(existingClass.ClassName, true);
                var newFi = new FormInfo(dci.ClassFormDefinition);

                // For import from older version, import only non-system columns so that it doesn't modify the system fields configuration
                if (settings.IsOlderVersion)
                {
                    ApplyCustomFields(oldFi, newFi);

                    dci.ClassFormDefinition = oldFi.GetXmlDefinition();
                }
                else
                {
                    // For import from the same version, apply extra fields from existing version to the new version, keep others new
                    newFi.CombineWithForm(oldFi, false);

                    dci.ClassFormDefinition = newFi.GetXmlDefinition();
                }
            }
            else
            {
                dci.ClassFormDefinition = existingClass.ClassFormDefinition;
            }
        }


        /// <summary>
        /// Copies the system columns from existing object to the imported object
        /// </summary>
        /// <param name="obj">Imported object</param>
        /// <param name="existingObj">Existing object</param>
        /// <param name="excludeColumns">Excluded columns</param>
        private static void CopySystemColumns(BaseInfo obj, BaseInfo existingObj, IEnumerable<string> excludeColumns)
        {
            // Do not overwrite customized columns in the imported class, nor form definition which will be combined
            var exclude = obj.Generalized.CustomizedColumns.Union(excludeColumns);

            // Merge customized columns from both classes
            var customizedColumns = obj.Generalized.CustomizedColumns.Union(existingObj.Generalized.CustomizedColumns).ToArray();
            obj.Generalized.CustomizedColumns = new ReadOnlyCollection<string>(customizedColumns);

            // Copy all current columns except for excluded
            foreach (var column in existingObj.ColumnNames.Except(exclude))
            {
                obj.SetValue(column, existingObj.GetValue(column));
            }
        }


        /// <summary>
        /// Updates an existing alternative form.
        /// </summary>
        /// <param name="settings">Import settings</param>
        /// <param name="afi">Imported form</param>
        /// <param name="existing">Existing form</param>
        private static void UpdateAltForm(SiteImportSettings settings, AlternativeFormInfo afi, BaseInfo existing)
        {
            // Get the form class
            var mainDci = DataClassInfoProvider.GetDataClassInfo(afi.FormClassID);
            if (!mainDci.ClassShowAsSystemTable)
            {
                // If the form does not belong to system table, import normally
                return;
            }

            var existingForm = (AlternativeFormInfo)existing;

            // Keep all existing properties except for form definition and customized columns (other properties are system ones)
            if (settings.IsOlderVersion)
            {
                CopySystemColumns(afi, existingForm, new[] { "FormDefinition" });
            }

            var classFormDefinition = mainDci.ClassFormDefinition;

            if (afi.FormCoupledClassID > 0)
            {
                // If coupled class is defined combine form definitions
                var coupledDci = DataClassInfoProvider.GetDataClassInfo(afi.FormCoupledClassID);
                if (coupledDci != null)
                {
                    classFormDefinition = FormHelper.MergeFormDefinitions(classFormDefinition, coupledDci.ClassFormDefinition);
                }
            }

            // Prepare old and new form definitions
            var oldFormDef = FormHelper.MergeFormDefinitions(classFormDefinition, existingForm.FormDefinition);
            var newFormDef = FormHelper.MergeFormDefinitions(classFormDefinition, afi.FormDefinition);

            var oldFi = new FormInfo(oldFormDef);
            var newFi = new FormInfo(newFormDef);

            // For import from an older version, import only non-system columns so that it doesn't modify the system fields configuration
            if (settings.IsOlderVersion)
            {
                // Apply only the custom fields
                ApplyCustomFields(oldFi, newFi);
            }
            // For import from the same version, overwrite fields modified by imported alt.form
            else
            {
                // Remove fields that are not influenced by the imported alt.form
                var altFormFields = GetAltFormFieldNames(afi.FormDefinition);
                newFi.RemoveFields(f => !altFormFields.Contains(f.Name, StringComparer.InvariantCultureIgnoreCase));

                oldFi.CombineWithForm(newFi, true);
            }

            // Save the difference in definitions
            afi.FormDefinition = FormHelper.GetFormDefinitionDifference(classFormDefinition, oldFi.GetXmlDefinition(), true);
        }


        /// <summary>
        /// Returns field names from given alt.form definition.
        /// </summary>
        /// <param name="definition">Alt.form definition</param>
        private static IEnumerable<string> GetAltFormFieldNames(string definition)
        {
            HashSet<string> result = new HashSet<string>();

            if (!String.IsNullOrEmpty(definition))
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(definition);

                XmlElement root = doc.DocumentElement;
                if (root != null)
                {
                    foreach (XmlNode node in root.ChildNodes)
                    {
                        if (node.LocalName.Equals("field", StringComparison.InvariantCultureIgnoreCase))
                        {
                            string name = XmlHelper.GetAttributeValue(node, "column");
                            if (!String.IsNullOrEmpty(name))
                            {
                                result.Add(name);
                            }
                        }
                    }
                }
            }

            return result;
        }


        /// <summary>
        /// Applies custom fields from the new form definition to the old form definition. Leaves system fields from the old form definition.
        /// </summary>
        /// <param name="oldFi">Old form definition</param>
        /// <param name="newFi">New form definition</param>
        private static void ApplyCustomFields(FormInfo oldFi, FormInfo newFi)
        {
            oldFi.RemoveFields(f => !f.System);
            newFi.RemoveFields(f => f.System);

            // Combine forms, new fields have priority
            oldFi.CombineWithForm(newFi, new CombineWithFormSettings
            {
                IncludeCategories = true,
                RemoveEmptyCategories = true
            });
        }


        /// <summary>
        /// Loads the default selection for form controls
        /// </summary>
        private static void LoadDefaultSelection_Execute(object sender, ImportLoadSelectionArgs e)
        {
            var objectType = e.ObjectType;
            var siteObject = e.SiteObject;
            var settings = e.Settings;

            switch (objectType)
            {
                // Do not preselect for older version
                case FormUserControlInfo.OBJECT_TYPE:
                    {
                        DefaultSelectionParameters parameters = new DefaultSelectionParameters
                        {
                            ObjectType = objectType,
                            SiteObjects = siteObject,
                        };

                        if (!settings.IsOlderHotfixVersion || (settings.ImportType == ImportTypeEnum.None) || (settings.ImportType == ImportTypeEnum.AllForced))
                        {
                            parameters.ImportType = ImportTypeEnum.Default;
                        }
                        else
                        {
                            parameters.ImportType = ImportTypeEnum.New;
                        }
                        settings.LoadDefaultSelection(parameters);

                        // Cancel default selection
                        e.Select = false;
                    }
                    break;
            }
        }


        /// <summary>
        /// Fixes typos in data class form definition caused by Html5Input form control which might occur in version 11 and hotfix number 0, 1, 2 or 3.
        /// </summary>
        private static void FixHtml5InputControlTyposInDataClass(SiteImportSettings settings, DataClassInfo existingClass)
        {
            if (settings.Version.StartsWith("11", StringComparison.Ordinal) && ValidationHelper.GetInteger(settings.HotfixVersion, 0) < 4)
            {
                existingClass.ClassFormDefinition = FixHtml5InputControlTyposInFormDefinition(existingClass.ClassFormDefinition);
            }
        }


        /// <summary>
        /// Fixes typos in alternative form definition caused by Html5Input form control which might occur in version 11 and hotfix number 0, 1, 2 or 3.
        /// </summary>
        private static void FixHtml5InputControlTyposInAlternativeForm(SiteImportSettings settings, AlternativeFormInfo afi)
        {
            if (settings.Version.StartsWith("11", StringComparison.Ordinal) && ValidationHelper.GetInteger(settings.HotfixVersion, 0) < 4)
            {
                afi.FormDefinition = FixHtml5InputControlTyposInFormDefinition(afi.FormDefinition);
            }
        }


        /// <summary>
        /// Fixes typos caused by HTML5Input form control.
        /// </summary>
        /// <param name="formDefinition">Form definition in xml format possibly containing typo.</param>
        internal static string FixHtml5InputControlTyposInFormDefinition(string formDefinition)
        {
            if (String.IsNullOrWhiteSpace(formDefinition))
            {
                return formDefinition;
            }

            XDocument doc = null;
            try
            {
                doc = XDocument.Parse(formDefinition);
            }
            catch (XmlException)
            {
                return formDefinition;
            }

            foreach (var element in doc.XPathSelectElements("form/field/settings[controlname/text() = 'HTML5Input']/Maxlenght"))
            {
                element.Name = "Maxlength";
            }

            foreach (var element in doc.XPathSelectElements("form/field/settings[controlname/text() = 'HTML5Input']/Minlenght"))
            {
                element.Name = "Minlength";
            }

            return doc.ToString();
        }

        #endregion
    }
}