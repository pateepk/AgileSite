using CMS;
using CMS.Ecommerce.Web.UI;
using CMS.Base.Web.UI;
using CMS.UIControls;

[assembly: RegisterCustomClass("OrderRulesListExtender", typeof(OrderRulesListExtender))]

namespace CMS.Ecommerce.Web.UI
{
    /// <summary>
    /// Order rules list extender
    /// </summary>
    public class OrderRulesListExtender : ControlExtender<UniGrid>
    {
        /// <summary>
        /// Initializes the extender.
        /// </summary>
        public override void OnInit()
        {
            Control?.InfoObject?.SetValue("MacroRuleResourceName", "com.orderdiscount");
        }
    }
}