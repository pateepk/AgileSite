using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.Core;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.MacroEngine;
using CMS.Membership;
using CMS.Modules;
using CMS.SiteProvider;

namespace CMS.CKEditor.Web.UI
{
    /// <summary>
    /// Provides an Web Control for the CKEditor.
    /// </summary>
    [DefaultProperty("Value")]
    [ValidationProperty("Value")]
    [ToolboxData("<{0}:CKEditor runat=server></{0}:CKEditor>")]
    [Designer("CMS.CKEditor.Web.UI.CKEditorDesigner")]
    [ParseChildren(false)]
    public class CKEditorControl : Panel, IPostBackDataHandler
    {
        #region "Constants"

        private const string INLINE_CONTROL_CODE_TYPE = "CMSInlineControl";

        #endregion


        #region "Variables"

        private string mValue;
        private string mBasePath;
        private bool mIsInUpdatePanel;
        private bool sharedToolbar = true;

        private readonly List<string> mRemovePlugins = new List<String>();
        private readonly List<string> mRemoveButtons = new List<String>();
        private readonly List<string> mExtraPlugins = new List<String>(new[] { "CMSPlugins" });

        private DialogConfiguration mMediaDialogConfig;
        private DialogConfiguration mLinkDialogConfig;
        private DialogConfiguration mQuickInsertConfig;
        private DialogConfiguration mUrlDialogConfig;

        private static Regex mObjectRegExp;
        private static Regex mParamsRegExp;
        private static Regex mObjectTypeRegExp;
        private static Regex mObjectWidthRegExp;
        private static Regex mObjectHeightRegExp;
        private static Regex mObjectControlTypeRegExp;

        #endregion


        #region "Regular expresions"

        /// <summary>
        /// Object tag regular expression.
        /// </summary>
        internal static Regex ObjectRegExp
        {
            get
            {
                return mObjectRegExp ?? (mObjectRegExp = RegexHelper.GetRegex(@"<object\b.*?>(.*?)</object>"));
            }
        }


        /// <summary>
        /// Param tag regular expression.
        /// </summary>
        internal static Regex ParamsRegExp
        {
            get
            {
                return mParamsRegExp ?? (mParamsRegExp = RegexHelper.GetRegex(@"<param\s+(?:(name|value)=""([^""]*)""\s+)+\s*?/>"));
            }
        }


        /// <summary>
        /// Object type regular expression.
        /// </summary>
        internal static Regex ObjectTypeRegExp
        {
            get
            {
                return mObjectTypeRegExp ?? (mObjectTypeRegExp = RegexHelper.GetRegex(@"\stype=""([^""]*)"""));
            }
        }


        /// <summary>
        /// Regular expression that matches codetype attribute.
        /// </summary>
        internal static Regex ObjectCodeTypeRegExp
        {
            get
            {
                return mObjectControlTypeRegExp ?? (mObjectControlTypeRegExp = RegexHelper.GetRegex(@"\scodetype=""([^""]*)"""));
            }
        }


        /// <summary>
        /// Object width regular expression.
        /// </summary>
        internal static Regex ObjectWidthRegExp
        {
            get
            {
                return mObjectWidthRegExp ?? (mObjectWidthRegExp = RegexHelper.GetRegex(@"\swidth=""([^""]*)"""));
            }
        }


        /// <summary>
        /// Object width regular expression.
        /// </summary>
        internal static Regex ObjectHeightRegExp
        {
            get
            {
                return mObjectHeightRegExp ?? (mObjectHeightRegExp = RegexHelper.GetRegex(@"\sheight=""([^""]*)"""));
            }
        }

        #endregion


        #region "Base Configurations Properties"

        /// <summary>
        /// Culture code to set the culture of editor dialogs.
        /// </summary>
        public string DialogCultureCode
        {
            get;
            set;
        }


        /// <summary>
        /// Container of the CKEditor configuration.
        /// </summary>
        [Browsable(false)]
        public CKEditorConfiguration Config
        {
            get
            {
                if (ViewState["Config"] == null)
                {
                    ViewState["Config"] = new CKEditorConfiguration();
                }
                return (CKEditorConfiguration)ViewState["Config"];
            }
        }


        /// <summary>
        /// The HTML content of the editor.
        /// </summary>
        [DefaultValue("")]
        public string Value
        {
            get
            {
                return mValue;
            }
            set
            {
                if (MacroProcessor.ContainsMacro(value))
                {
                    mValue = MacroSecurityProcessor.RemoveSecurityParameters(value, true, null);
                }
                else
                {
                    mValue = value;
                }

                mValue =  XmlHelper.RemoveIllegalCharacters(mValue);
            }
        }
        
        
        /// <summary>
        ///<p>
        /// Sets or gets the virtual path to the editor's directory.
        /// </p>
        ///<p>
        /// The default value is "~/CMSAdminControls/CKEditor/".
        /// </p>
        ///<p>
        /// The base path can be also set in the Web.config file using the 
        /// appSettings section. Just set the "CKEditor:BasePath" for that. 
        /// For example:
        /// codeconfigurationappSettingsadd key="CKEditor:BasePath" value="/scripts/ckeditor/" //appSettings/configuration/code>
        /// </p>
        /// </summary>
        [DefaultValue("~/CMSAdminControls/CKEditor/")]
        public string BasePath
        {
            get
            {
                object o = ViewState["BasePath"] ?? SettingsHelper.AppSettings["CKEditor:BasePath"];

                return ValidationHelper.GetString(o, "~/CMSAdminControls/CKEditor/");
            }
            set
            {
                ViewState["BasePath"] = value;
            }
        }


        /// <summary>
        /// Indicates if editor should use source files for development and debug.
        /// </summary>
        [Category("Configurations")]
        [DefaultValue(false)]
        private bool UseSource
        {
            get
            {
                object o = ViewState["UseSource"] ?? SettingsHelper.AppSettings["CKEditor:UseSource"];
                return ValidationHelper.GetBoolean(o, false);
            }
            set
            {
                ViewState["UseSource"] = value;
            }
        }


        /// <summary>
        /// Editor area CSS.
        /// </summary>
        [Category("Configurations")]
        public string EditorAreaCSS
        {
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    Config.Remove("ContentsCss");
                }
                else
                {
                    ContentsCss = ResolveUrl(value);
                }
            }
        }


        /// <summary>
        /// Returns the object name of the CKEditor instance, to be used in javascript.
        /// </summary>
        public string ObjectID
        {
            get
            {
                return "CKEDITOR.instances." + ClientID;
            }
        }


        /// <summary>
        /// Indicates whether CKEditor is used in inline mode. This property is set to config manually, because its needed earlier then custom config is built.
        /// </summary>
        public bool UseInlineMode
        {
            get;
            set;
        }


        /// <summary>
        /// Resolved editor content.
        /// </summary>
        [DefaultValue("")]
        public string ResolvedValue
        {
            get
            {
                string value = Value;
                if (string.IsNullOrEmpty(value))
                {
                    return value;
                }

                value = UnEscapeMacros(value);
                value = UnResolveInlineControls(value);
                value = HTMLHelper.UnResolveUrls(value, null);

                if (FixXHTML)
                {
                    value = HTMLHelper.FixXHTML(value, new FixXHTMLSettings()
                    {
                        SelfClose = true,
                        Javascript = true,
                        LowerCase = true
                    }, null);
                }

                if (FormatURL)
                {
                    value = HTMLHelper.FixUrl(value);
                }

                if (!MacroStaticSettings.AllowOnlySimpleMacros && MacroProcessor.ContainsMacro(value))
                {
                    value = MacroSecurityProcessor.AddSecurityParameters(value, MacroIdentityOption.FromUserInfo(MembershipContext.AuthenticatedUser), null);
                }

                return value;
            }
            set
            {
                string resolveValue = HTMLHelper.ResolveUrls(value, null);
                resolveValue = ResolveInlineControls(resolveValue);

                Value = resolveValue;
            }
        }


        /// <summary>
        /// Indicates if CKEditor is on live site.
        /// </summary>
        [Category("Configurations")]
        public bool IsLiveSite
        {
            get
            {
                string c = Config["IsLiveSite"];
                if (c == null)
                {
                    return false;
                }
                return bool.Parse(c);
            }
            set
            {
                Config["IsLiveSite"] = (value ? "true" : "false");
            }
        }


        /// <summary>
        /// Resolver name used in "Insert macro" plugin.
        /// </summary>
        [Category("Configurations")]
        public string ResolverName
        {
            get
            {
                return Config["ResolverName"];
            }
            set
            {
                Config["ResolverName"] = value;
            }
        }


        /// <summary>
        /// Additional parameters for the editor dialogs.
        /// </summary>
        [Category("Configurations")]
        public string DialogParameters
        {
            set
            {
                Config["DialogParameters"] = value;
            }
        }


        /// <summary>
        /// Media dialog configuration.
        /// </summary>
        [Category("Configurations")]
        public DialogConfiguration MediaDialogConfig
        {
            get
            {
                return mMediaDialogConfig ?? (mMediaDialogConfig = new DialogConfiguration());
            }
            set
            {
                mMediaDialogConfig = value;
            }
        }


        /// <summary>
        /// Link dialog configuration.
        /// </summary>
        [Category("Configurations")]
        public DialogConfiguration LinkDialogConfig
        {
            get
            {
                return mLinkDialogConfig ?? (mLinkDialogConfig = new DialogConfiguration());
            }
            set
            {
                mLinkDialogConfig = value;
            }
        }


        /// <summary>
        /// Quickly insert media configuration.
        /// </summary>
        [Category("Configurations")]
        public DialogConfiguration QuickInsertConfig
        {
            get
            {
                return mQuickInsertConfig ?? (mQuickInsertConfig = new DialogConfiguration());
            }
            set
            {
                mQuickInsertConfig = value;
            }
        }


        /// <summary>
        /// URL dialog configuration.
        /// </summary>
        [Category("Configurations")]
        public DialogConfiguration UrlDialogConfig
        {
            get
            {
                return mUrlDialogConfig ?? (mUrlDialogConfig = new DialogConfiguration());
            }
            set
            {
                mUrlDialogConfig = value;
            }
        }

        /// <summary>
        /// If true, the language should be automatically detected.
        /// </summary>
        [Category("Configurations")]
        public bool AutoDetectLanguage
        {
            set
            {
                Config["AutoDetectLanguage"] = (value ? "true" : "false");
            }
        }


        /// <summary>
        /// Starting path (for the content section).
        /// </summary>
        [Category("Configurations")]
        public string StartingPath
        {
            set
            {
                Config["StartingPath"] = value;

                const string PARAMETER_PATH = "path";

                if (!String.IsNullOrEmpty(FileBrowserImageBrowseUrl))
                {
                    FileBrowserImageBrowseUrl = ActualizeUrl(FileBrowserImageBrowseUrl, PARAMETER_PATH, value);
                }
                if (!String.IsNullOrEmpty(FileBrowserLinkBrowseUrl))
                {
                    FileBrowserLinkBrowseUrl = ActualizeUrl(FileBrowserLinkBrowseUrl, PARAMETER_PATH, value);
                }
            }
        }


        /// <summary>
        /// Gets or sets the value that indicates whether dirty bit should be used for value property
        /// If this bit is true, value is not forcibly loaded after postback if value was set
        /// </summary>
        [Category("Configurations")]
        public bool UseValueDirtyBit
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the current node.
        /// </summary>
        public ITreeNode Node
        {
            get;
            set;
        }

        #endregion


        #region "Configurations Properties"

        /// <summary>
        /// The maximum height to which the editor can reach using AutoGrow.
        /// Zero means unlimited. 
        ///
        /// Default value: 0
        /// </summary>
        [Category("Configurations")]
        public int AutoGrowMaxHeight
        {
            get
            {
                string c = Config["autoGrow_maxHeight"];
                if (c == null)
                {
                    return 0;
                }
                return int.Parse(c);
            }
            set
            {
                Config["autoGrow_maxHeight"] = value.ToString();
            }
        }


        /// <summary>
        /// The minimum height to which the editor can reach using AutoGrow.  
        ///
        /// Default value: 200
        /// </summary>
        [Category("Configurations")]
        public int AutoGrowMinHeight
        {
            get
            {
                string c = Config["autoGrow_minHeight"];
                if (c == null)
                {
                    return 200;
                }
                return int.Parse(c);
            }
            set
            {
                Config["autoGrow_minHeight"] = value.ToString();
            }
        }


        /// <summary>
        /// Whether the replaced element (usually a textarea) is to be 
        /// updated automatically when posting the form containing the editor.
        ///
        /// Default value: true
        /// </summary>
        [Category("Configurations")]
        public bool AutoUpdateElement
        {
            get
            {
                string c = Config["autoUpdateElement"];
                if (c == null)
                {
                    return true;
                }
                return bool.Parse(c);
            }
            set
            {
                Config["autoUpdateElement"] = (value ? "true" : "false");
            }
        }


        /// <summary>
        /// The base Z-index for floating dialogs and popups.
        ///
        /// Default value: 10000
        /// </summary>
        [Category("Configurations")]
        public int BaseFloatZIndex
        {
            get
            {
                string c = Config["baseFloatZIndex"];
                if (c == null)
                {
                    return 10000;
                }
                return int.Parse(c);
            }
            set
            {
                Config["baseFloatZIndex"] = value.ToString();
            }
        }


        /// <summary>
        /// The base href URL used to resolve relative and 
        /// absolute URLs in the editor content.
        ///
        /// Default value: empty string
        /// 
        /// Set to null for default value
        /// </summary>
        /// <returns>null if the default value is set</returns>
        [Category("Configurations")]
        public string BaseHref
        {
            get
            {
                return Config["baseHref"];
            }
            set
            {
                Config["baseHref"] = value;
            }
        }


        /// <summary>
        /// The keystrokes that are blocked by default as the browser 
        /// implementation is buggy. These default keystrokes are handled by the editor.
        ///
        /// Default value: 
        /// [ CKEDITOR.CTRL + 66, CKEDITOR.CTRL + 73, CKEDITOR.CTRL + 85 ] // CTRL+B,I,U
        ///
        /// NOTE: This is a pure javascript value, so be careful not to break things!
        /// 
        /// Set to null for default value
        /// </summary>
        /// <returns>null if the default value is set</returns>
        [Category("Configurations")]
        public string BlockedKeystrokes
        {
            get
            {
                return Config["blockedKeystrokes"];
            }
            set
            {
                Config["blockedKeystrokes"] = value;
            }
        }


        /// <summary>
        /// Sets the "class" attribute to be used on the body 
        /// element of the editing area.
        ///
        /// Default value: empty string
        /// 
        /// Set to null for default value
        /// </summary>
        /// <returns>null if the default value is set</returns>
        [Category("Configurations")]
        public string BodyClass
        {
            get
            {
                return Config["bodyClass"];
            }
            set
            {
                Config["bodyClass"] = value;
            }
        }


        /// <summary>
        /// Sets the "id" attribute to be used on the body 
        /// element of the editing area.
        ///
        /// Default value: empty string
        /// 
        /// Set to null for default value
        /// </summary>
        /// <returns>null if the default value is set</returns>
        [Category("Configurations")]
        public string BodyId
        {
            get
            {
                return Config["bodyId"];
            }
            set
            {
                Config["bodyId"] = value;
            }
        }


        /// <summary>
        /// Whether to show the browser native context menu when 
        /// the CTRL or the META (Mac) key is pressed while opening 
        /// the context menu. 
        ///
        /// Default value: true
        /// </summary>
        [Category("Configurations")]
        public bool BrowserContextMenuOnCtrl
        {
            get
            {
                string c = Config["browserContextMenuOnCtrl"];
                if (c == null)
                {
                    return true;
                }
                return bool.Parse(c);
            }
            set
            {
                Config["browserContextMenuOnCtrl"] = (value ? "true" : "false");
            }
        }


        /// <summary>
        /// The CSS file(s) to be used to apply style to the contents. 
        /// It should reflect the CSS used in the final pages where the 
        /// contents are to be used.
        ///
        /// This can be a string or a string[] (Array).
        ///
        /// Default value: CKEDITOR.basePath + 'contents.css'
        /// 
        /// Set to null for default value
        /// </summary>
        /// <returns>null if the default value is set</returns>
        [Category("Configurations")]
        public string ContentsCss
        {
            get
            {
                return Config["contentsCss"];
            }
            set
            {
                Config["contentsCss"] = value;
            }
        }


        /// <summary>
        /// The writing direction of the language used to write the 
        /// editor contents. 
        /// Allowed values are 'ltr' for Left-To-Right language 
        /// (like English), or 'rtl' for Right-To-Left languages (like Arabic).
        ///
        /// Default value: ltr (LanguageDirection.LeftToRight)
        /// </summary>
        [Category("Configurations")]
        public LanguageDirection ContentsLangDirection
        {
            get
            {
                switch (Config["contentsLangDirection"])
                {
                    case "ltr":
                        return LanguageDirection.LeftToRight;
                    case "rtl":
                        return LanguageDirection.RightToLeft;
                    default:
                        return LanguageDirection.Ui;
                }
            }
            set
            {
                switch (value)
                {
                    case LanguageDirection.LeftToRight:
                        Config["contentsLangDirection"] = "ltr";
                        break;
                    case LanguageDirection.RightToLeft:
                        Config["contentsLangDirection"] = "rtl";
                        break;
                    default:
                    case LanguageDirection.Ui:
                        Config["contentsLangDirection"] = string.Empty;
                        break;
                }
            }
        }


        /// <summary>
        /// The URL path for the custom configuration file to be loaded. If not overloaded with inline configurations, it defaults to the "config.js" file present in the root of the CKEditor installation directory.
        ///
        /// CKEditor will recursively load custom configuration files defined inside other custom configuration files.
        ///
        /// Default value: CKEDITOR.basePath + 'config.js'
        /// 
        /// Set to null for default value
        /// </summary>
        /// <returns>null if the default value is set</returns>
        [Category("Configurations")]
        public string CustomConfig
        {
            get
            {
                return Config["customConfig"];
            }
            set
            {
                Config["customConfig"] = value;
            }
        }


        /// <summary>
        /// The characters to be used for indenting the HTML produced by the editor. 
        /// Using characters different than ' ' (space) and '\t' (tab) is definitely a bad idea as it'll mess the code.
        /// 
        /// Default value: '\t'
        /// 
        /// Set to null for default value
        /// </summary>
        /// <returns>null if the default value is set</returns>
        public string DataIndentationChars
        {
            get
            {
                return Config["dataIndentationChars"];
            }
            set
            {
                Config["dataIndentationChars"] = value;
            }
        }


        /// <summary>
        /// The language to be used if CKEDITOR.config.language is left 
        /// empty and it's not possible to localize the editor to the 
        /// user language.
        ///
        /// Default value: 'en'
        /// 
        /// Set to null for default value
        /// </summary>
        /// <returns>null if the default value is set</returns>
        [Category("Configurations")]
        public string DefaultLanguage
        {
            get
            {
                return Config["defaultLanguage"];
            }
            set
            {
                Config["defaultLanguage"] = value;
            }
        }


        /// <summary>
        /// The color of the dialog background cover. 
        /// It should be a valid CSS color string.
        ///
        /// Default value: 'white'
        /// </summary>
        [Category("Appearance")]
        [TypeConverter(typeof(WebColorConverter))]
        public Color DialogBackgroundCoverColor
        {
            get
            {
                string c = Config["dialog_backgroundCoverColor"];
                if (c == null)
                {
                    return Color.White;
                }
                return ColorTranslator.FromHtml(c);
            }
            set
            {
                Config["dialog_backgroundCoverColor"] = String.Format("#{0:x2}{1:x2}{2:x2}", value.R, value.G, value.B);
            }
        }


        /// <summary>
        /// The opacity of the dialog background cover. 
        /// It should be a number within the range [0.0, 1.0]. 
        ///
        /// Default value: 0.5
        /// </summary>
        [Category("Configurations")]
        public decimal DialogBackgroundCoverOpacity
        {
            get
            {
                string c = Config["dialog_backgroundCoverOpacity"];
                if (c == null)
                {
                    return 0.5m;
                }
                return decimal.Parse(c);
            }
            set
            {
                Config["dialog_backgroundCoverOpacity"] = value.ToString("0.##");
            }
        }


        /// <summary>
        /// The guideline to follow when generating the dialog buttons. There are 3 possible options:
        /// 'OS' - the buttons will be displayed in the default order of the user's OS;
        /// 'ltr' - for Left-To-Right order;
        /// 'rtl' - for Right-To-Left order
        ///
        /// Default value: OS
        /// </summary>
        [Category("Configurations")]
        public DialogButtonsOrder DialogButtonsOrder
        {
            get
            {
                switch (Config["dialog_buttonsOrder"])
                {
                    case "rtl":
                        return DialogButtonsOrder.Rtl;
                    case "ltr":
                        return DialogButtonsOrder.Ltr;
                    default:
                    case "OS":
                        return DialogButtonsOrder.OS;
                }
            }
            set
            {
                switch (value)
                {
                    case DialogButtonsOrder.Rtl:
                        Config["dialog_buttonsOrder"] = "rtl";
                        break;
                    case DialogButtonsOrder.Ltr:
                        Config["dialog_buttonsOrder"] = "ltr";
                        break;
                    default:
                    case DialogButtonsOrder.OS:
                        Config["dialog_buttonsOrder"] = "OS";
                        break;
                }
            }
        }


        /// <summary>
        /// The distance of magnetic borders used in moving and 
        /// resizing dialogs, measured in pixels.
        ///
        /// Default value: 20
        /// </summary>
        [Category("Configurations")]
        public int DialogMagnetDistance
        {
            get
            {
                string c = Config["dialog_magnetDistance"];
                if (c == null)
                {
                    return 20;
                }
                return int.Parse(c);
            }
            set
            {
                Config["dialog_magnetDistance"] = value.ToString();
            }
        }


        /// <summary>
        /// Tells if user should not be asked to confirm close, if any dialog field was modified. 
        /// By default it is set to false meaning that the confirmation dialog will be shown.
        ///
        /// Default value: false
        /// </summary>
        [Category("Configurations")]
        public bool DialogNoConfirmCancel
        {
            get
            {
                string c = Config["dialog_noConfirmCancel"];
                if (c == null)
                {
                    return true;
                }
                return bool.Parse(c);
            }
            set
            {
                Config["dialog_noConfirmCancel"] = (value ? "true" : "false");
            }
        }


        /// <summary>
        /// Disables the built-in words spell checker if browser provides one. 
        /// 
        /// Note: Although word suggestions provided by browsers (natively) will not appear in CKEditor's default 
        /// context menu, users can always reach the native context menu by holding the Ctrl key when right-clicking 
        /// if browserContextMenuOnCtrl is enabled or you're simply not using the context menu plugin.
        ///
        /// Default value: true
        /// </summary>
        [Category("Configurations")]
        public bool DisableNativeSpellChecker
        {
            get
            {
                string c = Config["disableNativeSpellChecker"];
                if (c == null)
                {
                    return true;
                }
                return bool.Parse(c);
            }
            set
            {
                Config["disableNativeSpellChecker"] = (value ? "true" : "false");
            }
        }


        /// <summary>
        /// Disables the "table tools" offered natively by the 
        /// browser (currently Firefox only) to make quick table 
        /// editing operations, like adding or deleting rows and columns.
        ///
        /// Default value: true
        /// </summary>
        [Category("Configurations")]
        public bool DisableNativeTableHandles
        {
            get
            {
                string c = Config["disableNativeTableHandles"];
                if (c == null)
                {
                    return true;
                }
                return bool.Parse(c);
            }
            set
            {
                Config["disableNativeTableHandles"] = (value ? "true" : "false");
            }
        }


        /// <summary>
        /// Disables the ability of resize objects (image and tables) 
        /// in the editing area.
        ///
        /// Default value: false
        /// </summary>
        [Category("Configurations")]
        public bool DisableObjectResizing
        {
            get
            {
                string c = Config["disableObjectResizing"];
                if (c == null)
                {
                    return false;
                }
                return bool.Parse(c);
            }
            set
            {
                Config["disableObjectResizing"] = (value ? "true" : "false");
            }
        }


        /// <summary>
        /// Disables inline styling on read-only elements. 
        ///
        /// Default value: false
        /// </summary>
        [Category("Configurations")]
        public bool DisableReadonlyStyling
        {
            get
            {
                string c = Config["disableReadonlyStyling"];
                if (c == null)
                {
                    return false;
                }
                return bool.Parse(c);
            }
            set
            {
                Config["disableReadonlyStyling"] = (value ? "true" : "false");
            }
        }


        /// <summary>
        /// Sets the DOCTYPE to be used when loading the editor content as HTML.
        ///
        /// Default value: '&lt;!DOCTYPE html&gt;'
        /// 
        /// Set to null for default value
        /// </summary>
        /// <returns>null if the default value is set</returns>
        [Category("Configurations")]
        public string DocType
        {
            get
            {
                return Config["docType"];
            }
            set
            {
                Config["docType"] = value;
            }
        }


        /// <summary>
        /// Whether to render or not the editing block area in 
        /// the editor interface. 
        ///
        /// Default value: true
        /// </summary>
        [Category("Configurations")]
        public bool EditingBlock
        {
            get
            {
                string c = Config["editingBlock"];
                if (c == null)
                {
                    return true;
                }
                return bool.Parse(c);
            }
            set
            {
                Config["editingBlock"] = (value ? "true" : "false");
            }
        }


        /// <summary>
        /// The e-mail address anti-spam protection option. The protection will be applied when creating or modifying e-mail links through the editor interface.
        /// 
        /// Two methods of protection can be chosen:
        /// The e-mail parts (name, domain and any other query string) are assembled into a 
        /// function call pattern. Such function must be provided by the developer in 
        /// the pages that will use the contents.
        /// 
        /// Only the e-mail address is obfuscated into a special string that has no 
        /// meaning for humans or spam bots, but which is properly rendered and 
        /// accepted by the browser.
        /// 
        /// Both approaches require JavaScript to be enabled.
        /// 
        /// Default value: empty string = disabled
        /// 
        /// Set to null for default value
        /// </summary>
        /// <returns>null if the default value is set</returns>
        [Category("Configurations")]
        public string EmailProtection
        {
            get
            {
                return Config["emailProtection"];
            }
            set
            {
                Config["emailProtection"] = value;
            }
        }


        /// <summary>
        /// Allow context-sensitive tab key behaviors, including the following scenarios:
        /// 
        /// When selection is anchored inside table cells:
        /// If TAB is pressed, select the contents of the "next" cell. If in the last cell in the table, add a new row to it and focus its first cell.
        /// If SHIFT+TAB is pressed, select the contents of the "previous" cell. Do nothing when it's in the first cell.
        ///
        /// Default value: true
        /// </summary>
        [Category("Configurations")]
        public bool EnableTabKeyTools
        {
            get
            {
                string c = Config["enableTabKeyTools"];
                if (c == null)
                {
                    return true;
                }
                return bool.Parse(c);
            }
            set
            {
                Config["enableTabKeyTools"] = (value ? "true" : "false");
            }
        }


        /// <summary>
        /// Sets the behavior for the ENTER key. 
        /// It also dictates other behavior rules in the editor, 
        /// like whether the br element is to be used as a paragraph 
        /// separator when indenting text. 
        /// The allowed values are the following constants, 
        /// and their relative behavior:
        /// * CKEDITOR.ENTER_P (1): new p paragraphs are created; (EnterMode.P)
        /// * CKEDITOR.ENTER_BR (2): lines are broken with br elements; (EnterMode.BR)
        /// * CKEDITOR.ENTER_DIV (3): new div blocks are created. (EnterMode.DIV)
        ///
        /// Note: It's recommended to use the CKEDITOR.ENTER_P value because 
        /// of its semantic value and correctness. 
        /// The editor is optimized for this value.
        ///
        /// Default value: CKEDITOR.ENTER_P
        /// </summary>
        [Category("Configurations")]
        public EnterMode EnterMode
        {
            get
            {
                string c = Config["enterMode"];
                if (c == null)
                {
                    return EnterMode.P;
                }
                switch (c)
                {
                    case "CKEDITOR.ENTER_BR":
                        return EnterMode.BR;
                    case "CKEDITOR.ENTER_DIV":
                        return EnterMode.DIV;
                    default:
                    case "CKEDITOR.ENTER_P":
                        return EnterMode.P;
                }
            }
            set
            {
                switch (value)
                {
                    case EnterMode.BR:
                        Config["enterMode"] = "CKEDITOR.ENTER_BR";
                        break;
                    case EnterMode.DIV:
                        Config["enterMode"] = "CKEDITOR.ENTER_DIV";
                        break;
                    default:
                    case EnterMode.P:
                        Config["enterMode"] = "CKEDITOR.ENTER_P";
                        break;
                }
            }
        }


        /// <summary>
        /// Whether to use HTML entities in the output.
        ///
        /// Default value: true
        /// </summary>
        [Category("Configurations")]
        public bool Entities
        {
            get
            {
                string c = Config["entities"];
                if (c == null)
                {
                    return true;
                }
                return bool.Parse(c);
            }
            set
            {
                Config["entities"] = (value ? "true" : "false");
            }
        }


        /// <summary>
        /// A comma separated list of additional entities to be used. 
        /// Entity names or numbers must be used in a form that 
        /// excludes the '&amp;' prefix and the ';' ending.
        ///
        /// Default value: '#39' (The single quote (') character)
        /// 
        /// Set to null for default value
        /// </summary>
        /// <returns>null if the default value is set</returns>
        [Category("Configurations")]
        public string EntitiesAdditional
        {
            get
            {
                return Config["entities_additional"];
            }
            set
            {
                Config["entities_additional"] = value;
            }
        }


        /// <summary>
        /// Whether to convert some symbols, mathematical symbols, 
        /// and Greek letters to HTML entities. 
        /// This may be more relevant for users typing text written in Greek. 
        /// The list of entities can be found at the 
        ///<a href="http://www.w3.org/TR/html4/sgml/entities.html#h-24.3.1">W3C HTML 4.01 Specification, section 24.3.1.</a>
        ///
        /// Default value: true
        /// </summary>
        [Category("Configurations")]
        public bool EntitiesGreek
        {
            get
            {
                string c = Config["entities_greek"];
                if (c == null)
                {
                    return true;
                }
                return bool.Parse(c);
            }
            set
            {
                Config["entities_greek"] = (value ? "true" : "false");
            }
        }


        /// <summary>
        /// Whether to convert some Latin characters (Latin 
        /// alphabet No. 1, ISO 8859-1) to HTML entities. 
        /// The list of entities can be found at the 
        ///<a href="http://www.w3.org/TR/html4/sgml/entities.html#h-24.2.1">W3C HTML 4.01 Specification, section 24.2.1.</a>
        ///
        /// Default value: true
        /// </summary>
        [Category("Configurations")]
        public bool EntitiesLatin
        {
            get
            {
                string c = Config["entities_latin"];
                if (c == null)
                {
                    return true;
                }
                return bool.Parse(c);
            }
            set
            {
                Config["entities_latin"] = (value ? "true" : "false");
            }
        }


        /// <summary>
        /// Whether to convert all remaining characters, 
        /// not comprised in the ASCII character table, 
        /// to their relative numeric representation of HTML entity. 
        /// For example, the phrase "This is Chinese: 汉语." is outputted 
        /// as "This is Chinese: ." 
        ///
        /// Default value: false
        /// </summary>
        [Category("Configurations")]
        public bool EntitiesProcessNumerical
        {
            get
            {
                string c = Config["entities_processNumerical"];
                if (c == null)
                {
                    return false;
                }
                return bool.Parse(c);
            }
            set
            {
                Config["entities_processNumerical"] = (value ? "true" : "false");
            }
        }


        /// <summary>
        /// A list of additional plugins to be loaded. This setting makes it easier 
        /// to add new plugins without having to touch plugins setting.
        /// </summary>
        /// <remarks>Contains "CMSPlugins" as the first item by default.</remarks>
        public ICollection<string> ExtraPlugins
        {
            get
            {
                return mExtraPlugins;
            }
        }


        /// <summary>
        /// The location of an external file browser that should be launched when the 
        /// Browse Server button is pressed. If configured, the Browse Server button 
        /// will appear in the Link and Image dialog windows.
        /// 
        /// Default value: empty string = disabled
        /// 
        /// Set to null for default value
        /// </summary>
        /// <returns>null if the default value is set</returns>
        [Category("Configurations")]
        public string FileBrowserBrowseUrl
        {
            get
            {
                return Config["filebrowserBrowseUrl"];
            }
            set
            {
                Config["filebrowserBrowseUrl"] = value;
            }
        }


        /// <summary>
        /// The location of an external file browser, that should be launched
        /// when "Browse Server" button is pressed in the Link tab of Image dialog. 
        /// If not set, CKEditor will use FilebrowserBrowseUrl
        ///
        /// Default value: undefined
        /// 
        /// Set to null for default value
        /// </summary>
        /// <returns>null if the default value is set</returns>
        [Category("Configurations")]
        public string FileBrowserImageBrowseLinkUrl
        {
            get
            {
                return Config["filebrowserImageBrowseLinkUrl"];
            }
            set
            {
                Config["filebrowserImageBrowseLinkUrl"] = value;
            }
        }


        /// <summary>
        /// The location of an external file browser, that should be launched when "Browse Server" button is pressed in the Image dialog.
        /// If not set, CKEditor will use FilebrowserBrowseUrl
        ///
        /// Default value: undefined
        /// 
        /// Set to null for default value
        /// </summary>
        /// <returns>null if the default value is set</returns>
        [Category("Configurations")]
        public string FileBrowserImageBrowseUrl
        {
            get
            {
                return Config["filebrowserImageBrowseUrl"];
            }
            set
            {
                Config["filebrowserImageBrowseUrl"] = value;
            }
        }


        /// <summary>
        /// The location of a script that handles file uploads in the Image dialog.
        /// If not set, CKEditor will use FileBrowserUploadUrl
        ///
        /// Default value: undefined
        /// 
        /// Set to null for default value
        /// </summary>
        /// <returns>null if the default value is set</returns>
        [Category("Configurations")]
        public string FileBrowserImageUploadUrl
        {
            get
            {
                return Config["filebrowserImageUploadUrl"];
            }
            set
            {
                Config["filebrowserImageUploadUrl"] = value;
            }
        }


        /// <summary>
        /// The default height of file browser window in CKEditor is set 
        /// to 70% of screen height. 
        /// If for some reasons, the default values are not suitable for you, 
        /// you can change it to any other value.
        ///
        /// To set the size of the window in pixels, 
        /// just set the number value (e.g. "800"). 
        /// If you prefer to set height and width of the window 
        /// in percentage of the screen, remember to add percent 
        /// sign at the end (e.g. "60%").
        ///
        /// Default value: undefined (Unit.Empty)
        ///
        /// Set to Unit.Empty to reset to default value
        /// </summary>
        [Category("Configurations")]
        public Unit FileBrowserImageWindowHeight
        {
            get
            {
                string c = Config["filebrowserImageWindowHeight"];
                if (c == null)
                {
                    return Unit.Empty;
                }
                return Unit.Parse(c);
            }
            set
            {
                Config["filebrowserImageWindowHeight"] = value.ToString();
            }
        }


        /// <summary>
        /// The default width of file browser window in CKEditor is set 
        /// to 80% of screen width.
        /// If for some reasons, the default values are not suitable for you, 
        /// you can change it to any other value.
        ///
        /// To set the size of the window in pixels, 
        /// just set the number value (e.g. "800"). 
        /// If you prefer to set height and width of the window 
        /// in percentage of the screen, remember to add percent 
        /// sign at the end (e.g. "60%").
        ///
        /// Default value: undefined (Unit.Empty)
        ///
        /// Set to Unit.Empty to reset to default value
        /// </summary>
        [Category("Configurations")]
        public Unit FileBrowserImageWindowWidth
        {
            get
            {
                string c = Config["filebrowserImageWindowWidth"];
                if (c == null)
                {
                    return Unit.Empty;
                }
                return Unit.Parse(c);
            }
            set
            {
                Config["filebrowserImageWindowWidth"] = value.ToString();
            }
        }


        /// <summary>
        /// The location of an external file browser, that should be launched when "Browse Server" button is pressed in the Link dialog. 
        /// If not set, CKEditor will use see cref="CKEDITOR.config.filebrowserBrowseUrl"/ 
        ///
        /// Default value: undefined
        ///
        /// Set to null for default value
        /// </summary>
        /// <returns>null if the default value is set</returns>
        [Category("Configurations")]
        public string FileBrowserLinkBrowseUrl
        {
            get
            {
                return Config["filebrowserLinkBrowseUrl"];
            }
            set
            {
                Config["filebrowserLinkBrowseUrl"] = value;
            }
        }


        /// <summary>
        /// The location of a script that handles file uploads in the Link dialog.
        /// If not set, CKEditor will use see cref="CKEDITOR.config.filebrowserUploadUrl"/
        ///
        /// Default value: undefined
        ///
        /// Set to null for default value
        /// </summary>
        /// <returns>null if the default value is set</returns>
        [Category("Configurations")]
        public string FileBrowserLinkUploadUrl
        {
            get
            {
                return Config["filebrowserLinkUploadUrl"];
            }
            set
            {
                Config["filebrowserLinkUploadUrl"] = value;
            }
        }


        /// <summary>
        /// The default height of file browser window in CKEditor is set 
        /// to 70% of screen height. 
        /// If for some reasons, the default values are not suitable for you, 
        /// you can change it to any other value.
        ///
        /// To set the size of the window in pixels, 
        /// just set the number value (e.g. "800"). 
        /// If you prefer to set height and width of the window 
        /// in percentage of the screen, remember to add percent 
        /// sign at the end (e.g. "60%").
        ///
        /// Default value: undefined (Unit.Empty)
        ///
        /// Set to Unit.Empty to reset to default value
        /// </summary>
        [Category("Configurations")]
        public Unit FileBrowserLinkWindowHeight
        {
            get
            {
                string c = Config["filebrowserLinkWindowHeight"];
                if (c == null)
                {
                    return Unit.Empty;
                }
                return Unit.Parse(c);
            }
            set
            {
                Config["filebrowserLinkWindowHeight"] = value.ToString();
            }
        }


        /// <summary>
        /// The default width of file browser window in CKEditor is set 
        /// to 80% of screen width.
        /// If for some reasons, the default values are not suitable for you, 
        /// you can change it to any other value.
        ///
        /// To set the size of the window in pixels, 
        /// just set the number value (e.g. "800"). 
        /// If you prefer to set height and width of the window 
        /// in percentage of the screen, remember to add percent 
        /// sign at the end (e.g. "60%").
        ///
        /// Default value: undefined (Unit.Empty)
        ///
        /// Set to Unit.Empty to reset to default value
        /// </summary>
        [Category("Configurations")]
        public Unit FileBrowserLinkWindowWidth
        {
            get
            {
                string c = Config["filebrowserLinkWindowWidth"];
                if (c == null)
                {
                    return Unit.Empty;
                }
                return Unit.Parse(c);
            }
            set
            {
                Config["filebrowserLinkWindowWidth"] = value.ToString();
            }
        }


        /// <summary>
        /// The "features" to use in the file browser popup window.
        ///
        /// Default value: "location=no,menubar=no,toolbar=no,dependent=yes,minimizable=no,modal=yes,alwaysRaised=yes,resizable=yes,scrollbars=yes"
        /// 
        /// Set to null for default value
        /// </summary>
        /// <returns>null if the default value is set</returns>
        [Category("Configurations")]
        public string FileBrowserWindowFeatures
        {
            get
            {
                return Config["filebrowserWindowFeatures"];
            }
            set
            {
                Config["filebrowserWindowFeatures"] = value;
            }
        }


        /// <summary>
        /// The height of the file browser popup window.
        /// 
        /// Default value: 70%
        /// </summary>
        [Category("Configurations")]
        public Unit FileBrowserWindowHeight
        {
            get
            {
                string c = Config["filebrowserWindowHeight"];
                if (c == null)
                {
                    return Unit.Percentage(70);
                }
                return Unit.Parse(c);
            }
            set
            {
                Config["filebrowserWindowHeight"] = value.ToString();
            }
        }


        /// <summary>
        /// The width of the file browser popup window.
        /// 
        /// Default value: 80%
        /// </summary>
        [Category("Configurations")]
        public Unit FileBrowserWindowWidth
        {
            get
            {
                string c = Config["filebrowserWindowWidth"];
                if (c == null)
                {
                    return Unit.Percentage(80);
                }
                return Unit.Parse(c);
            }
            set
            {
                Config["filebrowserWindowWidth"] = value.ToString();
            }
        }


        /// <summary>
        /// The location of the script that handles file uploads.
        /// If set, the Upload tab will appear in the Link, Image, and Flash dialog windows.
        ///
        /// It is also possible to set a separate url for a selected dialog box, 
        /// using the dialog name in file browser settings: filebrowser[dialogName]UploadUrl.
        ///
        /// Default value: undefined
        /// 
        /// Set to null for default value
        /// </summary>
        /// <returns>null if the default value is set</returns>
        [Category("Configurations")]
        public string FileBrowserUploadUrl
        {
            get
            {
                return Config["filebrowserUploadUrl"];
            }
            set
            {
                Config["filebrowserUploadUrl"] = value;
            }
        }


        /// <summary>
        /// Whether a filler text (non-breaking space entity — &amp;nbsp;) will be inserted into 
        /// empty block elements in HTML output. This is used to render block elements properly 
        /// with line-height. When a function is specified instead, it will be 
        /// passed a CKEDITOR.htmlParser.element to decide whether adding the 
        /// filler text by expecting a Boolean return value.
        ///
        /// Default value: true
        /// </summary>
        [Category("Configurations")]
        public bool FillEmptyBlocks
        {
            get
            {
                string c = Config["fillEmptyBlocks"];
                if (c == null)
                {
                    return true;
                }
                return bool.Parse(c);
            }
            set
            {
                Config["fillEmptyBlocks"] = (value ? "true" : "false");
            }
        }


        /// <summary>
        /// Defines the style to be used to highlight results with the find dialog. 
        /// NOTE: This is a pure javascript value, so be careful not to break things!
        ///
        /// Default value: {element: 'span', styles: {'background-color': '#004', color: '#fff'}}
        /// 
        /// Set to null for default value
        /// </summary>
        /// <returns>null if the default value is set</returns>
        [Category("Configurations")]
        public string FindHighlight
        {
            get
            {
                return Config["find_highlight"];
            }
            set
            {
                Config["find_highlight"] = value;
            }
        }


        /// <summary>
        /// If true, the editor fixes output HTML code to XHTML.
        /// </summary>
        public bool FixXHTML
        {
            get
            {
                object o = ViewState["FixXHTML"] ?? SettingsHelper.AppSettings["CMSWYSIWYGFixXHTML"];
                return ValidationHelper.GetBoolean(o, true);
            }
            set
            {
                ViewState["FixXHTML"] = value;
            }
        }


        /// <summary>
        /// The text to be displayed in the Font Size combo if none of the 
        /// available values matches the current cursor position or text 
        /// selection.
        ///
        /// Default value: empty string
        ///
        /// Set to null for default value
        /// </summary>
        /// <returns>null if the default value is set</returns>
        [Category("Configurations")]
        public string FontSizeDefaultLabel
        {
            get
            {
                return Config["fontSize_defaultLabel"];
            }
            set
            {
                Config["fontSize_defaultLabel"] = value;
            }
        }


        /// <summary>
        /// The list of fonts size to be displayed in the Font Size combo in the toolbar. 
        /// Entries are separated by semi-colons (';'). Any kind of "CSS like" size can 
        /// be used, like '12px', '2.3em', '130%', 'larger' or 'x-small'.
        ///
        /// A display name may be optionally defined by prefixing the entries with the
        /// name and the slash character. For example,'Bigger Font/14px' will be 
        /// displayed as 'Bigger Font' in the list, but will be outputted as '14px'.
        ///
        /// Default value: "8/8px;9/9px;10/10px;11/11px;12/12px;14/14px;16/16px;18/18px;20/20px;22/22px;24/24px;26/26px;28/28px;36/36px;48/48px;72/72px"
        /// 
        /// Set to null for default value
        /// </summary>
        /// <returns>null if the default value is set</returns>
        [Category("Configurations")]
        public string FontSizeSizes
        {
            get
            {
                return Config["fontSize_sizes"];
            }
            set
            {
                Config["fontSize_sizes"] = value;
            }
        }


        /// <summary>
        /// The style definition to be used to apply the font size in the text. 
        ///
        /// Default value:
        /// {
        /// element   : 'span',
        /// styles    : { 'font-size' : '#(size)' },
        /// overrides : [ { element : 'font', attributes : { 'size' : null } } ]
        /// }
        /// 
        /// Set to null for default value
        /// </summary>
        /// <returns>null if the default value is set</returns>
        [Category("Configurations")]
        public string FontSizeStyle
        {
            get
            {
                return Config["fontSize_style"];
            }
            set
            {
                Config["fontSize_style"] = value;
            }
        }


        /// <summary>
        /// The text to be displayed in the Font combo if none of the 
        /// available values matches the current cursor position or text 
        /// selection. 
        ///
        /// Default value empty string
        /// 
        /// Set to null for default value
        /// </summary>
        /// <returns>null if the default value is set</returns>
        [Category("Configurations")]
        public string FontDefaultLabel
        {
            get
            {
                return Config["font_defaultLabel"];
            }
            set
            {
                Config["font_defaultLabel"] = value;
            }
        }


        /// <summary>
        /// The list of fonts names to be displayed in the Font combo in the toolbar. 
        /// Entries are separated by semi-colons (';'), while it's possible to have 
        /// more than one font for each entry, in the HTML way (separated by comma).
        /// 
        /// A display name may be optionally defined by prefixing the entries with the 
        /// name and the slash character. 
        /// For example,'Arial/Arial, Helvetica, sans-serif' will be displayed as 'Arial' in the 
        /// list, but will be outputted as 'Arial, Helvetica, sans-serif'.
        ///
        /// Default value: "Arial/Arial, Helvetica, sans-serif;Comic Sans MS/Comic Sans MS, cursive;Courier New/Courier New, Courier, 
        /// monospace;Georgia/Georgia, serif;Lucida Sans Unicode/Lucida Sans Unicode, Lucida Grande, 
        /// sans-serif;Tahoma/Tahoma, Geneva, sans-serif;Times New Roman/Times New Roman, Times, 
        /// serif;Trebuchet MS/Trebuchet MS, Helvetica, sans-serif;Verdana/Verdana, Geneva, sans-serif"
        /// 
        /// Set to null for default value
        /// </summary>
        /// <returns>null if the default value is set</returns>
        [Category("Configurations")]
        public string FontNames
        {
            get
            {
                return Config["font_names"];
            }
            set
            {
                Config["font_names"] = value;
            }
        }


        /// <summary>
        /// The style definition to be used to apply the font in the text.
        ///
        /// Default value:
        /// {
        /// element                : 'span',
        /// styles                : { 'font-family' : '#(family)' },
        /// overrides        : [ { element : 'font', attributes : { 'face' : null } } ]
        /// }
        /// 
        /// Set to null for default value
        /// </summary>
        /// <returns>null if the default value is set</returns>
        [Category("Configurations")]
        public string FontStyle
        {
            get
            {
                return Config["font_style"];
            }
            set
            {
                Config["font_style"] = value;
            }
        }


        /// <summary>
        /// Whether to force all pasting operations to insert on plain text into the 
        /// editor, loosing any formatting information possibly available in the source text.
        /// Note: paste from word (dialog) is not affected by this configuration.
        ///
        /// Default value: false
        /// </summary>
        [Category("Configurations")]
        public bool ForcePasteAsPlainText
        {
            get
            {
                string c = Config["forcePasteAsPlainText"];
                if (c == null)
                {
                    return false;
                }
                return bool.Parse(c);
            }
            set
            {
                Config["forcePasteAsPlainText"] = (value ? "true" : "false");
            }
        }


        /// <summary>
        /// Whether to force using '&amp;' instead of '&amp;amp;' in elements 
        /// attributes values, it's not recommended to change this 
        /// setting for compliance with the W3C XHTML 1.0 standards (C.12, XHTML 1.0).
        ///
        /// Default value: false
        /// </summary>
        [Category("Configurations")]
        public bool ForceSimpleAmpersand
        {
            get
            {
                string c = Config["forceSimpleAmpersand"];
                if (c == null)
                {
                    return false;
                }
                return bool.Parse(c);
            }
            set
            {
                Config["forceSimpleAmpersand"] = (value ? "true" : "false");
            }
        }


        /// <summary>
        /// The style definition to be used to apply the "Address" format. 
        ///
        /// Default value: { element: 'address' }
        /// 
        /// Set to null for default value
        /// </summary>
        /// <returns>null if the default value is set</returns>
        [Category("Configurations")]
        public string FormatAddress
        {
            get
            {
                return Config["format_address"];
            }
            set
            {
                Config["format_address"] = value;
            }
        }


        /// <summary>
        /// The style definition to be used to apply the "Normal (DIV)" format. 
        ///
        /// Default value: {element:'div'}
        /// 
        /// Set to null for default value
        /// </summary>
        /// <returns>null if the default value is set</returns>
        [Category("Configurations")]
        public string FormatDiv
        {
            get
            {
                return Config["format_div"];
            }
            set
            {
                Config["format_div"] = value;
            }
        }


        /// <summary>
        /// The style definition to be used to apply the "Heading 1" format. 
        ///
        /// Default value: { element: 'h1' }
        /// 
        /// Set to null for default value
        /// </summary>
        /// <returns>null if the default value is set</returns>
        [Category("Configurations")]
        public string FormatH1
        {
            get
            {
                return Config["format_h1"];
            }
            set
            {
                Config["format_h1"] = value;
            }
        }


        /// <summary>
        /// The style definition to be used to apply the "Heading 2" format. 
        ///
        /// Default value: { element: 'h2' }
        /// 
        /// Set to null for default value
        /// </summary>
        /// <returns>null if the default value is set</returns>
        [Category("Configurations")]
        public string FormatH2
        {
            get
            {
                return Config["format_h2"];
            }
            set
            {
                Config["format_h2"] = value;
            }
        }


        /// <summary>
        /// The style definition to be used to apply the "Heading 3" format. 
        ///
        /// Default value: { element: 'h3' }
        /// 
        /// Set to null for default value
        /// </summary>
        /// <returns>null if the default value is set</returns>
        [Category("Configurations")]
        public string FormatH3
        {
            get
            {
                return Config["format_h3"];
            }
            set
            {
                Config["format_h3"] = value;
            }
        }


        /// <summary>
        /// The style definition to be used to apply the "Heading 4" format. 
        ///
        /// Default value: { element: 'h4' }
        /// </summary>
        [Category("Configurations")]
        public string FormatH4
        {
            get
            {
                return Config["format_h4"];
            }
            set
            {
                Config["format_h4"] = value;
            }
        }


        /// <summary>
        /// The style definition to be used to apply the "Heading 5" format. 
        ///
        /// Default value: { element: 'h5' }
        /// 
        /// Set to null for default value
        /// </summary>
        /// <returns>null if the default value is set</returns>
        [Category("Configurations")]
        public string FormatH5
        {
            get
            {
                return Config["format_h5"];
            }
            set
            {
                Config["format_h5"] = value;
            }
        }


        /// <summary>
        /// The style definition to be used to apply the "Heading 6" format. 
        ///
        /// Default value: { element: 'h6' }
        /// 
        /// Set to null for default value
        /// </summary>
        /// <returns>null if the default value is set</returns>
        [Category("Configurations")]
        public string FormatH6
        {
            get
            {
                return Config["format_h6"];
            }
            set
            {
                Config["format_h6"] = value;
            }
        }


        /// <summary>
        /// The style definition to be used to apply the "Normal" format. 
        ///
        /// Default value: { element: 'p' }
        /// 
        /// Set to null for default value
        /// </summary>
        /// <returns>null if the default value is set</returns>
        [Category("Configurations")]
        public string FormatP
        {
            get
            {
                return Config["format_p"];
            }
            set
            {
                Config["format_p"] = value;
            }
        }


        /// <summary>
        /// The style definition to be used to apply the "Formatted" format.  
        ///
        /// Default value: { element: 'pre' }
        /// 
        /// Set to null for default value
        /// </summary>
        /// <returns>null if the default value is set</returns>
        [Category("Configurations")]
        public string FormatPre
        {
            get
            {
                return Config["format_pre"];
            }
            set
            {
                Config["format_pre"] = value;
            }
        }


        /// <summary>
        /// A list of semi colon separated style names (by default tags) 
        /// representing the style definition for each entry to be 
        /// displayed in the Format combo in the toolbar. 
        /// Each entry must have its relative definition configuration 
        /// in a setting named "format_(tagName)". 
        /// For example, the "p" entry has its definition taken from 
        /// config.format_p. 
        ///
        /// Default value: "p;h1;h2;h3;h4;h5;h6;pre;address;div"
        /// 
        /// Set to null for default value
        /// </summary>
        /// <returns>null if the default value is set</returns>
        [Category("Configurations")]
        public string FormatTags
        {
            get
            {
                return Config["format_tags"];
            }
            set
            {
                Config["format_tags"] = value;
            }
        }


        /// <summary>
        /// If true, the editor runs the URL formatting.
        /// </summary>
        public bool FormatURL
        {
            get
            {
                object o = ViewState["FormatURL"] ?? SettingsHelper.AppSettings["CMSWYSIWYGFormatURL"];
                return ValidationHelper.GetBoolean(o, true);
            }
            set
            {
                ViewState["FormatURL"] = value;
            }
        }


        /// <summary>
        /// Indicates whether the contents to be edited are being input as a full 
        /// HTML page. A full page includes the &lt;html&gt;, &lt;head&gt;, and &lt;body&gt; 
        /// elements. The final output will also reflect this setting, including 
        /// the &lt;body&gt; contents only if this setting is disabled.
        ///
        /// Default value: false
        /// </summary>
        [Category("Configurations")]
        public bool FullPage
        {
            get
            {
                string c = Config["fullPage"];
                if (c == null)
                {
                    return false;
                }
                return bool.Parse(c);
            }
            set
            {
                Config["fullPage"] = (value ? "true" : "false");
            }
        }


        /// <summary>
        /// The height of the editing area (that includes the editor content). 
        /// This can be an integer, for pixel sizes, or any CSS-defined length unit.
        ///
        /// Note: Percentage unit is not supported yet. e.g. 30%.
        ///
        /// Default value: "200"
        /// </summary>
        [Category("Configurations")]
        [DefaultValue("200px")]
        public override Unit Height
        {
            get
            {
                return Unit.Parse(Config["height"] ?? "200");
            }
            set
            {
                if (value.IsEmpty)
                {
                    Config.Remove("height");
                }
                else
                {
                    Config["height"] = value.ToString();
                }
            }
        }


        /// <summary>
        /// Whether escape HTML when editor update original input element. 
        ///
        /// Default value: false
        /// </summary>
        [Category("Configurations")]
        [DefaultValue(true)]
        public bool HtmlEncodeOutput
        {
            get
            {
                string c = Config["htmlEncodeOutput"];
                if (c == null)
                {
                    return false;
                }
                return bool.Parse(c);
            }
            set
            {
                Config["htmlEncodeOutput"] = (value ? "true" : "false");
            }
        }


        /// <summary>
        /// Whether the editor must output an empty value (empty string) if
        /// it's contents is made by an empty paragraph only. 
        ///
        /// Default value: true
        /// </summary>
        [Category("Configurations")]
        public bool IgnoreEmptyParagraph
        {
            get
            {
                string c = Config["ignoreEmptyParagraph"];
                if (c == null)
                {
                    return true;
                }
                return bool.Parse(c);
            }
            set
            {
                Config["ignoreEmptyParagraph"] = (value ? "true" : "false");
            }
        }


        /// <summary>
        /// Whether to remove links when emptying the link URL field 
        /// in the image dialog. 
        ///
        /// Default value: true
        /// </summary>
        [Category("Configurations")]
        public bool ImageRemoveLinkByEmptyURL
        {
            get
            {
                string c = Config["image_removeLinkByEmptyURL"];
                if (c == null)
                {
                    return true;
                }
                return bool.Parse(c);
            }
            set
            {
                Config["image_removeLinkByEmptyURL"] = (value ? "true" : "false");
            }
        }


        /// <summary>
        /// A list associating keystrokes to editor commands. 
        /// Each element in the list is an array where the first item is 
        /// the keystroke, and the second is the name of the command to be executed. 
        ///
        /// Default value: 
        /// [
        /// [ CKEDITOR.ALT + 121 /*F10*/, 'toolbarFocus' ],
        /// [ CKEDITOR.ALT + 122 /*F11*/, 'elementsPathFocus' ],
        /// [ CKEDITOR.SHIFT + 121 /*F10*/, 'contextMenu' ],
        /// [ CKEDITOR.CTRL + 90 /*Z*/, 'undo' ],
        /// [ CKEDITOR.CTRL + 89 /*Y*/, 'redo' ],
        /// [ CKEDITOR.CTRL + CKEDITOR.SHIFT + 90 /*Z*/, 'redo' ],
        /// [ CKEDITOR.CTRL + 76 /*L*/, 'link' ],
        /// [ CKEDITOR.CTRL + 66 /*B*/, 'bold' ],
        /// [ CKEDITOR.CTRL + 73 /*I*/, 'italic' ],
        /// [ CKEDITOR.CTRL + 85 /*U*/, 'underline' ],
        /// [ CKEDITOR.ALT + 109 /*-*/, 'toolbarCollapse' ]
        /// ]
        ///
        /// Set to null for default value
        ///
        /// NOTE: This is a pure javascript value, so be careful not to break things!
        /// 
        /// Set to null for default value
        /// </summary>
        /// <returns>null if the default value is set</returns>
        [Category("Configurations")]
        public string Keystrokes
        {
            get
            {
                return Config["keystrokes"];
            }
            set
            {
                Config["keystrokes"] = value;
            }
        }


        /// <summary>
        /// The user interface language localization to use. If left empty, the 
        /// editor will automatically be localized to the user language. If the 
        /// user language is not supported, the language specified in the
        /// defaultLanguage configuration setting is used.
        ///
        /// Default value: empty string
        /// 
        /// Set to null for default value
        /// </summary>
        /// <returns>null if the default value is set</returns>
        [Category("Configurations")]
        public string Language
        {
            get
            {
                return Config["language"];
            }
            set
            {
                Config["language"] = value;
            }
        }


        /// <summary>
        /// A comma separated list of items group names to be 
        /// displayed in the context menu. The order of items will 
        /// reflect the order specified in this list if no priority was 
        /// defined in the groups.
        ///
        /// Default value: 'clipboard,table,anchor,link,image'
        /// 
        /// Set to null for default value
        /// </summary>
        /// <returns>null if the default value is set</returns>
        [Category("Configurations")]
        public string MenuGroups
        {
            get
            {
                return Config["menu_groups"];
            }
            set
            {
                Config["menu_groups"] = value;
            }
        }


        /// <summary>
        /// The amount of time, in milliseconds, the editor waits before 
        /// showing submenu options when moving the mouse over options 
        /// that contains submenus, like the "Cell Properties" entry for tables. 
        ///
        /// Default value: 400
        /// </summary>
        [Category("Configurations")]
        public int MenuSubMenuDelay
        {
            get
            {
                string c = Config["menu_subMenuDelay"];
                if (c == null)
                {
                    return 400;
                }
                return int.Parse(c);
            }
            set
            {
                Config["menu_subMenuDelay"] = value.ToString();
            }
        }


        /// <summary>
        /// The HTML to load in the editor when the "new page" command 
        /// is executed. 
        ///
        /// Default value: empty string
        /// 
        /// Set to null for default value
        /// </summary>
        /// <returns>null if the default value is set</returns>
        [Category("Configurations")]
        public string NewpageHtml
        {
            get
            {
                return Config["newpage_html"];
            }
            set
            {
                Config["newpage_html"] = value;
            }
        }


        /// <summary>
        /// The file that provides the MS Word cleanup function for 
        /// pasting operations. 
        /// Note: This is a global configuration shared by all editor instances 
        /// present in the page. 
        ///
        /// Default value: &lt;plugin path&gt; + 'filter/default.js'
        /// 
        /// Set to null for default value
        /// </summary>
        /// <returns>null if the default value is set</returns>
        [Category("Configurations")]
        public string PasteFromWordCleanupFile
        {
            get
            {
                return Config["pasteFromWordCleanupFile"];
            }
            set
            {
                Config["pasteFromWordCleanupFile"] = value;
            }
        }


        /// <summary>
        /// Whether to transform MS Word outline numbered headings into lists.
        ///
        /// Default value: false
        /// </summary>
        [Category("Configurations")]
        public bool PasteFromWordNumberedHeadingToList
        {
            get
            {
                string c = Config["pasteFromWordNumberedHeadingToList"];
                if (c == null)
                {
                    return false;
                }
                return bool.Parse(c);
            }
            set
            {
                Config["pasteFromWordNumberedHeadingToList"] = (value ? "true" : "false");
            }
        }


        /// <summary>
        /// Whether to prompt the user about the clean up of content being pasted from MS Word. 
        ///
        /// Default value: false
        /// </summary>
        [Category("Configurations")]
        public bool PasteFromWordPromptCleanup
        {
            get
            {
                string c = Config["pasteFromWordPromptCleanup"];
                if (c == null)
                {
                    return false;
                }
                return bool.Parse(c);
            }
            set
            {
                Config["pasteFromWordPromptCleanup"] = (value ? "true" : "false");
            }
        }


        /// <summary>
        /// Whether to ignore all font related formatting styles, including:
        /// * font size;
        /// * font family;
        /// * font foreground/background color.
        ///
        /// Default value: false
        /// </summary>
        [Category("Configurations")]
        public bool PasteFromWordRemoveFontStyles
        {
            get
            {
                string c = Config["pasteFromWordRemoveFontStyles"];
                if (c == null)
                {
                    return false;
                }
                return bool.Parse(c);
            }
            set
            {
                Config["pasteFromWordRemoveFontStyles"] = (value ? "true" : "false");
            }
        }


        /// <summary>
        /// Whether to remove element styles that can't be 
        /// managed with the editor.
        /// Note that this doesn't handle the font specific styles, 
        /// which depends on the pasteFromWordRemoveFontStyles setting instead. 
        ///
        /// Default value: false
        /// </summary>
        [Category("Configurations")]
        public bool PasteFromWordRemoveStyles
        {
            get
            {
                string c = Config["pasteFromWordRemoveStyles"];
                if (c == null)
                {
                    return false;
                }
                return bool.Parse(c);
            }
            set
            {
                Config["pasteFromWordRemoveStyles"] = (value ? "true" : "false");
            }
        }


        /// <summary>
        /// Comma separated list of plugins to be used for an editor 
        /// instance, besides, the actual plugins that to be loaded could 
        /// be still affected by two other settings: extraPlugins and removePlugins.
        ///
        /// Default value: "&lt;default list of plugins&gt;"
        /// 
        /// Set to null for default value
        /// </summary>
        /// <returns>null if the default value is set</returns>
        [Category("Configurations")]
        public string Plugins
        {
            get
            {
                return (string)ViewState["Plugins"];
            }
            set
            {
                Config["plugins"] = value;
                ViewState["Plugins"] = value;
            }
        }


        /// <summary>
        /// List of regular expressions to be executed over the input HTML, 
        /// indicating code that must stay untouched.
        ///
        /// Default value: [] (empty array)
        ///
        /// Set to null for default value
        ///
        /// NOTE: This is a pure javascript value, so be careful not to break things!
        /// 
        /// Set to null for default value
        /// </summary>
        /// <returns>null if the default value is set</returns>
        [Category("Configurations")]
        public string ProtectedSource
        {
            get
            {
                return Config["protectedSource"];
            }
            set
            {
                Config["protectedSource"] = value;
            }
        }


        /// <summary>
        /// If false, makes the editor start in read-only state.
        /// </summary>
        public override bool Enabled
        {
            get
            {
                return Config["readOnly"] == null;
            }
            set
            {
                // CKEditor has weird behavior on this property. If is 
                // set (no matter what value) than is treated as truth.
                // Because of this weird behavior we need to remove this
                // property from config.
                if (value)
                {
                    Config.Remove("readOnly");
                }
                else
                {
                    Config["readOnly"] = "true";
                }
            }
        }


        /// <summary>
        /// List of toolbar button names that must not be rendered. 
        /// This will work as well for non-button toolbar items, like the Font combos.
        /// <remarks>This setting is case sensitive</remarks>
        /// </summary>
        [Category("Configurations")]
        public ICollection<string> RemoveButtons
        {
            get
            {
                return mRemoveButtons;
            }
        }


        /// <summary>
        /// The dialog contents to removed. It's a string composed by 
        /// dialog name and tab name with a colon between them.
        /// 
        /// Separate each pair with semicolon.
        /// Note: All names are case-sensitive.
        /// Note: Be cautious when specifying dialog tabs that are 
        /// mandatory, like 'info', dialog functionality might be broken because of this!
        ///
        /// Default value: empty string
        /// 
        /// Set to null for default value
        /// </summary>
        /// <returns>null if the default value is set</returns>
        [Category("Configurations")]
        public string RemoveDialogTabs
        {
            get
            {
                return Config["removeDialogTabs"];
            }
            set
            {
                Config["removeDialogTabs"] = value;
            }
        }


        /// <summary>
        /// A comma separated list of elements attributes to be 
        /// removed when executing the "remove format" command. 
        ///
        /// Default value: "class,style,lang,width,height,align,hspace,valign"
        /// 
        /// Set to null for default value
        /// </summary>
        /// <returns>null if the default value is set</returns>
        [Category("Configurations")]
        public string RemoveFormatAttributes
        {
            get
            {
                return Config["removeFormatAttributes"];
            }
            set
            {
                Config["removeFormatAttributes"] = value;
            }
        }


        /// <summary>
        /// A comma separated list of elements to be removed when 
        /// executing the remove format command. Note that only 
        /// inline elements are allowed.
        ///
        /// Default value: "b,big,code,del,dfn,em,font,i,ins,kbd,q,s,samp,small,span,strike,strong,sub,sup,tt,u,var"
        /// 
        /// Set to null for default value
        /// </summary>
        /// <returns>null if the default value is set</returns>
        [Category("Configurations")]
        public string RemoveFormatTags
        {
            get
            {
                return Config["removeFormatTags"];
            }
            set
            {
                Config["removeFormatTags"] = value;
            }
        }


        /// <summary>
        /// List of plugins that must not be loaded.
        /// This is a tool setting which makes it easier to avoid loading plugins defined in the CKEDITOR.config.plugins setting, 
        /// without having to touch it and potentially breaking it.
        /// </summary>
        [Category("Configurations")]
        public ICollection<string> RemovePlugins
        {
            get
            {
                return mRemovePlugins;
            }
        }


        /// <summary>
        /// The dimensions for which the editor resizing is enabled. 
        /// Possible values are both, vertical, and horizontal.
        ///
        /// Default value: vertical
        /// </summary>
        [Category("Configurations")]
        public ResizeDirection ResizeDir
        {
            get
            {
                switch (Config["resize_dir"])
                {
                    case "both":
                        return ResizeDirection.Both;
                    case "horizontal":
                        return ResizeDirection.Horizontal;
                    default:
                    case "vertical":
                        return ResizeDirection.Vertical;
                }
            }
            set
            {
                switch (value)
                {
                    case ResizeDirection.Both:
                        Config["resize_dir"] = "both";
                        break;
                    case ResizeDirection.Horizontal:
                        Config["resize_dir"] = "horizontal";
                        break;
                    default:
                    case ResizeDirection.Vertical:
                        Config["resize_dir"] = "vertical";
                        break;
                }
            }
        }


        /// <summary>
        /// Whether to enable the resizing feature. 
        /// If this feature is disabled, the resize handle will not be visible.
        ///
        /// Default value: true
        /// </summary>
        [Category("Configurations")]
        public bool ResizeEnabled
        {
            get
            {
                string c = Config["resize_enabled"];
                if (c == null)
                {
                    return true;
                }
                return bool.Parse(c);
            }
            set
            {
                Config["resize_enabled"] = (value ? "true" : "false");
            }
        }


        /// <summary>
        /// The maximum editor height, in pixels, when resizing it with 
        /// the resize handle. 
        ///
        /// Default value: 3000
        /// </summary>
        [Category("Configurations")]
        public int ResizeMaxHeight
        {
            get
            {
                string c = Config["resize_maxHeight"];
                if (c == null)
                {
                    return 3000;
                }
                return int.Parse(c);
            }
            set
            {
                Config["resize_maxHeight"] = value.ToString();
            }
        }


        /// <summary>
        /// The maximum editor width, in pixels, when resizing it 
        /// with the resize handle. 
        ///
        /// Default value: 3000
        /// </summary>
        [Category("Configurations")]
        public int ResizeMaxWidth
        {
            get
            {
                string c = Config["resize_maxWidth"];
                if (c == null)
                {
                    return 3000;
                }
                return int.Parse(c);
            }
            set
            {
                Config["resize_maxWidth"] = value.ToString();
            }
        }


        /// <summary>
        /// The minimum editor height, in pixels, when resizing the 
        /// editor interface by using the resize handle. Note: It falls back 
        /// to editor's actual height if it is smaller than the default value.
        ///
        /// Default value: 250
        /// </summary>
        [Category("Configurations")]
        public int ResizeMinHeight
        {
            get
            {
                string c = Config["resize_minHeight"];
                if (c == null)
                {
                    return 250;
                }
                return int.Parse(c);
            }
            set
            {
                Config["resize_minHeight"] = value.ToString();
            }
        }


        /// <summary>
        /// The minimum editor width, in pixels, when resizing the editor 
        /// interface by using the resize handle. Note: It falls back 
        /// to editor's actual width if it is smaller than the default value.
        ///
        /// Default value: 750
        /// </summary>
        [Category("Configurations")]
        public int ResizeMinWidth
        {
            get
            {
                string c = Config["resize_minWidth"];
                if (c == null)
                {
                    return 750;
                }
                return int.Parse(c);
            }
            set
            {
                Config["resize_minWidth"] = value.ToString();
            }
        }


        /// <summary>
        /// If enabled (set to true), turns on SCAYT automatically after loading the editor.
        ///
        /// Default value: false
        /// </summary>
        [Category("Configurations")]
        public bool ScaytAutoStartup
        {
            get
            {
                string c = Config["scayt_autoStartup"];
                if (c == null)
                {
                    return false;
                }
                return bool.Parse(c);
            }
            set
            {
                Config["scayt_autoStartup"] = (value ? "true" : "false");
            }
        }


        /// <summary>
        /// Customizes the display of SCAYT context menu commands 
        /// ("Add Word", "Ignore" and "Ignore All"). 
        /// It must be a string with one or more of the following words 
        /// separated by a pipe ("|"):
        ///
        /// "off": disables all options.
        /// "all": enables all options.
        /// "ignore": enables the "Ignore" option.
        /// "ignoreall": enables the "Ignore All" option.
        /// "add": enables the "Add Word" option.
        ///
        /// Default value: "all"
        /// 
        /// Set to null for default value
        /// </summary>
        /// <returns>null if the default value is set</returns>
        [Category("Configurations")]
        public string ScaytContextCommands
        {
            get
            {
                return Config["scayt_contextCommands"];
            }
            set
            {
                Config["scayt_contextCommands"] = value;
            }
        }


        /// <summary>
        /// Defines the order SCAYT context menu items by groups. 
        /// This must be a string with one or more of the following 
        /// words separated by a pipe character ('|'):
        /// 
        /// suggest – main suggestion word list,
        /// moresuggest – more suggestions word list,
        /// control – SCAYT commands, such as "Ignore" and "Add Word".
        ///
        /// Default value: "suggest|moresuggest|control"
        /// 
        /// Set to null for default value
        /// </summary>
        /// <returns>null if the default value is set</returns>
        [Category("Configurations")]
        public string ScaytContextMenuItemsOrder
        {
            get
            {
                return Config["scayt_contextMenuItemsOrder"];
            }
            set
            {
                Config["scayt_contextMenuItemsOrder"] = value;
            }
        }


        /// <summary>
        /// Links SCAYT to custom dictionaries. 
        /// It's a string containing dictionary ids separated by commas (","). 
        /// Available only for licensed version. 
        /// Further details at http://wiki.spellchecker.net/doku.php?id=custom_dictionary_support . 
        ///
        /// Example: Scayt_CustomDictionaryIds = "3021,3456,3478";
        ///
        /// Default value: empty string
        /// 
        /// Set to null for default value
        /// </summary>
        /// <returns>null if the default value is set</returns>
        [Category("Configurations")]
        public string ScaytCustomDictionaryIds
        {
            get
            {
                return Config["scayt_customDictionaryIds"];
            }
            set
            {
                Config["scayt_customDictionaryIds"] = value;
            }
        }


        /// <summary>
        /// Sets the customer ID for SCAYT. 
        /// Required for migration from free version with banner to paid version. 
        /// Further details at http://wiki.spellchecker.net/doku.php?id=custom_dictionary_support . 
        ///
        /// Default value: empty string
        /// 
        /// Set to null for default value
        /// </summary>
        /// <returns>null if the default value is set</returns>
        [Category("Configurations")]
        public string ScaytCustomerid
        {
            get
            {
                return Config["scayt_customerid"];
            }
            set
            {
                Config["scayt_customerid"] = value;
            }
        }


        /// <summary>
        /// Defines the number of SCAYT suggestions to show in the main context menu. 
        /// Possible values are:
        /// 
        /// 0 (zero) – All suggestions are displayed in the main context menu.
        /// Positive number – The maximum number of suggestions to show in the 
        /// context menu. Other entries will be shown in the "More Suggestions" sub-menu.
        /// Negative number – No suggestions are shown in the main context menu. All 
        /// entries will be listed in the the "Suggestions" sub-menu.
        ///
        /// Default value: 5
        /// </summary>
        [Category("Configurations")]
        public int ScaytMaxSuggestions
        {
            get
            {
                string c = Config["scayt_maxSuggestions"];
                if (c == null)
                {
                    return 5;
                }
                return int.Parse(c);
            }
            set
            {
                Config["scayt_maxSuggestions"] = value.ToString();
            }
        }


        /// <summary>
        /// Enables/disables the "More Suggestions" sub-menu in the context 
        /// menu. Possible values are 'on' and 'off'.
        ///
        /// Default value: "on"
        /// </summary>
        [Category("Configurations")]
        public ScaytMoreSuggestions ScaytMoreSuggestions
        {
            get
            {
                switch (Config["scayt_moreSuggestions"])
                {
                    case "off":
                        return ScaytMoreSuggestions.Off;
                    default:
                    case "on":
                        return ScaytMoreSuggestions.On;
                }
            }
            set
            {
                switch (value)
                {
                    case ScaytMoreSuggestions.Off:
                        Config["scayt_moreSuggestions"] = "vertical";
                        break;
                    default:
                    case ScaytMoreSuggestions.On:
                        Config["scayt_moreSuggestions"] = "on";
                        break;
                }
            }
        }


        /// <summary>
        /// Sets the default spell checking language for SCAYT. 
        /// Possible values are: 'en_US', 'en_GB', 'pt_BR', 'da_DK', 'nl_NL', 
        /// 'en_CA', 'fi_FI', 'fr_FR', 'fr_CA', 'de_DE', 'el_GR', 
        /// 'it_IT', 'nb_NO', 'pt_PT', 'es_ES', 'sv_SE'.
        ///
        /// Default value: "en_US"
        /// 
        /// Set to null for default value
        /// </summary>
        /// <returns>null if the default value is set</returns>
        [Category("Configurations")]
        public string ScaytSLang
        {
            get
            {
                return Config["scayt_sLang"];
            }
            set
            {
                Config["scayt_sLang"] = value;
            }
        }


        /// <summary>
        /// Sets the URL to SCAYT core. Required to switch to 
        /// the licensed version of SCAYT application.
        /// 
        /// Further details available at http://wiki.webspellchecker.net/doku.php?id=migration:hosredfreetolicensedck
        ///
        /// Default value: empty string
        /// 
        /// Set to null for default value
        /// </summary>
        /// <returns>null if the default value is set</returns>
        [Category("Configurations")]
        public string ScaytSrcUrl
        {
            get
            {
                return Config["scayt_srcUrl"];
            }
            set
            {
                Config["scayt_srcUrl"] = value;
            }
        }


        /// <summary>
        /// Sets the visibility of particular tabs in the SCAYT dialog window 
        /// and toolbar button. This setting must contain a 1 (enabled) 
        /// or 0 (disabled) value for each of the following entries, in 
        /// this precise order, separated by a 
        /// comma (','): 'Options', 'Languages', and 'Dictionary'.
        ///
        /// Default value: "1,1,1"
        /// 
        /// Set to null for default value
        /// </summary>
        /// <returns>null if the default value is set</returns>
        [Category("Configurations")]
        public string ScaytUiTabs
        {
            get
            {
                return Config["scayt_uiTabs"];
            }
            set
            {
                Config["scayt_uiTabs"] = value;
            }
        }


        /// <summary>
        /// Makes it possible to activate a custom dictionary in SCAYT. 
        /// The user dictionary name must be used. 
        /// Available only for the licensed version.
        ///
        /// Default value: empty string
        /// 
        /// Set to null for default value
        /// </summary>
        /// <returns>null if the default value is set</returns>
        [Category("Configurations")]
        public string ScaytUserDictionaryName
        {
            get
            {
                return Config["scayt_userDictionaryName"];
            }
            set
            {
                Config["scayt_userDictionaryName"] = value;
            }
        }


        /// <summary>
        /// Shared spaces top.
        ///
        /// Default value: empty string
        /// </summary>
        [Category("Configurations")]
        [TypeConverter(typeof(WebControl)), Themeable(false), DefaultValue(""), IDReferenceProperty]
        public string SharedSpacesTop
        {
            get
            {
                object o = ViewState["SharedSpacesTop"];
                if (o != null)
                {
                    return (string)o;
                }
                return string.Empty;
            }
            set
            {
                ViewState["SharedSpacesTop"] = value;
            }
        }


        /// <summary>
        /// Shared spaces top ClientID.
        ///
        /// Default value: empty string
        /// </summary>
        [Category("Configurations")]
        [Themeable(false), DefaultValue("")]
        public string SharedSpacesTopClientID
        {
            get
            {
                object o = ViewState["SharedSpacesTopClientID"];
                if (o != null)
                {
                    return (string)o;
                }
                return string.Empty;
            }
            set
            {
                ViewState["SharedSpacesTopClientID"] = value;
            }
        }


        /// <summary>
        /// Shared spaces bottom.
        ///
        /// Default value: empty string
        /// </summary>
        [Category("Configurations")]
        [TypeConverter(typeof(WebControl)), Themeable(false), DefaultValue(""), IDReferenceProperty]
        public string SharedSpacesBottom
        {
            get
            {
                object o = ViewState["SharedSpacesBottom"];
                if (o != null)
                {
                    return (string)o;
                }
                return string.Empty;
            }
            set
            {
                ViewState["SharedSpacesBottom"] = value;
            }
        }


        /// <summary>
        /// Shared spaces bottom ClientID.
        ///
        /// Default value: empty string
        /// </summary>
        [Category("Configurations")]
        [Themeable(false), DefaultValue("")]
        public string SharedSpacesBottomClientID
        {
            get
            {
                object o = ViewState["SharedSpacesBottomClientID"];
                if (o != null)
                {
                    return (string)o;
                }
                return string.Empty;
            }
            set
            {
                ViewState["SharedSpacesBottomClientID"] = value;
            }
        }


        /// <summary>
        /// Similarly to the enterMode setting, it defines the behavior 
        /// of the Shift+Enter key combination.
        ///
        /// Default value: EnterMode.BR
        /// </summary>
        [Category("Configurations")]
        public EnterMode ShiftEnterMode
        {
            get
            {
                string c = Config["shiftEnterMode"];
                if (c == null)
                {
                    return EnterMode.P;
                }
                switch (c)
                {
                    case "CKEDITOR.ENTER_P":
                        return EnterMode.P;
                    case "CKEDITOR.ENTER_DIV":
                        return EnterMode.DIV;
                    default:
                    case "CKEDITOR.ENTER_BR":
                        return EnterMode.BR;
                }
            }
            set
            {
                switch (value)
                {
                    case EnterMode.P:
                        Config["shiftEnterMode"] = "CKEDITOR.ENTER_P";
                        break;
                    case EnterMode.DIV:
                        Config["shiftEnterMode"] = "CKEDITOR.ENTER_DIV";
                        break;
                    default:
                    case EnterMode.BR:
                        Config["shiftEnterMode"] = "CKEDITOR.ENTER_BR";
                        break;
                }
            }
        }


        /// <summary>
        /// The editor skin name. Note that it is not possible to have 
        /// editors with different skin settings in the same page. In 
        /// such case just one of the skins will be used for all editors.
        /// 
        /// This is a shortcut to CKEDITOR.skinName.
        /// 
        /// It is possible to install skins outside the default skin folder in 
        /// the editor installation. In that case, the absolute URL path to 
        /// that folder should be provided, separated by a comma ('skin_name,skin_path').
        ///
        /// Default value: "moono"
        /// 
        /// Set to null for default value
        /// </summary>
        /// <returns>null if the default value is set</returns>
        [Category("Configurations")]
        public string Skin
        {
            get
            {
                return Config["skin"];
            }
            set
            {
                Config["skin"] = value;
            }
        }


        /// <summary>
        /// The number of columns to be generated by the smiles matrix. 
        ///
        /// Default value: 8
        /// </summary>
        [Category("Configurations")]
        public int SmileyColumns
        {
            get
            {
                string c = Config["smiley_columns"];
                if (c == null)
                {
                    return 8;
                }
                return int.Parse(c);
            }
            set
            {
                Config["smiley_columns"] = value.ToString();
            }
        }


        /// <summary>
        /// The description to be used for each of the smiles 
        /// defined in the CKEDITOR.config.smiley_images setting. 
        /// Each entry in this array list must match its relative pair 
        /// in the CKEDITOR.config.smiley_images setting.  
        ///
        /// Default value: 
        /// {'smiley', 'sad', 'wink', 'laugh', 'frown',  'cheeky', 'blush', 'surprise', 'indecision', 'angry', 'angel',
        /// 'cool',  'devil', 'crying', 'enlightened', 'no', 'yes', 'heart', 'broken heart',  'kiss', 'mail' }
        /// </summary>
        [Category("Configurations")]
        public string[] SmileyDescriptions
        {
            get
            {
                return (string[])ViewState["Smiley_Descriptions"];
            }
            set
            {
                if (value != null)
                {
                    StringBuilder sb = new StringBuilder();
                    bool comma = false;
                    sb.Append('[');
                    foreach (string smile in value)
                    {
                        if (comma)
                        {
                            sb.Append(",");
                        }
                        else
                        {
                            comma = true;
                        }
                        sb.Append(ScriptHelper.GetString(smile));
                    }
                    sb.Append(']');
                    Config["smiley_descriptions"] = sb.ToString();
                    ViewState["Smiley_Descriptions"] = value;
                }
                else
                {
                    Config.Remove("smiley_descriptions");
                    ViewState.Remove("Smiley_Descriptions");
                }
            }
        }


        /// <summary>
        /// The file names for the smileys to be displayed. 
        /// These files must be contained inside the URL path defined 
        /// with the CKEDITOR.config.smiley_path setting. 
        ///
        /// Default value: 
        /// {'regular_smile.png', 'sad_smile.png',  'wink_smile.png', 'teeth_smile.png', 'confused_smile.png',
        /// 'tongue_smile.png', 'embarrassed_smile.png', 'omg_smile.png',  'whatchutalkingabout_smile.png',
        /// 'angry_smile.png', 'angel_smile.png',  'shades_smile.png', 'devil_smile.png', 'cry_smile.png',
        /// 'lightbulb.png',  'thumbs_down.png', 'thumbs_up.png', 'heart.png', 'broken_heart.png',  'kiss.png', 'envelope.png'}
        /// </summary>
        [Category("Configurations")]
        public string[] SmileyImages
        {
            get
            {
                return (string[])ViewState["Smiley_Images"];
            }
            set
            {
                if (value != null)
                {
                    StringBuilder sb = new StringBuilder();
                    bool comma = false;
                    sb.Append('[');
                    foreach (string smile in value)
                    {
                        if (comma)
                        {
                            sb.Append(",");
                        }
                        else
                        {
                            comma = true;
                        }
                        sb.Append(ScriptHelper.GetString(smile));
                    }
                    sb.Append(']');
                    Config["smiley_images"] = sb.ToString();
                    ViewState["Smiley_Images"] = value;
                }
                else
                {
                    Config.Remove("smiley_images");
                    ViewState.Remove("Smiley_Images");
                }
            }
        }


        /// <summary>
        /// The base path used to build the URL for the smiley images. 
        /// It must end with a slash. 
        ///
        /// Default value: CKEDITOR.basePath + 'plugins/smiley/images/'
        /// 
        /// Set to null for default value
        /// </summary>
        /// <returns>null if the default value is set</returns>
        [Category("Configurations")]
        public string SmileyPath
        {
            get
            {
                return Config["smiley_path"];
            }
            set
            {
                Config["smiley_path"] = value;
            }
        }


        /// <summary>
        /// Controls CSS tab-size property of the sourcearea view.
        /// Works only with dataIndentationChars set to '\t'.
        /// Please consider that not all browsers support CSS tab-size property yet.
        ///
        /// Default value: 4
        /// </summary>
        [Category("Configurations")]
        public int SourceAreaTabSize
        {
            get
            {
                string s = Config["sourceAreaTabSize"];
                if (s == null)
                {
                    return 4;
                }
                return int.Parse(s);
            }
            set
            {
                Config["sourceAreaTabSize"] = value.ToString();
            }
        }


        /// <summary>
        /// Sets whether an editable element should have focus when the editor is loading for the first time.
        ///
        /// Default value: false
        /// </summary>
        [Category("Configurations")]
        public bool StartupFocus
        {
            get
            {
                string c = Config["startupFocus"];
                if (c == null)
                {
                    return false;
                }
                return bool.Parse(c);
            }
            set
            {
                Config["startupFocus"] = (value ? "true" : "false");
            }
        }


        /// <summary>
        /// The mode to load at the editor startup. 
        /// It depends on the plugins loaded. 
        /// By default, the "wysiwyg" and "source" modes are available. 
        ///
        /// Default value: "wysiwyg"
        /// 
        /// Set to null for default value
        /// </summary>
        /// <returns>null if the default value is set</returns>
        [Category("Configurations")]
        public string StartupMode
        {
            get
            {
                return Config["startupMode"];
            }
            set
            {
                Config["startupMode"] = value;
            }
        }


        /// <summary>
        /// Whether to automatically enable the "show block" command 
        /// when the editor loads. 
        ///
        /// Default value: false
        /// </summary>
        [Category("Configurations")]
        public bool StartupOutlineBlocks
        {
            get
            {
                string c = Config["startupOutlineBlocks"];
                if (c == null)
                {
                    return false;
                }
                return bool.Parse(c);
            }
            set
            {
                Config["startupOutlineBlocks"] = (value ? "true" : "false");
            }
        }


        /// <summary>
        /// The "styles definition set" to use in the editor.
        /// They will be used in the styles combo and the style selector of the div container.
        /// The styles may be defined in the page containing the editor, or can be loaded on demand from an external file.
        /// In the second case, if this setting contains only a name, the styles.js file will be loaded
        ///  from the CKEditor root folder (what ensures backward compatibility with CKEditor 4.0).
        /// Otherwise, this setting has the name:url syntax, making it possible to set the URL from which loading the styles file.
        /// Note that the name has to be equal to the name used in CKEDITOR.stylesSet.add while registering styles set.
        ///
        /// Default value: 'default'
        ///
        /// Examples:
        /// // Load from the styles' styles folder (mystyles.js file).
        /// StylesSet = "mystyles";
        ///
        /// // Load from a relative URL.
        /// StylesSet = "mystyles:/editorstyles/styles.js";
        ///
        /// // Load from a full URL.
        /// StylesSet = "mystyles:http://www.example.com/editorstyles/styles.js";
        ///
        /// // Load from a list of definitions.
        /// StylesSet = [
        /// { name : 'Strong Emphasis', element : 'strong' },
        /// { name : 'Emphasis', element : 'em' }, ... ];
        ///
        /// Set to null for default value
        /// </summary>
        /// <returns>null if the default value is set</returns>
        [Category("Configurations")]
        public string StylesSet
        {
            get
            {
                return Config["stylesSet"];
            }
            set
            {
                Config["stylesSet"] = value;
            }
        }


        /// <summary>
        /// The editor tabindex value.
        ///
        /// Default value: 0 (zero)
        /// </summary>
        [Category("Configurations")]
        public override short TabIndex
        {
            get
            {
                string c = Config["tabIndex"];
                if (c == null)
                {
                    return 0;
                }
                return short.Parse(c);
            }
            set
            {
                Config["tabIndex"] = value.ToString();
            }
        }


        /// <summary>
        /// Instructs the editor to add a number of spaces (&amp;nbsp;) to 
        /// the text when hitting the TAB key. 
        /// If set to zero, the TAB key will be used to move the cursor 
        /// focus to the next element in the page, out of the editor focus. 
        ///
        /// Default value: 0
        /// </summary>
        [Category("Configurations")]
        public int TabSpaces
        {
            get
            {
                string c = Config["tabSpaces"];
                if (c == null)
                {
                    return 0;
                }
                return int.Parse(c);
            }
            set
            {
                Config["tabSpaces"] = value.ToString();
            }
        }


        /// <summary>
        /// The templates definition set to use. 
        /// It accepts a list of names separated by comma. 
        /// It must match definitions loaded with the templates_files setting. 
        ///
        /// Default value: "default"
        /// 
        /// Set to null for default value
        /// </summary>
        /// <returns>null if the default value is set</returns>
        [Category("Configurations")]
        public string Templates
        {
            get
            {
                return Config["templates"];
            }
            set
            {
                Config["templates"] = value;
            }
        }


        /// <summary>
        /// The list of templates definition files to load. 
        ///
        /// Default value: Array of "plugins/templates/templates/default.js"
        /// </summary>
        [Category("Configurations")]
        public string[] TemplatesFiles
        {
            get
            {
                return (string[])ViewState["templates_files"];
            }
            set
            {
                if (value != null)
                {
                    StringBuilder sb = new StringBuilder();
                    bool comma = false;
                    sb.Append('[');
                    foreach (string template in value)
                    {
                        if (comma)
                        {
                            sb.Append(",");
                        }
                        else
                        {
                            comma = true;
                        }
                        sb.Append(ScriptHelper.GetString(template));
                    }
                    sb.Append(']');
                    Config["templates_files"] = sb.ToString();
                    ViewState["templates_files"] = value;
                }
                else
                {
                    Config.Remove("templates_files");
                    ViewState.Remove("templates_files");
                }
            }
        }


        /// <summary>
        /// Whether the "Replace actual contents" checkbox 
        /// is checked by default in the Templates dialog. 
        ///
        /// Default value: true
        /// </summary>
        [Category("Configurations")]
        public bool TemplatesReplaceContent
        {
            get
            {
                string c = Config["templates_replaceContent"];
                if (c == null)
                {
                    return true;
                }
                return bool.Parse(c);
            }
            set
            {
                Config["templates_replaceContent"] = (value ? "true" : "false");
            }
        }


        /// <summary>
        /// The theme to be used to build the UI.
        ///
        /// Default value: "default"
        /// 
        /// Set to null for default value
        /// </summary>
        /// <returns>null if the default value is set</returns>
        [Category("Configurations")]
        public string Theme
        {
            get
            {
                return Config["theme"];
            }
            set
            {
                Config["theme"] = value;
            }
        }


        /// <summary>
        /// Customizes the human-readable title of this editor. This title 
        /// is displayed in tooltips and impacts various accessibility 
        /// aspects, e.g. it is commonly used by screen readers for 
        /// distinguishing editor instances and for navigation. 
        /// Accepted values are a string or false.
        /// 
        /// Note: When config.title is set globally, the same value 
        /// will be applied to all editor instances loaded with this 
        /// config. This may severely affect accessibility as screen 
        /// reader users will be unable to distinguish particular 
        /// editor instances and navigate between them.
        /// 
        /// Note: Setting config.title = false may also impair 
        /// accessibility in a similar way.
        /// 
        /// Note: Please do not confuse this property with 
        /// CKEDITOR.editor.name which identifies the instance 
        /// in the CKEDITOR.instances literal.
        ///
        /// Default value: based on editor.name
        /// 
        /// Set to null for default value
        /// </summary>
        /// <returns>null if the default value is set</returns>
        [Category("Configurations")]
        public string Title
        {
            get
            {
                return Config["title"];
            }
            set
            {
                Config["title"] = value;
            }
        }

        /// <summary>
        /// The toolbox (alias toolbar) definition. 
        /// It is a toolbar name or an array of toolbars (strips), 
        /// each one being also an array, containing a list of UI items. 
        ///
        /// If set to null, generate toolbar automatically using all available buttons
        /// and toolbarGroups as a toolbar groups layout.
        ///
        /// Value type: This is pure javascript, so be careful with it!
        /// You can easily break that code if you do not escape characters or 
        /// you misplace an important character.
        /// 
        /// Default value: null
        /// 
        /// Set to null for default value
        /// </summary>
        /// <returns>null if the default value is set</returns>
        [Category("Configurations")]
        public string Toolbar
        {
            get
            {
                return Config["toolbar"];
            }
            set
            {
                Config["toolbar"] = value;
            }
        }


        /// <summary>
        /// The toolbar definition. 
        /// It is an array of toolbars (strips), each one being also an array, 
        /// containing a list of UI items. 
        /// Note that this setting is composed by "toolbar_" added by the 
        /// toolbar name, which in this case is called "Basic". 
        /// This second part of the setting name can be anything. 
        /// You must use this name in the Toolbar setting, so you instruct the 
        /// editor which toolbar_(name) setting to you. 
        ///
        /// Default value: 
        /// [
        /// ['Bold', 'Italic', '-', 'NumberedList', 'BulletedList', '-', 'InsertLink', 'Unlink']
        /// ];
        ///
        /// Set to null for default value
        ///
        /// NOTE: This is a pure javascript value, so be careful not to break things!
        /// 
        /// Set to null for default value
        /// </summary>
        /// <returns>null if the default value is set</returns>
        [Category("Configurations")]
        public string ToolbarBasic
        {
            get
            {
                return Config["toolbar_Basic"];
            }
            set
            {
                Config["toolbar_Basic"] = value;
            }
        }


        /// <summary>
        /// Whether the toolbar can be collapsed by the user. 
        /// If disabled, the Collapse Toolbar button will not be displayed.
        ///
        /// Default value: false
        /// </summary>
        [Category("Configurations")]
        public bool ToolbarCanCollapse
        {
            get
            {
                string c = Config["toolbarCanCollapse"];
                if (c == null)
                {
                    return false;
                }
                return bool.Parse(c);
            }
            set
            {
                Config["toolbarCanCollapse"] = (value ? "true" : "false");
            }
        }


        /// <summary>
        /// This is the default toolbar definition used by the editor. It contains all editor features. 
        ///
        /// Default value: 
        /// [
        /// ['Source', '-'],
        /// ['Cut', 'Copy', 'Paste', 'PasteText', 'PasteFromWord', 'Scayt', '-'],
        /// ['Undo', 'Redo', 'Find', 'Replace', 'RemoveFormat', '-'],
        /// ['Bold', 'Italic', 'Underline', 'Strike', 'Subscript', 'Superscript', '-'],
        /// ['NumberedList', 'BulletedList', 'Outdent', 'Indent', 'Blockquote', 'CreateDiv', '-'],
        /// ['JustifyLeft', 'JustifyCenter', 'JustifyRight', 'JustifyBlock', '-'],
        /// ['InsertLink', 'Unlink', 'Anchor', '-'],
        /// ['InsertImageOrMedia', 'QuicklyInsertImage', 'Table', 'HorizontalRule', 'SpecialChar', '-'],
        /// ['InsertForms', 'InsertPolls', 'InsertRating', 'InsertYouTubeVideo', 'InsertWidget', '-'],
        /// ['Styles', 'Format', 'Font', 'FontSize'],
        /// ['TextColor', 'BGColor', '-'],
        /// ['InsertMacro', '-'],
        /// ['Maximize', 'ShowBlocks']
        /// ];
        ///
        /// Set to null for default value
        ///
        /// NOTE: This is a pure javascript value, so be careful not to break things!
        /// 
        /// Set to null for default value
        /// </summary>
        /// <returns>null if the default value is set</returns>
        [Category("Configurations")]
        public string ToolbarFull
        {
            get
            {
                return Config["toolbar_Full"];
            }
            set
            {
                Config["toolbar_Full"] = value;
            }
        }


        /// <summary>
        /// The "UI space" to which rendering the toolbar.
        /// For the default editor implementation, the recommended options are 'top' and 'bottom'.
        ///
        /// Default value: 'top'
        /// </summary>
        [Category("Configurations")]
        public string ToolbarLocation
        {
            get
            {
                object o = ViewState["ToolbarLocation"];
                if (o != null)
                {
                    return (string)o;
                }
                return string.Empty;
            }
            set
            {
                if (!String.IsNullOrEmpty(value))
                {
                    // Convert 'fcktoolbar' to 'cktoolbar'
                    if (CMSString.Compare(value, "out:fcktoolbar", true) == 0)
                    {
                        value = "Out:CKToolbar";
                    }
                    if (CMSString.Compare(value, "fcktoolbar", true) == 0)
                    {
                        value = "CKToolbar";
                    }
                    // If toolbar location strats with out = shared toolbar
                    if (value.ToLowerCSafe().StartsWithCSafe("out:"))
                    {
                        SharedSpacesTopClientID = value.Substring(4);
                        SharedSpacesBottomClientID = "CKFooter";
                    }
                    else
                    {
                        SharedSpacesBottomClientID = "";
                        SharedSpacesTopClientID = "";
                        sharedToolbar = false;
                    }
                }
                ViewState["ToolbarLocation"] = value;
            }
        }


        /// <summary>
        /// Toolbar set.
        /// </summary>
        [Category("Configurations"), DefaultValue("Default")]
        public string ToolbarSet
        {
            get
            {
                return Toolbar;
            }
            set
            {
                if (!String.IsNullOrEmpty(value))
                {
                    Toolbar = value;
                }
            }
        }


        /// <summary>
        /// Whether the toolbar must start expanded when the editor is loaded. 
        /// Setting this option to false will affect toolbar only when ToolbarCanCollapse is set to true.
        ///
        /// Default value: true
        /// </summary>
        [Category("Configurations")]
        public bool ToolbarStartupExpanded
        {
            get
            {
                string c = Config["toolbarStartupExpanded"];
                if (c == null)
                {
                    return true;
                }
                return bool.Parse(c);
            }
            set
            {
                Config["toolbarStartupExpanded"] = (value ? "true" : "false");
            }
        }


        /// <summary>
        /// The base user interface color to be used by the editor. Not all skins are compatible with this setting.
        /// </summary>
        [Category("Appearance")]
        [TypeConverter(typeof(WebColorConverter))]
        public Color UIColor
        {
            get
            {
                if (Config["uiColor"] == null)
                {
                    return Color.Empty;
                }
                return ColorTranslator.FromHtml(Config["uiColor"]);
            }
            set
            {
                Config["uiColor"] = String.Format("#{0:x2}{1:x2}{2:x2}", value.R, value.G, value.B);
            }
        }


        /// <summary>
        /// The number of undo steps to be saved. The higher this setting value the more memory is used for it. 
        ///
        /// Default value: 20
        /// </summary>
        [Category("Configurations")]
        public int UndoStackSize
        {
            get
            {
                string c = Config["undoStackSize"];
                if (c == null)
                {
                    return 20;
                }
                return int.Parse(c);
            }
            set
            {
                Config["undoStackSize"] = value.ToString();
            }
        }


        /// <summary>
        /// Indicates that some of the editor features, like alignment and text direction,
        /// should used the "computed value" of the feature to indicate it's on/off state,
        /// instead of using the "real value".
        /// If enabled, in a left to right written document,
        /// the "Left Justify" alignment button will show as active,
        /// even if the alignment style is not explicitly applied to the current
        /// paragraph in the editor.
        ///
        /// Default value: true
        /// </summary>
        [Category("Configurations")]
        public bool UseComputedState
        {
            get
            {
                string c = Config["useComputedState"];
                if (c == null)
                {
                    return true;
                }
                return bool.Parse(c);
            }
            set
            {
                Config["useComputedState"] = (value ? "true" : "false");
            }
        }


        /// <summary>
        /// The editor width in CSS size format or pixel integer.
        ///
        /// Default value: empty (Unit.Empty)
        ///
        /// Set to Unit.Empty to reset to default value
        /// </summary>
        [Category("Configurations")]
        [DefaultValue("")]
        public override Unit Width
        {
            get
            {
                string c = Config["width"];
                if (c == null)
                {
                    return Unit.Empty;
                }
                return Unit.Parse(c);
            }
            set
            {
                Config["width"] = value.ToString();
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Initializes a new instance of the CKEditorControl.
        /// </summary>
        public CKEditorControl()
        {
            SetDefaultToolbarFromWebConfig();
        }

        #endregion


        #region "Control events"


        /// <summary>
        /// Controls OnPreRender event.
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (Visible)
            {
                // Register script for pendingCallbacks repair
                ScriptHelper.FixPendingCallbacks(Page);

                ScriptHelper.RegisterJQuery(Page);

                // Ensure modal script
                ScriptHelper.RegisterDialogScript(Page);

                // Check if is in update panel or partial postback
                mIsInUpdatePanel = RequestHelper.IsAsyncPostback() || ControlsHelper.IsInUpdatePanel(this) || (ControlsHelper.GetChildControl(Page, typeof(UpdatePanel)) != null);
                if (mIsInUpdatePanel)
                {
                    // Ensure jQuery doc write plugin if in update panel
                    ScriptHelper.RegisterScriptFile(Page, "jquery/jquery-docwrite.js", false);

                    mBasePath = (BasePath.StartsWithCSafe("~") ? ResolveUrl(BasePath) : BasePath).TrimEnd('/');
                    string basePathScript = ScriptHelper.GetScript(String.Format("window.CKEDITOR_BASEPATH = '{0}/';\n", mBasePath + (UseSource ? "/_source/" : string.Empty)));

                    ScriptHelper.RegisterClientScriptBlock(this, GetType(), "CKEDITOR_BASEPATH", basePathScript);
                }

                InitializeDialogs();

                ConfigPreRender();

                InitializeScripts();
            }
        }


        /// <summary>
        /// Renders the control at design-time.
        /// </summary>
        protected override void Render(HtmlTextWriter writer)
        {
            // Write begin tag
            foreach (string attrName in Attributes.Keys)
            {
                writer.AddAttribute(attrName, Attributes[attrName]);
            }
            writer.RenderBeginTag(HtmlTextWriterTag.Div);


            writer.Write("<textarea name=\"{0}\" id=\"{1}\" rows=\"4\" cols=\"40\" style=\"width: {2}; height: {3};\" >{4}</textarea>",
                UniqueID,
                ClientID,
                Width,
                Height,
                HttpUtility.HtmlEncode(Value));

            ScriptHelper.RegisterCMS(Page);

            writer.RenderEndTag();
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Set default toolbar from web config key if it's set
        /// </summary>
        private void SetDefaultToolbarFromWebConfig()
        {
            string defaultToolbar = CoreServices.AppSettings["CKEditor:DefaultToolbarSet"].ToString("");
            if (!String.IsNullOrEmpty(defaultToolbar))
            {
                Toolbar = defaultToolbar;
            }
        }


        /// <summary>
        /// Initialize dialogs configuration.
        /// </summary>
        private void InitializeDialogs()
        {
            Config["ApplicationPath"] = ResolveUrl("~");
            if (UseSource)
            {
                Config["UseSource"] = "true";
            }
            mBasePath = BasePath.TrimEnd('/') + (UseSource ? "/_source/" : string.Empty);
            Config["CMSPluginUrl"] = UrlResolver.ResolveUrl(BasePath + "Plugins/CMSPlugins/", false, false);
            ContentsLangDirection = LanguageDirection.LeftToRight;

            if (IsLiveSite)
            {
                // Remove macro selector on live site
                RemoveButtons.Add("InsertMacro");

                if (CultureHelper.IsPreferredCultureRTL())
                {
                    ContentsLangDirection = LanguageDirection.RightToLeft;
                }
                Language = CultureHelper.GetShortCultureCode(CultureHelper.GetPreferredCulture());
            }
            else
            {
                if (CultureHelper.IsUICultureRTL())
                {
                    ContentsLangDirection = LanguageDirection.RightToLeft;
                }
                Language = CultureHelper.GetShortCultureCode(CultureHelper.GetPreferredUICultureCode());
            }

            // Init 'Insert image or media' dialog
            MediaDialogConfig.SelectableContent = SelectableContentEnum.OnlyMedia;
            MediaDialogConfig.OutputFormat = OutputFormatEnum.HTMLMedia;

            if (!string.IsNullOrEmpty(DialogCultureCode))
            {
                MediaDialogConfig.Culture = DialogCultureCode;
            }

            Config["MediaDialogURL"] = CMSDialogHelper.GetDialogUrl(MediaDialogConfig, IsLiveSite, false, mBasePath);

            string width = MediaDialogConfig.DialogWidth.ToString();
            string height = MediaDialogConfig.DialogHeight.ToString();
            if (MediaDialogConfig.UseRelativeDimensions)
            {
                width += "%25";
                height += "%25";
            }
            Config["MediaDialogWidth"] = width;
            Config["MediaDialogHeight"] = height;

            // Init 'Insert link' dialog
            LinkDialogConfig.SelectableContent = SelectableContentEnum.AllContent;
            LinkDialogConfig.OutputFormat = OutputFormatEnum.HTMLLink;

            if (!string.IsNullOrEmpty(DialogCultureCode))
            {
                LinkDialogConfig.Culture = DialogCultureCode;
            }

            Config["LinkDialogURL"] = CMSDialogHelper.GetDialogUrl(LinkDialogConfig, IsLiveSite, false, mBasePath);

            width = LinkDialogConfig.DialogWidth.ToString();
            height = LinkDialogConfig.DialogHeight.ToString();
            if (LinkDialogConfig.UseRelativeDimensions)
            {
                width += "%25";
                height += "%25";
            }
            Config["LinkDialogWidth"] = width;
            Config["LinkDialogHeight"] = height;

            // Init 'Quickly insert image' dialog
            QuickInsertConfig.SelectableContent = SelectableContentEnum.OnlyMedia;

            if (!string.IsNullOrEmpty(DialogCultureCode))
            {
                QuickInsertConfig.Culture = DialogCultureCode;
            }

            string quickInsertURL = CMSDialogHelper.GetDialogUrl(QuickInsertConfig, IsLiveSite, false, mBasePath);
            Config["QuickInsertURL"] = quickInsertURL != null ? quickInsertURL.Replace("/InsertImageOrMedia/", "/QuicklyInsertImage/") : String.Empty;

            // Due to backward compatibility
            UrlDialogConfig = MediaDialogConfig.Clone();
            UrlDialogConfig.OutputFormat = OutputFormatEnum.URL;

            if (!string.IsNullOrEmpty(DialogCultureCode))
            {
                UrlDialogConfig.Culture = DialogCultureCode;
            }

            // Insert image 'Browse server' dialog URL
            UrlDialogConfig.SelectableContent = SelectableContentEnum.OnlyImages;
            FileBrowserImageBrowseUrl = CMSDialogHelper.GetDialogUrl(UrlDialogConfig, IsLiveSite, false, null);
            FileBrowserWindowWidth = LinkDialogConfig.UseRelativeDimensions ? Unit.Percentage(LinkDialogConfig.DialogWidth) : Unit.Pixel(LinkDialogConfig.DialogWidth);
            FileBrowserWindowHeight = LinkDialogConfig.UseRelativeDimensions ? Unit.Percentage(LinkDialogConfig.DialogHeight) : Unit.Pixel(LinkDialogConfig.DialogHeight);

            // Insert link 'Browse server' dialog URL
            UrlDialogConfig.SelectableContent = SelectableContentEnum.AllContent;
            FileBrowserLinkBrowseUrl = CMSDialogHelper.GetDialogUrl(UrlDialogConfig, IsLiveSite, false, null);

            // Add attributes to append to the image path
            if (IsLiveSite)
            {
                // Always append chset
                string chset = Guid.NewGuid().ToString();
                string appendParams = "chset=" + chset;

                int documentId = MediaDialogConfig.AttachmentDocumentID;
                if (documentId > 0)
                {
                    // Add requirement for latest version of files for current document
                    appendParams += "&latestfordocid=" + documentId;
                    appendParams += "&hash=" + ValidationHelper.GetHashString("d" + documentId, new HashSettings(""));
                }

                Config["AppendToImagePath"] = appendParams;
            }
        }


        /// <summary>
        /// Initialize additional scripts.
        /// </summary>
        private void InitializeScripts()
        {
            mBasePath = (BasePath.StartsWithCSafe("~") ? ResolveUrl(BasePath) : BasePath).TrimEnd('/');
            string scriptPath = ResolveUrl(String.Format("{0}{1}/ckeditor.js", mBasePath, (UseSource ? "/_source" : string.Empty)));

            // Load main editor scripts. 
            // The script is registered directly by ScriptManger to avoid double minification when using external storage
            ScriptManager.RegisterClientScriptInclude(Page, typeof(string), "CKEditorScript", scriptPath);
            String methodName = UseInlineMode ? "inline" : "replace";
            ScriptHelper.RegisterClientScriptBlock(Page, typeof(string), "CKEditor_Encode", @"
function CKEditor_TextBoxEncode(id, e) {
    if (CKEDITOR && CKEDITOR.instances && CKEDITOR.instances[id]) {
        var instance = CKEDITOR.instances[id];
        if (e && (typeof (Page_BlockSubmit) === 'undefined' || !Page_BlockSubmit)) {
            instance.destroy();
            var f = document.getElementById(id);
            if (f) {
                f.style.visibility = 'hidden';
            }
        }
        else {
            instance.updateElement();
        }
    }
}

function CKReplace(id, config) {
    if (CKEDITOR && CKEDITOR.instances) {
        var instance = CKEDITOR.instances[id];
        if (instance) {
            config = config || instance.config;
            try {
                instance.destroy();
            } catch(e) { }
        }
        var t = document.getElementById(id);
        if (t) {
            CKEDITOR." + methodName + @"(id, config);
            CKEDITOR.instances[id].on('blur', function() {{ this.updateElement(); }});
            
            // Instance config is loaded too late, add this property directly.
            CKEDITOR.instances[id].config.useInlineMode = " + (UseInlineMode ? "true" : "false") + @";
        }
    }
}", true);

            StringBuilder sbScript = new StringBuilder();

            if (BrowserHelper.IsIE())
            {
                // Bug Fix: IE9 >>> http://bugs.jquery.com/ticket/13378
                sbScript.Append(@"document.documentElement.focus();");
            }

            // Replace text area
            String replaceScript = String.Format("CKReplace('{0}', {{ {1} }});\n", ClientID, Config.GetJsConfigArray());

            if (mIsInUpdatePanel)
            {
                // Delayed loading in update panel
                sbScript.AppendFormat(@"
function CKReplace_{0}(){{
    if (CKEDITOR && CKEDITOR.on){{
        if(window.CKTimer_{0}){{
            clearTimeout(window.CKTimer_{0});
        }}
        if ( CKEDITOR.status == 'loaded' || CKEDITOR.status == 'basic_ready' || CKEDITOR.status == 'ready' )
        {{
            CKReplace_{0}_Run();
        }} 
        else {{
            CKEDITOR.on( 'loaded', function(e){{
                CKReplace_{0}_Run();          
            }});
        }}
    }}
    else{{
        window.CKTimer_{0} = setTimeout('CKReplace_{0}();', 100);
    }}
}};
function CKReplace_{0}_Run() {{
    if(typeof(CKEDITOR.instances['{0}']) !== 'undefined') {{
        try {{
            CKEDITOR.instances['{0}'].destroy();
        }} catch(e) {{ }}
    }}
    {1}
}}
if (Sys.WebForms.PageRequestManager) {{
    function CKEditor_TextBoxEncode_{0}() {{
        CKEditor_TextBoxEncode('{0}', 1);
    }}
    Sys.WebForms.PageRequestManager.getInstance().add_beginRequest(CKEditor_TextBoxEncode_{0});
}}

CKReplace_{0}();", ClientID, replaceScript);
            }
            else
            {
                sbScript.Append(replaceScript);
            }

            // Start script
            ScriptHelper.RegisterStartupScript(Page, typeof(string), ClientID + "_CKE_Startup", sbScript.ToString(), true);

            // Postback script
            ScriptHelper.RegisterOnSubmitStatement(Page, typeof(string), "CKEditor_Submit_" + ClientID, string.Format("CKEditor_TextBoxEncode('{0}', {1});\n", ClientID, mIsInUpdatePanel ? 1 : 0));

            // Register CMS script
            ScriptHelper.RegisterCMS(Page);
        }


        private static string ResolveInlineControls(string resolveValue)
        {
            if (!String.IsNullOrEmpty(resolveValue))
            {
                return ControlsHelper.RegExControl.Replace(resolveValue, ResolveInlineControlsEvaluator);
            }
            return resolveValue;
        }


        private static string ResolveInlineControlsEvaluator(Match match)
        {
            try
            {
                StringBuilder sb = new StringBuilder();

                string width = null;
                string height = null;
                string controlName = null;
                string lControlName = null;
                string controlParameter = null;
                Hashtable parameters = ControlsHelper.ParseInlineParameters(match.Groups["macro"].ToString(), ref controlParameter, ref controlName);
                if (controlName != null)
                {
                    lControlName = controlName.ToLowerCSafe();
                }
                else
                {
                    return match.ToString();
                }
                
                if (parameters.Count > 0)
                {
                    foreach (DictionaryEntry parameter in parameters)
                    {
                        string name = parameter.Key.ToString().ToLowerCSafe();
                        string value = parameter.Value.ToString();
                        switch (name)
                        {
                            case "width":
                                width = value;
                                break;

                            case "height":
                                height = value;
                                break;

                            default:
                                sb.AppendFormat("<param name=\"{0}\" value=\"{1}\" />", name, value);
                                break;
                        }
                    }

                    if (lControlName == "widget")
                    {
                        // Don't use width and height for widgets
                        sb.AppendFormat("<param name=\"width\" value=\"{0}\" />", width);
                        sb.AppendFormat("<param name=\"height\" value=\"{0}\" />", height);
                        width = height = null;
                    }
                }
                sb.Append("</object>");

                // Append 'display: none' for inline mode. It renders HTML before CMSPlugins processes it and renders '<object' tag to HTML witch makes ugly missing-plugin blink in most browsers.
                string objDefinition = String.Format("<object style=\"display: none\" codetype=\"{0}\" type=\"{1}\" ", INLINE_CONTROL_CODE_TYPE, controlName);

                // Ensure width and height in object definition
                if ((width != null) && (height != null))
                {
                    objDefinition += String.Format("width=\"{0}\" height=\"{1}\" ", width, height);
                }
                objDefinition += ">";
                sb.Insert(0, objDefinition);

                return sb.ToString();
            }
            catch
            {
                return match.ToString();
            }
        }


        private static string UnResolveInlineControls(string resolveValue)
        {
            if (!String.IsNullOrEmpty(resolveValue))
            {
                return ObjectRegExp.Replace(resolveValue, UnResolveInlineControlsEvaluator);
            }
            return resolveValue;
        }


        private static string UnResolveInlineControlsEvaluator(Match match)
        {
            string inlineControl = match.ToString();
            Match codeType = ObjectCodeTypeRegExp.Match(inlineControl);
            
            // Check that object is CMS inline control
            if ((codeType.Success) && (codeType.Groups[1].Value.EqualsCSafe(INLINE_CONTROL_CODE_TYPE, true)))
            {
                Match type = ObjectTypeRegExp.Match(inlineControl);
                if (type.Success)
                {
                    string controlName = type.Groups[1].Value.ToLowerCSafe();
                    // Exclude flash object
                    if (!controlName.Contains("flash"))
                    {
                        StringBuilder sb = new StringBuilder();

                        sb.Append("{^");
                        sb.Append(controlName);

                        Match width = ObjectWidthRegExp.Match(inlineControl);
                        Match height = ObjectHeightRegExp.Match(inlineControl);
                        if (CMSString.Compare(controlName, "widget") != 0)
                        {
                            if (width.Success)
                            {
                                sb.Append("|(width)");
                                sb.Append(width.Groups[1].Value);
                            }

                            if (height.Success)
                            {
                                sb.Append("|(height)");
                                sb.Append(height.Groups[1].Value);
                            }
                        }

                        MatchCollection parameters = ParamsRegExp.Matches(inlineControl);
                        if (parameters.Count > 0)
                        {
                            foreach (Match parameter in parameters)
                            {
                                string[] param = new string[2];

                                // Get parameter definition
                                for (int i = 0; i < parameter.Groups[1].Captures.Count; i++)
                                {
                                    if (CMSString.Compare(parameter.Groups[1].Captures[i].Value, "name") == 0)
                                    {
                                        param[0] = parameter.Groups[2].Captures[i].Value;
                                    }
                                    else
                                    {
                                        param[1] = parameter.Groups[2].Captures[i].Value;
                                    }
                                }
                                // Skip cms type parameter
                                if (CMSString.Compare(param[0], "cms_type", false) != 0)
                                {
                                    sb.AppendFormat("|({0}){1}", param[0], param[1]);
                                }
                            }
                        }

                        sb.Append("^}");

                        return sb.ToString();
                    }
                }
            }
            return inlineControl;
        }


        private static string UnEscapeMacros(string resolveValue)
        {
            return MacroProcessor.DecodeMacros(resolveValue);
        }

        #endregion


        #region "Helper methods"

        /// <summary>
        /// Updates the ExtraPlugins config from its List equivalent.
        /// </summary>
        private void UpdateConfigExtraPlugins()
        {
            if (mExtraPlugins.Count > 0)
            {
                Config["extraPlugins"] = mExtraPlugins.Join(",");
            }
        }


        /// <summary>
        /// Updates the RemovePlugins config from its List equivalent.
        /// </summary>
        private void UpdateConfigRemovePlugins()
        {
            if (mRemovePlugins.Count > 0)
            {
                Config["removePlugins"] = mRemovePlugins.Join(",");
            }
        }


        /// <summary>
        /// Updates the RemoveButtons config from its List equivalent.
        /// </summary>
        private void UpdateConfigRemoveButtons()
        {
            if (mRemoveButtons.Count > 0)
            {
                Config["removeButtons"] = mRemoveButtons.Join(",");
            }
        }


        /// <summary>
        /// Gets controls ClientID.
        /// </summary>
        /// <param name="name">Controls name</param>
        protected string GetControlClientID(string name)
        {
            using (Control c = FindControl(name))
            {
                if (c == null)
                {
                    return string.Empty;
                }
                return c.ClientID;
            }
        }


        private void PrepareSharedSpacesConfig()
        {
            string top = SharedSpacesTopClientID;
            if (top.Length == 0)
            {
                top = SharedSpacesTop;
                if (top.Length > 0)
                {
                    top = GetControlClientID(top);
                }
            }
            string bottom = SharedSpacesBottomClientID;
            if (bottom.Length == 0)
            {
                bottom = SharedSpacesBottom;
                if (bottom.Length > 0)
                {
                    bottom = GetControlClientID(bottom);
                }
            }
            if (top.Length > 0 || bottom.Length > 0)
            {
                string config = "{";
                if (top.Length > 0)
                {
                    config += String.Format("top:'{0}'", top);
                }
                if (bottom.Length > 0)
                {
                    if (top.Length > 0)
                    {
                        config += ',';
                    }
                    config += String.Format("bottom:'{0}'", bottom);

                    // Removes the resizer as it's not usable in a shared elements path.
                    // This may cause duplicate values if the user has already specified this, but it does not matter at all
                    RemovePlugins.Add("resize");
                }
                config += "}";
                Config["sharedSpaces"] = config;
            }

            // Removes the maximize button as it's not correctly usable in inline toolbar.
            if (top.Length == 0)
            {
                RemoveButtons.Add("Maximize");
            }
        }


        /// <summary>
        /// Do stuff that is required to prepare certain config entries,
        /// just before rendering
        /// </summary>
        protected void ConfigPreRender()
        {
            Config["SiteName"] = SiteContext.CurrentSiteName;

            if (Node != null)
            {
                Config["CurrentDocumentID"] = Node.DocumentID.ToString();
                Config["CurrentCulture"] = Node.DocumentCulture;
                Config["CurrentAliasPath"] = Node.NodeAliasPath;
            }

            Config["CurrentGroupID"] = ModuleCommands.CommunityGetCurrentGroupID().ToString();

            if (!Enabled)
            {
                ToolbarSet = "Disabled";

                // Disable context menu plugin and plugins which require it
                RemovePlugins.Add("contextmenu");
                RemovePlugins.Add("toolbar");
                RemovePlugins.Add("tabletools");
            }

            // Get UI elements related to the editor
            var uiElements = GetUIElementsForWysiwygEditor();

            // Remove toolbar buttons of modules which aren't availbale
            RemoveButtonsOfUnavailableModules(uiElements);

            bool personalizeToolbareOnLiveSite = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CKEditor:PersonalizeToolbarOnLiveSite"], false);

            // Personalization is used only if there is not livesite or current user is not Global admin
            if (!MembershipContext.AuthenticatedUser.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin) && (!IsLiveSite || personalizeToolbareOnLiveSite))
            {
                PersonalizeToolbar(uiElements);
            }

            // Prepare shared spaces config
            if (sharedToolbar || !UseInlineMode)
            {
                PrepareSharedSpacesConfig();
            }

            // Refresh config items from their equivalent arrays if needed
            UpdateConfigRemovePlugins();
            UpdateConfigRemoveButtons();
            UpdateConfigExtraPlugins();
        }


        /// <summary>
        /// Get all WYSIWYG editor UI elements (results are cached).
        /// </summary>
        /// <returns>InfoDataSet with all UI elements that are associated with WYSIWYG editor</returns>
        private InfoDataSet<UIElementInfo> GetUIElementsForWysiwygEditor()
        {
            var wysiwygUIElementInfo = UIElementInfoProvider.GetUIElementInfo("CMS.WYSIWYGEditor", "cmswysiwygeditor");

            InfoDataSet<UIElementInfo> uiElements = null;
            int cacheMinutes = SettingsKeyInfoProvider.GetIntValue(SiteContext.CurrentSiteName + ".CMSCacheMinutes");

            using (var cs = new CachedSection<InfoDataSet<UIElementInfo>>(ref uiElements, cacheMinutes, true, null, "wysiwygchilduielements", wysiwygUIElementInfo.ElementFullName))
            {
                if (cs.LoadData)
                {
                    uiElements = UIElementInfoProvider.GetUIElements()
                        .WhereStartsWith("ElementIDPath", wysiwygUIElementInfo.ElementIDPath)
                        .WhereEquals("ElementChildCount", 0)
                        .Columns("ElementName", "ElementResourceID")
                        .TypedResult;

                    if (cs.Cached)
                    {
                        cs.CacheDependency = CacheHelper.GetCacheDependency(new[] { "cms.uielement|all" });
                    }

                    cs.Data = uiElements;
                }
            }

            return uiElements;
        }


        /// <summary>
        /// Removes buttons from WYSIWYG editor based on registered UI elements.
        /// </summary>
        /// <param name="UIElements">All UI elements available in WYSIWYG editor</param>
        private void RemoveButtonsOfUnavailableModules(IEnumerable<UIElementInfo> UIElements)
        {
            foreach (UIElementInfo UIElement in UIElements)
            {
                var module = ResourceInfoProvider.GetResourceInfo(UIElement.ElementResourceID);

                // CMS.WYSIWYGeditor is default module for all elements
                if (!module.ResourceName.Equals("cms.wysiwygeditor", StringComparison.OrdinalIgnoreCase) && !CheckModuleAvailability(module))
                {
                    RemoveButtons.Add(UIElement.ElementName);
                }
            }
        }


        private static bool CheckModuleAvailability(ResourceInfo module)
        {
            return ResourceInfoProvider.IsResourceAvailable(module.ResourceID)
                && ResourceSiteInfoProvider.IsResourceOnSite(module.ResourceName, SiteContext.CurrentSiteName);
        }


        /// <summary>
        /// Personalize editors toolbar.
        /// </summary>
        /// <param name="uiElements">All UI elements available in WYSIWYG editor</param>
        private void PersonalizeToolbar(IEnumerable<UIElementInfo> uiElements)
        {
            // As inline mode use different source plug-in, disable it also for source disabled.
            if (UseInlineMode && !MembershipContext.AuthenticatedUser.IsAuthorizedPerUIElement("CMS.WYSIWYGEditor", "source"))
            {
                RemoveButtons.Add("Sourcedialog");
            }

            // Check if personalization is enabled
            var personalizationEnabled = SettingsKeyInfoProvider.GetBoolValue(SiteContext.CurrentSiteName + ".CMSPersonalizeUserInterface");
            if (personalizationEnabled)
            {
                var user = MembershipContext.AuthenticatedUser;

                bool hasAnyButtons = false;
                foreach (UIElementInfo uiElement in uiElements)
                {
                    string elemName = uiElement.ElementName;

                    // Use elements' resource to check if current user is authorized 
                    if (!user.IsAuthorizedPerUIElement(uiElement.ElementResourceID, elemName))
                    {
                        RemoveButtons.Add(elemName);
                    }
                    else
                    {
                        hasAnyButtons = true;
                    }
                }

                // Hide toolbar if no buttons are left
                if (!hasAnyButtons)
                {
                    RemovePlugins.Add("toolbar");
                }
            }
        }


        /// <summary>
        /// Actualize specified selector url path with specified parameters.
        /// </summary>
        /// <param name="selectorUrl">Selector url</param>
        /// <param name="parameterName">Parameter name</param>
        /// <param name="parameterValue">Parameter value</param>
        private static string ActualizeUrl(string selectorUrl, string parameterName, string parameterValue)
        {
            if (!String.IsNullOrEmpty(parameterValue))
            {
                return URLHelper.UpdateParameterInUrl(selectorUrl, parameterName, parameterValue);
            }
            else
            {
                return URLHelper.RemoveParameterFromUrl(selectorUrl, parameterName);
            }
        }

        #endregion


        #region "Postback Handling"

        /// <summary>
        /// Processes postback data for an ASP.NET server control.
        /// </summary>
        public virtual bool LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
            string postedValue = postCollection[postDataKey];

            if (HtmlEncodeOutput)
            {
                // Revert the Html encoding.
                postedValue = postedValue.Replace("&lt;", "<").Replace("&gt;", ">").Replace("&amp;", "&");
            }

            if (!postedValue.EqualsCSafe(Value))
            {
                Value = postedValue;
            }

            return true;
        }


        /// <summary>
        /// Signals the server control to notify the ASP.NET application that the state of the control has changed.
        /// </summary>
        public virtual void RaisePostDataChangedEvent()
        {
            // Do nothing
        }

        #endregion
    }
}
