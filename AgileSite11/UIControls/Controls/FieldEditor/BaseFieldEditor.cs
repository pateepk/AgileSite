using System;
using System.Collections;
using System.Linq;

using CMS.Base;
using CMS.FormEngine;

namespace CMS.UIControls
{
    /// <summary>
    /// Base control for field editor and form builder.
    /// </summary>
    public class BaseFieldEditor : CMSUserControl
    {
        #region "Events"

        /// <summary>
        /// Event raised when field name was changed.
        /// </summary>
        public event OnFieldNameChangedEventHandler OnFieldNameChanged;


        /// <summary>
        /// Event raised when OK button is clicked and before xml definition is changed.
        /// </summary>
        public event EventHandler OnBeforeDefinitionUpdate;


        /// <summary>
        /// Event raised when OK button is clicked and after xml definition is changed.
        /// </summary>
        public event EventHandler OnAfterDefinitionUpdate;

        #endregion


        #region "Properties"

        /// <summary>
        /// Name of edited class.
        /// </summary>
        public virtual string ClassName
        {
            get;
            set;
        }


        /// <summary>
        /// Edited form
        /// </summary>
        protected FormInfo FormInfo
        {
            get;
            set;
        }

        #endregion


        #region "Protected methods"
        
        /// <summary>
        /// Indicates whether such a change has been made to the field, it requires database structure update.
        /// </summary>
        /// <param name="oldFieldInfo">Field prior to the change.</param>
        /// <param name="updatedFieldInfo">Field after the change.</param>
        protected static bool IsDatabaseChangeRequired(FormFieldInfo oldFieldInfo, FormFieldInfo updatedFieldInfo)
        {
            if (updatedFieldInfo == null)
            {
                return false;
            }
            if (oldFieldInfo == null)
            {
                return true;
            }

            bool originalIsMacro;
            string originalDefaultValue = oldFieldInfo.GetPropertyValue(FormFieldPropertyEnum.DefaultValue, out originalIsMacro).ToString(String.Empty);

            bool isMacro;
            string defaultValue = updatedFieldInfo.GetPropertyValue(FormFieldPropertyEnum.DefaultValue, out isMacro).ToString(String.Empty);

            return (oldFieldInfo.Name != updatedFieldInfo.Name) ||
                   (oldFieldInfo.DataType != updatedFieldInfo.DataType) ||
                   (oldFieldInfo.Size != updatedFieldInfo.Size) ||
                   (oldFieldInfo.Precision != updatedFieldInfo.Precision) ||
                   (oldFieldInfo.AllowEmpty != updatedFieldInfo.AllowEmpty) ||
                   (originalDefaultValue != defaultValue) ||
                   (originalIsMacro != isMacro);
        }


        /// <summary>
        /// Initializes UI context with "CurrentFormFields".
        /// </summary>
        /// <param name="formInfo">Currently edited form definition</param>
        protected void InitUIContext(FormInfo formInfo)
        {
            if (formInfo != null)
            {
                // Get all fields except the system ones
                var fields = formInfo.GetFields(true, true);

                var result = new ArrayList(fields.Count);

                foreach (var field in fields)
                {
                    bool isMacro;
                    string caption = field.GetPropertyValue(FormFieldPropertyEnum.FieldCaption, out isMacro);

                    if (isMacro || String.IsNullOrEmpty(caption))
                    {
                        caption = field.Name;
                    }

                    if (fields.Any(f => (f.Name != field.Name) && (f.Caption == caption)))
                    {
                        // Add field name if more fields have similar caption
                        caption += $" [{field.Name}]";
                    }

                    result.Add($"{field.Guid};{caption}");
                }

                UIContext["CurrentFormFields"] = result;
                ViewState["CurrentFormFields"] = result;
            }
        }


        /// <summary>
        /// Raises OnFieldNameChanged event if the new name differs from the old one.
        /// </summary>
        /// <param name="oldName">Old field name</param>
        /// <param name="newName">New field name</param>
        protected void RaiseOnFieldNameChanged(string oldName, string newName)
        {
            if (oldName != newName)
            {
                OnFieldNameChanged?.Invoke(this, oldName, newName);
            }
        }


        /// <summary>
        /// Raises the OnBeforeDefinitionUpdate event
        /// </summary>
        protected void RaiseBeforeDefinitionUpdate()
        {
            // Raise on before definition update event
            OnBeforeDefinitionUpdate?.Invoke(this, EventArgs.Empty);
        }


        /// <summary>
        /// Raises the OnAfterDefinitionUpdate event
        /// </summary>
        protected void RaiseAfterDefinitionUpdate()
        {
            // Raise on after definition update event
            OnAfterDefinitionUpdate?.Invoke(this, EventArgs.Empty);
        }


        /// <summary>
        /// Generates the unique category name
        /// </summary>
        protected string GenerateCategoryName()
        {
            var categoryNames = FormInfo.GetFields<FormCategoryInfo>()
                                        .Where(x => !string.IsNullOrEmpty(x.CategoryName))
                                        .Select(x => x.CategoryName)
                                        .ToList();

            var current = categoryNames.Count + 1;

            while (categoryNames.Contains("Category_" + current, StringComparer.OrdinalIgnoreCase))
            {
                current++;
            }

            return "Category_" + current;
        }

        #endregion
    }
}
