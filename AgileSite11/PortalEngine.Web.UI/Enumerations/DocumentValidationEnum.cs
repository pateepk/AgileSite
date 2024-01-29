namespace CMS.PortalEngine.Web.UI
{
    /// <summary>
    /// Document validation type
    /// </summary>   
    public enum DocumentValidationEnum : int
    {
        /// <summary>
        /// Document CSS validation
        /// </summary>
        CSS = 0,

        /// <summary>
        /// Document (X)HTML validation
        /// </summary>
        HTML = 1,

        /// <summary>
        /// Document validation for broken links
        /// </summary>
        Link = 2,

        /// <summary>
        /// (X)HTML document accessibility validation
        /// </summary>
        Accessibility
    }
}