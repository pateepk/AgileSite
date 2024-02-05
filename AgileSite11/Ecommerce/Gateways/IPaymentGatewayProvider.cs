namespace CMS.Ecommerce
{
    /// <summary>
    /// General payment gateway provider implementation.
    /// </summary>
    public interface IPaymentGatewayProvider
    {
        /// <summary>
        /// Sets existing <see cref="OrderInfo"/> ID.
        /// </summary>
        int OrderId
        {
            set;
        }


        /// <summary>
        /// Returns whether both gateway and provider shall use delayed payment method.
        /// </summary>
        bool UseDelayedPayment();
    }
}