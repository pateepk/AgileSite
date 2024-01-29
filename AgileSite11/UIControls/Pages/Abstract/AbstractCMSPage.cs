using System;
using System.Collections.Generic;
using System.Threading;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using AjaxControlToolkit;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.DocumentEngine.Web.UI;
using CMS.EventLog;
using CMS.Base.Web.UI.ActionsConfig;
using CMS.FormEngine.Web.UI;
using CMS.Helpers;
using CMS.IO;
using CMS.Localization;
using CMS.Membership;
using CMS.Modules;
using CMS.OutputFilter;
using CMS.PortalEngine;
using CMS.PortalEngine.Web.UI;
using CMS.SiteProvider;

using MessageTypeEnum = CMS.Base.Web.UI.MessageTypeEnum;

namespace CMS.UIControls
{
    /// <summary>
    /// Base page for all the CMS pages.
    /// </summary>
    public abstract class AbstractCMSPage : Page, ICMSPage
    {
        #region "Variables"

        private TreeProvider mTree;
        private bool mUseViewStateUserKey = true;

        private static bool? mUseXUACompatible;
        private static string mXUACompatible;
        private bool mCurrentUseXUACompatible = true;
        private MessagesPlaceHolder mMessagesPlaceHolder;

        // Container control for the page managers.
        private Control mManagersContainer;

        // Container control for the context menus.
        private Control mContextMenuContainer;

        /// Container control for the page footers.
        private Control mFootersContainer;

        /// Container control for the log controls.
        private Control mLogsContainer;

        // Header actions
        private HeaderActions mHeaderActions;

        // Script manager control.
        private ScriptManager mScriptManagerControl;

        // Document manager control.
        private ICMSDocumentManager mDocumentManager;

        // List of attributes to post process
        private List<ICMSAttribute> mPostProcessAttributes;

        // Blank page URL
        private static string mBlankPageUrl;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets the value that indicates whether global hotkey script should be registered
        /// </summary>
        protected bool RegisterGlobalHotKeyScript
        {
            get;
            set;
        }


        /// <summary>
        /// Page's UI context
        /// </summary>
        public virtual UIContext UIContext
        {
            get
            {
                return UIContext.Current;
            }
        }


        /// <summary>
        /// Returns the URL of a blank page
        /// </summary>
        public static string BlankPageUrl
        {
            get
            {
                return mBlankPageUrl ?? (mBlankPageUrl = SystemContext.ApplicationPath.TrimEnd('/') + "/CMSPages/blank.htm");
            }
        }


        /// <summary>
        /// Container control for the page managers.
        /// </summary>
        public virtual Control ManagersContainer
        {
            get
            {
                return mManagersContainer;
            }
            set
            {
                mManagersContainer = value;
            }
        }


        /// <summary>
        /// Container control for the context menus.
        /// </summary>
        public virtual Control ContextMenuContainer
        {
            get
            {
                if (mContextMenuContainer == null)
                {
                    // Try to find special control
                    ContextMenuPlaceHolder container = (ContextMenuPlaceHolder)ControlsHelper.GetChildControl(this, typeof(ContextMenuPlaceHolder));
                    if (container != null)
                    {
                        mContextMenuContainer = container.InnerControl;
                    }

                    return mContextMenuContainer ?? (mContextMenuContainer = ManagersContainer);
                }

                return mContextMenuContainer;
            }
            set
            {
                mContextMenuContainer = value;
            }
        }


        /// <summary>
        /// Container control for the page footers.
        /// </summary>
        public virtual Control FooterContainer
        {
            get
            {
                return mFootersContainer ?? (mFootersContainer = ControlsHelper.GetLastChildControl(Page, typeof(PlaceHolder)));
            }
            set
            {
                mFootersContainer = value;
            }
        }


        /// <summary>
        /// Container control for the log controls.
        /// </summary>
        public virtual Control LogsContainer
        {
            get
            {
                // Try to use form as the logs container
                return mLogsContainer ?? (mLogsContainer = (Control)Page.Form ?? this);
            }
            set
            {
                mLogsContainer = value;
            }
        }


        /// <summary>
        /// Script manager control.
        /// </summary>
        public virtual ScriptManager ScriptManagerControl
        {
            get
            {
                // Get current script manager if not defined
                return mScriptManagerControl ?? (mScriptManagerControl = ScriptManager.GetCurrent(this));
            }
            set
            {
                mScriptManagerControl = value;
            }
        }


        /// <summary>
        /// Gets node ID of current document initialized from query string.
        /// </summary>
        public virtual int NodeID
        {
            get
            {
                return QueryHelper.GetInteger("nodeid", 0);
            }
        }


        /// <summary>
        /// Gets document ID of current document initialized from query string.
        /// </summary>
        public virtual int DocumentID
        {
            get
            {
                return QueryHelper.GetInteger("documentid", 0);
            }
        }


        /// <summary>
        /// Gets culture code of current document initialized from query string.
        /// </summary>
        public virtual string CultureCode
        {
            get
            {
                return QueryHelper.GetString("culture", null);
            }
        }


        /// <summary>
        /// Tree provider object.
        /// </summary>
        public TreeProvider Tree
        {
            get
            {
                return mTree ?? (mTree = new TreeProvider(MembershipContext.AuthenticatedUser));
            }
        }


        /// <summary>
        /// Document manager control.
        /// </summary>
        public virtual ICMSDocumentManager DocumentManager
        {
            get
            {
                return mDocumentManager ?? (mDocumentManager = CreateDocumentManager());
            }
        }


        /// <summary>
        /// If true, ViewStateUserKey is used.
        /// </summary>
        public virtual bool UseViewStateUserKey
        {
            get
            {
                return mUseViewStateUserKey;
            }
            set
            {
                mUseViewStateUserKey = value;
            }
        }


        /// <summary>
        /// Indicates if content preferred culture should be ensured.
        /// </summary>
        public virtual bool EnsurePreferredCulture
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if the page is in tab mode.
        /// </summary>
        public virtual bool TabMode
        {
            get
            {
                return QueryHelper.GetBoolean("tabmode", false);
            }
        }


        /// <summary>
        /// Determines whether the current page is located under the CMS Desk.
        /// </summary>
        public virtual bool IsCMSDesk
        {
            get
            {
                return this is CMSDeskPage;
            }
        }


        /// <summary>
        /// If true, the page is a UI page
        /// </summary>
        protected bool IsStandardPage
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if document manager should be ensured
        /// </summary>
        protected bool EnsureDocumentManager
        {
            get;
            set;
        }


        /// <summary>
        /// Globally enables or disables addition of X-UA-Compatible header to the page.
        /// </summary>
        public static bool UseXUACompatible
        {
            get
            {
                if (mUseXUACompatible == null)
                {
                    mUseXUACompatible = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSUseXUACompatible"], true);
                }

                return mUseXUACompatible.Value;
            }
            set
            {
                mUseXUACompatible = value;
            }
        }


        /// <summary>
        /// Defines content of X-UA-Compatible header that will be rendered to the page.
        /// </summary>
        public static string XUACompatibleValue
        {
            get
            {
                return mXUACompatible ?? (mXUACompatible = ValidationHelper.GetString(SettingsHelper.AppSettings["CMSXUACompatibleValue"], "IE=Edge"));
            }
            set
            {
                mXUACompatible = value;
            }
        }


        /// <summary>
        /// If true, X-UA-Compatible header will be added to the page.
        /// </summary>
        public bool CurrentUseXUACompatible
        {
            get
            {
                return UseXUACompatible && mCurrentUseXUACompatible;
            }
            set
            {
                mCurrentUseXUACompatible = value;
            }
        }


        /// <summary>
        /// Local page messages placeholder
        /// </summary>
        public virtual MessagesPlaceHolder MessagesPlaceHolder
        {
            get
            {
                if (mMessagesPlaceHolder == null)
                {
                    ICMSMasterPage master = Master as ICMSMasterPage;
                    if (master != null)
                    {
                        return master.MessagesPlaceHolder;
                    }
                }

                return mMessagesPlaceHolder;
            }
            set
            {
                mMessagesPlaceHolder = value;
            }
        }


        /// <summary>
        /// Gets placeholder located after form element.
        /// </summary>
        public virtual PlaceHolder AfterFormPlaceHolder
        {
            get
            {
                ICMSMasterPage master = Master as ICMSMasterPage;
                if (master != null)
                {
                    return master.AfterFormPlaceholder;
                }

                return null;
            }
        }


        /// <summary>
        /// Local header actions
        /// </summary>
        public virtual HeaderActions HeaderActions
        {
            get
            {
                if (mHeaderActions == null)
                {
                    ICMSMasterPage master = Master as ICMSMasterPage;
                    if (master != null)
                    {
                        mHeaderActions = master.HeaderActions;
                    }
                }

                return mHeaderActions;
            }
            set
            {
                mHeaderActions = value;
            }
        }


        /// <summary>
        /// Indicates whether page is a dialog.
        /// False by default.
        /// </summary>
        public bool IsDialog
        {
            get
            {
                return UIContext.IsDialog;
            }
            set
            {
                UIContext.IsDialog = value;
            }
        }


        /// <summary>
        /// Indicates whether page is root dialog (top dialog page with header and footer)
        /// </summary>
        public bool IsRootDialog
        {
            get
            {
                return UIContext.IsRootDialog;
            }
            set
            {
                UIContext.IsRootDialog = value;
            }
        }


        /// <summary>
        /// Tells you whether context help script generation into page is enabled.
        /// </summary>
        protected virtual bool EnableContextHelp
        {
            get
            {
                return ((PortalContext.ViewMode == ViewModeEnum.UI) || (PortalContext.ViewMode == ViewModeEnum.Design) || (this is IAdminPage));
            }
        }

        #endregion


        #region "Image URLs methods"

        /// <summary>
        /// Gets UI image URL.
        /// </summary>
        /// <param name="imagePath">Partial image path starting from ~/App_Themes/(Skin_Folder)/Images/ (e.g. '/CMSModules/Membership/module.png')</param>
        /// <param name="isLiveSite">Indicates if URL should be returned for live site</param>
        /// <param name="ensureDefaultTheme">Indicates if default theme should be ensured</param>
        public string GetImageUrl(string imagePath, bool isLiveSite = false, bool ensureDefaultTheme = false)
        {
            return UIHelper.GetImageUrl(Page, imagePath, isLiveSite, ensureDefaultTheme);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        protected AbstractCMSPage()
        {
            IsStandardPage = true;
            RegisterGlobalHotKeyScript = true;
        }


        /// <summary>
        /// Creates a document manager for this page
        /// </summary>
        private CMSDocumentManager CreateDocumentManager()
        {
            return new CMSDocumentManager
            {
                ID = "docMan",
                ShortID = "DM",
                IsLiveSite = false,
                NodeID = QueryHelper.GetInteger("nodeid", 0),
                CultureCode = QueryHelper.GetString("culture", null),
                DocumentID = QueryHelper.GetInteger("documentid", 0),
                Tree = Tree,
                StopProcessing = true
            };
        }

        #endregion


        #region "Page events"

        /// <summary>
        /// PreInit event handler.
        /// </summary>
        protected override void OnPreInit(EventArgs e)
        {
            if (IsStandardPage)
            {
                PageContext.CurrentPage = this;
            }

            base.OnPreInit(e);

            if (IsStandardPage)
            {
                // Process the page attributes
                AttributesHelper.ProcessAttributes(this, ref mPostProcessAttributes);
            }

            // Init dialog properties (can be set from inherited pages)
            if (QueryHelper.GetBoolean("dialog", false) || QueryHelper.GetBoolean("isindialog", false))
            {
                IsDialog = true;
            }
            if (QueryHelper.GetBoolean("dialog", false) && !QueryHelper.GetBoolean("isindialog", false))
            {
                IsRootDialog = true;
            }
        }


        /// <summary>
        /// Page initialization.
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            if (IsStandardPage && EnsureDocumentManager)
            {
                EnsureDocumentManagerInternal();
            }

            base.OnInit(e);
        }


        /// <summary>
        /// Page PreRender
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            if (IsStandardPage)
            {
                // Process the page attributes
                AttributesHelper.PostProcessAttributes(this, mPostProcessAttributes);

                if (CurrentUseXUACompatible)
                {
                    Response.AppendHeader("X-UA-Compatible", XUACompatibleValue);
                }

                // Render preferred culture hidden field to ensure culture
                if (EnsurePreferredCulture && DatabaseHelper.IsDatabaseAvailable && !RequestHelper.IsAsyncPostback())
                {
                    ScriptHelper.RegisterClientScriptBlock(this, typeof(string), "CMSLanguage", "<input type=\"hidden\" name=\"lng\" id=\"lng\" value=\"" + HTMLHelper.HTMLEncode(LocalizationContext.PreferredCultureCode) + "\" />");
                }

                if (!ViewStateDebug.Settings.Live && ViewStateDebug.DebugCurrentRequest)
                {
                    // If the ViewState debug is not displayed on live site, we need to save the information manually
                    if (PortalContext.ViewMode.IsLiveSite())
                    {
                        ViewStateDebug.GetViewStates(Page);
                    }
                }

                // Register Bootstrap JavaScript files
                PortalScriptHelper.RegisterBootstrapScript(PortalContext.ViewMode, Page);
            }

            SetFormAction();

            // Load context help
            LoadContextHelp();

            base.OnPreRender(e);

            if (IsStandardPage && (Form == null))
            {
                RegisterClientApplication();
            }
        }


        /// <summary>
        /// Raises the PreRenderComplete event after the OnPreRenderComplete event and before the page is rendered.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data</param>
        protected override void OnPreRenderComplete(EventArgs e)
        {
            base.OnPreRenderComplete(e);

            if (IsStandardPage)
            {
                ModifyLinksToThemeCSSFiles();

                if (Form != null)
                {
                    RegisterClientApplication();
                }
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Sets the form action for current page if was rewritten by URL rewriter
        /// </summary>
        protected virtual void SetFormAction()
        {
            if (OutputFilterContext.ApplyOutputFilter)
            {
                string relativePath = ResponseOutputFilter.GetFilterRelativePath();
                string siteName = SiteContext.CurrentSiteName;

                // Repair action for URL rewriting
                if (!URLHelper.IsExcluded(relativePath, ResponseOutputFilter.GetExcludedFormFilterURLs(siteName)))
                {
                    // Change the URL
                    string currentUrl = URLHelper.UnResolveUrl(RequestContext.CurrentURL, SystemContext.ApplicationPath);
                    Form.Action = URLHelper.UrlEncodeQueryString(HTMLHelper.GetFormAction(currentUrl));
                }
            }
        }


        /// <summary>
        /// Ensures the script manager on the page.
        /// </summary>
        public virtual ScriptManager EnsureScriptManager()
        {
            EnsureChildControls();

            ScriptManager manager = ScriptManagerControl;
            if (manager == null)
            {
                manager = ScriptManager.GetCurrent(this);
                ScriptManagerControl = manager;
            }
            if (manager == null)
            {
                // Get predefined managers container
                Control container = ManagersContainer;
                if (container != null)
                {
                    // Create new script manager
                    manager = new ScriptManager();
                    manager.ID = "manScript";
                    manager.ScriptMode = ScriptMode.Release;

                    container.Controls.Add(manager);
                }
            }
            else
            {
                manager.ScriptMode = ScriptMode.Release;
            }

            return manager;
        }


        /// <summary>
        /// Gets control ideal for containing document manager.
        /// </summary>
        /// <returns>Control that should contain document manager</returns>
        protected virtual Control GetDocumentManagerContainer()
        {
            Control container;

            // Use managers container
            if (ManagersContainer != null)
            {
                container = ManagersContainer;
            }
            else
            {
                // Try to find placeholder
                container = ControlsHelper.GetChildControl(Page, typeof(PlaceHolder)) ?? Form;
            }

            return container;
        }


        /// <summary>
        /// Ensures the document manager on the page.
        /// </summary>
        private void EnsureDocumentManagerInternal()
        {
            // If document manager doesn't have local messages placeholder
            if (!DocumentManager.HasLocalMessagesPlaceHolder())
            {
                // Use master page placeholder if present
                ICMSMasterPage master = Master as ICMSMasterPage;
                if ((master != null) && (master.PlaceholderLabels != null))
                {
                    DocumentManager.LocalMessagesPlaceHolder = MessagesPlaceHolder;
                }
            }

            // Property is not overridden
            if (mDocumentManager != null)
            {
                mDocumentManager.StopProcessing = false;

                // Add to controls collection
                Control container = GetDocumentManagerContainer();
                if (container != null)
                {
                    // Encapsulate in update panel if script manager is registered
                    ScriptManager sManager = ScriptManager.GetCurrent(this);
                    if (sManager != null)
                    {
                        // Update panel
                        CMSUpdatePanel pnlInfo = new CMSUpdatePanel { ID = "pI" };
                        container.Controls.Add(pnlInfo);
                        container = pnlInfo.ContentTemplateContainer;
                    }

                    container.Controls.AddAt(0, (CMSDocumentManager)mDocumentManager);
                }
            }
        }


        /// <summary>
        /// Registers the client application data
        /// </summary>
        private void RegisterClientApplication()
        {
            // Ensure data for client application
            EnsureClientApplicationData();

            // Ensure client application
            ScriptHelper.RegisterClientApplication(Page);
        }


        /// <summary>
        /// Ensures client application data.
        /// </summary>
        private void EnsureClientApplicationData()
        {
            var app = RequestContext.ClientApplication;
            var currentThreadCulture = Thread.CurrentThread.CurrentUICulture;

            app.Add("applicationUrl", Page.ResolveUrl("~/"));
            app.Add("imagesUrl", UIHelper.GetImageUrl(Page, "/", false, true));
            app.Add("isRTL", currentThreadCulture.TextInfo.IsRightToLeft ? "true" : "false");
            app.Add("isDialog", IsRootDialog);
            app.Add("isDebuggingEnabled", CMSHttpContext.Current.IsDebuggingEnabled);
            app.Add("language", currentThreadCulture.TwoLetterISOLanguageName);

            // Application name to refresh breadcrumbs with current application
            var element = UIContext.UIElement;
            if ((element != null) && (element.Application != null))
            {
                app.Add("applicationName", HTMLHelper.HTMLEncode(UIElementInfoProvider.GetElementCaption(element.Application)));
            }

            // Include currently edited object to provide information for breadcrumbs suffix (Only if wasn't provided yet)
            if (!app.ContainsKey("breadcrumbsSuffix"))
            {
                var uiSuffix = UIContextHelper.GetElementBreadcrumbsSuffix(element);
                // Get edited object type
                var obj = UIContext.EditedObject as BaseInfo;
                if (obj != null)
                {
                    var ti = obj.TypeInfo;

                    var levelColumn = ti.ObjectLevelColumn;
                    var isRoot = (levelColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN) && (obj.GetIntegerValue(levelColumn, 0) == 0);

                    // Make sure when creating a new object, type is not propagated
                    if ((uiSuffix == null) && !isRoot)
                    {
                        var objectType = ti.ObjectType;
                        // Ensure generic object type for documents
                        if (DocumentHelper.IsDocumentObjectType(objectType))
                        {
                            objectType = ti.OriginalObjectType;
                        }

                        uiSuffix = obj.Generalized.ObjectID > 0 ? TypeHelper.GetNiceObjectTypeName(objectType) : "";
                    }

                    if (uiSuffix != null)
                    {
                        app.Add("breadcrumbsSuffix", HTMLHelper.HTMLEncode(uiSuffix));
                    }
                }
            }
        }


        /// <summary>
        /// Adds specified action to the page header actions.
        /// </summary>
        /// <param name="action">Header action</param>
        public void AddHeaderAction(HeaderAction action)
        {
            HeaderActions actions = HeaderActions;
            if (actions != null)
            {
                actions.AddAction(action);
            }
            else
            {
                throw new Exception("[AbstractCMSPage.SetAction]: The page does not contain HeaderActions control where the action can be added.");
            }
        }


        /// <summary>
        /// Adds the list of header actions to the page header actions.
        /// </summary>
        /// <param name="actions">Header actions</param>
        public void AddHeaderActions(params HeaderAction[] actions)
        {
            foreach (var action in actions)
            {
                AddHeaderAction(action);
            }
        }


        /// <summary>
        /// Ensures the page footer control.
        /// </summary>
        public virtual void EnsureFootersControl()
        {
            EnsureChildControls();

            if (FooterContainer != null)
            {
                FooterContainer.Controls.Clear();
                FooterContainer.Controls.Add(new LiteralControl("<div id=\"CMSFooter\"><div id=\"CKToolbar\" ></div></div>"));
            }
        }


        /// <summary>
        /// Initializes the debug controls.
        /// </summary>
        public void InitDebug()
        {
            if (LogsContainer != null)
            {
                // Include debug if enabled
                if (DebugHelper.AnyLiveDebugEnabled && DebugHelper.ShowDebugOnLiveSite)
                {
                    Control debug = LoadUserControl("~/CMSAdminControls/Debug/Debug.ascx");
                    debug.ID = "dbg";

                    LogsContainer.Controls.Add(debug);
                }
            }
        }


        /// <summary>
        /// Loads the user control based on the given path
        /// </summary>
        /// <param name="controlPath">Control path</param>
        public Control LoadUserControl(string controlPath)
        {
            VirtualPathLog.Log(controlPath);
            return LoadControl(controlPath);
        }


        /// <summary>
        /// Modifies links to CSS files contained in themes to use minification.
        /// </summary>
        private void ModifyLinksToThemeCSSFiles()
        {
            // Modify the links to theme CSS files that were auto-loaded by runtime to use minification
            // Make sure that DB is ready (connection and settings) and avoid duplicate link generation on postback
            // The order of conditions is important
            if (CssLinkHelper.MinifyCurrentRequest &&
                (Page.Header != null) &&
                DatabaseHelper.IsDatabaseAvailable)
            {
                foreach (Control control in Page.Header.Controls)
                {
                    HtmlLink link = control as HtmlLink;
                    if (link != null)
                    {
                        // Try to modify only links which contain CSS style sheets
                        bool isCSS = link.Href.EndsWithCSafe(".css", true);

                        if (!isCSS)
                        {
                            // Link with the attribute 'type="text/css"' is also a style sheet
                            string typeAttr = link.Attributes["type"] ?? string.Empty;
                            isCSS |= typeAttr.EqualsCSafe("text/css", true);
                        }

                        if (isCSS)
                        {
                            // Use "GetResource.ashx" or "GetCSS.aspx" for the style sheet link
                            link.Href = UrlResolver.ResolveUrl(CssLinkHelper.GetPhysicalCssUrl(link.Href));
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Resolves the given URL
        /// </summary>
        /// <param name="url">URL to resolve</param>
        public new string ResolveUrl(string url)
        {
            return UrlResolver.ResolveUrl(url);
        }


        /// <summary>
        /// Registers script that modified body element. (For IE7,8 compatibility reasons. See KB927917 for more information.)
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="key">Key</param>
        /// <param name="script">Script</param>
        /// <param name="addScriptTags">True to enclose the script block in script and /script tags, otherwise false</param>
        public void RegisterBodyModifyingScript(Type type, string key, string script, bool addScriptTags)
        {
            if (addScriptTags)
            {
                script = ScriptHelper.GetScript(script);
            }
            if (AfterFormPlaceHolder != null)
            {
                AfterFormPlaceHolder.Controls.Add(new LiteralControl(script));
            }
            else
            {
                ScriptHelper.RegisterStartupScript(this, type, key, script);
            }
        }

        #endregion


        #region "Eval methods"

        /// <summary>
        /// Templated Eval, returns the value converted to specific type.
        /// </summary>
        /// <typeparam name="ReturnType">Result type</typeparam>
        /// <param name="columnName">Column name</param>
        public virtual ReturnType Eval<ReturnType>(string columnName)
        {
            return ValidationHelper.GetValue<ReturnType>(Eval(columnName));
        }


        /// <summary>
        /// Evaluates the item data (safe version), with html encoding.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="encode">If true the text is encoded</param>
        public virtual object Eval(string columnName, bool encode)
        {
            object value = Eval(columnName);
            if (encode)
            {
                value = HTMLHelper.HTMLEncode(ValidationHelper.GetString(value, string.Empty));
            }

            return value;
        }


        /// <summary>
        /// Evaluates the item data and doesn't encode it. Method should be used for columns with html content.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public virtual string EvalHTML(string columnName)
        {
            return ValidationHelper.GetString(Eval(columnName, false), string.Empty);
        }


        /// <summary>
        /// Evaluates the item data, encodes it to be used in javascript code and encapsulates it with "'".
        /// </summary>
        /// <param name="columnName">Column name</param>        
        public virtual string EvalJSString(string columnName)
        {
            object value = Eval(columnName);
            return ScriptHelper.GetString(ValidationHelper.GetString(value, string.Empty));
        }


        /// <summary>
        /// Evaluates the item data and encodes it. Method should be used for columns with string nonhtml content.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="defaultValue">Default value to be used</param>
        public virtual string EvalText(string columnName, string defaultValue = "")
        {
            return ValidationHelper.GetString(Eval(columnName, true), defaultValue);
        }


        /// <summary>
        /// Evaluates the item data and converts it to the integer.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public virtual int EvalInteger(string columnName)
        {
            return Eval<int>(columnName);
        }


        /// <summary>
        /// Evaluates the item data and converts it to the double.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public virtual double EvalDouble(string columnName)
        {
            return Eval<double>(columnName);
        }


        /// <summary>
        /// Evaluates the item data and converts it to the decimal.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public virtual decimal EvalDecimal(string columnName)
        {
            return Eval<decimal>(columnName);
        }


        /// <summary>
        /// Evaluates the item data and converts it to the date time.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public virtual DateTime EvalDateTime(string columnName)
        {
            return Eval<DateTime>(columnName);
        }


        /// <summary>
        /// Evaluates the item data and converts it to the guid.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public virtual Guid EvalGuid(string columnName)
        {
            return Eval<Guid>(columnName);
        }


        /// <summary>
        /// Evaluates the item data and converts it to the bool.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public virtual bool EvalBool(string columnName)
        {
            return Eval<bool>(columnName);
        }

        #endregion


        #region "Messages methods"

        /// <summary>
        /// Returns localized string.
        /// </summary>
        /// <param name="stringName">String to localize.</param>
        public static string GetString(string stringName)
        {
            return ResHelper.GetString(stringName);
        }


        /// <summary>
        /// Shows the general changes saved message.
        /// </summary>
        public void ShowChangesSaved()
        {
            ShowConfirmation(GetString("General.ChangesSaved"));
        }


        /// <summary>
        /// Shows the general confirmation message.
        /// </summary>
        /// <param name="text">Custom message</param>
        /// <param name="persistent">Indicates if the message is persistent</param>
        public void ShowConfirmation(string text, bool persistent = false)
        {
            ShowMessage(MessageTypeEnum.Confirmation, text, null, null, persistent);
        }


        /// <summary>
        /// Shows the given information on the page, optionally with a tooltip text.
        /// </summary>
        /// <param name="text">Information message</param>
        /// <param name="description">Additional description</param>
        /// <param name="tooltipText">Tooltip text</param>
        /// <param name="persistent">Indicates if the message is persistent</param>
        public virtual void ShowInformation(string text, string description = null, string tooltipText = null, bool persistent = true)
        {
            ShowMessage(MessageTypeEnum.Information, text, description, tooltipText, persistent);
        }


        /// <summary>
        /// Logs the exception and displays a general error message
        /// </summary>
        /// <param name="source">Error source for the event log</param>
        /// <param name="eventCode">Event code for the event log</param>
        /// <param name="ex">Exception to log</param>
        /// <param name="showOriginalMessage">If true, original exception message is shown</param>
        public void LogAndShowError(string source, string eventCode, Exception ex, bool showOriginalMessage = false)
        {
            string resString = (showOriginalMessage ? "General.ErrorOccuredLogMessage" : "General.ErrorOccuredLog");

            string text = String.Format(ResHelper.GetString(resString), source, eventCode, ex.Message);

            ShowError(text);

            EventLogProvider.LogException(source, eventCode, ex);
        }


        /// <summary>
        /// Shows the specified error message, optionally with a tooltip text.
        /// </summary>
        /// <param name="text">Error message text</param>
        /// <param name="description">Additional description</param>
        /// <param name="tooltipText">Tooltip text</param>
        /// <param name="persistent">Indicates if the message is persistent</param>
        public virtual void ShowError(string text, string description = null, string tooltipText = null, bool persistent = true)
        {
            ShowMessage(MessageTypeEnum.Error, text, description, tooltipText, persistent);
        }


        /// <summary>
        /// Shows the specified warning message, optionally with a tooltip text.
        /// </summary>
        /// <param name="text">Warning message text</param>
        /// <param name="description">Additional description</param>
        /// <param name="tooltipText">Tooltip text</param>
        /// <param name="persistent">Indicates if the message is persistent</param>
        public virtual void ShowWarning(string text, string description = null, string tooltipText = null, bool persistent = true)
        {
            ShowMessage(MessageTypeEnum.Warning, text, description, tooltipText, persistent);
        }


        /// <summary>
        /// Shows the specified message, optionally with a tooltip text and description.
        /// </summary>
        /// <param name="type">Message type</param>
        /// <param name="text">Message text</param>
        /// <param name="description">Additional description</param>
        /// <param name="tooltipText">Tooltip text</param>
        /// <param name="persistent">Indicates if the message is persistent</param>
        public virtual void ShowMessage(MessageTypeEnum type, string text, string description, string tooltipText, bool persistent)
        {
            MessagesPlaceHolder plcM = MessagesPlaceHolder;
            if (plcM != null)
            {
                plcM.ShowMessage(type, text, description, tooltipText, persistent);
            }
            else
            {
                throw new Exception("[CMSPage.ShowMessage]: Missing messages placeholder on a page or master page.");
            }
        }


        /// <summary>
        /// Adds text to existing message on the page.
        /// </summary>
        /// <param name="type">Message type</param>
        /// <param name="text">Information message</param>
        /// <param name="separator">Separator</param>
        public virtual void AddMessage(MessageTypeEnum type, string text, string separator = null)
        {
            MessagesPlaceHolder plcM = MessagesPlaceHolder;
            if (plcM != null)
            {
                switch (type)
                {
                    // Error message
                    case MessageTypeEnum.Error:
                        plcM.AddError(text, separator);
                        break;

                    // Warning message
                    case MessageTypeEnum.Warning:
                        plcM.AddWarning(text, separator);
                        break;

                    // Changes were saved message
                    case MessageTypeEnum.Confirmation:
                        plcM.AddConfirmation(text, separator);
                        break;

                    default:
                        plcM.AddInformation(text, separator);
                        break;
                }
            }
            else
            {
                throw new Exception("[CMSPage.AddMessage]: Missing messages placeholder on a page or master page.");
            }
        }


        /// <summary>
        /// Adds information text to existing message on the page.
        /// </summary>
        /// <param name="text">Information message</param>
        /// <param name="separator">Separator</param>
        public virtual void AddInformation(string text, string separator = null)
        {
            AddMessage(MessageTypeEnum.Information, text, separator);
        }


        /// <summary>
        /// Adds error text to existing message on the page.
        /// </summary>
        /// <param name="text">Information message</param>
        /// <param name="separator">Separator</param>
        public virtual void AddError(string text, string separator = null)
        {
            AddMessage(MessageTypeEnum.Error, text, separator);
        }


        /// <summary>
        /// Adds warning text to existing message on the page.
        /// </summary>
        /// <param name="text">Information message</param>
        /// <param name="separator">Separator</param>
        public virtual void AddWarning(string text, string separator = null)
        {
            AddMessage(MessageTypeEnum.Warning, text, separator);
        }


        /// <summary>
        /// Adds confirmation text to existing message on the page.
        /// </summary>
        /// <param name="text">Information message</param>
        /// <param name="separator">Separator</param>
        public virtual void AddConfirmation(string text, string separator = null)
        {
            AddMessage(MessageTypeEnum.Confirmation, text, separator);
        }

        #endregion


        #region "Context help methods"

        /// <summary>
        /// Loads application description and help topics with respect to current UI context.
        /// Does nothing when context help not enabled or not on standard page.
        /// </summary>
        protected void LoadContextHelp()
        {
            if (!EnableContextHelp || !IsStandardPage)
            {
                return;
            }

            if ((UIContext != null) && (UIContext.UIElement != null))
            {
                var uiElementHelpTopics = UIContext.UIElement.HelpTopics;
                var app = UIContext.UIElement.Application;
                if (app == null)
                {
                    // UIElement has no application
                    RegisterContextHelpScript(null, null, null, uiElementHelpTopics);

                    return;
                }

                string appCaption = UIElementInfoProvider.GetElementCaption(app);
                string appDescriptionUrl = UIContextHelper.GetApplicationDescriptionUrl(app);

                RegisterContextHelpScript(appCaption, appDescriptionUrl, app.HelpTopics, UIContext.UIElement.IsApplication ? null : uiElementHelpTopics);
            }
            else
            {
                RegisterContextHelpScript(null, null, null, null);
            }
        }


        /// <summary>
        /// Registers script which passes application description and help topics to UI.
        /// If applicationName is null or empty, no application context is assumed (and applicationDescriptionUrl, applicationHelpTopics is not used).
        /// </summary>
        /// <param name="applicationName">Name of the application, or null</param>
        /// <param name="applicationDescriptionUrl">URL to application description, or null</param>
        /// <param name="applicationHelpTopics">Help topics for application, or null</param>
        /// <param name="uiElementHelpTopics">Help topics for UI element, or null</param>
        protected void RegisterContextHelpScript(string applicationName, string applicationDescriptionUrl, IEnumerable<BaseInfo> applicationHelpTopics, IEnumerable<BaseInfo> uiElementHelpTopics)
        {
            List<object> uiElementHelpTopicsJsonCollection = new List<object>();
            object applicationJsonObject = null;

            if (!String.IsNullOrEmpty(applicationName))
            {
                object appDescription = null;
                List<object> applicationHelpTopicsJsonCollection = new List<object>();

                if (!String.IsNullOrEmpty(applicationDescriptionUrl))
                {
                    appDescription = new
                    {
                        name = HTMLHelper.HTMLEncode(String.Format(GetString("contexthelp.description"), applicationName)),
                        url = HTMLHelper.EncodeForHtmlAttribute(DocumentationHelper.GetDocumentationTopicUrl(applicationDescriptionUrl))
                    };
                }
                if (applicationHelpTopics != null)
                {
                    foreach (var topic in applicationHelpTopics)
                    {
                        applicationHelpTopicsJsonCollection.Add(new
                        {
                            name = HTMLHelper.HTMLEncode(GetString(topic.GetStringValue("HelpTopicName", String.Empty))),
                            url = HTMLHelper.EncodeForHtmlAttribute(DocumentationHelper.GetDocumentationTopicUrl(topic.GetStringValue("HelpTopicLink", String.Empty)))
                        }
                            );
                    }
                }

                applicationJsonObject = new
                {
                    description = appDescription,
                    helpTopics = applicationHelpTopicsJsonCollection
                };
            }

            if (uiElementHelpTopics != null)
            {
                foreach (var topic in uiElementHelpTopics)
                {
                    uiElementHelpTopicsJsonCollection.Add(new
                    {
                        name = HTMLHelper.HTMLEncode(GetString(topic.GetStringValue("HelpTopicName", String.Empty))),
                        url = HTMLHelper.EncodeForHtmlAttribute(DocumentationHelper.GetDocumentationTopicUrl(topic.GetStringValue("HelpTopicLink", String.Empty)))
                    }
                    );
                }
            }

            object contextHelpData = new
            {
                contextHelp = new
                {
                    application = applicationJsonObject,
                    helpTopics = uiElementHelpTopicsJsonCollection
                },
                suppressContextHelp = IsDialog
            };

            RequestContext.ClientApplication.Add("contexthelp", contextHelpData);
        }

        #endregion
    }
}