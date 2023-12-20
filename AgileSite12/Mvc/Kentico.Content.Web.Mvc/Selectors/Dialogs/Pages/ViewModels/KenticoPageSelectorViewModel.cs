namespace Kentico.Components.Web.Mvc.Dialogs.Internal
{
    /// <summary>
    /// View model containing data for page selector initialization.
    /// </summary>
    public sealed class KenticoPageSelectorViewModel
    {
        /// <summary>
        /// Name of site from which the seletor loads pages data.
        /// </summary>
        public string SiteName { get; set; }

        /// <summary>
        /// Culture version of pages displayed in the selector.
        /// </summary>
        public string Culture { get; set; }

        /// <summary>
        /// End-point URL where the selector obtains child nodes data of a given parent node.
        /// </summary>
        public string GetChildPagesEndpointUrl { get; set; }


        /// <summary>
        /// The URL of an end-point which translates page GUID to page alias path.
        /// </summary>
        public string GetAliasPathEndpointUrl { get; set; }

        /// <summary>
        /// Metadata of the root page in the selector.
        /// </summary>
        public PageSelectorItemModel RootPage { get; set; }
    }
}
