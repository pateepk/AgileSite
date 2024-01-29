namespace CMS.Base
{
    /// <summary>
    /// System events
    /// </summary>
    public static class SystemEvents
    {
        /// <summary>
        /// Fires when exception occurs
        /// </summary>
        public static SimpleSystemHandler Exception = new SimpleSystemHandler { Name = "SystemEvents.Exception" };
    }
}