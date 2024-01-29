using System;

using CMS.Helpers;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Class for storing shopping cart GUID in the client's cookie.
    /// </summary>
    /// <seealso cref="CookieName.ShoppingCart"/>
    internal sealed class CookieShoppingCartClientStorage : IShoppingCartClientStorage
    {
        /// <summary>
        /// Gets the GUID of the <see cref="ShoppingCartInfo"/> from the cookie.
        /// </summary>
        /// <returns>Shopping cart GUID from client or <see cref="Guid.Empty"/> when not found.</returns>
        public Guid GetCartGuid()
        {
            return ValidationHelper.GetGuid(CookieHelper.GetValue(CookieName.ShoppingCart), Guid.Empty);
        }


        /// <summary>
        /// Stores the GUID of the <see cref="ShoppingCartInfo"/> to the client's cookie.
        /// </summary>
        /// <remarks>
        /// The value is set to cookie only if changed.
        /// </remarks>
        /// <param name="cartGuid">GUID of the shopping cart to be stored on the client's cookie.</param>
        public void SetCartGuid(Guid cartGuid)
        {
            var currentGuid = GetCartGuid();
            if (cartGuid == currentGuid)
            {
                return;
            }

            if (cartGuid == Guid.Empty)
            {
                CookieHelper.Remove(CookieName.ShoppingCart);
                return;
            }

            CookieHelper.SetValue(CookieName.ShoppingCart, cartGuid.ToString(), DateTime.Now.AddYears(1));
        }
    }
}