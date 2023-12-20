using Kentico.Content.Web.Mvc;
using Kentico.Forms.Web.Mvc.FormComponents;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Represents the form component filter collections.
    /// </summary>
    public static class FormBuilderFilters
    {
        /// <summary>
        /// Gets the collection of form component filters.
        /// </summary>
        public static ComponentFilterCollection<IFormComponentFilter> FormComponents { get; }


        /// <summary>
        /// Initializes a new instance of the <see cref="FormBuilderFilters"/> class.
        /// </summary>
        static FormBuilderFilters()
        {
            FormComponents = new ComponentFilterCollection<IFormComponentFilter>();
        }
    }
}
