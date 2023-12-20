using System;

using Kentico.Content.Web.Mvc;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Provides interface for retrieving various URLs for Page builder components.
    /// </summary>
    /// <typeparam name="TDefinition">Type of the component definition.</typeparam>
    internal interface IComponentUrlRetriever<in TDefinition>
        where TDefinition : IComponentDefinition
    {
        /// <summary>
        /// Gets the URL for specific component decorated with <see cref="IPathDecorator" /> if provided.
        /// </summary>
        /// <param name="component">Component with markup definition.</param>
        /// <returns>URL for specific component.</returns>
        /// <exception cref="InvalidOperationException">Throws when component controller is not correctly registered.</exception>
        string GetUrl(TDefinition component);
    }
}