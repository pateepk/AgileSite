namespace CMS.UIControls
{
    /// <summary>
    /// Defines when the UI Layout JavaScript plug-in should be initialized.
    /// </summary>
    public enum LoadModeEnum
    {
        /// <summary>
        /// Inline
        /// </summary>
        Inline,

        /// <summary>
        /// Document ready
        /// </summary>
        DocumentReady,

        /// <summary>
        /// Window load
        /// </summary>
        WindowLoad
    }
}