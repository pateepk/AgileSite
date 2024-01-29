namespace CMS.AmazonStorage
{
    /// <summary>
    /// Defines which type of objects is returned by S3ObjectInfoProvider.GetObjectList
    /// </summary>
    public enum ObjectTypeEnum
    {
        /// <summary>
        /// Files
        /// </summary>
        Files = 0,

        /// <summary>
        /// Directories
        /// </summary>
        Directories = 1,

        /// <summary>
        /// Files and directories
        /// </summary>
        FilesAndDirectories
    }
}
