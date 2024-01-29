using System;

using CMS.PortalEngine;
using CMS.FormEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.SiteProvider;
using CMS.Modules;

namespace APIExamples
{
    /// <summary>
    /// Holds widget-related API examples.
    /// </summary>
    /// <pageTitle>Widgets</pageTitle>
    internal class WidgetsMain
    {
        /// <summary>
        /// Holds widget category API examples.
        /// </summary>
        /// <groupHeading>Widget categories</groupHeading>
        private class WidgetCategories
        {
            /// <heading>Creating a new widget category</heading>
            private void CreateWidgetCategory()
            {
                // Creates a new widget category object
                WidgetCategoryInfo newCategory = new WidgetCategoryInfo();

                // Sets the widget category properties
                newCategory.WidgetCategoryDisplayName = "New category";
                newCategory.WidgetCategoryName = "NewCategory";

                // Saves the widget category to the database
                WidgetCategoryInfoProvider.SetWidgetCategoryInfo(newCategory);
            }


            /// <heading>Updating a widget category</heading>
            private void GetAndUpdateWidgetCategory()
            {
                // Gets the widget category
                WidgetCategoryInfo updateCategory = WidgetCategoryInfoProvider.GetWidgetCategoryInfo("NewCategory");
                if (updateCategory != null)
                {
                    // Updates the widget category properties
                    updateCategory.WidgetCategoryDisplayName = updateCategory.WidgetCategoryDisplayName.ToLower();

                    // Saves the changes to the database
                    WidgetCategoryInfoProvider.SetWidgetCategoryInfo(updateCategory);
                }
            }


            /// <heading>Updating multiple widget categories</heading>
            private void GetAndBulkUpdateWidgetCategories()
            {
                // Gets all widget categories whose name starts with 'NewCategory'
                var categories = WidgetCategoryInfoProvider.GetWidgetCategories().WhereStartsWith("WidgetCategoryName", "NewCategory");
                
                // Loops through individual widget categories
                foreach (WidgetCategoryInfo category in categories)
                {
                    // Updates the category properties
                    category.WidgetCategoryDisplayName = category.WidgetCategoryDisplayName.ToUpper();

                    // Saves the changes to the database
                    WidgetCategoryInfoProvider.SetWidgetCategoryInfo(category);
                }
            }


            /// <heading>Deleting a widget category</heading>
            private void DeleteWidgetCategory()
            {
                // Gets the widget category
                WidgetCategoryInfo deleteCategory = WidgetCategoryInfoProvider.GetWidgetCategoryInfo("NewCategory");

                if (deleteCategory != null)
                {
                    // Deletes the widget category
                    WidgetCategoryInfoProvider.DeleteWidgetCategoryInfo(deleteCategory);                    
                }
            }
        }


        /// <summary>
        /// Holds widget API examples.
        /// </summary>
        /// <groupHeading>Widgets</groupHeading>
        private class Widgets
        {
            /// <heading>Creating a new widget</heading>
            private void CreateWidget()
            {
                // Gets a parent web part and category for the widget
                WebPartInfo webpart = WebPartInfoProvider.GetWebPartInfo("AbuseReport");
                WidgetCategoryInfo category = WidgetCategoryInfoProvider.GetWidgetCategoryInfo("NewCategory");

                // Verifies that the parent category and web part exist, and that the web part is not inherited
                // Widgets cannot be created from inherited web parts
                if ((webpart != null) && (webpart.WebPartParentID == 0) && (category != null))
                {
                    // Creates a new widget object 
                    WidgetInfo newWidget = new WidgetInfo();

                    // Sets the widget properties
                    newWidget.WidgetName = "NewWidget";
                    newWidget.WidgetDisplayName = "New widget";
                    newWidget.WidgetDescription = webpart.WebPartDescription;

                    // Loads the property definitions from the parent web part
                    newWidget.WidgetProperties = FormHelper.GetFormFieldsWithDefaultValue(webpart.WebPartProperties, "visible", "false");

                    // Assigns the widget under the given category and web part
                    newWidget.WidgetWebPartID = webpart.WebPartID;
                    newWidget.WidgetCategoryID = category.WidgetCategoryID;

                    // Saves the new widget to the database
                    WidgetInfoProvider.SetWidgetInfo(newWidget);
                }
            }


            /// <heading>Updating a widget</heading>
            private void GetAndUpdateWidget()
            {
                // Gets the widget
                WidgetInfo updateWidget = WidgetInfoProvider.GetWidgetInfo("NewWidget");
                if (updateWidget != null)
                {
                    // Updates the widget properties
                    updateWidget.WidgetDisplayName = updateWidget.WidgetDisplayName.ToLower();

                    // Saves the changes
                    WidgetInfoProvider.SetWidgetInfo(updateWidget);
                }
            }


            /// <heading>Updating multiple widgets</heading>
            private void GetAndBulkUpdateWidgets()
            {
                // Gets all widgets whose name starts with 'NewWidget'
                var widgets = WidgetInfoProvider.GetWidgets().WhereStartsWith("WidgetName", "NewWidget");

                // Loops through individual widgets
                foreach (WidgetInfo widget in widgets)
                {
                    // Updates the widget properties
                    widget.WidgetDisplayName = widget.WidgetDisplayName.ToUpper();

                    // Saves the changes to the database
                    WidgetInfoProvider.SetWidgetInfo(widget);
                }                
            }


            /// <heading>Deleting a widget</heading>
            private void DeleteWidget()
            {
                // Gets the widget
                WidgetInfo deleteWidget = WidgetInfoProvider.GetWidgetInfo("NewWidget");

                if (deleteWidget != null)
                {
                    // Deletes the widget
                    WidgetInfoProvider.DeleteWidgetInfo(deleteWidget);
                }
            }
        }


        /// <summary>
        /// Holds widget security API examples.
        /// </summary>
        /// <groupHeading>Widget security</groupHeading>
        private class WidgetSecurity
        {
            /// <heading>Setting the security options for a widget</heading>
            private void SetSecurityLevel()
            {
                // Gets the widget
                WidgetInfo widget = WidgetInfoProvider.GetWidgetInfo("NewWidget");

                // Checks whether the widget exists
                if (widget != null)
                {
                    // Allows the widget to be used in editor widget zones
                    widget.WidgetForEditor = true;

                    // Configures the widget to be usable only by assigned roles
                    widget.AllowedFor = SecurityAccessEnum.AuthorizedRoles;

                    // Saves the widget changes to the database
                    WidgetInfoProvider.SetWidgetInfo(widget);
                }
            }


            /// <heading>Assigning a role to a widget</heading>
            private void AddWidgetToRole()
            {
                // Gets role, widget and permission objects
                RoleInfo role = RoleInfoProvider.GetRoleInfo("Admin", SiteContext.CurrentSiteID);
                WidgetInfo widget = WidgetInfoProvider.GetWidgetInfo("NewWidget");
                PermissionNameInfo permission = PermissionNameInfoProvider.GetPermissionNameInfo("AllowedFor", "Widgets", null);

                // Checks that all of the objects exist
                if ((role != null) && (widget != null) && (permission != null))
                {
                    // Assigns the role to the widget 
                    // Allows members of the role to work with the widget (if the widget is allowed only for authorized roles)
                    WidgetRoleInfoProvider.AddRoleToWidget(role.RoleID, widget.WidgetID, permission.PermissionId);
                }
            }


            /// <heading>Removing a role from a widget</heading>
            private void RemoveWidgetFromRole()
            {
                // Gets role, widget and permission objects
                RoleInfo role = RoleInfoProvider.GetRoleInfo("Admin", SiteContext.CurrentSiteID);
                WidgetInfo widget = WidgetInfoProvider.GetWidgetInfo("NewWidget");
                PermissionNameInfo permission = PermissionNameInfoProvider.GetPermissionNameInfo("AllowedFor", "Widgets", null);

                // Checks that all of the objects exist
                if ((role != null) && (widget != null) && (permission != null))
                {
                    // Removes the role from the widget
                    WidgetRoleInfoProvider.RemoveRoleFromWidget(role.RoleID, widget.WidgetID, permission.PermissionId);
                }
            }
        }
    }
}
