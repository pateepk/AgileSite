using System;
using System.Collections.Generic;
using System.Threading;

using CMS.DataEngine;
using CMS.DataEngine.CollectionExtensions;

namespace CMS.ContinuousIntegration.Internal
{
    /// <summary>
    /// Job provides ability to restore all objects of given object type present in the
    /// repository to the database, while reading each file in the repository at most once. 
    /// </summary>
    /// <remarks>
    /// Objects that are missing in the repository but can be found in the database are removed.
    /// <para>
    /// Job is internally using <see cref="FileSystemUpsertObjectsByTypeJob"/> and
    /// <see cref="FileSystemDeleteObjectsByTypeJob"/> jobs to perform each part of restore action.
    /// </para>
    /// <para>
    /// Job has no Run method as it is used in <see cref="FileSystemRestoreAllJob"/> that performs
    /// other actions between <see cref="RunUpsertObjectsByTypeJob"/> and <see cref="RunDeleteObjectsByTypeJob"/> calls.
    /// </para>
    /// </remarks>
    internal sealed class FileSystemRestoreObjectsByTypeInternalJob : AbstractFileSystemProgressLoggingJob, IDisposable
    {
        #region "Properties and variables"

        // File locations shared between delete and upsert jobs
        private ISet<RepositoryLocationsCollection> mFileLocations;

        // Delete job for given object type
        private FileSystemDeleteObjectsByTypeJob mDeleteObjectsByTypeJob;

        // Upsert job for given object type
        private FileSystemUpsertObjectsByTypeJob mUpdateObjectsByTypeJob;

        // Binding processor shared between delete and upsert jobs
        private CachedFileSystemBindingsProcessor mCachedBindingProcessor;

        // Indicates whether object was already disposed
        private bool disposed;


        /// <summary>
        /// File locations collection of all objects serialized in repository that is shared between delete and upsert jobs.
        /// </summary>
        private ISet<RepositoryLocationsCollection> FileLocations
        {
            get
            {
                return mFileLocations ?? (mFileLocations = GetRepositoryLocations(ObjectType));
            }
        }


        /// <summary>
        /// Object type the <see cref="FileSystemRestoreObjectsByTypeInternalJob"/> is dedicated to.
        /// </summary>
        internal string ObjectType
        {
            get;
            private set;
        }


        /// <summary>
        /// <see cref="ObjectTypeInfo"/> of <see cref="ObjectType"/>.
        /// </summary>
        internal ObjectTypeInfo TypeInfo
        {
            get;
            private set;
        }


        /// <summary>
        /// Cancellation token that was provided to the <see cref="FileSystemRestoreObjectsByTypeInternalJob"/> and will be shard between delete and upsert jobs.
        /// </summary>
        internal CancellationToken CancellationToken
        {
            get;
            private set;
        }


        /// <summary>
        /// <see cref="FileSystemUpsertObjectsByTypeJob"/> that is dedicated to the <see cref="ObjectType"/> and cares about insertion or update of objects serialized in repository.
        /// </summary>
        private FileSystemUpsertObjectsByTypeJob UpsertObjectsByTypeJob
        {
            get
            {
                return mUpdateObjectsByTypeJob ?? (mUpdateObjectsByTypeJob = GetNewJobWithSharedResourcesAndHandlers(FileSystemUpsertObjectsByTypeJobFactory.GetJob, TypeInfo));
            }
        }


        /// <summary>
        /// <see cref="FileSystemDeleteObjectsByTypeJob"/> that is dedicated to the <see cref="ObjectType"/> and cares about deletion of objects not-serialized in repository.
        /// </summary>
        private FileSystemDeleteObjectsByTypeJob DeleteObjectsByTypeJob
        {
            get
            {
                return mDeleteObjectsByTypeJob ?? (mDeleteObjectsByTypeJob = GetNewJobWithSharedResourcesAndHandlers(FileSystemDeleteObjectsByTypeJobFactory.GetJob, TypeInfo));
            }
        }


        /// <summary>
        /// Cached binding processor that is shared by <see cref="UpsertObjectsByTypeJob"/> and <see cref="DeleteObjectsByTypeJob"/>. Processor is disposed with this job.
        /// </summary>
        internal override FileSystemBindingsProcessor BindingsProcessor
        {
            get
            {
                return mCachedBindingProcessor ?? (mCachedBindingProcessor = new CachedFileSystemBindingsProcessor(TranslationHelper, RepositoryConfiguration, FileSystemWriter));
            }
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Creates new instance of the <see cref="FileSystemRestoreObjectsByTypeInternalJob"/>.
        /// </summary>
        /// <param name="configuration">File system repository configuration provided by executing job.</param>
        /// <param name="objectType">Object type the new job will be dedicated to (so as incorporated <see cref="FileSystemUpsertObjectsByTypeJob"/> and <see cref="FileSystemDeleteObjectsByTypeJob"/>).</param>
        /// <param name="cancellationToken">Token providing ability to cancel the job (so as incorporated <see cref="FileSystemUpsertObjectsByTypeJob"/> and <see cref="FileSystemDeleteObjectsByTypeJob"/>).</param>
        public FileSystemRestoreObjectsByTypeInternalJob(FileSystemRepositoryConfiguration configuration, string objectType, CancellationToken? cancellationToken)
            : base(configuration)
        {
            ObjectType = objectType;
            TypeInfo = ObjectTypeManager.GetTypeInfo(ObjectType);
            CancellationToken = cancellationToken ?? new CancellationToken();
        }


        /// <summary>
        /// Inserts all objects of given <see cref="ObjectType"/> present in repository to the database. If object already exists, it is updated. 
        /// </summary>
        public void RunUpsertObjectsByTypeJob()
        {
            UpsertObjectsByTypeJob.Run(ObjectType, FileLocations, CancellationToken);
        }


        /// <summary>
        /// Deletes objects of given <see cref="ObjectType"/>  type missing in the repository but present in the database.
        /// </summary>
        public void RunDeleteObjectsByTypeJob()
        {
            DeleteObjectsByTypeJob.Run(ObjectType, FileLocations, CancellationToken);
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Gets set of all repository locations grouped by serialized object.
        /// </summary>
        /// <param name="objectType">Locations of given object type are returned.</param>
        /// <returns>Set of all repository locations grouped by serialized object.</returns>
        private ISet<RepositoryLocationsCollection> GetRepositoryLocations(string objectType)
        {
            return RepositoryPathHelper.GetExistingSerializationFiles(objectType).ToHashSetCollection();
        }


        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources. <see cref="CachedFileSystemBindingsProcessor"/> in this case.
        /// </summary>
        public void Dispose()
        {
            if (disposed)
            {
                // Object was already disposed.
                return;
            }

            mCachedBindingProcessor.Dispose();

            disposed = true;
        }

        #endregion
    }
}
