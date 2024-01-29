namespace CMS.DataEngine
{
    /// <summary>
    /// Export format enumeration.
    /// </summary>
    public enum ExportFormatEnum : int
    {
        /// <summary>
        /// XML format
        /// </summary>
        XML = 0,

        /// <summary>
        /// JSON format
        /// </summary>
        JSON = 1,

        /// <summary>
        /// Atom 1.0 format
        /// </summary>
        ATOM10 = 2,

        /// <summary>
        /// RSS 2.0 format
        /// </summary>
        RSS20 = 3
    }
}