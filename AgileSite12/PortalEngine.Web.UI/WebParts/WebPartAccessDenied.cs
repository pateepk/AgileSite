using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Helpers;

namespace CMS.PortalEngine.Web.UI
{
    /// <summary>
    /// Web part to inform about access denied to webpart.
    /// </summary>
    [ToolboxItem(false)]
    public class WebPartAccessDenied : CMSAbstractWebPart
    {
        #region "Variables"

        private string mErrorTitle = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets the error title.
        /// </summary>
        public string ErrorTitle
        {
            get
            {
                if (mErrorTitle == null)
                {
                    mErrorTitle = GetInfoTitle();
                }

                return mErrorTitle;
            }
            set
            {
                mErrorTitle = value;
            }
        }


        /// <summary>
        /// Name of permission which is not allowed.
        /// </summary>
        public string PermissionName
        {
            get;
            set;
        }


        /// <summary>
        /// Object type of tested info
        /// </summary>
        public String ObjectType
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

            // Add the info controls
            AddInfoControls(this, ErrorTitle);
        }


        /// <summary>
        /// Adds the info controls to the given control
        /// </summary>
        /// <param name="parent">Parent control</param>
        /// <param name="title">Error title</param>
        public static void AddInfoControls(Control parent, string title)
        {
            Panel pnlError = new Panel();
            pnlError.CssClass = "WebPartError";
            parent.Controls.Add(pnlError);

            Label lblErrorTitle = new Label();
            lblErrorTitle.CssClass = "WebPartErrorTitle";
            lblErrorTitle.Text = title;

            pnlError.Controls.Add(lblErrorTitle);

            pnlError.Controls.Add(new LiteralControl(" <br />"));
        }


        /// <summary>
        /// Gets the title for the web part access denied
        /// </summary>
        private string GetInfoTitle()
        {
            if (!String.IsNullOrEmpty(ObjectType))
            {
                return String.Format(GetString("ui.objecttype.accessdenied"), PermissionName, ObjectType);
            }

            // Prepare the type string
            string type = "(unknown)";
            if ((PartInstance != null) && !String.IsNullOrEmpty(PartInstance.WebPartType))
            {
                type = PartInstance.WebPartType;
            }

            return String.Format(ResHelper.GetString("ui.accessdenied"), PermissionName, WebPartID, type);
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
