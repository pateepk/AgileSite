using CMS.DataEngine;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Defines an interface indicating that an object instance returns <see cref="IWhereCondition"/> from its inner state.
    /// </summary>
    internal interface IDocumentQueryFilter
    {
        /// <summary>
        /// Returns <see cref="IWhereCondition"/> object based on current instance's inner state.
        /// </summary>
        /// <param name="properties">Query properties representing the query inner state.</param>
        IWhereCondition GetWhereCondition(DocumentQueryProperties properties);
    }
}