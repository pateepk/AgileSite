using System;
using System.Collections.Generic;
using System.Linq;

using CMS.DataEngine;
using CMS.FormEngine;
using CMS.FormEngine.Web.UI;
using CMS.Helpers;

namespace CMS.UIControls
{
    /// <summary>
    /// Base control for form builder.
    /// </summary>
    public class BaseFormBuilder : BaseFieldEditor
    {
        #region "Constants"

        // Limit for the length of automatically generated field names
        private const int FIELD_NAME_LIMIT = 100;

        #endregion


        #region "Protected methods"

        /// <summary>
        /// Prepares new field.
        /// </summary>
        /// <param name="controlName">Code name of used control</param>
        protected FormFieldInfo PrepareNewField(string controlName)
        {
            FormFieldInfo ffi = new FormFieldInfo();

            string[] controlDefaultDataType = FormUserControlInfoProvider.GetUserControlDefaultDataType(controlName);
            ffi.DataType = controlDefaultDataType[0];
            ffi.Size = ValidationHelper.GetInteger(controlDefaultDataType[1], 0);
            ffi.Precision = DataTypeManager.GetDataType(TypeEnum.Field, ffi.DataType)?.DefaultPrecision ?? -1;
            ffi.FieldType = FormFieldControlTypeEnum.CustomUserControl;

            FormUserControlInfo control = FormUserControlInfoProvider.GetFormUserControlInfo(controlName);
            if (control != null)
            {
                ffi.SetPropertyValue(FormFieldPropertyEnum.FieldCaption, control.UserControlDisplayName);

                // Set default values of control's visible properties to the new field
                LoadDefaultValuesOfControlVisibleProperties(ffi, control);
            }

            ffi.AllowEmpty = true;
            ffi.PublicField = true;
            ffi.Name = GetUniqueFieldName(controlName);
            ffi.Settings["controlname"] = controlName;

            // For list controls create three default options
            if (FormHelper.HasListControl(ffi))
            {
                SpecialFieldsDefinition optionDefinition = new SpecialFieldsDefinition();

                for (int i = 1; i <= 3; i++)
                {
                    optionDefinition.Add(new SpecialField
                    {
                        Value = OptionsDesigner.DEFAULT_OPTION + i,
                        Text = OptionsDesigner.DEFAULT_OPTION + i
                    });
                }

                ffi.Settings["Options"] = optionDefinition.ToString();
            }

            if (controlName.Equals("CalendarControl", StringComparison.OrdinalIgnoreCase))
            {
                ffi.Settings["EditTime"] = false;
            }

            return ffi;
        }


        /// <summary>
        /// Saves form definition. Updates database column if both original and changed info is passed and the change requires database update.
        /// </summary>
        /// <param name="oldFieldInfo">Form field info prior to the change</param>
        /// <param name="updatedFieldInfo">Form field info after the change has been made.</param>
        /// <returns>Error message if an error occurred</returns>
        protected string SaveFormDefinitionInternal(FormFieldInfo oldFieldInfo = null, FormFieldInfo updatedFieldInfo = null)
        {
            DataClassInfo dci = DataClassInfoProvider.GetDataClassInfo(ClassName);

            if ((FormInfo != null) && (dci != null))
            {
                if ((oldFieldInfo != null) && (updatedFieldInfo != null))
                {
                    // Ensure nice field name based on the field caption
                    updatedFieldInfo.Name = EnsureNiceFieldName(oldFieldInfo, updatedFieldInfo);
                }

                RaiseBeforeDefinitionUpdate();

                // Update form definition   
                dci.ClassFormDefinition = FormInfo.GetXmlDefinition();

                // Save the class data
                DataClassInfoProvider.SetDataClassInfo(dci);

                ClearHashtables();

                RaiseAfterDefinitionUpdate();

                // Update inherited classes with new fields
                FormHelper.UpdateInheritedClasses(dci);

                return string.Empty;
            }

            return GetString("FormBuilder.ErrorSavingForm");
        }


        /// <summary>
        /// Adds form field info to the form to the specified position.
        /// </summary>
        /// <param name="ffi">Form field info which will be added</param>
        /// <param name="category">Category name</param>
        /// <param name="position">Field position in the category</param>
        protected string AddFieldInternal(FormFieldInfo ffi, string category, int position)
        {
            var dci = DataClassInfoProvider.GetDataClassInfo(ClassName);
            if (dci != null)
            {
                RaiseBeforeDefinitionUpdate();

                // Add field to form  
                FormInfo.AddFormItem(ffi);
                if (!String.IsNullOrEmpty(category) || position >= 0)
                {
                    FormInfo.MoveFormFieldToPositionInCategory(ffi.Name, category, position);
                }

                // Update form definition
                dci.ClassFormDefinition = FormInfo.GetXmlDefinition();

                // Ensure the transaction
                using (var tr = new CMSTransactionScope())
                {
                    try
                    {
                        // Save the class data
                        DataClassInfoProvider.SetDataClassInfo(dci);
                    }
                    catch (Exception)
                    {
                        return GetString("FormBuilder.ErrorSavingForm");
                    }

                    QueryInfoProvider.ClearDefaultQueries(dci, true, true);

                    // Hide field for alternative forms that require it
                    FormHelper.HideFieldInAlternativeForms(ffi, dci);

                    // Commit the transaction
                    tr.Commit();
                }

                ClearHashtables();

                RaiseAfterDefinitionUpdate();

                // Update inherited classes with new fields
                FormHelper.UpdateInheritedClasses(dci);
            }
            else
            {
                return GetString("FormBuilder.ErrorSavingForm");
            }

            return string.Empty;
        }


        /// <summary>
        /// Clones given field <paramref name="ffi"/>, adds the clone next to the original field and returns name of the new field in <paramref name="newFieldName"/> out parameter.
        /// </summary>
        /// <param name="ffi">Form field to be cloned</param>
        /// <param name="newFieldName">Field name of new field</param>
        protected string CloneFieldInternal(FormFieldInfo ffi, out string newFieldName)
        {
            if (ffi == null)
            {
                throw new ArgumentNullException(nameof(ffi));
            }

            var clone = (FormFieldInfo)ffi.Clone();
            var controlName = clone.Settings["controlname"].ToString();

            // Set unique name based on field's control name...
            clone.Name = newFieldName = GetUniqueFieldName(controlName);
            // ...and update unique identifier
            clone.Guid = Guid.NewGuid();

            // Get parent category and new position
            var category = FormInfo.GetClosestCategory(ffi);
            var fieldIndex = FormInfo.ItemsList.IndexOf(ffi);
            var categoryIndex = FormInfo.ItemsList.IndexOf(category);

            // Get number of hidden fields in front of new position in the category
            var hiddenFieldsCount = 0;
            for (var i = fieldIndex - 1; i > categoryIndex; i--)
            {
                var field = FormInfo.ItemsList[i] as FormFieldInfo;
                if ((field != null) && !field.Visible)
                {
                    hiddenFieldsCount++;
                }
            }

            return AddFieldInternal(clone, category?.CategoryName, fieldIndex - categoryIndex - hiddenFieldsCount);
        }


        /// <summary>
        /// Moves fields specified by <paramref name="fieldNames"/> to new position in category with name <paramref name="category"/>.
        /// </summary>
        /// <param name="fieldNames">Field names to move</param>
        /// <param name="category">Name of category to which the fields will be moved; if empty position is taken from the beginning of the form</param>
        /// <param name="position">Position in category/form to which the fields will be moved; starts from 0</param>
        /// <remarks>Returns an error message in case any field from <paramref name="fieldNames"/> doesn't exist. Method modifies current form definition but does not save the changes!</remarks>
        protected string MoveFieldsInternal(IEnumerable<string> fieldNames, string category, int position)
        {
            var errorMessage = "";
            var isFirst = true;
            var index = 0;
            var formItemList = FormInfo.ItemsList;

            foreach (var fieldName in fieldNames)
            {
                var field = FormInfo.GetFormField(fieldName);
                if (field != null)
                {
                    if (isFirst)
                    {
                        // Move first field to new position in given category and get its new index
                        FormInfo.MoveFormFieldToPositionInCategory(fieldName, category, position);
                        index = formItemList.IndexOf(field);
                    }
                    else
                    {
                        // Move other fields behind the previous ones
                        var currentIndex = formItemList.IndexOf(field);

                        formItemList.Remove(field);
                        index += (currentIndex > index ? 1 : 0);
                        formItemList.Insert(index, field);
                    }
                }
                else
                {
                    errorMessage = GetString("editedobject.notexists");
                    break;
                }

                isFirst = false;
            }

            return errorMessage;
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Ensures unique field name.
        /// </summary>
        /// <param name="name">Field name</param>
        private string GetUniqueFieldName(string name)
        {
            int uniqueIndex = 1;
            bool unique = false;
            string uniqueName = name;

            while (!unique)
            {
                if (FormInfo.GetFormField(uniqueName) == null)
                {
                    unique = true;
                }
                else
                {
                    uniqueName = name + "_" + uniqueIndex;
                    uniqueIndex++;
                }
            }
            return uniqueName;
        }


        /// <summary>
        /// Changes field name based on caption when the caption is changed firstly.
        /// </summary>
        /// <param name="oldInfo">Original field info</param>
        /// <param name="newInfo">Changed field info</param>
        private string EnsureNiceFieldName(FormFieldInfo oldInfo, FormFieldInfo newInfo)
        {
            if (newInfo == null)
            {
                return null;
            }

            string oldCaption = oldInfo?.GetPropertyValue(FormFieldPropertyEnum.FieldCaption) ?? "";
            string newCaption = newInfo.GetPropertyValue(FormFieldPropertyEnum.FieldCaption);
            string controlName = newInfo.Settings["controlname"].ToString();

            if (!newInfo.Name.StartsWith(controlName, StringComparison.InvariantCultureIgnoreCase) ||
                String.IsNullOrWhiteSpace(newCaption) ||
                oldCaption.Equals(newCaption, StringComparison.InvariantCultureIgnoreCase))
            {
                // Don't change field name if it's already been changed or it's empty or it wasn't changed this time
                return newInfo.Name;
            }

            newCaption = ResHelper.LocalizeString(newCaption);

            // Try to make new field name in identifier form from its caption and ensure its uniqueness
            string caption = ValidationHelper.GetCodeName(newCaption, useUnicode: false);
            if (!ValidationHelper.IsIdentifier(caption))
            {
                caption = ValidationHelper.GetIdentifier(newCaption);
            }

            // Return original field name for non-latin captions
            return ValidationHelper.IsIdentifier(caption.Trim('_')) ? GetUniqueFieldName(caption.Truncate(FIELD_NAME_LIMIT)) : newInfo.Name;
        }


        /// <summary>
        /// Clears HashTables.
        /// </summary>
        private void ClearHashtables()
        {
            // Clear the object type hashtable
            AbstractProviderDictionary.ReloadDictionaries(ClassName, true);

            // Clear the classes hashtable
            AbstractProviderDictionary.ReloadDictionaries("cms.class", true);

            // Clear class structures
            ClassStructureInfo.Remove(ClassName, true);

            // Clear form resolver
            FormEngineWebUIResolvers.ClearResolvers(true);
        }


        private void LoadDefaultValuesOfControlVisibleProperties(FormFieldInfo fieldInfo, FormUserControlInfo control)
        {
            // Get form info for current control
            var controlFormInfo = FormHelper.GetFormControlParameters(control.UserControlCodeName, control.UserControlMergedParameters, true);

            if (controlFormInfo != null)
            {
                // Get visible properties with some default value
                var properties = controlFormInfo.GetFields<FormFieldInfo>()
                                                .Where(x => x.Visible && !string.IsNullOrEmpty(x.GetPropertyValue(FormFieldPropertyEnum.DefaultValue)));

                foreach (var ffi in properties)
                {
                    bool isMacro;
                    var value = ffi.GetPropertyValue(FormFieldPropertyEnum.DefaultValue, out isMacro);

                    fieldInfo.Settings[ffi.Name] = isMacro ? null : value;
                    fieldInfo.SettingsMacroTable[ffi.Name] = isMacro ? value : null;
                }
            }
        }

        #endregion
    }
}
