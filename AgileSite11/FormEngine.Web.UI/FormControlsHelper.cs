using System;
using System.Linq;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Base;

namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Helper methods for form controls
    /// </summary>
    public class FormControlsHelper
    {
        /// <summary>
        /// Loads the given form control from its definition.
        /// </summary>
        /// <param name="page">Page where the form control should be loaded</param>
        /// <param name="controlName">Form control code name</param>
        /// <param name="fieldName">Field name</param>
        /// <param name="form">Form</param>
        /// <param name="loadDefaultProperties">If true, the default properties for the control are set</param>
        [Obsolete("Use FormUserControlLoader.LoadFormControl instead.")]
        public static FormEngineUserControl LoadFormControl(Page page, string controlName, string fieldName, BasicForm form = null, bool loadDefaultProperties = true)
        {
            return FormUserControlLoader.LoadFormControl(page, controlName, fieldName, form, loadDefaultProperties);
        }


        /// <summary>
        /// Selects single value in control of type ListControl.
        /// </summary>
        /// <param name="selectedValue">SelectedValue</param>
        /// <param name="list">List control</param>
        /// <param name="ignoreCase">If true, the selection is searched as case insensitive</param>
        [Obsolete("Method was not intended for public use and will be removed in the next version. " +
                  "In types derived from CMS.UIControls.ListFormControl use method SelectSingleValue(string) instead.")]
        public static void SelectSingleValue(string selectedValue, ListControl list, bool ignoreCase = false)
        {
            // Try to select value
            ListItem item = list.Items.FindByValue(selectedValue, ignoreCase);
            if (item != null)
            {
                list.SelectedValue = item.Value;
                return;
            }

            // Empty string is special case as it is not possible to store it in XML. Therefore it is returned as NULL.
            if ((selectedValue == null) && (list.Items.FindByValue(String.Empty) != null))
            {
                list.SelectedValue = String.Empty;
            }
        }


        /// <summary>
        /// Selects multiple values in control.
        /// </summary>
        /// <param name="selectedValues">Selected values</param>
        /// <param name="list">List item collection</param>
        /// <param name="ignoreCase">If true, compare is case insensitive</param>
        [Obsolete("Method was not intended for public use and will be removed in the next version. " +
                  "In types derived from CMS.UIControls.ListFormControl use method SelectMultipleValues(IEnumerable<string>) instead.")]
        public static void SelectMultipleValues(IEnumerable<string> selectedValues, ListItemCollection list, bool ignoreCase = false)
        {
            if (selectedValues == null)
            {
                return;
            }

            foreach (string value in selectedValues.Select(v => v.Trim()))
            {
                foreach (ListItem item in list)
                {
                    if ((item != null) && item.Value.EqualsCSafe(value, ignoreCase))
                    {
                        item.Selected = true;
                    }
                }
            }
        }


        /// <summary>
        /// Returns string with selected values (e.g.: "value1|value2|value3").
        /// </summary> 
        /// <param name="items">selected values</param>
        /// <param name="separator">[Optional] separator for selected values. Default value is "|".</param>
        [Obsolete("Method was not intended for public use and will be removed in the next version. " +
                  "In types derived from CMS.UIControls.ListFormControl use method GetSelectedValuesFromListItemCollection(string) instead.")]
        public static string GetSelectedValuesFromListItemCollection(ListItemCollection items, string separator = "|")
        {
            return string.Join(separator, items.Cast<ListItem>()
                                               .Where(i => (i != null) && i.Selected)
                                               .Select(i => i.Value));
        }
    }
}