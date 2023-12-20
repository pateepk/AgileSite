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
        public static readonly SimpleSystemHandler Exception = new SimpleSystemHandler { Name = "SystemEvents.Exception" };


        /// <summary>
        /// Fires when request for application restart was performed.
        /// </summary>
        public static readonly RestartRequiredHandler RestartRequired = new RestartRequiredHandler { Name = "SystemEvents.RestartRequired" };
    }
}