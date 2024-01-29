namespace CMS.CMSImportExport
{
    /// <summary>
    /// Import type enumeration.
    /// </summary>
    public enum ImportTypeEnum
    {
        /// <summary>
        /// Do not import any objects.
        /// </summary>
        None = 0,

        /// <summary>
        /// Import all objects unless there is a potential conflict between older and newer version data. 
        /// In this case, for older packages, some of the objects are imported only if they don't exist yet.
        /// Use AllForced to always overwrite data.
        /// </summary>
        AllNonConflicting = 1,

        /// <summary>
        /// Import only new objects.
        /// </summary>
        New = 2,

        /// <summary>
        /// Default.
        /// </summary>
        Default = 3,

        /// <summary>
        /// Forcibly import all objects, even if there can be potential collisions of data from older package.
        /// Use AllNonConflicting to avoid overwriting of the new data by the package data.
        /// </summary>
        AllForced = 4
    }
}