using System;


namespace CMS.Ecommerce
{
    /// <summary>
    /// Default implementation of <see cref="ITaxClassService"/> interface.
    /// </summary>
    public class TaxClassService : ITaxClassService
    {
        /// <summary>
        /// Gets a tax class to which the shipping option belongs based on <see cref="ShippingOptionInfo.ShippingOptionTaxClassID"/> property. 
        /// </summary>
        /// <param name="shipping">A shipping option to get tax class for.</param>
        /// <returns><see cref="TaxClassInfo"/> for specified shipping option or <c>null</c> when not found.</returns>
        public TaxClassInfo GetTaxClass(ShippingOptionInfo shipping)
        {
            if (shipping == null)
            {
                throw new ArgumentNullException(nameof(shipping));
            }

            return GetShippingTaxClass(shipping);
        }


        /// <summary>
        /// Gets a tax class to which the product belongs.
        /// </summary>
        /// <remarks>
        /// <see cref="SKUInfo.SKUTaxClassID"/> property is used as product/accessory tax class identifier. 
        /// The same property of the parent product is used for product variants.
        /// </remarks>
        /// <param name="product">A product, product variant or product accessory.</param>
        /// <returns><see cref="TaxClassInfo"/> for specified product or <c>null</c> when not found.</returns>
        public TaxClassInfo GetTaxClass(SKUInfo product)
        {
            if (product == null)
            {
                throw new ArgumentNullException(nameof(product));
            }

            if (product.IsProductVariant)
            {
                return GetVariantTaxClass(product);
            }

            if (product.IsProduct)
            {
                return GetProductTaxClass(product);
            }

            if (product.IsAccessoryProduct)
            {
                return GetAccessoryTaxClass(product);
            }

            throw new InvalidOperationException("Only products, product variants and accessory options are supported.");
        }


        /// <summary>
        /// Gets a tax class for given product variant.
        /// </summary>
        /// <param name="variant">Product variant.</param>
        /// <returns><see cref="TaxClassInfo"/> for specified variant or <c>null</c> when not found.</returns>
        protected virtual TaxClassInfo GetVariantTaxClass(SKUInfo variant)
        {
            var product = SKUInfoProvider.GetSKUInfo(variant.SKUParentSKUID);
            if (product != null)
            {
                return GetProductTaxClass(product);
            }

            return null;
        }


        /// <summary>
        /// Gets a tax class for given (main) product.
        /// </summary>
        /// <param name="product">Main product.</param>
        /// <returns><see cref="TaxClassInfo"/> for specified product or <c>null</c> when not found.</returns>
        protected virtual TaxClassInfo GetProductTaxClass(SKUInfo product)
        {
            return GetSKUTaxClass(product);
        }


        /// <summary>
        /// Gets a tax class for given product variant.
        /// </summary>
        /// <param name="accessory">Product accessory.</param>
        /// <returns><see cref="TaxClassInfo"/> for specified product accessory or <c>null</c> when not found.</returns>
        protected virtual TaxClassInfo GetAccessoryTaxClass(SKUInfo accessory)
        {
            return GetSKUTaxClass(accessory);
        }


        /// <summary>
        /// Gets a tax class for given sku.
        /// </summary>
        /// <param name="sku">Product variant.</param>
        /// <returns><see cref="TaxClassInfo"/> for specified product or <c>null</c> when not found.</returns>
        protected TaxClassInfo GetSKUTaxClass(SKUInfo sku)
        {
            return TaxClassInfoProvider.GetTaxClassInfo(sku.SKUTaxClassID);
        }


        /// <summary>
        /// Gets a tax class for given shipping option.
        /// </summary>
        /// <param name="shipping">Shipping option.</param>
        /// <returns><see cref="TaxClassInfo"/> for specified shipping option or <c>null</c> when not found.</returns>
        protected TaxClassInfo GetShippingTaxClass(ShippingOptionInfo shipping)
        {
            return TaxClassInfoProvider.GetTaxClassInfo(shipping.ShippingOptionTaxClassID);
        }
    }
}
