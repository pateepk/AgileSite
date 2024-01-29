using System;
using System.Data;

using CMS.Helpers;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Envelope form variant-type SKU object, that provides advanced methods a features.
    /// </summary>
    public class ProductVariant
    {
        #region "Private variables"

        private SKUInfo mProductVariant;
        private ProductAttributeSet mProductAttributes;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Gets a value indicating whether this ProductVariant is already saved (exists) in database.
        /// </summary>
        /// <value>
        ///   True if already saved (exists) in database; otherwise, false.
        /// </value>
        public bool Existing
        {
            get
            {
                return mProductVariant.SKUID != 0;
            }
        }


        /// <summary>
        /// Gets the product variant object.
        /// </summary>
        public SKUInfo Variant
        {
            get
            {
                return mProductVariant;
            }
            private set
            {
                mProductVariant = value;
            }
        }


        /// <summary>
        /// Gets the parent product ID of variant.
        /// </summary>        
        public int ParentProductID
        {
            get
            {
                return mProductVariant?.SKUParentSKUID ?? 0;
            }
        }


        /// <summary>
        /// Gets the product options of this variant.
        /// </summary>
        public ProductAttributeSet ProductAttributes
        {
            get
            {
                return mProductAttributes;
            }
            private set
            {
                mProductAttributes = value;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Initializes a new instance of the ProductVariant class.
        /// </summary>
        /// <param name="productId">The product ID.</param>
        /// <param name="productAttributes">The product options.</param>
        public ProductVariant(int productId, ProductAttributeSet productAttributes)
        {
            mProductAttributes = productAttributes;

            CreateVariant(productId);
            VariantHelper.RegenerateSKUNameAndNumber(this);
        }


        /// <summary>
        /// Initializes a new instance of the ProductVariant class.
        /// </summary>
        /// <param name="variantId">The variant ID.</param>
        public ProductVariant(int variantId)
        {
            mProductVariant = SKUInfoProvider.GetSKUInfo(variantId);

            DataSet variantOptionRelations = VariantOptionInfoProvider.GetVariantOptions().WhereEquals("VariantSKUID", variantId);

            mProductAttributes = new ProductAttributeSet();

            if (!DataHelper.DataSourceIsEmpty(variantOptionRelations))
            {
                foreach (DataRow dr in variantOptionRelations.Tables[0].Rows)
                {
                    int optionId = ValidationHelper.GetInteger(dr["OptionSKUID"], 0);
                    mProductAttributes.Add(optionId);
                }
            }
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Saves this instance of product variant with specified product options to database.
        /// </summary>
        public void Set()
        {
            VariantHelper.SetProductVariant(this);
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Creates the variant.
        /// </summary>
        /// <param name="productId">The product ID.</param>
        /// <exception cref="System.Exception">[ProductVariant.CreateVariant]: Specified product (by ID) does not exist in database.</exception>
        private void CreateVariant(int productId)
        {

            SKUInfo parentProduct = SKUInfoProvider.GetSKUInfo(productId);

            if (parentProduct == null)
            {
                throw new Exception("[ProductVariant.CreateVariant]: Specified product (by ID) does not exist in database.");
            }

            mProductVariant = new SKUInfo
            {
                SKUParentSKUID = parentProduct.SKUID,
                SKUName = parentProduct.SKUName,
                SKUNumber = parentProduct.SKUNumber,
                SKUPrice = parentProduct.SKUPrice,
                SKUEnabled = true,
                SKUSiteID = parentProduct.SKUSiteID,
                SKUProductType =
                parentProduct.SKUProductType
            };
        }

        #endregion
    }
}
