using System;
using System.Linq;
using System.Text;

using CMS.IO;
using CMS.Helpers;
using CMS.Base;


namespace CMS.PortalEngine
{
    /// <summary>
    /// Virtual web part virtual file object
    /// </summary>
    internal class VirtualWebPartVirtualFileObject : IVirtualFileObject
    {
        #region "Variables"

        private bool mIsStoredExternally;
        private string mPhysicalFilePath;

        #endregion


        #region "IVirtualFileObject"

        /// <summary>
        /// Gets virtual file content for current object
        /// </summary>
        public string Content
        {
            get
            {
                string content = null;

                // Web part layout
                if (WebPartLayoutID > 0)
                {
                    WebPartLayoutInfo wpli = WebPartLayoutInfoProvider.GetWebPartLayoutInfo(WebPartLayoutID);
                    if (wpli != null)
                    {
                        content = wpli.WebPartLayoutCode;
                    }
                }

                // Web part
                if (String.IsNullOrEmpty(content))
                {
                    if (WebPartID > 0)
                    {
                        WebPartInfo wpi = WebPartInfoProvider.GetWebPartInfo(WebPartID);
                        if (wpi != null)
                        {
                            string webPartFile = URLHelper.GetPhysicalPath(WebPartInfoProvider.GetWebPartUrl(wpi, false));
                            using (StreamReader reader = StreamReader.New(webPartFile))
                            {
                                content = reader.ReadToEnd();
                            }
                            mIsStoredExternally = true;
                            mPhysicalFilePath = webPartFile;
                        }
                    }
                }

                // Output cache
                if (UsePartialCache)
                {
                    content = PartialCacheHelper.AddOutputCacheDirective(content);
                }
                return content;
            }
        }


        /// <summary>
        /// Indicates whether file content is stored externally (e.g. file system)
        /// </summary>
        public bool IsStoredExternally
        {
            get
            {
                return mIsStoredExternally;
            }
        }


        /// <summary>
        /// Gets object hash string used for identification of compiled version in .NET cache
        /// </summary>
        public string ObjectHash
        {
            get
            {
                // Web part layout
                if (WebPartLayoutID > 0)
                {
                    WebPartLayoutInfo wpli = WebPartLayoutInfoProvider.GetWebPartLayoutInfo(WebPartLayoutID);
                    if (wpli != null)
                    {
                        return wpli.WebPartLayoutVersionGUID;
                    }
                }
                return null;
            }
        }


        /// <summary>
        /// Gets the physical file path if exists
        /// </summary>
        public string PhysicalFilePath
        {
            get
            {
                return mPhysicalFilePath;
            }
        }


        #endregion


        #region "Properties"

        /// <summary>
        /// Web part layout id
        /// </summary>
        private int WebPartLayoutID
        {
            get;
            set;
        }

        /// <summary>
        /// Web part id
        /// </summary>
        private int WebPartID
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether web part partial cache should be used
        /// </summary>
        private bool UsePartialCache
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
        VirtualWebPartVirtualFileObject(string virtualPath)
        {
            Parse(virtualPath);
        }

        #endregion


        #region "GetVirtualFileObjectHandler callback method"

        /// <summary>
        /// Creates IVirtualFileObject for specified virtual path (GetVirtualFileObjectHandler callback method) 
        /// </summary>
        /// <param name="virtualPath">Virtual path</param>
        internal static IVirtualFileObject GetVirtualFileObject(string virtualPath)
        {
            return new VirtualWebPartVirtualFileObject(virtualPath);
        }

        #endregion


        #region "Parsing method"

        /// <summary>
        /// Parse virtual path from format WebPartInfoProvider.VirtualWebPartsDirectory + /DocumentID/NodeID/ControlID---pc/WebPartID/WebPartLayoutID.ascx
        /// </summary>
        /// <param name="virtualPath">Virtual path</param>
        private void Parse(string virtualPath)
        {
            // Remove .ascx extension
            virtualPath = virtualPath.Substring(0, virtualPath.Length - 5);
            // Split path to the separate paths
            string[] pathParts = virtualPath.Split('/');

            // Web part Layout
            int objId = ValidationHelper.GetInteger(pathParts[7], 0);
            if (objId > 0)
            {
                WebPartLayoutInfo wpli = WebPartLayoutInfoProvider.GetWebPartLayoutInfo(objId);
                if (wpli != null)
                {
                    WebPartLayoutID = objId;
                }
            }

            // Web part
            objId = ValidationHelper.GetInteger(pathParts[6], 0);
            if (objId > 0)
            {
                WebPartInfo wpi = WebPartInfoProvider.GetWebPartInfo(objId);
                if (wpi != null)
                {
                    WebPartID = objId;
                    mIsStoredExternally = true;
                    mPhysicalFilePath = URLHelper.GetPhysicalPath(WebPartInfoProvider.GetWebPartUrl(wpi, false));
                }
            }

            // Output cache setting
            UsePartialCache = (pathParts[5].Contains(VirtualPathHelper.URLParametersSeparator + "pc"));
        }

        #endregion
    }
}
