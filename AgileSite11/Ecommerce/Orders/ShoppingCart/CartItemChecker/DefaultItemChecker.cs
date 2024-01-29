using System.Collections.Generic;
using System.Linq;

namespace CMS.Ecommerce
{
    internal class DefaultItemChecker : ICartItemChecker
    {
        private static readonly OptionCategorySelectionTypeEnum[] OBLIGATORY_TYPES =
        {
            OptionCategorySelectionTypeEnum.Dropdownlist,
            OptionCategorySelectionTypeEnum.RadioButtonsHorizontal,
            OptionCategorySelectionTypeEnum.RadioButtonsVertical
        };


        /// <summary>
        /// Checks if the <paramref name="newItemParams"/> are valid and product configuration can be added to the given <paramref name="cart"/>.
        /// </summary>
        /// <param name="newItemParams">New item parameters (product with options)</param>
        /// <param name="cart"><see cref="ShoppingCartInfo"/> where the new item will be placed.</param>
        /// <remarks>
        /// Following checks are performed:
        /// <list type="bullet">
        ///     <item><description><see cref="SKUInfo.SKUEnabled"/> for product/variant/option</description></item>
        ///     <item><description>Using of global/site products/options according current settings</description></item>
        ///     <item><description>Presence and validity of options for obligatory product categories</description></item>
        ///     <item><description>
        ///     Other invalid scenarios (e.g. presence of options used in variants, adding of main product with variants, 
        ///     disabled options for the given main product, adding standalone options e.t.c)
        ///     </description></item>
        /// </list>
        /// </remarks>
        public bool CheckNewItem(ShoppingCartItemParameters newItemParams, ShoppingCartInfo cart)
        {
            // Get checked product
            var skuObj = SKUInfoProvider.GetSKUInfo(newItemParams.SKUID);
            if ((skuObj == null) || (!skuObj.SKUEnabled))
            {
                return false;
            }

            if (skuObj.IsProductVariant)
            {
                // Get parent product of variant
                var parent = skuObj.Parent as SKUInfo;

                if (parent != null)
                {
                    skuObj = parent;
                }
            }
            else if (skuObj.HasVariants)
            {
                // If SKU has variants and parent product is added
                return false;
            }

            if (!CheckMainProduct(cart, skuObj))
            {
                return false;
            }

            if (!CheckOptions(newItemParams, skuObj, cart))
            {
                return false;
            }

            // Success
            return true;
        }


        private static bool CheckMainProduct(ShoppingCartInfo cart, SKUInfo skuObj)
        {
            // Can not add option as a product
            if (skuObj.SKUOptionCategoryID > 0)
            {
                return false;
            }

            if (skuObj.IsGlobal)
            {
                // Global products must be allowed
                if (!ECommerceSettings.AllowGlobalProducts(cart.SiteName))
                {
                    return false;
                }
            }
            else
            {
                // Site specific product must belong to the same site as shopping cart does
                if (skuObj.SKUSiteID != cart.ShoppingCartSiteID)
                {
                    return false;
                }
            }

            return true;
        }


        private static bool CheckOptions(ShoppingCartItemParameters newItemParams, SKUInfo skuObj, ShoppingCartInfo cart)
        {
            // Get option categories usable for filtered product (even disabled categories can be used if bound with the product)
            var allCategories = OptionCategoryInfoProvider.GetProductOptionCategories(skuObj.SKUID, false).ToList();

            // Get categories used in variants (subset of allCategories)
            var variantCategoryIDs = VariantHelper.GetProductVariantsCategoryIDs(skuObj.SKUID);

            var obligatoryCategories = GetObligatoryCategoriesForProduct(skuObj, allCategories, variantCategoryIDs);
            var coveredObligatoryCategories = new HashSet<int>();

            var allowGlobalOptions = ECommerceSettings.AllowGlobalProductOptions(cart.SiteName);

            // Check all options in loop
            foreach (var optionSku in newItemParams.ProductOptions.Select(o => SKUInfoProvider.GetSKUInfo(o.SKUID)))
            {
                if (optionSku?.SKUOptionCategory != null)
                {
                    var optionCategory = optionSku.SKUOptionCategory;

                    if (obligatoryCategories.Contains(optionCategory.CategoryID))
                    {
                        if (!coveredObligatoryCategories.Add(optionCategory.CategoryID))
                        {
                            // There are multiple options for 'one-option-only' obligatory category
                            return false;
                        }
                    }

                    if (variantCategoryIDs.Contains(optionCategory.CategoryID))
                    {
                        // Category is used for variants (cannot be added as separate option)
                        return false;
                    }

                    if (!CheckSKUOption(cart, optionSku, skuObj, allCategories, allowGlobalOptions))
                    {
                        return false;
                    }
                }
                else
                {
                    // Invalid option - checked 'option' is not an option or does not exist
                    return false;
                }
            }

            // Success when all obligatory categories are covered
            return obligatoryCategories.SetEquals(coveredObligatoryCategories);
        }


        private static HashSet<int> GetObligatoryCategoriesForProduct(SKUInfo skuObj, ICollection<OptionCategoryInfo> allCategories, ICollection<int> variantCategoryIDs)
        {
            var obligatoryCategories = new HashSet<int>();

            // Identify obligatory categories according selection type
            foreach (var category in allCategories)
            {
                var selectionType = category.CategorySelectionType;

                // Check if category is obligatory and not used in variants
                if (OBLIGATORY_TYPES.Contains(selectionType) && !variantCategoryIDs.Contains(category.CategoryID))
                {
                    // Only categories with available options 
                    if (SKUInfoProvider.GetSKUOptionsForProduct(skuObj.SKUID, category.CategoryID, true).TopN(1).HasResults())
                    {
                        obligatoryCategories.Add(category.CategoryID);
                    }
                }
            }

            return obligatoryCategories;
        }


        private static bool CheckSKUOption(ShoppingCartInfo cart, SKUInfo option, SKUInfo skuObj, ICollection<OptionCategoryInfo> allCategories, bool allowGlobalOptions)
        {
            int categoryID = option.SKUOptionCategoryID;

            // Check if option is allowed
            if (!ProductHelper.IsOptionAllowed(skuObj.SKUID, option.SKUID))
            {
                return false;
            }

            // Check if category is assigned to product
            if (allCategories.All(c => c.CategoryID != categoryID))
            {
                return false;
            }

            // Check if option is enabled
            if (!option.SKUEnabled)
            {
                return false;
            }

            if (option.IsGlobal)
            {
                // Global options must be allowed
                if (!allowGlobalOptions)
                {
                    return false;
                }
            }
            else
            {
                // Site specific option must belong to the same site as shopping cart does
                if (option.SKUSiteID != cart.ShoppingCartSiteID)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
