using System;

using CMS.Base;
using CMS.Membership;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Helper class that helps with customer manipulation. 
    /// </summary>
    public class CustomerHelper : AbstractHelper<CustomerHelper>
    {
        /// <summary>
        /// Maps given <paramref name="user"/> to new <see cref="CustomerInfo"/>.
        /// </summary>
        /// <param name="user"><see cref="UserInfo"/> for mapping.</param>
        /// <returns><see cref="CustomerInfo"/> mapped from <paramref name="user"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="user"/> is null.</exception>
        public static CustomerInfo MapToCustomer(UserInfo user)
        {
            return HelperObject.MapToCustomerInternal(user);
        }


        /// <summary>
        /// Maps given <paramref name="user"/> to new <see cref="CustomerInfo"/>.
        /// </summary>
        /// <param name="user"><see cref="UserInfo"/> for mapping.</param>
        /// <returns><see cref="CustomerInfo"/> mapped from <paramref name="user"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="user"/> is null.</exception>
        protected virtual CustomerInfo MapToCustomerInternal(UserInfo user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return new CustomerInfo
            {
                CustomerUserID = user.UserID,
                CustomerEmail = user.Email,
                CustomerFirstName = user.FirstName,
                CustomerLastName = user.LastName,
                CustomerPhone = user.UserSettings.UserPhone,
            };
        }
    }
}
