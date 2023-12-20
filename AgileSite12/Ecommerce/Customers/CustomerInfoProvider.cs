using System;
using System.Linq;

using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.EmailEngine;
using CMS.EventLog;
using CMS.FormEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.SiteProvider;


namespace CMS.Ecommerce
{
    /// <summary>
    /// Class providing CustomerInfo management.
    /// </summary>
    public class CustomerInfoProvider : AbstractInfoProvider<CustomerInfo, CustomerInfoProvider>
    {
        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        public CustomerInfoProvider()
            : base(CustomerInfo.TYPEINFO, new HashtableSettings
            {
                ID = true,
                UseWeakReferences = true
            })
        {
        }

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns the query for all customers.
        /// </summary>
        public static ObjectQuery<CustomerInfo> GetCustomers()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns customer with specified ID.
        /// </summary>
        /// <param name="customerId">Customer ID</param>        
        public static CustomerInfo GetCustomerInfo(int customerId)
        {
            return ProviderObject.GetCustomerInfoInternal(customerId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified customer.
        /// </summary>
        /// <param name="customerObj">Customer to be set</param>
        public static void SetCustomerInfo(CustomerInfo customerObj)
        {
            ProviderObject.SetCustomerInfoInternal(customerObj);
            SynchronizeUser(customerObj);
        }


        /// <summary>
        /// Deletes specified customer.
        /// </summary>
        /// <param name="customerObj">Customer to be deleted</param>
        public static void DeleteCustomerInfo(CustomerInfo customerObj)
        {
            ProviderObject.DeleteCustomerInfoInternal(customerObj);
        }


        /// <summary>
        /// Deletes customer with specified ID.
        /// </summary>
        /// <param name="customerId">Customer ID</param>
        public static void DeleteCustomerInfo(int customerId)
        {
            var customerObj = GetCustomerInfo(customerId);
            DeleteCustomerInfo(customerObj);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Returns customer with specified user ID.
        /// </summary>
        /// <param name="userId">User ID of the required customer</param>        
        public static CustomerInfo GetCustomerInfoByUserID(int userId)
        {
            return ProviderObject.GetCustomerInfoByUserIDInternal(userId);
        }


        /// <summary>
        /// Indicates if user is authorized to modify customers. 'EcommerceModify' OR 'ModifyCustomers' permission is checked.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="user">User to be checked</param>        
        public static bool IsUserAuthorizedToModifyCustomer(string siteName, IUserInfo user)
        {
            return ECommerceHelper.IsUserAuthorizedForPermission(EcommercePermissions.CUSTOMERS_MODIFY, siteName, user);
        }


        /// <summary>
        /// Invalidates customer (and user when registered) who made given order.
        /// </summary>
        /// <param name="customerId">ID of the customer to be invalidated.</param>
        public static void InvalidateCustomer(int customerId)
        {
            ProviderObject.InvalidateCustomerInternal(customerId);
        }


        /// <summary>
        /// Registers the customer (<paramref name="ci"/>) with the given user name
        /// (<paramref name="userName"/>) and password (<paramref name="pass"/>),
        /// and notifies the customer with an invitation email.
        /// If <paramref name="pass"/> is <c>null</c>, the system generates a new password.
        /// If <paramref name="userName"/> is <c>null</c>, the customer's email is used instead.
        /// </summary>
        /// <param name="ci">Customer object.</param>
        /// <param name="emailTemplateCodeName">Template code name of the invitation email.</param>
        /// <param name="userName">Customer's user name.</param>
        /// <param name="pass">Customer's password.</param>
        /// <returns><c>True</c> if the method was successful.</returns>
        public static bool RegisterAndNotify(CustomerInfo ci, string emailTemplateCodeName, string userName = null, string pass = null)
        {
            return ProviderObject.RegisterAndNotifyInternal(ci, emailTemplateCodeName, userName, pass);
        }


        /// <summary>
        /// Registers the customer (<paramref name="ci"/>) with the given user name
        /// (<paramref name="userName"/>) and password (<paramref name="password"/>).
        /// If <paramref name="userName"/> is <c>null</c>, the customer's email is used instead.
        /// </summary>
        /// <param name="ci">Customer object.</param>
        /// <param name="password">Customer's password.</param>
        /// <param name="userName">Customer's user name.</param>
        /// <returns>Customer's new user or <c>null</c> if customer could not be registered. An error is logged to the event log in that case.</returns>
        public static UserInfo RegisterCustomer(CustomerInfo ci, string password, string userName = null)
        {
            return ProviderObject.RegisterCustomerInternal(ci, password, userName);
        }


        /// <summary>
        /// Send login details notification email based on given template. Resolves macros "UserName", "Password", "UserFullName", "AccountUrl".
        /// Returns true on success. 
        /// </summary>
        /// <param name="user">New user</param>
        /// <param name="password">New password</param>
        /// <param name="emailTemplateCodeName">Template</param>
        /// <param name="siteId">Site Id</param>
        public static bool SendLoginDetailsNotificationEmail(UserInfo user, string password, string emailTemplateCodeName, int siteId)
        {
            return ProviderObject.SendLoginDetailsNotificationEmailInternal(user, password, emailTemplateCodeName, siteId);
        }


        /// <summary>
        /// Copies customers data to user object (first name, last name, full name, phone, email). Returns true if data were different. 
        /// </summary>
        /// <param name="customer">Customer info object as a source of user info data.</param>
        /// <param name="user">User info object to be updated.</param>
        public static bool CopyDataFromCustomerToUser(CustomerInfo customer, UserInfo user)
        {
            return ProviderObject.CopyDataFromCustomerToUserInternal(customer, user);
        }


        /// <summary>
        /// Copies users data to customer object (first name, last name, phone, email). Returns true if data were different. 
        /// </summary>
        /// <param name="customer">Customer to be updated.</param>
        /// <param name="user">User info as a source of data.</param>
        public static bool CopyDataFromUserToCustomer(CustomerInfo customer, UserInfo user)
        {
            return ProviderObject.CopyDataFromUserToCustomerInternal(customer, user);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Returns customer with specified ID.
        /// </summary>
        /// <param name="customerId">Customer ID</param>        
        protected virtual CustomerInfo GetCustomerInfoInternal(int customerId)
        {
            return GetInfoById(customerId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified customer.
        /// </summary>
        /// <param name="customerObj">Customer to be set</param>        
        protected virtual void SetCustomerInfoInternal(CustomerInfo customerObj)
        {
            bool newCustomer = (customerObj.CustomerID <= 0);

            // Set when the customer was created if required
            if (newCustomer && customerObj.Generalized.UpdateTimeStamp)
            {
                customerObj.CustomerCreated = DateTime.Now;
            }

            SetInfo(customerObj);

            // Special handling for customer registration
            if (customerObj.CustomerIsRegistered && (customerObj.CustomerSiteID > 0))
            {
                // Registered user does not have site ID
                customerObj.CustomerSiteID = 0;

                using (CMSActionContext context = new CMSActionContext())
                {
                    // Disable logging of tasks
                    context.DisableLogging();
                    SetInfo(customerObj);
                }
            }
        }


        /// <summary>
        /// Deletes specified customer.
        /// </summary>
        /// <param name="customerObj">Customer to be deleted</param>        
        protected virtual void DeleteCustomerInfoInternal(CustomerInfo customerObj)
        {
            if (customerObj == null)
            {
                return;
            }

            using (var tr = BeginTransaction())
            {
                // Delete related shopping carts
                var carts = ShoppingCartInfoProvider.GetShoppingCarts().WhereEquals("ShoppingCartCustomerID", customerObj.CustomerID);
                foreach (var cart in carts)
                {
                    ShoppingCartInfoProvider.DeleteShoppingCartInfo(cart);
                }

                // Delete customer
                DeleteInfo(customerObj);

                tr.Commit();
            }
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Returns customer with specified user ID.
        /// </summary>
        /// <param name="userId">User ID of the required customer</param>   
        protected virtual CustomerInfo GetCustomerInfoByUserIDInternal(int userId)
        {
            // Find the first customer assigned to requested user
            return GetObjectQuery().TopN(1)
                       .WhereEquals("CustomerUserID", userId)
                       .OrderBy("CustomerCreated")
                       .FirstOrDefault();
        }


        /// <summary>
        /// Invalidates customer (and user when registered) who made given order.
        /// </summary>
        /// <param name="customerId">ID of the customer to be invalidated.</param>
        protected virtual void InvalidateCustomerInternal(int customerId)
        {
            // Get customer info
            CustomerInfo ci = GetCustomerInfo(customerId);

            if (ci != null)
            {
                // Invalidate customer
                ci.Generalized.Invalidate(false);

                // Invalidate user when customer is registered
                if (ci.CustomerIsRegistered)
                {
                    ci.CustomerUser?.Generalized.Invalidate(false);
                }
            }
        }


        /// <summary>
        /// Registers the customer (<paramref name="ci"/>) with the given user name
        /// (<paramref name="userName"/>) and password (<paramref name="pass"/>),
        /// and notifies the customer with an invitation email.
        /// If <paramref name="pass"/> is <c>null</c>, the system generates a new password.
        /// If <paramref name="userName"/> is <c>null</c>, the customer's email is used instead.
        /// </summary>
        /// <param name="ci">Customer object.</param>
        /// <param name="emailTemplateCodeName">Template code name of the invitation email.</param>
        /// <param name="userName">Customer's user name.</param>
        /// <param name="pass">Customer's password.</param>
        /// <returns><c>True</c> if the method was successful.</returns>
        protected virtual bool RegisterAndNotifyInternal(CustomerInfo ci, string emailTemplateCodeName, string userName, string pass)
        {
            if ((ci == null) || (ci.CustomerID <= 0))
            {
                return false;
            }

            if (string.IsNullOrEmpty(emailTemplateCodeName))
            {
                var service = Service.Resolve<IEventLogService>();
                service.LogEvent(EventType.ERROR, "CustomerRegistration", "LOGINDETAILS", "No e-mail template provided.");

                return false;
            }

            int siteId = ci.CustomerSiteID;

            // Set new user and register him with generated password
            var siteName = SiteInfoProvider.GetSiteName(siteId);
            string password = String.IsNullOrEmpty(pass) ? UserInfoProvider.GenerateNewPassword(siteName) : pass;
            UserInfo ui = RegisterCustomer(ci, password, userName);

            if (ui != null)
            {
                // Notify with email
                return SendLoginDetailsNotificationEmailInternal(ui, password, emailTemplateCodeName, siteId);
            }

            return false;
        }


        /// <summary>
        /// Registers the customer (<paramref name="ci"/>) with the given user name
        /// (<paramref name="userName"/>) and password (<paramref name="password"/>).
        /// If <paramref name="userName"/> is <c>null</c>, the customer's email is used instead.
        /// </summary>
        /// <param name="ci">Customer object.</param>
        /// <param name="password">Customer's password.</param>
        /// <param name="userName">Customer's user name.</param>
        /// <returns>Customer's new user.</returns>
        protected virtual UserInfo RegisterCustomerInternal(CustomerInfo ci, string password, string userName)
        {
            if ((ci == null) || ci.CustomerID <= 0)
            {
                return null;
            }

            userName = String.IsNullOrEmpty(userName) ? ci.CustomerEmail : userName;

            // Check registration details 
            if (!CheckCustomerRegistrationDetails(ci, password, userName))
            {
                return null;
            }

            if (ci.CustomerSiteID > 0)
            {
                var site = SiteInfoProvider.GetSiteInfo(ci.CustomerSiteID);
                if ((site != null) && UserInfoProvider.UserNameSitePrefixEnabled(site.SiteName) && !UserInfoProvider.IsSitePrefixedUser(userName))
                {
                    userName = UserInfoProvider.EnsureSitePrefixUserName(userName, site);
                }
            }
            // Init new user with customer details
            var user = new UserInfo();
            FormHelper.LoadDefaultValues(PredefinedObjectType.USER, user);
            user.UserName = userName;
            user.Enabled = true;
            user.SiteIndependentPrivilegeLevel = UserPrivilegeLevelEnum.None;
            user.IsExternal = false;

            CopyDataFromCustomerToUser(ci, user);

            using (var tr = BeginTransaction())
            {
                // Set to database
                UserInfoProvider.SetPassword(user, password);

                if (ci.CustomerSiteID > 0)
                {
                    UserSiteInfoProvider.AddUserToSite(user.UserID, ci.CustomerSiteID);
                }

                // Link to customer
                ci.CustomerUserID = user.UserID;
                SetCustomerInfo(ci);

                tr.Commit();
            }

            return user;
        }


        /// <summary>
        /// Copies customers data to user object (first name, last name, full name, phone, email). Returns true if data were different. 
        /// </summary>
        /// <param name="customer">Customer info object as a source of user info data.</param>
        /// <param name="user">User info object to be updated.</param>
        protected virtual bool CopyDataFromCustomerToUserInternal(CustomerInfo customer, UserInfo user)
        {
            bool updated = false;

            if ((customer == null) || (user == null))
            {
                return false;
            }

            if (!customer.CustomerFirstName.EqualsCSafe(user.FirstName))
            {
                user.FirstName = customer.CustomerFirstName;
                updated = true;
            }

            if (!customer.CustomerEmail.EqualsCSafe(user.Email))
            {
                user.Email = customer.CustomerEmail;
                updated = true;
            }

            if (!customer.CustomerLastName.EqualsCSafe(user.LastName))
            {
                user.LastName = customer.CustomerLastName;
                updated = true;
            }

            if (!customer.CustomerPhone.EqualsCSafe(user.UserSettings.UserPhone))
            {
                user.UserSettings.UserPhone = customer.CustomerPhone;
                updated = true;
            }

            user.FullName = UserInfoProvider.GetFullName(customer.CustomerFirstName, user.MiddleName, customer.CustomerLastName);

            return updated;
        }


        /// <summary>
        /// Copies users data to customer object (first name, last name, phone, email). Returns true if data were different. 
        /// </summary>
        /// <param name="customer">Customer to be updated.</param>
        /// <param name="user">User info as a source of data.</param>
        protected virtual bool CopyDataFromUserToCustomerInternal(CustomerInfo customer, UserInfo user)
        {
            bool updated = false;

            if ((customer == null) || (user == null))
            {
                return false;
            }

            if (!String.IsNullOrEmpty(user.FirstName) && !customer.CustomerFirstName.EqualsCSafe(user.FirstName))
            {
                customer.CustomerFirstName = user.FirstName;
                updated = true;
            }

            if (!String.IsNullOrEmpty(user.Email) && !customer.CustomerEmail.EqualsCSafe(user.Email))
            {
                customer.CustomerEmail = user.Email;
                updated = true;
            }

            if (!String.IsNullOrEmpty(user.LastName) && !customer.CustomerLastName.EqualsCSafe(user.LastName))
            {
                customer.CustomerLastName = user.LastName;
                updated = true;
            }

            if (!customer.CustomerPhone.EqualsCSafe(user.UserSettings.UserPhone))
            {
                customer.CustomerPhone = user.UserSettings.UserPhone;
                updated = true;
            }

            return updated;
        }


        /// <summary>
        /// Send login details notification email based on given template. Resolves macros "UserName", "Password", "UserFullName", "AccountUrl".
        /// Returns true on success. 
        /// </summary>
        /// <param name="user">New user</param>
        /// <param name="password">New password</param>
        /// <param name="emailTemplateCodeName">Template</param>
        /// <param name="siteId">Site Id</param>
        protected virtual bool SendLoginDetailsNotificationEmailInternal(UserInfo user, string password, string emailTemplateCodeName, int siteId)
        {
            if (user == null || String.IsNullOrEmpty(password))
            {
                return false;
            }

            EmailTemplateInfo template = EmailTemplateProvider.GetEmailTemplate(emailTemplateCodeName, siteId);
            if (template == null)
            {
                EventLogProvider.LogException("CustomerRegistration", "LOGINDETAILS", null, siteId, $"E-mail template '{emailTemplateCodeName}' not found.");
                return false;
            }

            string emFrom = EmailHelper.GetSender(template, ECommerceSettings.SendEmailsFrom(siteId));
            if (String.IsNullOrEmpty(emFrom))
            {
                EventLogProvider.LogException("CustomerRegistration", "LOGINDETAILS", null, siteId, "Setting 'Send e-commerce e-mails from' is not set.");
                return false;
            }

            // Email properties
            EmailMessage message = new EmailMessage();
            message.From = emFrom;
            message.Recipients = user.Email;
            message.EmailFormat = EmailFormatEnum.Default;

            // Set default subject if template subject is not available
            message.Subject = EmailHelper.GetSubject(template, ResHelper.GetString("com.customer.notificationemaildefaultsubject"));

            var resolver = MembershipResolvers.GetPasswordResolver(user, password);

            // Add extra sources
            resolver.SetNamedSourceData("AccountUrl", URLHelper.GetAbsoluteUrl(SettingsKeyInfoProvider.GetValue("CMSMyAccountURL", siteId)));

            // Send message 
            var siteName = SiteInfoProvider.GetSiteName(siteId);
            EmailSender.SendEmailWithTemplateText(siteName, message, template, resolver, false);

            return true;
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Selects collection of customers for given user and ensures synchronization of values first name, last name, phone and email.
        /// </summary>
        internal static void SynchronizeCustomers(UserInfo user)
        {
            // Do not throw exception if license validation fails
            using (new CMSActionContext { EmptyDataForInvalidLicense = true })
            {
                if (user == null)
                {
                    return;
                }

                var customers = GetCustomers().WhereEquals("CustomerUserID", user.UserID);

                foreach (CustomerInfo customer in customers)
                {
                    if (CopyDataFromUserToCustomer(customer, user))
                    {
                        ProviderObject.SetCustomerInfoInternal(customer);
                    }
                }
            }
        }


        /// <summary>
        /// Ensures synchronization of customer's user if customer's first name, last name, phone or email has changed.
        /// </summary>
        /// <param name="customer"></param>
        internal static void SynchronizeUser(CustomerInfo customer)
        {
            if ((customer == null) || !customer.CustomerIsRegistered)
            {
                return;
            }

            // Special handling for customer -> user synchronization
            var user = customer.CustomerUser;
            if (CopyDataFromCustomerToUser(customer, user))
            {
                UserInfoProvider.SetUserInfo(user);
            }
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Checks customer registration details and logs messages into the Event log in case of error.
        /// </summary>
        /// <param name="customer">Customer object</param>
        /// <param name="password">New password</param>
        /// <param name="userName">New username</param>
        /// <returns>True if everything is OK, false otherwise.</returns>
        private bool CheckCustomerRegistrationDetails(CustomerInfo customer, string password, string userName)
        {
            var siteId = customer.CustomerSiteID;
            bool checkFailed = false;

            if (customer.CustomerIsRegistered)
            {
                EventLogProvider.LogException("CustomerRegistration", "LOGINDETAILS", null, siteId, $"Customer '{customer.CustomerEmail}' is already registered.");
                checkFailed = true;
            }
            else if (!CheckCustomerRegistrationPassword(password, siteId)
                     || !CheckCustomerRegistrationUsername(userName, siteId)
                     || !CheckCustomerRegistrationEmail(customer.CustomerEmail, siteId))
            {
                checkFailed = true;
            }

            return !checkFailed;
        }


        /// <summary>
        /// Checks customer registration password and logs message into the Event log in case of error.
        /// </summary>
        /// <param name="password">New password</param>
        /// <param name="siteId">Site identifier.</param>
        /// <returns>True if everything is OK, false otherwise.</returns>
        private bool CheckCustomerRegistrationPassword(string password, int siteId)
        {
            if (password == null)
            {
                EventLogProvider.LogException("CustomerRegistration", "LOGINDETAILS", null, siteId, "Missing password.");
                return false;
            }

            return true;
        }


        /// <summary>
        /// Checks customer registration user name and logs message into the Event log in case of error.
        /// </summary>
        /// <param name="userName">New username</param>
        /// <param name="siteId">Site identifier</param>
        /// <returns>True if everything is OK, false otherwise.</returns>
        private bool CheckCustomerRegistrationUsername(string userName, int siteId)
        {
            var checkFailed = false;

            if (String.IsNullOrEmpty(userName))
            {
                EventLogProvider.LogException("CustomerRegistration", "LOGINDETAILS", null, siteId, "Missing user name.");
                checkFailed = true;
            }
            else if (!ValidationHelper.IsUserName(userName))
            {
                EventLogProvider.LogException("CustomerRegistration", "LOGINDETAILS", null, siteId, $"'{userName}' is not a valid user name.");
                checkFailed = true;
            }
            else if ((UserInfoProvider.GetUserInfo(userName) != null) || !UserInfoProvider.IsUserNamePrefixUnique(userName, 0))
            {
                EventLogProvider.LogException("CustomerRegistration", "LOGINDETAILS", null, siteId, $"User '{userName}' already exists.");
                checkFailed = true;
            }

            return !checkFailed;
        }


        /// <summary>
        /// Checks customer registration email and logs message into the Event log in case of error.
        /// </summary>
        /// <param name="email">New email</param>
        /// <param name="siteId">Site identifier</param>
        /// <returns>True if everything is OK, false otherwise.</returns>
        private bool CheckCustomerRegistrationEmail(string email, int siteId)
        {
            bool checkFailed = false;

            if (String.IsNullOrEmpty(email))
            {
                EventLogProvider.LogException("CustomerRegistration", "LOGINDETAILS", null, siteId, "Missing e-mail address.");
                checkFailed = true;
            }
            else if (!UserInfoProvider.IsEmailUnique(email, new SiteInfoIdentifier(siteId), 0))
            {
                EventLogProvider.LogException("CustomerRegistration", "LOGINDETAILS", null, siteId, "E-mail address has to be unique.");
                checkFailed = true;
            }

            return !checkFailed;
        }

        #endregion
    }
}