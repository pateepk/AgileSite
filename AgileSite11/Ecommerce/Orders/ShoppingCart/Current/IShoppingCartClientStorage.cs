using System;

using CMS;
using CMS.Ecommerce;

[assembly: RegisterImplementation(typeof(IShoppingCartClientStorage), typeof(CookieShoppingCartClientStorage), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// Interface for storing shopping cart GUID on the client's side.
    /// </summary>
    public interface IShoppingCartClientStorage
    {
        /// <summary>
        /// Gets the GUID of the <see cref="ShoppingCartInfo"/> stored on client.
        /// </summary>
        /// <returns>Shopping cart GUID from client or <see cref="Guid.Empty"/> when not found.</returns>
        Guid GetCartGuid();


        /// <summary>
        /// Stores the GUID of the <see cref="ShoppingCartInfo"/> to the client.
        /// </summary>
        /// <remarks>
        /// It is recommended to save the value to the client only if the value has changed.
        /// </remarks>
        /// <param name="cartGuid">GUID of the shopping cart to be stored on the client's side.</param>
        void SetCartGuid(Guid cartGuid);
    }
}