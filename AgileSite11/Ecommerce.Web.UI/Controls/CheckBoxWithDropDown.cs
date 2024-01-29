using System;
using System.Web.UI.WebControls;

using CMS.Base.Web.UI;
using CMS.Helpers;
using CMS.UIControls;

namespace CMS.Ecommerce.Web.UI
{
    /// <summary>
    /// CheckBoxWithDropDown control, inherited from CMSUserControl.
    /// </summary>
    public class CheckBoxWithDropDown : CMSUserControl
    {
        #region "Variables"
        
        /// <summary>
        /// Panel for label. Using editing-form-label-cell class.
        /// </summary>
        private CMSPanel pnlLbl;
        

        /// <summary>
        /// Panel for dropdown. Using editing-form-value-cell class.
        /// </summary>
        private CMSPanel pnlDrp;


        /// <summary>
        /// Panel for dropdown and label. Use form-group class.
        /// </summary>
        private CMSPanel pnlDrpLbl;

        
        /// <summary>
        /// Wrapping panel. Using form-horizontal class.
        /// </summary>
        private CMSPanel pnlWrapper;
        

        /// <summary>
        /// Checkbox 
        /// </summary>
        private CMSCheckBox chckbox;

        
        /// <summary>
        /// Label for dropdown.
        /// </summary>
        private LocalizedLabel lblForDropdown;


        /// <summary>
        /// Dropdown list.
        /// </summary>
        private CMSDropDownList drpList;
        

        /// <summary>
        /// Selected dropdown item.
        /// </summary>
        private int mSelectedDropDownItem;

        #endregion      


        #region "Properties"

        /// <summary>
        /// Gets selected item in dropdownlist.
        /// </summary>
        public int SelectedDropDownItem
        {
            get
            {
                return mSelectedDropDownItem;
            }
        }


        /// <summary>
        /// Gets or sets value of control.
        /// </summary>
        public int Value;


        /// <summary>
        /// Gets or sets text next to checkbox input.
        /// </summary>
        public string CheckboxText;


        /// <summary>
        /// Gets or sets if checkbox input is checked.
        /// </summary>
        public bool CheckboxChecked;


        /// <summary>
        /// Gets or sets text next to dropdownlist.
        /// </summary>
        public string DropDownLabel;


        /// <summary>
        /// Gets or sets if dropdownlist and text is visible.
        /// </summary>
        public bool DropDownVisible;


        /// <summary>
        /// Gets or sets collection of items to be bound to dropdownlist.
        /// </summary>
        public ListItemCollection DropDownItems;


        /// <summary>
        /// Gets or sets default prompt message in dropdownlist.
        /// </summary>
        public string DropDownPrompt;


        /// <summary>
        /// Enables or Disables control.
        /// </summary>
        public bool Enabled = true;
        
        #endregion


        #region "Event Handlers"

        /// <summary>
        /// Event fired after checkbox selection changed.
        /// </summary>
        public event EventHandler OnCheckBoxSelectionChanged;


        /// <summary>
        /// Event fired after dropdownlist selection changed.
        /// </summary>
        public event EventHandler OnDropDownSelectionChanged;

        #endregion


        #region "Life Cycle"

        /// <summary>
        /// Overrides OnLoad method. 
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            SetupControls();

            if (!RequestHelper.IsPostBack())
            {
                LoadDefaultControlValues();
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Setups and initializes control.
        /// </summary>
        private void SetupControls()
        {

            InitializePanels();
            lblForDropdown = new LocalizedLabel();
            drpList = new CMSDropDownList();
            chckbox = new CMSCheckBox();

            pnlDrp.Controls.Add(drpList);
            pnlLbl.Controls.Add(lblForDropdown);
            pnlDrpLbl.Controls.Add(pnlLbl);
            pnlDrpLbl.Controls.Add(pnlDrp);
            pnlWrapper.Controls.Add(chckbox);
            pnlWrapper.Controls.Add(pnlDrpLbl);

            lblForDropdown.AddCssClass("control-label editing-form-label");
            drpList.AddCssClass("DropDownField form-control");
         
            // Add panel to main control
            base.Controls.Add(pnlWrapper);

            // Set additional parameters
            chckbox.AutoPostBack = true;
            chckbox.CheckedChanged += chckbox_CheckedChanged;
            drpList.AutoPostBack = true;
            drpList.SelectedIndexChanged += drpList_SelectedIndexChanged;
        }


        /// <summary>
        /// Initialize wrapping panels.
        /// </summary>
        private void InitializePanels()
        {
            pnlWrapper = new CMSPanel();
           
            pnlDrpLbl = new CMSPanel();
            pnlDrpLbl.AddCssClass("form-group selector-subitem");

            pnlDrp = new CMSPanel();
            pnlDrp.AddCssClass("editing-form-value-cell");
            
            pnlLbl = new CMSPanel();
            pnlLbl.AddCssClass("editing-form-label-cell");
        }


        /// <summary>
        /// Loads default values to control.
        /// </summary>
        private void LoadDefaultControlValues()
        {
            pnlDrpLbl.Visible = false;
            chckbox.Text = CheckboxText;
            chckbox.Enabled = Enabled;
            chckbox.Checked = CheckboxChecked;
        }

        #endregion


        #region "Event handlers"

        /// <summary>
        /// Event fired after checkbox selection changed.
        /// </summary>
        protected void chckbox_CheckedChanged(object sender, EventArgs e)
        {
            if (chckbox.Checked)
            {
                // Display and fill dropdownlist
                pnlDrpLbl.Visible = DropDownVisible;
                lblForDropdown.Text = DropDownLabel;
                drpList.DataSource = DropDownItems;
                drpList.DataTextField = "Text";
                drpList.DataValueField = "Value";
                drpList.DataBind();
                drpList.Items.Insert(0, new ListItem(DropDownPrompt, "-2"));
            }
            else
            {
                pnlDrpLbl.Visible = false;
            }

            // Save state of checkbox
            CheckboxChecked = chckbox.Checked;

            // Raise the CheckBoxSelectionChanged event.
            OnCheckBoxSelectionChanged?.Invoke(this, EventArgs.Empty);
        }


        /// <summary>
        /// Event fired after dropdownlist selection changed.
        /// </summary>
        void drpList_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selectedValue = ValidationHelper.GetInteger(drpList.SelectedValue, -2);

            mSelectedDropDownItem = selectedValue;

            // Raise the DropDownSelectionChanged event
            OnDropDownSelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}
