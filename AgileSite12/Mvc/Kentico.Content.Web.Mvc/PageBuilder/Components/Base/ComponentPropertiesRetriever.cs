using System;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Retrieves properties for a component controller from request body or route data.
    /// </summary>
    internal class ComponentPropertiesRetriever : IComponentPropertiesRetriever
    {
        private readonly IPageBuilderRouteDataRetriever routeDataRetriever;
        private readonly IPageBuilderPostDataRetriever<PageBuilderPostData> postDataRetriever;
        private readonly IComponentPropertiesSerializer serializer;
        private readonly bool isInEditMode;


        /// <summary>
        /// Creates an instance of <see cref="ComponentPropertiesRetriever"/> class.
        /// </summary>
        /// <param name="routeDataRetriever">Retriever for data from route data.</param>
        /// <param name="postDataRetriever">Retriever for data from POST data.</param>
        /// <param name="serializer">Serializer of the component properties.</param>
        /// <param name="isInEditMode">Indicates whether edit mode is enabled.</param>
        /// <exception cref="ArgumentNullException">Is thrown when <paramref name="routeDataRetriever"/>, <paramref name="postDataRetriever"/> or <paramref name="serializer"/> is <c>null</c>.</exception>
        public ComponentPropertiesRetriever(IPageBuilderRouteDataRetriever routeDataRetriever, IPageBuilderPostDataRetriever<PageBuilderPostData> postDataRetriever, IComponentPropertiesSerializer serializer, bool isInEditMode)
        {
            this.routeDataRetriever = routeDataRetriever ?? throw new ArgumentNullException(nameof(routeDataRetriever));
            this.postDataRetriever = postDataRetriever ?? throw new ArgumentNullException(nameof(postDataRetriever));
            this.serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            this.isInEditMode = isInEditMode;
        }


        /// <summary>
        /// Retrieves component properties.
        /// </summary>
        /// <param name="propertiesType">Type of the component properties.</param>
        public IComponentProperties Retrieve(Type propertiesType)
        {
            if (propertiesType == null)
            {
                throw new ArgumentNullException(nameof(propertiesType));
            }

            var properties = GetPropertiesFromPostData(propertiesType) ?? GetPropertiesFromRouteData<IComponentProperties>();

            return properties ?? throw new InvalidOperationException($"Unable to retrieve the component properties of type {propertiesType.FullName}.");
        }


        /// <summary>
        /// Retrieves component properties.
        /// </summary>
        /// <typeparam name="TPropertiesType">Type of the component properties.</typeparam>
        public TPropertiesType Retrieve<TPropertiesType>()
            where TPropertiesType : class, IComponentProperties, new()
        {
            var properties = GetPropertiesFromPostData<TPropertiesType>() ?? GetPropertiesFromRouteData<TPropertiesType>();

            return properties ?? throw new InvalidOperationException($"Unable to retrieve the component properties of type {typeof(TPropertiesType).FullName}.");
        }


        private IComponentProperties GetPropertiesFromPostData(Type propertiesType)
        {
            var data = isInEditMode ? postDataRetriever.Retrieve() : null;
            if (data == null)
            {
                return null;
            }

            return serializer.Deserialize(data.Data, propertiesType);
        }


        private TPropertiesType GetPropertiesFromPostData<TPropertiesType>()
            where TPropertiesType : class, IComponentProperties, new()
        {
            var data = isInEditMode ? postDataRetriever.Retrieve() : null;
            if (data == null)
            {
                return null;
            }

            return serializer.Deserialize<TPropertiesType>(data.Data);
        }


        private TPropertiesType GetPropertiesFromRouteData<TPropertiesType>()
            where TPropertiesType : IComponentProperties
        {
            return routeDataRetriever.Retrieve<TPropertiesType>();
        }
    }
}
