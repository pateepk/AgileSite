using System;
using System.Reflection;

namespace CMS.DataEngine
{
    /// <summary>
    /// Provides functionality for info object provider loading.
    /// </summary>
    public static class InfoProviderLoader
    {
        /// <summary>
        /// Load provider delegate.
        /// </summary>
        public delegate void OnLoadProvider(object sender, LoadProviderEventArgs e);


        /// <summary>
        /// Event to load provider
        /// </summary>
        public static event OnLoadProvider LoadProvider;


        /// <summary>
        /// Returns provider by its object type.
        /// </summary>
        /// <typeparam name="TProvider">Info object provider type</typeparam>
        /// <param name="objectType">Provider object type</param>
        /// <param name="exceptionIfNotFound">
        /// If true, an exception is thrown if the provider is not found.
        /// If false, the null is returned if provider is not found.
        /// </param>
        public static TProvider GetInfoProvider<TProvider>(string objectType, bool exceptionIfNotFound = true)
            where TProvider : class, IInfoProvider
        {
            return GetInfoProvider(objectType, exceptionIfNotFound) as TProvider;
        }


        /// <summary>
        /// Gets the loaded provider by its object type.
        /// </summary>
        /// <param name="objectType">Provider object type</param>
        /// <param name="exceptionIfNotFound">If true, an exception is thrown if the provider is not found. If false, the null is returned if provider is not found</param>
        public static IInfoProvider GetInfoProvider(string objectType, bool exceptionIfNotFound = true)
        {
            var provider = InfoProviderCache.GetInfoProvider(objectType);
            if (!IsProviderValid(provider))
            {
                // Try to load provider by event, this part handles the providers with dynamic types
                var arguments = new LoadProviderEventArgs(objectType);
                LoadProvider?.Invoke(null, arguments);

                if (arguments.ProviderLoaded)
                {
                    provider = arguments.Provider;
                    if (IsProviderValid(provider))
                    {
                        InfoProviderCache.Register(objectType, provider);
                    }
                }
                else
                {
                    // Get the provider type from the object type
                    var typeInfo = ObjectTypeManager.GetTypeInfo(objectType);
                    if (typeInfo != null)
                    {
                        var providerType = typeInfo.ProviderType;
                        if (providerType == null)
                        {
                            if (objectType != typeInfo.OriginalObjectType)
                            {
                                // Get original type provider for types derived from other type, such types share provider with the original type
                                provider = GetInfoProvider(typeInfo.OriginalObjectType, false);
                            }
                        }
                        else
                        {
                            provider = GetProviderObjectByReflection(providerType);
                        }

                        InfoProviderCache.Register(objectType, provider);
                    }
                }
            }

            return CheckResultValidity(objectType, exceptionIfNotFound, provider);
        }


        private static IInfoProvider GetProviderObjectByReflection(Type providerType)
        {
            if (providerType == null)
            {
                throw new ArgumentNullException(nameof(providerType));
            }

            PropertyInfo pi = null;

            var originalProviderType = providerType;

            while (providerType != null)
            {
                // Execute the dummy method to initialize the provider
                pi = providerType.GetProperty("ProviderObject", BindingFlags.Static | BindingFlags.Public);
                if (pi != null)
                {
                    break;
                }

                providerType = providerType.BaseType;
            }

            if (pi == null)
            {
                throw new InvalidOperationException($"Unable to find property ProviderObject in the provider of type '{originalProviderType}'. Cannot initialize the provider.");
            }

            return (IInfoProvider)pi.GetValue(null, null);
        }


        /// <summary>
        /// Checks if result is not empty and valid, otherwise throws an exception or null (based <paramref name="exceptionIfNotFound"/>).
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="exceptionIfNotFound">Indicates if exception should be thrown</param>
        /// <param name="provider">Info provider instance</param>
        private static IInfoProvider CheckResultValidity(string objectType, bool exceptionIfNotFound, IInfoProvider provider)
        {
            if (IsProviderValid(provider))
            {
                return provider;
            }

            if (exceptionIfNotFound)
            {
                string message = provider == null ? "The object type '{0}' is missing the provider type configuration." : "The object type '{0}' has invalid provider.";

                throw new InvalidOperationException(String.Format(message, objectType));
            }

            return null;
        }


        private static bool IsProviderValid(IInfoProvider provider)
        {
            var validableProvider = provider as IInternalProvider;
            return validableProvider != null && validableProvider.IsValid;
        }
    }
}
