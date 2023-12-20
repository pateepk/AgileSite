using System;
using System.Collections.Generic;
using System.Linq;

using Kentico.Forms.Web.Mvc.FormComponents;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Class provides extension methods for enumerables of form component definitions.
    /// </summary>
    public static class FormComponentDefinitionEnumerableExtensions
    {
        /// <summary>
        /// Applies form component filters on a collection of <paramref name="formComponents"/> and returns a filtered collection.
        /// </summary>
        /// <returns>Filtered form component collection.</returns>
        public static IEnumerable<FormComponentDefinition> Filter(this IEnumerable<FormComponentDefinition> formComponents, IEnumerable<IFormComponentFilter> formComponentFilters, FormComponentFilterContext context)
        {
            if (formComponents == null)
            {
                throw new ArgumentNullException(nameof(formComponents));
            }

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (formComponentFilters == null)
            {
                return formComponents;
            }

            foreach (var filter in formComponentFilters)
            {
                formComponents = filter.Filter(formComponents, context);
            }

            return formComponents;
        }
    }
}
