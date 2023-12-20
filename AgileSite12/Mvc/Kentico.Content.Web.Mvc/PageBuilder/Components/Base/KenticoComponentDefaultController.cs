using System.Web.Mvc;

namespace Kentico.PageBuilder.Web.Mvc.Internal
{
    /// <summary>
    /// Default controller for Page builder component.
    /// </summary>
    public sealed class KenticoComponentDefaultController : ComponentController
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