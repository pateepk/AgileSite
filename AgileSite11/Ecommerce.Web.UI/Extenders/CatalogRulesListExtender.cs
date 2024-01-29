using CMS;
using CMS.Base.Web.UI;
using CMS.UIControls;

[assembly: RegisterCustomClass("CatalogRulesListExtender", typeof(CMS.Ecommerce.Web.UI.CatalogRulesListExtender))]

namespace CMS.Ecommerce.Web.UI
{
    /// <summary>
    /// Catalog rules list extender
    /// </summary>
    public class CatalogRulesListExtender : ControlExtender<UniGrid>
    {
        /// <summary>
        /// Initializes the extender.
        /// </summary>
        public override void OnInit()
        {
            Control?.InfoObject?.SetValue("MacroRuleResourceName", "com.catalogdiscount");
        }
    }
}
