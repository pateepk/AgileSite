using System;

using CMS.Helpers;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Class for storing shopping cart GUID in the session.
    /// </summary>
    internal sealed class ShoppingCartSession : IShoppingCartSession
    {
        /// <summary>
        /// Gets the GUID of the <see cref="ShoppingCartInfo"/> stored in session.
        /// </summary>
        /// <returns>Shopping cart GUID from session or <see cref="Guid.Empty"/> when not found.</returns>
        public Guid GetCartGuid()
        {
            return ValidationHelper.GetGuid(SessionHelper.GetValue(CookieName.ShoppingCart), Guid.Empty);
        }


        /// <summary>
        /// Stores the GUID of the <see cref="ShoppingCartInfo"/> to the session.
        /// </summary>
        /// <remarks>
        /// It is recommended to save the value to the session only if the value has changed.
        /// </remarks>
        /// <param name="cartGuid">GUID of the shopping cart to be stored to the session.</param>
        public void SetCartGuid(Guid cartGuid)
        {
            var currentGuid = GetCartGuid();
            if (cartGuid == currentGuid)
            {
                return;
            }

            if (cartGuid == Guid.Empty)
            {
                SessionHelper.Remove(CookieName.ShoppingCart);
                return;
            }

            SessionHelper.SetValue(CookieName.ShoppingCart, cartGuid);
        }
    }
}
