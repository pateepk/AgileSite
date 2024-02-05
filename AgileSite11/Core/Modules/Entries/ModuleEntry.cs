using System;
using System.Linq;
using System.Reflection;

namespace CMS.Core
{
    /// <summary>
    /// Represents the standard module entry.
    /// </summary>
    public abstract class ModuleEntry
    {
        #region "Variables"

        private bool mAutoInitialize = true;
        private bool mIsDiscoverable = true;
        private bool? mIsInstalled;

        private readonly object locker = new object();

        private bool preInitInProgress;
        private bool initInProgress;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets the module metadata.
        /// </summary>
        public ModuleMetadata ModuleMetadata
        {
            get;
            protected set;
        }


        /// <summary>
        /// Gets the module info.
        /// </summary>
        public ModuleInfo ModuleInfo
        {
            get;
            protected set;
        }


        /// <summary>
        /// True if the module was initialized
        /// </summary>
        public bool Initialized
        {
            get;
            protected set;
        }


        /// <summary>
        /// True if the module was pre-initialized
        /// </summary>
        public bool PreInitialized
        {
            get;
            protected set;
        }


        /// <summary>
        /// If true, the module is allowed to be initialized automatically
        /// </summary>
        public bool AutoInitialize
        {
            get
            {
                return mAutoInitialize;
            }
            protected set
            {
                mAutoInitialize = value;
            }
        }


        /// <summary>
        /// If true, this module is automatically discoverable
        /// </summary>
        public bool IsDiscoverable
        {
            get
            {
                return mIsDiscoverable;
            }
            set
            {
                mIsDiscoverable = value;
            }
        }


        /// <summary>
        /// Indicates if module is designed as installable.
        /// </summary>
        /// <remarks>
        /// Installable modules which are not installed in the database are omitted from initialization.
        /// </remarks>
        public bool IsInstallable
        {
            get;
            private set;
        }


        /// <summary>
        /// Indicates if installable module is installed.
        /// </summary>
        /// <remarks>
        /// <para>The value of this property is available in application init phase.</para>
        /// <para>Returns false for modules which are not installable.</para>
        /// <para>Returns true for installable modules which are being developed on this instance.</para>
        /// </remarks>
        /// <seealso cref="IsInstallable"/>
        internal bool IsInstalled
        {
            get
            {
                return mIsInstalled.HasValue && mIsInstalled.Value;
            }
            set
            {
                mIsInstalled = value;
            }
        }


        /// <summary>
        /// Gets assembly the module resides in.
        /// </summary>
        /// <remarks>
        /// Available only for modules discovered via <see cref="ModuleDiscovery"/>.
        /// </remarks>
        internal Assembly Assembly
        {
            get;
            set;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        protected ModuleEntry(ModuleMetadata metadata, bool isInstallable = false)
        {
            if (metadata == null)
            {
                throw new ArgumentNullException("metadata");
            }

            ModuleMetadata = metadata;
            IsInstallable = isInstallable;
            ModuleInfo = new ModuleInfo(this);
        }

        #endregion


        #region Initialization methods

        /// <summary>
        /// Pre-initializes the module.
        /// </summary>
        public void PreInit()
        {
            if (!PreInitialized)
            {
                lock (locker)
                {
                    if (!PreInitialized && !preInitInProgress)
                    {
                        preInitInProgress = true;

                        OnPreInit();

                        PreInitialized = true;
                    }
                }
            }
        }


        /// <summary>
        /// Initializes the module.
        /// </summary>
        public void Init()
        {
            PreInit();

            if (!Initialized)
            {
                lock (locker)
                {
                    if (!Initialized && !initInProgress)
                    {
                        initInProgress = true;

                        OnInit();

                        Initialized = true;
                    }
                }
            }
        }


        /// <summary>
        /// Handles the module pre-initialization.
        /// </summary>
        protected virtual void OnPreInit()
        {
            // No actions by default
        }


        /// <summary>
        /// Handles the module initialization.
        /// </summary>
        protected virtual void OnInit()
        {
            // No actions by default
        }

        #endregion
    }
}
