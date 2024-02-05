using System;

using CMS.IO;
using CMS.Base;

namespace CMS.Helpers
{


    #region "Permission exception"

    /// <summary>
    /// Permission exception.
    /// </summary>
    public class PermissionException : Exception
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public PermissionException()
        {
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">Exception message</param>
        public PermissionException(string message)
            : base(message)
        {
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="ex">Inner exception</param>
        public PermissionException(string message, Exception ex)
            : base(message, ex)
        {
        }
    }

    #endregion


    /// <summary>
    /// Methods to work with the files representing attachments.
    /// </summary>
    public static class AttachmentHelper
    {
        /// <summary>
        /// Request timeout.
        /// </summary>
        private static int mScriptTimeout = -1;


        /// <summary>
        /// Gets or sets the script timeout in seconds.
        /// </summary>
        public static int ScriptTimeout
        {
            get
            {
                if (mScriptTimeout < 0)
                {
                    mScriptTimeout = ValidationHelper.GetInteger(SettingsHelper.AppSettings["CMSFileScriptTimeout"], 7200);
                }

                return mScriptTimeout;
            }
            set
            {
                mScriptTimeout = value;
            }
        }


        /// <summary>
        /// Returns full file name ([name.extension] if extension is specified) or ([name] only if extension is not specified), If width and height are specified, thumbnail file name is generated.
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="fileExtension">File extension</param>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        public static string GetFullFileName(string fileName, string fileExtension, int width = 0, int height = 0)
        {
            if ((width > 0) && (height > 0))
            {
                fileName = ImageHelper.GetImageThumbnailFileName(fileName, width, height);
            }

            // if file extension is not specified
            if (!string.IsNullOrEmpty(fileExtension))
            {
                if (fileExtension.StartsWithCSafe("."))
                {
                    fileName = fileName + fileExtension;
                }
                else
                {
                    fileName = fileName + "." + fileExtension;
                }
            }

            return fileName;
        }


        /// <summary>
        /// Gets the file subfolder (first two letters from the file name).
        /// </summary>
        /// <param name="filename">File name</param>
        public static string GetFileSubfolder(string filename)
        {
            if (filename.Length < 2)
            {
                return filename;
            }

            return filename.Substring(0, 2);
        }


        /// <summary>
        /// Returns file relative path due to files folder.
        /// </summary>
        /// <param name="guid">File GUID</param>
        /// <param name="extension">File extension</param>
        public static string GetFileRelativePath(string guid, string extension)
        {
            string fileName = GetFullFileName(guid, extension);
            string subfolder = GetFileSubfolder(fileName);

            return DirectoryHelper.CombinePath(subfolder, fileName);
        }
    }
}