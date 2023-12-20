using System.Web.Mvc;

using Kentico.Forms.Web.Mvc;
using Kentico.Forms.Web.Mvc.Sections;

[assembly: RegisterFormSection(KenticoDefaultFormSectionController.IDENTIFIER, typeof(KenticoDefaultFormSectionController), "{$kentico.formbuilder.section.default.name$}", Description = "{$kentico.formbuilder.section.default.description$}", IconClass = "icon-square")]
[assembly: RegisterFormSection(KenticoDefaultFormSectionController.IDENTIFIER_UNKNOWN, typeof(KenticoDefaultFormSectionController), "{$kentico.formbuilder.section.default.name$}")]

namespace Kentico.Forms.Web.Mvc.Sections
{
    /// <summary>
    /// Default form section
    /// </summary>
    public class KenticoDefaultFormSectionController : Controller
    {
        /// <summary>
        /// Represents the Default section identifier.
        /// </summary>
        public const string IDENTIFIER = "Kentico.DefaultSection";

        /// <summary>
        /// Represents identifier of the section which replaces faulty sections.
        /// </summary>
        public const string IDENTIFIER_UNKNOWN = "Kentico.UnknownSection";


        /// <summary>
        /// Default action
        /// </summary>
        public ActionResult Index()
        {
            return PartialView("~/Views/Shared/Kentico/FormBuilder/_DefaultSection.cshtml");
        }
    }
}