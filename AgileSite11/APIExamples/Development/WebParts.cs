using System;

using CMS.PortalEngine;
using CMS.SiteProvider;

namespace APIExamples
{
    /// <summary>
    /// Holds web part API examples.
    /// </summary>
    /// <pageTitle>Web parts</pageTitle>
    internal class WebPartsMain
    {
        /// <summary>
        /// Holds web part category API examples.
        /// </summary>
        /// <groupHeading>Web part categories</groupHeading>
        private class WebPartCategories
        {
            /// <heading>Creating a web part category</heading>
            private void CreateWebPartCategory()
            {
                // Gets the root category of the web part tree
                WebPartCategoryInfo root = WebPartCategoryInfoProvider.GetWebPartCategoryInfoByCodeName("/");

                if (root != null)
                {
                    // Creates a new web part category object
                    WebPartCategoryInfo newCategory = new WebPartCategoryInfo();

                    // Sets web part category properties
                    newCategory.CategoryDisplayName = "New category";
                    newCategory.CategoryName = "NewCategory";
                    newCategory.CategoryParentID = root.CategoryID;

                    // Saves the web part category to the database
                    WebPartCategoryInfoProvider.SetWebPartCategoryInfo(newCategory);
                }
            }


            /// <heading>Updating a web part category</heading>
            private void GetAndUpdateWebPartCategory()
            {
                // Gets the web part category
                WebPartCategoryInfo updateCategory = WebPartCategoryInfoProvider.GetWebPartCategoryInfoByCodeName("NewCategory");                
                if (updateCategory != null)
                {
                    // Updates the web part category properties
                    updateCategory.CategoryDisplayName = updateCategory.CategoryDisplayName.ToLower();

                    // Saves the changes to the database
                    WebPartCategoryInfoProvider.SetWebPartCategoryInfo(updateCategory);
                }
            }


            /// <heading>Updating multiple web part categories</heading>
            private void GetAndBulkUpdateWebPartCategories()
            {
                // Gets all Web part categories whose display name starts with 'New category'
                var categories = WebPartCategoryInfoProvider.GetCategories().WhereStartsWith("CategoryDisplayName", "New category");
                
                // Loops through the web part category objects
                foreach (WebPartCategoryInfo modifyCategory in categories)
                {
                    // Updates the properties
                    modifyCategory.CategoryDisplayName = modifyCategory.CategoryDisplayName.ToUpper();

                    // Saves the changes to the database
                    WebPartCategoryInfoProvider.SetWebPartCategoryInfo(modifyCategory);
                }
            }


            /// <heading>Deleting a web part category</heading>
            private void DeleteWebPartCategory()
            {
                // Gets the web part category
                WebPartCategoryInfo deleteCategory = WebPartCategoryInfoProvider.GetWebPartCategoryInfoByCodeName("NewCategory");

                // Deletes the web part category
                WebPartCategoryInfoProvider.DeleteCategoryInfo(deleteCategory);
            }
        }        
        
        /// <summary>
        /// Holds web part API examples.
        /// </summary>
        /// <groupHeading>Web parts</groupHeading>
        private class WebParts
        {            
            /// <heading>Creating a new web part</heading>
            private void CreateWebPart()
            {
                // Gets a parent category for the new web part
                WebPartCategoryInfo category = WebPartCategoryInfoProvider.GetWebPartCategoryInfoByCodeName("NewCategory");

                if (category != null)
                {
                    // Creates a new web part object
                    WebPartInfo newWebpart = new WebPartInfo();

                    // Sets the web part properties
                    newWebpart.WebPartDisplayName = "New web part";
                    newWebpart.WebPartName = "NewWebpart";
                    newWebpart.WebPartDescription = "This is a new web part.";
                    newWebpart.WebPartFileName = "FileName";
                    newWebpart.WebPartProperties = "<form></form>";
                    newWebpart.WebPartCategoryID = category.CategoryID;

                    // Saves the web part to the database
                    WebPartInfoProvider.SetWebPartInfo(newWebpart);                    
                }                
            }


            /// <heading>Updating a web part</heading>              
            private void GetAndUpdateWebPart()
            {
                // Gets the web part
                WebPartInfo updateWebpart = WebPartInfoProvider.GetWebPartInfo("NewWebpart");
                if (updateWebpart != null)
                {
                    // Updates the web part properties
                    updateWebpart.WebPartDisplayName = updateWebpart.WebPartDisplayName.ToLower();

                    // Saves the changes to the database
                    WebPartInfoProvider.SetWebPartInfo(updateWebpart);                    
                }
            }


            /// <heading>Updating multiple web parts</heading>
            private void GetAndBulkUpdateWebParts()
            {
                // Gets all web parts whose name starts with 'NewWebpart'
                var webparts = WebPartInfoProvider.GetWebParts().WhereStartsWith("WebPartName", "NewWebpart");
                
                // Loop through individual web parts
                foreach (WebPartInfo webPart in webparts)
                {                    
                    // Updates the web part properties
                    webPart.WebPartDisplayName = webPart.WebPartDisplayName.ToUpper();

                    // Saves the changes to the database
                    WebPartInfoProvider.SetWebPartInfo(webPart);
                }
            }


            /// <heading>Deleting a web part</heading>
            private void DeleteWebPart()
            {
                // Gets the web part
                WebPartInfo deleteWebpart = WebPartInfoProvider.GetWebPartInfo("NewWebpart");

                // Deletes the web part
                WebPartInfoProvider.DeleteWebPartInfo(deleteWebpart);
            }
        }


        /// <summary>
        /// Holds web part layout API examples.
        /// </summary>
        /// <groupHeading>Web part layouts</groupHeading>
        private class WebPartLayouts
        {
            /// <heading>Creating a new layout for a web part</heading>
            private void CreateWebPartLayout()
            {
                // Gets the web part
                WebPartInfo webpart = WebPartInfoProvider.GetWebPartInfo("NewWebpart");
                if (webpart != null)
                {
                    // Creates a new web part layout object
                    WebPartLayoutInfo newLayout = new WebPartLayoutInfo();

                    // Sets the web part layout properties
                    newLayout.WebPartLayoutDisplayName = "New layout";
                    newLayout.WebPartLayoutCodeName = "NewLayout";
                    newLayout.WebPartLayoutWebPartID = webpart.WebPartID;
                    newLayout.WebPartLayoutCode = "This is the new layout of the NewWebpart web part.";

                    // Saves the web part layout to the database
                    WebPartLayoutInfoProvider.SetWebPartLayoutInfo(newLayout);
                }                
            }


            /// <heading>Updating a web part layout</heading>
            private void GetAndUpdateWebPartLayout()
            {
                // Gets the web part layout
                WebPartLayoutInfo updateLayout = WebPartLayoutInfoProvider.GetWebPartLayoutInfo("NewWebpart", "NewLayout");
                if (updateLayout != null)
                {
                    // Updates the web part layout properties
                    updateLayout.WebPartLayoutDisplayName = updateLayout.WebPartLayoutDisplayName.ToLower();

                    // Saves the changes to the database
                    WebPartLayoutInfoProvider.SetWebPartLayoutInfo(updateLayout);
                }
            }


            /// <heading>Updating multiple web part layouts</heading>
            private void GetAndBulkUpdateWebPartLayouts()
            {                
                // Gets all web part layouts whose code name starts with 'NewLayout'
                var layouts = WebPartLayoutInfoProvider.GetWebPartLayouts().WhereStartsWith("WebPartLayoutCodeName", "NewLayout");
                
                // Loops through individual web part layouts
                foreach (WebPartLayoutInfo layout in layouts)
                {                    
                    // Updates the web part layout properties
                    layout.WebPartLayoutDisplayName = layout.WebPartLayoutDisplayName.ToUpper();

                    // Saves the changes to the database
                    WebPartLayoutInfoProvider.SetWebPartLayoutInfo(layout);
                }
            }


            /// <heading>Deleting a web part layout</heading>
            private void DeleteWebPartLayout()
            {
                // Gets the web part layout
                WebPartLayoutInfo deleteLayout = WebPartLayoutInfoProvider.GetWebPartLayoutInfo("NewWebpart", "NewLayout");

                // Deletes the web part layout
                WebPartLayoutInfoProvider.DeleteWebPartLayoutInfo(deleteLayout);
            }
        }

        /// <summary>
        /// Holds web part container API examples.
        /// </summary>
        /// <groupHeading>Web part containers</groupHeading>
        private class WebPartContainers
        {
            /// <heading>Creating a new web part container</heading>
            private void CreateWebPartContainer()
            {
                // Creates a new web part container object
                WebPartContainerInfo newContainer = new WebPartContainerInfo();

                // Sets the container's properties
                newContainer.ContainerDisplayName = "New container";
                newContainer.ContainerName = "NewContainer";
                newContainer.ContainerTextBefore = "<div class=\"NewContainer\">";
                newContainer.ContainerTextAfter = "</div>";

                // Saves the web part container to the database
                WebPartContainerInfoProvider.SetWebPartContainerInfo(newContainer);
            }


            /// <heading>Updating a web part container</heading>
            private void GetAndUpdateWebPartContainer()
            {
                // Gets the web part container
                WebPartContainerInfo updateContainer = WebPartContainerInfoProvider.GetWebPartContainerInfo("NewContainer");
                if (updateContainer != null)
                {
                    // Updates the container's properties
                    updateContainer.ContainerDisplayName = updateContainer.ContainerDisplayName.ToLower();

                    // Saves the changes to the database
                    WebPartContainerInfoProvider.SetWebPartContainerInfo(updateContainer);
                }
            }


            /// <heading>Updating multiple web part containers</heading>
            private void GetAndBulkUpdateWebPartContainers()
            {
                // Get all web part containers whose name starts with 'NewContainer'
                var containers = WebPartContainerInfoProvider.GetContainers().WhereStartsWith("ContainerName", "NewContainer");
                
                // Loops through individual web part containers
                foreach (WebPartContainerInfo container in containers)
                {                    
                    // Updates the container properties
                    container.ContainerDisplayName = container.ContainerDisplayName.ToUpper();

                    // Saves the changes to the database
                    WebPartContainerInfoProvider.SetWebPartContainerInfo(container);
                }
            }


            /// <heading>Deleting a web part container</heading>
            private void DeleteWebPartContainer()
            {
                // Gets the web part container
                WebPartContainerInfo deleteContainer = WebPartContainerInfoProvider.GetWebPartContainerInfo("NewContainer");

                // Deletes the web part container
                WebPartContainerInfoProvider.DeleteWebPartContainerInfo(deleteContainer);
            }

            /// <heading>Assigning a web part container to a site</heading>
            private void AddWebPartContainerToSite()
            {
                // Gets the web part container
                WebPartContainerInfo container = WebPartContainerInfoProvider.GetWebPartContainerInfo("NewContainer");
                if (container != null)
                {
                    int containerId = container.ContainerID;
                    int siteId = SiteContext.CurrentSiteID;

                    // Saves the site binding to the database
                    WebPartContainerSiteInfoProvider.AddContainerToSite(containerId, siteId);
                }
            }


            /// <heading>Removing a web part container from a site</heading>
            private void RemoveWebPartContainerFromSite()
            {
                // Gets the web part container
                WebPartContainerInfo removeContainer = WebPartContainerInfoProvider.GetWebPartContainerInfo("NewContainer");
                if (removeContainer != null)
                {
                    int siteId = SiteContext.CurrentSiteID;

                    // Deletes the site binding from the database
                    WebPartContainerSiteInfoProvider.RemoveContainerFromSite(removeContainer.ContainerID, siteId);
                }
            }
        }
    }
}