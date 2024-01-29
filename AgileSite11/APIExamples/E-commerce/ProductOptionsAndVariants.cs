using System;
using System.Collections.Generic;

using CMS.Ecommerce;
using CMS.SiteProvider;
using CMS.Base;

namespace APIExamples
{
    /// <summary>
    /// Holds product option and variant API examples.
    /// </summary>
    /// <pageTitle>Product options and variants</pageTitle>
    internal class ProductOptionsAndVariants
    {
        /// <summary>
        /// Holds product option category API examples.
        /// </summary>
        /// <groupHeading>Product option categories</groupHeading>
        private class ProductOptionCategories
        {
            /// <heading>Creating an option category</heading>
            private void CreateOptionCategory()
            {
                // Creates a new option category object
                OptionCategoryInfo newCategory = new OptionCategoryInfo();

                // Sets the option category properties
                newCategory.CategoryDisplayName = "New category";
                newCategory.CategoryName = "NewCategory";
                newCategory.CategoryType = OptionCategoryTypeEnum.Products;
                newCategory.CategorySelectionType = OptionCategorySelectionTypeEnum.Dropdownlist;
                newCategory.CategoryDisplayPrice = true;
                newCategory.CategoryEnabled = true;
                newCategory.CategoryDefaultRecord = "";
                newCategory.CategorySiteID = SiteContext.CurrentSiteID;

                // Saves the option category to the database
                OptionCategoryInfoProvider.SetOptionCategoryInfo(newCategory);
            }


            /// <heading>Updating an option category</heading>
            private void GetAndUpdateOptionCategory()
            {
                // Gets the option category
                OptionCategoryInfo updateCategory = OptionCategoryInfoProvider.GetOptionCategoryInfo("NewCategory", SiteContext.CurrentSiteName);
                if (updateCategory != null)
                {
                    // Updates the option category properties
                    updateCategory.CategoryDisplayName = updateCategory.CategoryDisplayName.ToLowerCSafe();

                    // Saves the changes to the database
                    OptionCategoryInfoProvider.SetOptionCategoryInfo(updateCategory);
                }
            }


            /// <heading>Updating multiple option categories</heading>
            private void GetAndBulkUpdateOptionCategories()
            {
                // Gets all option categories whose name starts with 'New'
                var categories = OptionCategoryInfoProvider.GetOptionCategories().WhereStartsWith("CategoryName", "New");

                // Loops through the option categories
                foreach (OptionCategoryInfo modifyCategory in categories)
                {
                    // Updates the option category properties
                    modifyCategory.CategoryDisplayName = modifyCategory.CategoryDisplayName.ToUpperCSafe();

                    // Saves the changes to the database
                    OptionCategoryInfoProvider.SetOptionCategoryInfo(modifyCategory);
                }
            }


            /// <heading>Adding option categories to a product</heading>
            private void AddCategoryToProduct()
            {
                // Gets the product
                SKUInfo product = SKUInfoProvider.GetSKUs()
                                                   .WhereEquals("SKUName", "NewProduct")
                                                   .WhereNull("SKUOptionCategoryID")
                                                   .FirstObject;

                // Gets the option category
                OptionCategoryInfo category = OptionCategoryInfoProvider.GetOptionCategoryInfo("NewCategory", SiteContext.CurrentSiteName);

                if ((product != null) && (category != null))
                {
                    // Adds the category to the product
                    SKUOptionCategoryInfoProvider.AddOptionCategoryToSKU(category.CategoryID, product.SKUID);
                }
            }


            /// <heading>Removing option categories from a product</heading>
            private void RemoveCategoryFromProduct()
            {
                // Gets the product
                SKUInfo product = SKUInfoProvider.GetSKUs()
                                                   .WhereEquals("SKUName", "NewProduct")
                                                   .FirstObject;

                // Gets the option category
                OptionCategoryInfo category = OptionCategoryInfoProvider.GetOptionCategoryInfo("NewCategory", SiteContext.CurrentSiteName);

                if ((product != null) && (category != null))
                {
                    // Removes the option category from the product
                    ProductHelper.RemoveOptionCategory(product.SKUID, category.CategoryID);
                }
            }


            /// <heading>Deleting an option category</heading>
            private void DeleteOptionCategory()
            {
                // Gets the option category
                OptionCategoryInfo deleteCategory = OptionCategoryInfoProvider.GetOptionCategoryInfo("NewCategory", SiteContext.CurrentSiteName);

                if (deleteCategory != null)
                {
                    // Deletes the option category
                    OptionCategoryInfoProvider.DeleteOptionCategoryInfo(deleteCategory);
                }
            }
        }


        /// <summary>
        /// Holds product option API examples.
        /// </summary>
        /// <groupHeading>Product options</groupHeading>
        private class ProductOptions
        {
            /// <heading>Creating a product option</heading>
            private void CreateOption()
            {
                // Gets a department
                DepartmentInfo department = DepartmentInfoProvider.GetDepartmentInfo("NewDepartment", SiteContext.CurrentSiteName);

                // Gets an option category
                OptionCategoryInfo category = OptionCategoryInfoProvider.GetOptionCategoryInfo("NewCategory", SiteContext.CurrentSiteName);

                if ((department != null) && (category != null))
                {
                    // Creates a new product option object
                    SKUInfo newOption = new SKUInfo();

                    // Sets the product option properties
                    newOption.SKUName = "NewProductOption";
                    newOption.SKUPrice = 199;
                    newOption.SKUEnabled = true;
                    newOption.SKUDepartmentID = department.DepartmentID;
                    newOption.SKUOptionCategoryID = category.CategoryID;
                    newOption.SKUSiteID = SiteContext.CurrentSiteID;
                    newOption.SKUProductType = SKUProductTypeEnum.Product;

                    // Saves the product option to the database
                    SKUInfoProvider.SetSKUInfo(newOption);
                }
            }


            /// <heading>Updating a product option</heading>
            private void GetAndUpdateOption()
            {
                // Gets the product option
                SKUInfo option = SKUInfoProvider.GetSKUs()
                                              .WhereEquals("SKUName", "NewProductOption")
                                              .WhereNotNull("SKUOptionCategoryID")
                                              .FirstObject;
                if (option != null)
                {
                    // Updates the product option properties
                    option.SKUName = option.SKUName.ToLowerCSafe();

                    // Saves the changes to the database
                    SKUInfoProvider.SetSKUInfo(option);
                }
            }


            /// <heading>Updating multiple product options</heading>
            private void GetAndBulkUpdateOptions()
            {
                // Gets a product option category
                OptionCategoryInfo optionCategory = OptionCategoryInfoProvider.GetOptionCategoryInfo("NewCategory", SiteContext.CurrentSiteName);

                // Gets all product options with the option category
                var options = SKUInfoProvider.GetSKUs().WhereEquals("SKUOptionCategoryID", optionCategory.CategoryID);

                // Loops through the product options
                foreach (SKUInfo modifyOption in options)
                {
                    // Updates the product option properties
                    modifyOption.SKUName = modifyOption.SKUName.ToUpperCSafe();

                    // Saves the changes to the database
                    SKUInfoProvider.SetSKUInfo(modifyOption);
                }
            }


            /// <heading>Allowing product options for a product</heading>
            private void AllowOptionForProduct()
            {
                // Gets the product
                SKUInfo product = SKUInfoProvider.GetSKUs()
                                               .WhereEquals("SKUName", "NewProduct")
                                               .WhereNull("SKUOptionCategoryID")
                                               .FirstObject;

                // Prepares a list for holding product option IDs
                List<int> optionIds = new List<int>();

                // Gets all product options whose name starts with 'New'
                var options = SKUInfoProvider.GetSKUs()
                                               .WhereStartsWith("SKUName", "New")
                                               .WhereNotNull("SKUOptionCategoryID");

                // Adds the IDs of the options to the list
                foreach (SKUInfo option in options)
                {
                    optionIds.Add(option.SKUID);
                }

                // Gets the option category
                OptionCategoryInfo category = OptionCategoryInfoProvider.GetOptionCategoryInfo("NewCategory", SiteContext.CurrentSiteName);

                if ((product != null) && (optionIds.Count > 0) && (category != null))
                {
                    // Allows the options for the product
                    ProductHelper.AllowOptions(product.SKUID, category.CategoryID, optionIds);
                }
            }


            /// <heading>Removing product options from a product</heading>
            private void RemoveOptionFromProduct()
            {
                // Gets the product
                SKUInfo product = SKUInfoProvider.GetSKUs()
                                               .WhereEquals("SKUName", "NewProduct")
                                               .WhereNull("SKUOptionCategoryID")
                                               .FirstObject;

                // Prepares a list for holding product option IDs
                List<int> optionIds = new List<int>();

                // Gets all product options whose name starts with 'New'
                var options = SKUInfoProvider.GetSKUs()
                                               .WhereEquals("SKUName", "New")
                                               .WhereNotNull("SKUOptionCategoryID");

                // Adds the IDs of the options to the list
                foreach (SKUInfo option in options)
                {
                    optionIds.Add(option.SKUID);
                }

                // Gets the option category
                OptionCategoryInfo category = OptionCategoryInfoProvider.GetOptionCategoryInfo("NewCategory", SiteContext.CurrentSiteName);

                if ((product != null) && (category != null) && (optionIds.Count > 0))
                {
                    // Removes the options in the list from the product
                    ProductHelper.RemoveOptions(product.SKUID, category.CategoryID, optionIds);
                }
            }


            /// <heading>Deleting a product option</heading>
            private void DeleteOption()
            {
                // Gets the product option
                SKUInfo option = SKUInfoProvider.GetSKUs()
                                              .WhereEquals("SKUName", "NewProductOption")
                                              .WhereNotNull("SKUOptionCategoryID")
                                              .FirstObject;
                if (option != null)
                {
                    // Deletes the product option
                    SKUInfoProvider.DeleteSKUInfo(option);
                }
            }
        }


        /// <summary>
        /// Holds product variant API examples.
        /// </summary>
        /// <groupHeading>Product variants</groupHeading>
        private class ProductVariants
        {
            /// <heading>Creating a product variant</heading>
            private void CreateVariants()
            {
                // Gets the product
                SKUInfo product = SKUInfoProvider.GetSKUs()
                                               .WhereEquals("SKUName", "NewProduct")
                                               .WhereNull("SKUOptionCategoryID")
                                               .FirstObject;

                if (product != null)
                {
                    // Prepares a list of option category IDs
                    List<int> categoryIDs = new List<int>();

                    // Creates two attribute option categories with product options
                    for (int i = 1; i <= 2; i++)
                    {
                        // Creates a new option category object and sets its properties
                        OptionCategoryInfo newCategory = new OptionCategoryInfo
                        {
                            CategoryDisplayName = "New attribute category " + i,
                            CategoryName = "NewAttributeCategory" + i,
                            CategoryType = OptionCategoryTypeEnum.Attribute,
                            CategorySelectionType = OptionCategorySelectionTypeEnum.Dropdownlist,
                            CategoryDisplayPrice = true,
                            CategoryEnabled = true,
                            CategoryDefaultRecord = "",
                            CategorySiteID = SiteContext.CurrentSiteID
                        };

                        // Saves the category to the database
                        OptionCategoryInfoProvider.SetOptionCategoryInfo(newCategory);

                        // Assigns the option category to the product
                        SKUOptionCategoryInfoProvider.AddOptionCategoryToSKU(newCategory.CategoryID, product.SKUID);
                        categoryIDs.Add(newCategory.CategoryID);

                        // Creates two product options for the category
                        foreach (String color in new[] { "Black", "White" })
                        {
                            // Creates a product option object and sets its properties
                            SKUInfo newOption = new SKUInfo
                            {
                                SKUName = "NewColorOption" + color,
                                SKUPrice = 0,
                                SKUEnabled = true,
                                SKUOptionCategoryID = newCategory.CategoryID,
                                SKUSiteID = SiteContext.CurrentSiteID,
                                SKUProductType = SKUProductTypeEnum.Product
                            };

                            // Saves the product option
                            SKUInfoProvider.SetSKUInfo(newOption);

                            // Assigns the product option to the product
                            SKUAllowedOptionInfoProvider.AddOptionToProduct(product.SKUID, newOption.SKUID);
                        }
                    }

                    // Generates the product variants
                    List<ProductVariant> variants = VariantHelper.GetAllPossibleVariants(product.SKUID, categoryIDs);

                    // Saves the variants to the database
                    foreach (ProductVariant variant in variants)
                    {
                        VariantHelper.SetProductVariant(variant);
                    }
                }
            }


            /// <heading>Updating product variants</heading>
            private void GetAndUpdateVariants()
            {
                // Gets the product
                SKUInfo product = SKUInfoProvider.GetSKUs()
                                                   .WhereEquals("SKUName", "NewProduct")
                                                   .WhereNull("SKUOptionCategoryID")
                                                   .FirstObject;

                if (product != null)
                {
                    // Gets the product's variants
                    var variants = VariantHelper.GetVariants(product.SKUID);
                    
                    // Loops through the product variants
                    foreach (SKUInfo updateVariant in variants)
                    {
                        // Updates the variant properties and saves them to the database
                        updateVariant.SKUName = updateVariant.SKUName.ToLowerCSafe();
                        SKUInfoProvider.SetSKUInfo(updateVariant);
                    }
                }
            }


            /// <heading>Deleting product variants</heading>
            private void DeleteVariants()
            {
                // Gets the product
                SKUInfo product = SKUInfoProvider.GetSKUs()
                                                   .WhereEquals("SKUName", "NewProduct")
                                                   .WhereNull("SKUOptionCategoryID")
                                                   .FirstObject;

                if (product != null)
                {
                    // Deletes all variants
                    VariantHelper.DeleteAllVariants(product.SKUID);

                    // Gets the product options
                    var options = SKUInfoProvider.GetSKUs().WhereStartsWith("SKUName", "NewColorOption");

                    // Loops through the product options
                    foreach (SKUInfo option in options)
                    {
                        // Deletes the product option
                        SKUInfoProvider.DeleteSKUInfo(option);
                    }

                    // Gets the option categories
                    var categories = OptionCategoryInfoProvider.GetOptionCategories().WhereStartsWith("CategoryName", "NewAttributeCategory");

                    // Loops through the option categories
                    foreach (OptionCategoryInfo category in categories)
                    {
                        // Deletes the categories
                        OptionCategoryInfoProvider.DeleteOptionCategoryInfo(category);
                    }
                }
            }
        }
    }
}
