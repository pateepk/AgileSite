using System;

using CMS.PortalEngine;
using CMS.SiteProvider;

namespace APIExamples
{
    /// <summary>
    /// Holds page template API examples.
    /// </summary>
    /// <pageTitle>Page templates</pageTitle>
    internal class PageTemplatesMain
    {
        /// <summary>
        /// Holds page template category API examples.
        /// </summary>
        /// <groupHeading>Page template categories</groupHeading>
        private class PageTemplateCategories
        {
            /// <heading>Creating a page template category</heading>
            private void CreatePageTemplateCategory()
            {
                // Creates a new page template category object
                PageTemplateCategoryInfo newCategory = new PageTemplateCategoryInfo();

                // Sets the category properties
                newCategory.DisplayName = "New category";
                newCategory.CategoryName = "NewCategory";                

                // Saves the page template category to the database
                PageTemplateCategoryInfoProvider.SetPageTemplateCategoryInfo(newCategory);
            }


            /// <heading>Updating a page template category</heading>
            private void GetAndUpdatePageTemplateCategory()
            {
                // Gets the page template category
                PageTemplateCategoryInfo updateCategory = PageTemplateCategoryInfoProvider.GetPageTemplateCategoryInfo("NewCategory");
                if (updateCategory != null)
                {
                    // Updates the category properties
                    updateCategory.DisplayName = updateCategory.DisplayName.ToLower();

                    // Saves the changes to the database
                    PageTemplateCategoryInfoProvider.SetPageTemplateCategoryInfo(updateCategory);
                }
            }


            /// <heading>Updating multiple page template categories</heading>
            private void GetAndBulkUpdatePageTemplateCategories()
            {                
                // Gets all page template categories from the first level of the tree
                var categories = PageTemplateCategoryInfoProvider.GetPageTemplateCategories().WhereEquals("CategoryLevel", 1);

                // Loops through the page template categories
                foreach (PageTemplateCategoryInfo category in categories)
                {
                    // Updates the category properties
                    category.DisplayName = category.DisplayName.ToUpper();

                    // Saves the changes to the database
                    PageTemplateCategoryInfoProvider.SetPageTemplateCategoryInfo(category);
                }                
            }


            /// <heading>Deleting a page template category</heading>
            private void DeletePageTemplateCategory()
            {
                // Gets the page template category
                PageTemplateCategoryInfo deleteCategory = PageTemplateCategoryInfoProvider.GetPageTemplateCategoryInfo("NewCategory");

                if (deleteCategory != null)
                {
                    // Deletes the page template category
                    PageTemplateCategoryInfoProvider.DeletePageTemplateCategory(deleteCategory);
                }
            }
        }


        /// <summary>
        /// Holds page template API examples.
        /// </summary>
        /// <groupHeading>Page templates</groupHeading>
        private class PageTemplates
        {
            /// <heading>Creating a page template</heading>
            private void CreatePageTemplate()
            {
                // Gets a parent category for the new page template
                PageTemplateCategoryInfo category = PageTemplateCategoryInfoProvider.GetPageTemplateCategoryInfo("NewCategory");
                if (category != null)
                {
                    // Creates a new page template object
                    PageTemplateInfo newTemplate = new PageTemplateInfo();

                    // Sets the page template properties
                    newTemplate.DisplayName = "New template";
                    newTemplate.CodeName = "NewTemplate";
                    newTemplate.Description = "This is a page template description.";
                    newTemplate.CategoryID = category.CategoryId;
                    newTemplate.PageTemplateSiteID = SiteContext.CurrentSiteID;                                        
                    newTemplate.PageTemplateType = PageTemplateTypeEnum.Portal;
                    newTemplate.ShowAsMasterTemplate = false;
                    newTemplate.IsReusable = true;
                    newTemplate.InheritPageLevels = ""; // Configures page nesting for all ancestor pages

                    // Saves the page template to the database
                    PageTemplateInfoProvider.SetPageTemplateInfo(newTemplate);
                }
            }


            /// <heading>Updating a page template</heading>
            private void GetAndUpdatePageTemplate()
            {
                // Gets the page template
                PageTemplateInfo updateTemplate = PageTemplateInfoProvider.GetPageTemplateInfo("NewTemplate");
                if (updateTemplate != null)
                {
                    // Updates the page template properties
                    updateTemplate.DisplayName = updateTemplate.DisplayName.ToLower();

                    // Saves the changes
                    PageTemplateInfoProvider.SetPageTemplateInfo(updateTemplate);
                }
            }


            /// <heading>Updating multiple page templates</heading>
            private void GetAndBulkUpdatePageTemplates()
            {                
                // Gets all page templates assigned to the current site whose code name starts with 'NewTemplate'
                var templates = PageTemplateInfoProvider.GetTemplates()
                                                            .WhereStartsWith("PageTemplateCodeName", "NewTemplate")
                                                            .OnSite(SiteContext.CurrentSiteID);
                
                // Loops through individual page templates
                foreach (PageTemplateInfo template in templates)
                {                    
                    // Updates the template properties
                    template.DisplayName = template.DisplayName.ToUpper();

                    // Saves the changes to the database
                    PageTemplateInfoProvider.SetPageTemplateInfo(template);
                }
            }


            /// <heading>Assigning a page template to a site</heading>
            private void AddPageTemplateToSite()
            {
                // Gets the page template
                PageTemplateInfo template = PageTemplateInfoProvider.GetPageTemplateInfo("NewTemplate");
                if (template != null)
                {
                    // Assigns the template to the current site
                    PageTemplateSiteInfoProvider.AddPageTemplateToSite(template.PageTemplateId, SiteContext.CurrentSiteID);
                }
            }


            /// <heading>Removing a page template from a site</heading>
            private void RemovePageTemplateFromSite()
            {
                // Gets the page template
                PageTemplateInfo removeTemplate = PageTemplateInfoProvider.GetPageTemplateInfo("NewTemplate");
                if (removeTemplate != null)
                {
                    // Gets the binding object representing the relationship between the template and the current site
                    PageTemplateSiteInfo templateSite = PageTemplateSiteInfoProvider.GetPageTemplateSiteInfo(removeTemplate.PageTemplateId, SiteContext.CurrentSiteID);

                    if (templateSite != null)
                    {
                        // Removes the template from the current site
                        PageTemplateSiteInfoProvider.DeletePageTemplateSiteInfo(templateSite);
                    }
                }
            }


            /// <heading>Deleting a page template</heading>
            private void DeletePageTemplate()
            {
                // Gets the page template
                PageTemplateInfo deleteTemplate = PageTemplateInfoProvider.GetPageTemplateInfo("NewTemplate");

                if (deleteTemplate != null)
                {
                    // Deletes the page template
                    PageTemplateInfoProvider.DeletePageTemplate(deleteTemplate);
                }
            }
        }


        /// <summary>
        /// Holds page template scope API examples.
        /// </summary>
        /// <groupHeading>Page template scopes</groupHeading>
        private class PageTemplateScopes
        {
            /// <heading>Defining a scope for a page template</heading>
            private void CreatePageTemplateScope()
            {
                // Gets a page template
                PageTemplateInfo template = PageTemplateInfoProvider.GetPageTemplateInfo("NewTemplate");

                // Checks whether the page template exists
                if (template != null)
                {
                    // Configures the page template to be usable only within certain scopes (not all pages)
                    template.PageTemplateForAllPages = false;

                    // Creates a new template scope object
                    PageTemplateScopeInfo newScope = new PageTemplateScopeInfo();

                    // Assigns the scope to the template and sets the scope properties
                    newScope.PageTemplateScopeTemplateID = template.PageTemplateId;
                    newScope.PageTemplateScopeSiteID = SiteContext.CurrentSiteID;
                    newScope.PageTemplateScopePath = "/";
                    newScope.PageTemplateScopeLevels = "/{0}/{1}";

                    // Saves the template scope to the database
                    PageTemplateScopeInfoProvider.SetPageTemplateScopeInfo(newScope);

                    // Saves the page template changes to the database
                    PageTemplateInfoProvider.SetPageTemplateInfo(template);
                }
            }


            /// <heading>Deleting page template scopes</heading>
            private void DeletePageTemplateScope()
            {                
                // Gets a page template
                PageTemplateInfo template = PageTemplateInfoProvider.GetPageTemplateInfo("NewTemplate");

                // Checks whether the page template exists
                if (template != null)
                {
                    // Gets all scopes used by the given page template
                    var scopes = PageTemplateScopeInfoProvider.GetTemplateScopes().WhereEquals("PageTemplateScopeTemplateID", template.PageTemplateId);

                    foreach (PageTemplateScopeInfo deleteScope in scopes)
                    {
                        // Deletes the page template scope
                        PageTemplateScopeInfoProvider.DeletePageTemplateScopeInfo(deleteScope);
                    }

                    // Configures the page template to be usable on all pages
                    template.PageTemplateForAllPages = true;

                    // Saves the page template changes to the database
                    PageTemplateInfoProvider.SetPageTemplateInfo(template);
                }
            }
        }
    }
}
