using System;
using System.Collections.Generic;

using CMS.Base;
using CMS.DataEngine;

namespace CMS.ContinuousIntegration.Internal
{
    /// <summary>
    /// Factory providing enumerator of objects in database covered by continuous integration.
    /// </summary>
    public sealed class DatabaseObjectsEnumeratorFactory
    {
        #region "Delegates"

        /// <summary>
        /// Factory method creating a new object enumerator for given object type.
        /// </summary>
        /// <param name="objectType">Object type for which the enumerator is to be created.</param>
        /// <param name="where">Where condition</param>
        /// <returns>New instance of object enumerator.</returns>
        public delegate IEnumerable<BaseInfo> CreateEnumerator(string objectType, IWhereCondition where);

        #endregion


        #region "Fields"

        // Factory methods organized by object type
        private readonly SafeDictionary<string, CreateEnumerator> mFactoryMethods = new SafeDictionary<string, CreateEnumerator>(StringComparer.InvariantCultureIgnoreCase);
        private readonly CreateEnumerator mDefaultFactoryMethod;

        /// <summary>
        /// The only instance of the class.
        /// </summary>
        private static readonly DatabaseObjectsEnumeratorFactory instance = new DatabaseObjectsEnumeratorFactory();

        #endregion


        #region "Static methods"

        /// <summary>
        /// Registers a new object enumerator for given object type by providing a factory method for the enumerator.
        /// </summary>
        /// <param name="objectType">Object type for which the enumerator is to be used.</param>
        /// <param name="factoryMethod">Method which creates a new instance of the object enumerator.</param>
        public static void RegisterObjectEnumerator(string objectType, CreateEnumerator factoryMethod)
        {
            instance.RegisterObjectRegisterEnumeratorInternal(objectType, factoryMethod);
        }


        /// <summary>
        /// Gets a new instance of object enumerator suitable for <paramref name="objectType"/>. The instance is determined as follows.
        /// If specific object enumerator is available for object type, it is used.
        /// Otherwise the default object enumerator is used.
        /// </summary>
        /// <param name="objectType">Object type for which to instantiate an enumerator.</param>
        /// <param name="repositoryConfiguration">Configuration of file system repository.</param>
        /// <param name="where">Where condition</param>
        /// <returns>An instance of <see cref="IEnumerable{BaseInfo}"/>.</returns>
        internal static IEnumerable<BaseInfo> GetObjectEnumerator(string objectType, FileSystemRepositoryConfiguration repositoryConfiguration = null, IWhereCondition where = null)
        {
            return instance.GetObjectEnumeratorInternal(objectType, repositoryConfiguration, where);
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Registers a new object enumerator for given object type by providing a factory method for the enumerator.
        /// </summary>
        /// <param name="objectType">Object type for which the enumerator is to be used.</param>
        /// <param name="factoryMethod">Method which creates a new instance of the object enumerator.</param>
        private void RegisterObjectRegisterEnumeratorInternal(string objectType, CreateEnumerator factoryMethod)
        {
            if (objectType == null)
            {
                throw new ArgumentNullException("objectType", "Object type can not be null.");
            }
            if (factoryMethod == null)
            {
                throw new ArgumentNullException("factoryMethod", "Factory method can not be null.");
            }

            mFactoryMethods[objectType] = factoryMethod;
        }


        /// <summary>
        /// Gets a new instance of object enumerator suitable for <paramref name="objectType"/>. The instance is determined as follows.
        /// If specific object enumerator is available for object type, it is used.
        /// Otherwise the default object enumerator is used.
        /// </summary>
        /// <param name="objectType">Object type for which to instantiate an enumerator.</param>
        /// <param name="repositoryConfiguration">Configuration of file system repository.</param>
        /// <param name="where">Where condition</param>
        /// <returns>An instance of <see cref="IEnumerable{BaseInfo}"/>.</returns>
        private IEnumerable<BaseInfo> GetObjectEnumeratorInternal(string objectType, FileSystemRepositoryConfiguration repositoryConfiguration, IWhereCondition where)
        {
            var repositoryObjectType = RepositoryConfigurationEvaluator.GetRepositoryObjectType(objectType);

            // Get the full where condition for the given object type
            where = GetFullWhereCondition(repositoryObjectType, repositoryConfiguration, where);

            if (mFactoryMethods.ContainsKey(repositoryObjectType))
            {
                return mFactoryMethods[repositoryObjectType].Invoke(repositoryObjectType, where);
            }

            return mDefaultFactoryMethod.Invoke(repositoryObjectType, where);
        }


        /// <summary>
        /// Gets the full where condition. Combines the given where condition with the default where condition provided by filters.
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="repositoryConfiguration">Configuration of file system repository.</param>
        /// <param name="where">Where condition</param>
        private IWhereCondition GetFullWhereCondition(string objectType, FileSystemRepositoryConfiguration repositoryConfiguration, IWhereCondition where)
        {
            var typeWhere = GetDefaultTypeWhereCondition(objectType, repositoryConfiguration);
            if (typeWhere != null)
            {
                where = typeWhere.Where(where);
            }

            return where;
        }


        /// <summary>
        /// Gets a default where condition 
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="repositoryConfiguration">Configuration of file system repository.</param>
        private WhereCondition GetDefaultTypeWhereCondition(string objectType, FileSystemRepositoryConfiguration repositoryConfiguration)
        {
            var filterCondition = repositoryConfiguration == null ? GetDefaultFilterCondition(objectType) : RepositoryConfigurationEvaluator.GetObjectTypeFilterCondition(objectType, repositoryConfiguration);

            return filterCondition == null ? null : new WhereCondition(filterCondition);
        }


        /// <summary>
        /// Gets default filter condition for given object type from its type info
        /// </summary>
        /// <param name="objectType">Object type</param>
        private IWhereCondition GetDefaultFilterCondition(string objectType)
        {
            var typeInfo = ObjectTypeManager.GetTypeInfo(objectType);
            if ((typeInfo != null) && (typeInfo.ContinuousIntegrationSettings.FilterCondition != null))
            {
                return typeInfo.ContinuousIntegrationSettings.FilterCondition;
            }

            return null;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Initializes a new object enumerator factory with default factory method.
        /// </summary>
        private DatabaseObjectsEnumeratorFactory()
        {
            mDefaultFactoryMethod = (objectType, where) => new ObjectQuery(objectType).Where(where);
        }

        #endregion
    }
}
