using System;
using System.Collections.Generic;
using System.Linq;
using CMS.Base;
using CMS.Helpers;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Represents one line of the shopping cart content.
    /// </summary>
    public class ShoppingCartLine : IDataContainer
    {
        private readonly ShoppingCartItemInfo mItem;

        private readonly Dictionary<string, Func<ShoppingCartLine, object>> mValueAccessor = new Dictionary<string, Func<ShoppingCartLine, object>>(StringComparer.InvariantCultureIgnoreCase)
        {
            { "SKUProductType", i => i.SKUProductType },
            { "SKUSiteID", i => i.SKUSiteID },
            { "SKUParentSKUID", i => i.SKUParentSKUID},
            { "SKUName", i => i.SKUName },
            { "SKUNumber", i => i.SKUNumber},
            { "SKUImagePath", i => i.SKUImagePath},
            { "SKUID", i => i.SKUID },
            { "CartItemID", i => i.CartItemID },
            { "CartItemGUID", i => i.CartItemGUID},
            { "CartItemParentGUID", i => i.CartItemParentGUID},
            { "Units", i => i.Units },
            { "IsProductOption", i => i.IsProductOption},
            { "IsAccessoryProduct", i => i.IsAccessoryProduct },
            { "CartItemText", i => i.CartItemText },
            { "UnitPrice", i => i.UnitPrice },
            { "TotalPrice", i => i.TotalPrice },
            { "DiscountSummary", i => i.DiscountSummary },
            { "UnitTotalDiscount", i => i.UnitTotalDiscount},
            { "TotalDiscount", i => i.TotalDiscount },
            { "UnitWeight", i => i.UnitWeight },
            { "TotalWeight", i => i.TotalWeight }
        };



        /// <summary>
        /// Creates a new instance of the <see cref="ShoppingCartLine"/>.
        /// </summary>
        /// <param name="item">Cart item to create the line from.</param>
        public ShoppingCartLine(ShoppingCartItemInfo item)
        {
            mItem = item;
        }


        /// <summary>
        /// The type of the product.
        /// </summary>
        public string SKUProductType => mItem.SKU.SKUProductType.ToStringRepresentation();


        /// <summary>
        /// SKU site ID.
        /// </summary>
        public int SKUSiteID => mItem.SKU.SKUSiteID;


        /// <summary>
        /// ID of the parent SKU.
        /// </summary>
        public int SKUParentSKUID => mItem.SKU.SKUParentSKUID;


        /// <summary>
        /// Product name.
        /// </summary>
        public string SKUName => mItem.SKU.SKUName;


        /// <summary>
        /// Product number.
        /// </summary>
        public string SKUNumber => mItem.SKU.SKUNumber;


        /// <summary>
        /// Product image path.
        /// </summary>
        public string SKUImagePath => mItem.SKU.SKUImagePath;


        /// <summary>
        /// SKU ID.
        /// </summary>
        public int SKUID => mItem.SKUID;


        /// <summary>
        /// Shopping cart item ID.
        /// </summary>
        public int CartItemID => mItem.CartItemID;


        /// <summary>
        /// Shopping cart item GUID.
        /// </summary>
        public Guid CartItemGUID => mItem.CartItemGUID;


        /// <summary>
        /// Shopping cart parent GUID.
        /// </summary>
        public Guid CartItemParentGUID => mItem.CartItemParentGUID;


        /// <summary>
        /// Quantity of the item.
        /// </summary>
        public int Units => mItem.CartItemUnits;


        /// <summary>
        /// Indicates if this item represent a product option.
        /// </summary>
        public bool IsProductOption => mItem.IsProductOption;


        /// <summary>
        /// Indicates if this item represent an accessory item.
        /// </summary>
        public bool IsAccessoryProduct => mItem.IsAccessoryProduct;


        /// <summary>
        /// Text of the text product option.
        /// </summary>
        public string CartItemText => mItem.CartItemText;


        /// <summary>
        /// Unit price of the item.
        /// </summary>
        public decimal UnitPrice => mItem.UnitPrice;


        /// <summary>
        /// Total price of the item.
        /// </summary>
        public decimal TotalPrice => mItem.TotalPrice;


        /// <summary>
        /// Summary of the discounts applied on this item.
        /// </summary>
        public ValuesSummary DiscountSummary => mItem.DiscountSummary;


        /// <summary>
        /// Unit total discount of the item.
        /// </summary>
        public decimal UnitTotalDiscount => mItem.UnitTotalDiscount;


        /// <summary>
        /// Total discount of the item.
        /// </summary>
        public decimal TotalDiscount => mItem.TotalDiscount;


        /// <summary>
        /// Unit weight of the item.
        /// </summary>
        public double UnitWeight => mItem.UnitWeight;


        /// <summary>
        /// Total weight of the item.
        /// </summary>
        public double TotalWeight => mItem.TotalWeight;


        /// <summary>
        /// Item's property names.
        /// </summary>
        public virtual List<string> ColumnNames => mValueAccessor.Keys.ToList();


        /// <summary>
        /// Gets or sets the value of the column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public object this[string columnName]
        {
            get
            {
                return GetValue(columnName);
            }
            set
            {
                SetValue(columnName, value);
            }
        }


        /// <summary>
        /// Gets the value of column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public object GetValue(string columnName)
        {
            object value;
            TryGetValue(columnName, out value);
            return value;
        }


        /// <summary>
        /// Sets value of column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Column value</param> 
        public bool SetValue(string columnName, object value)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Returns value of column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Returns the value</param>
        /// <returns>Returns true if the operation was successful (the value was present)</returns>
        public virtual bool TryGetValue(string columnName, out object value)
        {
            if (mValueAccessor.ContainsKey(columnName))
            {
                value = mValueAccessor[columnName](this);
                return true;
            }

            value = null;
            return false;
        }


        /// <summary>
        /// Returns true if the object contains specified column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public virtual bool ContainsColumn(string columnName)
        {
            return ColumnNames.Contains(columnName);
        }
    }
}