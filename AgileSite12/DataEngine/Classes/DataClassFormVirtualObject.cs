using CMS.Helpers;
using CMS.IO;

namespace CMS.DataEngine
{
    /// <summary>
    /// Data class form virtual file object
    /// </summary>
    internal class DataClassFormVirtualObject : IVirtualFileObject
    {
        #region "IVirtualFileObject"

        /// <summary>
        /// Gets virtual file content for current object
        /// </summary>
        public string Content
        {
            get
            {
                return LayoutHelper.AddLayoutDirectives(CurrentObject.ClassFormLayout);
            }
        }


        /// <summary>
        /// Indicates whether file content is stored externally (e.g. file system)
        /// </summary>
        public bool IsStoredExternally
        {
            get
            {
                return (DataClassInfoProvider.StoreFormLayoutsInExternalStorage && File.Exists(URLHelper.GetPhysicalPath(CurrentObject.Generalized.GetVirtualFileRelativePath(DataClassInfo.EXTERNAL_COLUMN_CODE))));
            }
        }


        /// <summary>
        /// Gets object hash string used for identification of compiled version in .NET cache
        /// </summary>
        public string ObjectHash
        {
            get
            {
                return CurrentObject.ClassVersionGUID;
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
        /// Current object virtual path
        /// </summary>
        private string VirtualPath
        {
            get;
            set;
        }


        /// <summary>
        /// Current object instance
        /// </summary>
        private DataClassInfo CurrentObject
        {
            get;
            set;
        }

        #endregion


        #region "Constructor"

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="virtualPath">Virtual path</param>
        /// <param name="currentObject">Object instance</param>
        DataClassFormVirtualObject(string virtualPath, DataClassInfo currentObject)
        {
            VirtualPath = virtualPath;
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
            
            DataClassFormVirtualObject virt = null;
            DataClassInfo info = DataClassInfoProvider.GetVirtualObject(virtualPath);
            if (info != null)
            {
                virt = new DataClassFormVirtualObject(virtualPath, info);
            }
            return virt;
        }

        #endregion
    }
}
