using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;

using CMS.Base.Web.UI;
using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Abstract class for list-type form controls.
    /// </summary>
    public abstract class ListFormControl : FormEngineUserControl
    {
        #region "Variables"

        private string[] mSelectedValues = new string[0];
        private string mSeparator;

        #endregion


        #region "Abstract properties"

        /// <summary>
        /// List control.
        /// </summary>
        protected abstract ListControl ListControl
        {
            get;
        }


        /// <summary>
        /// Specifies the selection mode of the <see cref="ListControl"/>.
        /// </summary>
        protected abstract ListSelectionMode SelectionMode
        {
            get;
        }


        /// <summary>
        /// Form control name which is used in error message if loading failed.
        /// </summary>
        protected abstract string FormControlName
        {
            get;
        }


        /// <summary>
        /// Default CSS class which is applied to the <see cref="ListControl"/> if no other CSS class was specified.
        /// </summary>
        protected abstract string DefaultCssClass
        {
            get;
        }

        #endregion


        #region "Public properties"

        /// <summary>
        /// Gets or sets form control value.
        /// </summary>
        public override object Value
        {
            get
            {
                return GetControlValue();
            }
            set
            {
                SetControlValue(value);
            }
        }


        /// <summary>
        /// Gets or sets the enabled state of the control.
        /// </summary>
        public override bool Enabled
        {
            get
            {
                return ListControl.Enabled;
            }
            set
            {
                ListControl.Enabled = value;
            }
        }


        /// <summary>
        /// Macro source.
        /// </summary>
        public string MacroSource
        {
            get
            {
                return GetValue("macro", String.Empty);
            }
            set
            {
                SetValue("macro", value);
            }
        }


        /// <summary>
        /// Indicates whether the items load to the <see cref="ListControl"/> will be alphabetically sorted.
        /// </summary>
        public bool SortItems
        {
            get
            {
                return GetValue("SortItems", false);
            }
            set
            {
                SetValue("SortItems", value);
            }
        }


        /// <summary>
        /// Text pattern containing macro. Valid when the data source is query or macro.
        /// </summary>
        public string TextFormat
        {
            get
            {
                return GetValue("TextFormat", String.Empty);
            }
            set
            {
                SetValue("TextFormat", value);
            }
        }


        /// <summary>
        /// Value pattern containing macro. Valid when the data source is query or macro.
        /// </summary>
        public string ValueFormat
        {
            get
            {
                return GetValue("ValueFormat", String.Empty);
            }
            set
            {
                SetValue("ValueFormat", value);
            }
        }


        /// <summary>
        /// Contains the character that is placed between selected items in multiple selection mode.
        /// </summary>
        public string Separator
        {
            get
            {
                return mSeparator ?? (mSeparator = ValidationHelper.GetString(GetValue("Separator"), "|"));
            }
            set
            {
                mSeparator = value;
            }
        }


        /// <summary>
        /// If true, the selection of items is case sensitive, otherwise the selection is case insensitive.
        /// Default is case insensitive.
        /// </summary>
        protected bool CaseSensitiveSelection
        {
            get;
            set;
        }

        #endregion


        #region "Control events"

        /// <summary>
        /// Load event handler.
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            LoadAndSelectList();

            ApplyCssClassAndStyles(ListControl);

            CheckRegularExpression = true;
            CheckFieldEmptiness = true;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns string with selected value of the <see cref="ListControl"/>.
        /// The string may contain more items separated by <see cref="Separator"/> in multiple selection mode.
        /// </summary>
        protected virtual object GetControlValue()
        {
            if (SelectionMode == ListSelectionMode.Single)
            {
                return ListControl.SelectedValue;
            }

            return GetSelectedValuesFromListItemCollection();
        }


        /// <summary>
        /// Sets selected item(s) of the <see cref="ListControl"/> based on the given <paramref name="value"/>.
        /// The value may contain more items separated by <see cref="Separator"/> in multiple selection mode.
        /// </summary>
        /// <param name="value">Value to be set</param>
        protected virtual void SetControlValue(object value)
        {
            LoadAndSelectList();

            // Convert the value to a proper type
            value = ConvertInputValue(value);

            mSelectedValues = ValidationHelper.GetString(value, String.Empty).Split(new[] { Separator }, StringSplitOptions.RemoveEmptyEntries);

            ListControl.ClearSelection();

            UpdateSelection();
        }


        /// <summary>
        /// Loads and selects control.
        /// </summary>
        /// <param name="forceReload">Indicates if items should be reloaded even if control contains some values</param>
        protected virtual void LoadAndSelectList(bool forceReload = false)
        {
            var items = ListControl.Items;

            if (forceReload && (items.Count > 0))
            {
                // Keep selected value
                mSelectedValues = ListControl.GetSelectedItems().Select(i => i.Value).ToArray();

                // Clears values if forced reload is requested
                items.Clear();
            }

            if (items.Count == 0)
            {
                var options = GetResolvedValue<string>("options", null);
                var query = ValidationHelper.GetString(GetValue("query"), null);
                var macro = MacroSource;

                try
                {
                    // Load list item collection with data from a specified source
                    new SpecialFieldsDefinition(resolver: ContextResolver)
                    {
                        FieldInfo = FieldInfo,
                        SortItems = SortItems,
                        AllowDuplicates = true
                    }
                    .LoadFromText(options)
                    .LoadFromQuery(query, valueFormat: ValueFormat, textFormat: TextFormat)
                    .LoadFromMacro(macro, ValueFormat, TextFormat)
                    .FillItems(items);
                }
                catch (Exception ex)
                {
                    DisplayException(ex);
                }

                UpdateSelection();
            }
        }


        /// <summary>
        /// Updates the control selection with currently selected value(s)
        /// </summary>
        private void UpdateSelection()
        {
            if (SelectionMode == ListSelectionMode.Single)
            {
                SelectSingleValue(mSelectedValues.FirstOrDefault());
            }
            else
            {
                SelectMultipleValues(mSelectedValues);
            }
        }


        /// <summary>
        /// Reloads control's content.
        /// </summary>
        protected override void ReloadControlInternal()
        {
            base.ReloadControlInternal();

            LoadAndSelectList(true);
        }


        /// <summary>
        /// Tries to apply <see cref="CMS.FormEngine.Web.UI.FormEngineUserControl.CssClass"/> CSS class and <see cref="CMS.FormEngine.Web.UI.FormEngineUserControl.ControlStyle"/> style to the <paramref name="control"/>.
        /// </summary>
        /// <param name="control">Control for styling</param>
        protected void ApplyCssClassAndStyles(WebControl control)
        {
            if (!String.IsNullOrEmpty(CssClass))
            {
                control.AddCssClass(CssClass);
                CssClass = null;
            }
            else if (String.IsNullOrEmpty(control.CssClass))
            {
                control.AddCssClass(DefaultCssClass);
            }

            if (!String.IsNullOrEmpty(ControlStyle))
            {
                control.Attributes.Add("style", ControlStyle);
                ControlStyle = null;
            }
        }


        /// <summary>
        /// Displays exception control with current error.
        /// </summary>
        /// <param name="ex">Thrown exception</param>
        private void DisplayException(Exception ex)
        {
            var ctrlError = new FormControlError();
            ctrlError.FormControlName = FormControlName;
            ctrlError.InnerException = ex;

            Controls.Add(ctrlError);

            ListControl.Visible = false;
        }


        /// <summary>
        /// Selects single value in control.
        /// </summary>
        /// <param name="selectedValue">SelectedValue</param>
        protected void SelectSingleValue(string selectedValue)
        {
            // Empty string is special case as it is not possible to store it in XML. Therefore it is returned as NULL.
            if (selectedValue == null)
            {
                var emptyItem = ListControl.Items.FindByValue(String.Empty);
                if (emptyItem != null)
                {
                    emptyItem.Selected = true;
                }
                return;
            }

            SelectMultipleValues(new [] { selectedValue });
        }


        /// <summary>
        /// Selects multiple values in control.
        /// </summary>
        /// <param name="selectedValues">Selected values</param>
        protected void SelectMultipleValues(IEnumerable<string> selectedValues)
        {
            if (selectedValues == null)
            {
                return;
            }

            var comparison = CaseSensitiveSelection ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase;

            foreach (string value in selectedValues.Select(v => v.Trim()))
            {
                foreach (ListItem item in ListControl.Items)
                {
                    if (SelectListItemByValue(item, value, comparison))
                    {
                        break;
                    }
                }
            }
        }


        private bool SelectListItemByValue(ListItem listItem, string value, StringComparison comparison)
        {
            if (listItem?.Value == null)
            {
                return false;
            }

            if (listItem.Value.Equals(value, comparison))
            {
                listItem.Selected = true;

                return true;
            }

            if (FieldInfo != null)
            {
                var typedListValue = DataTypeManager.ConvertToSystemType(TypeEnum.Field, FieldInfo.DataType, listItem.Value);
                var typedValue = DataTypeManager.ConvertToSystemType(TypeEnum.Field, FieldInfo.DataType, value);

                if (typedValue != null && typedListValue != null && typedValue.Equals(typedListValue))
                {
                    listItem.Selected = true;

                    return true;
                }
            }

            return false;
        }


        /// <summary>
        /// Returns string with selected values (e.g.: "value1|value2|value3").
        /// </summary>
        protected string GetSelectedValuesFromListItemCollection()
        {
            return string.Join(Separator, ListControl.Items.Cast<ListItem>()
                                               .Where(i => (i != null) && i.Selected)
                                               .Select(i => i.Value));
        }

        #endregion
    }
}