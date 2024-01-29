using System;
using System.ComponentModel;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

using CMS.Core;
using CMS.DataEngine;

namespace CMS.WebApi
{
    /// <summary>
    /// Restrict access when necessary feature is not available in the best license.
    /// </summary>
    /// <remarks>
    /// This API supports the framework infrastructure and is not intended to be used directly from your code.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class IsFeatureAvailableAttribute : AuthorizationFilterAttribute
    {
        private const string DEFAULT_ERROR_MESSAGE = "Feature {0} is not available under your license.";

        private readonly string mMessage;
        private readonly FeatureEnum mFeature;
        private ILicenseService mLicenseService;


        internal ILicenseService LicenseService
        {
            get
            {
                return mLicenseService ?? (mLicenseService = ObjectFactory<ILicenseService>.StaticSingleton());
            }
            set
            {
                mLicenseService = value;
            }
        }


        /// <summary>
        /// Creates feature available attribute. Feature will be checked similarly to <see cref="ILicenseService.IsFeatureAvailable"/>.
        /// </summary>
        /// <param name="feature">Necessary feature</param>
        /// <param name="message">Error message, if null or empty <see cref="DEFAULT_ERROR_MESSAGE"/> is used</param>
        public IsFeatureAvailableAttribute(FeatureEnum feature, string message = null)
        {
            mFeature = feature;
            mMessage = string.IsNullOrEmpty(message) ? DEFAULT_ERROR_MESSAGE : message;
        } 

        
        /// <summary>
        /// Restrict access when necessary feature is not available in the best license.
        /// </summary>
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            if (!LicenseService.IsFeatureAvailable(mFeature, String.Empty))
            {
                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Forbidden, String.Format(mMessage, mFeature));
            }
        }
    }
}
