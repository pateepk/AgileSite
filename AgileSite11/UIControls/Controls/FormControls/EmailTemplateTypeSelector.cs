using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.Design;

using CMS.Base.Web.UI;
using CMS.FormEngine.Web.UI;
using CMS.Helpers;

namespace CMS.UIControls
{
    /// <summary>
    /// Form control for the email template type selection.
    /// </summary>
    [ToolboxData("<{0}:EmailTemplateTypeSelector runat=server></{0}:EmailTemplateTypeSelector>")]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class EmailTemplateTypeSelector : FormControl
    {
        #region "Private variables"

        private CMSDropDownList drpList = null;
        private bool mAllowAll = true;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Specifies, whether the selector allows all selection.
        /// </summary>
        public bool AllowAll
        {
            get
            {
                return mAllowAll;
            }
            set
            {
                mAllowAll = value;
            }
        }


        /// <summary>
        /// Specifies, whether the selector allows empty selection.
        /// </summary>
        public bool AllowEmpty
        {
            get;
            set;
        }


        /// <summary>
        /// Drop down control
        /// </summary>
        public CMSDropDownList DropDown
        {
            get
            {
                return EnsureChildControl<CMSDropDownList>(ref drpList);
            }
        }

        #endregion


        /// <summary>
        /// Constructor
        /// </summary>
        public EmailTemplateTypeSelector()
        {
            FormControlName = "Email_template_type_selector";
        }


        /// <summary>
        /// OnLoad event handler
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            SpecialFieldsDefinition specialFields = new SpecialFieldsDefinition();

            // Add (all) option
            if (AllowAll)
            {
                specialFields.Add(new SpecialField() { Text = ResHelper.GetString("general.selectall"), Value = String.Empty });
            }

            // Add (none) option
            if (AllowEmpty)
            {
                specialFields.Add(new SpecialField() { Text = ResHelper.GetString("general.selectnone"), Value = String.Empty });
            }

            SetValue("Items", specialFields.ToString());
        }
    }
}