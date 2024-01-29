using CMS.Helpers;
using CMS.Base;

namespace CMS.AmazonStorage
{
    /// <summary>
    /// Helper methods and properties for Amazon web services integration.
    /// </summary>
    public static class AmazonHelper
    {
        #region "Constants"

        /// <summary>
        /// Path to azure file page.
        /// </summary>
        public const string AMAZON_FILE_PAGE = "~/CMSPages/GetAmazonFile.aspx";

        #endregion


        #region "Variables"

        private static bool? mPublicAccess = null;
        private static string mEndPoint = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Returns whether Amazon S3 storage sets public read access to uploaded files and whether direct storage links are generated.
        /// </summary>
        public static bool PublicAccess
        {
            get
            {
                if (mPublicAccess == null)
                {
                    GetEndPointAndAccess();
                }

                return mPublicAccess.Value;
            }
            set
            {
                mPublicAccess = value;
            }
 


        }


        /// <summary>
        /// Returns Amazon S3 endpoint.
        /// </summary>
        public static string EndPoint
        {
            get
            {
                if (mEndPoint == null)
                {
                    mEndPoint = ValidationHelper.GetString(SettingsHelper.AppSettings["CMSAmazonEndPoint"], null);
                    GetEndPointAndAccess();
                }

                return mEndPoint;
            }
            set
            {
                mEndPoint = value;
            }
        }

        #endregion


        #region "Public Methods"

        /// <summary>
        /// Returns download path for given path.
        /// </summary>
        /// <param name="path">Path</param>
        public static string GetDownloadPath(string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                return AMAZON_FILE_PAGE + "?path=" + URLHelper.EscapeSpecialCharacters(path);
            }

            return null;
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Gets end point and public access from web.config and sets properties.
        /// </summary>
        private static void GetEndPointAndAccess()
        {
            mEndPoint = ValidationHelper.GetString(SettingsHelper.AppSettings["CMSAmazonEndPoint"], null);

            // Get default endpoint
            if (mEndPoint == null)
            {
                mEndPoint = "http://" + SettingsHelper.AppSettings["CMSAmazonBucketName"] + ".s3.amazonaws.com";
                mPublicAccess = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSAmazonPublicAccess"], false);
            }
            else
            {
                mEndPoint = URLHelper.AddHTTPToUrl(mEndPoint);

                // Set up public access to true if not specified to false by key
                mPublicAccess = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSAmazonPublicAccess"], true);
            }
        }

        #endregion
    }
}
