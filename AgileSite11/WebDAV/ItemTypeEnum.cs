namespace CMS.WebDAV
{
    /// <summary>
    /// Item type enumeration.
    /// </summary>
    public enum ItemTypeEnum
    {
        #region "General"

        /// <summary>
        /// None.
        /// </summary>
        None = 0,


        /// <summary>
        /// Root item.
        /// </summary>
        Root = 1,

        #endregion


        #region "Files"

        /// <summary>
        /// Unsorted attachment.
        /// </summary>
        UnsortedAttachmentFile = 2,

        /// <summary>
        /// Document field attachment.
        /// </summary>
        FieldAttachmentFile = 3,


        /// <summary>
        /// Content file.
        /// </summary>
        ContentFile = 4,


        /// <summary>
        /// Media file.
        /// </summary>
        MediaFile = 5,


        /// <summary>
        /// Meta file.
        /// </summary>
        MetaFile = 6,

        #endregion


        #region "Folders"

        /// <summary>
        /// Attachment folder.
        /// </summary>
        AttachmentFolder = 11,


        /// <summary>
        /// Attachment culture specified folder.
        /// </summary>
        AttachmentCultureFolder = 12,


        /// <summary>
        /// Attachment alias path folder.
        /// </summary>
        AttachmentAliasPathFolder = 13,


        /// <summary>
        /// Attachment unsorted folder.
        /// </summary>
        AttachmentUnsortedFolder = 14,


        /// <summary>
        /// Attachment field folder.
        /// </summary>
        AttachmentFieldNameFolder = 15,


        /// <summary>
        /// Content folder.
        /// </summary>
        ContentFolder = 16,


        /// <summary>
        /// Content culture specific folder.
        /// </summary>
        ContentCultureCodeFolder = 17,


        /// <summary>
        /// Content alias path folder.
        /// </summary>
        ContentAliasPathFolder = 18,


        /// <summary>
        /// Media folder.
        /// </summary>
        MediaFolder = 19,


        /// <summary>
        /// Media library name.
        /// </summary>
        MediaLibraryName = 20,


        /// <summary>
        /// Media library folder.
        /// </summary>
        MetaFileFolder = 21,


        /// <summary>
        /// Groups.
        /// </summary>
        Groups = 22,


        /// <summary>
        /// Group name.
        /// </summary>
        GroupName = 23,


        /// <summary>
        /// Group attachment folder.
        /// </summary>
        GroupAttachmentFolder = 24,


        /// <summary>
        /// Group media folder.
        /// </summary>
        GroupMediaFolder = 25,

        #endregion


        #region "Support"


        /// <summary>
        /// Not supported.
        /// </summary>
        NotSupported = 26,

        /// <summary>
        /// Not supported group.
        /// </summary>
        NotSupportedGroups = 27,


        /// <summary>
        /// Not supported media library.
        /// </summary>
        NotSupportedMediaLibrary = 28,

        #endregion
    }
}