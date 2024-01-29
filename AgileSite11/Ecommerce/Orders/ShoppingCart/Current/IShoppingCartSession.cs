using System;

using CMS;
using CMS.Ecommerce;

[assembly: RegisterImplementation(typeof(IShoppingCartSession), typeof(ShoppingCartSession), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// Interface for storing shopping cart GUID in session.
    /// </summary>
    public interface IShoppingCartSession
    {
        /// <summary>
        /// Gets the GUID of the <see cref="ShoppingCartInfo"/> stored in session.
        /// </summary>
        /// <returns>Shopping cart GUID from session or <see cref="Guid.Empty"/> when not found.</returns>
        Guid GetCartGuid();


        /// <summary>
        /// Stores the GUID of the <see cref="ShoppingCartInfo"/> to the session.
        /// </summary>
        /// <remarks>
        /// It is recommended to save the value to the session only if the value has changed.
        /// </remarks>
        /// <param name="cartGuid">GUID of the shopping cart to be stored to the session.</param>
        void SetCartGuid(Guid cartGuid);
    }
}
