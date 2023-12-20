using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Helpers;
using CMS.IO;
using CMS.Membership;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Editor with syntax highlight support.
    /// </summary>
    [Description("Text editor with syntax highlighting support")]
    [ToolboxData(@"<{0}:ExtendedTextArea ID=""ExtendedTextArea1"" runat=""server""></{0}:ExtendedTextArea>")]
    [DefaultProperty("Text"), ControlValueProperty("Text"), ValidationProperty("Text"), DefaultEvent("TextChanged"), SupportsEventValidation]
    public class ExtendedTextArea : CMSTextArea
    {
        #region "Constants"

        /// <summary>
        /// Default CSS class name for this control.
        /// </summary>
        private const string CODEMIRROR_CSSCLASS = "CM";


        /// <summary>
        /// Path to the directory that holds CodeMirror files (CSS and JS).
        /// </summary>
        public const string CODEMIRROR_BASE_PATH = @"~/CMSAdminControls/CodeMirror";


        /// <summary>
        /// Relative path to the main CodeMirror script file.
        /// </summary>
        private const string CODEMIRROR_SCRIPT_FILE = @"/lib/codemirror.js";

        #endregion


        #region "Variables"

        private static bool? mSyntaxHighlightingEnabled = null;
        private static bool? mShowLineNumbersGlobal = null;
        private static int? mLineCountersThreshold = null;

        private int mCursorLine = 0;
        private int mCursorChar = 0;
        private bool mHasFocus = false;
        private bool mSetPosition = false;

        private string mTabKey = "\t";

        private bool mMarkErrors = true;
        private String mParentElementID = String.Empty;
        private String mFullScreenParentElementID = String.Empty;

        private bool mHighlightingEnabled = true;
        private bool mHighlightMacros = true;

        private bool mShowToolbar = true;

        private bool mIsMacroMixedMode = true;

        private string mMacroDelimiter = "#";

        private char mSectionSeparator = '/';
        private int mSectionPadding = 3;
        private bool mEnableTabKey = true;
        private string mResolverName;
        private string mResolverSessionKey;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets the key to use in tab.
        /// </summary>
        [Browsable(false)]
        public string TabKey
        {
            get
            {
                return mTabKey;
            }
            set
            {
                mTabKey = value;
            }
        }


        /// <summary>
        /// Indicates if Tab key adds '\t' into the text. Default value is true.
        /// </summary>
        [Browsable(false)]
        public bool EnableTabKey
        {
            get
            {
                return mEnableTabKey;
            }
            set
            {
                mEnableTabKey = value;
            }
        }


        /// <summary>
        /// Gets the client ID of the syntax highlight editor object used by this control.
        /// </summary>
        [Browsable(false)]
        public string EditorID
        {
            get
            {
                return string.Format("{0}_editor", ClientID);
            }
        }


        /// <summary>
        /// Gets or sets the behaviour mode (single-line, multiline, or password) of this editor control. 
        /// Only TextBoxMode.MultiLine is supported
        /// </summary>
        /// <value>One of the values from TextBoxMode enumeration. Default is TextBoxMode.MultiLine.</value>
        [Browsable(true)]
        [Description("Determines the behavior mode (single-line, multiline, or password) of this editor control")]
        [Category("Appearance")]
        [DefaultValue(TextBoxMode.MultiLine)]
        public override TextBoxMode TextMode
        {
            get
            {
                return base.TextMode;
            }
            set
            {
                // Prevents from changing the text mode (must always generate textarea HTML tag)
            }
        }


        /// <summary>
        /// Gets or sets whether the editor should resize automatically when the containing window resizes.
        /// </summary>
        [Browsable(true)]
        [Description("Determines whether the editor should resize automatically when the containing window resizes")]
        [Category("Appearance")]
        [DefaultValue(false)]
        public bool AutoSize
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets element's ID, which height will be used in autosize computing.
        /// </summary>
        [Browsable(false)]
        public String ParentElementID
        {
            get
            {
                return mParentElementID;
            }
            set
            {
                mParentElementID = value;
            }
        }


        /// <summary>
        /// Gets or sets element's ID, which height and top position will be used in fullscreen mode.
        /// </summary>
        [Browsable(false)]
        public String FullScreenParentElementID
        {
            get
            {
                return mFullScreenParentElementID;
            }
            set
            {
                mFullScreenParentElementID = value;
            }
        }


        /// <summary>
        /// Gets or sets whether the height of the editor should be resized to fit its content dynamically.
        /// </summary>
        [Browsable(true)]
        [Description("Determines whether the height of the editor should be resized to fit its content dynamically")]
        [Category("Appearance")]
        [DefaultValue(false)]
        public bool DynamicHeight
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets whether syntactic errors should be marked in code.
        /// </summary>
        /// <value>True, if syntactic errors should be marked, otherwise false. Default is true.</value>
        [Browsable(true)]
        [Description("Determines whether syntactic errors should be marked in code")]
        [Category("ExtendedTextArea Advanced")]
        [DefaultValue(true)]
        public bool MarkErrors
        {
            get
            {
                return mMarkErrors;
            }
            set
            {
                mMarkErrors = value;
            }
        }


        /// <summary>
        /// Gets or sets the text editing capabilities of this editor.
        /// </summary>
        /// <value>One of the values of EditorModeEnum. Default is EditorModeEnum.Basic.</value>
        [Browsable(true)]
        [Description("Determines the text editing capabilities of this editor")]
        [Category("ExtendedTextArea Advanced")]
        [DefaultValue(EditorModeEnum.Basic)]
        public EditorModeEnum EditorMode
        {
            get;
            set;
        }


        /// <summary>
        /// Determines whether the editor is in single line mode (i.e. only one line of text can be edited).
        /// </summary>
        [Browsable(true)]
        [Description("Determines whether the editor is in single line mode (i.e. only one line of text can be edited)")]
        [Category("ExtendedTextArea Advanced")]
        [DefaultValue(false)]
        public bool SingleLineMode
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the programming language that should be highlighted.
        /// </summary>
        /// <value>One of the Language enumerations. Default is Language.ASPNE.</value>
        [Browsable(true)]
        [Description("Determines the programming language that should be highlighted")]
        [Category("ExtendedTextArea Advanced")]
        [DefaultValue(LanguageEnum.ASPNET)]
        public LanguageEnum Language
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets if the highlighting is turned on or off.
        /// </summary>
        /// <value>True, if the highlighting is enabled, otherwise false. Default is true.</value>
        [Browsable(true)]
        [Description("Determines if the highlighting is turned on or off")]
        [Category("ExtendedTextArea Advanced")]
        [DefaultValue(true)]
        public bool HighlightingEnabled
        {
            get
            {
                return mHighlightingEnabled;
            }
            set
            {
                mHighlightingEnabled = value;
            }
        }


        /// <summary>
        /// Gets or sets if the line numbers are displayed.
        /// </summary>
        /// <value>True, if line numbers will be displayed, otherwise false. Default is false.</value>
        [Browsable(true)]
        [Description("Determines if the line numbers are displayed")]
        [Category("ExtendedTextArea Advanced")]
        [DefaultValue(false)]
        public bool ShowLineNumbers
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets if the toolbar is shown in the code editor.
        /// </summary>
        /// <value>True, if the toolbar should be visible, otherwise false. Default is true.</value>      
        [Browsable(true)]
        [Description("Determines if the toolbar is shown in the code editor")]
        [Category("ExtendedTextArea Advanced")]
        [DefaultValue(true)]
        public bool ShowToolbar
        {
            get
            {
                return mShowToolbar;
            }
            set
            {
                mShowToolbar = value;
            }
        }


        /// <summary>
        /// Gets or sets if the bookmarks panel is shown in the code editor.
        /// </summary>
        /// <value>True, if the bookmarks should be visible, otherwise false. Default is true.</value>        
        [Browsable(true)]
        [Description("Determines if the the bookmarks panel is shown in the code editor")]
        [Category("ExtendedTextArea Advanced")]
        [DefaultValue(false)]
        public bool ShowBookmarks
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets if the bookmarks are refreshed dynamically during editing.
        /// </summary>
        /// <value>True, if bookmarks should be refreshed dynamically, otherwise false. Default is false.</value>
        [Browsable(true)]
        [Description("Determines if the the bookmarks are refreshed dynamically during editing")]
        [Category("ExtendedTextArea Advanced")]
        [DefaultValue(false)]
        public bool DynamicBookmarks
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets if braces should be automatically matched (highlights open and closing brace).
        /// </summary>
        /// <value>True, if brace matching is enabled, otherwise false. Default is false.</value>
        [Browsable(true)]
        [Description("Determines if braces should be automatically matched (highlights open and closing brace)")]
        [Category("ExtendedTextArea Advanced")]
        [DefaultValue(false)]
        public bool BraceMatching
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the total number of lines.
        /// </summary>
        [Browsable(false)]
        public int LineNumber
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the number of bookmarks.
        /// </summary>
        [Browsable(false)]
        public int Count
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the collection of bookmark names.
        /// </summary>
        [Browsable(false)]
        public Hashtable Names
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the collection of bookmark line numbers.
        /// </summary>
        [Browsable(false)]
        public Hashtable Lines
        {
            get;
            set;
        }


        /// <summary>
        /// If true, Editor is set to use small fonts (CodeMirror/css/smallfont.css class is included).
        /// </summary>
        [Browsable(false)]
        public bool UseSmallFonts
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets whether the position member is enabled.
        /// </summary>
        [Browsable(true)]
        [Description("Determines whether the position member is enabled")]
        [Category("ExtendedTextArea Basic")]
        [DefaultValue(false)]
        public bool EnablePositionMember
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets whether the bookmarks are enabled.
        /// </summary>
        [Browsable(true)]
        [Description("Determines whether whether the bookmarks are enabled")]
        [Category("ExtendedTextArea Basic")]
        [DefaultValue(false)]
        public bool EnableSections
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the regular expression which is used to detect bookmarks.
        /// </summary>
        [Browsable(true)]
        [Description("Determines the regular expression which is used to detect bookmarks")]
        [Category("ExtendedTextArea Basic")]
        public string RegularExpression
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the macro delimiter used.
        /// </summary>        
        [Browsable(true)]
        [Description("Determines the macro delimiter used")]
        [Category("ExtendedTextArea Basic")]
        [DefaultValue("#")]
        public string MacroDelimiter
        {
            get
            {
                return mMacroDelimiter;
            }
            set
            {
                mMacroDelimiter = value;
            }
        }


        /// <summary>
        /// Determines if full screen mode is enabled
        /// </summary>
        public bool AllowFullscreen
        {
            get;
            set;
        }


        /// <summary>
        /// Determines whether the button "Insert macro" is visible.
        /// </summary>
        public bool ShowInsertMacro
        {
            get;
            set;
        }


        /// <summary>
        /// Determines whether to show a button that enables user to insert image URL into editor using a InsertImageOrMedia dialog.
        /// </summary>
        public bool ShowInsertImage
        {
            get;
            set;
        }


        /// <summary>
        /// Configuration of Insert image dialog.
        /// </summary>
        public DialogConfiguration InsertImageDialogConfiguration
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether the editor is in pure macro editing mode (whole text is considered as macro) or mixed mode, where auto completion is fired only inside {%%} environment.
        /// This setting will influence the Insert Macro functionality when in mixed mode the brackets are inserted, whereas in the pure macro mode, the macro is inserted without brackets.
        /// </summary>
        public bool IsMacroMixedMode
        {
            get
            {
                return mIsMacroMixedMode;
            }
            set
            {
                mIsMacroMixedMode = value;
            }
        }


        /// <summary>
        /// Gets or sets the resolver which will be used in the Insert macro dialog (if allowed with ShowInsertMacro).
        /// </summary>
        public string ResolverName
        {
            get
            {
                return mResolverName;
            }
            set
            {
                mResolverName = value;
                SessionHelper.SetValue(ResolverSessionKey, value);
            }
        }


        /// <summary>
        /// Determines if macros should be highlighted.
        /// </summary>
        public bool HighlightMacros
        {
            get
            {
                return mHighlightMacros;
            }
            set
            {
                mHighlightMacros = value;
            }
        }


        /// <summary>
        /// Gets if syntax highlighting is enabled/disabled globally.
        /// </summary>
        private static bool SyntaxHighlightingEnabledGlobal
        {
            get
            {
                if (mSyntaxHighlightingEnabled == null)
                {
                    mSyntaxHighlightingEnabled = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSEnableSyntaxHighlighting"], true);
                }

                return mSyntaxHighlightingEnabled.Value;
            }
        }


        /// <summary>
        /// Gets if syntax highlighting is enabled/disabled for language used in this editor.
        /// </summary>
        private bool SyntaxHighlightingEnabledLocal
        {
            get
            {
                string configKey = "CMSEnableSyntaxHighlighting." + Enum.GetName(typeof(LanguageEnum), Language);
                return ValidationHelper.GetBoolean(SettingsHelper.AppSettings[configKey], true);
            }
        }


        /// <summary>
        /// Gets if syntax highlighting is enabled for this control.
        /// </summary>
        public bool SyntaxHighlightingEnabled
        {
            get
            {
                return ((EditorMode == EditorModeEnum.Advanced) &&
                        SyntaxHighlightingEnabledGlobal && SyntaxHighlightingEnabledLocal);
            }
        }


        /// <summary>
        /// Gets if line numbering is enabled/disabled globally.
        /// </summary>
        private static bool ShowLineNumbersGlobal
        {
            get
            {
                if (mShowLineNumbersGlobal == null)
                {
                    mShowLineNumbersGlobal = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSShowLineNumbers"], true);
                }
                return mShowLineNumbersGlobal.Value;
            }
        }


        /// <summary>
        /// Gets the maximum number of lines after which some advanced features are turned off to increase performance.
        /// </summary>
        private static int LineCountersThreshold
        {
            get
            {
                if (mLineCountersThreshold == null)
                {
                    mLineCountersThreshold = ValidationHelper.GetInteger(SettingsHelper.AppSettings["CMSLineCountersThreshold"], 100000);
                }
                return mLineCountersThreshold.Value;
            }
        }


        /// <summary>
        /// Gets the key to the resolver name stored in the session.
        /// </summary>
        private string ResolverSessionKey
        {
            get
            {
                return mResolverSessionKey ?? (mResolverSessionKey = "ConditionBuilderResolver_" + Guid.NewGuid());
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Initializes a new instance of ExtendedTextArea class.
        /// </summary>
        public ExtendedTextArea()
        {
            // Set default values for overridden properties            
            base.TextMode = TextBoxMode.MultiLine;
            base.Wrap = false;

            AllowFullscreen = true;
            Names = new Hashtable();
            Lines = new Hashtable();
            Language = LanguageEnum.ASPNET;
            EditorMode = EditorModeEnum.Basic;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Retrieves scroll position markers from hidden fields.
        /// </summary>
        /// <param name="e">An System.EventArgs object that contains the event data</param>
        protected override void OnLoad(EventArgs e)
        {
            if (RequestHelper.IsPostBack() && MaintainScrollPositionOnPostback)
            {
                string[] position = (Page.Request.Form[PositionMarker] + ";;;").Split(';');

                ScrollPosition = ValidationHelper.GetInteger(position[0], 1);
                mHasFocus = ValidationHelper.GetBoolean(position[1], false);
                mCursorLine = ValidationHelper.GetInteger(position[2], 0);
                mCursorChar = ValidationHelper.GetInteger(position[3], 0);
            }

            // Decode macros before resigning if "raw" XML is edited
            DecodeMacrosBeforeSign = (Language == LanguageEnum.XML);
        }


        /// <summary>
        /// Registers client script for generating postback events prior to rendering on the client, if AutPostBack is true.
        /// </summary>
        /// <param name="e">An System.EventArgs object that contains the event data</param>
        protected override void OnPreRender(EventArgs e)
        {
            // Remove scrollbars when unnecessary
            Attributes.Add("style", "overflow: auto;");

            if (SyntaxHighlightingEnabled)
            {
                AdvancedOnPreRender(e);
            }
            else
            {
                BasicOnPreRender(e);
            }

            if (!IsLiveSite)
            {
                ScriptHelper.RegisterDialogScript(Page);
            }
        }


        /// <summary>
        /// Registers basic client script for generating postback events prior to rendering on the client, if AutoPostBack is true.
        /// </summary>
        /// <param name="e">An System.EventArgs object that contains the event data</param>
        private void BasicOnPreRender(EventArgs e)
        {
            if (EnableTabKey)
            {
                // Set javascript to IE or Mozzila
                if (BrowserHelper.IsIE())
                {
                    // IE
                    ScriptHelper.RegisterClientScriptBlock(this, GetType(), "TabKeyTextBox", ScriptHelper.GetScript(
    @"
function TabKeyTextBox(e, elem, tabKey) { 
    var key = window.event ? event.keyCode : e.which;
    if ( key == 9 ) {
        var el = document.getElementById(elem);
        el.selection = document.selection.createRange();
        el.selection.text = tabKey;
        return false;
    } else {
        return true
    };
}
"
                    ));

                    Attributes.Add("onkeydown", string.Format("return TabKeyTextBox(event,'{0}' , '{1}');", ClientID, TabKey));
                }
                else
                {
                    // Other browsers
                    ScriptHelper.RegisterClientScriptBlock(this, GetType(), "TabKeyTextBox", ScriptHelper.GetScript(
    @"
function TabKeyTextBox(e, elem, tabKey) { 
    var key = window.event ? event.keyCode : e.which;
    if ( key == 9 ) {
        var pos=0; 
        var el = document.getElementById(elem);
        pos = el.selectionStart + tabKey.length;
        var sp = el.scrollTop;
        el.value = el.value.substr(0, el.selectionStart) + tabKey + el.value.substr(el.selectionStart,el.value.length);
        el.setSelectionRange(pos, pos);
        el.scrollTop=sp;
        el.focus();
        return false;
    } else {
        return true;
    }
}
"
                    ));

                    Attributes.Add("onkeydown", string.Format("return TabKeyTextBox(event,'{0}' , '{1}');", ClientID, TabKey));
                }
            }

            base.OnPreRender(e);
        }


        /// <summary>
        /// Registers advanced client script for generating postback events prior to rendering on the client, if AutPostBack is true.
        /// </summary>
        /// <param name="e">An System.EventArgs object that contains the event data</param>
        private void AdvancedOnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            // register script & stylesheet in this page
            RegisterCodeMirror();
        }


        /// <summary>
        /// Renders the ExtendedTextArea control to the specified HtmlTextWriter object.
        /// </summary>
        /// <param name="writer">The HtmlTextWriter that receives the rendered output</param>
        protected override void Render(HtmlTextWriter writer)
        {
            if (SyntaxHighlightingEnabled)
            {
                AdvancedRender(writer);
            }
            else
            {
                BasicRender(writer);
            }
        }


        /// <summary>
        /// Renders the ExtendedTextArea control to the specified HtmlTextWriterobject in Basic mode.
        /// </summary>
        /// <param name="writer">The HtmlTextWriter that receives the rendered output</param>
        private void BasicRender(HtmlTextWriter writer)
        {
            base.Render(writer);
        }


        /// <summary>
        /// Renders the ExtendedTextArea control to the specified HtmlTextWriter object in Advanced mode.
        /// </summary>
        /// <param name="writer">The HtmlTextWriter that receives the rendered output</param>
        private void AdvancedRender(HtmlTextWriter writer)
        {
            // Select the color scheme depending on language used
            string colorScheme;

            switch (Language)
            {
                case LanguageEnum.ASPNET:
                case LanguageEnum.HTML:
                case LanguageEnum.HTMLMixed:
                    colorScheme = "silver";
                    break;

                case LanguageEnum.CSS:
                case LanguageEnum.LESS:
                    colorScheme = "orange";
                    break;

                case LanguageEnum.SQL:
                    colorScheme = "green";
                    break;

                default:
                    colorScheme = "blue";
                    break;
            }

            this.AddCssClass(string.Format("{0} {0}-{1}", CODEMIRROR_CSSCLASS, colorScheme));

            if (SingleLineMode)
            {
                this.AddCssClass("CM-singleline");
            }

            // Add additional classes
            if (DynamicHeight)
            {
                this.AddCssClass("AutoSize");
            }
            if (ReadOnly || !IsEnabled)
            {
                this.AddCssClass("ReadOnly");
            }

            // Wrap CodeMirror editor in a containing div so that CSS can be used and remove original CSS to prevent mixing of styles 
            writer.AddAttribute(HtmlTextWriterAttribute.Class, CssClass);
            CssClass = "";

            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            if (DynamicHeight)
            {
                this.AddCssClass("DynamicHeightTextBox");
            }

            // Renders a textarea that will be replaced with CodeMirror on runtime
            base.Render(writer);

            writer.RenderEndTag();
            writer.WriteLine();
        }


        /// <summary>
        /// Gets the scroll position value
        /// </summary>
        protected override string GetScrollPositionValue()
        {
            return SyntaxHighlightingEnabled ? String.Format("{0};{1};{2};{3}", ScrollPosition, mHasFocus, mCursorLine, mCursorChar) : base.GetScrollPositionValue();
        }


        /// <summary>
        /// Registers CodeMirror script and stylesheet for current page.
        /// </summary>
        private void RegisterCodeMirror()
        {
            if (!Visible)
            {
                return;
            }

            // Register CodeMirror's JS file            
            ScriptHelper.RegisterScriptFile(Page, CODEMIRROR_BASE_PATH + CODEMIRROR_SCRIPT_FILE);

            // Register CodeMirror's language file
            ScriptHelper.RegisterScriptFile(Page, GetLanguageFile());

            // Register CodeMirror's CSS file (CodeMirror is styled in Bootstrap)
            CssRegistration.RegisterBootstrap(Page);

            string mode = GetMode(Language);

            CssRegistration.RegisterCssLink(Page, CODEMIRROR_BASE_PATH + "/theme/cms.css");

            // Parser
            switch (mode.ToLowerCSafe())
            {
                case "htmlmixed":
                    // Register nested modes
                    RegisterMode("css");
                    RegisterMode("xml");
                    RegisterMode("javascript");
                    break;

                case "aspnet":
                    // Register nested modes
                    RegisterMode("css");
                    RegisterMode("xml");
                    RegisterMode("javascript");
                    RegisterMode("clike");
                    break;
            }

            RegisterMode(mode);

            // Macro mode requires c-like syntax
            if (mHighlightMacros)
            {
                RegisterMode("clike");

                ScriptHelper.RegisterScriptFile(Page, CODEMIRROR_BASE_PATH + "/lib/util/overlay.js");
            }

            ScriptHelper.RegisterScriptFile(Page, CODEMIRROR_BASE_PATH + "/lib/util/formatting.js");
            ScriptHelper.RegisterScriptFile(Page, CODEMIRROR_BASE_PATH + "/lib/util/searchcursor.js");

            string basePath = ResolveUrl(CODEMIRROR_BASE_PATH);
            bool readOnly = (ReadOnly || !IsEnabled);

            string insertImageDialogUrl = ShowInsertImage ? CMSDialogHelper.GetDialogUrl(InsertImageDialogConfiguration ?? new DialogConfiguration(), IsLiveSite, encode: true) : String.Empty;

            // Prepare the script
            string script = $@"
var {EditorID} = CodeMirror.fromTextArea(document.getElementById('{ClientID}'),
{{
    mode: '{mode + (mHighlightMacros ? "_macro" : "")}',
    matchBrackets: {BraceMatching.ToString().ToLowerCSafe()},
    lineNumbers: {(ShowLineNumbers && ShowLineNumbersGlobal).ToString().ToLowerCSafe()},
    readOnly: {readOnly.ToString().ToLowerCSafe()},
    path: '{basePath}',
    width: '{Width}',
    height: '{(DynamicHeight ? "dynamic" : Height.ToString())}',
    direction: '{(CultureHelper.IsUICultureRTL() ? "rtl" : "")}',   
    lineCounting: {GetShowLineCounters(Text)},
    name: '{EditorID}',
    autoSize: {AutoSize.ToString().ToLowerCSafe()},
    dynamicHeight: {DynamicHeight.ToString().ToLowerCSafe()},
    parentElement: '{ParentElementID}',
    fullScreenParentID: '{FullScreenParentElementID}',
    allowFullscreen: {AllowFullscreen.ToString().ToLowerCSafe()},
    showInsertMacro: {ShowInsertMacro.ToString().ToLowerCSafe()},
    isMacroMixedMode: {IsMacroMixedMode.ToString().ToLowerCSafe()},
    resolverSessionKey: '{ResolverSessionKey}',
    singleLineMode: {SingleLineMode.ToString().ToLowerCSafe()},
    onBlur: function() {{ {GetSaveScript(false)} }},
    showInsertImage: {ShowInsertImage.ToString().ToLowerInvariant()},
    insertImageDialogUrl: '{insertImageDialogUrl}'
}});
{GetBookmarksScript()}
{GetToolbarScript()}
";

            // Inject the script block            
            ScriptHelper.RegisterStartupScript(this, GetType(), "ETA_Init_" + ClientID, script, true);

            // Restore scroll position
            script = RestoreScrollPosition();

            if (!String.IsNullOrEmpty(script))
            {
                ScriptHelper.RegisterStartupScript(Page, GetType(), "ETA_Startup_" + ClientID, script, true);
            }

            // Register script that forces highlighter to update its value on partial postback and save position            
            ScriptHelper.RegisterOnSubmitStatement(Page, typeof(string), "ETA_Update_" + ClientID, GetSaveScript(true));
        }


        /// <summary>
        /// Explicitly sets the editor position
        /// </summary>
        /// <param name="focus">Focus the editor</param>
        /// <param name="line">Line to jump to</param>
        /// <param name="ch">Char on the line to jump to</param>
        public void SetPosition(bool focus, int line, int ch)
        {
            mSetPosition = true;
            ScrollPosition = 0;
            mHasFocus = focus;
            mCursorLine = line;
            mCursorChar = ch;
        }


        /// <summary>
        /// Returns the script that sets the remembered scroll position
        /// </summary>
        private string RestoreScrollPosition()
        {
            if (mSetPosition || (RequestHelper.IsPostBack() && MaintainScrollPositionOnPostback && ((ScrollPosition > 0) || (LineNumber > 0) || (mCursorChar > 0) || mHasFocus)))
            {
                return String.Format("{0}.restorePosition({1}, {2}, {3}, {4});", EditorID, ScrollPosition, mHasFocus.ToString().ToLowerCSafe(), mCursorLine, mCursorChar);
            }

            return null;
        }


        /// <summary>
        /// Registers the given parsing mode
        /// </summary>
        /// <param name="mode">Mode to register</param>
        private void RegisterMode(string mode)
        {
            ScriptHelper.RegisterScriptFile(Page, String.Format("{0}/mode/{1}/{1}.js", CODEMIRROR_BASE_PATH, mode));
        }


        /// <summary>
        /// Returns a toolbar script to inject into this control during rendering (if ShowToolbar is true).
        /// </summary>
        /// <returns>A script that displays a toolbar for CodeMirror at runtime.</returns>
        private string GetToolbarScript()
        {
            // Do not show toolbar in single line mode
            if (mShowToolbar && !SingleLineMode)
            {
                return string.Format(@"var {0}_toolbar = new Toolbar({0});", EditorID);
            }
            else
            {
                return string.Empty;
            }
        }


        /// <summary>
        /// Returns a path to the language file for current (preferred) culture. 
        /// When specified file cannot be found, a default language file with invariant culture is used.
        /// </summary>        
        /// <returns>String that represents a file with dictionary containing localized strings</returns>
        private static string GetLanguageFile()
        {
            string cultureCode = MembershipContext.AuthenticatedUser.PreferredUICultureCode.ToLowerCSafe();
            string langFile = string.Format(@"{0}/lang/{1}.js", CODEMIRROR_BASE_PATH, cultureCode);

            // Check if localized resources exist for the specified language, otherwise substitute for invariant culture
            if (!FileHelper.FileExists(langFile))
            {
                langFile = string.Format(@"{0}/lang/en-us.js", CODEMIRROR_BASE_PATH);
            }

            return langFile;
        }


        /// <summary>
        /// Returns a bookmarks script to inject into this control during rendering (if ShowBookmarks is true).
        /// </summary>
        /// <returns>A script that displays a bookmarks panel for CodeMirror at runtime.</returns>
        private string GetBookmarksScript()
        {
            if (ShowBookmarks && !ReadOnly)
            {
                // Check if dynamic bookmarks should be used
                if (DynamicBookmarks)
                {
                    string bookmarkStyle = GetBookmarkStyle(Language);
                    return string.Format(@"var {0}_bookmarks = new DynamicBookmarks({0}, '{1}');", EditorID, bookmarkStyle);
                }
                else
                {
                    // Get bookmarks from text that will be rendered
                    ExtractBookmarks();
                    string bookmarks = GetPreformattedBookmarks();

                    return string.Format(@"var {0}_bookmarks = new StaticBookmarks({0}, {1});", EditorID, bookmarks);
                }
            }
            else
            {
                return string.Empty;
            }
        }


        /// <summary>
        /// Returns script that forces highlighter to update its value on partial postback and save position
        /// </summary>
        /// <param name="submit">If true, the script is for submit statement</param>
        private string GetSaveScript(bool submit)
        {
            return String.Format(@"{0}.save(); {1}",
                EditorID,
                (MaintainScrollPositionOnPostback ? String.Format(@"{0}.savePosition('{1}', {2});", EditorID, PositionMarker, submit.ToString().ToLowerCSafe()) : ""));
        }


        /// <summary>
        /// Returns the parsing mode for the syntax highlighting
        /// </summary>
        /// <param name="language">One of the supported languages (see LanguageEnum enumeration)</param>
        private static string GetMode(LanguageEnum language)
        {
            // Get array of script files for a given language
            switch (language)
            {
                case LanguageEnum.CSS:
                    return "css";

                case LanguageEnum.CSharp:
                case LanguageEnum.CMSSharp:
                    return "clike";

                case LanguageEnum.JavaScript:
                    return "javascript";

                case LanguageEnum.ASPNET:
                    return "aspnet";

                case LanguageEnum.HTML:
                case LanguageEnum.HTMLMixed:
                    return "htmlmixed";

                case LanguageEnum.SQL:
                    return "plsql";

                case LanguageEnum.XML:
                    return "xml";

                case LanguageEnum.LESS:
                    return "less";

                default:
                    return "xml";
            }
        }


        /// <summary>
        /// Returns a bookmark style to use in the editor for a specified language.
        /// </summary>
        /// <param name="language">One of the supported languages from LanguageEnum enumeration</param>
        /// <returns>Text code that is used to determine what format to use when searching for bookmarks</returns>
        private static string GetBookmarkStyle(LanguageEnum language)
        {
            switch (language)
            {
                case LanguageEnum.ASPNET:
                    return "ASP";

                case LanguageEnum.CSS:
                    return "CSS";

                case LanguageEnum.CSharp:
                    return "C#";

                case LanguageEnum.CMSSharp:
                    return "C#";

                case LanguageEnum.JavaScript:
                    return "JS";

                case LanguageEnum.HTML:
                    return "XML";

                case LanguageEnum.HTMLMixed:
                    return "XML";

                case LanguageEnum.SQL:
                    return "SQL";

                case LanguageEnum.XML:
                    return "XML";

                case LanguageEnum.LESS:
                    return "LESS";

                default:
                    return string.Empty;
            }
        }


        /// <summary>
        /// Returns a language specific regular expression to use when creating bookmars for the editor.
        /// </summary>
        /// <param name="language">One of the supported languages from LanguageEnum enumeration</param>
        /// <returns>Regular expression that represents a bookmark in a specified language</returns>
        private static string GetBookmarkRegex(LanguageEnum language)
        {
            switch (language)
            {
                case LanguageEnum.ASPNET:
                    return @"<%--#(\w\|\s)+#--%>";

                case LanguageEnum.CSS:
                case LanguageEnum.LESS:
                    return @"/\*\s*#\s*([\w-/\+\*.=~\!@\$%\^&\(\[\]\);:<>\?\s]+)\s*#\s*\*/";

                case LanguageEnum.CSharp:
                    return @"#region (\w|\s)+";

                case LanguageEnum.JavaScript:
                    return @"(/){2}#(\w|\s)+#";

                case LanguageEnum.HTML:
                    return @"<!--#(\w|\s)+#-->";

                case LanguageEnum.HTMLMixed:
                    return @"<!--#(\w|\s)+#-->";

                case LanguageEnum.SQL:
                    return @"--#(\w|\s)+#";

                case LanguageEnum.XML:
                    return @"<!--#(\w|\s)+#-->";

                default:
                    return string.Empty;
            }
        }


        /// <summary>
        /// Determines if the line counters are displayed in the toolbar depending on the number of lines to show.
        /// </summary>
        /// <param name="text">Text to be displayed</param>
        /// <returns>True if line counters are displayed, otherwise false</returns>
        private static string GetShowLineCounters(string text)
        {
            // Counts line breaks in the text to display (has to be fast)
            int count = 1;
            int start = 0;
            int maxLines = LineCountersThreshold;
            while ((start = text.IndexOfCSafe('\n', start)) != -1)
            {
                count++;
                start++;
                if (count > maxLines)
                {
                    return "false";
                }
            }

            return "true";
        }


        /// <summary>
        /// Retrieves bookmarks from the editor text and stores them in the hashtables.
        /// </summary>
        public void ExtractBookmarks()
        {
            if (string.IsNullOrEmpty(Text) || (Count > 0))
            {
                return;
            }

            // Get parsing regex if specified explicitly or fall back to language default
            string regexStr = string.IsNullOrEmpty(RegularExpression) ? GetBookmarkRegex(Language) : RegularExpression;
            Names.Clear();
            Lines.Clear();
            Count = 0;

            using (StringReader reader = new StringReader(Text))
            {
                LineNumber = 1;
                string line;
                CMSRegex regex = new CMSRegex(regexStr, RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

                do
                {
                    line = reader.ReadLine();
                    LineNumber++;

                    // All lines matching the bookmark regex will be added to the array
                    if (!string.IsNullOrEmpty(line) && regex.IsMatch(line.Trim()))
                    {
                        // Return name of bookmark that lies between macro delimiters
                        int start = line.IndexOfCSafe(MacroDelimiter);
                        int end = line.LastIndexOfCSafe(MacroDelimiter);
                        end = (((end > -1) && (end != start)) ? end : line.Length);
                        start = (start > -1 ? start + MacroDelimiter.Length : 0);

                        string currentName = line.Substring(start, end - start).Trim();

                        // Add bookmark and its line number to their respective tables
                        Names[Count] = currentName;
                        Lines[Count] = LineNumber;
                        Count++;
                    }
                } while (line != null);
            }
        }


        /// <summary>
        /// Preparses the bookmarks retrieved from the text.
        /// </summary>
        /// <returns>A string representing an array of pairs, where pair consists of a name and line number</returns>
        public string GetPreformattedBookmarks()
        {
            // Build string arrays that will hold bookmarks arranged in an array of [name, lineNumber] pairs
            StringBuilder bookmarks = new StringBuilder();
            bookmarks.Append("[");

            if (Count > 0)
            {
                // Create sorted dictionary with bookmarks and their respective line numbers
                SortedDictionary<string, int> sortedBookmarks = new SortedDictionary<string, int>();
                for (int i = 0; i < Count; i++)
                {
                    string tempValue = Names[i] as string;

                    try
                    {
                        sortedBookmarks.Add(tempValue, (int)Lines[i] - 1);
                    }
                    catch (ArgumentException)
                    {
                        // Duplicate key might already exist, so append a single whitespace
                        while (sortedBookmarks.ContainsKey(tempValue))
                        {
                            tempValue = tempValue + " ";
                        }

                        sortedBookmarks.Add(tempValue, (int)Lines[i] - 1);
                    }
                }

                // Format bookmarks into hierarchical tree
                foreach (KeyValuePair<string, int> couple in sortedBookmarks)
                {
                    string tempMark = couple.Key.Trim();
                    int count = tempMark.Count(x => x == mSectionSeparator);

                    if (count > 0)
                    {
                        tempMark = tempMark.Substring(tempMark.LastIndexOfCSafe(mSectionSeparator) + 1);
                        tempMark = tempMark.PadLeft((count * mSectionPadding + tempMark.Length), '\u00A0');
                    }

                    bookmarks.AppendFormat(@"['{0}', {1}], ", tempMark, couple.Value);
                }

                // Manually delete last two chars ", " to ensure the array is correctly formed before passing it to JS
                bookmarks.Remove(bookmarks.Length - 2, 2);
            }

            bookmarks.Append("]");
            return bookmarks.ToString();
        }


        /// <summary>
        /// Return command which sets given value (general JS command) to the editor (handles case when SyntaxHighlighting is disabled).
        /// </summary>
        /// <param name="textToSetJsCommand">JS command to set as a value to the editor</param>
        public string GetValueSetterCommand(string textToSetJsCommand)
        {
            if (SyntaxHighlightingEnabled)
            {
                return EditorID + ".setValue(" + textToSetJsCommand + ");";
            }
            else
            {
                return "document.getElementById('" + ClientID + "').value = " + textToSetJsCommand + ";";
            }
        }


        /// <summary>
        /// Return command which gets value of editor (handles case when SyntaxHighlighting is disabled).
        /// </summary>
        public string GetValueGetterCommand()
        {
            if (SyntaxHighlightingEnabled)
            {
                return EditorID + ".getValue()";
            }
            else
            {
                return "document.getElementById('" + ClientID + "').value";
            }
        }

        #endregion
    }
}
