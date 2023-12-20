using System;
using System.Net;
using System.Web.Mvc;

using CMS.Helpers;
using CMS.MediaLibrary;
using CMS.Membership;
using CMS.SiteProvider;

using Kentico.Components.Web.Mvc.Dialogs.Internal;

namespace Kentico.Components.Web.Mvc.Dialogs
{
    /// <summary>
    /// Action filter attribute to check media library permission.
    /// </summary>
    internal sealed class MediaLibraryActionFilterAttribute : ActionFilterAttribute
    {
        private const string VIEW_NAME = "~/Views/Shared/Kentico/Selectors/Dialogs/Shared/_Error.cshtml";
        private readonly string libraryParameterName;
        private readonly string permissionName;


        /// <summary>
        /// Indicates if the error message should be displayed if permission is not granted.
        /// </summary>
        public bool DisplayErrorMessage { get; set; }


        /// <summary>
        /// Indicates if the library name parameter is required.
        /// </summary>
        public bool LibraryNameRequired { get; set; } = true;


        /// <summary>
        /// Action filter attribute to check mdia library permission.
        /// </summary>
        /// <param name="libraryParameterName">Action parameter name which contains library code name to check.</param>
        /// <param name="permissionName">Name of the permission to check.</param>
        public MediaLibraryActionFilterAttribute(string libraryParameterName, string permissionName)
        {
            if (string.IsNullOrEmpty(libraryParameterName))
            {
                throw new ArgumentException(nameof(libraryParameterName));
            }

            this.libraryParameterName = libraryParameterName;
            this.permissionName = permissionName;
        }


        /// <summary>
        /// Called by the ASP.NET MVC framework before the action method executes.
        /// </summary>
        /// <param name="filterContext">The filter context.</param>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            var libraryName = filterContext.ActionParameters[libraryParameterName]?.ToString();
            if (string.IsNullOrEmpty(libraryName) && !LibraryNameRequired)
            {
                return;
            }

            var library = GetLibrary(libraryName);
            if (library == null)
            {
                if (DisplayErrorMessage)
                {
                    var model = new KenticoDialogErrorViewModel
                    {
                        ErrorMessage = ResHelper.GetString("kentico.components.mediafileselector.librarynotfound")
                    };
                    filterContext.Result = new ViewResult()
                    {
                        ViewName = VIEW_NAME,
                        ViewData = new ViewDataDictionary<KenticoDialogErrorViewModel>(model),
                    };
                }
                else
                {
                    filterContext.Result = new HttpStatusCodeResult(HttpStatusCode.NotFound);
                }
            }
            else if (!IsAuthorized(library, permissionName))
            {
                if (DisplayErrorMessage)
                {
                    var model = new KenticoDialogErrorViewModel
                    {
                        ErrorMessage = ResHelper.GetString("kentico.components.mediafileselector.notauthorized")
                    };
                    filterContext.Result = new ViewResult()
                    {
                        ViewName = VIEW_NAME,
                        ViewData = new ViewDataDictionary<KenticoDialogErrorViewModel>(model),
                    };
                }
                else
                {
                    filterContext.Result = new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                }
            }
        }


        private static bool IsAuthorized(MediaLibraryInfo library, string permissionName)
        {
            return MediaLibraryInfoProvider.IsUserAuthorizedPerLibrary(library, permissionName, MembershipContext.AuthenticatedUser);
        }


        private static MediaLibraryInfo GetLibrary(string libraryName)
        {
            return MediaLibraryInfoProvider.GetMediaLibraryInfo(libraryName, SiteContext.CurrentSiteName);
        }
    }
}
