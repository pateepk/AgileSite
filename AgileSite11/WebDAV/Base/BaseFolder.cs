using System;

using ITHit.WebDAV.Server;
using ITHit.WebDAV.Server.Response;

namespace CMS.WebDAV
{
    /// <summary>
    /// Base class for general folder.
    /// </summary>
    internal abstract class BaseFolder : HierarchyItem, IFolderLock
    {
        #region "Constructors"

        /// <summary>
        /// Creates general folder.
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="name">Name</param>
        /// <param name="created">Creation date</param>
        /// <param name="modified">Modification date</param>
        /// <param name="urlParser">URL parser</param>
        /// <param name="engine">WebDAV engine</param>
        /// <param name="parent">Parent folder</param>
        public BaseFolder(string path, string name, DateTime created, DateTime modified, UrlParser urlParser, WebDAVEngine engine, IFolder parent)
            : base(path, name, created, modified, urlParser, engine, parent)
        {
        }

        #endregion


        #region "IFolderLock Members"

        /// <summary>
        /// Reserves new item name for future use.
        /// </summary>
        /// <param name="newItemName">Name of the new item to reserve</param>
        /// <param name="lockInfo">Describes a lock on the new item</param>
        /// <returns>WebDAV response</returns>
        public WebDAVResponse CreateLockNull(string newItemName, ref LockInfo lockInfo)
        {
            return new CreatedResponse();
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
        /// Deletes this folder.
        /// </summary>
        /// <returns>WebDAV response</returns>
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

        #endregion
    }
}