using System;
using System.ComponentModel;
using System.Web.Http.Description;
using System.Web.Mvc;

using Kentico.Content.Web.Mvc;
using Kentico.Web.Mvc;

using Newtonsoft.Json;

namespace Kentico.Components.Web.Mvc.Dialogs.Internal
{
    /// <summary>
    /// Represents dialog of media files selector.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [ApiExplorerSettings(IgnoreApi = true)]
    [VirtualContextAuthorize]
    public sealed class KenticoMediaFilesDialogController : Controller
    {
        private readonly IMediaLibraryFolderStructureLoader folderStructureLoader;
        private readonly IMediaLibraryDataLoader dataLoader;

        /// <summary>
        /// Creates an instance of the <see cref="KenticoMediaFilesDialogController"/> class.
        /// </summary>
        public KenticoMediaFilesDialogController()
            : this(new MediaLibraryFolderStructureLoader(), new MediaLibraryDataLoader())
        {
        }


        /// <summary>
        /// Creates an instance of the <see cref="KenticoMediaFilesDialogController"/> class.
        /// </summary>
        /// <param name="folderStructureLoader">Loader of media library folder structure.</param>
        /// <param name="dataLoader">Media data loader.</param>
        internal KenticoMediaFilesDialogController(IMediaLibraryFolderStructureLoader folderStructureLoader, IMediaLibraryDataLoader dataLoader)
        {
            this.folderStructureLoader = folderStructureLoader ?? throw new ArgumentNullException(nameof(folderStructureLoader));
            this.dataLoader = dataLoader ?? throw new ArgumentNullException(nameof(dataLoader));
        }


        /// <summary>
        /// Default controller action.
        /// </summary>
        [HttpGet]
        [MediaLibraryActionFilter("libraryName", "LibraryAccess", DisplayErrorMessage = true, LibraryNameRequired = false)]
        public ActionResult Index(string libraryName)
        {
            // libraryName parameter is required for the action attribute
            return View("~/Views/Shared/Kentico/Selectors/Dialogs/MediaFiles/_Dialog.cshtml", new KenticoMediaFilesDialogViewModel
            {
                DataEndpointUrl = Url.Kentico().AuthenticateUrl(Url.Action("GetFiles")).ToString(),
                TreeDataEndpointUrl = Url.Kentico().AuthenticateUrl(Url.Action("GetFolderStructure")).ToString(),
                ModelDataEndpointUrl = Url.Kentico().AuthenticateUrl(Url.Action("GetSelectedFiles")).ToString(),
                LibrariesDataEndpointUrl = Url.Kentico().AuthenticateUrl(Url.Action("GetLibraries")).ToString(),
                UploaderEndpointUrl = Url.Kentico().AuthenticateUrl(Url.HttpRouteUrl(SelectorRoutes.MEDIA_FILES_UPLOADER_ROUTE_NAME, null)).ToString()
            });
        }


        /// <summary>
        /// Gets media files for selector dialog.
        /// </summary>
        /// <param name="libraryName">Name of the media library.</param>
        /// <param name="folderPath">Folder in which media files are placed.</param>
        /// <param name="allowedExtensions">Semicolon separated list of allowed file extensions.</param>
        [HttpGet]
        [MediaLibraryActionFilter("libraryName", "LibraryAccess")]
        public ContentResult GetFiles(string libraryName, string folderPath, string allowedExtensions)
        {
            folderPath = String.IsNullOrEmpty(folderPath) ? string.Empty : $"{folderPath}/";
            var files = dataLoader.LoadFiles(libraryName, folderPath, allowedExtensions, Url);

            return GetJsonResult(files);
        }


        /// <summary>
        /// Gets media files folder structure.
        /// </summary>
        /// <param name="libraryName">Name of the media library.</param>
        [HttpGet]
        [MediaLibraryActionFilter("libraryName", "LibraryAccess")]
        public ContentResult GetFolderStructure(string libraryName)
        {
            var node = folderStructureLoader.Load(libraryName);

            return GetJsonResult(node);
        }


        /// <summary>
        /// Gets model for selected values of files.
        /// </summary>
        /// <param name="values">List of selected values.</param>
        [HttpPost]
#pragma warning disable SCS0016 // Controller method is vulnerable to CSRF
        public ContentResult GetSelectedFiles(Guid[] values)
#pragma warning restore SCS0016 // Controller method is vulnerable to CSRF
        {
            if (values == null)
            {
                return null;
            }

            var files = dataLoader.LoadFiles(values, Url);

            return GetJsonResult(files);
        }


        /// <summary>
        /// Gets libraries for current site.
        /// </summary>
        [HttpGet]
        public ContentResult GetLibraries()
        {
            var libraries = dataLoader.LoadLibraries();

            return GetJsonResult(libraries);
        }


        private ContentResult GetJsonResult(object model)
        {
            return Content(JsonConvert.SerializeObject(model), "application/json");
        }
    }
}
