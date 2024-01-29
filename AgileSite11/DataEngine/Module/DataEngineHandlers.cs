using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Base;
using CMS.Helpers;

namespace CMS.DataEngine
{
    /// <summary>
    /// Handlers within Data engine module
    /// </summary>
    internal class DataEngineHandlers
    {
        #region "Constants"

        // Sequence of all settings key names that should fully clear cache upon change
        private static readonly Lazy<string[]> CLEAR_FULL_CACHE_KEYS = new Lazy<string[]>(() => new []
        {
            "cmscacheminutes",
            "cmscachepageinfo",
            "cmscacheimages",
            "cmsmaxcachefilesize",
            "cmsdefaultaliaspath",
            "cmsdefaultculturecode",
            "cmscombinewithdefaultculture"
        });


        // Sequence of all settings key names that should clear CSS cache upon change
        private static readonly Lazy<string[]> CLEAR_CSS_STYLES_KEYS = new Lazy<string[]>(() => new []
        {
            "cmsresourcecompressionenabled",
            "cmsstylesheetminificationenabled",
            "cmsresolvemacrosincss"
        });


        // Sequence of all settings key names that should clear partial cache upon change
        private static readonly Lazy<string[]> CLEAR_PARTIAL_CACHE_KEYS = new Lazy<string[]>(() => new []
        {
            "cmsenablepartialcache"
        });


        // Sequence of all settings key names that should set progressive caching
        private static readonly Lazy<string[]> PROGRESSIVE_CACHING_KEYS = new Lazy<string[]>(() => new []
        {
            "cmsprogressivecaching"
        });

        #endregion


        /// <summary>
        /// Initializes the handlers
        /// </summary>
        public static void Init()
        {
            ThreadEvents.Init.After += EnsureConnectionString;

            ThreadEvents.Run.Before += EnsureThreadRequestScope;
            ThreadEvents.Finalize.Execute += DisposeThreadRequestScope;

            RequestEvents.Prepare.Execute += InitRequestContext;
            RequestEvents.Finalize.Execute += DisposeRequestScope;

            SettingsKeyInfoProvider.OnSettingsKeyChanged += HandleCache;

            DataClassInfo.TYPEINFO.Events.Update.Before += UpdateSearchSettings;
        }


        /// <summary>
        /// Removes obsolete search settings from predefined object types.
        /// </summary>
        private static void UpdateSearchSettings(object sender, ObjectEventArgs e)
        {
            var obj = e.Object as DataClassInfo;

            if (obj != null && obj.ItemChanged("ClassXmlSchema"))
            {
                if (obj.ClassName.Equals(PredefinedObjectType.USERSETTINGS, StringComparison.OrdinalIgnoreCase))
                {
                    e.CallWhenFinished(() =>
                    {
                        var dc = DataClassInfoProvider.GetDataClassInfo(PredefinedObjectType.USER);

                        dc.RemoveObsoleteSearchSettings();
                        DataClassInfoProvider.SetDataClassInfo(dc);
                    });
                }
                else if (obj.ClassName.Equals(PredefinedObjectType.SKU, StringComparison.OrdinalIgnoreCase) || obj.ClassName.Equals("cms.tree", StringComparison.OrdinalIgnoreCase))
                {
                    e.CallWhenFinished(() =>
                    {
                        var dc = DataClassInfoProvider.GetDataClassInfo(PredefinedObjectType.DOCUMENT);

                        dc.RemoveObsoleteSearchSettings();
                        DataClassInfoProvider.SetDataClassInfo(dc);
                    });
                }
            }
        }


        /// <summary>
        /// Initializes the request context to use proper database based on current domain name
        /// </summary>
        private static void InitRequestContext(object sender, EventArgs eventArgs)
        {
            ConnectionHelper.InitRequestContext();

            // Create request scope
            if (ConnectionHelper.ConnectionAvailable)
            {
                if (ConnectionHelper.UseContextConnection)
                {
                    ConnectionContext.EnsureThreadScope(null);
                }

                // Enable debugging
                DebugHelper.SetInitialDebug();
            }
        }


        /// <summary>
        /// Disposes the request connection scope
        /// </summary>
        private static void DisposeRequestScope(object sender, EventArgs e)
        {
            ConnectionContext.DisposeThreadScope();
        }


        /// <summary>
        /// Fires when thread cleanups its context
        /// </summary>
        private static void DisposeThreadRequestScope(object sender, ThreadEventArgs e)
        {
            var thr = e.Thread;

            if (thr.Mode != ThreadModeEnum.Sync)
            {
                ConnectionContext.DisposeThreadScope();
            }
        }


        /// <summary>
        /// Fires before the thread runs
        /// </summary>
        private static void EnsureThreadRequestScope(object sender, ThreadEventArgs e)
        {
            var thr = e.Thread;

            // Create thread scope
            if ((thr.Mode != ThreadModeEnum.Sync) && ConnectionHelper.UseContextConnection)
            {
                ConnectionContext.EnsureThreadScope(thr.ConnectionString);
            }
        }


        /// <summary>
        /// Fires when the new thread starts
        /// </summary>
        private static void EnsureConnectionString(object sender, ThreadEventArgs e)
        {
            var thr = e.Thread;

            if ((thr.Mode == ThreadModeEnum.Async) && (thr.ConnectionString == null))
            {
                // Get the connection string settings from the current scope
                var scope = ConnectionContext.CurrentConnectionScope;
                if (scope != null)
                {
                    thr.ConnectionString = scope.ConnectionString;
                }
            }
        }


        /// <summary>
        /// Based on presence of <see cref="SettingsKeyChangedEventArgs.KeyName"/> from <paramref name="eventArgs"/> in
        /// <paramref name="settingsKeys"/> executes <paramref name="clearingAction"/>.
        /// </summary>
        /// <param name="settingsKeys">Sequence of settings keys names</param>
        /// <param name="eventArgs">Settings key arguments of current handler</param>
        /// <param name="clearingAction">Action to be executed on <paramref name="settingsKeys"/> hit</param>
        private static void ClearCacheFor(IEnumerable<string> settingsKeys, SettingsKeyChangedEventArgs eventArgs, Action clearingAction)
        {
            if (!settingsKeys.Contains(eventArgs.KeyName, StringComparer.OrdinalIgnoreCase))
            {
                // Setting key is not in the sequence
                return;
            }

            // Execute defined clearing action
            clearingAction();
        }


        /// <summary>
        /// Settings key changed handler
        /// </summary>
        private static void HandleCache(object sender, SettingsKeyChangedEventArgs e)
        {
            ClearCacheFor(CLEAR_FULL_CACHE_KEYS.Value, e, () => CacheHelper.ClearCache(null));
            ClearCacheFor(CLEAR_CSS_STYLES_KEYS.Value, e, () => CacheHelper.ClearCSSCache());

            ClearCacheFor(CLEAR_PARTIAL_CACHE_KEYS.Value, e, () => CacheHelper.ClearPartialCache());

            ClearCacheFor(PROGRESSIVE_CACHING_KEYS.Value, e, () => { CacheHelper.ProgressiveCaching = e.KeyValue.ToBoolean(false); });
        }
    }
}