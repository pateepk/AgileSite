using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using CMS.Base;
using CMS.DataEngine;
using CMS.IO;
using CMS.Helpers;

namespace CMS.ContinuousIntegration.Internal
{
    /// <summary>
    /// Stores all objects specified by constructor parameters to the file system.
    /// </summary>
    /// <remarks>
    /// Instances of this class do not support reusing (i.e. <see cref="AbstractFileSystemAllJob.Run"/> can be called at most once in a lifetime of the class' instance.).
    /// <para>Instance operation (job) can be canceled using given <see cref="CancellationToken"/> provided to the <see cref="AbstractFileSystemAllJob.Run"/> at any time.
    /// <para>Instance operation (job) terminates as soon as cancellation request is detected.</para>
    /// </para>
    /// </remarks>
    internal class FileSystemStoreAllJob : AbstractFileSystemAllJob
    {
        #region "Properties and variables"

        // Folder name where migrations (SQL scripts with DB changes which are not covered with CI) are stored
        private const string MIGRATIONS_FOLDER = "@Migrations";

        // Stores localized/customized string stating "Object type"
        private string mObjectTypeString;

        // Stores cancellation token provided to the job
        private CancellationToken mCancellationToken;


        /// <summary>
        /// Returns message that is logged when operation was canceled using <see cref="CancellationToken"/>.
        /// </summary>
        protected override string OperationCancelledMessage
        {
            get
            {
                return ResHelper.GetString("ci.serialization.canceled");
            }
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Creates a new instance of FileSystemStoreAllJob that stores all objects of given object types to the file system.
        /// </summary>
        /// <param name="configuration">Repository configuration.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> is not provided.</exception>
        public FileSystemStoreAllJob(FileSystemRepositoryConfiguration configuration)
            : base(configuration)
        {
        }

        #endregion


        #region "Protected methods"

        /// <summary>
        /// Stores all objects specified by constructor parameters to the file system. This method can be called only once
        /// in a lifetime of the class' instance.
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
            mCancellationToken = cancellationToken;
            mObjectTypeString = ResHelper.GetString("general.objecttype");

            // Clear hashes of serialization files
            RaiseLogProgress(ResHelper.GetString("ci.serialization.deletingexistingdata"));
            ClearFileHashes();

            DeleteRepositoryContent();

            StoreObjects();
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Stores all objects specified by its object type(s) to the file system. 
        /// </summary>
        internal void StoreObjects()
        {
            if (RepositoryConfiguration.ObjectTypes == null)
            {
                // No object types to store provided
                return;
            }

            FileSystemWriter.RepositoryHashManager.Clear();

            int total = RepositoryConfiguration.ObjectTypes.Count;
            CancellableForEach(mCancellationToken, RepositoryConfiguration.ObjectTypes, (objectType, counter) =>
            {
                StoreAllObjectTypeObjects(objectType, counter, total);
            });

            if (UseFileMetadata)
            {
                SaveFileHashes();
            }
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Stores all objects of given <paramref name="objectType"/> to the repository.
        /// <see cref="FileSystemStoreJob"/> registered for the type is retrieved from <see cref="FileSystemStoreJobFactory"/>.
        /// </summary>
        /// <remarks>Not-mentioned parameters are present for logging purposes.</remarks>
        private void StoreAllObjectTypeObjects(string objectType, int counter, int total)
        {
            ObjectTypeInfo typeInfo;
            try
            {
                typeInfo = ObjectTypeManager.GetTypeInfo(objectType);

                using (var bindingsProcessor = new CachedFileSystemBindingsProcessor(TranslationHelper, RepositoryConfiguration, FileSystemWriter))
                {
                    var fileSystemJobConfiguration = new FileSystemJobConfiguration
                    {
                        RepositoryPathHelper = RepositoryPathHelper,
                        TranslationHelper = TranslationHelper,
                        BindingsProcessor = bindingsProcessor,
                        FileSystemWriter = FileSystemWriter,
                        SkipLicenseChecking = true
                    };

                    var storeJob = FileSystemStoreJobFactory
                        .GetJob(typeInfo, RepositoryConfiguration)
                        .InitializeWith(fileSystemJobConfiguration);

                    var query = GetObjectsForStoring(objectType);

                    if (query.Any())
                    {
                        RaiseLogProgress(String.Format(
                           "{0} {1}/{2}: {3} ...",
                           mObjectTypeString,
                           counter + 1,
                           total,
                           TypeHelper.GetNiceObjectTypeName(objectType)
                        ));
                    }

                    CancellableForEach(mCancellationToken, query, info =>
                    {
                        StoreSingleObjectTypeObject(storeJob, typeInfo, objectType, info, counter, total);
                    });
                }
            }
            catch (OperationCanceledException)
            {
                // Cancellation exception is handled by Run method of the AbstractFileSystemAllJob
                throw;
            }
            catch (ObjectTypeSerializationException)
            {
                // Exception was thrown by a single info object that failed to serialize. Such exception is already properly wrapped
                throw;
            }
            catch (Exception exception)
            {
                throw new ObjectTypeSerializationException(
                        String.Format(
                            "Objects for serialization of object type \"{0}\" ({1}) could not be retrieved. See inner exception for further details.",
                            TypeHelper.GetNiceObjectTypeName(objectType),
                            objectType),
                        exception,
                        objectType);
            }
        }


        /// <summary>
        /// Stores given <paramref name="info"/> to the repository using provided <paramref name="storeJob"/>.
        /// </summary>
        /// <remarks>Not-mentioned parameters are present for logging purposes.</remarks>
        private void StoreSingleObjectTypeObject(FileSystemStoreJob storeJob, ObjectTypeInfo typeInfo, string objectType, BaseInfo info, int counter, int total)
        {
            try
            {
                if (!RepositoryConfigurationEvaluator.IsObjectIncluded(info, RepositoryConfiguration, TranslationHelper))
                {
                    // Repository configuration prevents this very object from being serialized
                    return;
                }

                storeJob.Run(info);
                RaiseLogProgress(String.Format(
                    "{0} {1}/{2}: {3} {4}",
                    mObjectTypeString,
                    counter + 1,
                    total,
                    TypeHelper.GetNiceObjectTypeName(objectType),
                    GetLogObjectName(typeInfo, info)));
            }
            catch (Exception exception)
            {
                var typeName = typeInfo.ObjectType;
                var infoId = info.Generalized.ObjectID;
                throw new ObjectTypeSerializationException(
                    String.Format(
                        "Serialization of object type \"{0}\" ({1}) failed for object \"{2}\" ({3}: {4}). See inner exception for further details.",
                        TypeHelper.GetNiceObjectTypeName(objectType),
                        typeName,
                        GetLogObjectName(typeInfo, info),
                        typeInfo.IDColumn,
                        infoId),
                    exception,
                    typeName,
                    infoId);
            }
        }


        /// <summary>
        /// Gets enumeration of objects for given <paramref name="objectType"/>.
        /// </summary>
        /// <param name="objectType">Object type to enumerate.</param>
        /// <returns>Enumeration of objects of given object type.</returns>
        private ICollection<BaseInfo> GetObjectsForStoring(string objectType)
        {
            return DatabaseObjectsEnumeratorFactory.GetObjectEnumerator(objectType, RepositoryConfiguration).ToArray();
        }


        /// <summary>
        /// Saves hashes of serialization files.
        /// </summary>
        private void SaveFileHashes()
        {
            RaiseLogProgress(ResHelper.GetString("ci.serialization.optimizingrepository"));

            FileSystemWriter.RepositoryHashManager.UpdateFilesMetadataInDatabase();
        }


        /// <summary>
        /// Deletes all hashes of serialization files.
        /// </summary>
        private void ClearFileHashes()
        {
            FileMetadataInfoProvider.DeleteAllFileMetadataInfos();
        }


        /// <summary>
        /// Deletes all files from the repository directory, except files stored directly in the root.
        /// </summary>
        private void DeleteRepositoryContent()
        {
            if (Directory.Exists(RepositoryConfiguration.RepositoryRootPath))
            {
                foreach (var directory in Directory.GetDirectories(RepositoryConfiguration.RepositoryRootPath).Where(d => !d.EndsWith(MIGRATIONS_FOLDER, StringComparison.OrdinalIgnoreCase)))
                {
                    if (!FileSystemRepositoryHelper.DeleteDirectory(directory, true))
                    {
                        // MSDN: In some cases, if you have the specified directory open in File Explorer, the Delete method may not be able to delete it.
                        // In that case delete at least all files in it.
                        Directory.DeleteDirectoryStructure(directory);
                    }
                }
            }
        }

        #endregion
    }
}
