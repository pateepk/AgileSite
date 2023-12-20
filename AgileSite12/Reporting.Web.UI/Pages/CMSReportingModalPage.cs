using System;

using CMS.Base.Web.UI;
using CMS.UIControls;

namespace CMS.Reporting.Web.UI
{
    /// <summary>
    /// Base page for the CMS Reporting modal pages to apply global settings to the pages.
    /// </summary>
    [UIElement("CMS.Reporting", "Reporting")]
    public abstract class CMSReportingModalPage : CMSModalPage
    {
        #region "Constants"

        /// <summary>
        /// Short link to help page.
        /// </summary>
        private const string HELP_TOPIC_LINK = "reports_creating";

        #endregion


        #region "Methods"

        /// <summary>
        /// OnLoad event handler.
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (CurrentMaster != null)
            {
                PageTitle.HelpTopicName = HELP_TOPIC_LINK;
                CurrentMaster.PanelContent.RemoveCssClass("dialog-content");
            }
        }


        /// <summary>
        /// Register javascript for resize and rollbacks events
        /// </summary>
        /// <param name="footerID">ID of page's footer</param>
        /// <param name="panelID">ID of page's main panel</param>
        protected void RegisterScripts(string footerID, string panelID)
        {
            ScriptHelper.RegisterJQuery(Page);

            string script = 
@"$cmsj(document.body).ready(initializeResize);
           
function initializeResize() {
    resizeareainternal();
    $cmsj(window).resize(function() { resizeareainternal(); });
}

function resizeareainternal() {
    var height = document.body.clientHeight;
    var panel = document.getElementById('" + panelID + @"');
    var footer = document.getElementById('" + footerID + @"');
    var header = document.getElementById('" + CurrentMaster.HeaderContainer.ClientID + @"');
    panel.style.height = (height - footer.clientHeight - panel.offsetTop - header.clientHeight) + 'px';
}

function RefreshContent() { 
    if (wopener.ReloadPage) {
        wopener.ReloadPage();
    }
}";

            ScriptHelper.RegisterClientScriptBlock(this, typeof(string), "ResizePageScript", ScriptHelper.GetScript(script));
        }

        #endregion
    }
}
