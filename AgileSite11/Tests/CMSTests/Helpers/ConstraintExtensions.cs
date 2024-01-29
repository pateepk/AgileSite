using NUnit.Framework.Constraints;

namespace CMS.Tests
{
    /// <summary>
    /// Extensions of <see cref="Constraint"/> class.
    /// </summary>
    internal static class ConstraintExtensions
    {
        /// <summary>
        /// Returns an <see cref="ExponentiallyDelayedConstraint"/> representing an exponential delay to the match so that a match can be evaluated in the future.
        /// The retry interval increases exponentially while being limited by 60 seconds (i.e. 0, 1, 2, 4, 8, 16, 32, 60, 60, 60, ...)
        /// </summary>
        /// <param name="constraint">Constraint.</param>
        /// <param name="retryCount">Retry count.</param>
        /// <remarks>
        /// Performs the match immediately and if it does not get the expected result, performs number of retries up to given <paramref name="retryCount"/>
        /// with exponentially increasing interval between retries.
        /// </remarks>
        public static ExponentiallyDelayedConstraint WithExponentialRetries(this Constraint constraint, int retryCount)
        {
            var baseContraint = constraint.Builder == null ? constraint : constraint.Builder.Resolve();
            return new ExponentiallyDelayedConstraint(baseContraint, retryCount);
        }


        /// <summary>
        /// <para>
        /// Returns an <see cref="ExponentiallyDelayedConstraint"/> representing an exponential delay to the match so that a match can be evaluated in the future.
        /// The retry interval increases exponentially while being limited by <paramref name="maxBackoffTimeInSeconds"/> seconds.
        /// </para>
        /// <para>
        /// Example where N is <paramref name="maxBackoffTimeInSeconds"/>:
        /// <para>0, 1, 2, 4, N, N, N, ... for 4&lt;N&lt;=8</para>
        /// <para>0, 1, 2, 4, 8, 16, N, N, N ... for 16&lt;N&lt;=32</para>
        /// </para>
        /// </summary>
        /// <param name="constraint">Constraint.</param>
        /// <param name="retryCount">Retry count.</param>
        /// <param name="maxBackoffTimeInSeconds">Maximal backoff interval between retries in seconds.</param>
        /// <remarks>
        /// Performs the match immediately and if it does not get the expected result, performs number of retries up to given <paramref name="retryCount"/>
        /// with exponentially increasing interval between retries.
        /// </remarks>
        public static ExponentiallyDelayedConstraint WithExponentialRetries(this Constraint constraint, int retryCount, int maxBackoffTimeInSeconds)
        {
            var baseContraint = constraint.Builder == null ? constraint : constraint.Builder.Resolve();
            return new ExponentiallyDelayedConstraint(baseContraint, retryCount, maxBackoffTimeInSeconds);
        }
    }
}