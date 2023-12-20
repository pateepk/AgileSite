using System;

using CMS;
using CMS.Activities;
using CMS.Core;
using CMS.DocumentEngine;
using CMS.Helpers;

[assembly: RegisterImplementation(typeof(IRatingActivityLogger), typeof(RatingActivityLogger), Priority = RegistrationPriority.SystemDefault, Lifestyle = Lifestyle.Singleton)]

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Provides method for logging rating activity.
    /// </summary>
    public class RatingActivityLogger : IRatingActivityLogger
    {
        private readonly IActivityLogService mActivityLogService = Service.Resolve<IActivityLogService>();


        /// <summary>
        /// Logs rating activity.
        /// </summary>
        /// <param name="value">Rating value</param>
        /// <param name="currentDocument">Rated document</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="currentDocument"/> is <c>null</c>.</exception>
        public void LogRating(double value, TreeNode currentDocument)
        {
            if (currentDocument == null)
            {
                throw new ArgumentNullException("currentDocument");
            }

            var initializer = new RatingActivityInitializer(value, currentDocument);
            mActivityLogService.Log(initializer, CMSHttpContext.Current.Request);
        }
    }
}