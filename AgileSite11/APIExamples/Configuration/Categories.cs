using CMS.DocumentEngine;
using CMS.Membership;
using CMS.SiteProvider;
using CMS.Taxonomy;

namespace APIExamples
{
    /// <summary>
    /// Holds category API examples.
    /// </summary>
    /// <pageTitle>Categories</pageTitle>
    internal class Categories
    {
        /// <heading>Creating a new page category</heading>
        private void CreateCategory()
        {
            // Creates a new category object
            CategoryInfo category = new CategoryInfo();

            // Sets the category properties
            category.CategoryDisplayName = "Carnivores";
            category.CategoryName = "Carnivores";
            category.CategoryDescription = "Animals that feed on other animals";
            category.CategorySiteID = SiteContext.CurrentSiteID;
            category.CategoryEnabled = true;

            // Saves the category to the database
            CategoryInfoProvider.SetCategoryInfo(category);
        }


        /// <heading>Updating a page category</heading>
        private void GetAndUpdateCategory()
        {
            // Gets the page category
            CategoryInfo category = CategoryInfoProvider.GetCategoryInfo("Carnivores", SiteContext.CurrentSiteName);

            // Checks if the previous method retrieved the record
            if (category != null)
            {
                // Updates the category properties
                category.CategoryDescription = "People who are not vegetarians";

                // Saves the changes to the database
                CategoryInfoProvider.SetCategoryInfo(category);
            }
        }


        /// <heading>Updating multiple page categories</heading>
        private void GetAndBulkUpdateCategories()
        {
            // Gets all first level categories on the current site
            var categories = CategoryInfoProvider.GetCategories().WhereEquals("CategoryLevel", 0).OnSite(SiteContext.CurrentSiteID);

            // Loops through the categories
            foreach (CategoryInfo category in categories)
            {
                // Updates the code names of the categories
                category.CategoryName = category.CategoryName.ToLower();

                // Saves the changes to the database
                CategoryInfoProvider.SetCategoryInfo(category);
            }
        }
               

        /// <heading>Creating subcategories</heading>
        private void CreateSubcategory()
        {
            // Gets the parent category
            CategoryInfo parentCategory = CategoryInfoProvider.GetCategoryInfo("carnivores", SiteContext.CurrentSiteName);

            if (parentCategory != null)
            {
                // Creates a new category object
                CategoryInfo subcategory = new CategoryInfo();

                // Sets basic properties
                subcategory.CategoryDisplayName = "Dogs";
                subcategory.CategoryName = "Dogs";
                subcategory.CategoryDescription = "Man's best friends";
                subcategory.CategorySiteID = SiteContext.CurrentSiteID;
                subcategory.CategoryEnabled = true;

                // Assigns to the parent category
                subcategory.CategoryParentID = parentCategory.CategoryID;

                // Saves the category to the database
                CategoryInfoProvider.SetCategoryInfo(subcategory);
            }
        }

        
        /// <heading>Adding pages to categories</heading>
        private void AddDocumentToCategory()
        {
            // Gets the category
            CategoryInfo category = CategoryInfoProvider.GetCategoryInfo("Dogs", SiteContext.CurrentSiteName);

            if (category != null)
            {
                // Creates a new instance of the tree provider, needed for page operations
                TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

                // Gets a page
                TreeNode root = tree.SelectNodes()
                    .Path("/")
                    .OnCurrentSite()
                    .CombineWithDefaultCulture()
                    .FirstObject;

                // Adds the page to the category
                DocumentCategoryInfoProvider.AddDocumentToCategory(root.DocumentID, category.CategoryID);
            }
        }


        /// <heading>Removing pages from categories</heading>
        private void RemoveDocumentFromCategory()
        {
            // Gets the category
            CategoryInfo category = CategoryInfoProvider.GetCategoryInfo("Dogs", SiteContext.CurrentSiteName);

            if (category != null)
            {
                // Creates a new instance of the tree provider, needed for page operations
                TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

                // Gets the page
                TreeNode root = tree.SelectNodes()
                    .Path("/")
                    .OnCurrentSite()
                    .CombineWithDefaultCulture()
                    .FirstObject;

                // Gets the page category relationship record
                DocumentCategoryInfo documentCategory = DocumentCategoryInfoProvider.GetDocumentCategoryInfo(root.DocumentID, category.CategoryID);

                if (documentCategory != null)
                {
                    // Removes the relationship record from the database
                    DocumentCategoryInfoProvider.DeleteDocumentCategoryInfo(documentCategory);
                }
            }
        }


        /// <heading>Deleting page categories</heading>
        private void DeleteCategory()
        {
            // Gets the page category
            CategoryInfo category = CategoryInfoProvider.GetCategoryInfo("Carnivores", SiteContext.CurrentSiteName);

            if (category != null)
            {
                // Deletes the category
                CategoryInfoProvider.DeleteCategoryInfo(category);
            }
        }
    }
}
