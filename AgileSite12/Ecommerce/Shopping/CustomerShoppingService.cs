using System;

using CMS;
using CMS.Core;
using CMS.Ecommerce;
using CMS.Helpers;
using CMS.Membership;
using CMS.Protection;
using CMS.SiteProvider;

[assembly: RegisterImplementation(typeof(ICustomerShoppingService), typeof(CustomerShoppingService), Priority = RegistrationPriority.SystemDefault, Lifestyle = Lifestyle.Singleton)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// Provides functionality for customer and user management while working with shopping cart.
    /// </summary>
    public class CustomerShoppingService : ICustomerShoppingService
    {
        private readonly ICustomerRegistrationRepositoryFactory mCustomerRegistrationRepositoryFactory;


        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerShoppingService"/> class.
        /// </summary>
        /// <param name="customerRegistrationRepositoryFactory"></param>
        public CustomerShoppingService(ICustomerRegistrationRepositoryFactory customerRegistrationRepositoryFactory)
        {
            mCustomerRegistrationRepositoryFactory = customerRegistrationRepositoryFactory;
        }


        /// <summary>
        /// Creates <see cref="UserInfo"/> for customer, in case following conditions are met:
        /// 1. customer does not have user account yet
        /// 2. site requires customer to have account
        /// 3. user's ip address was not banned
        /// </summary>
        /// <param name="customer">Customer to create <see cref="UserInfo"/> for.</param>
        /// <exception cref="InvalidOperationException">Thrown when user was banned from registration.</exception>
        public void AutoRegisterCustomer(CustomerInfo customer)
        {
            if ((customer == null) || customer.CustomerIsRegistered)
            {
                return;
            }

            var repository = mCustomerRegistrationRepositoryFactory.GetRepository(SiteContext.CurrentSiteID);
            var customerSite = SiteInfoProvider.GetSiteInfo(customer.CustomerSiteID);

            if (repository.IsCustomerRegisteredAfterCheckout)
            {
                // Ban IP addresses which are blocked for registration
                var registrationBan = !BannedIPInfoProvider.IsAllowed(customerSite.SiteName, BanControlEnum.Registration);
                var allUserActionBan = !BannedIPInfoProvider.IsAllowed(customerSite.SiteName, BanControlEnum.AllNonComplete);

                if (registrationBan || allUserActionBan)
                {
                    throw new InvalidOperationException(ResHelper.GetString("banip.ipisbannedregistration"));
                }

                // Auto-register user and send mail notification
                CustomerInfoProvider.RegisterAndNotify(customer, repository.RegisteredAfterCheckoutTemplate);

                repository.Clear();
            }
        }
    }
}
