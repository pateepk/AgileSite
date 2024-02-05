namespace CMS.DataEngine
{
    /// <summary>
    /// Enumerates the possible actions when changing the settings key.
    /// </summary>
    public enum SettingsKeyActionEnum
    {
        /// <summary>
        /// Specifies that the settings key had already existed and was updated.
        /// </summary>
        Update,


        /// <summary>
        /// Specifies that the settings key was newly created.
        /// </summary>
        Insert,


        /// <summary>
        /// Specifies that the settings key was removed.
        /// </summary>
        Delete
    }
}