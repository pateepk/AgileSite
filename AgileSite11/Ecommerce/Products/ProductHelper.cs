using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.Base;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Provides advanced manipulation with products and product options.
    /// </summary>
    public class ProductHelper : AbstractHelper<ProductHelper>
    {
        #region "Public methods"

        /// <summary>
        /// Makes all options from option category specified by optionCategoryId parameter allowed 
        /// for product given by skuId parameter.
        /// </summary>
        /// <param name="skuId">ID of the product to which options are to be allowed.</param>
        /// <param name="optionCategoryId">ID of the product option category which options are to be allowed.</param>
        public static void AllowAllOptions(int skuId, int optionCategoryId)
        {
            HelperObject.AllowAllOptionsInternal(skuId, optionCategoryId);
        }


        /// <summary>
        /// Allows product options from option category to product. 
        /// Individual options are checked whether they belong to given category.
        /// </summary>
        /// <param name="skuId">ID of the product to allow options for.</param>
        /// <param name="optionCategoryId">ID of the option category to allow options from. Set to 0 to skip options checking.</param>
        /// <param name="optionIds">SKUIDs of product options from option category to be allowed.</param>
        public static void AllowOptions(int skuId, int optionCategoryId, IEnumerable<int> optionIds)
        {
            HelperObject.AllowOptionsInternal(skuId, optionCategoryId, optionIds);
        }


        /// <summary>
        /// Remove product options from option category from product. 
        /// Individual options are checked whether they belong to given category.
        /// </summary>
        /// <param name="skuId">ID of the product to remove options for.</param>
        /// <param name="optionCategoryId">ID of the option category to remove options from. Set to 0 to skip options checking.</param>
        /// <param name="optionIds">SKUIDs of product options from option category to be removed.</param>
        public static void RemoveOptions(int skuId, int optionCategoryId, IEnumerable<int> optionIds)
        {
            HelperObject.RemoveOptionsInternal(skuId, optionCategoryId, optionIds);
        }


        /// <summary>
        /// Removes option category from product. Allowed options are removed too.
        /// </summary>
        /// <param name="skuId">ID of the product to remove option category from.</param>
        /// <param name="optionCategoryId">ID of the option category to be removed.</param>
        public static void RemoveOptionCategory(int skuId, int optionCategoryId)
        {
            HelperObject.RemoveOptionCategoryInternal(skuId, optionCategoryId);
        }


        /// <summary>
        /// Returns true if option specified by optionId is allowed for given product.
        /// </summary>
        /// <param name="productId">SKUID of product to check option for.</param>
        /// <param name="optionId">SKUID of option to check.</param>
        public static bool IsOptionAllowed(int productId, int optionId)
        {
            return HelperObject.IsOptionAllowedInternal(productId, optionId);
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Makes all options from option category specified by optionCategoryId parameter allowed 
        /// for product given by skuId parameter.
        /// </summary>
        /// <param name="skuId">ID of the product to which options are to be allowed.</param>
        /// <param name="optionCategoryId">ID of the product option category which options are to be allowed.</param>
        protected virtual void AllowAllOptionsInternal(int skuId, int optionCategoryId)
        {
            using (var transaction = new CMSTransactionScope())
            {
                // Get SKU option category
                SKUOptionCategoryInfo category = SKUOptionCategoryInfoProvider.GetSKUOptionCategoryInfo(optionCategoryId, skuId);
                if (category != null)
                {
                    // Removes SKUs allowed options from the particular product
                    RemoveAllowedOptionsInternal(skuId, optionCategoryId);

                    // Enable all options
                    category.AllowAllOptions = true;
                    category.Update();

                    transaction.Commit();
                }
                else
                {
                    throw new Exception("[ProductHelper.AllowAllOptions]: No SKUOptionCategoryInfo object found.");
                }
            }
        }


        /// <summary>
        /// Allows product options from option category to product. 
        /// Individual options are checked whether they belong to given category.
        /// </summary>
        /// <param name="skuId">ID of the product to allow options for.</param>
        /// <param name="optionCategoryId">ID of the option category to allow options from.</param>
        /// <param name="optionIds">SKUIDs of product options from option category to be allowed.</param>
        protected virtual void AllowOptionsInternal(int skuId, int optionCategoryId, IEnumerable<int> optionIds)
        {
            // Check whether any options are supplied
            if ((optionIds == null) || (!optionIds.Any()))
            {
                throw new ArgumentException("[ProductHelper.AllowOptions]: No product options to be allowed.");
            }

            using (var transaction = new CMSTransactionScope())
            {
                // Get SKU-option category binding
                SKUOptionCategoryInfo skuCategory = SKUOptionCategoryInfoProvider.GetSKUOptionCategoryInfo(optionCategoryId, skuId);
                if (skuCategory == null)
                {
                    throw new ArgumentException("[ProductHelper.AllowOptions]: Product is not using requested option category. Product option cannot be allowed.");
                }

                // Clear flag allowing all options
                skuCategory.AllowAllOptions = false;
                skuCategory.Update();

                // Add all options to the product
                foreach (int optionId in optionIds)
                {
                    // Select particular option
                    SKUInfo option = SKUInfoProvider.GetSKUInfo(optionId);

                    // Check if option is assigned to the particular category
                    if ((option != null) && (option.SKUOptionCategoryID == optionCategoryId))
                    {
                        // Add product option to product
                        SKUAllowedOptionInfoProvider.AddOptionToProduct(skuId, optionId);
                    }
                    else
                    {
                        throw new ArgumentException("[ProductHelper.AllowOptions]: Option does not exist or is not assigned to the selected option category. Product option cannot be allowed.");
                    }
                }

                transaction.Commit();
            }
        }


        /// <summary>
        /// Remove product options from option category from product. 
        /// Individual options are checked whether they belong to given category.
        /// </summary>
        /// <param name="skuId">ID of the product to allow options for.</param>
        /// <param name="optionCategoryId">ID of the option category to allow options from.</param>
        /// <param name="optionIds">SKUIDs of product options from option category to be allowed.</param>
        protected virtual void RemoveOptionsInternal(int skuId, int optionCategoryId, IEnumerable<int> optionIds)
        {
            // Check whether any options are supplied
            if ((optionIds == null) || (!optionIds.Any()))
            {
                throw new ArgumentException("[ProductHelper.RemoveOptions]: No product options to be allowed.");
            }

            using (var transaction = new CMSTransactionScope())
            {
                // Get SKU-option category binding
                SKUOptionCategoryInfo category = SKUOptionCategoryInfoProvider.GetSKUOptionCategoryInfo(optionCategoryId, skuId);
                if (category == null)
                {
                    throw new ArgumentException("[ProductHelper.RemoveOptions]: Product is not using requested option category. Product options cannot be removed.");
                }

                // Remove options from the product
                foreach (int optionId in optionIds)
                {
                    // Select particular option
                    SKUInfo option = SKUInfoProvider.GetSKUInfo(optionId);

                    // Check if option is assigned to the particular category
                    if ((option != null) && (option.SKUOptionCategoryID == optionCategoryId))
                    {
                        // Remove product option to product
                        SKUAllowedOptionInfoProvider.RemoveOptionFromProduct(skuId, optionId);
                    }
                    else
                    {
                        throw new ArgumentException("[ProductHelper.AllowOptions]: Option does not exist or is not assigned to the selected option category. Product option cannot be allowed.");
                    }
                }

                transaction.Commit();
            }
        }


        /// <summary>
        /// Removes all allowed options from given option category from given product.
        /// </summary>
        /// <param name="skuId">ID of the product to remove options from.</param>
        /// <param name="optionCategoryId">ID of the option category which options are to be removed from product.</param>
        protected virtual void RemoveAllowedOptionsInternal(int skuId, int optionCategoryId)
        {
            using (var transaction = new CMSTransactionScope())
            {
                // Get SKU option category
                var category = SKUOptionCategoryInfoProvider.GetSKUOptionCategoryInfo(optionCategoryId, skuId);
                if (category == null)
                {
                    throw new ArgumentException("[ProductHelper.RemoveAllowedOptions]: Product is not bound to option category.");
                }

                // Get all options from particular option category
                DataSet options = SKUInfoProvider.GetSKUOptions(optionCategoryId, false);
                if (!DataHelper.DataSourceIsEmpty(options))
                {
                    foreach (DataRow option in options.Tables[0].Rows)
                    {
                        // Remove allowed option from given product
                        SKUAllowedOptionInfoProvider.RemoveOptionFromProduct(skuId, ValidationHelper.GetInteger(option["SKUID"], 0));
                    }
                }

                transaction.Commit();
            }
        }


        /// <summary>
        /// Removes option category from product. Allowed options are removed too.
        /// </summary>
        /// <param name="skuId">ID of the product to remove option category from.</param>
        /// <param name="optionCategoryId">ID of the option category to be removed.</param>
        protected virtual void RemoveOptionCategoryInternal(int skuId, int optionCategoryId)
        {
            using (var transaction = new CMSTransactionScope())
            {
                // Remove all allowed options from given option category from given product
                RemoveAllowedOptionsInternal(skuId, optionCategoryId);

                // Remove option category from given product
                SKUOptionCategoryInfoProvider.RemoveOptionCategoryFromSKU(optionCategoryId, skuId);

                transaction.Commit();
            }
        }


        /// <summary>
        /// Returns true if option specified by optionId is allowed for given product.
        /// </summary>
        /// <param name="productId">SKUID of product to check option for.</param>
        /// <param name="optionId">SKUID of option to check.</param>
        protected virtual bool IsOptionAllowedInternal(int productId, int optionId)
        {
            // Look for explicit product-option binding
            if (SKUAllowedOptionInfoProvider.GetSKUOptionInfo(productId, optionId) != null)
            {
                return true;
            }

            // Check if all options are allowed for corresponding option category and product
            SKUInfo option = SKUInfoProvider.GetSKUInfo(optionId);
            if (option != null)
            {
                SKUOptionCategoryInfo category = SKUOptionCategoryInfoProvider.GetSKUOptionCategoryInfo(option.SKUOptionCategoryID, productId);
                if (category != null)
                {
                    return category.AllowAllOptions;
                }
            }

            // This option is not allowed
            return false;
        }

        #endregion
    }
}
