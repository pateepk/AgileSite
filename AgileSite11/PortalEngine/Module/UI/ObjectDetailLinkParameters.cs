namespace CMS.PortalEngine
{
    /// <summary>
    /// Object detail link parameters.
    /// </summary>
    /// <remarks>
    /// Use when creating link to the object detail.
    /// </remarks>
    public class ObjectDetailLinkParameters
    {
        /// <summary>
        /// Object id or object name. Can be left empty for object creation to be shown.
        /// </summary>
        public object ObjectIdentifier { get; set; }


        /// <summary>
        /// Name of the parent tab name. Will be selected when navigating back in tabs.
        /// </summary>
        public string ParentTabName { get; set; }


        /// <summary>
        /// Navigation to the parent tabs will be possible if enabled.
        /// </summary>
        public bool AllowNavigationToListing { get; set; }


        /// <summary>
        /// Name of the tab. Can be used to navigate to an individual tab.
        /// </summary>
        public string TabName { get; set; }


        /// <summary>
        /// Parent id or parent name.
        /// </summary>
        public object ParentObjectIdentifier { get; set; }


        /// <summary>
        /// Gets or sets whether the given link should remain in the browser navigation bar, enabling the window reload.
        /// By default the link is not persistent, after navigating to the single object, URL is discarded and reload will lead to the 
        /// application root.
        /// </summary>
        public bool Persistent { get; set; }
    }
}