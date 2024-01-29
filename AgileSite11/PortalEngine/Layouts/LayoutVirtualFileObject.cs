using System;
using System.Linq;
using System.Text;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.IO;

namespace CMS.PortalEngine
{
    /// <summary>
    /// Layout virtual file object
    /// </summary>
    internal class LayoutVirtualFileObject : IVirtualFileObject
    {
        #region "IVirtualFileObject"

        /// <summary>
        /// Gets virtual file content for current object
        /// </summary>
        public string Content
        {
            get
            {
                return LayoutInfoProvider.AddLayoutDirectives(CurrentObject.LayoutCode, LayoutTypeEnum.Ascx);
            }
        }


        /// <summary>
        /// Indicates whether file content is stored externally (e.g. file system)
        /// </summary>
        public bool IsStoredExternally
        {
            get
            {
                return (LayoutInfoProvider.StoreLayoutsInExternalStorage && File.Exists(URLHelper.GetPhysicalPath(CurrentObject.Generalized.GetVirtualFileRelativePath(LayoutInfo.EXTERNAL_COLUMN_CODE))));
            }
        }


        /// <summary>
        /// Gets object hash string used for identification of compiled version in .NET cache
        /// </summary>
        public string ObjectHash
        {
            get
            {
                return CurrentObject.LayoutVersionGUID;
            }
        }


        /// <summary>
        /// Gets the physical file path if exists
        /// </summary>
        public string PhysicalFilePath
        {
            get
            {
                return null;
            }
        }

        #endregion


        #region "Properties"

        /// <summary>
        /// Current object instance
        /// </summary>
        private LayoutInfo CurrentObject
        {
            get;
            set;
        }

        #endregion


        #region "Constructor"

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="currentObject">Object instance</param>
        LayoutVirtualFileObject(LayoutInfo currentObject)
        {
            CurrentObject = currentObject;
        }

        #endregion


        #region "GetVirtualFileObjectHandler callback method"

        /// <summary>
        /// Creates IVirtualFileObject for specified virtual path (GetVirtualFileObjectHandler callback method) 
        /// </summary>
        /// <param name="virtualPath">Virtual path</param>
        internal static IVirtualFileObject GetVirtualFileObject(string virtualPath)
        {
            // Path without version GUID is not virtual
            if ((!virtualPath.Contains(VirtualPathHelper.VERSION_GUID_PREFIX)))
            {
                return null;
            }

            LayoutVirtualFileObject virt = null;
            LayoutInfo info = LayoutInfoProvider.GetVirtualObject(virtualPath);
            if (info != null)
            {
                virt = new LayoutVirtualFileObject(info);
            }
            return virt;
        }

        #endregion
    }
}
