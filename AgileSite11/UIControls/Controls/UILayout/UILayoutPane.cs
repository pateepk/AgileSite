using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Security.Permissions;
using System.Text;
using System.Web;
using System.Web.UI;

using CMS.Base.Web.UI;
using CMS.Helpers;
using CMS.IO;
using CMS.MacroEngine;
using CMS.Membership;
using CMS.Modules;
using CMS.SiteProvider;

namespace CMS.UIControls
{
    /// <summary>
    /// Part of UI layout (can be center or one of cardinal directions).
    /// </summary>
    [ParseChildren(typeof(UILayoutPane), ChildrenAsProperties = true),
    AspNetHostingPermission(SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal),
    AspNetHostingPermission(SecurityAction.Demand, Level = AspNetHostingPermissionLevel.Minimal),
    DefaultProperty("Template"),
    ToolboxData("<{0}:UILayoutPane runat=\"server\"></{0}:UILayoutPane>")]
    public class UILayoutPane : CMSUserControl
    {
        #region "Constants"

        /// <summary>
        /// Constant for automatic size of a pane. 
        /// </summary>
        public const string PANE_AUTO_SIZE = "auto";

        #endregion


        #region "Private variables"

        private string mNoFrames;
        private bool? mUsePseudoCloseCallback;
        private string mSrc;
        private string mControlID;
        private InlineUserControl mUserControl;

        #endregion


        #region "Public properties"

        #region "Behavior"

        /// <summary>
        /// Indicates whether existence of ascx file should be checked
        /// </summary>
        public bool CheckPhysicalFile
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether append Src to layout pane. Used in situations when URL is set by another control.
        /// </summary>
        public bool AppendSrc
        {
            get;
            set;
        } = true;


        /// <summary>
        /// Determines whether to use 'PseudoClose' callback for closing the pane.
        /// By default the value is true only when RenderAs==HtmlTextWriterTag.Iframe.
        /// </summary>
        public bool UsePseudoCloseCallback
        {
            get
            {
                if (mUsePseudoCloseCallback == null)
                {
                    mUsePseudoCloseCallback = (RenderAs == HtmlTextWriterTag.Iframe);
                }
                return mUsePseudoCloseCallback.Value;
            }
            set
            {
                mUsePseudoCloseCallback = value;
            }
        }


        /// <summary>
        /// Can a pane be closed?
        /// </summary>
        public bool? Closable
        {
            get;
            set;
        }


        /// <summary>
        /// When open, can a pane be resized?
        /// </summary>
        public bool? Resizable
        {
            get;
            set;
        }


        /// <summary>
        /// When closed, can a pane 'slide open' over adjacent panes?
        /// </summary>
        public bool? Slidable
        {
            get;
            set;
        }


        /// <summary>
        /// If 'true', then when moused-over, the pane's zIndex is raised and overflow is set to 'visible'. This allows pop-ups and drop-downs to overlap adjacent panes.
        /// WARNING: Enable this only for panes that do not scroll!
        /// </summary>
        public bool? ShowOverflowOnHover
        {
            get;
            set;
        }


        /// <summary>
        /// When enabled, layout will 'mask' iframes on the page when the resizer-bar is 'dragged' to resize a pane. 
        /// This solved problems related to dragging an element over an iframe. 
        /// </summary>
        public bool? MaskContents
        {
            get;
            set;
        }


        /// <summary>
        /// Triggers child-layout.resizeAll() when this pane is resized.
        /// </summary>
        public bool? ResizeChildLayout
        {
            get;
            set;
        }


        /// <summary>
        /// LIVE Resizing as resizer is dragged.
        /// </summary>
        public bool? LivePaneResizing
        {
            get;
            set;
        }


        /// <summary>
        /// True = re-measure header/footer heights as resizer is dragged.
        /// </summary>
        public bool? LiveContentResizing
        {
            get;
            set;
        }


        /// <summary>
        /// True = trigger onresize callback REPEATEDLY if LivePaneResizing==true.
        /// </summary>
        public bool? TriggerEventsDuringLiveResize
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the pane is rendered
        /// </summary>
        public bool RenderPane
        {
            get;
            set;
        } = true;

        #endregion


        #region "JS events"

        /// <summary>
        /// Script or function name called before 'show' is done.
        /// </summary>
        public string OnShowStartScript
        {
            get;
            set;
        }


        /// <summary>
        /// Script or function name called after 'show' is done.
        /// </summary>
        public string OnShowEndScript
        {
            get;
            set;
        }


        /// <summary>
        /// Script or function name called before 'hide' is done.
        /// </summary>
        public string OnHideStartScript
        {
            get;
            set;
        }


        /// <summary>
        /// Script or function name called after 'hide' is done.
        /// </summary>
        public string OnHideEndScript
        {
            get;
            set;
        }


        /// <summary>
        /// Script or function name called before 'open' is done.
        /// </summary>
        public string OnOpenStartScript
        {
            get;
            set;
        }


        /// <summary>
        /// Script or function name called after 'open' is done.
        /// </summary>
        public string OnOpenEndScript
        {
            get;
            set;
        }


        /// <summary>
        /// Script or function name called before 'close' is done.
        /// </summary>
        public string OnCloseStartScript
        {
            get;
            set;
        }


        /// <summary>
        /// Script or function name called after 'close' is done.
        /// </summary>
        public string OnCloseEndScript
        {
            get;
            set;
        }


        /// <summary>
        /// Script or function name called after 'resize' is done.
        /// </summary>
        public string OnResizeEndScript
        {
            get;
            set;
        }


        /// <summary>
        /// Script or function name called before 'resize' is done.
        /// </summary>
        public string OnResizeStartScript
        {
            get;
            set;
        }

        #endregion


        #region "Effects"

        /// <summary>
        /// Animation effect for open/close. 
        /// Choose a preset effect OR can specify a custom fxName as long as you also specify fxSettings (even if fxSettings is just empty - {}).
        /// </summary>
        public string FxName
        {
            get;
            set;
        }


        /// <summary>
        /// Speed of animations – standard jQuery keyword like 'fast', or a millisecond value.
        /// </summary>
        public string FxSpeed
        {
            get;
            set;
        }


        /// <summary>
        /// If a non-standard effect is specified, then fxSettings is REQUIRED (can be empty though).
        /// </summary>
        public string FxSettings
        {
            get;
            set;
        }

        #endregion


        #region "Selectors"

        /// <summary>
        /// Selector string for INNER div/element. This div will auto-size so only it scrolls, and not the entire pane. 
        /// </summary>
        public string ContentSelector
        {
            get;
            set;
        }


        /// <summary>
        /// Selector string for INNER divs/elements. These elements will be 'ignored' when calculations are done to auto-size the content element. 
        /// This may be necessary if there are elements inside the pane that are absolutely-positioned and intended to 'overlay' other elements. 
        /// </summary>
        public string ContentIgnoreSelector
        {
            get;
            set;
        }

        #endregion


        #region "Styling"

        /// <summary>
        /// When enabled, the layout will apply basic styles directly to resizers &amp; buttons.
        /// </summary>
        public bool? ApplyDemoStyles
        {
            get;
            set;
        }



        /// <summary>
        /// Prefix used for stylesheet classes related to this pane.
        /// </summary>
        public string PaneClass
        {
            get;
            set;
        }


        /// <summary>
        /// Prefix used for auto-generated classNames for each resizer bar.
        /// </summary>
        public string ResizerClass
        {
            get;
            set;
        }


        /// <summary>
        /// Prefix used for auto-generated classNames for each toggler buttons. 
        /// </summary>
        public string TogglerClass
        {
            get;
            set;
        }


        /// <summary>
        /// Prefix used for auto-generated classNames for each custom buttons. 
        /// </summary>
        public string ButtonClass
        {
            get;
            set;
        }


        /// <summary>
        /// Cursor when the mouse is over the resizer bar.
        /// </summary>
        public string ResizerCursor
        {
            get;
            set;
        }


        /// <summary>
        /// Cursor when resizer-bar triggers 'sliding open' (when pane is closed).
        /// </summary>
        public string SliderCursor
        {
            get;
            set;
        }


        /// <summary>
        /// Opacity of resizer bar when dragging to resize a pane. 
        /// </summary>
        public int? ResizerDragOpacity
        {
            get;
            set;
        }

        #endregion


        #region "Texts"

        /// <summary>
        /// Tooltip shown when resizer-bar can be 'dragged' to resize a pane.
        /// </summary>
        public string ResizerTip
        {
            get;
            set;
        }


        /// <summary>
        /// Tooltip when the resizer-bar triggers 'sliding open'.
        /// </summary>
        public string SliderTip
        {
            get;
            set;
        }

        #endregion


        #region "Sizes"

        /// <summary>
        /// Specifies the initial size of the panes - 'height' for north &amp; south panes - 'width' for east and west.
        /// If PANE_AUTO_SIZE, then pane will size to fit its content - most useful for north/south panes (to auto-fit your banner or toolbar), but also works for east/west panes.
        /// You can use either absolute or percentage values.
        /// </summary>
        public string Size
        {
            get;
            set;
        }


        /// <summary>
        /// Maximum-size limit when resizing a pane (0 = as large as pane can go).
        /// </summary>
        public int? MaxSize
        {
            get;
            set;
        }


        /// <summary>
        /// Spacing between pane and adjacent pane when pane is open.
        /// </summary>
        public int? SpacingOpen
        {
            get;
            set;
        }


        /// <summary>
        /// Spacing between pane and adjacent pane - when pane is 'open' or 'closed'
        /// </summary>
        public int? SpacingClosed
        {
            get;
            set;
        }


        /// <summary>
        /// Length of toggler-button when pane is 'open'. 
        /// </summary>
        public int? TogglerLengthOpen
        {
            get;
            set;
        }


        /// <summary>
        /// Length of toggler-button when pane is 'closed'. 
        /// </summary>
        public int? TogglerLengthClosed
        {
            get;
            set;
        }


        /// <summary>
        /// Minimum-size limit when resizing a pane (0 = as small as pane can go).
        /// </summary>
        public int? MinSize
        {
            get;
            set;
        }

        #endregion


        #region "Rendering options"


        /// <summary>
        /// 
        /// </summary>
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public List<UILayoutValue> Values
        {
            get;
            set;
        } = new List<UILayoutValue>();


        /// <summary>
        /// HTML tag representing the pane. Use block elements or iframe.
        /// </summary>
        public HtmlTextWriterTag RenderAs
        {
            get;
            set;
        } = HtmlTextWriterTag.Div;


        /// <summary>
        /// Direction (location) of the pane.
        /// </summary>
        public PaneDirectionEnum Direction
        {
            get;
            set;
        } = PaneDirectionEnum.Center;


        /// <summary>
        /// Indicates if the pane is 'closed' when layout is created
        /// </summary>
        public bool InitClosed
        {
            get;
            set;
        }


        #region "Template"

        /// <summary>
        /// Container to instantiate the template in.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Control Container
        {
            get;
            private set;
        }


        /// <summary>
        /// Template containing child controls.
        /// </summary>
        [Browsable(false), PersistenceMode(PersistenceMode.InnerProperty), DefaultValue(typeof(ITemplate), ""), Description("Control template")]
        public virtual ITemplate Template
        {
            get;
            set;
        }

        #endregion


        #region "Control"

        /// <summary>
        /// Allows to get or set a relative path to custom control (e.g. ~/Controls/Control.ascx). 
        /// This control will be displayed instead of UILayoutPane's template. (It has higher priority.)
        /// </summary>
        public string ControlPath
        {
            get;
            set;
        }


        /// <summary>
        /// Control ID
        /// </summary>
        public string ControlID
        {
            get
            {
                if ((mControlID == null) && (ControlPath != null))
                {
                    int slashIndex = ControlPath.LastIndexOf("/", StringComparison.Ordinal);
                    if (ControlPath.Length > slashIndex + 1)
                    {
                        string fileName = ControlPath.Substring(slashIndex + 1);
                        fileName = fileName.Substring(0, fileName.IndexOf(".", StringComparison.Ordinal));
                        mControlID = fileName;
                    }
                }
                if (string.IsNullOrEmpty(mControlID))
                {
                    mControlID = "nestedControl";
                }
                return mControlID;
            }
            set
            {
                mControlID = value;
            }
        }


        /// <summary>
        /// If true, control is wrapped in UpdatePanel.
        /// </summary>
        public bool UseUpdatePanel
        {
            get;
            set;
        } = true;

        #endregion


        #region "IFrame"

        /// <summary>
        /// Represents src attribute of the rendered element. Applied when RenderAs==HtmlTextWriterTag.Iframe.
        /// </summary>
        public string Src
        {
            get
            {
                if (mSrc == null)
                {
                    List<UITabItem> tabs = null;
                    int selectedTab = 0;

                    if (!String.IsNullOrEmpty(ModuleName))
                    {
                        // Reserve log item
                        DataRow sdr = SecurityDebug.StartSecurityOperation("LoadUILayoutPane");

                        // Get UI elements which represent the tabs
                        DataSet ds = String.IsNullOrEmpty(ElementName) 
                                        ? UIElementInfoProvider.GetRootChildUIElements(ModuleName) 
                                        : UIElementInfoProvider.GetChildUIElements(ModuleName, ElementName);

                        // Get current user
                        var currentUser = MembershipContext.AuthenticatedUser;
                        int i = 0;
                        if (!DataHelper.DataSourceIsEmpty(ds))
                        {
                            tabs = new List<UITabItem>(ds.Tables[0].Rows.Count);
                            foreach (DataRow dr in ds.Tables[0].Rows)
                            {
                                string elementName = ValidationHelper.GetString(dr["ElementName"], string.Empty);

                                if (currentUser.IsAuthorizedPerUIElement(ModuleName, elementName, ModuleAvailabilityForSiteRequired))
                                {
                                    // Get parameters of the tab
                                    var tab = new UITabItem();

                                    // Get element parameters
                                    tab.RedirectUrl = UrlResolver.ResolveUrl(MacroResolver.Resolve(ValidationHelper.GetString(dr["ElementTargetUrl"], string.Empty)));

                                    // Handle additional initialization of the tab
                                    UIElementInfo uiElem = new UIElementInfo(dr);
                                    tab = RaiseTabCreated(uiElem, tab, i);

                                    CMSPage page = Page as CMSPage;
                                    if ((page != null) && (tab != null))
                                    {
                                        tab = page.RaiseTabCreated(uiElem, tab, i);
                                    }

                                    // If tab initialized, add it to the tab collection
                                    if (tab != null)
                                    {
                                        tabs.Add(tab);

                                        // Preselect tab by URL, if query parameter name is set
                                        if ((!String.IsNullOrEmpty(QueryParameterName)) && elementName.Equals(QueryHelper.GetString(QueryParameterName, string.Empty), StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            selectedTab = i;
                                        }

                                        i++;
                                    }
                                }
                            }
                        }

                        // Log the security
                        if (sdr != null)
                        {
                            SecurityDebug.FinishSecurityOperation(sdr, currentUser.UserName, ModuleName, ElementName, i, SiteContext.CurrentSiteName);
                        }
                    }

                    string url;

                    // Call the script for tab which is selected
                    if ((tabs != null) && (tabs.Count > 0))
                    {
                        url = tabs[selectedTab].RedirectUrl;
                        if (!String.IsNullOrEmpty(url) && !url.Contains("?"))
                        {
                            url += RequestContext.CurrentQueryString;
                        }
                    }
                    else
                    {
                        string resource = null;
                        string permission = null;
                        bool missingPermission = false;

                        CMSPage page = Page as CMSPage;
                        if (page != null)
                        {
                            missingPermission = page.RaiseCheckTabSecurity(ref resource, ref permission);
                        }

                        // Permission is required
                        if (missingPermission)
                        {
                            url = URLHelper.ResolveUrl(AdministrationUrlHelper.GetAccessDeniedUrl(resource, permission, null));
                        }
                        // No tab is visible
                        else
                        {
                            url = URLHelper.ResolveUrl(AdministrationUrlHelper.GetInformationUrl("uiprofile.uinotavailable"));
                        }
                    }

                    mSrc = url;
                }
                return mSrc;
            }
            set
            {
                mSrc = value;
            }
        }



        /// <summary>
        /// String that is rendered only along with iframe.
        /// </summary>
        public string NoFrames
        {
            get
            {
                return mNoFrames ?? (mNoFrames = GetString("general.noiframe"));
            }
            set
            {
                mNoFrames = GetString(value);
            }
        }


        /// <summary>
        /// Indicates whether frame border should be rendered
        /// </summary>
        public bool FrameBorder
        {
            get;
            set;
        }

        #endregion

        #endregion


        #region "UI elements"

        /// <summary>
        /// Tab creation event.
        /// </summary>
        public event UITabs.TabCreatedEventHandler OnTabCreated;


        /// <summary>
        /// Code name of the module.
        /// </summary>
        public string ModuleName
        {
            get;
            set;
        }


        /// <summary>
        /// Code name of the UIElement.
        /// </summary>
        public string ElementName
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if site availability of the corresponding module (module with name in format "cms.[ElementName]") is required for each UI element in the menu. Takes effect only when corresponding module exists.
        /// </summary>
        public bool ModuleAvailabilityForSiteRequired
        {
            get;
            set;
        }


        /// <summary>
        /// Name of URL query parameter that is used for tab preselection.
        /// </summary>
        public string QueryParameterName
        {
            get;
            set;
        }

        #endregion


        #region "General"

        /// <summary>
        /// Control for renderascontrol mode. 
        /// </summary>
        public InlineUserControl UserControl
        {
            get
            {
                EnsureChildControls();
                return mUserControl;
            }
        }

        #endregion


        #endregion


        #region "Page events"

        /// <summary>
        /// Load event handler
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            EnsureChildControls();
        }


        /// <summary>
        /// Render event handler
        /// </summary>
        protected override void Render(HtmlTextWriter writer)
        {
            if (DesignMode)
            {
                writer.Write("[UILayoutPane: " + ID + " ]");
            }
            else if (RenderPane)
            {
                // Render opening tag
                writer.Write(GetStartTag());
                if (RenderAs == HtmlTextWriterTag.Iframe)
                {
                    // Render NoFrames text
                    writer.Write(NoFrames);
                }
                else
                {
                    // Render control itself
                    base.Render(writer);
                }
                // Render closing tag
                writer.Write(GetEndTag());
            }
        }


        /// <summary>
        /// Creates child controls.
        /// </summary>
        protected override void CreateChildControls()
        {
            Controls.Clear();
            Container = new Control();

            if (ControlPath != null)
            {
                // Make sure the control is present
                if (CheckPhysicalFile && !File.Exists(Server.MapPath(ControlPath)))
                {
                    Visible = false;
                    return;
                }

                // Dynamically load control and add it to Controls collection
                InlineUserControl control = (InlineUserControl)Page.LoadUserControl(ControlPath);
                control.IsLiveSite = false;
                control.ID = ControlID;
                mUserControl = control;

                // Initialize values
                foreach (UILayoutValue value in Values)
                {
                    control.SetValue(value.Key, value.Value);
                }
                if (UseUpdatePanel)
                {
                    Controls.Add(WrapToUpdatePanel(control));
                }
                else
                {
                    Controls.Add(control);
                }
            }
            else
            {
                if (Template != null)
                {
                    Template.InstantiateIn(Container);

                    if (UseUpdatePanel)
                    {
                        Controls.Add(WrapToUpdatePanel(Container));
                    }
                    else
                    {
                        Controls.Add(Container);
                    }
                }
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Wraps given control to update panel.
        /// </summary>
        /// <param name="control">Control to wrap</param>
        /// <returns>Wrapping update panel</returns>
        private Control WrapToUpdatePanel(Control control)
        {
            // Create wrapping update panel
            CMSUpdatePanel pnlUpdate = new CMSUpdatePanel();
            pnlUpdate.ID = "pnlUpdate";
            pnlUpdate.ContentTemplateContainer.Controls.Add(control);
            return pnlUpdate;
        }


        /// <summary>
        /// Raises OnTabCreated event.
        /// </summary>
        /// <param name="element">UI element</param>
        /// <param name="tab">Tab object</param>
        /// <param name="tabIndex">Index of a tab</param>
        /// <returns>Potentially modified tab object</returns>
        public UITabItem RaiseTabCreated(UIElementInfo element, UITabItem tab, int tabIndex)
        {
            if (OnTabCreated != null)
            {
                var e = new TabCreatedEventArgs
                    {
                        UIElement = element,
                        Tab = tab,
                        TabIndex = tabIndex
                    };

                OnTabCreated(this, e);

                return e.Tab;
            }

            return tab;
        }


        /// <summary>
        /// Gets the HTML of element's start tag.
        /// </summary>
        private string GetStartTag()
        {
            StringBuilder startTag = new StringBuilder();
            startTag.Append("<" + RenderAs.ToString().ToLowerInvariant());
            startTag.Append(" id=\"" + ClientID + "\"");

            if (RenderAs == HtmlTextWriterTag.Iframe)
            {
                startTag.Append(" name=\"" + ID + "\"");
                if (AppendSrc)
                {
                    startTag.Append(" src=\"" + Src + "\"");
                }
                startTag.Append(" frameborder=\"" + Convert.ToInt32(FrameBorder) + "\"");
            }

            if (!String.IsNullOrEmpty(PaneClass))
            {
                startTag.Append(" class=\"" + PaneClass + "\"");
            }

            startTag.Append(">");
            return startTag.ToString();
        }


        /// <summary>
        /// Gets the HTML of element's end tag.
        /// </summary>
        private string GetEndTag()
        {
            return "</" + RenderAs.ToString().ToLowerInvariant() + ">";
        }


        /// <summary>
        /// Fills given StringBuilder with pane settings.
        /// </summary>
        /// <param name="sb">StringBuilder to fill with settings.</param>
        public void ApplyPaneSettings(StringBuilder sb)
        {
            if (sb != null)
            {
                PaneDirectionEnum directionValue;

                if (CultureHelper.IsUICultureRTL())
                {
                    // Switch panes in RTL
                    switch (Direction)
                    {
                        case PaneDirectionEnum.East:
                            directionValue = PaneDirectionEnum.West;
                            break;

                        case PaneDirectionEnum.West:
                            directionValue = PaneDirectionEnum.East;
                            break;

                        default:
                            directionValue = Direction;
                            break;
                    }
                }
                else
                {
                    directionValue = Direction;
                }
                sb.AppendLine(directionValue.ToString().ToLowerInvariant() + ": {");
                sb.AppendLine("paneSelector: '#" + ClientID + "'");
                if (Closable != null)
                {
                    sb.AppendLine(",closable: " + Closable.ToString().ToLowerInvariant());
                }
                if (Resizable != null)
                {
                    sb.AppendLine(",resizable: " + Resizable.ToString().ToLowerInvariant());
                }
                if (Slidable != null)
                {
                    sb.AppendLine(",slidable: " + Slidable.ToString().ToLowerInvariant());
                }
                if (ShowOverflowOnHover != null)
                {
                    sb.AppendLine(",showOverflowOnHover: " + ShowOverflowOnHover.ToString().ToLowerInvariant());
                }
                if (MaskContents != null)
                {
                    sb.AppendLine(",maskContents: " + MaskContents.ToString().ToLowerInvariant());
                }
                if (ResizeChildLayout != null)
                {
                    sb.AppendLine(",resizeChildren: " + ResizeChildLayout.ToString().ToLowerInvariant());
                }
                if (LivePaneResizing != null)
                {
                    sb.AppendLine(",livePaneResizing: " + LivePaneResizing.ToString().ToLowerInvariant());
                }
                if (LiveContentResizing != null)
                {
                    sb.AppendLine(",liveContentResizing: " + LiveContentResizing.ToString().ToLowerInvariant());
                }
                if (TriggerEventsDuringLiveResize != null)
                {
                    sb.AppendLine(",triggerEventsDuringLiveResize: " + TriggerEventsDuringLiveResize.ToString().ToLowerInvariant());
                }
                if (ContentSelector != null)
                {
                    sb.AppendLine(",contentSelector: \"" + ContentSelector + "\"");
                }
                if (ContentIgnoreSelector != null)
                {
                    sb.AppendLine(",contentIgnoreSelector: \"" + ContentIgnoreSelector + "\"");
                }
                if (Size != null)
                {
                    string paneSizeValue = Size;
                    if (!ValidationHelper.IsInteger(Size))
                    {
                        paneSizeValue = "\"" + paneSizeValue + "\"";
                    }
                    sb.AppendLine(",size: " + paneSizeValue);
                }
                if (MaxSize != null)
                {
                    sb.AppendLine(",maxSize: " + MaxSize);
                }
                if (MinSize != null)
                {
                    sb.AppendLine(",minSize: " + MinSize);
                }
                if (SpacingOpen != null)
                {
                    sb.AppendLine(",spacing_open: " + SpacingOpen);
                }
                if (SpacingClosed != null)
                {
                    sb.AppendLine(",spacing_closed: " + SpacingClosed);
                }
                if (TogglerLengthOpen != null)
                {
                    sb.AppendLine(",togglerLength_open: " + TogglerLengthOpen);
                }
                if (TogglerLengthClosed != null)
                {
                    sb.AppendLine(",togglerLength_closed: " + TogglerLengthClosed);
                }
                if (UsePseudoCloseCallback)
                {
                    sb.AppendLine(",onclose_start: $cmsj.layout.callbacks.pseudoClose");
                }
                if (ApplyDemoStyles != null)
                {
                    sb.AppendLine(",applyDemoStyles: " + ApplyDemoStyles);
                }
                if (PaneClass != null)
                {
                    sb.AppendLine(",paneClass: \"" + PaneClass + "\"");
                }
                if (ResizerClass != null)
                {
                    sb.AppendLine(",resizerClass: \"" + ResizerClass + "\"");
                }
                if (TogglerClass != null)
                {
                    sb.AppendLine(",togglerClass: \"" + TogglerClass + "\"");
                }
                if (ButtonClass != null)
                {
                    sb.AppendLine(",buttonClass: \"" + ButtonClass + "\"");
                }
                sb.AppendLine(",resizerTip: \"" + GetString(ResizerTip ?? "general.resize") + "\"");
                if (ResizerCursor != null)
                {
                    sb.AppendLine(",resizerCursor: \"" + ResizerCursor + "\"");
                }
                if (ResizerDragOpacity != null)
                {
                    sb.AppendLine(",resizerDragOpacity: " + ResizerDragOpacity);
                }
                sb.AppendLine(",sliderTip: \"" + GetString(SliderTip ?? "general.slide") + "\"");
                if (SliderCursor != null)
                {
                    sb.AppendLine(",sliderCursor: \"" + SliderCursor + "\"");
                }
                if (FxName != null)
                {
                    sb.AppendLine(",fxName: \"" + FxName + "\"");
                }
                if (FxSpeed != null)
                {
                    string fxSpeedValue = FxSpeed;
                    if (!ValidationHelper.IsInteger(FxSpeed))
                    {
                        fxSpeedValue = "\"" + fxSpeedValue + "\"";
                    }
                    sb.AppendLine(",fxSpeed: " + fxSpeedValue);
                }
                if (FxSettings != null)
                {
                    sb.AppendLine(",fxSettings: {" + FxSettings + "}");
                }
                // JS events
                if (!string.IsNullOrEmpty(OnShowStartScript))
                {
                    sb.AppendLine(",onshow_start: " + OnShowStartScript);
                }
                if (!string.IsNullOrEmpty(OnShowEndScript))
                {
                    sb.AppendLine(",onshow_end: " + OnShowEndScript);
                }
                if (!string.IsNullOrEmpty(OnHideStartScript))
                {
                    sb.AppendLine(",onhide_start: " + OnHideStartScript);
                }
                if (!string.IsNullOrEmpty(OnHideEndScript))
                {
                    sb.AppendLine(",onhide_end: " + OnHideEndScript);
                }
                if (!string.IsNullOrEmpty(OnOpenStartScript))
                {
                    sb.AppendLine(",onopen_start: " + OnOpenStartScript);
                }
                if (!string.IsNullOrEmpty(OnOpenEndScript))
                {
                    sb.AppendLine(",onopen_end: " + OnOpenEndScript);
                }
                if (!string.IsNullOrEmpty(OnCloseStartScript))
                {
                    sb.AppendLine(",onclose_start: " + OnCloseStartScript);
                }
                if (!string.IsNullOrEmpty(OnCloseEndScript))
                {
                    sb.AppendLine(",onclose_end: " + OnCloseEndScript);
                }
                if (!string.IsNullOrEmpty(OnResizeStartScript))
                {
                    sb.AppendLine(",onresize_start: " + OnResizeStartScript);
                }
                if (!string.IsNullOrEmpty(OnResizeEndScript))
                {
                    sb.AppendLine(",onresize_end: " + OnResizeEndScript);
                }
                sb.AppendLine(",initClosed: " + (InitClosed ? "true" : "false"));

                sb.AppendLine("}");
            }
        }

        #endregion
    }
}