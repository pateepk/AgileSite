using CMS.MacroEngine;

namespace CMS.DeviceProfiles
{
    /// <summary>
    /// Resolvers used in e-mail templates and other macro visual components.
    /// </summary>
    public class DeviceProfilesResolvers : ResolverDefinition
    {
        #region "Variables"

        private static MacroResolver mDeviceProfilesResolver;

        #endregion


        /// <summary>
        /// Returns device profiles macro resolver.
        /// </summary>
        public static MacroResolver DeviceProfilesResolver
        {
            get
            {
                if (mDeviceProfilesResolver == null)
                {
                    MacroResolver resolver = MacroResolver.GetInstance();

                    resolver.PrioritizeProperty("CurrentDevice");
                    resolver.ShowOnlyPrioritized = true;

                    mDeviceProfilesResolver = resolver;
                }

                return mDeviceProfilesResolver;
            }
        }        
    }
}