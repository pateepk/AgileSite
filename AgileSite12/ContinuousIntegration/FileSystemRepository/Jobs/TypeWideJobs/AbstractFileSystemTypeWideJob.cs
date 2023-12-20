using System;
using System.Collections.Generic;
using System.Threading;

using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.ContinuousIntegration.Internal
{
    /// <summary>
    /// Abstract class incorporating behavior same for all objects of a given type jobs.
    /// </summary>
    /// <seealso cref="FileSystemUpsertObjectsByTypeJob"/>
    /// <seealso cref="FileSystemDeleteObjectsByTypeJob"/>
    public abstract class AbstractFileSystemTypeWideJob : AbstractFileSystemProgressLoggingJob
    {
        /// <summary>
        /// Constructor required for creation of a new instance of derived class.
        /// </summary>
        /// <param name="configuration">Repository configuration.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> is null.</exception>
        internal AbstractFileSystemTypeWideJob(FileSystemRepositoryConfiguration configuration)
            : base(configuration)
        {
        }


        /// <summary>
        /// Checks license, <paramref name="objectType"/> and <paramref name="fileLocations"/>.
        /// Then initializes cancellation token and tries execute <see cref="RunInternal"/>.
        /// </summary>
        /// <param name="objectType">Name of object type to process.</param>
        /// <param name="fileLocations">Set of all locations' of objects stored in the repository that are of given <paramref name="objectType"/>.</param>
        /// <param name="cancellationToken">Operation can be canceled at any time using given cancellation token. This method's operation terminates as soon as cancellation request is detected.</param>
        /// <remarks>This operation can be canceled using given <paramref name="cancellationToken"/> at any time.</remarks>
        /// <exception cref="ArgumentException">Thrown when <paramref name="objectType"/> is either <see langword="null"/> or empty.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="fileLocations"/> is not provided.</exception>
        /// <exception cref="LicenseException">Thrown when license requirements for continuous integration are not met.</exception>
        internal void Run(string objectType, ISet<RepositoryLocationsCollection> fileLocations, CancellationToken? cancellationToken = null)
        {
            if (String.IsNullOrEmpty(objectType))
            {
                throw new ArgumentException("Object type cannot be null nor empty.");
            }
            if (fileLocations == null)
            {
                throw new ArgumentNullException("fileLocations");
            }

            CheckContinuousIntegrationLicense();
            var initializedCancellationToken = InitializeCancellationToken(cancellationToken);

            RunInternal(objectType, fileLocations, initializedCancellationToken);
        }


        /// <summary>
        /// Executes the operation performed on all objects of provided <paramref name="objectType"/> itself.
        /// </summary>
        /// <param name="objectType">Name of object type to process.</param>
        /// <param name="fileLocations">Set of all locations' of objects stored in the repository that are of given <paramref name="objectType"/>.</param>
        /// <param name="cancellationToken">Operation can be canceled at any time using given cancellation token. This method's operation terminates as soon as cancellation request is detected.</param>
        /// <remarks>Provided <paramref name="objectType"/> or <paramref name="fileLocations"/> are never <see langword="null"/>.</remarks>
        protected abstract void RunInternal(string objectType, ISet<RepositoryLocationsCollection> fileLocations, CancellationToken cancellationToken);


        /// <summary>
        /// Gets format of message informing about object's action.
        /// </summary>
        /// <param name="objectType">Object type.</param>
        protected string GetObjectInfoMessageFormat(string objectType)
        {
            return String.Format("{0} {1}: {{0}} {{1}}",
                        ResHelper.GetAPIString("general.objecttype", "Object type"),
                        TypeHelper.GetNiceObjectTypeName(objectType));
        }


        /// <summary>
        /// Process queue of objects using given function for processing every object in the queue. If the function returns null, object is considered as successfully processed.
        /// If function returns object, then the returned object is enqueued. Queue is iterated again while at least one object was successfully processed.
        /// </summary>
        /// <typeparam name="T">Type of objects in the queue.</typeparam>
        /// <param name="queue">Queue to process.</param>
        /// <param name="function">Function used for processing every object in the queue. If the function returns null, object is considered as successfully processed. If function returns object, then the returned object is enqueued.</param>
        /// <param name="cancellationToken">Operation can be canceled at any time using given cancellation token. This method's operation terminates as soon as cancellation request is detected.</param>
        /// <exception cref="OperationCanceledException">Thrown when operation was canceled using the <paramref name="cancellationToken"/>.</exception>
        protected void CancellablyProcessQueue<T>(CancellationToken cancellationToken, Queue<T> queue, Func<T, T> function) where T : class
        {
            int lastIterationQueueCount = 0;
            while ((queue.Count > 0) && (lastIterationQueueCount != queue.Count))
            {
                lastIterationQueueCount = queue.Count;
                for (var i = 0; i < lastIterationQueueCount; i++)
                {
                    var processedObject = queue.Dequeue();
                    var res = function(processedObject);
                    if (res != null)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        queue.Enqueue(processedObject);
                    }
                }
            }
        }
    }
}
