namespace CMS.DataEngine
{
    /// <summary>
    /// Configuration class for <see cref="AbstractInfoProvider{TInfo, TProvider, TQuery}.BulkDelete(IWhereCondition, BulkDeleteSettings)"/>.
    /// </summary>
    public sealed class BulkDeleteSettings
    {
        /// <summary>
        /// Indicates whether dependencies of deleted objects are also removed, as it was deleted via API.
        /// </summary>
        /// <remarks>Default <c>false</c>.</remarks>
        public bool RemoveDependencies
        {
            get;
            set;
        }


        /// <summary>
        /// Object type to delete. The object type determines the set of deleted dependencies, and may restrict the set of deleted data. If not provided, default provider object type is used.
        /// </summary>
        public string ObjectType
        {
            get;
            set;
        }
    }
}
