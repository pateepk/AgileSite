using System;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Base.Web.UI;
using CMS.Helpers;

namespace CMS.Search.Web.UI
{
    /// <summary>
    /// Closable control which displays specified info message with info icon.
    /// </summary>
    [ToolboxData("<{0}:InfoMessage runat=server></{0}:InfoMessage>")]
    public class InfoMessage : WebControl
    {
        /// <summary>
        /// Message to show.
        /// </summary>
        public string Message
        {
            get; set;
        }


        /// <summary>
        /// Initializes a new instance of the InfoMessage class.
        /// </summary>
        public InfoMessage()
            : base("div")
        {
        }


        /// <summary>
        /// PreRender event handler
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            
            ScriptHelper.RegisterClientScriptBlock(Page, typeof(string), "infomessageclose", @"
function CMSCloseInfoMessage(id) {
    var elm = $cmsj('#' + id);
    $cmsj('#' + id + ' i').click(function () {
        elm.hide();
    });
}",
                true);

            CssClass += " alert-dismissable alert-info alert";
        }


        /// <summary>
        /// Renders the contents to the specified writer.
        /// </summary>
        protected override void RenderContents(HtmlTextWriter writer)
        {
            base.RenderContents(writer);

            writer.Write("<span class=\"alert-icon\">");

            CMSIcon icon = new CMSIcon()
            {
                ID = "iconAlert",
                CssClass = "icon-i-circle",
                AlternativeText = ResHelper.LocalizeString("general.info")
            };
            icon.RenderControl(writer);
            writer.Write("</span>");

            writer.Write("<div class=\"alert-label\">");
            Label label = new Label()
            {
                ID = "lblText",
                Text = Message
            };
            label.RenderControl(writer);
            writer.Write("</div>");

            writer.Write("<span class=\"alert-close\">");
            writer.Write("<i class=\"close icon-modal-close\"></i>");
            writer.Write("<span class=\"sr-only\">Close</span>");

            writer.Write("</span>");

            string script = $@"
$cmsj(document).ready(function() {{
    if (window.CMSCloseInfoMessage) {{ 
        CMSCloseInfoMessage('{ClientID}'); 
    }} 
}});";
            ScriptHelper.RegisterStartupScript(Page, typeof(string), "infomessageclose_" + ClientID, script, true);
        }
    }
}
