using System;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using CMS.Base.Web.UI;
using CMS.Helpers;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// File upload dialog.
    /// </summary>
    [ToolboxItem(false)]
    public class Uploader : Panel, INamingContainer, IUploaderControl
    {
        #region "Variables"

        private bool mEnabled = true;

        #endregion


        #region "Inner controls"

        /// <summary>
        /// Current file link.
        /// </summary>
        protected HtmlAnchor htmlAnchor = new HtmlAnchor();


        /// <summary>
        /// Current file delete button.
        /// </summary>
        protected CMSAccessibleButton btnDelete = new CMSAccessibleButton
        {
            IconCssClass = "icon-bin",
            IconOnly = true
        };


        /// <summary>
        /// Current file action button.
        /// </summary>
        protected CMSAccessibleButton btnAction = new CMSAccessibleButton
        {
            IconCssClass = "icon-edit",
            IconOnly = true
        };


        /// <summary>
        /// Upload file field.
        /// </summary>
        protected CMSFileUpload inputFile = new CMSFileUpload();


        /// <summary>
        /// Main layout panel.
        /// </summary>
        protected Panel pnlMain = new Panel();


        /// <summary>
        /// Current file panel.
        /// </summary>
        protected Panel pnlCurrent = new Panel();


        /// <summary>
        /// Upload control panel.
        /// </summary>
        protected Panel pnlUpload = new Panel();


        /// <summary>
        /// Actions panel.
        /// </summary>
        protected Panel pnlActions = new Panel();


        /// <summary>
        /// Label link.
        /// </summary>
        protected Label lblLink = new Label();

        #endregion


        #region "Properties"

        /// <summary>
        /// Indicates whether the confirmation is needed to remove file from uploader.
        /// </summary>
        public bool RequireDeleteConfirmation
        {
            get;
            set;
        }


        /// <summary>
        /// New image width after it is uploaded.
        /// </summary>
        public int ResizeToWidth
        {
            get
            {
                return ValidationHelper.GetInteger(ViewState["ResizeToWidth"], 0);
            }
            set
            {
                ViewState["ResizeToWidth"] = value;
            }
        }


        /// <summary>
        /// New image height after it is uploaded.
        /// </summary>
        public int ResizeToHeight
        {
            get
            {
                return ValidationHelper.GetInteger(ViewState["ResizeToHeight"], 0);
            }
            set
            {
                ViewState["ResizeToHeight"] = value;
            }
        }


        /// <summary>
        /// New image max side size after it is uploaded.
        /// </summary>
        public int ResizeToMaxSideSize
        {
            get
            {
                return ValidationHelper.GetInteger(ViewState["ResizeToMaxSideSize"], 0);
            }
            set
            {
                ViewState["ResizeToMaxSideSize"] = value;
            }
        }


        /// <summary>
        /// Current file Url to use for the file link.
        /// </summary>
        public string CurrentFileUrl
        {
            get
            {
                return ValidationHelper.GetString(ViewState["CurrentFileUrl"], "");
            }
            set
            {
                ViewState["CurrentFileUrl"] = value;
            }
        }


        /// <summary>
        /// Current file link target frame.
        /// </summary>
        public string CurrentFileTarget
        {
            get
            {
                return ValidationHelper.GetString(ViewState["CurrentFileTarget"], "_blank");
            }
            set
            {
                ViewState["CurrentFileTarget"] = value;
            }
        }


        /// <summary>
        /// Current file name to display.
        /// </summary>
        public string CurrentFileName
        {
            get
            {
                return ValidationHelper.GetString(ViewState["CurrentFileName"], "");
            }
            set
            {
                ViewState["CurrentFileName"] = value;
            }
        }


        /// <summary>
        /// Enables or disables the control.
        /// </summary>
        public override bool Enabled
        {
            get
            {
                return mEnabled;
            }
            set
            {
                mEnabled = value;
            }
        }


        /// <summary>
        /// Returns the currently posted file or null when no file posted.
        /// </summary>
        public HttpPostedFile PostedFile
        {
            get
            {
                EnsureChildControls();
                if ((inputFile.PostedFile != null) && !String.IsNullOrEmpty(inputFile.PostedFile.FileName))
                {
                    return inputFile.PostedFile;
                }
                else
                {
                    return null;
                }
            }
        }


        /// <summary>
        /// Returns action button.
        /// </summary>
        public CMSAccessibleButton ActionButton
        {
            get
            {
                return btnAction;
            }
        }


        /// <summary>
        /// If true, action button is displayed.
        /// </summary>
        public bool ShowActionButton
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["ShowActionButton"], false);
            }
            set
            {
                ViewState["ShowActionButton"] = value;
            }
        }


        /// <summary>
        /// Gets actions panel.
        /// </summary>
        public Panel ActionsPanel
        {
            get
            {
                return pnlActions;
            }
        }


        /// <summary>
        /// Indicates if tooltips should be shown.
        /// </summary>
        public bool ShowTooltip
        {
            get;
            set;
        }


        /// <summary>
        /// Tool tip width.
        /// </summary>
        public int TooltipWidth
        {
            get;
            set;
        }


        /// <summary>
        /// Tool tip body.
        /// </summary>
        public string TooltipBody
        {
            get;
            set;
        }


        /// <summary>
        /// Returns inner upload control.
        /// </summary>
        public FileUpload UploadControl
        {
            get
            {
                return inputFile;
            }
        }

        #endregion


        #region "Public Methods"

        /// <summary>
        /// Constructor.
        /// </summary>
        public Uploader()
        {
            InitControls();
        }


        /// <summary>
        /// Clears the data.
        /// </summary>
        public void Clear()
        {
            CurrentFileName = "";
            CurrentFileUrl = "";
        }

        #endregion


        #region "Protected Methods"

        /// <summary>
        /// Initializes controls.
        /// </summary>
        protected void InitControls()
        {
            CssClass = "uploader";

            inputFile.ID = "inputFile";
            inputFile.Attributes.Add("class", "uploader-input-file");

            htmlAnchor.ID = "htmlAnchor";
            htmlAnchor.Attributes.Add("class", "uploader-current-file");
            htmlAnchor.EnableViewState = false;

            btnDelete.ID = "btnDelete";
            btnDelete.CssClass = "uploader-delete";
            btnDelete.EnableViewState = false;

            pnlActions.ID = "pnlActions";
            pnlActions.CssClass = "uploader-external-edit";

            btnAction.ID = "btnAction";
            btnAction.CssClass = "uploader-action";
            btnAction.EnableViewState = false;

            lblLink.ID = "lblLink";
            lblLink.EnableViewState = false;

            //base.PreRender += new System.EventHandler( Page_PreRender );
            Load += Page_Load;
        }


        /// <summary>
        /// Renders the control at run-time.
        /// </summary>
        protected override void CreateChildControls()
        {
            // Create main label
            Controls.Add(pnlMain);

            pnlMain.CssClass = "uploader-main";

            // Add current file
            pnlMain.Controls.Add(pnlCurrent);

            pnlCurrent.CssClass = "uploader-current";
            pnlCurrent.ID = "cPnl";
            pnlCurrent.Controls.Add(new LiteralControl("<span id=\"" + ClientID + "_currentFile\" style=\"visibility: auto;\">"));
            pnlCurrent.Controls.Add(btnAction);

            // Add panel for additional actions only when needed
            if (pnlActions.HasControls())
            {
                pnlCurrent.Controls.Add(pnlActions);
            }
            pnlCurrent.Controls.Add(btnDelete);

            if (RequireDeleteConfirmation)
            {
                btnDelete.Attributes.Add("onclick", "if (DeleteConfirm()) { DeleteUploaderFile('" + ClientID + "'); } return false;");
            }
            else
            {
                btnDelete.Attributes.Add("onclick", "DeleteUploaderFile('" + ClientID + "'); return false;");
            }

            pnlCurrent.Controls.Add(new LiteralControl("</span><input type=\"hidden\" name=\"" + ClientID + "_deleteFile\" id=\"" + ClientID + "_deleteFile\" value=\"0\" />"));

            // Add file selector
            pnlMain.Controls.Add(pnlUpload);

            pnlUpload.CssClass = "uploader-upload";
            pnlUpload.Controls.Add(inputFile);
        }


        /// <summary>
        /// Shadows the PreRender method.
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            if (ShowTooltip)
            {
                lblLink.Controls.Add(htmlAnchor);
                pnlCurrent.Controls.Add(lblLink);
            }
            else
            {
                pnlCurrent.Controls.Add(htmlAnchor);
            }

            base.OnPreRender(e);

            // Process the current file information
            if (CurrentFileName != "")
            {
                // Show the current file info
                pnlCurrent.Visible = true;
                
                htmlAnchor.HRef = Page.ResolveUrl(CurrentFileUrl);
                htmlAnchor.Target = CurrentFileTarget;
                htmlAnchor.InnerText = CurrentFileName;

                // Show or hide action button
                btnAction.Visible = ShowActionButton;

                // Tooltip
                if (ShowTooltip && !string.IsNullOrEmpty(TooltipBody))
                {
                    // Register tool tip script
                    ScriptHelper.RegisterTooltip(Page);

                    lblLink.Attributes.Add("onmouseout", "UnTip()");
                    lblLink.Attributes.Add("onmouseover", "Tip('" + TooltipBody + "', WIDTH, -" + TooltipWidth + ")");
                }
            }
            else
            {
                // Hide the current file info
                pnlCurrent.Visible = false;
            }
        }


        /// <summary>
        /// Renders the control at design-time.
        /// </summary>
        protected override void Render(HtmlTextWriter output)
        {
            if (Context == null)
            {
                output.Write("Uploader : [" + ID + "]<br />Upload:");
                inputFile.RenderControl(output);
            }
            else
            {
                // Set the controls enabled properties
                inputFile.Enabled = Enabled;
                btnDelete.Enabled = Enabled;
                btnAction.Enabled = Enabled;

                if (Enabled)
                {
                    btnDelete.ToolTip = ResHelper.GetString("general.delete");
                }

                base.Render(output);
            }
        }


        /// <summary>
        /// OnLoad event.
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Add client script to ensure the proper encoding type
            if (ControlsHelper.IsInAsyncPostback(Page))
            {
                ScriptHelper.RegisterClientScriptBlock(Page, typeof(string), "UploadMultipart", ScriptHelper.GetScript("document.forms[0].encoding = 'multipart/form-data';"));
            }

            ScriptHelper.RegisterClientScriptBlock(this, typeof(string), "DeleteConfirmation", ScriptHelper.GetScript(
                "function DeleteConfirm() { return confirm(" + ScriptHelper.GetString(ResHelper.GetString("uploader.deleteconfirmation")) + "); } "));
        }


        /// <summary>
        /// Page load event handler, loads the data.
        /// </summary>
        protected void Page_Load(Object sender, EventArgs e)
        {
            // Set request timeout
            Page.Server.ScriptTimeout = AttachmentHelper.ScriptTimeout;

            ScriptHelper.RegisterClientScriptBlock(this, typeof(string), "Uploader", ScriptHelper.GetScript(@"
function DeleteUploaderFile(uploaderId) { 
    document.getElementById(uploaderId + '_deleteFile').value = '1'; 
    var pnl = document.getElementById(uploaderId + '_cPnl');
    if(pnl != null) {
        pnl.style.display = 'none';
    }
    var uploaderElem = document.getElementById(uploaderId + '_inputFile'); 
    
    // Mark as changed
    if (window.Changed != null) {
        window.Changed();
    }

    if (uploaderElem.onchange) { 
        uploaderElem.onchange();
    }
}"));

            ScriptHelper.RegisterClientScriptBlock(this, typeof(string), "PostBack", ScriptHelper.GetScript(
                "function UpdatePage(){ " + Page.ClientScript.GetPostBackEventReference(this, "") + "; } \n"));

            // Set action button click event handler
            btnAction.Click += btnAction_Click;

            if (PostedFile != null)
            {
                if (OnUploadFile != null)
                {
                    OnUploadFile(this, e);
                }
            }

            if (ValidationHelper.GetInteger(Page.Request.Params[ClientID + "_deleteFile"], 0) == 1)
            {
                OnDeleteFile(this, e);
            }
        }


        /// <summary>
        /// BtnAction click event handler.
        /// </summary>
        private void btnAction_Click(object sender, EventArgs e)
        {
            if (OnActionClick != null)
            {
                OnActionClick(sender, e);
            }
        }

        #endregion


        #region "Control events"

        /// <summary>
        /// Raises when the current file requests to be deleted.
        /// </summary>
        public event EventHandler OnDeleteFile;

        /// <summary>
        /// Raises when a new file is given to be uploaded.
        /// </summary>
        public event EventHandler OnUploadFile;

        /// <summary>
        /// OnActionClick event.
        /// </summary>
        public event EventHandler OnActionClick;


        /// <summary>
        /// Delete file event handler.
        /// </summary>
        public void RaiseDeleteFile(object sender, EventArgs e)
        {
            if (OnDeleteFile != null)
            {
                OnDeleteFile(this, e);
            }
        }

        #endregion
    }
}
