namespace CMS.Helpers
{
    /// <summary>
    /// Letter case and redirection options for URLs.
    /// </summary>
    public enum CaseRedirectEnum : int
    {
        /// <summary>
        /// No check or redirection.
        /// </summary>
        None = 0,

        /// <summary>
        /// Alias redirection.
        /// </summary>
        Exact = 1,

        /// <summary>
        /// Redirect to lowercase variant.
        /// </summary>
        LowerCase = 2,

        /// <summary>
        /// Redirect to uppercase variant.
        /// </summary>
        UpperCase = 3
    }
}