using System;
using System.Collections.Generic;

using CMS;
using CMS.Base.Web.UI;
using CMS.Core;
using CMS.Personas.Web.UI.Internal;
using CMS.UIControls;

[assembly: RegisterCustomClass("PersonaReportExtender", typeof(PersonaReportExtender))]

namespace CMS.Personas.Web.UI.Internal
{
    /// <summary>
    /// Personas report extender
    /// </summary>
    public sealed class PersonaReportExtender : PageExtender<CMSPage>
    {
        /// <summary>
        /// Initializes the page.
        /// </summary>
        public override void OnInit()
        {
            Page.Load += PageOnLoad;
        }


        private void PageOnLoad(object sender, EventArgs eventArgs)
        {
            ScriptHelper.RegisterRequireJs(Page);
            var personasConfiguration = Service.Resolve<IPersonaReportExtenderService>().GetPersonaConfiguration();
            var localizationService = Service.Resolve<ILocalizationService>();

            ScriptHelper.RegisterModule(Page, "CMS.Personas/Report/report", new
            {
                personas = personasConfiguration,
                resourceStrings = new Dictionary<string, string>
                {
                    {"personas.personareport.header", localizationService.GetString("personas.personareport.header")},
                    {"personas.personareport.nodata", localizationService.GetString("personas.personareport.nodata")},
                    {"personas.personareport.showall", localizationService.GetString("personas.personareport.showall")}
                }                
            });
        }
    }
}