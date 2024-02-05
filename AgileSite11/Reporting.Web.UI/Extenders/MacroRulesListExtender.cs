using CMS;
using CMS.Base.Web.UI;
using CMS.Reporting.Web.UI;
using CMS.UIControls;

[assembly: RegisterCustomClass("MacroRulesListExtender", typeof(MacroRulesListExtender))]

namespace CMS.Reporting.Web.UI
{
    /// <summary>
    /// Macro rules list extender
    /// </summary>
    public class MacroRulesListExtender : ControlExtender<UniGrid>
    {
        /// <summary>
        /// Initializes the extender.
        /// </summary>
        public override void OnInit()
        {
            if ((Control != null) && (Control.InfoObject != null))
            {
                Control.InfoObject.SetValue("MacroRuleResourceName", "cms.reporting");
            }
        }
    }
}

