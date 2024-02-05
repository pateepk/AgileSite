using System;

using CMS;
using CMS.FormEngine.Web.UI;
using CMS.Core;
using CMS.Newsletters.Web.UI;
using CMS.UIControls;

[assembly: RegisterCustomClass("EmailEditTabsExtender", typeof(EmailEditTabsExtender))]

namespace CMS.Newsletters.Web.UI
{
    /// <summary>
    /// Extender vertical tabs within email detail.
    /// </summary>
    public class EmailEditTabsExtender : UITabsExtender
    {
        /// <summary>
        /// Initialization of tabs.
        /// </summary>
        public override void OnInitTabs()
        {
            Control.OnTabCreated += Control_OnTabCreated;
        }


        private void Control_OnTabCreated(object sender, TabCreatedEventArgs e)
        {
            var uiElement = e.UIElement;

            if (uiElement.ElementName.Equals(EmailBuilderHelper.EMAIL_BUILDER_UI_ELEMENT, StringComparison.OrdinalIgnoreCase))
            {
                var uiContext = UIContext.Current;
                var issue = uiContext.EditedObject as IssueInfo;

                if ((issue != null) && issue.IssueIsABTest)
                {
                    e.Tab.RedirectUrl = EmailBuilderHelper.GetOriginalVariantIssueUrl(issue);
                }
            }
        }
    }
}
