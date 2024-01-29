namespace CMS.Ecommerce
{
    /// <summary>
    /// Type of checkout process.
    /// </summary>
    public enum CheckoutProcessEnum
    {
        /// <summary>
        /// Custom checkout process.
        /// </summary>
        Custom = 0,

        /// <summary>
        /// Checkout process for the live site.
        /// </summary>
        LiveSite = 1,

        /// <summary>
        /// Checkout process for the CMSDesk order section.
        /// </summary>
        CMSDeskOrder = 2,

        /// <summary>
        /// Checkout process for the CMSDesk order items section.
        /// </summary>
        CMSDeskOrderItems = 3,

        /// <summary>
        /// Checkout process for the CMSDesk customer section.
        /// </summary>
        CMSDeskCustomer = 4
    }
}