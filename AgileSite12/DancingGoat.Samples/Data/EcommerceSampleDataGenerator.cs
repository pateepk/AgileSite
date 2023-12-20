using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS.Globalization;
using CMS.Helpers;
using CMS.SiteProvider;
using CMS.Membership;
using CMS.DataEngine;
using CMS.Ecommerce;

namespace CMS.DancingGoat.Samples
{
    /// <summary>
    /// Sample ecommerce data generator providing sample data for Dancing Goat demo site.
    /// </summary>
    public sealed class EcommerceSampleDataGenerator : ISampleDataGenerator
    {
        private const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        private readonly IList<CountryInfo> countries;
        private readonly Random rand;
        private readonly string[] customerNames =
        {
            "Deneen Fernald", "Antonio Buker", "Marlon Loos", "Nolan Steckler", "Johnetta Tall",
            "Florence Ramsdell", "Modesto Speaker", "Alissa Ferguson", "Calvin Hollier", "Diamond Paik",
            "Mardell Dohrmann", "Dinorah Clower", "Andrea Humbert", "Tyrell Galvan", "Yong Inskeep",
            "Tom Goldschmidt", "Kimbery Rincon", "Genaro Kneeland", "Roselyn Mulvey", "Nancee Jacobson",
            "Jaime Link", "Fonda Belnap", "Muoi Ishmael", "Pearlene Minjarez", "Eustolia Studstill",
            "Marilynn Manos", "Pamila Turnbow", "Lieselotte Satcher", "Sharron Mellon", "Bennett Heatherington",
            "Spring Hessel", "Lashay Blazier", "Veronika Lecuyer", "Mark Spitz", "Peggy Olson",
            "Tyron Bednarczyk", "Terese Betty", "Bibi Kling", "Bruno Spier", "Cristen Bussey",
            "Daine Pridemore", "Gerald Turpen", "Lela Briese", "Sharda Bonnie", "Omar Martin",
            "Marlyn Pettie", "Shiela Cleland", "Marica Granada", "Garland Reagan", "Mora Gillmore",
            "Mariana Rossow", "Betty Pollan", "Analisa Costilla", "Evelyn Mendez", "April Rubino",
            "Zachariah Roberson", "Sheilah Steinhauser", "Araceli Vallance", "Lashawna Weise", "Charline Durante",
            "Melania Nightingale", "Ema Stiltner", "Lynelle Threet", "Dorcas Cully", "Gregg Carranco",
            "Karla Heiner", "Judson Siegmund", "Alyson Oday", "Winston Laxton", "Jarod Turrentine",
            "Israel Shanklin", "Miquel Jorstad", "Brianne Darrow", "Tamara Rulison", "Elliot Rameriz",
            "Gearldine Nova", "Debi Fritts", "Leota Cape", "Tyler Saleem", "Starr Hyden",
            "Loreen Spigner", "Raisa Germain", "Grace Vigue", "Maryann Munsch", "Jason Chon",
            "Gisele Mcquillen", "Juliane Comeaux", "Willette Dodrill", "Sherril Weymouth", "Ashleigh Dearman",
            "Bret Bourne", "Brittney Cron", "Dustin Evans", "Barbie Dinwiddie", "Ricki Wiener",
            "Bess Pedretti", "Monica King", "Edgar Schuetz", "Jettie Boots", "Jefferson Hinkle"
        };


        /// <summary>
        /// Generates sample ecommerce data. Suitable only for Dancing Goat demo site.
        /// </summary>
        /// <param name="siteID">ID of the site to generate sample data for.</param>
        public void Generate(int siteID)
        {
            GenerateEcommerceData(siteID);
        }


        /// <summary>
        /// Constructor. Creates new instance of EcommerceSampleDataGenerator.
        /// </summary>
        public EcommerceSampleDataGenerator()
        {
            countries = CountryInfoProvider.GetCountries().ToList();
            rand = new Random(DateTime.Now.Millisecond);
        }


        /// <summary>
        /// Runs generator of Ecommerce data. It includes Customers, Orders, Order items and Order addresses
        /// </summary>
        private void GenerateEcommerceData(int siteID)
        {
            var siteName = SiteInfoProvider.GetSiteName(siteID);

            var currency = CurrencyInfoProvider.GetCurrencies(siteID).Where("CurrencyIsMain", QueryOperator.Equals, 1).TopN(1).FirstOrDefault();
            var paymentMethods = PaymentOptionInfoProvider.GetPaymentOptions(siteID).ToList();
            var shippingOptions = ShippingOptionInfoProvider.GetShippingOptions(siteID).ToList();
            var orderStatuses = OrderStatusInfoProvider.GetOrderStatuses(siteID).ToDictionary(status => status.StatusName);

            // Products from these manufacturers are excluded from products accessible for generator,
            // because they are too expensive and it degenerates some reports, e.g. Sales by department or Sales by manufacturer
            var manufacturerExceptionList = new List<int> { ManufacturerInfoProvider.GetManufacturerInfo("Anfim", siteName).ManufacturerID,
                                                        ManufacturerInfoProvider.GetManufacturerInfo("Mazzer", siteName).ManufacturerID,
                                                        ManufacturerInfoProvider.GetManufacturerInfo("Macap", siteName).ManufacturerID};

            var products = SKUInfoProvider.GetSKUs(siteID).ToList().Where(sku => sku.IsProduct && !manufacturerExceptionList.Contains(sku.SKUManufacturerID)).ToList();

            // Name used for business user's login and customer's email.
            const string userName = "alex";

            IList<int> customerIds;
            int numberOfCustomers = CustomerInfoProvider.GetCustomers().WhereEquals("CustomerSiteID", siteID).Count;

            // Try to decide if sample users are already generated in the database.
            // If there is more than 50 customers we assume that data are already generated.
            if (numberOfCustomers < 50)
            {
                numberOfCustomers = customerNames.Length;
                customerIds = new List<int>();
                for (int i = 0; i < numberOfCustomers; i++)
                {
                    customerIds.Add(GenerateCustomer(customerNames[i], siteID).CustomerID);
                }
            }
            else
            {
                // Get existing customers, ignore business customer's account
                // don't generate any additional data related to the account.
                DataSet ds = CustomerInfoProvider.GetCustomers().Column("CustomerID")
                                 .WhereEquals("CustomerSiteID", siteID)
                                 .WhereNotEquals("CustomerEmail", userName);
                customerIds = DataHelper.GetIntegerValues(ds.Tables[0], "CustomerID");
                // Set actual list size to prevent out of bound access
                numberOfCustomers = customerIds.Count;
            }

            // For each day of last month generate growing amount of orders
            int days = 0;
            int orders = 0;
            for (int i = 0; i <= 30; i++)
            {
                days++;

                // Randomly adjust number of order for current day from -1 to +1 orders.
                int adjustNumberOfOrders = 0;
                if (i > 5)
                {
                    adjustNumberOfOrders = rand.Next(-1, 2);
                }

                // For each day generate number of orders which is equal number of days divided by 2
                // This ensures growing trend of order report in time.
                for (int j = 0; j < ((days / 2) + adjustNumberOfOrders); j++)
                {
                    // Choose order status according to order date
                    OrderStatusInfo orderStatus;
                    if (i < 25)
                    {
                        orderStatus = orderStatuses["Completed"];
                    }
                    else if (i < 29)
                    {
                        orderStatus = orderStatuses["Processing"];
                    }
                    else
                    {
                        orderStatus = orderStatuses["New"];
                    }

                    var oi = new OrderInfo
                    {
                        OrderCustomerID = customerIds[orders % numberOfCustomers],
                        OrderCurrencyID = currency.CurrencyID,
                        OrderSiteID = siteID,
                        OrderStatusID = orderStatus.StatusID,
                        OrderIsPaid = "Completed".Equals(orderStatus.StatusName, StringComparison.Ordinal) || rand.Next(0, 2) != 0,
                        OrderShippingOptionID = shippingOptions[rand.Next(shippingOptions.Count)].ShippingOptionID,
                        OrderPaymentOptionID = paymentMethods[rand.Next(paymentMethods.Count)].PaymentOptionID,
                        // Set order totals
                        OrderGrandTotal = 0,
                        OrderGrandTotalInMainCurrency = 0,
                        OrderTotalPrice = 0,
                        OrderTotalPriceInMainCurrency = 0,
                        OrderTotalShipping = 10,
                        OrderTotalTax = 10,
                    };

                    // Order must be set before creating order items, addresses and setting order date
                    OrderInfoProvider.SetOrderInfo(oi);

                    var totalPrice = GenerateOrderItems(oi, products);
                    GenerateOrderAddress(oi.OrderID, GetRandomCountryID(), AddressType.Billing);
                    GenerateOrderAddress(oi.OrderID, GetRandomCountryID(), AddressType.Shipping);

                    // Update existing order
                    oi.OrderDate = DateTime.Now.AddDays(-30 + i);
                    oi.OrderTotalPrice = totalPrice;
                    oi.OrderTotalPriceInMainCurrency = totalPrice;
                    oi.OrderGrandTotal = totalPrice;
                    oi.OrderGrandTotalInMainCurrency = totalPrice;

                    // Generate invoice
                    var sci = ShoppingCartInfoProvider.GetShoppingCartInfoFromOrder(oi.OrderID);
                    oi.OrderInvoiceNumber = OrderInfoProvider.GenerateInvoiceNumber(sci);
                    oi.OrderInvoice = ShoppingCartInfoProvider.GetOrderInvoice(sci);

                    // Set info
                    OrderInfoProvider.SetOrderInfo(oi);
                    orders++;
                }
            }

            // Generates a user for demo purposes. User is assigned to a B2B role.
            // Creates a customer with company data.
            // Generates orders for created customer.
            // Don't generate another user if one already exists
            var existingUser = UserInfoProvider.GetUserInfo(userName);
            if (existingUser == null)
            {
                var customer = new CustomerInfo
                {
                    CustomerEmail = "alex@localhost.local",
                    CustomerFirstName = "Alexander",
                    CustomerLastName = "Adams",
                    CustomerSiteID = siteID,
                    CustomerCompany = "Alex & Co. Ltd",
                    CustomerTaxRegistrationID = "12S379BDF798",
                    CustomerOrganizationID = "WRQ7987VRG79"
                };

                CustomerInfoProvider.SetCustomerInfo(customer);
                var user = CustomerInfoProvider.RegisterCustomer(customer, "", userName);

                var silverPartnerRole = RoleInfoProvider.GetRoleInfo("SilverPartner", siteID);
                if (silverPartnerRole != null)
                {
                    UserInfoProvider.AddUserToRole(user.UserID, silverPartnerRole.RoleID);
                }

                for (int i = 0; i < 5; i++)
                {
                    var cart = new ShoppingCartInfo
                    {
                        ShoppingCartCulture = CultureHelper.GetDefaultCultureCode(siteName),
                        ShoppingCartCurrencyID = currency.CurrencyID,
                        ShoppingCartSiteID = siteID,
                        ShoppingCartCustomerID = customer.CustomerID,
                        ShoppingCartBillingAddress = GenerateAddress(GetRandomCountryID(), customer.CustomerID),
                        ShoppingCartShippingAddress = GenerateAddress(GetRandomCountryID(), customer.CustomerID),
                        User = user
                    };

                    ShoppingCartInfoProvider.SetShoppingCartInfo(cart);

                    // Add item to the cart
                    var product = products.ElementAt(rand.Next(products.Count));
                    var cartItemParams = new ShoppingCartItemParameters(product.SKUID, rand.Next(5));
                    ShoppingCartInfoProvider.SetShoppingCartItem(cart, cartItemParams);

                    // Evaluate cart
                    cart.Evaluate();

                    // Create an order from the cart data
                    ShoppingCartInfoProvider.SetOrder(cart);
                }
            }
        }

        private int GetRandomCountryID()
        {
            return countries[rand.Next(countries.Count)].CountryID;
        }


        /// <summary>
        /// Generates order items to given order using random products. Computes order
        /// price and sets it to given totalPrice parameter.
        /// </summary>
        /// <param name="order">Order object to assign items</param>
        /// <param name="products">List of products to select random order items from.</param>
        private decimal GenerateOrderItems(OrderInfo order, IList<SKUInfo> products)
        {
            var skus = new HashSet<SKUInfo>();
            decimal price = 0;

            // Select random number (1 or 2) of different random products
            do
            {
                var sku = products[rand.Next(products.Count)];
                if (!skus.Contains(sku))
                {
                    skus.Add(sku);
                }
            }
            while (skus.Count < rand.Next(1, 3));

            foreach (var sku in skus)
            {
                var orderItem = new OrderItemInfo
                {
                    OrderItemOrderID = order.OrderID,
                    OrderItemSKUID = sku.SKUID,
                    OrderItemSKUName = sku.SKUName,
                    OrderItemUnitPrice = sku.SKUPrice,
                    OrderItemUnitCount = rand.Next(1, 3),
                };

                var itemTotalPrice = orderItem.OrderItemUnitPrice * orderItem.OrderItemUnitCount;
                orderItem.OrderItemTotalPrice = itemTotalPrice;
                orderItem.Insert();
                price += itemTotalPrice;
            }

            return price;
        }


        /// <summary>
        /// Generates customer with random name and returns the customer object.
        /// </summary>
        /// <param name="fullName">Full name of customer</param>
        /// <param name="siteID">ID of the site to assign customer to</param>
        private CustomerInfo GenerateCustomer(string fullName, int siteID)
        {
            var words = fullName.Trim().Split(' ');
            var firstName = words[0];
            var lastName = words[1];

            var customer = new CustomerInfo
            {
                CustomerEmail = $"{firstName.ToLowerInvariant()}@{lastName.ToLowerInvariant()}.local",
                CustomerFirstName = firstName,
                CustomerLastName = lastName,
                CustomerSiteID = siteID
            };

            CustomerInfoProvider.SetCustomerInfo(customer);
            return customer;
        }


        /// <summary>
        /// Generates order address and returns address object.
        /// </summary>
        /// <param name="orderId">ID of order object to assign items</param>
        /// <param name="countryId">ID of the country to be used in new address</param>
        /// <param name="addressType">Type of the address</param>
        private void GenerateOrderAddress(int orderId, int countryId, AddressType addressType)
        {
            var address = new OrderAddressInfo
            {
                AddressLine1 = "Main street " + rand.Next(300),
                AddressCity = "City " + rand.Next(300),
                AddressZip = new string(Enumerable.Repeat(chars, 8).Select(s => s[rand.Next(s.Length)]).ToArray()),
                AddressCountryID = countryId,
                AddressPersonalName = "Home address",
                AddressOrderID = orderId,
                AddressType = (int)addressType
            };

            OrderAddressInfoProvider.SetAddressInfo(address);
        }


        /// <summary>
        /// Generates address and returns the address object.
        /// </summary>
        /// <param name="countryID">ID of the country to be used in a new address.</param>
        /// <param name="customerID">ID of the customer to be used in a new address.</param>
        private AddressInfo GenerateAddress(int countryID, int customerID)
        {
            var address = new AddressInfo
            {
                AddressName = "Address " + rand.Next(300),
                AddressLine1 = "Main street " + rand.Next(300),
                AddressCity = "City " + rand.Next(300),
                AddressZip = new string(Enumerable.Repeat(chars, 8).Select(s => s[rand.Next(s.Length)]).ToArray()),
                AddressCountryID = countryID,
                AddressCustomerID = customerID,
                AddressPersonalName = "Home address"
            };

            AddressInfoProvider.SetAddressInfo(address);
            return address;
        }
    }
}
