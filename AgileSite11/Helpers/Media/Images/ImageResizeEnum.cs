namespace CMS.Helpers
{
    /// <summary>
    /// Image resize enumeration.
    /// </summary>
    public enum ImageResizeEnum : int
    {
        /// <summary>
        /// Default resize behavior according to site settings.
        /// </summary>
        Auto = 0,

        /// <summary>
        /// Use URL settings
        /// </summary>
        Force = 1
    }
}
