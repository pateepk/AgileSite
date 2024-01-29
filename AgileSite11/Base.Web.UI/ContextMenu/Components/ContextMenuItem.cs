using System;
using System.ComponentModel;
using System.Web.UI.WebControls;
using System.Web.UI;

using CMS.Base.Web.UI;
using CMS.Helpers;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Separator for context menu.
    /// </summary>
    [ToolboxItem(false)]
    public class ContextMenuItem : CMSPanel, IPostBackEventHandler, INamingContainer
    {
        #region "Variables"

        private Image mIcon = null;
        private LocalizedLabel mLabel = null;
        private ContextMenuContainer mSubMenuContainer = null;

        private bool controlsLoaded = false;

        #endregion


        #region "Properties"

        /// <summary>
        /// If true, the item is considered last.
        /// </summary>
        public bool Last
        {
            get;
            set;
        }


        /// <summary>
        /// Context menu container for submenu.
        /// </summary>
        public ContextMenuContainer SubMenuContainer
        {
            get
            {
                if (mSubMenuContainer == null)
                {
                    mSubMenuContainer = new ContextMenuContainer();
                    mSubMenuContainer.ID = "sm";
                    mSubMenuContainer.EnableViewState = false;
                }

                return mSubMenuContainer;
            }
            set
            {
                mSubMenuContainer = value;
            }
        }


        /// <summary>
        /// Item label.
        /// </summary>
        public LocalizedLabel Label
        {
            get
            {
                if (mLabel == null)
                {
                    mLabel = new LocalizedLabel();
                    mLabel.CssClass = "Name";
                    mLabel.ID = "l";
                    mLabel.EnableViewState = false;
                }

                return mLabel;
            }
            set
            {
                mLabel = value;
            }
        }


        /// <summary>
        /// Label text.
        /// </summary>
        public string Text
        {
            get
            {
                return Label.Text;
            }
            set
            {
                Label.Text = value;
            }
        }


        /// <summary>
        /// If set, the item text is generated from the given resource string
        /// </summary>
        public string ResourceString
        {
            get;
            set;
        }


        /// <summary>
        /// ID of the sub context menu.
        /// </summary>
        public string SubMenuID
        {
            get
            {
                return SubMenuContainer.MenuID;
            }
            set
            {
                SubMenuContainer.MenuID = value;
            }
        }


        /// <summary>
        /// URL to which the context menu item redirects
        /// </summary>
        public string RedirectUrl
        {
            get;
            set;
        }


        /// <summary>
        /// If set, the value is assigned to the onclick attribute
        /// </summary>
        public string OnClientClick
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Click event
        /// </summary>
        public event EventHandler Click;


        /// <summary>
        /// Constructor.
        /// </summary>
        public ContextMenuItem()
        {
            CssClass = "Item";
        }


        /// <summary>
        /// PreRender event handler.
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            LoadControls();
        }


        /// <summary>
        /// Loads the controls to the item control
        /// </summary>
        private void LoadControls()
        {
            if (controlsLoaded)
            {
                return;
            }
            controlsLoaded = true;

            if ((Controls.Count == 0) && (mIcon == null) && (mLabel == null) && String.IsNullOrEmpty(ResourceString))
            {
                Visible = false;
                return;
            }

            // Apply the resource string
            if (String.IsNullOrEmpty(Text) && !String.IsNullOrEmpty(ResourceString))
            {
                Text = ResHelper.GetString(ResourceString);
            }

            // Setup the control
            if (Last)
            {
                CssClass = "item-last";
            }

            Controls.AddAt(0, new LiteralControl("<div class=\"ItemPadding\">"));

            // Add icon
            if (mIcon != null)
            {
                Controls.Add(mIcon);
            }

            if ((mIcon != null) && (mLabel != null))
            {
                Controls.Add(new LiteralControl("&nbsp;"));

                // Set alternate text for icon if missing
                if (String.IsNullOrEmpty(mIcon.AlternateText))
                {
                    mIcon.AlternateText = mLabel.Text;
                }
            }

            // Add label
            if (mLabel != null)
            {
                Controls.Add(mLabel);
            }

            Controls.Add(new LiteralControl("</div>"));

            string onClick = null;

            // Client click event
            if (!String.IsNullOrEmpty(OnClientClick))
            {
                onClick += OnClientClick;
            }

            // Redirect URL
            if (!String.IsNullOrEmpty(RedirectUrl))
            {
                string url = UrlResolver.ResolveUrl(RedirectUrl);

                onClick += "window.location.replace('" + ScriptHelper.GetString(url, false) + "');";
            }

            // Add click event
            if (Click != null)
            {
                onClick += this.Page.ClientScript.GetPostBackEventReference(this, null);
            }

            // Add onclick code
            if (!String.IsNullOrEmpty(onClick))
            {
                this.Attributes.Add("onclick", onClick);
            }
        }


        /// <summary>
        /// Postback event handling
        /// </summary>
        /// <param name="eventArgument">Event argument</param>
        public void RaisePostBackEvent(string eventArgument)
        {
            // Raise the click event
            if (Click != null)
            {
                Click(this, new EventArgs());
            }
        }


        /// <summary>
        /// Render event handler.
        /// </summary>
        protected override void Render(HtmlTextWriter writer)
        {
            LoadControls();

            if (mSubMenuContainer != null)
            {
                mSubMenuContainer.RenderStartTag(writer);
            }

            base.Render(writer);

            if (mSubMenuContainer != null)
            {
                mSubMenuContainer.RenderEndTag(writer);
            }
        }

        #endregion
    }
}