using System;

namespace CMS.DataEngine
{
    /// <summary>
    /// Class containing information how dependent objects should be removed.
    /// </summary>
    internal sealed class RemoveDependencyInfo
    {
        /// <summary>
        /// Object type of the dependency.
        /// </summary>
        public string ObjectType
        {
            get;
            private set;
        }


        /// <summary>
        /// If true, API method will be used for object deletion instead of sql query.
        /// </summary>
        public bool UseApi
        {
            get;
            private set;
        }


        /// <summary>
        /// Settings with information about how to remove dependent object with the API.
        /// Used when <see cref="UseApi"/> is set to true, null otherwise.
        /// </summary>
        public RemoveDependencyWithApiSettings RemoveWithApiSettings
        {
            get;
            private set;
        }


        /// <summary>
        /// Query to remove dependent object with database query.
        /// Used when <see cref="UseApi"/> is set to false, null otherwise.
        /// </summary>
        public IDataQuery RemoveQuery
        {
            get;
            private set;
        }


        /// <summary>
        /// Creates new RemoveDependencyInfo that removes dependent objects by using the API.
        /// </summary>
        /// <param name="objectType">Object type of the dependency.</param>
        /// <param name="removeWithApiSettings">Settings describing how dependent objects should be removed</param>
        public RemoveDependencyInfo(string objectType, RemoveDependencyWithApiSettings removeWithApiSettings)
        {
            ObjectType = objectType;
            UseApi = true;
            RemoveWithApiSettings = removeWithApiSettings;
        }


        /// <summary>
        /// Creates new RemoveDependencyInfo that removes the dependent objects by suing database query.
        /// </summary>
        /// <param name="objectType">Object type of the dependency.</param>
        /// <param name="removeQuery">Remove query for dependent objects</param>
        public RemoveDependencyInfo(string objectType, IDataQuery removeQuery)
        {
            ObjectType = objectType;
            UseApi = false;
            RemoveQuery = removeQuery;
        }
    }


    /// <summary>
    /// Represents operation that should be performed with dependent object.
    /// </summary>
    internal enum RemoveDependencyOperationEnum
    {
        Delete = 1,
        Update = 2,
    }


    /// <summary>
    /// Contains information about how to remove dependent object with the API.
    /// </summary>
    internal sealed class RemoveDependencyWithApiSettings
    {
        /// <summary>
        /// Settings that should be applied to data query to select dependent objects.
        /// </summary>
        public Action<DataQuerySettings> DataQuerySettings
        {
            get;
            private set;
        }


        /// <summary>
        /// Identifies operation that should be performed with dependent object.
        /// </summary>
        public RemoveDependencyOperationEnum Operation
        {
            get;
            private set;
        }


        /// <summary>
        /// Name of the column that contains dependency identifier. Value ot this column should be replaced
        /// with <see cref="DefaultValue"/> when <see cref="Operation"/> is set to update. 
        /// </summary>
        public string DependencyColumnName
        {
            get;
            private set;
        }


        /// <summary>
        /// Default value for dependency identifier column. This value should be stored in
        /// <see cref="DependencyColumnName"/> when <see cref="Operation"/> is set to update. 
        /// </summary>
        public object DefaultValue
        {
            get;
            private set;
        }


        // Hide constructor
        private RemoveDependencyWithApiSettings()
        {
        }


        /// <summary>
        /// Creates settings for deleting dependent objects.
        /// </summary>
        /// <param name="whereCondition">Where condition selecting dependent objects</param>
        public static RemoveDependencyWithApiSettings CreateDeleteSettings(WhereCondition whereCondition)
        {
            return new RemoveDependencyWithApiSettings
            {
                DataQuerySettings = settings => settings.Where(whereCondition),
                Operation = RemoveDependencyOperationEnum.Delete,
            };
        }


        /// <summary>
        /// Creates settings for removing the dependent objects by updating their dependency column.
        /// </summary>
        /// <param name="whereCondition">Where condition selecting dependent objects</param>
        /// <param name="dependencyColumnName">Name of the column where dependency identifier is stored</param>
        /// <param name="defaultValue">Default value that should be placed to dependency column</param>
        public static RemoveDependencyWithApiSettings CreateUpdateSettings(WhereCondition whereCondition, string dependencyColumnName, object defaultValue)
        {
            return new RemoveDependencyWithApiSettings
            {
                DataQuerySettings = settings => settings.Where(whereCondition),
                Operation = RemoveDependencyOperationEnum.Update,
                DependencyColumnName = dependencyColumnName,
                DefaultValue = defaultValue,
            };
        }


        /// <summary>
        /// Creates settings for deleting dependent hierarchical objects.
        /// </summary>
        /// <param name="whereCondition">Where condition selecting dependent objects</param>
        /// <param name="objectPathColumnName">Name of the column where hierarchical path to object is stored</param>
        public static RemoveDependencyWithApiSettings CreateObjectPathDeleteSettings(WhereCondition whereCondition, string objectPathColumnName)
        {
            return new RemoveDependencyWithApiSettings
            {
                DataQuerySettings = settings =>
                {
                    settings.Where(whereCondition);
                    settings.OrderByDescending(objectPathColumnName);
                },
                Operation = RemoveDependencyOperationEnum.Delete,
            };
        }
    }
}