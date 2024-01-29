using System;
using System.Collections.Generic;
using System.Linq;

using CMS.ApplicationDashboard.Web.UI.Internal;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.Modules;
using CMS.SiteProvider;
using CMS.UIControls;

namespace CMS.ApplicationDashboard.Web.UI
{
    internal class ApplicationDashboardModuleHandlers
    {
        /// <summary>
        /// Initialization of all handlers.
        /// </summary>
        public static void Init()
        {
            CMSPage.OnAfterPagePreRender += RegisterBreadcrumbsPinJavaScript;
        }


        private static void RegisterBreadcrumbsPinJavaScript(object sender, EventArgs e)
        {
            var page = sender as CMSPage;
            if (page == null)
            {
                return;
            }

            // Don't register breadcrumbs pin when there is no current site (for example when there is no running site assigned to current domain)
            if (string.IsNullOrEmpty(page.CurrentSiteName))
            {
                return;
            }

            var uiElement = page.UIContext.UIElement;
            if (uiElement == null)
            {
                return;
            }
            int elementLevel = uiElement.ElementLevel;
            if ((elementLevel < UIElementInfoProvider.APPLICATION_LEVEL) || (elementLevel > UIElementInfoProvider.MAX_ELEMENT_LEVEL))
            {
                return;
            }

            if (page.EditedObject != null)
            {
                var editedObject = (BaseInfo)page.EditedObject;
                if (editedObject.Generalized.ObjectID == 0)
                {
                    return;
                }
            }

            bool enableSingleObjectsOnDashboard = ValidationHelper.GetBoolean(page.UIContext.Data["EnableSingleObjectsOnDashboard"], false);
            bool pageHasObjectId = (page.UIContext.ObjectID > 0) || (page.UIContext.ParentObjectID > 0);

            if ((elementLevel > UIElementInfoProvider.APPLICATION_LEVEL) && (!pageHasObjectId || !enableSingleObjectsOnDashboard))
            {
                return;
            }

            var dashboardItems = GetDashboardItems();
            var publishParameters = (elementLevel == UIElementInfoProvider.APPLICATION_LEVEL) ?
                GetPublishParametersForApplication(uiElement, dashboardItems):
                GetPublishParametersForSingleObject(page, uiElement, dashboardItems);

            RequestContext.ClientApplication.Add("breadcrumbsPin", publishParameters);
        }


        private static Dictionary<UserDashboardSetting, DashboardItem> GetDashboardItems()
        {
            var currentUser = MembershipContext.AuthenticatedUser;
            var userSpecificSettings = Service.Resolve<IUserSpecificDashboardItemsLoader>().GetUserSpecificDashboardItems(currentUser);
            var dashboardItems = userSpecificSettings ?? Service.Resolve<IDefaultDashboardItemsLoader>().GetDefaultDashboardItems(currentUser, SiteContext.CurrentSite);
            return dashboardItems;
        }


        private static DashboardItemPinSettingsModel GetPublishParametersForSingleObject(CMSPage page, UIElementInfo uiElement, Dictionary<UserDashboardSetting, DashboardItem> settings)
        {
            string codeName = Service.Resolve<IUIElementObjectPropertiesProvider>().GetCodeName((page.UIContext.ObjectID > 0) ? page.UIContext.ObjectID : page.UIContext.ParentObjectID, page.UIContext.UIElement);
            if (codeName == null)
            {
                // Object does not have code name, it is not suitable to be used on the dashboard
                return null;
            }

            Guid applicationGuid = GetApplicationGuid(page.UIContext.UIElement);
            
            var editedObject = ((BaseInfo)page.EditedObject);
            bool isPinned = settings.Any(s =>
                (s.Key.ElementGuid == uiElement.ElementGUID) &&
                (s.Key.ApplicationGuid == applicationGuid) &&
                (s.Key.ObjectName == codeName) &&
                (editedObject.IsGlobal || (s.Key.ObjectSiteName == page.CurrentSiteName))
                );

            return new DashboardItemPinSettingsModel
            {
                ApplicationGuid = applicationGuid,
                ElementGuid = uiElement.ElementGUID,
                ObjectName = codeName,
                ObjectType = Service.Resolve<IUIElementObjectPropertiesProvider>().GetObjectTypeFromUIElement(uiElement),
                ObjectSiteName = editedObject.IsGlobal ? null : page.CurrentSiteName,
                IsPinned = isPinned
            };
        }


        private static DashboardItemPinSettingsModel GetPublishParametersForApplication(UIElementInfo uiElement, Dictionary<UserDashboardSetting, DashboardItem> settings)
        {
            bool isPinned = settings.Any(s => (s.Key.ApplicationGuid == uiElement.ElementGUID) && !s.Key.ElementGuid.HasValue && string.IsNullOrEmpty(s.Key.ObjectName));

            return new DashboardItemPinSettingsModel
            {
                ApplicationGuid = uiElement.ElementGUID,
                IsPinned = isPinned
            };
        }


        private static Guid GetApplicationGuid(UIElementInfo uiElement)
        {
            var elementPathIds = uiElement.ElementIDPath.Split(new []{"/"}, StringSplitOptions.RemoveEmptyEntries);
            if (elementPathIds.Length <= UIElementInfoProvider.APPLICATION_LEVEL)
            {
                throw new Exception("Only elements at level 4 are supported.");
            }

            return UIElementInfoProvider.GetUIElementInfo(ValidationHelper.GetInteger(elementPathIds[UIElementInfoProvider.APPLICATION_LEVEL], 0)).ElementGUID;
        }
    }
}