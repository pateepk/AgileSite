﻿using CMS;
using CMS.Activities;
using CMS.Core;
using CMS.Helpers;

using Kentico.Activities.Web.Mvc;

[assembly: RegisterImplementation(typeof(IActivityUrlService), typeof(ActivityMvcUrlService), Priority = RegistrationPriority.SystemDefault)]

namespace Kentico.Activities.Web.Mvc
{
    /// <summary>
    /// Provides methods to get correct URL and URL referrer to insert in <see cref="IActivityInfo" /> for MVC.
    /// </summary>
    internal class ActivityMvcUrlService : IActivityUrlService
    {
        /// <summary>
        /// Gets URL of request the activity was logged for. In this case it is equal to referrer.
        /// </summary>
        /// <returns>URL</returns>
        public string GetActivityUrl()
        {
            return RequestContext.URLReferrer;
        }


        /// <summary>
        /// Gets URL referrer of request the activity was referred from. Empty string in this case.
        /// </summary>
        /// <returns>Empty string</returns>
        public string GetActivityUrlReferrer()
        {
            return string.Empty;
        }
    }
}
