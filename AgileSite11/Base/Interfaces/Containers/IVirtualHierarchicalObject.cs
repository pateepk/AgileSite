namespace CMS.Base
{
    /// <summary>
    /// Interface for hierarchical object that is able to provide virtual content (for properties that would return null)
    /// </summary>
    public interface IVirtualHierarchicalObject : IHierarchicalObject
    {
        /// <summary>
        /// Returns property with given name (either object or property value).
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Returns the value</param>
        /// <param name="notNull">If true, the object always tries to return non-null value for properties, at least, it returns empty objects</param>
        /// <returns>Returns true if the operation was successful (the value was present)</returns>
        bool TryGetProperty(string columnName, out object value, bool notNull);
    }
}