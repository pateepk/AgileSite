using System;

using CMS.EventLog;
using CMS.Helpers;
using CMS.IO;
using CMS.MediaLibrary;
using CMS.DocumentEngine;
using CMS.SiteProvider;

using ITHit.WebDAV.Server;
using ITHit.WebDAV.Server.Response;

namespace CMS.WebDAV
{
    /// <summary>
    /// Base class for attachment and content resource.
    /// </summary>
    internal abstract class BaseAttachmentResource : HierarchyItem, IResource
    {
        #region "Variables"

        private System.IO.Stream mInputStream = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets binary data.
        /// </summary>
        public abstract byte[] BinaryData
        {
            get;
        }


        /// <summary>
        /// File input stream of attachment.
        /// </summary>
        public virtual System.IO.Stream InputStream
        {
            get
            {
                if (mInputStream == null)
                {
                    // Check if attachment is stored in file system
                    if (WebDAVHelper.IsAttachmentStoredInFileSystem(UrlParser.Node))
                    {
                        if (UrlParser.Attachment != null)
                        {
                            // Ensure physical path of attachment info
                            string filePath = AttachmentBinaryHelper.EnsurePhysicalFile(UrlParser.Attachment);
                            // Set input stream
                            mInputStream = FileStream.New(filePath, FileMode.Open, FileAccess.Read, FileShare.None);
                        }
                    }
                }

                return mInputStream;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Common class for attachment resource and content resource.
        /// </summary>
        /// <param name="path">URL path</param>
        /// <param name="name">Object name</param>
        /// <param name="mimeType">Object mime type</param>
        /// <param name="size">Object size</param>
        /// <param name="created">Creation date</param>
        /// <param name="modified">Modification date</param>
        /// <param name="urlParser">URL parser</param>
        /// <param name="engine">WebDAV engine</param>
        /// <param name="parent">Parent folder</param>
        public BaseAttachmentResource(string path, string name, string mimeType, long size, DateTime created, DateTime modified, UrlParser urlParser, WebDAVEngine engine, IFolder parent)
            : base(path, name, created, modified, urlParser, engine, parent)
        {
            ContentType = mimeType;
            ContentLength = size;
        }


        #region IResource Members

        /// <summary>
        /// Gets the media type of the resource.
        /// </summary>
        public virtual string ContentType
        {
            get;
            protected set;
        }


        /// <summary>
        /// Gets the size of the resource content in bytes.
        /// </summary>
        public virtual long ContentLength
        {
            get;
            protected set;
        }


        /// <summary>
        /// Writes the content of the resource to the specified stream. 
        /// </summary>
        /// <param name="output">Output stream</param>
        /// <param name="byteStart">The zero-based byte offset in resource content at which to begin
        /// copying bytes to the output stream.</param>
        /// <param name="count">The number of bytes to be written to the output stream</param>
        /// <returns>WebDAV response</returns>
        public virtual WebDAVResponse WriteToStream(System.IO.Stream output, long byteStart, long count)
        {
            try
            {
                TreeNode node = UrlParser.Node;

                // Check 'Read' permission or group administrator
                if (WebDAVHelper.IsCurrentUserAuthorizedPerDocument(node, NodePermissionsEnum.Read) || WebDAVHelper.IsGroupAdministrator(UrlParser.Group))
                {
                    var attachment = UrlParser.Attachment;
                    if (attachment != null)
                    {
                        string siteName = SiteContext.CurrentSiteName;
                        int versionHistoryId = node.DocumentCheckedOutVersionHistoryID;
                        var filesLocationType = FileHelper.FilesLocationType(siteName);

                        // Binary data from file system
                        if ((versionHistoryId == 0) && (filesLocationType != FilesLocationTypeEnum.Database))
                        {
                            string filePath = AttachmentBinaryHelper.EnsurePhysicalFile(attachment);
                            WriteFile(output, byteStart, count, filePath);
                        }
                        // Binary data from DB
                        else
                        {
                            WriteBytes(output, byteStart, count, BinaryData);
                        }
                    }
                }
                else
                {
                    return new AccessDeniedResponse();
                }
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("BaseAttachmentResource_WriteToStream", "WebDAV", ex);
                return new ServerErrorResponse();
            }

            return new OkResponse();
        }


        /// <summary>
        /// Saves the content of the resource from the specified stream to the WebDAV repository.
        /// </summary>
        /// <param name="content">Stream to read the content of the resource from</param>
        /// <param name="contentType">Indicates the media type of the resource</param>
        /// <returns>WebDAV response</returns>
        public abstract WebDAVResponse SaveFromStream(System.IO.Stream content, string contentType);

        #endregion


        #endregion


        #region "Overridden Methods"

        /// <summary>
        /// Creates a copy of this item with a new name in the destination folder. 
        /// </summary>
        /// <param name="folder">Destination folder</param>
        /// <param name="destName">Name of the destination item</param>
        /// <param name="deep">Indicates whether to copy entire subtree</param>
        public override WebDAVResponse CopyTo(IFolder folder, string destName, bool deep)
        {
            return new AccessDeniedResponse();
        }


        /// <summary>
        /// Moves this item to the destination folder under a new name.
        /// </summary>
        /// <param name="folder">Destination folder</param>
        /// <param name="destName">Name of the destination item</param>
        public override WebDAVResponse MoveTo(IFolder folder, string destName)
        {
            return new AccessDeniedResponse();
        }


        /// <summary>
        /// Deletes this item.
        /// </summary>
        public override WebDAVResponse Delete()
        {
            return new AccessDeniedResponse();
        }


        /// <summary>
        /// Locks this item.
        /// </summary>
        /// <param name="lockInfo">Describes a lock on this item</param>
        public override WebDAVResponse Lock(ref LockInfo lockInfo)
        {
            return new OkResponse();
        }


        /// <summary>
        /// Removes lock with the specified token from this item or deletes lock-null item. 
        /// </summary>
        /// <param name="lockToken">Lock with this token should be removed from the item</param>
        public override WebDAVResponse Unlock(string lockToken)
        {
            return new NoContentResponse();
        }

        #endregion


        #region "Protected Methods"

        /// <summary>
        /// Ensures attachment binary data.
        /// </summary>
        /// <param name="newAttachment">New attachment info</param>
        protected void EnsureBinaryData(DocumentAttachment newAttachment)
        {
            if (newAttachment != null)
            {
                // Ensure binary data
                if (InputStream != null)
                {
                    newAttachment.AttachmentBinary = Core.BinaryData.GetByteArrayFromStream(InputStream);
                    newAttachment.AttachmentSize = (int)InputStream.Length;
                }
                else if (BinaryData != null)
                {
                    newAttachment.AttachmentBinary = BinaryData;
                    newAttachment.AttachmentSize = BinaryData.Length;
                }
                else
                {
                    throw new Exception("[BaseAttachmentResource.EnsureBinaryData]: Missing binary data.");
                }
            }
        }


        /// <summary>
        /// Ensures media file binary data.
        /// </summary>
        /// <param name="mediaFile">Media file whose binary data should be ensured</param>
        protected void EnsureBinaryData(MediaFileInfo mediaFile)
        {
            if (mediaFile != null)
            {
                // Ensure binary data
                if (InputStream != null)
                {
                    mediaFile.FileBinaryStream = InputStream;
                    mediaFile.FileSize = InputStream.Length;
                }
                else if (BinaryData != null)
                {
                    mediaFile.FileBinary = BinaryData;
                    mediaFile.FileSize = BinaryData.Length;
                }
                else
                {
                    throw new Exception("[BaseAttachmentResource.EnsureBinaryData]: Missing binary data.");
                }
            }
        }


        /// <summary>
        /// Moves attachment to media folder.
        /// </summary>
        /// <param name="library">Media library</param>
        /// <param name="safeFileName">Safe file name of media file</param>
        /// <param name="originalFileName">Original (unsafe) file name of media file</param>
        /// <param name="filePath">File path of media directory</param>
        /// <returns>Returns True if media file was stored</returns>
        protected virtual bool MoveToMediaFolder(MediaLibraryInfo library, string safeFileName, string originalFileName, string filePath)
        {
            if (library != null)
            {
                // New media file path
                string mediaFilePath = filePath + "/" + safeFileName;

                // Get existing media file
                MediaFileInfo currentMfi = MediaFileInfoProvider.GetMediaFileInfo(library.LibraryID, mediaFilePath);

                // Media file doesn't exist in DB
                if (currentMfi == null)
                {
                    // Get media file from file system
                    FileInfo fi = WebDAVHelper.GetFileInfo(library, filePath + "/" + originalFileName);

                    // Media file exists in file system
                    if (fi != null)
                    {
                        // Check if binary data are available
                        if ((BinaryData == null) && (InputStream == null))
                        {
                            throw new Exception("[BaseAttachmentResource.MoveToMediaFolder]: Missing binary data.");
                        }

                        // Write binary data
                        if (BinaryData != null)
                        {
                            StorageHelper.SaveFileToDisk(fi.FullName, BinaryData);
                        }
                        // Write binary data from stream
                        else if (InputStream != null)
                        {
                            StorageHelper.SaveFileToDisk(fi.FullName, InputStream);
                        }

                        return true;
                    }
                }
                // Create media file info from attachment info
                MediaFileInfo newMediaFile = WebDAVHelper.CreateMediaFileInfoFromAttachment(currentMfi, UrlParser.Attachment);
                if (newMediaFile != null)
                {
                    newMediaFile.FileName = IO.Path.GetFileNameWithoutExtension(safeFileName);
                    newMediaFile.FileExtension = IO.Path.GetExtension(safeFileName);
                    newMediaFile.FileMimeType = MimeTypeHelper.GetMimetype(newMediaFile.FileExtension);
                    newMediaFile.FileLibraryID = library.LibraryID;
                    newMediaFile.FilePath = mediaFilePath;

                    // Ensure binary data
                    EnsureBinaryData(newMediaFile);

                    // Resize media file
                    WebDAVHelper.ResizeMediaFileInfo(newMediaFile);

                    // Save media file info
                    MediaFileInfoProvider.SetMediaFileInfo(newMediaFile, false);

                    // Clean binary data
                    WebDAVHelper.CleanBinaryData(newMediaFile);

                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}