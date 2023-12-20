using System;
using System.Collections.Generic;

using CMS.Base;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Class representing a delivery (a set of items shipped together using a shipping option).
    /// </summary>
    /// <remarks>
    /// Use <see cref="DefaultDeliveryBuilder"/> or derived class to create instances of this type.
    /// </remarks>
    /// <seealso cref="DeliveryItem"/>
    /// <seealso cref="DefaultDeliveryBuilder"/>
    public sealed class Delivery
    {
        /// <summary>
        /// The items shipped in the delivery. Only products and product options of the product type are present.
        /// </summary>
        public IEnumerable<DeliveryItem> Items
        {
            get;
            internal set;
        }


        /// <summary>
        /// The shipping date (when the package leaves the store).
        /// </summary>
        public DateTime ShippingDate
        {
            get;
            internal set;
        }


        /// <summary>
        /// Weight of the whole package.
        /// </summary>
        public decimal Weight
        {
            get;
            internal set;
        }


        /// <summary>
        /// The shipping option used for the delivery.
        /// </summary>
        public ShippingOptionInfo ShippingOption
        {
            get;
            internal set;
        }


        /// <summary>
        /// The address to which the delivery is shipped.
        /// </summary>
        public AddressInfo DeliveryAddress
        {
            get;
            internal set;
        }


        /// <summary>
        /// Custom data.
        /// </summary>
        public IDataContainer CustomData
        {
            get;
            internal set;
        }


        /// <summary>
        /// Returns clone of delivery object
        /// </summary>
        internal Delivery Clone()
        {
            return new Delivery
            {
                Items = Items,
                ShippingDate = ShippingDate,
                Weight = Weight,
                ShippingOption = ShippingOption,
                DeliveryAddress = DeliveryAddress,
                CustomData = CustomData
            };
        }
    }
}
