using System;

using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.UIControls.Internal;

namespace CMS.UIControls
{
    /// <summary>
    /// Provides utility methods for the hotfixing process.
    /// </summary>
    internal class HotfixHelper
    {
        internal const string EVENT_SOURCE = "HotfixProcedure";
        internal const string EVENT_CODE = "HOTFIX";

        internal const string HOTFIX_VERSION_KEY_NAME = "CMSHotfixVersion";
        internal const string HOTFIX_DATA_VERSION_KEY_NAME = "CMSHotfixDataVersion";
        internal const string HOTFIX_PROCEDURE_IN_PROGRESS_KEY_NAME = "CMSHotfixProcedureInProgress";

        internal const string UPGRADE_VERSION_KEY_NAME = "CMSDBVersion";
        internal const string UPGRADE_DATA_VERSION_KEY_NAME = "CMSDataVersion";

        private Version mCMSVersion;


        internal Version CMSVersion
        {
            get
            {
                return mCMSVersion ?? (mCMSVersion = Base.CMSVersion.Version);
            }
            set
            {
                mCMSVersion = value;
            }
        }


        /// <summary>
        /// Gets a value indicating whether hotfix procedure is to be executed.
        /// </summary>
        /// <param name="hotfixVersion">Hotfix version according to the database as applied by the Hotfix utility.</param>
        /// <param name="hotfixDataVersion">Hotfix version according to the database as applied by the last hotfix procedure run. If <paramref name="hotfixVersion"/> is 0 then this value is 0 as well.</param>
        /// <returns>Returns true if hotfix application is to be skipped, otherwise returns false.</returns>
        /// <exception cref="InvalidOperationException">Thrown when hotfix version is lower than hotfix data version (data are ahead of actually applied hotfix) or an unfinished upgrade is detected.</exception>
        /// <remarks>
        /// <para>
        /// Hotfix version equivalent to hotfix data version indicates the instance's files are on the same hotfix version as the database data
        /// and the hotfix procedure is not to be applied.
        /// </para>
        /// <para>
        /// Hotfix version higher than hotfix data version indicates the instance's files were hotfixed by the Hotfix utility and the database data has to be brought
        /// to an up-to-date state by applying the hotfix procedure.
        /// </para>
        /// <para>
        /// Hotfix version lower than hotfix data version indicates the database data is already on higher version than the instance's files which can happen
        /// when deployment of a new hotfix is not atomic (e.g. Azure update groups). Hotfix procedure is not to be applied.
        /// </para>
        /// <para>
        /// The method also tests whether assembly version corresponds to the hotfix version in the database to prevent premature hotfix procedure run
        /// in a situation when Hotfix utility has already updated the database but hotfixed assemblies have not been deployed yet (i.e. the instance
        /// runs the old assemblies).
        /// </para>
        /// </remarks>
        public bool SkipHotfixing(out int hotfixVersion, out int hotfixDataVersion)
        {
            hotfixVersion = GetSettingsKeyIntValue(HOTFIX_VERSION_KEY_NAME, true);
            if (hotfixVersion < 1)
            {
                hotfixDataVersion = 0;

                return true;
            }

            hotfixDataVersion = GetSettingsKeyIntValue(HOTFIX_DATA_VERSION_KEY_NAME, true);
            if (hotfixVersion == hotfixDataVersion)
            {
                return true;
            }

            if (hotfixVersion < hotfixDataVersion)
            {
                throw new InvalidOperationException("Database hotfix version as applied by the Hotfix utility is lower than the database data version as applied by the application HotfixProcedure.");
            }

            // Test whether instance is deployed with up-to-date files
            if (!HotfixTestHelper.IsRunningInTestMode && CMSVersion.Build != hotfixVersion)
            {
                LogHotfixAndBuilderVersionMismatch(hotfixVersion, CMSVersion.Build);

                return true;
            }

            ValidateDataAndDbVersionOrThrow();

            return false;
        }


        /// <summary>
        /// Gets a value indicating whether hotfix procedure is to be executed. This method is to be called as part of a transaction
        /// after previous <see cref="SkipHotfixing"/> check indicated the hotfix procedure is to be executed.
        /// </summary>
        /// <param name="hotfixVersion">Hotfix version according to the database as applied by the Hotfix utility.</param>
        /// <param name="hotfixDataVersion">Hotfix version according to the database as applied by the last hotfix procedure run. If <paramref name="hotfixVersion"/> is 0 then this value is 0 as well.</param>
        /// <returns>Returns true if hotfix application is to be skipped, otherwise returns false.</returns>
        /// <exception cref="InvalidOperationException">Thrown when hotfix version is lower than hotfix data version (data are ahead of actually applied hotfix) or an unfinished upgrade is detected.</exception>
        /// <remarks>
        /// <para>
        /// Unlike the <see cref="SkipHotfixing"/> method, this method avoids obtaining the version values from a cache as hotfix procedure execution in a web farm environment
        /// requires up-to-date values.
        /// </para>
        /// <para>
        /// See <see cref="SkipHotfixing"/> method's Remarks section on details of the individual version values.
        /// </para>
        /// </remarks>
        public bool RevalidateSkipHotfixing(out int hotfixVersion, out int hotfixDataVersion)
        {
            hotfixVersion = GetSettingsKeyIntValue(HOTFIX_VERSION_KEY_NAME, false);
            if (hotfixVersion < 1)
            {
                hotfixDataVersion = 0;

                return true;
            }

            hotfixDataVersion = GetSettingsKeyIntValue(HOTFIX_DATA_VERSION_KEY_NAME, false);
            if (hotfixVersion == hotfixDataVersion)
            {
                return true;
            }

            // Test whether instance is deployed with up-to-date files
            if (!HotfixTestHelper.IsRunningInTestMode && CMSVersion.Build != hotfixVersion)
            {
                LogHotfixAndBuilderVersionMismatch(hotfixVersion, CMSVersion.Build);

                return true;
            }

            return false;
        }


        private int GetSettingsKeyIntValue(string keyName, bool allowCachedRead)
        {
            if (allowCachedRead)
            {
                return SettingsKeyInfoProvider.GetIntValue(keyName);
            }

            return ValidationHelper.GetInteger(SettingsKeyInfoProvider.GetValueFromDB(keyName), 0);
        }


        private void LogHotfixAndBuilderVersionMismatch(int hotfixVersion, int buildNumber)
        {
            EventLogProvider.LogWarning(EVENT_SOURCE, EVENT_CODE, null, 0, $"Hotfix procedure cannot be applied. Database hotfix version ({hotfixVersion}) is different from assembly application version ({buildNumber}).");
        }


        /// <summary>
        /// Validates that 'CMSDataVersion' and 'CMSDBVersion' match (a mismatch indicates an unfinished upgrade).
        /// </summary>
        private void ValidateDataAndDbVersionOrThrow()
        {
            string dataVersion = SettingsKeyInfoProvider.GetValue(UPGRADE_DATA_VERSION_KEY_NAME);
            string dbVersion = SettingsKeyInfoProvider.GetValue(UPGRADE_VERSION_KEY_NAME);
            if (!dataVersion.Equals(dbVersion, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Upgrade procedure has not run on the upgraded instance yet. Hotfix procedure cannot be applied. This may be caused by applying a hotfix before running an upgraded instance for the first time, preventing the upgrade from finishing.");
            }
        }


        /// <summary>
        /// Marks hotfix procedure start in the database (if the procedure is not already running on a different instance).
        /// </summary>
        /// <returns>Returns true if the mark was set, otherwise returns false.</returns>
        public bool MarkHotfixProcedureStart()
        {
            if (ConnectionHelper.ExecuteNonQuery($"UPDATE [CMS_SettingsKey] SET [KeyValue] = 'True' WHERE [KeyName] = '{HOTFIX_PROCEDURE_IN_PROGRESS_KEY_NAME}' AND ([KeyValue] != 'True' OR [KeyValue] IS NULL)", null, QueryTypeEnum.SQLQuery) == 1)
            {
                return true;
            }

            return false;
        }


        /// <summary>
        /// Marks hotfix procedure end in the database.
        /// </summary>
        public void MarkHotfixProcedureEnd()
        {
            ConnectionHelper.ExecuteNonQuery($"UPDATE [CMS_SettingsKey] SET [KeyValue] = 'False' WHERE [KeyName] = '{HOTFIX_PROCEDURE_IN_PROGRESS_KEY_NAME}'", null, QueryTypeEnum.SQLQuery);
        }
    }
}
