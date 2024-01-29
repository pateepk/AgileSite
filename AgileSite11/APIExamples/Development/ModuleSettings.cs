using System;

using CMS.DataEngine;
using CMS.Modules;
using CMS.Base;

namespace APIExamples
{
    /// <summary>
    /// Holds module setting API examples.
    /// </summary>
    /// <pageTitle>Module settings</pageTitle>
    internal class ModuleSettings
    {
        /// <summary>
        /// Holds setting category API examples.
        /// </summary>
        /// <groupHeading>Setting categories</groupHeading>
        private class SettingCategories
        {
            /// <heading>Creating a setting category</heading>
            private void CreateSettingsCategory()
            {
                // Gets a parent category for the new setting category (the root category in this case)                
                SettingsCategoryInfo parentCategory = SettingsCategoryInfoProvider.GetRootSettingsCategoryInfo();
                if (parentCategory != null)
                {
                    // Creates a new settings category object
                    SettingsCategoryInfo newCategory = new SettingsCategoryInfo();

                    // Sets the category properties
                    newCategory.CategoryDisplayName = "New Setting Category";
                    newCategory.CategoryName = "NewSettingCategory";
                    newCategory.CategoryOrder = 0;
                    newCategory.CategoryParentID = parentCategory.CategoryID;
                    newCategory.CategoryIsGroup = false;
                    newCategory.CategoryIsCustom = true;

                    // Assigns the category to a custom module (the default 'Custom' module in this case)
                    newCategory.CategoryResourceID = ResourceInfoProvider.GetResourceInfo("CMS.CustomSystemModule").ResourceID;

                    // Saves the new setting category to the database
                    SettingsCategoryInfoProvider.SetSettingsCategoryInfo(newCategory);
                }
            }


            /// <heading>Updating a setting category</heading>
            private void GetAndUpdateSettingsCategory()
            {
                // Gets the setting category
                SettingsCategoryInfo updateCategory = SettingsCategoryInfoProvider.GetSettingsCategoryInfoByName("NewSettingCategory");
                if (updateCategory != null)
                {
                    // Updates the category property
                    updateCategory.CategoryDisplayName = updateCategory.CategoryDisplayName.ToLowerCSafe();

                    // Saves the updated setting category to the database
                    SettingsCategoryInfoProvider.SetSettingsCategoryInfo(updateCategory);
                }
            }


            /// <heading>Updating multiple setting categories</heading>
            private void GetAndBulkUpdateSettingsCategories()
            {
                // Gets all setting categories whose code name starts with 'New'
                var categories = SettingsCategoryInfoProvider.GetSettingsCategories()
                                                                    .WhereStartsWith("CategoryName", "New")
                                                                    .WhereEquals("CategoryIsGroup", 0);
                
                // Loops through individual categories
                foreach (SettingsCategoryInfo category in categories)
                {
                    // Updates the category properties
                    category.CategoryDisplayName = category.CategoryDisplayName.ToUpper();

                    // Saves the updated setting category to the database
                    SettingsCategoryInfoProvider.SetSettingsCategoryInfo(category);
                }
            }


            /// <heading>Deleting a setting category</heading>
            private void DeleteSettingsCategory()
            {
                // Gets the setting category
                SettingsCategoryInfo deleteCategory = SettingsCategoryInfoProvider.GetSettingsCategoryInfoByName("NewSettingCategory");

                if (deleteCategory != null)
                {
                    // Deletes the setting category
                    SettingsCategoryInfoProvider.DeleteSettingsCategoryInfo(deleteCategory);
                }
            }
        }


        /// <summary>
        /// Holds setting group API examples.
        /// </summary>
        /// <groupHeading>Setting groups</groupHeading>
        private class SettingGroups
        {
            /// <heading>Creating a setting group</heading>
            private void CreateSettingsGroup()
            {
                // Gets a parent setting category
                SettingsCategoryInfo settingCategory = SettingsCategoryInfoProvider.GetSettingsCategoryInfoByName("NewSettingCategory");
                if (settingCategory != null)
                {
                    // Creates a new setting group object
                    SettingsCategoryInfo newGroup = new SettingsCategoryInfo();

                    // Sets the setting group properties
                    newGroup.CategoryDisplayName = "New Setting Group";
                    newGroup.CategoryName = "NewSettingsGroup";
                    newGroup.CategoryOrder = 0;
                    newGroup.CategoryParentID = settingCategory.CategoryID;
                    newGroup.CategoryIsGroup = true;
                    newGroup.CategoryIsCustom = true;

                    // Assigns the group to a custom module (the default 'Custom' module in this case)
                    newGroup.CategoryResourceID = ResourceInfoProvider.GetResourceInfo("CMS.CustomSystemModule").ResourceID;

                    // Saves the setting group to the database
                    SettingsCategoryInfoProvider.SetSettingsCategoryInfo(newGroup);
                }
            }


            /// <heading>Updating a setting group</heading>
            private void GetAndUpdateSettingsGroup()
            {
                // Gets the setting group
                SettingsCategoryInfo updateGroup = SettingsCategoryInfoProvider.GetSettingsCategoryInfoByName("NewSettingGroup");
                if (updateGroup != null)
                {
                    // Updates the setting group properties
                    updateGroup.CategoryDisplayName = updateGroup.CategoryDisplayName.ToLowerCSafe();

                    // Saves the updated setting group to the database
                    SettingsCategoryInfoProvider.SetSettingsCategoryInfo(updateGroup);
                }
            }


            /// <heading>Updating multiple setting groups</heading>
            private void GetAndBulkUpdateSettingsGroups()
            {
                // Gets all setting groups whose code name starts with 'New'
                var groups = SettingsCategoryInfoProvider.GetSettingsCategories()
                                                                .WhereStartsWith("CategoryName", "New")
                                                                .WhereEquals("CategoryIsGroup", 1);
                
                // Loops through individual setting groups
                foreach (SettingsCategoryInfo group in groups)
                {
                    // Updates the group properties
                    group.CategoryDisplayName = group.CategoryDisplayName.ToUpper();

                    // Saves the updated setting group to the database
                    SettingsCategoryInfoProvider.SetSettingsCategoryInfo(group);
                }
            }


            /// <heading>Deleting a setting group</heading>
            private void DeleteSettingsGroup()
            {
                // Gets the setting group
                SettingsCategoryInfo deleteGroup = SettingsCategoryInfoProvider.GetSettingsCategoryInfoByName("NewSettingGroup");

                if (deleteGroup != null)
                {
                    // Deletes the setting group
                    SettingsCategoryInfoProvider.DeleteSettingsCategoryInfo(deleteGroup);
                }
            }
        }


        /// <summary>
        /// Holds setting key API examples.
        /// </summary>
        /// <groupHeading>Setting keys</groupHeading>
        private class SettingKeys
        {
            /// <heading>Creating a setting key</heading>
            private void CreateSettingsKey()
            {
                // Gets a parent setting group
                SettingsCategoryInfo settingGroup = SettingsCategoryInfoProvider.GetSettingsCategoryInfoByName("NewSettingGroup");
                if (settingGroup != null)
                {
                    // Creates a new setting key object
                    SettingsKeyInfo newKey = new SettingsKeyInfo();

                    // Sets the setting key properties
                    newKey.KeyDisplayName = "New setting key";
                    newKey.KeyName = "NewSettingKey";
                    newKey.KeyDescription = "This setting key was created through the API.";
                    newKey.KeyType = "string";
                    newKey.KeyValue = "Setting key value";
                    newKey.KeyCategoryID = settingGroup.CategoryID;
                    newKey.KeyDefaultValue = null;

                    // Saves the new setting key to the database
                    SettingsKeyInfoProvider.SetSettingsKeyInfo(newKey);
                }
            }


            /// <heading>Updating a setting key</heading>
            private void GetAndUpdateSettingsKey()
            {
                // Gets the setting key
                SettingsKeyInfo updateKey = SettingsKeyInfoProvider.GetSettingsKeyInfo("NewSettingKey");
                if (updateKey != null)
                {
                    // Updates the setting key properties
                    updateKey.KeyDisplayName = updateKey.KeyDisplayName.ToLowerCSafe();

                    // Saves the updated setting key to the database
                    SettingsKeyInfoProvider.SetSettingsKeyInfo(updateKey);
                }
            }


            /// <heading>Updating multiple setting keys</heading>
            private void GetAndBulkUpdateSettingsKeys()
            {
                // Gets a setting group
                SettingsCategoryInfo settingGroup = SettingsCategoryInfoProvider.GetSettingsCategoryInfoByName("NewSettingGroup");

                // Gets all setting keys in the specified setting group
                var keys = SettingsKeyInfoProvider.GetSettingsKeys().WhereEquals("KeyCategoryID", settingGroup.CategoryID);

                // Loops through individual setting keys
                foreach (SettingsKeyInfo modifyKey in keys)
                {
                    // Updates the setting key properties
                    modifyKey.KeyDisplayName = modifyKey.KeyDisplayName.ToUpper();

                    // Saves the updated setting key to the database
                    SettingsKeyInfoProvider.SetSettingsKeyInfo(modifyKey);
                }
            }


            /// <heading>Deleting a setting key</heading>
            private void DeleteSettingsKey()
            {
                // Gets the setting key
                SettingsKeyInfo deleteKey = SettingsKeyInfoProvider.GetSettingsKeyInfo("NewSettingKey");

                if (deleteKey != null)
                {
                    // Deletes the setting key
                    SettingsKeyInfoProvider.DeleteSettingsKeyInfo(deleteKey);
                }
            }
        }
    }
}
