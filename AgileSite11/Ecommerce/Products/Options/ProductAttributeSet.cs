using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CMS.Helpers;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Product attributes set with enhanced functionality
    /// </summary>
    public class ProductAttributeSet : IEnumerable<SKUInfo>
    {
        #region "Private variables"

        private List<SKUInfo> mProductAttributes = null;
        private List<int> mProductAttributeIds = null;

        #endregion


        #region "Internal properties"

        /// <summary>
        /// Gets or sets the product options.
        /// </summary>
        /// <value>
        /// The product options.
        /// </value>
        internal List<SKUInfo> ProductAttributes
        {
            get
            {
                if (mProductAttributes == null)
                {
                    FillProductAttributes();
                }
                return mProductAttributes;
            }
            set
            {
                mProductAttributes = value;
            }
        }


        /// <summary>
        /// Gets the product option IDs.
        /// </summary>
        /// <value>
        /// The product option IDs.
        /// </value>
        internal IEnumerable<int> ProductAttributeIDs
        {
            get
            {
                if (mProductAttributes == null)
                {
                    // Only copy, we don't want external code to ruin internal structure
                    return mProductAttributeIds.ToList();
                }

                return mProductAttributes.Select(po => po.SKUID);
            }
        }


        /// <summary>
        /// Gets the unique compare code.
        /// </summary>
        /// <value>
        /// The compare code.
        /// </value>
        internal string GetCompareCode
        {
            get
            {
                List<int> optionIds = ProductAttributeIDs.ToList();
                optionIds.Sort();

                return TextHelper.Join("|", optionIds);
            }
        }

        #endregion


        #region "Public properties"

        /// <summary>
        /// Gets the SKUInfo with the specified position.
        /// </summary>
        /// <value>
        /// The SKUInfo.
        /// </value>
        /// <param name="index">The position.</param>
        /// <returns>SKUInfo with the specified position.</returns>
        public SKUInfo this[int index]
        {
            get
            {
                return ProductAttributes[index];
            }
            private set
            {
                ProductAttributes[index] = value;
            }
        }


        /// <summary>
        /// Gets the count of product options in set.
        /// </summary>
        /// <value>
        /// The count of product options in set.
        /// </value>
        public int Count
        {
            get
            {
                return ProductAttributes.Count;
            }
        }


        /// <summary>
        /// Gets the category IDs for specified product options.
        /// </summary>
        /// <value>
        /// The category IDs.
        /// </value>
        public IEnumerable<int> CategoryIDs
        {
            get
            {
                return ProductAttributes.Select(po => po.SKUOptionCategoryID).Distinct();
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Initializes a new instance of the ProductAttributeSet class.
        /// </summary>
        /// <param name="productAttributes">The product options.</param>
        public ProductAttributeSet(params SKUInfo[] productAttributes)
        {
            // Check if it is an option?
            mProductAttributes = new List<SKUInfo>(productAttributes);
        }


        /// <summary>
        /// Initializes a new instance of the ProductAttributeSet class.
        /// </summary>
        /// <param name="productAttributeIds">The product option IDs.</param>
        public ProductAttributeSet(params int[] productAttributeIds)
        {
            mProductAttributeIds = new List<int>(productAttributeIds);
        }


        /// <summary>
        /// Initializes a new instance of the ProductAttributeSet class.
        /// </summary>
        /// <param name="productAttributeIds">The product option IDs.</param>
        public ProductAttributeSet(IEnumerable<int> productAttributeIds)
        {
            mProductAttributeIds = new List<int>(productAttributeIds);
        }

        /// <summary>
        /// Initializes a new instance of the ProductAttributeSet class.
        /// </summary>
        internal ProductAttributeSet()
        {
            mProductAttributeIds = new List<int>();
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Adds the specified attribute product option.
        /// </summary>
        /// <param name="productAttribute">The product option.</param>
        internal void Add(SKUInfo productAttribute)
        {
            // Check if it is attribute option            
            if (IsProductAttributeAttribute(productAttribute))
            {
                // Add to list
                ProductAttributes.Add(productAttribute);
            }
            else
            {
                throw new Exception("[ProductAttributeSet.Add]: SKUInfo is not an attribute product option.");
            }
        }


        /// <summary>
        /// Adds the specified (via ID) product option.
        /// </summary>
        /// <param name="productAttributeId">The product option ID.</param>
        internal void Add(int productAttributeId)
        {
            if (mProductAttributes == null)
            {
                mProductAttributeIds.Add(productAttributeId);
            }
            else
            {
                // Load SKU via ID
                SKUInfo productAttribute = SKUInfoProvider.GetSKUInfo(productAttributeId);

                // Add this option
                Add(productAttribute);
            }
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Fills the product options.
        /// </summary>        
        private void FillProductAttributes()
        {
            mProductAttributes = new List<SKUInfo>();

            foreach (int id in mProductAttributeIds)
            {
                SKUInfo pOption = SKUInfoProvider.GetSKUInfo(id);

                if (IsProductAttributeAttribute(pOption))
                {
                    mProductAttributes.Add(pOption);
                }
            }
        }


        /// <summary>
        /// Determines whether is specified product option an attribute.
        /// </summary>
        /// <param name="productAttribute">The product option.</param>
        /// <returns>
        /// True if specified product option is an attribute; otherwise, false.
        /// </returns>
        private bool IsProductAttributeAttribute(SKUInfo productAttribute)
        {
            OptionCategoryInfo category = null;

            if (productAttribute != null)
            {
                // Check if it is attribute option
               category = ((OptionCategoryInfo)productAttribute.Parent);
            }

            return ((category != null) && category.GetStringValue("CategoryType", string.Empty).Equals("ATTRIBUTE", StringComparison.Ordinal));
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Determines whether this ProductAttributeSet [contains] [the specified product option].
        /// </summary>
        /// <param name="productAttribute">The product option.</param>
        /// <returns>
        ///   True if this ProductAttributeSet [contains] [the specified product option]; otherwise, false.
        /// </returns>
        public bool Contains(SKUInfo productAttribute)
        {
            return Contains(productAttribute.SKUID);
        }


        /// <summary>
        /// Determines whether this ProductAttributeSet [contains] [the specified product option].
        /// </summary>
        /// <param name="productAttributeId">The product option ID.</param>        
        public bool Contains(int productAttributeId)
        {
            return ProductAttributeIDs.Contains(productAttributeId);
        }


        /// <summary>
        /// Determines whether this ProductAttributeSet [contains] [the specified product options].
        /// </summary>
        /// <param name="productAttributeIds">The product option IDs.</param>       
        public bool Contains(IEnumerable<int> productAttributeIds)
        {
            return productAttributeIds.All(id => ProductAttributeIDs.Contains(id));
        }

        #endregion


        #region IEnumerable

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        public IEnumerator<SKUInfo> GetEnumerator()
        {
            return ProductAttributes.GetEnumerator();
        }


        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>        
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
