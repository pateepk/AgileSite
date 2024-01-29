using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.Core;
using CMS.DocumentEngine.Web.UI;
using CMS.FormEngine.Web.UI;
using CMS.Helpers;
using CMS.MacroEngine;
using CMS.Modules;
using CMS.PortalEngine;
using CMS.PortalEngine.Web.UI;
using CMS.SiteProvider;

namespace CMS.UIControls
{
    /// <summary>
    /// Server control for rendering tab navigation in administration user interface. 
    /// Allows to create tabs from module's UI elements.
    /// </summary>
    public class UITabs : CMSUserControl, ICallbackEventHandler, IPostBackEventHandler
    {
        #region "Variables"

        private string mRenderedHTML = String.Empty;
        private List<UITabItem> mTabItems;
        private Literal ltlScrollPanelContent;
        private bool mScriptsRegistered;
        private string mInitScript;

        private int mDefaultTab = -2;
        private string mRequestedTabName;

        private MacroResolver mContextResolver;

        #endregion


        #region "Delegates & Events"

        /// <summary>
        /// Tab creation delegate.
        /// </summary>
        public delegate void TabCreatedEventHandler(object sender, TabCreatedEventArgs e);

        /// <summary>
        /// Security check delegate.
        /// </summary>
        public delegate bool CheckTabSecurityEventHandler(ref string resource, ref string permission);

        /// <summary>
        /// Delegate for event raised after tabs are created.
        /// </summary>
        public delegate void TabsLoadedEventHandler(List<UITabItem> tabs);

        /// <summary>
        /// Tab creation event.
        /// </summary>
        public event TabCreatedEventHandler OnTabCreated;

        /// <summary>
        /// Event raised after all tabs are created.
        /// </summary>
        public event TabsLoadedEventHandler OnTabsLoaded;

        /// <summary>
        /// Event for tab clicked.
        /// </summary>
        public event EventHandler OnTabClicked;

        #endregion


        #region "Properties"

        /// <summary>
        /// Code name of the UIElement.
        /// </summary>
        public string ElementName
        {
            get;
            set;
        }


        /// <summary>
        /// Code name of the module.
        /// </summary>
        public string ModuleName
        {
            get;
            set;
        }


        /// <summary>
        /// Context resolver.
        /// </summary>
        private MacroResolver ContextResolver
        {
            get
            {
                if (mContextResolver == null)
                {
                    var resolver = MacroContext.CurrentResolver.CreateChild();

                    resolver.SetNamedSourceData("Page", Page);
                    resolver.SetNamedSourceData("UIContext", UIContext);

                    resolver.SetNamedSourceDataCallback("EditedObject", x => UIContext.EditedObject, false);
                    resolver.SetNamedSourceDataCallback("EditedObjectParent", x => UIContext.EditedObjectParent, false);

                    mContextResolver = resolver;
                }

                return mContextResolver;
            }
        }


        /// <summary>
        /// Name of the javascript function which is called when specified tab (UI element) is clicked. 
        /// UI element code name is passed as parameter.
        /// </summary>
        public string JavaScriptHandler
        {
            get;
            set;
        }


        /// <summary>
        /// URL of the page that is used instead of selected tab's URL, when tabs control is first loaded.
        /// </summary>
        public string StartPageURL
        {
            get;
            set;
        }


        /// <summary>
        /// Gets the value which indicates whether there is some tab displayed or not.
        /// </summary>
        public bool TabsEmpty
        {
            get;
            private set;
        } = true;


        /// <summary>
        /// Indicates whether the content of the tab will be loaded into target frame.
        /// </summary>
        public bool OpenTabContentAfterLoad
        {
            get;
            set;
        } = true;


        /// <summary>
        /// If true, the selected tab is remembered across session
        /// </summary>
        public bool RememberSelectedTab
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the sub tabs are allowed in these tabs
        /// </summary>
        public bool AllowSubTabs
        {
            get;
            set;
        } = true;


        /// <summary>
        /// If true, target frame is not in parent frames but iframe
        /// </summary>
        public bool UseIFrame
        {
            get;
            set;
        }


        /// <summary>
        /// Tab control ID prefix
        /// </summary>    
        public string TabControlIdPrefix
        {
            get
            {
                return ValidationHelper.GetString(ViewState["TabControlIdPrefix"], String.Empty);
            }
            set
            {
                value = value?.Replace(" ", String.Empty);
                ViewState["TabControlIdPrefix"] = value;
            }
        }


        /// <summary>
        /// Horizontal or vertical layout.
        /// </summary>    
        public TabControlLayoutEnum TabControlLayout
        {
            get
            {
                if (Convert.ToString(ViewState["TabControlLayout"]) == String.Empty)
                {
                    ViewState["TabControlLayout"] = TabControlLayoutEnum.Horizontal;
                }
                return ((TabControlLayoutEnum)(ViewState["TabControlLayout"]));
            }
            set
            {
                ViewState["TabControlLayout"] = value;
            }
        }


        /// <summary>
        /// Collection of tabs.
        /// </summary>
        public List<UITabItem> TabItems
        {
            get
            {
                return mTabItems ?? (mTabItems = new List<UITabItem>());
            }
            set
            {
                mTabItems = value;
            }
        }


        /// <summary>
        /// Internal collection of tabs (built either from Tabs or TabItems).
        /// </summary>
        protected List<UITabItem> TabItemsInternal
        {
            get
            {
                List<UITabItem> tabItemsInternal;
                if (TabItems.Count > 0)
                {
                    // Sort tabs by index to be sure the order is ensured for multiple actions
                    if (TabItems.Count > 1)
                    {
                        // At least one action has index
                        if (TabItems.Exists(t => (t.Index != -1)))
                        {
                            // Sort the actions
                            TabItems.Sort((t1, t2) => t1.Index.CompareTo(t2.Index));
                        }
                    }
                    tabItemsInternal = TabItems;
                }
                else
                {
                    tabItemsInternal = new List<UITabItem>();
                }

                // Get empty tabs and remove them from collection
                var tabsToRemove = tabItemsInternal.Where(tab => tab.IsEmpty()).ToList();
                foreach (var tab in tabsToRemove)
                {
                    tabItemsInternal.Remove(tab);
                }

                return tabItemsInternal;
            }
        }


        /// <summary>
        /// If is set true, first item will be selected by default if is not some other item selected
        /// </summary>
        public virtual bool SelectFirstItemByDefault
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["SelectFirstItemByDefault"], true);
            }
            set
            {
                ViewState["SelectFirstItemByDefault"] = value;
            }
        }


        /// <summary>
        /// Index of the selected tab.
        /// </summary>    
        public int SelectedTab
        {
            get
            {
                return ValidationHelper.GetInteger(ViewState["SelectedTab"], DefaultTab);
            }
            set
            {
                ViewState["SelectedTab"] = value;
            }
        }


        /// <summary>
        /// Name of the selected tab.
        /// </summary>    
        public string SelectedTabName
        {
            get
            {
                // If some tab is selected through index, return its name
                var selectedTab = SelectedTab;
                if (selectedTab >= 0)
                {
                    var tab = GetTabItem(selectedTab);
                    if (tab != null)
                    {
                        return tab.TabName;
                    }
                }

                return ValidationHelper.GetString(ViewState["SelectedTabName"], RequestedTabName);
            }
            set
            {
                ViewState["SelectedTabName"] = value;
            }
        }


        /// <summary>
        /// Default tab to select
        /// </summary>
        private int DefaultTab
        {
            get
            {
                if (mDefaultTab < -1)
                {
                    mDefaultTab = GetDefaultTab();
                }

                return mDefaultTab;
            }
        }


        /// <summary>
        /// Returns tab to select. First search query string (tabName), second search session and if not found, select first tab (if SelectFirstItemByDefault is set).
        /// </summary>
        private string RequestedTabName
        {
            get
            {
                return mRequestedTabName ?? (mRequestedTabName = GetRequestedTabName());
            }
        }

        
        /// <summary>
        /// Tab code name, which will be selected by default.
        /// </summary>
        public string DefaultTabName
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if postback is fired when tab is clicked.
        /// </summary>
        public bool UsePostback
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["usePostback"], false);
            }
            set
            {
                ViewState["usePostback"] = value;
            }
        }


        /// <summary>
        /// If URL for tab items is set, this property specifies target frame for all URLs.
        /// </summary>
        public string UrlTarget
        {
            get
            {
                return ValidationHelper.GetString(ViewState["UrlTarget"], String.Empty);
            }
            set
            {
                ViewState["UrlTarget"] = value;
            }
        }

        #endregion


        #region "Control events"

        /// <summary>
        /// Create child controls
        /// </summary>
        protected override void CreateChildControls()
        {
            ltlScrollPanelContent = new Literal();
            Controls.Add(ltlScrollPanelContent);

            base.CreateChildControls();

            if (RequestHelper.IsPostBack() && (Page != null))
            {

                if (ControlsHelper.IsInUpdatePanel(this))
                {
                    ControlsHelper.UpdateCurrentPanel(this);
                }
            }
        }


        /// <summary>
        /// Load event handler.
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            if (!StopProcessing)
            {
                if (!String.IsNullOrEmpty(ModuleName))
                {
                    // Reserve log item
                    DataRow logItem = SecurityDebug.StartSecurityOperation("LoadUITabs");

                    // Load tabs
                    LoadTabs(TabItems, ModuleName, ElementName);

                    // Raise event
                    RaiseTabsLoaded();

                    if (TabItems.Count > 0)
                    {
                        TabsEmpty = false;
                    }

                    // Log the security
                    if (logItem != null)
                    {
                        SecurityDebug.FinishSecurityOperation(logItem, CurrentUser.UserName, ModuleName, ElementName, TabItems.Count, SiteContext.CurrentSiteName);
                    }
                }
                else if ((TabItemsInternal != null) && (TabItemsInternal.Count > 0))
                {
                    TabsEmpty = false;
                }
            }

            base.OnLoad(e);
        }


        /// <summary>
        /// PreRender event handler.
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (StopProcessing)
            {
                return;
            }

            Page.PreRenderComplete += Page_PreRenderComplete;

            // If first load and UI element is set
            if (!RequestHelper.IsPostBack() && (!String.IsNullOrEmpty(ModuleName) || !String.IsNullOrEmpty(ElementName)) || (DefaultTab >= 0))
            {
                // Ensure correct first tab selection
                DoTabSelection();
            }

            // If tab is selected through query string, remember it
            if (!RequestHelper.IsPostBack() && QueryHelper.Contains("tabName"))
            {
                RememberTab();
            }
        }


        /// <summary>
        /// Fires when PreRender is complete
        /// </summary>
        private void Page_PreRenderComplete(object sender, EventArgs e)
        {
            if (!StopProcessing)
            {
                RegisterScripts();
            }
        }


        /// <summary>
        /// Renders the control.
        /// </summary>
        protected override void Render(HtmlTextWriter output)
        {
            if (!StopProcessing)
            {
                if (mRenderedHTML == String.Empty)
                {
                    GenerateMenu();
                }

                // Render the tab content into the scroll panel
                if (ltlScrollPanelContent != null)
                {
                    ltlScrollPanelContent.Text = mRenderedHTML;
                }
            }

            base.Render(output);
        }

        #endregion


        #region "Methods"

        #region "Tab items manipulation"

        /// <summary>
        /// Loads the default tab from the context
        /// </summary>
        private int GetDefaultTab()
        {
            // If tab parameter is specified in URL, use it for tab preselection
            var defaultTab = QueryHelper.GetInteger("tab", -1);
            if (defaultTab >= 0)
            {
                return defaultTab;
            }

            return -1;
        }


        /// <summary>
        /// Loads the default tab from the context
        /// </summary>
        private string GetRequestedTabName()
        {
            // If tab parameter is specified in URL, use it for tab pre-selection
            var defaultTab = QueryHelper.GetString("tabName", "");
            if (!String.IsNullOrEmpty(defaultTab))
            {
                var tab = GetTabItem(defaultTab);
                if (tab != null)
                {
                    return defaultTab;
                }
            }

            // Try to get remembered tab
            var rememberedTab = GetRememberedTab();
            if (!String.IsNullOrEmpty(rememberedTab))
            {
                return rememberedTab;
            }

            // Ensure default tab name really exists
            if (!String.IsNullOrEmpty(DefaultTabName) && TabItems.Find(x => DefaultTabName.Equals(x.TabName, StringComparison.OrdinalIgnoreCase)) != null)
            {
                return DefaultTabName;
            }

            if (SelectFirstItemByDefault)
            {
                var tab = GetTabItem(0);
                if (tab != null)
                {
                    while (tab.HasSubItems)
                    {
                        tab = tab.SubItems[0];
                    }

                    return tab.TabName;
                }
            }

            return String.Empty;
        }


        /// <summary>
        /// Gets the tab by its index
        /// </summary>
        /// <param name="tabIndex">Tab index</param>
        private UITabItem GetTabItem(int tabIndex)
        {
            return TabItemsInternal[tabIndex];
        }


        /// <summary>
        /// Looks within list of tabs and their subtabs for tab with given tabName.
        /// </summary>
        /// <param name="tabName">Needle</param>
        /// <param name="tabs">Haystack</param>
        private UITabItem GetTabItem(string tabName, List<UITabItem> tabs = null)
        {
            // Use top level tabs by default
            if (tabs == null)
            {
                tabs = TabItemsInternal;
            }

            foreach (var tab in tabs)
            {
                // Check current tab name
                if (!string.IsNullOrEmpty(tabName) && tabName.Equals(tab.TabName, StringComparison.OrdinalIgnoreCase))
                {
                    return tab;
                }

                // Check sub tabs names if any
                if (tab.HasSubItems)
                {
                    var found = GetTabItem(tabName, tab.SubItems);
                    if (found != null)
                    {
                        return found;
                    }
                }
            }

            // Tab name not found
            return null;
        }


        /// <summary>
        /// Adds a tab.
        /// </summary>
        /// <param name="tab">Tab</param>
        public void AddTab(UITabItem tab)
        {
            if (tab == null)
            {
                return;
            }

            // Ensure correct index
            if (tab.Index == -1)
            {
                tab.Index = TabItems.Count;
            }
            else
            {
                // Post processing of tab attribute
                for (int i = 0; i < TabItems.Count; i++)
                {
                    if (TabItems[i].Index == tab.Index)
                    {
                        // Replace tab with the same index
                        TabItems[i] = tab;

                        // Stop processing
                        return;
                    }
                }
            }

            // If tab with the same index was not found, add it to the list
            TabItems.Add(tab);
        }

        #endregion


        #region "Tabs loading"

        /// <summary>
        /// Gets the data for the tabs
        /// </summary>
        /// <param name="moduleName">Module name</param>
        /// <param name="parentElementName">Parent element name</param>
        private static DataSet GetTabsData(string moduleName, string parentElementName)
        {
            DataSet ds = string.IsNullOrEmpty(parentElementName) 
                            ? UIElementInfoProvider.GetRootChildUIElements(moduleName) 
                            : UIElementInfoProvider.GetChildUIElements(moduleName, parentElementName);

            return ds;
        }


        /// <summary>
        /// Loads child elements of specified UI elements info supplied list.
        /// </summary>
        /// <param name="tabItems">List of tab items to append loaded tabs to.</param>
        /// <param name="moduleName">Code name of the module</param>
        /// <param name="parentElementName">Code name of the UI element, which child elements are to be loaded</param>
        /// <param name="parentTab">Tab to be used as parent for loaded tabs.</param>
        private void LoadTabs(List<UITabItem> tabItems, string moduleName, string parentElementName, UITabItem parentTab = null)
        {
            // Get UI elements which represent the tabs
            var ds = GetTabsData(moduleName, parentElementName);

            if (DataHelper.DataSourceIsEmpty(ds))
            {
                return;
            }

            string siteName = SiteContext.CurrentSiteName;

            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                // Get element name
                string elementName = ValidationHelper.GetString(dr["ElementName"], String.Empty);
                int elementModuleID = ValidationHelper.GetInteger(dr["ElementResourceID"], 0);

                DataRow logItem = SecurityDebug.StartSecurityOperation("TabPermissionCheck");

                int index = tabItems.Count;

                // Get parameters for tab
                var tab = CreateTab(dr, index, parentTab);

                // Check if user can see tab
                if ((tab != null) && (tab.SkipCheckPermissions || CurrentUser.IsAuthorizedPerUIElement(elementModuleID, elementName)))
                {
                    if (parentTab == null)
                    {
                        // If tab is to be expanded
                        if (tab.Expand)
                        {
                            // Load child tabs on the same level
                            LoadTabs(tabItems, moduleName, elementName);
                        }
                        else if (AllowSubTabs)
                        {
                            var uiElem = new UIElementInfo(dr);

                            // Add respective child tabs if any
                            HandleSubTabs(uiElem, tab);
                        }
                    }

                    // Do not add expanded tab
                    if (!tab.Expand)
                    {
                        tabItems.Add(tab);
                    }
                }

                // Log the operation
                if (logItem != null)
                {
                    SecurityDebug.FinishSecurityOperation(logItem, CurrentUser.UserName, moduleName, elementName, (tab != null), siteName);
                    SecurityDebug.SetLogItemImportant(logItem);
                }
            }
        }


        /// <summary>
        /// Handles sub tabs of ui element.
        /// </summary>
        /// <param name="uiElem">Element to handle subtabs for.</param>
        /// <param name="tab">Tab item where sub items are to be added.</param>
        private void HandleSubTabs(UIElementInfo uiElem, UITabItem tab)
        {
            if (uiElem.ElementType != UIElementTypeEnum.PageTemplate)
            {
                return;
            }

            // Check if element represents layout template
            var template = PageTemplateInfoProvider.GetPageTemplateInfo(uiElem.ElementPageTemplateID);
            if ((template != null) && (template.PageTemplateIsLayout))
            {
                // Apply tab extender
                ApplyElementExtender(uiElem);

                // Get child element's module name (possibly different from parent's)
                string childModuleName = ModuleName;
                var ri = ResourceInfoProvider.GetResourceInfo(uiElem.ElementResourceID);
                if (ri != null)
                {
                    childModuleName = ri.ResourceName;
                }

                // Load child tabs
                LoadTabs(tab.SubItems, childModuleName, uiElem.ElementName, tab);
            }
        }


        /// <summary>
        /// Returns initialized array with parameters which.
        /// </summary>
        private UITabItem CreateTab(DataRow drElement, int tabIndex, UITabItem parentTab)
        {
            var uiElem = new UIElementInfo(drElement);

            // Check UI element availability
            if (!UIContextHelper.CheckElementAvailabilityInUI(uiElem))
            {
                return null;
            }

            var tab = CreateTab(uiElem, tabIndex);

            // Handle additional initialization of the tab
            tab = RaiseTabCreated(uiElem, tab, tabIndex);

            if (tab != null)
            {
                tab.ParentTabItem = parentTab;

                var page = Page as CMSPage;
                if (page != null)
                {
                    tab = page.RaiseTabCreated(uiElem, tab, tabIndex);
                }
            }

            HandleSelectedTab(uiElem, tabIndex);

            return tab;
        }


        /// <summary>
        /// Creates the tab
        /// </summary>
        /// <param name="uiElem">UI Element</param>
        /// <param name="tabIndex">Tab index</param>
        private UITabItem CreateTab(UIElementInfo uiElem, int tabIndex)
        {
            var caption = UIElementInfoProvider.GetElementCaption(uiElem);

            var tab = new UITabItem
                      {
                          Index = tabIndex,
                          TabName = uiElem.ElementName,
                          Text = HTMLHelper.HTMLEncode(caption),
                          RedirectUrl = GetTabUrl(uiElem),
                          OnClientClick = GetTabOnClick(uiElem)
                      };

            return tab;
        }


        /// <summary>
        /// Raises tabs loaded event
        /// </summary>
        private void RaiseTabsLoaded()
        {
            OnTabsLoaded?.Invoke(TabItems);
        }


        /// <summary>
        /// Raises OnTabCreated event.
        /// </summary>
        /// <param name="element">UI element</param>
        /// <param name="tab">Tab object</param>
        /// <param name="tabIndex">Index of a tab</param>
        /// <returns>Potentially modified tab object</returns>
        private UITabItem RaiseTabCreated(UIElementInfo element, UITabItem tab, int tabIndex)
        {
            if (OnTabCreated == null)
            {
                return tab;
            }

            var e = new TabCreatedEventArgs
            {
                UIElement = element,
                Tab = tab,
                TabIndex = tabIndex
            };

            OnTabCreated(this, e);

            return e.Tab;
        }


        /// <summary>
        /// Gets and applies tabs extender from given tabs ui element.
        /// </summary>
        /// <param name="element">UI element to take extender from.</param>
        private void ApplyElementExtender(UIElementInfo element)
        {
            var data = new UIContextData();
            data.LoadData(element.ElementProperties);

            var extenderAssemblyName = ValidationHelper.GetString(data["TabExtender"], null);
            var extenderClassName = ValidationHelper.GetString(data["ExtenderClassName"], null);
            if (!String.IsNullOrEmpty(extenderClassName))
            {
                try
                {
                    LoadExtender(extenderAssemblyName, extenderClassName, this);
                }
                catch (Exception ex)
                {
                    // Log exception
                    CoreServices.EventLog.LogException("UITabs", "LoadExtender", ex);

                    // Stop further processing of tabs
                    StopProcessing = true;

                    // Add error message
                    WebPartError.AddErrorControls(this, ex.Message, ex);
                }
            }
        }


        /// <summary>
        /// Loads the control extender of the specified type for the specified control.
        /// </summary>
        /// <param name="assemblyName">Assembly name where the extender type is located</param>
        /// <param name="className">Class name of the extender type</param>
        /// <param name="control">Control to be extended</param>
        public static UITabsExtender LoadExtender(string assemblyName, string className, UITabs control)
        {
            var extender = (UITabsExtender)ClassHelper.GetClass(assemblyName, className);
            if (extender != null)
            {
                extender.Init(control, false);
                extender.OnInitTabs();

                return extender;
            }

            throw new Exception($"UITabsExtender with assembly name '{assemblyName}' and class '{className}' was not found.");
        }

        #endregion


        #region "Tabs rendering"

        /// <summary>
        /// Generates menu.
        /// </summary>
        private void GenerateMenu()
        {
            if (TabItemsInternal == null)
            {
                return;
            }

            // Check the count
            int count = TabItemsInternal.Count;
            if (count < 0)
            {
                return;
            }

            mRenderedHTML = String.Empty;

            var sb = new StringBuilder(100 * count + 200);

            // Basic menu code
            var cssClass = (TabControlLayout == TabControlLayoutEnum.Vertical) ? "nav-tabs-container-vertical" : "nav-tabs-container-horizontal";
            sb.AppendFormat("<div class=\"{0}\" id=\"{1}\">", cssClass, ClientID);
            sb.Append("<ul class=\"nav nav-tabs\">");

            // Generate the pages
            for (int i = 0; i < count; i++)
            {
                UITabItem currentTab = TabItemsInternal[i];

                // If URL is defined or subtabs available
                if ((currentTab.RedirectUrl != null) || currentTab.HasSubItems || UsePostback)
                {
                    GenerateMenuItem(sb, i, i, currentTab, 0);

                    // End the row
                    bool last = ((i + 1) == count);

                    // Content after the last tab
                    if (last && SystemContext.DevelopmentMode && CurrentUser.CheckPrivilegeLevel(UserPrivilegeLevelEnum.GlobalAdmin))
                    {
                        // Add editing icon in development mode
                        var link = PortalUIHelper.GetResourceUIElementLink(ModuleName, ElementName);
                        if (!String.IsNullOrEmpty(link))
                        {
                            sb.Append("<li>", link, "</li>");
                        }
                    }
                }
            }

            sb.Append("</ul>\n");
            sb.Append("</div>\n");

            mRenderedHTML = sb.ToString();
        }


        /// <summary>
        /// Generates individual tab item as li HTML element.
        /// </summary>
        /// <param name="sb">String builder to write output to.</param>
        /// <param name="selectIndex">Index top level tab to select when generated tab is clicked.</param>
        /// <param name="index">Index of the tab.</param>
        /// <param name="tab">Tab to be rendered.</param>
        /// <param name="level">Level of tabs. Top level has level = 0</param>
        private void GenerateMenuItem(StringBuilder sb, int selectIndex, int index, UITabItem tab, int level)
        {
            string prefix = TabControlIdPrefix;

            // Decide weather sub items are to be rendered
            var renderSubItems = tab.HasSubItems;

            // Ensure name for this tab
            EnsureTabName(tab, index);

            // Check if tab or one of its subtabs is selected
            var isSelected = IsTabSelected(tab);
            var isVertical = (TabControlLayout == TabControlLayoutEnum.Vertical);
            var isActive = isSelected && (!isVertical || !renderSubItems || (level > 0));
            var isDropDownTab = renderSubItems && !isVertical;

            var mClass = "";
            if ((tab.CssClass != null) && (tab.CssClass.Trim() != string.Empty))
            {
                mClass = tab.CssClass;
            }

            // Get text for tab
            var text = GetTabText(tab, isSelected);

            var idSuffix = (level > 0) ? "sub" + index : "";

            // Javascript
            string click = null;
            if (!renderSubItems)
            {
                var selectTab = selectIndex.ToString();
                if (isVertical)
                {
                    selectTab += idSuffix;
                }

                var defaultOnClientClick = string.Format("{0}SelTab({1}, '{2}', this.getAttribute('data-href'), {3});", prefix, ScriptHelper.GetString(selectTab), UrlTarget, ScriptHelper.GetString(text));
                click = " onclick=\"" + defaultOnClientClick + tab.OnClientClick + "; return false;\"";
            }

            string className = isActive ? mClass + " active" : mClass;
            sb.Append("<li id=\"", prefix, "TabItem_", selectIndex, idSuffix,
                "\" class=\"", className, 
                "\" data-href=\"", HTMLHelper.EncodeForHtmlAttribute(tab.RedirectUrl), "\" ");

            // Title
            if (!String.IsNullOrEmpty(tab.Tooltip))
            {
                sb.Append("title=\"", HTMLHelper.HTMLEncode(tab.Tooltip), "\"");
            }

            // Javascript
            sb.Append(click);

            sb.Append(">");

            if ((tab.RedirectUrl != null) && (tab.RedirectUrl.Trim() != String.Empty))
            {
                GenerateTabLinkOpening(sb, tab, isSelected, isActive, prefix, index, mClass);

                GenerateTabLinkText(sb, isDropDownTab, text, tab.Text);

                sb.Append("</a>");

                if (renderSubItems)
                {
                    var id = GetSubTabID(tab);

                    string subTabClass;
                    if (isVertical)
                    {
                        subTabClass = "nav nav-tabs nav-sub-tabs";
                        subTabClass += isSelected ? " in" : " collapse";
                    }
                    else
                    {
                        subTabClass = "dropdown-menu";
                    }

                    // Render submenu
                    sb.AppendFormat("<ul role=\"menu\" class=\"{0}\" id=\"{1}\">", subTabClass, id);
                    int i = 0;
                    foreach (var subTab in tab.SubItems)
                    {
                        GenerateMenuItem(sb, index, i++, subTab, level + 1);
                    }
                    sb.Append("</ul>");
                }
            }
            else
            {
                sb.Append("<div>", text, "</div>");
            }

            sb.Append("</li>\n");
        }


        /// <summary>
        /// Renders content of tab with supplied string builder. 
        /// </summary>
        /// <param name="sb">String builder to append result to.</param>
        /// <param name="isDropDownTab">Indicates if tab is drop down tab (i.e. has subtabs and this is mode is set to horizontal tabs.</param>
        /// <param name="text">Text for active state of tab.</param>
        /// <param name="originalText">Tabs original (passive) text.</param>
        private void GenerateTabLinkText(StringBuilder sb, bool isDropDownTab, string text, string originalText)
        {
            sb.AppendFormat("<span class=\"tab-title\" >{0}</span>", text);

            // Render original tab name for drop down tab
            if (isDropDownTab)
            {
                sb.AppendFormat("<span class=\"tab-original-title\" >{0}</span>", originalText);

                // Add submenu icon
                sb.Append("<i class=\"caret\" aria-hidden=\"true\"></i>");
            }
        }


        /// <summary>
        /// Generates opening A tag for tab. attributes needed for drop down tabs and collapsible tabs to work.
        /// </summary>
        /// <param name="sb">String builder to append result to.</param>
        /// <param name="tab">Rendered tab.</param>
        /// <param name="isSelected">Indicates if tab is selected.</param>
        /// <param name="isActive">Indicates if tab is active.</param>
        /// <param name="prefix">Tab control ID prefix.</param>
        /// <param name="index">Order of tab.</param>
        /// <param name="cssClass">Extra css class.</param>
        private void GenerateTabLinkOpening(StringBuilder sb, UITabItem tab, bool isSelected, bool isActive, string prefix, int index, string cssClass)
        {
            var renderSubItems = tab.HasSubItems;
            var isVertical = TabControlLayout == TabControlLayoutEnum.Vertical;

            // Start link
            sb.Append("<a id=\"", prefix, "TabLink_", index, "\" class=\"");
            sb.Append(cssClass + (isSelected ? "LinkSelected" : "Link"));
            if (renderSubItems)
            {
                if (!isVertical)
                {
                    sb.Append(" dropdown-toggle");
                }
                else if (!isSelected)
                {
                    sb.Append(" collapsed");
                }
            }
            sb.Append("\"");

            if (renderSubItems)
            {
                if (isVertical)
                {
                    // Set sub tab id to collapse/expand
                    var id = GetSubTabID(tab);
                    sb.AppendFormat(" tabindex=\"-1\" role=\"tab\" data-target=\"#{0}\"", id);

                    // Use collapsible group for sub items
                    sb.AppendFormat(" data-toggle=\"{0}\"", (isSelected && !isActive) ? "collapse-disabled" : "collapse");
                }
                else
                {
                    // Use drop down for sub items
                    sb.Append(" data-toggle=\"dropdown\"");
                }
            }
            else
            {
                // Use URL for ordinary tabs
                sb.Append(" href=\"", HTMLHelper.EncodeForHtmlAttribute(tab.RedirectUrl), "\"");
            }

            // Append target
            if (UrlTarget != String.Empty)
            {
                sb.Append(" target=\"", UrlTarget, "\"");
            }

            sb.Append(">");
        }


        /// <summary>
        /// Checks weather given tab has TabName set and creates name if empty.
        /// </summary>
        /// <param name="tab">Tab to ensure name for.</param>
        /// <param name="index">Order of tab withing tabs. Used for generation new name.</param>
        private void EnsureTabName(UITabItem tab, int index)
        {
            // Ensure tab name if empty
            if (string.IsNullOrEmpty(tab.TabName))
            {
                tab.TabName = index.ToString();
                if (tab.ParentTabItem != null)
                {
                    tab.TabName = string.Format("{0}_{1}", tab.ParentTabItem.TabName, tab.TabName);
                }
            }
        }


        /// <summary>
        /// Returns text for given tab. Value of Text property is returned by default. 
        /// Returns text from selected tab for horizontal tab having selected subtab.
        /// </summary>
        /// <param name="tab">Tab to return text for.</param>
        /// <param name="isSelected">Flag indicating if tab is selected.</param>
        private string GetTabText(UITabItem tab, bool isSelected)
        {
            // Vertical tabs do not change its text
            if (TabControlLayout == TabControlLayoutEnum.Vertical)
            {
                return tab.Text;
            }

            if (isSelected && tab.HasSubItems && !string.IsNullOrEmpty(SelectedTabName))
            {
                var selectedTab = GetTabItem(SelectedTabName, tab.SubItems);
                if (selectedTab != null)
                {
                    return selectedTab.Text;
                }
            }

            return tab.Text;
        }


        /// <summary>
        /// Gets the onclick script for the tab
        /// </summary>
        /// <param name="uiElem">UI Element</param>
        private string GetTabOnClick(UIElementInfo uiElem)
        {
            // Set help topic
            return $" UIT_Selected({ScriptHelper.GetString(uiElem.ElementName)});";
        }


        /// <summary>
        /// Returns ID of given sub tab created of its name.
        /// </summary>
        /// <param name="tab">Tab to create id for</param>
        private string GetSubTabID(UITabItem tab)
        {
            return "tab_" + tab.TabName.Replace(".", "_") + "_submenu";
        }


        /// <summary>
        /// Gets the element URL
        /// </summary>
        /// <param name="uiElem">UI Element</param>
        private string GetTabUrl(UIElementInfo uiElem)
        {
            // Create element URL
            var url = UIContextHelper.GetElementUrl(uiElem, UIContext);

            url = HandleTabQueryString(url, uiElem);

            return UrlResolver.ResolveUrl(url);
        }


        /// <summary>
        /// Handles the tab query string for the given URL
        /// </summary>
        /// <param name="url">URL to handle</param>
        /// <param name="uiElem">UI Element</param>
        public string HandleTabQueryString(string url, UIElementInfo uiElem)
        {
            if(String.IsNullOrEmpty(url))
            {
                return url;
            }

            var contextData = new UIContextData();

            contextData.LoadData(uiElem.ElementProperties);

            // Append URL suffix and correct child and parent ID
            url = UIContextHelper.CorrectChildParentRelations(url, uiElem, UIContext);

            if (uiElem.ElementType == UIElementTypeEnum.PageTemplate)
            {
                // If target element displays breadcrumbs with parent target, add current display title value
                if (ValidationHelper.GetString(contextData["breadcrumbstarget"], String.Empty) == "_parent")
                {
                    url = URLHelper.AddParameterToUrl(url, "parenttitle", UIContext.DisplayTitle.ToString());
                }

                // Do not show title under tabs
                url = URLHelper.UpdateParameterInUrl(url, "displaytitle", "false");
            }
            else
            {
                url = URLHelper.RemoveParameterFromUrl(url, "displaytitle");
            }

            // Append IsInDialog parameter
            if (QueryHelper.Contains("dialog"))
            {
                url = URLHelper.UpdateParameterInUrl(url, "isindialog", "true");
            }

            // Remove the dialog parameter for sub-pages
            url = URLHelper.RemoveParameterFromUrl(url, "dialog");
            url = URLHelper.RemoveParameterFromUrl(url, "tabName");

            url = ContextResolver.ResolveMacros(url);

            // Append hash for dialog
            url = UIContextHelper.AppendDialogHash(UIContext, url);
            url = URLHelper.EnsureHashToQueryParameters(url);

            return url;
        }

        #endregion


        #region "Selected tab handling"

        /// <summary>
        /// Gets the selected tab item in first level of tabs.
        /// </summary>
        private UITabItem GetSelectedTab()
        {
            return GetTabItem(SelectedTabName);
        }


        /// <summary>
        /// Selects the first tab or displays error message. Returns true if some tab was selected.
        /// </summary>
        /// <param name="redirect">Indicates if redirect is done</param>
        public bool DoTabSelection(bool redirect = true)
        {
            bool result = false;

            string url = null;
            string script = null;

            // Call the script for tab which is selected
            if (!TabsEmpty)
            {
                var selectedTab = GetSelectedTab();
                if (selectedTab != null)
                {
                    script = selectedTab.OnClientClick;
                }

                if (redirect)
                {
                    // If set, override initial selected tab URL by start page URL
                    if (StartPageURL != null)
                    {
                        url = StartPageURL;
                    }
                    // Prepare the URL
                    else
                    {
                        url = (selectedTab != null) ? selectedTab.RedirectUrl : String.Empty;

                        // Append URL for saved info.
                        bool saved = ValidationHelper.GetBoolean(UIContext["saved"], false);
                        if (saved)
                        {
                            url = URLHelper.AddParameterToUrl(url, "saved", "1");

                            // Recalculate hash
                            url = UIContextHelper.AppendDialogHash(UIContext, url);
                        }
                    }
                }

                result = true;
            }
            else
            {
                // Check what was the reason of not displaying tabs
                string resource = null;
                string permission = null;

                var page = Page as CMSPage;
                page?.RaiseCheckTabSecurity(ref resource, ref permission);

                // Prepare info page url
                url = AdministrationUrlHelper.GetInformationUrl("uiprofile.uinotavailable");
            }

            if (OpenTabContentAfterLoad)
            {
                SetInitScript(GetRedirectScript(UrlResolver.ResolveUrl(url), script));
            }

            return result;
        }


        /// <summary>
        /// Handles the selected tab
        /// </summary>
        /// <param name="uiElem">UI Element</param>
        /// <param name="tabIndex">Tab index</param>
        private void HandleSelectedTab(UIElementInfo uiElem, int tabIndex)
        {
            var element = UIContextHelper.CheckSelectedElement(uiElem, UIContext);
            if (element == null)
            {
                return;
            }

            // Select current tab
            SelectedTab = tabIndex;

            // Suffix (object IDS and path hierarchy) -> move to next window
            var selectionSuffix = ValidationHelper.GetString(UIContext["selectionSuffix"], String.Empty);

            // Create StartPage URL based on direction element
            var url = UIContextHelper.CorrectChildParentRelations(UIContextHelper.GetElementUrl(element, UIContext), element, UIContext);

            // No title after tabs
            url = URLHelper.AddParameterToUrl(url, "displaytitle", "false");

            StartPageURL = url + selectionSuffix;
        }


        /// <summary>
        /// Checks if given tab or one of its child tabs is selected.
        /// </summary>
        /// <param name="tab">Tab to be checked</param>
        /// <param name="selectedTabName">Selected tab name</param>
        /// <param name="checkChildren">If true, children of the tabs are searched as well</param>
        private bool IsTabSelected(UITabItem tab, string selectedTabName = null, bool checkChildren = true)
        {
            if (selectedTabName == null)
            {
                selectedTabName = SelectedTabName;
            }

            if (string.IsNullOrEmpty(selectedTabName))
            {
                return false;
            }

            // Check the tab itself
            if (selectedTabName.Equals(tab.TabName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // Check sub items
            if (checkChildren && tab.HasSubItems)
            {
                foreach (var subTab in tab.SubItems)
                {
                    if (IsTabSelected(subTab, selectedTabName))
                    {
                        return true;
                    }
                }
            }

            return false;
        }


        #region "State retention"

        /// <summary>
        /// Gets the remembered tab name. Returns empty string if no tab was remembered or none of the remembered tabs was found
        /// </summary>
        private string GetRememberedTab()
        {
            if (RememberSelectedTab && !String.IsNullOrEmpty(ModuleName))
            {
                var rememberedTabs = SessionHelper.GetValue(GetRememberKey(ModuleName, ElementName)) as List<string>;
                if (rememberedTabs != null)
                {
                    // Get index of first found remembered tab
                    for (int i = rememberedTabs.Count - 1; i >= 0; i--)
                    {
                        // Get the tab index by name
                        var tabName = rememberedTabs[i];
                        var tab = GetTabItem(tabName);

                        if (tab != null)
                        {
                            return tab.TabName;
                        }
                    }
                }
            }

            return String.Empty;
        }


        /// <summary>
        /// Gets the key to remember the selected tab
        /// </summary>
        /// <param name="moduleName">Module name</param>
        /// <param name="elementName">Element name</param>
        private static string GetRememberKey(string moduleName, string elementName)
        {
            return $"SelectedTab|{moduleName}|{elementName}";
        }


        /// <summary>
        /// Remembers the default tab in the context
        /// </summary>
        /// <param name="tabName">Element name to remember</param>
        private void RememberTab(string tabName)
        {
            if (!String.IsNullOrEmpty(tabName) && !String.IsNullOrEmpty(ModuleName))
            {
                var key = GetRememberKey(ModuleName, ElementName);

                // Ensure list of remembered tabs
                var rememberedTabs = SessionHelper.GetValue(key) as List<string>;
                if (rememberedTabs == null)
                {
                    rememberedTabs = new List<string>();

                    SessionHelper.SetValue(key, rememberedTabs);
                }

                // Check if the list contains the tab at the top
                if ((rememberedTabs.Count == 0) || !tabName.Equals(rememberedTabs[rememberedTabs.Count - 1], StringComparison.OrdinalIgnoreCase))
                {
                    // Add the remembered tab to the end of list
                    rememberedTabs.Remove(tabName);
                    rememberedTabs.Add(tabName);
                }
            }
        }


        /// <summary>
        /// Remembers the selected tab if configured
        /// </summary>
        private void RememberTab()
        {
            // Remember selected tab
            if (RememberSelectedTab)
            {
                var selectedTab = GetSelectedTab();
                if (selectedTab != null)
                {
                    RememberTab(selectedTab.TabName);
                }
            }
        }


        /// <summary>
        /// Gets the key to remember the selected tab
        /// </summary>
        /// <param name="moduleName">Module name</param>
        /// <param name="elementName">Element name</param>
        public static void ForgetRememberedTab(string moduleName, string elementName)
        {
            var key = GetRememberKey(moduleName, elementName);

            SessionHelper.Remove(key);
        }

        #endregion

        #endregion


        #region "Scripts related methods"

        /// <summary>
        /// Sets the initialization script
        /// </summary>
        /// <param name="script">Script</param>
        private void SetInitScript(string script)
        {
            if (!String.IsNullOrEmpty(script) && RememberSelectedTab)
            {
                script = ScriptHelper.AddScript("window.disableTabCallback = true;", script);
                script = ScriptHelper.AddScript(script, "window.disableTabCallback = false;");
            }

            mInitScript = script;
        }


        /// <summary>
        /// Register scripts
        /// </summary>
        private void RegisterScripts()
        {
            // Scripts are already registered
            if (mScriptsRegistered)
            {
                return;
            }

            // Set registered flag
            mScriptsRegistered = true;

            ScriptHelper.RegisterLoader(Page);
            ScriptHelper.RegisterModule(Page, "CMS/UITabs", mInitScript);

            string script = GetScripts();

            ControlsHelper.RegisterClientScriptBlock(this, Page, typeof(string), "Tabs_" + ClientID, ScriptHelper.GetScript(script));
        }


        /// <summary>
        /// Gets the scripts for the tabs
        /// </summary>
        private string GetScripts()
        {
            // Specific tab control script
            string prefix = TabControlIdPrefix;
            string postBack = UsePostback ? ControlsHelper.GetPostBackEventReference(this, "#").Replace("'#'", "'selecttab|' + i") : String.Empty;

            var script = String.Format(
@"
function {0}SelTab(i, frm, url, text) {{
    if (Tabs.redir(url, frm{1})) {{
        Tabs.selTab(i, {2}, {3}, text);
        {4}
    }}
}}
",
                prefix,
                (UseIFrame ? ", true" : null),
                ScriptHelper.GetString(ClientID),
                ScriptHelper.GetString(prefix),
                postBack
                );

            // Javascript handler
            var jsHandler = !String.IsNullOrEmpty(JavaScriptHandler) ? String.Format(
                @"
if ({0}) {{ 
    {0}(elementName); 
}}
",
                JavaScriptHandler) : null;

            // Callback to remember the selected tab
            var callback =
                RememberSelectedTab ? String.Format(
                    @"
if (!window.disableTabCallback) {{
    {0}
}}
",
                    Page.ClientScript.GetCallbackEventReference(this, "elementName", "null", null)
                    ) : null;

            script += String.Format(
                @"
function UIT_Selected(elementName) {{
    {0}{1}
}}
",
                callback,
                jsHandler
                );

            return script;
        }


        /// <summary>
        /// Gets the redirection script
        /// </summary>
        /// <param name="url">URL to redirect</param>
        /// <param name="script">Additional script</param>
        private string GetRedirectScript(string url, string script)
        {
            // Ensure correct url otherwise hash check failed
            if ((url != null) && url.Contains("&amp;"))
            {
                url = url.Replace("&amp;", "&");
            }

            return String.Format(
                "Tabs.redir('{0}', '{1}'{3}); {2}",
                ScriptHelper.GetString(ScriptHelper.ResolveUrl(url), false),
                ScriptHelper.GetString(UrlTarget, false),
                script,
                (UseIFrame ? ", true" : null)
            );
        }

        #endregion

        #endregion


        #region "IPostBackEventHandler Members"

        /// <summary>
        /// RaisePostbackEvent handler
        /// </summary>
        public void RaisePostBackEvent(string eventArgument)
        {
            if (String.IsNullOrEmpty(eventArgument))
            {
                return;
            }

            var args = eventArgument.Split('|');
            if (args.Length == 2 && args[0].Equals("selecttab", StringComparison.OrdinalIgnoreCase))
            {
                SelectedTab = args[1].ToInteger(0);
                if (Page.Request.Params[Page.postEventSourceID] == UniqueID)
                {
                    // Raise OnTabClicked
                    OnTabClicked?.Invoke(this, null);
                }
            }
        }

        #endregion


        #region "ICallbackEventHandler Members"

        /// <summary>
        /// Raises the callback event
        /// </summary>
        /// <param name="eventArgument">Event argument</param>
        public void RaiseCallbackEvent(string eventArgument)
        {
            RememberTab(eventArgument);
        }


        /// <summary>
        /// Gets the callback result
        /// </summary>
        public string GetCallbackResult()
        {
            return null;
        }

        #endregion
    }
}