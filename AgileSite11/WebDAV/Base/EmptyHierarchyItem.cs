using System;
using System.Collections.Generic;

using ITHit.WebDAV.Server;
using ITHit.WebDAV.Server.Response;

namespace CMS.WebDAV
{
    /// <summary>
    /// Class that represents empty folder.
    /// </summary>
    internal class EmptyHierarchyItem : HierarchyItem, IFolder, IFolderLock
    {
        #region "Constructors"

        /// <summary>
        /// Initializes empty hierarchy item.
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="name">Name</param>
        /// <param name="created">Creation date</param>
        /// <param name="modified">Modification date</param>
        /// <param name="urlParser">Url parser</param>
        /// <param name="engine">WebDAV engine</param>
        /// <param name="parent">Parent item</param>
        public EmptyHierarchyItem(string path, string name, DateTime created, DateTime modified, UrlParser urlParser, WebDAVEngine engine, IFolder parent)
            : base(path, name, created, modified, urlParser, engine, parent)
        {
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
        /// Deletes this item.
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


        #region "IFolder Members"

        /// <summary>
        /// Gets the array of this folder's children.
        /// </summary>
        public IHierarchyItem[] Children
        {
            get
            {
                List<IHierarchyItem> children = new List<IHierarchyItem>();

                return children.ToArray();
            }
        }


        /// <summary>
        /// Creates new WebDAV resource with the specified name in this folder. 
        /// </summary>
        /// <param name="resourceName">Name of the resource to create</param>
        /// <returns>WebDAV response</returns>
        public WebDAVResponse CreateResource(string resourceName)
        {
            return new AccessDeniedResponse();
        }


        /// <summary>
        /// Creates new WebDAV folder with the specified name in this folder. 
        /// </summary>
        /// <param name="folderName">Name of the folder to create</param>
        /// <returns>WebDAV response</returns>
        public WebDAVResponse CreateFolder(string folderName)
        {
            return new AccessDeniedResponse();
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
            return new AccessDeniedResponse();
        }

        #endregion
    }
}