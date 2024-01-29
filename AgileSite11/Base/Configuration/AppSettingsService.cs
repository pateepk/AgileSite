using CMS.Core;

namespace CMS.Base
{
    /// <summary>
    /// AppSettings service
    /// </summary>
    public class AppSettingsService : IAppSettingsService
    {
        /// <summary>
        /// Gets the specific settings from the app config
        /// </summary>
        public string this[string key]
        {
            get 
            {
                return SettingsHelper.AppSettings[key];    
            }
        }
    }
}
