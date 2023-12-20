using System;
using System.Threading;

using NUnit.Framework.Constraints;

namespace CMS.Tests
{
    /// <summary>
    /// Applies an exponential delay to the match so that a match can be evaluated in the future.
    /// The polling interval is 0, 1, 2, 4, 8, ... up to given maximum seconds for each retry.
    /// </summary>
    internal class ExponentiallyDelayedConstraint : PrefixConstraint
    {
        private readonly int mRetryCount;
        private readonly int mMaxBackoffTimeInSeconds;


        /// <summary>
        /// Gets text describing a constraint
        /// </summary>
        public override string Description
        {
            get
            {
                return $"{BaseConstraint.Description} after {mRetryCount} retries in {GetTotalBackoffInSeconds()} seconds";
            }
        }


        /// <summary>
        /// Creates a new ExponentiallyDelayedConstraint with maximum backoff interval 60 seconds.
        /// </summary>
        /// <param name="baseConstraint">The inner constraint to decorate.</param>
        /// <param name="retryCount">Retry count.</param>
        /// <exception cref="ArgumentException">If the value of <paramref name="retryCount"/> is less or equal 0.</exception>
        public ExponentiallyDelayedConstraint(IConstraint baseConstraint, int retryCount)
            : this(baseConstraint, retryCount, 60)
        {
        }


        /// <summary>
        /// Creates a new ExponentiallyDelayedConstraint.
        /// </summary>
        /// <param name="baseConstraint">The inner constraint to decorate.</param>
        /// <param name="retryCount">Retry count.</param>
        /// <param name="maxBackoffTimeInSeconds">Maximal backoff interval.</param>
        /// <exception cref="ArgumentException">If the value of <paramref name="retryCount"/> is less or equal 0.</exception>
        /// <exception cref="ArgumentException">If the value of <paramref name="maxBackoffTimeInSeconds"/> is less or equal 0.</exception>
        public ExponentiallyDelayedConstraint(IConstraint baseConstraint, int retryCount, int maxBackoffTimeInSeconds)
            : base(baseConstraint)
        {
            if (retryCount <= 0)
            {
                throw new ArgumentException("Retry count must be greater than 0.", nameof(retryCount));
            }

            if (maxBackoffTimeInSeconds <= 0)
            {
                throw new ArgumentException("Maximal backoff time must be greater than 0.", nameof(maxBackoffTimeInSeconds));
            }

            mRetryCount = retryCount;
            mMaxBackoffTimeInSeconds = maxBackoffTimeInSeconds;
        }


        /// <summary>
        /// Test whether the constraint is satisfied by a given value.
        /// Overridden to exponential backoff wait before calling the base constraint.
        /// </summary>
        /// <param name="actual">The value to be tested.</param>
        /// <returns>A ConstraintResult.</returns>
        public override ConstraintResult ApplyTo<TActual>(TActual actual)
        {
            if (BaseConstraint.ApplyTo(actual).IsSuccess)
            {
                return new ConstraintResult(this, actual, true);
            }

            for (int i = 0; i < mRetryCount; i++)
            {
                ExponentialBackoff(i);

                if (BaseConstraint.ApplyTo(actual).IsSuccess)
                {
                    return new ConstraintResult(this, actual, true);
                }
            }

            return new ConstraintResult(this, actual, false);
        }


        /// <summary>
        /// Test whether the constraint is satisfied by a delegate.
        /// Overridden to exponential backoff wait before calling the base constraint with result of delegate invoking.
        /// </summary>
        /// <param name="del">The delegate whose value is to be tested.</param>
        /// <returns>A ConstraintResult.</returns>
        public override ConstraintResult ApplyTo<TActual>(ActualValueDelegate<TActual> del)
        {
            object obj = null;

            // starting from -1 to cover first iteration as a standard try and then perform mRetryCount retries
            for (int i = -1; i < mRetryCount; i++)
            {
                ExponentialBackoff(i);

                try
                {
                    obj = del();
                    if (BaseConstraint.ApplyTo(obj).IsSuccess)
                    {
                        return new ConstraintResult(this, obj, isSuccess: true);
                    }
                }
                catch when (i < mRetryCount - 1)
                {
                    // Ignore any exceptions when polling except last retry
                }
            }

            // All retries were unsuccessful
            return new ConstraintResult(this, obj, isSuccess: false);
        }


        /// <summary>
        /// Test whether the constraint is satisfied by a given reference.
        /// Overridden to exponential backoff wait before calling the base constraint with the dereferenced value.
        /// </summary>
        /// <param name="actual">A reference to the value to be tested.</param>
        /// <returns>A ConstraintResult.</returns>
        public override ConstraintResult ApplyTo<TActual>(ref TActual actual)
        {
            if (BaseConstraint.ApplyTo(actual).IsSuccess)
            {
                return new ConstraintResult(this, actual, true);
            }

            for (int i = 0; i < mRetryCount; i++)
            {
                ExponentialBackoff(i);

                if (BaseConstraint.ApplyTo(actual).IsSuccess)
                {
                    return new ConstraintResult(this, actual, true);
                }
            }

            return new ConstraintResult(this, actual, false);
        }


        /// <summary>
        /// Returns the string representation of the constraint.
        /// </summary>
        protected override string GetStringRepresentation()
        {
            return $"<after {mRetryCount} retries in {GetTotalBackoffInSeconds()} seconds {BaseConstraint}>";
        }


        private int GetTotalBackoffInSeconds()
        {
            int totalTime = 0;
            for (int i = 0; i < mRetryCount; i++)
            {
                totalTime += GetBackoffTime(i);
            }

            return totalTime;
        }


        private void ExponentialBackoff(int iteration)
        {
            var backoffTimeInSeconds = GetBackoffTime(iteration);
            if (backoffTimeInSeconds == 0)
            {
                return;
            }

            Thread.Sleep(backoffTimeInSeconds * 1000);
        }


        /// <summary>
        /// Gets backoff time (0, 1, 2, 4, 8, ... limited by <see cref="mMaxBackoffTimeInSeconds"/>).
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

            return Math.Min(result, mMaxBackoffTimeInSeconds);
        }
    }
}