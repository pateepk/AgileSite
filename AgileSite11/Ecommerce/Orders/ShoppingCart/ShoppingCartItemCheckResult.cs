using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CMS.Helpers;
using CMS.Base;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Class encapsulating the results of cart item check.
    /// </summary>
    public class ShoppingCartItemCheckResult
    {
        #region "Variables"

        private int mInventoryUnits = -1;
        private int mMinUnits = -1;
        private int mMaxUnits = -1;
        private List<string> mOtherErrors = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Affected shopping cart item object.
        /// </summary>
        public ShoppingCartItemInfo CartItem
        {
            get;
            set;
        }


        /// <summary>
        /// List of other (custom) error messages.
        /// </summary>
        public List<string> OtherErrors
        {
            get
            {
                return mOtherErrors ?? (mOtherErrors = new List<string>());
            }
        }


        /// <summary>
        /// Indicates whether result has any other (custom) errors.
        /// </summary>
        public bool HasOtherErrors
        {
            get
            {
                return (mOtherErrors != null) && (OtherErrors.Count > 0);
            }
        }


        /// <summary>
        /// Number of available units when not enough. Value is set to -1 when all requested units are available.
        /// </summary>
        public int InventoryUnits
        {
            get
            {
                return mInventoryUnits;
            }
            protected set
            {
                mInventoryUnits = value;
            }
        }


        /// <summary>
        /// Minimum unit count in one order. Value is set to -1 when minimum units in order condition is met.
        /// </summary>
        public int MinUnits
        {
            get
            {
                return mMinUnits;
            }
            protected set
            {
                mMinUnits = value;
            }
        }


        /// <summary>
        /// Maximum unit count in one order. Value is set to -1 when maximum units in order condition is met.
        /// </summary>
        public int MaxUnits
        {
            get
            {
                return mMaxUnits;
            }
            protected set
            {
                mMaxUnits = value;
            }
        }


        /// <summary>
        /// Indicates that minimum unit count in one order condition is not met.
        /// </summary>
        public bool MinUnitsNotReached
        {
            get
            {
                return MinUnits >= 0;
            }
        }


        /// <summary>
        /// Indicates that maximum unit count in one order condition is not met.
        /// </summary>
        public bool MaxUnitsExceeded
        {
            get
            {
                return MaxUnits >= 0;
            }
        }


        /// <summary>
        /// Indicates that requested unit count is not available.
        /// </summary>
        public bool NotEnoughUnits
        {
            get
            {
                return InventoryUnits >= 0;
            }
        }


        /// <summary>
        /// Indicates that product is disabled.
        /// </summary>
        public bool ProductDisabled
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates that product has expired.
        /// </summary>
        public bool ProductValidityExpired
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates that product can not be purchased, because it requires registered customer.
        /// </summary>
        public bool RegisteredCustomerRequired
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether the check has failed.
        /// </summary>
        public bool CheckFailed
        {
            get
            {
                return NotEnoughUnits ||
                    MinUnitsNotReached ||
                    MaxUnitsExceeded ||
                    ProductDisabled ||
                    ProductValidityExpired ||
                    RegisteredCustomerRequired ||
                    HasOtherErrors;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Includes specified check result into this object in order to find the most restrictive values.
        /// No action is taken when results belong to different SKUs.
        /// </summary>
        /// <param name="result">Result to be included.</param>
        public virtual void CombineWithResult(ShoppingCartItemCheckResult result)
        {
            // If cart items specified, check whether results belong to the same sku
            if ((result.CartItem != null) && (CartItem != null) && (result.CartItem.SKUID != CartItem.SKUID))
            {
                return;
            }

            // Get minimum available count
            InventoryUnits = GetMin(InventoryUnits, result.InventoryUnits);

            // Get the larger minimum count
            MinUnits = GetMax(MinUnits, result.MinUnits);

            // Get the smaller maximum count
            MaxUnits = GetMin(MaxUnits, result.MaxUnits);

            // Apply OR on bool flags
            RegisteredCustomerRequired |= result.RegisteredCustomerRequired;
            ProductDisabled |= result.ProductDisabled;
            ProductValidityExpired |= result.ProductValidityExpired;

            // Append other error messages
            if (result.HasOtherErrors)
            {
                foreach (var err in result.OtherErrors)
                {
                    MarkOtherError(err);
                }
            }
        }


        /// <summary>
        /// Returns check result formatted as string. Message contains individual 
        /// error messages separated with specified separator. Empty string is returned when no errors found.
        /// </summary>
        /// <param name="separator">String to be used as a separator between messages.</param>
        public virtual string GetMessage(string separator)
        {
            if (CheckFailed)
            {
                string skuName = string.Empty;
                StringBuilder error = new StringBuilder();
                var useVariantParentSkuName = false;

                // Get localized SKU name
                if (CartItem?.SKU != null)
                {
                    skuName = ResHelper.LocalizeString(CartItem.SKU.SKUName);
                    useVariantParentSkuName = CartItem.SKU.IsProductVariant && (CartItem.SKU.SKUTrackInventory == TrackInventoryTypeEnum.ByProduct) && (CartItem.VariantParent != null);
                }

                // Return error message for disabled or expired product
                if (ProductDisabled || ProductValidityExpired)
                {
                    error.AppendFormat(separator + ResHelper.GetString("com.inventory.productdisabled"), skuName);
                }

                // Return error message when registered customer is required
                if (RegisteredCustomerRequired)
                {
                    error.AppendFormat(separator + ResHelper.GetString("com.membershipforanonym"), skuName);
                }

                // Use variant parent name for results that aggregates variants with same parrent
                if (useVariantParentSkuName)
                {
                    skuName = ResHelper.LocalizeString(CartItem.VariantParent.SKUName);
                }

                // Return error message if minimum unit not reached
                if (MinUnitsNotReached)
                {
                    error.AppendFormat(separator + ResHelper.GetString("com.inventory.minunitsnotreached"), skuName, MinUnits);
                }

                // Return error message if maximum unit exceeded
                if (MaxUnitsExceeded)
                {
                    error.AppendFormat(separator + ResHelper.GetString("com.inventory.maxunitsexceeded"), skuName, MaxUnits);
                }

                // Return error message when not enough units available
                if (NotEnoughUnits)
                {
                    error.AppendFormat(separator + ResHelper.GetString("com.inventory.notenoughunits"), skuName, InventoryUnits);
                }

                // Append other errors
                if (HasOtherErrors)
                {
                    foreach (string otherError in OtherErrors)
                    {
                        error.Append(separator + otherError);
                    }
                }

                // Remove starting separator
                string errorText = error.ToString();
                if (errorText.StartsWith(separator, StringComparison.Ordinal))
                {
                    errorText = errorText.Remove(0, separator.Length);
                }

                return errorText;
            }

            return string.Empty;
        }


        /// <summary>
        /// Returns formatted result message when check has failed. Message is HTML-formatted: each error on its own line. 
        /// Returns empty string if check passed.
        /// </summary>
        public virtual string GetFormattedMessage()
        {
            return HTMLHelper.HTMLEncode(GetMessage("|")).Replace("|", "<br />");
        }


        /// <summary>
        /// Marks this result as "Minimum unit count not reached".
        /// </summary>
        /// <param name="minUnits">Minimum unit count in one order which was not satisfied.</param>
        public virtual void MarkMinUnitsFailed(int minUnits)
        {
            MinUnits = GetMax(MinUnits, minUnits);
        }


        /// <summary>
        /// Marks this result as "Maximum unit count not reached".
        /// </summary>
        /// <param name="maxUnits">Maximum unit count in one order which was not satisfied.</param>
        public virtual void MarkMaxUnitsFailed(int maxUnits)
        {
            MaxUnits = GetMin(MaxUnits, maxUnits);
        }


        /// <summary>
        /// Marks this result as "Not enough units available".
        /// </summary>
        /// <param name="maxUnits">Maximum available unit count.</param>
        public virtual void MarkNotEnoughUnits(int maxUnits)
        {
            InventoryUnits = GetMin(InventoryUnits, maxUnits);
        }


        /// <summary>
        /// Marks this result with error message. Use this method to report custom errors. Duplicate entries are removed.
        /// </summary>
        /// <param name="errorMessage">Error message to add.</param>
        public virtual void MarkOtherError(string errorMessage)
        {
            if (!OtherErrors.Contains(errorMessage))
            {
                OtherErrors.Add(errorMessage);
            }
        }


        #region "Protected methods"

        /// <summary>
        /// Returns smaller value from params. Values below 0 are considered to be "not specified".
        /// </summary>
        /// <param name="val1">First value</param>
        /// <param name="val2">Second value</param>
        protected int GetMin(int val1, int val2)
        {
            // When val1 not specified, return val2
            if (val1 < 0)
            {
                return val2;
            }

            // When val2 not specified, return val1
            if (val2 < 0)
            {
                return val1;
            }

            // Return smaller value when both specified
            return Math.Min(val1, val2);
        }


        /// <summary>
        /// Returns larger value from params. Values below 0 are considered to be "not specified".
        /// </summary>
        /// <param name="val1">First value</param>
        /// <param name="val2">Second value</param>
        protected int GetMax(int val1, int val2)
        {
            // When val1 not specified, return val2
            if (val1 < 0)
            {
                return val2;
            }

            // When val2 not specified, return val1
            if (val2 < 0)
            {
                return val1;
            }

            // Return larger value when both specified
            return Math.Max(val1, val2);
        }

        #endregion

        #endregion
    }
}
