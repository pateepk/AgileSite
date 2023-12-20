using System;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Provides an interface for retrieving component properties.
    /// </summary>
    /// <typeparam name="TPropertiesType">Type of the section properties.</typeparam>
    public interface IComponentPropertiesRetriever<out TPropertiesType>
    where TPropertiesType : class, IComponentProperties, new()
    {
        /// <summary>
        /// Retrieves component properties.
        /// </summary>
        TPropertiesType Retrieve();
    }


    /// <summary>
    /// Provides an interface for retrieving component properties.
    /// </summary>
    internal interface IComponentPropertiesRetriever
    {
        /// <summary>
        /// Retrieves component properties based on generic type.
        /// </summary>
        /// <typeparam name="TPropertiesType">Type of the component properties.</typeparam>
        TPropertiesType Retrieve<TPropertiesType>()
            where TPropertiesType : class, IComponentProperties, new();



        /// <summary>
        /// Retrieves component properties based on given type.
        /// </summary>
        IComponentProperties Retrieve(Type propertiesType);
    }
}