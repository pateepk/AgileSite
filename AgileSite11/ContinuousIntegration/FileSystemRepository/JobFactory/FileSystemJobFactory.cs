using System;
using System.Collections.Concurrent;

using CMS.Core;
using CMS.DataEngine;


namespace CMS.ContinuousIntegration.Internal
{
    /// <summary>
    /// Factory for file system repository jobs.
    /// </summary>
    /// <typeparam name="TJobFactory">Type of the factory inherited from this abstract class.</typeparam>
    /// <typeparam name="TJob">Type of job to be instantiated by the factory.</typeparam>
    public abstract class FileSystemJobFactory<TJobFactory, TJob>
        where TJobFactory : FileSystemJobFactory<TJobFactory, TJob>, new()
    {
        #region "Delegates"
        
        /// <summary>
        /// Factory method creating a new job from given configuration.
        /// </summary>
        /// <param name="configuration">Repository manager configuration for the job to be created.</param>
        /// <returns>New instance of job.</returns>
        public delegate TJob CreateJob(FileSystemRepositoryConfiguration configuration);

        #endregion


        #region "Fields"

        // Factory methods organized by object type
        private readonly ConcurrentDictionary<string, CreateJob> mJobFactoryMethods = new ConcurrentDictionary<string, CreateJob>(StringComparer.InvariantCultureIgnoreCase);
        private readonly CreateJob mDefaultJobFactoryMethod;

        /// <summary>
        /// The only instance of the class.
        /// </summary>
        private static readonly FileSystemJobFactory<TJobFactory, TJob> instance = new TJobFactory();

        #endregion


        #region "Static methods"

        /// <summary>
        /// Registers a new job for given object type by providing a factory method for the job.
        /// </summary>
        /// <param name="objectType">Object type for which the job is to be used.</param>
        /// <param name="factoryMethod">Method which creates a new instance of the job.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="objectType"/> or <paramref name="factoryMethod"/> is null.</exception>
        public static void RegisterJob(string objectType, CreateJob factoryMethod)
        {
            instance.RegisterJobInternal(objectType, factoryMethod);
        }


        /// <summary>
        /// Gets a new instance of <typeparamref name="TJob"/> job suitable for <paramref name="typeInfo"/>. The instance is determined as follows.
        /// If specific job is available for object type of type info, it is used.
        /// If specific job is available for original object type of type info, it is used.
        /// Otherwise the default job is used.
        /// </summary>
        /// <param name="typeInfo">Type info for which to instantiate a <typeparamref name="TJob"/> based on object type (or original object type).</param>
        /// <param name="configuration">Configuration to be used when creating an instance of the job.</param>
        /// <returns>An instance of <typeparamref name="TJob"/>.</returns>
        /// <exception cref="BaseInfo">Thrown when <paramref name="typeInfo"/> is null.</exception>
        internal static TJob GetJob(ObjectTypeInfo typeInfo, FileSystemRepositoryConfiguration configuration)
        {
            return instance.GetJobInternal(typeInfo, configuration);
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Registers a new job for given object type by providing a factory method for the job.
        /// </summary>
        /// <param name="objectType">Object type for which the job is to be used.</param>
        /// <param name="factoryMethod">Method which creates a new instance of the job.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="objectType"/> or <paramref name="factoryMethod"/> is null.</exception>
        private void RegisterJobInternal(string objectType, CreateJob factoryMethod)
        {
            if (objectType == null)
            {
                throw new ArgumentNullException(nameof(objectType), "Object type can not be null.");
            }
            if (factoryMethod == null)
            {
                throw new ArgumentNullException(nameof(factoryMethod), "Factory method can not be null.");
            }

            mJobFactoryMethods[objectType] = factoryMethod;
        }


        /// <summary>
        /// Gets a new instance of <typeparamref name="TJob"/> job suitable for <paramref name="typeInfo"/>. The instance is determined as follows.
        /// If specific job is available for object type of type info, it is used.
        /// If specific job is available for original object type of type info, it is used.
        /// Otherwise the default job is used.
        /// </summary>
        /// <param name="typeInfo">Type info for which to instantiate a <typeparamref name="TJob"/> based on object type (or original object type).</param>
        /// <param name="configuration">Configuration to be used when creating an instance of the job.</param>
        /// <returns>An instance of <typeparamref name="TJob"/>.</returns>
        /// <exception cref="BaseInfo">Thrown when <paramref name="typeInfo"/> is null.</exception>
        private TJob GetJobInternal(ObjectTypeInfo typeInfo, FileSystemRepositoryConfiguration configuration)
        {
            if (typeInfo == null)
            {
                throw new ArgumentNullException(nameof(typeInfo), "Type info can not be null.");
            }

            CreateJob jobFactoryMethod;
            if (mJobFactoryMethods.TryGetValue(typeInfo.ObjectType, out jobFactoryMethod))
            {
                return jobFactoryMethod.Invoke(configuration);
            }

            if ((typeInfo.OriginalTypeInfo != null) && mJobFactoryMethods.TryGetValue(typeInfo.OriginalObjectType, out jobFactoryMethod))
            {
                return jobFactoryMethod.Invoke(configuration);
            }

            return mDefaultJobFactoryMethod.Invoke(configuration);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Static constructor.
        /// </summary>
        static FileSystemJobFactory()
        {
            TypeManager.RegisterGenericType(typeof(FileSystemJobFactory<TJobFactory, TJob>));
        }


        /// <summary>
        /// Initializes a new job factory with default factory method.
        /// </summary>
        /// <param name="defaultJobFactoryMethod">Method which creates a new instance of the job.</param>
        protected FileSystemJobFactory(CreateJob defaultJobFactoryMethod)
        {
            if (defaultJobFactoryMethod == null)
            {
                throw new ArgumentNullException(nameof(defaultJobFactoryMethod), "Default job can not be null.");
            }

            mDefaultJobFactoryMethod = defaultJobFactoryMethod;
        }

        #endregion
    }
}
