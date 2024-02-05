using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMS.MediaLibrary
{
    /// <summary>
    /// Web farm task types for Media library module
    /// </summary>
    public class MediaTaskType
    {
        /// <summary>
        /// Updates (inserts) media file.
        /// </summary>
        public const string UpdateMediaFile = "UPDATEMEDIAFILE";

        /// <summary>
        /// Deletes media file.
        /// </summary>
        public const string DeleteMediaFile = "DELETEMEDIAFILE";

        /// <summary>
        /// Creates media folder.
        /// </summary>
        public const string CreateMediaFolder = "CREATEMEDIAFOLDER";

        /// <summary>
        /// Renema media folder.
        /// </summary>
        public const string RenameMediaFolder = "RENAMEMEDIAFOLDER";

        /// <summary>
        /// Deletes media folder.
        /// </summary>
        public const string DeleteMediaFolder = "DELETEMEDIAFOLDER";

        /// <summary>
        /// Copy media folder.
        /// </summary>
        public const string CopyMediaFolder = "COPYMEDIAFOLDER";

        /// <summary>
        /// Copy media folder.
        /// </summary>
        public const string MoveMediaFolder = "MOVEMEDIAFOLDER";

        /// <summary>
        /// Copy media file.
        /// </summary>
        public const string CopyMediaFile = "COPYMEDIAFILE";

        /// <summary>
        /// Moves media file.
        /// </summary>
        public const string MoveMediaFile = "MOVEMEDIAFILE";

        /// <summary>
        /// Delete media file preview.
        /// </summary>
        public const string DeleteMediaFilePreview = "DELETEMEDIAFILEPREVIEW";
    }
}
