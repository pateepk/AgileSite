using System;

namespace CMS.Core
{
    /// <summary>
    /// Main entry point to the application.
    /// </summary>
    public static class AppCore
    {
        private static readonly object mLock = new object();
        private static bool preInitInProgress;
        private static bool initInProgress;


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
    }
}