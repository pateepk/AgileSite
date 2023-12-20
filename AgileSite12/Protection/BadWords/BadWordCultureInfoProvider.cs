using System;
using System.Linq;

using CMS.DataEngine;

namespace CMS.Protection
{
    /// <summary>
    /// Class providing BadWordCultureInfo management.
    /// </summary>
    public class BadWordCultureInfoProvider : AbstractInfoProvider<BadWordCultureInfo, BadWordCultureInfoProvider>
    {
        #region "Public static methods"

        /// <summary>
        /// Returns a query for all the <see cref="BadWordCultureInfo"/> objects.
        /// </summary>
        public static ObjectQuery<BadWordCultureInfo> GetBadWordCultures()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns the BadWordCultureInfo structure for the specified BadWordCulture.
        /// </summary>
        /// <param name="wordId">Word ID</param>
        /// <param name="cultureId">Culture ID</param>
        public static BadWordCultureInfo GetBadWordCultureInfo(int wordId, int cultureId)
        {
            return ProviderObject.GetBadWordCultureInfoInternal(wordId, cultureId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified BadWordCulture.
        /// </summary>
        /// <param name="badWordCulture">BadWordCulture object to set</param>
        public static void SetBadWordCultureInfo(BadWordCultureInfo badWordCulture)
        {
            ProviderObject.SetBadWordCultureInfoInternal(badWordCulture);
        }


        /// <summary>
        /// Deletes specified BadWordCulture.
        /// </summary>
        /// <param name="infoObj">BadWordCulture object</param>
        public static void DeleteBadWordCultureInfo(BadWordCultureInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified BadWordCulture.
        /// </summary>
        /// <param name="wordId">Word ID</param>
        /// <param name="cultureId">Culture ID</param>
        protected static void DeleteBadWordCultureInfo(int wordId, int cultureId)
        {
            BadWordCultureInfo infoObj = GetBadWordCultureInfo(wordId, cultureId);
            DeleteBadWordCultureInfo(infoObj);
        }


        /// <summary>
        /// Adds bad word to specified culture and cleares bad word hashtables.
        /// </summary>
        /// <param name="wordId">ID of bad word</param>
        /// <param name="cultureId">ID of culture</param>
        public static void AddBadWordToCulture(int wordId, int cultureId)
        {
            BadWordCultureInfo bwcObject = new BadWordCultureInfo();
            bwcObject.CultureID = cultureId;
            bwcObject.WordID = wordId;

            SetBadWordCultureInfo(bwcObject);
        }


        /// <summary>
        /// Removes bad word from specified culture and cleares bad word hashtables.
        /// </summary>
        /// <param name="wordId">ID of bad word</param>
        /// <param name="cultureId">ID of culture</param>
        public static void RemoveBadWordFromCulture(int wordId, int cultureId)
        {
            DeleteBadWordCultureInfo(wordId, cultureId);
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Returns the BadWordCultureInfo object.
        /// </summary>
        /// <param name="wordId">Word ID</param>
        /// <param name="cultureId">Culture ID</param>
        protected virtual BadWordCultureInfo GetBadWordCultureInfoInternal(int wordId, int cultureId)
        {
            var where = new WhereCondition().WhereEquals("WordID", wordId).WhereEquals("CultureID", cultureId);

            return ProviderObject.GetObjectQuery().Where(where).TopN(1).BinaryData(true).FirstOrDefault();
        }


        /// <summary>
        /// Sets (updates or inserts) specified BadWordCulture object.
        /// </summary>
        /// <param name="badWordCulture">BadWordCulture to set</param>
        protected virtual void SetBadWordCultureInfoInternal(BadWordCultureInfo badWordCulture)
        {
            if (badWordCulture != null)
            {
                // Check IDs
                if ((badWordCulture.WordID <= 0) || (badWordCulture.CultureID <= 0))
                {
                    throw new Exception("[BadWordCultureInfoProvider.SetBadWordCultureInfo]: Object IDs not set.");
                }

                // Get existing
                BadWordCultureInfo existing = GetBadWordCultureInfoInternal(badWordCulture.WordID, badWordCulture.CultureID);
                if (existing != null)
                {
                    badWordCulture.Generalized.UpdateData();
                }
                else
                {
                    badWordCulture.Generalized.InsertData();
                }

                // Clear hashtables of bad words
                if (badWordCulture.Generalized.TouchCacheDependencies)
                {
                    ClearBadWordProviderHashtables();
                }
            }
            else
            {
                throw new Exception("[BadWordCultureInfoProvider.SetBadWordCultureInfo]: No BadWordCultureInfo object set.");
            }
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(BadWordCultureInfo info)
        {
            if (info != null)
            {
                base.DeleteInfo(info);

                // Clear hashtables of bad words
                if (info.Generalized.TouchCacheDependencies)
                {
                    ClearBadWordProviderHashtables();
                }
            }
        }


        /// <summary>
        /// Clears hashtables in bad words provider.
        /// </summary>
        private static void ClearBadWordProviderHashtables()
        {
            ProviderHelper.ClearHashtables(BadWordInfo.OBJECT_TYPE, true);
        }

        #endregion
    }
}