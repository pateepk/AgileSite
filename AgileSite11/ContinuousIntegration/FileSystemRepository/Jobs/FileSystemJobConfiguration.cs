namespace CMS.ContinuousIntegration.Internal
{
    /// <summary>
    /// Configuration for File System Job
    /// </summary>
    internal class FileSystemJobConfiguration
    {
        /// <summary>
        /// Object for working with repository's file paths.
        /// </summary>
        public RepositoryPathHelper RepositoryPathHelper
        {
            get;
            set;
        }


        /// <summary>
        /// Translation helper that is used for deserialization.
        /// </summary>
        public ContinuousIntegrationTranslationHelper TranslationHelper
        {
            get;
            set;
        }


        /// <summary>
        /// Object for working with bindings in the file system repository.
        /// </summary>
        public FileSystemBindingsProcessor BindingsProcessor
        {
            get;
            set;
        }


        /// <summary>
        /// Object for writing to file system and storing hash of written file in hash manager.
        /// </summary>
        public IFileSystemWriter FileSystemWriter
        {
            get;
            set;
        }


        /// <summary>
        /// Object for reading file system and storing hash of read file in its own hash manager.
        /// </summary>
        public ICachedFileSystemReader FileSystemReader
        {
            get;
            set;
        }


        /// <summary>
        /// Object for working with separated fields in file system repository.
        /// </summary>
        public SeparatedFieldProcessor SeparatedFieldProcessor
        {
            get;
            set;
        }


        /// <summary>
        /// Object for collecting content staging tasks during CI restore.
        /// </summary>
        public ContentStagingTaskCollection ContentStagingTaskCollection
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether objects' file meta-data are used or not.
        /// </summary>
        public bool? UseFileMetadata
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether license checking should be omitted in this instance of a job.
        /// </summary>
        public bool? SkipLicenseChecking
        {
            get;
            set;
        }
    }
}
