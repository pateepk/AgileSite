using System;

using CMS;
using CMS.Base.Web.UI;
using CMS.Helpers;
using CMS.MacroEngine;
using CMS.UIControls;

[assembly: RegisterCustomClass("MacroRuleExtender", typeof(MacroRuleExtender))]

namespace CMS.UIControls
{
    /// <summary>
    /// Macro rule list <see cref="UniGrid"/> extender.
    /// </summary>
    public class MacroRuleExtender : ControlExtender<UniGrid>
    {
        /// <summary>
        /// OnInit event.
        /// </summary>
        public override void OnInit()
        {
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
                    if(!Enum.TryParse(parameter?.ToString(), out MacroRuleAvailabilityEnum availability))
                    {
                        availability = MacroRuleAvailabilityEnum.MainApplication;
                    }

                    return availability.ToLocalizedString(null);
            }

            return parameter;
        }
    }
}
