namespace CMS.LicenseProvider
{
    /// <summary>
    /// License check events
    /// </summary>
    internal static class LicenseCheckEvents
    {
        /// <summary>
        /// Fires when checking the current number of feature-related objects that are limited by license limitations.
        /// </summary>
        public static ObjectCountCheckHandler ObjectCountCheckEvent = new ObjectCountCheckHandler { Name = "LicenseCheckEvents.ObjectCountCheckEvent" };
    }
}
