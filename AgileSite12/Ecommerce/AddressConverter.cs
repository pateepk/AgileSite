using System.Collections.Generic;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.Ecommerce;

[assembly: RegisterImplementation(typeof(IAddressConverter), typeof(AddressConverter), Priority = RegistrationPriority.SystemDefault)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// Provides conversion methods for addresses.
    /// </summary>
    public class AddressConverter : IAddressConverter
    {
        /// <summary>
        /// Converts <see cref="OrderAddressInfo"/> to <see cref="AddressInfo"/>.
        /// </summary>
        /// <param name="orderAddress">Order address to copy data from.</param>
        /// <returns>New <see cref="AddressInfo"/> based on <see cref="OrderAddressInfo"/> data.</returns>
        public virtual AddressInfo Convert(OrderAddressInfo orderAddress)
        {
            if (orderAddress == null)
            {
                return null;
            }

            var address = new AddressInfo();

            CopyDataColumns(orderAddress, address);

            return address;
        }

        /// <summary>
        /// Converts <see cref="AddressInfo"/> to <see cref="OrderAddressInfo"/>.
        /// </summary>
        /// <param name="address">Address to copy data from.</param>
        /// <param name="orderId">ID of the order to assign address to.</param>
        /// <param name="addressType">Type of the address.</param>
        /// <returns>New <see cref="OrderAddressInfo"/> based on <see cref="AddressInfo"/> data.</returns>
        public virtual OrderAddressInfo Convert(AddressInfo address, int orderId, AddressType addressType)
        {
            if (address == null)
            {
                return null;
            }

            var orderAddress = new OrderAddressInfo();

            CopyDataColumns(address, orderAddress);
             
            orderAddress.AddressCity = address.AddressCity;
            orderAddress.AddressCountryID = address.AddressCountryID;
            orderAddress.AddressZip = address.AddressZip;
            orderAddress.AddressStateID = address.AddressStateID;
            orderAddress.AddressPhone = address.AddressPhone;
            orderAddress.AddressPersonalName = address.AddressPersonalName;
            orderAddress.AddressLine1 = address.AddressLine1;
            orderAddress.AddressLine2 = address.AddressLine2;
            orderAddress.AddressOrderID = orderId;
            orderAddress.AddressType = (int)addressType;

            return orderAddress;
        }


        /// <summary>
        /// Copies data from <paramref name="sourceAddress"/> to <paramref name="destinationAddress"/>.
        /// This method is called from <see cref="Convert(OrderAddressInfo)"/> and <see cref="Convert(AddressInfo, int, AddressType)"/>
        /// to copy data between object types.
        /// </summary>
        /// <param name="sourceAddress">Source address to copy data from.</param>
        /// <param name="destinationAddress">Destination address to copy data to.</param>
        protected virtual void CopyDataColumns(BaseInfo sourceAddress, BaseInfo destinationAddress)
        {
            var columnNames = new List<string>(sourceAddress.ColumnNames);
            columnNames.Remove(sourceAddress.TypeInfo.IDColumn);
            columnNames.Remove(sourceAddress.TypeInfo.GUIDColumn);
            columnNames.Remove(sourceAddress.TypeInfo.TimeStampColumn);

            foreach (var columnName in columnNames)
            {
                destinationAddress.SetValue(columnName, sourceAddress.GetValue(columnName));
            }
        }
    }
}
