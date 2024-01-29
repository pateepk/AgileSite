using System;

using CMS.Ecommerce;
using CMS.SiteProvider;
using CMS.Membership;
using CMS.Base;
using CMS.Globalization;
using CMS.Newsletters;
using CMS.ContactManagement;


namespace APIExamples
{
    /// <summary>
    /// Holds customer API examples.
    /// </summary>
    /// <pageTitle>Customers</pageTitle>
    internal class CustomersMain
    {
        /// <summary>
        /// Holds customer API examples.
        /// </summary>
        /// <groupHeading>Customers</groupHeading>
        private class Customers
        {
            /// <heading>Creating an anonymous customer</heading>
            private void CreateAnonymousCustomer()
            {
                // Creates a new customer object
                CustomerInfo newCustomer = new CustomerInfo
                {
                    // Sets the customer properties
                    CustomerFirstName = "Joe",
                    CustomerLastName = "Smith",
                    CustomerEmail = "smith@localhost.local",
                    CustomerSiteID = SiteContext.CurrentSiteID
                };

                // Saves the anonymous customer to the database
                CustomerInfoProvider.SetCustomerInfo(newCustomer);
            }


            /// <heading>Creating a registered customer</heading>
            private void CreateRegisteredCustomer()
            {
                // Creates a new user
                UserInfo newUser = new UserInfo
                {
                    // Sets the user properties
                    UserName = "Smith",
                    UserEnabled = true
                };

                // Sets the user's privilege level
                newUser.SiteIndependentPrivilegeLevel = UserPrivilegeLevelEnum.Editor;

                // Saves the user to the database
                UserInfoProvider.SetUserInfo(newUser);

                // Assigns the user to the current site
                UserInfoProvider.AddUserToSite(newUser.UserName, SiteContext.CurrentSiteName);

                // Creates a new customer object
                CustomerInfo newCustomer = new CustomerInfo
                {
                    // Sets the customer properties (e.g. assigns the customer to the user)
                    CustomerFirstName = "Joe",
                    CustomerLastName = "Smith",
                    CustomerEmail = "smith@localhost.local",
                    CustomerSiteID = SiteContext.CurrentSiteID,
                    CustomerUserID = newUser.UserID
                };

                // Saves the registered customer to the database
                CustomerInfoProvider.SetCustomerInfo(newCustomer);
            }


            /// <heading>Updating a customer</heading>
            private void GetAndUpdateCustomer()
            {
                // Gets the first customer whose last name is 'Smith'
                CustomerInfo customer = CustomerInfoProvider.GetCustomers()
                                                                .WhereEquals("CustomerLastName", "Smith")
                                                                .FirstObject;
                if (customer != null)
                {
                    // Updates the customer's properties
                    customer.CustomerLastName = customer.CustomerLastName.ToLowerCSafe();

                    // Saves the changes to the database
                    CustomerInfoProvider.SetCustomerInfo(customer);
                }
            }


            /// <heading>Updating multiple customers</heading>
            private void GetAndBulkUpdateCustomers()
            {
                // Gets all customers whose last name starts with 'Sm'
                var customers = CustomerInfoProvider.GetCustomers().WhereStartsWith("CustomerLastName", "Sm");

                // Loops through the customers
                foreach (CustomerInfo modifyCustomer in customers)
                {
                    // Updates the customer's properties
                    modifyCustomer.CustomerLastName = modifyCustomer.CustomerLastName.ToUpperCSafe();

                    // Saves the changes to the database
                    CustomerInfoProvider.SetCustomerInfo(modifyCustomer);
                }
            }


            /// <heading>Deleting customers</heading>
            private void DeleteCustomer()
            {
                // Gets all customers whose last name starts with 'Sm'
                var customers = CustomerInfoProvider.GetCustomers().WhereStartsWith("CustomerLastName", "Sm");

                // Loops through the customers
                foreach (CustomerInfo deleteCustomer in customers)
                {
                    // Deletes the related user for registered customers
                    UserInfo user = deleteCustomer.CustomerUser;
                    if (user != null)
                    {
                        UserInfoProvider.DeleteUser(user);
                    }

                    // Deletes the customer
                    CustomerInfoProvider.DeleteCustomerInfo(deleteCustomer);
                }
            }
        }


        /// <summary>
        /// Holds customer address API examples.
        /// </summary>
        /// <groupHeading>Customer addresses</groupHeading>
        private class CustomerAddress
        {
            /// <heading>Creating an address</heading>
            private void CreateAddress()
            {
                // Gets the first customer whose last name is 'Smith'
                CustomerInfo customer = CustomerInfoProvider.GetCustomers()
                                                            .WhereEquals("CustomerLastName", "Smith")
                                                            .FirstObject;

                // Gets a country for the address
                CountryInfo country = CountryInfoProvider.GetCountryInfo("USA");

                // Gets a state for the address
                StateInfo state = StateInfoProvider.GetStateInfo("Alabama");

                if ((customer != null) && (country != null))
                {
                    // Creates a new address object and sets its properties
                    AddressInfo newAddress = new AddressInfo
                    {
                        AddressName = "New address",
                        AddressLine1 = "Address line 1",
                        AddressLine2 = "Address line 2",
                        AddressCity = "Address city",
                        AddressZip = "Address ZIP code",
                        AddressPersonalName = customer.CustomerInfoName,
                        AddressCustomerID = customer.CustomerID,
                        AddressCountryID = country.CountryID,
                        AddressStateID = state.StateID
                    };

                    // Saves the address to the dataabase
                    AddressInfoProvider.SetAddressInfo(newAddress);
                }
            }


            /// <heading>Updating an address</heading>
            private void GetAndUpdateAddress()
            {
                // Gets the first customer whose last name is 'Smith'
                CustomerInfo customer = CustomerInfoProvider.GetCustomers()
                                                            .WhereEquals("CustomerLastName", "Smith")
                                                            .FirstObject;

                if (customer != null)
                {
                    // Gets the first enabled address stored for the customer
                    AddressInfo address = AddressInfoProvider.GetAddresses()
                                                                  .WhereEquals("AddressCustomerID", customer.CustomerID)
                                                                  .FirstObject;

                    if (address != null)
                    {
                        // Updates the address properties
                        address.AddressName = address.AddressName.ToLowerCSafe();

                        // Saves the changes to the database
                        AddressInfoProvider.SetAddressInfo(address);
                    }
                }
            }


            /// <heading>Updating multiple addresses</heading>
            private void GetAndBulkUpdateAddresses()
            {
                // Gets the first customer whose last name is 'Smith'
                CustomerInfo customer = CustomerInfoProvider.GetCustomers()
                                                            .WhereEquals("CustomerLastName", "Smith")
                                                            .FirstObject;

                if (customer != null)
                {
                    // Gets all addresses stored for the customer
                    var addresses = AddressInfoProvider.GetAddresses().WhereEquals("AddressCustomerID", customer.CustomerID);

                    // Loops through the addresses
                    foreach (AddressInfo modifyAddress in addresses)
                    {
                        // Updates the address properties
                        modifyAddress.AddressName = modifyAddress.AddressName.ToUpperCSafe();

                        // Saves the changes to the database
                        AddressInfoProvider.SetAddressInfo(modifyAddress);
                    }
                }
            }


            /// <heading>Deleting an address</heading>
            private void DeleteAddress()
            {
                // Gets the address
                AddressInfo address = AddressInfoProvider.GetAddresses()
                                                              .WhereStartsWith("AddressName", "New")
                                                              .FirstObject;

                if (address != null)
                {
                    // Deletes the address
                    AddressInfoProvider.DeleteAddressInfo(address);
                }
            }
        }

        /// <summary>
        /// Holds customer newsletter subscription API examples.
        /// </summary>
        /// <groupHeading>Customer newsletter subscriptions</groupHeading>
        private class CustomerEmailCampaigns
        {
            /// <heading>Subscribing customers to newsletters</heading>
            private void SubscribeCustomerToNewsletter()
            {
                // Gets the first customer whose last name is 'Smith'
                CustomerInfo customer = CustomerInfoProvider.GetCustomers()
                                                            .WhereEquals("CustomerLastName", "Smith")
                                                            .FirstObject;

                // Gets the newsletter
                NewsletterInfo newsletter = NewsletterInfoProvider.GetNewsletterInfo("DancingGoatNewsletter", SiteContext.CurrentSiteID);

                if ((customer != null) && (newsletter != null))
                {
                    // Prepares instances of the Kentico contact provider, email feed subscription service and contact relation assigner
                    IContactProvider contactProvider = CMS.Core.Service.Resolve<IContactProvider>();
                    ISubscriptionService subscriptionService = CMS.Core.Service.Resolve<ISubscriptionService>();
                    IContactRelationAssigner contactRelationAssigner = CMS.Core.Service.Resolve<IContactRelationAssigner>();

                    // Either gets an existing contact by email or creates a new contact object with the customer's email address and name
                    ContactInfo customerContact = contactProvider.GetContactForSubscribing(customer.CustomerEmail, customer.CustomerFirstName, customer.CustomerLastName);

                    // Subscribes the contact to the newsletter
                    subscriptionService.Subscribe(customerContact, newsletter, new SubscribeSettings
                    {
                        AllowOptIn = true, // Allows double opt-in subscription for newsletters that have it enabled
                        SendConfirmationEmail = true, // Allows sending of confirmation emails for the subscription

                        // Removes the email address from the opt-out list for all marketing emails on the given site (if present)
                        RemoveAlsoUnsubscriptionFromAllNewsletters = true, 
                    });

                    // Links the customer with the contact
                    contactRelationAssigner.Assign(
                        customer.CustomerID,
                        MemberTypeEnum.EcommerceCustomer,
                        customerContact.ContactID);
                }
            }


            /// <heading>Unsubscribing customers from newsletters</heading>
            private void UnsubscribeCustomerFromNewsletter()
            {
                // Gets the first customer whose last name is 'Smith'
                CustomerInfo customer = CustomerInfoProvider.GetCustomers()
                                                            .WhereEquals("CustomerLastName", "Smith")
                                                            .FirstObject;

                if (customer != null)
                {
                    // Prepares an instance of the Kentico email feed subscription service
                    ISubscriptionService subscriptionService = CMS.Core.Service.Resolve<ISubscriptionService>();

                    // Adds the customer to the marketing email opt-out list (for all email feeds)
                    // Ensures that the customer no longer receives and marketing emails
                    subscriptionService.UnsubscribeFromAllNewsletters(customer.CustomerEmail, null, sendConfirmationEmail : true);
                }
            }
        }


        /// <summary>
        /// Holds customer wishlist API examples.
        /// </summary>
        /// <groupHeading>Customer wishlists</groupHeading>
        private class CustomerWishlist
        {
            /// <heading>Adding products to a wishlist</heading>
            private void AddProductToWishlist()
            {
                // Gets the product
                SKUInfo product = SKUInfoProvider.GetSKUs()
                                                   .WhereEquals("SKUName", "NewProduct")
                                                   .FirstObject;

                // Gets the first customer whose last name is 'Smith'
                var customer = CustomerInfoProvider.GetCustomers()
                                                   .WhereEquals("CustomerLastName", "Smith")
                                                   .FirstObject;

                if ((customer != null) && (product != null))
                {
                    // Adds the product to the customer's wishlist
                    WishlistItemInfoProvider.AddSKUToWishlist(customer.CustomerUserID, product.SKUID, SiteContext.CurrentSiteID);
                }
            }


            /// <heading>Removing products from a wishlist</heading>
            private void RemoveProductFromWishlist()
            {
                // Gets the product
                var product = SKUInfoProvider.GetSKUs()
                                               .WhereEquals("SKUName", "NewProduct")
                                               .FirstObject;

                // Gets the first customer whose last name is 'Smith'
                CustomerInfo customer = CustomerInfoProvider.GetCustomers()
                                                            .WhereEquals("CustomerLastName", "Smith")
                                                            .FirstObject;

                if ((customer != null) && (product != null))
                {
                    // Gets the product from the wishlist
                    WishlistItemInfo wishlistItem = WishlistItemInfoProvider.GetWishlistItemInfo(customer.CustomerUserID, product.SKUID, SiteContext.CurrentSiteID);

                    if (wishlistItem != null)
                    {
                        // Removes the product from the wishlist
                        WishlistItemInfoProvider.DeleteWishlistItemInfo(wishlistItem);
                    }
                }
            }
        }
    }
}
