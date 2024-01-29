using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Web;
using System.Web.UI;

using CMS.Base;
using CMS.Base.Web.UI;

namespace CMS.UIControls
{
    /// <summary>
    /// .NET representation of the UI Layout JavaScript plug-in
    /// </summary>
    [ParseChildren(typeof(UILayoutPane), ChildrenAsProperties = true),
    AspNetHostingPermission(SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal),
    AspNetHostingPermission(SecurityAction.Demand, Level = AspNetHostingPermissionLevel.Minimal),
    DefaultProperty("Panes"),
    ToolboxData("<{0}:UILayout runat=\"server\"> </{0}:UILayout>")]
    public class UILayout : CMSWebControl, INamingContainer
    {
        #region "Constants"
        #endregion


        #region "Private variables"

        /// <summary>
        /// Indicates if processing of the placeholder should be stopped.
        /// </summary>
        protected bool mStopProcessing = false;

        private string mContainerSelector;
        private bool mClosable = true;
        private bool mResizable = true;
        private bool mSlidable = true;

        #endregion


        #region "Public properties"

        #region "Basic properties"

        /// <summary>
        /// Element the layout is created under. Most commonly 'body' element (default value). Can be any block-element.
        /// </summary>
        public string ContainerSelector
        {
            get
            {
                if (mContainerSelector == null)
                {
                    if (ParentLayoutPane != null)
                    {
                        mContainerSelector = "#" + ParentLayoutPane.ClientID;
                    }
                    else
                    {
                        mContainerSelector = "body";
                    }
                }
                return mContainerSelector;
            }
            set
            {
                mContainerSelector = value;
            }
        }


        /// <summary>
        /// Collection of panes. Cannot contain more than one pane for one direction (NESW or center).
        /// </summary>
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public List<UILayoutPane> Panes
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if processing of the UI panel should be stopped.
        /// </summary>
        public bool StopProcessing
        {
            get
            {
                return mStopProcessing;
            }
            set
            {
                mStopProcessing = value;
            }
        }

        #endregion


        #region "Styling"

        /// <summary>
        /// When enabled, the layout will apply basic styles directly to resizers and buttons.
        /// </summary>
        public bool? ApplyDemoStyles
        {
            get;
            set;
        }

        #endregion


        #region "Behavior"

        /// <summary>
        /// Defines when the UI Layout JavaScript plug-in should be initialized.
        /// Sometimes it can be useful to register the layout script after document is ready or window is loaded.
        /// </summary>
        public LoadModeEnum LoadOn
        {
            get;
            set;
        }


        /// <summary>
        /// This option handles of bookmarks that are passed on the URL of the page.
        /// </summary>
        public bool? ScrollToBookmarkOnLoad
        {
            get;
            set;
        }


        /// <summary>
        /// Can a pane be closed?
        /// </summary>
        public bool Closable
        {
            get
            {
                return mClosable;
            }
            set
            {
                mClosable = value;
            }
        }


        /// <summary>
        /// When open, can a pane be resized?
        /// </summary>
        public bool Resizable
        {
            get
            {
                return mResizable;
            }
            set
            {
                mResizable = value;
            }
        }


        /// <summary>
        /// When closed, can a pane 'slide open' over adjacent panes?
        /// </summary>
        public bool Slidable
        {
            get
            {
                return mSlidable;
            }
            set
            {
                mSlidable = value;
            }
        }

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

        #endregion


        #region "Private properties"

        /// <summary>
        /// Gets a parent layout pane (if exists)
        /// </summary>
        private UILayoutPane ParentLayoutPane
        {
            get
            {
                Control parent = Parent;

                while (parent != null)
                {
                    if (parent is UILayoutPane)
                    {
                        return parent as UILayoutPane;
                    }
                    else
                    {
                        parent = parent.Parent;
                    }
                }
                return null;
            }
        }

        #endregion


        #region "Page events"

        /// <summary>
        /// Init event handler
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if (StopProcessing)
            {
            }
            else
            {
                // Preliminary check for pane consistency
                bool multiple = Panes.GroupBy(p => p.Direction).Any(g => g.Count() > 1);
                if (multiple)
                {
                    throw new Exception("[UILayout]: There are more panes with the same direction.");
                }
                bool centerExist = Panes.Any(p => p.Direction == PaneDirectionEnum.Center);
                if (!centerExist)
                {
                    throw new Exception("[UILayout]: There is no center pane defined.");
                }
                // Add nested panes
                foreach (UILayoutPane pane in Panes.Where(p => p.Visible))
                {
                    Controls.Add(pane);
                }
            }
        }


        /// <summary>
        /// PreRender event handler
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (!StopProcessing && Visible)
            {
                RegisterInitScript();
            }
        }


        private void RegisterInitScript()
        {
            // Create initializing script
            var sb = new StringBuilder();

            sb.AppendLine("var layout_" + ClientID + ";");

            if (LoadOn == LoadModeEnum.DocumentReady)
            {
                sb.AppendLine("$cmsj(document).ready(function(){");
            }
            else if (LoadOn == LoadModeEnum.WindowLoad)
            {
                sb.AppendLine("$cmsj(window).load(function(){");
            }

            sb.AppendLine("layout_" + ClientID + " = $cmsj('" + ContainerSelector + "').layout({");

            if (ScrollToBookmarkOnLoad != null)
            {
                sb.AppendLine("scrollToBookmarkOnLoad: " + ScrollToBookmarkOnLoad.ToString().ToLowerCSafe());
            }
            sb.AppendLine("defaults:{");

            var sbProp = new StringBuilder();

            if (ApplyDemoStyles != null)
            {
                sbProp.AppendLine(",applyDemoStyles: " + ApplyDemoStyles.ToString().ToLowerCSafe());
            }
            if (!string.IsNullOrEmpty(OnShowStartScript))
            {
                sbProp.AppendLine(",onshow_start: " + OnShowStartScript);
            }
            if (!string.IsNullOrEmpty(OnShowEndScript))
            {
                sbProp.AppendLine(",onshow_end: " + OnShowEndScript);
            }
            if (!string.IsNullOrEmpty(OnHideStartScript))
            {
                sbProp.AppendLine(",onhide_start: " + OnHideStartScript);
            }
            if (!string.IsNullOrEmpty(OnHideEndScript))
            {
                sbProp.AppendLine(",onhide_end: " + OnHideEndScript);
            }
            if (!string.IsNullOrEmpty(OnOpenStartScript))
            {
                sbProp.AppendLine(",onopen_start: " + OnOpenStartScript);
            }
            if (!string.IsNullOrEmpty(OnOpenEndScript))
            {
                sbProp.AppendLine(",onopen_end: " + OnOpenEndScript);
            }
            if (!string.IsNullOrEmpty(OnCloseStartScript))
            {
                sbProp.AppendLine(",onclose_start: " + OnCloseStartScript);
            }
            if (!string.IsNullOrEmpty(OnCloseEndScript))
            {
                sbProp.AppendLine(",onclose_end: " + OnCloseEndScript);
            }
            if (!string.IsNullOrEmpty(OnResizeStartScript))
            {
                sbProp.AppendLine(",onresize_start: " + OnResizeStartScript);
            }
            if (!string.IsNullOrEmpty(OnResizeEndScript))
            {
                sbProp.AppendLine(",onresize_end: " + OnResizeEndScript);
            }

            sbProp.AppendLine(",enableCursorHotkey:false");

            sb.Append(sbProp.ToString().TrimStart(','));
            sb.AppendLine("}");

            bool usesPseudoClose = false;

            foreach (UILayoutPane pane in Panes.Where(p => p.Visible))
            {
                sb.Append(",");
                // Get pane settings
                pane.ApplyPaneSettings(sb);
                // Check whether any of frames needs a callback script
                usesPseudoClose |= pane.UsePseudoCloseCallback;
            }

            sb.AppendLine("});");

            // Register all nested layouts to one array
            sb.AppendLine("if (window.layouts == undefined) {");
            sb.AppendLine("layouts = new Array();");
            sb.AppendLine("}");
            sb.AppendLine("layouts[layouts.length] = layout_" + ClientID + ";");

            if ((LoadOn == LoadModeEnum.DocumentReady) || (LoadOn == LoadModeEnum.WindowLoad))
            {
                sb.AppendLine("});");
            }

            var script = sb.ToString();

            // Register Layout UI library
            ScriptHelper.RegisterJQueryUILayout(Page, true, true, usesPseudoClose);

            // Register initializing script
            var page = Page as AbstractCMSPage;

            if (page != null)
            {
                page.RegisterBodyModifyingScript(typeof (string), "UILayout_" + ClientID, script, true);
            }
        }

        #endregion


        #region "Methods"

        #endregion
    }
}
