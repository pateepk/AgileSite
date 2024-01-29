using System;
using System.Threading;

using CMS.DataEngine;
using CMS.EventLog;

namespace CMS.ContinuousIntegration.Internal
{
    /// <summary>
    /// Abstract class incorporating behavior same for all all-jobs.
    /// </summary>
    /// <seealso cref="FileSystemStoreAllJob"/>
    /// <seealso cref="FileSystemRestoreAllJob"/>
    internal abstract class AbstractFileSystemAllJob : AbstractFileSystemProgressLoggingJob
    {
        #region "Properties and variables"


        // Synchronization flag preventing object reusing.
        protected int objectUsed;


        /// <summary>
        /// Returns message that is logged when operation was canceled using <see cref="CancellationToken"/>.
        /// </summary>
        protected abstract string OperationCancelledMessage
        {
            get;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor required for creation of a new instance of derived class.
        /// </summary>
        /// <param name="configuration">Repository configuration.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> is null.</exception>
        protected AbstractFileSystemAllJob(FileSystemRepositoryConfiguration configuration)
            : base(configuration)
        {
        }


        /// <summary>
        /// Checks license, number of usage and initializes cancellation token.
        /// <para>
        /// Tries execute <see cref="RunInternal"/>, while logging any exception to <see cref="CMS.EventLog"/>
        /// (including <see name="OperationCanceledException"/> potentially raised by the <see cref="CancellationToken"/>).
        /// </para>
        /// </summary>
        /// <param name="cancellationToken">Operation can be canceled at any time using given cancellation token. This method's operation terminates as soon as cancellation request is detected.</param>
        /// <remarks>
        /// This operation can be canceled using given <paramref name="cancellationToken"/> at any time.
        /// <para>This method can be called only once in a lifetime of the class' instance.</para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown when this method has already been called.</exception>
        /// <exception cref="LicenseException">Thrown when license requirements for continuous integration are not met.</exception>
        public void Run(CancellationToken? cancellationToken = null)
        {
            CheckContinuousIntegrationLicense();

            if (Interlocked.CompareExchange(ref objectUsed, 1, 0) != 0)
            {
                throw new InvalidOperationException(String.Format("[{0}.Run]: Reusing of the job object is not supported.", GetType().Name));
            }

            var initializedCancellationToken = InitializeCancellationToken(cancellationToken);
            try
            {
                RunInternal(initializedCancellationToken);
            }
            catch (OperationCanceledException)
            {
                RaiseLogProgress(OperationCancelledMessage, LogItemTypeEnum.Error);
            }
            catch (Exception ex)
            {
                RaiseLogProgress(EventLogProvider.GetExceptionLogMessage(ex), LogItemTypeEnum.Error);
            }
        }


        /// <summary>
        /// Action performing specified all-job itself.
        /// </summary>
        /// <param name="cancellationToken">Operation can be canceled at any time using given cancellation token. This method's operation terminates as soon as cancellation request is detected.</param>
        /// <remarks>
        /// This operation can be canceled using given <paramref name="cancellationToken"/> at any time.
        /// <para>This method's operation terminates as soon as cancellation request is detected.</para>
        /// </remarks>
        protected abstract void RunInternal(CancellationToken cancellationToken);

        #endregion
    }
}
