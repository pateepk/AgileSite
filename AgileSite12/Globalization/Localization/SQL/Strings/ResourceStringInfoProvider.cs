using System;
using System.Collections.Generic;
using System.Data;

using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.Localization
{
    /// <summary>
    /// Provides access to resource strings stored in db.
    /// </summary>
    public class ResourceStringInfoProvider : ResourceStringInfoProviderBase<ResourceStringInfoProvider>
    {
        #region "Variables"

        // Contains Resources pre-loaded in memory. Table of [cultureCode.ToLower + "." + stringKey.ToLower] -> [Text]
        private static readonly CMSStatic<ProviderDictionaryCollection> mResources = new CMSStatic<ProviderDictionaryCollection>();

        // Table lock for loading.
        private static readonly object tableLock = new object();

        #endregion


        #region "Properties"

        /// <summary>
        /// Contains Resources pre-loaded in memory. Table of [cultureCode.ToLower + "." + stringKey.ToLower] -> [Text]
        /// </summary>
        private static ProviderDictionaryCollection Resources
        {
            get
            {
                if (ProviderHelper.LoadTables(mResources))
                {
                    lock (tableLock)
                    {
                        if (ProviderHelper.LoadTables(mResources))
                        {
                            // Create hashtables if not present
                            LoadHashtableEnum loadingType = ProviderHelper.LoadHashTables(ResourceStringInfo.OBJECT_TYPE, LoadHashtableEnum.None);
                            var resourceCollection = new ProviderDictionaryCollection(ResourceStringInfo.OBJECT_TYPE, loadingType, LoadResourceStrings)
                            {
                                StringValues = new ProviderDictionary<string, string>(ResourceStringInfo.OBJECT_TYPE, "StringKey;CultureCode", StringComparer.InvariantCultureIgnoreCase, true)
                            };

                            // Load default items
                            resourceCollection.LoadDefaultItems();
                            mResources.Value = resourceCollection;
                        }
                    }
                }

                return mResources;
            }
        }


        /// <summary>
        /// Returns default UI culture.
        /// </summary>
        public static string DefaultUICulture
        {
            get
            {
                return CultureHelper.DefaultUICultureCode;
            }
        }


        /// <summary>
        /// Indicates if database is available and provider is ready to obtain translation from the database.  
        /// </summary>
        protected virtual bool DatabaseAvailable
        {
            get
            {
                return ConnectionHelper.IsConnectionStringInitialized;
            }
        }

        #endregion


        #region "Overriden methods"

        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(ResourceStringInfo info)
        {
            CheckObject(info);

            bool update = (info.StringID > 0);
            bool keyChanged = update && info.ItemChanged("StringKey");

            // Log staging tasks synchronously
            using (new CMSActionContext { AllowAsyncActions = false })
            {
                base.SetInfo(info);

                // Clear hashtables when resource string key changed
                if (keyChanged)
                {
                    ClearHashtables(true);
                }

                // Update translation
                UpdateTranslation(info, update);
            }
        }


        /// <summary>
        /// Updates the translation of the given resource string
        /// </summary>
        /// <param name="infoObj">Resource string</param>
        /// <param name="update">If true, the resource string is updated, otherwise a new one is inserted</param>
        private static void UpdateTranslation(ResourceStringInfo infoObj, bool update)
        {
            // Update translation
            var text = infoObj.TranslationText;
            var cultureCode = infoObj.CultureCode;

            var containsTranslation = (text != null) && !String.IsNullOrEmpty(cultureCode);
            if (containsTranslation)
            {
                // Get culture ID
                int cultureId = CultureInfoProvider.GetCultureID(cultureCode);
                if (cultureId == 0)
                {
                    throw new Exception("[SqlResourceManager.SetResourceStringInfo]: Culture '" + cultureCode + "' not found.");
                }

                ResourceTranslationInfo rti = null;

                if (update)
                {
                    // Get existing in case of update
                    rti = ResourceTranslationInfoProvider.GetResourceTranslationInfo(infoObj.StringID, cultureId);
                }

                if (rti == null)
                {
                    // Create new translation if none found or new
                    rti = new ResourceTranslationInfo();
                    rti.TranslationStringID = infoObj.StringID;
                }

                rti.TranslationText = text;
                rti.TranslationCultureID = cultureId;

                // Insert / update the translation
                ResourceTranslationInfoProvider.SetResourceTranslationInfo(rti);
            }
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(ResourceStringInfo info)
        {
            if (info != null)
            {
                // Update all culture codes in hashtable
                var cultureCodes = GetResourceCultureCodes(info.StringKey);

                foreach (string cultureCode in cultureCodes)
                {
                    string completeKey = GetCompleteKey(info.StringKey, cultureCode);

                    DeleteFromHashTable(completeKey);
                }
            }

            base.DeleteInfo(info);
        }


        /// <summary>
        /// Clear hashtables.
        /// </summary>
        /// <param name="logTasks">If true, web farm tasks are logged</param>
        protected override void ClearHashtables(bool logTasks)
        {
            base.ClearHashtables(logTasks);

            // Accessing Resources property could lead to dictionary loading just to be cleared, this causes issue during TearDown.
            mResources.Value?.Clear(logTasks);
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns ResourceStringInfo by specified key and specified cultureCode. Always returns string object.
        /// </summary>
        /// <param name="key">Resource string key</param>
        /// <param name="cultureCode">Culture code</param>
        public static ResourceStringInfo GetResourceStringInfo(string key, string cultureCode)
        {
            // Get the string
            var rs = GetResourceStringInfo(key);
            if (rs == null)
            {
                rs = new ResourceStringInfo();
                rs.StringKey = key;
            }

            // Init text
            if (cultureCode != null)
            {
                rs.TranslationText = GetString(key, cultureCode);
                rs.CultureCode = cultureCode;
            }

            return rs;
        }


        /// <summary>
        /// Deletes resource string translation with specified StringKey and specified UI culture.
        /// If cultureCode is null, then the resource string will be deleted from all cultures, otherwise will be deleted only from specified culture.
        /// </summary>
        /// <param name="stringKey">Resource string key</param>
        /// <param name="cultureCode">Culture code</param>
        public static void DeleteResourceStringInfo(string stringKey, string cultureCode = null)
        {
            // Delete all strings when culture code is null
            if (cultureCode == null)
            {
                // Delete the resource string
                var rsi = GetResourceStringInfo(stringKey);
                if (rsi != null)
                {
                    DeleteResourceStringInfo(rsi);
                }
            }
            else
            {
                // Delete only translation
                var rsi = GetResourceStringInfo(stringKey);
                if (rsi != null)
                {
                    // Handle the event
                    using (var h = rsi.TypeInfo.Events.Update.StartEvent(rsi))
                    {
                        h.DontSupportCancel();

                        if (h.CanContinue())
                        {
                            // Get culture ID
                            int cultureId = CultureInfoProvider.GetCultureID(cultureCode);

                            // Delete the translation
                            var rti = ResourceTranslationInfoProvider.GetResourceTranslationInfo(rsi.StringID, cultureId);
                            if (rti != null)
                            {
                                ResourceTranslationInfoProviderBase<ResourceTranslationInfoProvider>.DeleteResourceTranslationInfo(rti);
                            }
                        }

                        // Finish the event
                        h.FinishEvent();
                    }
                }
            }
        }

        #endregion


        #region "Hashtable methods"

        /// <summary>
        /// Deletes translation from <see cref="ProviderDictionaryCollection.StringValues"/> hashtable.
        /// </summary>
        /// <param name="completeStringKey">Complete resource string key</param>
        internal static void DeleteFromHashTable(string completeStringKey)
        {
            if (!String.IsNullOrEmpty(completeStringKey))
            {
                Resources?.StringValues.Delete(completeStringKey);
            }
        }


        /// <summary>
        /// Adds translation to <see cref="ProviderDictionaryCollection.StringValues"/> hashtable.
        /// </summary>
        /// <param name="completeStringKey">Complete resource string key (e.g., "en-us.general.default")</param>
        /// <param name="text">Translation text</param>
        internal static void AddToHashTable(string completeStringKey, string text)
        {
            if (!String.IsNullOrEmpty(completeStringKey))
            {
                Resources?.StringValues.Add(completeStringKey, text);
            }
        }


        /// <summary>
        /// Updates translation in <see cref="ProviderDictionaryCollection.StringValues"/> hashtable.
        /// </summary>
        /// <param name="completeStringKey">Complete resource string key (e.g., "en-us.general.default")</param>
        /// <param name="text">Translation text</param>
        internal static void UpdateInHashTable(string completeStringKey, string text)
        {
            if (!String.IsNullOrEmpty(completeStringKey))
            {
                Resources?.StringValues.Update(completeStringKey, text);
            }
        }

        #endregion


        #region "GetString methods"

        /// <summary>
        /// Returns string by specified key and specified cultureCode.
        /// </summary>
        /// <param name="key">Resource string key</param>
        /// <param name="cultureCode">Resource string UICulture code</param>
        public static string GetString(string key, string cultureCode)
        {
            return GetString(key, cultureCode, key);
        }


        /// <summary>
        /// Returns string by specified key and specified cultureCode.
        /// </summary>
        /// <param name="key">Resource string key</param>
        /// <param name="cultureCode">Resource string UICulture code</param>
        /// <param name="defaultValue">Default value in case string was not found</param>
        /// <param name="useDefaultCulture">If true the default culture translation is used if translation in chosen culture doesn't exist</param>
        public static string GetString(string key, string cultureCode, string defaultValue, bool useDefaultCulture = true)
        {
            return ProviderObject.GetStringInternal(key, cultureCode, defaultValue, useDefaultCulture);
        }


        /// <summary>
        /// Returns string by specified key and specified cultureCode.
        /// </summary>
        /// <param name="key">Resource string key</param>
        /// <param name="cultureCode">Resource string UICulture code</param>
        public static string GetStringFromDB(string key, string cultureCode)
        {
            return ProviderObject.GetStringFromDBInternal(key, cultureCode);
        }


        /// <summary>
        /// Loads the specified generation of the objects.
        /// </summary>
        /// <param name="sender">Instance of <see cref="ProviderDictionaryCollection"/>.</param>
        /// <param name="parameter">Optional parameter</param>
        /// <returns>Returns true if the generation was not empty</returns>
        private static void LoadResourceStrings(ProviderDictionaryCollection sender, object parameter)
        {
            var ds = GetTranslatedStrings();

            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                // Process the translations
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    string stringKey = Convert.ToString(dr["StringKey"]);
                    string cultureCode = Convert.ToString(dr["CultureCode"]);
                    string text = Convert.ToString(dr["TranslationText"]);

                    string completeKey = GetCompleteKey(stringKey, cultureCode);
                    sender.StringValues[completeKey] = text;
                }

                // Dispose the source data
                ds.Dispose();
            }
        }


        /// <summary>
        /// Returns the translations
        /// </summary>
        private static ObjectQuery<ResourceStringInfo> GetTranslatedStrings()
        {
            return GetResourceStrings()
                .Columns("TranslationText", "StringKey", "CultureCode")
                .From("View_CMS_ResourceString_Joined")
                .WhereNotNull("TranslationText");
        }


        /// <summary>
        /// Returns complete resource string key.
        /// </summary>
        /// <param name="stringKey">Resource string key</param>
        /// <param name="cultureCode">Culture code</param>
        public static string GetCompleteKey(string stringKey, string cultureCode)
        {
            if ((stringKey == null) || (cultureCode == null))
            {
                return null;
            }

            return cultureCode.ToLowerInvariant() + "." + stringKey.ToLowerInvariant();
        }


        /// <summary>
        /// Returns true if translation of specified String key exist in specified Culture.
        /// </summary>
        /// <param name="stringKey">Resource string key</param>
        /// <param name="cultureCode">Resource string Culture code</param>
        public static bool TranslationExists(string stringKey, string cultureCode)
        {
            return GetString(stringKey, cultureCode, null, false) != null;
        }


        /// <summary>
        /// Gets list of culture codes for given key.
        /// </summary>
        /// <param name="key">Resource string key</param>
        public static List<string> GetResourceCultureCodes(string key)
        {
            if (key == null)
            {
                return null;
            }

            DataSet ds = GetTranslatedStrings()
                .Distinct()
                .Column("CultureCode")
                .Where("StringKey", QueryOperator.Equals, key);

            return DataHelper.GetStringValues(ds.Tables[0], "CultureCode");
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Returns string by specified key and specified cultureCode.
        /// </summary>
        /// <param name="key">Resource string key</param>
        /// <param name="cultureCode">Resource string UICulture code</param>
        /// <param name="defaultValue">Default value in case string was not found</param>
        /// <param name="useDefaultCulture">If true the default culture translation is used if translation in chosen culture doesn't exist</param>
        protected virtual string GetStringInternal(string key, string cultureCode, string defaultValue, bool useDefaultCulture = true)
        {
            if (!DatabaseAvailable)
            {
                return null;
            }

            string result;
            string completeKey = GetCompleteKey(key, cultureCode);

            // Try to find required string
            if (!Resources.StringValues.TryGetValue(completeKey, out result))
            {
                // Get local value from the database
                result = GetStringFromDB(key, cultureCode);
                AddToHashTable(completeKey, result);
            }

            // If not found, try to find the default culture
            if ((result == null) && (cultureCode != null) && !String.Equals(cultureCode, DefaultUICulture, StringComparison.OrdinalIgnoreCase) && useDefaultCulture)
            {
                result = GetString(key, DefaultUICulture, defaultValue);
            }

            // Return key if not found
            return result ?? defaultValue;
        }


        /// <summary>
        /// Returns string by specified key and specified cultureCode.
        /// </summary>
        /// <param name="key">Resource string key</param>
        /// <param name="cultureCode">Resource string UICulture code</param>
        protected virtual string GetStringFromDBInternal(string key, string cultureCode)
        {
            if (!DatabaseAvailable)
            {
                return null;
            }

            // Get the data
            return GetTranslatedStrings()
                .TopN(1)
                .Column("TranslationText")
                .Where("StringKey", QueryOperator.Equals, key)
                .Where("CultureCode", QueryOperator.Equals, cultureCode)
                .GetScalarResult<string>();
        }

        #endregion
    }
}
