using System;

using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Class providing AddressInfo management.
    /// </summary>
    public class AddressInfoProvider : AbstractInfoProvider<AddressInfo, AddressInfoProvider>
    {
        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        public AddressInfoProvider()
            : base(AddressInfo.TYPEINFO, new HashtableSettings
				{
					ID = true,
                    UseWeakReferences = true
				})
        {
        }

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns the query for all addresses.
        /// </summary>
        public static ObjectQuery<AddressInfo> GetAddresses()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns address with specified ID.
        /// </summary>
        /// <param name="addressId">Address ID</param>        
        public static AddressInfo GetAddressInfo(int addressId)
        {
            return ProviderObject.GetInfoById(addressId);
        }


        /// <summary>
        /// Returns address with specified GUID.
        /// </summary>
        /// <param name="addressGuid">Address GUID</param>        
        public static AddressInfo GetAddressInfo(Guid addressGuid)
        {
            return ProviderObject.GetInfoByGuid(addressGuid);
        }


        /// <summary>
        /// Sets (updates or inserts) specified address.
        /// </summary>
        /// <param name="addressObj">Address to be set</param>
        public static void SetAddressInfo(AddressInfo addressObj)
        {
            ProviderObject.SetInfo(addressObj);
        }


        /// <summary>
        /// Deletes specified address.
        /// </summary>
        /// <param name="addressObj">Address to be deleted</param>
        public static void DeleteAddressInfo(AddressInfo addressObj)
        {
            ProviderObject.DeleteInfo(addressObj);
        }


        /// <summary>
        /// Deletes address with specified ID.
        /// </summary>
        /// <param name="addressId">Address ID</param>
        public static void DeleteAddressInfo(int addressId)
        {
            var address = GetAddressInfo(addressId);
            DeleteAddressInfo(address);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Returns query for all customer addresses.
        /// </summary>
        /// <param name="customerId">ID of the customer the address belongs to</param>
        public static ObjectQuery<AddressInfo> GetAddresses(int customerId)
        {
            return ProviderObject.GetAddressesInternal(customerId);
        }


        /// <summary>
        /// Returns address name created from the address parameters. The format of the address name is [personal or company name], [address line 1], [address line 2], [city].
        /// </summary>
        /// <param name="addressObj">Address data</param>
        public static string GetAddressName(AddressInfo addressObj)
        {
            return ProviderObject.GetAddressNameInternal(addressObj);
        }


        /// <summary>
        /// Returns address personal name created from the <paramref name="customer"/>.
        /// The format of the address name is '<see cref="CustomerInfo.CustomerFirstName"/> <see cref="CustomerInfo.CustomerLastName"/>'.
        /// </summary>
        /// <param name="customer">Customer from whom the personal address name is created.</param>
        /// <seealso cref="AddressInfo.AddressPersonalName"/>
        public static string GetAddressPersonalName(CustomerInfo customer)
        {
            return ProviderObject.GetAddressPersonalNameInternal(customer);
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Returns query for all addresses matching the specified parameters.
        /// </summary>
        /// <param name="customerId">ID of the customer the address belongs to</param>
        protected virtual ObjectQuery<AddressInfo> GetAddressesInternal(int customerId)
        {
            var addresses = GetAddresses()
                                .WhereEquals("AddressCustomerID", customerId);
          
            // Get query           
            return addresses;
        }


        /// <summary>
        /// Returns address name created from the address parameters. The format of the address name is [personal or company name], [address line 1], [address line 2], [city].
        /// </summary>
        /// <param name="addressObj">Address data</param>
        protected virtual string GetAddressNameInternal(AddressInfo addressObj)
        {
            string result = "";

            if (addressObj != null)
            {
                // Build address name
                result += addressObj.AddressPersonalName;
                if (addressObj.AddressLine1 != "")
                {
                    result += ", " + addressObj.AddressLine1;
                }
                if (addressObj.AddressLine2 != "")
                {
                    result += ", " + addressObj.AddressLine2;
                }
                result += ", " + addressObj.AddressCity;

                // Correct address name
                if (result.Length > 200)
                {
                    result = result.Remove(199);
                }
            }

            return result;
        }


        /// <summary>
        /// Returns address personal name created from the <paramref name="customer"/>.
        /// The format of the address name is '<see cref="CustomerInfo.CustomerFirstName"/> <see cref="CustomerInfo.CustomerLastName"/>'.
        /// </summary>
        /// <param name="customer">Customer from whom the personal address name is created.</param>
        /// <seealso cref="AddressInfo.AddressPersonalName"/>
        protected virtual string GetAddressPersonalNameInternal(CustomerInfo customer)
        {
            return TextHelper.LimitLength($"{customer.CustomerFirstName} {customer.CustomerLastName}", 200);
        }

        #endregion
    }
}