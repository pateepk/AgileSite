using System;
using System.ComponentModel;
using System.Net;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;

using CMS.Base;
using CMS.Core;
using CMS.IO;
using CMS.MediaLibrary;
using CMS.Membership;

namespace Kentico.Components.Web.Mvc.Dialogs.Internal
{
    /// <summary>
    /// Controller handles upload of media files.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [ApiExplorerSettings(IgnoreApi = true)]
    [VirtualContextAuthorize]
    public sealed class KenticoMediaFilesUploaderController : ApiController
    {
        private readonly IEventLogService eventLogService;
        private readonly ISiteService siteService;


        /// <summary>
        /// Creates instance of <see cref="KenticoMediaFilesUploaderController"/>.
        /// </summary>
        public KenticoMediaFilesUploaderController()
        {
            eventLogService = Service.Resolve<IEventLogService>();
            siteService = Service.Resolve<ISiteService>();
        }


        /// <summary>
        /// Uploads file to specified media library and its subfolder.
        /// </summary>
        /// <param name="libraryName">Name of the media library.</param>
        /// <param name="librarySubfolder">Folder in which media files are placed.</param>
        [HttpPost]
#pragma warning disable SCS0016 // Controller method is vulnerable to CSRF
        public void Upload(string libraryName, string librarySubfolder)
#pragma warning restore SCS0016 // Controller method is vulnerable to CSRF
        {
            try
            {
                var library = MediaLibraryInfoProvider.GetMediaLibraryInfo(libraryName, siteService.CurrentSite.SiteName);
                if (library == null)
                {
                    throw new InvalidOperationException("Specified library does not exist.");
                }

                if (!MediaLibraryInfoProvider.IsUserAuthorizedPerLibrary(library, "FileCreate", MembershipContext.AuthenticatedUser))
                {
                    throw new UnauthorizedAccessException("You are not authorized to create file in this library.");
                }

                foreach (var requestFileName in HttpContext.Current.Request.Files.AllKeys)
                {
                    AddMediaFile(requestFileName, library.LibraryID, librarySubfolder);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                eventLogService.LogException("MediaFilesUploader", "Upload", ex);
                throw new HttpResponseException(HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                eventLogService.LogException("MediaFilesUploader", "Upload", ex);
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }
        }


        private void AddMediaFile(string requestFileName, int libraryId, string librarySubfolder)
        {
            var file = HttpContext.Current.Request.Files[requestFileName];
            ValidateFile(file);

            var mediaFile = new MediaFileInfo(file, libraryId, librarySubfolder);
            MediaFileInfoProvider.SetMediaFileInfo(mediaFile);
        }


        private void ValidateFile(HttpPostedFile file)
        {
            var fileExtension = Path.GetExtension(file.FileName).TrimStart('.');

            if (!MediaLibraryHelper.IsExtensionAllowed(fileExtension, siteService.CurrentSite.SiteName))
            {
                throw new InvalidOperationException($"Cannot upload '{file.FileName}' file due to forbidden extension. Make sure the file extension is included in the value of 'Media file allowed extensions' site settings key.");
            }
        }
    }
}
