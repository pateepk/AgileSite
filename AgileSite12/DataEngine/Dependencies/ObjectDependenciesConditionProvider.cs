namespace CMS.DataEngine
{
    /// <summary>
    /// Describes <see cref="WhereCondition"/> provider for object types with specified path column.
    /// </summary>
    /// <seealso cref="ObjectTypeInfo.ObjectPathColumn"/>
    public abstract class ObjectDependenciesConditionProvider
    {
        /// <summary>
        /// Returns <see cref="WhereCondition"/> used to retrieve data from database based on object path.
        /// </summary>
        /// <param name="info">Info object instance</param>
        /// <param name="objectWhereCondition">Where condition applied to retrieve data.</param>
        /// <param name="pathColumnName">Name of the column containing object path.</param>
        public abstract WhereCondition GetWhereCondition(BaseInfo info, IWhereCondition objectWhereCondition, string pathColumnName);
    }
}