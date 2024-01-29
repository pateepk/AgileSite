using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace CMS.Core
{
    /// <summary>
    /// Main entry point to the application.
    /// </summary>
    public static class AppCore
    {
        #region "Variables"

        // An object used to synchronize access to this class.
        private static readonly object mLock = new object();

        // An object used to synchronize assembly resolver initialization
        private static readonly object assemblyResolverLock = new object();
         
        // Application core startup parameters.
        private static AppCoreSetup mSetup;

        private static bool assemblyResolverInitialized;
        private static bool assemblyResolverInitInProgress;
        private static bool preInitInProgress;
        private static bool initInProgress;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets a value indicating whether the application core has been pre-initialized.
        /// </summary>
        public static bool PreInitialized
        {
            get;
            private set;
        }


        /// <summary>
        /// Gets a value indicating whether the application core has been initialized.
        /// </summary>
        public static bool Initialized
        {
            get;
            private set;
        }


        /// <summary>
        /// Gets current application core startup parameters.
        /// </summary>
        private static AppCoreSetup CurrentSetup
        {
            get
            {
                lock (mLock)
                {
                    return mSetup ?? CreateDefaultSetup();
                }
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Initializes resolution of private assemblies. Private assemblies are to be initialized during application pre-initialization phase
        /// as modules processed during pre-initialization may reference private assemblies.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Private assemblies are located in the CMSDependencies folder and are intended for Kentico only.
        /// Kentico modules reference private assemblies to avoid conflict with customer's assemblies.
        /// </para>
        /// <para>
        /// This method is to be called from <see cref="PreInit"/>. However, in order to avoid application pre-initialization loop, it may be necessary to call it individually (prior to <see cref="PreInit"/>).
        /// </para>
        /// <para>
        /// No locks which could cause application pre-initialization deadlock are acquired during assembly resolver initialization.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown when a loop occurs in assembly resolver initialization.</exception>
        internal static void InitializeAssemblyResolver()
        {
            if (!assemblyResolverInitialized)
            {
                lock(assemblyResolverLock)
                {
                    if (!assemblyResolverInitialized)
                    {
                        if (assemblyResolverInitInProgress)
                        {
                            throw new InvalidOperationException("A loop occurred in assembly resolver initialization. This is likely a result of improper initialization methods call hierarchy.");
                        }

                        assemblyResolverInitInProgress = true;

                        InitializeAssemblyResolverCore();

                        Thread.MemoryBarrier();

                        assemblyResolverInitialized = true;
                    }
                }
            }
        }


        /// <summary>
        /// Determines whether the pre-initialization phase of the application core life-cycle is complete. If it is not, it executes it.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method is not intended to be used in custom code. Use CMS.DataEngine.CMSApplication.PreInit() instead.
        /// </para>
        /// <para>
        /// A loop in application core pre-initialization is typically caused by some module's pre-initialization code trying to perform actions which require the application core to be
        /// already pre-initialized. Module's pre-initialization must never rely on such actions.
        /// </para>
        /// </remarks>
        /// <returns>True, the pre-initialization phase of the application core life-cycle has been executed; otherwise, false.</returns>
        /// <exception cref="InvalidOperationException">Thrown when a loop occurs in application core pre-initialization.</exception>
        public static bool PreInit()
        {
            if (!PreInitialized)
            {
                lock (mLock)
                {
                    if (!PreInitialized)
                    {
                        if (preInitInProgress)
                        {
                            throw new InvalidOperationException("A loop occurred in application core pre-initialization. This is likely a result of improper pre-initialization in some module.");
                        }

                        // Set the flag preventing infinite recursion
                        preInitInProgress = true;

                        // Initialize resolution of private assemblies.
                        InitializeAssemblyResolver();

                        // Pre-initialize modules.
                        ModuleEntryManager.PreInit();

                        PreInitialized = true;

                        return true;
                    }
                }
            }

            return false;
        }
        

        /// <summary>
        /// Determines whether the initialization phase of the application core life-cycle is complete. If it is not, it executes it.
        /// </summary>
        /// <remarks>
        /// This method is not intended to be used in custom code. Use CMS.DataEngine.CMSApplication.Init() instead.
        /// </remarks>
        /// <returns>True, if the initialization phase of the application core life-cycle has been executed; otherwise, false.</returns>
        public static bool Init()
        {
            PreInit();

            if (!Initialized)
            {
                lock (mLock)
                {
                    if (!Initialized && !initInProgress)
                    {
                        // Set the flag preventing infinite recursion
                        initInProgress = true;

                        // Initialize modules.
                        ModuleEntryManager.Init();

                        Initialized = true;

                        return true;
                    }
                }
            }

            return false;
        }


        /// <summary>
        /// Provides the application core with startup parameters. The setup can be set only once, subsequent attempts are ignored.
        /// </summary>
        /// <param name="setup">Application core startup parameters.</param>
        public static void Setup(AppCoreSetup setup)
        {
            lock (mLock)
            {
                if (mSetup == null)
                {
                    mSetup = setup;
                }
            }
        }


        /// <summary>
        /// Creates default application startup parameters and returns it.
        /// </summary>
        /// <returns>Default application startup parameters.</returns>
        private static AppCoreSetup CreateDefaultSetup()
        {
#pragma warning disable BH1014 // Do not use System.IO
            var dependenciesFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CMSDependencies");
#pragma warning restore BH1014 // Do not use System.IO

            var builder = new AppCoreSetup.Builder().WithDependenciesFolderPath(dependenciesFolderPath);

            return builder.Build();
        }


        /// <summary>
        /// Initializes resolution of private assemblies.
        /// </summary>
        /// <remarks>
        /// Private assemblies are located in the CMSDependencies folder and are intended for Kentico only.
        /// Kentico modules reference private assemblies to avoid conflict with customer's assemblies.
        /// </remarks>
        private static void InitializeAssemblyResolverCore()
        {
            // Any locks which could possibly cause an application pre-initialization deadlock must be avoided
            var dependenciesFolderPath = CurrentSetup.DependenciesFolderPath;

            Trace.TraceInformation("Initialize assembly resolver with path {0}", dependenciesFolderPath);

#pragma warning disable BH1014 // Do not use System.IO
            if (Directory.Exists(dependenciesFolderPath))
            {
                var resolver = new DefaultAssemblyResolver(dependenciesFolderPath);
                AppDomain.CurrentDomain.AssemblyResolve += (sender, arguments) => resolver.GetAssembly(arguments);
            }
#pragma warning restore BH1014 // Do not use System.IO
        }

        #endregion
    }
}