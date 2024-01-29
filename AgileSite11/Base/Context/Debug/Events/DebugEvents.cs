namespace CMS.Base
{
    /// <summary>
    /// Debug events handlers.
    /// </summary>
    public static class DebugEvents
    {
        /// <summary>
        /// Fires when a new macro debug item is logged to debug.
        /// </summary>
        public static DebugHandler MacroDebugItemLogged = new DebugHandler { Name = "DebugEvents.MacroDebugItemLogged" };


        /// <summary>
        /// Fires when a new SQL debug item is logged to debug.
        /// </summary>
        public static DebugHandler SQLDebugItemLogged = new DebugHandler { Name = "DebugEvents.SQLDebugItemLogged" };


        /// <summary>
        /// Fires when a new IO debug item is logged to debug.
        /// </summary>
        public static DebugHandler IODebugItemLogged = new DebugHandler { Name = "DebugEvents.IODebugItemLogged" };


        /// <summary>
        /// Fires when a new cache debug item is logged to debug.
        /// </summary>
        public static DebugHandler CacheDebugItemLogged = new DebugHandler { Name = "DebugEvents.CacheDebugItemLogged" };


        /// <summary>
        /// Fires when a new macro debug item is logged to debug.
        /// </summary>
        public static DebugHandler ViewStateDebugItemLogged = new DebugHandler { Name = "DebugEvents.ViewStateDebugItemLogged" };


        /// <summary>
        /// Fires when a new security debug item is logged to debug.
        /// </summary>
        public static DebugHandler SecurityDebugItemLogged = new DebugHandler { Name = "DebugEvents.SecurityDebugItemLogged" };


        /// <summary>
        /// Fires when a new analytics debug item is logged to debug.
        /// </summary>
        public static DebugHandler AnalyticsDebugItemLogged = new DebugHandler { Name = "DebugEvents.AnalyticsDebugItemLogged" };


        /// <summary>
        /// Fires when debug settings are reset. The event arguments do not contain any debug data.
        /// </summary>
        public static DebugHandler SettingsReset = new DebugHandler { Name = "DebugEvents.SettingsReset" };
    }
}