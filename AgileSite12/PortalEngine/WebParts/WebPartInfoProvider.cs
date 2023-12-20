using System;
using System.Linq;

using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.IO;


namespace CMS.PortalEngine
{
    /// <summary>
    /// Class to provide the <see cref="WebPartInfo"/> management.
    /// </summary>
    public class WebPartInfoProvider : AbstractInfoProvider<WebPartInfo, WebPartInfoProvider>
    {
        #region "Properties"

        /// <summary>
        /// Relative path of base directory for the web part files.
        /// </summary>
        public static string WebPartsDirectory
        {
            get
            {
                return "~/CMSWebParts";
            }
        }


        /// <summary>
        /// Relative path of the directory for the web part virtual objects.
        /// </summary>
        public static string VirtualWebPartsDirectory
        {
            get
            {
                return "~/CMSVirtualFiles/VirtualWebParts";
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        public WebPartInfoProvider()
            : base(WebPartInfo.TYPEINFO, new HashtableSettings
            {
                ID = true,
                Name = true,
            })
        {
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Returns all web parts;
        /// </summary>
        public static ObjectQuery<WebPartInfo> GetWebParts()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Sets the specified WebPart info data.
        /// </summary>
        /// <param name="infoObj">CultureInfo object to set (save as new or update existing)</param>
        public static void SetWebPartInfo(WebPartInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Returns WebPartInfo object for specified WebPartName.
        /// </summary>
        /// <param name="webPartName">Name of WebPart to retrieve</param>
        public static WebPartInfo GetWebPartInfo(string webPartName)
        {
            return ProviderObject.GetInfoByCodeName(webPartName);
        }


        /// <summary>
        /// Returns WebPartInfo object for specified WebPartID.
        /// </summary>
        /// <param name="webPartId">ID of WebPart to retrieve</param>
        public static WebPartInfo GetWebPartInfo(int webPartId)
        {
            return ProviderObject.GetInfoById(webPartId);
        }


        /// <summary>
        /// Delete specified WebPart.
        /// </summary>
        /// <param name="wi">WebPart object</param>
        public static void DeleteWebPartInfo(WebPartInfo wi)
        {
            ProviderObject.DeleteInfo(wi);
        }


        /// <summary>
        /// Delete specified WebPart.
        /// </summary>
        /// <param name="webPartName">Name of the WebPart to delete</param>
        public static void DeleteWebPartInfo(string webPartName)
        {
            WebPartInfo wi = GetWebPartInfo(webPartName);
            if (wi != null)
            {
                DeleteWebPartInfo(wi);
            }
        }


        /// <summary>
        /// Delete specified WebPart.
        /// </summary>
        /// <param name="webPartId">ID of the WebPart to delete</param>
        public static void DeleteWebPartInfo(int webPartId)
        {
            WebPartInfo wi = GetWebPartInfo(webPartId);
            if (wi != null)
            {
                DeleteWebPartInfo(wi);
            }
        }


        /// <summary>
        /// Deletes all the child web parts of the specified web part.
        /// </summary>
        /// <param name="webPartId">Web part ID</param>
        public static void DeleteChildWebParts(int webPartId)
        {
            var childwebParts = GetWebParts()
                                    .Where(x => x.WebPartParentID == webPartId)
                                    .Select(x => x.WebPartID);

            foreach (int childWebpartId in childwebParts)
            {
                DeleteWebPartInfo(childWebpartId);
            }
        }


        /// <summary>
        /// Gets the base web part for the specified web part.
        /// </summary>
        /// <param name="wpi">Web part</param>
        public static WebPartInfo GetBaseWebPart(WebPartInfo wpi)
        {
            if (wpi == null)
            {
                return null;
            }

            // Get web part info from parent
            if (wpi.WebPartParentID > 0)
            {
                wpi = GetWebPartInfo(wpi.WebPartParentID);
            }

            return wpi;
        }


        /// <summary>
        /// Returns URL to specified WebPart.
        /// </summary>
        /// <param name="partInfo">Web part info object</param>
        /// <param name="resolveUrl">If true, the URL is resolved</param>
        public static string GetWebPartUrl(WebPartInfo partInfo, bool resolveUrl = true)
        {
            if (partInfo == null)
            {
                return string.Empty;
            }

            // Get base web part
            partInfo = GetBaseWebPart(partInfo);
            if (partInfo == null)
            {
                return string.Empty;
            }

            string url = Path.EnsureSlashes(partInfo.WebPartFileName, true);

            if (!url.StartsWithCSafe("~/"))
            {
                url = WebPartsDirectory + "/" + url.TrimStart('/');
            }

            // Resolve URL if necessary
            if (resolveUrl)
            {
                url = URLHelper.ResolveUrl(url);
            }

            return url;
        }


        /// <summary>
        /// Returns webpart physical path.
        /// </summary>
        /// <param name="webpartPath">Webpart path</param>
        public static string GetWebPartPhysicalPath(string webpartPath)
        {
            return GetFullPhysicalPath(webpartPath, 0);
        }


        /// <summary>
        /// Returns full path of the web part.
        /// </summary>
        /// <param name="partInfo">Web part info object</param>
        /// <param name="webFullPath">Full path to the root of the web project (e.g. c:\WebProject\)</param>
        public static string GetFullPhysicalPath(WebPartInfo partInfo, string webFullPath = null)
        {
            if (partInfo == null)
            {
                return string.Empty;
            }

            return GetFullPhysicalPath(partInfo.WebPartFileName, partInfo.WebPartParentID, webFullPath);
        }


        /// <summary>
        /// Returns full path of the web part.
        /// </summary>
        /// <param name="webPartFileName">Web part file name</param>
        /// <param name="parentWebPartId">Parent web part ID</param>
        /// <param name="webFullPath">Full path to the root of the web project (e.g. c:\WebProject\)</param>
        public static string GetFullPhysicalPath(string webPartFileName, int parentWebPartId, string webFullPath = null)
        {
            if (parentWebPartId > 0)
            {
                WebPartInfo parent = GetWebPartInfo(parentWebPartId);
                if (parent == null)
                {
                    return string.Empty;
                }

                webPartFileName = parent.WebPartFileName;
            }

            return FileHelper.GetFullPhysicalPath(webPartFileName.Trim(), WebPartsDirectory, webFullPath);
        }


        /// <summary>
        /// Loads all the web parts into the static cache.
        /// </summary>
        public static void LoadAllWebParts()
        {
            ProviderObject.LoadAllInfos();
        }


        /// <summary>
        /// Returns virtual URL to the web part.
        /// </summary>
        /// <param name="cacheMinutes">Cache minutes</param>
        /// <param name="part">Web part instance</param>
        /// <param name="pi">IPageInfo object</param>
        /// <param name="wpi">Web part info</param>
        public static string GetVirtualWebPartUrl(IPageInfo pi, WebPartInstance part, int cacheMinutes, WebPartInfo wpi = null)
        {
            string url = null;

            // Try get web part info if not set
            if (wpi == null)
            {
                wpi = GetWebPartInfo(part.WebPartType);
            }

            if (wpi != null)
            {
                // Add partial cache suffix if required
                string controlId = part.ControlID;
                if (cacheMinutes > 0)
                {
                    controlId += VirtualPathHelper.URLParametersSeparator + "pc";
                }

                // Add available values to the path
                url = "/" + pi.DocumentID + "/" + pi.NodeID + "/" + controlId + "/" + wpi.WebPartID + "/";

                // Try get web part layout ID
                string webPartLayoutName = Convert.ToString(part.GetValue("WebPartLayout"));
                int wplid = 0;
                if (!String.IsNullOrEmpty(webPartLayoutName))
                {
                    var wpli = WebPartLayoutInfoProvider.GetWebPartLayoutInfo(part.WebPartType, webPartLayoutName);
                    if (wpli != null)
                    {
                        wplid = wpli.WebPartLayoutID;
                    }
                }

                // Create path in format: WebPartInfoProvider.VirtualWebPartsDirectory + /DocumentID/NodeID/ControlID---pc/WebPartID/WebPartLayoutID.ascx
                url = VirtualWebPartsDirectory + url + wplid + ".ascx";
            }

            return url;
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(WebPartInfo info)
        {
            if (info != null)
            {
                int oldCategoryId = ValidationHelper.GetInteger(info.GetOriginalValue("WebPartCategoryID"), 0);

                // Save the web parts
                info.WebPartDefaultConfiguration = info.DefaultConfiguration.GetZonesXML();

                base.SetInfo(info);

                // Update web part category children count
                WebPartCategoryInfoProvider.UpdateCategoryWebPartChildCount(oldCategoryId, info.WebPartCategoryID);

                // Clear cached web part/widget definitions
                info.mFormInfo = null;
                info.mDefaultConfiguration = null;

                PortalFormHelper.ClearWebPartFormInfos(true);
                PortalFormHelper.ClearWidgetFormInfos(true);
            }
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(WebPartInfo info)
        {
            base.DeleteInfo(info);

            // Update web part category children count
            WebPartCategoryInfoProvider.UpdateCategoryWebPartChildCount(0, info.WebPartCategoryID);
        }

        #endregion
    }
}