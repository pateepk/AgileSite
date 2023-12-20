using System;

using CMS;
using CMS.DocumentEngine.Web.UI;
using CMS.Helpers;
using CMS.MacroEngine;
using CMS.MediaLibrary.Web.UI;

[assembly: RegisterExtension(typeof(MediaLibraryMethods), typeof(TransformationNamespace))]

namespace CMS.MediaLibrary.Web.UI
{
    /// <summary>
    /// Media library methods - wrapping methods for macro resolver.
    /// </summary>
    public class MediaLibraryMethods : MacroMethodContainer
    {
        /// <summary>
        /// Returns direct URL to the media file, user permissions are not checked.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof (string), "Returns direct URL to the media file, user permissions are not checked.", 5)]
        [MacroMethodParam(0, "libraryId", typeof (object), "Media library ID.")]
        [MacroMethodParam(1, "filePath", typeof (object), "File path.")]
        [MacroMethodParam(2, "fileGuid", typeof (object), "File GUID.")]
        [MacroMethodParam(3, "fileName", typeof (object), "File name.")]
        [MacroMethodParam(4, "useSecureLinks", typeof (object), "Determines whether to generate secure link.")]
        [MacroMethodParam(5, "downloadlink", typeof (object), "Determines whether disposition parameter should be added to permanent link.")]
        public static object GetMediaFileUrl(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 5:
                    return MediaLibraryTransformationFunctions.GetMediaFileUrl(parameters[0], parameters[1], parameters[2], parameters[3], parameters[4]);

                case 6:
                    return MediaLibraryTransformationFunctions.GetMediaFileUrl(parameters[0], parameters[1], parameters[2], parameters[3], parameters[4], parameters[5]);

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns URL to media file which is rewritten to calling GetMediaFile.aspx page where user permissions are checked.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof (string), "Returns URL to media file which is rewritten to calling GetMediaFile.aspx page where user permissions are checked.", 2)]
        [MacroMethodParam(0, "fileGuid", typeof (object), "File GUID.")]
        [MacroMethodParam(1, "fileName", typeof (object), "File name.")]
        public static object GetMediaFileUrlWithCheck(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                    return MediaLibraryTransformationFunctions.GetMediaFileUrl(parameters[0], parameters[1]);

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns direct URL to the media file, user permissions are not checked.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof (string), "Returns direct URL to the media file, user permissions are not checked.", 2)]
        [MacroMethodParam(0, "libraryId", typeof (object), "Media library ID.")]
        [MacroMethodParam(1, "filePath", typeof (object), "File path.")]
        public static string GetMediaFileDirectUrl(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                    return MediaLibraryTransformationFunctions.GetMediaFileDirectUrl(parameters[0], parameters[1]);

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns URL to detail of media file.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof (string), "Returns URL to detail of media file.", 1)]
        [MacroMethodParam(0, "fileId", typeof (object), "File ID.")]
        [MacroMethodParam(1, "parameter", typeof (string), "Query parameter name (\"fileId\" by default).")]
        public static object GetMediaFileDetailUrl(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    return MediaLibraryTransformationFunctions.GetMediaFileDetailUrl(parameters[0]);

                case 2:
                    return MediaLibraryTransformationFunctions.GetMediaFileDetailUrl(ValidationHelper.GetString(parameters[1], ""), parameters[0]);

                default:
                    throw new NotSupportedException();
            }
        }
    }
}