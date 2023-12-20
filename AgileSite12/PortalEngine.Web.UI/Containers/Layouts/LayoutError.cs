using System;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.EventLog;
using CMS.DataEngine;


namespace CMS.PortalEngine.Web.UI
{
    /// <summary>
    /// Layout to supplement the layout that failed to load.
    /// </summary>
    internal class LayoutError : CMSAbstractLayout
    {
        #region "Variables"

        /// <summary>
        /// Error panel.
        /// </summary>
        protected Panel pnlError = new Panel();

        /// <summary>
        /// Error title.
        /// </summary>
        protected Label lblErrorTitle = new Label();

        /// <summary>
        /// Error message.
        /// </summary>
        protected Label lblErrorMessage = new Label();

        /// <summary>
        /// Inner exception to display.
        /// </summary>
        protected Exception mInnerException = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Inner exception to display.
        /// </summary>
        public Exception InnerException
        {
            get
            {
                return mInnerException;
            }
            set
            {
                mInnerException = value;
            }
        }

        #endregion


        /// <summary>
        /// Creates child controls procedure.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            pnlError.CssClass = "LayoutError";
            Controls.Add(pnlError);

            lblErrorTitle.CssClass = "LayoutErrorTitle";
            lblErrorTitle.Text = "[Error loading the layout]";
            pnlError.Controls.Add(lblErrorTitle);

            pnlError.Controls.Add(new LiteralControl(" <br />"));

            // Add additional message for non-livesite mode
            if ((PagePlaceholder.ViewMode != ViewModeEnum.LiveSite) && (!SettingsKeyInfoProvider.VirtualObjectsAllowed))
            {
                lblErrorMessage.CssClass = "LayoutErrorMessage";
                lblErrorMessage.Text = GetString("VirtualPathProvider.NotRunning");
                if (InnerException != null)
                {
                    lblErrorMessage.ToolTip = InnerException.Message;
                }
                pnlError.Controls.Add(lblErrorMessage);   
            }
            // Add standard error message
            else if (InnerException != null)
            {
                lblErrorMessage.CssClass = "LayoutErrorMessage";
                lblErrorMessage.Text = InnerException.Message;
                lblErrorMessage.ToolTip = EventLogProvider.GetExceptionLogMessage(InnerException);
                pnlError.Controls.Add(lblErrorMessage);
            }
        }
    }
}