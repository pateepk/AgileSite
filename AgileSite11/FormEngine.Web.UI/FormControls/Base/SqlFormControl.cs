using System;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Base.Web.UI;
using CMS.Helpers;
using CMS.Membership;

namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Base class for SQL form controls
    /// </summary>
    public abstract class SqlFormControl : FormEngineUserControl
    {
        #region "Variables"

        /// <summary>
        /// Regular expression to validate the safe expression
        /// </summary>
        protected Regex mSafeRegEx = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Regular expression to validate the safe expression
        /// </summary>
        public Regex SafeRegEx
        {
            get
            {
                if (mSafeRegEx == null)
                {
                    // Build the regex
                    mSafeRegEx = GetSafeRegEx();
                }

                return mSafeRegEx;
            }
        }


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
                TextBoxControl.Enabled = value;
            }
        }


        /// <summary>
        /// Original control value
        /// </summary>
        protected virtual object OriginalValue
        {
            get
            {
                return ViewState["OriginalValue"];
            }
            set
            {
                ViewState["OriginalValue"] = value;
            }
        }


        /// <summary>
        /// Gets or sets field value.
        /// </summary>
        public override object Value
        {
            get
            {
                return TextBoxControl.Text.Trim();
            }
            set
            {
                string stringValue = ValidationHelper.GetString(value, "");

                TextBoxControl.Text = stringValue;

                // Set original value if user is not authorized
                if (!MembershipContext.AuthenticatedUser.IsAuthorizedPerResource("CMS.Design", "EditSQLCode") && !IsSafeValue(TextBoxControl.Text))
                {
                    OriginalValue = stringValue;
                }
            }
        }


        /// <summary>
        /// Gets ClientID of the textbox with emailinput.
        /// </summary>
        public override string ValueElementID
        {
            get
            {
                return TextBoxControl.ClientID;
            }
        }


        /// <summary>
        /// Editing textbox
        /// </summary>
        protected abstract CMSTextBox TextBoxControl
        {
            get;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        public SqlFormControl()
        {
            RememberOriginalValue = true;
        }


        /// <summary>
        /// Creates the child control collection
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            // Add the marker
            string marker = GetControlInfoMarker();
            if (!String.IsNullOrEmpty(marker))
            {
                Controls.Add(new LiteralControl(marker));
            }
        }


        /// <summary>
        /// Gets the control info marker code
        /// </summary>
        public override string GetControlInfoMarker()
        {
            string marker = base.GetControlInfoMarker();

            // Display notification icon
            if (!MembershipContext.AuthenticatedUser.IsAuthorizedPerResource("CMS.Design", "EditSQLCode"))
            {
                var wrapper = new Label
                {
                    CssClass = "info-icon"
                };

                var tooltip = ResHelper.GetString("SqlFormControl.NotAuthorizedInfo");
                var icon = new CMSIcon
                {
                    CssClass = "icon-exclamation-triangle warning-icon",
                    AlternativeText = tooltip,
                    ToolTip = tooltip
                };

                wrapper.Controls.Add(icon);

                marker += wrapper.GetRenderedHTML();
            }

            return marker;
        }


        /// <summary>
        /// Page load.
        /// </summary>
        protected void Page_Load(object sender, EventArgs e)
        {
            // Set control style and css class
            if (!string.IsNullOrEmpty(ControlStyle))
            {
                TextBoxControl.Attributes.Add("style", ControlStyle);
            }
            if (!string.IsNullOrEmpty(CssClass))
            {
                TextBoxControl.CssClass = CssClass;
            }
        }


        /// <summary>
        /// Returns true if the value is safe (matches the Regex)
        /// </summary>
        /// <param name="value">Value</param>
        protected bool IsSafeValue(string value)
        {
            return SafeRegEx.IsMatch(value);
        }


        /// <summary>
        /// Validates the given value
        /// </summary>
        /// <param name="value">Value to validate</param>
        /// <param name="originalValue">Original value</param>
        protected bool ValidateValue(string value, string originalValue)
        {
            // If authorize to edit SQL code, always valid
            if (MembershipContext.AuthenticatedUser.IsAuthorizedPerResource("CMS.Design", "EditSQLCode"))
            {
                return true;
            }

            if (value == originalValue)
            {
                // Value didn't change, valid
                return true;
            }
            else
            {
                // Check value safety if changed
                bool result = IsSafeValue(value);
                if (!result)
                {
                    ValidationError = ResHelper.GetString("SqlFormControl.NotAuthorized");
                }

                return result;
            }
        }


        /// <summary>
        /// Validates the value of the control
        /// </summary>
        public override bool IsValid()
        {
            return ValidateValue(TextBoxControl.Text, (string)OriginalValue);
        }


        /// <summary>
        /// Returns true if the given macro value is valid value for this control
        /// </summary>
        /// <param name="macro">Macro to check</param>
        /// <param name="originalValue">Original value</param>
        public override bool ValidateMacroValue(string macro, string originalValue)
        {
            if (!base.ValidateMacroValue(macro, originalValue))
            {
                return false;
            }

            return ValidateValue(macro, originalValue);
        }


        /// <summary>
        /// Gets the regular expression for the safe value
        /// </summary>
        protected abstract Regex GetSafeRegEx();

        #endregion
    }
}