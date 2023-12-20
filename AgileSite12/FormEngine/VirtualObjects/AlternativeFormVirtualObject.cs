using CMS.DataEngine;
using CMS.Helpers;
using CMS.IO;

namespace CMS.FormEngine
{
    /// <summary>
    /// Alternative form virtual file object
    /// </summary>
    internal class AlternativeFormVirtualObject : IVirtualFileObject
    {
        #region "IVirtualFileObject"

        /// <summary>
        /// Gets virtual file content for current object
        /// </summary>
        public string Content
        {
            get
            {
                return LayoutHelper.AddLayoutDirectives(CurrentObject.FormLayout);
            }
        }


        /// <summary>
        /// Indicates whether file content is stored externally (e.g. file system)
        /// </summary>
        public bool IsStoredExternally
        {
            get
            {
                return (AlternativeFormInfoProvider.StoreAlternativeFormsInExternalStorage && File.Exists(URLHelper.GetPhysicalPath(CurrentObject.Generalized.GetVirtualFileRelativePath(AlternativeFormInfo.EXTERNAL_COLUMN_CODE))));
            }
        }


        /// <summary>
        /// Gets object hash string used for identification of compiled version in .NET cache
        /// </summary>
        public string ObjectHash
        {
            get
            {
                return CurrentObject.FormVersionGUID;
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
        private AlternativeFormInfo CurrentObject
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
        AlternativeFormVirtualObject(string virtualPath, AlternativeFormInfo currentObject)
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
            
            AlternativeFormVirtualObject virt = null;
            AlternativeFormInfo info = AlternativeFormInfoProvider.GetVirtualObject(virtualPath);
            if (info != null)
            {
                virt = new AlternativeFormVirtualObject(virtualPath, info);
            }
            return virt;
        }

        #endregion
    }
}
