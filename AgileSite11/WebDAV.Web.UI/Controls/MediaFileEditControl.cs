using CMS.MediaLibrary;
using CMS.Membership;
using CMS.UIControls;

namespace CMS.WebDAV.Web.UI
{
    /// <summary>
    /// Media file WebDAV control.
    /// </summary>
    public class MediaFileEditControl : WebDAVEditControl
    {
        /// <summary>
        /// Creates instance.
        /// </summary>
        public MediaFileEditControl()
        {
            mControlType = FileTypeEnum.MediaFile;
        }


        /// <summary>
        /// Gets the media file URL.
        /// </summary>
        protected override string GetUrl()
        {
            return WebDAVURLProvider.GetMediaFileWebDAVUrl(MediaLibraryName, MediaFilePath, GroupName);
        }


        /// <summary>
        /// Reload controls data.
        /// </summary>
        /// <param name="forceReload">Indicates if controls </param>
        public override void ReloadData(bool forceReload)
        {
            // Check media file permissions
            var mli = (MediaLibraryInfo)MediaLibraryInfo;

            // Check authorization to filemodify or manage
            if ((mli != null) && ((MediaLibraryInfoProvider.IsUserAuthorizedPerLibrary(mli, "filemodify", MembershipContext.AuthenticatedUser)
                                   || MediaLibraryInfoProvider.IsUserAuthorizedPerLibrary(mli, "manage", MembershipContext.AuthenticatedUser)) 
                                   || !CheckPermission))
            {
                Enabled = true;

                // Check if module 'Community' is loaded
                if (mli.LibraryGroupID > 0)
                {
                    // Check 'GroupAdministrator' permission 
                    Enabled = (GroupInfo != null) && MembershipContext.AuthenticatedUser.IsGroupAdministrator(mli.LibraryGroupID);
                }
            }
            else
            {
                Enabled = false;
            }

            base.ReloadData(forceReload);
        }
    }
}