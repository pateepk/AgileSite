using System.Collections.Generic;

using CMS.DataEngine;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Class providing AddressInfo management.
    /// </summary>
    public class OrderAddressInfoProvider : AbstractInfoProvider<OrderAddressInfo, OrderAddressInfoProvider>
    {
        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        public OrderAddressInfoProvider()
            : base(OrderAddressInfo.TYPEINFO, new HashtableSettings
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
        public static ObjectQuery<OrderAddressInfo> GetAddresses()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Sets (updates or inserts) specified address.
        /// </summary>
        /// <param name="addressObj">Address to be set</param>
        public static void SetAddressInfo(OrderAddressInfo addressObj)
        {
            ProviderObject.SetInfo(addressObj);
        }


        /// <summary>
        /// Deletes specified address.
        /// </summary>
        /// <param name="addressObj">Address to be deleted</param>
        public static void DeleteAddressInfo(OrderAddressInfo addressObj)
        {
            ProviderObject.DeleteInfo(addressObj);
        }


        /// <summary>
        /// Gets the address information.
        /// </summary>
        /// <param name="addressId">The address unique identifier.</param>        
        public static OrderAddressInfo GetAddressInfo(int addressId)
        {
            return ProviderObject.GetInfoById(addressId);
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
    }
}