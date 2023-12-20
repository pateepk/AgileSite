using System;

using CMS.DataEngine;

namespace CMS.PortalEngine
{
    /// <summary>
    /// Class providing WebPartContainerInfo management.
    /// </summary>
    public class WebPartContainerInfoProvider : AbstractInfoProvider<WebPartContainerInfo, WebPartContainerInfoProvider>
    {
        #region "Constants"

        /// <summary>
        /// Specifies the character that is used as a placeholder in the web part container editing
        /// </summary>
        public const string WP_CHAR = "□";

        #endregion


        #region "Public properties"

        /// <summary>
        /// Gets the WebPart containers directory path.
        /// </summary>
        public static string WebPartContainersDirectory
        {
            get
            {
                return "~/CMSVirtualFiles/WebPartContainers";
            }
        }


        /// <summary>
        /// Gets or sets the value that indicates whether WebPart containers should be stored externally.
        /// </summary>
        public static bool StoreWebPartContainersInExternalStorage
        {
            get
            {
                return SettingsKeyInfoProvider.GetBoolValue("CMSStoreWebpartContainersInFS");
            }
            set
            {
                SettingsKeyInfoProvider.SetGlobalValue("CMSStoreWebpartContainersInFS", value);
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        public WebPartContainerInfoProvider()
            : base(WebPartContainerInfo.TYPEINFO, new HashtableSettings
            {
                ID = true,
                Name = true,
                Load = LoadHashtableEnum.All
            })
        {
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Returns object with specified GUID.
        /// </summary>
        /// <param name="containerGuid">Object GUID</param>
        public static WebPartContainerInfo GetWebPartContainerInfoByGUID(Guid containerGuid)
        {
            return ProviderObject.GetInfoByGuid(containerGuid);
        }


        /// <summary>
        /// Returns the WebPartContainerInfo structure for the specified webPartContainer.
        /// </summary>
        /// <param name="containerName">Name of webpart container</param>
        public static WebPartContainerInfo GetWebPartContainerInfo(string containerName)
        {
            return ProviderObject.GetInfoByCodeName(containerName);
        }


        /// <summary>
        /// Returns the WebPartContainerInfo structure for the specified webPartContainer.
        /// </summary>
        /// <param name="containerId">WebPartContainer id</param>
        public static WebPartContainerInfo GetWebPartContainerInfo(int containerId)
        {
            return ProviderObject.GetInfoById(containerId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified webPartContainer.
        /// </summary>
        /// <param name="webPartContainer">WebPartContainer to set</param>
        public static void SetWebPartContainerInfo(WebPartContainerInfo webPartContainer)
        {
            ProviderObject.SetInfo(webPartContainer);
        }


        /// <summary>
        /// Deletes specified webPartContainer.
        /// </summary>
        /// <param name="webPartContainerObj">WebPartContainer object</param>
        public static void DeleteWebPartContainerInfo(WebPartContainerInfo webPartContainerObj)
        {
            ProviderObject.DeleteInfo(webPartContainerObj);
        }


        /// <summary>
        /// Deletes specified webPartContainer.
        /// </summary>
        /// <param name="webPartContainerId">WebPartContainer id</param>
        public static void DeleteWebPartContainerInfo(int webPartContainerId)
        {
            WebPartContainerInfo webPartContainerObj = GetWebPartContainerInfo(webPartContainerId);
            DeleteWebPartContainerInfo(webPartContainerObj);
        }


        /// <summary>
        /// Returns all web part containers.
        /// </summary>
        public static ObjectQuery<WebPartContainerInfo> GetContainers()
        {
            return ProviderObject.GetObjectQuery();
        }

        #endregion
    }
}