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


        #region "Public methods - Advanced"

        /// <summary>
        /// Creates new order address with data from specified IAddress object.
        /// </summary>
        /// <param name="address">The address to copy data from.</param>
        public static OrderAddressInfo CreateOrderAddressInfo(IAddress address)
        {
            return ProviderObject.CreateOrderAddressInfoInternal(address);
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Creates new order address with data from specified IAddress object.
        /// </summary>
        /// <param name="address">The address to copy data from.</param>
        protected virtual OrderAddressInfo CreateOrderAddressInfoInternal(IAddress address)
        {
            var orderAddress = new OrderAddressInfo();

            var originalAddress = address as BaseInfo;
            if (originalAddress != null)
            {
                // Set info fields (custom fields included)
                var columnNames = new List<string>(originalAddress.ColumnNames);
                columnNames.Remove(originalAddress.TypeInfo.IDColumn);
                columnNames.Remove(originalAddress.TypeInfo.GUIDColumn);
                columnNames.Remove(originalAddress.TypeInfo.TimeStampColumn);

                foreach (var columnName in columnNames)
                {
                    orderAddress.SetValue(columnName, originalAddress.GetValue(columnName));
                }
            }
            // Set data through interface (could differ from info data)
            orderAddress.AddressCity = address.AddressCity;
            orderAddress.AddressCountryID = address.AddressCountryID;
            orderAddress.AddressZip = address.AddressZip;
            orderAddress.AddressStateID = address.AddressStateID;
            orderAddress.AddressPhone = address.AddressPhone;
            orderAddress.AddressPersonalName = address.AddressPersonalName;
            orderAddress.AddressLine1 = address.AddressLine1;
            orderAddress.AddressLine2 = address.AddressLine2;

            return orderAddress;
        }

        #endregion
    }
}