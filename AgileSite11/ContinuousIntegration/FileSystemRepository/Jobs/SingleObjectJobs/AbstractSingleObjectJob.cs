using System;

using CMS.DataEngine;

namespace CMS.ContinuousIntegration.Internal
{
    /// <summary>
    /// Abstract class incorporating behavior same for all single-object-processing jobs.
    /// </summary>
    /// <seealso cref="FileSystemStoreJob"/>
    /// <seealso cref="FileSystemDeleteJob"/>
    public abstract class AbstractSingleObjectJob : AbstractFileSystemJob
    {
        /// <summary>
        /// Constructor required for creation of a new instance of derived class.
        /// </summary>
        /// <param name="configuration">Repository configuration.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> is null.</exception>
        protected AbstractSingleObjectJob(FileSystemRepositoryConfiguration configuration)
            : base(configuration)
        {
        }


        /// <summary>
        /// Checks license and provided <paramref name="baseInfo"/>.
        /// <para>Tries execute <see cref="RunInternal"/>, wrapping any exception to <see cref="ObjectSerializationException"/></para>
        /// </summary>
        /// <param name="baseInfo">Base info which will be processed by the job.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="baseInfo"/> is null.</exception>
        /// <exception cref="LicenseException">Thrown when license requirements for continuous integration are not met.</exception>
        /// <exception cref="ObjectSerializationException">Thrown when storing of the object to the repository failed.</exception>
        /// <remarks>
        /// This method does not check if <paramref name="baseInfo"/> is of supported object type.
        /// <para>This method does not ensure repository access synchronization.</para>
        /// </remarks>
        internal void Run(BaseInfo baseInfo)
        {
            CheckContinuousIntegrationLicense();

            if (baseInfo == null)
            {
                throw new ArgumentNullException("baseInfo", "Info object cannot be null.");
            }

            try
            {
                RunInternal(baseInfo);
            }
            catch (Exception ex)
            {
                throw new ObjectSerializationException(ObjectSerializationExceptionMessage(baseInfo), ex);
            }
        }


        /// <summary>
        /// Executes the operation performed on provided <paramref name="baseInfo"/> itself.
        /// </summary>
        /// This method does not check if <paramref name="baseInfo"/> is of supported object type.
        /// <remarks>Provided <paramref name="baseInfo"/> is never <see langword="null"/>.</remarks>
        protected abstract void RunInternal(BaseInfo baseInfo);


        /// <summary>
        /// Returns text that is shown in <see cref="ObjectSerializationException"/> thrown when execution of <see cref="RunInternal(BaseInfo)"/> fails with an exception.
        /// </summary>
        /// <param name="baseInfo">Object that was passed to the <see cref="RunInternal(BaseInfo)"/> method.</param>
        /// <remarks>Provided <paramref name="baseInfo"/> is never <see langword="null"/>.</remarks>
        protected abstract string ObjectSerializationExceptionMessage(BaseInfo baseInfo);
    }
}
