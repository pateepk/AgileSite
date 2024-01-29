using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Base.Web.UI;
using CMS.Helpers;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// Color picker control.
    /// </summary>
    [ToolboxItem(false)]
    public class ColorPicker : WebControl, INamingContainer
    {
        #region "Variables"

        /// <summary>
        /// No color selected constant.
        /// </summary>
        public static string NOT_SELECTED = String.Empty;

        // Textbox with color value.
        private TextBox mTxtColor = new CMSTextBox();

        // Dialog button.
        private readonly CMSAccessibleButton mIconButton = new CMSAccessibleButton();

        // Literal for displaying the preview of the color.
        private readonly LiteralControl mLtlPreview = new LiteralControl();

        private const string CONTROL_CSS = "form-control input-width-20";

        private const string BUTTON_CSS = "color-picker-icon";

        private const string COLOR_PICKER_PATH = "~/CMSAdminControls/ColorPicker";

        private string mSupportFolder;

        #endregion


        #region "Properties"

        /// <summary>
        /// Occurs when the color changed.
        /// </summary>
        public event EventHandler ColorChanged;


        /// <summary>
        /// Textbox displaying the selected color value in HEX format.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public TextBox ColorTextBox
        {
            get
            {
                return mTxtColor;
            }
            set
            {
                mTxtColor = value;
            }
        }


        /// <summary>
        /// Selected color.
        /// </summary>
        [Category("Data"), Description("Selected color in hex format (#xxxxxx OR #xxx).")]
        public string SelectedColor
        {
            get
            {
                string color = mTxtColor.Text.Trim();

                // Check if color code is valid
                return (ValidationHelper.IsColor(color)) ? color : NOT_SELECTED;
            }
            set
            {
                mTxtColor.Text = (String.IsNullOrEmpty(value) || (!ValidationHelper.IsColor(value))) ? NOT_SELECTED : value;

                SetPreview(mTxtColor.Text);
            }
        }


        /// <summary>
        /// Path to the folder with supporting files.
        /// </summary>
        [Category("Behavior"), DefaultValue("ColorPicker"), Description("Path to the folder with supporting files.")]
        public string SupportFolder
        {
            get
            {
                if (String.IsNullOrEmpty(mSupportFolder))
                {
                    mSupportFolder = COLOR_PICKER_PATH;
                }
                return mSupportFolder;
            }
            set
            {
                mSupportFolder = value;
            }
        }


        /// <summary>
        /// Enable autopostback on change.
        /// </summary>
        [Category("Behavior"), DefaultValue(false), Description("Enable autopostback on change.")]
        public bool AutoPostback
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if control is used in live site mode.
        /// </summary>
        [Category("Behavior"), DefaultValue(false), Description("Indicates if control is used in live site mode.")]
        public bool IsLiveSite
        {
            get;
            set;
        }


        /// <summary>
        /// Allows calling SelectColor in OnInit (in some cases this is not wanted).
        /// </summary>
        [Browsable(false)]
        public bool AllowOnInitInitialization
        {
            get;
            set;
        }

        #endregion


        #region "Protected methods"

        /// <summary>
        /// OnInit event handler.
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if (AllowOnInitInitialization && RequestHelper.IsPostBack())
            {
                // Try to get color from request
                string formValue = Page.Request.Form[UniqueID + "$txtColor"];
                if (!String.IsNullOrEmpty(formValue))
                {
                    SelectedColor = formValue;
                }
            }

            mIconButton.IconCssClass = "icon-palette";
            mIconButton.ScreenReaderDescription = ResHelper.GetString("dialog.colorpicker.title");
            mIconButton.IconOnly = true;
        }


        /// <summary>
        /// OnLoad event handler.
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Enter pressed in textbox
            if ((ColorChanged != null) && RequestHelper.IsPostBack() && (Page.Request.Params[Page.postEventSourceID] == mTxtColor.UniqueID))
            {
                ColorChanged(this, e);
            }

            // Color in format #XXXXXX
            mTxtColor.ID = "txtColor";
            mTxtColor.MaxLength = 7;
            mTxtColor.CssClass = CONTROL_CSS;
            if (AutoPostback)
            {
                mTxtColor.AutoPostBack = true;
                mTxtColor.TextChanged += mTxtColor_TextChanged;
            }
            Controls.Add(mTxtColor);

            Controls.Add(mLtlPreview);

            string script = String.Format("modalDialog('{0}/{1}.aspx?color=' + escape(document.getElementById('{2}').value) + '&controlid={2}&previewid={3}&postback={4}', 'colorpicker', 300, 345, null, true); return false;",
                ResolveUrl(SupportFolder).TrimEnd('/'),
                (IsLiveSite ? "LiveColorPicker" : "ColorPicker"),
                mTxtColor.ClientID,
                mLtlPreview.ClientID,
                AutoPostback);
            mIconButton.Attributes.Add("onclick", script);
            mIconButton.AddCssClass(BUTTON_CSS);
            mIconButton.Enabled = Enabled;
            Controls.Add(mIconButton);
        }


        /// <summary>
        /// PreRender event handler.
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            // Make place for preview
            SetPreview(SelectedColor);

            // Register the dialog script
            ScriptHelper.RegisterDialogScript(Page);

            base.OnPreRender(e);
        }


        /// <summary>
        /// Renders the control at design-time.
        /// </summary>
        protected override void Render(HtmlTextWriter output)
        {
            if (Context == null)
            {
                output.Write("[ColorPicker: {0}]", ID);
                return;
            }

            output.Write("<div class=\"color-picker-form-control\">");
            mTxtColor.RenderControl(output);
            mIconButton.RenderControl(output);
            mLtlPreview.RenderControl(output);
            output.Write("</div>");
        }


        /// <summary>
        /// Color changed event.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event arguments</param>
        protected void mTxtColor_TextChanged(object sender, EventArgs e)
        {
            if (ColorChanged != null)
            {
                ColorChanged(sender, e);
            }
        }


        /// <summary>
        /// Sets literal control for preview.
        /// </summary>
        /// <param name="selectedColor">Value of selected color</param>
        private void SetPreview(string selectedColor)
        {
            mLtlPreview.Text = String.Format("<div id=\"{0}\" class=\"color-preview\" {1}>&nbsp;</div>", mLtlPreview.ClientID, String.IsNullOrEmpty(selectedColor) ? String.Empty : " style=\"background-color:" + selectedColor + ";\"");
        }

        #endregion
    }
}
