using System;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Encapsulates extension methods for <see cref="FormComponentConfiguration"/>.
    /// </summary>
    internal static class FormComponentConfigurationExtensions
    {
        /// <summary>
        /// Determines whether <see cref="FormComponentConfiguration"/> is of type <see cref="FormComponentIdentifiers.INVALID_COMPONENT_IDENTIFIER"/>.
        /// </summary>
        /// <param name="formComponentConfiguration">Instance of <see cref="FormComponentConfiguration"/> which to test for type <see cref="FormComponentIdentifiers.INVALID_COMPONENT_IDENTIFIER"/>.</param>
        /// <returns>Returns true if <see cref="FormComponentConfiguration"/> is of type <see cref="FormComponentIdentifiers.INVALID_COMPONENT_IDENTIFIER"/>.</returns>
        public static bool IsInvalidComponent(this FormComponentConfiguration formComponentConfiguration)
        {
            return formComponentConfiguration.TypeIdentifier == FormComponentIdentifiers.INVALID_COMPONENT_IDENTIFIER;
        }


        /// <summary>
        /// Returns <see cref="Exception"/> contained in <see cref="FormComponentConfiguration"/> of type <see cref="FormComponentIdentifiers.INVALID_COMPONENT_IDENTIFIER"/>.
        /// </summary>
        /// <param name="formComponentConfiguration">Instance of <see cref="FormComponentConfiguration"/> of type <see cref="FormComponentIdentifiers.INVALID_COMPONENT_IDENTIFIER"/>, from which the <see cref="Exception"/> is extracted.</param>
        /// <returns>Returns <see cref="Exception"/> or null.</returns>
        public static Exception GetException(this FormComponentConfiguration formComponentConfiguration)
        {
            var properties = formComponentConfiguration.Properties as InvalidComponentProperties;
            if (properties != null)
            {
                return properties.Exception;
            }

            return null;
        }
    }
}
