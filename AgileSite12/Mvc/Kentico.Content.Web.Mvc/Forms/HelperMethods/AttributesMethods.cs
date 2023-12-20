using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Kentico.Forms.Web.Mvc.AnnotationExtensions
{
    /// <summary>
    /// Encapsulates extension methods regarding <see cref="Attribute"/>s retrieval from class instances.
    /// </summary>
    internal static class AttributesMethodsExtensions
    {
        /// <summary>
        /// Returns collection of <see cref="PropertyInfo"/>s annotated with <see cref="Attribute"/> of type <typeparamref name="T"/>.
        /// Optionally inspects ancestors of the <see cref="PropertyInfo"/>s.
        /// </summary>
        /// <typeparam name="T">Attribute type.</typeparam>
        /// <param name="model">Instance from which to retrieve <see cref="PropertyInfo"/>s annotated with <see cref="Attribute"/>s of type <typeparamref name="T"/>.</param>
        /// <param name="inherit">True to inspect ancestors of the properties; otherwise false.</param>
        /// <returns>Returns collection of <see cref="PropertyInfo"/>s annotated with <see cref="Attribute"/> of type <typeparamref name="T"/>. </returns>
        public static IEnumerable<PropertyInfo> GetAnnotatedProperties<T>(this object model, bool inherit = true) where T : Attribute
        {
            return model.GetType().GetProperties().Where(prop => prop.GetCustomAttribute<T>(inherit) != null);
        }
    }
}
