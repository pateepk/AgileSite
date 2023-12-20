using System.Collections.Generic;

using Kentico.Content.Web.Mvc;

namespace Kentico.Forms.Web.Mvc.FormComponents
{
    /// <summary>
    /// Defines methods that are used for filtering form components.
    /// </summary>
    public interface IFormComponentFilter : IComponentFilter
    {
        /// <summary>
        /// Applies filtering on the given <paramref name="formComponents" /> collection.
        /// </summary>
        /// <param name="formComponents">Collection of form components to filter.</param>
        /// <param name="context">Context in which to filter the form components.</param>
        /// <returns>Filtered form components.</returns>
        IEnumerable<FormComponentDefinition> Filter(IEnumerable<FormComponentDefinition> formComponents, FormComponentFilterContext context);
    }
}
