namespace CMS.Base
{
    /// <summary>
    /// Read-only version of the abstract hierarchical object with SetValue hidden
    /// </summary>
    public abstract class ReadOnlyAbstractHierarchicalObject<ObjectType> : AbstractHierarchicalObject<ObjectType>
        where ObjectType : ReadOnlyAbstractHierarchicalObject<ObjectType>
    {
        /// <summary>
        /// Hide the set value method
        /// </summary>
        private new bool SetValue(string columnName, object value)
        {
            return false;
        }
    }
}
