using System;
using System.Threading;
using System.Collections.Generic;

using CMS.DataEngine;

namespace CMS.ContinuousIntegration.Internal
{
    /// <summary>
    /// Abstract class incorporating usual properties and basic methods for inter-job usage.
    /// </summary>
    /// <remarks>This class is not intended for inheritance. Non-abstract jobs are supposed to be used for behavioral amendments.</remarks>
    /// <seealso cref="AbstractSingleObjectJob"/>
    /// <seealso cref="AbstractFileSystemTypeWideJob"/>
    /// <seealso cref="AbstractFileSystemAllJob"/>
    /// <seealso cref="AbstractFileSystemProgressLoggingJob"/>
    public abstract class AbstractFileSystemJob
    {
        #region "Variables"

        // Translation helper that is used for deserialization.
        private ContinuousIntegrationTranslationHelper mTranslationHelper;

        // Object for working with repository's file paths.
        private RepositoryPathHelper mRepositoryPathHelper;

        // Object for working with bindings in the file system repository.
        private FileSystemBindingsProcessor mBindingsProcessor;

        // Object for working with separated fields in file system repository.
        private SeparatedFieldProcessor mSeparatedFieldProcessor;

        // Object for reading file system and storing hash of read file in its own hash manager.
        private ICachedFileSystemReader mFileSystemReader;

        // Object for writing to file system and storing hash of written file in hash manager.
        private IFileSystemWriter mFileSystemWriter;

        // Object for collecting content staging tasks during CI restore.
        private ContentStagingTaskCollection mContentStagingTaskCollection;

        // Indicates if initialization method has already been used, effectively sealing the instance against configuration reset.
        private bool mInitialized;

        // Indicates whether objects' file meta-data are used or not.
        private bool? mUseFileMetadata;

        #endregion


        #region "Properties"

        /// <summary>
        /// Provides object translation. Used in serialization to optimize database calls.
        /// </summary>
        /// <remarks>
        /// The <see cref="DataEngine.TranslationHelper.TranslationTable"/> property contains data records that correspond with the database.        
        /// For translation records identified using partially correct parameters (either the code name or GUID is different than the value in the database), 
        /// the actual values are loaded from the database for all fields.        
        /// An index key referencing the correct data is then created from the partial parameters. The index key is used to obtain the record in future calls.
        /// </remarks>
        protected internal ContinuousIntegrationTranslationHelper TranslationHelper
        {
            get
            {
                return mTranslationHelper ?? (mTranslationHelper = new ContinuousIntegrationTranslationHelper());
            }
            internal set
            {
                mTranslationHelper = value;
            }
        }


        /// <summary>
        /// Gets the current instance of repository configuration
        /// </summary>
        protected FileSystemRepositoryConfiguration RepositoryConfiguration
        {
            get;
        }


        /// <summary>
        /// Object for working with bindings in the file system repository.
        /// </summary>
        internal virtual FileSystemBindingsProcessor BindingsProcessor
        {
            get
            {
                return mBindingsProcessor ?? (mBindingsProcessor = new FileSystemBindingsProcessor(TranslationHelper, RepositoryConfiguration, FileSystemWriter));
            }
            private set
            {
                mBindingsProcessor = value;
            }
        }


        /// <summary>
        /// Object for working with separated fields in file system repository.
        /// </summary>
        internal SeparatedFieldProcessor SeparatedFieldProcessor
        {
            get
            {
                return mSeparatedFieldProcessor ?? (mSeparatedFieldProcessor = new SeparatedFieldProcessor(RepositoryPathHelper, FileSystemReader, FileSystemWriter));
            }
            private set
            {
                mSeparatedFieldProcessor = value;
            }
        }


        /// <summary>
        /// Object for writing to file system and storing hash of written file in hash manager.
        /// </summary>
        internal IFileSystemWriter FileSystemWriter
        {
            get
            {
                return mFileSystemWriter ?? (mFileSystemWriter = new FileSystemWriter(RepositoryConfiguration, new RepositoryHashManager()));
            }
            private set
            {
                mFileSystemWriter = value;
            }
        }


        /// <summary>
        /// Object for reading file system and storing hash of read file in its own hash manager.
        /// </summary>
        internal ICachedFileSystemReader FileSystemReader
        {
            get
            {
                return mFileSystemReader ?? (mFileSystemReader = new CachedFileSystemReader(RepositoryConfiguration, new RepositoryHashManager()));
            }
            private set
            {
                mFileSystemReader = value;
            }
        }


        /// <summary>
        /// Object for working with file paths in file system repository.
        /// </summary>
        protected RepositoryPathHelper RepositoryPathHelper
        {
            get
            {
                return mRepositoryPathHelper ?? (mRepositoryPathHelper = new RepositoryPathHelper(RepositoryConfiguration, TranslationHelper));
            }
            private set
            {
                mRepositoryPathHelper = value;
            }
        }


        /// <summary>
        /// Object for collecting content staging tasks during CI restore.
        /// </summary>
        protected ContentStagingTaskCollection ContentStagingTaskCollection
        {
            get
            {
                return mContentStagingTaskCollection ?? (mContentStagingTaskCollection = new ContentStagingTaskCollection());
            }
            private set
            {
                mContentStagingTaskCollection = value;
            }
        }


        /// <summary>
        /// Indicates whether objects' file meta-data are used or not (see <see cref="FileMetadataInfo"/>).
        /// </summary>
        protected internal bool UseFileMetadata
        {
            get
            {
                return (mUseFileMetadata ?? (mUseFileMetadata = ContinuousIntegrationHelper.IsObjectSerializationEnabled)).Value;
            }
            private set
            {
                mUseFileMetadata = value;
            }
        }


        /// <summary>
        /// Indicates whether license should be checked when a job is run
        /// (property is used within <see cref="CheckContinuousIntegrationLicense"/> method).
        /// </summary>
        /// <remarks>
        /// This property is supposed to be <see langword="true"/> if and only if
        /// the job is initialized from another job (that checked the license already).
        /// <para>This property is present to prevent unnecessary license checks.</para>
        /// </remarks>
        internal bool SkipLicenseChecking
        {
            get;
            private set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor required for creation of a new instance of derived class.
        /// </summary>
        /// <param name="configuration">Repository configuration.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> is null.</exception>
        protected AbstractFileSystemJob(FileSystemRepositoryConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration), "Repository configuration cannot be null.");
            }

            RepositoryConfiguration = configuration;
        }


        /// <summary>
        /// Initializes job by injecting instances of provided object properties into respective properties.
        /// </summary>
        /// <param name="fileSystemJobConfiguration">Configuration for the file system job.</param>
        internal void InitializeWithInternal(FileSystemJobConfiguration fileSystemJobConfiguration)
        {
            // Check that initialization was not performed yet
            if (mInitialized)
            {
                throw new InvalidOperationException($"Instance of job '{GetType().FullName}' has already been initialized.");
            }

            if (fileSystemJobConfiguration == null)
            {
                throw new ArgumentNullException(nameof(fileSystemJobConfiguration));
            }

            if (fileSystemJobConfiguration.RepositoryPathHelper != null)
            {
                RepositoryPathHelper = fileSystemJobConfiguration.RepositoryPathHelper;
            }

            if (fileSystemJobConfiguration.TranslationHelper != null)
            {
                TranslationHelper = fileSystemJobConfiguration.TranslationHelper;
            }

            if (fileSystemJobConfiguration.BindingsProcessor != null)
            {
                BindingsProcessor = fileSystemJobConfiguration.BindingsProcessor;
            }

            if (fileSystemJobConfiguration.SeparatedFieldProcessor != null)
            {
                SeparatedFieldProcessor = fileSystemJobConfiguration.SeparatedFieldProcessor;
            }

            if (fileSystemJobConfiguration.FileSystemWriter != null)
            {
                FileSystemWriter = fileSystemJobConfiguration.FileSystemWriter;
            }

            if (fileSystemJobConfiguration.FileSystemReader != null)
            {
                FileSystemReader = fileSystemJobConfiguration.FileSystemReader;
            }

            if (fileSystemJobConfiguration.ContentStagingTaskCollection != null)
            {
                ContentStagingTaskCollection = fileSystemJobConfiguration.ContentStagingTaskCollection;
            }

            if (fileSystemJobConfiguration.UseFileMetadata.HasValue)
            {
                UseFileMetadata = fileSystemJobConfiguration.UseFileMetadata.Value;
            }

            if (fileSystemJobConfiguration.SkipLicenseChecking.HasValue)
            {
                SkipLicenseChecking = fileSystemJobConfiguration.SkipLicenseChecking.Value;
            }

            // Seal the instance against multiple initialization
            mInitialized = true;
        }


        /// <summary>
        /// Check license requirement for Continuous Integration and throws <see cref="LicenseException"/> if they are not met.
        /// </summary>
        /// <exception cref="LicenseException">Thrown when license requirements for continuous integration are not met.</exception>
        protected internal void CheckContinuousIntegrationLicense()
        {
            if (SkipLicenseChecking)
            {
                // License is supposed not to be checked
                return;
            }

            if (!ContinuousIntegrationHelper.CheckLicense())
            {
                throw new LicenseException("License requirements for continuous integration are not met.");
            }
        }


        /// <summary>
        /// Registers a new translation record into <see cref="TranslationHelper"/>.
        /// </summary>
        /// <param name="baseInfo">Base info to register.</param>
        protected void RegisterTranslationRecord(BaseInfo baseInfo)
        {
            TranslationHelper.RegisterRecord(baseInfo);
        }


        /// <summary>
        /// Gets new job of <typeparamref name="TJob"/> using <paramref name="factoryJobGetter"/> and initializes it with all properties used in
        /// <see cref="FileSystemJobExtesions.InitializeWith{T}(T,FileSystemJobConfiguration)"/>
        /// of the job calling this method.
        /// </summary>
        /// <typeparam name="TJob">Type of job that will be created.</typeparam>
        /// <param name="factoryJobGetter"><see cref="FileSystemJobFactory{TJobFactory, TJob}"/> method creating a new job.</param>
        /// <param name="typeInfo"><see cref="ObjectTypeInfo"/> to get the job's factory job for.</param>
        protected TJob GetNewJobWithSharedResources<TJob>(Func<ObjectTypeInfo, FileSystemRepositoryConfiguration, TJob> factoryJobGetter, ObjectTypeInfo typeInfo)
            where TJob : AbstractFileSystemJob
        {
            return GetNewJobWithSharedResources(_ => factoryJobGetter(typeInfo, RepositoryConfiguration));
        }


        /// <summary>
        /// Creates new job using provided <paramref name="jobCreator"/> and initializes it with all properties used in
        /// <see cref="FileSystemJobExtesions.InitializeWith{T}(T,FileSystemJobConfiguration)"/>
        /// of the job calling this method.
        /// </summary>
        /// <typeparam name="TJob">Type of job that will be created.</typeparam>
        /// <param name="jobCreator">Method creating new job from provided configuration.</param>
        internal TJob GetNewJobWithSharedResources<TJob>(Func<FileSystemRepositoryConfiguration, TJob> jobCreator)
            where TJob : AbstractFileSystemJob
        {
            var fileSystemJobConfiguration = new FileSystemJobConfiguration
            {
                RepositoryPathHelper = RepositoryPathHelper,
                TranslationHelper = TranslationHelper,
                BindingsProcessor = BindingsProcessor,
                FileSystemWriter = FileSystemWriter,
                FileSystemReader = FileSystemReader,
                SeparatedFieldProcessor = SeparatedFieldProcessor,
                ContentStagingTaskCollection = ContentStagingTaskCollection,
                UseFileMetadata = UseFileMetadata,
                SkipLicenseChecking = SkipLicenseChecking
            };

            return jobCreator(RepositoryConfiguration)
                .InitializeWith(fileSystemJobConfiguration);
        }


        /// <summary>
        /// If provided <paramref name="cancellationToken"/> is not null, the token is used;
        /// otherwise, new <see cref="CancellationToken"/> is created so as the derived <see langword="class"/>
        /// does not need to <see langword="null"/>-checked each token's use.
        /// </summary>
        /// <param name="cancellationToken">(Null-able) token provided by the class user.</param>
        protected CancellationToken InitializeCancellationToken(CancellationToken? cancellationToken)
        {
            return cancellationToken ?? new CancellationToken();
        }


        /// <summary>
        /// Enumerates through <paramref name="collection"/> checking <paramref name="cancellationToken"/>
        /// before each iteration and performing <paramref name="action"/> on each item in the collection.
        /// </summary> 
        /// <typeparam name="T">Type of items in the <paramref name="collection"/>.</typeparam>
        /// <param name="cancellationToken">Token providing cancellation functionality.</param>
        /// <param name="collection">Collection of items the <paramref name="action"/> should be performed on.</param>
        /// <param name="action">Action that should be performed on each item of <paramref name="collection"/>.</param>
        /// <exception cref="OperationCanceledException">Thrown when <paramref name="cancellationToken"/> enters canceled state.</exception>
        protected void CancellableForEach<T>(CancellationToken cancellationToken, IEnumerable<T> collection, Action<T> action)
        {
            CancellableForEach(cancellationToken, collection, (item, _) => action(item));
        }


        /// <summary>
        /// Enumerates through <paramref name="collection"/> checking <paramref name="cancellationToken"/>
        /// before each iteration and performing <paramref name="action"/> on each item in the collection.
        /// </summary> 
        /// <typeparam name="T">Type of items in the <paramref name="collection"/>.</typeparam>
        /// <param name="cancellationToken">Token providing cancellation functionality.</param>
        /// <param name="collection">Collection of items the <paramref name="action"/> should be performed on.</param>
        /// <param name="action">Action that should be performed on each item of <paramref name="collection"/>.</param>
        /// <exception cref="OperationCanceledException">Thrown when <paramref name="cancellationToken"/> enters canceled state.</exception>
        protected void CancellableForEach<T>(CancellationToken cancellationToken, IEnumerable<T> collection, Action<T, int> action)
        {
            var enumerator = collection.GetEnumerator();

            if (!enumerator.MoveNext())
            {
                // Check cancellation token even for empty collections
                cancellationToken.ThrowIfCancellationRequested();
                return;
            }

            var counter = 0;
            do
            {
                cancellationToken.ThrowIfCancellationRequested();

                action(enumerator.Current, counter);
                counter++;
            }
            while (enumerator.MoveNext());
        }

        #endregion
    }
}
