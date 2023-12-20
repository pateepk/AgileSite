using System;

using Kentico.Forms.Web.Mvc;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Interface for annotated object properties serializer.
    /// </summary>
    internal interface IAnnotatedPropertiesSerializer
    {
        /// <summary>
        /// Serializes object properties that are decorated with the <see cref="EditingComponentAttribute"/> attribute to a JSON string.
        /// Properties that are not decorated are not included in the serialized JSON.
        /// </summary>
        /// <param name="obj">Object to be serialized.</param>
        /// <returns>JSON string representing annotated object properties.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="obj" /> is <c>null</c>.</exception>
        string Serialize(object obj);
    }
}
