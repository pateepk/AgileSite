using System;
using System.Threading;

using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Rest.Azure;
using Microsoft.Rest.TransientFaultHandling;

using CMS.DataEngine;

namespace CMS.Search.Azure
{
    /// <summary>
    /// Performs exponential retry strategy.
    /// </summary>
    /// <remarks>
    /// Time interval between two attempts is exponentially increased.
    /// Retry is performed after 0, 1, 2, 4, 8, 16 ... seconds limited by <see cref="MaxBackoffTimeSeconds"/>.
    /// </remarks>
    internal class ExponentialRetryStrategy : ISearchServiceRetryStrategy
    {
        private int mRetryCount;


        /// <summary>
        /// Number of retry attempts to perform after operation fails for the first time.
        /// </summary>
        /// <remarks>Retry count may equal zero, to not perform any retry strategy.</remarks>
        public int RetryCount
        {
            get
            {
                return mRetryCount;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value", "Retry count can not be less than zero.");
                }

                mRetryCount = value;
            }
        }


        /// <summary>
        /// Defines maximum time in seconds that is awaited between two attempts to perform a failed operation.
        /// </summary>
        public int MaxBackoffTimeSeconds
        {
            get;
            set;
        }


        /// <summary>
        /// Defines an object responsible for detecting specific transient conditions on Azure Search service operations.
        /// By default every exception is handled as transient.
        /// </summary>
        public ITransientErrorDetectionStrategy TransientErrorDetectionStrategy
        {
            get;
            set;
        }


        /// <summary>
        /// Creates exponential retry strategy for Azure Search service operations.
        /// </summary>
        /// <param name="retryCount">Number of retry attempts to perform after operation fails in the first attempt.</param>
        /// <param name="maxBackoffTimeSeconds">Defines maximum time in seconds that is awaited between two attempts to perform a failed operation.</param>
        public ExponentialRetryStrategy(int retryCount, int maxBackoffTimeSeconds)
        {
            RetryCount = retryCount;
            MaxBackoffTimeSeconds = maxBackoffTimeSeconds;
        }


        /// <summary>
        /// Creates exponential retry strategy for Azure Search service operations.
        /// </summary>
        /// <param name="retryCount">Number of retry attempts to perform after operation fails in the first attempt.</param>
        /// <param name="maxBackoffTimeSeconds">Defines maximum time in seconds that is awaited between two attempts to perform a failed operation.</param>
        /// <param name="transientErrorDetectionStrategy">Defines an object responsible for detecting specific transient conditions.</param>
        public ExponentialRetryStrategy(int retryCount, int maxBackoffTimeSeconds, ITransientErrorDetectionStrategy transientErrorDetectionStrategy) 
            : this(retryCount, maxBackoffTimeSeconds)
        {
            TransientErrorDetectionStrategy = transientErrorDetectionStrategy;
        }


        /// <summary>
        /// Creates exponential retry strategy for Azure Search service operations.
        /// </summary>
        /// <remarks>
        /// Default value for <see cref="RetryCount"/> is acquired from <see cref="SearchEngineConfiguration.RetryCount"/>.
        /// Default value for <see cref="MaxBackoffTimeSeconds"/> is acquired from <see cref="SearchEngineConfiguration.MaxBackoffTimeSeconds"/>.
        /// </remarks>
        public ExponentialRetryStrategy()
            : this(SearchEngineConfiguration.Instance.RetryCount, 
                  SearchEngineConfiguration.Instance.MaxBackoffTimeSeconds, 
                  SearchEngineConfiguration.Instance.TransientErrorDetectionStrategy) { }


        /// <summary>
        /// Performs exponential retry strategy on <paramref name="operation"/> if it fails during it's execution.
        /// </summary>
        /// <param name="operation">Operation that should be retried if fails during execution.</param>
        /// <returns>
        /// Returns the value of <paramref name="operation"/> if successfully finished. 
        /// Otherwise default value of <typeparamref name="T"/> is returned.
        /// </returns>
        /// <seealso cref="SearchServiceManager"/>
        /// <seealso cref="CloudException"/>
        public T Perform<T>(Func<T> operation)
        {
            Func<IndexBatch, T> funcWrapper = emptyParam => operation();

            return Perform(null, funcWrapper);
        }


        /// <summary>
        /// Performs exponential retry strategy on <paramref name="operation"/> if it fails during execution of index action in <paramref name="batch"/>.
        /// </summary>
        /// <param name="batch">Index actions performed by <paramref name="operation"/>.</param>
        /// <param name="operation">Operation that should be retried if fails during execution.</param>
        /// <returns>
        /// Returns the value of <paramref name="operation"/> if successfully finished. 
        /// Otherwise default value of <typeparamref name="T"/> is returned.
        /// </returns>
        /// <seealso cref="SearchServiceManager"/>
        /// <seealso cref="IndexBatchException"/>
        /// <seealso cref="CloudException"/>
        public T Perform<T>(IndexBatch batch, Func<IndexBatch, T> operation)
        {
            return Strategy(batch, operation);
        }


        /// <summary>
        /// Exponential retry strategy applied on <paramref name="operation"/> if operation fails during execution.
        /// </summary>
        private T Strategy<T>(IndexBatch batch, Func<IndexBatch, T> operation)
        {
            T result = default(T);
            for (int i = 0; i < RetryCount + 1; ++i)
            {
                try
                {
                    result = operation(batch);
                    break;
                }
                // Do not catch exceptions during the last attempt
                catch (IndexBatchException ibe) when ((i < RetryCount) && (TransientErrorDetectionStrategy?.IsTransient(ibe) ?? true))
                {
                    if (batch != null)
                    {
                        batch = ibe.FindFailedActionsToRetry(batch, NamingHelper.GetValidFieldName(SearchFieldsConstants.ID));
                    }
                }
                catch (CloudException ce) when ((i < RetryCount) && (TransientErrorDetectionStrategy?.IsTransient(ce) ?? true)) { }

                Thread.Sleep(GetBackoffTime(i) * 1000);
            }

            return result;
        }


        /// <summary>
        /// Returns how many seconds to wait before retrying operation again.
        /// </summary>
        private int GetBackoffTime(int iteration)
        {
            if (iteration <= 0)
            {
                return 0;
            }

            int result = 1;
            for (int i = 1; i < iteration; ++i)
            {
                result *= 2;
            }

            return Math.Min(result, MaxBackoffTimeSeconds);
        }
    }
}
