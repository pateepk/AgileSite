using System;
using System.Collections.Generic;
using System.Data;

using CMS.Base;
using CMS.DataEngine;
using CMS.FormEngine.Web.UI;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.MacroEngine;
using CMS.Membership;
using CMS.Modules;
using CMS.SiteProvider;

namespace CMS.UIControls
{
    /// <summary>
    /// Provides functionality for working with applications.
    /// </summary>
    public class ApplicationUIHelper
    {
        #region "Variables"

        private static readonly IList<string> ApplicationSelectColumns = new[] { "ElementID", "ElementDisplayName", "ElementDescription", "ElementName", "ElementResourceID", "ElementParentID", "ElementOrder", "ElementLevel", "ElementGUID", "ElementVisibilityCondition", "ElementAccessCondition", "ElementFeature", "ElementIDPath", "ElementIconClass", "ElementIconPath", "ElementIsGlobalApplication", "ElementCaption", "ElementCheckModuleReadPermission", nameof(UIElementInfo.ElementRequiresGlobalAdminPriviligeLevel) };

        #endregion


        #region "Constants"

        /// <summary>
        /// Link to page where the user can submit a support issue.
        /// </summary>
        public const string REPORT_BUG_URL = "http://www.kentico.com/support/submit-support-issue?utm_campaign=helpbar";

        #endregion


        /// <summary>
        /// Filter applications which are not available for a given user.
        /// </summary>
        /// <param name="ds">Dataset with applications and categories</param>
        /// <param name="user">Related user</param>
        /// <param name="ensureCategories">Indicates whether resulting data set also contains categories</param>
        public static DataSet FilterApplications(DataSet ds, UserInfo user, bool ensureCategories)
        {
            if (ds == null)
            {
                return null;
            }

            user = user ?? MembershipContext.AuthenticatedUser;

            bool isGlobAdmin = user.CheckPrivilegeLevel(UserPrivilegeLevelEnum.GlobalAdmin);
            
            // Check if site is running with valid license
            bool currentSiteIsNotOperating = String.IsNullOrEmpty(SiteContext.CurrentSiteName) || (LicenseHelper.ValidateLicenseForDomain(RequestContext.CurrentDomain) != LicenseValidationEnum.Valid);

            // Check CMS and CMS Desk elements
            bool ancestorsAvailable = currentSiteIsNotOperating || (user.IsAuthorizedPerUIElement("cms", "cms") && user.IsAuthorizedPerUIElement("cms", "administration"));

            // Category list - number of elements, visible, available and category order
            var categories = new Dictionary<int, ApplicationCategoryRecord>();

            // Copy table structure 
            DataSet filteredApps = ds.Clone();

            if (ensureCategories)
            {
                DataColumn dc = new DataColumn("CategoryOrder", typeof(Int32));
                dc.DefaultValue = Int32.MaxValue;
                filteredApps.Tables[0].Columns.Add(dc);
            }

            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                UIElementInfo currentElement = new UIElementInfo(dr);
                bool requiresGlobalAdmin = currentElement.ElementRequiresGlobalAdminPriviligeLevel;

                if (HideElement(isGlobAdmin, requiresGlobalAdmin, !currentSiteIsNotOperating, currentElement.ElementIsGlobalApplication))
                {
                    continue;
                }


                #region "Categories"

                // Get parent element ID (Category ID)            
                int categoryID = currentElement.ElementParentID;

                // Category data container
                ApplicationCategoryRecord categoryData;

                // Ensure category
                if (!categories.ContainsKey(categoryID))
                {
                    // Get category element
                    UIElementInfo categoryElement = UIElementInfoProvider.GetUIElementInfo(categoryID);

                    // Category visibility
                    bool isVisible = UIContextHelper.CheckElementVisibilityCondition(categoryElement);

                    // Category permissions
                    bool isAccessible = isGlobAdmin || ancestorsAvailable && PermissionConditions(categoryElement, requiresGlobalAdmin, isGlobAdmin) && UIContextHelper.CheckFeatureAvailableInUI(categoryElement);

                    // Add to the category collection
                    categoryData = new ApplicationCategoryRecord(0, isVisible, isAccessible, categoryElement.ElementOrder);
                    categories.Add(categoryID, categoryData);
                }
                else
                {
                    categoryData = categories[categoryID];
                }

                if (currentElement.ElementRequiresGlobalAdminPriviligeLevel)
                {
                    // Always display category for global application
                    if (!categoryData.IsVisible)
                    {
                        categoryData.ShowOnlyGlobalApps = true;
                        categoryData.IsVisible = true;
                    }
                }
                // Do not process elements under invisible category
                else if (!categoryData.IsVisible || categoryData.ShowOnlyGlobalApps)
                {
                    continue;
                }

                // Check category permissions - Do not process elements under inaccessible category
                if (!categoryData.IsAccessible && !isGlobAdmin)
                {
                    continue;
                }

                #endregion


                if (!CheckElementRestrictions(currentElement, requiresGlobalAdmin, isGlobAdmin))
                {
                    continue;
                }

                // Increase number of items for category
                categoryData.ChildCount++;

                // Localize display names
                string displayName = ValidationHelper.GetString(dr["ElementCaption"], "");
                if (String.IsNullOrEmpty(displayName))
                {
                    displayName = ValidationHelper.GetString(dr["ElementDisplayName"], "");
                }

                displayName = ResHelper.LocalizeString(displayName, user.PreferredUICultureCode);

                filteredApps.Tables[0].ImportRow(dr);
                filteredApps.Tables[0].Rows[filteredApps.Tables[0].Rows.Count - 1]["ElementDisplayName"] = displayName;
            }

            string orderBy = "ElementDisplayName";

            if (ensureCategories)
            {
                DataTable table = filteredApps.Tables[0];
                orderBy = "CategoryOrder, " + orderBy;

                // Append categories to application data set
                foreach (var category in categories)
                {
                    // Check number of applications / visibility / accessibility
                    if ((category.Value.ChildCount > 0) && category.Value.IsVisible && category.Value.IsAccessible)
                    {
                        // Add to DataSet
                        DataRow newRow = table.NewRow();
                        UIElementInfo catInfo = UIElementInfoProvider.GetUIElementInfo(category.Key);

                        foreach (string columnName in ApplicationSelectColumns)
                        {
                            object columnValue = catInfo.GetValue(columnName);
                            if (columnName.Equals("ElementDisplayName", StringComparison.OrdinalIgnoreCase))
                            {
                                columnValue = ResHelper.LocalizeString(UIElementInfoProvider.GetElementCaption(catInfo, false), user.PreferredUICultureCode);
                            }

                            newRow[columnName] = columnValue ?? DBNull.Value;
                        }
                        newRow["CategoryOrder"] = category.Value.Order;
                        table.Rows.Add(newRow);
                    }
                }
            }

            // Sort categories and applications
            filteredApps.Tables[0].DefaultView.Sort = orderBy;

            return filteredApps;
        }


        /// <summary>
        /// Decides if element should be hidden based on <paramref name="userIsGlobalAdmin"/>, <paramref name="requiresGlobalAdmin"/>, 
        /// <paramref name="currentSiteIsOperating"/> and <paramref name="elementIsGlobal"/> flags.
        /// </summary>
        /// <returns>Returns true if an element should be hidden. False otherwise.</returns>
        private static bool HideElement(bool userIsGlobalAdmin, bool requiresGlobalAdmin, bool currentSiteIsOperating, bool elementIsGlobal)
        {
            return !((userIsGlobalAdmin || !requiresGlobalAdmin) && (currentSiteIsOperating || elementIsGlobal));
        }


        /// <summary>
        /// Returns applications data set.
        /// </summary>
        /// <param name="condition">Additional WHERE condition</param>
        public static DataSet LoadApplications(WhereCondition condition = null)
        {
            DataSet ds = null;

            // Try to retrieve data set from cache
            using (var cs = new CachedSection<DataSet>(ref ds, 1440, true, null, "appListDataSource", condition?.ToString(true)))
            {
                // Check whether data should be loaded
                if (cs.LoadData)
                {
                    // Compose query
                    var query = UIElementInfoProvider.GetUIElements()
                                                     .Columns(ApplicationSelectColumns)
                                                     .Where(new WhereCondition(@"ElementIDPath LIKE (SELECT ElementIDPath  FROM CMS_UIElement WHERE ElementName = 'administration' 
                                                    AND ElementResourceID IN (SELECT ResourceID FROM CMS_Resource WHERE ResourceName = 'cms')) + '/%'")
                                                     )
                                                     .WhereEquals("ElementLevel", 3)
                                                     .Where(condition);

                    // Get the data
                    ds = query.Result;

                    if (cs.Cached)
                    {
                        // Prepare cache dependency
                        cs.CacheDependency = CacheHelper.GetCacheDependency(new[] { "cms.uielement|all", "cms.rolepermission|all", "cms.userrole|all", "cms.membershiprole|all", "cms.membershipuser|all", "cms.role|all", "cms.membership|all" });
                    }

                    cs.Data = ds;
                }
            }

            return ds;
        }


        /// <summary>
        /// Checks whether resource is assigned to the current site.
        /// </summary>
        /// <param name="element">UI element info</param>
        /// <param name="isGlobalApp">Is global application flag</param>
        /// <param name="isGlobalAdmin">Is global administrator flag</param>
        private static bool ResourceOnSiteCondition(UIElementInfo element, bool isGlobalApp, bool isGlobalAdmin)
        {
            if (isGlobalApp)
            {
                return isGlobalAdmin;
            }

            // Check the module-site assignment
            if (!ResourceSiteInfoProvider.IsResourceOnSite(ApplicationUrlHelper.GetResourceName(element.ElementResourceID), SiteContext.CurrentSiteName))
            {
                return false;
            }

            return true;
        }


        /// <summary>
        /// Checks Element view permission or tries to check Read permissions of the resource. Also if user is authorized per UI element.
        /// </summary>
        /// <param name="element">UI element info</param>
        /// <param name="isGlobalApp">Is global application flag</param>
        /// <param name="isGlobalAdmin">Is global administrator flag</param>
        /// <param name="automaticReadCheck">Indicates whether check read module permission for empty view permission</param>
        private static bool PermissionConditions(UIElementInfo element, bool isGlobalApp, bool isGlobalAdmin, bool automaticReadCheck = true)
        {
            if (!isGlobalApp && !isGlobalAdmin)
            {
                return CheckElementAccess(element, null, automaticReadCheck).ElementAccessCheckStatus == ElementAccessCheckStatus.NoRestrictions;
            }

            return true;
        }


        /// <summary>
        /// Checks permission and access condition for single UI element. 
        /// If element's ElementCheckModuleReadPermission property and <paramref name="automaticReadCheck"/> are set, read permission for element's module is also checked.
        /// </summary>
        /// <param name="element">Element info</param>
        /// <param name="resolver">Macro resolver</param>
        /// <param name="automaticReadCheck">Indicates whether read module permission should be checked. Has no effect when element's ElementCheckModuleReadPermission property is false.</param>
        /// <returns>
        /// Returns <see cref="ElementAccessCheckResult"/> type with access check result.
        /// If <paramref name="element"/> is null, returned <see cref="ElementAccessCheckResult"/> has status
        /// <see cref="ElementAccessCheckStatus.NoRestrictions"/> and its <see cref="ElementAccessCheckResult.UIElementInfo"/> is null.
        /// </returns>
        internal static ElementAccessCheckResult CheckElementAccess(UIElementInfo element, MacroResolver resolver = null, bool automaticReadCheck = true)
        {
            if (element != null)
            {
                UserInfo user = MembershipContext.AuthenticatedUser;
                String resourceName = ApplicationUrlHelper.GetResourceName(element.ElementResourceID);

                // (Access condition)             
                if (!String.IsNullOrEmpty(element.ElementAccessCondition))
                {
                    bool result = ValidationHelper.GetBoolean(((resolver == null)
                        ? MacroResolver.Resolve(element.ElementAccessCondition)
                        : resolver.ResolveMacros(element.ElementAccessCondition)), false);

                    // Log the operation
                    SecurityDebug.LogSecurityOperation(user.UserName, "Access condition:<br />" + element.ElementAccessCondition, resourceName, element.ElementName, result, SiteContext.CurrentSiteName);

                    if (!result)
                    {
                        return new ElementAccessCheckResult(ElementAccessCheckStatus.AccessConditionFailed, element);
                    }
                }

                if (automaticReadCheck && element.ElementCheckModuleReadPermission)
                {
                    ResourceInfo ri = ResourceInfoProvider.GetResourceInfo(element.ElementResourceID);

                    // Check 'read' or 'readglobal' permission of element's module (if read exists)
                    const string read = "Read";
                    const string globalRead = "GlobalRead";
                    bool hasRead = false;
                    bool hasGlobalRead = false;

                    foreach (String permission in ri.PermissionNames)
                    {
                        if (permission.Equals(read, StringComparison.OrdinalIgnoreCase))
                        {
                            hasRead = true;
                        }

                        if (permission.Equals(globalRead, StringComparison.OrdinalIgnoreCase))
                        {
                            hasGlobalRead = true;
                        }
                    }

                    if (hasRead || hasGlobalRead)
                    {
                        // Check whether user has read or global read. He has to have at least on of these permissions to pass this check.
                        if (!((hasRead && user.IsAuthorizedPerResource(ri.ResourceName, read))
                            || (hasGlobalRead && user.IsAuthorizedPerResource(ri.ResourceName, globalRead))))
                        {
                            return new ElementAccessCheckResult(ElementAccessCheckStatus.ReadPermissionFailed, element);
                        }
                    }
                }

                // UI permissions
                if (!user.IsAuthorizedPerUIElement(element.ElementResourceID, element.ElementName))
                {
                    return new ElementAccessCheckResult(ElementAccessCheckStatus.UIElementAccessFailed, element);
                }
            }

            return new ElementAccessCheckResult(ElementAccessCheckStatus.NoRestrictions, element);
        }


        /// <summary>
        /// Encapsulates all necessary checks 
        /// </summary>
        /// <param name="element">UI element info</param>
        /// <param name="isGlobalApp">Is global application flag</param>
        /// <param name="isGlobalAdmin">Is global administrator flag</param>
        private static bool CheckElementRestrictions(UIElementInfo element, bool isGlobalApp, bool isGlobalAdmin)
        {
            // Checks license, visibility macro condition and resource availability
            if (!UIContextHelper.CheckElementAvailabilityInUI(element))
            {
                return false;
            }

            // ResourceOnSite
            if (!ResourceOnSiteCondition(element, isGlobalApp, isGlobalAdmin))
            {
                return false;
            }

            // Element content permission or module permission + UI personalization
            if (!PermissionConditions(element, isGlobalApp, isGlobalAdmin))
            {
                return false;
            }

            return true;
        }

 
        /// <summary>
        /// Indicates whether current element can be accessed only by global administrator
        /// </summary>
        /// <param name="elem">UI element</param>
        /// <param name="rootElementId">Root element id used for root element(displayed in dialog)</param>
        public static bool IsAccessibleOnlyByGlobalAdministrator(UIElementInfo elem, int rootElementId = 0)
        {
            if (elem != null)
            {
                // Get parents from ID path
                var elements = elem.GetParentElements();

                // Loop through parents to root and return true if one of the parents is global application or required root element
                do
                {
                    if (elem.ElementRequiresGlobalAdminPriviligeLevel)
                    {
                        return true;
                    }

                    // Do not check elements over custom root element
                    if (elem.ElementID == rootElementId)
                    {
                        break;
                    }

                    elem = elements[elem.ElementParentID] as UIElementInfo;
                } while (elem != null);
            }

            return false;
        }
    }
}