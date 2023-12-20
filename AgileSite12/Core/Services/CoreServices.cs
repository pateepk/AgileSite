using System;

namespace CMS.Core
{
    /// <summary>
    /// Provides basic core services
    /// </summary>
    public class CoreServices
    {
        #region "Properties"

        /// <summary>
        /// Localization service
        /// </summary>
        public static ILocalizationService Localization 
        {
            get
            {
                return Service.Resolve<ILocalizationService>();
            }
        }


        /// <summary>
        /// Settings service for connection strings
        /// </summary>
        [Obsolete("Use CMS.Core.Service.Resolve<IConnectionStringService> instead.")]
        public static IConnectionStringService ConnectionStrings
        {
            get
            {
                return Service.Resolve<IConnectionStringService>();
            }
        }


        /// <summary>
        /// Settings service for app settings
        /// </summary>
        public static IAppSettingsService AppSettings 
        {
            get
            {
                return Service.Resolve<IAppSettingsService>();
            }
        }


        /// <summary>
        /// Settings service for site settings
        /// </summary>
        public static ISettingsService Settings
        {
            get
            {
                return Service.Resolve<ISettingsService>();
            }
        }


        /// <summary>
        /// Conversion service for the data values
        /// </summary>
        public static IConversionService Conversion
        {
            get
            {
                return Service.Resolve<IConversionService>();
            }
        }


        /// <summary>
        /// Event log service
        /// </summary>
        public static IEventLogService EventLog
        {
            get
            {
                return Service.Resolve<IEventLogService>();
            }
        }


        /// <summary>
        /// Web farm service
        /// </summary>
        public static IWebFarmService WebFarm
        {
            get
            {
                return Service.Resolve<IWebFarmService>();
            }
        }

        #endregion
    }
}
