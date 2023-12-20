using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Helpers;
using CMS.PortalEngine;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Panel.
    /// </summary>
    [ToolboxItem(true)]
    public class CMSPanel : Panel, IShortID
    {
        #region "Variables"

        private bool? mIsLiveSite = null;
        private bool mFixedPosition = false;

        #endregion


        #region "Properties"

        /// <summary>
        /// Short ID of the control.
        /// </summary>
        public string ShortID
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if control is used on live site.
        /// </summary>
        public virtual bool IsLiveSite
        {
            get
            {
                if (mIsLiveSite == null)
                {
                    IAdminPage page = Page as IAdminPage;
                    mIsLiveSite = (page == null);

                    // Try to get the property value from parent controls
                    mIsLiveSite = ControlsHelper.GetParentProperty<AbstractUserControl, bool>(this, s => s.IsLiveSite, mIsLiveSite.Value);
                }

                return mIsLiveSite.Value;
            }
            set
            {
                mIsLiveSite = value;
            }
        }


        /// <summary>
        /// Indicates if panel should be always on top of a page.
        /// </summary>
        public virtual bool FixedPosition
        {
            get
            {
                return mFixedPosition;
            }
            set
            {
                mFixedPosition = value;
            }
        }


        /// <summary>
        /// If true, only child controls are render to the output
        /// </summary>
        public bool RenderChildrenOnly
        {
            get;
            set;
        }


        /// <summary>
        /// Parent client ID to be used for fixed panels placeholders
        /// </summary>
        public string FixedParentClientID
        {
            get;
            set;
        }


        /// <summary>
        /// Resource string for the grouping text
        /// </summary>
        public string GroupingTextResourceString
        {
            get;
            set;
        }


        /// <summary>
        /// Determines whether is possible to collapse the panel by clicking its grouping text.
        /// </summary>
        public bool Collapsible
        {
            get;
            set;
        }


        /// <summary>
        /// Initial collapsed state.
        /// </summary>
        public bool IsCollapsed
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Init event handler.
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected override void OnInit(EventArgs e)
        {
            this.SetShortID();

            base.OnInit(e);
        }


        /// <summary>
        /// PreRender event handler
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (Visible && FixedPosition)
            {
                ScriptHelper.RegisterJQuery(Page);
                ScriptHelper.RegisterScriptFile(Page, "Controls/CMSPanel.js");

                // Register panel script
                ScriptHelper.RegisterClientScriptBlock(this, typeof(string), "fixpanel_" + ClientID, String.Format("$cmsj(document).ready(function () {{ CMSFixPosition('{0}', '{1}'); }});$cmsj(window).resize(function() {{ CMSFixPosition('{0}', '{1}'); }});", ClientID, FixedParentClientID), true);
            }

            // Apply localization of the grouping text
            if (String.IsNullOrEmpty(GroupingText) && !String.IsNullOrEmpty(GroupingTextResourceString))
            {
                GroupingText = ResHelper.GetString(GroupingTextResourceString);
            }

            if ((Collapsible || IsCollapsed) && !String.IsNullOrEmpty(GroupingText))
            {
                ScriptHelper.RegisterJQuery(Page);

                string script = @"
var legend_" + ClientID + @" = $cmsj('#" + ClientID + @" fieldset legend');
legend_" + ClientID + @".click(function() {
    var lgdElem = $cmsj(this);
    lgdElem.siblings().toggle('slow');
    lgdElem.parent().toggleClass('CollapsedFieldset');
});
legend_" + ClientID + @".css('cursor','pointer');
";
                if(IsCollapsed)
                {
                    script += @"
legend_" + ClientID + @".siblings().hide();
legend_" + ClientID + @".parent().addClass('CollapsedFieldset');
";
                }
                
                ScriptHelper.RegisterStartupScript(this, typeof(string), "collapsibleFieldSet_" + ClientID, script, true);
            }
        }


        /// <summary>
        /// Render event handler
        /// </summary>
        /// <param name="writer">Writer</param>
        protected override void Render(HtmlTextWriter writer)
        {
            if (RenderChildrenOnly)
            {
                RenderChildren(writer);
            }
            else
            {
                base.Render(writer);
            }
        }

        #endregion
    }
}
