using System;

using CMS.Modules;
using CMS.SiteProvider;
using CMS.Membership;

namespace APIExamples
{
    /// <summary>
    /// Holds module-related API examples.
    /// </summary>
    /// <pageTitle>Modules</pageTitle>
    internal class ModulesMain
    {
        /// <summary>
        /// Holds module API examples.
        /// </summary>
        /// <groupHeading>Modules</groupHeading>
        private class Modules
        {
            /// <heading>Creating a new module</heading>
            private void CreateModule()
            {
                // Creates a new module object
                ResourceInfo newModule = new ResourceInfo();

                // Sets the module properties
                newModule.ResourceDisplayName = "Custom module";
                newModule.ResourceName = "Custom.Module";
                
                // Ensures that the module is in development mode (not sealed)
                newModule.ResourceIsInDevelopment = true;

                // Saves the module to the database
                ResourceInfoProvider.SetResourceInfo(newModule);
            }


            /// <heading>Updating a module</heading>
            private void GetAndUpdateModule()
            {
                // Gets the module
                ResourceInfo updateModule = ResourceInfoProvider.GetResourceInfo("Custom.Module");
                if (updateModule != null)
                {
                    // Updates the module properties
                    updateModule.ResourceDisplayName = updateModule.ResourceDisplayName.ToLower();

                    // Saves the changes to the database
                    ResourceInfoProvider.SetResourceInfo(updateModule);
                }
            }


            /// <heading>Updating multiple modules</heading>
            private void GetAndBulkUpdateModules()
            {
                // Get all modules assigned to the current site whose code name has the 'Custom.' prefix
                var modules = ResourceInfoProvider.GetResources(SiteContext.CurrentSiteID)
                                                    .WhereStartsWith("ResourceName", "Custom.");
                
                // Loops through individual modules
                foreach (ResourceInfo module in modules)
                {
                    // Updates the module properties
                    module.ResourceDisplayName = module.ResourceDisplayName.ToUpper();

                    // Saves the changes to the database
                    ResourceInfoProvider.SetResourceInfo(module);
                }
            }


            /// <heading>Assigning a module to a site</heading>
            private void AddModuleToSite()
            {
                // Gets the module
                ResourceInfo module = ResourceInfoProvider.GetResourceInfo("Custom.Module");
                if (module != null)
                {
                    // Assigns the module to the current site
                    ResourceSiteInfoProvider.AddResourceToSite(module.ResourceID, SiteContext.CurrentSiteID);
                }
            }


            /// <heading>Removing a module from a site</heading>
            private void RemoveModuleFromSite()
            {
                // Gets the module
                ResourceInfo module = ResourceInfoProvider.GetResourceInfo("Custom.Module");
                if (module != null)
                {
                    // Gets the binding object representing the relationship between the module and the current site
                    ResourceSiteInfo moduleSite = ResourceSiteInfoProvider.GetResourceSiteInfo(module.ResourceID, SiteContext.CurrentSiteID);

                    if (moduleSite != null)
                    {
                        // Removes the module from the current site
                        ResourceSiteInfoProvider.DeleteResourceSiteInfo(moduleSite);
                    }
                }
            }


            /// <heading>Deleting a module</heading>
            private void DeleteModule()
            {
                // Gets the module
                ResourceInfo deleteModule = ResourceInfoProvider.GetResourceInfo("Custom.Module");

                // Deletes the module
                ResourceInfoProvider.DeleteResourceInfo(deleteModule);
            }
        }


        /// <summary>
        /// Holds module permission API examples.
        /// </summary>
        /// <groupHeading>Module permissions</groupHeading>
        private class ModulePermissions
        {
            /// <heading>Defining a new permission for a module</heading>
            private void CreatePermission()
            {
                // Gets the module
                ResourceInfo module = ResourceInfoProvider.GetResourceInfo("Custom.Module");
                
                if (module != null)
                {
                    // Creates a new permission object
                    PermissionNameInfo newPermission = new PermissionNameInfo();

                    // Sets the permission properties
                    newPermission.PermissionDisplayName = "Read";
                    newPermission.PermissionName = "Read";
                    newPermission.ResourceId = module.ResourceID;
                    newPermission.PermissionDisplayInMatrix = true;

                    // Saves the permission to the database
                    PermissionNameInfoProvider.SetPermissionInfo(newPermission);
                }
            }


            /// <heading>Updating a module permission</heading>
            private void GetAndUpdatePermission()
            {
                // Gets the module permission
                PermissionNameInfo updatePermission = PermissionNameInfoProvider.GetPermissionNameInfo("Read", "Custom.Module", null);
                if (updatePermission != null)
                {
                    // Updates the permission properties
                    updatePermission.PermissionDisplayName = updatePermission.PermissionDisplayName.ToLower();

                    // Saves the changes to the database
                    PermissionNameInfoProvider.SetPermissionInfo(updatePermission);
                }
            }


            /// <heading>Updating multiple module permissions</heading>
            private void GetAndBulkUpdatePermissions()
            {
                // Gets a module
                ResourceInfo module = ResourceInfoProvider.GetResourceInfo("Custom.Module");

                if (module != null)
                {
                    // Gets all permissions of the specified module
                    var permissions = PermissionNameInfoProvider.GetPermissionNames().WhereEquals("ResourceId", module.ResourceID);

                    // Loops through the module's permissions
                    foreach (PermissionNameInfo permission in permissions)
                    {
                        // Updates the permission properties
                        permission.PermissionDisplayName = permission.PermissionDisplayName.ToUpper();

                        // Saves the changes to the database
                        PermissionNameInfoProvider.SetPermissionInfo(permission);
                    }
                }
            }


            /// <heading>Assigning a module permission to a role</heading>
            private void AddPermissionToRole()
            {
                // Gets the module permission
                PermissionNameInfo permission = PermissionNameInfoProvider.GetPermissionNameInfo("Read", "Custom.Module", null);

                // Gets the role
                RoleInfo role = RoleInfoProvider.GetRoleInfo("Admin", SiteContext.CurrentSiteID);

                if ((permission != null) && (role != null))
                {
                    // Creates an object representing the role-permission relationship
                    RolePermissionInfo newRolePermission = new RolePermissionInfo();

                    // Assigns the permission to the role
                    newRolePermission.PermissionID = permission.PermissionId;
                    newRolePermission.RoleID = role.RoleID;

                    // Saves the role-permission relationship into the database
                    RolePermissionInfoProvider.SetRolePermissionInfo(newRolePermission);
                }
            }


            /// <heading>Removing a module permission from a role</heading>
            private void RemovePermissionFromRole()
            {
                // Gets the module permission
                PermissionNameInfo permission = PermissionNameInfoProvider.GetPermissionNameInfo("Read", "Custom.Module", null);

                // Gets the role
                RoleInfo role = RoleInfoProvider.GetRoleInfo("Admin", SiteContext.CurrentSiteID);

                if ((permission != null) && (role != null))
                {
                    // Gets the object representing the role-permission relationship
                    RolePermissionInfo deleteRolePermission = RolePermissionInfoProvider.GetRolePermissionInfo(role.RoleID, permission.PermissionId);

                    if (deleteRolePermission != null)
                    {
                        // Removes the permission from the role
                        RolePermissionInfoProvider.DeleteRolePermissionInfo(deleteRolePermission);
                    }
                }
            }


            /// <heading>Deleting a module permission</heading>
            private void DeletePermission()
            {
                // Gets the module permission
                PermissionNameInfo deletePermission = PermissionNameInfoProvider.GetPermissionNameInfo("Read", "Custom.Module", null);

                if (deletePermission != null)
                {
                    // Deletes the permission
                    PermissionNameInfoProvider.DeletePermissionInfo(deletePermission);
                }
            }
        }


        /// <summary>
        /// Holds UI element API examples.
        /// </summary>
        /// <groupHeading>UI elements</groupHeading>
        private class ModuleUIElements
        {
            /// <heading>Creating a new UI element</heading>
            private void CreateUIElement()
            {
                // Gets the module to which the UI element will belong
                ResourceInfo module = ResourceInfoProvider.GetResourceInfo("Custom.Module");
                if (module != null)
                {
                    // Gets a parent UI element (the CMS > Administration > Custom element in this case)
                    UIElementInfo parentElement = UIElementInfoProvider.GetUIElementInfo("CMS", "Custom");
                    
                    if (parentElement != null)
                    {
                        // Creates a new UI element object
                        UIElementInfo newElement = new UIElementInfo();

                        // Set the properties
                        newElement.ElementDisplayName = "Custom element";
                        newElement.ElementName = "CustomElement";
                        newElement.ElementResourceID = module.ResourceID;
                        newElement.ElementIsCustom = true;
                        newElement.ElementParentID = parentElement.ElementID;

                        // Saves the new UI element to the database
                        UIElementInfoProvider.SetUIElementInfo(newElement);
                    }
                }
            }


            /// <heading>Updating a UI element</heading>
            private void GetAndUpdateUIElement()
            {
                // Gets the UI element
                UIElementInfo updateElement = UIElementInfoProvider.GetUIElementInfo("Custom.Module", "CustomElement");
                if (updateElement != null)
                {
                    // Updates the UI element properties
                    updateElement.ElementDisplayName = updateElement.ElementDisplayName.ToLower();

                    // Saves the changes to the database
                    UIElementInfoProvider.SetUIElementInfo(updateElement);
                }
            }


            /// <heading>Updating multiple UI elements</heading>
            private void GetAndBulkUpdateUIElements()
            {
                // Gets a module
                ResourceInfo module = ResourceInfoProvider.GetResourceInfo("Custom.Module");

                if (module != null)
                {
                    // Gets all UI elements that belong to the specified module
                    var elements = UIElementInfoProvider.GetUIElements().WhereEquals("ElementResourceID", module.ResourceID);

                    // Loops through the module's UI elements
                    foreach (UIElementInfo element in elements)
                    {
                        // Updates the UI element properties
                        element.ElementDisplayName = element.ElementDisplayName.ToUpper();

                        // Saves the changes to the database
                        UIElementInfoProvider.SetUIElementInfo(element);
                    }
                }
            }


            /// <heading>Assigning a UI element to a role (UI personalization)</heading>
            private void AddUIElementToRole()
            {
                // Gets the role
                RoleInfo role = RoleInfoProvider.GetRoleInfo("Admin", SiteContext.CurrentSiteID);

                // Gets the UI element
                UIElementInfo element = UIElementInfoProvider.GetUIElementInfo("Custom.Module", "CustomElement");

                if ((role != null) && (element != null))
                {
                    // Creates an object representing the role-UI element relationship
                    RoleUIElementInfo newRoleElement = new RoleUIElementInfo();

                    // Assigns the UI element to the role
                    newRoleElement.RoleID = role.RoleID;
                    newRoleElement.ElementID = element.ElementID;

                    // Saves the new relationship to the database
                    RoleUIElementInfoProvider.SetRoleUIElementInfo(newRoleElement);
                }
            }


            /// <heading>Removing a UI element from a role (UI personalization)</heading>
            private void RemoveUIElementFromRole()
            {
                // Gets the role
                RoleInfo role = RoleInfoProvider.GetRoleInfo("Admin", SiteContext.CurrentSiteID);

                // Gets the UI element
                UIElementInfo element = UIElementInfoProvider.GetUIElementInfo("Custom.Module", "CustomElement");

                if ((role != null) && (element != null))
                {
                    // Gets the object representing the relationship between the role and the UI element
                    RoleUIElementInfo deleteRoleElement = RoleUIElementInfoProvider.GetRoleUIElementInfo(role.RoleID, element.ElementID);

                    if (deleteRoleElement != null)
                    {
                        // Removes the UI element from the role
                        RoleUIElementInfoProvider.DeleteRoleUIElementInfo(deleteRoleElement);
                    }
                }
            }


            /// <heading>Deleting a UI element</heading>
            private void DeleteUIElement()
            {
                // Gets the UI element
                UIElementInfo deleteElement = UIElementInfoProvider.GetUIElementInfo("Custom.Module", "CustomElement");

                if (deleteElement != null)
                {
                    // Deletes the UI element
                    UIElementInfoProvider.DeleteUIElementInfo(deleteElement);
                }
            }
        }
    }
}
