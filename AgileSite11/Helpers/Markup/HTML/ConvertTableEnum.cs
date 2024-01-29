namespace CMS.Helpers
{
    /// <summary>
    /// Table to DIV conversion setting enum
    /// </summary>
    public enum ConvertTableEnum : int
    {
        /// <summary>
        /// No conversion
        /// </summary>
        None = 0,

        /// <summary>
        /// All except marked with class="_nodivs"
        /// </summary>
        All = 1,

        /// <summary>
        /// All explicitly marked with class="_divs"
        /// </summary>
        Marked = 2
    }
}
