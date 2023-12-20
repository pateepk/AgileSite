namespace Kentico.Components.Web.Mvc.Dialogs.Internal
{
    /// <summary>
    /// View model for media files selector dialog.
    /// </summary>
    public sealed class KenticoMediaFilesDialogViewModel
    {
        /// <summary>
        /// Data endpoint URL.
        /// </summary>
        public string DataEndpointUrl { get; set; }


        /// <summary>
        /// Tree data endpoint URL.
        /// </summary>
        public string TreeDataEndpointUrl { get; set; }


        /// <summary>
        /// Endpoint URL for retrieving model of selected values.
        /// </summary>
        public string ModelDataEndpointUrl { get; set; }


        /// <summary>
        /// Endpoint URL for retrieving model of media libraries.
        /// </summary>
        public string LibrariesDataEndpointUrl { get; set; }


        /// <summary>
        /// Uploader endpoint URL.
        /// </summary>
        public string UploaderEndpointUrl { get; set; }
    }
}
