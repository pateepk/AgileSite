
namespace Kentico.Components.Web.Mvc
{
    internal static class SelectorRoutes
    {
        /// <summary>
        /// Default action name for selector dialogs.
        /// </summary>
        public const string DEFAULT_ACTION_NAME = "Index";

        /// <summary>
        /// Name of the selctors route.
        /// </summary>
        public const string SELECTORS_ROUTE_NAME = "Kentico.Components.Selectors";

        
        /// <summary>
        /// Default route for Page builder selectors.
        /// </summary>
        public const string SELECTORS_ROUTE = "Kentico.Components/Dialogs/{controller}/{action}";


        /// <summary>
        /// Media files selector dialog controller name.
        /// </summary>
        public const string MEDIA_FILES_SELECTOR_CONTROLLER_NAME = "KenticoMediaFilesDialog";


        /// <summary>
        /// Media files selector dialog path.
        /// </summary>
        public const string MEDIA_FILES_SELECTOR_ROUTE = "Kentico.Components/Dialogs/KenticoMediaFilesDialog";
        
        
        /// <summary>
        /// Name of the media files uploader route.
        /// </summary>
        public const string MEDIA_FILES_UPLOADER_ROUTE_NAME = "Kentico.Components.MediaFilesUploader";


        /// <summary>
        /// Media files uploader controller name.
        /// </summary>
        public const string MEDIA_FILES_UPLOADER_CONTROLLER_NAME = "KenticoMediaFilesUploader";


        /// <summary>
        /// Media files uploader path.
        /// </summary>
        public const string MEDIA_FILES_UPLOADER_ROUTE = "Kentico.Uploaders/" + MEDIA_FILES_UPLOADER_CONTROLLER_NAME + "/Upload";


        /// <summary>
        /// Page selector controller name.
        /// </summary>
        public const string PAGE_SELECTOR_CONTROLLER_NAME = "KenticoPageSelectorDialog";


        /// <summary>
        /// Page selector dialog path.
        /// </summary>
        public const string PAGE_SELECTOR_ROUTE = "Kentico.Components/Dialogs/KenticoPageSelectorDialog";
    }
}
