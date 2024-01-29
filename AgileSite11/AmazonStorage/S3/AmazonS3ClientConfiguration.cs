using CMS.Base;
using CMS.Helpers;

namespace CMS.AmazonStorage
{
    /// <summary>
    /// Configuration class for Amazon S3 clients.
    /// </summary>
    /// <seealso cref="AccountInfo"/>
    public static class AmazonS3ClientConfiguration
    {
        /// <summary>
        /// Default value of <see cref="RestApiEndPointUrl"/> property.
        /// </summary>
        /// <remarks>
        /// The default URL is <c>https://s3.amazonaws.com</c>.
        /// </remarks>
        public const string DEFAULT_REST_API_END_POINT_URL = "https://s3.amazonaws.com";


        private static string mRestApiEndPointUrl;


        /// <summary>
        /// Gets or sets the URL of the Amazon REST API endpoint. The value is read from <c>CMSAmazonRestApiEndPoint</c> application configuration key, unless explicitly set.
        /// Returns <see cref="DEFAULT_REST_API_END_POINT_URL"/> if the application configuration does not specify any URL.
        /// </summary>
        public static string RestApiEndPointUrl
        {
            get
            {
                return mRestApiEndPointUrl ?? (mRestApiEndPointUrl = ValidationHelper.GetString(SettingsHelper.AppSettings["CMSAmazonRestApiEndPoint"], DEFAULT_REST_API_END_POINT_URL));
            }
            set
            {
                mRestApiEndPointUrl = value;
            }
        }
    }
}
