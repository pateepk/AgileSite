using System;
using System.Web;

using Kentico.Content.Web.Mvc;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Retrieves various URLs for Page builder component.
    /// </summary>
    /// <typeparam name="TDefinition">Type of the component definition.</typeparam>
    internal abstract class AbstractComponentUrlRetriever<TDefinition> : IComponentUrlRetriever<TDefinition>
        where TDefinition : IComponentDefinition
    {
        protected readonly HttpContextBase context;
        protected readonly IPathDecorator decorator;


        /// <summary>
        /// Creates an instance of <see cref="AbstractComponentUrlRetriever{TDefinitionInterface}"/> class.
        /// </summary>
        /// <param name="context">HTTP context in which the URL is retrieved.</param>
        /// <param name="decorator">Decorates markup URL with additional information.</param>
        /// <exception cref="ArgumentNullException">Throws when <paramref name="context"/> is null.</exception>
        protected AbstractComponentUrlRetriever(HttpContextBase context, IPathDecorator decorator)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
            this.decorator = decorator;
        }


        /// <summary>
        /// Gets the URL for specific component decorated with <see cref="IPathDecorator" /> if provided.
        /// </summary>
        /// <param name="component">Component with markup definition.</param>
        /// <returns>URL for specific component.</returns>
        /// <exception cref="InvalidOperationException">Throws when component controller is not correctly registered.</exception>
        public string GetUrl(TDefinition component)
        {
            if (component == null)
            {
                throw new ArgumentNullException(nameof(component));
            }

            var url = GenerateUrl(component);
            if (string.IsNullOrEmpty(url))
            {
                throw new InvalidOperationException("Component URL cannot be retrieved. Make sure component controller is correctly registered.");
            }

            return decorator == null ? url : decorator.Decorate(url);
        }


        /// <summary>
        /// Generates URL for specific component.
        /// </summary>
        /// <param name="component">Component with markup definition.</param>
        /// <returns>URL for specific component.</returns>
        protected abstract string GenerateUrl(TDefinition component);
    }
}
