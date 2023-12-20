using System;

using CMS.Base;
using CMS.Helpers;
using CMS.DataEngine;

using SystemIO = System.IO;

namespace CMS.Synchronization
{
    /// <summary>
    /// Provides methods for providing object hierarchy XML
    /// </summary>
    internal static class ObjectXmlHelper
    {
        /// <summary>
        /// Executes the given action using with emptied XML cache for the given object but allowing caching of ObjectXmlHelper data during the action. 
        /// </summary>
        /// <param name="infoObj">Info object</param>
        /// <param name="action">Action to execute</param>
        public static void ExecuteWithEmptyXMLCacheForObject(BaseInfo infoObj, Action action)
        {
            var storageKey = GetObjectXmlCacheKey(infoObj);

            RequestStockHelper.ExecuteWithEmptyStorage(action, storageKey, false);
        }


        /// <summary>
        /// Gets a cache key for the object XML
        /// </summary>
        /// <param name="infoObj">Info object</param>
        private static string GetObjectXmlCacheKey(GeneralizedInfo infoObj)
        {
            return String.Format("objectdata|{0}|{1}", infoObj.TypeInfo.ObjectType, infoObj.ObjectID);
        }


        /// <summary>
        /// Returns XML representation of given instance of info object.
        /// Caches the object XML for the given settings in current thread/request.
        /// Use <see cref="AbstractStockHelper{RequestStockHelper}.ExecuteWithEmptyStorage(Action, string, bool)" /> wrapper to execute a block of code with its own cache.
        /// </summary>
        /// <param name="settings">Export settings</param>
        /// <param name="infoObj">Info object to export</param>
        /// <returns>XML representation of given instance of info object.</returns>
        public static string GetObjectXml(SynchronizationObjectSettings settings, BaseInfo infoObj)
        {
            string xml;
            string storageKey = GetObjectXmlCacheKey(infoObj);
            string key = settings.RequestStockKey;

            // Try get cached value
            if (infoObj.Generalized.ObjectID > 0)
            {
                xml = GetObjectXmlFromCache(settings, storageKey, key);

                if (xml != null)
                {
                    return xml;
                }
            }

            xml = GetObjectXmlInternal(settings, infoObj);

            CacheObjectXml(settings, xml, storageKey, key);

            return xml;
        }


        /// <summary>
        /// Gets the object XML based on the given settings
        /// </summary>
        /// <param name="settings">Settings</param>
        /// <param name="infoObj">Info object</param>
        private static string GetObjectXmlInternal(SynchronizationObjectSettings settings, GeneralizedInfo infoObj)
        {
            using (var stream = new SystemIO.MemoryStream())
            {
                using (var writer = IO.StreamWriter.New(stream))
                {
                    var objWriter = new ObjectXmlWriter(writer, settings);

                    objWriter.WriteObjectXml(infoObj);

                    writer.Flush();
                    stream.Position = 0;

                    using (var reader = IO.StreamReader.New(stream))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
        }


        /// <summary>
        /// Puts the result of the export to the cache (including TranslationHelper if present).
        /// </summary>
        /// <param name="settings">Export settings object</param>
        /// <param name="export">Exported data</param>
        /// <param name="storageKey">Storage key</param>
        /// <param name="key">Cache key</param>
        private static void CacheObjectXml(SynchronizationObjectSettings settings, string export, string storageKey, string key)
        {
            if (CMSActionContext.CurrentUseCacheForSynchronizationXMLs)
            {
                RequestStockHelper.AddToStorage(storageKey, key, export, false);

                // Cache also TH if present
                if (settings.TranslationHelper != null)
                {
                    RequestStockHelper.AddToStorage(storageKey, key + "|TH", settings.TranslationHelper, false);
                }
            }
        }


        /// <summary>
        /// Tries find exported data and TranslationHelper in cache.
        /// </summary>
        /// <param name="settings">Export settings object</param>
        /// <param name="storageKey">Storage key</param>
        /// <param name="key">Cache key</param>
        private static string GetObjectXmlFromCache(SynchronizationObjectSettings settings, string storageKey, string key)
        {
            if (!CMSActionContext.CurrentUseCacheForSynchronizationXMLs)
            {
                return null;
            }

            // Try to find TranslationHelper in the cache
            if (settings.TranslationHelper != null)
            {
                // Try to get also TranslationHelper from cache
                TranslationHelper helper = (TranslationHelper)RequestStockHelper.GetItem(storageKey, key + "|TH", false);
                if (helper != null)
                {
                    settings.TranslationHelper = helper;
                }
                else
                {
                    // If TH should be filled but was not found in the cache abort cache retrieval
                    return null;
                }
            }

            // Try to get XML from request cache
            return (string)RequestStockHelper.GetItem(storageKey, key, false);
        }
    }
}
