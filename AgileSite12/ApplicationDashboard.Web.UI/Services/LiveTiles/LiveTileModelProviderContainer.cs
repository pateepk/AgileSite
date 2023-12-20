using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CMS.Modules;

namespace CMS.ApplicationDashboard.Web.UI
{
    internal static class LiveTileModelProviderContainer
    {
        private static readonly Dictionary<string, Lazy<ILiveTileModelProvider>> mLazyProviders = new Dictionary<string, Lazy<ILiveTileModelProvider>>();
        /// <summary>
        /// Registers the live model provider factory for given resource and UI element names.
        /// </summary>
        /// <remarks>
        /// If provider is already registered for resource and element name, it is replaced with a new one.
        /// </remarks>
        /// <param name="resourceName">Name of the resource</param>
        /// <param name="elementName">Name of the UI element</param>
        /// <param name="liveTileModelProviderFactory">Factory for creating an implementation of <see cref="ILiveTileModelProvider" /> interface</param>
        /// <exception cref="ArgumentNullException"><paramref name="resourceName"/> or <paramref name="elementName"/> or <paramref name="liveTileModelProviderFactory"/> is null</exception>
        public static void RegisterProviderFactory(string resourceName, string elementName, Func<ILiveTileModelProvider> liveTileModelProviderFactory)
        {
            if (resourceName == null)
            {
                throw new ArgumentNullException("resourceName");
            }

            if (elementName == null)
            {
                throw new ArgumentNullException("elementName");
            }

            if (liveTileModelProviderFactory == null)
            {
                throw new ArgumentNullException("liveTileModelProviderFactory");
            }

            var providerKey = string.Format("{0}|{1}", resourceName, elementName);

            mLazyProviders[providerKey] = new Lazy<ILiveTileModelProvider>(liveTileModelProviderFactory, true);
        }


        /// <summary>
        /// Gets live model provider for given UI element object.
        /// </summary>
        /// <remarks>
        /// Provider has to be registered in advance with <see cref="RegisterProviderFactory"/>.
        /// </remarks>
        /// <param name="uiElement">UI element object info</param>
        /// <exception cref="ArgumentNullException"><paramref name="uiElement"/> is null</exception>
        /// <returns>Live model provider implementing the <see cref="ILiveTileModelProvider"/> interface</returns>
        public static ILiveTileModelProvider GetProvider(UIElementInfo uiElement)
        {
            if (uiElement == null)
            {
                throw new ArgumentNullException("uiElement");
            }

            string resourceName = ResourceInfoProvider.GetResourceInfo(uiElement.ElementResourceID).ResourceName;
            return GetProvider(resourceName, uiElement.ElementName);
        }


        /// <summary>
        /// Gets live model provider for given resource ID and UI element name. If such provider does not exist, returns null.
        /// </summary>
        /// <remarks>
        /// Provider has to be registered in advance with <see cref="RegisterProviderFactory"/>.
        /// </remarks>
        /// <param name="resourceName">Name of the resource</param>
        /// <param name="elementName">Name of the UI element</param>
        /// <returns>Live model provider implementing the <see cref="ILiveTileModelProvider"/> interface, if exists; otherwise, null</returns>
        private static ILiveTileModelProvider GetProvider(string resourceName, string elementName)
        {
            string providerKey = string.Format("{0}|{1}", resourceName, elementName);

            Lazy<ILiveTileModelProvider> providerFactory;

            if (mLazyProviders.TryGetValue(providerKey, out providerFactory))
            {
                return providerFactory.Value;
            }

            return null;
        }
    }
}
