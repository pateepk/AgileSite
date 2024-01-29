namespace CMS.DataEngine
{
    /// <summary>
    /// Enumeration of the SQL operations.
    /// </summary>
    public enum SqlOperationTypeEnum
    {
        /// <summary>
        /// Uknown query.
        /// </summary>
        UnknownQuery = 0,

        /// <summary>
        /// Selection query.
        /// </summary>
        SelectQuery = 1,

        /// <summary>
        /// Insertion query.
        /// </summary>
        InsertQuery = 2,

        /// <summary>
        /// Update query.
        /// </summary>
        UpdateQuery = 3,

        /// <summary>
        /// Delete query.
        /// </summary>
        DeleteQuery = 4,

        /// <summary>
        /// Selection all query.
        /// </summary>
        SelectAll = 7,

        /// <summary>
        /// Deletion all query.
        /// </summary>
        DeleteAll = 10,

        /// <summary>
        /// Selection query from specified date of modification.
        /// </summary>
        SelectModifiedFrom = 11,

        /// <summary>
        /// Insert record with the identity column.
        /// </summary>
        InsertWithIdentity = 14,

        /// <summary>
        /// Updates multiple items based on the columns and where condition.
        /// </summary>
        UpdateAll = 15,

        /// <summary>
        /// General select query
        /// </summary>
        GeneralSelect = 16,

        /// <summary>
        /// General insert query
        /// </summary>
        GeneralInsert = 17,

        /// <summary>
        /// General update query
        /// </summary>
        GeneralUpdate = 18,

        /// <summary>
        /// General delete query
        /// </summary>
        GeneralDelete = 19,

        /// <summary>
        /// General upsert (insert/update) query
        /// </summary>
        GeneralUpsert = 20
    }
}