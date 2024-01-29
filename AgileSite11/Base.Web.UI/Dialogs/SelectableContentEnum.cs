using System;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Enumeration defining the type of content which could be selected.
    /// </summary>
    public enum SelectableContentEnum
    {
        /// <summary>
        /// Only images can be selected.
        /// </summary>
        OnlyImages,

        /// <summary>
        /// Only images/audio/video can be selected
        /// </summary>
        OnlyMedia,

        /// <summary>
        /// All file types can be selected.
        /// </summary>
        AllContent,

        /// <summary>
        /// All content files can be selected.
        /// </summary>
        AllFiles
    }
}