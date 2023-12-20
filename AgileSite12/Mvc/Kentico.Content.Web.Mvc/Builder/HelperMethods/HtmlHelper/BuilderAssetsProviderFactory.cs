using System;
using System.Web.Routing;

namespace Kentico.Builder.Web.Mvc
{
    /// <summary>
    /// A simple factory for <see cref="BuilderAssetsProvider"/> implementations. Creates a new provider
    /// by calling its constructor while passing supplied request context as its only parameter.
    /// </summary>
    internal class BuilderAssetsProviderFactory : IBuilderAssetsProviderFactory
    {
        /// <summary>
        /// Gets an instance of <typeparamref name="TBuilderAssetsProvider"/> for given <paramref name="requestContext"/>.
        /// </summary>
        /// <typeparam name="TBuilderAssetsProvider">Builder assets provider type to instantiate.</typeparam>
        /// <typeparam name="TBuilderAssetsProviderInterface">Builder assets provider interface to return.</typeparam>
        /// <param name="requestContext">Request context for the assets provider.</param>
        /// <returns>Returns an instance of the asset provider.</returns>
        public TBuilderAssetsProviderInterface Get<TBuilderAssetsProvider, TBuilderAssetsProviderInterface>(RequestContext requestContext) 
            where TBuilderAssetsProvider : BuilderAssetsProvider
            where TBuilderAssetsProviderInterface : class, IBuilderAssetsProvider
        {
            var provider = Activator.CreateInstance(typeof(TBuilderAssetsProvider), requestContext);

            return (TBuilderAssetsProviderInterface)provider;
        }
    }
}
