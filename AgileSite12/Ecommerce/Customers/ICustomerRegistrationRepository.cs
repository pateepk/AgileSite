namespace CMS.Ecommerce
{
    /// <summary>
    /// Repository for storing data related to customer registration after checkout process is finished.
    /// </summary>
    public interface ICustomerRegistrationRepository
    {
        /// <summary>
        /// Indicates that customer will be registered after checkout.
        /// </summary>
        bool IsCustomerRegisteredAfterCheckout
        {
            get;
            set;
        }


        /// <summary>
        /// E-mail template code name used for registration after checkout.
        /// </summary>
        string RegisteredAfterCheckoutTemplate
        {
            get;
            set;
        }


        /// <summary>
        /// Clears repository values.
        /// </summary>
        void Clear();
    }
}