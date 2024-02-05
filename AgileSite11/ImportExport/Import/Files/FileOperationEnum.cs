namespace CMS.CMSImportExport
{
    /// <summary>
    /// File operation enumeration.
    /// </summary>
    public enum FileOperationEnum : int
    {
        /// <summary>
        /// No operation.
        /// </summary>
        None = 0,

        /// <summary>
        /// Copy file.
        /// </summary>
        CopyFile = 1,

        /// <summary>
        /// Copy directory.
        /// </summary>
        CopyDirectory = 2,
    }
}