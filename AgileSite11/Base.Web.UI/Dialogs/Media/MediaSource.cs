using System;
using System.Runtime.Serialization;
using System.Security;

using CMS.Base.Web.UI;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Media source data container class.
    /// </summary>
    [Serializable]
    public class MediaSource : ISerializable
    {
        #region "Variables"

        // Document attachment

        // CMS file

        // Media file

        // Media file or CMS file site ID 

        // General
        private MediaTypeEnum? mMediaType;
        private string mExtension;
        private MediaSourceEnum mSourceType = MediaSourceEnum.Web;

        // Meta file

        #endregion


        #region "Properties"

        /// <summary>
        /// Site ID.
        /// </summary>
        public int SiteID
        {
            get;
            set;
        }


        /// <summary>
        /// GUID of the selected node.
        /// </summary>
        public Guid NodeGuid
        {
            get;
            set;
        }


        /// <summary>
        /// CMS file alias path.
        /// </summary>
        public string NodeAliasPath
        {
            get;
            set;
        }


        /// <summary>
        /// Node ID.
        /// </summary>
        public int NodeID
        {
            get;
            set;
        }


        /// <summary>
        /// Version history ID of the document.
        /// </summary>
        public int HistoryID
        {
            get;
            set;
        }


        /// <summary>
        /// CMS file document id.
        /// </summary>
        public int DocumentID
        {
            get;
            set;
        }


        /// <summary>
        /// GUID of the selected media file.
        /// </summary>
        public Guid MediaFileGuid
        {
            get;
            set;
        }


        /// <summary>
        /// Media file library ID.
        /// </summary>
        public int MediaFileLibraryID
        {
            get;
            set;
        }


        /// <summary>
        /// Media file library group ID.
        /// </summary>
        public int MediaFileLibraryGroupID
        {
            get;
            set;
        }


        /// <summary>
        /// Media file path.
        /// </summary>
        public string MediaFilePath
        {
            get;
            set;
        }


        /// <summary>
        /// Media file id.
        /// </summary>
        public int MediaFileID
        {
            get;
            set;
        }


        /// <summary>
        /// File size in bytes.
        /// </summary>
        public long FileSize
        {
            get;
            set;
        }


        /// <summary>
        /// GUID of the selected attachment.
        /// </summary>
        public Guid AttachmentGuid
        {
            get;
            set;
        }


        /// <summary>
        /// Attachment ID.
        /// </summary>
        public int AttachmentID
        {
            get;
            set;
        }


        /// <summary>
        /// Type of media.
        /// </summary>
        public MediaTypeEnum MediaType
        {
            get
            {
                if (mMediaType == null)
                {
                    mMediaType = MediaHelper.GetMediaType(Extension);
                }
                return mMediaType.Value;
            }
        }


        /// <summary>
        /// Image width.
        /// </summary>
        public int MediaWidth
        {
            get;
            set;
        }


        /// <summary>
        /// Image height.
        /// </summary>
        public int MediaHeight
        {
            get;
            set;
        }


        /// <summary>
        /// Type of media source.
        /// </summary>
        public MediaSourceEnum SourceType
        {
            get
            {
                return mSourceType;
            }
            set
            {
                mSourceType = value;
            }
        }


        /// <summary>
        /// File extension.
        /// </summary>
        public string Extension
        {
            get
            {
                return mExtension;
            }
            set
            {
                mExtension = value;
                mMediaType = null;
            }
        }


        /// <summary>
        /// File name.
        /// </summary>
        public string FileName
        {
            get;
            set;
        }


        /// <summary>
        /// GUID of the selected metafile.
        /// </summary>
        public Guid MetaFileGuid
        {
            get;
            set;
        }


        /// <summary>
        /// Metafile ID.
        /// </summary>
        public int MetaFileID
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        public MediaSource()
        {
        }

        
        /// <summary>
        /// Constructor for deserialization.
        /// </summary>
        /// <param name="info">Serialization inf</param>
        /// <param name="context">Streaming context</param>
        public MediaSource(SerializationInfo info, StreamingContext context)
        {
            AttachmentGuid = (Guid)info.GetValue("AttachmentGuid", typeof(Guid));
            AttachmentID = (int)info.GetValue("AttachmentID", typeof(int));
            NodeGuid = (Guid)info.GetValue("NodeGuid", typeof(Guid));
            DocumentID = (int)info.GetValue("DocumentID", typeof(int));
            NodeAliasPath = (string)info.GetValue("NodeAliasPath", typeof(string));
            NodeID = (int)info.GetValue("NodeID", typeof(int));
            HistoryID = (int)info.GetValue("HistoryID", typeof(int));
            MediaFileGuid = (Guid)info.GetValue("MediaFileGuid", typeof(Guid));
            MediaFileID = (int)info.GetValue("MediaFileID", typeof(int));
            MediaFileLibraryID = (int)info.GetValue("MediaFileLibraryID", typeof(int));
            MediaFileLibraryGroupID = (int)info.GetValue("MediaFileLibraryGroupID", typeof(int));
            MediaFilePath = (string)info.GetValue("MediaFilePath", typeof(string));
            SiteID = (int)info.GetValue("SiteID", typeof(int));
            SourceType = (MediaSourceEnum)info.GetValue("SourceType", typeof(MediaSourceEnum));
            Extension = (string)info.GetValue("Extension", typeof(string));
            MediaWidth = (int)info.GetValue("MediaWidth", typeof(int));
            MediaHeight = (int)info.GetValue("MediaHeight", typeof(int));
            FileSize = (long)info.GetValue("FileSize", typeof(long));
            FileName = (string)info.GetValue("FileName", typeof(string));
            MetaFileGuid = (Guid)info.GetValue("MetaFileGuid", typeof(Guid));
            MetaFileID = (int)info.GetValue("MetaFileID", typeof(int));
        }


        /// <summary>
        /// Gets object data.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Serialization context</param>
        [SecurityCritical]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("AttachmentGuid", AttachmentGuid);
            info.AddValue("AttachmentID", AttachmentID);
            info.AddValue("NodeGuid", NodeGuid);
            info.AddValue("DocumentID", DocumentID);
            info.AddValue("NodeAliasPath", NodeAliasPath);
            info.AddValue("NodeID", NodeID);
            info.AddValue("HistoryID", HistoryID);
            info.AddValue("MediaFileGuid", MediaFileGuid);
            info.AddValue("MediaFileID", MediaFileID);
            info.AddValue("MediaFileLibraryID", MediaFileLibraryID);
            info.AddValue("MediaFileLibraryGroupID", MediaFileLibraryGroupID);
            info.AddValue("MediaFilePath", MediaFilePath);
            info.AddValue("SiteID", SiteID);
            info.AddValue("SourceType", SourceType);
            info.AddValue("Extension", Extension);
            info.AddValue("MediaWidth", MediaWidth);
            info.AddValue("MediaHeight", MediaHeight);
            info.AddValue("FileSize", FileSize);
            info.AddValue("FileName", FileName);
            info.AddValue("MetaFileGuid", MetaFileGuid);
            info.AddValue("MetaFileID", MetaFileID);
        }

        #endregion
    }
}