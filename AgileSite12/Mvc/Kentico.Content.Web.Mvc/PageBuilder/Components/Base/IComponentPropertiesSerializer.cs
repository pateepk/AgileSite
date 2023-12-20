using System;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Interface for component properties serializer.
    /// </summary>
    internal interface IComponentPropertiesSerializer 
    {
        /// <summary>
        /// Deserializes the JSON string to component properties object representation.
        /// </summary>
        /// <param name="json">Component properties JSON string.</param>
        /// <param name="propertiesType">Type of the registered component properties model.</param>
        /// <exception cref="ArgumentException"><paramref name="json"/> is <c>null</c> or empty.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="propertiesType"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException"><paramref name="json" /> is in incorrect format.</exception>
        IComponentProperties Deserialize(string json, Type propertiesType);


        /// <summary>
        /// Deserializes JSON string to properties defined by the <typeparamref name="TPropertiesType"/> type.
        /// </summary>
        /// <typeparam name="TPropertiesType">Type of the component properties.</typeparam>
        /// <param name="json">JSON string.</param>
        /// <returns>Deserialized properties object. If <paramref name="json"/> is empty, returns an object of type <typeparamref name="TPropertiesType"/> with default properties values.</returns>
        /// <exception cref="InvalidOperationException"><paramref name="json"/> is in incorrect format.</exception>
        TPropertiesType Deserialize<TPropertiesType>(string json)
            where TPropertiesType : class, IComponentProperties, new();
    }
}
