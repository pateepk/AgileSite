using System;

namespace CMS.ContinuousIntegration.Internal
{
    /// <summary>
    /// Extension methods for file system jobs.
    /// </summary>
    internal static class FileSystemJobExtesions
    {
        /// <summary>
        /// Fluently initializes job by injecting instances of provided object properties into respective properties.
        /// </summary>
        /// <param name="fileSystemJob">File system job to initialize.</param>
        /// <param name="fileSystemJobConfiguration">Configuration for the file system job.</param>
        internal static T InitializeWith<T>(this T fileSystemJob, FileSystemJobConfiguration fileSystemJobConfiguration)
            where T : AbstractFileSystemJob
        {
            fileSystemJob.InitializeWithInternal(fileSystemJobConfiguration);
            return fileSystemJob;
        }


        /// <summary>
        /// Fluently sets log progress handler of the <paramref name="fileSystemJob"/> job.
        /// </summary>
        /// <param name="fileSystemJob">File system job to assign action to.</param>
        /// <param name="logProgressAction">Action that should be performed when <see cref="AbstractFileSystemProgressLoggingJob.LogProgress"/> is triggered.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="logProgressAction"/> is not provided.</exception>
        internal static T SetLogProgressHandler<T>(this T fileSystemJob, EventHandler<LogProgressEventArgs> logProgressAction)
            where T : AbstractFileSystemProgressLoggingJob
        {
            fileSystemJob.SetLogProgressHandlerInternal(logProgressAction);
            return fileSystemJob;
        }
    }
}
