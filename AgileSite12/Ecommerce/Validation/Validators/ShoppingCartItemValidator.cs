using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Base;
using CMS.Helpers;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Class representing validator used for validation of individual shopping cart items.
    /// </summary>
    public class ShoppingCartItemValidator : IValidator
    {
        private List<IValidationError> errors;


        /// <summary>
        /// Gets shopping cart this validator was initialized for.
        /// </summary>
        public ShoppingCartItemInfo Item
        {
            get;
        }


        /// <summary>
        /// Gets an enumeration of validation errors associated with this validator. An empty enumeration is returned
        /// if validation succeeded.
        /// </summary>
        public IEnumerable<IValidationError> Errors => errors ?? Enumerable.Empty<IValidationError>();


        /// <summary>
        /// Gets a value indicating whether validation succeeded.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when <see cref="Validate"/> has not been called yet.</exception>
        public bool IsValid => errors != null ? errors.Count == 0 : throw new InvalidOperationException($"The {nameof(Validate)}() method must be called prior to accessing the {nameof(IsValid)} property.");


        /// <summary>
        /// Initializes a new instance of the <see cref="ShoppingCartItemValidator"/> class for given <paramref name="item"/>.
        /// </summary>
        /// <param name="item">Shopping cart item for which to perform the validation.</param>
        public ShoppingCartItemValidator(ShoppingCartItemInfo item)
        {
            Item = item;
        }


        /// <summary>
        /// Validates individual shopping cart item.
        /// The following conditions must hold true for each item in cart for validation to pass:
        /// 1) All shopping cart items are enabled
        /// 2) Max units in one order are not exceeded
        /// 3) There is enough units in the inventory
        /// 4) Customer is registered, if there is a membership type product in the cart
        /// 5) Product validity is valid, if there is a membership or e-product type product in the cart.
        /// </summary>
        /// <returns>Returns true if validation passed, otherwise returns false.</returns>
        public virtual bool Validate()
        {
            errors = new List<IValidationError>();

            var skuName = String.Empty;
            var useVariantParentSkuName = false;

            // Do not process the check
            if (Item?.ShoppingCart == null || (Item.SKU == null))
            {
                return true;
            }

            if (Item?.SKU != null)
            {
                skuName = Item.SKU.SKUName;
                useVariantParentSkuName = Item.SKU.IsProductVariant && (Item.SKU.SKUTrackInventory == TrackInventoryTypeEnum.ByProduct) && (Item.VariantParent != null);
            }

            // Get up-to-date SKU due to possible inventory change by other users
            SKUInfo sku = SKUInfoProvider.GetSKUInfo(Item.SKU.SKUID);
            if (!ValidateSKUIsEnabled(sku))
            {
                errors.Add(new SKUDisabledOrExpiredValidationError(sku.SKUID, skuName));
            }

            // Check registered customer for membership product
            if (!ValidateCustomerMembership(sku))
            {
                errors.Add(new RegisteredCustomerRequiredValidationError(sku.SKUID, skuName));
            }

            // Use variant parent name for results that aggregates variants with same parent
            if (useVariantParentSkuName)
            {
                skuName = Item.VariantParent.SKUName;
            }


            // Min and max units in one order limitations
            if (!ValidateMinCartUnits(sku))
            {
                errors.Add(new SKUMinUnitsNotReachedValidationError(sku.SKUID, skuName, sku.SKUMinItemsInOrder));
            }

            if (!ValidateMaxCartUnits(sku))
            {
                errors.Add(new SKUMaxUnitsExceededValidationError(sku.SKUID, skuName, sku.SKUMaxItemsInOrder));
            }

            // Sum of all units of the SKU in the edited order
            int orderUnits = ShoppingCartItemInfoProvider.GetSKUOrderUnits(Item);
            // SKU available units
            int itemUnits = (sku.SKUAvailableItems > 0) ? sku.SKUAvailableItems : 0;
            // Sum of all available units
            int inventoryUnits = orderUnits + itemUnits;

            if (!ValidateUnitsInStock(sku, inventoryUnits))
            {
                errors.Add(new SKUNotInStockValidationError(sku.SKUID, skuName, inventoryUnits));
            }

            return IsValid;
        }


        /// <summary>
        /// Validates that SKU is enabled and not expired.
        /// </summary>
        /// <param name="sku">SKU to be validated.</param>
        /// <returns>Result of validation</returns>
        protected virtual bool ValidateSKUIsEnabled(SKUInfo sku)
        {
            // Check whether the product is enabled
            var productEnabled = sku.SKUEnabled;

            // Check whether parent of variant is enabled in case SKU is variant and it is enabled
            if (productEnabled && sku.IsProductVariant)
            {
                if (Item.VariantParent != null)
                {
                    productEnabled = Item.VariantParent.SKUEnabled;
                }
            }

            // Check validity for e-products and memberships
            if ((sku.SKUValidUntil != DateTimeHelper.ZERO_TIME) &&
                (sku.SKUValidity == ValidityEnum.Until) &&
                (sku.SKUValidUntil < DateTime.Now) &&
                ((sku.SKUProductType == SKUProductTypeEnum.EProduct) ||
                    (sku.SKUProductType == SKUProductTypeEnum.Membership)))
            {
                return false;
            }

            return productEnabled;
        }


        /// <summary>
        /// Validates that customer is either a member or membership is not required.
        /// </summary>
        /// <param name="sku">SKU to be validated.</param>
        /// <returns>Returns false if membership is required and customer is not a member, otherwise return true.</returns>
        protected virtual bool ValidateCustomerMembership(SKUInfo sku)
        {
            if (sku.SKUProductType == SKUProductTypeEnum.Membership)
            {
                ShoppingCartInfo cart = Item.ShoppingCart;
                // Public user is unable to purchase membership. Ignore this check for non-existing customer = steps before the one with customer detail
                if ((cart.Customer != null) && (cart.ShoppingCartUserID <= 0))
                {
                    return false;
                }
            }

            return true;
        }


        /// <summary>
        /// Validates that cart contains at least required amount of units.
        /// </summary>
        /// <param name="sku">SKU to be validated.</param>
        /// <returns>Result of validation</returns>
        protected virtual bool ValidateMinCartUnits(SKUInfo sku)
        {
            int minUnits = sku.SKUMinItemsInOrder;
            // Sum of all units of the SKU in the shopping cart
            int cartUnits = ShoppingCartItemInfoProvider.GetSKUShoppingCartUnits(Item);

            if (!Item.IsBundleItem ||
                ((Item.ParentBundle?.SKU != null) && (Item.ParentBundle.SKU.SKUBundleInventoryType != BundleInventoryTypeEnum.RemoveBundle)))
            {
                // Check max units
                if ((minUnits > 0) && (cartUnits < minUnits))
                {
                    return false;
                }
            }

            return true;
        }


        /// <summary>
        /// Validates that cart does not contain more items than allowed.
        /// </summary>
        /// <param name="sku">SKU to be validated.</param>
        /// <returns>Result of validation</returns>
        protected virtual bool ValidateMaxCartUnits(SKUInfo sku)
        {
            int maxUnits = sku.SKUMaxItemsInOrder;
            // Sum of all units of the SKU in the shopping cart
            int cartUnits = ShoppingCartItemInfoProvider.GetSKUShoppingCartUnits(Item);

            // Ignore "Min/Max in order" check for bundle Items with "remove only bundle" inventory type
            if (!Item.IsBundleItem ||
                ((Item.ParentBundle?.SKU != null) && (Item.ParentBundle.SKU.SKUBundleInventoryType != BundleInventoryTypeEnum.RemoveBundle)))
            {
                // Check max units
                if ((maxUnits > 0) && (cartUnits > maxUnits))
                {
                    return false;
                }
            }

            return true;
        }


        /// <summary>
        /// Validates that cart does not contain more items than are currently in stock.
        /// </summary>
        /// <param name="sku">SKU to be validated.</param>
        /// <param name="inventoryUnits">Units currently in inventory</param>
        /// <returns>Result of validation</returns>
        protected virtual bool ValidateUnitsInStock(SKUInfo sku, int inventoryUnits)
        {
            // Check inventory
            bool checkInventory = false;

            if (sku.SKUProductType == SKUProductTypeEnum.Bundle)
            {
                if ((sku.SKUBundleInventoryType == BundleInventoryTypeEnum.RemoveBundle) ||
                    (sku.SKUBundleInventoryType == BundleInventoryTypeEnum.RemoveBundleAndProducts))
                {
                    // Check for bundle
                    checkInventory = sku.SKUSellOnlyAvailable;
                }
            }
            else if (Item.IsBundleItem)
            {
                if (Item.ParentBundle?.SKU != null)
                {
                    BundleInventoryTypeEnum ItemInventory = Item.ParentBundle.SKU.SKUBundleInventoryType;

                    if ((ItemInventory == BundleInventoryTypeEnum.RemoveProducts) ||
                        (ItemInventory == BundleInventoryTypeEnum.RemoveBundleAndProducts))
                    {
                        // Check for bundle Item
                        checkInventory = sku.SKUSellOnlyAvailable;
                    }
                }
            }
            else
            {
                // Check for all other product types
                checkInventory = sku.SKUSellOnlyAvailable;
            }

            // Check available Items
            if (checkInventory)
            {
                int cartUnits = ShoppingCartItemInfoProvider.GetSKUShoppingCartUnits(Item);

                if (cartUnits > inventoryUnits)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
