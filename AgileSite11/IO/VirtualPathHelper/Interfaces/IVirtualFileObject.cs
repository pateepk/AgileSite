namespace CMS.IO
{
    /// <summary>
    /// Virtual file object interface, represents object used for DbVirtualFile logic
    /// </summary>
    public interface IVirtualFileObject
    {
        /// <summary>
        /// Gets virtual file content for current object
        /// </summary>
        string Content
        {
            get;
        }


        /// <summary>
        /// Indicates whether file content is stored externally (e.g. file system)
        /// </summary>
        bool IsStoredExternally
        {
            get;
        }


        /// <summary>
        /// Gets object hash string used for identification of compiled version in .NET cache
        /// </summary>
        string ObjectHash
        {
            get;
        }


        /// <summary>
        /// Gets the physical file path if exists
        /// </summary>
        string PhysicalFilePath
        {
            get;
        }
    }
}
