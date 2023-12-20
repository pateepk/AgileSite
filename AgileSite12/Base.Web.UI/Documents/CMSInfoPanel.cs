using System;
using System.Web.UI.WebControls;
using System.Web.UI;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Panel for information.
    /// </summary>
    public class CMSInfoPanel : CMSPanel
    {
        #region "Variables"

        /// <summary>
        /// Document info label
        /// </summary>
        protected Label lblDocumentInfo = new Label { ID = "lD", CssClass = "object-edit-menu-info", EnableViewState = false };

        #endregion


        #region "Properties"

        /// <summary>
        /// Info label
        /// </summary>
        public Label Label 
        { 
            get
            {
                EnsureChildControls();
                return lblDocumentInfo;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        public CMSInfoPanel()
        {
            CssClass = "object-edit-menu-info-wrapper";
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Creates control child controls
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            Controls.Add(lblDocumentInfo);
        }


        /// <summary>
        /// Renders control content
        /// </summary>
        /// <param name="writer">HTML writer</param>
        public override void RenderControl(HtmlTextWriter writer)
        {
            Visible = !string.IsNullOrEmpty(lblDocumentInfo.Text);

            base.RenderControl(writer);
        }

        #endregion
    }
}