using System;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Base.Web.UI;
using CMS.FormEngine.Web.UI;
using CMS.IO;
using CMS.Helpers;

namespace CMS.UIControls
{
    /// <summary>
    /// Panel-groups for properties.
    /// </summary>
    public class CategoryPanel : PlaceHolder
    {
        #region "Constants"

        private const int HIDDEN_FIELD_INDEX = 0;
        private const int HEADING_INDEX = 1;
        private const int RIGHT_PANEL_INDEX = 2;
        private const int LAST_RENDERED_CONTROL_INDEX = RIGHT_PANEL_INDEX;

        #endregion


        #region "Variables"

        // If set to <c>true</c> generated html will contain default css classes.
        private bool mGenerateDefaultCssClasses = true;

        // Indicates whether show children.
        private bool mShowChildren = true;

        // Item identifier
        private string mIdentifier;

        // Heading control
        private FormCategoryHeading heading;

        // Tag to render as
        private HtmlTextWriterTag mRenderAs = HtmlTextWriterTag.Table;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets the panel identifier used for list panel navigation
        /// </summary>
        public string Identifier
        {
            get
            {
                return mIdentifier ?? (mIdentifier = Guid.NewGuid().ToString());
            }
        }


        /// <summary>
        /// Gets or sets title of the panel.
        /// </summary>
        public string Text
        {
            get;
            set;
        }


        /// <summary>
        /// Resource string for text property
        /// </summary>
        public String ResourceString
        {
            get;
            set;
        }


        /// <summary>
        /// If set to <c>true</c> panel is collapsible.
        /// </summary>        
        public bool AllowCollapsing
        {
            get;
            set;
        }


        /// <summary>
        ///  Default settings for collapsing.
        /// </summary>
        public bool Collapsed
        {
            get;
            set;
        }


        /// <summary>
        /// If set to <c>true</c>control generates default css classes.
        /// </summary>
        public bool GenerateDefaultCssClasses
        {
            get
            {
                return mGenerateDefaultCssClasses;
            }
            set
            {
                mGenerateDefaultCssClasses = value;
            }
        }


        /// <summary>
        /// Gets or sets DisplayRightPanel property. If set to <c>true</c> panel will be displayed.
        /// </summary>
        public bool DisplayRightPanel
        {
            get;
            set;
        }


        /// <summary>
        /// Gets access to the right panel instance.
        /// </summary>
        public CMSPanel RightPanel
        {
            get
            {
                // Check for the Design Mode in Visual Studio
                if (Context != null)
                {
                    return Controls[RIGHT_PANEL_INDEX] as CMSPanel;
                }

                return null;
            }
        }


        /// <summary>
        /// Tag to render as.
        /// </summary>
        public HtmlTextWriterTag RenderAs
        {
            get
            {
                return mRenderAs;
            }
            set
            {
                mRenderAs = value;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Default constructor.
        /// </summary>
        public CategoryPanel()
        {
            Load += Page_Load;
        }

        #endregion


        #region "Overrides"

        /// <summary>
        /// RenderChildren event handler
        /// </summary>
        protected override void RenderChildren(HtmlTextWriter writer)
        {
            // Children will be rendered in special order, there is no need to render anything at this time.
        }


        /// <summary>
        /// Init event handler
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            // Check for the Design Mode in Visual Studio
            if (Context == null)
            {
                return;
            }

            // Add hidden field to the control collection
            HiddenField hf = new HiddenField();
            Controls.AddAt(HIDDEN_FIELD_INDEX, hf);

            // Add heading
            heading = new FormCategoryHeading
            {
                ClientIDMode = ClientIDMode.Static,
                ID = Identifier,
                Level = 4,
                IsAnchor = true,
                Text = Text ?? ResHelper.GetString(ResourceString)
            };

            Controls.AddAt(HEADING_INDEX, heading);

            // Load CMSPanel and add it to the controls collection
            CMSPanel rightPanel = new CMSPanel();
            Controls.AddAt(RIGHT_PANEL_INDEX, rightPanel);

            base.OnInit(e);
        }


        /// <summary>
        /// Sends server control content to a provided <c>System.Web.UI.HtmlTextWriter</c> object,
        /// which writes the content to be rendered on the client.
        /// </summary>
        /// <param name="writer">The <c>System.Web.UI.HtmlTextWriter</c> object that receives the server control content</param>
        protected override void Render(HtmlTextWriter writer)
        {
            // Check for the Design Mode in Visual Studio
            if (Context == null)
            {
                writer.Write("[CategoryPanel: " + ID + "]");
                return;
            }

            heading.Text = Text;
            heading.RefreshText();

            // Render hidden field
            Controls[HIDDEN_FIELD_INDEX].RenderControl(writer);

            // Render actions
            RightPanel.AddCssClass("category-panel-header-right");
            RightPanel.AddCssClass("pull-right");

            switch (RenderAs)
            {
                case HtmlTextWriterTag.Div:
                    Controls[HEADING_INDEX].RenderControl(writer);
                    writer.Write(@"<div class=""editing-form-category-fields"">" + GetChildrenRowsString() + "</div>");
                    break;

                case HtmlTextWriterTag.Table:
                    // Render headline
                    writer.Write("<div class='editing-form-category'>" + GetRightPanelString());
                    Controls[HEADING_INDEX].RenderControl(writer);
                    writer.Write(@"</div>");

                    // Render collapse/expand action
                    writer.Write(GetCollapseImageString());

                    // Rows generating
                    writer.Write(@"<div {1}><table {0}>{2}</table></div>", @"class=""EditingFormCategoryTableContent""", (mShowChildren ? string.Empty : @" name=""CollapsedTable"""), GetChildrenRowsString());
                    break;

                default:
                    throw new Exception("Rendering as '" + RenderAs + "' is not supported.");
            }

            base.Render(writer);
        }

        #endregion


        #region "Protected Methods"

        /// <summary>
        /// Load event handler
        /// </summary>
        protected void Page_Load(object sender, EventArgs e)
        {
            // Check for the Design Mode in Visual Studio
            if (Context == null)
            {
                return;
            }

            // Resolve hidden field (if it is collapsed or not)
            HiddenField ctrl = Controls[HIDDEN_FIELD_INDEX] as HiddenField;
            if ((ctrl != null) && (ctrl.Value == "collapsed"))
            {
                mShowChildren = false;
            }

            // Resolve header actions
            if (RightPanel != null)
            {
                RightPanel.Visible = DisplayRightPanel;
            }

            if (!String.IsNullOrEmpty(ResourceString))
            {
                Text = ResHelper.GetString(ResourceString);
            }

            string collapseImage = UIHelper.GetImageUrl(Page, "CMSModules/CMS_PortalEngine/WebpartProperties/");
            ScriptHelper.RegisterClientScriptBlock(Parent.Page, typeof(Page), "imagepath", ScriptHelper.GetScript(String.Format("var imagePath='{0}';", collapseImage)));
        }

        #endregion


        #region "Private Methods"

        /// <summary>
        /// Gets html string for the input control.
        /// </summary>
        /// <param name="control">Control which should be rendered as html</param>
        /// <returns>String representing input control as html.</returns>
        private string GetControlAsHtml(Control control)
        {
            var sb = new StringBuilder();
            var sw = new StringWriter(sb);
            var hw = new HtmlTextWriter(sw);
            control.RenderControl(hw);
            return sb.ToString();
        }


        /// <summary>
        /// Gets html string for the right panel.
        /// </summary>
        /// <returns>String representing right panel control.</returns>
        private string GetRightPanelString()
        {
            return GetControlAsHtml(RightPanel);
        }


        /// <summary>
        /// Gets html string for the collapse image.
        /// </summary>
        /// <returns>String representing collapse image.</returns>
        private string GetCollapseImageString()
        {
            string collapseImageString = "";
            if (AllowCollapsing)
            {
                string collapseImage;
                string collapseName;
                if (mShowChildren)
                {
                    collapseImage = UIHelper.GetImageUrl(Page, "CMSModules/CMS_PortalEngine/WebpartProperties/minus.png");
                    collapseName = "minus";
                }
                else
                {
                    collapseImage = UIHelper.GetImageUrl(Page, "CMSModules/CMS_PortalEngine/WebpartProperties/plus.png");
                    collapseName = "plus";
                }

                collapseImageString = string.Format(@"<img runat=""server"" border=""0"" src=""{0}"" name=""{1}"" alt=""{1}"" />", collapseImage, collapseName);
            }
            return collapseImageString;
        }


        /// <summary>
        /// Gets html string for the rendered children.
        /// </summary>
        /// <returns>String representing collapse image.</returns>
        private string GetChildrenRowsString()
        {
            StringBuilder sb = new StringBuilder();
            // First child is hidden control, second child is heading and third is HeaderAction control
            // These controls have been rendered before
            for (int i = LAST_RENDERED_CONTROL_INDEX + 1; i < Controls.Count; i++)
            {
                sb.Append(GetControlAsHtml(Controls[i]));
            }
            return sb.ToString();
        }

        #endregion
    }
}