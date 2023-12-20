using System;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections.Generic;

using CMS.Base.Web.UI;
using CMS.EventLog;
using CMS.Helpers;

namespace CMS.PortalEngine.Web.UI
{
    /// <summary>
    /// Layout web part containing further zones.
    /// </summary>
    public abstract class CMSAbstractLayoutWebPart : CMSAbstractWebPart
    {
        #region "Variables"

        /// <summary>
        /// Layout container.
        /// </summary>
        protected Control mLayoutContainer = null;

        /// <summary>
        /// Layout placeholder
        /// </summary>
        protected PlaceHolder mLayoutPlaceholder = null;

        /// <summary>
        /// Web part zones list.
        /// </summary>
        protected List<CMSWebPartZone> mWebPartZones = new List<CMSWebPartZone>();

        /// <summary>
        /// String builder for building of the layout.
        /// </summary>
        protected StringBuilder sb = null;

        /// <summary>
        /// Allow design mode.
        /// </summary>
        protected bool? mAllowDesignMode = null;


        /// <summary>
        /// Constant serving as no container flag
        /// </summary>
        public static Control NO_CONTAINER = new Control();

        #endregion

        
        #region "Properties"

        /// <summary>
        /// Returns true if the web part is visible.
        /// </summary>
        public override bool IsVisible
        {
            get
            {
                // Always visible in design mode
                return IsDesign || base.IsVisible;
            }
            set
            {
                base.IsVisible = value;
            }
        }


        /// <summary>
        /// Web part zones list.
        /// </summary>
        public List<CMSWebPartZone> WebPartZones
        {
            get
            {
                return mWebPartZones;
            }
        }


        /// <summary>
        /// Returns true if the children components have any variants
        /// </summary>
        public override bool ChildrenHaveVariants
        {
            get
            {
                // Process all nested zones
                foreach (CMSWebPartZone zone in WebPartZones)
                {
                    if (zone.HasVariants || zone.ChildrenHaveVariants)
                    {
                        // If any zone has variants within itself or its children, return true
                        return true;
                    }
                }

                return false;
            }
        }


        /// <summary>
        /// Allow design mode.
        /// </summary>
        public bool AllowDesignMode
        {
            get
            {
                if (mAllowDesignMode == null)
                {
                    mAllowDesignMode = (ValidationHelper.GetBoolean(GetValue("AllowDesignMode"), true) && (ViewMode != ViewModeEnum.DesignDisabled) && (ViewMode != ViewModeEnum.EditDisabled));
                }

                return mAllowDesignMode.Value;
            }
            set
            {
                mAllowDesignMode = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor.
        /// </summary>
        public CMSAbstractLayoutWebPart()
        {
            mProvideSetPropertyScript = true;
        }


        /// <summary>
        /// Creates the control child controls
        /// </summary>
        protected override void CreateChildControls()
        {
            // Create placeholder for the layout controls
            mLayoutPlaceholder = new PlaceHolder();
            mLayoutPlaceholder.ID = "plcLayout";
            this.Controls.Add(mLayoutPlaceholder);

            mLayoutContainer = mLayoutPlaceholder;

            base.CreateChildControls();
        }


        /// <summary>
        /// Starts the rendering of the layout.
        /// </summary>
        /// <param name="keepExistingControls">If true, the existing layout controls are kept</param>
        protected void StartLayout(bool keepExistingControls = false)
        {
            ClearLayout(keepExistingControls);
        }


        /// <summary>
        /// Clears the currently generated layout
        /// </summary>
        /// <param name="keepExistingControls">If true, the existing layout controls are kept</param>
        protected virtual void ClearLayout(bool keepExistingControls)
        {
            if (!keepExistingControls && mLayoutContainer != null)
            {
                mLayoutContainer.Controls.Clear();
            }
            if (this.WebPartZones != null)
            {
                this.WebPartZones.Clear();
            }

            sb = new StringBuilder();

            mHeaderContainer = null;
        }


        /// <summary>
        /// Finishes the layout designing.
        /// </summary>
        /// <param name="addToControls">If true, the output layout is added to the controls collection</param>
        protected string FinishLayout(bool addToControls = true)
        {
            if (sb.Length > 0)
            {
                string content = sb.ToString();

                if (addToControls)
                {
                    mLayoutContainer.Controls.Add(new LiteralControl(content));
                }
                sb.Length = 0;

                return content;
            }

            return null;
        }

       
        /// <summary>
        /// Appends the add to property action (increases by 1).
        /// </summary>
        /// <param name="title">Action title</param>
        /// <param name="propertyName">Property name</param>
        /// <param name="iconClass">Action icon class</param>
        /// <param name="text">Action text</param>
        protected void AppendAddAction(string title, string propertyName, string iconClass = "icon-plus", string text = null)
        {
            AppendActionInternal(title, propertyName, 1, iconClass, text);
        }


        /// <summary>
        /// Appends the add to property action (decreases by 1).
        /// </summary>
        /// <param name="title">Action title</param>
        /// <param name="propertyName">Property name</param>
        /// <param name="iconClass">Action icon class</param>
        /// <param name="text">Action text</param>
        protected void AppendRemoveAction(string title, string propertyName, string iconClass = "icon-times", string text = null)
        {
            AppendActionInternal(title, propertyName, -1, iconClass, text);
        }


        /// <summary>
        /// Appends the ty action
        /// </summary>
        /// <param name="title">Action title</param>
        /// <param name="propertyName">Property name</param>
        /// <param name="value">The value ("1" - add, "-1" - remove)</param>
        /// <param name="iconClass">Action icon class</param>
        /// <param name="text">Action text</param>
        private void AppendActionInternal(string title, string propertyName, int value, string iconClass, string text = null)
        {
            if (text == null)
            {
                text = title;
            }

            var script = string.Format("AddToWebPartProperty('{0}', '{1}', '{2}', true);", ShortClientID, propertyName, value);

            AppendAction(script, iconClass, title, text);
        }


        /// <summary>
        /// Appends the given action
        /// </summary>
        /// <param name="script">Script to execute on click</param>
        /// <param name="iconClass">Icon class</param>
        /// <param name="title">Action title (tooltip)</param>
        /// <param name="text">Action text</param>
        protected void AppendAction(string script, string iconClass, string title, string text = null)
        {
            if (text == null)
            {
                text = title;
            }
            Append("<a href=\"#\" onclick=\"", script," return false;\" class=\"LayoutAction\"><i class=\"", iconClass, "\" tooltip=\"", title, "\" ></i><span class=\"ActionTitle\">", text, "</span></a>");
        }
        

        /// <summary>
        /// Appends the given text to the layout.
        /// </summary>
        /// <param name="text">Text to append</param>
        protected void Append(params object[] text)
        {
            foreach (object part in text)
            {
                sb.Append(part);
            }
        }


        /// <summary>
        /// Appends the items information
        /// </summary>
        /// <param name="currentItem">Current item</param>
        /// <param name="items">Items</param>
        protected void AppendItemsInfo(int currentItem, int items)
        {
            Append(
@"
<input type=""hidden"" id=""", ClientID, "_current\" name=\"", ClientID, "_current\" value=\"", currentItem, @""" />
<input type=""hidden"" id=""", ClientID, "_items\" value=\"", items, @""" />
"
            );
        }


        /// <summary>
        /// Gets the value of the last current item as saved in a cookie
        /// </summary>
        /// <param name="defaultItem">Default item index if information not found</param>
        protected int GetLastCurrentItem(int defaultItem)
        {
            string cookieName = CookieName.GetEditorCookieName(this.InstanceGUID + "Current");

            return ValidationHelper.GetInteger(CookieHelper.GetValue(cookieName), defaultItem);
        }


        /// <summary>
        /// Appends the action to switch to the next item
        /// </summary>
        /// <param name="title">Action title</param>
        protected void AppendNextItemAction(string title)
        {
            string script = string.Format("NextLayoutItem('{0}', '{1}');", ClientID, GetContainerId());

            AppendAction(script, "icon-chevron-right", title);
        }


        /// <summary>
        /// Appends the action to switch to the previous item
        /// </summary>
        /// <param name="title">Action title</param>
        protected void AppendPreviousItemAction(string title)
        {
            string script = string.Format("PreviousLayoutItem('{0}', '{1}');", ClientID, GetContainerId());

            AppendAction(script, "icon-chevron-left", title);
        }


        /// <summary>
        /// Appends the code to render the item end
        /// </summary>
        protected void AppendItemEnd()
        {
            Append("</div>");
        }


        /// <summary>
        /// Appends the code to render the item start
        /// </summary>
        /// <param name="currentItem">Current item</param>
        /// <param name="i">Item index</param>
        protected void AppendItemStart(int currentItem, int i)
        {
            bool current = (i == currentItem);

            Append("<div id=\"", ClientID, "_item", i, "\" style=\"display: ", current ? "block" : "none", ";\">");
        }


        /// <summary>
        /// Registers the script that changes the currently displayed item of the wizard
        /// </summary>
        /// <param name="offset">Offset of the change</param>
        protected void RegisterChangeLayoutItemScript(int offset)
        {
            // Change only by javascript
            string script = String.Format("ChangeLayoutItem('{0}', {1}, '{2}', '{3}');", this.ClientID, offset, this.InstanceGUID, this.GetContainerId());

            ScriptHelper.RegisterStartupScript(this, typeof(string), this.ClientID + "_change", ScriptHelper.GetScript(script));
        }


        /// <summary>
        /// Adds the web part to the layout
        /// </summary>
        /// <param name="path">Web part path</param>
        protected CMSAbstractWebPart AddWebPart(string path)
        {
            CMSAbstractWebPart part = (CMSAbstractWebPart)AddControl(path);

            return part;
        }


        /// <summary>
        /// Adds the control to the layout.
        /// </summary>
        /// <param name="path">Control path to add</param>
        protected Control AddControl(string path)
        {
            Control c = Page.LoadUserControl(path);

            AddControl(c);

            return c;
        }


        /// <summary>
        /// Adds the control to the layout.
        /// </summary>
        /// <param name="control">Control to add</param>
        protected void AddControl(Control control)
        {
            if (sb.Length > 0)
            {
                mLayoutContainer.Controls.Add(new LiteralControl(sb.ToString()));
                sb.Length = 0;
            }

            mLayoutContainer.Controls.Add(control);
        }


        /// <summary>
        /// Adds the zone with given ID to the layout.
        /// </summary>
        /// <param name="id">Zone ID</param>
        /// <param name="title">Zone title</param>
        /// <param name="container">Container to which add the zone</param>
        protected CMSWebPartZone AddZone(string id, string title, Control container = null)
        {
            // Add the zone
            CMSWebPartZone zone = new CMSWebPartZone();

            // Ensure zone title 
            PageTemplateInstance pageTemplateInstance = PartInstance.ParentZone.ParentTemplateInstance;
            WebPartZoneInstance zoneInst = pageTemplateInstance.GetZone(id);

            if (zoneInst != null) 
            {
                string zoneInstanceTitle = ValidationHelper.GetString(zoneInst.Properties["ZoneTitle"], String.Empty);
                title = !String.IsNullOrEmpty(zoneInstanceTitle) ? zoneInstanceTitle : title;
            }

            zone.PagePlaceholder = PagePlaceholder;
            zone.ID = id;
            zone.ZoneTitle = title;
            zone.LayoutZone = true;
            zone.ParentWebPart = this;

            if (IsDesign)
            {
                zone.CssClass = "NotHandle";
            }

            var parentZone = this.ParentZone;
            zone.LayoutType = parentZone.LayoutType;

            // Set the widget zone
            if (IsWidget)
            {
                zone.WidgetZoneType = parentZone.WidgetZoneType;
            }

            if (container == null)
            {
                // Add to the flow of the layout
                AddControl(zone);
            }
            else if (container != NO_CONTAINER)
            {
                // Add to specific container
                container.Controls.Add(zone);
            }

            WebPartZones.Add(zone);

            PagePlaceholder.Layout.RegisterZone(zone);

            return zone;
        }


        /// <summary>
        /// Adds the container for the header to the layout.
        /// </summary>
        protected PlaceHolder AddHeaderContainer()
        {
            // Add the header placeholder
            PlaceHolder plcHeader = new PlaceHolder();
            plcHeader.ID = "plcHeader";
            mHeaderContainer = plcHeader;
            AddControl(plcHeader);

            return plcHeader;
        }


        /// <summary>
        /// Loads the web part layout.
        /// </summary>
        public virtual void LoadLayout(bool reloadData)
        {
            // Always maintain scroll position if the layout is in design mode
            if (IsDesign)
            {
                Page.MaintainScrollPositionOnPostBack = true;
            }
            
            if (PartInstance != null)
            {
                // Ensure the macros to be resolved if enabled
                mDisableMacros = ValidationHelper.GetBoolean(GetValue("DisableMacros"), false);
            }

            try
            {
                // Set the control cycle status
                if (mPageCycle < PageCycleEnum.ParentInitializing)
                {
                    mPageCycle = PageCycleEnum.ParentInitializing;
                }

                EnsureChildControls();

                // Prepare the layout
                PrepareLayout();

                // Reposition the header if header container created
                if ((mHeaderContainer != null) && (mHeaderControl != null) && (mHeaderControl.Parent != mHeaderContainer))
                {
                    mHeaderControl.Parent.Controls.Remove(mHeaderControl);
                    mHeaderContainer.Controls.AddAt(0, mHeaderControl);
                }

                // Load the zones content
                LoadZones(reloadData);
            }
            catch (Exception ex)
            {
                // Log the error to event log
                EventLogProvider.LogException("Design", "LOADLAYOUT", ex);

                ClearLayout(false);

                // Generate error report
                string errorTitle = WebPartError.GetErrorTitle(this.IsWidget, this.PartInstance, this.ID);
                WebPartError.AddErrorControls(mLayoutContainer, errorTitle, ex);
            }
        }


        /// <summary>
        /// Prepares the web part layout.
        /// </summary>
        protected virtual void PrepareLayout()
        {
            // Do nothing by default
        }


        /// <summary>
        /// Loads the web part zones contained within the web part.
        /// </summary>
        protected virtual void LoadZones(bool reloadData)
        {
            CMSAbstractLayout layout = PagePlaceholder.Layout;
            PageTemplateInstance pti = layout.TemplateInstance;

            // Load the zones
            foreach (CMSWebPartZone zone in WebPartZones)
            {
                layout.LoadZone(zone, reloadData);
            }
        }


        /// <summary>
        /// Returns true, if the web part view mode is design mode
        /// </summary>
        protected bool ViewModeIsDesign()
        {
            return PortalContext.IsDesignMode(ViewMode, false);
        }

        #endregion
    }
}
