using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

using CMS.Helpers;
using CMS.Core;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Class providing helper methods for media. Overridable helper
    /// </summary>
    public class MediaHelper : AbstractHelper<MediaHelper>
    {
        #region "Variables"

        private Regex mRegShortYouTube;

        #endregion


        #region "Properties"

        /// <summary>
        /// Regular expression for short youtube URL.
        /// </summary>
        public Regex RegShortYouTube
        {
            get
            {
                if (mRegShortYouTube == null)
                {
                    mRegShortYouTube = RegexHelper.GetRegex("(?:http\\:\\/\\/)?youtu\\.be/([a-zA-Z0-9-_]+)(?:\\??.*)");
                }
                return mRegShortYouTube;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns media type according to the specified extension.
        /// </summary>
        /// <param name="ext">Extension to check</param>
        public static MediaTypeEnum GetMediaType(string ext)
        {
            return
                ImageHelper.IsImage(ext) ? MediaTypeEnum.Image :
                                                                   IsAudioVideo(ext) ? MediaTypeEnum.AudioVideo : MediaTypeEnum.Unknown;
        }


        /// <summary>
        /// Returns true if it is audio or video extension, otherwise returns false.
        /// </summary>
        /// <param name="extension">File extension</param>
        public static bool IsAudioVideo(string extension)
        {
            return IsAudio(extension) || IsVideo(extension);
        }


        /// <summary>
        /// Returns true if it is audio extension, otherwise returns false.
        /// </summary>
        /// <param name="extension">File extension</param>
        public static bool IsAudio(string extension)
        {
            return HelperObject.IsAudioInternal(extension);
        }


        /// <summary>
        /// Returns true if it is video extension, otherwise returns false.
        /// </summary>
        /// <param name="extension">File extension</param>
        public static bool IsVideo(string extension)
        {
            return HelperObject.IsVideoInternal(extension);
        }


        /// <summary>
        /// Generates the IMG tag according to given parameters.
        /// </summary>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        /// <param name="color">Image border color</param>
        /// <param name="align">Image align</param>
        /// <param name="border">Image border width</param>
        /// <param name="hspace">Image HSpace</param>
        /// <param name="vspace">Image VSpace</param>
        /// <param name="style">Image style</param>
        public static string GetImageStyleAtt(int width, int height, string align, int border, string color, int hspace, int vspace, string style)
        {
            return HelperObject.GetImageStyleAttInternal(width, height, align, border, color, hspace, vspace, style);
        }


        /// <summary>
        /// Returns HTML code of the image/image with link/image with special behavior.
        /// </summary>
        /// <param name="parameters">Collection with parameters of the image</param>        
        public static string GetImage(ImageParameters parameters)
        {
            return HelperObject.GetImageInternal(parameters);
        }


        /// <summary>
        /// Returns HTML code of the audio/video player
        /// </summary>
        /// <param name="parameters">Collection with parameters of the audio/video player</param>        
        public static string GetAudioVideo(AudioVideoParameters parameters)
        {
            return HelperObject.GetAudioVideoInternal(parameters);
        }


        /// <summary>
        /// Returns HTML code of the YouTube player.
        /// </summary>
        /// <param name="parameters">Collection with parameters of the YouTube player</param> 
        public static string GetYouTubeVideo(YouTubeVideoParameters parameters)
        {
            return HelperObject.GetYouTubeVideoInternal(parameters);
        }


        /// <summary>
        /// Gets allowed extensions for images depends on settings
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static IEnumerable<string> GetAllowedImageExtensions(string siteName)
        {
            var settingsExtensions = CoreServices.Settings[siteName + ".CMSUploadExtensions"].Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            if (settingsExtensions.Length > 0)
            {
                return ImageHelper.ImageExtensions.Intersect(settingsExtensions);
            }

            return ImageHelper.ImageExtensions;
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Returns true if it is audio extension, otherwise returns false.
        /// </summary>
        /// <param name="extension">File extension</param>
        protected virtual bool IsAudioInternal(string extension)
        {
            return IsMatchingMediaType(extension, "CMSAudioExtensions", "audio/");
        }


        /// <summary>
        /// Returns true if it is video extension, otherwise returns false.
        /// </summary>
        /// <param name="extension">File extension</param>
        protected virtual bool IsVideoInternal(string extension)
        {
            return IsMatchingMediaType(extension, "CMSVideoExtensions", "video/");
        }


        private static bool IsMatchingMediaType(string extension, string appSettingKey, string mimeTypePrefix)
        {
            if (extension == null)
            {
                return false;
            }

            var normalizedExtension = extension.TrimStart('.').ToLowerInvariant();
            var extensions = ValidationHelper.GetString(SettingsHelper.AppSettings[appSettingKey], "").Trim().Trim(';').ToLowerInvariant();
            if (!String.IsNullOrEmpty(extensions))
            {
                return $";{extensions};".Contains($";{normalizedExtension};");
            }

            return MimeTypeHelper.GetMimetype(extension).StartsWith(mimeTypePrefix, StringComparison.OrdinalIgnoreCase);
        }


        /// <summary>
        /// Generates the IMG tag according to given parameters.
        /// </summary>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        /// <param name="color">Image border color</param>
        /// <param name="align">Image align</param>
        /// <param name="border">Image border width</param>
        /// <param name="hspace">Image HSpace</param>
        /// <param name="vspace">Image VSpace</param>
        /// <param name="style">Image style</param>
        protected virtual string GetImageStyleAttInternal(int width, int height, string align, int border, string color, int hspace, int vspace, string style)
        {
            // Style attribute
            StringBuilder styleAtt = new StringBuilder();

            if (style != null)
            {
                style = style.TrimEnd(';');
                if (!String.IsNullOrEmpty(style))
                {
                    styleAtt.Append(HTMLHelper.HTMLEncode(HttpUtility.UrlDecode(style)) + ";");
                }
            }

            // Width
            if (width > 0)
            {
                styleAtt.AppendFormat("width: {0}px;", width);
            }

            // Height
            if (height > 0)
            {
                styleAtt.AppendFormat("height: {0}px;", height);
            }

            // Border
            if (border > 0)
            {
                styleAtt.AppendFormat("border: {0}px solid {1};", border, HTMLHelper.HTMLEncode(color));
            }
            else if (border == 0)
            {
                styleAtt.Append("border: none;");
            }

            // Align
            if (!String.IsNullOrEmpty(align))
            {
                if ((align == "left") || (align == "right"))
                {
                    styleAtt.AppendFormat("float: {0};", align);
                }
                else
                {
                    styleAtt.AppendFormat("vertical-align: {0};", HTMLHelper.HTMLEncode(align));
                }
            }

            // Margin (HSpace & VSpace)
            if (hspace == vspace)
            {
                if (hspace >= 0)
                {
                    styleAtt.AppendFormat("margin: {0}px;", hspace);
                }
            }
            else
            {
                string marginH = null;
                if (hspace >= 0)
                {
                    marginH = hspace + "px;";
                }
                else if (hspace < 0)
                {
                    marginH = "auto;";
                }
                if (vspace > 0)
                {
                    styleAtt.AppendFormat("margin: {0}px {1}", vspace, marginH);
                }
                else
                {
                    styleAtt.Append("margin: auto " + marginH);
                }
            }

            return styleAtt.ToString();
        }


        /// <summary>
        /// Returns HTML code of the image/image with link/image with special behavior.
        /// </summary>
        /// <param name="parameters">Collection with parameters of the image</param>        
        protected virtual string GetImageInternal(ImageParameters parameters)
        {
            if (String.IsNullOrEmpty(parameters?.Url))
            {
                return null;
            }

            StringBuilder img = new StringBuilder("<img src=\"");
            if (parameters.SizeToURL)
            {
                string url = HttpUtility.HtmlDecode(parameters.Url);
                if (parameters.Width > 0)
                {
                    url = URLHelper.AddParameterToUrl(url, "width", parameters.Width.ToString());
                }

                if (parameters.Height > 0)
                {
                    url = URLHelper.AddParameterToUrl(url, "height", parameters.Height.ToString());
                }
                img.Append(HTMLHelper.HTMLEncode(url));
            }
            else
            {
                img.Append(HTMLHelper.HTMLEncode(parameters.Url));
            }
            img.Append("\"");

            // ID
            if (!String.IsNullOrEmpty(parameters.Id))
            {
                img.AppendFormat(" id=\"{0}\"", HTMLHelper.HTMLEncode(HttpUtility.UrlDecode(parameters.Id)));
            }

            // Alternate text
            if (!String.IsNullOrEmpty(parameters.Alt))
            {
                img.AppendFormat(" alt=\"{0}\"", HTMLHelper.HTMLEncode(HttpUtility.UrlDecode(parameters.Alt)));
            }
            else
            {
                // Decorative image
                img.AppendFormat(" alt=\"\"");
            }

            // Style
            string style = GetImageStyleAtt(0, 0, parameters.Align, parameters.BorderWidth, parameters.BorderColor, parameters.HSpace, parameters.VSpace, parameters.Style);
            if (!String.IsNullOrEmpty(style))
            {
                img.AppendFormat(" style=\"{0}\"", style);
            }

            // Height attribute
            if (parameters.Height > 0)
            {
                img.AppendFormat(" height=\"{0}\"", parameters.Height);
            }

            // Width attribute
            if (parameters.Width > 0)
            {
                img.AppendFormat(" width=\"{0}\"", parameters.Width);
            }

            // Class
            if (!String.IsNullOrEmpty(parameters.Class))
            {
                img.AppendFormat(" class=\"{0}\"", HTMLHelper.HTMLEncode(HttpUtility.UrlDecode(parameters.Class)));
            }

            // Tooltip
            if (!String.IsNullOrEmpty(parameters.Tooltip))
            {
                img.AppendFormat(" title=\"{0}\"", HTMLHelper.HTMLEncode(HttpUtility.UrlDecode(parameters.Tooltip)));
            }

            string imgTag = img + " />";

            // Link and Target
            if (!String.IsNullOrEmpty(parameters.Link))
            {
                string link = String.Format("<a href=\"{0}\"", HTMLHelper.HTMLEncode(parameters.Link));
                if (!String.IsNullOrEmpty(parameters.Target))
                {
                    link += String.Format(" target=\"{0}\"", HTMLHelper.HTMLEncode(parameters.Target));
                }
                imgTag = String.Format("{0}>{1}</a>", link, imgTag);
            }
            else
            {
                // Behavior
                if (!String.IsNullOrEmpty(parameters.Behavior))
                {
                    string link = String.Format("<a href=\"{0}\" target=\"{1}\"", HTMLHelper.HTMLEncode(parameters.Url), HTMLHelper.HTMLEncode(parameters.Behavior));
                    imgTag = String.Format("{0}>{1}</a>", link, imgTag);
                }
            }

            return imgTag;
        }


        /// <summary>
        /// Returns HTML code of the audio/video player
        /// </summary>
        /// <param name="parameters">Collection with parameters of the audio/video player</param>        
        protected virtual string GetAudioVideoInternal(AudioVideoParameters parameters)
        {
            if (String.IsNullOrEmpty(parameters?.Extension))
            {
                return null;
            }

            var tagName = IsAudio(parameters.Extension) ? "audio" : "video";
            var sb = new StringBuilder();

            sb.AppendFormat("<{0}", tagName);
            sb.AppendFormat(" width=\"{0}\"", parameters.Width);
            sb.AppendFormat(" height=\"{0}\"", parameters.Height);

            // Common properties
            if (parameters.Loop)
            {
                sb.Append(" loop");
            }
            if (parameters.AutoPlay)
            {
                sb.Append(" autoplay");
            }
            if (parameters.Controls)
            {
                sb.Append(" controls");
            }
            sb.Append(">\n");

            // Source
            sb.Append("<source");
            sb.AppendFormat(" src=\"{0}\"", HTMLHelper.HTMLEncode(parameters.Url));
            sb.AppendFormat(" type=\"{0}\"", MimeTypeHelper.GetMimetype(parameters.Extension));
            sb.Append(" />");
            sb.Append(GetString("Media.NotSupported"));
            sb.AppendFormat("</{0}>\n", tagName);

            return sb.ToString();
        }


        /// <summary>
        /// Returns HTML code of the YouTube player.
        /// </summary>
        /// <param name="parameters">Collection with parameters of the YouTube player</param> 
        protected virtual string GetYouTubeVideoInternal(YouTubeVideoParameters parameters)
        {
            if (parameters == null)
            {
                return null;
            }

            StringBuilder urlParams = new StringBuilder();
            
            // Cut off protocol in order to create protocol agnostic URL
            string videoUrl = URLHelper.RemoveProtocol(parameters.Url);
            
            // Handle short youtube URL
            if (videoUrl.ToLowerInvariant().Contains("youtu.be/"))
            {
                Match m = RegShortYouTube.Match(videoUrl.TrimStart('/'));
                if (m.Success)
                {
                    string query = URLHelper.GetQuery(videoUrl);
                    videoUrl = String.Format("www.youtube.com/watch?v={0}", m.Groups[1].Value);
                    // Preserve query parameters
                    videoUrl = URLHelper.AppendQuery(videoUrl, query);
                }
            }

            // Ensure protocol agnostic URL (also prevents XSS via javascript:)
            videoUrl = "//" + videoUrl.TrimStart('/');

            // Change address from web site
            parameters.Url = GetYouTubeEmbedURL(videoUrl);

            // Prepare URL parameters
            urlParams.Append(parameters.FullScreen ? "&fs=1" : "&fs=0");
            urlParams.Append(parameters.AutoPlay ? "&autoplay=1" : String.Empty);
            urlParams.Append(!parameters.RelatedVideos ? "&rel=0" : String.Empty);
            urlParams.Append("&enablejsapi=1&version=3");

            parameters.Url = HTMLHelper.EncodeForHtmlAttribute(String.Format("{0}{1}{2}", parameters.Url, parameters.Url.Contains("?") ? "&" : "?", urlParams.ToString().TrimStart('&')));

            // Return YouTube player
            return String.Format("<iframe type=\"text/html\" width=\"{0}\" height=\"{1}\" src=\"{2}\" frameborder=\"0\" allowfullscreen></iframe>\n", parameters.Width, parameters.Height, parameters.Url);
        }


        /// <summary>
        /// Changes youtube URL to correct format for embedding.
        /// </summary>
        /// <param name="inputUrl">YouTube URL</param>
        private string GetYouTubeEmbedURL(string inputUrl)
        {
            inputUrl = inputUrl.Replace("/watch?v=", "/embed/");

            // Split URL by last slash
            int slashIndex = inputUrl.LastIndexOf('/');
            if (slashIndex >= 0)
            {
                string urlStart = inputUrl.Substring(0, slashIndex);
                string urlEnd = inputUrl.Substring(slashIndex);

                // Check if there is ampersand in end section of URL
                int ampIndex = urlEnd.IndexOf('&');
                if (ampIndex >= 0)
                {
                    return String.Format("{0}{1}?{2}", urlStart, urlEnd.Substring(0, ampIndex), urlEnd.Substring(ampIndex + 1));
                }
            }

            return inputUrl;
        }

        #endregion
    }
}