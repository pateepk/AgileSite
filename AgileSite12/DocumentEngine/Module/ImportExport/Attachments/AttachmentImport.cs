using System.Collections.Generic;

using CMS.Base;
using CMS.CMSImportExport;
using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Handles special actions during the Attachment import process.
    /// </summary>
    internal static class AttachmentImport
    {
        #region "Variables"

        private const string TEMP_FILES_LOCATION_TYPE_SETTING_NAME = "CMSTempFilesLocationType";
        private static readonly HashSet<string> keysToRemove = new HashSet<string>
        {
            "CMSStoreFilesInDatabase",
            "CMSStoreFilesInFileSystem",
        };

        #endregion


        #region "Methods"

        /// <summary>
        /// Initializes import handlers
        /// </summary>
        public static void Init()
        {
            SpecialActionsEvents.GetBinaryDataSourcePath.Execute += GetBinaryDataSourcePath_Execute;
        }



        /// <summary>
        /// Converts old keys CMSStoreFilesInDatabase and CMSStoreFilesInFileSystem into new key CMSFilesLocationType
        /// </summary>
        /// <param name="settingKey">Currently imported setting key</param>
        internal static void ConvertSettingsForFilesLocationType(SettingsKeyInfo settingKey)
        {
            // Get temporary setting
            var locationTypeSetting = SettingsKeyInfoProvider.GetSettingsKeyInfo(TEMP_FILES_LOCATION_TYPE_SETTING_NAME) ?? new SettingsKeyInfo
            {
                KeyName = TEMP_FILES_LOCATION_TYPE_SETTING_NAME,
                KeyDisplayName = TEMP_FILES_LOCATION_TYPE_SETTING_NAME,
                KeyType = "int",
                KeyIsHidden = true,
                KeyIsGlobal = true
            };

            int locationTypeValue = ValidationHelper.GetInteger(locationTypeSetting.KeyValue, -1);

            var keyValue = ValidationHelper.GetBoolean(settingKey.KeyValue, false);

            bool storeFilesInFileSystem = settingKey.KeyName.EqualsCSafe("CMSStoreFilesInFileSystem") && keyValue;
            bool storeFilesInDatabase = settingKey.KeyName.EqualsCSafe("CMSStoreFilesInDatabase") && keyValue;

            bool locationTypeIsDatabase = locationTypeValue == (int)FilesLocationTypeEnum.Database;
            bool locationTypeIsFileSystem = locationTypeValue == (int)FilesLocationTypeEnum.FileSystem;

            FilesLocationTypeEnum result;

            // Check if settings key was already set (second key is importing now)
            if (locationTypeSetting.KeyID > 0)
            {
                // Previous key was set
                if ((locationTypeIsDatabase && storeFilesInFileSystem) || (locationTypeIsFileSystem && storeFilesInDatabase))
                {
                    // If previous key was set to true and current is set to true, use both storages
                    result = FilesLocationTypeEnum.Both;
                }
                else
                {
                    // Don't change if second key is not true
                    return;
                }
            }
            else
            {
                if (storeFilesInFileSystem)
                {
                    result = FilesLocationTypeEnum.FileSystem;
                }
                else if (storeFilesInDatabase)
                {
                    result = FilesLocationTypeEnum.Database;
                }
                else
                {
                    // Do not create location type setting if key is false
                    return;
                }
            }

            using (var context = new CMSActionContext())
            {
                // Disable all possible logging
                context.DisableAll();

                // Save temporary setting used for converting value
                locationTypeSetting.KeyValue = ((int)result).ToString();
                SettingsKeyInfoProvider.SetSettingsKeyInfo(locationTypeSetting);
            }

            // Save original setting
            var locationType = SettingsKeyInfoProvider.GetSettingsKeyInfo("CMSFilesLocationType", settingKey.SiteID);
            locationType.KeyValue = locationTypeSetting.KeyValue;
            SettingsKeyInfoProvider.SetSettingsKeyInfo(locationType);
        }


        private static void GetBinaryDataSourcePath_Execute(object sender, GetBinaryDataSourcePathEventArgs e)
        {
            var settings = e.Settings;
            var infoObj = e.Object;

            var attachment = infoObj as AttachmentInfo;
            if (attachment != null)
            {
                // Get path
                e.Path = ImportProvider.GetBinaryDataSourcePath(settings, infoObj, "cms_attachment", attachment.AttachmentGUID.ToString(), attachment.AttachmentExtension);
            }
        }

        #endregion
    }
}