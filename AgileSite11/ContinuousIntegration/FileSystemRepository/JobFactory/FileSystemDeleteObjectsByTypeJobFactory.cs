namespace CMS.ContinuousIntegration.Internal
{
    /// <summary>
    /// Factory for file system repository restore objects by type jobs.
    /// </summary>
    public sealed class FileSystemDeleteObjectsByTypeJobFactory : FileSystemJobFactory<FileSystemDeleteObjectsByTypeJobFactory, FileSystemDeleteObjectsByTypeJob>
    {
        /// <summary>
        /// Initializes delete objects by type job factory with default job set to <see cref="FileSystemDeleteObjectsByTypeJob"/>.
        /// </summary>
        /// <remarks>
        /// The constructor is public for the purpose of the class instantiation by the abstract predecessor of the class. There is no need
        /// to instantiate it in your custom code.
        /// </remarks>
        public FileSystemDeleteObjectsByTypeJobFactory()
            : base(c => new FileSystemDeleteObjectsByTypeJob(c))
        {
        }
    }
}
