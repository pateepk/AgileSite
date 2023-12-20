using System;
using System.Web.UI.WebControls;

using CMS.Base.Web.UI;
using CMS.Helpers;


namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Textbox form control with ability to display fixed placeholder text in front of the editable field.
    /// </summary>
    public class TextBoxWithPlaceholder : FormEngineUserControl
    {
        private CMSTextBox mTextBox;
        private Panel mPnlContainer;
        private Label mLblHidden;


        /// <summary>
        /// Textbox control
        /// </summary>
        public CMSTextBox TextBox
        {
            get
            {
                if (mTextBox == null)
                {
                    mTextBox = new CMSTextBox { ID = "txtText" };
                }
                return mTextBox;
            }
        }


        /// <summary>
        /// Maximum text length.
        /// </summary>
        public int MaxLength
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("size"), 0);
            }
            set
            {
                SetValue("size", value);
                TextBox.MaxLength = value;
            }
        }


        /// <summary>
        /// Text that will be displayed in front of the editable field.
        /// </summary>
        public string PlaceholderText
        {
            get
            {
                return ValidationHelper.GetString(GetValue("placeholdertext"), String.Empty);
            }
            set
            {
                SetValue("placeholdertext", value);
            }
        }


        /// <summary>
        /// Gets or sets the enabled state of the control.
        /// </summary>
        public override bool Enabled
        {
            get
            {
                return TextBox.Enabled;
            }
            set
            {
                TextBox.Enabled = value;
            }
        }


        /// <summary>
        /// Gets or sets form control value.
        /// </summary>
        public override object Value
        {
            get
            {
                if (Trim)
                {
                    return TextBox.Text.Trim();
                }

                return TextBox.Text;
            }
            set
            {
                // Convert the value to a proper type
                value = ConvertInputValue(value);

                TextBox.Text = ValidationHelper.GetString(value, null);
            }
        }


        /// <summary>
        /// Constructor of the form control.
        /// </summary>
        public TextBoxWithPlaceholder()
        {
            Trim = true;
        }


        /// <summary>
        /// OnInit event handler.
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            mPnlContainer = new Panel
            {
                ID = "pnlContainer",
                CssClass = "cms-input-group cms-textwithplaceholder-container"
            };

            mLblHidden = new Label
            {
                ID = "ltlHidden",
                CssClass = "cms-textwithplaceholder-hidden",
                EnableViewState = false
            };

            mPnlContainer.Controls.Add(TextBox);
            mPnlContainer.Controls.Add(mLblHidden);

            Controls.Add(mPnlContainer);
        }


        /// <summary>
        /// OnPreRender event handler.
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            mLblHidden.Text = PlaceholderText;
            mPnlContainer.Attributes.Add("data-content", HTMLHelper.EncodeForHtmlAttribute(PlaceholderText));

            string script = $@"
document.getElementById('{TextBox.ClientID}').style.paddingLeft = (parseFloat(window.getComputedStyle(
    document.getElementById('{mLblHidden.ClientID}')
).width) + 24) + ""px""
";
            ScriptHelper.RegisterStartupScript(this, typeof(string), "TextboxWithPlaceholderScript_" + ClientID, script, true);
        }
    }
}
