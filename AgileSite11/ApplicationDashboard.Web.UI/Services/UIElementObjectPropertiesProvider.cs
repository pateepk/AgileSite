using System;
using System.Linq;
using System.Text;

using CMS.Base;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.Modules;
using CMS.SiteProvider;

namespace CMS.ApplicationDashboard.Web.UI
{
    /// <summary>
    /// Provides method for obtaining properties of related info object contained within the <see cref="UIElementInfo"/>.
    /// </summary>
    internal class UIElementObjectPropertiesProvider : IUIElementObjectPropertiesProvider
    {
        private readonly ISiteService mSiteService;


        /// <summary>
        /// Creates new instance of <see cref="UIElementObjectPropertiesProvider"/>.
        /// </summary>
        /// <param name="siteService">Provides method for obtaining <see cref="ISiteInfo"/> for current context</param>
        /// <exception cref="ArgumentNullException"><paramref name="siteService"/> is null</exception>
        public UIElementObjectPropertiesProvider(ISiteService siteService)
        {
            if (siteService == null)
            {
                throw new ArgumentNullException("siteService");
            }

            mSiteService = siteService;
        }


        /// <summary>
        /// Returns display name of the object matching given <paramref name="codeName"/> on <paramref name="siteID"/> for the <paramref name="uiElement"/>.
        /// </summary>
        /// <param name="codeName">Code name identifying the desired info object</param>
        /// <param name="siteID">ID of the <see cref="SiteInfo"/> the desired object is assigned to</param>
        /// <param name="uiElement">UI element containing desired object</param>
        /// <returns>Display name of found object; if not found, returns null.</returns>
        public string GetDisplayName(string codeName, int siteID, UIElementInfo uiElement)
        {
            if (uiElement == null)
            {
                return null;
            }

            if (string.IsNullOrEmpty(codeName))
            {
                return null;
            }

            string objectType = GetObjectTypeFromUIElement(uiElement);
            if (objectType == null)
            {
                LogObjectTypeNotFoundEvent(uiElement);
                return null;
            }

            var baseInfo = ProviderHelper.GetInfoByName(objectType, codeName, siteID);
            if (baseInfo == null)
            {
                return null;
            }

            if (baseInfo.TypeInfo.DisplayNameColumn == ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
            {
                return null;
            }

            return baseInfo.Generalized.ObjectDisplayName;
        }


        /// <summary>
        /// Returns code name of the object identified by given <paramref name="objectID"/> contained within the <paramref name="uiElement"/>.
        /// </summary>
        /// <param name="objectID">ID of the identifying the desired info object</param>
        /// <param name="uiElement">UI element containing desired object</param>
        /// <returns>Code name of found object; if not found, returns null.</returns>
        public string GetCodeName(int objectID, UIElementInfo uiElement)
        {
            if (uiElement == null)
            {
                return null;
            }

            var objectType = GetObjectTypeFromUIElement(uiElement);
            if (objectType == null)
            {
                LogObjectTypeNotFoundEvent(uiElement);
                return null;
            }

            var baseInfo = ProviderHelper.GetInfoById(objectType, objectID);
            if (baseInfo == null)
            {
                return null;
            }

            if (baseInfo.TypeInfo.CodeNameColumn == ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
            {
                return null;
            }

            return baseInfo.Generalized.ObjectCodeName;
        }


        /// <summary>
        /// Returns object type of the object containing within the given <paramref name="uiElement"/>
        /// </summary>
        /// <param name="uiElement">UI element containing desired object type</param>
        /// <returns>Type of the found object; if not found, returns null</returns>
        public string GetObjectTypeFromUIElement(UIElementInfo uiElement)
        {
            if (uiElement == null)
            {
                return null;
            }

            XmlData data = new XmlData();
            data.LoadData(uiElement.ElementProperties);
            string objectType = ValidationHelper.GetString(data["ObjectType"], String.Empty);
            if (String.IsNullOrEmpty(objectType))
            {
                return GetObjectTypeFromUIElement(UIElementInfoProvider.GetUIElementInfo(uiElement.ElementParentID));
            }

            return objectType;
        }


        /// <summary>
        /// Logs warning event telling that system could not found object type for given <paramref name="uiElement"/>.
        /// </summary>
        private void LogObjectTypeNotFoundEvent(UIElementInfo uiElement)
        {
            int siteId = SiteInfoProvider.GetSiteID(mSiteService.CurrentSite.SiteName);
            EventLogProvider.LogWarning("UIElementObjectPropertiesProvider", "UNKNOWN_OBJECT_TYPE", null,
                siteId, string.Format("Object type could not be found for UIElement {0}. It has to be specified explicitly in the element properties", uiElement.ElementFullName));
        }
    }
}
