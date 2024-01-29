using System.Collections.Generic;
using System.Text;

using CMS.Helpers;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Container for results of cart item checks.
    /// </summary>
    public class ShoppingCartCheckResult
    {
        #region "Variables"

        /// <summary>
        /// Dictionary of ShoppingCartItem check results indexed by the ID of SKU.
        /// </summary>
        protected Dictionary<int, ShoppingCartItemCheckResult> mItemResultsBySKUID = null;


        /// <summary>
        /// List of ShoppingCartItem check results.
        /// </summary>
        protected List<ShoppingCartItemCheckResult> mItemResults = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Results for all products which have failed during check.
        /// </summary>
        public List<ShoppingCartItemCheckResult> ItemResults
        {
            get
            {
                return mItemResults ?? (mItemResults = new List<ShoppingCartItemCheckResult>());
            }
        }


        /// <summary>
        /// Dictionary of ShoppingCartItem check results indexed by the ID of SKU.
        /// </summary>
        protected Dictionary<int, ShoppingCartItemCheckResult> ItemResultsBySKUID
        {
            get
            {
                return mItemResultsBySKUID ?? (mItemResultsBySKUID = new Dictionary<int, ShoppingCartItemCheckResult>());
            }
        }


        /// <summary>
        /// Indicates that shipping option is not available for shopping cart.
        /// </summary>
        public bool ShippingOptionNotAvailable
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether check has failed (whole cart check has failed or at least one cart item check has failed).
        /// </summary>
        public bool CheckFailed
        {
            get
            {
                return ShippingOptionNotAvailable || ItemsCheckFailed;
            }
        }


        /// <summary>
        /// Indicates whether check has failed (at least one cart item check has failed).
        /// </summary>
        public bool ItemsCheckFailed
        {
            get
            {
                return (mItemResults != null) && (ItemResults.Count > 0);
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Adds specified item check result to this cart check result object.
        /// </summary>
        /// <param name="itemResult">Item check result to be added.</param>
        public virtual void AddCartItemResult(ShoppingCartItemCheckResult itemResult)
        {
            // Check parameter
            if ((itemResult == null) || (itemResult.CartItem == null))
            {
                return;
            }

            // Process item only if carrying information about failed check
            if (itemResult.CheckFailed)
            {
                int skuId = itemResult.CartItem.SKUID;

                // Use variant parent SKUID for results that aggregates variants with same parrent
                if (itemResult.CartItem.SKU.IsProductVariant && (itemResult.CartItem.SKU.SKUTrackInventory == TrackInventoryTypeEnum.ByProduct) && (itemResult.CartItem.VariantParent != null))
                {
                    skuId = itemResult.CartItem.VariantParent.SKUID;
                }

                if (ItemResultsBySKUID.ContainsKey(skuId))
                {
                    // Get existing result
                    ShoppingCartItemCheckResult existing = ItemResultsBySKUID[skuId];

                    // Combine existing result with parameter
                    existing.CombineWithResult(itemResult);
                }
                else
                {
                    // Add new item when not found
                    ItemResultsBySKUID.Add(skuId, itemResult);
                    ItemResults.Add(itemResult);
                }
            }
        }


        /// <summary>
        /// Returns check result formatted as string. Message consists of common error message followed by individual item
        /// error messages separated using specified separators. Empty string is returned when no errors found.
        /// </summary>
        /// <param name="itemSeparator">String to be used as a separator between messages from individual items.</param>
        /// <param name="errorSeparator">String to be used as a separator between messages from one item.</param>
        public virtual string GetMessage(string itemSeparator, string errorSeparator)
        {
            if (!CheckFailed)
            {
                return string.Empty;
            }

            // Prepare general message
            var message = new StringBuilder(ResHelper.GetString("com.cart.meetrestrictions"));

            // Compose message when check failed
            if (ShippingOptionNotAvailable)
            {
                message.AppendFormat("{0}{1}", itemSeparator, ResHelper.GetString("com.checkout.shippingoptionnotapplicable"));
            }

            // Append messages for failed items
            foreach (var itemCheckResult in ItemResults)
            {
                message.AppendFormat("{0}{1}", itemSeparator, itemCheckResult.GetMessage(errorSeparator));
            }

            return message.ToString();
        }


        /// <summary>
        /// Returns HTML formatted error message when check has failed, meaning that errors from each item are on its own line using [br /] HTML tag. 
        /// Returns empty string if check passed.
        /// </summary>
        public virtual string GetHTMLFormattedMessage()
        {
            return HTMLHelper.HTMLEncode(GetMessage("|", "|")).Replace("|", "<br />");
        }


        /// <summary>
        /// Returns formatted error message when check has failed, meaning that errors from each item are on its own line using end-of-line char. 
        /// Returns empty string if check passed.
        /// </summary>
        public virtual string GetFormattedMessage()
        {
            return GetMessage("|", "|").Replace("|", "\n\n");
        }

        #endregion
    }
}
