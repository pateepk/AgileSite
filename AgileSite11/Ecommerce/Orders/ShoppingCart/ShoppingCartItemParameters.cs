using System;
using System.Collections;
using System.Collections.Generic;

using CMS.Helpers;
using CMS.Base;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Data container of the parameters which represent configuration of the shopping cart item to be added/updated in the shopping cart object.
    /// </summary>
    public class ShoppingCartItemParameters
    {
        private List<ShoppingCartItemParameters> mProductOptions = null;
        private Hashtable mCustomParameters = null;


        #region "Public properties"

        /// <summary>
        /// Product SKU ID.
        /// </summary>
        public int SKUID
        {
            get;
            set;
        }


        /// <summary>
        /// Number of product units.
        /// </summary>
        public int Quantity
        {
            get;
            set;
        }


        /// <summary>
        /// Product custom price. It is in site main currency.
        /// </summary>
        [Obsolete("This property was intended for donations. Use third-party payment service instead.")]
        public double Price
        {
            get;
            set;
        }


        /// <summary>
        /// Product custom text defined by the customer, e.g. custom label for a T-shirt. Makes sense only for the text product option.
        /// </summary>
        public string Text
        {
            get;
            set;
        }


        /// <summary>
        /// Collection of the product options.
        /// </summary>
        public List<ShoppingCartItemParameters> ProductOptions
        {
            get
            {
                return mProductOptions ?? (mProductOptions = new List<ShoppingCartItemParameters>());
            }
            set
            {
                mProductOptions = value;
            }
        }


        /// <summary>
        /// Collection (name - value pairs) of the shopping cart item custom parameters.
        /// </summary>
        public Hashtable CustomParameters
        {
            get
            {
                return mCustomParameters ?? (mCustomParameters = new Hashtable());
            }
            set
            {
                mCustomParameters = value;
            }
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Returns initialized shopping cart item parameters container from the current URL parameters.
        /// </summary>
        public static ShoppingCartItemParameters GetShoppingCartItemParameters()
        {
            // Get product info
            int skuId = QueryHelper.GetInteger("productid", 0);
            int quantity = QueryHelper.GetInteger("quantity", 0);

            // Get product options
            string[] options = QueryHelper.GetString("options", "").Split(',');
            int[] intOptions = ValidationHelper.GetIntegers(options, 0);

            // Init product parameters
            ShoppingCartItemParameters skuParams = new ShoppingCartItemParameters(skuId, quantity, intOptions);

            // Init product custom parameters
            QueryHelper query = new QueryHelper();
            foreach (string param in query.ColumnNames)
            {
                switch (param.ToLowerCSafe())
                {
                    // Skip default parameters
                    case "productid":
                    case "quantity":
                    case "options":
                    case "aliaspath":
                        break;

                    // Add custom parameters
                    default:
                        skuParams.CustomParameters[param] = query.GetValue(param);
                        break;
                }
            }

            return skuParams;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - creates empty shopping cart item parameters container. 
        /// </summary>
        public ShoppingCartItemParameters()
        {
        }


        /// <summary>
        /// Constructor - creates initialized shopping cart item parameters container.
        /// </summary>
        /// <param name="item">Shopping cart item.</param>
        public ShoppingCartItemParameters(ShoppingCartItemInfo item)
        {
            // Init product
            SKUID = item.SKUID;
            Quantity = item.CartItemUnits;

            // Get item product options without bundle items
            List<ShoppingCartItemInfo> options = item.ProductOptions.FindAll(x => !x.IsBundleItem);
            foreach (ShoppingCartItemInfo option in options)
            {
                ProductOptions.Add(new ShoppingCartItemParameters(option));
            }

            // If item text is set
            if (!String.IsNullOrEmpty(item.CartItemText))
            {
                Text = item.CartItemText;
            }
        }


        /// <summary>
        /// Constructor - creates initialized shopping cart item parameters container.
        /// </summary>
        /// <param name="skuId">Product SKU ID.</param>
        /// <param name="quantity">Number of product units.</param>
        public ShoppingCartItemParameters(int skuId, int quantity)
        {
            // Init product
            SKUID = skuId;
            Quantity = quantity;
        }


        /// <summary>
        /// Constructor - creates initialized shopping cart item parameters container.
        /// </summary>
        /// <param name="skuId">Product SKU ID.</param>
        /// <param name="quantity">Number of product units.</param>
        /// <param name="options">SKU IDs of product options.</param>
        public ShoppingCartItemParameters(int skuId, int quantity, IEnumerable<int> options)
        {
            // Init product
            SKUID = skuId;
            Quantity = quantity;

            // Init product options
            foreach (int option in options)
            {
                if ((option > 0) && ProductHelper.IsOptionAllowed(skuId, option))
                {
                    ProductOptions.Add(new ShoppingCartItemParameters(option, quantity));
                }
            }
        }


        /// <summary>
        /// Constructor - creates initialized shopping cart item parameters container.
        /// </summary>
        /// <param name="skuId">Product SKU ID.</param>
        /// <param name="quantity">Number of product units.</param>
        /// <param name="options">List of product options parameters.</param>
        public ShoppingCartItemParameters(int skuId, int quantity, List<ShoppingCartItemParameters> options)
        {
            // Init product
            SKUID = skuId;
            Quantity = quantity;

            if (options != null)
            {
                // Update quantities
                foreach (ShoppingCartItemParameters option in options)
                {
                    option.Quantity = quantity;
                }
            }

            // Store product options
            ProductOptions = options;
        }

        #endregion
    }
}