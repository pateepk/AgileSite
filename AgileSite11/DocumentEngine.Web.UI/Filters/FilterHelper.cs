using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.FormEngine.Web.UI;
using CMS.Helpers;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// Provides helper methods for filter controls.
    /// </summary>
    internal static class FilterHelper
    {
        #region "Methods"

        /// <summary>
        /// Stores the custom filter state for all its internal controls
        /// </summary>
        /// <param name="filter">Filter control</param>
        /// <param name="state">State storage</param>
        internal static void StoreCustomFilterState(IFilterControl filter, FilterState state)
        {
            foreach (Control filterControl in EnumerateFilterControls((Control)filter))
            {
                StoreFilterState(filterControl, state);
            }
        }


        /// <summary>
        /// Restores the custom filter state for all its internal controls
        /// </summary>
        /// <param name="filter">Filter control</param>
        /// <param name="state">State storage</param>
        internal static void RestoreCustomFilterState(IFilterControl filter, FilterState state)
        {
            foreach (Control filterControl in EnumerateFilterControls((Control)filter))
            {
                RestoreFilterState(filterControl, state);
            }
        }


        private static void StoreFilterState(this Control control, FilterState state)
        {
            if (control is TextBox)
            {
                TextBox filterControl = (TextBox)control;
                state.AddValue(filterControl.UniqueID, filterControl.Text);
            }
            else if (control is DropDownList)
            {
                DropDownList filterControl = (DropDownList)control;
                state.AddValue(filterControl.UniqueID, filterControl.SelectedValue);
            }
            else if (control is CheckBox)
            {
                CheckBox filterControl = (CheckBox)control;
                state.AddValue(filterControl.UniqueID, filterControl.Checked);
            }
            else if (control is FormEngineUserControl)
            {
                FormEngineUserControl filterControl = (FormEngineUserControl)control;
                state.AddValue(filterControl.UniqueID, filterControl.Value);
            }
            else if (control is IFilterControl)
            {
                var filterControl = (IFilterControl)control;

                FilterState filterState = new FilterState();
                filterControl.StoreFilterState(filterState);

                state.AddValue(filterControl.UniqueID, filterState);
            }
            else
            {
                throw new Exception(String.Format("[FilterHelper.StoreFilterState]: Unsupported control type ({0})", control.GetType()));
            }
        }


        private static void RestoreFilterState(Control control, FilterState state)
        {
            if (control is TextBox)
            {
                TextBox filterControl = (TextBox)control;
                filterControl.Text = state.GetString(filterControl.UniqueID);
            }
            else if (control is DropDownList)
            {
                DropDownList filterControl = (DropDownList)control;
                filterControl.SelectedValue = state.GetString(filterControl.UniqueID);
            }
            else if (control is CheckBox)
            {
                CheckBox filterControl = (CheckBox)control;
                filterControl.Checked = state.GetBoolean(filterControl.UniqueID);
            }
            else if (control is FormEngineUserControl)
            {
                FormEngineUserControl filterControl = (FormEngineUserControl)control;
                filterControl.Value = state.GetValue<object>(filterControl.UniqueID);
            }
            else if (control is IFilterControl)
            {
                var filterControl = (IFilterControl)control;

                FilterState filterState = state.GetValue<FilterState>(filterControl.UniqueID);
                filterControl.RestoreFilterState(filterState);
            }
            else
            {
                throw new Exception(String.Format("[FilterHelper.RestoreFilterState]: Unsupported control type ({0})", control.GetType()));
            }
        }


        private static IEnumerable<Control> EnumerateFilterControls(Control parentControl)
        {
            foreach (Control currentControl in parentControl.Controls)
            {
                if (IsFilterControl(currentControl))
                {
                    yield return currentControl;
                }
                else
                {
                    foreach (Control control in EnumerateFilterControls(currentControl))
                    {
                        yield return control;
                    }
                }
            }
        }


        private static bool IsFilterControl(Control control)
        {
            if (control is TextBox)
            {
                return true;
            }

            if (control is DropDownList)
            {
                return true;
            }

            if (control is CheckBox)
            {
                return true;
            }

            if (control is FormEngineUserControl)
            {
                return true;
            }

            if (control is IFilterControl)
            {
                return true;
            }

            return false;
        }

        #endregion
    }
}