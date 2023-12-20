namespace CMS.OnlineMarketing.Web.UI
{
    /// <summary>
    /// Describes implementation marking document node in pages application by additional HTML code.
    /// </summary>
    internal interface IABDocumentMarker
    {
        /// <summary>
        /// Returns HTML code of all A/B icons to mark the document specified in constructor.
        /// </summary>
        /// <returns>HTML of all icons marking the specified document</returns>
        string GetIcons();
    }
}