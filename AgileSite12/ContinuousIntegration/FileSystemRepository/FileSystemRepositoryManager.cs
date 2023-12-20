using System;
using System.Collections.Generic;
using System.Threading;

using CMS.ContinuousIntegration.Internal;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;

namespace CMS.ContinuousIntegration
{
    /// <summary>
    /// Manages file system repository for continuous integration - stores and restores supported objects to and from the repository. Ensures mutual exclusion
    /// of the repository access across processes.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class is responsible for the synchronization of objects de/serialization to the repository.
    /// </para>
    /// <para>
    /// Members of this class are thread-safe.
    /// </para>
    /// </remarks>
    public class FileSystemRepositoryManager : IDisposable
    {
        #region "Variables"

        private bool disposed;

        /// <summary>
        /// Singleton instance of <see cref="FileSystemRepositoryManager"/>.
        /// </summary>
        internal static FileSystemRepositoryManager currentInstance;


        /// <summary>
        /// Lock over initialization of the singleton ensures that only one instance of <see cref="FileSystemRepositoryManager"/> will be created.
        /// </summary>
        private static readonly object instanceLock = new object();


        /// <summary>
        /// Global mutex over the re/storing of objects process that prevents running multiple object serialization or deserialization at one time (in any process).
        /// </summary>
        private readonly Mutex serializeMutex;


        /// <summary>
        /// Global mutex over the storing of all objects process. The storing of all objects is considered a long running operation.
        /// The mutual exclusion is ensured across processes.
        /// </summary>
        private readonly Mutex storeAllMutex;


        /// <summary>
        /// Global mutex over the restoring of all objects process. The restoring of all objects is considered a long running operation.
        /// The mutual exclusion is ensured across processes.
        /// </summary>
        private readonly Mutex restoreAllMutex;


        /// <summary>
        /// Store all job for internal use. Serves for all objects storing. Each run of all objects storing creates
        /// a new instance. Overlapping is not possible.
        /// </summary>
        internal FileSystemStoreAllJob storeAllJob;


        /// <summary>
        /// Restore job for internal use. Serves for all objects restoring. Each run of all objects restoring creates
        /// a new instance. Overlapping is not possible.
        /// </summary>
        internal FileSystemRestoreAllJob restoreAllJob;


        private readonly IFileSystemRepositoryConfigurationBuilder mConfigurationBuilder;
        private FileSystemRepositoryConfiguration mConfiguration;

        #endregion


        #region "Properties"


        /// <summary>
        /// Gets the cached instance of file system repository configuration.
        /// </summary>
        /// <remarks>
        /// This configuration object can be different than the actually saved. Use <see cref="CurrentConfiguration"/> to get the latest configuration.
        /// </remarks>
        internal FileSystemRepositoryConfiguration CachedConfiguration
        {
            get
            {
                return mConfiguration;
            }
        }


        /// <summary>
        /// Gets the current instance of file system repository configuration.
        /// </summary>
        private FileSystemRepositoryConfiguration CurrentConfiguration
        {
            get
            {
                RebuildConfiguration();
                return mConfiguration;
            }
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Returns instance of <see cref="FileSystemRepositoryManager"/>.
        /// </summary>
        /// <remarks>
        /// This class is a singleton. Although it is disposable, there is no need
        /// for explicit disposal.
        /// </remarks>
        /// <exception cref="RepositoryConfigurationException">Thrown when loading of repository configuration fails.</exception>
        internal static FileSystemRepositoryManager GetInstance()
        {
            if (currentInstance == null)
            {
                lock (instanceLock)
                {
                    if (currentInstance == null)
                    {
                        var manager = new FileSystemRepositoryManager();

                        Thread.MemoryBarrier();

                        currentInstance = manager;
                    }
                }
            }

            return currentInstance;
        }


        /// <summary>
        /// Restores all supported objects from the file system repository. 
        /// </summary>
        /// <param name="messageHandler">Handler with messages from restore process.</param>
        /// <param name="cancellationToken">Operation can be canceled at any time using given cancellation token. This method's operation terminates as soon as cancellation request is detected.</param>
        /// <returns>Returns result object of the restoring process. Successful restoring is indicating by <see cref="RepositoryActionResult.Success"/> flag.</returns>
        /// <remarks>
        /// Storing of all and storing of individual objects (<see cref="Store(BaseInfo)"/>, <see cref="Delete(BaseInfo)"/>) is disabled when restoring of all objects is running.
        /// </remarks>
        /// <exception cref="RepositoryConfigurationException">Thrown when loading of repository configuration fails.</exception>
        public static RepositoryActionResult RestoreAll(Action<LogItem> messageHandler = null, CancellationToken? cancellationToken = null)
        {
            ThrowIfRestoreAllDisabled();
            return GetInstance().RestoreAllInternal(messageHandler, cancellationToken);
        }


        /// <summary>
        /// Restores all supported objects from the file system repository. 
        /// </summary>
        /// <param name="messageHandler">Handler with messages from restore process.</param>
        /// <param name="cancellationToken">Operation can be canceled at any time using given cancellation token. This method's operation terminates as soon as cancellation request is detected.</param>
        /// <returns>Returns result object of the restoring process. Successful restoring is indicating by <see cref="RepositoryActionResult.Success"/> flag.</returns>
        /// <remarks>
        /// Storing of all and storing of individual objects (<see cref="Store(BaseInfo)"/>, <see cref="Delete(BaseInfo)"/>) is disabled when restoring of all objects is running.
        /// </remarks>
        internal virtual RepositoryActionResult RestoreAllInternal(Action<LogItem> messageHandler = null, CancellationToken? cancellationToken = null)
        {
            var actionResult = new RepositoryActionResult();

            bool restoreAllMutexOwnership = false;
            try
            {
                bool serializationMutexOwnership = serializeMutex.WaitOne();
                try
                {
                    if (IsRestoreAllRunningGlobally(true, out restoreAllMutexOwnership) || IsStoreAllRunningGlobally(false, false, out _))
                    {
                        return actionResult.LogError(ResHelper.GetString("ci.serialization.running"));
                    }
                }
                finally
                {
                    if (serializationMutexOwnership)
                    {
                        serializeMutex.ReleaseMutex();
                    }
                }

                restoreAllJob = new FileSystemRestoreAllJob(CurrentConfiguration)
                    .SetLogProgressHandler((sender, e) =>
                    {
                        actionResult.Log(e.LogItem);
                        messageHandler?.Invoke(e.LogItem);
                    });

                RunRestoreAllJob(cancellationToken);
            }
            catch (Exception ex)
            {
                actionResult.LogError(EventLogProvider.GetExceptionLogMessage(ex));
            }
            finally
            {
                if (restoreAllMutexOwnership)
                {
                    restoreAllMutex.ReleaseMutex();
                }
            }

            return actionResult;
        }


        /// <summary>
        /// Stores all supported objects to the file system repository. 
        /// </summary>
        /// <param name="messageHandler">Handler with messages from store process.</param>
        /// <param name="cancellationToken">Operation can be canceled at any time using given cancellation token. This method's operation terminates as soon as cancellation request is detected.</param>
        /// <returns>Returns result object of the restoring process. Successful restoring is indicating by <see cref="RepositoryActionResult.Success"/> flag.</returns>
        /// <remarks>
        /// <para>
        /// Target location on the file system will be cleaned up before storing.
        /// </para>
        /// <para>
        /// Restoring of all is disabled when storing of all objects is running.
        /// </para>
        /// </remarks>
        /// <exception cref="RepositoryConfigurationException">Thrown when loading of repository configuration fails.</exception>
        public static RepositoryActionResult StoreAll(Action<LogItem> messageHandler = null, CancellationToken? cancellationToken = null)
        {
            return GetInstance().StoreAllInternal(messageHandler, cancellationToken);
        }


        /// <summary>
        /// Stores all supported objects to the file system repository. 
        /// </summary>
        /// <param name="messageHandler">Handler with messages from store process.</param>
        /// <param name="cancellationToken">Operation can be canceled at any time using given cancellation token. This method's operation terminates as soon as cancellation request is detected.</param>
        /// <returns>Returns result object of the restoring process. Successful restoring is indicating by <see cref="RepositoryActionResult.Success"/> flag.</returns>
        /// <remarks>
        /// <para>
        /// Target location on the file system will be cleaned up before storing.
        /// </para>
        /// <para>
        /// Restoring of all is disabled when storing of all objects is running.
        /// </para>
        /// </remarks>
        internal virtual RepositoryActionResult StoreAllInternal(Action<LogItem> messageHandler = null, CancellationToken? cancellationToken = null)
        {
            var actionResult = new RepositoryActionResult();

            bool storeAllMutexOwnership = false;
            try
            {
                bool serializationMutexOwnership = serializeMutex.WaitOne();
                try
                {
                    if (IsStoreAllRunningGlobally(false, true, out storeAllMutexOwnership) || IsRestoreAllRunningGlobally(false, out _))
                    {
                        return actionResult.LogError(ResHelper.GetString("ci.serialization.running"));
                    }
                }
                finally
                {
                    if (serializationMutexOwnership)
                    {
                        serializeMutex.ReleaseMutex();
                    }
                }

                storeAllJob = new FileSystemStoreAllJob(CurrentConfiguration)
                    .SetLogProgressHandler((sender, e) =>
                    {
                        actionResult.Log(e.LogItem);

                        messageHandler?.Invoke(e.LogItem);
                    });

                RunStoreAllJob(cancellationToken);
            }
            catch (Exception ex)
            {
                actionResult.LogError(EventLogProvider.GetExceptionLogMessage(ex));
            }
            finally
            {
                if (storeAllMutexOwnership)
                {
                    storeAllMutex.ReleaseMutex();
                }
            }

            return actionResult;
        }


        /// <summary>
        /// Stores given object to the file system repository, if this object's serialization is enabled by the repository configuration.
        /// </summary>
        /// <returns>Returns true if the object has been stored, false otherwise (i.e. object type is not to be included in the repository).</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="info"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when <see cref="RestoreAll"/> is running (in current or any other process), thus preventing the repository access.</exception>
        /// <exception cref="RepositoryConfigurationException">Thrown when loading of repository configuration fails.</exception>
        public static bool Store(BaseInfo info)
        {
            return GetInstance().StoreInternal(info);
        }


        /// <summary>
        /// Stores given objects to the file system repository, if this object's serialization is enabled by the repository configuration.
        /// </summary>
        /// <param name="typeInfo">Type info object that describes objects in <paramref name="infoObjects"/> collection.</param>
        /// <param name="infoObjects">Objects to store. All objects must be of same type as <paramref name="typeInfo"/>.</param>
        /// <param name="translationHelper">Translation helper objects that caches translation data between multiple method calls.</param>
        /// <returns>Returns true if the object has been stored, false otherwise (i.e. object type is not to be included in the repository or collection was empty).</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="infoObjects"/> contains objects with different object types.</exception>
        /// <exception cref="InvalidOperationException">Thrown when <see cref="RestoreAll"/> is running (in current or any other process), thus preventing the repository access.</exception>
        /// <exception cref="RepositoryConfigurationException">Thrown when loading of repository configuration fails.</exception>
        internal static bool Store(ObjectTypeInfo typeInfo, IEnumerable<BaseInfo> infoObjects, ContinuousIntegrationTranslationHelper translationHelper = null)
        {
            var instance = GetInstance();
            return instance.SerializeInternal(instance.RunStoreJob, typeInfo, infoObjects, translationHelper);
        }


        /// <summary>
        /// Stores given object to the file system repository, if this object's serialization is enabled by the repository configuration.
        /// </summary>
        /// <returns>Returns true if the object has been stored, false otherwise (i.e. object type is not to be included in the repository).</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="info"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when <see cref="RestoreAllInternal"/> is running (in current or any other process), thus preventing the repository access.</exception>
        internal bool StoreInternal(BaseInfo info)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info", "Info object cannot be null.");
            }

            try
            {
                return SerializeInternal(RunStoreJob, info.TypeInfo, new[] { info });
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException($"Can not store object '{info}' when restoring of all objects is running.", ex);
            }

        }


        /// <summary>
        /// Deletes given object from the file system repository, if this object's serialization is enabled by the repository configuration.
        /// </summary>
        /// <returns>Returns true if the object has been removed from the repository, false otherwise (i.e. object type is not to be included in the repository).</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="info"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when <see cref="RestoreAllInternal"/> is running (in current or any other process), thus preventing the repository access.</exception>
        /// <exception cref="RepositoryConfigurationException">Thrown when loading of repository configuration fails.</exception>
        public static bool Delete(BaseInfo info)
        {
            return GetInstance().DeleteInternal(info);
        }


        /// <summary>
        /// Deletes given objects from the file system repository, if this object's serialization is enabled by the repository configuration.
        /// </summary>
        /// <param name="typeInfo">Type info object that describes objects in <paramref name="infoObjects"/> collection.</param>
        /// <param name="infoObjects">Objects to store. All objects must be of same type as <paramref name="typeInfo"/>.</param>
        /// <param name="translationHelper">Translation helper objects that caches translation data between multiple method calls.</param>
        /// <returns>Returns true if the object has been removed from the repository, false otherwise (i.e. object type is not to be included in the repository or collection was empty).</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="infoObjects"/> contains objects with different object types.</exception>
        /// <exception cref="InvalidOperationException">Thrown when <see cref="RestoreAllInternal"/> is running (in current or any other process), thus preventing the repository access.</exception>
        /// <exception cref="RepositoryConfigurationException">Thrown when loading of repository configuration fails.</exception>
        internal static bool Delete(ObjectTypeInfo typeInfo, IEnumerable<BaseInfo> infoObjects, ContinuousIntegrationTranslationHelper translationHelper = null)
        {
            var instance = GetInstance();
            return instance.SerializeInternal(instance.RunDeleteJob, typeInfo, infoObjects, translationHelper);
        }


        /// <summary>
        /// Deletes given object from the file system repository, if this object's serialization is enabled by the repository configuration.
        /// </summary>
        /// <returns>Returns true if the object has been removed from the repository, false otherwise (i.e. object type is not to be included in the repository).</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="info"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when <see cref="RestoreAllInternal"/> is running (in current or any other process), thus preventing the repository access.</exception>
        internal bool DeleteInternal(BaseInfo info)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info", "Info object cannot be null.");
            }

            try
            {
                return SerializeInternal(RunDeleteJob, info.TypeInfo, new[] { info });
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException($"Can not delete object '{info}' when restoring of all objects is running.", ex);
            }
        }


        /// <summary>
        /// Serializes given objects to/from the file system repository, if this object's serialization is enabled by the repository configuration.
        /// </summary>
        /// <param name="serializeAction">Serialization action to be called</param>
        /// <param name="typeInfo">Type info object that describes objects in <paramref name="infoObjects"/> collection.</param>
        /// <param name="infoObjects">Objects to store. All objects must be of same type as <paramref name="typeInfo"/>.</param>
        /// <param name="translationHelper">Translation helper objects that caches translation data between multiple method calls.</param>
        /// <returns>Returns true if the object has been serialized to/from the repository, false otherwise (i.e. object type is not to be included in the repository or collection was empty).</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="infoObjects"/> contains objects with different object types.</exception>
        /// <exception cref="InvalidOperationException">Thrown when <see cref="RestoreAllInternal"/> is running (in current or any other process), thus preventing the repository access.</exception>
        internal bool SerializeInternal(Action<ObjectTypeInfo, IEnumerable<BaseInfo>, FileSystemRepositoryConfiguration, ContinuousIntegrationTranslationHelper> serializeAction, ObjectTypeInfo typeInfo, IEnumerable<BaseInfo> infoObjects, ContinuousIntegrationTranslationHelper translationHelper = null)
        {
            var configuration = CachedConfiguration;
            if (!RepositoryConfigurationEvaluator.IsObjectTypeIncluded(typeInfo.ObjectType, configuration))
            {
                return false;
            }

            bool lockAcquired = false;
            try
            {
                lockAcquired = serializeMutex.WaitOne();

                if (IsRestoreAllRunningGlobally(false, out _))
                {
                    throw new InvalidOperationException($"Can not serialize objects of type '{typeInfo.ObjectType}' when restoring of all objects is running.");
                }

                if (!IsStoreAllRunningGlobally(true, false, out _))
                {
                    serializeAction(typeInfo, infoObjects, configuration, translationHelper);
                    return true;
                }
            }
            finally
            {
                if (lockAcquired)
                {
                    serializeMutex.ReleaseMutex();
                }
            }

            return false;
        }


        /// <summary>
        /// Rebuilds CI configuration based on <see cref="FileSystemRepositoryConfigurationBuilder"/>.
        /// Should be called immediately after any <see cref="DataClassInfo"/> is changed.
        /// </summary>
        /// <remarks>
        /// Method is called automatically after any <see cref="DataClassInfo"/> is inserted or deleted.
        /// Method is called automatically when any Custom Table field definition, namespace or code name is changed.
        /// </remarks>
        internal void RebuildConfiguration()
        {
            mConfiguration = mConfigurationBuilder.Build();
        }

        #endregion


        #region "Protected methods"

        /// <summary>
        /// Stores all supported objects to the file system repository. 
        /// </summary>
        /// <param name="cancellationToken">Operation can be canceled at any time using given cancellation token. This method's operation terminates as soon as cancellation request is detected.</param>
        /// <remarks>
        /// <para>
        /// Target location on the file system will be cleaned up before storing.
        /// </para>
        /// <para>
        /// This method is called from synchronized context. Overrides to this method do not need to synchronize against this class' public member calls.
        /// </para>
        /// <para>
        /// This member is virtual for the purpose of testing only.
        /// </para>
        /// </remarks>
        internal virtual void RunStoreAllJob(CancellationToken? cancellationToken = null)
        {
            storeAllJob.Run(cancellationToken);
        }


        /// <summary>
        /// Restores all supported objects from the file system repository.
        /// </summary>
        /// <param name="cancellationToken">Operation can be canceled at any time using given cancellation token. This method's operation terminates as soon as cancellation request is detected.</param>
        /// <remarks>
        /// <para>
        /// This method is called from synchronized context. Overrides to this method do not need to synchronize against this class' public member calls.
        /// </para>
        /// <para>
        /// This member is virtual for the purpose of testing only.
        /// </para>
        /// </remarks>
        internal virtual void RunRestoreAllJob(CancellationToken? cancellationToken = null)
        {
            restoreAllJob.Run(cancellationToken);
        }


        /// <summary>
        /// Stores given objects to the file system repository.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method is called from synchronized context. Overrides to this method do not need to synchronize against this class' public member calls.
        /// </para>
        /// <para>
        /// This member is virtual for the purpose of testing only.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentException">Thrown when <paramref name="infoObjects"/> contains objects with different object types.</exception>
        internal virtual void RunStoreJob(ObjectTypeInfo typeInfo, IEnumerable<BaseInfo> infoObjects, FileSystemRepositoryConfiguration configuration, ContinuousIntegrationTranslationHelper translationHelper = null)
        {
            var storeJob = FileSystemStoreJobFactory.GetJob(typeInfo, configuration);
            storeJob.TranslationHelper = translationHelper;

            try
            {
                // Use single binding processor for all objects so the write can be delayed to the end
                using (var bindingsProcessor = new CachedFileSystemBindingsProcessor(storeJob.TranslationHelper, configuration, storeJob.FileSystemWriter))
                {
                    storeJob.InitializeWith(new FileSystemJobConfiguration { BindingsProcessor = bindingsProcessor });

                    var sampleRepositoryObjectType = RepositoryConfigurationEvaluator.GetRepositoryObjectType(typeInfo.ObjectType);

                    foreach (var info in infoObjects)
                    {
                        if (!RepositoryTypesEquals(sampleRepositoryObjectType, info))
                        {
                            throw new ArgumentException("Collection must contain info objects of same type.", "infoObjects");
                        }

                        if (RepositoryConfigurationEvaluator.IsObjectIncluded(info, configuration, storeJob.TranslationHelper))
                        {
                            storeJob.Run(info);
                        }
                    }
                }
            }
            finally
            {
                // Save serialization file hashes to the DB
                storeJob.FileSystemWriter.RepositoryHashManager.UpdateFilesMetadataInDatabase(true);
            }
        }


        /// <summary>
        /// Deletes serialization files from the repository.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method is called from synchronized context. Overrides to this method do not need to synchronize against this class' public member calls.
        /// </para>
        /// <para>
        /// This member is virtual for the purpose of testing only.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentException">Thrown when <paramref name="infoObjects"/> contains objects with different object types.</exception>
        internal virtual void RunDeleteJob(ObjectTypeInfo typeInfo, IEnumerable<BaseInfo> infoObjects, FileSystemRepositoryConfiguration configuration, ContinuousIntegrationTranslationHelper translationHelper = null)
        {
            var deleteJob = FileSystemDeleteJobFactory.GetJob(typeInfo, configuration);
            deleteJob.TranslationHelper = translationHelper;

            try
            {
                // Use single binding processor for all objects so the write can be delayed to the end
                using (var bindingsProcessor = new CachedFileSystemBindingsProcessor(deleteJob.TranslationHelper, configuration, deleteJob.FileSystemWriter))
                {
                    deleteJob.InitializeWith(new FileSystemJobConfiguration { BindingsProcessor = bindingsProcessor });

                    var sampleRepositoryObjectType = RepositoryConfigurationEvaluator.GetRepositoryObjectType(typeInfo.ObjectType);

                    foreach (var info in infoObjects)
                    {
                        if (!RepositoryTypesEquals(sampleRepositoryObjectType, info))
                        {
                            throw new ArgumentException("Collection must contain info objects of same type.", "infoObjects");
                        }

                        deleteJob.Run(info);
                    }
                }
            }
            finally
            {
                // Delete serialization file hashes from the DB
                deleteJob.FileSystemWriter.RepositoryHashManager.UpdateFilesMetadataInDatabase(true);
            }
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Returns true when info is matching the repository object type.
        /// </summary>
        /// <param name="objectType">Repository object type</param>
        /// <param name="info">Info object</param>
        private static bool RepositoryTypesEquals(string objectType, BaseInfo info)
        {
            string infoObjectType = RepositoryConfigurationEvaluator.GetRepositoryObjectType(info.TypeInfo.ObjectType);

            return String.Equals(objectType, infoObjectType, StringComparison.InvariantCultureIgnoreCase);
        }


        /// <summary>
        /// Gets name for global mutex based on repository root path and type.
        /// </summary>
        /// <param name="type">Type of the mutex.</param>
        /// <param name="repositoryRootPath">Repository root path</param>
        /// <returns>Global mutex name (including the 'Global\' prefix).</returns>
        private string GetRepositoryMutexName(string type, string repositoryRootPath)
        {
            var safeMutexName = FileSystemRepositoryHelper.GetFileSystemName(type + repositoryRootPath, 100, 10);

            return "Global\\" + safeMutexName;
        }


        /// <summary>
        /// <para>
        /// Indicates whether the storing of all objects is running or not. The method tries to speculatively acquire
        /// the global mutex <see cref="storeAllMutex"/> to determine whether storing is running and releases it immediately unless <paramref name="wait"/>
        /// is true.
        /// </para>
        /// <para>
        /// This method modifies the mutex state by speculatively acquiring it.
        /// </para>
        /// </summary>
        /// <returns>True if storing of all objects is running, false otherwise.</returns>
        private bool IsStoreAllRunningGlobally(bool wait, bool keepMutex, out bool lockAcquired)
        {
            // Inter process detection of long running operation
            lockAcquired = storeAllMutex.WaitOne(wait ? Timeout.Infinite : 0);
            if (lockAcquired)
            {
                if (!keepMutex)
                {
                    storeAllMutex.ReleaseMutex();
                    lockAcquired = false;
                }

                return false;
            }

            return true;
        }


        /// <summary>
        /// <para>
        /// Indicates whether the restoring of all objects is running or not. The method tries to speculatively acquire
        /// the global mutex <see cref="restoreAllMutex"/> to determine whether restoring is running and releases it immediately unless <paramref name="keepMutex"/>
        /// is true.
        /// </para>
        /// <para>
        /// This method modifies the mutex state by speculatively acquiring it.
        /// </para>
        /// </summary>
        /// <returns>True if storing of all objects is running, false otherwise.</returns>
        private bool IsRestoreAllRunningGlobally(bool keepMutex, out bool lockAcquired)
        {
            // Inter process detection of long running operation
            lockAcquired = restoreAllMutex.WaitOne(0);
            if (lockAcquired)
            {
                if (!keepMutex)
                {
                    restoreAllMutex.ReleaseMutex();
                    lockAcquired = false;
                }

                return false;
            }

            return true;
        }


        /// <summary>
        /// Disposes <paramref name="disposedObject"/> if it is not null.
        /// </summary>
        private void DisposeIfNotNull(IDisposable disposedObject)
        {
            if (disposedObject != null)
            {
                disposedObject.Dispose();
            }
        }


        /// <summary>
        /// Throws <see cref="InvalidOperationException"/> when restore all operation is not enabled.
        /// </summary>
        private static void ThrowIfRestoreAllDisabled()
        {
            if (!ContinuousIntegrationHelper.RestoreAllEnabled)
            {
                throw new InvalidOperationException("The RestoreAll operation of Continuous integration feature is disabled. Related web.config key 'CMSContinuousIntegrationRestoreAllEnabled' is set to false.");
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Private constructor ensures uniqueness of singleton class.
        /// </summary>
        /// <exception cref="RepositoryConfigurationException">Thrown when loading of repository configuration fails.</exception>
        protected FileSystemRepositoryManager()
            : this(null)
        {
        }


        /// <summary>
        /// Private constructor ensures uniqueness of singleton class.
        /// </summary>
        /// <remarks>
        /// Do not use this constructor directly for other then testing purposes.
        /// </remarks>
        /// <param name="configurationBuilder">Configuration builder that should be used to create repository configuration when needed.</param>
        /// <exception cref="RepositoryConfigurationException">Thrown when loading of repository configuration fails.</exception>
        internal FileSystemRepositoryManager(IFileSystemRepositoryConfigurationBuilder configurationBuilder)
        {
            mConfigurationBuilder = configurationBuilder ?? new FileSystemRepositoryConfigurationBuilder(null, true);
            RebuildConfiguration();

            try
            {
                serializeMutex = new Mutex(false, GetRepositoryMutexName("StoreRestore_", mConfiguration.RepositoryRootPath), out _, FileSystemRepositorySynchronizationHelper.GetMutexSecurity());
                storeAllMutex = new Mutex(false, GetRepositoryMutexName("StoreAllState_", mConfiguration.RepositoryRootPath), out _, FileSystemRepositorySynchronizationHelper.GetMutexSecurity());
                restoreAllMutex = new Mutex(false, GetRepositoryMutexName("RestoreAllState_", mConfiguration.RepositoryRootPath), out _, FileSystemRepositorySynchronizationHelper.GetMutexSecurity());
            }
            catch
            {
                // If initialization of storeAllRestoreAllMutex throws exception, dispose mutexes
                DisposeIfNotNull(serializeMutex);
                DisposeIfNotNull(storeAllMutex);
                DisposeIfNotNull(restoreAllMutex);
                throw;
            }
        }

        #endregion


        #region "IDisposable"

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="FileSystemRepositoryManager"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        /// <summary>
        /// When overridden in a derived class, releases the unmanaged resources used by the <see cref="FileSystemRepositoryManager"/>, and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        /// <remarks>
        /// The <see cref="FileSystemRepositoryManager"/> uses only managed resources. If unmanaged resources are used in inherited class, make sure its destructor contains call to <c>Dispose(false)</c>
        /// (i.e. this class does not have any destructor implemented).
        /// </remarks>
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                serializeMutex.Dispose();
                storeAllMutex.Dispose();
                restoreAllMutex.Dispose();
            }

            disposed = true;
        }

        #endregion
    }
}
