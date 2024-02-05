namespace CMS.WebFarmSync.Internal
{
    /// <summary>
    /// Contains methods related to licensing of web farms. For internal use only. 
    /// </summary>
    /// TODO: When extenders relocated, delete this class.
    public class WebFarmLicenseHelper
    {
        /// <summary>
        /// Indicates if there are proper licenses for web farms in the system.
        /// </summary>
        public static bool LicenseIsValid
        {
            get
            {
                return WebFarmSync.WebFarmLicenseHelper.IsWebFarmLicenseValid();
            }
        }
    }
}
