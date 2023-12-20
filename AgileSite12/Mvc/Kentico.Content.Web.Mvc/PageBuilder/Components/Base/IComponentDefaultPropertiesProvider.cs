using System;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Provides interface for providing component default properties.
    /// </summary>
    internal interface IComponentDefaultPropertiesProvider
    {
        /// <summary>
        /// Gets component default properties.
        /// </summary>
        /// <param name="propertiesType">Type of the component properties.</param>
        /// <typeparam name="TPropertiesInterface">Type of the component properties interface.</typeparam>
        TPropertiesInterface Get<TPropertiesInterface>(Type propertiesType)
            where TPropertiesInterface : IComponentProperties;


        /// <summary>
        /// Gets component default properties.
        /// </summary>
        /// <typeparam name="TPropertiesType">Type of the component properties.</typeparam>
        TPropertiesType Get<TPropertiesType>()
            where TPropertiesType : class, IComponentProperties, new();
    }
}