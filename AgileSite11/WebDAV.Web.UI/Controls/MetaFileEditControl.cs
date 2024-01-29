using System;

using CMS.UIControls;

namespace CMS.WebDAV.Web.UI
{
    /// <summary>
    /// Metafile WebDAV control.
    /// </summary>
    public class MetaFileEditControl : WebDAVEditControl
    {
        /// <summary>
        /// Creates instance.
        /// </summary>
        public MetaFileEditControl()
        {
            mControlType = FileTypeEnum.MetaFile;
        }


        /// <summary>
        /// Gets the meta file URL.
        /// </summary>
        protected override string GetUrl()
        {
            return WebDAVURLProvider.GetMetaFileWebDAVUrl(MetaFileGUID, FileName, SiteName);
        }
    }
}