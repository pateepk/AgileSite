using System;

namespace CMS.OnlineMarketing.Web.UI
{
    /// <summary>
    /// Class for creating links that show YouTube video in Magnific Popup lightbox.
    /// </summary>
    public class MagnificPopupYouTubeLinkBuilder
    {
        private const string YOUTUBE_ADDRESS = "https://www.youtube.com/watch?v=";


        /// <summary>
        /// Returns link that can be used on provided page to show YouTube video in Magnific Popup lightbox.
        /// </summary>
        /// <param name="videoID">YouTube video ID</param>
        /// <param name="linkID">Unique client ID that element will have</param>
        /// <param name="content">Content that will be in the link</param>
        /// <exception cref="ArgumentException"><paramref name="videoID"/> or <paramref name="linkID"/> or <paramref name="content"/> is null or empty</exception>
        public string GetLink(string videoID, string linkID, string content)
        {
            if (string.IsNullOrEmpty(videoID))
            {
                throw new ArgumentException("[MagnificPopupYouTubeLinkBuilder.GetLink]: Cannot be empty or null", "videoID");
            }
            if (string.IsNullOrEmpty(linkID))
            {
                throw new ArgumentException("[MagnificPopupYouTubeLinkBuilder.GetLink]: Cannot be empty or null", "linkID");
            }
            if (string.IsNullOrEmpty(content))
            {
                throw new ArgumentException("[MagnificPopupYouTubeLinkBuilder.GetLink]: Cannot be empty or null", "content");
            }

            return string.Format(@"<a href=""{0}"" id=""{1}"" target=""_top"">{2}</a>", YOUTUBE_ADDRESS + videoID, linkID, content);
        }
    }
}