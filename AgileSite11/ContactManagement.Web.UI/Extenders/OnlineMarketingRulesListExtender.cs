using CMS;
using CMS.Base.Web.UI;
using CMS.ContactManagement.Web.UI;
using CMS.UIControls;

[assembly: RegisterCustomClass("OnlineMarketingRulesListExtender", typeof(OnlineMarketingRulesListExtender))]

namespace CMS.ContactManagement.Web.UI
{
    /// <summary>
    /// Macro rule list extender in OM/Configuration. Sets "MacroRuleResourceName" to OM to the edited object, so that
    /// right permissions (Contact Management -> Read global configuration) can be set in Contact management module.
    /// </summary>
    public class OnlineMarketingRulesListExtender : ControlExtender<UniGrid>
    {
        /// <summary>
        /// OnInit event handler
        /// </summary>
        public override void OnInit()
        {
            if ((Control != null) && (Control.InfoObject != null))
            {
                Control.InfoObject.SetValue("MacroRuleResourceName", "cms.onlinemarketing");
            }
        }
    }
}
