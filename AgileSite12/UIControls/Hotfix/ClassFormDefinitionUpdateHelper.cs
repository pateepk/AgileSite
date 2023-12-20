using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS.Base;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.FormEngine;
using CMS.Helpers;

namespace CMS.UIControls
{
    /// <summary>
    /// Provides methods for updating class form definitions and alternative form definitions during upgrade or hotfix processes.
    /// </summary>
    public class ClassFormDefinitionUpdateHelper
    {
        private const string FORM_DEFINITION_TABLE = "Temp_FormDefinition";
        private const string FORM_DEFINITION_NAME_COLUMN = "ObjectName";
        private const string FORM_DEFINITION_VALUE_COLUMN = "FormDefinition";

        private TableManager mTableManager;
        private readonly string mEventLogSource;


        private TableManager TableManager
        {
            get
            {
                return mTableManager ?? (mTableManager = new TableManager(null));
            }
        }


        /// <summary>
        /// Constructor. Ensures all events logged within actions of this object will use given source.
        /// </summary>
        /// <param name="eventLogSource">Name of event log source to be used for logging</param>
        public ClassFormDefinitionUpdateHelper(string eventLogSource)
        {
            mEventLogSource = eventLogSource;
        }


        /// <summary>
        /// Updates class form and alt.form definitions based on records stored in temporary table "Temp_FormDefinition" and refreshes document and system database views.
        /// The temporary table is created by upgrade or hotfix database scripts and is dropped at the end of this method.
        /// </summary>
        public void UpdateClassFormDefinitions()
        {
            using (var context = new CMSActionContext())
            {
                context.DisableLogging();
                context.CreateVersion = false;
                context.LogIntegration = false;
                context.ContinuousIntegrationAllowObjectSerialization = false;

                if (TableManager.TableExists(FORM_DEFINITION_TABLE))
                {
                    UpdateClasses();
                    UpdateAlternativeForms();

                    DropTempTable();
                }
            }
        }


        /// <summary>
        /// Updates class form definitions.
        /// </summary>
        private void UpdateClasses()
        {
            DataSet classes = GetFormDefinitions();
            if (DataHelper.DataSourceIsEmpty(classes))
            {
                return;
            }

            foreach (DataRow row in classes.Tables[0].Rows)
            {
                string objectName = DataHelper.GetStringValue(row, FORM_DEFINITION_NAME_COLUMN);
                string newDefinition = DataHelper.GetStringValue(row, FORM_DEFINITION_VALUE_COLUMN);

                if (string.IsNullOrEmpty(objectName) || string.IsNullOrEmpty(newDefinition))
                {
                    continue;
                }

                var dataClass = DataClassInfoProvider.GetDataClassInfo(objectName);
                if (dataClass == null)
                {
                    continue;
                }

                try
                {
                    var newVersionFi = new FormInfo(newDefinition);
                    var oldVersionFi = new FormInfo(dataClass.ClassFormDefinition);

                    // Get removed system fields
                    var removedfields = GetRemovedSystemFields(oldVersionFi, newVersionFi);

                    // Remove the system fields from class's alt.forms
                    foreach (var field in removedfields)
                    {
                        FormHelper.RemoveFieldFromAlternativeForms(dataClass, field, 0);
                    }

                    // Copy custom fields only for system tables
                    if (dataClass.ClassShowAsSystemTable)
                    {
                        CopyCustomFields(oldVersionFi, newVersionFi, false);
                    }

                    // Save the modified form definition
                    dataClass.ClassFormDefinition = newVersionFi.GetXmlDefinition();

                    // Save the new definition
                    dataClass.Update();
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException("Updating data class '" + dataClass.ClassName + "' failed.", e);
                }
            }

            // The components may have been upgraded later than the composite class
            CleanCompositeClassesSearchSettings();
        }


        /// <summary>
        /// Removes search settings for columns that no longer exist from classes consisting of multiple components.
        /// </summary>
        private static void CleanCompositeClassesSearchSettings()
        {
            foreach (var classInfo in DataClassInfoProvider.GetClasses())
            {
                classInfo.RemoveObsoleteSearchSettings();
                classInfo.Update();
            }
        }


        /// <summary>
        /// Updates an existing alternative forms form definitions. Appends existing custom fields to new version definitions.
        /// </summary>
        private void UpdateAlternativeForms()
        {
            DataSet classes = GetFormDefinitions(true);
            if (DataHelper.DataSourceIsEmpty(classes))
            {
                return;
            }

            foreach (DataRow row in classes.Tables[0].Rows)
            {
                string objectName = DataHelper.GetStringValue(row, FORM_DEFINITION_NAME_COLUMN);
                string newDefinition = DataHelper.GetStringValue(row, FORM_DEFINITION_VALUE_COLUMN);

                if (!string.IsNullOrEmpty(objectName))
                {
                    var altForm = AlternativeFormInfoProvider.GetAlternativeFormInfo(objectName);
                    if (altForm != null)
                    {
                        var mainDci = DataClassInfoProvider.GetDataClassInfo(altForm.FormClassID);
                        var classFormDefinition = mainDci.ClassFormDefinition;

                        if (altForm.FormCoupledClassID > 0)
                        {
                            // If coupled class is defined combine form definitions
                            var coupledDci = DataClassInfoProvider.GetDataClassInfo(altForm.FormCoupledClassID);
                            if (coupledDci != null)
                            {
                                classFormDefinition = FormHelper.MergeFormDefinitions(classFormDefinition, coupledDci.ClassFormDefinition);
                            }
                        }

                        var oldVersionDefinition = FormHelper.MergeFormDefinitions(classFormDefinition, altForm.FormDefinition);
                        var newVersionDefinition = FormHelper.MergeFormDefinitions(classFormDefinition, newDefinition);

                        var newVersionFi = new FormInfo(newVersionDefinition);
                        var oldVersionFi = new FormInfo(oldVersionDefinition);

                        CopyCustomFields(oldVersionFi, newVersionFi, true);

                        // Save the modified form definition
                        altForm.FormDefinition = FormHelper.GetFormDefinitionDifference(classFormDefinition, newVersionFi.GetXmlDefinition(), true);
                        altForm.Update();
                    }
                }
            }
        }


        /// <summary>
        /// Copies custom fields from old version of form definition to the new form definition.
        /// </summary>
        /// <param name="oldVersionFi">Old version form definition</param>
        /// <param name="newVersionFi">New version form definition</param>
        /// <param name="overwrite">Indicates whether existing fields should be overwritten. Alternative form fields need to be overwritten due to the combination with upgraded class form.</param>
        private static void CopyCustomFields(FormInfo oldVersionFi, FormInfo newVersionFi, bool overwrite)
        {
            // Remove all system fields from old definition to get only custom fields
            oldVersionFi.RemoveFields(f => f.System);

            // Combine forms so that custom fields from old definition are appended to the new definition
            newVersionFi.CombineWithForm(oldVersionFi, new CombineWithFormSettings
            {
                IncludeCategories = false,
                RemoveEmptyCategories = true,
                OverwriteExisting = overwrite
            });
        }


        /// <summary>
        /// Returns list with names of system fields which were removed to the new version.
        /// </summary>
        /// <param name="oldDefinition">Old form definition</param>
        /// <param name="newDefinition">New form definition</param>
        private static IEnumerable<string> GetRemovedSystemFields(FormInfo oldDefinition, FormInfo newDefinition)
        {
            if ((oldDefinition != null) && (newDefinition != null))
            {
                var oldSystemFields = oldDefinition.ItemsList.OfType<FormFieldInfo>().Where(f => f.System).Select(f => f.Name);
                var newSystemFields = newDefinition.ItemsList.OfType<FormFieldInfo>().Where(f => f.System).Select(f => f.Name);

                // Get difference of the sets
                return oldSystemFields.Except(newSystemFields);
            }

            return Enumerable.Empty<string>();
        }


        /// <summary>
        /// Returns dataset with class names (or alt.form full names) and form definitions which should be used for the upgrade.
        /// </summary>
        /// <param name="getAltForms">Indicates if alt.form definitions should be returned</param>
        private DataSet GetFormDefinitions(bool getAltForms = false)
        {
            string queryText = $"SELECT [{FORM_DEFINITION_NAME_COLUMN}], [{FORM_DEFINITION_VALUE_COLUMN}] FROM [{FORM_DEFINITION_TABLE}] WHERE {(getAltForms ? "IsAltForm = 1" : "IsAltForm = 0 OR IsAltForm IS NULL")}";

            DataSet ds = null;
            try
            {
                ds = ConnectionHelper.ExecuteQuery(queryText, null, QueryTypeEnum.SQLQuery);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException(mEventLogSource, "GETFORMDEFINITION", ex);
            }

            return ds;
        }


        /// <summary>
        /// Drops temporary table with class form and alt.form definitions.
        /// </summary>
        private void DropTempTable()
        {
            try
            {
                TableManager.DropTable(FORM_DEFINITION_TABLE);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException(mEventLogSource, "DROPTEMPTABLE", ex);
            }
        }
    }
}
