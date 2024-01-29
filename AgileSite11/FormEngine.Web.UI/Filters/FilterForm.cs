using System.Collections;
using System.Web.UI;

using CMS.Helpers;

namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Filter form
    /// </summary>
    public class FilterForm : BasicForm, IFilterControl
    {
        #region "Properties"

        /// <summary>
        /// Control which is being filtered by this control
        /// </summary>
        public Control FilteredControl
        {
            get;
            set;
        }


        /// <summary>
        /// Filter value
        /// </summary>
        public object Value
        {
            get;
            set;
        }


        /// <summary>
        /// Returns the where condition for the filter
        /// </summary>
        public string WhereCondition
        {
            get
            {
                return GetWhereCondition();
            }
        }


        /// <summary>
        /// Raises when the filter changes its state
        /// </summary>
        public event ActionEventHandler OnFilterChanged;

        #endregion


        #region "Methods"

        /// <summary>
        /// Initializes macro resolver data sources.
        /// </summary>
        protected override void InitResolver()
        {
            base.InitResolver();

            var resolver = ContextResolver;

            // Redirect edited and parent object to UI context, this form is not an editing form
            resolver.SetNamedSourceDataCallback("EditedObject", r => UIContext.EditedObject);
            resolver.SetNamedSourceDataCallback("ParentObject", r => UIContext.EditedObjectParent);
        }


        /// <summary>
        /// Resets the filter state
        /// </summary>
        public void ResetFilter()
        {
            if (FormInformation == null)
            {
                return;
            }

            // Create new data container
            Data = FormInformation.CreateDataContainer();

            LoadControlValues();

            // Wrapped filter controls
            foreach (DictionaryEntry entry in FieldControls)
            {
                var wrapperCtrl = entry.Value as FilterFormControlWrapper;
                if (wrapperCtrl != null)
                {
                    wrapperCtrl.FilterControl.ResetFilter();
                }
            }
        }


        /// <summary>
        /// Stores filter state to the specified object.
        /// </summary>
        /// <param name="state">The object that holds the filter state.</param>
        public void StoreFilterState(FilterState state)
        {
            if (FieldControls == null)
            {
                return;
            }

            // Add values from the form controls
            foreach (DictionaryEntry entry in FieldControls)
            {
                var wrapperCtrl = entry.Value as FilterFormControlWrapper;
                if (wrapperCtrl != null)
                {
                    // Wrapped filter control
                    wrapperCtrl.FilterControl.StoreFilterState(state);
                }
                else
                {
                    var ctrl = entry.Value as FormEngineUserControl;
                    var col = entry.Key as string;

                    if ((ctrl != null) && (col != null))
                    {
                        // Add the control value
                        state.AddValue(col, ctrl.Value);

                        // Add other values
                        var otherValues = ctrl.GetOtherValues();
                        if (otherValues != null)
                        {
                            for (int i = 0; i <= otherValues.GetUpperBound(0); i++)
                            {
                                col = ValidationHelper.GetString(otherValues[i, 0], "");
                                var val = otherValues[i, 1];

                                state.AddValue(col, val);
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Restores filter state from the specified object.
        /// </summary>
        /// <param name="state">The object that holds the filter state.</param>
        public void RestoreFilterState(FilterState state)
        {
            var data = Data;
            if (data == null)
            {
                return;
            }

            // Load the values from stored state
            foreach (var col in state.Names)
            {
                data.SetValue(col, state.GetValue(col, data.GetValue(col)));
            }

            LoadControlValues();

            // Wrapped filter controls
            foreach (DictionaryEntry entry in FieldControls)
            {
                var wrapperCtrl = entry.Value as FilterFormControlWrapper;
                if (wrapperCtrl != null)
                {
                    wrapperCtrl.FilterControl.RestoreFilterState(state);
                }
            }
        }


        /// <summary>
        /// Raises the filter changed event
        /// </summary>
        public void RaiseFilterChanged()
        {
            if (OnFilterChanged != null)
            {
                OnFilterChanged();
            }
        }

        #endregion
    }
}