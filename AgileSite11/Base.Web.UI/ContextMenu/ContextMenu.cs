using System;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.Helpers;
using CMS.IO;

namespace CMS.Base.Web.UI
{
    #region "Property values enumeration"

    /// <summary>
    /// Vertical position enumeration.
    /// </summary>
    public enum VerticalPositionEnum
    {
        /// <summary>
        /// Current cursor position.
        /// </summary>
        Cursor = 0,

        /// <summary>
        /// Element top.
        /// </summary>
        Top = 1,

        /// <summary>
        /// Element bottom.
        /// </summary>
        Bottom = 2,

        /// <summary>
        /// Element middle.
        /// </summary>
        Middle = 3
    }


    /// <summary>
    /// Horizontal position enumeration.
    /// </summary>
    public enum HorizontalPositionEnum
    {
        /// <summary>
        /// Current cursor position.
        /// </summary>
        Cursor = 0,

        /// <summary>
        /// Element left.
        /// </summary>
        Left = 1,

        /// <summary>
        /// Element right.
        /// </summary>
        Right = 2,

        /// <summary>
        /// Element center.
        /// </summary>
        Center = 3
    }


    /// <summary>
    /// Mouse button enumeration.
    /// </summary>
    public enum MouseButtonEnum
    {
        /// <summary>
        /// No button.
        /// </summary>
        None = 0,

        /// <summary>
        /// Left button.
        /// </summary>
        Left = 1,

        /// <summary>
        /// Right button.
        /// </summary>
        Right = 2,

        /// <summary>
        /// Both buttons.
        /// </summary>
        Both = 3
    }

    #endregion


    /// <summary>
    /// Context menu control.
    /// </summary>
    [ToolboxItem(false)]
    public class ContextMenu : CMSPanel, ICallbackEventHandler
    {
        #region "Variables"

        /// <summary>
        /// Menu ID.
        /// </summary>
        protected string mMenuID = "contextMenu";

        /// <summary>
        /// Timeout to show the menu until it disappears.
        /// </summary>
        protected int mShowTimeout = 1000;

        /// <summary>
        /// Menu parameter.
        /// </summary>
        protected string mParameter = null;

        /// <summary>
        /// Vertical position.
        /// </summary>
        protected VerticalPositionEnum mVerticalPosition = VerticalPositionEnum.Cursor;

        /// <summary>
        /// Horizontal position.
        /// </summary>
        protected HorizontalPositionEnum mHorizontalPosition = HorizontalPositionEnum.Cursor;

        /// <summary>
        /// Mouse button.
        /// </summary>
        protected MouseButtonEnum mMouseButton = MouseButtonEnum.Right;

        /// <summary>
        /// Context menu control.
        /// </summary>
        protected CMSContextMenuControl mContextMenuControl = null;

        /// <summary>
        /// Prefix for the resource strings which will be used for the strings of the context menu.
        /// </summary>
        protected string mResourcePrefix = "general";

        /// <summary>
        /// Container unique ID.
        /// </summary>
        protected string mContainerUniqueID = null;

        /// <summary>
        /// Context menu suffix.
        /// </summary>
        public const string CONTEXT_MENU_SUFFIX = "_contextMenuControl";

        #endregion


        #region "Events"

        /// <summary>
        /// Raised when the menu requires to reload its data (for dynamic menus).
        /// </summary>
        public event EventHandler OnReloadData;

        #endregion


        #region "Properties"

        /// <summary>
        /// CSS Class for the outer envelope of the menu
        /// </summary>
        public string OuterCssClass 
        {
            get; 
            set; 
        }


        /// <summary>
        /// Menu ID, must be unique within page context.
        /// </summary>
        public string MenuID
        {
            get
            {
                return mMenuID;
            }
            set
            {
                mMenuID = value;
            }
        }


        /// <summary>
        /// Identifier of parent element under which the context menu lies. 
        /// This parameter is optional.
        /// </summary>
        public virtual string ParentElementClientID
        {
            get;
            set;
        }


        /// <summary>
        /// Timeout to show the menu until it disappears.
        /// </summary>
        public int ShowTimeout
        {
            get
            {
                return mShowTimeout;
            }
            set
            {
                mShowTimeout = value;
            }
        }


        /// <summary>
        /// Menu parameter.
        /// </summary>
        public string Parameter
        {
            get
            {
                return mParameter ?? (mParameter = ValidationHelper.GetString(HttpContext.Current.Request.Params[MenuID + "_parameter"], string.Empty));
            }
            set
            {
                mParameter = value;
            }
        }


        /// <summary>
        /// Vertical position.
        /// </summary>
        public VerticalPositionEnum VerticalPosition
        {
            get
            {
                return mVerticalPosition;
            }
            set
            {
                mVerticalPosition = value;
            }
        }


        /// <summary>
        /// Horizontal position.
        /// </summary>
        public HorizontalPositionEnum HorizontalPosition
        {
            get
            {
                return mHorizontalPosition;
            }
            set
            {
                mHorizontalPosition = value;
            }
        }


        /// <summary>
        /// Mouse button.
        /// </summary>
        public MouseButtonEnum MouseButton
        {
            get
            {
                return mMouseButton;
            }
            set
            {
                mMouseButton = value;
            }
        }


        /// <summary>
        /// Offset X of the menu.
        /// </summary>
        public int OffsetX
        {
            get;
            set;
        }


        /// <summary>
        /// Offset Y of the menu.
        /// </summary>
        public int OffsetY
        {
            get;
            set;
        }


        /// <summary>
        /// CSS class of highlighted element when context menu is opened.
        /// </summary>
        public string ActiveItemCssClass
        {
            get;
            set;
        }


        /// <summary>
        /// CSS class of highlighted element when context menu is closed.
        /// </summary>
        public string ActiveItemInactiveCssClass
        {
            get;
            set;
        }


        /// <summary>
        /// Determines how many levels above the clicked element the active element is.
        /// </summary>
        public int ActiveItemOffset
        {
            get;
            set;
        }


        /// <summary>
        /// Determines whether to load the menu dynamically.
        /// </summary>
        public bool Dynamic
        {
            get;
            set;
        }


        /// <summary>
        /// Menu level.
        /// </summary>
        public int MenuLevel
        {
            get;
            set;
        }


        /// <summary>
        /// Content displayed during loading.
        /// </summary>
        public string LoadingContent
        {
            get;
            set;
        }


        /// <summary>
        /// Show menu on mouse over action?
        /// </summary>
        public bool ShowMenuOnMouseOver
        {
            get;
            set;
        }


        /// <summary>
        /// Context menu control.
        /// </summary>
        public CMSContextMenuControl ContextMenuControl
        {
            get
            {
                return mContextMenuControl;
            }
            set
            {
                mContextMenuControl = value;
            }
        }


        /// <summary>
        /// Prefix for the resource strings which will be used for the strings of the context menu.
        /// Default is "general".
        /// </summary>
        public string ResourcePrefix
        {
            get
            {
                return mResourcePrefix;
            }
            set
            {
                mResourcePrefix = value;
            }
        }


        /// <summary>
        /// Container unique ID.
        /// </summary>
        public string ContainerUniqueID
        {
            get
            {
                return mContainerUniqueID;
            }
            set
            {
                mContainerUniqueID = value;
            }
        }


        /// <summary>
        /// If true, the menu opens in the upward direction instead down
        /// </summary>
        public bool OpenUp
        {
            get;
            set;
        }

        #endregion


        #region "Page events"

        /// <summary>
        /// Control PreRender event.
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            ScriptHelper.RegisterJQuery(Page);
            ScriptHelper.RegisterScriptFile(Page, "Controls/contextmenu.js");

            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(
@"
menuSettings['{0}'] = {{ 
    activecss : '{1}',
    inactivecss : '{2}',
    activecssoffset : {3},
    button : '{4}',
    vertical : '{5}',
    offsetx : {6},
    offsety : {7},
    horizontal : '{8}',
    dynamic : {9},
    mouseover : {10},
    level : {11},
    rtl : {12},
    up : {13}
}};
CM_Init('{0}');
",
               MenuID,
               ActiveItemCssClass,
               ActiveItemInactiveCssClass,
               ActiveItemOffset,
               MouseButton,
               VerticalPosition,
               OffsetX,
               OffsetY,
               HorizontalPosition,
               Dynamic.ToString().ToLowerCSafe(),
               ShowMenuOnMouseOver.ToString().ToLowerCSafe(),
               MenuLevel,
               (IsLiveSite ? CultureHelper.IsPreferredCultureRTL() : CultureHelper.IsUICultureRTL()).ToString().ToLowerCSafe(),
               OpenUp.ToString().ToLowerCSafe()
           );

            if (Dynamic)
            {
                sb.Append(
@"
function ", MenuID, @"_LoadMenu(param) {
    ", Page.ClientScript.GetCallbackEventReference(this, "'" + ContainerUniqueID + ";load:' + param", "CM_Receive", "'" + MenuID + "'"), @";
}
dynamicMenus['", MenuID, "'] = ", MenuID, @"_LoadMenu;
"
               );

                if (LoadingContent != null)
                {
                    sb.Append("loadingContent['", MenuID, "'] = ", ScriptHelper.GetString(LoadingContent), ";");
                }
            }

            sb.Append(
@"

function ContextMenuAsyncCloser_", MenuID, @"(sender, args)
{
    HideContextMenu('", MenuID, @"',true);
}

RegisterAsyncCloser(ContextMenuAsyncCloser_" + MenuID + @");
"
            );

            ScriptHelper.RegisterClientScriptBlock(this, typeof(string), this.ClientID, ScriptHelper.GetScript(sb.ToString()));
        }


        /// <summary>
        /// Renders the control.
        /// </summary>
        protected override void Render(HtmlTextWriter writer)
        {
            // Ensure separate class names for different menu levels
            string className = OuterCssClass + " ContextMenuLevel_" + MenuLevel;

            writer.Write("<input type=\"hidden\" id=\"" + MenuID + "_parameter\" name=\"" + MenuID + "_parameter\" value=\"" + HTMLHelper.HTMLEncode(Parameter) + "\" />");

            writer.Write("<div id=\"" + MenuID + "\" style=\"display: none; position: absolute; z-index: 20201;\" class=\"" + className + "\" onmouseout=\"CM_MenuOut(" + MenuLevel + ");\" onmouseover=\"CM_MenuOver(" + MenuLevel + ");\">");

            base.Render(writer);

            writer.Write("</div>");
        }

        #endregion


        #region "Other methods"

        /// <summary>
        /// Constructor
        /// </summary>
        public ContextMenu()
        {
            OuterCssClass = "ContextMenu cms-bootstrap";
        }


        /// <summary>
        /// Loads the specified control to the menu.
        /// </summary>
        /// <param name="path">Control path</param>
        public void LoadUserControl(string path)
        {
            try
            {
                // Load the control
                ContextMenuControl = (CMSContextMenuControl)Page.LoadUserControl(path);
                if (ContextMenuControl != null)
                {
                    ContextMenuControl.ID = ID + CONTEXT_MENU_SUFFIX;
                    ContextMenuControl.ContextMenu = this;
                    ContextMenuControl.IsLiveSite = IsLiveSite;
                    Controls.Add(ContextMenuControl);
                }
            }
            catch (Exception ex)
            {
                // Add exception message to the menu
                Controls.Add(new LiteralControl(ex.Message));
            }
        }

        #endregion


        #region "Callback handling"

        private string evArgument = string.Empty;

        /// <summary>
        /// Raises the callback event.
        /// </summary>
        /// <param name="eventArgument">Event argument</param>
        public void RaiseCallbackEvent(string eventArgument)
        {
            // Remove container unique ID
            if (eventArgument != null)
            {
                eventArgument = eventArgument.Split(new char[] { ';' }, StringSplitOptions.None)[1];
            }

            evArgument = eventArgument;
            if (eventArgument != null)
            {
                if (eventArgument.ToLowerCSafe().StartsWithCSafe("load:"))
                {
                    Parameter = eventArgument.Substring(5);
                    if (OnReloadData != null)
                    {
                        OnReloadData(this, new EventArgs());
                    }
                }
            }
        }


        /// <summary>
        /// Returns the callback result.
        /// </summary>
        public string GetCallbackResult()
        {
            if (evArgument.ToLowerCSafe().StartsWithCSafe("load:"))
            {
                // Render the content to the result
                StringBuilder sb = new StringBuilder(256);
                StringWriter tw = new StringWriter(sb);
                HtmlTextWriter hw = new HtmlTextWriter(tw);

                base.Render(hw);

                return sb.ToString();
            }
            return string.Empty;
        }

        #endregion
    }
}
