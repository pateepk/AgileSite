using System;
using System.Collections;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.ComponentModel;

using CMS.Base.Web.UI;
using CMS.Helpers;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Scroll panel control.
    /// </summary>
    public class ScrollPanel : CMSPanel
    {
        #region "Private variables"

        private int? mScrollAreaDefaultSize = null;
        private readonly CMSPanel mScrollAreaContainer = new CMSPanel { ID = "s"};

        #endregion


        #region "Public properties"

        /// <summary>
        /// Gets or sets the layout of the scroll panel.
        /// </summary>
        [DefaultValue(RepeatDirection.Horizontal)]
        public RepeatDirection Layout
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the forward scroller control ID.
        /// </summary>
        public string ForwardScrollerControlID
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the backward scroller control ID.
        /// </summary>
        public string BackwardScrollerControlID
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the CSS class of the inner items which are to be scrolled.
        /// </summary>
        public string InnerItemClass
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the CSS class of the item separator.
        /// </summary>
        public string InnerItemSeparatorClass
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the scroll step in pixels.
        /// </summary>
        [DefaultValue(300)]
        public int ScrollStep
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the scroll area CSS class.
        /// </summary>
        public string ScrollAreaCssClass
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the default size of the inner scroll area.
        /// </summary>
        public int ScrollAreaDefaultSize
        {
            get
            {
                if (mScrollAreaDefaultSize.HasValue)
                {
                    return mScrollAreaDefaultSize.Value;
                }

                return 0;
            }
            set
            {
                mScrollAreaDefaultSize = value;
            }
        }


        /// <summary>
        /// Gets the scroll area container.
        /// </summary>
        public CMSPanel ScrollAreaContainer
        {
            get
            {
                return mScrollAreaContainer;
            }
        }


        /// <summary>
        /// Indicates whether the scroll panel is used in a RTL culture.
        /// </summary>
        public bool IsRTL
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the javascript handler raised after the forward scroll event.
        /// </summary>
        public string OnForwardScroll
        {
            get;
            set;
        }

        #endregion


        #region "Constructor"

        /// <summary>
        /// Initializes a new instance of the <see cref="ScrollPanel"/> class.
        /// </summary>
        public ScrollPanel()
        {
            ScrollStep = 300;
            Layout = RepeatDirection.Horizontal;
        }

        #endregion


        #region "Page events"

        /// <summary>
        /// Init event handler.
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            Controls.Add(ScrollAreaContainer);
            base.OnInit(e);
        }
        

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.PreRender"/> event.
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (!Visible)
            {
                // Do not render script
                return;
            }

            // Register the JavaScript files
            ScriptHelper.RegisterJQueryTools(Page);
            ScriptHelper.RegisterScriptFile(Page, "Controls/ScrollPanel.js");

            // Register script that will ensure repositioning of the scroll panel buttons and the container height when CKToolbar is displayed
            string script = @"
                if (window.CKEDITOR) {
                    CKEDITOR.on('instanceCreated', function(e) {
                        e.editor.on('instanceReady', function(e) { spRefreshScrollButtons(); });
                        e.editor.on('focus', function(e) { spRefreshScrollButtons(); });
                        e.editor.on('afterCommandExec', function(e) {
                            if (e.data.name === 'toolbarCollapse') {
                                spRefreshScrollButtons();
                            }
                        });
                    });
                }";

            ScriptHelper.RegisterStartupScript(this, typeof(string), "spCKEditor", script, true);

            // Setup the panel as a scrollable panel
            Style.Add("overflow", "hidden");

            // Scroll controls
            WebControl forwardScrollControl = null;
            WebControl backwardScrollControl = null;

            // Initialize the forward scroller
            if (!string.IsNullOrEmpty(ForwardScrollerControlID))
            {
                forwardScrollControl = FindControl(ForwardScrollerControlID) as WebControl;
                if (forwardScrollControl != null)
                {
                    // Assign the onclick event
                    forwardScrollControl.Attributes.Add("onclick", "spScrollForward('" + ClientID + "'); if (event.stopImmediatePropagation) { event.stopImmediatePropagation(); }");
                    // Hide the forward scroll control
                    forwardScrollControl.Style.Add("display", "none");
                }
                else
                {
                    return;
                }
            }

            // Initialize the backward scroller
            if (!string.IsNullOrEmpty(BackwardScrollerControlID))
            {
                backwardScrollControl = FindControl(BackwardScrollerControlID) as WebControl;
                if (backwardScrollControl != null)
                {
                    // Assign the onclick event
                    backwardScrollControl.Attributes.Add("onclick", "spScrollBack('" + ClientID + "'); if (event.stopImmediatePropagation) { event.stopImmediatePropagation(); }");
                    // Hide the backward scroll control
                    backwardScrollControl.Style.Add("display", "none");
                }
                else
                {
                    return;
                }
            }

            // Build the init scroll panel script
            StringBuilder sb = new StringBuilder();
            sb.Append(@"
$cmsj(document).ready(function () {
    var spObj = new Object;
    spObj.ForwardScroller = $cmsj('#", forwardScrollControl.ClientID, @"');
    spObj.BackwardScroller = $cmsj('#", backwardScrollControl.ClientID, @"');
    spObj.OnForwardScroll = ", (!string.IsNullOrEmpty(OnForwardScroll) ? OnForwardScroll : "null" ), @";
    spObj.ItemClass = '", InnerItemClass, @"';
    spObj.ItemSeparatorClass = '", InnerItemSeparatorClass, @"';
    spObj.LastItem = null;
    spObj.SeparatorItem = null;
    spObj.Container = null;
    spObj.InnerContainerId = '", ScrollAreaContainer.ClientID, @"';
    spObj.ScrollAreaContainer = null;
    spObj.AdjustScrollAreaSize = ", (!mScrollAreaDefaultSize.HasValue ? "true" : "false"), @";
    spObj.IsVerticalLayout = ", (Layout == RepeatDirection.Vertical ? "true" : "false"), @";
    spObj.ScrollStep = ", ScrollStep, @";
    spObj.ForwardScrollStep = null;
    spObj.ItemWidth = null;
    spObj.ItemHeight = null;
    spObj.IsRTL = ", (IsRTL ? "true" : "false"), @";
    spObj.ForwardScrollEnabled = false;
    scrollPanels['", ClientID, @"'] = spObj;
    scrollPanelInit('", ClientID, @"', true);
})");
            ScriptHelper.RegisterStartupScript(this, typeof(string), "scrollPanel_" + ClientID, sb.ToString(), true);
        }


        /// <summary>
        /// Render event handler.
        /// </summary>
        protected override void Render(HtmlTextWriter writer)
        {
            if (DesignMode)
            {
                writer.Write("[ScrollPanel: " + ID + "]");
            }
            else
            {
                base.Render(writer);
            }
        }


        /// <summary>
        /// Outputs the content of a server control's children to a provided <see cref="T:System.Web.UI.HtmlTextWriter"/> object, which writes the content to be rendered on the client.
        /// </summary>
        protected override void RenderChildren(HtmlTextWriter writer)
        {
            string toRender = "<div id=\"" + ScrollAreaContainer.ClientID + "\"";
            if (!string.IsNullOrEmpty(ScrollAreaCssClass))
            {
                toRender += " class=\"" + ScrollAreaCssClass + "\"";
            }
            if (mScrollAreaDefaultSize.HasValue)
            {
                // Set the width/height of the scroll area when defined
                string sizeType = (Layout == RepeatDirection.Horizontal) ? "width" : "height";
                toRender += " style=\"" + sizeType + ": " + mScrollAreaDefaultSize.Value.ToString() + "px;" + "\"";
            }
            toRender += ">";
            writer.Write(toRender);

            foreach (Control control in Controls)
            {
                if(control != ScrollAreaContainer)
                {
                    control.RenderControl(writer);
                }
            }

            writer.Write("</div>");
        }

        #endregion
    }
}
