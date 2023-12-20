using System.Linq;

using CMS.DataEngine;

namespace CMS.Localization
{
    /// <summary>
    /// Resource translation management.
    /// </summary>
    public class ResourceTranslationInfoProvider : ResourceTranslationInfoProviderBase<ResourceTranslationInfoProvider>
    {
        #region "Overriden methods"

        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(ResourceTranslationInfo info)
        {
            CheckObject(info);

            base.SetInfo(info);

            // Update hash table
            var completeStringKey = GetCompleteStringKey(info);
            ResourceStringInfoProvider.UpdateInHashTable(completeStringKey, info.TranslationText);
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(ResourceTranslationInfo info)
        {
            if (info != null)
            {
                // Update hash table
                var completeStringKey = GetCompleteStringKey(info);
                ResourceStringInfoProvider.DeleteFromHashTable(completeStringKey);
            }

            base.DeleteInfo(info);
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Gets the resource translation from the database.
        /// </summary>
        /// <param name="stringId">String ID</param>
        /// <param name="cultureId">Culture ID</param>
        public static ResourceTranslationInfo GetResourceTranslationInfo(int stringId, int cultureId)
        {
            if ((stringId <= 0) || (cultureId <= 0))
            {
                return null;
            }

            return GetResourceTranslations()
                .TopN(1)
                .Where("TranslationStringID", QueryOperator.Equals, stringId)
                .Where("TranslationCultureID", QueryOperator.Equals, cultureId)
                .FirstOrDefault();
        }


        /// <summary>
        /// Gets the complete resource string key for the given translation
        /// </summary>
        /// <param name="infoObj">Translation info</param>
        private static string GetCompleteStringKey(ResourceTranslationInfo infoObj)
        {
            string completeStringKey = null;

            var cultureInfo = CultureInfoProvider.GetCultureInfo(infoObj.TranslationCultureID);
            var resStringInfo = ResourceStringInfoProvider.GetResourceStringInfo(infoObj.TranslationStringID);

            if ((cultureInfo != null) && (resStringInfo != null))
            {
                completeStringKey = ResourceStringInfoProvider.GetCompleteKey(resStringInfo.StringKey, cultureInfo.CultureCode);
            }
            return completeStringKey;
        }

        #endregion
    }
}