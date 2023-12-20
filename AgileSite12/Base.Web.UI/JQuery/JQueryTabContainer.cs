using System;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.Helpers;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// JQuery tabs container class.
    /// </summary>
    [ToolboxData("<{0}:JQueryTabContainer runat=server></{0}:JQueryTabContainer>"), Serializable]
    [ParseChildren(typeof(JQueryTab))]
    public class JQueryTabContainer : Panel
    {
        #region "Variables"

        private int mSelectedTabIndex;
        private int mCachedSelectedTabIndex;
        private bool mIinitialized;
        private jQueryTabsPositionEnum mTabsPosition = jQueryTabsPositionEnum.Top;
        private string mCssClassPrefix = "TabControl";
        private bool mShowTabs = true;
        private string mOnClientTabClick = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Active tab.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public JQueryTab SelectedTab
        {
            get
            {
                int selectedTabIndex = SelectedTabIndex;
                if ((selectedTabIndex < 0) || (selectedTabIndex >= Tabs.Count))
                {
                    return null;
                }
                EnsureActiveTab();
                return Tabs[selectedTabIndex];
            }
            set
            {
                int index = Tabs.IndexOf(value);
                if (index < 0)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                SelectedTabIndex = index;
            }
        }


        /// <summary>
        /// Active tab index.
        /// </summary>
        [Category("Behavior"), DefaultValue(-1)]
        public int SelectedTabIndex
        {
            get
            {
                if (Tabs.Count == 0)
                {
                    return -1;
                }
                return mSelectedTabIndex;
            }
            set
            {
                if (value < -1)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                if ((Tabs.Count == 0) && !mIinitialized)
                {
                    mCachedSelectedTabIndex = value;
                }
                else if (SelectedTabIndex != value)
                {
                    if ((SelectedTabIndex != -1) && (SelectedTabIndex < Tabs.Count))
                    {
                        Tabs[SelectedTabIndex].Selected = false;
                    }
                    if (value >= Tabs.Count)
                    {
                        mSelectedTabIndex = Tabs.Count - 1;
                        mCachedSelectedTabIndex = value;
                    }
                    else
                    {
                        mSelectedTabIndex = value;
                        mCachedSelectedTabIndex = -1;
                    }
                    if ((SelectedTabIndex != -1) && (SelectedTabIndex < Tabs.Count))
                    {
                        Tabs[SelectedTabIndex].Selected = true;
                    }
                }
            }
        }


        /// <summary>
        /// Active tab index.
        /// </summary>
        [Category("Behavior"), DefaultValue("")]
        public string OnClientTabClick
        {
            get
            {
                return mOnClientTabClick;
            }
            set
            {
                mOnClientTabClick = value;
            }
        }


        /// <summary>
        /// Returns current tabs collection.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public JQueryTabCollection Tabs
        {
            get
            {
                return (JQueryTabCollection)Controls;
            }
        }


        /// <summary>
        /// Gets or sets tabs position.
        /// </summary>
        [DefaultValue(0), Category("Appearance")]
        public jQueryTabsPositionEnum TabsPosition
        {
            get
            {
                return mTabsPosition;
            }
            set
            {
                mTabsPosition = value;
            }
        }


        /// <summary>
        /// Gets or sets tabs css class prefix.
        /// Default value is "TabControl".
        /// </summary>
        [DefaultValue("TabControl"), Category("Appearance")]
        public string CssClassPrefix
        {
            get
            {
                return mCssClassPrefix;
            }
            set
            {
                mCssClassPrefix = value;
            }
        }


        /// <summary>
        /// Indicates if tabs should by rendered and jQuery tabs code should by registered.
        /// </summary>
        [DefaultValue(true), Category("Appearance")]
        public bool ShowTabs
        {
            get
            {
                return mShowTabs;
            }
            set
            {
                mShowTabs = value;
            }
        }

        #endregion


        #region "Constructor"

        /// <summary>
        /// Constructor.
        /// </summary>
        public JQueryTabContainer()
        {
            mSelectedTabIndex = 0;
            mCachedSelectedTabIndex = -1;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Called after a child control is added to the System.Web.UI.Control.Controls collection of the System.Web.UI.Control object.
        /// </summary>
        protected override void AddedControl(Control control, int index)
        {
            ((JQueryTab)control).SetOwner(this, index);
            base.AddedControl(control, index);
        }


        /// <summary>
        /// Notifies the server control that an element, either XML or HTML, was parsed, and adds the element to the server control's System.Web.UI.ControlCollection object.
        /// </summary>
        protected override void AddParsedSubObject(object obj)
        {
            JQueryTab child = obj as JQueryTab;
            if (child != null)
            {
                Controls.Add(child);
            }
            else if (!(obj is LiteralControl))
            {
                throw new HttpException(string.Format("TabContainer cannot have children of type '{0}'.", new object[] { obj.GetType() }));
            }
        }


        /// <summary>
        /// Init event handler.
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            Page.RegisterRequiresControlState(this);
            mIinitialized = true;

            if (mCachedSelectedTabIndex > -1)
            {
                SelectedTabIndex = mCachedSelectedTabIndex;
                if (SelectedTabIndex < Tabs.Count)
                {
                    Tabs[SelectedTabIndex].Selected = true;
                }
            }
            else if (Tabs.Count > 0)
            {
                SelectedTabIndex = 0;
            }
        }


        /// <summary>
        /// Init event handler.
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            // Get selected tab index from request
            SelectedTabIndex = ValidationHelper.GetInteger(CMSHttpContext.Current.Request.Form[UniqueID + "$SelectedTab"], 0);
        }


        /// <summary>
        /// PreRender event handler.
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (ShowTabs)
            {
                // Ensure UI tabs class and initial style
                this.AddCssClass("JqueryUITabs");
                Attributes["style"] += "display:none;";

                ScriptHelper.RegisterJQueryUI(Page);

                // Fire resize after tab is fully loaded so MessagesPlaceHolder is positioned correctly
                string script = String.Format("$cmsj('#{0}').bind('tabsactivate tabscreate', function () {{ $cmsj(window).resize(); }});", ClientID);

                // Prepare other startup scripts
                string tabsOptions = String.Format(@"
{{
beforeActivate: function(event, ui) {{ 
    $cmsj('#{0}_SelectedTab').val(ui.newTab.index());
    {1}
}},
active: {2}
}}", ClientID, OnClientTabClick, SelectedTabIndex);

                script += String.Format("$cmsj('#{0}').tabs({1}).show();", ClientID, tabsOptions);

                ScriptHelper.RegisterStartupScript(this, GetType(), "jTabs_" + ClientID, script, true);
            }
        }


        /// <summary>
        /// Called after a child control is removed from the System.Web.UI.Control.Controls collection of the System.Web.UI.Control object.
        /// </summary>
        protected override void RemovedControl(Control control)
        {
            JQueryTab panel = control as JQueryTab;
            if ((panel != null) && panel.Selected && (SelectedTabIndex < Tabs.Count))
            {
                EnsureActiveTab();
            }
            base.RemovedControl(control);
        }


        /// <summary>
        /// Renders the contents of the control to the specified writer. This method is used primarily by control developers.
        /// </summary>
        protected override void Render(HtmlTextWriter writer)
        {
            if (Visible)
            {
                Page.VerifyRenderingInServerForm(this);

                AddAttributesToRender(writer);
                writer.RenderBeginTag(HtmlTextWriterTag.Div);

                if ((ShowTabs) && (TabsPosition == jQueryTabsPositionEnum.Top))
                {
                    RenderHeader(writer);
                }
                if (!Height.IsEmpty)
                {
                    writer.AddStyleAttribute(HtmlTextWriterStyle.Height, Height.ToString());
                }
                writer.AddAttribute(HtmlTextWriterAttribute.Id, ClientID + "_body");
                writer.RenderBeginTag(HtmlTextWriterTag.Div);

                RenderChildren(writer);

                writer.RenderEndTag();
                if ((ShowTabs) && (TabsPosition == jQueryTabsPositionEnum.Bottom))
                {
                    RenderHeader(writer);
                }
                // Hidden field for selected tab index
                writer.Write(@"<input type=""hidden"" id=""{0}_SelectedTab"" name=""{1}$SelectedTab"" value=""{2}"" />", ClientID, UniqueID, SelectedTabIndex);
                writer.RenderEndTag();
            }
        }


        /// <summary>
        /// Renders the header of the control to the specified writer.
        /// </summary>
        protected virtual void RenderHeader(HtmlTextWriter writer)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "nav-tabs-container-horizontal");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            writer.AddAttribute(HtmlTextWriterAttribute.Id, ClientID + "_header");
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "nav nav-tabs");
            writer.RenderBeginTag(HtmlTextWriterTag.Ul);
            foreach (JQueryTab panel in Tabs)
            {
                if (panel.Visible)
                {
                    panel.RenderHeader(writer);
                }
            }
            writer.RenderEndTag();
            writer.RenderEndTag();
        }


        /// <summary>
        /// Creates a new System.Web.UI.ControlCollection object to hold the child controls (both literal and server) of the server control.
        /// </summary>
        protected override ControlCollection CreateControlCollection()
        {
            return new JQueryTabCollection(this);
        }


        private void EnsureActiveTab()
        {
            if ((mSelectedTabIndex < 0) || (mSelectedTabIndex >= Tabs.Count))
            {
                mSelectedTabIndex = 0;
            }
            for (int i = 0; i < Tabs.Count; i++)
            {
                Tabs[i].Selected = (i == SelectedTabIndex);
            }
        }

        #endregion
    }
}
