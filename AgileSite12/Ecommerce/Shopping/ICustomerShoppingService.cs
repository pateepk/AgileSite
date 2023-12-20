using CMS.Membership;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Provides interface for customer and user management while working with shopping cart.
    /// </summary>
    public interface ICustomerShoppingService
    {
        /// <summary>
        /// Creates <see cref="UserInfo"/> for specified customer.
        /// </summary>
        /// <param name="customer">Customer to create <see cref="UserInfo"/> for.</param>
        void AutoRegisterCustomer(CustomerInfo customer);
    }
}
