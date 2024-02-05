using System;
using System.ComponentModel;
using System.Web.UI;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.Helpers;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Context menu container class (calls the assigned context menu).
    /// </summary>
    [ToolboxItem(false)]
    [ParseChildren(false)]
    public class ContextMenuContainer : CMSWebControl
    {
        #region "Variables"

        /// <summary>
        /// Menu control.
        /// </summary>
        protected ContextMenu mMenuControl = null;

        /// <summary>
        /// Menu ID.
        /// </summary>
        protected string mMenuID = null;

        /// <summary>
        /// Context menu CSS class.
        /// </summary>
        protected string mContextMenuCssClass = "";

        /// <summary>
        /// CSS style.
        /// </summary>
        protected string mCssStyle = "";

        /// <summary>
        /// Menu parameter.
        /// </summary>
        protected string mParameter = "";

        /// <summary>
        /// Prefix for the resource strings which will be used for the strings of the context menu.
        /// </summary>
        protected string mResourcePrefix = "general";

        /// <summary>
        /// Indicates if the control should perform the operations.
        /// </summary>
        protected bool mStopProcessing = false;

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
        /// If true, relative ancestor div is checked
        /// </summary>
        protected bool mCheckRelative = false;

        /// <summary>
        /// Default render tag.
        /// </summary>
        protected HtmlTextWriterTag mRenderAsTag = HtmlTextWriterTag.Div;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets the request key used for request storage
        /// </summary>
        private string RequestKey
        {
            get
            {
                return "CxMenuControl_" + MenuID;
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
        /// If true, relative ancestor div is checked
        /// </summary>
        public bool CheckRelative
        {
            get
            {
                return mCheckRelative;
            }
            set
            {
                mCheckRelative = value;
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
        /// Control which is the parent for the context menu.
        /// </summary>
        public virtual Control ContextMenuParent
        {
            get;
            set;
        }


        /// <summary>
        /// Menu ID.
        /// </summary>
        public virtual string MenuID
        {
            get
            {
                return mMenuID ?? (mMenuID = "m" + ID);
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
        /// Context menu CSS class.
        /// </summary>
        public virtual string ContextMenuCssClass
        {
            get
            {
                return mContextMenuCssClass;
            }
            set
            {
                mContextMenuCssClass = value;
            }
        }


        /// <summary>
        /// Container CSS style.
        /// </summary>
        public virtual string CssStyle
        {
            get
            {
                return mCssStyle;
            }
            set
            {
                mCssStyle = value;
            }
        }


        /// <summary>
        /// Menu parameter.
        /// </summary>
        public virtual string Parameter
        {
            get
            {
                return mParameter;
            }
            set
            {
                mParameter = value;
            }
        }


        /// <summary>
        /// Name of the javascript method which provides the element context to the menu
        /// </summary>
        public virtual string GetContextMethod 
        { 
            get; 
            set; 
        }


        /// <summary>
        /// Parameter for the menu control.
        /// </summary>
        public string MenuParameter
        {
            get;
            set;
        }


        /// <summary>
        /// Menu control.
        /// </summary>
        public virtual string MenuControlPath
        {
            get;
            set;
        }


        /// <summary>
        /// Menu control.
        /// </summary>
        public virtual ContextMenu MenuControl
        {
            get
            {
                return EnsureMenuControl();
            }
            set
            {
                mMenuControl = value;
                RequestStockHelper.Add(RequestKey, value);
            }
        }


        /// <summary>
        /// Enable mouse click.
        /// </summary>
        public virtual bool EnableMouseClick
        {
            get;
            set;
        }


        /// <summary>
        /// Specifies how the container should be rendered (as which tag).
        /// </summary>
        [DefaultValue(HtmlTextWriterTag.Div)]
        public virtual HtmlTextWriterTag RenderAsTag
        {
            get
            {
                return mRenderAsTag;
            }
            set
            {
                mRenderAsTag = value;
            }
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
        /// Prefix for the resource strings which will be used for the strings of the context menu.
        /// Default is "general".
        /// </summary>
        public virtual string ResourcePrefix
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
        /// Indicates if the control should perform the operations.
        /// </summary>
        public virtual bool StopProcessing
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


        #region "Page events"

        /// <summary>
        /// Control Init event.
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            if (StopProcessing)
            {
                return;
            }

            AddMenuControl(false);
        }


        /// <summary>
        /// Control Load event.
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (StopProcessing)
            {
                return;
            }

            AddMenuControl(false);
        }


        /// <summary>
        /// Control PreRender event.
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (StopProcessing)
            {
                return;
            }

            AddMenuControl(true);

            ScriptHelper.RegisterScriptFile(Page, "Controls/contextmenu.js");
        }


        /// <summary>
        /// Control Render event.
        /// </summary>
        protected override void Render(HtmlTextWriter writer)
        {
            if (Context == null)
            {
                writer.Write(" [ ContextMenuContainer : " + ID + " ]");
            }
            else
            {
                // Render the menu control
                base.Render(writer);
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Renders the start tag for the menu container.
        /// </summary>
        /// <param name="writer">HTML writer</param>
        public void RenderStartTag(HtmlTextWriter writer)
        {
            // Create the control envelope
            string tag;

            if (IsEnabled)
            {
                tag = GetStartTag(MenuID, Parameter, EnableMouseClick, RenderAsTag, CssClass, CssStyle, ToolTip, CheckRelative, GetContextMethod);
            }
            else
            {
                tag = GetStartDisabledTag(RenderAsTag, EnableMouseClick, CssClass, CssStyle, ToolTip);
            }

            writer.Write(tag);
        }


        /// <summary>
        /// Renders the begin tag of the control
        /// </summary>
        /// <param name="writer">Html writer for output</param>
        public override void RenderBeginTag(HtmlTextWriter writer)
        {
            if (!String.IsNullOrEmpty(MenuID))
            {
                // Create the control envelope
                string tag;

                if (IsEnabled)
                {
                    tag = GetStartTag(MenuID, Parameter, EnableMouseClick, RenderAsTag, CssClass, CssStyle, ToolTip, CheckRelative, GetContextMethod);
                }
                else
                {
                    tag = GetStartDisabledTag(RenderAsTag, EnableMouseClick, CssClass, CssStyle, ToolTip);
                }

                writer.Write(tag);
            }
        }


        /// <summary>
        /// Renders the end tag of the control
        /// </summary>
        /// <param name="writer">Html writer for output</param>
        public override void RenderEndTag(HtmlTextWriter writer)
        {
            if (!String.IsNullOrEmpty(MenuID))
            {
                // Close the menu envelope
                writer.Write(GetEndTag(RenderAsTag));
            }
        }


        /// <summary>
        /// Ensures the menu control.
        /// </summary>
        protected ContextMenu EnsureMenuControl()
        {
            if (mMenuControl == null)
            {
                // Get the existing menu control from context
                mMenuControl = (ContextMenu)RequestStockHelper.GetItem(RequestKey);
                if ((mMenuControl == null) && (MenuControlPath != null))
                {
                    ContextMenu menu = new ContextMenu();

                    // Initialize context menu properties
                    InitializeMenuControl(menu);

                    // Add the menu control
                    menu.LoadUserControl(MenuControlPath);

                    MenuControl = menu;
                }
            }

            return mMenuControl;
        }


        /// <summary>
        /// Initializes menu control properties
        /// </summary>
        /// <param name="menu">Context menu</param>
        public void InitializeMenuControl(ContextMenu menu)
        {
            menu.EnableViewState = false;
            menu.ID = "m" + MenuID;
            menu.MenuID = MenuID;
            menu.Parameter = MenuParameter;
            menu.ParentElementClientID = ParentElementClientID;
            menu.Page = Page;

            menu.MouseButton = MouseButton;
            menu.VerticalPosition = VerticalPosition;
            menu.HorizontalPosition = HorizontalPosition;
            menu.OffsetX = OffsetX;
            menu.OffsetY = OffsetY;
            menu.ActiveItemCssClass = ActiveItemCssClass;
            menu.ActiveItemInactiveCssClass = ActiveItemInactiveCssClass;
            menu.IsLiveSite = IsLiveSite;
            menu.CssClass = ContextMenuCssClass;

            menu.OnReloadData += ReloadMenuData;
        }


        /// <summary>
        /// Adds control to page (to the ManagersContainer).
        /// </summary>
        /// <param name="throwException">If true, the process throws an exception when fails</param>
        private void AddMenuControl(bool throwException)
        {
            try
            {
                object added = RequestStockHelper.GetItem(RequestKey + "_Added");
                // Menu control hasn't been added or is set explicitly by external code
                if (added == null)
                {
                    // Ensure the menu control
                    if (MenuControlPath != null)
                    {
                        // Ensure the menu control
                        EnsureMenuControl();
                    }

                    if (MenuControl != null)
                    {
                        // Get the parent for the menu control
                        Control menuParent = null;

                        // Add the menu control to the page
                        if (Page is ICMSPage)
                        {
                            // Add to the manager's container
                            ICMSPage page = (ICMSPage)Page;
                            if (page.ContextMenuContainer != null)
                            {
                                menuParent = page.ContextMenuContainer;
                            }
                        }

                        if ((ContextMenuParent != null) && ((menuParent == null) || (!ControlsHelper.IsInUpdatePanel(menuParent) && RequestHelper.IsAJAXRequest())))
                        {
                            // Add to previously set element
                            menuParent = ContextMenuParent;
                        }

                        if (menuParent == null)
                        {
                            // Add to the beginning of the page regularly
                            menuParent = Page.Form;
                        }

                        if ((menuParent != null) && !menuParent.Controls.Contains(MenuControl))
                        {
                            menuParent.Controls.AddAt(0, MenuControl);
                            RequestStockHelper.Add(RequestKey + "_Added", true);
                        }
                    }
                }
            }
            catch
            {
                if (throwException)
                {
                    throw;
                }
            }
        }


        /// <summary>
        /// Reload menu data.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event arguments</param>
        protected virtual void ReloadMenuData(object sender, EventArgs e)
        {
        }


        /// <summary>
        /// Gets the start tag for context menu container.
        /// </summary>
        /// <param name="menuId">Menu ID</param>
        /// <param name="parameter">Parameter</param>
        public static string GetStartTag(string menuId, string parameter)
        {
            return GetStartTag(menuId, parameter, false, true);
        }


        /// <summary>
        /// Gets the start tag for context menu container.
        /// </summary>
        /// <param name="menuId">Menu ID</param>
        /// <param name="parameter">Parameter</param>
        /// <param name="enableMouseClick">Enable mouse click</param>
        /// <param name="divTag">If true, renders as div tag, otherwise renders as span tag</param>
        public static string GetStartTag(string menuId, string parameter, bool enableMouseClick, bool divTag)
        {
            return GetStartTag(menuId, parameter, enableMouseClick, divTag ? HtmlTextWriterTag.Div : HtmlTextWriterTag.Span, null, null);
        }


        /// <summary>
        /// Gets the start tag for context menu container.
        /// </summary>
        /// <param name="menuId">Menu ID</param>
        /// <param name="parameter">Parameter</param>
        /// <param name="enableMouseClick">Enable mouse click</param>
        /// <param name="tag">Tag to be rendered</param>
        /// <param name="cssClass">Tag's CSS class</param>
        /// <param name="style">Tag's inline style</param>
        public static string GetStartTag(string menuId, string parameter, bool enableMouseClick, HtmlTextWriterTag tag, string cssClass, string style)
        {
            return GetStartTag(menuId, parameter, enableMouseClick, tag, cssClass, style, null, false, null);
        }


        /// <summary>
        /// Gets the start tag for context menu container.
        /// </summary>
        /// <param name="menuId">Menu ID</param>
        /// <param name="parameter">Parameter</param>
        /// <param name="enableMouseClick">Enable mouse click</param>
        /// <param name="tag">Tag to be rendered</param>
        /// <param name="cssClass">Tag's CSS class</param>
        /// <param name="style">Tag's inline style</param>
        /// <param name="toolTip">Text rendered as HTML attribute "title"</param>
        /// <param name="checkRelative">If true, relative div ancestor is checked</param>
        /// <param name="getContextMethod">Name of the javascript method which provides the element context to the menu</param>
        public static string GetStartTag(string menuId, string parameter, bool enableMouseClick, HtmlTextWriterTag tag, string cssClass, string style, string toolTip, bool checkRelative, string getContextMethod)
        {
            string param = (!string.IsNullOrEmpty(parameter) ? parameter : "''");

            string mouseClick = string.Empty;
            if (!enableMouseClick)
            {
                mouseClick = " onclick=\"return false;\"";
            }

            if (!string.IsNullOrEmpty(cssClass))
            {
                cssClass = " class=\"" + cssClass + "\"";
            }

            if (!string.IsNullOrEmpty(toolTip))
            {
                toolTip = " title=\"" + toolTip + "\"";
            }

            if (!string.IsNullOrEmpty(style))
            {
                style = " style=\"" + style + "\"";
            }

            return "<" + tag.ToString().ToLowerCSafe() + cssClass + style + mouseClick + toolTip + " onmouseout=\"return CM_Out(event, '" + menuId + "');\" onmouseover=\"return CM_Over(event, '" + menuId + "', this, " + param + ");\" onmousedown=\"return false;\" onmouseup=\"return CM_Up(event, '" + menuId + "', this, " + param + "," + checkRelative.ToString().ToLowerCSafe() +  (getContextMethod != null ? ", " + getContextMethod : "") + ");\">";
        }


        /// <summary>
        /// Gets the start tag for context menu container in disabled state
        /// </summary>
        /// <param name="tag">Tag to be rendered</param>
        /// <param name="enableMouseClick">Enable mouse click</param>
        /// <param name="cssClass">Tag's CSS class</param>
        /// <param name="style">Tag's inline style</param>
        /// <param name="toolTip">Text rendered as HTML attribute "title"</param>
        public static string GetStartDisabledTag(HtmlTextWriterTag tag, bool enableMouseClick, string cssClass, string style, string toolTip)
        {
            string mouseClick = String.Empty;
            if (enableMouseClick)
            {
                mouseClick = " onclick=\"return false;\"";
            }

            if (!string.IsNullOrEmpty(cssClass))
            {
                cssClass = " class=\"" + cssClass + "\"";
            }

            if (!string.IsNullOrEmpty(toolTip))
            {
                toolTip = " title=\"" + toolTip + "\"";
            }

            if (!string.IsNullOrEmpty(style))
            {
                style = " style=\"" + style + "\"";
            }

            return "<" + tag.ToString().ToLowerCSafe() + cssClass + style + mouseClick + toolTip + ">";
        }


        /// <summary>
        /// Gets the div end tag for context menu container.
        /// </summary>
        public static string GetEndTag()
        {
            return GetEndTag(true);
        }


        /// <summary>
        /// Gets the end tag for context menu container.
        /// </summary>
        /// <param name="divTag">If true, renders as div tag, otherwise renders as span tag</param>
        public static string GetEndTag(bool divTag)
        {
            return GetEndTag(divTag ? HtmlTextWriterTag.Div : HtmlTextWriterTag.Span);
        }


        /// <summary>
        /// Gets the end tag for context menu container.
        /// </summary>
        /// <param name="tag">End tag to be rendered</param>
        public static string GetEndTag(HtmlTextWriterTag tag)
        {
            return "</" + tag.ToString().ToLowerCSafe() + ">";
        }

        #endregion
    }
}
