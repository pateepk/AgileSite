using System;

namespace CMS.Helpers
{
    /// <summary>
    /// Enumeration for excluded pages by the system.
    /// </summary>
    public enum ExcludedSystemEnum : int
    {
        /// <summary>
        /// Unknown excluded status.
        /// </summary>
        Unknown = -1,

        /// <summary>
        /// Not excluded.
        /// </summary>
        NotExcluded = 0,

        /// <summary>
        /// Excluded page - Not specific.
        /// </summary>
        Excluded = 1,

        /// <summary>
        /// Administration page.
        /// </summary>
        Administration = 2,

        /// <summary>
        /// Request is processed by CMS handlers (GetAttachmentHandler, GetMetafileHandler, GetMediaHandler etc.)
        /// </summary>
        GetFileHandler = 3,

        /// <summary>
        /// Portal template page.
        /// </summary>
        PortalTemplate = 4,

        /// <summary>
        /// Logon page.
        /// </summary>
        LogonPage = 5,

        // Trackback page is not supported
        //TrackbackPage = 6,
        
        // Handler 404 status is not supported
        //Handler404 = 7,

        /// <summary>
        /// CMS dialog (requires authentication).
        /// </summary>
        CMSDialog = 8,

        /// <summary>
        /// Physical file.
        /// </summary>
        PhysicalFile = 9,

        /// <summary>
        /// WebDAV request.
        /// </summary>
        WebDAV = 10,

        /// <summary>
        /// GetResource.ashx request
        /// </summary>
        GetResource = 11,

        /// <summary>
        /// App_Themes folder
        /// </summary>
        AppThemes = 12
    }
}