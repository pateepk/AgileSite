using System;
using System.Collections.Generic;
using System.Linq;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Encapsulates extension methods for <see cref="FormBuilderConfiguration"/>.
    /// </summary>
    internal static class FormBuilderConfigurationExtensions
    {
        /// <summary>
        /// Returns collection of <see cref="Exception"/>s collected from <see cref="FormComponentConfiguration"/> elements of type <see cref="FormComponentIdentifiers.INVALID_COMPONENT_IDENTIFIER"/>.
        /// </summary>
        /// <param name="formBuilderConfiguration">Instance from which to retrieve <see cref="Exception"/>s.</param>
        /// <returns>Returns collection of <see cref="Exception"/>s.</returns>
        public static IEnumerable<Exception> GetConfigurationExceptions(this FormBuilderConfiguration formBuilderConfiguration)
        {
            return formBuilderConfiguration.EditableAreas.SelectMany(
                    area => area.Sections.SelectMany(
                        section => section.Zones.SelectMany(
                            zone => zone.FormComponents
                                .Where(component => component.IsInvalidComponent())
                                .Select(component => component.GetException())
                            )
                        )
                   );
        }
    }
}
