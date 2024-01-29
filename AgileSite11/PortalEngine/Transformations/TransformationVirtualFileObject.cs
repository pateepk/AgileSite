using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using CMS.Helpers;
using CMS.IO;

namespace CMS.PortalEngine
{
    /// <summary>
    /// Transformation virtual file object
    /// </summary>
    internal class TransformationVirtualFileObject : IVirtualFileObject
    {
        #region "IVirtualFileObject"

        /// <summary>
        /// Gets virtual file content for current object
        /// </summary>
        public string Content
        {
            get
            {
                bool processEditDelete = !IsStoredExternally && VirtualPath.Contains(VirtualPathHelper.URLParametersSeparator + "showeditdelete");

                EditModeButtonEnum mode = EditModeButtonEnum.None;
                if (processEditDelete)
                {
                    // Get current mode
                    Regex regex = RegexHelper.GetRegex("showeditdelete-(\\d)");
                    Match match = regex.Match(VirtualPath);
                    mode = (EditModeButtonEnum)ValidationHelper.GetInteger(match.Groups[1].Value, 0);
                }

                return TransformationInfoProvider.AddTransformationDirectives(CurrentObject.TransformationCode, true, mode);
            }
        }


        /// <summary>
        /// Indicates whether file content is stored externally (e.g. file system)
        /// </summary>
        public bool IsStoredExternally
        {
            get
            {
                return (TransformationInfoProvider.StoreTransformationsInExternalStorage && File.Exists(URLHelper.GetPhysicalPath(CurrentObject.Generalized.GetVirtualFileRelativePath(TransformationInfo.EXTERNAL_COLUMN_CODE))));
            }
        }


        /// <summary>
        /// Gets object hash string used for identification of compiled version in .NET cache
        /// </summary>
        public string ObjectHash
        {
            get
            {
                return CurrentObject.TransformationVersionGUID;
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
        private TransformationInfo CurrentObject
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
        TransformationVirtualFileObject(string virtualPath, TransformationInfo currentObject)
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

            TransformationVirtualFileObject virt = null;
            TransformationInfo info = TransformationInfoProvider.GetVirtualObject(virtualPath);
            if (info != null)
            {
                virt = new TransformationVirtualFileObject(virtualPath, info);
            }
            return virt;
        }

        #endregion
    }
}
