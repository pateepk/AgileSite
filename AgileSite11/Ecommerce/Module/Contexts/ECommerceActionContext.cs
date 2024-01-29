using CMS.Base;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Ecommerce Action context. Ensures context for the actions block.
    /// </summary>
    public sealed class ECommerceActionContext : ExtendedActionContext<ECommerceActionContext>
    {
        #region "Variables"

        /// <summary>
        /// Indicates if system automatically sets the parent product price of the cheapest product variant.
        /// </summary>
        private bool? mSetLowestPriceToParent;

        #endregion


        #region "Public instance properties"

        /// <summary>
        /// Indicates if system automatically set the parent product price of the cheapest product variant.
        /// </summary>
        public bool SetLowestPriceToParent
        {
            set
            {
                // Keep current settings
                StoreOriginalValue(ref OriginalData.mSetLowestPriceToParent, CurrentSetLowestPriceToParent);

                // Ensure requested settings
                CurrentSetLowestPriceToParent = value;
            }
        }

        #endregion


        #region "Public static properties"

        /// <summary>
        /// Indicates if system automatically set the parent product price of the cheapest product variant within the context.
        /// </summary>
        public static bool CurrentSetLowestPriceToParent
        {
            get
            {
                return Current.mSetLowestPriceToParent ?? true;
            }
            private set
            {
                Current.mSetLowestPriceToParent = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Restores the original values to the context
        /// </summary>
        protected override void RestoreOriginalValues()
        {
            // Restore current data context
            var o = OriginalData;

            if (o.mSetLowestPriceToParent.HasValue)
            {
                CurrentSetLowestPriceToParent = o.mSetLowestPriceToParent.Value;
            }

            base.RestoreOriginalValues();
        }

        #endregion
    }
}
