using System;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Extension method for <see cref="Type"/> class.
    /// </summary>
    internal static class TypeExtensions
    {
        /// <summary>
        /// Walks type hierarchy of a type towards base classes and looks for <paramref name="genericTypeDefinition"/>.
        /// Returns the type from <paramref name="type"/>'s class hierarchy whose generic type definition matches <paramref name="genericTypeDefinition"/>.
        /// </summary>
        /// <param name="type">Type whose class hierarchy to examine.</param>
        /// <param name="genericTypeDefinition">Generic type definition to look for.</param>
        /// <returns>Returns type whose generic type definition matches <paramref name="genericTypeDefinition"/> by examining inheritance hierarchy of <paramref name="type"/>. Returns null if no match is found.</returns>
        /// <seealso cref="Type.GetGenericTypeDefinition"/>
        /// <seealso cref="Type.IsGenericTypeDefinition"/>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> or <paramref name="genericTypeDefinition"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when type represented by <paramref name="genericTypeDefinition"/> is not a generic type definition.</exception>
        public static Type FindTypeByGenericDefinition(this Type type, Type genericTypeDefinition)
        {
            if (genericTypeDefinition == null)
            {
                throw new ArgumentNullException(nameof(genericTypeDefinition));
            }

            if (!genericTypeDefinition.IsGenericTypeDefinition)
            {
                throw new ArgumentException($"Given type '{genericTypeDefinition.FullName ?? genericTypeDefinition.ToString()}' is not a generic type definition. Use {typeof(Type).FullName}.{nameof(Type.GetGenericTypeDefinition)} to obtain generic type definition of a generic type.");
            }

            var examinedType = type ?? throw new ArgumentNullException(nameof(type));

            do
            {
                if (examinedType.IsGenericType && examinedType.GetGenericTypeDefinition() == genericTypeDefinition)
                {
                    return examinedType;
                }
                examinedType = examinedType.BaseType;
            } while (examinedType != null);

            return null;
        }
        
        
        /// <summary>
        /// Gets default value of given <paramref name="type"/>.
        /// </summary>
        /// <param name="type">Type to get default value for.</param>
        /// <returns>Default value.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> is null.</exception>
        public static object GetDefaultValue(this Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            else
            {
                return null;
            }
        }
    }
}
