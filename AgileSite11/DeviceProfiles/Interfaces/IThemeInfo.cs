namespace CMS.DeviceProfiles
{
    /// <summary>
    /// Interface for the themeable info objects
    /// </summary>
    public interface IThemeInfo
    {
        #region "Public properties"

        /// <summary>
        /// Indicates whether the theme path points at an external storage.
        /// </summary>
        bool UsesExternalStorage
        {
            get;
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Gets the theme path for the object
        /// </summary>
        string GetThemePath();

        #endregion
    }
}
