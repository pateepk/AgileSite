using System;
using System.Linq;
using System.Net;
using System.Text;

using CMS;
using CMS.Base;
using CMS.SocialMarketing;

using LinqToTwitter;

[assembly: RegisterImplementation(typeof(ITwitterRetryPolicyProvider), typeof(TwitterRetryPolicyProvider), Priority = CMS.Core.RegistrationPriority.SystemDefault)]

namespace CMS.SocialMarketing
{
    /// <summary>
    /// Provides methods for performing retry policy when the exception is caused by a network problem.
    /// </summary>
    internal class TwitterRetryPolicyProvider : ITwitterRetryPolicyProvider
    {
        private const int RETRY_LIMIT = 3;
        private static readonly CMSStatic<int> mRetryCount = new CMSStatic<int>();

        /// <summary>
        /// Increases retry count, and checks whether the count exceeds retry limit.
        /// </summary>
        /// <returns>True if the policy was successfully applied and another try can be performed; false if the exception should be handled in standard pipeline.</returns>
        public bool ApplyRetryPolicy()
        {
            mRetryCount.Value++;
            return (mRetryCount.Value <= RETRY_LIMIT);
        }


        /// <summary>
        /// Checks whether the given <paramref name="exception"/> is eligible for the retry policy. Given exception has to be <see cref="TwitterQueryException"/>, 
        /// cannot have known error code provided by twitter API and the underlying inner exception has to be <see cref="WebException"/>.
        /// In other cases the exception is not eligible and should be proceeded in the standard pipeline.
        /// </summary>
        /// <param name="exception">Exception to be checked</param>
        /// <returns>True if the exception is candidate for retry policy; otherwise, false</returns>
        public bool IsEligible(Exception exception)
        {
            var twitterException = exception as TwitterQueryException;
            return (twitterException != null) && (twitterException.ErrorCode == 0) && (twitterException.InnerException is WebException);
        }
        
        
        /// <summary>
        /// Resets the retry count to zero.
        /// </summary>
        public void Reset()
        {
            mRetryCount.Value = 0;
        }
    }
}
