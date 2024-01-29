using System;
using System.Web.Caching;

using CMS.Helpers;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Represents a shopping cart cache.
    /// </summary>
    internal sealed class ShoppingCartCache : IShoppingCartCache
    {
        private const string CACHE_KEY_PREFIX = "CurrentShoppingCart";
        private const string CACHE_KEY_SESSION_NAME = "CurrentShoppingCartCacheKey";

        /// <summary>
        /// Returns cached shopping cart of the current visitor.
        /// </summary>
        public ShoppingCartInfo GetCart()
        {
            var keyName = GetKeyName();

            ShoppingCartInfo cart;
            CacheHelper.TryGetItem(keyName, out cart);

            return cart;
        }


        /// <summary>
        /// Stores current visitor's <see cref="ShoppingCartInfo"/> to the cache.
        /// </summary>
        /// <param name="cart">Shopping cart to be stored.</param>
        public void StoreCart(ShoppingCartInfo cart)
        {
            var keyName = GetKeyName();

            var dependencyKey = (cart != null) ? $"{ShoppingCartInfo.OBJECT_TYPE}|byid|{cart.ShoppingCartID}" : null;
            var dependency = CacheHelper.GetCacheDependency(dependencyKey);

            CacheHelper.Add(keyName, cart, dependency, Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(SessionHelper.SessionTimeout));
        }


        private string GetKeyName()
        {
            var cacheKey = SessionHelper.GetValue(CACHE_KEY_SESSION_NAME) as string;
            if (string.IsNullOrEmpty(cacheKey))
            {
                cacheKey = Guid.NewGuid().ToString();
                SessionHelper.SetValue(CACHE_KEY_SESSION_NAME, cacheKey);
            }

            return CacheHelper.GetCacheItemName(null, CACHE_KEY_PREFIX, cacheKey);
        }
    }
}