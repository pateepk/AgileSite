namespace CMS.DataEngine
{
    /// <summary>
    /// Execution type of the query.
    /// </summary>
    public enum QueryExecutionTypeEnum : int
    {
        /// <summary>
        /// Unknown.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// ExecuteNonQuery.
        /// </summary>
        ExecuteNonQuery = 1,

        /// <summary>
        /// ExecuteQuery.
        /// </summary>
        ExecuteQuery = 2,

        /// <summary>
        /// ExecuteReader.
        /// </summary>
        ExecuteReader = 3,

        /// <summary>
        /// ExecuteScalar.
        /// </summary>
        ExecuteScalar = 4,
    }
}