using System.Web.Mvc;

namespace Kentico.PageBuilder.Web.Mvc.PageTemplates.Internal
{
    /// <summary>
    /// Default controller for page template.
    /// </summary>
    public sealed class KenticoPageTemplateDefaultController : PageTemplateController
    {
        /// <summary>
        /// Default action for markup retrieval.
        /// </summary>
        public ActionResult Index()
        {
            var model = this.GetModel();
            var viewName = this.GetViewName();

            return PartialView(viewName, model);
        }
    }
}