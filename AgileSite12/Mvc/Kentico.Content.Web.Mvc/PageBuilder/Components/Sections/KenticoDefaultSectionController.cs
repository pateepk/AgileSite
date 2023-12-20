using System.Web.Mvc;

namespace Kentico.PageBuilder.Web.Mvc.Sections
{
    /// <summary>
    /// Default built-in PageBuilder section.
    /// </summary>
    public class KenticoDefaultSectionController : Controller
    {
        /// <summary>
        /// Default action.
        /// </summary>
        public ActionResult Index()
        {
            return PartialView("~/Views/Shared/Kentico/PageBuilder/_DefaultSection.cshtml");
        }
    }
}
