namespace CMS.Helpers
{
    /// <summary>
    /// Defines formats available for functionality of DataExportHelper.
    /// </summary>
    public enum DataExportFormatEnum
    {
        /// <summary>
        /// Excel 2007.
        /// </summary>
        [EnumStringRepresentation("XLSX")]
        XLSX = 0,

        /// <summary>
        /// Comma-separated values.
        /// </summary>
        [EnumStringRepresentation("CSV")]
        CSV = 1,

        /// <summary>
        /// EXtensible Markup Language.
        /// </summary>
        [EnumStringRepresentation("XML")]
        XML = 2
    }
}