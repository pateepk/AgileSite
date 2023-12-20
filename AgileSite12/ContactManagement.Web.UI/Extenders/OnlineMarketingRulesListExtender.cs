using System;

using CMS;
using CMS.Base.Web.UI;
using CMS.ContactManagement.Web.UI;
using CMS.Helpers;
using CMS.MacroEngine;
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
            Control.InfoObject?.SetValue("MacroRuleResourceName", "cms.onlinemarketing");
            Control.OnExternalDataBound += OnExternalDataBound;
        }


        /// <summary>
        /// Handles external data-bound event of <see cref="UniGrid"/>.
        /// </summary>
        private object OnExternalDataBound(object sender, string sourceName, object parameter)
        {
            switch (sourceName?.ToLowerInvariant())
            {
                case "macroruleavailability":
                    if (!Enum.TryParse(parameter?.ToString(), out MacroRuleAvailabilityEnum availability))
                    {
                        availability = MacroRuleAvailabilityEnum.MainApplication;
                    }

                    return availability.ToLocalizedString(null);
            }

            return parameter;
        }
    }
}
