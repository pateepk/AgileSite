using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.EventLog;
using CMS.Helpers;

namespace CMS.PortalEngine.Web.UI
{
    /// <summary>
    /// Web part to supplement the web part that failed to load.
    /// </summary>
    [ToolboxItem(false)]
    public class WebPartError : CMSAbstractWebPart
    {
        #region "Variables"

        private string mErrorTitle = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets the value that indicates whether 
        /// current error web part is widget and display appropriate error title
        /// </summary>
        public bool IsWidgetError
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the error title.
        /// </summary>
        public string ErrorTitle
        {
            get
            {
                return mErrorTitle ?? (mErrorTitle = GetErrorTitle(IsWidgetError, PartInstance, ID));
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

        #endregion


        #region "Methods"

        /// <summary>
        /// Creates child controls procedure.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            // Add the error controls
            AddErrorControls(this, ErrorTitle, InnerException);
        }


        /// <summary>
        /// Adds the error controls to the given control
        /// </summary>
        /// <param name="parent">Parent control</param>
        /// <param name="title">Error title</param>
        /// <param name="ex">Exception to report</param>
        public static void AddErrorControls(Control parent, string title, Exception ex)
        {
            Panel pnlError = new Panel();
            pnlError.CssClass = "WebPartError";
            parent.Controls.Add(pnlError);

            Label lblErrorTitle = new Label();
            lblErrorTitle.CssClass = "WebPartErrorTitle";
            lblErrorTitle.Text = title;

            pnlError.Controls.Add(lblErrorTitle);

            pnlError.Controls.Add(new LiteralControl(" <br />"));

            if (ex != null)
            {
                Label lblError = new Label();

                lblError.CssClass = "WebPartErrorMessage";
                lblError.Text = HTMLHelper.HTMLEncode(ex.Message);
                lblError.ToolTip = HTMLHelper.HTMLEncode(EventLogProvider.GetExceptionLogMessage(ex));

                pnlError.Controls.Add(lblError);
            }
        }


        /// <summary>
        /// Gets the title for the web part error
        /// </summary>
        /// <param name="isWidget">Flag whether the web part is widget or not</param>
        /// <param name="instance">Web part instance</param>
        /// <param name="id">ID of the web part</param>
        public static string GetErrorTitle(bool isWidget, WebPartInstance instance, string id)
        {
            var component = isWidget ? "Widget" : "WebPart";

            // Prepare the type string
            string type = "(unknown)";
            if (instance != null)
            {
                type = instance.WebPartType;
            }

            return String.Format("[Error loading the {0} '{1}' of type '{2}']", component, id, type);
        }


        /// <summary>
        /// Render action.
        /// </summary>
        protected override void Render(HtmlTextWriter writer)
        {
            if (StandAlone)
            {
                RenderChildren(writer);
            }
            else
            {
                base.Render(writer);
            }
        }

        #endregion
    }
}