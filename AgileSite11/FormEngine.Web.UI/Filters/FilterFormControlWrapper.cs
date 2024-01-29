using System.Web.UI;

namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Wrapper for a filter control to become form engine control
    /// </summary>
    public class FilterFormControlWrapper : FormEngineUserControl
    {
        private IFilterControl mFilterControl;


        /// <summary>
        /// Provides the control value
        /// </summary>
        public override object Value
        {
            get
            {
                return FilterControl.Value;
            }
            set
            {
                FilterControl.Value = value;
            }
        }


        /// <summary>
        /// Filter control
        /// </summary>
        public IFilterControl FilterControl
        {
            get
            {
                EnsureChildControls();

                return mFilterControl;
            }
            private set
            {
                mFilterControl = value;
            }
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="filterCtrl">Wrapped filter control</param>
        public FilterFormControlWrapper(IFilterControl filterCtrl)
        {
            FilterControl = filterCtrl;
        }


        /// <summary>
        /// Creates child controls
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            Controls.Add((Control)FilterControl);
        }


        /// <summary>
        /// Gets the filter where condition
        /// </summary>
        public override string GetWhereCondition()
        {
            return FilterControl.WhereCondition;
        }


        /// <summary>
        /// Connects the given form control to a form if necessary
        /// </summary>
        /// <param name="form">Form control</param>
        protected override void ConnectToForm(BasicForm form)
        {
            base.ConnectToForm(form);

            // Connect the filter control to the filtered control
            var filterForm = form as FilterForm;
            if (filterForm != null)
            {
                var ctrl = FilterControl;

                ctrl.FilteredControl = filterForm.FilteredControl;
                ctrl.OnFilterChanged += filterForm.RaiseFilterChanged;
            }
        }
    }
}