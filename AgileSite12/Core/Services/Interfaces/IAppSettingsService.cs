namespace CMS.Core
{
    /// <summary>
    /// AppSettings service interface
    /// </summary>
    public interface IAppSettingsService
    {
        /// <summary>
        /// Gets the specific settings from the app config
        /// </summary>
        string this[string key]
        {
            get;
        }
    }
}
