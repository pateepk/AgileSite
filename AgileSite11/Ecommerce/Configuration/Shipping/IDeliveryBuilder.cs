using System;

using CMS;
using CMS.Ecommerce;

[assembly: RegisterImplementation(typeof(IDeliveryBuilder), typeof(DefaultDeliveryBuilder), Priority = CMS.Core.RegistrationPriority.Fallback, Lifestyle = CMS.Core.Lifestyle.Transient)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// Interface for classes able to create <see cref="Delivery"/> objects from the calculation request.
    /// </summary>
    /// <seealso cref="DefaultDeliveryBuilder"/>
    public interface IDeliveryBuilder
    {
        /// <summary>
        /// Sets the currently constructed <see cref="Delivery"/> according to the supplied calculation request data and adds request items selected by 
        /// the <paramref name="itemSelector"/> predicate.
        /// </summary>
        /// <param name="request">The calculation request data to be packed in the constructed <see cref="Delivery"/>.</param>
        /// <param name="itemSelector">The predicate deciding which items are added to <see cref="Delivery"/>. 
        /// When <c>null</c>, all items are added.</param>
        void SetFromCalculationRequest(CalculationRequest request, Func<CalculationRequestItem, bool> itemSelector = null);


        /// <summary>
        /// Sets an address to which the currently constructed <see cref="Delivery"/> will be shipped.
        /// </summary>
        /// <param name="address">The target address.</param>
        void SetDeliveryAddress(IAddress address);


        /// <summary>
        /// Sets the shipping option used for the currently constructed <see cref="Delivery"/>.
        /// </summary>
        /// <param name="shippingOption">The used shipping option.</param>
        void SetShippingOption(ShippingOptionInfo shippingOption);


        /// <summary>
        /// Sets weight of the package with the whole <see cref="Delivery"/>.
        /// </summary>
        /// <param name="weight">Weight of the <see cref="Delivery"/>.</param>
        void SetWeight(decimal weight);


        /// <summary>
        /// Sets the date of shipping.
        /// </summary>
        /// <param name="shippingDate">The date of shipping.</param>
        void SetShippingDate(DateTime shippingDate);


        /// <summary>
        /// Sets custom data under the given key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="data">Custom data to be stored under given <paramref name="key"/>.</param>
        void SetCustomData(string key, object data);


        /// <summary>
        /// Creates a new instance of <see cref="Delivery"/> based on builder settings.
        /// </summary>
        /// <returns>A new <see cref="Delivery"/> object.</returns>
        Delivery BuildDelivery();
    }
}