using CMS;
using CMS.Base.Web.UI;
using CMS.Helpers;
using CMS.UIControls;

[assembly: RegisterCustomClass("MVCPageTemplatesListingExtender", typeof(MVCPageTemplatesListingExtender))]

namespace CMS.UIControls
{
    /// <summary>
    /// Extender for grid with page templates (MVC) .
    /// </summary>
    public class MVCPageTemplatesListingExtender : ControlExtender<UniGrid>
    {
        /// <summary>
        /// Initializes the extender.
        /// </summary>
        public override void OnInit()
        {
            var smartTip = ControlsHelper.GetChildControl<SmartTipControl>(Control.Page);
            
            if (smartTip == null)
            {
                return;
            }

            string link = $"<a target='_blank' href='{DocumentationHelper.GetDocumentationTopicUrl("page_templates_using_mvc")}'>{Control.GetString("app.pagetemplatesmvc.smarttip.link")}</a>";
            smartTip.Content += link;
        }
    }
}
