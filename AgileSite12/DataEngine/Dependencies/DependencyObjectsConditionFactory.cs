using System;

using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Factory used to register custom <see cref="WhereCondition"/> provider for object types with specified path column.
    /// </summary>
    /// <seealso cref="ObjectTypeInfo.ObjectPathColumn"/>
    /// <seealso cref="ObjectDependenciesConditionProvider"/>.
    public static class DependencyObjectsConditionFactory
    {
        private static readonly SafeDictionary<string, ObjectDependenciesConditionProvider> mProviders = new SafeDictionary<string, ObjectDependenciesConditionProvider>(StringComparer.OrdinalIgnoreCase);


        /// <summary>
        /// Registers given <paramref name="provider"/> for given <paramref name="objectType"/>.
        /// </summary>
        public static void RegisterConditionProvider(string objectType, ObjectDependenciesConditionProvider provider)
        {
            mProviders[objectType] = provider ?? throw new ArgumentNullException(nameof(provider));
        }


        /// <summary>
        /// Returns registered provider for given <paramref name="objectType"/> or null.
        /// </summary>
        public static ObjectDependenciesConditionProvider GetConditionProvider(string objectType)
        {
            return mProviders[objectType];
        }
    }
}
