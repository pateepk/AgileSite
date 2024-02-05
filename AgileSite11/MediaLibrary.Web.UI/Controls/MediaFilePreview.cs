using System;
using System.Web.UI;

using CMS.Base.Web.UI;
using CMS.Helpers;
using CMS.SiteProvider;
using CMS.UIControls;

namespace CMS.MediaLibrary.Web.UI
{
    /// <summary>
    /// Media file preview class.
    /// </summary>
    public class MediaFilePreview : CMSUserControl
    {
        #region "Variables"

        private bool mDisplayActiveContent = true;
        private bool mUseSecureLinks = true;

        #endregion


        #region "Properties"

        /// <summary>
        /// Preview preffix for identification preview file.
        /// </summary>
        public string PreviewSuffix
        {
            get;
            set;
        }


        /// <summary>
        /// Icon set name.
        /// </summary>
        public string IconSet
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if active content (video etc.) should be displayed.
        /// </summary>
        public bool DisplayActiveContent
        {
            get
            {
                return mDisplayActiveContent;
            }
            set
            {
                mDisplayActiveContent = value;
            }
        }


        /// <summary>
        /// Indicates whether the links to media file should be processed in a secure way.
        /// </summary>
        public bool UseSecureLinks
        {
            get
            {
                return mUseSecureLinks;
            }
            set
            {
                mUseSecureLinks = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Shows preview for file if exists or file icon.
        /// </summary>
        /// <param name="fileInfo">Media file info</param>
        /// <param name="width">Width of preview to display</param>
        /// <param name="height">Height of preview to display</param>
        /// <param name="maxSideSize">Max side size of preview to display</param>
        /// <param name="previewSuffix">Media file preview suffix</param>
        /// <param name="iconSet">Name of the subfolder where icon images are located</param>
        /// <param name="page">Page</param>
        public static string ShowPreviewOrIcon(MediaFileInfo fileInfo, int width, int height, int maxSideSize, string previewSuffix, string iconSet, Page page)
        {
            if (fileInfo == null)
            {
                return String.Empty;
            }

            SiteInfo si = SiteInfoProvider.GetSiteInfo(fileInfo.FileSiteID);
            if (si == null)
            {
                return String.Empty;
            }

            string fileUrl = UrlResolver.ResolveUrl(MediaFileInfoProvider.GetMediaFileUrl(fileInfo.FileGUID, fileInfo.FileName));

            // Display preview image
            if (MediaLibraryHelper.HasPreview(si.SiteName, fileInfo.FileLibraryID, fileInfo.FilePath))
            {
                string dimensions = String.Format("?{0}{1}{2}", ((width > 0) ? String.Format("width={0}&", width) : String.Empty), ((height > 0) ? String.Format("height={0}&", height) : String.Empty), ((maxSideSize > 0) ? "maxsidesize=" + maxSideSize : String.Empty));
                dimensions = dimensions.TrimEnd('&');

                string widthHeight = ((width > 0) ? String.Format("width=\"{0}\" ", width) : String.Empty) + ((height > 0) ? String.Format("height=\"{0}\"", height) : String.Empty);

                // Output preview image
                return String.Format("<img alt=\"{0}\" src=\"{1}&amp;preview=1\" border=\"0\" {2} />", fileInfo.FileDescription, HTMLHelper.HTMLEncode((fileUrl + ((dimensions != "?") ? dimensions : String.Empty))), widthHeight);
            }

            // Display image preview
            if (ImageHelper.IsImage(fileInfo.FileExtension))
            {
                // New dimensions
                int[] newDims = ImageHelper.EnsureImageDimensions(width, height, maxSideSize, fileInfo.FileImageWidth, fileInfo.FileImageHeight);

                // If new dimensions are different use them
                if (((newDims[0] != fileInfo.FileImageWidth) && (newDims[1] != fileInfo.FileImageHeight)) && ((newDims[0] != width) || (newDims[1] != height)) && (newDims[0] > 0) && (newDims[1] > 0))
                {
                    width = newDims[0];
                    height = newDims[1];
                }

                // Output image
                return String.Format("<img width=\"{0}\" height=\"{1}\" alt=\"{2}\" src=\"{3}\" border=\"0\" />", width, height, fileInfo.FileDescription, HTMLHelper.HTMLEncode(String.Format("{0}?width={1}&height={2}", fileUrl, width, height)));
            }

            // Display MIME type icon 
            return UIHelper.GetFileIcon(page, fileInfo.FileExtension, FontIconSizeEnum.Notifications, fileInfo.FileDescription);
        }

        #endregion
    }
}
