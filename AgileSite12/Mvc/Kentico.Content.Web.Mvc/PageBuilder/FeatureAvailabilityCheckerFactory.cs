using Kentico.Content.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc.Personalization;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Factory that creates feature availability checker based on given <see cref="ComponentDefinition"/> type.
    /// </summary>
    internal sealed class FeatureAvailabilityCheckerFactory : IFeatureAvailabilityCheckerFactory
    {
        /// <summary>
        /// Returns feature availability checker based on given condition definition type.
        /// </summary>
        public IFeatureAvailabilityChecker GetAvailabilityChecker<TDefinition>()
            where TDefinition : ComponentDefinitionBase
        {
            if (typeof(TDefinition) == typeof(ConditionTypeDefinition))
            {
                return new PersonalizationAvailabilityChecker();
            }

            return null;
        }
    }
}
