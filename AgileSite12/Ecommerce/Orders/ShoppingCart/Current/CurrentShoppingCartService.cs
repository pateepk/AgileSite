using System;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.Membership;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Represents a service providing current shopping cart functionality.
    /// </summary>
    internal class CurrentShoppingCartService : ICurrentShoppingCartService
    {
        protected readonly IShoppingCartClientStorage mClientStorage;
        protected readonly IShoppingCartCache mCartCache;
        protected readonly IShoppingCartFactory mCartFactory;
        protected readonly ICurrentShoppingCartStrategy mCartStrategy;
        protected readonly IShoppingCartRepository mCartRepository;
        protected readonly IShoppingCartSession mCartSession;


        /// <summary>
        /// Constructor. Creates new instance of <see cref="CurrentShoppingCartService"/>.
        /// </summary>
        /// <param name="clientStorage">Client-site storage of shopping cart identifier.</param>
        /// <param name="cartCache">Server-side cache able to hold shopping cart for current visitor.</param>
        /// <param name="cartSession">Session storage of shopping cart identifier.</param>
        /// <param name="cartFactory">Factory used to create new shopping cart when visitor has no cart.</param>
        /// <param name="cartStrategy">Strategy used for evaluation of candidate shopping cart and its preparation for use.</param>
        /// <param name="cartRepository">Persistent storage of shopping cart objects.</param>
        public CurrentShoppingCartService(ICurrentShoppingCartStrategy cartStrategy, IShoppingCartRepository cartRepository, IShoppingCartFactory cartFactory, IShoppingCartCache cartCache, IShoppingCartClientStorage clientStorage, IShoppingCartSession cartSession)
        {
            mCartStrategy = cartStrategy;
            mCartRepository = cartRepository;
            mCartFactory = cartFactory;
            mCartCache = cartCache;
            mClientStorage = clientStorage;
            mCartSession = cartSession;
        }


        /// <summary>
        /// Finds the most suitable shopping cart for given user. Creates and initializes new one when no cart found.
        /// </summary>
        /// <param name="user">User to get current shopping cart for.</param>
        /// <param name="site">ID or the codename of the site.</param>
        /// <returns>Visitor's current shopping cart.</returns>
        /// <remarks>The cart is evaluated if it has been retrieved from database.</remarks>
        public ShoppingCartInfo GetCurrentShoppingCart(UserInfo user, SiteInfoIdentifier site)
        {
            var anonymizeCart = false;
            var evaluationIsNeeded = false;
            var siteId = site.ObjectID;

            var cart = GetCandidateCart(ref anonymizeCart, ref evaluationIsNeeded);

            cart = HandleCandidateCart(cart, user, siteId, anonymizeCart, ref evaluationIsNeeded);

            if (cart == null)
            {
                cart = CreateCart(user, siteId);
            }
            else
            {
                mCartStrategy.RefreshCart(cart);
                if (evaluationIsNeeded)
                {
                    EvaluateCartSafely(cart);
                }
            }

            RememberCart(cart);

            return cart;
        }


        /// <summary>
        /// Sets current shopping cart for current visitor.
        /// </summary>
        public void SetCurrentShoppingCart(ShoppingCartInfo cart)
        {
            RememberCart(cart);

            ECommerceContext.InvalidateCurrentShoppingCartCache();
        }


        /// <summary>
        /// Looks for current visitor's shopping cart candidate.
        /// </summary>
        /// <remarks>
        /// When the candidate cart is not found in cache, session is used to obtain shopping cart identifier.
        /// When the shopping cart identifier is not found in session, client storage is used to obtain it.
        /// </remarks>
        /// <param name="anonymize">Set to true if shopping cart identifier is taken from the client.</param>
        /// <param name="evaluationIsNeeded">Indicates that cart has to be evaluated afterwards.</param>
        /// <returns>Candidate cart or <c>null</c> when no candidate cart found.</returns>
        protected ShoppingCartInfo GetCandidateCart(ref bool anonymize, ref bool evaluationIsNeeded)
        {
            var cart = mCartCache.GetCart();
            if (cart != null)
            {
                return cart;
            }

            // Didn't find cart in cache -> need to evaluate cart again
            evaluationIsNeeded = true;

            var cartGuid = mCartSession.GetCartGuid();
            cart = mCartRepository.GetCart(cartGuid);

            if (cart == null)
            {
                cartGuid = mClientStorage.GetCartGuid();
                cart = mCartRepository.GetCart(cartGuid);

                // When processing a 404 error, the Session is always null.
                // Anonymize the cart only if session exists, to prevent cart cleanup when some resource returns a 404 error.
                anonymize = (CMSHttpContext.Current?.Session != null);
            }

            return cart;
        }


        /// <summary>
        /// Examines candidate shopping cart and prepares it to be used by the visitor.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Candidate shopping <paramref name="cart"/> can be dropped, taken over by user or replaced with user older cart from the repository.
        /// </para>
        /// <para>
        /// Candidate shopping <paramref name="cart"/> is dropped if it is not usable on given <paramref name="site"/>
        /// or <paramref name="user"/> can not take over the cart according to <see cref="mCartStrategy"/>.
        /// </para>
        /// </remarks>
        /// <param name="cart">Candidate shopping cart. When <c>null</c>, an existing cart from <see cref="mCartRepository"/> is retrieved.</param>
        /// <param name="user">User in which context is the candidate cart examined.</param>
        /// <param name="site">ID or code name of the site.</param>
        /// <param name="anonymizeCart">Private data are cleared when <c>true</c>. Private data are not cleared when the cart is replaced with user's own cart.</param>
        /// <param name="evaluationIsNeeded">Indicates that cart has to be evaluated afterwards.</param>
        /// <returns>Shopping cart which is suitable for shopping by <paramref name="user"/> on <paramref name="site"/>. Returns <c>null</c> when no such cart found.</returns>
        /// <seealso cref="HandleForeignCart"/>
        /// <seealso cref="ICurrentShoppingCartStrategy.CartCanBeUsedOnSite"/>
        protected ShoppingCartInfo HandleCandidateCart(ShoppingCartInfo cart, UserInfo user, SiteInfoIdentifier site, bool anonymizeCart, ref bool evaluationIsNeeded)
        {
            if ((cart != null) && !mCartStrategy.CartCanBeUsedOnSite(cart, site))
            {
                cart = null;
            }

            if (cart == null)
            {
                cart = GetExistingCart(user, site);
            }
            else if (cart.ShoppingCartUserID != user.UserID)
            {
                cart = HandleForeignCart(cart, user, site, anonymizeCart, ref evaluationIsNeeded);
            }

            return cart;
        }


        /// <summary>
        /// Gets and prepares user's shopping cart from the repository.
        /// </summary>
        /// <param name="user">User for which the shopping cart is returned.</param>
        /// <param name="site">ID or code name of the site to look for cart.</param>
        /// <returns>User's shopping cart retrieved from the <see cref="mCartRepository"/> or <c>null</c> when not found.</returns>
        protected ShoppingCartInfo GetExistingCart(UserInfo user, SiteInfoIdentifier site)
        {
            var cart = mCartRepository.GetUsersCart(user, site);

            // Recalculate cart with up-to-date values
            EvaluateCartSafely(cart);

            return cart;
        }


        /// <summary>
        /// Handles candidate shopping cart which does not belong to <paramref name="user"/>.
        /// </summary>
        /// <remarks>
        /// Candidate shopping cart is dropped when it can not be taken over by <paramref name="user"/>.
        /// </remarks>
        /// <param name="cart">Candidate shopping cart not belonging to <paramref name="user"/>.</param>
        /// <param name="user">User in which context is the candidate cart examined.</param>
        /// <param name="site">ID or code name of the site.</param>
        /// <param name="anonymizeCart">Private data are cleared when <c>true</c>.</param>
        /// <param name="evaluationIsNeeded">Indicates that cart has to be evaluated afterwards.</param>
        /// <returns>Shopping cart which is suitable for shopping by <paramref name="user"/> on <paramref name="site"/>. Returns <c>null</c> when cart can not be taken over.</returns>
        /// <seealso cref="ICurrentShoppingCartStrategy.UserCanTakeOverCart"/>
        /// <seealso cref="ICurrentShoppingCartStrategy.TakeOverCart"/>
        /// <seealso cref="ICurrentShoppingCartStrategy.AnonymizeShoppingCart"/>
        protected ShoppingCartInfo HandleForeignCart(ShoppingCartInfo cart, UserInfo user, SiteInfoIdentifier site, bool anonymizeCart, ref bool evaluationIsNeeded)
        {
            if (!mCartStrategy.UserCanTakeOverCart(cart, user))
            {
                return null;
            }

            if (!user.IsPublic())
            {
                cart = ProcessStoredCarts(cart, user, site, ref anonymizeCart);

                mCartStrategy.TakeOverCart(cart, user);

                // Cart might have been assigned to another user
                evaluationIsNeeded = true;
            }

            if (anonymizeCart)
            {
                mCartStrategy.AnonymizeShoppingCart(cart);

                // Customer-related data might have been cleaned
                evaluationIsNeeded = true;

                mCartRepository.SetCart(cart);
            }

            return cart;
        }


        /// <summary>
        /// Handles priority of candidate shopping <paramref name="cart"/> over shopping cart already stored for <paramref name="user"/> in the <see cref="mCartRepository"/>.
        /// </summary>
        /// <param name="cart">Candidate shopping cart.</param>
        /// <param name="user">User for which is the preference examined.</param>
        /// <param name="site">Site on which is the preference examined.</param>
        /// <param name="anonymizeCart">Flag set to <c>true</c> when candidate shopping cart is used. </param>
        /// <returns>Preferred shopping cart.</returns>
        protected ShoppingCartInfo ProcessStoredCarts(ShoppingCartInfo cart, UserInfo user, SiteInfoIdentifier site, ref bool anonymizeCart)
        {
            if (mCartStrategy.PreferStoredCart(cart, user))
            {
                cart = GetExistingCart(user, site) ?? cart;
            }
            else
            {
                mCartRepository.DeleteUsersCart(user, site);

                anonymizeCart = true;
            }

            return cart;
        }


        /// <summary>
        /// Creates new cart for <paramref name="user"/> on <paramref name="site"/> using <see cref="mCartFactory"/>.
        /// </summary>
        /// <param name="user">User for who the cart is created.</param>
        /// <param name="site">ID or site codename where the cart is created.</param>
        /// <returns>Shopping cart prepared to be used by <paramref name="user"/>.</returns>
        protected ShoppingCartInfo CreateCart(UserInfo user, SiteInfoIdentifier site)
        {
            var cart = mCartFactory.CreateCart(site, user);

            mCartStrategy.TakeOverCart(cart, user);

            return cart;
        }


        /// <summary>
        /// Stores given shopping cart into cache and carts identifier to client's storage.
        /// </summary>
        /// <param name="cart">Shopping cart to be remembered.</param>
        protected void RememberCart(ShoppingCartInfo cart)
        {
            mCartCache.StoreCart(cart);

            var cartGuid = cart?.ShoppingCartGUID ?? Guid.Empty;

            mClientStorage.SetCartGuid(cartGuid);
            mCartSession.SetCartGuid(cartGuid);
        }


        /// <summary>
        /// Stores shopping cart to cache temporarily and then evaluates it.
        /// This is prevention from infinite recursion of cart evaluation in case that macro using <see cref="ECommerceContext.CurrentShoppingCart"/> is resolved during evaluation.
        /// </summary>
        private void EvaluateCartSafely(ShoppingCartInfo cart)
        {
            if (cart != null)
            {
                mCartCache.StoreCart(cart);
                cart.Evaluate();
            }
        }
    }
}
