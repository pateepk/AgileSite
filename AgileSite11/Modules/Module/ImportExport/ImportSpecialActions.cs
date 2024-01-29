using System;
using System.Data;

using CMS.CMSImportExport;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Base;
using CMS.SiteProvider;
using CMS.Core;

namespace CMS.Modules
{
    /// <summary>
    /// Handles special actions during the import/export process.
    /// </summary>
    internal static class ImportSpecialActions
    {
        #region "Methods"

        /// <summary>
        /// Initializes export handlers
        /// </summary>
        public static void Init()
        {
            ImportExportEvents.GetImportData.After += GetImportData_After;
            
            ImportExportEvents.ImportChild.After += ImportChild_After;
            ImportExportEvents.ImportObject.After += ImportObject_After;

            SpecialActionsEvents.ObjectProcessed.Execute += ObjectProcessed_Execute;
            SpecialActionsEvents.HandleExistingObject.Before += HandleExistingObject_Before;
            SpecialActionsEvents.ImportLoadDefaultSelection.Execute += LoadDefaultSelection_Execute;
            SpecialActionsEvents.ImportEnsureAutomaticSelection.Execute += ImportEnsureAutomaticSelection_Execute;
            SpecialActionsEvents.ProcessMainObject.Before += SetDefaultColumnValueForPrivilegeLevelFromOlderVersion;
        }


        private static void SetDefaultColumnValueForPrivilegeLevelFromOlderVersion(object sender, ImportEventArgs e)
        {
            var settings = e.Settings;
            var infoObj = e.Object;
            var objectType = infoObj.TypeInfo.ObjectType;

            using (new ImportSpecialCaseContext(settings))
            {
                if (objectType == UIElementInfo.OBJECT_TYPE && settings.IsOlderVersion)
                {
                    var value = ValidationHelper.GetBoolean(infoObj.GetValue(nameof(UIElementInfo.ElementIsGlobalApplication)), false);

                    infoObj.SetValue(nameof(UIElementInfo.ElementRequiresGlobalAdminPriviligeLevel), value);
                }
            }
        }


        private static void ImportObject_After(object sender, ImportEventArgs e)
        {
            // Only assign sites when importing new site or resource
            if (e.Parameters.ExistingObject != null)
            {
                return;
            }

            var siteInfo = e.Object as SiteInfo;
            if (siteInfo != null)
            {
                // Assign all modules to site if modules not available in the package, if available, assigns only imported modules
                if (!e.Settings.IsIncluded(PredefinedObjectType.RESOURCE))
                {
                    var resourceIds = ResourceInfoProvider.GetResources().Column("ResourceID").GetListResult<int>();
                    var siteId = siteInfo.SiteID;

                    // Create bindings to site for all resources
                    foreach (var resourceId in resourceIds)
                    {
                        ResourceSiteInfoProvider.AddResourceToSite(resourceId, siteId);
                    }
                }

                return;
            }

            var resourceInfo = e.Object as ResourceInfo;
            if (resourceInfo != null)
            {
                // Assign modules to all sites if sites not available in the package, if site available, assigns imported modules only to imported site
                if (!e.Settings.IsIncluded(PredefinedObjectType.SITE))
                {
                    var siteIds = SiteInfoProvider.GetSites().Column("SiteID").GetListResult<int>();
                    var resourceId = resourceInfo.ResourceID;

                    // Create bindings to site for all resources
                    foreach (var siteId in siteIds)
                    {
                        ResourceSiteInfoProvider.AddResourceToSite(resourceId, siteId);
                    }

                }
            }
        }


        private static void ImportEnsureAutomaticSelection_Execute(object sender, ImportLoadSelectionArgs e)
        {
            if (e.ObjectType == DataClassInfo.OBJECT_TYPE)
            {
                e.DependencyIDColumn = "ClassResourceID";
                e.DependencyObjectType = ResourceInfo.OBJECT_TYPE;
                e.DependencyObject = ModuleManager.GetReadOnlyObject(ResourceInfo.OBJECT_TYPE);
            }
        }


        private static void GetImportData_After(object sender, ImportGetDataEventArgs e)
        {
            if (e.Settings.Version.StartsWith("8", StringComparison.Ordinal) && (e.ObjectType == ResourceInfo.OBJECT_TYPE))
            {
                // Disable customization flag (version 9 handles the flag during export)
                DataTable uiElements = e.Data.Tables["cms_uielement"];
                DataHelper.ChangeBooleanValues(uiElements, "ElementIsCustom", false, null);
            }
        }

        
        private static void ImportChild_After(object sender, ImportEventArgs e)
        {
            QueryInfo query = e.Object as QueryInfo;
            if (query == null)
            {
                return;
            }

            DataClassInfo dci = e.ParentObject as DataClassInfo;

            // Keep queries of system tables, document types and custom tables custom
            if ((dci != null) && !dci.ClassShowAsSystemTable && !dci.ClassIsDocumentType && !dci.ClassIsCustomTable)
            {
                // Keep queries of "Custom" module custom
                ResourceInfo resource = ResourceInfoProvider.GetResourceInfo(dci.ClassResourceID);
                if ((resource != null) && (resource.ResourceName != ModuleName.CUSTOMSYSTEM))
                {
                    // Update all other queries not to be custom
                    query.QueryIsCustom = false;
                    query.Update();
                }
            }
        }


        private static void LoadDefaultSelection_Execute(object sender, ImportLoadSelectionArgs e)
        {
            var settings = e.Settings;
            var objectType = e.ObjectType;
            var siteObject = e.SiteObject;

            switch (objectType)
            {
                // Do not preselect for older version
                case ResourceInfo.OBJECT_TYPE:
                    DefaultSelectionParameters parameters = new DefaultSelectionParameters()
                    {
                        ObjectType = objectType,
                        SiteObjects = siteObject,
                        ClearProgressLog = false
                    };

                    if (!settings.IsOlderHotfixVersion || (settings.ImportType == ImportTypeEnum.None) || (settings.ImportType == ImportTypeEnum.AllForced))
                    {
                        parameters.ImportType = ImportTypeEnum.Default;
                    }
                    else
                    {
                        parameters.ImportType = ImportTypeEnum.New;
                    }
                    settings.LoadDefaultSelection(parameters);

                    // Cancel default selection
                    e.Select = false;
                    break;
            }
        }


        private static void HandleExistingObject_Before(object sender, ImportEventArgs e)
        {
            var settings = e.Settings;
            var infoObj = e.Object;

            // Special handling for existing UI elements
            if ((infoObj.TypeInfo.ObjectType == UIElementInfo.OBJECT_TYPE) && settings.IsOlderVersion)
            {
                e.Parameters.SkipObjectUpdate = true;
            }
        }


        private static void ObjectProcessed_Execute(object sender, ImportEventArgs e)
        {
            var settings = e.Settings;
            var infoObj = e.Object;

            // ### Special case - new module is imported
            EnsureModuleInstallation(settings, infoObj);
        }


        private static void EnsureModuleInstallation(SiteImportSettings settings, BaseInfo infoObj)
        {
            if (!settings.IsInstallableModule || !infoObj.TypeInfo.ObjectType.Equals(ResourceInfo.OBJECT_TYPE, StringComparison.OrdinalIgnoreCase))
            {
                // No module is being currently imported
                return;
            }

            ResourceInfo resource = infoObj as ResourceInfo;
            if ((resource != null) && resource.ResourceName.Equals(settings.ModuleName, StringComparison.OrdinalIgnoreCase))
            {
                // Assign currently installed module to all sites
                foreach (var site in SiteInfoProvider.GetSites())
                {
                    if (!ResourceSiteInfoProvider.IsResourceOnSite(resource.ResourceName, site.SiteName))
                    {
                        ResourceSiteInfoProvider.AddResourceToSite(resource.ResourceID, site.SiteID);
                    }
                }
            }
        }
        
        #endregion
    }
}