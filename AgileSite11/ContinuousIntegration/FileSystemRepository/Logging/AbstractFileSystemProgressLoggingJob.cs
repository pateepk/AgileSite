using System;
using System.Linq;

using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.ContinuousIntegration.Internal
{
    /// <summary>
    /// Abstract class incorporating logging of job's progress.
    /// </summary>
    /// <remarks>This class is not intended for inheritance. Non-abstract jobs are supposed to be used for behavioral amendments.</remarks>
    /// <seealso cref="AbstractFileSystemAllJob"/>
    /// <seealso cref="AbstractFileSystemTypeWideJob"/>
    /// <seealso cref="FileSystemRestoreObjectsByTypeInternalJob"/>
    public abstract class AbstractFileSystemProgressLoggingJob : AbstractFileSystemJob
    {
        // Maximal length of the name of an info object
        private const int MAXIMAL_NAME_LENGTH = 50;


        #region "Events"

        /// <summary>
        /// Raised for each reported log message.
        /// </summary>
        internal event EventHandler<LogProgressEventArgs> LogProgress;

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor required for creation of a new instance of derived class.
        /// </summary>
        /// <param name="configuration">Repository configuration.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> is null.</exception>
        internal AbstractFileSystemProgressLoggingJob(FileSystemRepositoryConfiguration configuration)
            : base(configuration)
        {
        }


        /// <summary>
        /// Gets display name or code name or GUID or ID of provided <paramref name="info"/>.
        /// </summary>
        /// <param name="info">Information of the object that the name of should be retrieved.</param>
        /// <remarks>Maximal length of the name is limited to about 50 characters, longer names are trimmed.</remarks>
        protected static string GetLogObjectName(BaseInfo info)
        {
            return GetLogObjectName(info.TypeInfo, info);
        }


        /// <summary>
        /// Gets display name or code name or GUID or ID of provided <paramref name="info"/>.
        /// </summary>
        /// <param name="typeInfo">Information of the <paramref name="info"/> object's type.</param>
        /// <param name="info">Information of the object that the name of should be retrieved.</param>
        /// <remarks>Maximal length of the name is limited to about 50 characters, longer names are trimmed.</remarks>
        protected static string GetLogObjectName(ObjectTypeInfo typeInfo, BaseInfo info)
        {
            string result;

            if (typeInfo.IsBinding)
            {
                // Join IDs of all objects that are binded together (usually 2)
                result = String.Join(", ", typeInfo.GetBindingColumns().Select(c => $"{c} {info.GetValue(c)}"));
            }
            else
            {
                result = ResHelper.LocalizeString(info.Generalized.ObjectDisplayName);

                // remove object type name in case it was part of the display name so it is not shown twice in the log
                // appears when object does not have DisplayNameColumn nor CodeNameColumn nor GUIDColumn specified
                string objectType = typeInfo.ObjectType;
                if (result.StartsWith(objectType, StringComparison.InvariantCultureIgnoreCase))
                {
                    result = result.Substring(objectType.Length).TrimStart();
                }
            }

            return TextHelper.LimitLength(result, MAXIMAL_NAME_LENGTH);
        }


        /// <summary>
        /// Logs message to progress log.
        /// </summary>
        /// <param name="message">Text of the message.</param>
        /// <param name="messageType">Message type.</param>
        /// <param name="actionType">Action type</param>
        protected void RaiseLogProgress(string message, LogItemTypeEnum messageType = LogItemTypeEnum.Info, LogItemActionTypeEnum actionType = LogItemActionTypeEnum.Unknown)
        {
            if (LogProgress == null)
            {
                // No event handler attached
                return;
            }

            var logItem = new LogItem(message, messageType, actionType);

            RaiseLogProgress(logItem);
        }


        /// <summary>
        /// Logs message to progress log.
        /// </summary>
        /// <param name="logItem">Log item</param>
        protected void RaiseLogProgress(LogItem logItem)
        {
            if (LogProgress == null)
            {
                // No event handler attached
                return;
            }

            LogProgress(this, new LogProgressEventArgs(logItem));
        }


        /// <summary>
        /// Sets log progress handler of the job.
        /// </summary>
        /// <param name="logProgressAction">Action that should be performed when <see cref="AbstractFileSystemProgressLoggingJob.LogProgress"/> is triggered.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="logProgressAction"/> is not provided.</exception>
        internal void SetLogProgressHandlerInternal(EventHandler<LogProgressEventArgs> logProgressAction)
        {
            if (logProgressAction == null)
            {
                throw new ArgumentNullException("logProgressAction");
            }
            LogProgress += logProgressAction;
        }


        /// <summary>
        /// Gets new job of <typeparamref name="TJob"/> using <paramref name="factoryJobGetter"/> and initializes it with all properties used in
        /// <see cref="FileSystemJobExtesions.InitializeWith{T}(T,FileSystemJobConfiguration)"/>
        /// and registers all handlers using <see cref="FileSystemJobExtesions.SetLogProgressHandler{T}(T, EventHandler{LogProgressEventArgs})"/>
        /// of the job calling this method.
        /// </summary>
        /// <typeparam name="TJob">Type of job that will be created.</typeparam>
        /// <param name="factoryJobGetter"><see cref="FileSystemJobFactory{TJobFactory, TJob}"/> method creating a new job.</param>
        /// <param name="typeInfo"><see cref="ObjectTypeInfo"/> to get the job's factory job for.</param>
        protected TJob GetNewJobWithSharedResourcesAndHandlers<TJob>(Func<ObjectTypeInfo, FileSystemRepositoryConfiguration, TJob> factoryJobGetter, ObjectTypeInfo typeInfo)
            where TJob : AbstractFileSystemProgressLoggingJob
        {
            return GetNewJobWithSharedResourcesAndHandlers(_ => factoryJobGetter(typeInfo, RepositoryConfiguration));
        }


        /// <summary>
        /// Creates new job using provided <paramref name="jobCreator"/> and initializes it with all properties used in
        /// <see cref="FileSystemJobExtesions.InitializeWith{T}(T,FileSystemJobConfiguration)"/>
        /// and registers all handlers using <see cref="FileSystemJobExtesions.SetLogProgressHandler{T}(T, EventHandler{LogProgressEventArgs})"/>
        /// of the job calling this method.
        /// </summary>
        /// <typeparam name="TJob">Type of job that will be created.</typeparam>
        /// <param name="jobCreator">Method creating new job from provided configuration.</param>
        internal TJob GetNewJobWithSharedResourcesAndHandlers<TJob>(Func<FileSystemRepositoryConfiguration, TJob> jobCreator)
            where TJob : AbstractFileSystemProgressLoggingJob
        {
            return GetNewJobWithSharedResources(jobCreator)
                .SetLogProgressHandler((sender, e) => LogProgress?.Invoke(sender, e));
        }

        #endregion
    }
}
