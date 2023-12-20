using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS.Base;
using CMS.Core;
using CMS.DataEngine.Query;
using CMS.Helpers;

namespace CMS.DataEngine
{
    /// <summary>
    /// Provides support for removing dependencies within the hierarchy of <see cref="BaseInfo" /> objects.
    /// Removes dependencies either through replacing value (for not required dependencies) or by deleting the whole dependent object (for required dependencies)
    /// </summary>
    internal sealed class ObjectDependenciesRemover
    {
        /// <summary>
        /// Removes dependencies for the objects of the given type matching the given where condition
        /// </summary>
        /// <param name="objectType">Base object type</param>
        /// <param name="where">Where condition for the deleted objects</param>
        /// <param name="deleteAll">If false, only required dependencies are deleted, dependencies with default value are replaced with default value and nullable values are replaced with null</param>
        /// <param name="clearCache">If true, caches of all object types which were potentially modified are cleared (provider hashtables and object type cache dummy keys)</param>
        public void RemoveObjectDependencies(string objectType, IWhereCondition where, bool deleteAll = false, bool clearCache = true)
        {
            where = where ?? new WhereCondition();

            var obj = ModuleManager.GetReadOnlyObject(objectType, true);

            // Get list of dependencies to remove
            var dependencies = GetRemoveDependenciesList(obj, deleteAll, where);
            if (dependencies.Any())
            {
                RemoveObjectDependencies(objectType, dependencies, clearCache);
            }
        }


        /// <summary>
        /// Removes object dependencies for a single object. First tries to execute removedependencies query, if not found, automatic process is executed.
        /// </summary>
        /// <param name="infoObj">Info object for which the dependencies should be removed</param>
        /// <param name="deleteAll">If false, only required dependencies are deleted, dependencies with default value are replaced with default value and nullable values are replaced with null</param>
        /// <param name="clearCache">If true, caches of all object types which were potentially modified are cleared (provider hashtables and object type cache dummy keys)</param>
        public void RemoveObjectDependencies(BaseInfo infoObj, bool deleteAll = false, bool clearCache = true)
        {
            // Try to find specialized query first
            string queryName = infoObj.TypeInfo.ObjectClassName + ".removedependencies";
            if (QueryInfoProvider.GetQueryInfo(queryName, false) != null)
            {
                var parameters = new QueryDataParameters();

                // Prepare the parameters
                if (infoObj.TypeInfo.IsBinding)
                {
                    // For binding, use all binding columns for parameters
                    if (infoObj.TypeInfo.SiteIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                    {
                        parameters.Add("@SiteID", infoObj.Generalized.ObjectSiteID);
                    }
                    if (infoObj.TypeInfo.ParentIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                    {
                        parameters.Add("@ParentID", infoObj.Generalized.ObjectParentID);
                    }
                }
                else
                {
                    // Use ID only
                    parameters.Add("@ID", infoObj.Generalized.ObjectID);
                }

                // Remove the dependencies
                ConnectionHelper.ExecuteQuery(queryName, parameters);
            }
            else
            {
                // Query does not exist, delete the dependencies automatically
                RemoveObjectDependenciesAuto(infoObj, deleteAll, clearCache);
            }
        }


        /// <summary>
        /// Automatic process of remove dependencies procedure.
        /// </summary>
        /// <param name="infoObj">Info object</param>
        /// <param name="deleteAll">If false, only required dependencies are deleted, dependencies with default value are replaced with default value and nullable values are replaced with null</param>
        /// <param name="clearCache">If true, caches of all objecttypes which were potentially modified are cleared. This includes provider hashtables and cache entries depending on the given object types data.</param>
        private void RemoveObjectDependenciesAuto(BaseInfo infoObj, bool deleteAll, bool clearCache = true)
        {
            var dependencies = GetRemoveDependenciesList(infoObj, deleteAll);
            if (!dependencies.Any())
            {
                return;
            }

            RemoveObjectDependencies(infoObj.TypeInfo.OriginalObjectType, dependencies, clearCache);
        }


        /// <summary>
        /// Removes object dependencies for the given object type
        /// </summary>
        /// <param name="baseObjectType">Base object type</param>
        /// <param name="dependencies">List of dependencies to delete</param>
        /// <param name="clearCache">If true, the related object cache is cleared</param>
        private void RemoveObjectDependencies(string baseObjectType, ICollection<RemoveDependencyInfo> dependencies, bool clearCache = true)
        {
            RemoveObjectDependenciesByAPI(baseObjectType, dependencies);
            RemoveObjectDependenciesByQuery(dependencies);

            if (clearCache)
            {
                ClearDependenciesCache(dependencies);
            }
        }


        private static void ClearDependenciesCache(IEnumerable<RemoveDependencyInfo> dependencies)
        {
            // Clear hashtables and create web farm task if needed
            var objTypes = dependencies.Select(x => x.ObjectType).ToArray();

            foreach (var objType in objTypes)
            {
                ProviderHelper.ClearHashtables(objType, true);
            }

            // Clear the cache of the cleared objects
            CacheHelper.TouchKeys(
                objTypes
                    .Where(objType => ObjectTypeManager.GetTypeInfo(objType).TouchCacheDependencies)
                    .Select(objType => objType + "|all")
            );
        }


        private static void RemoveObjectDependenciesByQuery(IEnumerable<RemoveDependencyInfo> queries)
        {
            // Process dependent objects that should be removed by database query
            var dbQueries =
                queries
                    .Where(q => !q.UseApi)
                    .Select(x => x.RemoveQuery);

            try
            {
                var batch = new QueryBatch(dbQueries);

                batch.Execute();
            }
            catch (Exception ex)
            {
                CoreServices.EventLog.LogException("System", "REMOVEDEPENDENCIES", ex);

                throw;
            }
        }


        private static void RemoveObjectDependenciesByAPI(string baseObjectType, IEnumerable<RemoveDependencyInfo> queries)
        {
            // Process dependent objects that should be removed using the API
            var apiQueries = queries.Where(q => q.UseApi);

            foreach (var apiQuery in apiQueries)
            {
                RemoveDependencyUsingAPI(baseObjectType, apiQuery.ObjectType, apiQuery.RemoveWithApiSettings);
            }
        }


        /// <summary>
        /// Removes given subset of objects by API.
        /// </summary>
        /// <param name="baseObjectType">Object type that caused the <paramref name="objectType"/> deletion.</param>
        /// <param name="objectType">ObjectType to handle</param>
        /// <param name="removeSettings">Settings describing how the dependent objects should be removed</param>
        private static void RemoveDependencyUsingAPI(string baseObjectType, string objectType, RemoveDependencyWithApiSettings removeSettings)
        {
            using (var ctx = new CMSActionContext())
            {
                bool createVersion = CMSActionContext.CurrentCreateVersion;

                ctx.DisableAll();

                // Delete objects with API
                var obj = ModuleManager.GetReadOnlyObject(objectType, true);

                // Child included to parent dataset during the parent deletion doesn't need own version created for the recycle bin.
                if ((obj.TypeInfo.ParentObjectType != baseObjectType) || !obj.TypeInfo.IncludeToVersionParentDataSet)
                {
                    // Enable version creation so it can be stored in the recycle bin
                    // unless it has been disabled in outer action context (e.g. during destroy operation)
                    ctx.CreateVersion = createVersion;
                }

                var result = GetDependencyData(removeSettings, obj);
                if (DataHelper.DataSourceIsEmpty(result))
                {
                    return;
                }

                // We can't use paged query to select objects we are about to delete
                // Deletion of objects would shift row numbers and break the page-ing. 
                var rows = result.Tables[0].Rows.Cast<DataRow>()
                                                 .Select(row => obj.Generalized.NewObject(new LoadDataSettings(row)))
                                                 .Where(removeObj => removeObj != null);

                foreach (var removeObj in rows)
                {
                    switch (removeSettings.Operation)
                    {
                        case RemoveDependencyOperationEnum.Delete:
                            removeObj.Delete();
                            break;
                        case RemoveDependencyOperationEnum.Update:
                            removeObj.SetValue(removeSettings.DependencyColumnName, removeSettings.DefaultValue);
                            removeObj.Update();
                            break;
                    }
                }
            }
        }


        /// <summary>
        /// Returns list of couples objecttype-querytext-connectionstring-useapiflag in correct order in which it should be processed.
        /// </summary>
        /// <param name="infoObj">Info object</param>
        /// <param name="getAll">If true all dependencies chain is returned, if false, only required dependencies are returned (according to required ObjectDependencies parameter)</param>
        /// <param name="baseWhere">Base where condition which should be used while generating the remove dependencies procedure</param>
        /// <param name="processed">List of already processed object types (to avoid infinite loop)</param>
        internal List<RemoveDependencyInfo> GetRemoveDependenciesList(BaseInfo infoObj, bool getAll, IWhereCondition baseWhere = null, HashSet<string> processed = null)
        {
            baseWhere = baseWhere ?? new WhereCondition();
            processed = processed ?? new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
            var queries = new List<RemoveDependencyInfo>();
            var typeInfo = infoObj.TypeInfo;

            // We need to add current and its composite type to make sure potential loops are avoided
            processed.Add(typeInfo.ObjectType);
            processed.Add(typeInfo.CompositeObjectType);

            // Add all regular dependency references
            var dependentObjectTypesToRemove = typeInfo.GetDependencyReferences()
                .Where(dependencyReference => FilterDependency(dependencyReference, processed))
                .ToList();

            foreach (var dependencyReference in dependentObjectTypesToRemove)
            {
                queries.AddRange(ProcessRemoveDependency(infoObj, getAll, baseWhere, dependencyReference, processed));
            }

            // For hierarchical objects there has to be an path deletion call at the end
            var pathWhere = GetDependentObjectPathWhereCondition(infoObj, baseWhere);
            if ((pathWhere != null) && !pathWhere.WhereIsEmpty)
            {
                // Create and add queries for dependency references of hierarchical objects
                foreach (var dependencyReference in dependentObjectTypesToRemove)
                {
                    queries.AddRange(ProcessRemoveDependency(infoObj, getAll, pathWhere, dependencyReference, processed));
                }

                // Add query for hierarchical object itself
                queries.Add(GetHierarchicalDependency(infoObj, pathWhere));
            }

            // Remove previously added types to avoid loops, internal recursion won't add these types again
            processed.Remove(typeInfo.ObjectType);
            processed.Remove(typeInfo.CompositeObjectType);

            return queries;
        }


        /// <summary>
        /// Gets hierarchical dependency for processing children for objects with path column
        /// </summary>
        /// <param name="infoObj">Info object</param>
        /// <param name="pathWhere">Where condition which should be used while generating the remove dependencies procedure</param>
        private RemoveDependencyInfo GetHierarchicalDependency(BaseInfo infoObj, WhereCondition pathWhere)
        {
            if (pathWhere == null)
            {
                throw new ArgumentNullException(nameof(pathWhere));
            }

            if (pathWhere.WhereIsEmpty)
            {
                throw new ArgumentException("Path condition is empty.", nameof(pathWhere));
            }

            var typeInfo = infoObj.TypeInfo;
            RemoveDependencyInfo hierarchicalDependency;

            if (AllowDeleteWithApi(typeInfo))
            {
                // Create dependency which deletes the objects one-by-one using API
                var removeApiSettings = RemoveDependencyWithApiSettings.CreateObjectPathDeleteSettings(pathWhere, typeInfo.ObjectPathColumn);
                hierarchicalDependency = new RemoveDependencyInfo(typeInfo.ObjectType, removeApiSettings);
            }
            else
            {
                // Create dependency which deletes the objects with database query
                var deleteQuery =
                    new DataQuery(typeInfo.ObjectClassName, QueryName.GENERALDELETE)
                        .Where(pathWhere)
                        .Where(typeInfo.TypeCondition?.GetWhereCondition());

                hierarchicalDependency = new RemoveDependencyInfo(typeInfo.ObjectType, deleteQuery);
            }

            return hierarchicalDependency;
        }


        /// <summary>
        /// Applies condition to dependency reference to check if should be included into remove dependencies. 
        /// </summary>
        /// <param name="dependencyReference">Dependency reference</param>
        /// <param name="processed">Collection of processed types used for loop detection.</param>
        private static bool FilterDependency(DependencyReference dependencyReference, IEnumerable<string> processed)
        {
            switch (dependencyReference.IntegrityType)
            {
                case ObjectDependencyEnum.NotRequired:
                case ObjectDependencyEnum.RequiredHasDefault:
                    // Dependent object is updated with null or default value in the dependency column.
                    return true;

                default:
                    {
                        var typeInfo = ObjectTypeManager.GetTypeInfo(dependencyReference.DependentObjectType, true);

                        // Dependent object should be deleted
                        return typeInfo.DeleteAsDependency && !processed.Contains(dependencyReference.DependentObjectType, StringComparer.InvariantCultureIgnoreCase);
                    }
            }
        }


        /// <summary>
        /// Processes a single dependency
        /// </summary>
        /// <param name="infoObj">Info object</param>
        /// <param name="getAll">If true all dependencies chain is returned, if false, only required dependencies are returned (according to required ObjectDependencies parameter)</param>
        /// <param name="baseWhere">Base where condition which should be used while generating the remove dependencies procedure</param>
        /// <param name="dependencyReference">Metadata describing dependency relation.</param>
        /// <param name="processed">List of already processed object types (to avoid infinite loop)</param>
        private IEnumerable<RemoveDependencyInfo> ProcessRemoveDependency(BaseInfo infoObj, bool getAll, IWhereCondition baseWhere, DependencyReference dependencyReference, HashSet<string> processed)
        {
            var dependentObjectsQueries = new List<RemoveDependencyInfo>();
            var dependentObj = ModuleManager.GetReadOnlyObject(dependencyReference.DependentObjectType, true);
            var dependentObjTypeInfo = dependentObj.TypeInfo;
            var dependencyObj = ModuleManager.GetReadOnlyObject(dependencyReference.DependencyObjectType, true);
            var dependencyObjTypeInfo = dependencyObj.TypeInfo;

            // Check whether dependencies should be deleted. Otherwise their foreign key column should be set to default value.
            bool deleteDependencies = getAll || (dependencyReference.IntegrityType == ObjectDependencyEnum.Binding) || (dependencyReference.IntegrityType == ObjectDependencyEnum.Required);

            // Prepare default value for update. Default is NULL.
            object defaultValue = null;
            if (!deleteDependencies)
            {
                // Update operation
                if (dependencyReference.IntegrityType == ObjectDependencyEnum.RequiredHasDefault)
                {
                    // Get the default value
                    var defaultObj = dependencyObj.GetDefaultObject();
                    if (defaultObj != null)
                    {
                        var defaultObjectId = defaultObj.Generalized.ObjectID;

                        // Check that the deleted object is not the default object
                        if (infoObj.Generalized.ObjectID == defaultObjectId)
                        {
                            throw new InvalidOperationException($@"The default object of object type ""{dependencyObjTypeInfo.ObjectType}"" cannot be deleted.");
                        }

                        defaultValue = defaultObjectId;
                    }
                }
            }

            // Where condition selecting all dependent objects
            var where = GetDependentObjectWhereCondition(infoObj, dependentObj, dependencyObj, dependencyReference.DependencyColumnName, baseWhere);


            #region "Process dependent objects with API"

            // Objects which are explicitly marked to be deleted using API or objects containing external columns are handled in a special way
            var externalColumns = dependentObj.Generalized.GetExternalColumns();

            // Delete / Update with API
            if (AllowDeleteWithApi(dependentObjTypeInfo) || ((externalColumns != null) && (externalColumns.Count > 0)))
            {
                if (deleteDependencies)
                {
                    // Delete operation
                    var removeApiSettings = RemoveDependencyWithApiSettings.CreateDeleteSettings(where);
                    dependentObjectsQueries.Add(new RemoveDependencyInfo(dependentObjTypeInfo.ObjectType, removeApiSettings));
                }
                else
                {
                    // Update operation
                    var removeApiSettings = RemoveDependencyWithApiSettings.CreateUpdateSettings(where, dependencyReference.DependencyColumnName, defaultValue);
                    dependentObjectsQueries.Add(new RemoveDependencyInfo(dependentObjTypeInfo.ObjectType, removeApiSettings));
                }

                return dependentObjectsQueries;
            }

            #endregion


            #region "Process dependent objects with database query"

            // Delete by database query
            IDataQuery removeQuery;

            if (deleteDependencies)
            {
                // Delete query
                // Add dependencies of dependencies only when the dependency is required or all dependencies chain should be returned
                var addDependencies = GetRemoveDependenciesList(dependentObj, getAll, where, processed);

                dependentObjectsQueries.AddRange(addDependencies);

                removeQuery = new DataQuery(dependentObjTypeInfo.ObjectClassName, QueryName.GENERALDELETE);
            }
            else
            {
                // Update query
                var updateColumnName = dependencyReference.DependencyColumnName;

                var updateTypeInfo = dependentObjTypeInfo;
                if (updateTypeInfo.IsComposite)
                {
                    // We can't execute the update query on the composite type as its primary class may not contain the updated column, so we need to switch to correct underlying type which contains this column
                    updateTypeInfo =
                        updateTypeInfo.ConsistsOf.Select(
                            objectType => ObjectTypeManager.GetTypeInfo(objectType)
                        ).First(
                            typeInfo => typeInfo.ClassStructureInfo.ContainsColumn(updateColumnName)
                        );
                }

                removeQuery = new DataQuery(updateTypeInfo.ObjectClassName, QueryName.GENERALUPDATE);
                removeQuery.SetUpdateQueryValues(new Dictionary<string, object> { { updateColumnName, defaultValue } });
            }

            removeQuery.ApplySettings(q => q
                .Where(where)
                .Where(dependentObjTypeInfo.TypeCondition?.GetWhereCondition().With(w => w.WhereIsComplex = true))
            );

            // Add actual dependency
            dependentObjectsQueries.Add(
                new RemoveDependencyInfo(dependentObjTypeInfo.ObjectType, removeQuery)
            );

            return dependentObjectsQueries;

            #endregion
        }


        /// <summary>
        /// Returns if a depending objects which are defined by given <paramref name="typeInfo"/> can be deleted with API or with generated query.
        /// </summary>
        /// <param name="typeInfo">Type info be checked</param>
        private bool AllowDeleteWithApi(ObjectTypeInfo typeInfo)
        {
            return typeInfo.DeleteObjectWithAPI || ObjectEvents.RequireEventHandling.StartEvent(typeInfo).Result;
        }


        ///<summary>
        /// Returns dictionary collection of where conditions selecting dependent objects for each dependent type.
        /// </summary>
        /// <remarks>
        /// Dependent object types consists of types defined in <see cref="ObjectTypeInfo.DependsOn"/> property, and child types.
        /// In case that current object type represents site or group, then also site or group objects are included.
        /// If current object has <see cref="ObjectTypeInfo.ObjectPathColumn"/> defined, then also objects of same type with sub path are included.
        /// </remarks>
        /// <param name="infoObj">Info object</param>
        /// <param name="baseWhere">Where condition selecting dependency objects. If null, current (this) instance of BaseInfo is the only dependency.</param>
        /// <param name="conditions">Collection containing already processed types.</param>
        /// <param name="processedObjectTypes">Set of object types which were processed in the call hierarchy of this method.</param>
        internal Dictionary<string, ICollection<WhereCondition>> GetDependentObjectsWhereConditions(BaseInfo infoObj, IWhereCondition baseWhere = null, Dictionary<string, ICollection<WhereCondition>> conditions = null, HashSet<string> processedObjectTypes = null)
        {
            baseWhere = baseWhere ?? new WhereCondition();
            conditions = conditions ?? new Dictionary<string, ICollection<WhereCondition>>(StringComparer.InvariantCultureIgnoreCase);
            processedObjectTypes = processedObjectTypes ?? new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

            var typeInfo = infoObj.TypeInfo;

            // Get condition selecting all path-dependent objects
            var pathWhere = AddPathDependentWhereCondition(infoObj, baseWhere, conditions);

            // Add current type to processed to stop object type loops in recursive mode
            processedObjectTypes.Add(typeInfo.ObjectType);
            processedObjectTypes.Add(typeInfo.CompositeObjectType);

            // Process static references to dependent types that have not been processed yet (avoid infinite recursion)
            var dependencyReferences = typeInfo.GetDependencyReferences().Where(d => !processedObjectTypes.Contains(d.DependentObjectType, StringComparer.InvariantCultureIgnoreCase));
            foreach (var dependencyReference in dependencyReferences)
            {
                var dependentObject = ModuleManager.GetReadOnlyObject(dependencyReference.DependentObjectType, true);
                var dependencyObject = ModuleManager.GetReadOnlyObject(dependencyReference.DependencyObjectType, true);

                // Get condition selecting all statically dependent objects
                var where = GetDependentObjectWhereCondition(infoObj, dependentObject, dependencyObject, dependencyReference.DependencyColumnName, baseWhere);
                where.WhereIsComplex = true;

                AddDependentWhereCondition(conditions, dependencyReference.DependentObjectType, where);

                // Recursively process all dependencies of statically dependent objects
                GetDependentObjectsWhereConditions(dependentObject, where, conditions, processedObjectTypes);

                if (pathWhere != null)
                {
                    // Get condition selecting all objects that statically depends on the path-dependent objects
                    var pathDependentWhere = GetDependentObjectWhereCondition(infoObj, dependentObject, dependencyObject, dependencyReference.DependencyColumnName, pathWhere);
                    if ((pathDependentWhere != null) && !pathDependentWhere.WhereIsEmpty)
                    {
                        // Add condition - these objects are not processed using recursion call because pathWhere condition has already selected all sub-path dependent objects
                        AddDependentWhereCondition(conditions, dependencyReference.DependentObjectType, pathDependentWhere);

                        // Recursively process all dependencies of objects that statically depends on path-dependent objects
                        GetDependentObjectsWhereConditions(dependentObject, pathDependentWhere, conditions, processedObjectTypes);
                    }
                }
            }

            // Removed current type from processed items
            processedObjectTypes.Remove(typeInfo.ObjectType);
            processedObjectTypes.Remove(typeInfo.CompositeObjectType);

            // Add conditions selecting all objects that dynamically depends on currently processed object or objects.
            // If all dependent objects are to be returned recursively, dynamic dependencies of path-dependent objects are also added.
            AddDynamicDependentWhereConditions(infoObj, baseWhere, conditions, pathWhere);

            return conditions;
        }


        /// <summary>
        /// Adds where condition that selects path dependencies of objects specified by <paramref name="baseWhere"/> to <paramref name="conditions"/> collection.
        /// Returns the added condition or null if no condition was added.
        /// </summary>
        /// <param name="infoObj">Info object</param>
        /// <param name="baseWhere">Base where condition</param>
        /// <param name="conditions">Collection containing already inserted conditions</param>
        private IWhereCondition AddPathDependentWhereCondition(BaseInfo infoObj, IWhereCondition baseWhere, Dictionary<string, ICollection<WhereCondition>> conditions)
        {
            if (infoObj.TypeInfo.ObjectPathColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
            {
                var pathWhere = GetDependentObjectPathWhereCondition(infoObj, baseWhere);

                if ((pathWhere != null) && !pathWhere.WhereIsEmpty && AddDependentWhereCondition(conditions, infoObj.TypeInfo.ObjectType, pathWhere))
                {
                    return pathWhere;
                }
            }

            return null;
        }


        /// <summary>
        /// Adds where conditions that select dynamic dependencies of objects specified by <paramref name="baseWhere"/> and <paramref name="pathWhere"/> to <paramref name="conditions"/> collection.
        /// </summary>
        /// <param name="infoObj">Info object</param>
        /// <param name="baseWhere">Base where condition</param>
        /// <param name="conditions">Collection containing already inserted conditions</param>
        /// <param name="pathWhere">Where condition for the path for hierarchical objects</param>
        private void AddDynamicDependentWhereConditions(BaseInfo infoObj, IWhereCondition baseWhere, Dictionary<string, ICollection<WhereCondition>> conditions, IWhereCondition pathWhere = null)
        {
            var dependencyObjectTypes = infoObj.TypeInfo.ConsistsOf ?? new[] { infoObj.TypeInfo.ObjectType };

            foreach (var dependencyReference in infoObj.TypeInfo.GetDynamicDependencyReferences())
            {
                var dependentObject = ModuleManager.GetReadOnlyObject(dependencyReference.DependentObjectType, true);

                foreach (var dependencyObjectType in dependencyObjectTypes)
                {
                    var dependencyObject = ModuleManager.GetReadOnlyObject(dependencyObjectType, true);

                    var where = GetDependentObjectWhereCondition(infoObj, dependentObject, dependencyObject, dependencyReference.DependencyColumnName, baseWhere);
                    where.WhereEquals(dependencyReference.DependencyObjectTypeColumnName, dependencyObjectType);
                    where.WhereIsComplex = true;

                    AddDependentWhereCondition(conditions, dependencyReference.DependentObjectType, where);

                    if ((pathWhere != null) && !pathWhere.WhereIsEmpty)
                    {
                        // Add conditions selecting dynamic dependencies of path-dependent objects
                        var pathDependentWhere = GetDependentObjectWhereCondition(infoObj, dependentObject, dependencyObject, dependencyReference.DependencyColumnName, pathWhere);
                        AddDependentWhereCondition(conditions, dependencyReference.DependentObjectType, pathDependentWhere);
                    }
                }
            }
        }


        /// <summary>
        /// Inserts new where condition to collection of where conditions. Returns true if the condition was inserted, false otherwise.
        /// </summary>
        /// <param name="conditions">Collection containing already inserted conditions</param>
        /// <param name="objectType">Type of dependent objects that is selected by <paramref name="where"/></param>
        /// <param name="where">Where condition that select dependent objects of type <paramref name="objectType"/></param>
        private static bool AddDependentWhereCondition(IDictionary<string, ICollection<WhereCondition>> conditions, string objectType, WhereCondition where)
        {
            if (where.WhereIsEmpty)
            {
                return false;
            }

            if (!conditions.ContainsKey(objectType))
            {
                conditions[objectType] = new HashSet<WhereCondition>();
            }
            var objTypeConditions = conditions[objectType];
            if (objTypeConditions.Contains(where))
            {
                return false;
            }

            where.Immutable();
            conditions[objectType].Add(where);

            return true;
        }


        /// <summary>
        /// Returns where condition selecting objects that are located on sub path of current object's <see cref="ObjectTypeInfo.ObjectPathColumn"/> 
        /// or on sub path of dependency objects specified by <paramref name="baseWhere"/> condition.
        /// </summary>      
        /// <param name="infoObj">Info object</param>
        /// <param name="baseWhere">Where condition selecting dependency objects. If null, current (this) instance of base info is the only dependency.</param>
        private static WhereCondition GetDependentObjectPathWhereCondition(BaseInfo infoObj, IWhereCondition baseWhere = null)
        {
            var ti = infoObj.TypeInfo;

            // If object has object path column then all objects with sub path are dependent
            if (ti.ObjectPathColumn == ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
            {
                return null;
            }

            WhereCondition finalPathWhere = new WhereCondition();
            // In case of site object, search only on given site
            if (ti.IsSiteObject && (ti.SiteIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN))
            {
                var sitePathWhere = new WhereCondition();
                sitePathWhere.WhereIsComplex = true;

                var siteID = infoObj.GetIntegerValue(ti.SiteIDColumn, 0);
                if (siteID != 0)
                {
                    finalPathWhere.Where(sitePathWhere.WhereEquals(ti.SiteIDColumn, siteID));
                }
            }

            var innerPathWhere = new WhereCondition();

            if ((baseWhere == null) || baseWhere.WhereIsEmpty)
            {
                // No base where condition defined, current object is dependency
                innerPathWhere.WhereStartsWith(ti.ObjectPathColumn, infoObj.GetStringValue(ti.ObjectPathColumn, "") + "/");
            }
            else
            {
                innerPathWhere = ObjectDependenciesRemoverConditionHelper.GetMultipleSubTrees(infoObj, baseWhere, ti.ObjectPathColumn);
            }

            // Prevent possible unnecessary brackets in query
            if (!finalPathWhere.WhereIsEmpty)
            {
                innerPathWhere.WhereIsComplex = true;
                finalPathWhere.And().Where(innerPathWhere);
            }
            else
            {
                finalPathWhere = innerPathWhere;
            }

            finalPathWhere.WhereIsComplex = true;

            return finalPathWhere;
        }


        /// <summary>
        /// Returns where condition selecting dependent objects.
        /// </summary>      
        /// <param name="infoObj">Info object</param>
        /// <param name="dependentObj">Read only info object that depends on type specified by <paramref name="dependencyObj"/> parameter.</param>
        /// <param name="dependencyObj">Read only info object that is a dependency of type specified in <paramref name="dependentObj"/> parameter.</param>
        /// <param name="dependencyColumn">Name of the column containing ID of dependency object.</param>
        /// <param name="baseWhere">Where condition selecting dependency objects. If null current (this) instance of base info is the only dependency.</param>
        private WhereCondition GetDependentObjectWhereCondition(BaseInfo infoObj, BaseInfo dependentObj, BaseInfo dependencyObj, string dependencyColumn, IWhereCondition baseWhere)
        {
            // Where condition selecting all dependent objects
            var where = dependentObj.TypeInfo.CreateWhereCondition();
            if ((baseWhere == null) || (baseWhere.WhereIsEmpty))
            {
                // No dependency where condition, dependent objects depends on current object
                where.WhereEquals(dependencyColumn, infoObj.GetValue(dependencyObj.TypeInfo.IDColumn));
            }
            else
            {
                // Dependency condition is defined. Use it to select all dependencies
                var innerSelect = infoObj.GetDataQuery(true, settings => settings.Where(baseWhere), false).AsSingleColumn(dependencyObj.TypeInfo.IDColumn, true);

                // Allow materialization for objects placed on separated database
                innerSelect.AllowMaterialization = true;

                // Select all dependent objects of these dependencies
                where.WhereIn(dependencyColumn, innerSelect);
            }

            return where;
        }


        /// <summary>
        /// Returns info objects matching given <see cref="RemoveDependencyInfo"/>
        /// </summary>
        internal IInfoDataSet GetDependencyInfoObjects(RemoveDependencyInfo dependency)
        {
            var obj = ModuleManager.GetReadOnlyObject(dependency.ObjectType, true);
            return (IInfoDataSet)GetDependencyData(dependency.RemoveWithApiSettings, obj);
        }


        private static DataSet GetDependencyData(RemoveDependencyWithApiSettings removeSettings, BaseInfo obj)
        {
            var query = obj.GetDataQuery(true, removeSettings.DataQuerySettings, false);
            query.IncludeBinaryData = false;

            return query.Result;
        }
    }
}
