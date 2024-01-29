using System;
using System.Web;

using CMS.Base;
using CMS.IO;

namespace CMS.Helpers
{
    /// <summary>
    /// Provides the methods for the safe access to the HTTPContext.Current.
    /// </summary>
    public static class HTTPHelper
    {
        private const string ATTACHMENT_DISPOSITION = "attachment";
        private const string INLINE_DISPOSITION = "inline";

        /// <summary>
        /// List of file extensions for which the disposition is set to inline.
        /// </summary>
        private static string mDispositionInlineExtensions;


        /// <summary>
        /// List of file extensions for which the default disposition is inline. 
        /// (Beside image, '.html' and '.htm' extensions, default extensions are '.pdf', '.swf')
        /// </summary>
        public static string DispositionInlineExtensions
        {
            get
            {
                if (mDispositionInlineExtensions == null)
                {
                    string valueToSet = ValidationHelper.GetString(SettingsHelper.AppSettings["CMSGetFileDispositionInlineExtensions"], "pdf;swf");
                    valueToSet = ";" + valueToSet.ToLowerCSafe().Trim(new [] { ';' }) + ";";
                    mDispositionInlineExtensions = valueToSet;
                }

                return mDispositionInlineExtensions;
            }
        }


        /// <summary>
        /// Sets the file disposition to the output
        /// </summary>
        /// <param name="fileName">File name (e.g. image.png)</param>
        /// <param name="extension">File extension</param>
        public static void SetFileDisposition(string fileName, string extension)
        {
            HttpContext context = HttpContext.Current;

            // Get disposition from query string
            string disposition = QueryHelper.GetString("disposition", String.Empty);

            if (String.IsNullOrEmpty(disposition))
            {
                // Get disposition from context
                disposition = ValidationHelper.GetString(context.Items["disposition"], String.Empty);
            }

            // Always set disposition of pdf files for mobile devices as attachment
            if ((CMSString.Compare(extension, ".pdf", true) == 0) && BrowserHelper.IsMobileDevice())
            {
                disposition = ATTACHMENT_DISPOSITION;
            }

            var resultDisposition = String.Empty;
            // Image, html/htm or disposition inline extension
            if (ImageHelper.IsImage(extension) || ImageHelper.IsHtml(extension) || IsDispositionInline(extension))
            {
                // Disposition type - For images "inline" is default
                if (disposition.Equals(ATTACHMENT_DISPOSITION, StringComparison.OrdinalIgnoreCase))
                {
                    resultDisposition = ATTACHMENT_DISPOSITION;
                }
                else
                {
                    resultDisposition = INLINE_DISPOSITION;
                }
            }
            // Other file type
            else
            {
                // Disposition type - For files "attachment" is default type
                if (disposition.Equals(INLINE_DISPOSITION, StringComparison.OrdinalIgnoreCase))
                {
                    resultDisposition = INLINE_DISPOSITION;
                }
                else
                {
                    resultDisposition = ATTACHMENT_DISPOSITION;
                }
            }

            EnsureExtensionInFileName(ref fileName, extension);

            AddContentDispositionHeader(resultDisposition, fileName, context.Response);
        }

        private static void EnsureExtensionInFileName(ref string fileName, string extension)
        {
            if (!Path.HasExtension(fileName))
            {
                fileName += extension;
            }
        }

        private static void AddContentDispositionHeader(string disposition, string fileName, HttpResponse response)
        {
            response.AddHeader("Content-Disposition", $@"{disposition}; filename=""{GetDispositionFilename(fileName)}""");
        }


        /// <summary>
        /// Indicates if file with given extension should be displayed inline browser.
        /// </summary>
        /// <param name="extension">File extension</param>
        public static bool IsDispositionInline(string extension)
        {
            if (extension == null)
            {
                extension = string.Empty;
            }

            extension = extension.ToLowerCSafe().TrimStart('.');
            return DispositionInlineExtensions.Contains(";" + extension + ";") || DispositionInlineExtensions.Contains(";." + extension + ";");
        }
        

        /// <summary>
        /// File name encoding function.
        /// </summary>
        /// <param name="filename">Filename to encode</param>
        public static string FilenameEncode(string filename)
        {
            if (HttpContext.Current != null)
            {
                return HttpUtility.UrlEncode(filename, HttpContext.Current.Response.HeaderEncoding);
            }

            return HttpUtility.UrlEncode(filename);
        }


        /// <summary>
        /// Gets file name in format to be used in response headers for content-disposition file name.
        /// </summary>
        /// <param name="filename">Original file name</param>
        public static string GetDispositionFilename(string filename)
        {
            if (string.IsNullOrEmpty(filename))
            {
                return filename;
            }

            // Do not encode the file name if Gecko browser (Firefox) or WebKit browsers (Chrome, Safari). 
            // These browsers handle the encoding differently.
            if (!BrowserHelper.IsGecko() && !BrowserHelper.IsWebKit())
            {
                // Encode file name and replace encoded spaces (+) with spaces to have friendly file name
                filename = FilenameEncode(filename).Replace("+", " ");
            }

            return filename;
        }
    }
}