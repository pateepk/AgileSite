namespace CMS.ContinuousIntegration.Internal
{
    /// <summary>
    /// Factory for file system repository restore objects by type jobs.
    /// </summary>
    public sealed class FileSystemUpsertObjectsByTypeJobFactory : FileSystemJobFactory<FileSystemUpsertObjectsByTypeJobFactory, FileSystemUpsertObjectsByTypeJob>
    {
        /// <summary>
        /// Initializes insert and update objects by type job factory with default job set to <see cref="FileSystemUpsertObjectsByTypeJob"/>.
        /// </summary>
        /// <remarks>
        /// The constructor is public for the purpose of the class instantiation by the abstract predecessor of the class. There is no need
        /// to instantiate it in your custom code.
        /// </remarks>
        public FileSystemUpsertObjectsByTypeJobFactory()
            : base(c => new FileSystemUpsertObjectsByTypeJob(c))
        {
        }
    }
}
