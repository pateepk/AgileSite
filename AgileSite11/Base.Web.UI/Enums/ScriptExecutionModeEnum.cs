namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Javascript execution modes enumeration.
    /// </summary>
    public enum ScriptExecutionModeEnum : int
    {
        /// <summary>
        /// Default, parser blocking execution mode.
        /// </summary>
        Normal = 0,

        /// <summary>
        /// Downloads the script without blocking the parser and executes once it's finished.
        /// </summary>
        Asynchronous = 1,

        /// <summary>
        /// Delays script execution until the parser has finished.
        /// </summary>
        Deferred = 2
    }
}
