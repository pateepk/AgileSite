using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.Design;
using System.Web.UI.WebControls;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.DocumentEngine.Web.UI.Configuration;
using CMS.Helpers;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// General tab control class.
    /// </summary>
    [ToolboxData("<{0}:BasicTabControl runat=server></{0}:BasicTabControl>")]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class BasicTabControl : CMSWebControl, INamingContainer, IPostBackEventHandler
    {
        #region "Variables"

        private const string CLASS_DOCUMENT = "TabControl";
        private string mRenderedHTML = String.Empty;
        private List<TabItem> mTabItems;
        private string[,] mTabs = null;
        private Literal mLtrScrollPanelContent;
        private bool mScriptsRegistered;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Hides the control when no data is loaded. Default value is False.
        /// </summary>
        [Category("Behavior"), DefaultValue(true), Description("Hides the control when no data loaded. Default value is False.")]
        public virtual bool HideControlForZeroRows
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["HideControlForZeroRows"], false);
            }
            set
            {
                ViewState["HideControlForZeroRows"] = value;
            }
        }


        /// <summary>
        /// If true, target frame is not in parent frames but iframe
        /// </summary>
        public bool UseIFrame
        {
            get;
            set;
        }


        /// <summary>
        /// Text to be shown when the control is hidden by HideControlForZeroRows.
        /// </summary>        
        [Category("Behavior"), DefaultValue(""), Description("Text to be shown when the control is hidden by HideControlForZeroRows.")]
        public virtual string ZeroRowsText
        {
            get
            {
                return ResHelper.LocalizeString(ValidationHelper.GetString(ViewState["ZeroRowsText"], String.Empty));
            }
            set
            {
                ViewState["ZeroRowsText"] = value;
            }
        }


        /// <summary>
        /// Get or set rendered HTML code.
        /// </summary>
        [Category("Behavior"), DefaultValue(true), Description("Get or set rendered HTML code")]
        public string RenderedHTML
        {
            get
            {
                return mRenderedHTML;
            }
            set
            {
                mRenderedHTML = value;
            }
        }


        /// <summary>
        /// Tab control ID prefix
        /// </summary>    
        [Category("Appearance"), DefaultValue(""), Description("Tab control id prefix.")]
        public string TabControlIdPrefix
        {
            get
            {
                return ValidationHelper.GetString(ViewState["TabControlIdPrefix"], String.Empty);
            }
            set
            {
                if (value != null)
                {
                    value = value.Replace(" ", String.Empty);
                }
                ViewState["TabControlIdPrefix"] = value;
            }
        }


        /// <summary>
        /// Horizontal or vertical layout.
        /// </summary>    
        [Category("Appearance"), DefaultValue(TabControlLayoutEnum.Horizontal), Description("Horizontal or vertical layout.")]
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
        public List<TabItem> TabItems
        {
            get
            {
                return mTabItems ?? (mTabItems = new List<TabItem>());
            }
            set
            {
                mTabItems = value;
            }
        }


        /// <summary>
        /// Internal collection of tabs (built either from Tabs or TabItems).
        /// </summary>
        protected List<TabItem> TabItemsInternal
        {
            get
            {
                List<TabItem> tabItemsInternal;
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
                    tabItemsInternal = new List<TabItem>();
                    if (mTabs != null)
                    {
                        int dimension = mTabs.GetUpperBound(1);
                        for (int i = 0; i <= mTabs.GetUpperBound(0); i++)
                        {
                            TabItem tab = new TabItem();
                            tab.Index = i;
                            if (dimension >= 0)
                            {
                                tab.Text = mTabs[i, 0];
                            }
                            if (dimension >= 1)
                            {
                                tab.OnClientClick = mTabs[i, 1];
                            }
                            if (dimension >= 2)
                            {
                                tab.RedirectUrl = mTabs[i, 2];
                            }
                            if (dimension >= 3)
                            {
                                tab.Tooltip = mTabs[i, 3];
                            }
                            if (dimension >= 4)
                            {
                                tab.LeftItemImage = mTabs[i, 4];
                            }
                            if (dimension >= 5)
                            {
                                tab.MiddleItemImage = mTabs[i, 5];
                            }
                            if (dimension >= 6)
                            {
                                tab.RightItemImage = mTabs[i, 6];
                            }
                            if (dimension >= 7)
                            {
                                tab.ItemStyle = mTabs[i, 7];
                            }
                            if (dimension >= 8)
                            {
                                tab.AlternatingCssSuffix = mTabs[i, 8];
                            }
                            if (dimension >= 9)
                            {
                                tab.CssClass = mTabs[i, 9];
                            }
                            if (dimension >= 10)
                            {
                                tab.ImageAlternativeText = mTabs[i, 10];
                            }
                            if (dimension >= 11)
                            {
                                tab.SuppressDefaultOnClientClick = ValidationHelper.GetBoolean(mTabs[i, 11], false);
                            }
                            tabItemsInternal.Add(tab);
                        }
                    }
                }

                // Get empty tabs and remove them from collection
                List<TabItem> tabsToRemove = tabItemsInternal.Where(tab => tab.IsEmpty()).ToList();
                foreach (TabItem tab in tabsToRemove)
                {
                    tabItemsInternal.Remove(tab);
                }

                return tabItemsInternal;
            }
        }


        /// <summary>
        /// Content located before the first tab (in its own cell)
        /// </summary>
        public string BeforeFirstTab
        {
            get;
            set;
        }


        /// <summary>
        /// Content located after the last tab (in its own cell)
        /// </summary>
        public string AfterLastTab
        {
            get;
            set;
        }


        /// <summary>
        /// If is set true, first item will be selected by default if is not some other item selected
        /// </summary>
        [Category("Behavior"), DefaultValue(true), Description("If is set true, first item will be selected by default if is not some other item selected.")]
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
        [Category("Behavior"), DefaultValue(0), Description("Index of the selected tab.")]
        public int SelectedTab
        {
            get
            {
                int defTab = 0;

                if (!SelectFirstItemByDefault)
                {
                    defTab = -1;
                }

                return ValidationHelper.GetInteger(ViewState["SelectedTab"], defTab);
            }
            set
            {
                ViewState["SelectedTab"] = value;
            }
        }


        /// <summary>
        /// Indicates if client script should be generated for each tab.
        /// </summary>
        [Category("Behavior"), DefaultValue(false), Description("Indicates if client script should be generated for each tab.")]
        public bool UseClientScript
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["useClientScript"], false);
            }
            set
            {
                ViewState["useClientScript"] = value;
            }
        }


        /// <summary>
        /// Indicates if postback is fired when tab is clicked.
        /// </summary>
        [Category("Behavior"), DefaultValue(false), Description("Indicates if postback is fired when tab is clicked.")]
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
        [Category("Behavior"), DefaultValue(""), Description("If URL for tab items is set, this property specifies target frame for all URLs.")]
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


        /// <summary>
        /// Render the link title attribute?
        /// </summary>
        [Category("Behavior"), Description("Render the title attribute within the menu links.")]
        public bool RenderLinkTitle
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["RenderLinkTitle"], false);
            }
            set
            {
                ViewState["RenderLinkTitle"] = value;
            }
        }


        /// <summary>
        /// If true, the tab renders the link as well as the javascript
        /// </summary>
        public bool RenderLinks
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether the tab control should support inbuilt scrolling.
        /// </summary>
        [Category("Behavior"), DefaultValue(false), Description("Indicates whether the tab control should support inbuilt scrolling.")]
        public bool EnableScrolling
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["EnableScrolling"], false);
            }
            set
            {
                ViewState["EnableScrolling"] = value;
            }
        }


        /// <summary>
        /// Gets or sets the CSS style used for the scroll panel wrapper tag.
        /// </summary>
        public string ScrollPanelCss
        {
            get;
            set;
        }


        /// <summary>
        /// Event for tab clicked.
        /// </summary>
        public event EventHandler OnTabClicked;

        #endregion


        #region "Control events"

        /// <summary>
        /// Create child controls
        /// </summary>
        protected override void CreateChildControls()
        {
            // Use scroll panel when required
            if (EnableScrolling)
            {
                // Forward scroller control
                Panel pnlForward = new Panel
                {
                    ID = "pnlForward",
                    CssClass = "FormCategoryForwardScroller"
                };

                // Backward scroller control
                Panel pnlBackward = new Panel
                {
                    ID = "pnlBackward",
                    CssClass = "FormCategoryBackwardScroller"
                };

                ScrollPanel scrollPanel = new ScrollPanel()
                {
                    Layout = (RepeatDirection)TabControlLayout,
                    CssClass = ScrollPanelCss,
                    ForwardScrollerControlID = pnlForward.ID,
                    BackwardScrollerControlID = pnlBackward.ID,
                    ScrollStep = 300
                };

                // Literal - used as a scroll panel container for rendered html
                mLtrScrollPanelContent = new Literal();
                scrollPanel.Controls.Add(mLtrScrollPanelContent);

                Controls.Add(pnlForward);
                Controls.Add(pnlBackward);
                Controls.Add(scrollPanel);
            }

            base.CreateChildControls();

            if (RequestHelper.IsPostBack())
            {
                if (Page != null)
                {
                    if (Page.Request.Params[Page.postEventSourceID] == UniqueID)
                    {
                        if (UsePostback)
                        {
                            // Raise OnTabClicked
                            if (OnTabClicked != null)
                            {
                                OnTabClicked(this, null);
                            }
                        }
                    }

                    if (ControlsHelper.IsInUpdatePanel(this))
                    {
                        ControlsHelper.UpdateCurrentPanel(this);
                    }
                }
            }
        }


        /// <summary>
        /// OnPreRender override - register javascript functions
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            RegisterScripts();

            base.OnPreRender(e);
        }





        /// <summary>
        /// Renders the control.
        /// </summary>
        protected override void Render(HtmlTextWriter output)
        {
            if (mRenderedHTML == String.Empty)
            {
                GenerateMenu();
            }

            // Hide control for zero rows or display zero rows text
            if ((TabItemsInternal == null) || (TabItemsInternal.Count == 0))
            {
                if (HideControlForZeroRows)
                {
                    return;
                }

                if (string.IsNullOrEmpty(ZeroRowsText))
                {
                    return;
                }
                output.Write(ZeroRowsText);
                return;
            }

            if (EnableScrolling)
            {
                // Render the tab content into the scroll panel
                if (mLtrScrollPanelContent != null)
                {
                    mLtrScrollPanelContent.Text = RenderedHTML;
                }

                base.Render(output);
            }
            else
            {
                // Render only the generated html
                output.Write(mRenderedHTML);
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Verifies the selected tab index making sure that it doesn't exceed the number of tabs
        /// </summary>
        protected int VerifySelectedTab()
        {
            // Make sure that the selected tab is not out of range, default to first tab if it is
            int selTab = SelectedTab;
            if (selTab >= TabItemsInternal.Count)
            {
                SelectedTab = 0;
                selTab = 0;
            }
            return selTab;
        }


        /// <summary>
        /// Register scrips
        /// </summary>
        protected void RegisterScripts()
        {
            // Scripts are already registered
            if (mScriptsRegistered)
            {
                return;
            }

            // Set registered flag
            mScriptsRegistered = true;

            // Create target based on iframe usage
            String target = UseIFrame ? "frame = frames[target]" : @"if (parent && parent.frames) {
                                                                        frame = parent.frames[target];
                                                                    }";
            // Common script for all tab controls
            string script =
@"function BTC_Redir(url, target) {
    if (url != '') {
        if ((target == '_blank') || (target == '_new')) {
            window.open(url);
            return true;
        }
        else if (target == '_self') {
            this.location.href = url;
            return true;
        }
        else if (target != '') {
            var frame;"
            + target + @"
            try {
                if (!frame || (frame.CheckChanges && !frame.CheckChanges())) {
                    return false;
                }
            }
            catch (ex) {
                // When not a web page
            }
            frame.location.href = url;
            if (typeof (frame.focus) == 'function') {
                frame.focus();
            }
            return true;
        }
        else {
            parent.location.href = url;
            return true;
        }
    }

    return true;
}

function Get(id) {
    return document.getElementById(id);
}      

function SetClass(id, cl) {
    var el = Get(id);
    if (el != null) {
        el.className = cl;
    }
}
 
function BTC_SelTab(i, clientId, p) { 
    var elem = Get(clientId + '_SelectedTab'); 
    if (elem) { elem.value = i; }

    var old = Get(p + 'TabControlSelItemNo').value; 
    if (old != '') 
    { 
        try {
            SetClass(p + 'TabItem_'+ old, 'TabControl');
            SetClass(p + 'TabLeft_'+ old, 'TabControlLeft'); 
            SetClass(p + 'TabRight_'+ old, 'TabControlRight');
            SetClass(p + 'TabLink_'+ old, 'TabControlLink');
        } catch (e) {}
    } 

    // select chosen tab
    SetClass(p + 'TabItem_'+ i, 'TabControlSelected'); 
    SetClass(p + 'TabLeft_'+ i, 'TabControlSelectedLeft'); 
    SetClass(p + 'TabRight_'+ i, 'TabControlSelectedRight');

    Get(p + 'TabControlSelItemNo').value = i; 
    SetClass(p + 'TabLink_'+ i, 'TabControlLinkSelected');
}";

            ControlsHelper.RegisterClientScriptBlock(this, Page, typeof(string), "BasicTabControl", ScriptHelper.GetScript(script));

            // Specific tab control script
            string prefix = TabControlIdPrefix;
            string postBack = UsePostback ? ControlsHelper.GetPostBackEventReference(this, "#").Replace("'#'", "'selecttab|' + i") : String.Empty;

            script =
                @"function " + prefix + @"SelTab(i, frm, url) {
    if (BTC_Redir(url, frm)) {
        BTC_SelTab(i, " + ScriptHelper.GetString(ClientID) + ", " + ScriptHelper.GetString(prefix) + @");
        " + postBack + @"
    }
}";

            ControlsHelper.RegisterClientScriptBlock(this, Page, typeof(string), prefix + "SelTab", ScriptHelper.GetScript(script));
        }


        /// <summary>
        /// Adds a tab.
        /// </summary>
        /// <param name="tab">Tab</param>
        public void AddTab(TabItem tab)
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


        /// <summary>
        /// Inserts tab at specified index.
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="tab">Tab</param>
        public void InsertTab(int index, TabItem tab)
        {
            if (tab == null)
            {
                return;
            }

            tab.Index = index;
            AddTab(tab);
        }

        /// <summary>
        /// Gets the redirection script
        /// </summary>
        /// <param name="url">URL to redirect</param>
        /// <param name="script">Additional script</param>
        public string GetRedirScript(string url, string script)
        {
            // Ensure correct url otherwise hash check failed
            if ((url != null) && url.Contains("&amp;"))
            {
                url = url.Replace("&amp;", "&");
            }

            return String.Format("BTC_Redir('{0}', '{1}'); {2}", ScriptHelper.GetString(url, false), ScriptHelper.GetString(UrlTarget, false), script);
        }


        /// <summary>
        /// Generate menu
        /// </summary>
        protected void GenerateMenu()
        {
            if (TabItemsInternal == null)
            {
                return;
            }

            int selTab = VerifySelectedTab();

            // Check the count
            int count = TabItemsInternal.Count;
            if (count < 0)
            {
                return;
            }

            mRenderedHTML = String.Empty;

            string prefix = TabControlIdPrefix;
            string mClass = CLASS_DOCUMENT;

            StringBuilder sb = new StringBuilder(200 + count * 100);

            // Basic menu code
            sb.Append("<input type=\"hidden\" id=\"", prefix, "TabControlSelItemNo\" value=\"", selTab, "\" />");
            sb.Append("<input type=\"hidden\" name=\"", ClientID, "_SelectedTab\" id=\"", ClientID, "_SelectedTab\" value=\"", selTab, "\" />");

            sb.Append("<table cellspacing=\"0\" class=\"", mClass, "Table\" ");

            if (TabControlLayout == TabControlLayoutEnum.Vertical)
            {
                sb.Append("width=\"100%\" ");
            }

            sb.Append(">");

            // Generate the pages
            for (int i = 0; i < count; i++)
            {
                mClass = CLASS_DOCUMENT;
                TabItem currentTab = TabItemsInternal[i];
                // DocumentMenuClass
                if ((currentTab.CssClass != null) && (currentTab.CssClass.Trim() != string.Empty))
                {
                    mClass = currentTab.CssClass;
                }

                // If URL is defined
                if ((currentTab.RedirectUrl != null) || UsePostback)
                {
                    bool first = (i == 0);
                    if (first || (TabControlLayout == TabControlLayoutEnum.Vertical))
                    {
                        // Content before the first tab
                        if (first && !String.IsNullOrEmpty(BeforeFirstTab))
                        {
                            if (TabControlLayout == TabControlLayoutEnum.Vertical)
                            {
                                sb.Append("<tr class=\"", mClass, "Row\"><td class=\"", mClass, "Left\">&nbsp;</td><td class=\"", mClass, "\">", BeforeFirstTab, "</td></tr>");
                            }
                            else
                            {
                                sb.Append("<td>", BeforeFirstTab, "</td>");
                            }
                        }

                        sb.Append("<tr class=\"", mClass, "Row\">");
                    }

                    string className = mClass;
                    if (selTab == i)
                    {
                        className += "Selected";
                    }
                    sb.Append("<td id=\"", prefix, "TabLeft_", i, "\" class=\"", className, "Left");

                    // Alternating styles. Append 'Alt' or ''.
                    if (currentTab.AlternatingCssSuffix != null)
                    {
                        sb.Append(currentTab.AlternatingCssSuffix);
                    }
                    sb.Append("\"");

                    string click = null;

                    // Javascript
                    if (UseClientScript)
                    {
                        string defaultOnClientClick = null;
                        if (!currentTab.SuppressDefaultOnClientClick)
                        {
                            defaultOnClientClick = prefix + "SelTab(" + i + ",'" + UrlTarget + "', '" + ScriptHelper.GetString(ScriptHelper.ResolveUrl(currentTab.RedirectUrl), false) + "');";
                        }
                        click = " onclick=\"" + defaultOnClientClick + currentTab.OnClientClick + "\"";
                        sb.Append(click);
                    }

                    sb.Append(">&nbsp;</td>");
                    sb.Append("<td id=\"", prefix, "TabItem_", i, "\" class=\"", className);

                    // Alternating styles. Append 'Alt' or ''.
                    if (currentTab.AlternatingCssSuffix != null)
                    {
                        sb.Append(currentTab.AlternatingCssSuffix);
                    }

                    sb.Append("\" ");

                    // Title
                    if (!String.IsNullOrEmpty(currentTab.Tooltip))
                    {
                        sb.Append(" title=\"", HTMLHelper.HTMLEncode(currentTab.Tooltip), "\"");
                    }

                    // Javascript
                    if (UseClientScript)
                    {
                        sb.Append(click);
                    }

                    // Index was out of
                    if (currentTab.ItemStyle != null)
                    {
                        sb.Append(AddStyle(currentTab.ItemStyle));
                    }
                    sb.Append(">");

                    // Left Image
                    if (!string.IsNullOrEmpty(currentTab.LeftItemImage))
                    {
                        string alt = String.Empty;
                        if (currentTab.ImageAlternativeText != null)
                        {
                            alt = currentTab.ImageAlternativeText;
                        }
                        sb.Append("<img border=\"0\" ", alt, " src=\"", currentTab.LeftItemImage, "\" />");
                    }

                    if ((currentTab.RedirectUrl != null) && (currentTab.RedirectUrl.Trim() != String.Empty))
                    {
                        if (!UseClientScript || RenderLinks)
                        {
                            sb.Append("<a id=\"", prefix, "TabLink_", i, "\" class=\"");
                            if (selTab == i)
                            {
                                sb.Append(mClass + "LinkSelected");
                            }
                            else
                            {
                                sb.Append(mClass + "Link");
                            }

                            sb.Append("\" href=\"", HTMLHelper.EncodeForHtmlAttribute(currentTab.RedirectUrl), "\"");

                            if (UrlTarget != String.Empty)
                            {
                                sb.Append(" target=\"", UrlTarget, "\"");
                            }
                            if (currentTab.ItemStyle != null)
                            {
                                sb.Append(AddStyle(currentTab.ItemStyle));
                            }

                            // Render link title
                            if (RenderLinkTitle)
                            {
                                sb.Append(" title=\"", HTMLHelper.HTMLEncode(currentTab.Text), "\" ");
                            }

                            sb.Append(">");
                        }
                        else
                        {
                            sb.Append("<span class=\"" + mClass + "Link\" >");
                        }

                        // Main Image
                        if (currentTab.MiddleItemImage != null)
                        {
                            if (!string.IsNullOrEmpty(currentTab.MiddleItemImage))
                            {
                                string alt = String.Empty;
                                if (currentTab.ImageAlternativeText != null)
                                {
                                    alt = currentTab.ImageAlternativeText;
                                }
                                sb.Append("<img border=\"0\" ", alt, " src=\"", currentTab.MiddleItemImage, "\" />");
                            }
                            else
                            {
                                sb.Append(currentTab.Text);
                            }
                        }
                        else
                        {
                            sb.Append(currentTab.Text);
                        }

                        if (!UseClientScript || RenderLinks)
                        {
                            sb.Append("</a>");
                        }
                        else
                        {
                            sb.Append("</span>");
                        }
                    }
                    else
                    {
                        // Main Image
                        if (currentTab.MiddleItemImage != null)
                        {
                            if (!string.IsNullOrEmpty(currentTab.MiddleItemImage))
                            {
                                string alt = String.Empty;
                                if (currentTab.ImageAlternativeText != null)
                                {
                                    alt = currentTab.ImageAlternativeText;
                                }
                                sb.Append("<img border=\"0\" ", alt, " src=\"", currentTab.MiddleItemImage, "\" />");
                            }
                            else
                            {
                                sb.Append(currentTab.Text);
                            }
                        }
                        else
                        {
                            sb.Append(currentTab.Text);
                        }
                    }

                    // Right image
                    // Main Image
                    if (!string.IsNullOrEmpty(currentTab.RightItemImage))
                    {
                        string alt = String.Empty;
                        if (currentTab.ImageAlternativeText != null)
                        {
                            alt = currentTab.ImageAlternativeText;
                        }
                        sb.Append("<img border=\"0\" ", alt, " src=\"", currentTab.RightItemImage, "\" />");
                    }

                    sb.Append("</td>\n");

                    if (TabControlLayout != TabControlLayoutEnum.Vertical)
                    {
                        sb.Append("<td id=\"", prefix, "TabRight_", i, "\" class=\"", className, "Right");

                        // Alternating styles. Append 'Alt' or ''.
                        if (currentTab.AlternatingCssSuffix != null)
                        {
                            sb.Append(currentTab.AlternatingCssSuffix);
                        }
                        sb.Append("\"");

                        // Javascript
                        if (UseClientScript)
                        {
                            sb.Append(click);
                        }

                        // Append style
                        if (currentTab.ItemStyle != null)
                        {
                            sb.Append(AddStyle(currentTab.ItemStyle));
                        }

                        sb.Append(">&nbsp;</td>");
                    }

                    // End the row
                    bool last = (i + 1 == TabItemsInternal.Count);
                    if (last || (TabControlLayout == TabControlLayoutEnum.Vertical))
                    {
                        // Content after the last tab
                        if (last && !String.IsNullOrEmpty(AfterLastTab))
                        {
                            if (TabControlLayout == TabControlLayoutEnum.Vertical)
                            {
                                sb.Append("<tr class=\"", mClass, "Row\"><td class=\"", mClass, "Left\">&nbsp;</td><td class=\"", mClass, "\">", AfterLastTab, "</td></tr>");
                            }
                            else
                            {
                                sb.Append("<td>", AfterLastTab, "</td>");
                            }
                        }

                        sb.Append("</tr>\n");
                    }
                }
            }

            sb.Append("</table>\n");

            mRenderedHTML = sb.ToString();
        }


        /// <summary>
        /// Add style from string
        /// </summary>
        /// <param name="styleText">Style expression</param>
        private static string AddStyle(string styleText)
        {
            string result = String.Empty;

            if (!String.IsNullOrEmpty(styleText))
            {
                result += " style=\"";
                result += styleText;
                result += "\"";
            }

            return result;
        }


        /// <summary>
        /// Returns TabControlLayoutEnum value derived from name
        /// </summary>
        public static TabControlLayoutEnum GetTabMenuLayout(string layout)
        {
            if (layout == null)
            {
                return TabControlLayoutEnum.Horizontal;
            }
            switch (layout.ToLowerCSafe())
            {
                case "vertical":
                    return TabControlLayoutEnum.Vertical;

                // Horizontal ID default value
                default:
                    return TabControlLayoutEnum.Horizontal;
            }
        }

        #endregion


        #region "IPostBackEventHandler Members"

        /// <summary>
        /// RaisePostbackEvent handler
        /// </summary>
        public void RaisePostBackEvent(string eventArgument)
        {
            if (!String.IsNullOrEmpty(eventArgument))
            {
                string[] args = eventArgument.Split('|');

                if ((args.Length == 2))
                {
                    switch (args[0].ToLowerCSafe())
                    {
                        case "selecttab":
                            SelectedTab = args[1].ToInteger(0);
                            break;
                    }
                }
            }
        }

        #endregion
    }
}
