using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.ContinuousIntegration.Internal
{
    /// <summary>
    /// Restores all objects specified by constructor parameters to the database.
    /// </summary>
    /// <remarks>
    /// Instances of this class do not support reusing (i.e. <see cref="AbstractFileSystemAllJob.Run"/> can be called at most once in a lifetime of the class' instance.).
    /// <para>Instance operation (job) can be canceled using given <see cref="CancellationToken"/> provided to the <see cref="AbstractFileSystemAllJob.Run"/> at any time.
    /// <para>Instance operation (job) terminates as soon as cancellation request is detected.</para>
    /// </para>
    /// </remarks>
    internal class FileSystemRestoreAllJob : AbstractFileSystemAllJob
    {
        #region "Properties"

        private MemoryCache<FileSystemRestoreObjectsByTypeInternalJob> mTypeWideJobs;


        /// <summary>
        /// Cache containing jobs for individual object types that are first processed for
        /// insert/update and later on processed in opposite order for deletion.
        /// </summary>
        protected MemoryCache<FileSystemRestoreObjectsByTypeInternalJob> RestoreObjectsByTypeInternalJobs
        {
            get
            {
                return mTypeWideJobs ?? (mTypeWideJobs = new MemoryCache<FileSystemRestoreObjectsByTypeInternalJob>());
            }
            set
            {
                mTypeWideJobs = value;
            }
        }


        /// <summary>
        /// Returns message that is logged when operation was canceled using <see cref="CancellationToken"/>.
        /// </summary>
        protected override string OperationCancelledMessage
        {
            get
            {
                return ResHelper.GetString("ci.deserialization.canceled");
            }
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Creates a new instance of FileSystemRestoreAllJob that restores all objects of given object types to the database.
        /// </summary>
        /// <param name="configuration">Repository configuration.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> is not provided.</exception>
        public FileSystemRestoreAllJob(FileSystemRepositoryConfiguration configuration)
            : base(configuration)
        {
        }


        #endregion


        #region "Protected methods"


        /// <summary>
        /// Restores all objects specified by constructor parameters to the database. 
        /// </summary>
        /// <param name="cancellationToken">Operation can be canceled at any time using given cancellation token. This method's operation terminates as soon as cancellation request is detected.</param>
        /// <exception cref="OperationCanceledException">Thrown when operation was canceled.</exception>
        /// <remarks>
        /// Target location on the file system will be cleaned up before serialization.
        /// <para>This operation can be canceled using given <paramref name="cancellationToken"/> at any time.</para>
        /// <para>This method's operation terminates as soon as cancellation request is detected.</para>
        /// </remarks>
        protected override void RunInternal(CancellationToken cancellationToken)
        {
            using (new RepositoryActionContext { IsRestoreOperationRunning = true })
            {
                LogRestoreOperationRun();

                if (UseFileMetadata)
                {
                    FileSystemWriter.RepositoryHashManager.LoadFilesMetadataFromDatabase();
                }

                try
                {
                    var orderedObjectTypes = GetOrderedObjectTypes();

                    CreateAndUpsertObjects(orderedObjectTypes, cancellationToken);
                    CreateAndUpsertNewCustomTableItemObjects(orderedObjectTypes, cancellationToken);
                    DeleteAndDisposeObjects(orderedObjectTypes, cancellationToken);

                    ContentStagingTaskCollection.RaiseTasksCollected();
                }
                finally
                {
                    // In case an exception was thrown, dispose all jobs that were cached, but not executed in both for-each loops above (they are not disposed yet)
                    foreach(var typeWideJob in RestoreObjectsByTypeInternalJobs.GetItems().Select(item => item.Value).Where(job => job != null))
                    {
                        typeWideJob.Dispose();
                    }
                }

                if (UseFileMetadata)
                {
                    UpdateFilesMetadata();
                }
            }
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Returns sequence of object types ordered by their dependencies.
        /// </summary>
        private IEnumerable<string> GetOrderedObjectTypes()
        {
            var availableObjectTypes = GetAvailableObjectTypes();
            var objectTypeAnalyzer = new ObjectTypeSequenceAnalyzer(new EnumerationObjectTypeFilter(RepositoryConfiguration.ObjectTypes), availableObjectTypes);

            return objectTypeAnalyzer
                .GetSequence()
                .Select(objectType => objectType.ObjectType)
                .Distinct();
        }


        /// <summary>
        /// Returns all available object types including possible dynamic ones
        /// </summary>
        private IEnumerable<string> GetAvailableObjectTypes()
        {
            // dynamic object types are not available in all object types, we have to add them manually
            return ObjectTypeManager.AllObjectTypes.Union(RepositoryConfiguration.ObjectTypes);
        }


        /// <summary>
        /// Executes <see cref="CreateAndUpsert(string, CancellationToken)"/> for each object type in given <paramref name="orderedObjectTypes"/>.
        /// </summary>
        private void CreateAndUpsertObjects(IEnumerable<string> orderedObjectTypes, CancellationToken cancellationToken)
        {
            CancellableForEach(cancellationToken, orderedObjectTypes, objectType =>
            {
                TryRun(
                    CreateAndUpsert,
                    objectType,
                    cancellationToken,
                    "Restoration of object type \"{0}\" failed during objects' insertion and update. See inner exception for further details.");
            });
        }


        /// <summary>
        /// Executes <see cref="CreateAndUpsert(string, CancellationToken)"/> for new custom table items which type is new in this restore operation
        /// and was not already processed as part of <paramref name="processedObjectTypes"/>.
        /// </summary>
        private void CreateAndUpsertNewCustomTableItemObjects(IEnumerable<string> processedObjectTypes, CancellationToken cancellationToken)
        {
            RepositoryConfiguration.ReloadCustomTableObjectTypes();
            var newCustomTableObjectTypes = RepositoryConfiguration.ObjectTypes.Except(processedObjectTypes);

            CreateAndUpsertObjects(newCustomTableObjectTypes, cancellationToken);
        }


        /// <summary>
        /// Executes <see cref="DeleteAndDispose(string, CancellationToken)"/> for each object type in given <paramref name="orderedObjectTypes"/>.
        /// </summary>
        private void DeleteAndDisposeObjects(IEnumerable<string> orderedObjectTypes, CancellationToken cancellationToken)
        {
            var reverseOrderedObjectTypes = orderedObjectTypes.Reverse();

            CancellableForEach(cancellationToken, reverseOrderedObjectTypes, objectType =>
            {
                TryRun(
                    DeleteAndDispose,
                    objectType,
                    cancellationToken,
                    "Restoration of object type \"{0}\" failed during objects' deletion. See inner exception for further details.");
            });
        }


        /// <summary>
        /// Logs restore operation run to module usage tracking counter
        /// </summary>
        private void LogRestoreOperationRun()
        {
            ObjectFactory<IModuleUsageCounter>.New().Increment("ContinuousIntegrationRestoreRuns");
        }


        /// <summary>
        /// Updates FileMetadataInfos in the DB based on data collected in <see cref="RepositoryHashManager"/>.
        /// </summary>
        private void UpdateFilesMetadata()
        {
            RaiseLogProgress(ResHelper.GetString("ci.serialization.optimizingrepository"));

            FileSystemWriter.RepositoryHashManager.UpdateFilesMetadataInDatabase();
        }


        /// <summary>
        /// Creates new instance of a <see cref="FileSystemRestoreObjectsByTypeInternalJob"/> job dedicated to specified <paramref name="objectType"/>. 
        /// </summary>
        /// <remarks>
        /// Job is provided with <paramref name="cancellationToken"/> that is owned by the <see cref="FileSystemRestoreAllJob"/>.
        /// <para>New job is also provided with all resources and handlers the current job posses, but for <see cref="FileSystemBindingsProcessor"/>.</para>
        /// </remarks>
        private FileSystemRestoreObjectsByTypeInternalJob CreateNewRestoreInternalJob(string objectType, CancellationToken cancellationToken)
        {
            var fileSystemJobConfiguration = new FileSystemJobConfiguration
            {
                RepositoryPathHelper = RepositoryPathHelper,
                TranslationHelper = TranslationHelper,
                FileSystemWriter = FileSystemWriter,
                UseFileMetadata = UseFileMetadata,
                ContentStagingTaskCollection = ContentStagingTaskCollection,
                SkipLicenseChecking = true
            };

            return new FileSystemRestoreObjectsByTypeInternalJob(RepositoryConfiguration, objectType, cancellationToken)
                .InitializeWith(fileSystemJobConfiguration)
                .SetLogProgressHandler((sender, args) => RaiseLogProgress(args.LogItem));
        }


        /// <summary>
        /// Creates <see cref="FileSystemRestoreObjectsByTypeInternalJob"/> job dedicated to specified <paramref name="objectType"/>
        /// and executes its <see cref="FileSystemRestoreObjectsByTypeInternalJob.RunUpsertObjectsByTypeJob"/> method.
        /// </summary>
        /// <remarks>Job is provided with <paramref name="cancellationToken"/> that is owned by the <see cref="FileSystemRestoreAllJob"/>.</remarks>
        private void CreateAndUpsert(string objectType, CancellationToken cancellationToken)
        {
            var job = RestoreObjectsByTypeInternalJobs.FetchItem(objectType, () => CreateNewRestoreInternalJob(objectType, cancellationToken));

            job.RunUpsertObjectsByTypeJob();
        }
        

        /// <summary>
        /// Gets instance of <see cref="FileSystemRestoreObjectsByTypeInternalJob"/> job dedicated to specified <paramref name="objectType"/>
        /// from <see cref="RestoreObjectsByTypeInternalJobs"/> and executes its  <see cref="FileSystemRestoreObjectsByTypeInternalJob.DeleteObjectsByTypeJob"/> method.
        /// The job is then disposed in order to dispose its <see cref="CachedFileSystemBindingsProcessor"/>; job is also removed from the cache.
        /// </summary>
        private void DeleteAndDispose(string objectType, CancellationToken cancellationToken)
        {
            using (var job = RestoreObjectsByTypeInternalJobs.RemoveItem(objectType))
            {
                job.RunDeleteObjectsByTypeJob();
            }
        }
        

        /// <summary>
        /// Tries run provided <paramref name="method"/>. Method is given both <paramref name="objectType"/> and <paramref name="cancellationToken"/> parameters.
        /// <para>
        /// If method throws <see cref="OperationCanceledException"/>, it is re-thrown;
        /// any other exception is wrapped into new <see cref="ObjectTypeSerializationException"/> provided with <paramref name="exceptionMessageFormat"/>.
        /// </para>
        /// </summary>
        /// <param name="method"></param>
        /// <param name="objectType"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="exceptionMessageFormat"></param>
        private void TryRun(Action<string, CancellationToken> method, string objectType, CancellationToken cancellationToken, string exceptionMessageFormat)
        {
            try
            {
                method(objectType, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // Cancellation exception is handled by Run method of the AbstractFileSystemAllJob
                throw;
            }
            catch (Exception exception)
            {
                var niceObjectTypeName = TypeHelper.GetNiceObjectTypeName(objectType);
                throw new ObjectTypeSerializationException(
                    String.Format(exceptionMessageFormat, niceObjectTypeName),
                    exception,
                    niceObjectTypeName);
            }
        }

        #endregion
    }
}
