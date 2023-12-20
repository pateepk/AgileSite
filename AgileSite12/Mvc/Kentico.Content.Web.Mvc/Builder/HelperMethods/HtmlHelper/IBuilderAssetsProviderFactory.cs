using System.Web.Routing;

using CMS;
using CMS.Core;

using Kentico.Builder.Web.Mvc;

[assembly: RegisterImplementation(typeof(IBuilderAssetsProviderFactory), typeof(BuilderAssetsProviderFactory), Priority = RegistrationPriority.SystemDefault)]

namespace Kentico.Builder.Web.Mvc
{
    /// <summary>
    /// Factory for <see cref="BuilderAssetsProvider"/> implementations.
    /// </summary>
    internal interface IBuilderAssetsProviderFactory
    {
        /// <summary>
        /// Gets an instance of <typeparamref name="TBuilderAssetsProvider"/> for given <paramref name="requestContext"/>.
        /// </summary>
        /// <typeparam name="TBuilderAssetsProvider">Builder assets provider type to instantiate.</typeparam>
        /// <typeparam name="TBuilderAssetsProviderInterface">Builder assets provider interface to return.</typeparam>
        /// <param name="requestContext">Request context for the assets provider.</param>
        /// <returns>Returns an instance of the asset provider.</returns>
        TBuilderAssetsProviderInterface Get<TBuilderAssetsProvider, TBuilderAssetsProviderInterface>(RequestContext requestContext) 
            where TBuilderAssetsProvider : BuilderAssetsProvider
            where TBuilderAssetsProviderInterface : class, IBuilderAssetsProvider;
    }
}
