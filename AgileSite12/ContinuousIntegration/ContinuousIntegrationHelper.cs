using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Base;
using CMS.DataEngine;
using CMS.LicenseProvider;
using CMS.ContinuousIntegration.Internal;

namespace CMS.ContinuousIntegration
{
    /// <summary>
    /// Continuous integration helper.
    /// </summary>
    internal class ContinuousIntegrationHelper : AbstractHelper<ContinuousIntegrationHelper>
    {
        internal const string ENABLED_CI_KEY = "CMSEnableCI";


        /// <summary>
        /// Indicates if object serialization is enabled.
        /// </summary>
        public static bool IsObjectSerializationEnabled
        {
            get
            {
                return HelperObject.IsObjectSerializationEnabledInternal;
            }
        }


        /// <summary>
        /// Indicates if object serialization is enabled.
        /// </summary>
        protected virtual bool IsObjectSerializationEnabledInternal
        {
            get
            {
                return SettingsKeyInfoProvider.GetBoolValue(ENABLED_CI_KEY) && CMSActionContext.CurrentContinuousIntegrationAllowObjectSerialization;
            }
        }


        /// <summary>
        /// Indicates whether restore all operation is enabled.
        /// </summary>
        /// <remarks>
        /// This property returns true, unless explicitly set to false by the web.config key <c>CMSContinuousIntegrationRestoreAllEnabled</c>
        /// The key is for internal development only (to prevent unintended RestoreAll operation execution).
        /// </remarks>
        public static BoolAppSetting RestoreAllEnabled = new BoolAppSetting("CMSContinuousIntegrationRestoreAllEnabled", true);


        /// <summary>
        /// Indicates whether license requirements are met. 
        /// </summary>
        /// <returns>True if license requirements are met.</returns>
        public static bool CheckLicense()
        {
            return HelperObject.CheckLicenseInternal();
        }


        /// <summary>
        /// Indicates whether object serialization is enabled and specified object type supports it.
        /// </summary>
        /// <param name="typeInfo">Object type information</param>
        public static bool IsSupportedForObjectType(ObjectTypeInfo typeInfo)
        {
            return HelperObject.IsSupportedForObjectTypeInternal(typeInfo);
        }


        /// <summary>
        /// Iterates through collection of dependent object types and where conditions and select all objects that satisfy at least one condition. Executes passed action on all objects. 
        /// Selected objects are passed to <paramref name="infoAction"/> grouped by their type. 
        /// </summary>
        /// <remarks>
        /// <paramref name="infoAction"/> may be executed multiple times even with objects of the same type. When number of where conditions for given type exceeds internal limit, 
        /// then conditions are split into multiple queries. That means that objects of the same type are not always served altogether in one action call.
        /// </remarks>
        /// <param name="depWhereConditions">Collection of object types and where conditions that selects dependent objects.</param>
        /// <param name="infoAction">Action that is executed for all objects that satisfy the where condition. Objects are grouped by type in one action call.</param>
        public static void ProcessDependentObjects(IEnumerable<KeyValuePair<string, ICollection<WhereCondition>>> depWhereConditions, Action<ObjectTypeInfo, ICollection<BaseInfo>> infoAction)
        {
            HelperObject.ProcessDependentObjectsInternal(depWhereConditions, infoAction);
        }


        /// <summary>
        /// Gets conditions selecting objects that depend on dependency objects specified by passed where condition.
        /// </summary>
        /// <param name="typeInfo">Type info of dependency object</param>
        /// <param name="dependencyWhere">Where condition selecting dependency objects</param>
        public static IEnumerable<KeyValuePair<string, ICollection<WhereCondition>>> GetDependentObjectsWhereConditions(ObjectTypeInfo typeInfo, IWhereCondition dependencyWhere)
        {
            return HelperObject.GetDependentObjectsWhereConditionsInternal(typeInfo, dependencyWhere);
        }


        /// <summary>
        /// Indicates whether object serialization is enabled and specified object type supports it.
        /// </summary>
        /// <param name="typeInfo">Object type information</param>
        protected virtual bool IsSupportedForObjectTypeInternal(ObjectTypeInfo typeInfo)
        {
            return typeInfo.ContinuousIntegrationSettings.Enabled && IsObjectSerializationEnabled;
        }


        /// <summary>
        /// Iterates through collection of dependent object types and where conditions and select all objects that satisfy at least one condition. Executes passed action on all objects. 
        /// Selected objects are passed to <paramref name="infoAction"/> grouped by their type. 
        /// </summary>
        /// <remarks>
        /// <paramref name="infoAction"/> may be executed multiple times even with objects of the same type. When number of where conditions for given type exceeds internal limit, 
        /// then conditions are split into multiple queries. That means that objects of the same type are not always served altogether in one action call.
        /// </remarks>
        /// <param name="depWhereConditions">Collection of object types and where conditions that selects dependent objects.</param>
        /// <param name="infoAction">Action that is executed for all objects that satisfy the where condition. Objects are grouped by type in one action call.</param>
        protected virtual void ProcessDependentObjectsInternal(IEnumerable<KeyValuePair<string, ICollection<WhereCondition>>> depWhereConditions, Action<ObjectTypeInfo, ICollection<BaseInfo>> infoAction)
        {
            if (depWhereConditions == null)
            {
                return;
            }

            foreach (var pair in depWhereConditions)
            {
                var typeInfo = ObjectTypeManager.GetTypeInfo(pair.Key, true);

                Action<WhereCondition> processConditionAction = whereCondition =>
                {
                    var infoObjects = DatabaseObjectsEnumeratorFactory.GetObjectEnumerator(typeInfo.ObjectType, FileSystemRepositoryManager.GetInstance().CachedConfiguration, whereCondition).ToArray();
                    infoAction(typeInfo, infoObjects);
                };

                // Aggregation limit. Where conditions can be complex and aggregating them can exceed SQL server parser capacities. 
                const int LIMIT = 10;
                var groupedConditionsCounter = 0;

                var where = typeInfo.CreateWhereCondition();
                where.WhereIsComplex = true;

                foreach (var condition in pair.Value)
                {
                    groupedConditionsCounter++;
                    where.Or(condition);

                    if (groupedConditionsCounter >= LIMIT)
                    {
                        processConditionAction(where);

                        groupedConditionsCounter = 0;
                        where = typeInfo.CreateWhereCondition();
                        where.WhereIsComplex = true;
                    }
                }
                // Last batch was not processed unless it reached the LIMIT, process it now
                if (groupedConditionsCounter != 0)
                {
                    processConditionAction(where);
                }
            }
        }


        /// <summary>
        /// Gets conditions selecting objects that depend on dependency objects specified by passed where condition.
        /// </summary>
        /// <param name="typeInfo">Type info of dependency object</param>
        /// <param name="dependencyWhere">Where condition selecting dependency objects</param>
        protected virtual IEnumerable<KeyValuePair<string, ICollection<WhereCondition>>> GetDependentObjectsWhereConditionsInternal(ObjectTypeInfo typeInfo, IWhereCondition dependencyWhere)
        {
            var readOnlyInfo = ModuleManager.GetReadOnlyObject(typeInfo.ObjectType, true);

            var configuration = FileSystemRepositoryManager.GetInstance().CachedConfiguration;

            // Add type condition to base condition. Make sure we won't select dependent objects of another types.
            var baseWhere = typeInfo.CreateWhereCondition().Where(dependencyWhere);
            baseWhere.WhereIsComplex = true;
            if (typeInfo.TypeCondition != null)
            {
                baseWhere.Where(typeInfo.TypeCondition.GetWhereCondition());
            }

            // Get where condition for each dependent type that is supported in current CI configuration.
            return 
                new ObjectDependenciesRemover()
                    .GetDependentObjectsWhereConditions(readOnlyInfo, baseWhere)
                    .Where(pair => RepositoryConfigurationEvaluator.IsObjectTypeIncluded(pair.Key, configuration));
        }


        /// <summary>
        /// Indicates whether license requirements are met. 
        /// </summary>
        /// <returns>True if license requirements are met.</returns>
        internal virtual bool CheckLicenseInternal()
        {
            return LicenseHelper.IsFeatureAvailableInBestLicense(FeatureEnum.ContinuousIntegration);
        }
    }
}
