using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

using CMS.Base;
using CMS.Core;
using CMS.Helpers;
using CMS.IO;

namespace CMS.DataEngine
{
    /// <summary>
    /// Provides access to settings keys.
    /// </summary>
    public class SettingsKeyInfoProvider : AbstractInfoProvider<SettingsKeyInfo, SettingsKeyInfoProvider>
    {
        #region "Events"

        /// <summary>
        /// Occurs when the settings key is changed.
        /// </summary>
        public static event EventHandler<SettingsKeyChangedEventArgs> OnSettingsKeyChanged;

        #endregion


        #region "Variables"

        private readonly object settingsLock = new object();

        // Constant value used for global keys identifier in settings key collection
        private const string GLOBAL_KEYS = "";

        private const string WEBFARM_TASK_CLEAR_SITE_HASHTABLES = "delete";
        private const string WEBFARM_TASK_CLEAR_ALL_HASHTABLES = "clearsettings";

        // Contains settings keys for given sites.
        private StringSafeDictionary<ProviderDictionaryCollection> mSettings;

        /// <summary>
        /// Executes when URL is requested from the settings
        /// </summary>
        public static URLHandler GetURL = new URLHandler();

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets the value that indicates whether the virtual objects are in deployment mode (reflects CMSDeploymentMode hidden setting).
        /// </summary>
        public static bool DeploymentMode
        {
            get
            {
                return GetBoolValue("CMSDeploymentMode");
            }
            set
            {
                SetGlobalValue("CMSDeploymentMode", value);
            }
        }


        /// <summary>
        /// Gets the value that indicates whether virtual path provider is running or deployment mode is set
        /// </summary>
        public static bool VirtualObjectsAllowed
        {
            get
            {
                return VirtualPathHelper.UsingVirtualPathProvider || DeploymentMode;
            }
        }


        /// <summary>
        /// Gets the collection of settings stored by site name.
        /// </summary>
        /// <remarks>
        /// Use this property directly only in cases where you don't need loaded values (e.g remove or update of cached values).
        /// If you want to read the values use <see cref="LoadSettings(string)"/> method instead.
        /// </remarks>
        private StringSafeDictionary<ProviderDictionaryCollection> Settings
        {
            get
            {
                var result = mSettings;
                if (result == null)
                {
                    lock (settingsLock)
                    {
                        result = mSettings;
                        if (result == null)
                        {
                            result = new StringSafeDictionary<ProviderDictionaryCollection>();
                            mSettings = result;
                        }
                    }
                }
                return result;
            }
            set
            {
                lock (settingsLock)
                {
                    mSettings = value;
                }
            }
        }


        /// <summary>
        /// Returns true if the data is available for this provider
        /// </summary>
        internal bool IsDataAvailable => (DataSource != null) || DatabaseHelper.IsDatabaseAvailable;

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        public SettingsKeyInfoProvider()
            : base(SettingsKeyInfo.TYPEINFO)
        {
        }

        #endregion


        #region "SettingsKeyInfo management methods"

        /// <summary>
        /// Sets the specified settings key data.
        /// </summary>
        /// <param name="infoObj">Settings key info object</param>
        public static void SetSettingsKeyInfo(SettingsKeyInfo infoObj)
        {
            ProviderObject.SetSettingsKeyInfoInternal(infoObj);
        }


        /// <summary>
        /// Returns the settings key with specified ID.
        /// </summary>
        /// <param name="keyId">Key ID</param>
        public static SettingsKeyInfo GetSettingsKeyInfo(int keyId)
        {
            return ProviderObject.GetInfoById(keyId);
        }


        /// <summary>
        /// Returns the settings key info for key with the specified name. The name can be either KeyName (for global settings) or SiteName.KeyName (for local settings).
        /// </summary>
        /// <param name="keyName">Key name in format "[site name].[key name]" for site setting or "[key name]" for global setting, see <see cref="SettingsKeyName"/> for more information</param>
        public static SettingsKeyInfo GetSettingsKeyInfo(SettingsKeyName keyName)
        {
            if (keyName == null)
            {
                return null;
            }
            return ProviderObject.GetSettingsKeyInfoInternal(keyName.KeyName, keyName.SiteName);
        }


        /// <summary>
        /// Returns the settings key info for key with the specified name.
        /// </summary>
        /// <param name="keyName">Key name to retrieve</param>
        /// <param name="siteIdentifier">Site identifier (site ID or site name), see <see cref="SiteInfoIdentifier"/> and <see cref="InfoIdentifier"/> for more information</param>
        public static SettingsKeyInfo GetSettingsKeyInfo(string keyName, SiteInfoIdentifier siteIdentifier)
        {
            return ProviderObject.GetSettingsKeyInfoInternal(keyName, siteIdentifier);
        }


        /// <summary>
        /// Deletes the specified settings key.
        /// </summary>
        /// <param name="infoObj">Settings key</param>
        public static void DeleteSettingsKeyInfo(SettingsKeyInfo infoObj)
        {
            ProviderObject.DeleteSettingsKeyInfoInternal(infoObj);
        }


        /// <summary>
        /// Searches display name and description of the specified key for the specified text.
        /// The display name and description are localized and HTML encoded prior to searching.
        /// </summary>
        /// <param name="key">Key to search</param>
        /// <param name="searchText">Text to search for</param>
        /// <param name="searchInDescription">Indicates if the key description should be included in the search</param>
        public static bool SearchSettingsKey(SettingsKeyInfo key, string searchText, bool searchInDescription)
        {
            return ProviderObject.SearchSettingsKeyInternal(key, searchText, searchInDescription);
        }


        /// <summary>
        /// Moves specified key up.
        /// </summary>
        /// <param name="keyName">Key code name</param>
        public static void MoveSettingsKeyUp(string keyName)
        {
            if (!String.IsNullOrEmpty(keyName))
            {
                var infoObj = GetSettingsKeyInfo(keyName);
                infoObj?.Generalized.MoveObjectUp();
            }
        }


        /// <summary>
        /// Moves specified key down.
        /// </summary>
        /// <param name="keyName">Key code name</param>
        public static void MoveSettingsKeyDown(string keyName)
        {
            if (!String.IsNullOrEmpty(keyName))
            {
                var infoObj = GetSettingsKeyInfo(keyName);
                infoObj?.Generalized.MoveObjectDown();
            }
        }


        /// <summary>
        /// Returns all settings keys.
        /// </summary>
        public static ObjectQuery<SettingsKeyInfo> GetSettingsKeys()
        {
            return ProviderObject.GetSettingsKeysInternal();
        }


        /// <summary>
        /// Returns a DataSet with all the keys for given site and category sorted by KeyDisplayName.
        /// If site is not specified, only the global settings are loaded.
        /// </summary>
        /// <param name="categoryId">Settings category ID</param>
        /// <param name="siteIdentifier">Site identifier (site ID or site name), see <see cref="SiteInfoIdentifier"/> and <see cref="InfoIdentifier"/> for more information</param>
        public static ObjectQuery<SettingsKeyInfo> GetSettingsKeys(int categoryId, SiteInfoIdentifier siteIdentifier = null)
        {
            return ProviderObject.GetObjectQuery()
                .OnSite(siteIdentifier)
                .WhereEquals("KeyCategoryID", categoryId);
        }

        #endregion


        #region "Helper methods"

        /// <summary>
        /// Returns URL extension used for files friendly URLs for specified site.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static string GetFilesUrlExtension(string siteName)
        {
            return GetValue(siteName + ".CMSFilesFriendlyURLExtension");
        }

        #endregion


        #region "Load methods"

        /// <summary>
        /// Gets the settings
        /// </summary>
        /// <param name="siteName">Site name</param>
        private ObjectQuery<SettingsKeyInfo> GetSettings(string siteName)
        {
            // Get settings
            return GetObjectQueryInternal()
                .Columns("KeyName", "KeyValue")
                .OnSite(siteName);
        }


        /// <summary>
        /// Loads the settings from the given DataSet
        /// </summary>
        /// <param name="sender">Instance of <see cref="ProviderDictionaryCollection"/>.</param>
        /// <param name="ds">DataSet with the settings</param>
        private void LoadSettings(ProviderDictionaryCollection sender, DataSet ds)
        {
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                if (sender != null)
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        // Prepare the key
                        string keyName = dr["KeyName"].ToString();

                        // Load the value
                        sender.StringValues[keyName] = DataHelper.GetStringValue(dr, "KeyValue", null);
                    }

                }
            }
        }


        /// <summary>
        /// Loads the objects.
        /// </summary>
        /// <param name="sender">Instance of <see cref="ProviderDictionaryCollection"/>.</param>
        /// <param name="parameter">Site name as a parameter</param>
        private void LoadSettingsWithReaderCheck(ProviderDictionaryCollection sender, object parameter)
        {
            string siteName = parameter.ToString();

            // Get settings
            var settings = GetSettings(siteName);

            if (settings.SupportsReader)
            {
                // Load from reader
                var reader = settings.ExecuteReader();
                LoadSettings(sender, reader);
            }
            else
            {
                // Load from data set
                LoadSettings(sender, settings);
            }
        }


        /// <summary>
        /// Loads the settings from the given data reader
        /// </summary>
        /// <param name="sender">Instance of <see cref="ProviderDictionaryCollection"/>.</param>
        /// <param name="reader">Data reader</param>
        private void LoadSettings(ProviderDictionaryCollection sender, DbDataReader reader)
        {
            // Load with the reader
            using (reader)
            {
                if (sender != null)
                {
                    while (reader.Read())
                    {
                        // Prepare the key
                        string keyName = reader.GetString(0);
                        string value = ValidationHelper.GetString(reader.GetValue(1), null);

                        // Load the value
                        sender.StringValues[keyName] = value;
                    }
                }
            }
        }


        /// <summary>
        /// Loads all keys for specific site if not loaded yet.
        /// </summary>
        /// <param name="siteName">Site name to load</param>
        /// <returns>Currently loaded or already existing collection of keys for specific site.</returns>
        private ProviderDictionaryCollection LoadSettings(string siteName)
        {
            // Ensure the site name value
            if (siteName == null)
            {
                siteName = String.Empty;
            }

            var keys = Settings[siteName];
            if (ProviderHelper.LoadTables(keys))
            {
                lock (hashtableLock)
                {
                    keys = Settings[siteName];
                    if (ProviderHelper.LoadTables(keys))
                    {
                        // Create hashtables if not present
                        LoadHashtableEnum loadingType = ProviderHelper.LoadHashTables(SettingsKeyInfo.OBJECT_TYPE, LoadHashtableEnum.All);

                        var keyCollection = new ProviderDictionaryCollection(SettingsKeyInfo.OBJECT_TYPE, loadingType, LoadSettingsWithReaderCheck, siteName);
                        keyCollection.StringValues = new ProviderDictionary<string, string>(SettingsKeyInfo.OBJECT_TYPE, "KeyName", StringComparer.InvariantCultureIgnoreCase, siteName, true);

                        // Ensures that LoadData callback is executed
                        keyCollection.LoadDefaultItems();

                        Settings[siteName] = keyCollection;
                        keys = keyCollection;
                    }
                }
            }

            return keys;
        }


        /// <summary>
        /// Clears the cached value of the specified settings key.
        /// </summary>
        /// <param name="info">Object info for the settings key which should be deleted</param>
        protected override void DeleteObjectFromHashtables(SettingsKeyInfo info)
        {
            base.DeleteObjectFromHashtables(info);

            string keyName = info.KeyName;

            SiteInfoIdentifier siteIdentifier = info.SiteID;

            var site = (siteIdentifier == null) ? GLOBAL_KEYS : siteIdentifier.ObjectCodeName;

            var keys = Settings[site];
            keys?.StringValues.Remove(keyName, false);

            CreateWebFarmTask(WEBFARM_TASK_CLEAR_SITE_HASHTABLES, site);
        }


        /// <summary>
        /// Updates the cached value of the specified settings key.
        /// </summary>
        /// <param name="info">Object info for the settings key which should be deleted</param>
        protected override void UpdateObjectInHashtables(SettingsKeyInfo info)
        {
            base.UpdateObjectInHashtables(info);

            SiteInfoIdentifier siteIdentifier = info.SiteID;

            string keyName = info.KeyName;
            string keyValue = info.KeyValue;

            var site = (siteIdentifier == null) ? GLOBAL_KEYS : siteIdentifier.ObjectCodeName;

            var keys = Settings[site];
            keys?.StringValues.Add(keyName, keyValue, false);

            CreateWebFarmTask(WEBFARM_TASK_CLEAR_SITE_HASHTABLES, site);
        }

        #endregion


        #region "Settings value management methods"

        /// <summary>
        /// Sets the value of the global key if the key exists.
        /// </summary>
        /// <param name="keyName">Key name for global setting</param>
        /// <param name="value">Key value</param>
        /// <param name="logSynchronization">If true, synchronization task is logged</param>
        public static void SetGlobalValue(string keyName, object value, bool logSynchronization = true)
        {
            ProviderObject.SetValueInternal(keyName, 0, value, logSynchronization);
        }


        /// <summary>
        /// Sets the value of the given key if the key exists.
        /// </summary>
        /// <param name="keyName">Key name</param>
        /// <param name="siteIdentifier">Site identifier (site ID or site name), see <see cref="SiteInfoIdentifier"/> and <see cref="InfoIdentifier"/> for more information</param>
        /// <param name="value">Key value</param>
        /// <param name="logSynchronization">If true, synchronization task is logged</param>
        public static void SetValue(string keyName, SiteInfoIdentifier siteIdentifier, object value, bool logSynchronization = true)
        {
            ProviderObject.SetValueInternal(keyName, siteIdentifier, value, logSynchronization);
        }


        /// <summary>
        /// Returns value of the specified key from the database or "" if not found.
        /// </summary>
        /// <param name="keyName">Key name in format "[site name].[key name]" for site setting or "[key name]" for global setting, see <see cref="SettingsKeyName"/> for more information</param>
        /// <returns>Returns "" if the key does not exist</returns>
        /// <remarks>If the <see cref="SystemContext.DevelopmentMode"/> is <c>true</c> and if <paramref name="keyName"/> is not found <see cref="InvalidOperationException"/> is thrown instead.</remarks>
        public static string GetValue(SettingsKeyName keyName)
        {
            if (keyName == null)
            {
                return "";
            }
            return ProviderObject.GetValueInternal(keyName.KeyName, keyName.SiteName);
        }


        /// <summary>
        /// Returns value of the specified key from the database or "" if not found. Falls back to the <see cref="GetValue(CMS.DataEngine.SettingsKeyName)"/> if site-specific key is not found.
        /// </summary>
        /// <param name="keyName">Key name</param>
        /// <param name="siteIdentifier">Site identifier (site ID or site name), see <see cref="SiteInfoIdentifier"/> and <see cref="InfoIdentifier"/> for more information</param>
        /// <returns>Returns "" if the key does not exist</returns>
        /// <remarks>If the <see cref="SystemContext.DevelopmentMode"/> is <c>true</c> and if <paramref name="keyName"/> is not found <see cref="InvalidOperationException"/> is thrown instead.</remarks>
        public static string GetValue(string keyName, SiteInfoIdentifier siteIdentifier)
        {
            return ProviderObject.GetValueInternal(keyName, siteIdentifier);
        }


        /// <summary>
        /// Returns value of the specified key from the database or "" if not found. Falls back to the <see cref="GetValue(CMS.DataEngine.SettingsKeyName)"/> if site-specific key is not found.
        /// </summary>
        /// <param name="keyName">Key name</param>
        /// <param name="siteIdentifier">Site identifier (site ID or site name), see <see cref="SiteInfoIdentifier"/> and <see cref="InfoIdentifier"/> for more information</param>
        /// <param name="nullIfNotFound">Return null if the key does not exist</param>
        /// <returns>Returns "" if the key does not exist</returns>
        /// <remarks>If the <see cref="SystemContext.DevelopmentMode"/> is <c>true</c> and if <paramref name="keyName"/> is not found <see cref="InvalidOperationException"/> is thrown instead.</remarks>
        internal static string GetValue(string keyName, SiteInfoIdentifier siteIdentifier, bool nullIfNotFound)
        {
            return ProviderObject.GetValueInternal(keyName, siteIdentifier, nullIfNotFound);
        }


        /// <summary>
        /// Gets the settings key value from DB.
        /// </summary>
        /// <param name="keyName">Key name in format "[site name].[key name]" for site setting or "[key name]" for global setting, see <see cref="SettingsKeyName"/> for more information</param>
        /// <remarks>If the <see cref="SystemContext.DevelopmentMode"/> is <c>true</c> and if <paramref name="keyName"/> is not found <see cref="InvalidOperationException"/> is thrown instead.</remarks>
        public static string GetValueFromDB(SettingsKeyName keyName)
        {
            return GetValueFromDB(keyName.KeyName, keyName.SiteName);
        }


        /// <summary>
        /// Gets the settings key value from DB.
        /// </summary>
        /// <param name="keyName">Key name</param>
        /// <param name="siteIdentifier">Site identifier (site ID or site name), see <see cref="SiteInfoIdentifier"/> and <see cref="InfoIdentifier"/> for more information</param>
        /// <remarks>If the <see cref="SystemContext.DevelopmentMode"/> is <c>true</c> and if <paramref name="keyName"/> is not found <see cref="InvalidOperationException"/> is thrown instead.</remarks>
        public static string GetValueFromDB(string keyName, SiteInfoIdentifier siteIdentifier)
        {
            return ProviderObject.GetValueFromDBInternal(keyName, siteIdentifier);
        }


        /// <summary>
        /// Gets the URL value from settings.
        /// </summary>
        /// <param name="keyName">Key name in format "[site name].[key name]" for site setting or "[key name]" for global setting, see <see cref="SettingsKeyName"/> for more information</param>
        /// <param name="defaultUrl">Default URL</param>
        /// <remarks>If the <see cref="SystemContext.DevelopmentMode"/> is <c>true</c> and if <paramref name="keyName"/> is not found <see cref="InvalidOperationException"/> is thrown instead.</remarks>
        public static string GetURLValue(SettingsKeyName keyName, string defaultUrl)
        {
            return GetURLValue(keyName.KeyName, keyName.SiteName, defaultUrl);
        }


        /// <summary>
        /// Gets the URL value from settings.
        /// </summary>
        /// <param name="keyName">Key name</param>
        /// <param name="siteIdentifier">Site identifier (site ID or site name), see <see cref="SiteInfoIdentifier"/> and <see cref="InfoIdentifier"/> for more information</param>
        /// <param name="defaultUrl">Default URL</param>
        /// <remarks>If the <see cref="SystemContext.DevelopmentMode"/> is <c>true</c> and if <paramref name="keyName"/> is not found <see cref="InvalidOperationException"/> is thrown instead.</remarks>
        public static string GetURLValue(string keyName, SiteInfoIdentifier siteIdentifier, string defaultUrl)
        {
            return ProviderObject.GetURLValueInternal(keyName, siteIdentifier, defaultUrl);
        }


        /// <summary>
        /// Returns the integer value of the specified key from the database or 0 if not found.
        /// </summary>
        /// <param name="keyName">Key name in format "[site name].[key name]" for site setting or "[key name]" for global setting, see <see cref="SettingsKeyName"/> for more information</param>
        /// <remarks>If the <see cref="SystemContext.DevelopmentMode"/> is <c>true</c> and if <paramref name="keyName"/> is not found <see cref="InvalidOperationException"/> is thrown instead.</remarks>
        public static int GetIntValue(SettingsKeyName keyName)
        {
            return GetIntValue(keyName.KeyName, keyName.SiteName);
        }


        /// <summary>
        /// Returns the integer value of the specified key from the database or 0 if not found.
        /// </summary>
        /// <param name="keyName">Key name</param>
        /// <param name="siteIdentifier">Site identifier (site ID or site name), see <see cref="SiteInfoIdentifier"/> and <see cref="InfoIdentifier"/> for more information</param>
        /// <remarks>If the <see cref="SystemContext.DevelopmentMode"/> is <c>true</c> and if <paramref name="keyName"/> is not found <see cref="InvalidOperationException"/> is thrown instead.</remarks>
        public static int GetIntValue(string keyName, SiteInfoIdentifier siteIdentifier)
        {
            return ProviderObject.GetIntValueInternal(keyName, siteIdentifier);
        }


        /// <summary>
        /// Returns the integer value of setting in web.config or value of specified key if setting is not present in web.config file.
        /// </summary>
        /// <param name="configKey">Web.config key</param>
        /// <param name="keyName">Key name</param>
        /// <param name="settingDefaultValue">Default value of the setting when web.config key is missing and database is not available</param>
        /// <param name="siteIdentifier">Site identifier (site ID or site name), see <see cref="SiteInfoIdentifier"/> and <see cref="InfoIdentifier"/> for more information</param>
        /// <remarks>If the <see cref="SystemContext.DevelopmentMode"/> is <c>true</c> and if <paramref name="keyName"/> is not found <see cref="InvalidOperationException"/> is thrown instead.</remarks>
        public static int GetIntValue(string configKey, string keyName, int settingDefaultValue, SiteInfoIdentifier siteIdentifier = null)
        {
            return ProviderObject.GetIntValueInternal(configKey, keyName, settingDefaultValue, siteIdentifier);
        }


        /// <summary>
        /// Returns the double value of the specified key from the database or 0 if not found.
        /// </summary>
        /// <param name="keyName">Key name in format "[site name].[key name]" for site setting or "[key name]" for global setting, see <see cref="SettingsKeyName"/> for more information</param>
        /// <remarks>If the <see cref="SystemContext.DevelopmentMode"/> is <c>true</c> and if <paramref name="keyName"/> is not found <see cref="InvalidOperationException"/> is thrown instead.</remarks>
        public static double GetDoubleValue(SettingsKeyName keyName)
        {
            return GetDoubleValue(keyName.KeyName, keyName.SiteName);
        }


        /// <summary>
        /// Returns the double value of the specified key from the database or 0 if not found.
        /// </summary>
        /// <param name="keyName">Key name</param>
        /// <param name="siteIdentifier">Site identifier (site ID or site name), see <see cref="SiteInfoIdentifier"/> and <see cref="InfoIdentifier"/> for more information</param>
        /// <remarks>If the <see cref="SystemContext.DevelopmentMode"/> is <c>true</c> and if <paramref name="keyName"/> is not found <see cref="InvalidOperationException"/> is thrown instead.</remarks>
        public static double GetDoubleValue(string keyName, SiteInfoIdentifier siteIdentifier)
        {
            return ProviderObject.GetDoubleValueInternal(keyName, siteIdentifier);
        }


        /// <summary>
        /// Returns the decimal value of the specified key from the database or 0 if not found.
        /// </summary>
        /// <param name="keyName">Key name in format "[site name].[key name]" for site setting or "[key name]" for global setting, see <see cref="SettingsKeyName"/> for more information</param>
        /// <remarks>If the <see cref="SystemContext.DevelopmentMode"/> is <c>true</c> and if <paramref name="keyName"/> is not found <see cref="InvalidOperationException"/> is thrown instead.</remarks>
        public static decimal GetDecimalValue(SettingsKeyName keyName)
        {
            return GetDecimalValue(keyName.KeyName, keyName.SiteName);
        }


        /// <summary>
        /// Returns the decimal value of the specified key from the database or 0 if not found.
        /// </summary>
        /// <param name="keyName">Key name</param>
        /// <param name="siteIdentifier">Site identifier (site ID or site name), see <see cref="SiteInfoIdentifier"/> and <see cref="InfoIdentifier"/> for more information</param>
        /// <remarks>If the <see cref="SystemContext.DevelopmentMode"/> is <c>true</c> and if <paramref name="keyName"/> is not found <see cref="InvalidOperationException"/> is thrown instead.</remarks>
        public static decimal GetDecimalValue(string keyName, SiteInfoIdentifier siteIdentifier)
        {
            return ProviderObject.GetDecimalValueInternal(keyName, siteIdentifier);
        }


        /// <summary>
        /// Returns the boolean value of the specified key from the database or false if not found.
        /// </summary>
        /// <param name="keyName">Key name in format "[site name].[key name]" for site setting or "[key name]" for global setting, see <see cref="SettingsKeyName"/> for more information</param>
        /// <remarks>If the <see cref="SystemContext.DevelopmentMode"/> is <c>true</c> and if <paramref name="keyName"/> is not found <see cref="InvalidOperationException"/> is thrown instead.</remarks>
        public static bool GetBoolValue(SettingsKeyName keyName)
        {
            return GetBoolValue(keyName.KeyName, keyName.SiteName);
        }


        /// <summary>
        /// Returns the boolean value of the specified key from the database or false if not found.
        /// </summary>
        /// <param name="keyName">Key name</param>
        /// <param name="siteIdentifier">Site identifier (site ID or site name), see <see cref="SiteInfoIdentifier"/> and <see cref="InfoIdentifier"/> for more information</param>
        /// <remarks>If the <see cref="SystemContext.DevelopmentMode"/> is <c>true</c> and if <paramref name="keyName"/> is not found <see cref="InvalidOperationException"/> is thrown instead.</remarks>
        public static bool GetBoolValue(string keyName, SiteInfoIdentifier siteIdentifier)
        {
            return ProviderObject.GetBoolValueInternal(keyName, siteIdentifier);
        }


        /// <summary>
        /// Returns the boolean value of setting in web.config or value of specified key if setting is not present in web.config file.
        /// </summary>
        /// <param name="configKey">Web.config key</param>
        /// <param name="keyName">Name of the setting</param>
        /// <param name="settingDefaultValue">Default value of the setting when web.config key is missing and database is not available</param>
        /// <remarks>If the <see cref="SystemContext.DevelopmentMode"/> is <c>true</c> and if <paramref name="keyName"/> is not found <see cref="InvalidOperationException"/> is thrown instead.</remarks>
        public static bool GetBoolValue(string configKey, string keyName, bool settingDefaultValue)
        {
            return ProviderObject.GetBoolValueInternal(configKey, keyName, settingDefaultValue);
        }


        /// <summary>
        /// Checks if the value of the specified settings value is inherited.
        /// </summary>
        /// <param name="keyName">Key name in format "[site name].[key name]" for site setting or "[key name]" for global setting, see <see cref="SettingsKeyName"/> for more information</param>
        public static bool IsValueInherited(SettingsKeyName keyName)
        {
            return IsValueInherited(keyName.KeyName, keyName.SiteName);
        }


        /// <summary>
        /// Checks if the value of the specified settings value is inherited.
        /// </summary>
        /// <param name="keyName">Key name</param>
        /// <param name="siteIdentifier">Site identifier (site ID or site name), see <see cref="SiteInfoIdentifier"/> and <see cref="InfoIdentifier"/> for more information</param>
        public static bool IsValueInherited(string keyName, SiteInfoIdentifier siteIdentifier)
        {
            return ProviderObject.IsValueInheritedInternal(keyName, siteIdentifier);
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Returns all settings keys.
        /// </summary>
        protected virtual ObjectQuery<SettingsKeyInfo> GetSettingsKeysInternal()
        {
            return GetObjectQuery();
        }


        /// <summary>
        /// Returns the boolean value of the specified key from the database or false if not found.
        /// </summary>
        /// <param name="keyName">Key name</param>
        /// <param name="siteIdentifier">Site identifier (site ID or site name), see <see cref="SiteInfoIdentifier"/> and <see cref="InfoIdentifier"/> for more information</param>
        /// <remarks>If the <see cref="SystemContext.DevelopmentMode"/> is <c>true</c> and if <paramref name="keyName"/> is not found <see cref="InvalidOperationException"/> is thrown instead.</remarks>
        protected virtual bool GetBoolValueInternal(string keyName, SiteInfoIdentifier siteIdentifier)
        {
            var value = GetValue(keyName, siteIdentifier);
            return ValidationHelper.GetBoolean(value, false);
        }


        /// <summary>
        /// Returns the boolean value of setting in web.config or value of specified key if setting is not present in web.config file.
        /// </summary>
        /// <param name="configKey">Web.config key</param>
        /// <param name="keyName">Name of the setting</param>
        /// <param name="settingDefaultValue">Default value of the setting when web.config key is missing and database is not available</param>
        /// <remarks>If the <see cref="SystemContext.DevelopmentMode"/> is <c>true</c> and if <paramref name="keyName"/> is not found <see cref="InvalidOperationException"/> is thrown instead.</remarks>
        protected virtual bool GetBoolValueInternal(string configKey, string keyName, bool settingDefaultValue)
        {
            var defaultValue = (IsDataAvailable ? GetBoolValue(keyName) : settingDefaultValue);

            // If config key not defined, return default value
            if (String.IsNullOrEmpty(configKey))
            {
                return defaultValue;
            }

            return ValidationHelper.GetBoolean(SettingsHelper.AppSettings[configKey], defaultValue);
        }


        /// <summary>
        /// Returns the integer value of setting in web.config or value of specified key if setting is not present in web.config file.
        /// </summary>
        /// <param name="configKey">Web.config key</param>
        /// <param name="keyName">Key name</param>
        /// <param name="settingDefaultValue">Default value of the setting when web.config key is missing and database is not available</param>
        /// <param name="siteIdentifier">Site identifier (site ID or site name), see <see cref="SiteInfoIdentifier"/> and <see cref="InfoIdentifier"/> for more information</param>
        /// <remarks>If the <see cref="SystemContext.DevelopmentMode"/> is <c>true</c> and if <paramref name="keyName"/> is not found <see cref="InvalidOperationException"/> is thrown instead.</remarks>
        protected virtual int GetIntValueInternal(string configKey, string keyName, int settingDefaultValue, SiteInfoIdentifier siteIdentifier)
        {
            var defaultValue = (IsDataAvailable ? GetIntValueInternal(keyName, siteIdentifier, settingDefaultValue) : settingDefaultValue);

            // If config key not defined, return default value
            if (String.IsNullOrEmpty(configKey))
            {
                return defaultValue;
            }

            return ValidationHelper.GetInteger(SettingsHelper.AppSettings[configKey], defaultValue);
        }


        /// <summary>
        /// Returns the integer value of the specified key from the database or 0 if not found.
        /// </summary>
        /// <param name="keyName">Key name</param>
        /// <param name="siteIdentifier">Site identifier (site ID or site name), see <see cref="SiteInfoIdentifier"/> and <see cref="InfoIdentifier"/> for more information</param>
        /// <param name="defaultValue">Default value of the setting when web.config key is missing and database is not available</param>
        /// <remarks>If the <see cref="SystemContext.DevelopmentMode"/> is <c>true</c> and if <paramref name="keyName"/> is not found <see cref="InvalidOperationException"/> is thrown instead.</remarks>
        protected virtual int GetIntValueInternal(string keyName, SiteInfoIdentifier siteIdentifier, int defaultValue = 0)
        {
            var value = GetValue(keyName, siteIdentifier);
            return ValidationHelper.GetInteger(value, defaultValue);
        }


        /// <summary>
        /// Returns the double value of the specified key from the database or 0 if not found.
        /// </summary>
        /// <param name="keyName">Key name</param>
        /// <param name="siteIdentifier">Site identifier (site ID or site name), see <see cref="SiteInfoIdentifier"/> and <see cref="InfoIdentifier"/> for more information</param>
        /// <remarks>If the <see cref="SystemContext.DevelopmentMode"/> is <c>true</c> and if <paramref name="keyName"/> is not found <see cref="InvalidOperationException"/> is thrown instead.</remarks>
        protected virtual double GetDoubleValueInternal(string keyName, SiteInfoIdentifier siteIdentifier)
        {
            string value = GetValue(keyName, siteIdentifier);
            if (String.IsNullOrEmpty(value))
            {
                // Value not found
                return 0;
            }

            return Convert.ToDouble(value);
        }


        /// <summary>
        /// Returns the decimal value of the specified key from the database or 0 if not found.
        /// </summary>
        /// <param name="keyName">Key name</param>
        /// <param name="siteIdentifier">Site identifier (site ID or site name), see <see cref="SiteInfoIdentifier"/> and <see cref="InfoIdentifier"/> for more information</param>
        /// <remarks>If the <see cref="SystemContext.DevelopmentMode"/> is <c>true</c> and if <paramref name="keyName"/> is not found <see cref="InvalidOperationException"/> is thrown instead.</remarks>
        protected virtual decimal GetDecimalValueInternal(string keyName, SiteInfoIdentifier siteIdentifier)
        {
            string value = GetValue(keyName, siteIdentifier);
            if (String.IsNullOrEmpty(value))
            {
                // Value not found
                return 0;
            }

            return Convert.ToDecimal(value);
        }


        /// <summary>
        /// Gets the URL value from settings.
        /// </summary>
        /// <param name="keyName">Key name</param>
        /// <param name="siteIdentifier">Site identifier (site ID or site name), see <see cref="SiteInfoIdentifier"/> and <see cref="InfoIdentifier"/> for more information</param>
        /// <param name="defaultUrl">Default URL</param>
        protected virtual string GetURLValueInternal(string keyName, SiteInfoIdentifier siteIdentifier, string defaultUrl)
        {
            string value = GetValue(keyName, siteIdentifier);
            string url = DataHelper.GetNotEmpty(value, defaultUrl);

            // Execute the GetURL events to run transformations
            var e = GetURL.StartEvent(url);

            url = e.URL;

            return url;
        }


        /// <summary>
        /// Returns value of the specified key from the database.
        /// </summary>
        /// <param name="keyName">Key name</param>
        /// <param name="siteIdentifier">Site identifier (site ID or site name), see <see cref="SiteInfoIdentifier"/> and <see cref="InfoIdentifier"/> for more information</param>
        /// <param name="nullIfNotFound">Return null if the key does not exist</param>
        /// <remarks>If the <see cref="SystemContext.DevelopmentMode"/> is <c>true</c> and if <paramref name="keyName"/> is not found <see cref="InvalidOperationException"/> is thrown instead.</remarks>
        protected virtual string GetValueInternal(string keyName, SiteInfoIdentifier siteIdentifier, bool nullIfNotFound = false)
        {
            if (!IsDataAvailable)
            {
                return nullIfNotFound ? null : String.Empty;
            }

            string value = null;

            // If the site is present, get local value
            if (siteIdentifier != null)
            {
                // Get the local settings
                var keys = LoadSettings(siteIdentifier);
                if ((keys != null) && !keys.StringValues.TryGetValue(keyName, out value))
                {
                    if (keys.LoadingType != LoadHashtableEnum.All)
                    {
                        // Get local value from the database
                        value = GetValueFromDB(keyName, siteIdentifier);
                        keys.StringValues[keyName] = value;
                    }
                }

                // Return local value if not inherited
                if (value != null)
                {
                    return value;
                }
            }

            // Try to find the global value
            var globalKeys = LoadSettings(GLOBAL_KEYS);
            if ((globalKeys != null) && (keyName != null) && !globalKeys.StringValues.TryGetValue(keyName, out value))
            {
                if (globalKeys.LoadingType != LoadHashtableEnum.All)
                {
                    value = GetValueFromDB(keyName);
                    globalKeys.StringValues[keyName] = value;
                }
                else
                {
                    ReportNotExistingSettingsKey(keyName);
                }
            }

            // Ensure non-null value if required
            if (!nullIfNotFound && value == null)
            {
                value = "";
            }

            return value;
        }


        /// <summary>
        /// Gets the settings key value from DB.
        /// </summary>
        /// <param name="keyName">Key name</param>
        /// <param name="siteIdentifier">Site identifier (site ID or site name), see <see cref="SiteInfoIdentifier"/> and <see cref="InfoIdentifier"/> for more information</param>
        /// <remarks>If the <see cref="SystemContext.DevelopmentMode"/> is <c>true</c> and if <paramref name="keyName"/> is not found <see cref="InvalidOperationException"/> is thrown instead.</remarks>
        protected virtual string GetValueFromDBInternal(string keyName, SiteInfoIdentifier siteIdentifier)
        {
            if (!IsDataAvailable)
            {
                return null;
            }

            var q = GetObjectQuery()
                .OnSite(siteIdentifier)
                .WhereEquals("KeyName", keyName)
                .Column("KeyValue");

            var result = q.GetScalarResult<string>();

            if ((siteIdentifier == null) && !q.HasResults())
            {
                ReportNotExistingSettingsKey(keyName);
            }

            return result;
        }


        /// <summary>
        /// Throws <see cref="InvalidOperationException"/> for not existing key under <see cref="SystemContext.DevelopmentMode"/>.
        /// </summary>
        private void ReportNotExistingSettingsKey(string keyName)
        {
            if (SystemContext.DevelopmentMode)
            {
                throw new InvalidOperationException($"Settings key with name '{keyName}' doesn't exist.");
            }
        }


        /// <summary>
        /// Sets the value of the given key if the key exists.
        /// </summary>
        /// <param name="keyName">Key name</param>
        /// <param name="siteIdentifier">Site identifier (site ID or site name), see <see cref="SiteInfoIdentifier"/> and <see cref="InfoIdentifier"/> for more information</param>
        /// <param name="value">Key value</param>
        /// <param name="logSynchronization">If true, synchronization task is logged</param>
        protected virtual void SetValueInternal(string keyName, SiteInfoIdentifier siteIdentifier, object value, bool logSynchronization = true)
        {
            // Get the settings key object
            var key = GetSettingsKeyInfo(keyName, siteIdentifier);

            if (key == null)
            {
                // Key does not exist
                throw new InvalidOperationException($"Requested key name '{keyName}' doesn't exist.");
            }

            using (var actionContext = new CMSActionContext())
            {
                // Disable logging if required
                if (!logSynchronization)
                {
                    actionContext.DisableLogging();
                }

                // Convert value
                if ((value == null) || (value == DBNull.Value))
                {
                    key.KeyValue = null;
                }
                else if (value is bool)
                {
                    key.KeyValue = (bool)value ? "True" : "False";
                }
                else if (value is double)
                {
                    var culture = CultureHelper.GetCultureInfo("en-us");
                    key.KeyValue = Convert.ToString((double)value, culture.NumberFormat);
                }
                else if (value is decimal)
                {
                    var culture = CultureHelper.GetCultureInfo("en-us");
                    key.KeyValue = Convert.ToString((decimal)value, culture.NumberFormat);
                }
                else
                {
                    key.KeyValue = value.ToString();
                }

                // Set key
                SetSettingsKeyInfo(key);
            }
        }


        /// <summary>
        /// Sets the specified settings key data.
        /// </summary>
        /// <param name="infoObj">Settings key info object</param>
        protected virtual void SetSettingsKeyInfoInternal(SettingsKeyInfo infoObj)
        {
            if (infoObj == null)
            {
                return;
            }

            // Is the key being updated?
            bool isUpdate = infoObj.KeyID > 0;
            var genInfo = infoObj.Generalized;

            if (!infoObj.IsGlobal)
            {
                var globalSettingsKey = GetSettingsKeyInfo(infoObj.KeyName, null);
                if (globalSettingsKey == null)
                {
                    throw new InvalidOperationException("Global setting was not found for key name '" + infoObj.KeyName + "'");
                }
            }

            // Settings key is inherited if it is a site setting with null value
            var isInherited = !infoObj.IsGlobal && (infoObj.KeyValue == null);
            if (isInherited)
            {
                // Do not set the key and delete the existing record when the key is updated. When the key is inserted with null, do not insert it.
                if (isUpdate)
                {
                    DeleteSettingsKeyInfo(infoObj);

                    // Raise OnSettingsKeyChanged event
                    RaiseOnSettingsKeyChanged(infoObj, true, genInfo.ObjectSiteName);
                }

                return;
            }


            using (var tr = BeginTransaction())
            {
                // Site keys should be updated if relevant global key properties changed
                var updateSiteKeys = isUpdate && infoObj.IsGlobal && infoObj.AnyItemChanged("KeyName", "KeyDisplayName", "KeyType", "KeyCategoryID", "KeyIsGlobal");

                var originalKeyName = ValidationHelper.GetString(infoObj.Generalized.GetOriginalValue("KeyName"), null);

                // Update global setting
                SetInfo(infoObj);

                if (updateSiteKeys)
                {
                    // Update site keys
                    var siteKeys = GetSettingsKeys()
                                        .WhereNotNull("SiteID")
                                        .WhereEquals("KeyName", originalKeyName);

                    foreach (var siteKey in siteKeys)
                    {
                        if (infoObj.KeyIsGlobal)
                        {
                            // Remove site key
                            DeleteSettingsKeyInfo(siteKey);
                        }
                        else
                        {
                            // Update required site key properties
                            siteKey.KeyName = infoObj.KeyName;
                            siteKey.KeyDisplayName = infoObj.KeyDisplayName;
                            siteKey.KeyType = infoObj.KeyType;

                            // Update Category ID, it's required for export
                            siteKey.KeyCategoryID = infoObj.KeyCategoryID;

                            SetSettingsKeyInfo(siteKey);
                        }
                    }
                }

                tr.Commit();
            }

            // Raise OnSettingsKeyChanged event
            RaiseOnSettingsKeyChanged(infoObj, isUpdate, genInfo.ObjectSiteName);
        }


        /// <summary>
        /// Raises handler for settings key change
        /// </summary>
        /// <param name="settingsKey">Settings key</param>
        /// <param name="isUpdate">Indicates if setting is updated</param>
        /// <param name="siteName">Site name</param>
        private static void RaiseOnSettingsKeyChanged(SettingsKeyInfo settingsKey, bool isUpdate, string siteName)
        {
            if (OnSettingsKeyChanged == null)
            {
                return;
            }

            var action = isUpdate ? SettingsKeyActionEnum.Update : SettingsKeyActionEnum.Insert;
            OnSettingsKeyChanged(null, new SettingsKeyChangedEventArgs(settingsKey.KeyName, settingsKey.KeyValue, settingsKey.SiteID, siteName, action));
        }


        /// <summary>
        /// Deletes the specified settings key.
        /// </summary>
        /// <param name="infoObj">Settings key</param>
        protected virtual void DeleteSettingsKeyInfoInternal(SettingsKeyInfo infoObj)
        {
            if (infoObj == null)
            {
                return;
            }

            var keysToDelete = new List<SettingsKeyInfo>();

            if (infoObj.IsGlobal)
            {
                // Delete all keys with the same name, if deleting a global key
                var sameKeys = GetSettingsKeys().WhereEquals("KeyName", infoObj.KeyName);

                keysToDelete.AddRange(sameKeys);
            }
            else
            {
                // Delete only specific site key
                keysToDelete.Add(infoObj);
            }

            foreach (var key in keysToDelete)
            {
                ProviderObject.DeleteInfo(key);
            }
        }


        /// <summary>
        /// Searches display name and description of the specified key for the specified text.
        /// The display name and description are localized and HTML encoded prior to searching.
        /// </summary>
        /// <param name="key">Key to search</param>
        /// <param name="searchText">Text to search for</param>
        /// <param name="searchInDescription">Indicates if the key description should be included in the search</param>
        protected virtual bool SearchSettingsKeyInternal(SettingsKeyInfo key, string searchText, bool searchInDescription)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (String.IsNullOrEmpty(searchText))
            {
                throw new ArgumentNullException(nameof(searchInDescription));
            }

            Func<string, bool> isMatch = text =>
            {
                if (String.IsNullOrEmpty(text))
                {
                    return false;
                }
                
                text = CoreServices.Localization.LocalizeString(text);
                text = HTMLHelper.HTMLEncode(text);

                return text.IndexOf(searchText, StringComparison.InvariantCultureIgnoreCase) >= 0;
            };

            if (searchInDescription)
            {
                return isMatch(key.KeyDisplayName) || isMatch(key.KeyDescription);
            }

            return isMatch(key.KeyDisplayName);
        }


        /// <summary>
        /// Returns the settings key info for key with the specified name.
        /// </summary>
        /// <param name="keyName">Key name to retrieve</param>
        /// <param name="siteIdentifier">Site identifier (site ID or site name), see <see cref="SiteInfoIdentifier"/> and <see cref="InfoIdentifier"/> for more information</param>
        protected virtual SettingsKeyInfo GetSettingsKeyInfoInternal(string keyName, SiteInfoIdentifier siteIdentifier)
        {
            if (String.IsNullOrEmpty(keyName))
            {
                return null;
            }

            var query = GetObjectQuery()
                .OnSite(siteIdentifier)
                .WhereEquals("KeyName", keyName);

            var ds = query.Result;
            if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0].Rows.Count > 0))
            {
                return new SettingsKeyInfo(ds.Tables[0].Rows[0]);
            }

            // Try to create the site key, in case it is inherited
            if (siteIdentifier != null)
            {
                // Try to get global key
                var globalKey = GetSettingsKeyInfo(keyName);
                if (globalKey != null)
                {
                    // Create site key
                    var key = new SettingsKeyInfo
                    {
                        // Set required properties
                        KeyName = globalKey.KeyName,
                        KeyDisplayName = globalKey.KeyDisplayName,
                        KeyType = globalKey.KeyType,
                        KeyIsHidden = globalKey.KeyIsHidden,

                        // Set site ID
                        SiteID = siteIdentifier,

                        // Set Category ID, it's required for export
                        KeyCategoryID = globalKey.KeyCategoryID,
                    };
                    return key;
                }
            }

            return null;
        }


        /// <summary>
        /// Checks if the value of the specified settings value is inherited.
        /// </summary>
        /// <param name="keyName">Key name</param>
        /// <param name="siteIdentifier">Site identifier (site ID or site name), see <see cref="SiteInfoIdentifier"/> and <see cref="InfoIdentifier"/> for more information</param>
        protected virtual bool IsValueInheritedInternal(string keyName, SiteInfoIdentifier siteIdentifier)
        {
            if (siteIdentifier == null)
            {
                // Global key is never inherited
                return false;
            }
            else if (siteIdentifier.ObjectID == 0)
            {
                // Keys for non existing sites are always inherited
                return true;
            }

            // Try to get the site setting value
            string value = null;

            var keys = LoadSettings(siteIdentifier);
            if ((keys != null) && !keys.StringValues.TryGetValue(keyName, out value))
            {
                value = GetValueFromDB(keyName, siteIdentifier);
                keys.StringValues[keyName] = value;
            }

            // Site value is inherited if the value is null
            bool isInherited = value == null;
            return isInherited;
        }


        /// <summary>
        /// Clears the object's hashtables.
        /// </summary>
        /// <param name="logTasks">If true, web farm tasks are logged</param>
        protected override void ClearHashtables(bool logTasks)
        {
            base.ClearHashtables(logTasks);

            Settings = null;

            if (logTasks)
            {
                CreateWebFarmTask(WEBFARM_TASK_CLEAR_ALL_HASHTABLES, String.Empty);
            }
        }


        /// <summary>
        /// Runs the processing of specific web farm task for current provider
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="data">Custom task data</param>
        /// <param name="binary">Binary data</param>
        protected override void ProcessWebFarmTaskInternal(string actionName, string data, byte[] binary)
        {
            var normalizedActionName = actionName.ToLowerInvariant();

            switch (normalizedActionName)
            {
                case WEBFARM_TASK_CLEAR_ALL_HASHTABLES:
                    ClearHashtables(false);
                    break;

                case WEBFARM_TASK_CLEAR_SITE_HASHTABLES:
                    Settings[data] = null;
                    break;
            }
        }


        /// <summary>
        /// Gets the object query for the provider
        /// </summary>
        protected override ObjectQuery<SettingsKeyInfo> GetObjectQueryInternal()
        {
            var q = base.GetObjectQueryInternal();

            // Ensure query source
            q.TypeInfo = SettingsKeyInfo.TYPEINFO;
            q.CustomQueryText = SqlHelper.GENERAL_SELECT;
            q.ConnectionStringName = ConnectionHelper.DEFAULT_CONNECTIONSTRING_NAME;
            q.DefaultQuerySource = "CMS_SettingsKey";

            return q;
        }

        #endregion
    }
}