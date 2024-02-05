using System;
using System.IO;

using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.SiteProvider;
using CMS.Membership;

using ITHit.WebDAV.Server;
using ITHit.WebDAV.Server.Response;

namespace CMS.WebDAV
{
    /// <summary>
    /// Class that represents the meta file.
    /// </summary>
    internal class MetaFileResource : HierarchyItem, IResource
    {
        #region "Variables"

        private byte[] mBinaryData = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets binary data of meta file.
        /// </summary>
        public byte[] BinaryData
        {
            get
            {
                if (mBinaryData == null)
                {
                    try
                    {
                        // Get binary data
                        mBinaryData = MetaFileInfoProvider.GetFile(UrlParser.MetaFileInfo, SiteContext.CurrentSiteName);
                    }
                    catch (Exception ex)
                    {
                        EventLogProvider.LogException("MetaFileResource_BinaryData", "WebDAV", ex);
                    }
                }

                return mBinaryData;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Initializes metafile resource.
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="name">Name</param>
        /// <param name="mimeType">Mime type</param>
        /// <param name="size">File size</param>
        /// <param name="created">Creation date</param>
        /// <param name="modified">Modification date</param>
        /// <param name="urlParser">Url parser</param>
        /// <param name="engine">WebDAV engine</param>
        /// <param name="parent">Parent folder</param>
        public MetaFileResource(string path, string name, string mimeType, long size, DateTime created, DateTime modified, UrlParser urlParser, WebDAVEngine engine, IFolder parent)
            : base(path, name, created, modified, urlParser, engine, parent)
        {
            ContentLength = size;
            ContentType = mimeType;
        }

        #endregion


        #region "IResource Members"

        /// <summary>
        /// Gets the media type of the resource.
        /// </summary>
        public string ContentType
        {
            get;
            protected set;
        }


        /// <summary>
        /// Gets the size of the resource content in bytes.
        /// </summary>
        public long ContentLength
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
        public WebDAVResponse WriteToStream(Stream output, long byteStart, long count)
        {
            try
            {
                MetaFileInfo mfi = UrlParser.MetaFileInfo;

                // Check if current user can read meta file
                if ((mfi != null) && MembershipContext.AuthenticatedUser.IsAuthorizedPerMetaFile(PermissionsEnum.Read, mfi.MetaFileObjectType, SiteContext.CurrentSiteName))
                {
                    var filesLocationType = FileHelper.FilesLocationType(SiteContext.CurrentSiteName);

                    // Check if is store in file system
                    if (filesLocationType != FilesLocationTypeEnum.Database)
                    {
                        string siteName = (mfi.MetaFileSiteID > 0) ? SiteContext.CurrentSiteName : null;

                        // Ensure physical file
                        string filePath = MetaFileInfoProvider.EnsurePhysicalFile(mfi, siteName);

                        // Binary data from file system
                        WriteFile(output, byteStart, count, filePath);
                    }
                    else
                    {
                        // Binary data from DB
                        WriteBytes(output, byteStart, count, BinaryData);
                    }
                }
                else
                {
                    return new AccessDeniedResponse();
                }
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("MetaFileResource_WriteToStream", "WebDAV", ex);
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
        public WebDAVResponse SaveFromStream(Stream content, string contentType)
        {
            if ((content != null) && (content.Length > 0))
            {
                try
                {
                    MetaFileInfo mfi = UrlParser.MetaFileInfo;

                    // Check if current user can modify meta file
                    if ((mfi != null) && MembershipContext.AuthenticatedUser.IsAuthorizedPerMetaFile(PermissionsEnum.Modify, mfi.MetaFileObjectType, SiteContext.CurrentSiteName))
                    {
                        // Set input stream
                        mfi.InputStream = content;

                        MetaFileInfoProvider.SetMetaFileInfo(mfi);
                    }
                    else
                    {
                        return new AccessDeniedResponse();
                    }
                }
                catch (Exception ex)
                {
                    EventLogProvider.LogException("MetaFileResource_SaveFromStream", "WebDAV", ex);
                    return new ServerErrorResponse();
                }
            }
            return new OkResponse();
        }

        #endregion


        #region "HierarchyItem Members Override"

        /// <summary>
        /// Creates a copy of this item with a new name in the destination folder.
        /// </summary>
        /// <param name="folder">Destination folder</param>
        /// <param name="destName">Name of the destination item</param>
        /// <param name="deep">Indicates whether to copy entire subtree</param>
        /// <returns>WebDAV response</returns>
        public override WebDAVResponse CopyTo(IFolder folder, string destName, bool deep)
        {
            return new AccessDeniedResponse();
        }


        /// <summary>
        /// Moves this item to the destination folder under a new name.
        /// </summary>
        /// <param name="folder">Destination folder</param>
        /// <param name="destName">Name of the destination item</param>
        /// <returns>WebDAV response</returns>
        public override WebDAVResponse MoveTo(IFolder folder, string destName)
        {
            return new AccessDeniedResponse();
        }


        /// <summary>
        /// Deletes this attachment.
        /// </summary>
        /// <returns>WebDAV response</returns>
        public override WebDAVResponse Delete()
        {
            return new AccessDeniedResponse();
        }

        #endregion


        #region "ILock Members"

        /// <summary>
        /// Locks this item.
        /// </summary>
        /// <param name="lockInfo">Describes a lock on this item</param>
        public override WebDAVResponse Lock(ref LockInfo lockInfo)
        {
            try
            {
                MetaFileInfo mfi = UrlParser.MetaFileInfo;

                // Check if user can modify meta file
                if ((mfi != null) && MembershipContext.AuthenticatedUser.IsAuthorizedPerMetaFile(PermissionsEnum.Modify, mfi.MetaFileObjectType, SiteContext.CurrentSiteName))
                {
                    return new OkResponse();
                }
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("MetaFileResource_Lock", "WebDAV", ex);
                return new ConflictResponse();
            }

            return new ForbiddenResponse();
        }

        #endregion
    }
}