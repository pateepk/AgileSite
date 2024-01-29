using System;

using Microsoft.Azure.Search.Models;

namespace CMS.Search.Azure
{
    /// <summary>
    /// Represents a contract for Azure Search service retry strategy.
    /// </summary>
    internal interface ISearchServiceRetryStrategy
    {
        /// <summary>
        /// Provides retry policy and tries to perform given <paramref name="operation"/>.
        /// </summary>
        /// <typeparam name="T">Type of object returned by <paramref name="operation"/>.</typeparam>
        /// <param name="operation">Operation to be performed.</param>
        /// <returns>Returns the return value of the performed <paramref name="operation"/> of type <typeparamref name="T"/>.</returns>
        T Perform<T>(Func<T> operation);


        /// <summary>
        /// Provides retry policy and tries to perform <paramref name="operation"/> which uses index actions in the <paramref name="batch"/>.
        /// </summary>
        /// <typeparam name="T">Type of object returned by <paramref name="operation"/>.</typeparam>
        /// <param name="batch">Index actions to be performed by <paramref name="operation"/>.</param>
        /// <param name="operation">Operation to be performed.</param>
        /// <returns>Returns the return value of the <paramref name="operation"/> of type <typeparamref name="T"/>.</returns>
        T Perform<T>(IndexBatch batch, Func<IndexBatch, T> operation);
    }
}
