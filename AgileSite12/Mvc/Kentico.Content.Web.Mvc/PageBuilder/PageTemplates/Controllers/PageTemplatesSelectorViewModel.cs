using System.Linq;

namespace Kentico.PageBuilder.Web.Mvc.PageTemplates.Internal
{
    /// <summary>
    /// Model for page template selector encapsulating custom and default templates list along with smart tip.
    /// </summary>
    public class PageTemplatesSelectorViewModel
    {
        /// <summary>
        /// Encapsulates list of custom and default templates.
        /// </summary>
        public PageTemplatesViewModel Templates {get; set;}


        /// <summary>
        /// Text of the info label displayed in selector.
        /// </summary>
        public string InfoMessage { get; set; }


        /// <summary>
        /// Indicates presence of default templates in <see cref="Templates"/>.
        /// </summary>
        public bool HasDefaultTemplates => Templates.DefaultPageTemplates.Any();


        /// <summary>
        /// Indicates presence of custom templates in <see cref="Templates"/>.
        /// </summary>
        public bool HasCustomTemplates => Templates.CustomPageTemplates.Any();
    }
}
