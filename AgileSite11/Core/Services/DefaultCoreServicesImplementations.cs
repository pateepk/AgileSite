using System;

using CMS.Core.Internal;

namespace CMS.Core
{
    /// <summary>
    /// Contains definition of core services and their default implementations.
    /// </summary>
    /// <remarks>
    /// The core services are required to be available during early application initialization phase and their
    /// implementation must be registered within inversion of control container in order for the application
    /// initialization to work.
    /// </remarks>
    /// <seealso cref="TypeManager.IoCContainer"/>
    internal static class DefaultCoreServicesImplementations
    {
        private static readonly Type[][] mTransient = {
            new [] { typeof(IPerformanceCounter), typeof(DefaultPerformanceCounter) },
        };


        private static readonly Type[][] mSingleton = {
            new [] { typeof(IWebFarmService), typeof(DefaultWebFarmService) },
            new [] { typeof(ISettingsService), typeof(DefaultSettingsService) },
            new [] { typeof(ILocalizationService), typeof(DefaultLocalizationService) },
            new [] { typeof(IInstallableModulesInformationService), typeof(DefaultInstallableModulesInformationService) },
            new [] { typeof(IEventLogService), typeof(DefaultEventLogService) },
            new [] { typeof(IDateTimeNowService), typeof(DefaultDateTimeNowService) },
            new [] { typeof(IConversionService), typeof(DefaultConversionService) },
            new [] { typeof(IConnectionStringService), typeof(DefaultConnectionStringService) },
            new [] { typeof(IAppSettingsService), typeof(DefaultAppSettingsService) },
        };


        /// <summary>
        /// Gets array of pairs of types (interface, default implementation) which are to be registered with transient lifestyle.
        /// </summary>
        public static Type[][] Transient
        {
            get
            {
                return mTransient;
            }
        }


        /// <summary>
        /// Gets array of pairs of types (interface, default implementation) which are to be registered with singleton lifestyle.
        /// </summary>
        public static Type[][] Singleton
        {
            get
            {
                return mSingleton;
            }
        }


        /// <summary>
        /// Registers core services with proper lifestyle as fallback implementations within <paramref name="container"/>.
        /// </summary>
        /// <param name="container">Inversion of control container to perform registration on.</param>
        /// <seealso cref="Transient"/>
        /// <seealso cref="Singleton"/>
        public static void Register(IoCContainer container)
        {
            foreach (var coreService in Singleton)
            {
                var registration = new Registration(coreService[0]).ImplementedBy(coreService[1]).LifestyleSingleton().IsFallback();
                container.Register(registration);
            }

            foreach (var coreService in Transient)
            {
                var registration = new Registration(coreService[0]).ImplementedBy(coreService[1]).LifestyleTransient().IsFallback();
                container.Register(registration);
            }
        }
    }
}
