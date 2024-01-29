namespace CMS.ImportExport
{
    /// <summary>
    /// Which part of data table should be exported.
    /// </summary>
    internal enum ExportContents
    {
        /// <summary>
        /// Header row.
        /// </summary>
        Header,

        /// <summary>
        /// Data of a table.
        /// </summary>
        Data,

        /// <summary>
        /// Whole table (header and data).
        /// </summary>
        Table
    }
}
