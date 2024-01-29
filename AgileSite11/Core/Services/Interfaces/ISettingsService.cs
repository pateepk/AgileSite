namespace CMS.Core
{
    /// <summary>
    /// Settings service interface
    /// </summary>
    public interface ISettingsService
    {
        /// <summary>
        /// Returns true if the settings service is available
        /// </summary>
        bool IsAvailable
        {
            get;
        }


        /// <summary>
        /// Gets the specific settings from the database
        /// </summary>
        /// <param name="keyName">Settings key</param>
        string this[string keyName]
        {
            get;
        }
    }
}
