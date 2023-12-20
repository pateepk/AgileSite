using System.ComponentModel;
using System.Web.Hosting;
using System.Web.Http.Description;
using System.Web.Mvc;

namespace Kentico.Builder.Web.Mvc.Internal
{
    /// <summary>
    /// Controller to serve localization for resource strings to client side.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [ApiExplorerSettings(IgnoreApi = true)]
    public sealed class KenticoLocalizationScriptController : Controller
    {
        private readonly ILocalizationScriptProvider localizationScriptProvider;


        /// <summary>
        /// Creates an instance of <see cref="KenticoLocalizationScriptController"/> class.
        /// </summary>
        public KenticoLocalizationScriptController()
        {
            var resourcesPath = HostingEnvironment.MapPath("~/App_Data/Global/Resources");
            localizationScriptProvider = new LocalizationScriptProvider(resourcesPath);
        }


        internal KenticoLocalizationScriptController(ILocalizationScriptProvider localizationScriptProvider)
        {
            this.localizationScriptProvider = localizationScriptProvider;
        }


        /// <summary>
        /// Gets the script with localization of resource strings.
        /// </summary>
        /// <param name="cultureCode">Culture code of the localization.</param>
        [HttpGet]
        public ActionResult Script(string cultureCode)
        {
            var script = localizationScriptProvider.Get(cultureCode);

            return Content(script, "application/javascript");
        }
    }
}