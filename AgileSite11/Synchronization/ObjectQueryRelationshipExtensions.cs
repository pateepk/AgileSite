using System;

using CMS.DataEngine;

namespace CMS.Synchronization
{
    /// <summary>
    /// Object query extension methods for getting data related to the query.
    /// </summary>
    internal static class ObjectQueryRelationshipExtensions
    {
        /// <summary>
        /// Gets the query for all children of the given type of the objects that the query represents
        /// </summary>
        /// <param name="objectType">Nested object type</param>
        /// <param name="query">Query</param>
        public static ObjectQuery GetChildren(this IObjectQuery query, string objectType)
        {
            var result = query.GetNested(objectType);

            // Get child settings
            var obj = ModuleManager.GetReadOnlyObject(query.ObjectType);
            var childWhere = obj.Generalized.GetChildWhereCondition(null, result.ObjectType);

            return result.Where(childWhere);
        }


        /// <summary>
        /// Gets the query for all children of the given type of the objects that the query represents
        /// </summary>
        /// <param name="objectType">Nested object type</param>
        /// <param name="query">Query</param>
        public static ObjectQuery GetBindings(this IObjectQuery query, string objectType)
        {
            return query.GetNested(objectType);
        }


        /// <summary>
        /// Gets the query for all other bindings of the given type of the objects that the query represents
        /// </summary>
        /// <param name="objectType">Nested object type</param>
        /// <param name="query">Query</param>
        public static ObjectQuery GetOtherBindings(this IObjectQuery query, string objectType)
        {
            var result = new ObjectQuery(objectType);

            // Get the parent ID column to restrict binding
            var ti = ObjectTypeManager.GetTypeInfo(query.ObjectType);
            var parentIdColumn = ObjectHelper.GetOtherBindingParentIdColumn(result.TypeInfo, ti);

            // Restrict the child query only to children of current result
            result.WhereIn(parentIdColumn, query);

            return result;
        }


        /// <summary>
        /// Gets the query for all nested objects of the given type of the objects that the query represents
        /// </summary>
        /// <param name="objectType">Nested object type</param>
        /// <param name="query">Query</param>
        internal static ObjectQuery GetNested(this IObjectQuery query, string objectType)
        {
            var result = new ObjectQuery(objectType);

            // Get the parent ID column to restrict children
            var ti = ObjectTypeManager.GetTypeInfo(query.ObjectType);
            var parentIdColumn = ObjectHelper.GetParentIdColumn(result.TypeInfo, ti);
            var parentIdColumnType = result.TypeInfo.GetObjectTypeForColumn(parentIdColumn);

            query = query.CloneWithColumn(GetIdColumn(ti, parentIdColumnType));

            // Restrict the child query only to children of current result
            result.WhereIn(parentIdColumn, query);

            return result;
        }


        private static IObjectQuery CloneWithColumn(this IObjectQuery query, string columnName)
        {
            // Get query for parent IDs
            var relatedIds = (IObjectQuery)query.CloneObject();
            relatedIds.SelectColumnsList.Load(columnName);

            return relatedIds;
        }


        /// <summary>
        /// Gets the query for all related objects of the given type of the objects that the query represents, referenced by the given column name
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="columnName">Column name with the foreign key that points to the related objects</param>
        /// <param name="query">Query</param>
        public static ObjectQuery GetRelatedObjects(this IObjectQuery query, string objectType, string columnName)
        {
            var result = new ObjectQuery(objectType);
            var relatedIds = query.CloneWithColumn(columnName);

            // Restrict the query only to parents of current result
            result.WhereIn(result.TypeInfo.IDColumn, relatedIds);

            return result;
        }


        /// <summary>
        /// Gets the query for all metafiles of the objects that the query represents
        /// </summary>
        /// <param name="query">Query</param>
        public static ObjectQuery GetMetaFiles(this IObjectQuery query)
        {
            var ti = ObjectTypeManager.GetTypeInfo(query.ObjectType);
            if (!ti.HasMetaFiles)
            {
                return null;
            }

            return new ObjectQuery(MetaFileInfo.OBJECT_TYPE)
                .Where("MetaFileObjectType", QueryOperator.Equals, query.ObjectType)
                .WhereIn("MetaFileObjectID", query);
        }


        /// <summary>
        /// Gets the query for all scheduled tasks of the objects that the query represents
        /// </summary>
        /// <param name="query">Query</param>
        public static ObjectQuery GetScheduledTasks(this IObjectQuery query)
        {
            var ti = ObjectTypeManager.GetTypeInfo(query.ObjectType);
            if (!ti.HasScheduledTasks)
            {
                return null;
            }

            return new ObjectQuery(PredefinedObjectType.OBJECTSCHEDULEDTASK)
                .Where("TaskObjectType", QueryOperator.Equals, query.ObjectType)
                .WhereIn("TaskObjectID", query);
        }


        /// <summary>
        /// Gets the query for all processes of the objects that the query represents
        /// </summary>
        /// <param name="query">Query</param>
        public static ObjectQuery GetProcesses(this IObjectQuery query)
        {
            var ti = ObjectTypeManager.GetTypeInfo(query.ObjectType);
            if (!ti.HasProcesses)
            {
                return null;
            }

            return new ObjectQuery(PredefinedObjectType.AUTOMATIONSTATE)
                .Where("StateObjectType", QueryOperator.Equals, query.ObjectType)
                .WhereIn("StateObjectID", query);
        }


        /// <summary>
        /// Gets ID column of the given type from the given type info
        /// </summary>
        /// <param name="ti">Type info</param>
        /// <param name="idColumnType">ID column type</param>
        private static string GetIdColumn(ObjectTypeInfo ti, string idColumnType)
        {
            if (ti.ObjectType == idColumnType)
            {
                return ti.IDColumn;
            }

            if (ti.ConsistsOf != null)
            {
                foreach (var componentType in ti.ConsistsOf)
                {
                    var componentTi = ObjectTypeManager.GetTypeInfo(componentType);
                    var col = GetIdColumn(componentTi, idColumnType);

                    if (!String.IsNullOrEmpty(col))
                    {
                        return col;
                    }
                }
            }

            return null;
        }
    }
}
