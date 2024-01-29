using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;

using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Provides advanced manipulation with product variants.
    /// </summary>
    public class VariantHelper : AbstractHelper<VariantHelper>
    {
        #region "Public methods"

        /// <summary>
        /// Saves this instance of product variant with specified product options to database.
        /// </summary>
        /// <param name="productVariant">The product variant.</param>
        public static void SetProductVariant(ProductVariant productVariant)
        {
            HelperObject.SetProductVariantInternal(productVariant);
        }


        /// <summary>
        /// Gets SKU object of product variant from parent product and options.
        /// </summary>
        /// <param name="productId">The product ID.</param>
        /// <param name="productAttributes">The product options.</param>
        /// <returns>SKU object.</returns>
        public static SKUInfo GetProductVariant(int productId, ProductAttributeSet productAttributes)
        {
            return HelperObject.GetProductVariantInternal(productId, productAttributes, false);
        }


        /// <summary>
        /// Creates new variant object (from product and options) without saving it.
        /// </summary>
        /// <param name="productId">The product ID.</param>
        /// <param name="productAttributes">The product options.</param>
        /// <returns>New variant object.</returns>
        public static ProductVariant CreateVariant(int productId, ProductAttributeSet productAttributes)
        {
            return HelperObject.CreateVariantInternal(productId, productAttributes);
        }


        /// <summary>
        /// Returns existing variants of specified product with added new categories specified with default options. Categories which are already included are ignored.
        /// </summary>
        /// <param name="productId">The product ID.</param>
        /// <param name="productAttributes">Default product options (and their categories) to add.</param>
        public static List<ProductVariant> AddNewCategoriesToVariantsOfProduct(int productId, ProductAttributeSet productAttributes)
        {
            return HelperObject.AddNewCategoriesToVariantsOfProductInternal(productId, productAttributes);
        }


        /// <summary>
        /// Deletes the specified variant.
        /// </summary>
        /// <param name="variant">The variant.</param>
        public static void DeleteVariant(ProductVariant variant)
        {
            HelperObject.DeleteVariantInternal(variant.Variant.SKUID);
        }


        /// <summary>
        /// Deletes the specified variant.
        /// </summary>
        /// <param name="productVariantId">The product variant ID.</param>
        public static void DeleteVariant(int productVariantId)
        {
            HelperObject.DeleteVariantInternal(productVariantId);
        }


        /// <summary>
        /// Deletes all variants one by one. If delete operation is successful for processing variant removeSucceededAction with variant ID as a parameter is launched.
        /// </summary>
        /// <param name="productId">The product ID with variants to delete.</param>
        /// <param name="removeSucceededAction">The remove succeeded action with variant ID as a parameter.</param>        
        public static void DeleteAllVariants(int productId, Action<int> removeSucceededAction = null)
        {
            HelperObject.DeleteAllVariantsInternal(productId, removeSucceededAction);
        }


        /// <summary>
        /// Gets the variants of specified product.
        /// </summary>
        /// <param name="productId">The product ID.</param>
        /// <returns>The variants of specified product.</returns>
        public static ObjectQuery<SKUInfo> GetVariants(int productId)
        {
            return HelperObject.GetVariantsInternal(productId);
        }


        /// <summary>
        /// Gets the product variants categories for specified product.
        /// </summary>
        /// <param name="productId">Product ID.</param>
        /// <returns>Ordered product option categories, that are used in specified product variants.</returns>
        public static ObjectQuery<OptionCategoryInfo> GetProductVariantsCategories(int productId)
        {
            return HelperObject.GetProductVariantsCategoriesInternal(productId);
        }


        /// <summary>
        /// Gets the product variants category IDs for specified product.
        /// </summary>
        /// <param name="productId">Product ID.</param>
        /// <returns>Ordered product option category IDs, that are used in specified product variants.</returns>
        public static ICollection<int> GetProductVariantsCategoryIDs(int productId)
        {
            // GetProductVariantsCategories create a complex query with join included -> column must be defined with table name prefix
            // Convert query to list before calling Select, otherwise there will be ambiguous column CategoryID
            return GetProductVariantsCategories(productId).Column("COM_OptionCategory.CategoryID").ToList().Select(category => category.CategoryID).ToList();
        }


        /// <summary>
        /// Gets all possible variants for specified product and all combinations (including existing variants) of options from categories.
        /// </summary>
        /// <param name="productId">The product ID.</param>
        /// <param name="optionCategoriesIds">The option categories ids.</param>
        /// <remarks>
        /// Please note, that some of returned objects are already stored in database (existing variants) and some not (all non-existing variants). To ensure that variant is already stored use Variant property CMS.Ecommerce.ProductVariant.Saved.
        /// </remarks>
        public static List<ProductVariant> GetAllPossibleVariants(int productId, IEnumerable<int> optionCategoriesIds)
        {
            return HelperObject.GetAllPossibleVariantsInternal(productId, optionCategoriesIds);
        }


        /// <summary>
        /// Gets all possible variants for specified set (Returns provided variants plus remaining combinations).
        /// </summary>
        /// <param name="productVariants">The product variants.</param>        
        /// <exception cref="System.Exception">[VariantHelper.GetAllPossibleVariants]: Specified variants must be of the same product.</exception>
        public static List<ProductVariant> GetAllPossibleVariants(List<ProductVariant> productVariants)
        {
            return HelperObject.GetAllPossibleVariantsInternal(productVariants);
        }


        /// <summary>
        /// Gets number of all variants, which can be generated including existing variants of options from categories.
        /// </summary>
        /// <param name="skuId">The product ID.</param>
        /// <param name="categoryIds">The option categories ids.</param>
        /// <returns>Number of all possible variants.</returns>
        public static int GetAllPossibleVariantsCount(int skuId, IEnumerable<int> categoryIds)
        {
            return HelperObject.GetAllPossibleVariantsCountInternal(skuId, categoryIds);
        }


        /// <summary>
        /// Checks if specified product options are assigned to product and are allowed in it.
        /// </summary>
        /// <param name="productId">The product ID.</param>
        /// <param name="productAttributes">The product options.</param>
        /// <returns>True if specified product options are assigned to product and are allowed in product; false otherwise.</returns>
        public static bool CheckAvailableProductAttributes(int productId, ProductAttributeSet productAttributes)
        {
            return HelperObject.CheckAvailableProductAttributesInternal(productId, productAttributes);
        }


        /// <summary>
        /// Checks if specified product options contains all and only categories from existing variants of specified product.
        /// </summary>
        /// <param name="productId">The product ID.</param>
        /// <param name="productAttributes">The product options to check.</param>
        /// <returns>True if product options contains all and only categories from existing variants; False otherwise.</returns>     
        public static bool CheckVariantCategoriesConsistency(int productId, ProductAttributeSet productAttributes)
        {
            return HelperObject.CheckVariantCategoriesConsistencyInternal(productId, productAttributes);
        }


        /// <summary>
        /// Checks if specified categories are used all and only in variants of specified product.
        /// </summary>
        /// <param name="productId">The product ID.</param>
        /// <param name="categoryIds">The category IDs.</param>
        /// <returns>True if specified categories are used all and only in variants of specified product; False otherwise.</returns>
        public static bool CheckVariantCategoriesConsistency(int productId, IEnumerable<int> categoryIds)
        {
            return HelperObject.CheckVariantCategoriesConsistencyInternal(productId, categoryIds);
        }


        /// <summary>
        /// Checks if the variant options in specified variant corresponds with options determined in parent product.
        /// </summary>
        /// <param name="variant">The variant.</param>
        /// <returns>True if variant options in specified variant corresponds with options determined in parent product and false otherwise</returns>
        public static bool CheckVariantOptions(ProductVariant variant)
        {
            return HelperObject.CheckVariantOptionsInternal(variant);
        }


        /// <summary>
        /// Determines that variant for specified product with specified options exists.
        /// </summary>
        /// <param name="productId">The product ID.</param>
        /// <param name="productAttributes">The product options.</param>
        /// <param name="onlyEnabled">If set to true [process only enabled variants].</param>
        /// <returns>
        /// True if variant for specified product with specified options exists and false otherwise.
        /// </returns>
        public static bool VariantExists(int productId, ProductAttributeSet productAttributes, bool onlyEnabled = true)
        {
            return HelperObject.VariantExistsInternal(productId, productAttributes, onlyEnabled);
        }


        /// <summary>
        /// Checks if the specified categories are used in variants of specified product.
        /// </summary>
        /// <param name="productId">The product ID.</param>
        /// <param name="optionCategoryIds">The option category IDs.</param>
        /// <returns>
        /// True if the specified categories are used in variants of specified product and false otherwise.
        /// </returns>
        public static bool AreCategoriesUsedInVariants(int productId, IEnumerable<int> optionCategoryIds)
        {
            return HelperObject.AreCategoriesUsedInVariantsInternal(productId, optionCategoryIds);
        }


        /// <summary>
        /// Regenerates the SKU name and number.
        /// </summary>
        /// <param name="productVariant">The product variant.</param>
        public static void RegenerateSKUNameAndNumber(ProductVariant productVariant)
        {
            HelperObject.RegenerateSKUNameAndNumberInternal(productVariant);
        }


        /// <summary>
        /// Gets enabled product option categories included disabled option categories used in product variants.
        /// </summary>
        /// <param name="productId">ID of product from which are option categories selected</param>
        /// <param name="categoryType">Type of option category which are selected</param>
        /// <returns>
        /// DataSet with enabled option categories used in product plus disabled categories already used in variants.
        /// </returns>
        public static DataSet GetUsedProductOptionCategories(int productId, OptionCategoryTypeEnum? categoryType = null)
        {
            return HelperObject.GetUsedProductOptionCategoriesInternal(productId, categoryType);
        }


        /// <summary>
        /// Gets options from option category assigned to sku which are enabled or already used in variants. 
        /// </summary>
        /// <param name="skuID">ID of SKU with assigned categories</param>
        /// <param name="categoryId">Option category from which are options selected</param>
        /// <returns>Data set with enabled options plus disabled options used in variants</returns>
        public static InfoDataSet<SKUInfo> GetEnabledOptionsWithVariantOptions(int skuID, int categoryId)
        {
            return HelperObject.GetEnabledOptionsWithVariantOptionsInternal(skuID, categoryId);
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Saves this instance of product variant with specified product options to database.
        /// </summary>
        /// <param name="productVariant">The product variant.</param>
        protected virtual void SetProductVariantInternal(ProductVariant productVariant)
        {
            if (productVariant == null || productVariant.Variant == null || productVariant.ProductAttributes == null)
            {
                return;
            }

            using (var transaction = new CMSTransactionScope())
            {
                SKUInfoProvider.SetSKUInfo(productVariant.Variant);

                DataSet dsExistingRelations = VariantOptionInfoProvider.GetVariantOptions()
                                                   .WhereEquals("VariantSKUID", productVariant.Variant.SKUID);

                // Product options that already are related with variant
                HashSet<int> alreadyRelatedOptionIds = new HashSet<int>();

                if (!DataHelper.DataSourceIsEmpty(dsExistingRelations))
                {
                    foreach (DataRow dr in dsExistingRelations.Tables[0].Rows)
                    {
                        alreadyRelatedOptionIds.Add(ValidationHelper.GetInteger(dr["OptionSKUID"], 0));
                    }
                }

                foreach (SKUInfo option in productVariant.ProductAttributes)
                {
                    // Save only non-existing (new) relations
                    if (alreadyRelatedOptionIds.Contains(option.SKUID))
                    {
                        continue;
                    }

                    VariantOptionInfoProvider.AddOptionToVariant(productVariant.Variant.SKUID, option.SKUID);
                }

                transaction.Commit();
            }
        }


        /// <summary>
        /// Gets SKU object of product variant from parent product and options.
        /// </summary>
        /// <param name="productId">The product ID.</param>
        /// <param name="productAttributes">The product options.</param>
        /// <param name="onlyEnabled">if set to true [process only enabled variants].</param>
        /// <returns>SKU object.</returns>
        protected virtual SKUInfo GetProductVariantInternal(int productId, ProductAttributeSet productAttributes, bool onlyEnabled = true)
        {
            var query = SKUInfoProvider.GetSKUs()
                            .TopN(1)
                            .WhereEquals("SKUParentSKUID", productId);

            if (onlyEnabled)
            {
                query.WhereTrue("SKUEnabled");
            }

            foreach (int prodOptionId in productAttributes.ProductAttributeIDs)
            {
                query.WhereIn("SKUID", VariantOptionInfoProvider.GetVariantOptions()
                                           .Column("VariantSKUID")
                                           .WhereEquals("OptionSKUID", prodOptionId)
                             );
            }

            return query.FirstOrDefault();
        }


        /// <summary>
        /// Gets the variants of specified product.
        /// </summary>
        /// <param name="productId">The product ID.</param>
        /// <returns>The variants of specified product.</returns>
        protected virtual ObjectQuery<SKUInfo> GetVariantsInternal(int productId)
        {
            return SKUInfoProvider.GetSKUs().WhereEquals("SKUParentSKUID", productId);
        }


        /// <summary>
        /// Gets the product variants categories in the specified order for the given product.
        /// </summary>
        /// <param name="productId">Product ID.</param>
        /// <returns>Ordered product option categories, that are used in specified product variants.</returns>
        protected virtual ObjectQuery<OptionCategoryInfo> GetProductVariantsCategoriesInternal(int productId)
        {
            // Only one variant needed
            var firstVariant = SKUInfoProvider.GetSKUs().TopN(1).WhereEquals("SKUParentSKUID", productId).Column("SKUID");

            // Get IDs of used categories
            var where = new WhereCondition()
                .WhereIn("COM_OptionCategory.CategoryID", new IDQuery<SKUInfo>("SKUOptionCategoryID")
                    .Source(s => s.Join<VariantOptionInfo>("SKUID", "OptionSKUID"))
                    .WhereEquals("VariantSKUID", firstVariant));


            // Join with SKUOptionCategory to retrieve categories in right order
            return
                OptionCategoryInfoProvider.GetOptionCategories()
                .Source(s => s.Join<SKUOptionCategoryInfo>("CategoryID", "CategoryID"))
                .OrderBy("SKUCategoryOrder")
                .WhereEquals("SKUID", productId)
                .Where(where)
                .Columns("COM_OptionCategory.*")
                .OrderBy("SKUCategoryOrder");
        }


        /// <summary>
        /// Creates new variant object (from product and options) without saving it.
        /// </summary>
        /// <param name="productId">The product ID.</param>
        /// <param name="productAttributes">The product options.</param>
        /// <returns>New variant object.</returns>
        protected virtual ProductVariant CreateVariantInternal(int productId, ProductAttributeSet productAttributes)
        {
            // Check link between product and options
            if (!CheckAvailableProductAttributesInternal(productId, productAttributes))
            {
                throw new Exception("[VariantHelper.CreateVariant]: One or more product options are not allowed in product. Variant can't be created.");
            }

            // Check if product options contains all and only categories from existing variants
            if (!CheckVariantCategoriesConsistencyInternal(productId, productAttributes))
            {
                throw new Exception("[VariantHelper.CreateVariant]: Product options does not contain all categories from existing variants or contains categories that are not used in variants. Variant can't be created.");
            }

            // Create variant
            return new ProductVariant(productId, productAttributes);
        }


        /// <summary>
        /// Deletes the specified variant.
        /// </summary>
        /// <param name="productVariantId">The product variant ID.</param>
        protected virtual void DeleteVariantInternal(int productVariantId)
        {
            SKUInfoProvider.DeleteSKUInfo(productVariantId);
        }


        /// <summary>
        /// Checks if specified product options are assigned to product and are allowed in it.
        /// </summary>
        /// <param name="productId">The product ID.</param>
        /// <param name="productAttributes">The product options.</param>
        /// <returns>True if specified product options are assigned to product and are allowed in product; false otherwise.</returns>
        protected virtual bool CheckAvailableProductAttributesInternal(int productId, ProductAttributeSet productAttributes)
        {
            Dictionary<int, DataSet> allowedProductAttributesCache = new Dictionary<int, DataSet>();

            if (productAttributes == null)
            {
                return true;
            }

            foreach (SKUInfo pOptionToCheck in productAttributes)
            {
                int pOptionCategoryId = pOptionToCheck.SKUOptionCategoryID;
                DataSet allowedProductAttributes = null;

                if (allowedProductAttributesCache.ContainsKey(pOptionCategoryId))
                {
                    allowedProductAttributes = allowedProductAttributesCache[pOptionCategoryId];
                }
                else
                {
                    allowedProductAttributes = SKUInfoProvider.GetSKUOptionsForProduct(productId, pOptionCategoryId, true).OrderBy("SKUOrder");
                    allowedProductAttributesCache.Add(pOptionCategoryId, allowedProductAttributes);
                }
                // DataSet is empty -> option is not assigned/allowed in product
                if (DataHelper.DataSourceIsEmpty(allowedProductAttributes))
                {
                    return false;
                }

                bool found = false;
                // Looking for option in allowedProductAttributes
                foreach (DataRow dRow in allowedProductAttributes.Tables[0].Rows)
                {
                    if (dRow["SKUID"].Equals(pOptionToCheck.SKUID))
                    {
                        found = true;
                        break;
                    }
                }
                // Option has not been found -> option is not assigned/allowed in product
                if (!found)
                {
                    return false;
                }
            }
            // All options are assigned and allowed in product
            return true;
        }


        /// <summary>
        /// Checks if specified product options contains all and only categories from existing variants of specified product.
        /// </summary>
        /// <param name="productId">The product ID.</param>
        /// <param name="productAttributes">The product options to check.</param>
        /// <returns>True if product options contains all and only categories from existing variants; False otherwise.</returns>       
        protected virtual bool CheckVariantCategoriesConsistencyInternal(int productId, ProductAttributeSet productAttributes)
        {
            IEnumerable<int> ids = productAttributes.Select(attribute => attribute.SKUOptionCategoryID);

            return CheckVariantCategoriesConsistencyInternal(productId, ids);
        }


        /// <summary>
        /// Checks if specified categories are used all and only in variants of specified product.
        /// </summary>
        /// <param name="productId">The product ID.</param>
        /// <param name="categoryIds">The category IDs.</param>
        /// <returns>True if specified categories are used all and only in variants of specified product; False otherwise.</returns>
        protected virtual bool CheckVariantCategoriesConsistencyInternal(int productId, IEnumerable<int> categoryIds)
        {
            var variantCategories = GetProductVariantsCategoriesInternal(productId).ToList();
            // Product has no variants
            if (!variantCategories.Any())
            {
                return true;
            }

            IEnumerable<int> variantCategoryIds = variantCategories.Select(oci => oci.CategoryID);

            return ScrambledEquals(categoryIds, variantCategoryIds);
        }


        /// <summary>
        /// Checks if the variant options in specified variant corresponds with options determined in parent product.
        /// </summary>
        /// <param name="variant">The variant.</param>
        /// <returns>True if variant options in specified variant corresponds with options determined in parent product and false otherwise</returns>
        protected virtual bool CheckVariantOptionsInternal(ProductVariant variant)
        {
            if (variant == null)
            {
                return false;
            }

            return CheckAvailableProductAttributesInternal(variant.ParentProductID, variant.ProductAttributes)
                && CheckVariantCategoriesConsistencyInternal(variant.ParentProductID, variant.ProductAttributes);
        }


        /// <summary>
        /// Determines that variant for specified product with specified options exists.
        /// </summary>
        /// <param name="productId">The product ID.</param>
        /// <param name="productAttributes">The product options.</param>
        /// <param name="onlyEnabled">if set to true [process only enabled variants].</param>
        /// <returns>
        /// True if variant for specified product with specified options exists and false otherwise.
        /// </returns>
        protected virtual bool VariantExistsInternal(int productId, ProductAttributeSet productAttributes, bool onlyEnabled = true)
        {
            return GetProductVariantInternal(productId, productAttributes, onlyEnabled) != null;
        }


        /// <summary>
        /// Returns existing variants of specified product with added new categories specified with default options. Categories which are already included are ignored.
        /// </summary>
        /// <param name="productId">The product ID.</param>
        /// <param name="productAttributes">Default product options (and their categories) to add.</param>
        protected virtual List<ProductVariant> AddNewCategoriesToVariantsOfProductInternal(int productId, ProductAttributeSet productAttributes)
        {
            if (!CheckAvailableProductAttributesInternal(productId, productAttributes))
            {
                throw new Exception("[VariantHelper.AddNewCategoriesToVariantsOfProduct]: Some of product options are not allowed in specified product.");
            }

            var categories = GetProductVariantsCategories(productId);
            var allVariants = GetVariantsInternal(productId);

            if (!allVariants.Any())
            {
                return new List<ProductVariant>();
            }
            // Existing variants transformed into ProductVariant envelope
            List<ProductVariant> variants = new List<ProductVariant>();

            foreach (var variantSKU in allVariants)
            {
                variants.Add(new ProductVariant(variantSKU.SKUID));
            }

            // Categories already included in variants
            HashSet<int> includedCategories = new HashSet<int>();

            foreach (var category in categories)
            {
                includedCategories.Add(category.CategoryID);
            }

            // Add new categories with default option            
            foreach (SKUInfo productAttribute in productAttributes)
            {
                if (!includedCategories.Contains(productAttribute.SKUOptionCategoryID))
                {
                    variants.ForEach(v => v.ProductAttributes.Add(productAttribute));

                    includedCategories.Add(productAttribute.SKUOptionCategoryID);
                }
            }

            return variants;
        }


        /// <summary>
        /// Deletes all variants one by one. If delete operation is successful for processing variant removeSucceededAction with variant ID as a parameter is launched.
        /// </summary>
        /// <param name="productId">The product ID with variants to delete.</param>
        /// <param name="removeSucceededAction">The remove succeeded action with variant ID as a parameter.</param>        
        protected virtual void DeleteAllVariantsInternal(int productId, Action<int> removeSucceededAction = null)
        {
            using (var transaction = new CMSTransactionScope())
            {
                DataSet dsAllVariants = GetVariantsInternal(productId);

                if (DataHelper.DataSourceIsEmpty(dsAllVariants))
                {
                    return;
                }

                foreach (DataRow dr in dsAllVariants.Tables[0].Rows)
                {
                    int variantId = ValidationHelper.GetInteger(dr["SKUID"], 0);

                    DeleteVariantInternal(variantId);

                    removeSucceededAction?.Invoke(variantId);
                }

                transaction.Commit();
            }
        }


        /// <summary>
        /// Gets all possible variants for specified product and all combinations (including existing variants) of options from categories.
        /// </summary>
        /// <remarks> 
        /// Please note, that some of returned objects are already stored in database (existing variants) and some not (all non-existing variants). To ensure that variant is already stored use Variant property CMS.Ecommerce.ProductVariant.Saved.
        /// </remarks> 
        /// <param name="productId">The product ID.</param>
        /// <param name="optionCategoriesIds">The option categories IDs</param>        
        /// <returns>All possible variants for specified product and all combinations of options from categories</returns>
        protected virtual List<ProductVariant> GetAllPossibleVariantsInternal(int productId, IEnumerable<int> optionCategoriesIds)
        {
            List<ProductVariant> listOfVariants = new List<ProductVariant>();

            // Get existing variants
            DataSet dsVariants = GetVariantsInternal(productId);

            if (!optionCategoriesIds.Any())
            {
                return listOfVariants;
            }

            if (!DataHelper.DataSourceIsEmpty(dsVariants))
            {
                // If variants exists, only already used categories are allowed
                if (!CheckVariantCategoriesConsistencyInternal(productId, optionCategoriesIds))
                {
                    throw new Exception("[VariantHelper.GetAllPossibleVariants]: Specified product already has variants with option categories that do not correspond with specified option categories.");
                }

                // Existing variants transformed to ProductVariant envelope
                foreach (DataRow dr in dsVariants.Tables[0].Rows)
                {
                    ProductVariant productVariant = new ProductVariant(ValidationHelper.GetInteger(dr["SKUID"], 0));
                    listOfVariants.Add(productVariant);
                }
            }

            return GenerateRemainingVariants(productId, listOfVariants, optionCategoriesIds);
        }


        /// <summary>
        /// Gets number of all variants, which can be generated including existing variants of options from categories.
        /// </summary>
        /// <param name="skuId">The product ID.</param>
        /// <param name="categoryIds">The option categories ids.</param>
        /// <returns>Number of all possible variants.</returns>
        protected virtual int GetAllPossibleVariantsCountInternal(int skuId, IEnumerable<int> categoryIds)
        {
            int result = 1;

            foreach (var categoryId in categoryIds)
            {
                var options = HelperObject.GetEnabledOptionsWithVariantOptionsInternal(skuId, categoryId);
                result *= options.Tables[0].Rows.Count;
            }

            return result;
        }


        /// <summary>
        /// Returns list of product variants with newly generated remaining combinations for specified product but with specified variants (listOfVariants) that doesn't need to be saved yet. 
        /// </summary>
        /// <param name="productId">The product id.</param>
        /// <param name="listOfVariants">The list of variants that are already created, but doesn't need to be saved yet.</param>
        /// <param name="optionCategoryIds">The option category ids. Must correspond with listOfVariants if specified.</param>         
        protected virtual List<ProductVariant> GenerateRemainingVariants(int productId, List<ProductVariant> listOfVariants, IEnumerable<int> optionCategoryIds)
        {
            if (listOfVariants == null)
            {
                listOfVariants = new List<ProductVariant>();
            }

            // Generate all possibilities
            List<string> allPossibilities = new List<string>();
            allPossibilities.Add(string.Empty);

            HashSet<string> existingVariantsCodes = new HashSet<string>();

            foreach (var productVariant in listOfVariants)
            {
                existingVariantsCodes.Add(productVariant.ProductAttributes.GetCompareCode);
            }

            foreach (int categoryId in optionCategoryIds)
            {
                DataSet dsOptions = SKUInfoProvider.GetSKUOptionsForProduct(productId, categoryId, true).OrderBy("SKUOrder");

                if (DataHelper.DataSourceIsEmpty(dsOptions))
                {
                    continue;
                }
                // Option ids sets are represented like strings for easier Cartesian product
                List<string> optionIds = new List<string>();

                foreach (DataRow optionRow in dsOptions.Tables[0].Rows)
                {
                    int optionId = ValidationHelper.GetInteger(optionRow["SKUID"], 0);
                    optionIds.Add(optionId.ToString());
                }
                // All possible option combinations represented as strings of option IDs
                allPossibilities = Cartesian(allPossibilities, optionIds);
            }
            // Strings of option IDs are transformed to ProductAttributeSets
            foreach (string optionIdsString in allPossibilities)
            {
                int[] ids = ValidationHelper.GetIntegers(optionIdsString.Split(','), 0);
                ProductAttributeSet productAttributesSet = new ProductAttributeSet();

                foreach (int id in ids)
                {
                    productAttributesSet.Add(id);
                }

                if (!existingVariantsCodes.Contains(productAttributesSet.GetCompareCode))
                {
                    // New variant does not exist
                    ProductVariant productVariant = new ProductVariant(productId, productAttributesSet);
                    listOfVariants.Add(productVariant);
                }
            }

            listOfVariants.Sort((a, b) =>
                {
                    int result = 0;
                    // Sort through multiple columns (all attributes order)
                    foreach (int categoryId in optionCategoryIds)
                    {
                        if (result == 0)
                        {
                            SKUInfo firstAttribute = a.ProductAttributes.FirstOrDefault(atr => atr.SKUOptionCategoryID == categoryId);
                            SKUInfo secondAttribute = b.ProductAttributes.FirstOrDefault(atr => atr.SKUOptionCategoryID == categoryId);

                            int firstOrder = firstAttribute?.SKUOrder ?? 0;
                            int secondOrder = secondAttribute?.SKUOrder ?? 0;

                            result = firstOrder.CompareTo(secondOrder);
                        }
                        else
                        {
                            break;
                        }
                    }

                    return result;
                }
                );

            return listOfVariants;
        }


        /// <summary>
        /// Gets all possible variants for specified set (Returns provided variants plus remaining combinations).
        /// </summary>
        /// <param name="productVariants">The product variants.</param>        
        /// <exception cref="System.Exception">[VariantHelper.GetAllPossibleVariants]: Specified variants must be of the same product.</exception>
        protected virtual List<ProductVariant> GetAllPossibleVariantsInternal(List<ProductVariant> productVariants)
        {
            if (productVariants.Count == 0)
            {
                return productVariants;
            }

            int productID = productVariants.First().Variant.SKUParentSKUID;

            if (productVariants.Any(productVariant => productVariant.Variant.SKUParentSKUID != productID))
            {
                throw new Exception("[VariantHelper.GetAllPossibleVariants]: Specified variants must be of the same product.");
            }

            IEnumerable<int> optionCategoryIds = productVariants.First().ProductAttributes.CategoryIDs;

            return GenerateRemainingVariants(productID, productVariants, optionCategoryIds);
        }


        /// <summary>
        /// Checks if the specified categories are used in variants of specified product.
        /// </summary>
        /// <param name="productId">The product ID.</param>
        /// <param name="optionCategoryIds">The option category IDs.</param>
        /// <returns>
        /// True if the specified categories are used in variants of specified product and false otherwise.
        /// </returns>
        protected virtual bool AreCategoriesUsedInVariantsInternal(int productId, IEnumerable<int> optionCategoryIds)
        {
            var variantCategories = GetProductVariantsCategoriesInternal(productId).ToList();

            if (!variantCategories.Any())
            {
                return false;
            }

            return optionCategoryIds.All(variantCategories.Select(c => c.CategoryID).ToHashSet().Contains);
        }


        /// <summary>
        /// Regenerates the SKU name and number.
        /// </summary>
        /// <param name="productVariant">The product variant.</param>
        protected virtual void RegenerateSKUNameAndNumberInternal(ProductVariant productVariant)
        {
            if (productVariant.ProductAttributes != null)
            {
                string generatedName = string.Empty;
                string generatedNumber = string.Empty;

                const string nameSeparator = ", ";
                const char numberSeparator = '-';

                Regex regex = RegexHelper.GetRegex(@"[ ]{1,}", RegexOptions.None);

                foreach (SKUInfo productAttribute in productVariant.ProductAttributes)
                {
                    generatedName += string.Format("{0}{1}", productAttribute.SKUName, nameSeparator);
                    // Replace whitespaces (possible multiple) with single numberSeparator
                    string productAttributeSKUName = regex.Replace(productAttribute.SKUName, numberSeparator.ToString());
                    productAttributeSKUName = productAttributeSKUName.Trim(numberSeparator);
                    generatedNumber += string.Format("{0}{1}", productAttributeSKUName, numberSeparator);
                }

                generatedName = TextHelper.TrimEndingWord(generatedName, nameSeparator.TrimEnd());
                generatedNumber = generatedNumber.Trim(numberSeparator);

                SKUInfo parentSku = productVariant.Variant.Parent as SKUInfo;

                if (parentSku != null)
                {
                    productVariant.Variant.SKUName = string.Format("{0} ({1})", parentSku.SKUName, generatedName);
                    productVariant.Variant.SKUNumber = string.Format("{0}{1}{2}", parentSku.SKUNumber, numberSeparator, generatedNumber).Trim(numberSeparator);
                }
            }
        }



        /// <summary>
        /// Gets enabled product option categories included disabled option categories used in product variants.
        /// </summary>
        /// <param name="productId">ID of product from which are option categories selected</param>
        /// <param name="categoryType">Type of option category which are selected</param>
        /// <returns>
        /// DataSet with enabled option categories used in product included disabled categories already used in variants.
        /// </returns>
        protected virtual DataSet GetUsedProductOptionCategoriesInternal(int productId, OptionCategoryTypeEnum? categoryType = null)
        {
            // Get enabled and assigned option categories
            DataSet assignedCategories = OptionCategoryInfoProvider.GetProductOptionCategories(productId, false, categoryType);

            if (!DataHelper.DataSourceIsEmpty(assignedCategories))
            {
                var usedProductOptionCategories = new DataSet();
                usedProductOptionCategories.Tables.Add(assignedCategories.Tables[0].Clone());

                foreach (DataRow category in assignedCategories.Tables[0].Rows)
                {
                    // Check if disabled option category is already used in variants (in this case must be visible even it is disabled)
                    if (!ValidationHelper.GetBoolean(category["CategoryEnabled"], false))
                    {
                        int categoryID = ValidationHelper.GetInteger(category["CategoryID"], 0);

                        if (AreCategoriesUsedInVariants(productId, new[] { categoryID }))
                        {
                            // Add option category because it is already used in variant
                            usedProductOptionCategories.Tables[0].ImportRow(category);
                        }
                    }
                    else
                    {
                        // Add option category because it is enabled and assigned to the product
                        usedProductOptionCategories.Tables[0].ImportRow(category);
                    }
                }

                return usedProductOptionCategories;
            }

            return null;
        }


        /// <summary>
        /// Gets options from option category assigned to sku which are enabled or already used in variants. 
        /// </summary>
        /// <param name="skuID">ID of SKU with assigned categories</param>
        /// <param name="categoryId">Option category from which are options selected</param>
        /// <returns>Data set with enabled options plus disabled options used in variants</returns>
        protected virtual InfoDataSet<SKUInfo> GetEnabledOptionsWithVariantOptionsInternal(int skuID, int categoryId)
        {
            DataSet allOptions = SKUInfoProvider.GetSKUOptionsForProduct(skuID, categoryId, false).OrderBy("SKUOrder");

            if (!DataHelper.DataSourceIsEmpty(allOptions))
            {
                DataSet enabledOptionsWithVariantOptions = new DataSet();
                enabledOptionsWithVariantOptions.Tables.Add(allOptions.Tables[0].Clone());

                foreach (DataRow option in allOptions.Tables[0].Rows)
                {
                    int optionId = ValidationHelper.GetInteger(option["SKUId"], 0);

                    // Check if disabled option is already used in variants (in this case must be visible even it is disabled)
                    if (!ValidationHelper.GetBoolean(option["SKUEnabled"], true))
                    {
                        // Check if some variant is defined by this option
                        SKUInfo variant = GetProductVariant(skuID, new ProductAttributeSet(optionId));

                        if ((variant != null) && (variant.SKUEnabled))
                        {
                            // Add option to data set because it is already used in variant
                            enabledOptionsWithVariantOptions.Tables[0].ImportRow(option);
                        }
                    }
                    else
                    {
                        // Add option to data set because it is already used in variant
                        enabledOptionsWithVariantOptions.Tables[0].ImportRow(option);
                    }
                }

                return new InfoDataSet<SKUInfo>(enabledOptionsWithVariantOptions);
            }

            return null;
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Cartesian product of two lists.
        /// </summary>
        /// <param name="first">The first list.</param>
        /// <param name="second">The second list.</param>      
        private List<string> Cartesian(List<string> first, List<string> second)
        {
            return (from sFirst in first
                    from sSecond in second
                    select (sFirst + "," + sSecond).Trim(',')).ToList();
        }


        /// <summary>
        /// Compares two scrambled IEnumerables.
        /// </summary>        
        /// <param name="list1">The list1.</param>
        /// <param name="list2">The list2.</param>        
        private bool ScrambledEquals<T>(IEnumerable<T> list1, IEnumerable<T> list2)
        {
            var counts = new Dictionary<T, int>();

            foreach (var s in list1)
            {
                if (counts.ContainsKey(s))
                {
                    counts[s]++;
                }
                else
                {
                    counts.Add(s, 1);
                }
            }

            foreach (var s in list2)
            {
                if (counts.ContainsKey(s))
                {
                    counts[s]--;
                }
                else
                {
                    return false;
                }
            }

            return counts.Values.All(c => c == 0);
        }

        #endregion
    }
}
