using System;
using System.Linq;
using System.Text;

using CMS.IO;
using CMS.Helpers;

namespace CMS.PortalEngine
{
    /// <summary>
    /// Device layout virtual file object
    /// </summary>
    internal class DeviceLayoutVirtualFileObject : IVirtualFileObject
    {
        #region "IVirtualFileObject"

        /// <summary>
        /// Gets virtual file content for current object
        /// </summary>
        public string Content
        {
            get
            {
                return LayoutInfoProvider.AddLayoutDirectives(CurrentObject.LayoutCode);
            }
        }


        /// <summary>
        /// Indicates whether file content is stored externally (e.g. file system)
        /// </summary>
        public bool IsStoredExternally
        {
            get
            {
                return (PageTemplateInfoProvider.StorePageTemplatesInExternalStorage && File.Exists(URLHelper.GetPhysicalPath(CurrentObject.Generalized.GetVirtualFileRelativePath(PageTemplateDeviceLayoutInfo.EXTERNAL_COLUMN_CODE))));
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
        private PageTemplateDeviceLayoutInfo CurrentObject
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
        DeviceLayoutVirtualFileObject(PageTemplateDeviceLayoutInfo currentObject)
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
            return GetVirtualFileObjectInternal(virtualPath, false);
        }


        /// <summary>
        /// Creates IVirtualFileObject for specified virtual path (GetVirtualFileObjectHandler callback method) 
        /// </summary>
        /// <param name="virtualPath">Virtual path</param>
        internal static IVirtualFileObject GetAdHocVirtualFileObject(string virtualPath)
        {
            return GetVirtualFileObjectInternal(virtualPath, true);
        }


        /// <summary>
        /// Creates IVirtualFileObject for specified virtual path (GetVirtualFileObjectHandler callback method) 
        /// </summary>
        /// <param name="virtualPath">Virtual path</param>
        /// <param name="adHoc">Indicates whether page template is AdHoc</param>
        private static IVirtualFileObject GetVirtualFileObjectInternal(string virtualPath, bool adHoc)
        {
            // Path without version GUID is not virtual
            if ((!virtualPath.Contains(VirtualPathHelper.VERSION_GUID_PREFIX)))
            {
                return null;
            }

            DeviceLayoutVirtualFileObject virt = null;
            PageTemplateDeviceLayoutInfo info = PageTemplateDeviceLayoutInfoProvider.GetVirtualObject(virtualPath, adHoc);
            if (info != null)
            {
                virt = new DeviceLayoutVirtualFileObject(info);
            }
            return virt;
        }

        #endregion

    }
}
