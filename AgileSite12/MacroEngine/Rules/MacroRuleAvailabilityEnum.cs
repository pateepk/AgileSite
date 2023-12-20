namespace CMS.MacroEngine
{
    /// <summary>
    /// Indicates in which application can the <see cref="MacroRuleInfo"/> be evaluated.
    /// </summary>
    public enum MacroRuleAvailabilityEnum
    {
        /// <summary>
        /// Macro rule can be evaluated in the main application (i.e. the implementation of underlying macros is available).
        /// </summary>
        MainApplication = 0,


        /// <summary>
        /// Macro rule can be evaluated in the MVC live-site (i.e. the implementation of underlying macros is available).
        /// </summary>
        MvcLiveSite = 1,


        /// <summary>
        /// Macro rule can be evaluated in both applications (i.e. the implementation of underlying macros is available).
        /// </summary>
        Both = 2
    }
}
