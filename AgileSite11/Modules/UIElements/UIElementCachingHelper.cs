using System;

using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.Modules
{
    /// <summary>
    /// Provides support for caching <see cref="UIElementInfo"/> objects.
    /// </summary>
    internal static class UIElementCachingHelper
    {
        private const string CHILD_ELEMENTS_CACHE_NAME = "ChildElementsInfoDataSet|{0}|{1}|{2}";


        /// <summary>
        /// Returns set of children UI elements.
        /// </summary>
        /// <param name="resourceName">Name of the resource (module)</param>
        /// <param name="elementName">CodeName of the UIElement</param>
        /// <param name="loadMethod">Method for element loading</param>
        /// <param name="cacheDurationInMinutes">Cache durability in minutes</param>
        internal static InfoDataSet<UIElementInfo> GetElements(string resourceName, string elementName, Func<InfoDataSet<UIElementInfo>> loadMethod, int cacheDurationInMinutes = 10)
        {
            InfoDataSet<UIElementInfo> elements = null;

            using (var cachedSection = new CachedSection<InfoDataSet<UIElementInfo>>(ref elements, cacheDurationInMinutes, true, GetCacheItemName(resourceName, elementName)))
            {
                if (cachedSection.LoadData)
                {
                    elements = loadMethod.Invoke();

                    if (cachedSection.Cached)
                    {
                        cachedSection.CacheDependency = GetCacheDependencies(resourceName, elementName);
                    }

                    cachedSection.Data = elements;
                }
            }

            return elements;
        }


        private static string GetCacheItemName(string resourceName, string elementName)
        {
            return string.Format(CHILD_ELEMENTS_CACHE_NAME, UIElementInfo.OBJECT_TYPE, resourceName, elementName);
        }


        private static CMSCacheDependency GetCacheDependencies(string resourceName, string elementName)
        {
            return CacheHelper.GetCacheDependency(new[]
            {
                GetNameCacheDependencyKey(ResourceInfo.OBJECT_TYPE, resourceName),
                GetNameCacheDependencyKey(UIElementInfo.OBJECT_TYPE, elementName),
                GetChildrenCacheDependencyKey(ResourceInfo.OBJECT_TYPE, resourceName)
            });
        }


        private static string GetNameCacheDependencyKey(string objectType, string name)
        {
            return $"{objectType}|byname|{name}";
        }


        private static string GetChildrenCacheDependencyKey(string objectType, string name)
        {
            var parent = ProviderHelper.GetInfoByName(objectType, name);
            return $"{objectType}|byid|{parent?.Generalized?.ObjectID}|children";
        }
    }
}
