using System.Web.UI.WebControls;

using CMS.Helpers;

namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Common class for custom user controls used in forms for selection of field visibility types.
    /// </summary>
    public abstract class FormEngineVisibilityControl : FormEngineUserControl
    {

        #region "Public properties"

        /// <summary>
        /// Gets or sets the enabled state of the control.
        /// </summary>
        public override bool Enabled
        {
            get
            {
                return base.Enabled;
            }
            set
            {
                base.Enabled = value;
                VisibilityControl.Enabled = value;
            }
        }


        /// <summary>
        /// Gets or sets field value.
        /// </summary>
        public override object Value
        {
            get
            {
                return VisibilityControl.SelectedValue;
            }
            set
            {
                EnsureChildControls();

                var item = VisibilityControl.Items.FindByValue(ValidationHelper.GetString(value, null), true);
                if (item != null)
                {
                    VisibilityControl.ClearSelection();
                    item.Selected = true;
                }
            }
        }


        /// <summary>
        /// List-type control that should be used for field visibility type selection.
        /// </summary>
        protected abstract ListControl VisibilityControl { get; }

        #endregion


        #region "Methods"

        /// <summary>
        /// Reloads data on CreateChildControls event.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            if (!StopProcessing)
            {
                ReloadData();
            }
        }


        /// <summary>
        /// Loads the child controls at run-time.
        /// </summary>
        private void ReloadData()
        {
            ListItemCollection items = VisibilityControl.Items;
            if (items.Count == 0)
            {
                items.Add(new ListItem(GetString("visibility.none"), FormFieldVisibilityTypeEnum.None.ToStringRepresentation()));
                items.Add(new ListItem(GetString("visibility.all"), FormFieldVisibilityTypeEnum.All.ToStringRepresentation()));
                items.Add(new ListItem(GetString("visibility.authenticated"), FormFieldVisibilityTypeEnum.Authenticated.ToStringRepresentation()));
            }
        }

        #endregion
    }
}