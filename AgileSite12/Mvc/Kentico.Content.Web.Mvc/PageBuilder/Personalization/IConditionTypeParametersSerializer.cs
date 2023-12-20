using System;

namespace Kentico.PageBuilder.Web.Mvc.Personalization
{
    /// <summary>
    /// Interface for condition type parameters serializer.
    /// </summary>
    internal interface IConditionTypeParametersSerializer
    {
        /// <summary>
        /// Deserializes the JSON string to condition type parameters object representation.
        /// </summary>
        /// <param name="json">Condition type parameters JSON string.</param>
        /// <param name="conditionType">Type of the registered condition type.</param>
        /// <exception cref="ArgumentException"><paramref name="json"/> is <c>null</c> or empty.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="conditionType"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException"><paramref name="json" /> is in incorrect format.</exception>
        IConditionType Deserialize(string json, Type conditionType);


        /// <summary>
        /// Deserializes the JSON string to condition type parameters object representation.
        /// </summary>
        /// <param name="json">Condition type parameters JSON string.</param>
        /// <returns>Deserialized condition type object. If <paramref name="json"/> is empty, returns an object of type <typeparamref name="TConditionType"/> with default properties values.</returns>
        /// <exception cref="InvalidOperationException"><paramref name="json" /> is in incorrect format.</exception>
        TConditionType Deserialize<TConditionType>(string json)
            where TConditionType : class, IConditionType, new();
    }
}
