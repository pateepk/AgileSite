namespace CMS.ContinuousIntegration.Internal
{
    /// <summary>
    /// Factory for file system repository store jobs.
    /// </summary>
    public sealed class FileSystemStoreJobFactory : FileSystemJobFactory<FileSystemStoreJobFactory, FileSystemStoreJob>
    {
        /// <summary>
        /// Initializes store job factory with default job set to <see cref="FileSystemStoreJob"/>.
        /// </summary>
        /// <remarks>
        /// The constructor is public for the purpose of the class instantiation by the abstract predecessor of the class. There is no need
        /// to instantiate it in your custom code.
        /// </remarks>
        public FileSystemStoreJobFactory()
            : base(c => new FileSystemStoreJob(c))
        {
        }
    }
}
