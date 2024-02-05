namespace CMS.ContinuousIntegration.Internal
{
    /// <summary>
    /// Factory for file system repository delete jobs.
    /// </summary>
    public sealed class FileSystemDeleteJobFactory : FileSystemJobFactory<FileSystemDeleteJobFactory, FileSystemDeleteJob>
    {
        /// <summary>
        /// Initializes delete job factory with default job set to <see cref="FileSystemDeleteJob"/>.
        /// </summary>
        /// <remarks>
        /// The constructor is public for the purpose of the class instantiation by the abstract predecessor of the class. There is no need
        /// to instantiate it in your custom code.
        /// </remarks>
        public FileSystemDeleteJobFactory()
            : base(c => new FileSystemDeleteJob(c))
        {
        }
    }
}
