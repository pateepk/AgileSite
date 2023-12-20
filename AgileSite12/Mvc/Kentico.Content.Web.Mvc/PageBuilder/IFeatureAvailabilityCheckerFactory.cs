using Kentico.Content.Web.Mvc;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Provides interface for a factory that creates feature availability checker based on given <see cref="ComponentDefinition"/> type.
    /// </summary>
    internal interface IFeatureAvailabilityCheckerFactory
    {
        /// <summary>
        /// Returns feature availability checker based on given condition definition type.
        /// </summary>
        IFeatureAvailabilityChecker GetAvailabilityChecker<TDefinition>()
            where TDefinition : ComponentDefinitionBase;

    }
}