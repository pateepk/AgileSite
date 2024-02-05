using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Localization;
using CMS.Membership;
using CMS.PortalEngine.Web.UI;
using CMS.PortalEngine;
using CMS.SiteProvider;

using CultureInfo = System.Globalization.CultureInfo;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// Editable region control for regular page templates text content.
    /// </summary>
    public class CMSEditableRegion : CMSAbstractEditableControl, INamingContainer
    {
        #region "Variables"

        /// <summary>
        /// Html area toolbar name.
        /// </summary>
        protected string mHtmlAreaToolbar = "";

        /// <summary>
        /// Html area toolbar location.
        /// </summary>
        protected string mHtmlAreaToolbarLocation = "Out:CKToolbar";

        /// <summary>
        /// If set to true HTML editor uses stylesheet.
        /// </summary>
        protected bool mUseStylesheet = true;

        /// <summary>
        /// Editor panel.
        /// </summary>
        protected Panel pnlEditor = null;

        /// <summary>
        /// Error label.
        /// </summary>
        protected Label lblError = null;

        /// <summary>
        /// TextBox editor object.
        /// </summary>
        protected CMSTextBox txtValue = null;

        /// <summary>
        /// Region title.
        /// </summary>
        protected Label lblTitle = null;

        /// <summary>
        /// Region content.
        /// </summary>
        protected Literal ltlContent = null;

        /// <summary>
        /// HTMLEditor object.
        /// </summary>
        protected CMSHtmlEditor mHtmlEditor = null;

        private TreeProvider mTreeProvider;
        private bool mShowToolbar = false;
        private bool? mEnabled = null;

        private const int NOT_KNOWN = -1;
        private int mResizeToWidth = NOT_KNOWN;
        private int mResizeToHeight = NOT_KNOWN;
        private int mResizeToMaxSideSize = NOT_KNOWN;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Type of server control which is displayed in the editable region.
        /// </summary>
        [Category("Appearance"), Description("The type of server control which is displayed in the editable region.")]
        public virtual CMSEditableRegionTypeEnum RegionType
        {
            get
            {
                if (ViewState["RegionType"] == null)
                {
                    ViewState["RegionType"] = CMSEditableRegionTypeEnum.TextBox;
                }
                return ((CMSEditableRegionTypeEnum)(ViewState["RegionType"]));
            }
            set
            {
                ViewState["RegionType"] = value;
            }
        }


        /// <summary>
        /// Control title which is displayed in the editable mode.
        /// </summary>
        [Category("Appearance"), Description("Control title which is displayed in the editable mode.")]
        public string RegionTitle
        {
            get
            {
                if (ViewState["RegionTitle"] == null)
                {
                    ViewState["RegionTitle"] = ID;
                }
                return Convert.ToString(ViewState["RegionTitle"]);
            }
            set
            {
                ViewState["RegionTitle"] = value;
            }
        }


        /// <summary>
        /// Indicates whether HTML editor is used in inline (no IFrame) mode.
        /// </summary>
        [Category("Behavior"), Description("Indicates whether HTML editor is used in inline (no IFrame) mode.")]
        public bool UseInlineMode
        {
            get;
            set;
        }


        /// <summary>
        /// Maximum length of the content.
        /// </summary>
        [Category("Behavior"), Description("Maximum length of the content.")]
        public int MaxLength
        {
            get
            {
                return ValidationHelper.GetInteger(ViewState["MaxLength"], 0);
            }
            set
            {
                ViewState["MaxLength"] = value;
            }
        }


        /// <summary>
        /// Minimum length of the content.
        /// </summary>
        [Category("Behavior"), Description("Minimum length of the content.")]
        public int MinLength
        {
            get
            {
                return ValidationHelper.GetInteger(ViewState["MinLength"], 0);
            }
            set
            {
                ViewState["MinLength"] = value;
            }
        }


        /// <summary>
        /// Height of the control. Max size can be <see cref="short.MaxValue"/>
        /// </summary>
        [Category("Appearance"), Description("Height of the control.")]
        public int DialogHeight
        {
            get
            {
                return ValidationHelper.GetValue<short>(ViewState["Height"], 0);
            }
            set
            {
                ViewState["Height"] = value;
            }
        }


        /// <summary>
        /// Width of the control. Max size can be <see cref="short.MaxValue"/>
        /// </summary>
        [Category("Appearance"), Description("Width of the control.")]
        public int DialogWidth
        {
            get
            {
                return ValidationHelper.GetValue<short>(ViewState["Width"], 0);
            }
            set
            {
                ViewState["Width"] = value;
            }
        }


        /// <summary>
        /// HTML editor css stylesheet.
        /// </summary>
        [Category("Appearance"), Description("The CSS stylesheet used by the control.")]
        public string HTMLEditorCssStylesheet
        {
            get
            {
                return ValidationHelper.GetString(ViewState["HTMLEditorCssStylesheet"], "");
            }
            set
            {
                ViewState["HTMLEditorCssStylesheet"] = value;
            }
        }


        /// <summary>
        /// Wrap the text if using text area field.
        /// </summary>
        [Category("Appearance"), Description("Wrap the text if using text area field.")]
        public bool WordWrap
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["Wrap"], true);
            }
            set
            {
                ViewState["Wrap"] = value;
            }
        }


        /// <summary>
        /// Whether the control automatically saves its state for use in round-trips.
        /// </summary>
        [Category("Behavior"), DefaultValue(false), Description("Whether the control automatically saves its state for use in round-trips.")]
        public override bool EnableViewState
        {
            get
            {
                if (ViewState["EnableViewState"] == null)
                {
                    ViewState["EnableViewState"] = false;
                }
                return Convert.ToBoolean(ViewState["EnableViewState"]);
            }
            set
            {
                ViewState["EnableViewState"] = value;
            }
        }


        /// <summary>
        /// HTML editor toolbar set name.
        /// </summary>
        public string HtmlAreaToolbar
        {
            get
            {
                return mHtmlAreaToolbar;
            }
            set
            {
                if (value == null)
                {
                    mHtmlAreaToolbar = "";
                }
                else
                {
                    mHtmlAreaToolbar = value;
                }
            }
        }


        /// <summary>
        /// HTML editor toolbar location.
        /// </summary>
        public string HtmlAreaToolbarLocation
        {
            get
            {
                return mHtmlAreaToolbarLocation;
            }
            set
            {
                if (value == null)
                {
                    mHtmlAreaToolbarLocation = "";
                }
                else
                {
                    mHtmlAreaToolbarLocation = value;
                }
            }
        }


        /// <summary>
        /// If set to true HTML editor uses stylesheet.
        /// </summary>
        public bool UseStylesheet
        {
            get
            {
                return mUseStylesheet;
            }
            set
            {
                mUseStylesheet = value;
            }
        }


        /// <summary>
        /// Control is enabled?
        /// </summary>
        public override bool Enabled
        {
            get
            {
                // Get inner control status
                if (txtValue != null)
                {
                    return txtValue.Enabled;
                }
                else if (HtmlEditor != null)
                {
                    return HtmlEditor.Enabled;
                }
                else
                {
                    return base.Enabled;
                }
            }
            set
            {
                mEnabled = value;

                // Set inner control status
                if (txtValue != null)
                {
                    txtValue.Enabled = value;
                }
                else if (HtmlEditor != null)
                {
                    HtmlEditor.Enabled = value;
                }
                else
                {
                    base.Enabled = value;
                }
            }
        }


        /// <summary>
        /// Width the image should be automatically resized to after it is uploaded.
        /// </summary>
        public int ResizeToWidth
        {
            get
            {
                if (DatabaseHelper.IsDatabaseAvailable && (mResizeToWidth == NOT_KNOWN))
                {
                    mResizeToWidth = ImageHelper.GetAutoResizeToWidth(SiteContext.CurrentSiteName);
                }
                return mResizeToWidth;
            }
            set
            {
                mResizeToWidth = value;
            }
        }


        /// <summary>
        /// Height the image should be automatically resized to after it is uploaded.
        /// </summary>
        public int ResizeToHeight
        {
            get
            {
                if (DatabaseHelper.IsDatabaseAvailable && (mResizeToHeight == NOT_KNOWN))
                {
                    mResizeToHeight = ImageHelper.GetAutoResizeToHeight(SiteContext.CurrentSiteName);
                }
                return mResizeToHeight;
            }
            set
            {
                mResizeToHeight = value;
            }
        }


        /// <summary>
        /// Max side size the image should be automatically resized to after it is uploaded.
        /// </summary>
        public int ResizeToMaxSideSize
        {
            get
            {
                if (DatabaseHelper.IsDatabaseAvailable && (mResizeToMaxSideSize == NOT_KNOWN))
                {
                    mResizeToMaxSideSize = ImageHelper.GetAutoResizeToMaxSideSize(SiteContext.CurrentSiteName);
                }
                return mResizeToMaxSideSize;
            }
            set
            {
                mResizeToMaxSideSize = value;
            }
        }


        /// <summary>
        /// HTML editor object.
        /// </summary>
        public CMSHtmlEditor HtmlEditor
        {
            get
            {
                if (mHtmlEditor == null)
                {
                    mHtmlEditor = new CMSHtmlEditor();
                }
                return mHtmlEditor;
            }
            set
            {
                mHtmlEditor = value;
            }
        }


        /// <summary>
        /// Edit page url which will be used for editing of the editable control. Used in On-site editing.
        /// </summary>
        protected override string EditPageUrl
        {
            get
            {
                return UrlResolver.ResolveUrl("~/CMSModules/PortalEngine/UI/OnSiteEdit/EditText.aspx");
            }
        }


        /// <summary>
        /// Overrides the generation of the SPAN tag with custom tag.
        /// </summary>
        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                SiteInfo currentSite = SiteContext.CurrentSite;
                if (currentSite != null)
                {
                    if (SettingsKeyInfoProvider.GetValue(currentSite.SiteName + ".CMSControlElement").ToLowerCSafe().Trim() == "div")
                    {
                        return HtmlTextWriterTag.Div;
                    }
                    else
                    {
                        return HtmlTextWriterTag.Span;
                    }
                }
                else
                {
                    return HtmlTextWriterTag.Div;
                }
            }
        }

        #endregion


        #region "Private properties"

        private TreeProvider TreeProvider => mTreeProvider ?? (mTreeProvider = new TreeProvider());

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor, initializes the parent portal manager.
        /// </summary>
        /// <param name="pageManager">Parent page manager</param>
        public CMSEditableRegion(IPageManager pageManager)
            : this()
        {
            PageManager = pageManager;
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        public CMSEditableRegion()
        {
        }


        /// <summary>
        /// Load event handler.
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Apply editor settings
            ApplySettings();
        }


        /// <summary>
        /// PreRender event handler.
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            // Register toolbar script
            if (mShowToolbar && ViewMode.IsEdit())
            {
                ScriptHelper.RegisterClientScriptBlock(this, typeof(string), ScriptHelper.TOOLBAR_SCRIPT_KEY, ScriptHelper.ToolbarScript);
            }

            // Set the Enabled property of the editable region only if it has not been already set or is enabled
            if (!mEnabled.HasValue || mEnabled.Value)
            {
                // Switch by region type and ensure enabling
                switch (RegionType)
                {
                    // HTML editor
                    case CMSEditableRegionTypeEnum.HtmlEditor:
                        HtmlEditor.Enabled = ViewMode.IsEdit();
                        break;

                    // Textarea/Textbox
                    case CMSEditableRegionTypeEnum.TextArea:
                    case CMSEditableRegionTypeEnum.TextBox:
                        if (txtValue != null)
                        {
                            txtValue.Enabled = ViewMode.IsEdit();
                        }
                        break;
                }
            }

            switch (ViewMode)
            {
                case ViewModeEnum.Edit:
                case ViewModeEnum.EditDisabled:
                    if (lblError != null)
                    {
                        lblError.Visible = (lblError.Text != "");
                    }

                    lblTitle.Text = RegionTitle;
                    lblTitle.Visible = (lblTitle.Text != "");

                    break;
            }
        }


        /// <summary>
        /// Renders the control.
        /// </summary>
        protected override void Render(HtmlTextWriter writer)
        {
            if (Context == null)
            {
                writer.Write("[CMSEditableRegion: " + ClientID + "]");
                return;
            }

            base.Render(writer);
        }


        /// <summary>
        /// Loads the region content.
        /// </summary>
        /// <param name="content">Content to load</param>
        /// <param name="forceReload">If true, the content is forced to reload</param>
        public override void LoadContent(string content, bool forceReload = false)
        {
            EnsureChildControls();

            // Ensure not null value
            content = content ?? "";

            // Resolve URLs
            content = HTMLHelper.ResolveUrls(content, null);

            switch (ViewMode)
            {
                case ViewModeEnum.Edit:
                case ViewModeEnum.EditDisabled:
                    switch (RegionType)
                    {
                        // HTML editor
                        case CMSEditableRegionTypeEnum.HtmlEditor:
                            if (forceReload || !RequestHelper.IsPostBack() || (ViewMode != ViewModeEnum.Edit))
                            {
                                HtmlEditor.ResolvedValue = content;
                            }
                            break;

                        // TextBox
                        case CMSEditableRegionTypeEnum.TextArea:
                        case CMSEditableRegionTypeEnum.TextBox:
                            if ((forceReload || !RequestHelper.IsPostBack()) && (txtValue != null))
                            {
                                txtValue.Text = content;
                            }
                            break;
                    }
                    break;

                default:
                    // Check authorized state
                    if ((!CheckPermissions || PageManager.IsAuthorized) &&
                        // And published state
                        ((PortalContext.ViewMode != ViewModeEnum.LiveSite) || !SelectOnlyPublished || ((SourcePageInfo != null) && SourcePageInfo.IsPublished)))
                    {
                        if (ltlContent != null)
                        {
                            ltlContent.Text = ResolveMacros(content);
                            if (ControlsHelper.ContainsDynamicControl(content))
                            {
                                ControlsHelper.ResolveDynamicControls(this);
                            }
                        }
                    }
                    break;
            }
        }


        /// <summary>
        /// Returns the list of the field IDs (Client IDs of the inner controls) that should be spell checked.
        /// </summary>
        public override List<string> GetSpellCheckFields()
        {
            if (ViewMode == ViewModeEnum.Edit)
            {
                if (RegionType == CMSEditableRegionTypeEnum.HtmlEditor)
                {
                    return new List<string>() { HtmlEditor.ClientID };
                }
                else if (txtValue != null)
                {
                    return new List<string>() { txtValue.ClientID };
                }
            }

            return null;
        }


        /// <summary>
        /// Returns the editable region content.
        /// </summary>
        public override string GetContent()
        {
            EnsureChildControls();

            if (ViewMode.IsEdit(true))
            {
                if (RegionType == CMSEditableRegionTypeEnum.HtmlEditor)
                {
                    return HtmlEditor.ResolvedValue;
                }
                else
                {
                    return txtValue.Text;
                }
            }

            return null;
        }


        /// <summary>
        /// Creates child controls.
        /// </summary>
        protected void ApplySettings()
        {
            EnsureChildControls();

            if (!ViewMode.IsEdit(true))
            {
                return;
            }

            if (DialogWidth > 0)
            {
                pnlEditor.Width = DialogWidth;
            }

            // Display the region control based on the region type
            switch (RegionType)
            {
                // HTML Editor
                case CMSEditableRegionTypeEnum.HtmlEditor:
                    if (DialogWidth > 0)
                    {
                        HtmlEditor.Width = DialogWidth;
                    }

                    if (DialogHeight > 0)
                    {
                        HtmlEditor.Height = DialogHeight;
                    }

                    // Set toolbar location
                    if (HtmlAreaToolbarLocation != "")
                    {
                        // Show the toolbar
                        if (HtmlAreaToolbarLocation.ToLowerCSafe() == "out:cktoolbar")
                        {
                            mShowToolbar = true;
                        }

                        HtmlEditor.ToolbarLocation = HtmlAreaToolbarLocation;
                    }

                    // Set the visual appearance
                    if (HtmlAreaToolbar != "")
                    {
                        HtmlEditor.ToolbarSet = HtmlAreaToolbar;
                    }

                    if (UseStylesheet)
                    {
                        // Get editor area css file
                        if (HTMLEditorCssStylesheet != "")
                        {
                            HtmlEditor.EditorAreaCSS = CssLinkHelper.GetStylesheetUrl(HTMLEditorCssStylesheet);
                        }
                        else if (SiteContext.CurrentSite != null)
                        {
                            SiteInfo si = SiteInfoProvider.GetSiteInfo(SiteContext.CurrentSiteName);
                            if (si != null)
                            {
                                int styleSheetId = (si.SiteDefaultEditorStylesheet > 0) ? si.SiteDefaultEditorStylesheet : si.SiteDefaultStylesheetID;

                                CssStylesheetInfo cssInfo = CssStylesheetInfoProvider.GetCssStylesheetInfo(styleSheetId);
                                if (cssInfo != null)
                                {
                                    HtmlEditor.EditorAreaCSS = CssLinkHelper.GetStylesheetUrl(cssInfo.StylesheetName);
                                }
                            }
                        }
                    }
                    else
                    {
                        HtmlEditor.EditorAreaCSS = HTMLEditorCssStylesheet;
                    }

                    // Set "Insert image or media" dialog configuration                            
                    HtmlEditor.MediaDialogConfig.ResizeToHeight = ResizeToHeight;
                    HtmlEditor.MediaDialogConfig.ResizeToWidth = ResizeToWidth;
                    HtmlEditor.MediaDialogConfig.ResizeToMaxSideSize = ResizeToMaxSideSize;

                    // Set "Insert link" dialog configuration  
                    HtmlEditor.LinkDialogConfig.ResizeToHeight = ResizeToHeight;
                    HtmlEditor.LinkDialogConfig.ResizeToWidth = ResizeToWidth;
                    HtmlEditor.LinkDialogConfig.ResizeToMaxSideSize = ResizeToMaxSideSize;

                    // Set "Quickly insert image" configuration
                    HtmlEditor.QuickInsertConfig.ResizeToHeight = ResizeToHeight;
                    HtmlEditor.QuickInsertConfig.ResizeToWidth = ResizeToWidth;
                    HtmlEditor.QuickInsertConfig.ResizeToMaxSideSize = ResizeToMaxSideSize;

                    break;

                // TextBox
                case CMSEditableRegionTypeEnum.TextArea:
                case CMSEditableRegionTypeEnum.TextBox:

                    if (RegionType == CMSEditableRegionTypeEnum.TextArea)
                    {
                        txtValue.TextMode = TextBoxMode.MultiLine;
                    }
                    else
                    {
                        txtValue.TextMode = TextBoxMode.SingleLine;
                    }

                    if (DialogWidth > 0)
                    {
                        txtValue.Width = DialogWidth - 8;
                    }
                    else
                    {
                        // Default width is 100%
                        txtValue.Width = new Unit(100, UnitType.Percentage);
                    }

                    if (DialogHeight > 0)
                    {
                        txtValue.Height = DialogHeight;
                    }
                    txtValue.Wrap = WordWrap;
                    break;
            }
        }


        /// <summary>
        /// Creates child controls.
        /// </summary>
        protected override void CreateChildControls()
        {
            Controls.Clear();
            base.EnsureChildControls();

            // Create controls by actual page mode
            switch (ViewMode)
            {
                case ViewModeEnum.Edit:
                case ViewModeEnum.EditDisabled:
                    // Main editor panel
                    pnlEditor = new Panel();
                    pnlEditor.ID = "pnlEditor";
                    pnlEditor.CssClass = "CMSEditableRegionEdit";
                    pnlEditor.Attributes.Add("data-tracksavechanges", "true");
                    Controls.Add(pnlEditor);

                    // Title label
                    lblTitle = new Label();
                    lblTitle.EnableViewState = false;
                    lblTitle.CssClass = "CMSEditableRegionTitle";
                    pnlEditor.Controls.Add(lblTitle);

                    // Error label
                    lblError = new Label();
                    lblError.EnableViewState = false;
                    lblError.CssClass = "CMSEditableRegionError";
                    pnlEditor.Controls.Add(lblError);

                    // Display the region control based on the region type
                    switch (RegionType)
                    {
                        case CMSEditableRegionTypeEnum.HtmlEditor:
                            // HTML Editor
                            HtmlEditor.IsLiveSite = false;
                            HtmlEditor.UseInlineMode = UseInlineMode;
                            HtmlEditor.ID = "HtmlEditor";
                            HtmlEditor.AutoDetectLanguage = false;
                            HtmlEditor.DefaultLanguage = Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName;
                            HtmlEditor.Node = SourcePageInfo;

                            // Set direction
                            HtmlEditor.Config["ContentsLangDirection"] = "ltr";

                            if (CultureHelper.IsPreferredCultureRTL())
                            {
                                HtmlEditor.Config["ContentsLangDirection"] = "rtl";
                            }

                            // Set the Enabled property of the editable region only if it has not been already set or is enabled
                            if (!mEnabled.HasValue || mEnabled.Value)
                            {
                                HtmlEditor.Enabled = (ViewMode.IsEdit());
                            }

                            // Set the language
                            try
                            {
                                CultureInfo ci = CultureHelper.GetCultureInfo(DataHelper.GetNotEmpty(MembershipContext.AuthenticatedUser.PreferredUICultureCode, LocalizationContext.PreferredCultureCode));
                                HtmlEditor.DefaultLanguage = ci.TwoLetterISOLanguageName;
                            }
                            catch
                            {
                            }

                            HtmlEditor.AutoDetectLanguage = false;
                            pnlEditor.Controls.Add((Control)HtmlEditor);

                            break;

                        // TextBox
                        case CMSEditableRegionTypeEnum.TextArea:
                        case CMSEditableRegionTypeEnum.TextBox:

                            txtValue = new CMSTextBox();
                            txtValue.ID = "txtValue";
                            txtValue.CssClass = "EditableRegionTextBox";

                            txtValue.Enabled = (ViewMode.IsEdit());
                            pnlEditor.Controls.Add(txtValue);
                            break;
                    }
                    break;

                // Display content in non editing modes
                default:
                    ltlContent = new Literal();
                    ltlContent.ID = "ltlContent";
                    ltlContent.EnableViewState = false;
                    Controls.Add(ltlContent);
                    break;
            }
        }


        /// <summary>
        /// Returns true if entered data is valid. If data is invalid, it returns false and displays an error message.
        /// </summary>
        public override bool IsValid()
        {
            bool mIsValid = true;
            string mError = "";

            switch (PageManager.ViewMode)
            {
                case ViewModeEnum.Edit:
                case ViewModeEnum.EditDisabled:
                    {
                        // Get value
                        string value = null;
                        switch (RegionType)
                        {
                            // HTML editor
                            case CMSEditableRegionTypeEnum.HtmlEditor:

                                if (HtmlEditor != null)
                                {
                                    value = HtmlEditor.ResolvedValue;
                                }
                                break;

                            // TextBox
                            case CMSEditableRegionTypeEnum.TextArea:
                            case CMSEditableRegionTypeEnum.TextBox:

                                if (txtValue != null)
                                {
                                    value = txtValue.Text;
                                }
                                break;
                        }

                        string plainText = HTMLHelper.StripTags(value);
                        int textLength = (plainText != null) ? plainText.Length : 0;

                        if ((textLength > MaxLength) && (MaxLength > 0))
                        {
                            mError = String.Format(ResHelper.GetString("EditableText.ErrorMax"), textLength, MaxLength);
                            mIsValid = false;
                        }

                        if ((textLength < MinLength) && (MinLength > 0))
                        {
                            mError = String.Format(ResHelper.GetString("EditableText.ErrorMin"), textLength, MinLength);
                            mIsValid = false;
                        }
                    }
                    break;
            }

            if (!mIsValid)
            {
                lblError.Text = mError;
            }

            return mIsValid;
        }


        /// <summary>
        /// Gets the custom dialog parameters used in the On-site editing when opening the modal edit dialog.
        /// The url parameters are in the following format: "name=value"
        /// </summary>
        public override string[] GetEditDialogParameters()
        {
            List<string> parameters = new List<string>();

            // Region type
            parameters.Add("regiontype=" + CMSEditableRegionTypeEnumFunctions.GetRegionTypeString(RegionType));

            // Max length
            if (MaxLength > 0)
            {
                parameters.Add("maxl=" + MaxLength);
            }

            // Min length
            if (MinLength > 0)
            {
                parameters.Add("minl=" + MinLength);
            }

            // Word wrap
            if (!WordWrap)
            {
                parameters.Add("wordwrap=0");
            }

            // Editor css stylesheet
            if (!String.IsNullOrEmpty(HTMLEditorCssStylesheet))
            {
                parameters.Add("editorcss=" + HTMLEditorCssStylesheet);
            }

            // Resize to height
            if (ResizeToHeight > 0)
            {
                parameters.Add("resizetoheight=" + ResizeToHeight);
            }

            // Resize to width
            if (ResizeToWidth > 0)
            {
                parameters.Add("resizetowidth=" + ResizeToWidth);
            }

            // Resize to max side size
            if (ResizeToMaxSideSize > 0)
            {
                parameters.Add("resizetomaxsidesize=" + ResizeToMaxSideSize);
            }

            // Toolbar set
            if (!String.IsNullOrEmpty(HtmlAreaToolbar))
            {
                parameters.Add("toolbarset=" + HtmlAreaToolbar);
            }

            return parameters.ToArray();
        }

        #endregion

    }
}
