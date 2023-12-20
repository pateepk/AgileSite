using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// Image selection dialog.
    /// </summary>
    [ToolboxItem(false)]
    public class ImageSelector : WebControl, INamingContainer, ICallbackEventHandler, IDialogControl
    {
        #region "Variables"

        /// <summary>
        /// Image path field.
        /// </summary>
        protected TextBox mTxtImagePath;

        /// <summary>
        /// Select image button.
        /// </summary>
        protected WebControl mBtnSelectImage;

        /// <summary>
        /// Preview image.
        /// </summary>
        protected Image mImgImagePreview;

        /// <summary>
        /// Clear path button.
        /// </summary>
        protected WebControl mBtnClearPath;

        /// <summary>
        /// Hidden value field.
        /// </summary>
        protected HiddenField mHidValue;

        /// <summary>
        /// Hidden preview URL.
        /// </summary>
        protected HiddenField mHidPreviewUrl;

        /// <summary>
        /// Hidden Full URL.
        /// </summary>
        protected HiddenField mHidFullUrl;

        /// <summary>
        /// Hidden default value field.
        /// </summary>
        protected HiddenField mHidDefaultValue;

        /// <summary>
        /// Hidden alternate text
        /// </summary>
        protected HiddenField mHidAlt;

        /// <summary>
        /// Script literal.
        /// </summary>
        protected Literal mLtlScript;

        /// <summary>
        /// Header panel.
        /// </summary>
        protected Panel mPnlHeader;

        /// <summary>
        /// Selector panel.
        /// </summary>
        protected Panel mPnlSelector;

        private string mDisplayText;
        private string mPreviewUrl;
        private string mFullSizeViewUrl;

        private string mCulture;
        private readonly bool mUseLinkButton;
        private bool mIsLiveSite = true;

        private DialogConfiguration mDialogConfig;

        #endregion


        #region "Properties"

        /// <summary>
        /// Clear path button.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public WebControl ClearPathButton
        {
            get
            {
                return mBtnClearPath;
            }

            set
            {
                mBtnClearPath = value;
            }
        }


        /// <summary>
        /// Image path textbox.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public TextBox ImagePathTextBox
        {
            get
            {
                return mTxtImagePath;
            }

            set
            {
                mTxtImagePath = value;
            }
        }


        /// <summary>
        /// Button invoking the image selection dialog.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public WebControl SelectImageButton
        {
            get
            {
                return mBtnSelectImage;
            }

            set
            {
                mBtnSelectImage = value;
            }
        }


        /// <summary>
        /// Image preview control.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Image ImagePreviewControl
        {
            get
            {
                return mImgImagePreview;
            }

            set
            {
                mImgImagePreview = value;
            }
        }


        /// <summary>
        /// Indicates if the Clear button should be displayed.
        /// </summary>
        [Category("Behavior"), DefaultValue(true), Description("Indicates if the Clear button should be displayed.")]
        public bool ShowClearButton
        {
            get
            {
                if (ViewState["ShowClearButton"] == null)
                {
                    ViewState["ShowClearButton"] = true;
                }
                return Convert.ToBoolean(ViewState["ShowClearButton"]);
            }
            set
            {
                ViewState["ShowClearButton"] = value;
            }
        }


        /// <summary>
        /// Indicates if the image preview be displayed.
        /// </summary>
        [Category("Behavior"), DefaultValue(true), Description("Indicates if the image preview should be displayed.")]
        public bool ShowImagePreview
        {
            get
            {
                if (ViewState["ShowImagePreview"] == null)
                {
                    ViewState["ShowImagePreview"] = true;
                }
                return Convert.ToBoolean(ViewState["ShowImagePreview"]);
            }
            set
            {
                ViewState["ShowImagePreview"] = value;
            }
        }


        /// <summary>
        /// Indicates if the path textbox should be displayed.
        /// </summary>
        [Category("Behavior"), DefaultValue(true), Description("Indicates if the path textbox should be displayed.")]
        public bool ShowTextBox
        {
            get
            {
                if (ViewState["ShowTextBox"] == null)
                {
                    ViewState["ShowTextBox"] = true;
                }
                return Convert.ToBoolean(ViewState["ShowTextBox"]);
            }
            set
            {
                ViewState["ShowTextBox"] = value;
            }
        }


        /// <summary>
        /// Gets or sets the alternate text value for the image
        /// </summary>
        [Category("Data"), DefaultValue(""), Description("Image alternate text")]
        public string AlternateText
        {
            get
            {
                return mHidAlt.Value;
            }
            set
            {
                mHidAlt.Value = value;
            }
        }


        /// <summary>
        /// Selector value: UseImagePath is set to FALSE -> selector value is Node GUID, UseImagePath is set to TRUE -> selector value is file path.
        /// </summary>
        [Category("Data"), DefaultValue(""), Description("Selector value: UseImagePath is set to FALSE -> selector value is Node GUID, UseImagePath is set to TRUE -> selector value is file path.")]
        public string Value
        {
            get
            {
                return mHidValue.Value;
            }
            set
            {
                mHidValue.Value = value;
                LoadDisplayValues(value);
            }
        }


        ///<summary>Width of the image preview.</summary>
        [Category("Behavior"), DefaultValue(0), Description("Width of the image preview.")]
        public int ImageWidth
        {
            get
            {
                return ValidationHelper.GetInteger(ViewState["ImageWidth"], 0);
            }
            set
            {
                ViewState["ImageWidth"] = value;
            }
        }


        /// <summary>
        /// Height of the image preview.
        /// </summary>
        [Category("Behavior"), DefaultValue(0), Description("Height of the image preview.")]
        public int ImageHeight
        {
            get
            {
                return ValidationHelper.GetInteger(ViewState["ImageHeight"], 0);
            }
            set
            {
                ViewState["ImageHeight"] = value;
            }
        }


        /// <summary>
        /// Indicates whether image path (TRUE) or path with node guid (FALSE) should be returned to the textbox field.
        /// </summary>
        [Category("Behavior"), DefaultValue(""), Description("Indicates whether image path (TRUE) or path with node guid (FALSE) should be returned to the textbox field.")]
        public bool UseImagePath
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["UseImagePath"], false);
            }
            set
            {
                ViewState["UseImagePath"] = value;
            }
        }


        /// <summary>
        /// Image max side size.
        /// </summary>
        [Category("Behavior"), DefaultValue(""), Description("Image max side size.")]
        public int ImageMaxSideSize
        {
            get
            {
                return ValidationHelper.GetInteger(ViewState["ImageMaxSideSize"], 0);
            }
            set
            {
                ViewState["ImageMaxSideSize"] = value;
            }
        }


        /// <summary>
        /// CSS class of the image preview.
        /// </summary>
        [Category("Behavior"), DefaultValue(0), Description("CSS class of the image preview.")]
        public string ImageCssClass
        {
            get
            {
                return ValidationHelper.GetString(ViewState["ImageCssClass"], "");
            }
            set
            {
                ViewState["ImageCssClass"] = value;
            }
        }


        /// <summary>
        /// CSS style of the image preview.
        /// </summary>
        [Category("Behavior"), DefaultValue(0), Description("CSS style of the image preview.")]
        public string ImageStyle
        {
            get
            {
                return ValidationHelper.GetString(ViewState["ImageStyle"], "");
            }
            set
            {
                ViewState["ImageStyle"] = value;
            }
        }


        /// <summary>
        /// Enable open in full size behavior.
        /// </summary>
        [Category("Behavior"), DefaultValue(true), Description("Enable open in full size behavior.")]
        public bool EnableOpenInFull
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["EnableOpenInFull"], true);
            }
            set
            {
                ViewState["EnableOpenInFull"] = value;
            }
        }


        /// <summary>
        /// Interface culture of the control.
        /// </summary>
        public string Culture
        {
            get
            {
                return mCulture;
            }
            set
            {
                mCulture = value;
            }
        }


        /// <summary>
        /// Enabled.
        /// </summary>
        public override bool Enabled
        {
            get
            {
                return base.Enabled;
            }
            set
            {
                mPnlSelector.Enabled = value;
            }
        }


        /// <summary>
        /// Indicates if control is used in live site mode.
        /// </summary>
        public bool IsLiveSite
        {
            get
            {
                return mIsLiveSite;
            }
            set
            {
                mIsLiveSite = value;
            }
        }


        /// <summary>
        /// Configuration of the dialog for inserting Images.
        /// </summary>
        public DialogConfiguration DialogConfig
        {
            get
            {
                if (mDialogConfig == null)
                {
                    mDialogConfig = new DialogConfiguration();
                    mDialogConfig.SelectableContent = SelectableContentEnum.OnlyImages;
                }
                return mDialogConfig;
            }
            set
            {
                mDialogConfig = value;
            }
        }


        /// <summary>
        /// Default value.
        /// </summary>
        public string DefaultValue
        {
            get
            {
                return ValidationHelper.GetString(mHidDefaultValue.Value, "");
            }
            set
            {
                mHidDefaultValue.Value = value;
            }
        }

        #endregion


        #region "Public Methods"

        /// <summary>
        /// Constructor.
        /// </summary>
        public ImageSelector()
        {
            InitControls();
        }


        /// <summary>
        /// Constructor with default image path.
        /// </summary>
        public ImageSelector(string defaultValue)
            : this(defaultValue, false)
        {
        }


        /// <summary>
        /// Constructor with default image path.
        /// </summary>
        public ImageSelector(string defaultValue, bool useLinkButton)
        {
            mUseLinkButton = useLinkButton;

            InitControls();

            if (defaultValue != null)
            {
                Value = defaultValue;
            }
        }

        #endregion


        #region "Protected Methods"

        /// <summary>
        /// Initializes controls.
        /// </summary>
        protected void InitControls()
        {
            mPnlSelector = new Panel();
            mPnlSelector.ID = "pnlSelector";
            mPnlSelector.CssClass = "image-selector";

            mPnlHeader = new Panel();
            mPnlHeader.ID = "pnlHeader";
            mPnlHeader.CssClass = "control-group-inline";

            mHidValue = new HiddenField();
            mHidValue.ID = "hidValue";

            mHidPreviewUrl = new HiddenField();
            mHidPreviewUrl.ID = "hidPreviewUrl";

            mHidFullUrl = new HiddenField();
            mHidFullUrl.ID = "hidFullUrl";

            mHidDefaultValue = new HiddenField();
            mHidDefaultValue.ID = "hidDefaultValue";

            mHidAlt = new HiddenField();
            mHidAlt.ID = "hidAlt";

            mLtlScript = new Literal();
            mLtlScript.ID = "ltlScript";
            mLtlScript.EnableViewState = false;

            mTxtImagePath = new CMSTextBox();
            mTxtImagePath.ID = "txtPath";
            mTxtImagePath.CssClass = "form-control";
            mTxtImagePath.ReadOnly = true;

            mImgImagePreview = new Image();
            mImgImagePreview.ID = "imgPreview";
            mImgImagePreview.AddCssClass("image-selector-image-preview");


            if (mUseLinkButton)
            {
                mBtnSelectImage = new Label();
                mBtnClearPath = new Label();
            }
            else
            {
                var btnSelImg = new CMSButton();
                btnSelImg.ButtonStyle = ButtonStyle.Default;
                mBtnSelectImage = btnSelImg;

                var btnClearPath = new CMSButton();
                btnClearPath.ButtonStyle = ButtonStyle.Default;
                mBtnClearPath = btnClearPath;
            }

            mBtnSelectImage.ID = "btnSelect";
            mBtnClearPath.ID = "btnClear";

            PreRender += ImageSelector_PreRender;
        }


        /// <summary>
        /// PreRender event handler.
        /// </summary>
        private void ImageSelector_PreRender(object sender, EventArgs e)
        {
            LoadDisplayValues(Value);

            mTxtImagePath.Text = mDisplayText;
            mImgImagePreview.ImageUrl = mPreviewUrl;

            mHidFullUrl.Value = "";

            // Set default value
            string defaultValueUrl = DefaultValue;
            if (!String.IsNullOrEmpty(defaultValueUrl))
            {
                // Add max side size parameter if needed
                if (ImageMaxSideSize > 0)
                {
                    defaultValueUrl = URLHelper.AddParameterToUrl(defaultValueUrl, "maxsidesize", ImageMaxSideSize.ToString());
                }

                // Add image width parameter if needed
                if (ImageWidth > 0)
                {
                    defaultValueUrl = URLHelper.AddParameterToUrl(defaultValueUrl, "width", ImageWidth.ToString());
                }

                // Add image height parameter if needed
                if (ImageHeight > 0)
                {
                    defaultValueUrl = URLHelper.AddParameterToUrl(defaultValueUrl, "height", ImageHeight.ToString());
                }
            }
            mHidDefaultValue.Value = defaultValueUrl;

            if (mFullSizeViewUrl != null)
            {
                if (EnableOpenInFull)
                {
                    mImgImagePreview.Attributes.Add("onclick", "OpenImage('" + ClientID + "');");
                    mImgImagePreview.ToolTip = ResHelper.GetString("CMSImageSelector.OpenFullSize", mCulture);
                }

                mHidFullUrl.Value = ResolveUrl(mFullSizeViewUrl);
            }

            if (!EnableOpenInFull)
            {
                mImgImagePreview.ToolTip = "";
            }

            mHidPreviewUrl.Value = "";
            if (mPreviewUrl != null)
            {
                mHidPreviewUrl.Value = ResolveUrl(mPreviewUrl);
            }

            if (mUseLinkButton)
            {
                ((Label)mBtnSelectImage).Text = ResHelper.GetString("CMSImageSelector.Select", mCulture);
                ((Label)mBtnClearPath).Text = ResHelper.GetString("general.clear", mCulture);
            }
            else
            {
                ((CMSButton)mBtnSelectImage).Text = ResHelper.GetString("CMSImageSelector.Select", mCulture);
                ((CMSButton)mBtnClearPath).Text = ResHelper.GetString("general.clear", mCulture);
            }

            // Register the dialog script
            ScriptHelper.RegisterDialogScript(Page);

            // Register the SetUrl function
            ScriptHelper.RegisterClientScriptBlock(this, typeof(string), "ImageSelector", ScriptHelper.GetScript(
                "function OpenImage(selectorId){ window.open(document.getElementById(selectorId + '_hidFullUrl').value, \"_blank\"); } \n" +
                "function ClearImageSelection(selectorId){ if(window.Changed){Changed();} var defaultValue = document.getElementById(selectorId + '_hidDefaultValue').value; document.getElementById(selectorId + '_hidAlt').value = ''; document.getElementById(selectorId + '_txtPath').value=defaultValue; document.getElementById(selectorId + '_hidValue').value = defaultValue; document.getElementById(selectorId + '_hidPreviewUrl').value = defaultValue;RefreshImagePreview(selectorId);}\n" +
                "function RefreshImagePreview(selectorId){ imgElem = document.getElementById(selectorId + '_imgPreview'); if (imgElem == null) { return }; if ( imgElem.style ){ imgElemStyle = imgElem.style; } else { imgElemStyle = imgElem; } if ( document.getElementById(selectorId + '_hidPreviewUrl').value != '' ){ imgElem.src = document.getElementById(selectorId + '_hidPreviewUrl').value; imgElemStyle.display = ''; imgElem.onclick = function() { OpenImage(this.id.replace('_imgPreview',''));} } else { imgElemStyle.display = 'none'; } } \n" +
                "function ReceiveDisplayImageValues(rvalue, context){ parts = rvalue.split('|'); document.getElementById(context + '_txtPath').value = parts[0]; document.getElementById(context + '_hidPreviewUrl').value = parts[1]; document.getElementById(context + '_hidFullUrl').value = parts[2]; RefreshImagePreview(context); }"
            ));

            ScriptHelper.RegisterClientScriptBlock(this, typeof(string), "SetImageValue_" + ClientID, ScriptHelper.GetScript(
                "function SetImageValue_" + ClientID + "(selectorId){ if(window.Changed){Changed();} var newValue = document.getElementById(selectorId + '_txtPath').value; document.getElementById(selectorId + '_hidValue').value = newValue; " + Page.ClientScript.GetCallbackEventReference(this, "newValue", "ReceiveDisplayImageValues", "selectorId") + "; } \n"
            ));

            mImgImagePreview.Visible = ShowImagePreview;
            if (ShowImagePreview)
            {
                if (mImgImagePreview.ImageUrl == "")
                {
                    mImgImagePreview.Attributes.Add("style", "display: none;");
                }

                mLtlScript.Text += ScriptHelper.GetScript("RefreshImagePreview('" + ClientID + "');");
                }

            if (!ShowTextBox)
            {
                mTxtImagePath.Attributes.Add("style", "display: none;");
            }
            mTxtImagePath.Attributes.Add("onchange", "SetImageValue_" + ClientID + "('" + ClientID + "');");
            mTxtImagePath.Enabled = mPnlSelector.Enabled;
            mBtnSelectImage.Enabled = mPnlSelector.Enabled;
            mBtnClearPath.Enabled = mPnlSelector.Enabled;

            mBtnSelectImage.CssClass = "SelectButton";
            mBtnClearPath.CssClass = "ClearButton";
            mBtnSelectImage.Enabled = mPnlSelector.Enabled;
            mBtnClearPath.Enabled = mPnlSelector.Enabled;

            if (mPnlSelector.Enabled)
            {
                // Configure Image dialog
                string width = DialogConfig.DialogWidth.ToString();
                string height = DialogConfig.DialogHeight.ToString();
                if (DialogConfig.UseRelativeDimensions)
                {
                    width += "%";
                    height += "%";
                }

                DialogConfig.EditorClientID = mTxtImagePath.ClientID;
                if (UseImagePath)
                {
                    DialogConfig.OutputFormat = OutputFormatEnum.URL;
                }
                else
                {
                    DialogConfig.HideAttachments = true;
                    DialogConfig.HideLibraries = true;
                    DialogConfig.HideWeb = true;
                    DialogConfig.OutputFormat = OutputFormatEnum.NodeGUID;
                }

                DialogConfig.AdditionalQueryParameters = "&" + DialogParameters.IMG_ALT_CLIENTID + "=" + mHidAlt.ClientID;

                string url = CMSDialogHelper.GetDialogUrl(DialogConfig, IsLiveSite, true, null, false);
                mBtnSelectImage.Attributes.Add("onclick", String.Format("modalDialog('{0}', 'InsertImage', '{1}', '{2}');", url, width, height) + " return false;");

                if (ShowClearButton)
                {
                    mBtnClearPath.Attributes.Add("onclick", "ClearImageSelection('" + ClientID + "'); return false;");
                }
            }
            else
            {
                mBtnSelectImage.Attributes.Add("onclick", "");
                if (ShowClearButton)
                {
                    mBtnClearPath.Attributes.Add("onclick", "");
                }
            }
        }


        /// <summary>
        /// Load event handler.
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            EnsureChildControls();
        }


        /// <summary>
        /// Renders the control at run-time.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            Controls.Add(mHidValue);
            Controls.Add(mHidPreviewUrl);
            Controls.Add(mHidFullUrl);
            Controls.Add(mHidDefaultValue);
            Controls.Add(mHidAlt);

            Controls.Add(mPnlSelector);

            mPnlSelector.Controls.Add(mPnlHeader);
            mPnlHeader.Controls.Add(mTxtImagePath);

            mPnlHeader.Controls.Add(mBtnSelectImage);

            if (ShowClearButton)
            {
                mPnlHeader.Controls.Add(mBtnClearPath);
            }

            if (ShowImagePreview)
            {
                mPnlSelector.Controls.Add(mImgImagePreview);
            }

            mPnlSelector.Controls.Add(mLtlScript);
        }


        /// <summary>
        /// Renders the control at design-time.
        /// </summary>
        protected override void Render(HtmlTextWriter output)
        {
            output.Write("<div class=\"control-group-inline\">");
            if (Context == null)
            {
                mTxtImagePath.RenderControl(output);
                mBtnSelectImage.RenderControl(output);
                mBtnClearPath.RenderControl(output);
            }
            else
            {
                base.Render(output);
            }
            output.Write("</div>");
        }


        /// <summary>
        /// Returns the display text for the given selector value.
        /// </summary>
        /// <param name="value">Selector value</param>
        protected void LoadDisplayValues(string value)
        {
            mDisplayText = null;
            mPreviewUrl = null;
            mFullSizeViewUrl = null;
            int siteId = 0;

            if (UseImagePath)
            {
                mFullSizeViewUrl = value;
                mDisplayText = value;
            }
            else
            {
                // Get the document in best matching culture
                var nodeGuid = value.ToGuid(Guid.Empty);
                if (nodeGuid != Guid.Empty)
                {
                    var tree = new TreeProvider();
                    var data = tree.SelectNodes()
                                   .Columns("NodeSiteID", "NodeGUID", "NodeAlias", "DocumentNamePath")
                                   .WhereEquals("NodeGUID", nodeGuid)
                                   .CombineWithAnyCulture()
                                   .Published(false)
                                   .Result;

                    if (!DataHelper.DataSourceIsEmpty(data))
                    {
                        var row = data.Tables[0].Rows[0];
                        siteId = row["NodeSiteID"].ToInteger(0);
                        mFullSizeViewUrl = AttachmentURLProvider.GetPermanentAttachmentUrl((Guid)row["NodeGUID"], (string)row["NodeAlias"]);
                        mDisplayText = (string)row["DocumentNamePath"];
                    }
                }
            }

            if (!string.IsNullOrEmpty(mFullSizeViewUrl))
            {
                // Get query string
                var url = mFullSizeViewUrl;
                if (ImageMaxSideSize > 0)
                {
                    url = URLHelper.AddParameterToUrl(url, "maxsidesize", ImageMaxSideSize.ToString());
                }

                if (ImageWidth > 0)
                {
                    url = URLHelper.AddParameterToUrl(url, "width", ImageWidth.ToString());
                }             
   
                if (ImageHeight > 0)
                {
                    url = URLHelper.AddParameterToUrl(url, "height", ImageHeight.ToString());
                }

                if (siteId > 0)
                {
                    var siteInfo = new SiteInfoIdentifier(siteId);
                    url = URLHelper.AddParameterToUrl(url, "sitename", siteInfo.ObjectCodeName);
                }

                // Set preview URL
                mPreviewUrl = url;
            }

            if (mImgImagePreview != null)
            {
                if (ImageCssClass != "")
                {
                    mImgImagePreview.AddCssClass(ImageCssClass);
                }

                string style = "cursor: pointer;";

                if (!EnableOpenInFull)
                {
                    style = "";
                }

                if (ImageStyle != "")
                {
                    style += ImageStyle;
                }
                mImgImagePreview.Attributes["style"] = style;
            }
        }


        /// <summary>
        /// Overrides the generation of the SPAN tag with custom tag.
        /// </summary>
        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                return CMSControlsHelper.GetControlTagKey();
            }
        }

        #endregion


        #region "Callback handling"

        /// <summary>
        /// Raises the callback event.
        /// </summary>
        public void RaiseCallbackEvent(string eventArgument)
        {
            string newGuid = eventArgument;
            LoadDisplayValues(newGuid);
        }


        /// <summary>
        /// Prepares the callback result.
        /// </summary>
        public string GetCallbackResult()
        {
            if (!String.IsNullOrEmpty(mPreviewUrl))
            {
                return mDisplayText + "|" + ResolveUrl(mPreviewUrl) + "|" + ResolveUrl(mFullSizeViewUrl);
            }

            return mDisplayText + "||";         
        }

        #endregion
    }
}
