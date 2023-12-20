using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.EventLog;
using CMS.Helpers;

namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Control to supplement the form control that failed to load.
    /// </summary>
    [ToolboxItem(false)]
    public class FormControlError : FormEngineUserControl
    {
        #region "Variables"

        private readonly Panel pnlError = new Panel();
        private readonly Label lblErrorTitle = new Label();
        private readonly Label lblErrorMessage = new Label();

        private string mErrorTitle;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets the error title.
        /// </summary>
        public string ErrorTitle
        {
            get
            {
                return mErrorTitle ?? (mErrorTitle = "[Error loading the FormControl '" + FormControlName + "']");
            }
            set
            {
                mErrorTitle = value;
            }
        }


        /// <summary>
        /// Inner exception to display.
        /// </summary>
        public Exception InnerException
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets field value. You need to override this method to make the control work properly with the form.
        /// </summary>
        public override object Value
        {
            get
            {
                return null;
            }
            set
            {
            }
        }


        /// <summary>
        /// Returns true if the control has value, if false, the value from the control should not be used within the form to update the data
        /// </summary>
        public override bool HasValue
        {
            get
            {
                return false;
            }
        }


        /// <summary>
        /// Gets or sets name of form control that failed to load.
        /// </summary>
        public string FormControlName
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Creates child controls procedure.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            pnlError.CssClass = "FormControlError";
            Controls.Add(pnlError);

            lblErrorTitle.CssClass = "FormControlErrorTitle";
            lblErrorTitle.Text = ErrorTitle;
            pnlError.Controls.Add(lblErrorTitle);

            pnlError.Controls.Add(new LiteralControl("<br />"));

            if (InnerException != null)
            {
                lblErrorMessage.CssClass = "FormControlErrorMessage";
                lblErrorMessage.Text = HTMLHelper.HTMLEncode(InnerException.Message);
                lblErrorMessage.ToolTip = HTMLHelper.HTMLEncode(EventLogProvider.GetExceptionLogMessage(InnerException));
                pnlError.Controls.Add(lblErrorMessage);
            }
        }


        /// <summary>
        /// Returns always false.
        /// </summary>
        public override bool IsValid()
        {
            return false;
        }

        #endregion
    }
}