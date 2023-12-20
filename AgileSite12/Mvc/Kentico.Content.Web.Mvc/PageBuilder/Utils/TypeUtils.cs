using System;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Contains a utility methods for retrieving types from generics.
    /// </summary>
    internal static class TypeUtils
    {
        /// <summary>
        /// Gets type of the generic matching the provided <paramref name="matchingType"/> from given <paramref name="type"/>.
        /// </summary>
        public static Type GetMatchingGenericType(Type type, Type matchingType)
        {
            return GetBaseGenericType(type, matchingType);
        }


        /// <summary>
        /// Indicates if given type is assignable to given generic type.
        /// </summary>
        /// <param name="givenType">Type to check.</param>
        /// <param name="genericType">Generic type.</param>
        public static bool IsAssignableToGenericType(Type givenType, Type genericType)
        {
            var interfaceTypes = givenType.GetInterfaces();

            foreach (var it in interfaceTypes)
            {
                if (it.IsGenericType && it.GetGenericTypeDefinition() == genericType)
                {
                    return true;
                }
            }

            if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
            {
                return true;
            }

            Type baseType = givenType.BaseType;
            if (baseType == null)
            {
                return false;
            }

            return IsAssignableToGenericType(baseType, genericType);
        }


        private static Type GetBaseGenericType(Type type, Type matchingType)
        {
            while (type.BaseType != null)
            {
                type = type.BaseType;
                if (type.IsGenericType && type.GetGenericTypeDefinition() == matchingType)
                {
                    return type.GetGenericArguments()[0];
                }
            }

            return null;
        }
    }
}
