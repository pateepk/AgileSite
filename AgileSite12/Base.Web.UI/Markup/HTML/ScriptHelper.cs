using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using CMS.Core;
using CMS.Helpers;
using CMS.IO;
using CMS.Localization;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Utility methods for script manipulation.
    /// </summary>
    public static class ScriptHelper
    {
        #region "Internal classes"

        /// <summary>
        /// Newtonsoft.JSON wrapper
        /// </summary>
        /// <remarks>
        /// Allows to remove Newtonsoft.JSON references from static signatures.
        /// </remarks>
        private class NewtonsoftJsonSerializer
        {
            /// <summary>
            /// The naming convention used for serialization of module parameters.
            /// </summary>
            private readonly JsonSerializerSettings CamelCaseSerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };


            /// <summary>
            /// Serializes the specified object to a JSON string using formatting and <see cref="JsonSerializerSettings"/>.
            /// </summary>
            /// <param name="value">The object to serialize.</param>
            /// <remarks>
            /// We have to use this method because of breaking change in code (Newtonsoft.Json), causing runtime error due to not-accessible methods
            /// </remarks>
            /// <returns>
            /// A JSON string representation of the object.
            /// </returns>
            public string SerializeObject(object value)
            {
                var jsonSerializer = JsonSerializer.Create(CamelCaseSerializerSettings);

                var sb = new StringBuilder(256);
                var sw = new StringWriter(sb, System.Globalization.CultureInfo.InvariantCulture);

                using (var jsonWriter = new JsonTextWriter(sw))
                {
                    jsonWriter.StringEscapeHandling = StringEscapeHandling.EscapeHtml;
                    jsonWriter.Formatting = Formatting.Indented;

                    jsonSerializer.Serialize(jsonWriter, value);
                }

                return sb.ToString();
            }
        }

        #endregion


        #region "Constants"

        /// <summary>
        /// Script registration key for angular modules.
        /// </summary>
        private const string ANGULAR_MODULES_KEY = "CMSAngularModules";


        /// <summary>
        /// Script registration key for the dialog script.
        /// </summary>
        public const string DIALOG_SCRIPT_KEY = "dialogScript";


        /// <summary>
        /// Script registration key for the non modal win script.
        /// </summary>
        public const string NEWWINDOW_SCRIPT_KEY = "newWindowScript";


        /// <summary>
        /// Script registration key for the editor toolbar.
        /// </summary>
        public const string TOOLBAR_SCRIPT_KEY = "toolbarScript";


        /// <summary>
        /// Edit document script key.
        /// </summary>
        public const string EDIT_DOCUMENT_SCRIPT_KEY = "editDocumentScript";


        /// <summary>
        /// Save document script key.
        /// </summary>
        public const string SAVE_DOCUMENT_SCRIPT_KEY = "ctrlSScript";


        /// <summary>
        /// Script registration key for cross-window scripting ("wopener").
        /// </summary>
        public const string WOPENER_SCRIPT_KEY = "wopenerScript";


        /// <summary>
        /// Script registration key for centralized dialog closing.
        /// </summary>
        public const string CLOSE_DIALOG_SCRIPT_KEY = "closeDialog";


        /// <summary>
        /// Script registration key for centralized retrieving of 'top' frame.
        /// </summary>
        public const string GET_TOP_SCRIPT_KEY = "getTop";


        /// <summary>
        /// Prefix used for registering script files.
        /// </summary>
        public const string SCRIPTFILE_PREFIX_KEY = "ScriptFile_";


        /// <summary>
        /// Script registration key for tooltip file.
        /// </summary>
        public const string TOOLTIP_SCRIPT_FILE_KEY = "TooltipScriptFileKey";


        /// <summary>
        /// Script registration key for jQuery JavaScript library.
        /// </summary>
        public const string JQUERY_KEY = "jQuery";


        /// <summary>
        /// Filename of jquery library
        /// </summary>
        public const string JQUERY_FILENAME = "jquery/jquery-core.js";


        /// <summary>
        /// Filename of bootstrap library.
        /// </summary>
        public const string BOOTSTRAP_FILENAME = "Bootstrap/bootstrap.min.js";


        /// <summary>
        /// Filename of custom bootstrap library.
        /// </summary>
        public const string BOOTSTRAP_CUSTOM_FILENAME = "Bootstrap/bootstrap.custom.js";


        /// <summary>
        /// Filename of MooTools library.
        /// </summary>
        public const string MOOTOOLS_FILENAME = "mootools.js";


        /// <summary>
        /// Script registration key for mootools JavaScript library.
        /// </summary>
        public const string MOOTOOLS_KEY = "mootools";


        /// <summary>
        /// Filename of Underscore library.
        /// </summary>
        public const string UNDERSCORE_FILENAME = "Underscore/underscore.min.js";


        /// <summary>
        /// Script registration key for Underscore library.
        /// </summary>
        public const string UNDERSCORE_KEY = "underscore";


        /// <summary>
        /// Script registration key for only numbers script.
        /// </summary>
        private const string ONLYNUMBERS_KEY = "OnlyNumbers";


        /// <summary>
        /// Script registration key for web components generated by Page builder build.
        /// </summary>
        private const string COMPONENTS_KEY = "components";


        #endregion


        #region "Variables"

        private static string mNewWindowScript;


        private static string mToolbarScript;


        private static string mWopenerScript;


        private static string mCloseDialogScript;


        private static string mGetTopScript;


        private static string mCapslockScript;


        private static string mOnlyNumbersScript;


        /// <summary>
        /// If true, progress script is allowed on pages.
        /// </summary>
        private static bool? mAllowProgressScript;

        private static readonly Lazy<NewtonsoftJsonSerializer> mJsonSerializer = new Lazy<NewtonsoftJsonSerializer>(() => new NewtonsoftJsonSerializer());

        #endregion


        #region "Properties"

        /// <summary>
        /// If true, the scripts in current request are minified.
        /// </summary>
        public static bool MinifyCurrentRequestScripts
        {
            get
            {
                return WebLanguagesContext.MinifyCurrentRequestScripts;
            }
            set
            {
                WebLanguagesContext.MinifyCurrentRequestScripts = value;
            }
        }


        /// <summary>
        /// Gets if script minification is enabled.
        /// </summary>
        public static bool ScriptMinificationEnabled
        {
            get
            {
                // MinifyCurrentRequestScripts has to be checked first as it does not contain any logic that needs connection string to be initialized
                return MinifyCurrentRequestScripts && CoreServices.Settings["CMSScriptMinificationEnabled"].ToBoolean(false) && !SystemContext.IsRunningOnAzure;
            }
        }


        /// <summary>
        /// If true, progress script is allowed on pages.
        /// </summary>
        public static bool AllowProgressScript
        {
            get
            {
                if (mAllowProgressScript == null)
                {
                    mAllowProgressScript = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSAllowProgressScript"], true);
                }
                return mAllowProgressScript.Value;
            }
            set
            {
                mAllowProgressScript = value;
            }
        }


        /// <summary>
        /// Script code for the dialog handling.
        /// </summary>
        public static string NewWindowScript
        {
            get
            {
                if (mNewWindowScript == null)
                {
                    const string script =
                        @"
function NewWindow(url, name, width, height) {
	var oWindow = window.open(url, name, 'height=' + height + ',width=' + width + ',toolbar=no,directories=no,menubar=no,dependent=yes,resizable=yes');
	oWindow.opener = this;
	oWindow.focus();
}
";
                    mNewWindowScript = GetScript(script);
                }
                return mNewWindowScript;
            }
            set
            {
                mNewWindowScript = value;
            }
        }


        /// <summary>
        /// HTML Editor toolbar script.
        /// </summary>
        public static string ToolbarScript
        {
            get
            {
                if (mToolbarScript == null)
                {
                    const string script = "if ( (parent != null) && (parent.ShowToolbar) ) { parent.ShowToolbar(); } \n";
                    mToolbarScript = GetScript(script);
                }
                return mToolbarScript;
            }
            set
            {
                mToolbarScript = value;
            }
        }


        /// <summary>
        /// Capslock check script (OnCasplockOn and OnCapslockOff functions must be implemented).
        /// </summary>
        public static string CapslockScript
        {
            get
            {
                if (mCapslockScript == null)
                {
                    const string script = @"
function CheckCapsLock(e) {
	kc = e.keyCode?e.keyCode:e.which;
	sk = e.shiftKey?e.shiftKey:((kc == 16)?true:false);
	if(((kc >= 65 && kc <= 90) && !sk)||((kc >= 97 && kc <= 122) && sk)){
		OnCapslockOn();
	} else {
		OnCapslockOff();
	}
}";
                    mCapslockScript = GetScript(script);
                }
                return mCapslockScript;
            }
            set
            {
                mCapslockScript = value;
            }
        }


        /// <summary>
        /// Only numbers script (Must be implemented in OnKeyDown event).
        /// </summary>
        public static string OnlyNumbersScript
        {
            get
            {
                if (mOnlyNumbersScript == null)
                {
                    const string script = @"
function OnlyNumbers(e, skipEnter) {
	var keynum;
	// Indicates whether the key is allowed
	var letFlow = 1;
	// Define allowed keys        
	var enter = 13;var bckspc = 8;
	var del = 46;var tab = 9;
	var arrLeft = 37;var arrUp = 38;
	var arrRight = 39;var arrBottom = 40;
	// IE
	if (e.keyCode) {
		keynum = e.keyCode;
	}
	// Firefox/Opera/Safari
	else if (e.which) {
		keynum = e.which;
	}
	// Is NOT a number ?
	if (!(((keynum > 47) && (keynum < 58)) || ((keynum > 95) && (keynum < 106)))) {
		letFlow = 0;
		// Is at least special key ?
		if ((keynum == enter) || (keynum == bckspc) || (keynum == del) ||
			(keynum == tab) || (keynum == arrLeft) || (keynum == arrUp) ||
			(keynum == arrRight) || (keynum == arrBottom)) {
			letFlow = 1;
		}

		if (skipEnter && (keynum == enter)){
			letFlow = 0;
		}
	}
	return (letFlow == 1);
}";

                    mOnlyNumbersScript = GetScript(script);
                }
                return mOnlyNumbersScript;
            }
            set
            {
                mOnlyNumbersScript = value;
            }
        }


        /// <summary>
        /// Cross-window scripting script ("wopener").
        /// </summary>
        public static string WOpenerScript
        {
            get
            {
                return mWopenerScript ?? (mWopenerScript = GetScript(
                    @"
if (wopener == null) { 
	var wopener = window.dialogArguments;
} 
if (wopener == null) {
	wopener = opener;
}
if ((wopener == null) && (top != null))
{ 
	if(top.getWopener) {
		wopener  = top.getWopener(window);
	}
	else {
		wopener =  window.top.opener ? window.top.opener : window.top.dialogArguments;
	}
}"));
            }

            set
            {
                mWopenerScript = value;
            }
        }


        /// <summary>
        /// Script for centralized dialog closing.
        /// </summary>
        public static string CloseDialogScript
        {
            get
            {
                // IE9 fix - setTimeout to prevent Sys is undefined
                return mCloseDialogScript ?? (mCloseDialogScript = GetScript(
                    @"
function CloseDialog(refreshPage) {

    // Check that the document content has not been changed without saving. Stop closing the dialog when user decides to save the content.
    if (window.CheckChanges && !CheckChanges()) {
        return false;
    }

    if (typeof(refreshPage) === ""undefined"") {
        refreshPage = true;
    }

    try {
        // IE9 fix - wopener doesn't have to be available
	    if(refreshPage && window.wopener && window.wopener.RefreshWOpener) {
		    wopener.RefreshWOpener(window);
	    }
    }
    catch(err) {}
	var canClose = true;
	if (window.onCloseDialog) {
		canClose = window.onCloseDialog();
	}
	if (canClose) {
		if(top.closeDialog && (top != window)) {
			setTimeout(function(){
                if(top && top.closeDialog && (top != window)){ 
                    top.closeDialog(window)
                }
            }, 1);
		} 
		else {
			top.close(); 
		}
	}
	return false; 
}"));
            }

            set
            {
                mCloseDialogScript = value;
            }
        }


        /// <summary>
        /// Script for centralized retrieving of 'top' frame.
        /// </summary>
        public static string GetTopScript
        {
            get
            {
                return mGetTopScript ?? (mGetTopScript = GetScript("function GetTop(){ if(top.getTop) { return top.getTop(window); } else { return top; } }"));
            }

            set
            {
                mGetTopScript = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Reset scroll positions for current request if MaintainScrollPositionOnPostback is enabled
        /// </summary>
        /// <param name="page">Current System.Web.UI.Page object</param>
        public static void ResetScrollPosition(Page page)
        {
            RegisterHiddenField(page, "__SCROLLPOSITIONX", "0");
            RegisterHiddenField(page, "__SCROLLPOSITIONY", "0");
        }


        /// <summary>
        /// Renders alert script to the page
        /// </summary>
        /// <param name="page">Page</param>
        /// <param name="message">Message to show</param>
        public static void Alert(Page page, string message)
        {
            string script = String.Format("setTimeout({0}, 0)", GetString(GetAlertScript(message, false)));

            RegisterStartupScript(page, typeof(string), "Alert_" + Guid.NewGuid(), script, true);
        }


        /// <summary>
        /// Adds a script to an existing script
        /// </summary>
        /// <param name="script">Original script</param>
        /// <param name="add">Script to add</param>
        public static string AddScript(string script, string add)
        {
            if (String.IsNullOrEmpty(add))
            {
                return script;
            }
            if (String.IsNullOrEmpty(script))
            {
                return add;
            }

            // Ensure proper ending of the previous script
            if (!script.Trim().EndsWithAny(StringComparison.InvariantCultureIgnoreCase, ";", "}"))
            {
                script += ";";
            }

            script += add;

            return script;
        }


        /// <summary>
        /// Returns the JavaScript alert message.
        /// </summary>
        /// <param name="message">Message to display</param>
        /// <param name="addScriptTags">True to enclose the script block in script and /script tags, otherwise false</param>
        public static string GetAlertScript(string message, bool addScriptTags = true)
        {
            string script = String.Format("alert({0});", GetString(message));
            if (addScriptTags)
            {
                return GetScript(script);
            }
            return script;
        }


        /// <summary>
        /// Builds an HTML script tag that can be used to include external script given its URL.
        /// Allows to explicitly disable script minification.
        /// </summary>
        /// <param name="url">URL of the script file</param>
        /// <param name="minify">True if minification should be used, otherwise false</param>
        /// <param name="executionMode">Specifies script's execution mode.</param>
        /// <returns>Script tag that references external URL</returns>
        public static string GetScriptTag(string url, bool minify = true, ScriptExecutionModeEnum executionMode = ScriptExecutionModeEnum.Normal)
        {
            if (String.IsNullOrEmpty(url))
            {
                return null;
            }

            // Transform and resolve URL
            url = GetScriptUrl(url, minify);

            string executionModeAttribute = string.Empty;
            switch (executionMode)
            {
                case ScriptExecutionModeEnum.Asynchronous:
                    executionModeAttribute = " async=\"async\"";
                    break;
                case ScriptExecutionModeEnum.Deferred:
                    executionModeAttribute = " defer=\"defer\"";
                    break;
                default:
                    break;
            }

            return String.Format(@"<script src=""{0}"" type=""text/javascript""{1}></script>", url, executionModeAttribute);
        }


        /// <summary>
        /// Encloses the specified script in an HTML script element.
        /// </summary>
        /// <param name="script">Script block to enclose in a element</param>
        public static string GetScript(string script)
        {
            // No script, no code
            if (String.IsNullOrEmpty(script))
            {
                return null;
            }

            // When using update panel in Safari, do not include CDATA section
            bool addCData = !(RequestHelper.IsAsyncPostback() && BrowserHelper.IsSafari());

            return String.Format(@"
<script type=""text/javascript"">
	{0}{1}{2}
</script>", addCData ? "//<![CDATA[\n" : String.Empty, script, addCData ? "\n//]]>" : String.Empty);
        }


        /// <summary>
        /// Gets the URL used to retrieve an external script file.
        /// Allows to explicitly disable script minification (does not apply
        /// if external storage is used or the file is in zip).
        /// </summary>
        /// <param name="path">Path to the script file</param>
        /// <param name="minify">True if minification should be used, otherwise false</param>
        /// <returns>URL to script file</returns>
        internal static string GetScriptUrl(string path, bool minify = true)
        {
            // Return path without change if path is absolute URL 
            if (IsAbsoluteUrl(path))
            {
                return path;
            }

            // Do not transform URL to GetResource.ashx format if path represents URL
            if (!IsGetResourceUsable(path))
            {
                return UrlResolver.ResolveUrl(path);
            }

            path = GetScriptFileUrl(path);

            // Transform the path to resource if minification is enabled, external storage is used or file is in zip package
            if (minify && (RequestHelper.AllowResourceCompression || ScriptMinificationEnabled) ||
                StorageHelper.IsExternalStorage(path) ||
                StorageHelper.IsZippedFilePath(path))
            {
                if (!path.StartsWithCSafe("~/") && !path.StartsWithCSafe("/"))
                {
                    // Resolve the URL according the current page
                    path = UrlResolver.ResolveUrl(path);
                }

                // Get the URL to the GetResource handler
                path = String.Format("~/CMSPages/GetResource.ashx?scriptfile={0}", HttpUtility.UrlEncode(path));
            }

            return UrlResolver.ResolveUrl(path);
        }


        /// <summary>
        /// Checks if URL request can be transformed to GetResource.ashx format.
        /// If path contains question mark then it is not a path to file but URL and it should not be handled by GetResource.ashx 
        /// If path ends with .ashx it is already URL and it should not be handled by GetResource.ashx 
        /// </summary>
        /// <param name="path">Path to file or URL.</param>
        private static bool IsGetResourceUsable(string path)
        {
            return !path.Contains('?') && !path.EndsWithCSafe(".ashx", true);
        }


        private static bool IsAbsoluteUrl(string url)
        {
            Uri result;
            return Uri.TryCreate(url, UriKind.Absolute, out result);
        }


        /// <summary>
        /// Encodes text to be used in JavaScript string and optionally encapsulates it with "'".
        /// </summary>
        /// <param name="text">Text to be encoded</param>
        /// <param name="encapsulate">If true, text is encapsulated it with "'"</param>
        /// <param name="encodeNewLine">If true, new line characters will be encoded</param>
        public static string GetString(string text, bool encapsulate = true, bool encodeNewLine = true)
        {
            if (text != null)
            {
                text = text.Replace(@"\", @"\\").Replace(@"'", @"\'").Replace("\"", @"\""").Replace("<", @"\<").Replace(">", @"\>").Replace("/", "\\/");

                if (encodeNewLine)
                {
                    text = text.Replace("\n", @"\n").Replace("\r", @"\r");
                }
                else
                {
                    text = text.Replace(@"\\n", @"\n").Replace(@"\\r", @"\r");
                }
            }

            if (encapsulate)
            {
                text = String.Format("'{0}'", text);
            }

            return text;
        }


        /// <summary>
        /// Localizes and encodes text to be used in JavaScript string and encapsulates it with "'".
        /// </summary>
        /// <param name="stringName"></param>
        public static string GetLocalizedString(string stringName)
        {
            return GetLocalizedString(stringName, true);
        }


        /// <summary>
        /// Localizes and encodes text to be used in JavaScript string and optionally encapsulates it with "'".
        /// </summary>
        /// <param name="stringName"></param>
        /// <param name="encapsulate"></param>
        public static string GetLocalizedString(string stringName, bool encapsulate)
        {
            string text = CoreServices.Localization.GetString(stringName);
            return GetString(text, encapsulate, false);
        }


        /// <summary>
        /// Resolves URL to be used in javascript. Virtual context prefix needs to be ensured.
        /// </summary>
        /// <param name="url">URL to resolve</param>
        public static string ResolveUrl(string url)
        {
            return UrlResolver.ResolveUrl(url, ensurePrefix: true);
        }


        /// <summary>
        /// Returns script to disable progress for a single following action.
        /// </summary>
        public static string GetDisableProgressScript()
        {
            return "window.noProgress = true;";
        }


        /// <summary>
        /// Formats string to be used for tooltip.
        /// </summary>
        /// <param name="text">Tooltip string</param>
        /// <param name="encode">Indicates whether the tooltip text should be encoded</param>
        /// <param name="escape">Indicates if apostrophes and backslashes should be escaped</param>
        public static string FormatTooltipString(string text, bool encode = true, bool escape = true)
        {
            if (text != null)
            {
                string baseText = (escape) ? text.Replace("\\", "\\\\").Replace("'", "\\'") : text;
                return encode ? HTMLHelper.HTMLEncodeLineBreaks(baseText) : HTMLHelper.EnsureHtmlLineEndings(baseText);
            }
            return null;
        }


        /// <summary>
        /// Appends tooltip to control.
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="tooltipText">Tooltip</param>
        /// <param name="cursor">Mouse pointer</param>
        public static void AppendTooltip(WebControl control, string tooltipText, string cursor)
        {
            AppendTooltip(control, tooltipText, cursor, 0, false);
        }


        /// <summary>
        /// Appends tooltip to control.
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="tooltipText">Tooltip</param>
        /// <param name="cursor">Mouse pointer</param>
        /// <param name="width">Width of the tooltip</param>
        /// <param name="encode">Indicates if the tooltip text should be encoded</param>
        public static void AppendTooltip(WebControl control, string tooltipText, string cursor, int width, bool encode)
        {
            control.Attributes.Remove("onmouseover");
            control.Attributes.Remove("onmouseout");

            // Append non empty tooltip only
            if (!String.IsNullOrEmpty(tooltipText))
            {
                if (!String.IsNullOrEmpty(cursor))
                {
                    control.Attributes.Remove("style");
                    control.Attributes.Add("style", String.Format("cursor: {0};", cursor));
                }

                string widthString = null;
                if (width > 0)
                {
                    widthString = ", WIDTH, " + width;
                }

                control.Attributes.Add("onmouseover", String.Format("Tip('{0}'{1})", FormatTooltipString(tooltipText, encode), widthString));
                control.Attributes.Add("onmouseout", "UnTip()");
            }
        }


        /// <summary>
        /// Appends tooltip to control.
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="tooltipText">Tooltip</param>
        /// <param name="cursor">Mouse pointer</param>
        /// <param name="width">Width of the tooltip</param>
        /// <param name="encode">Indicates if the tooltip text should be encoded</param>
        public static void AppendTooltip(HtmlGenericControl control, string tooltipText, string cursor, int width = 0, bool encode = false)
        {
            control.Attributes.Remove("onmouseover");
            control.Attributes.Remove("onmouseout");

            // Append non empty tooltip only
            if (!String.IsNullOrEmpty(tooltipText))
            {
                if (!String.IsNullOrEmpty(cursor))
                {
                    control.Attributes.Remove("style");
                    control.Attributes.Add("style", String.Format("cursor: {0};", cursor));
                }

                string widthString = null;
                if (width > 0)
                {
                    widthString = ", WIDTH, " + width;
                }

                control.Attributes.Add("onmouseover", String.Format("Tip('{0}'{1})", FormatTooltipString(tooltipText, encode), widthString));
                control.Attributes.Add("onmouseout", "UnTip()");
            }
        }


        /// <summary>
        /// Fixes common 'pendingCallbacks' JavaScript error.
        /// </summary>
        /// <param name="page">Page to register</param>
        public static void FixPendingCallbacks(Page page)
        {
            // Link cms.js
            RegisterCMS(page);

            RegisterClientScriptBlock(page, page.GetType(), "CallbackComplete_SyncFixed",
                GetScript("WebForm_CallbackComplete = WebForm_CallbackComplete_SyncFixed"));
        }


        /// <summary>
        /// Fixes SSL within WCF web services
        /// </summary>
        /// <param name="page">Page</param>
        /// <param name="serviceName">Service name</param>
        public static void FixSSLForWCFServices(Page page, string serviceName)
        {
            // Fix WCF for SSL accelerators
            if (RequestContext.IsSSL && !page.Request.IsSecureConnection)
            {
                RegisterStartupScript(page, typeof(string), "WCFFixSSL",
                    @"
(function fixServiceProxyPath(proxyClass) {    
    var path = proxyClass.get_path(),
        host = window.location.hostname,
        re = new RegExp('^http:\/\/' + host);

    path = path.replace(re, 'https://' + host);
    proxyClass.set_path(path);
})(" + serviceName + @")
", true);
            }
        }


        /// <summary>
        /// Ensures that the postback methods are present in the page
        /// </summary>
        /// <param name="ctrl">Control</param>
        public static void EnsurePostbackMethods(Control ctrl)
        {
            if (ctrl is ICallbackEventHandler)
            {
                ctrl.Page.ClientScript.GetCallbackEventReference(ctrl, "", "", "");
            }

            if (ctrl is IPostBackEventHandler)
            {
                ctrl.Page.ClientScript.GetPostBackEventReference(ctrl, "");
            }
        }


        /// <summary>
        /// Registers the script for modal dialogs. 
        /// Provides function modalDialog(url, name, width, height, otherParams, noWopener).
        /// </summary>
        /// <param name="page">Page to register</param>
        public static void RegisterDialogScript(Page page)
        {
            if (page != null)
            {
                if (!IsClientScriptBlockRegistered(DIALOG_SCRIPT_KEY))
                {
                    // Register the dialog script
                    AddToRegisteredClientScripts(DIALOG_SCRIPT_KEY);
                    RegisterScriptFromFile(page, "Dialogs/modaldialog.js");
                }
            }
        }


        /// <summary>
        /// Returns script which opens modal dialog.
        /// </summary>
        /// <param name="resolvedURL">Resolved URL to open in modal dialog.</param>
        /// <param name="dialogName">Name of dialog</param>
        /// <param name="windowWidth">Modal window width.</param>
        /// <param name="windowHeight">Modal window height</param>
        /// <param name="returnFalse">If true, the script returns false</param>
        public static string GetModalDialogScript(string resolvedURL, string dialogName, int windowWidth, int windowHeight, bool returnFalse = true)
        {
            return GetModalDialogScript(resolvedURL, dialogName, windowWidth.ToString(), windowHeight.ToString());
        }


        /// <summary>
        /// Returns script which opens modal dialog.
        /// </summary>
        /// <param name="resolvedURL">Resolved URL to open in modal dialog.</param>
        /// <param name="dialogName">Name of dialog</param>
        /// <param name="windowWidth">Modal window width.</param>
        /// <param name="windowHeight">Modal window height</param>
        /// <param name="returnFalse">If true, the script returns false</param>
        public static string GetModalDialogScript(string resolvedURL, string dialogName, string windowWidth = null, string windowHeight = null, bool returnFalse = true)
        {
            if (windowWidth == null)
            {
                windowWidth = "95%";
            }
            if (windowHeight == null)
            {
                windowHeight = "95%";
            }

            return String.Format("modalDialog('{0}', '{1}', '{2}', '{3}');{4}", resolvedURL, dialogName, windowWidth, windowHeight, returnFalse ? "return false;" : null);
        }


        /// <summary>
        /// Registers script for printing dialog.
        /// </summary>
        /// <param name="page">Page to register</param>
        public static void RegisterPrintDialogScript(Page page)
        {
            if (page != null)
            {
                RegisterClientScriptBlock(page, typeof(string), "myModalDialog", GetScript(
                    @"
function myModalDialog(url, name, width, height) { 
	win = window; 
	var dHeight = height; var dWidth = width; 
	if (( document.all )&&(navigator.appName != 'Opera')) { 
		try { win = wopener.window; } catch (e) {} 
		if ( parseInt(navigator.appVersion.substr(22, 1)) < 7 ) { dWidth += 4; dHeight += 58; }; 
		dialog = win.showModalDialog(url, this, 'dialogWidth:' + dWidth + 'px;dialogHeight:' + dHeight + 'px;resizable:yes;scroll:yes'); 
	} else { 
		oWindow = win.open(url, name, 'height=' + dHeight + ',width=' + dWidth + ',toolbar=no,directories=no,menubar=no,modal=yes,dependent=yes,resizable=yes,scroll=yes,scrollbars=yes'); oWindow.opener = this; oWindow.focus(); 
	} 
} 
"));
            }
        }


        /// <summary>
        /// Registers only numbers script for input fields.
        /// </summary>
        /// <param name="page">Page to register</param>
        public static void RegisterOnlyNumbersScript(Page page)
        {
            if (page != null)
            {
                if (!IsClientScriptBlockRegistered(ONLYNUMBERS_KEY))
                {
                    // Register the dialog script
                    RegisterClientScriptBlock(page, page.GetType(), ONLYNUMBERS_KEY, OnlyNumbersScript);
                }
            }
        }


        /// <summary>
        /// Registers the script which checks the page completeness on postback.
        /// </summary>
        /// <param name="page">Page to register</param>
        public static void RegisterWOpenerScript(Page page)
        {
            if (page != null)
            {
                if (!IsClientScriptBlockRegistered(WOPENER_SCRIPT_KEY))
                {
                    // Register the dialog script
                    RegisterClientScriptBlock(page, page.GetType(), WOPENER_SCRIPT_KEY, WOpenerScript);
                }
            }
        }


        /// <summary>
        /// Registers script for centralized dialog closing. (Contains functionality for refreshing opener window.)
        /// </summary>
        /// <param name="page">Page to register</param>
        public static void RegisterCloseDialogScript(Page page)
        {
            if (page != null)
            {
                if (!IsClientScriptBlockRegistered(CLOSE_DIALOG_SCRIPT_KEY))
                {
                    // Register the dialog script
                    RegisterClientScriptBlock(page, page.GetType(), CLOSE_DIALOG_SCRIPT_KEY, CloseDialogScript);
                }
            }
        }


        /// <summary>
        /// Registers script for centralized retrieving of 'top' frame. (Useful for dialogs.)
        /// </summary>
        /// <param name="page">Page to register</param>
        public static void RegisterGetTopScript(Page page)
        {
            if (page != null)
            {
                if (!IsClientScriptBlockRegistered(GET_TOP_SCRIPT_KEY))
                {
                    // Register the dialog script
                    RegisterClientScriptBlock(page, page.GetType(), GET_TOP_SCRIPT_KEY, GetTopScript);
                }
            }
        }


        /// <summary>
        /// Registers the script which checks the page completeness on postback.
        /// </summary>
        /// <param name="page">Page to register</param>
        public static void RegisterCompletePageScript(Page page)
        {
            if (page != null)
            {
                // Register CMS script
                RegisterCMS(page);

                if (!IsClientScriptBlockRegistered("CompletePage"))
                {
                    string script = GetScript("if ((window.originalPostback == null) && (window.__doPostBack != null)) { window.originalPostback = __doPostBack; __doPostBack = __doPostBackWithCheck; } \n");

                    // Register the script
                    RegisterClientScriptBlock(page, page.GetType(), "CompletePage", script);

                    // Register page loaded script
                    RegisterPageLoadedFlag(page);
                }
            }
        }


        /// <summary>
        /// Ensures registration of the progress icon script for the tree.
        /// </summary>
        /// <param name="page">Page to register</param>
        public static void RegisterTreeProgress(Page page)
        {
            if ((page != null) && (page.Form != null) && AllowProgressScript)
            {
                if (!IsClientScriptBlockRegistered("TreeProgress"))
                {
                    // Register the script
                    string progressScript =
                        String.Format(@"
if (window.TreeView_PopulateNode && !window.base_TreeView_PopulateNode) {{ window.base_TreeView_PopulateNode = TreeView_PopulateNode }};
TreeView_PopulateNode = function(data, index, node, selectNode, selectImageNode, lineType, text, path, databound, datapath, parentIsLast) {{
	if (!data) {{ return; }}
	if (!node.blur) {{ node = node[0]; }}
	node.blur();
	node.firstChild.src = '{0}';
	if (base_TreeView_PopulateNode) {{
		base_TreeView_PopulateNode(data, index, node, selectNode, selectImageNode, lineType, text, path, databound, datapath, parentIsLast); 
	}} 
}}
", UIHelper.GetImageUrl(page, "Design/Preloaders/preload.gif"));

                    RegisterStartupScript(page, page.GetType(), "TreeProgress", GetScript(progressScript));

                    // Mark as registered
                    AddToRegisteredClientScripts("TreeProgress");
                }
            }
        }


        /// <summary>
        /// Gets the script for the automatic window title.
        /// </summary>
        /// <param name="titlePart">Part of the window title</param>
        [Obsolete("Use custom implementation instead.")]
        public static string GetTitleScript(string titlePart)
        {
            return GetScript(String.Format(@"
var part = {0};
window.document.titlePart = part;
AutoTitle();", GetString(HttpUtility.HtmlDecode(titlePart))));
        }


        /// <summary>
        /// Generates the script to close the window and optionally refresh the opener content.
        /// </summary>
        /// <param name="page">Page</param>
        /// <param name="refreshOpener">If true, the opener is refreshed</param>
        [Obsolete("Use custom implementation instead.")]
        public static void CloseWindow(Page page, bool refreshOpener)
        {
            if (page != null)
            {
                if (!IsClientScriptBlockRegistered("CloseWindow"))
                {
                    // Register the script
                    string script = "window.close();";

                    if (refreshOpener)
                    {
                        script += @"
if (wopener != null) {
	if (wopener.Refresh != null) { 
		wopener.Refresh(); 
	}
	else { 
		wopener.location.replace(wopener.location)
	}
}";
                    }

                    RegisterClientScriptBlock(page, page.GetType(), "CloseWindow", GetScript(script));
                }
            }
        }


        /// <summary>
        /// Ensures registration of the application constants script. It provides constants applicationUrl, imagesUrl, isRTL.
        /// </summary>
        /// <param name="page">Page to register</param>
        public static void RegisterApplicationConstants(Page page)
        {
            if (page != null)
            {
                if (!IsClientScriptBlockRegistered("ApplicationConstants"))
                {
                    // Register the script
                    string constantsScript = GetScript(String.Format(@"
var applicationUrl = '{0}';
var imagesUrl = '{1}';
var isRTL = {2};", page.ResolveUrl("~/"),
                        UIHelper.GetImageUrl(page, "/", false, true),
                        (Thread.CurrentThread.CurrentUICulture.TextInfo.IsRightToLeft ? "true" : "false"))
                        );
                    RegisterClientScriptBlock(page, page.GetType(), "ApplicationConstants", constantsScript);
                }
            }
        }


        /// <summary>
        /// Registers the client script and renders the script from file into the page.
        /// </summary>
        /// <param name="page">The page object that is registering the client script block</param>
        /// <param name="url">The URL of the script file</param>
        public static void RegisterScriptFromFile(Page page, string url)
        {
            if (page == null)
            {
                return;
            }

            string key = SCRIPTFILE_PREFIX_KEY + url;
            if (!IsClientScriptBlockRegistered(key))
            {
                string content = null;

                // Cache the file content
                using (var cs = new CachedSection<string>(ref content, 60, true, null, "scriptfile", url))
                {
                    if (cs.LoadData)
                    {
                        // Get the data
                        url = GetScriptFileUrl(url);
                        string path = URLHelper.GetPhysicalPath(url);
                        content = File.ReadAllText(path);

                        // Store to the cache
                        if (cs.Cached)
                        {
                            cs.CacheDependency = CacheHelper.GetFileCacheDependency(path);
                        }

                        cs.Data = content;
                    }
                }

                // Register the block with script from file
                RegisterClientScriptBlock(page, typeof(string), key, GetScript(content));
            }
        }


        /// <summary>
        /// Registers helper function to call web services.
        /// </summary>
        /// <param name="page">Page to register</param>
        public static void RegisterWebServiceCallFunction(Page page)
        {
            RegisterScriptFile(page, "WebServiceCall.js");
        }


        /// <summary>
        /// Registers the client script and adds a script file reference to the page.
        /// Allows to explicitly disable script minification.
        /// </summary>
        /// <param name="element">Page element</param>
        /// <param name="url">The URL or name of the script file. If only name is provided, the file from ~/CMSScripts/ is taken. It is also possible to input URLs like "Controls/uniselector.js" to access subfolders.</param>
        /// <param name="minify">True if minification should be used, otherwise false</param>
        /// <param name="executionMode">Specifies script's execution mode.</param>
        public static void RegisterScriptFile(PageElement element, string url, bool minify = true, ScriptExecutionModeEnum executionMode = ScriptExecutionModeEnum.Normal)
        {
            // Prepare the script key
            string key = SCRIPTFILE_PREFIX_KEY + url;

            if (!IsClientScriptBlockRegistered(key))
            {
                if (element.Control != null)
                {
                    RegisterClientScriptInclude(element.Control, typeof(string), key, url, minify);
                }
                else
                {
                    var page = element.Page;

                    if (page.Form != null)
                    {
                        // Normal registration through the form
                        RegisterClientScriptInclude(page, typeof(string), key, url, minify);
                    }
                    // Add to the header in case the page doesn't have Form
                    else if ((page.Header != null) && !IsClientScriptBlockRegistered(url))
                    {
                        page.Header.Controls.Add(new LiteralControl(GetScriptTag(HTMLHelper.EncodeForHtmlAttribute(url), minify, executionMode)));

                        AddToRegisteredClientScripts(url);
                    }
                    else
                    {
                        throw new Exception("[ScriptHelper.RegisterClientScriptFile]: The script was registered too soon, no page form nor header was found.");
                    }
                }

                // Add flag indicating that script is registered
                AddToRegisteredClientScripts(key);
            }
        }


        /// <summary>
        /// Ensures registration of the main CMS script.
        /// </summary>
        /// <param name="page">Page to register</param>
        public static void RegisterCMS(Page page)
        {
            RegisterScriptFile(page, "cms.js");
        }


        /// <summary>
        /// Ensures registration of the resizer script.
        /// </summary>
        /// <param name="page">A page that registers the resizer</param>
        public static void RegisterResizer(Page page)
        {
            RegisterJQuery(page);
            RegisterScriptFile(page, "Controls/resizer.js");
            RegisterStartupScript(page, page.GetType(), "Resizer", "InitResizer();", true);
        }


        /// <summary>
        /// Ensures the registration of the loader module
        /// </summary>
        /// <param name="page">Page</param>
        /// <param name="text">Text to display while loading</param>
        public static void RegisterLoader(Page page, string text = null)
        {
            if (AllowProgressScript)
            {
                RegisterJQuery(page);

                RegisterModule(page, "CMS/Loader", new
                {
                    overlayHtml = GetLoaderOverlayHtml(),
                    loaderHtml = GetLoaderHtml(text)
                });
            }
        }


        /// <summary>
        /// Gets the HTML content of the standard system loader overlay.
        /// </summary>
        /// <param name="overlayClass">CSS class of the loader overlay div</param>
        /// <param name="overlayId">ID of the loader overlay div (default is standard cms-overlayer)</param>
        public static string GetLoaderOverlayHtml(string overlayClass = "overlayer overlayer-general", string overlayId = "cms-overlayer")
        {
            return String.Concat("<div id=\"", overlayId, "\" class=\"", overlayClass, "\"></div>");
        }


        /// <summary>
        /// Gets the HTML content of the standard system loader icon with specified text.
        /// </summary>
        /// <param name="text">Text to display</param>
        /// <param name="loaderClass">CSS class of the loader div</param>
        public static string GetLoaderInlineHtml(string text = null, string loaderClass = null)
        {
            return GetLoaderHtml(text, loaderClass, null, null);
        }


        /// <summary>
        /// Gets the HTML content of the standard system loader.
        /// </summary>
        /// <param name="text">Text to display</param>
        /// <param name="loaderClass">CSS class of the loader div</param>
        /// <param name="loaderId">ID of the loader div (default is standard cms-loader)</param>
        /// <param name="iconSizeClass">Size class of the icon (default is cms-icon-150)</param>
        public static string GetLoaderHtml(string text = null, string loaderClass = "loader loader-general", string loaderId = "cms-loader", string iconSizeClass = "cms-icon-100 loader-icon")
        {
            // If text is null, use the default value
            text = text ?? ResHelper.GetString("general.loading");

            var id = String.IsNullOrEmpty(loaderId) ? "" : " id=\"" + loaderId + "\"";
            var css = String.IsNullOrEmpty(loaderClass) ? "" : " class=\"" + loaderClass + "\"";
            var innerText = String.IsNullOrEmpty(text) ? "" : "<span class=\"loader-text\">" + text + "</span>";

            return String.Concat("<div", id, css, "><i aria-hidden=\"true\" class=\"icon-spinner spinning ", iconSizeClass, "\"></i>", innerText, "</div>");
        }


        /// <summary>
        /// Ensures registration of the save changes notification script.
        /// </summary>
        /// <param name="page">Page to register</param>
        /// <param name="fullFormScope">Indicates whether scope should be used for form (true) or just parts defined by data-tracksavechanges attribute</param>
        public static void RegisterSaveChanges(Page page, bool fullFormScope = true)
        {
            RegisterJQuery(page);
            RegisterScriptFile(page, "savechanges.js");
            RegisterStartupScript(page, typeof(string), "initChanges", @"
with (CMSContentManager) {
    frameset = false;
    allowSubmit = false;
    fullFormScope = " + fullFormScope.ToString().ToLowerInvariant() + @";
    oldSubmit = null;
}
", true);
        }


        /// <summary>
        /// Ensures registration of the spellchecker script.
        /// </summary>
        /// <param name="page">Page to register</param>
        public static void RegisterSpellChecker(Page page)
        {
            RegisterSpellChecker(page, true);
        }


        /// <summary>
        /// Ensures registration of the spellchecker script.
        /// </summary>
        /// <param name="page">Page to register</param>
        /// <param name="ensureDialogScript">Ensures the spell checker utilizes dialog script functionality</param>
        public static void RegisterSpellChecker(Page page, bool ensureDialogScript)
        {
            if (ensureDialogScript)
            {
                RegisterDialogScript(page);
            }
            RegisterScriptFile(page, "~/CMSAdminControls/SpellChecker/spell.js");
        }


        /// <summary>
        /// Registers a control which when invoked via keyboard shortcut saves the document.
        /// </summary>
        /// <typeparam name="T">A control that implements the IPostBackEventHandler interface</typeparam>
        /// <param name="saveControl">A control which when invoked saves the document</param>
        /// <param name="argument">A string of optional arguments to pass to the control that processes the postback</param>
        /// <param name="customScript">Custom java-script to execute</param>
        public static void RegisterSaveShortcut<T>(T saveControl, string argument, string customScript)
            where T : Control, IPostBackEventHandler
        {
            if ((saveControl == null) || (saveControl.Page == null))
            {
                return;
            }

            // Default control click
            string script = saveControl.Page.ClientScript.GetPostBackEventReference(saveControl, argument, false);

            // Extra actions
            if (!String.IsNullOrEmpty(customScript))
            {
                script = String.Concat(customScript, " ", script);
            }

            RegisterSaveShortcut(saveControl.Page, script);
        }


        /// <summary>
        /// Registers a script which when invoked via keyboard shortcut saves the document.
        /// </summary>
        /// <param name="page">A page that registers the shortcut</param>
        /// <param name="script">A script which is run when shortcut is used</param>
        public static void RegisterSaveShortcut(Page page, string script)
        {
            if ((page == null) || (String.IsNullOrEmpty(script)))
            {
                return;
            }

            script = String.Format(
                @"
cmsrequire(['CMS/EventHub'], function (hub) {{
    hub.subscribe('KeyPressed', function(e) {{
        // Handle Ctrl + S
        if (e.ctrlKey && !e.altKey && (e.key == 83)) {{
            e.wasHandled = true;
            {0};
        }}
    }});
}});
",
                script
                );

            RegisterRequireJs(page);
            RegisterClientScriptBlock(page, page.GetType(), "SaveShortcut", script, true);
        }


        /// <summary>
        /// Ensures registration of tool tip script.
        /// </summary>
        /// <param name="page">Page to register</param>
        public static void RegisterTooltip(Page page)
        {
            RegisterStartupScript(page, typeof(string), TOOLTIP_SCRIPT_FILE_KEY, GetScriptTag("ToolTip/wz_tooltip.js"));
        }


        /// <summary>
        /// Ensures registration of bootstrap tooltip script.
        /// </summary>
        /// <param name="page">Page to register</param>
        /// <param name="selector">jQuery selector for element(s) where bootstrap tooltip will be used</param>
        /// <param name="template">Tooltip popup template</param>
        public static void RegisterBootstrapTooltip(Page page, string selector, string template = null)
        {
            if (String.IsNullOrEmpty(selector))
            {
                return;
            }

            RegisterBootstrapScripts(page);

            RegisterModule(page, "CMS/BootstrapTooltip", new
            {
                selector,
                templateSelector = template
            });
        }


        /// <summary>
        /// Ensures registration of MooTools JavaScript library.
        /// </summary>
        /// <param name="page">Page to register</param>
        public static void RegisterMooTools(Page page)
        {
            if (!IsMooToolsRegistered())
            {
                AddToRegisteredClientScripts(MOOTOOLS_KEY);
                RegisterScriptFile(page, MOOTOOLS_FILENAME);
            }
        }


        /// <summary>
        /// Determines whether MooTools library is already registered.
        /// </summary>
        /// <returns>True if MooTools library is already registered</returns>
        public static bool IsMooToolsRegistered()
        {
            return IsClientScriptBlockRegistered(MOOTOOLS_KEY);
        }


        /// <summary>
        /// Ensures registration of Underscore JavaScript library.
        /// </summary>
        /// <param name="page">Page to register</param>
        [Obsolete("Use Javascript module register approach instead.")]
        public static void RegisterUnderscore(Page page)
        {
            if (!IsUnderscoreRegistered())
            {
                AddToRegisteredClientScripts(UNDERSCORE_KEY);
                RegisterScriptFile(page, UNDERSCORE_FILENAME);
            }
        }


        /// <summary>
        /// Determines whether Underscore JavaScript library is already registered.
        /// </summary>
        /// <returns>True if Underscore JavaScript library is already registered</returns>
        public static bool IsUnderscoreRegistered()
        {
            return IsClientScriptBlockRegistered(UNDERSCORE_KEY);
        }


        /// <summary>
        /// Ensures registration of jQuery JavaScript library.
        /// </summary>
        /// <param name="page">Page to register</param>
        public static void RegisterJQuery(Page page)
        {
            if (!IsJQueryRegistered())
            {
                AddToRegisteredClientScripts(JQUERY_KEY);
                RegisterScriptFile(page, JQUERY_FILENAME);

                // Compatibility package
                RegisterScriptFile(page, "jquery/jquery-cmscompatibility.js");
            }
        }


        /// <summary>
        /// Determines whether jQuery library is already registered.
        /// </summary>
        /// <returns>True if jQuery library is already registered</returns>
        public static bool IsJQueryRegistered()
        {
            return IsClientScriptBlockRegistered(JQUERY_KEY);
        }


        /// <summary>
        /// Registers jQuery UI layout library.
        /// </summary>
        /// <param name="page">Page to register</param>
        /// <param name="withStyles">Whether to include default styles</param>
        /// <param name="withCMSStyles">Whether to include default styles or default CMS styles</param>
        /// <param name="withCallbacks">Whether to register layout callbacks</param>
        public static void RegisterJQueryUILayout(Page page, bool withStyles, bool withCMSStyles, bool withCallbacks)
        {
            RegisterJQueryUI(page, false);
            RegisterScriptFile(page, "jquery/jQueryLayout/jquery-layout.js");
            if (withCallbacks)
            {
                RegisterScriptFile(page, "jquery/jQueryLayout/jquery-layout-callbacks.js");
            }
            if (withStyles)
            {
                string cssUrl = withCMSStyles ? "~/App_Themes/Design/UILayout.css" : "~/CMSScripts/jquery/jQueryLayout/jquery-layout-default.css";
                CssRegistration.RegisterCssLink(page, cssUrl);
            }
        }


        /// <summary>
        /// Ensures registration of jQuery UI JavaScript library.
        /// </summary>
        /// <param name="page">Page to register</param>
        /// <param name="withStyles">Whether to register also UI CSS</param>
        public static void RegisterJQueryUI(Page page, bool withStyles = true)
        {
            RegisterJQuery(page);
            RegisterScriptFile(page, "jquery/jqueryui/jquery-ui.js");
            if (withStyles)
            {
                CssRegistration.RegisterCssLink(page, "~/CMSScripts/jquery/jqueryui/jquery-ui.css");
            }
        }


        /// <summary>
        /// Ensures registration of jQuery tools JavaScript library.
        /// </summary>
        /// <param name="page">Page to register</param>
        public static void RegisterJQueryTools(Page page)
        {
            RegisterJQuery(page);
            RegisterScriptFile(page, "jquery/jquery-tools.js");
        }
        

        /// <summary>
        /// Ensures registration of jQuery templates JavaScript library.
        /// </summary>
        /// <param name="page">Page to register</param>
        public static void RegisterJQueryTemplates(Page page)
        {
            RegisterJQuery(page);
            RegisterScriptFile(page, "jquery/jquery-tmpl.js");
        }


        /// <summary>
        /// Ensures registration of jQuery dialog.
        /// </summary>
        /// <param name="page">Page to register</param>
        public static void RegisterJQueryDialog(Page page)
        {
            RegisterJQuery(page);
            RegisterScriptFile(page, "jquery/jquery-dialog.js");
        }


        /// <summary>
        /// Ensures registration of jQuery dropshadow JavaScript library.
        /// </summary>
        /// <param name="page">Page to register</param>
        [Obsolete("Use Javascript module register approach instead.")]
        public static void RegisterJQueryShadow(Page page)
        {
            RegisterJQuery(page);
            RegisterScriptFile(page, "jquery/jquery-dropshadow.js");
        }


        /// <summary>
        /// Ensures registration of jQuery highlighter JavaScript library.
        /// </summary>
        /// <param name="page">Page to register</param>
        public static void RegisterJQueryHighLighter(Page page)
        {
            RegisterJQuery(page);
            RegisterScriptFile(page, "jquery/jquery-highlighter.js");
        }


        /// <summary>
        /// Ensures registration of jQuery crop JavaScript library.
        /// </summary>
        /// <param name="page">Page to register</param>
        public static void RegisterJQueryCrop(Page page)
        {
            RegisterJQuery(page);
            RegisterScriptFile(page, "jquery/jquery-jcrop.js");
        }


        /// <summary>
        /// Ensures registration of jQuery cookie JavaScript library.
        /// </summary>
        /// <param name="page">Page to register</param>
        public static void RegisterJQueryCookie(Page page)
        {
            RegisterJQuery(page);
            RegisterScriptFile(page, "jquery/jquery-cookie.js");
        }


        /// <summary>
        /// Ensures registration of Bootstrap JavaScript library.
        /// </summary>
        /// <param name="page">Page to register.</param>
        public static void RegisterBootstrapScripts(Page page)
        {
            if (!IsJQueryRegistered())
            {
                RegisterJQuery(page);
            }
            if ((!IsClientScriptBlockRegistered(BOOTSTRAP_FILENAME)) || (!IsClientScriptBlockRegistered(BOOTSTRAP_CUSTOM_FILENAME)))
            {
                RegisterScriptFile(page, BOOTSTRAP_FILENAME);
                RegisterScriptFile(page, BOOTSTRAP_CUSTOM_FILENAME);
            }
        }


        /// <summary>
        /// Ensures registration of script which causes correct resizing of uni flat selectors.
        /// </summary>
        /// <param name="page">Page to register</param>
        public static void RegisterFlatResize(Page page)
        {
            // jQuery is required for this script
            RegisterJQuery(page);
            RegisterScriptFile(page, "Controls/FlatResize.js");
        }


        /// <summary>
        /// Ensures registration of jQuery appear JavaScript library.
        /// </summary>
        /// <param name="page">Page to register</param>
        public static void RegisterJQueryAppear(Page page)
        {
            RegisterJQuery(page);
            RegisterScriptFile(page, "jquery/jquery-appear.js");
        }


        /// <summary>
        /// Reload page header and select specified tab
        /// </summary>
        /// <param name="page">Page to register</param>
        /// <param name="newName">New name for direct refresh</param>
        public static void RefreshTabHeader(Page page, String newName = "")
        {
            // For directly set new name refresh breadcrumbs automatically, but only if no tab name is set
            if (!String.IsNullOrEmpty(newName))
            {
                var script = String.Format(
@"
if (parent.refreshBreadcrumbs && parent.document.pageLoaded) 
{{
    parent.refreshBreadcrumbs({0})
}}
",
                    GetString(ResHelper.LocalizeString(newName))
                );

                RegisterStartupScript(page, typeof(string), "RefreshTabHeader", script, true);
            }
        }


        /// <summary>
        /// Registers refresh tree script for documents UI
        /// </summary>
        public static void RefreshTree(Page page, int nodeId, int parentNodeId)
        {
            var script = String.Format(
@"
function RefreshTree(expandNodeId, selectNodeId) {{
	// Update tree
    if(window.self != parent) {{
	    parent.RefreshTree(expandNodeId, selectNodeId);
    }}
}}

RefreshTree({0}, {1});
",
                parentNodeId,
                nodeId
            );

            RegisterClientScriptBlock(page, typeof(string), String.Format("RefreshTree_{0}_{1}", parentNodeId, nodeId), script, true);
        }


        /// <summary>
        /// Registers js script for synchronization scroll bars in split mode.
        /// </summary>
        /// <param name="page">Page</param>
        /// <param name="basePage">Indicates if page is base.</param>
        /// <param name="body">Indicates if event 'scroll' should be bound on document body.</param>
        /// <param name="refresh">Indicates if other frame with the same culture should be refreshed.</param>
        /// <param name="unbind">Indicates if binding elements should be unbind.</param>
        public static void RegisterSplitModeSync(Page page, bool basePage, bool body, bool refresh, bool unbind = false)
        {
            if (page != null)
            {
                StringBuilder syncScrollScript = new StringBuilder();

                syncScrollScript.Append(@"
function SplitModeRefreshFrame() {
	parent.SplitModeRefreshFrame();
}");
                // Register js script for base page
                if (basePage)
                {
                    syncScrollScript.Append(@"
function InitSplitViewSyncScroll(body, refreshSameCulture, unbind) {
	if (parent.InitSplitViewSyncScroll && (parent.window != window)) {
		parent.InitSplitViewSyncScroll(window.frameElement, body, refreshSameCulture, unbind);
	}   
}");

                    RegisterStartupScript(page, typeof(string), "splitViewSyncInit_" + page.ClientID, GetScript(String.Format("InitSplitViewSyncScroll({0},{1},{2});", body.ToString().ToLowerCSafe(), refresh.ToString().ToLowerCSafe(), unbind.ToString().ToLowerCSafe())));
                }
                else
                {
                    syncScrollScript.Append(@"
function InitSplitViewSyncScroll(frameElement, body, refreshSameCulture, unbind) {
	if (parent.InitSplitViewSyncScroll && (parent.window != window)) {
		parent.InitSplitViewSyncScroll(frameElement, body, refreshSameCulture, unbind);
		}   
}");
                }

                RegisterClientScriptBlock(page, typeof(string), "splitViewSync_" + page.ClientID, GetScript(syncScrollScript.ToString()));
            }
        }


        /// <summary>
        /// Registers script for lazy load images.
        /// </summary>
        /// <param name="page">Page to register</param>
        /// <param name="parentClassName">Parent control class name</param>
        /// <param name="imageClassName">Image control class name</param>
        [Obsolete("Use custom implementation instead.")]
        public static void RegisterImageLazyLoad(Page page, string parentClassName, string imageClassName)
        {
            // Register jQuery appear plug-in
            RegisterJQueryAppear(page);

            // Get script
            string script = @"$cmsj('." + parentClassName + "').ready( function(){$cmsj('." + imageClassName + "').appear( function(){if ($cmsj(this).attr('rev') != null){$cmsj(this).attr('src', $cmsj(this).attr('rev'));}})});";

            // Register script
            RegisterClientScriptBlock(page, typeof(string), parentClassName + "_" + imageClassName, GetScript(script));
        }


        /// <summary>
        /// Register Facebook JavaScript SDK 
        /// </summary>
        /// <param name="page">Page to register</param>
        /// <param name="cultureCode">Culture code to use</param>
        /// <param name="apiKey">Facebook application ID</param>
        [Obsolete("Use custom implementation instead.")]
        public static void RegisterFacebookJavascriptSDK(Page page, string cultureCode, string apiKey = null)
        {
            string script = @"
$cmsj(document).ready(function() {
	var fbroot = document.getElementById('fb-root'); 
	if(!fbroot) { 
		fbroot = document.createElement('div'); 
		fbroot.id='fb-root'; 
		document.body.insertBefore(fbroot, document.body.firstChild);
	}

	(function(d, s, id) {
		var js, fjs = d.getElementsByTagName(s)[0];
		if (d.getElementById(id)) return;
		js = d.createElement(s); js.id = id;
		js.src = ""//connect.facebook.net/" + CultureHelper.GetFacebookCulture(cultureCode) + @"/all.js#xfbml=1" + (String.IsNullOrEmpty(apiKey) ? String.Empty : ("&appId=" + apiKey)) + @""";
		fjs.parentNode.insertBefore(js, fjs);
	}(document, 'script', 'facebook-jssdk'));
});

";

            // Register script
            RegisterJQuery(page);
            RegisterStartupScript(page, typeof(string), "FacebookJavascriptSDK", script, true);
        }


        /// <summary>
        /// Registers cmsedit.js script to the page and ensures initialization
        /// </summary>
        /// <param name="page">Page to register</param>
        /// <param name="ensureInit">Indicates whether InitializePage() should be called</param>
        public static void RegisterEditScript(Page page, bool ensureInit = true)
        {
            RegisterScriptFile(page, "cmsedit.js");
            if (ensureInit)
            {
                RegisterStartupScript(page, typeof(string), "InitializePage", GetScript("InitializePage();"));
            }
        }


        /// <summary>
        /// Registers script for shadow below header actions.
        /// </summary>
        /// <param name="page">Page to register</param>
        [Obsolete("Use Javascript module register approach instead.")]
        public static void RegisterHeaderActionsShadowScript(Page page)
        {
            RegisterJQuery(page);
            RegisterScriptFile(page, "Controls/HeaderShadow.js");
        }

        #endregion


        #region "Script registration methods (AJAX ready)"

        /// <summary>
        /// Registers the hidden field to a page
        /// </summary>
        /// <param name="ctrl">Control</param>
        /// <param name="name">Field name</param>
        /// <param name="value">Field value</param>
        public static void RegisterHiddenField(Control ctrl, string name, string value)
        {
            ScriptManager.RegisterHiddenField(ctrl, name, value);
        }


        /// <summary>
        /// Registers the hidden field to a page
        /// </summary>
        /// <param name="page">Page</param>
        /// <param name="name">Field name</param>
        /// <param name="value">Field value</param>
        public static void RegisterHiddenField(Page page, string name, string value)
        {
            ScriptManager.RegisterHiddenField(page, name, value);
        }


        /// <summary>
        /// Ensures registration of the array declaration script.
        /// </summary>
        /// <param name="element">Page element</param>
        /// <param name="arrayName">Name of the array to register</param>
        /// <param name="arrayValue">Value of the array to register</param>
        [Obsolete("Use custom implementation instead.")]
        public static void RegisterArrayDeclaration(PageElement element, string arrayName, string arrayValue)
        {
            if (!String.IsNullOrEmpty(arrayName) && (arrayValue != null))
            {
                if (element.Control != null)
                {
                    ScriptManager.RegisterArrayDeclaration(element.Control, arrayName, arrayValue);
                }
                else
                {
                    ScriptManager.RegisterArrayDeclaration(element.Page, arrayName, arrayValue);
                }
            }
        }


        /// <summary>
        /// Registers a client script block for AJAX and adds the script block to the page, optionally enclosing it in script tags.
        /// </summary>
        /// <param name="control">The control that is registering the client script block</param>
        /// <param name="type">The type of the client script block</param>
        /// <param name="key">A unique identifier for the script block</param>
        /// <param name="script">The script</param>
        /// <param name="addScriptTags">True to enclose the script block in script and /script tags, otherwise false</param>
        public static void RegisterClientScriptBlock(Control control, Type type, string key, string script, bool addScriptTags = false)
        {
            if (addScriptTags)
            {
                script = GetScript(script);
            }

            if (!String.IsNullOrEmpty(script))
            {
                ScriptManager.RegisterClientScriptBlock(control, type, key, script, false);

                // Add to collection of registered scripts
                AddToRegisteredClientScripts(key);
            }
        }


        /// <summary>
        /// Registers a client script block for AJAX and adds the script block to the page, optionally enclosing it in script tags.
        /// </summary>
        /// <param name="page">The page object that is registering the client script block</param>
        /// <param name="type">The type of the client script block</param>
        /// <param name="key">A unique identifier for the script block</param>
        /// <param name="script">The script</param>
        /// <param name="addScriptTags">True to enclose the script block in script and /script tags, otherwise false</param>
        public static void RegisterClientScriptBlock(Page page, Type type, string key, string script, bool addScriptTags = false)
        {
            if (String.IsNullOrEmpty(script) || IsClientScriptBlockRegistered(key))
            {
                return;
            }

            if (addScriptTags)
            {
                script = GetScript(script);
            }

            if (page.Form != null)
            {
                // Register the standard way if Form is present
                ScriptManager.RegisterClientScriptBlock(page, type, key, script, false);
            }
            else
            {
                // Register for PreRender event handler to process later
                page.PreRenderComplete += (s, ea) =>
                {
                    if (page.Form != null)
                    {
                        // Register the standard way if Form is present
                        ScriptManager.RegisterClientScriptBlock(page, type, key, script, false);
                    }
                    else
                    {
                        // Add to the header in case the page doesn't have Form
                        if ((page.Header != null))
                        {
                            page.Header.Controls.Add(new LiteralControl(script));
                        }
                    }
                };
            }

            // Add to collection of registered scripts
            AddToRegisteredClientScripts(key);
        }


        /// <summary>
        /// Registers the client script for AJAX and then adds a script file reference to the page.
        /// Allows to explicitly disable script minification.
        /// </summary>
        /// <param name="element">Page element</param>
        /// <param name="type">The type of the client script block</param>
        /// <param name="key">A unique identifier for the script block</param>
        /// <param name="url">The URL of the script file</param>
        /// <param name="minify">True if minification should be used, otherwise false</param>
        public static void RegisterClientScriptInclude(PageElement element, Type type, string key, string url, bool minify = true)
        {
            if (String.IsNullOrEmpty(url))
            {
                return;
            }

            // Transform and resolve URL
            url = GetScriptUrl(url, minify);

            if (element.Control != null)
            {
                ScriptManager.RegisterClientScriptInclude(element.Control, type, key, url);
            }
            else
            {
                ScriptManager.RegisterClientScriptInclude(element.Page, type, key, url);
            }
        }


        /// <summary>
        /// Registers the startup script.
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="type">Type</param>
        /// <param name="key">Key</param>
        /// <param name="script">Script</param>
        /// <param name="addScriptTags">True to enclose the script block in script and /script tags, otherwise false</param>
        public static void RegisterStartupScript(Control control, Type type, string key, string script, bool addScriptTags = false)
        {
            if (String.IsNullOrEmpty(script))
            {
                return;
            }

            if (addScriptTags)
            {
                script = GetScript(script);
            }

            ScriptManager.RegisterStartupScript(control, type, key, script, false);
        }


        /// <summary>
        /// Registers the startup script.
        /// </summary>
        /// <param name="page">Page</param>
        /// <param name="type">Type</param>
        /// <param name="key">Key</param>
        /// <param name="script">Script</param>
        /// <param name="addScriptTags">True to enclose the script block in script and /script tags, otherwise false</param>
        public static void RegisterStartupScript(Page page, Type type, string key, string script, bool addScriptTags = false)
        {
            if (String.IsNullOrEmpty(script) || IsStartupScriptRegistered(key))
            {
                return;
            }

            if (addScriptTags)
            {
                script = GetScript(script);
            }

            if (page.Form != null)
            {
                // Register the standard way if Form is present
                ScriptManager.RegisterStartupScript(page, type, key, script, false);
            }
            else
            {
                // Register for PreRender event handler to process later
                page.PreRenderComplete += (s, ea) =>
                {
                    if (page.Form != null)
                    {
                        // Register the standard way if Form is present
                        ScriptManager.RegisterStartupScript(page, type, key, script, false);
                    }
                    else
                    {
                        // Add to the header in case the page doesn't have Form
                        if (page.Header != null)
                        {
                            page.Header.Controls.AddAt(page.Header.Controls.Count, new LiteralControl(script));
                        }
                    }
                };
            }

            AddToRegisteredStartupScripts(key);
        }


        /// <summary>
        /// Registers the form submit statement.
        /// </summary>
        /// <param name="element">Page element</param>
        /// <param name="type">Type</param>
        /// <param name="key">Key</param>
        /// <param name="script">Script</param>
        public static void RegisterOnSubmitStatement(PageElement element, Type type, string key, string script)
        {
            if (!String.IsNullOrEmpty(script))
            {
                if (element.Control != null)
                {
                    ScriptManager.RegisterOnSubmitStatement(element.Control, type, key, script);
                }
                else
                {
                    ScriptManager.RegisterOnSubmitStatement(element.Page, type, key, script);
                }
            }
        }


        /// <summary>
        /// Requests particular script registration. Returns true if the registration is allowed
        /// </summary>
        /// <param name="key">Script key to request</param>
        public static bool RequestScriptRegistration(string key)
        {
            if (!IsClientScriptBlockRegistered(key))
            {
                AddToRegisteredClientScripts(key);
                return true;
            }

            return false;
        }


        /// <summary>
        /// Returns whether client script with given key is already registered.
        /// </summary>
        /// <param name="key">Key which identifies the script</param>
        /// <returns>TRUE if script is already registered</returns>
        public static bool IsClientScriptBlockRegistered(string key)
        {
            return (WebLanguagesContext.CurrentClientScriptBlocks[key] != null);
        }


        /// <summary>
        /// Adds key identifying script to collection of registered scripts.
        /// </summary>
        /// <param name="key">Key which identifies the script</param>
        public static void AddToRegisteredClientScripts(string key)
        {
            WebLanguagesContext.CurrentClientScriptBlocks[key] = true;
        }


        /// <summary>
        /// Returns whether startup script with given key is already registered.
        /// </summary>
        /// <param name="key">Key which identifies the script</param>
        /// <returns>TRUE if script is already registered</returns>
        public static bool IsStartupScriptRegistered(string key)
        {
            return (WebLanguagesContext.CurrentStartupScripts[key] != null);
        }


        /// <summary>
        /// Adds key identifying script to collection of registered startup scripts.
        /// </summary>
        /// <param name="key">Key which identifies the script</param>
        public static void AddToRegisteredStartupScripts(string key)
        {
            WebLanguagesContext.CurrentStartupScripts[key] = true;
        }


        /// <summary>
        /// Gets a URL to script file which may have been specified by name only.
        /// </summary>
        /// <param name="url">URL or name of the script file</param>
        /// <returns>URL to the script file</returns>
        private static string GetScriptFileUrl(string url)
        {
            // Scripts which are referenced by filename only are rooted in CMSScripts directory
            if (!ValidationHelper.IsURL(url))
            {
                url = "~/CMSScripts/" + url;
            }

            return url;
        }

        #endregion


        #region "JavaScript modules"

        /// <summary>
        /// Renders JavaScript code to start a client-side module.
        /// </summary>
        /// <remarks>
        /// Multiple instances of the same client-side module can be registered.
        /// The parameters object contains name/value pairs. Regular, anonymous and dictionary based classes are supported.
        /// </remarks>
        /// <param name="control">The control that will be used to register JavaScript code.</param>
        /// <param name="moduleId">The full name of the client-side module to register.</param>
        /// <param name="parameters">An object that contains the named parameters to set for the module.</param>
        public static void RegisterModule(Control control, string moduleId, object parameters = null)
        {
            string jsonParameters = SerializeModuleParametersToJson(parameters) ?? String.Empty;
            string scriptKey = Guid.NewGuid().ToString("N");
            string script = String.Format(@"cmsrequire(['{0}'], function(module) {{ new module({1}); }});", moduleId, jsonParameters);

            RegisterRequireJs(control.Page);

            RegisterStartupScript(control.Page, typeof(string), scriptKey, script, true);
        }


        /// <summary>
        /// Registers Angular module which will be rendered to start a client-side angular module.
        /// </summary>
        /// <param name="moduleID">The full name of the client-side module to register.</param>
        /// <param name="parameters">An object that contains the named parameters to set for the module.</param>
        public static void RegisterAngularModule(string moduleID, object parameters = null)
        {
            if (string.IsNullOrEmpty(moduleID))
            {
                throw new ArgumentException("[ScriptHelper.RegisterAngularModule]: Module ID has to be defined.", "moduleID");
            }

            AngularContext.RegisterAngularModule(moduleID, parameters);
        }


        /// <summary>
        /// Renders JavaScript code to start a client-side angular modules.
        /// </summary>
        /// <param name="page">The page that the modules will be started on.</param>
        public static void RenderAngularModulesScript(Page page)
        {
            if (page == null)
            {
                throw new ArgumentNullException("page");
            }

            var modules = AngularContext.RegisteredModules;

            // No registered modules, do not render
            if ((modules == null) || (modules.Count == 0))
            {
                return;
            }

            var modulesNames = String.Format("'{0}'", modules.Keys.Join("', '"));
            var modulesData = SerializeModuleParametersToJson(modules.Values);
            var modulesArguments = modules.Select((module, index) => String.Format("m{0}", index)).Join(", ");

            var script = String.Format(
@"cmsrequire(['angular', {0}], function(angular, {2}) {{
    var results = [],
        modulesData = {1};

    angular.forEach([{2}], function(moduleName, index){{
        if(angular.isFunction(moduleName)){{
            results.push(moduleName(modulesData[index]));
        }}
        else{{
            results.push(moduleName);
        }}
    }});

    angular.bootstrap(window.document, results);
}});", modulesNames, modulesData, modulesArguments);


            RegisterRequireJs(page);
            RegisterClientScriptBlock(page, typeof(string), ANGULAR_MODULES_KEY, script, true);
        }


        /// <summary>
        /// Serializes the specified object to a JSON string using default formatting
        /// </summary>
        /// <param name="value">The object to serialize.</param>
        /// <remarks>
        /// We have to use this method because of breaking change in code (Newtonsoft.Json), causing runtime error due to not-accessible methods
        /// </remarks>
        /// <returns>
        /// A JSON string representation of the object.
        /// </returns>
        public static string JsonSerializeObject(object value)
        {
            if (value == null)
            {
                return null;
            }

            return mJsonSerializer.Value.SerializeObject(value);
        }


        /// <summary>
        /// Converts an object with client-side module parameters to a JSON string.
        /// </summary>
        /// <param name="parameters">The object to serialize.</param>
        /// <returns>The object serialized to a JSON string, if the object is not null; otherwise, null.</returns>
        public static string SerializeModuleParametersToJson(object parameters)
        {
            return JsonSerializeObject(parameters);
        }


        /// <summary>
        /// Registers the require.js client script with the Page object.
        /// </summary>
        /// <param name="page">The page to register the client script.</param>
        public static void RegisterRequireJs(Page page)
        {
            RegisterScriptFile(page, "RequireJS/CMSConfigRequire.js");
            RegisterScriptFile(page, "RequireJS/require.js");

            RegisterRequireJsConfiguration(page);
        }


        /// <summary>
        /// Registers the require.js configuration client script with the Page object.
        /// </summary>
        /// <param name="page">The page to register the client script.</param>
        private static void RegisterRequireJsConfiguration(Page page)
        {
            RegisterScriptFile(page, String.Format("~/CMSPages/GetResource.ashx?scriptfile={0}&resolvemacros=1", HttpUtility.UrlEncode("~/CMSScripts/RequireJS/config.js")), false);
        }


        /// <summary>
        /// Registers a client script, that adds the client application state to the page, with the Page object.
        /// </summary>
        /// <param name="page">The page to register the client script.</param>
        public static void RegisterClientApplication(Page page)
        {
            const string scriptKey = "ClientApplication";
            if ((page == null) || IsClientScriptBlockRegistered(scriptKey))
            {
                return;
            }

            string clientApplicationJson = SerializeModuleParametersToJson(RequestContext.ClientApplication) ?? "{}";
            string script = String.Format(
@"
var CMS = CMS || {{}};
CMS.Application = {0};
"
                , clientApplicationJson);

            RegisterClientScriptBlock(page, typeof(string), scriptKey, script, true);
        }


        /// <summary>
        /// Registers the script which identifies that page was loaded.
        /// </summary>
        /// <param name="page">Page to register</param>
        public static void RegisterPageLoadedFlag(Page page)
        {
            // Set the loaded flag on page load
            RegisterStartupScript(page, page.GetType(), "PageLoadedFlag", GetScript("document.pageLoaded = true;"));
        }


        /// <summary>
        /// Registers a client script, that rises the PageLoaded event, with the Page object.
        /// </summary>
        /// <param name="page">The page to register the client script.</param>
        public static void RegisterPageLoadedEvent(Page page)
        {
            const string scriptKey = "PageLoadedEvent";
            if (page == null || IsStartupScriptRegistered(scriptKey))
            {
                return;
            }

            const string script =
@"
cmsrequire(['CMS/EventHub', 'CMS/Application'], function (hub, app) {
    hub.publish('PageLoaded', app.getData(null, window));
});
";
            RegisterPageLoadedFlag(page);
            RegisterRequireJs(page);
            RegisterStartupScript(page, typeof(string), scriptKey, script, true);
        }


        /// <summary>
        /// Hides the parent vertical tabs
        /// </summary>
        public static void HideVerticalTabs(Page page)
        {
            if (!RequestHelper.IsAsyncPostback())
            {
                // Register scripts
                RequestContext.ClientApplication.Add("hideVerticalTabs", true);

                RegisterRequireJs(page);
                RegisterClientScriptBlock(page, typeof(string), "HideVerticalTabs",
                                          @"
cmsrequire(['CMS/EventHub', 'CMS/Application'], function (hub, app) {
    var level = app.getWindowLevel(window);
    
    hub.publish('Tabs_Hide_' + (level - 1), { partial: true });
});
"
                    , true);
            }
        }


        /// <summary>
        /// Registers a client script, which is responsible for loading custom web components.
        /// </summary>
        /// <param name="page">The page to register the client script to.</param>
        public static void RegisterWebComponentsScript(Page page)
        {
#pragma warning disable BH1013 // 'ClientScript methods' should not be used.
            page.ClientScript.RegisterClientScriptInclude(COMPONENTS_KEY, URLHelper.ResolveUrl("~/CMSScripts/WebComponents/components.js"));
#pragma warning restore BH1013 // 'ClientScript methods' should not be used.
        }

        #endregion
    }
}