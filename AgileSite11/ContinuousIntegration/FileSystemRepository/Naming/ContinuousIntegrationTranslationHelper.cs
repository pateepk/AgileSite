using System;
using System.Collections.Generic;
using System.Linq;

using CMS.DataEngine;


namespace CMS.ContinuousIntegration.Internal
{
    /// <summary>
    /// Custom translation helper which ensures that <see cref="ContinuousIntegrationSettings.ObjectFileNameFields"/> are retrieved
    /// along with other translation data (to improve performance of file system names creation).
    /// </summary>
    public class ContinuousIntegrationTranslationHelper : TranslationHelper, IFilterConditionCache
    {
        /// <summary>
        /// Cache of filter conditions organized by object type and where condition.
        /// </summary>
        private readonly Dictionary<string, bool> filterConditionCache = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);


        internal override bool UseAdditionalFields
        {
            get
            {
                return true;
            }
        }


        /// <summary>
        /// Gets collection of names of additional fields that will be retrieved along with translation record.
        /// </summary>
        internal override ICollection<string> GetAdditionalFieldNames(ObjectTypeInfo typeInfo)
        {
            var additionalFields = new List<string>(typeInfo.ContinuousIntegrationSettings.ObjectFileNameFields);

            if (typeInfo.CategoryObject != null)
            {
                // Add category column to the additional fields
                additionalFields.Add(typeInfo.CategoryIDColumn);
            }

            foreach (var dependency in typeInfo.ContinuousIntegrationSettings.FilterDependencies)
            {
                // Add column names of additional dependencies used for object filtering
                additionalFields.Add(dependency.DependencyColumn);
            }

            return additionalFields;
        }


        /// <summary>
        /// Gets dictionary with data of additional fields [fieldName -> value].
        /// </summary>
        internal override IDictionary<string, object> GetAdditionalFieldsData(BaseInfo infoObject)
        {
            var typeInfo = infoObject.TypeInfo;
            var additionalFields = typeInfo.ContinuousIntegrationSettings.ObjectFileNameFields.ToDictionary(it => it, infoObject.GetValue);

            foreach (var filterDependency in typeInfo.ContinuousIntegrationSettings.FilterDependencies)
            {
                var dependencyColumn = filterDependency.DependencyColumn;
                additionalFields.Add(dependencyColumn, infoObject.GetIntegerValue(dependencyColumn, 0));
            }

            if (typeInfo.CategoryObject != null)
            {
                var category = infoObject.Generalized.ObjectCategory;
                if (category != null)
                {
                    // Add category ID to the additional fields
                    additionalFields.Add(typeInfo.CategoryIDColumn, category.Generalized.ObjectID);
                }
            }

            return additionalFields;
        }


        /// <summary>
        /// Tries to get a value indicating whether object type and its where condition meet filter condition.
        /// </summary>
        bool IFilterConditionCache.TryGet(string filteredObjectType, string whereCondition, out bool meetsFilterCondition)
        {
            var cacheKey = GetFilterConditionCacheKey(filteredObjectType, whereCondition);

            return filterConditionCache.TryGetValue(cacheKey, out meetsFilterCondition);
        }


        /// <summary>
        /// Adds a new cache entry indicating, whether object type and its where condition meet filter condition.
        /// </summary>
        void IFilterConditionCache.Add(string filteredObjectType, string whereCondition, bool meetsFilterCondition)
        {
            var cacheKey = GetFilterConditionCacheKey(filteredObjectType, whereCondition);

            filterConditionCache[cacheKey] = meetsFilterCondition;
        }


        /// <summary>
        /// Clears the whole cache.
        /// </summary>
        public override void Clear()
        {
            filterConditionCache.Clear();
            base.Clear();
        }


        private string GetFilterConditionCacheKey(string filteredObjectType, string whereCondition)
        {
            return filteredObjectType + "|" + whereCondition;
        }
    }
}
