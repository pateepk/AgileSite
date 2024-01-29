namespace CMS.DataEngine
{
    /// <summary>
    /// Defines scope where part of a query is used.
    /// </summary>
    public enum QueryScopeEnum
    {
        /// <summary>
        /// Not specified.
        /// </summary>
        None,

        /// <summary>
        /// Part of a query is used to select columns.
        /// </summary>
        Columns,

        /// <summary>
        /// Part of a query is used as WHERE clause.
        /// </summary>
        Where,

        /// <summary>
        /// Part of a query is used as ORDER BY clause.
        /// </summary>
        OrderBy,

        /// <summary>
        /// Query is regular query.
        /// </summary>
        Query
    }
}
