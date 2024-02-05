namespace CMS.DataEngine
{
    /// <summary>
    /// Operation type enumeration.
    /// </summary>
    public enum OperationTypeEnum
    {
        /// <summary>
        /// Synchronization, full data.
        /// </summary>
        Synchronization = 0,

        /// <summary>
        /// Export, full data.
        /// </summary>
        Export = 1,

        /// <summary>
        /// Export selection (only ID and code name column, no metafiles or relationships).
        /// </summary>
        ExportSelection = 2,

        /// <summary>
        /// Object version, complete data
        /// </summary>
        Versioning = 3,

        /// <summary>
        /// Data are the same as for the Synchronization. Used as a distinguisher for special cases.
        /// </summary>
        Integration = 4
    }
}