using System;
using System.Collections.Generic;
using System.Linq;

using CMS.DataEngine;

namespace CMS.ContinuousIntegration.Internal
{
    /// <summary>
    /// Class provides fluent extension for <see cref="FileSystemRepositoryConfigurationBuilder"/> and is meant for this single purpose.
    /// </summary>
    internal static class FileSystemRepositoryConfigurationBuilderExtensions
    {
        #region "Private methods"

        /// <summary>
        /// Selects only such <see cref="ObjectTypeInfo"/>s that DO support continuous integration.
        /// </summary>
        private static IEnumerable<ObjectTypeInfo> SelectTypesWithContinuousIntegrationSupport(this IEnumerable<ObjectTypeInfo> typeInfos)
        {
            return typeInfos.Where(typeInfo => typeInfo != null && typeInfo.SupportsContinuousIntegration());
        }


        /// <summary>
        /// Selects only such <see cref="ObjectTypeInfo"/>s that DO NOT support continuous integration.
        /// </summary>
        private static IEnumerable<ObjectTypeInfo> SelectTypesWithoutContinuousIntegrationSupport(this IEnumerable<ObjectTypeInfo> typeInfos)
        {
            return typeInfos.Where(typeInfo => typeInfo != null && !typeInfo.SupportsContinuousIntegration());
        }


        /// <summary>
        /// Converts given collection of <paramref name="objectTypes"/> to a collection of their <see cref="ObjectTypeInfo"/>.
        /// </summary>
        /// <param name="objectTypes">Collection of names of object types.</param>
        /// <param name="throwExceptionIfNotFound">If <see langword="true"/> and object type could not be retrieved for given object type, exception is thrown, <see langword="null"/> is yielded otherwise.</param>
        private static IEnumerable<ObjectTypeInfo> SelectTypeInfos(this IEnumerable<string> objectTypes, bool throwExceptionIfNotFound = false)
        {
            return objectTypes.Select(objectType => ObjectTypeManager.GetTypeInfo(objectType, throwExceptionIfNotFound));
        }


        /// <summary>
        /// Calls <see cref="SelectManyObjectTypesAndSubtypes"/> on all <paramref name="typeInfo"/>'s subtypes (selected by <paramref name="subTypesSelector"/>).
        /// </summary>
        /// <param name="typeInfo"><see cref="ObjectTypeInfo"/> to process sub types of.</param>
        /// <param name="subTypesSelector">Declares what object subtypes of <paramref name="typeInfo"/> should be processed (i.e. children or bindings or other bindings).</param>
        /// <remarks>Object type of <paramref name="typeInfo"/> is excluded from object types selected by <paramref name="subTypesSelector"/>.</remarks>
        private static IEnumerable<string> SelectManyDistinctSubTypes(this ObjectTypeInfo typeInfo, Func<ObjectTypeInfo, IEnumerable<string>> subTypesSelector)
        {
            return subTypesSelector(typeInfo)
                .Except(new[]
                    {
                        typeInfo.ObjectType
                    },
                    StringComparer.InvariantCultureIgnoreCase)
                .SelectTypeInfos()
                .SelectTypesWithContinuousIntegrationSupport()
                .SelectManyObjectTypesAndSubtypes();
        }

        /// <summary>
        /// Returns all <paramref name="typeInfos"/> provided and all <see cref="ObjectTypeInfo"/>s they wrap.
        /// </summary>
        /// <param name="typeInfos">Object types to process</param>
        private static IEnumerable<ObjectTypeInfo> SelectManyBindingObjectTypeInfos(this IEnumerable<ObjectTypeInfo> typeInfos)
        {
            foreach (var typeInfo in typeInfos)
            {
                yield return typeInfo;

                if (!typeInfo.IsComposite)
                {
                    // Not a wrapper object, no wrapped type infos to yield
                    continue;
                }

                foreach (var wrappedTypeInfo in typeInfo.ConsistsOf.SelectTypeInfos())
                {
                    yield return wrappedTypeInfo;
                }
            }
        }


        /// <summary>
        /// Validates whether all <paramref name="objectTypes"/> are among supported.
        /// </summary>
        /// <param name="objectTypes">Object types to be supported.</param>
        /// <exception cref="RepositoryConfigurationException">Thrown when one or more object types are not supported.</exception>
        private static ObjectTypeInfo[] ValidateContinuousIntegrationSupport(this IEnumerable<ObjectTypeInfo> objectTypes)
        {
            var enumeratedTypeInfos = objectTypes.ToArray();

            var unsupportedTypeNames = enumeratedTypeInfos
                .SelectTypesWithoutContinuousIntegrationSupport()
                .SelectObjectType()
                .ToArray();

            if (unsupportedTypeNames.Any())
            {
                throw new RepositoryConfigurationException(String.Format("One or more object types are not supported by the repository: {0}", String.Join(", ", unsupportedTypeNames)));
            }

            return enumeratedTypeInfos;
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Selects object type names of give <paramref name="typeInfos"/>.
        /// </summary>
        public static IEnumerable<string> SelectObjectType(this IEnumerable<ObjectTypeInfo> typeInfos)
        {
            return typeInfos.Select(typeInfo => typeInfo.ObjectType);
        }


        /// <summary>
        /// Converts given collection of <paramref name="objectTypes"/> to a collection of their <see cref="ObjectTypeInfo"/>,
        /// validates whether all <paramref name="objectTypes"/> are among supported in continuous integration.
        /// </summary>
        /// <param name="objectTypes">Collection of names of object types.</param>
        /// <exception cref="RepositoryConfigurationException">Thrown when one or more object types are not supported.</exception>
        public static IEnumerable<ObjectTypeInfo> ToContinuousIntegrationSupportingTypeInfos(this IEnumerable<string> objectTypes)
        {
            return objectTypes.ValidateContinuousIntegrationSupport();
        }


        /// <summary>
        /// Returns object types of all provided <paramref name="typeInfos"/>, infos they wraps and all their children recursively. Only types that support continuous integration.
        /// </summary>
        /// <param name="typeInfos">Collection of <see cref="ObjectTypeInfo"/>s to start depth-first search algorithm for related types supporting continuous integration on.</param>
        public static IEnumerable<string> SelectManyObjectTypesAndSubtypes(this IEnumerable<ObjectTypeInfo> typeInfos)
        {
            foreach (var typeInfo in typeInfos.SelectManyBindingObjectTypeInfos())
            {
                if (typeInfo.SupportsContinuousIntegration())
                {
                    yield return typeInfo.ObjectType;
                }

                var children = typeInfo.SelectManyDistinctSubTypes(info => info.ChildObjectTypes);
                var bindings = typeInfo.SelectManyDistinctSubTypes(info => info.BindingObjectTypes);
                var otherBindings = typeInfo.SelectManyDistinctSubTypes(info => info.OtherBindingObjectTypes);

                foreach (var child in children)
                {
                    yield return child;
                }

                foreach (var binding in bindings)
                {
                    yield return binding;
                }

                foreach (var otherBinding in otherBindings)
                {
                    yield return otherBinding;
                }
            }
        }


        /// <summary>
        /// Selects main object types of given <paramref name="objectTypes"/> collection.
        /// </summary>
        /// <param name="objectTypes">Base set of object types.</param>
        public static IEnumerable<string> SelectMainObjectTypes(this IEnumerable<string> objectTypes)
        {
            if (objectTypes == null)
            {
                yield break;
            }

            foreach (var typeInfo in objectTypes.SelectTypeInfos())
            {
                var mainTypeInfoCandidate = typeInfo;
                while (!mainTypeInfoCandidate.IsMainObject)
                {
                    mainTypeInfoCandidate = ObjectTypeManager.GetTypeInfo(mainTypeInfoCandidate.ParentObjectType);
                }

                yield return mainTypeInfoCandidate.ObjectType;
            }
        }


        /// <summary>
        /// Validates whether all <paramref name="objectTypes"/> are among supported.
        /// </summary>
        /// <param name="objectTypes">Object types to be supported.</param>
        /// <exception cref="RepositoryConfigurationException">Thrown when one or more object types are not supported.</exception>
        public static ObjectTypeInfo[] ValidateContinuousIntegrationSupport(this IEnumerable<string> objectTypes)
        {
            IEnumerable<ObjectTypeInfo> typeInfos;
            try
            {
                typeInfos = objectTypes
                    .SelectTypeInfos(throwExceptionIfNotFound: true)
                    .ToArray();
            }
            catch (Exception ex)
            {
                throw new RepositoryConfigurationException("Repository configuration contains invalid object type.", ex);
            }

            return typeInfos.ValidateContinuousIntegrationSupport();
        }

        #endregion
    }
}
