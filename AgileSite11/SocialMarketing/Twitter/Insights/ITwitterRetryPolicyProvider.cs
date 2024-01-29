using System;

namespace CMS.SocialMarketing
{
    /// <summary>
    /// Provides methods for performing retry policy when the exception is caused by a network problem.
    /// </summary>
    internal interface ITwitterRetryPolicyProvider
    {
        /// <summary>
        /// Applies retry policy and checks whether the exception should be handled in standard pipeline, or should be suppressed.
        /// </summary>
        /// <returns>True if the policy was successfully applied and another try can be performed; false if the exception should be handled in standard pipeline.</returns>
        bool ApplyRetryPolicy();


        /// <summary>
        /// Checks whether the given <paramref name="exception"/> is eligible to be used in retry policy.
        /// </summary>
        /// <param name="exception">Exception to be checked</param>
        /// <returns>True if the exception is candidate for retry policy; otherwise, false</returns>
        bool IsEligible(Exception exception);


        /// <summary>
        /// Ensures reset of retry policy. Should be called when <see cref="ApplyRetryPolicy"/> returns false so next attempt starts from the initial state.
        /// </summary>
        void Reset();
    }
}