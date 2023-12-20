using CMS.DataEngine;

namespace CMS.Localization
{
    /// <summary>
    /// Class providing ResourceTranslationInfo management.
    /// </summary>
    public class ResourceTranslationInfoProviderBase<TProvider> : AbstractInfoProvider<ResourceTranslationInfo, TProvider>
        where TProvider : ResourceTranslationInfoProviderBase<TProvider>, new()
    {
        #region "Public methods - Basic"

        /// <summary>
        /// Returns a query for all the ResourceTranslationInfo objects.
        /// </summary>
        public static ObjectQuery<ResourceTranslationInfo> GetResourceTranslations()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns ResourceTranslationInfo with specified ID.
        /// </summary>
        /// <param name="id">ResourceTranslationInfo ID</param>
        public static ResourceTranslationInfo GetResourceTranslationInfo(int id)
        {
            return ProviderObject.GetInfoById(id);
        }


        /// <summary>
        /// Sets (updates or inserts) specified ResourceTranslationInfo.
        /// </summary>
        /// <param name="infoObj">ResourceTranslationInfo to be set</param>
        public static void SetResourceTranslationInfo(ResourceTranslationInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified ResourceTranslationInfo.
        /// </summary>
        /// <param name="infoObj">ResourceTranslationInfo to be deleted</param>
        public static void DeleteResourceTranslationInfo(ResourceTranslationInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes ResourceTranslationInfo with specified ID.
        /// </summary>
        /// <param name="id">ResourceTranslationInfo ID</param>
        public static void DeleteResourceTranslationInfo(int id)
        {
            ResourceTranslationInfo infoObj = GetResourceTranslationInfo(id);
            DeleteResourceTranslationInfo(infoObj);
        }

        #endregion
    }
}