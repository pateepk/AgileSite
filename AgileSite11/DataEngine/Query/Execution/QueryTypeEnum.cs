namespace CMS.DataEngine
{
    /// <summary>
    /// Query type enumeration.
    /// </summary>
    public enum QueryTypeEnum : int
    {
        /// <summary>
        /// SQL query.
        /// </summary>
        SQLQuery = 0,

        /// <summary>
        /// Stored Procedure.
        /// </summary>
        StoredProcedure = 1,
    }
}