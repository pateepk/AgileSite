using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.DataEngine.Query;
using CMS.Helpers;

namespace CMS.Modules
{
    using TypedDataSet = InfoDataSet<UIElementInfo>;

    /// <summary>
    /// UIElement info data container class.
    /// </summary>
    public class UIElementInfoProvider : AbstractInfoProvider<UIElementInfo, UIElementInfoProvider>, IFullNameInfoProvider, IRelatedObjectCountProvider
    {
        #region "Constants"

        /// <summary>
        /// Defines application root level.
        /// </summary>
        public const int APPLICATION_LEVEL = 3;


        /// <summary>
        /// Maximum nesting level of UI element.
        /// </summary>
        public const int MAX_ELEMENT_LEVEL = 5;

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor. Init hashtables for ID and Fullname (ResourceName|ElementName)
        /// </summary>
        public UIElementInfoProvider()
            : base(UIElementInfo.TYPEINFO, new HashtableSettings
                {
                    ID = true,
                    GUID = true,
                    FullName = true
                })
        {
        }

        #endregion


        #region "Properties"

        /// <summary>
        /// Indicates whether allow editing only current selected module
        /// </summary>
        public static bool AllowEditOnlyCurrentModule
        {
            get
            {
                return !SystemContext.DevelopmentMode;
            }
        }

        #endregion


        #region "Public static methods"

        /// <summary>
        /// Gets the element caption. If caption is not set the display name is used.
        /// </summary>
        /// <param name="uiElement">UI element instance</param>
        /// <param name="localize">Indicates whether output string shout be localized</param>
        public static string GetElementCaption(UIElementInfo uiElement, bool localize = true)
        {
            if (uiElement == null)
            {
                return String.Empty;
            }

            string caption = uiElement.ElementCaption;
            if (String.IsNullOrEmpty(caption))
            {
                caption = uiElement.ElementDisplayName;
            }

            if (localize)
            {
                caption = ResHelper.LocalizeString(caption);
            }

            return caption;
        }


        /// <summary>
        /// Returns all UI elements.
        /// </summary>
        public static ObjectQuery<UIElementInfo> GetUIElements()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns object with specified GUID.
        /// </summary>
        /// <param name="guid">Object GUID</param>
        public static UIElementInfo GetUIElementInfo(Guid guid)
        {
            return ProviderObject.GetInfoByGuid(guid);
        }


        /// <summary>
        /// Returns the UIElement info object.
        /// </summary>
        /// <param name="elementId">ID of the UIElement to retrieve</param>
        public static UIElementInfo GetUIElementInfo(int elementId)
        {
            return ProviderObject.GetInfoById(elementId);
        }


        /// <summary>
        /// Returns the root UIElement info object for Resource.
        /// </summary>
        /// <param name="resourceId">ID of resource (module)</param>
        public static UIElementInfo GetRootUIElementInfo(int resourceId)
        {
            return ProviderObject.GetRootUIElementInfoInternal(resourceId);
        }


        /// <summary>
        /// Returns the UIElement info object.
        /// </summary>
        /// <param name="elementName">Codename of the UIElement to retrieve</param>
        /// <param name="resourceName">Name of the resource (module)</param>
        public static UIElementInfo GetUIElementInfo(string resourceName, string elementName)
        {
            return ProviderObject.GetUIElementInfoInternal(resourceName, elementName);
        }


        /// <summary>
        /// Returns the UIElement info object.
        /// </summary>
        /// <param name="elementName">Codename of the UIElement to retrieve</param>
        /// <param name="resourceId">ID of the resource (module)</param>
        public static UIElementInfo GetUIElementInfo(int resourceId, string elementName)
        {
            return ProviderObject.GetUIElementInfoInternal(resourceId, elementName);
        }


        /// <summary>
        /// Sets the specified UIElement data.
        /// </summary>
        /// <param name="infoObj">UIElement info data</param>
        public static void SetUIElementInfo(UIElementInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes the specified UIElement.
        /// </summary>
        /// <param name="infoObj">UIElement object</param>
        public static void DeleteUIElementInfo(UIElementInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes the specified UIElement.
        /// </summary>
        /// <param name="elementId">Element ID</param>
        public static void DeleteUIElementInfo(int elementId)
        {
            UIElementInfo infoObj = GetUIElementInfo(elementId);
            DeleteUIElementInfo(infoObj);
        }


        /// <summary>
        /// Returns DataSet with child elements of the specified element ordered by ElementOrder.
        /// Only elements with not empty ElementCaption will be returned because it says that such elements represent menu items.
        /// </summary>
        /// <param name="resourceName">Name of the resource (module)</param>
        /// <param name="elementName">CodeName of the UIElement</param>
        public static TypedDataSet GetChildUIElements(string resourceName, string elementName)
        {
            return ProviderObject.GetChildUIElementsInternal(resourceName, elementName);
        }


        /// <summary>
        /// Returns DataSet with child elements of the specified parent element.
        /// </summary>
        /// <param name="parentElementId">ID of the parent UIElement</param>
        public static ObjectQuery<UIElementInfo> GetChildUIElements(int parentElementId)
        {
            return ProviderObject.GetChildUIElementsInternal(parentElementId, null);
        }


        /// <summary>
        /// Moves specified UI element up within the parent UI element.
        /// </summary>
        /// <param name="elementId">ID of the UI element</param>
        public static void MoveUIElementUp(int elementId)
        {
            ProviderObject.MoveUIElementUpInternal(elementId);
        }


        /// <summary>
        /// Moves specified UI element down within the parent UI element.
        /// </summary>
        /// <param name="elementId">ID of the UI element</param>
        public static void MoveUIElementDown(int elementId)
        {
            ProviderObject.MoveUIElementDownInternal(elementId);
        }


        /// <summary>
        /// Sets correct ElementChildCount to the specified UIElement.
        /// </summary>
        /// <param name="elementId">ID of the UIElement</param>
        public static void SetUIElementChildCount(int elementId)
        {
            ProviderObject.SetUIElementChildCountInternal(elementId);
        }


        /// <summary>
        /// Returns all UIElements for which the roles have permission.
        /// </summary>
        /// <param name="rolesIds">Role IDs</param>
        /// <param name="columns">Data columns to return</param>
        public static TypedDataSet GetRolesUIElements(IEnumerable<int> rolesIds, string columns)
        {
            return ProviderObject.GetRolesUIElementsInternal(rolesIds, columns);
        }


        /// <summary>
        /// Returns root UIElement for the specified module.
        /// </summary>
        /// <param name="resourceName">Name of the module</param>
        public static UIElementInfo GetRootUIElementInfo(string resourceName)
        {
            return ProviderObject.GetRootUIElementInfoInternal(resourceName);
        }


        /// <summary>
        /// Returns child UIElements for the specified module.
        /// </summary>
        /// <param name="resourceName">Name of the module</param>
        public static TypedDataSet GetRootChildUIElements(string resourceName)
        {
            return ProviderObject.GetRootChildUIElementsInternal(resourceName);
        }


        /// <summary>
        /// Returns last element order for specified parent element.
        /// </summary>
        /// <param name="parentElementId">Parent element id</param>
        public static int GetLastElementOrder(int parentElementId)
        {
            return ProviderObject.GetLastElementOrderInternal(parentElementId);
        }


        /// <summary>
        /// Updates all counts for all sub-objects.
        /// </summary>
        public static void RefreshDataCounts()
        {
            ProviderObject.RefreshDataCountsInternal();
        }


        /// <summary>
        /// Gets top module's element (element with lowest element level)
        /// </summary>
        public static UIElementInfo GetModuleTopUIElement(int moduleID)
        {
            return ProviderObject.GetModuleTopUIElementInternal(moduleID);
        }


        /// <summary>
        /// Returns navigation string that describes a route to the application given by UI element.
        /// </summary>
        /// <param name="moduleName">Module name</param>
        /// <param name="elementName">Element name</param>
        /// <param name="cultureCode">Culture code</param>
        public static string GetApplicationNavigationString(string moduleName, string elementName, string cultureCode = null)
        {
            UIElementInfo info = GetUIElementInfo(moduleName, elementName);
            return info?.GetApplicationNavigationString(cultureCode) ?? String.Empty;
        }

        #endregion


        #region "Overridden methods"

        /// <summary>
        /// Updates all counts for all sub-objects.
        /// </summary>
        public void RefreshObjectsCounts()
        {
            RefreshDataCountsInternal();
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Gets top module's element (element with lowest element level)
        /// </summary>
        protected virtual UIElementInfo GetModuleTopUIElementInternal(int moduleID)
        {
            return GetUIElements()
                .WhereEquals("ElementResourceID", moduleID)
                .OrderByAscending("ElementLevel")
                .TopN(1)
                .FirstOrDefault();
        }


        /// <summary>
        /// Returns the UIElement info object.
        /// </summary>
        /// <param name="elementName">Codename of the UIElement to retrieve</param>
        /// <param name="resourceName">Name of the resource (module)</param>
        protected virtual UIElementInfo GetUIElementInfoInternal(string resourceName, string elementName)
        {
            return GetInfoByFullName($"{resourceName}|{elementName}");
        }


        /// <summary>
        /// Returns the UIElement info object.
        /// </summary>
        /// <param name="elementName">Codename of the UIElement to retrieve</param>
        /// <param name="resourceId">ID of the resource (module)</param>
        protected virtual UIElementInfo GetUIElementInfoInternal(int resourceId, string elementName)
        {
            // Get resource
            ResourceInfo ri = ResourceInfoProvider.GetResourceInfo(resourceId);
            if (ri != null)
            {
                return GetInfoByFullName($"{ri.ResourceName}|{elementName}");
            }

            return null;
        }


        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(UIElementInfo info)
        {
            if (info != null)
            {
                // Update
                if (info.ElementID > 0)
                {
                    UIElementInfo oldInfo = GetUIElementInfo(info.ElementID);
                    if (oldInfo != null)
                    {
                        int oldParentId = oldInfo.ElementParentID;
                        int newParentId = info.ElementParentID;

                        base.SetInfo(info);

                        if (oldParentId != newParentId)
                        {
                            SetUIElementChildCount(oldInfo.ElementParentID);
                            SetUIElementChildCount(info.ElementParentID);

                            if (info.ElementChildCount > 0)
                            {
                                // Clear hashtables to refresh children IDPaths
                                ClearHashtables(true);
                            }
                        }
                    }
                }
                // Insert
                else
                {
                    // Default values (not null columns)
                    info.ElementChildCount = 0;
                    info.ElementIDPath = "";
                    info.ElementLevel = 0;

                    // Ensure parent if not set
                    if (info.ElementParentID == 0)
                    {
                        var parent = GetUIElementInfoInternal(ModuleName.CMS, "cms");
                        if (parent != null)
                        {
                            info.ElementParentID = parent.ElementID;
                        }
                    }

                    base.SetInfo(info);

                    // Update child count
                    SetUIElementChildCount(info.ElementParentID);
                }
            }
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(UIElementInfo info)
        {
            if (info != null)
            {
                base.DeleteInfo(info);

                // Update child count
                SetUIElementChildCount(info.ElementParentID);
            }
        }


        /// <summary>
        /// Returns DataSet with child elements of the specified element ordered by ElementOrder.
        /// Only elements with not empty ElementCaption will be returned because it says that such elements represent menu items.
        /// </summary>
        /// <param name="resourceName">Name of the resource (module)</param>
        /// <param name="elementName">CodeName of the UIElement</param>
        protected virtual TypedDataSet GetChildUIElementsInternal(string resourceName, string elementName)
        {
            return UIElementCachingHelper.GetElements(resourceName, elementName, () => GetChildUiElements(resourceName, elementName));
        }


        /// <summary>
        /// Returns DataSet with child elements of the specified parent element.
        /// </summary>
        /// <param name="parentElementId">ID of the parent UIElement</param>
        /// <param name="columns">Data columns to return</param>
        protected virtual ObjectQuery<UIElementInfo> GetChildUIElementsInternal(int parentElementId, string columns)
        {
            WhereCondition where = new WhereCondition();
            if (parentElementId > 0)
            {
                where.WhereEquals("ElementParentID", parentElementId);
            }
            else
            {
                where.WhereNull("ElementParentID");
            }

            return GetUIElements().Where(where).OrderBy("ElementOrder").Columns(columns).BinaryData(true);
        }


        /// <summary>
        /// Moves specified UI element up within the parent UI element.
        /// </summary>
        /// <param name="elementId">ID of the UI element</param>
        protected virtual void MoveUIElementUpInternal(int elementId)
        {
            if (elementId > 0)
            {
                var infoObj = GetInfoById(elementId);

                infoObj?.Generalized.MoveObjectUp();
            }
        }


        /// <summary>
        /// Moves specified UI element down within the parent UI element.
        /// </summary>
        /// <param name="elementId">ID of the UI element</param>
        protected virtual void MoveUIElementDownInternal(int elementId)
        {
            if (elementId > 0)
            {
                var infoObj = GetInfoById(elementId);

                infoObj?.Generalized.MoveObjectDown();
            }
        }


        /// <summary>
        /// Sets correct ElementChildCount to the specified UIElement.
        /// </summary>
        /// <param name="elementId">ID of the UIElement</param>
        protected virtual void SetUIElementChildCountInternal(int elementId)
        {
            if (elementId > 0)
            {
                UIElementInfo ei = GetUIElementInfo(elementId);
                if (ei != null)
                {
                    ei.ElementChildCount = GetUIElements()
                        .WhereEquals("ElementParentID", elementId)
                        .Columns("ElementID")
                        .Count;

                    using (CMSActionContext context = new CMSActionContext())
                    {
                        // Disable logging of tasks
                        context.DisableLogging();

                        SetUIElementInfo(ei);
                    }
                }
            }
        }


        /// <summary>
        /// Returns all UIElements for which the given roles have permission.
        /// </summary>
        /// <param name="roleIds">Roles IDs</param>
        /// <param name="columns">Data columns to return</param>
        protected virtual TypedDataSet GetRolesUIElementsInternal(IEnumerable<int> roleIds, string columns)
        {
            if (roleIds == null)
            {
                return null;
            }

            var parameters = new QueryDataParameters();
            parameters.EnsureDataSet<UIElementInfo>();

            return ConnectionHelper.ExecuteQuery("cms.uielement.getroleselements", parameters, SqlHelper.GetWhereCondition("RoleID", roleIds), null, 0, columns).As<UIElementInfo>();
        }


        /// <summary>
        /// Returns the root UIElement info object for Resource.
        /// </summary>
        /// <param name="resourceId">ID of resource (module)</param>
        protected virtual UIElementInfo GetRootUIElementInfoInternal(int resourceId)
        {
            if (resourceId > 0)
            {
                return GetUIElements()
                    .WhereEquals("ElementResourceID", resourceId)
                    .OrderBy("ElementLevel", "ElementOrder")
                    .TopN(1)
                    .FirstOrDefault();
            }
            return null;
        }


        /// <summary>
        /// Returns root UIElement for the specified module.
        /// </summary>
        /// <param name="resourceName">Name of the module</param>
        protected virtual UIElementInfo GetRootUIElementInfoInternal(string resourceName)
        {
            if (!String.IsNullOrEmpty(resourceName))
            {
                string where = $@"ElementName = N'{SqlHelper.GetSafeQueryString(resourceName.Replace(".", ""), false)}'
    AND ElementResourceID IN (SELECT ResourceID FROM CMS_RESOURCE WHERE ResourceName = N'{SqlHelper.GetSafeQueryString(resourceName, false)}')";

                return GetUIElements()
                    .Where(where)
                    .TopN(1)
                    .FirstOrDefault();
            }

            return null;
        }


        /// <summary>
        /// Returns child UIElements for the specified module.
        /// </summary>
        /// <param name="resourceName">Name of the module</param>
        protected virtual TypedDataSet GetRootChildUIElementsInternal(string resourceName)
        {
            if (!String.IsNullOrEmpty(resourceName))
            {
                // Try to find root element with name based on module name
                UIElementInfo ui = GetRootUIElementInfo(resourceName);
                if (ui == null)
                {
                    // If not found set first top level element
                    ResourceInfo ri = ResourceInfoProvider.GetResourceInfo(resourceName);
                    if (ri != null)
                    {
                        ui = GetModuleTopUIElement(ri.ResourceID);
                    }
                }

                if (ui != null)
                {
                    string where = $"((ElementCaption IS NOT NULL) AND NOT (ElementCaption = '')) AND (ElementParentID = {ui.ElementID})";

                    return GetUIElements().Where(where).OrderBy("ElementOrder").BinaryData(true).TypedResult;
                }
            }
            return null;
        }


        /// <summary>
        /// Returns last order for specified parent element.
        /// </summary>
        /// <param name="parentElementId">Parent element id</param>
        protected virtual int GetLastElementOrderInternal(int parentElementId)
        {
            if (parentElementId > 0)
            {
                return GetUIElements()
                    .WhereEquals("ElementParentID", parentElementId)
                    .OrderByDescending("ElementOrder")
                    .TopN(1)
                    .Columns("ElementOrder")
                    .GetScalarResult(0);
            }

            return 0;
        }


        /// <summary>
        /// Updates all counts for all sub-objects.
        /// </summary>
        protected virtual void RefreshDataCountsInternal()
        {
            ConnectionHelper.ExecuteQuery("cms.uielement.refreshdatacounts", null);
        }


        /// <summary>
        /// Creates new dictionary for caching the objects by full name
        /// </summary>
        public ProviderInfoDictionary<string> GetFullNameDictionary()
        {
            return new ProviderInfoDictionary<string>(UIElementInfo.OBJECT_TYPE, "ElementResourceID;ElementName");
        }


        /// <summary>
        /// Gets the where condition that searches the object based on the given full name
        /// </summary>
        /// <param name="fullName">Object full name</param>
        public string GetFullNameWhereCondition(string fullName)
        {
            string resourceName;
            string elementName;

            if (ObjectHelper.ParseFullName(fullName, out resourceName, out elementName, "|"))
            {
                // Get the resource
                var resource = ResourceInfoProvider.GetResourceInfo(resourceName);
                if (resource == null)
                {
                    return null;
                }

                return $"ElementName = N'{SqlHelper.GetSafeQueryString(elementName, false)}' AND ElementResourceID = {resource.ResourceID}";
            }

            return null;
        }


        private TypedDataSet GetChildUiElements(string resourceName, string elementName)
        {
            if (!String.IsNullOrEmpty(elementName) && !String.IsNullOrEmpty(resourceName))
            {
                string where = $@"(ElementParentID IN (SELECT ElementID FROM CMS_UIElement WHERE (ElementName = N'{SqlHelper.GetSafeQueryString(elementName, false)}')
    AND (ElementResourceID IN (SELECT ResourceID FROM CMS_Resource WHERE ResourceName = N'{SqlHelper.GetSafeQueryString(resourceName, false)}'))))";

                return GetUIElements().Where(where).OrderBy("ElementOrder").BinaryData(true).TypedResult;
            }

            return null;
        }

        #endregion
    }
}