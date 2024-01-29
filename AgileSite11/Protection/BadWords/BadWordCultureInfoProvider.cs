using System;

using CMS.DataEngine;
using CMS.DataEngine.Query;
using CMS.Localization;

namespace CMS.Protection
{
    using TypedDataSet = InfoDataSet<BadWordCultureInfo>;

    /// <summary>
    /// Class providing BadWordCultureInfo management.
    /// </summary>
    public class BadWordCultureInfoProvider : AbstractInfoProvider<BadWordCultureInfo, BadWordCultureInfoProvider>
    {
        #region "Public static methods"

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


        /// <summary>
        /// Gets cultures of specified bad word.
        /// </summary>
        /// <param name="wordId">ID of bad word</param>
        [Obsolete("Use CMS.DataEngine.ObjectQuery<BadWordCultureInfo> instead")]
        public static InfoDataSet<CultureInfo> GetWordCultures(int wordId)
        {
            return ProviderObject.GetWordCulturesInternal(wordId);
        }


        /// <summary>
        /// Gets all cultures records.
        /// </summary>
        /// <param name="where">Where condition to filter data</param>
        /// <param name="orderBy">Order by statement</param>
        [Obsolete("Use CMS.DataEngine.ObjectQuery<BadWordCultureInfo> instead")]
        public static TypedDataSet GetBadWordCultures(string where, string orderBy)
        {
            return ProviderObject.GetObjectQuery().Where(where).OrderBy(orderBy).BinaryData(true).TypedResult;
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

            return ProviderObject.GetObjectQuery().Where(where).TopN(1).BinaryData(true).FirstObject;
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
        /// Gets cultures of specified bad word.
        /// </summary>
        /// <param name="wordId">ID of bad word</param>
        [Obsolete("Use CMS.DataEngine.ObjectQuery<BadWordCultureInfo> instead")]
        protected virtual InfoDataSet<CultureInfo> GetWordCulturesInternal(int wordId)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@WordID", wordId);
            parameters.EnsureDataSet<CultureInfo>();

            return ConnectionHelper.ExecuteQuery("badwords.wordculture.selectculturebywordid", parameters).As<CultureInfo>();
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