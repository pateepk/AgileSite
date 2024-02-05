namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Object order enumeration (for newly inserted object).
    /// </summary>
    public enum ObjectOrderEnum
    {
        /// <summary>
        /// Created objects will be put as last.
        /// </summary>
        Last = 0,

        /// <summary>
        /// Created objects will be put as first.
        /// </summary>
        First = 1,

        /// <summary>
        /// Newly created objects will be put in alphabetical order (assuming the rest is alphabetically ordered).
        /// </summary>
        Alphabetical = 2,
    }
}