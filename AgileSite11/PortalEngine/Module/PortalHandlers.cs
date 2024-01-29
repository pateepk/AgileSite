using System;
using System.Linq;
using System.Text;

using CMS.Base;
using CMS.DataEngine;
using CMS.DeviceProfiles;
using CMS.Helpers;
using CMS.Localization;
using CMS.Membership;
using CMS.Modules;

namespace CMS.PortalEngine
{
    /// <summary>
    /// Event handlers for portal engine module
    /// </summary>
    internal class PortalHandlers
    {
        /// <summary>
        /// Initializes the handlers
        /// </summary>
        public static void Init()
        {
            // Ensure live site mode for sign out
            SecurityEvents.SignOut.After += SignOut_After;

            SessionEvents.UpdateSession.Before += UpdateSession_Before;

            // Special macro resolving in localization strings
            LocalizationEvents.ResolveSubstitutionMacro.Execute += ResolveSubstitutionMacro_Execute;

            SettingsKeyInfoProvider.OnSettingsKeyChanged += ClearDeviceCache;

            // Clears provider's cache of target layouts when device profile is deleted
            DeviceProfileInfo.TYPEINFO.Events.Delete.After += ClearTargetLayoutIdentifierHashtable;
        }


        /// <summary>
        /// Checks if the session can be updated
        /// </summary>
        private static void UpdateSession_Before(object sender, CMSEventArgs e)
        {
            if (PortalContext.ViewMode != ViewModeEnum.LiveSite)
            {
                e.Cancel();
            }
        }


        /// <summary>
        /// Ensures LiveSite mode after SignOut
        /// </summary>
        private static void SignOut_After(object sender, SignOutEventArgs e)
        {
            PortalContext.ViewMode = ViewModeEnum.LiveSite;
        }


        /// <summary>
        /// Replaces special macros contained in resource string with navigation path to the given UI element.
        /// </summary>
        /// <param name="sender">Event argument</param>
        /// <param name="e">Sender object</param>
        private static void ResolveSubstitutionMacro_Execute(object sender, LocalizationEventArgs e)
        {
            string inputString = e.MacroValue;
            if (!String.IsNullOrEmpty(inputString) && (e.MacroType != null) && e.MacroType.EqualsCSafe("ui", true))
            {
                string[] parts = inputString.Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length == 2)
                {
                    var info = UIElementInfoProvider.GetUIElementInfo(parts[0], parts[1]);
                    if (info != null)
                    {
                        e.IsMatch = true;
                        e.MacroResult = info.GetApplicationNavigationString(e.CultureCode);
                    }
                }
            }
        }


        /// <summary>
        /// Settings key changed handler
        /// </summary>
        private static void ClearDeviceCache(object sender, SettingsKeyChangedEventArgs e)
        {
            if (e.KeyName.EqualsCSafe("CMSResizeImagesToDevice", true))
            {
                CacheHelper.TouchKey(DeviceProfileInfoProvider.DEVICE_IMAGE_CACHE_KEY);
            }
        }


        /// <summary>
        /// Clears provider's cache of target layouts based on device profile and source layout.
        /// </summary>
        private static void ClearTargetLayoutIdentifierHashtable(object sender, ObjectEventArgs e)
        {
            DeviceProfileLayoutInfoProvider.ClearTargetLayoutIdentifierHashtable(true);
        }
    }
}
