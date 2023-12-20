using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.IO;
using CMS.SiteProvider;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// Image selection dialog.
    /// </summary>
    [ToolboxItem(false)]
    public class MediaSelector : WebControl, INamingContainer, ICallbackEventHandler, IDialogControl
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
        /// Preview image panel.
        /// </summary>
        protected Panel mPnlImage;

        /// <summary>
        /// Preview control.
        /// </summary>
        protected WebControl mPreview;

        /// <summary>
        /// Clear path button.
        /// </summary>
        protected WebControl mBtnClearPath;

        /// <summary>
        /// Hidden value field.
        /// </summary>
        protected HiddenField mHidValue;

        /// <summary>
        /// Hidden Full URL.
        /// </summary>
        protected HiddenField mHidFullUrl;

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

        private readonly Regex mParamRegex = RegexHelper.GetRegex("(?:\\?|&)ext=(.*)", RegexOptions.Compiled | RegexHelper.DefaultOptions);

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
        public WebControl PreviewControl
        {
            get
            {
                return mPreview;
            }
            set
            {
                mPreview = value;
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
        public bool ShowPreview
        {
            get
            {
                if (ViewState["ShowPreview"] == null)
                {
                    ViewState["ShowPreview"] = true;
                }
                return Convert.ToBoolean(ViewState["ShowPreview"]);
            }
            set
            {
                ViewState["ShowPreview"] = value;
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
        /// Selector value: URL of the media.
        /// </summary>
        [Category("Data"), DefaultValue(""), Description("Selector value: URL of the media.")]
        public string Value
        {
            get
            {
                return GetUrlValue();
            }
            set
            {
                SetUrlValue(value);
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
        /// Image max side size.
        /// </summary>
        [Category("Behavior"), DefaultValue(""), Description("Image max side size.")]
        public int ImageMaxSideSize
        {
            get
            {
                int max = ValidationHelper.GetInteger(ViewState["ImageMaxSideSize"], 0);
                if ((max == 0) && (ImageHeight == 0) && (ImageWidth == 0))
                {
                    ImageMaxSideSize = 100;
                    max = 100;
                }
                return max;
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
                return ValidationHelper.GetString(ViewState["ImageCssClass"], String.Empty);
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
                return ValidationHelper.GetString(ViewState["ImageStyle"], String.Empty);
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
                }
                return mDialogConfig;
            }
            set
            {
                mDialogConfig = value;
            }
        }


        /// <summary>
        /// Gets or sets if custom dialog configuration is used.
        /// </summary>
        public bool UseCustomDialogConfig
        {
            get;
            set;
        }


        /// <summary>
        /// Sets textbox autopostback value
        /// </summary>
        public bool AutoPostback
        {
            get
            {
                if (mTxtImagePath != null)
                {
                    return mTxtImagePath.AutoPostBack;
                }
                return false;
            }
            set
            {
                if (mTxtImagePath != null)
                {
                    mTxtImagePath.AutoPostBack = value;
                }
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
            mPnlSelector.CssClass = "MediaSelector";

            mPnlHeader = new Panel();
            mPnlHeader.ID = "pnlHeader";
            mPnlHeader.CssClass = "MediaSelectorHeader";

            mHidValue = new HiddenField();
            mHidValue.ID = "hidValue";

            mHidFullUrl = new HiddenField();
            mHidFullUrl.ID = "hidFullUrl";

            mLtlScript = new Literal();
            mLtlScript.ID = "ltlScript";
            mLtlScript.EnableViewState = false;

            mTxtImagePath = new CMSTextBox();
            mTxtImagePath.ID = "txtPath";
            mTxtImagePath.ReadOnly = true;

            mPnlImage = new Panel();
            mPnlImage.ID = "pnlPreview";
            mPnlImage.CssClass = "media-selector-image";

            CreatePreviewControl();

            if (mUseLinkButton)
            {
                mBtnSelectImage = new LinkButton();
                mBtnClearPath = new LinkButton();
                ((LinkButton)mBtnClearPath).Click += mBtnClearPath_Click;
            }
            else
            {
                var btnSelImg = new CMSButton();
                btnSelImg.ButtonStyle = ButtonStyle.Default;
                mBtnSelectImage = btnSelImg;

                var btnClearPath = new CMSButton();
                btnClearPath.ButtonStyle = ButtonStyle.Default;
                btnClearPath.Click += mBtnClearPath_Click;
                mBtnClearPath = btnClearPath;
            }

            mBtnSelectImage.ID = "btnSelect";
            mBtnClearPath.ID = "btnClear";

            PreRender += MediaSelector_PreRender;
        }


        /// <summary>
        /// Create preview control
        /// </summary>
        /// <param name="isIcon">Indicates if font icon should be used for preview</param>
        protected void CreatePreviewControl(bool isIcon = false)
        {
            // Create new preview control
            WebControl previewControl = isIcon ? (WebControl)new Label() : new Image();

            previewControl.ID = "imgPreview";
            Control parentControl = null;

            // Check if there is a previously created control
            if (mPreview != null)
            {
                // Get its parent
                parentControl = mPreview.Parent;

                if (parentControl != null)
                {
                    parentControl.Controls.Remove(mPreview);
                }
            }

            // Store new control and add it to control hierarchy
            mPreview = previewControl;
            if (parentControl != null)
            {
                parentControl.Controls.Add(mPreview);
            }
        }


        /// <summary>
        /// PreRender event handler.
        /// </summary>
        protected void MediaSelector_PreRender(object sender, EventArgs e)
        {
            LoadDisplayValues(Value);

            PreviewControlSetup();

            mTxtImagePath.Text = ResolveUrl(mDisplayText);

            if (mUseLinkButton)
            {
                ((LinkButton)mBtnSelectImage).Text = ResHelper.GetString("CMSMediaSelector.Select", mCulture);
                ((LinkButton)mBtnClearPath).Text = ResHelper.GetString("CMSMediaSelector.Clear", mCulture);
            }
            else
            {
                ((CMSButton)mBtnSelectImage).Text = ResHelper.GetString("CMSMediaSelector.Select", mCulture);
                ((CMSButton)mBtnClearPath).Text = ResHelper.GetString("CMSMediaSelector.Clear", mCulture);
            }

            ScriptHelper.RegisterDialogScript(Page);
            ScriptHelper.RegisterJQuery(Page);

            // Register the SetUrl function
            ScriptHelper.RegisterClientScriptBlock(this, typeof(string), "MediaSelector", ScriptHelper.GetScript(
                "function OpenMediaImage(selectorId){ window.open(document.getElementById(selectorId + '_hidFullUrl').value, \"_blank\"); } \n" +
                "function ClearMediaSelection(selectorId){ if(window.Changed){Changed();}document.getElementById(selectorId + '_txtPath').value=''; document.getElementById(selectorId + '_hidValue').value=''; if (document.getElementById(selectorId + '_pnlPreview') != null){ document.getElementById(selectorId + '_pnlPreview').style.display='none'; } } \n" +
                "function RefreshMediaPreview(selectorId, value){ imgElem = document.getElementById(selectorId + '_pnlPreview'); if (imgElem == null) { return }; if ( imgElem.style ){ imgElemStyle = imgElem.style; } else { imgElemStyle = imgElem; } if (value != ''){$cmsj(imgElem).html(value);imgElemStyle.display = 'inline'; imgElemStyle.cursor='pointer'; imgElem.onclick = function() { OpenMediaImage(this.id.replace('_imgPreview',''));} } else { imgElemStyle.display = 'none'; } } \n" +
                "function ReceiveDisplayMediaValues(rvalue, context){ parts = rvalue.split('|'); document.getElementById(context).value = parts[0]; document.getElementById(context + '_hidFullUrl').value = parts[1]; RefreshMediaPreview(context, parts[2]); }"
                ));

            ScriptHelper.RegisterClientScriptBlock(this, typeof(string), "MediaSelector_" + ClientID, ScriptHelper.GetScript(
                "function SetMediaValue_" + ClientID + "(selectorId){ var newValue = document.getElementById(selectorId + '_txtPath').value; document.getElementById(selectorId + '_hidValue').value = newValue; " + Page.ClientScript.GetCallbackEventReference(this, "newValue", "ReceiveDisplayMediaValues", "selectorId") + "; } \n"
                ));

            if (!ShowTextBox)
            {
                mTxtImagePath.Attributes.Add("style", "display: none;");
            }
            else
            {
                mTxtImagePath.Attributes.Add("onchange", "SetMediaValue_" + ClientID + "('" + ClientID + "');");
            }

            mTxtImagePath.Enabled = mPnlSelector.Enabled;
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
                if (!UseCustomDialogConfig)
                {
                    DialogConfig.SelectableContent = SelectableContentEnum.AllFiles;
                    DialogConfig.OutputFormat = OutputFormatEnum.URL;
                    DialogConfig.HideWeb = true;
                }

                string url = CMSDialogHelper.GetDialogUrl(DialogConfig, IsLiveSite, true, null, false);
                mBtnSelectImage.Attributes.Add("onclick", String.Format("modalDialog('{0}', 'SelectFile', '{1}', '{2}', null);", url, width, height) + "; return false;");

                if (ShowClearButton)
                {
                    mBtnClearPath.Attributes.Add("onclick", String.Format("ClearMediaSelection('{0}');{1}", ClientID, AutoPostback ? String.Empty : "return false;"));
                }

                mBtnSelectImage.Attributes.Add("class", "SelectButton btn btn-default");
                mBtnClearPath.Attributes.Add("class", "ClearButton btn btn-default");
            }
            else
            {
                mBtnSelectImage.Attributes.Add("class", "SelectButton SelectButtonDisabled btn btn-disabled");
                mBtnClearPath.Attributes.Add("class", "ClearButton ClearButtonDisabled btn btn-disabled");


                mBtnSelectImage.Attributes.Add("onclick", String.Empty);

                if (ShowClearButton)
                {
                    mBtnClearPath.Attributes.Add("onclick", String.Empty);
                    if (mUseLinkButton)
                    {
                        ((LinkButton)mBtnClearPath).Click -= mBtnClearPath_Click;
                    }
                    else
                    {
                        ((CMSButton)mBtnClearPath).Click -= mBtnClearPath_Click;
                    }
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
            Controls.Add(mHidFullUrl);

            Controls.Add(mPnlSelector);

            mPnlSelector.Controls.Add(mPnlHeader);
            mPnlHeader.Controls.Add(mTxtImagePath);
            mPnlHeader.Controls.Add(mBtnSelectImage);

            if (ShowClearButton)
            {
                mPnlHeader.Controls.Add(mBtnClearPath);
            }

            if (ShowPreview)
            {
                mPnlSelector.Controls.Add(mPnlImage);
                mPnlImage.Controls.Add(mPreview);
            }

            mPnlSelector.Controls.Add(mLtlScript);
        }


        /// <summary>
        /// Renders the control at design-time.
        /// </summary>
        protected override void Render(HtmlTextWriter output)
        {
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
        }


        /// <summary>
        /// Returns the display text for the given selector value.
        /// </summary>
        /// <param name="value">Selector value</param>
        protected void LoadDisplayValues(string value)
        {
            if (String.IsNullOrEmpty(value))
            {
                mDisplayText = String.Empty;
                mFullSizeViewUrl = String.Empty;
                mPreviewUrl = String.Empty;
                return;
            }

            mDisplayText = HTMLHelper.HTMLEncode(value);
            mFullSizeViewUrl = URLHelper.RemoveParameterFromUrl(value, "ext");

            if (!String.IsNullOrEmpty(mFullSizeViewUrl))
            {
                string ext = null;

                // Find the extension parameter
                MatchCollection match = mParamRegex.Matches(value);
                if ((match.Count > 0) && (match[0].Groups.Count > 0))
                {
                    ext = Convert.ToString(match[0].Groups[1].Value);
                    int pos = ext.IndexOf('&');
                    if (pos > 0)
                    {
                        ext = ext.Substring(0, pos);
                    }
                }

                // Check whether the extension is Image
                if (ImageHelper.IsImage(ext))
                {
                    CreatePreviewControl();

                    // Get query string with dimension values
                    string queryString = String.Empty;

                    if (ImageMaxSideSize > 0)
                    {
                        queryString += "&maxsidesize=" + ImageMaxSideSize;
                    }
                    else
                    {
                        queryString += (ImageWidth > 0) ? "&width=" + ImageWidth : String.Empty;
                        queryString += (ImageHeight > 0) ? "&height=" + ImageHeight : String.Empty;
                    }

                    if (queryString != String.Empty)
                    {
                        queryString = "?" + queryString.TrimStart('&');
                    }

                    if (StorageHelper.IsExternalStorage(value) || IsCloudStorageUrl(value))
                    {
                        // Append also path and hash for external storage
                        queryString = URLHelper.AddParameterToUrl(queryString, "path", URLHelper.GetQueryValue(value, "path"));
                        queryString = URLHelper.AddParameterToUrl(queryString, "hash", URLHelper.GetQueryValue(value, "hash"));
                    }

                    // Set preview url
                    mPreviewUrl = URLHelper.RemoveQuery(value) + queryString;
                }
                else
                {
                    // Set only extension icon as preview
                    CreatePreviewControl(true);
                    ((Label)mPreview).Text = UIHelper.GetFileIcon(Page, ext, FontIconSizeEnum.Header);
                }

                if (mPreview != null)
                {
                    if (ImageCssClass != String.Empty)
                    {
                        mPreview.CssClass = ImageCssClass;
                    }

                    string style = "cursor: pointer;";

                    if (!EnableOpenInFull)
                    {
                        style = String.Empty;
                    }

                    if (ImageStyle != String.Empty)
                    {
                        style += ImageStyle;
                    }
                    mPreview.Attributes["style"] = style;
                }
            }
        }


        private bool IsCloudStorageUrl(string value)
        {
            return value.ToLowerCSafe().Contains("getazurefile.aspx") || value.ToLowerCSafe().Contains("getamazonfile.aspx");
        }


        /// <summary>
        /// Setup preview control
        /// </summary>
        protected void PreviewControlSetup()
        {
            // Clear panel attributes
            mPnlImage.Attributes.Clear();

            // If preview is image, set it's URL
            var imagePreview = mPreview as Image;
            if (imagePreview != null)
            {
                imagePreview.ImageUrl = mPreviewUrl;
            }

            mHidFullUrl.Value = String.Empty;
            if (mFullSizeViewUrl != null)
            {
                if (EnableOpenInFull)
                {
                    mPreview.Attributes.Add("onclick", "OpenMediaImage('" + ClientID + "');");
                    mPreview.ToolTip = ResHelper.GetString("CMSMediaSelector.OpenFullSize", mCulture);
                }

                mHidFullUrl.Value = ResolveUrl(mFullSizeViewUrl);
            }

            if (!EnableOpenInFull)
            {
                mPreview.ToolTip = String.Empty;
            }

            // Setup buttons
            mPreview.Visible = ShowPreview;
            if (ShowPreview && (string.IsNullOrEmpty(Value) || ((imagePreview != null) && string.IsNullOrEmpty(mPreviewUrl))))
            {
                mPnlImage.Attributes.Add("style", "display: none;");
            }
        }


        /// <summary>
        /// Overrides the generation of the SPAN tag with custom tag.
        /// </summary>
        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                if (SiteContext.CurrentSite != null)
                {
                    if (SettingsKeyInfoProvider.GetValue(SiteContext.CurrentSiteName + ".CMSControlElement").ToLowerCSafe().Trim() == "div")
                    {
                        return HtmlTextWriterTag.Div;
                    }
                    else
                    {
                        return HtmlTextWriterTag.Span;
                    }
                }
                return HtmlTextWriterTag.Span;
            }
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Returns unresolved url for selected media.
        /// </summary>
        protected string GetUrlValue()
        {
            return URLHelper.UnResolveUrl(mHidValue.Value, SystemContext.ApplicationPath);
        }


        /// <summary>
        /// Sets url and preview for selected media.
        /// </summary>
        protected void SetUrlValue(string value)
        {
            value = UrlResolver.ResolveUrl(value);
            mHidValue.Value = value;
            LoadDisplayValues(value);
        }


        /// <summary>
        /// Clears default value
        /// </summary>
        private void mBtnClearPath_Click(object sender, EventArgs e)
        {
            Value = null;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        public MediaSelector()
        {
            InitControls();
        }


        /// <summary>
        /// Constructor with default image path.
        /// </summary>
        public MediaSelector(string defaultValue)
            : this(defaultValue, false)
        {
        }


        /// <summary>
        /// Constructor with default image path.
        /// </summary>
        public MediaSelector(string defaultValue, bool useLinkButton)
        {
            mUseLinkButton = useLinkButton;

            InitControls();

            if (defaultValue != null)
            {
                Value = defaultValue;
            }
        }

        #endregion


        #region "Callback handling"

        /// <summary>
        /// Raises the callback event.
        /// </summary>
        public void RaiseCallbackEvent(string eventArgument)
        {
            LoadDisplayValues(eventArgument);
            PreviewControlSetup();
        }


        /// <summary>
        /// Prepares the callback result.
        /// </summary>
        public string GetCallbackResult()
        {
            if (!String.IsNullOrEmpty(mFullSizeViewUrl))
            {
                return mDisplayText + "|" + ResolveUrl(mFullSizeViewUrl) + "|" + mPreview.GetRenderedHTML();
            }
            return mDisplayText + "||";
        }

        #endregion
    }
}
