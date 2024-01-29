using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

using CMS.Core.Internal;


namespace CMS.Core
{
    /// <summary>
    /// Provides management of module info objects in the hosting CMS application.
    /// </summary>
    public class ModuleEntryManager
    {
        // The ModuleEntryManager is optimized for read access, which is lock free.
        // Most modules are discovered during EnsureModuleCollectionsInitialization, only a few are registered via RegisterModule().


        #region "Private data structures"

        /// <summary>
        /// Lightweight data structure holding collections of modules optimized for lookup and enumeration.
        /// Used to wrap the collections so they can be published in one assignment.
        /// </summary>
        private class ModuleCollections
        {
            private readonly Dictionary<string, ModuleInfo> mModuleLookup;
            private readonly List<ModuleInfo> mModules;


            /// <summary>
            /// Gets dictionary of modules.
            /// </summary>
            public Dictionary<string, ModuleInfo> ModuleLookup
            {
                get
                {
                    return mModuleLookup;
                }
            }


            /// <summary>
            /// Gets list of modules.
            /// </summary>
            public List<ModuleInfo> Modules
            {
                get
                {
                    return mModules;
                }
            }


            /// <summary>
            /// Initializes this data structure with corresponding dictionary and list collections.
            /// </summary>
            /// <param name="moduleLookup">Dictionary of modules for lookup.</param>
            /// <param name="modules">List of modules for enumeration.</param>
            public ModuleCollections(Dictionary<string, ModuleInfo> moduleLookup, List<ModuleInfo> modules)
            {
                mModuleLookup = moduleLookup;
                mModules = modules;
            }
        }

        #endregion


        #region "Variables"

        // Collections of module info objects. Can be assigned from lock context of mModulesLock only
        private static ModuleCollections mModuleCollections;

        // Object for locking initialization or publication of new module collections (mModuleCollections)
        private static readonly object mModulesLock = new object();

        #endregion


        #region "Properties"

        // If you need to ensure that Modules and ModuleLookup contains the same modules between two consecutive calls,
        // obtain a reference to ModuleCollections and work with it.

        /// <summary>
        /// Gets an enumerable collection of module info objects.
        /// </summary>
        public static IEnumerable<ModuleInfo> Modules
        {
            get
            {
                EnsureModuleCollectionsInitialization();

                return mModuleCollections.Modules.AsReadOnly();
            }
        }


        /// <summary>
        /// Gets a lookup collection of module info objects. For private use only as the returned dictionary is not read-only.
        /// </summary>
        private static Dictionary<string, ModuleInfo> ModuleLookup
        {
            get
            {
                EnsureModuleCollectionsInitialization();

                return mModuleCollections.ModuleLookup;
            }
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Registers a module info object.
        /// </summary>
        /// <param name="moduleInfo">The module info object to register.</param>
        /// <exception cref="ArgumentNullException"><paramref name="moduleInfo"/></exception>
        /// <exception cref="ArgumentException">Thrown when <see cref="ModuleInfo"/> with the same <see cref="ModuleInfo.Name"/> is already registered.</exception>
        public static void RegisterModule(ModuleInfo moduleInfo)
        {
            if (moduleInfo == null)
            {
                throw new ArgumentNullException("moduleInfo");
            }

            lock (mModulesLock)
            {
                // It is assumed that RegisterModule is not called very often and fails only rarely so the check is performed only within CS
                if (ModuleLookup.ContainsKey(moduleInfo.Name))
                {
                    throw new ArgumentException(String.Format("Module '{0}' has been already registered.", moduleInfo.Name), "moduleInfo");
                }

                // Create a copy of current collections, add new module and publish atomically
                RegisterModuleCore(moduleInfo);
            }
        }
        

        /// <summary>
        /// Gets a module info object with the specified name.
        /// </summary>
        /// <param name="name">The name of the module info object to get.</param>
        /// <exception cref="ArgumentNullException"><paramref name="name"/></exception>
        /// <returns>The module info object with the specified name, if found; otherwise, null.</returns>
        public static ModuleInfo GetModuleInfo(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            ModuleInfo module;

            return ModuleLookup.TryGetValue(name, out module) ? module : null;
        }


        /// <summary>
        /// Determines whether the module info object with the specified name is available.
        /// </summary>
        /// <param name="name">The name of the module info object to locate.</param>
        /// <exception cref="ArgumentNullException"><paramref name="name"/></exception>
        /// <returns>True, if the module info object with the specified name is available; otherwise, false.</returns>
        public static bool IsModuleLoaded(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            return ModuleLookup.ContainsKey(name);
        }


        /// <summary>
        /// Ensures modules contained in the app code assembly, which is not discoverable by default
        /// </summary>
        /// <remarks>
        /// The web site project has no explicit assembly where the <see cref="AssemblyDiscoverableAttribute"/> could be placed.
        /// </remarks>
        /// <param name="appCodeAssembly">Assembly which represents the app code.</param>
        public static void EnsureAppCodeModules(Assembly appCodeAssembly)
        {
            var discovery = new ModuleDiscovery();

            foreach (var module in discovery.GetModules(appCodeAssembly))
            {
                EnsureModule(module);
            }
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Ensures the collections of modules
        /// </summary>
        private static void EnsureModuleCollectionsInitialization()
        {
            if (mModuleCollections == null)
            {
                lock (mModulesLock)
                {
                    if (mModuleCollections == null)
                    {
                        // Prepare both collections
                        var modules = GetDiscoverableModules().ToList();
                        var moduleLookup = modules.ToDictionary(x => x.Name, StringComparer.InvariantCultureIgnoreCase);
                        var moduleItems = new ModuleCollections(moduleLookup, modules);

                        Thread.MemoryBarrier();

                        // Publish them atomically
                        mModuleCollections = moduleItems;
                    }
                }
            }
        }


        /// <summary>
        /// Ensures that the specified module is registered.
        /// </summary>
        /// <remarks>
        /// If a module info object with the same name is already available, the manager does nothing.
        /// If the application life-cycle has already started and the module has <see cref="ModuleEntry.AutoInitialize"/> set to true,
        /// the module initialization starts immediately to catch up (if called from multiple threads, the method does not return
        /// until the module has caught up the application life-cycle).
        /// </remarks>
        /// <param name="module">The module to register. If the module is already registered, this instance is not used (i.e. initialization occurs only on the first registered module instance of the name).</param>
        private static void EnsureModule(ModuleEntry module)
        {
            bool moduleAlreadyRegistered = ModuleLookup.ContainsKey(module.ModuleMetadata.Name);
            if (!moduleAlreadyRegistered)
            {
                lock (mModulesLock)
                {
                    moduleAlreadyRegistered = ModuleLookup.ContainsKey(module.ModuleMetadata.Name);
                    if (!moduleAlreadyRegistered)
                    {
                        var moduleInfo = new ModuleInfo(module);

                        // Create a copy of current collections, add new module and publish atomically
                        RegisterModuleCore(moduleInfo);
                    }
                }
            }

            if (moduleAlreadyRegistered)
            {
                // Make sure the following InitializeModule call is synchronized on one ModuleEntry object
                module = ModuleLookup[module.ModuleMetadata.Name].Module;
            }


            // Register assembly so that the type manager can discover embed types.
            TypeManager.RegisterDynamicAssembly(module.GetType().Assembly);

            // Catch up with the application life-cycle. If this method is called from multiple threads, the InitializeModule call will
            // make sure that this method does not return in any thread until the init catches up the application life-cycle.
            if (module.AutoInitialize)
            {
                InitializeModule(module);
            }
        }


        /// <summary>
        /// Creates a copy of <see cref="mModuleCollections"/>, adds <paramref name="moduleInfo"/> to the collections and atomically publishes the copy.
        /// This method is to be called from lock context of <see cref="mModulesLock"/>.
        /// </summary>
        /// <param name="moduleInfo">Module to be added to both <see cref="ModuleCollections"/>' collections.</param>
        private static void RegisterModuleCore(ModuleInfo moduleInfo)
        {
            var modules = new List<ModuleInfo>(Modules);
            var moduleLookup = new Dictionary<string, ModuleInfo>(ModuleLookup, StringComparer.InvariantCultureIgnoreCase);

            modules.Add(moduleInfo);
            moduleLookup.Add(moduleInfo.Name, moduleInfo);

            var moduleCollections = new ModuleCollections(moduleLookup, modules);

            Thread.MemoryBarrier();

            mModuleCollections = moduleCollections;
        }


        /// <summary>
        /// Returns an enumerable collection of discoverable modules sorted by module dependencies.
        /// </summary>
        /// <returns>An enumerable collection of discoverable modules sorted by module dependencies.</returns>
        private static IEnumerable<ModuleInfo> GetDiscoverableModules()
        {
            var entries = new ModuleDiscovery().GetModules().Where(x => x.IsDiscoverable).ToList();

            return new ModuleEntrySort().Sort(entries).Select(x => new ModuleInfo(x));
        }


        /// <summary>
        /// Initializes the specified module and types from its assembly to catch up with the application life-cycle.
        /// </summary>
        /// <param name="module">The module to initialize.</param>
        private static void InitializeModule(ModuleEntry module)
        {
            var assembly = module.GetType().Assembly;

            // Types must be pre-initialized prior to module pre-initialization
            if (TypeManager.PreInitialized)
            {
                TypeManager.PreInitializeTypes(assembly);
            }

            if (AppCore.PreInitialized)
            {
                module.PreInit();
            }

            if (TypeManager.Initialized)
            {
                TypeManager.InitializeTypes(assembly);
            }

            if (AppCore.Initialized)
            {
                module.Init();
            }
        }


        /// <summary>
        /// Gets module entries suitable for PreInit. All modules which have <see cref="ModuleEntry.AutoInitialize"/> set are suitable.
        /// </summary>
        /// <returns>Enumeration of module entries.</returns>
        private static IEnumerable<ModuleEntry> GetModuleEntriesForAutoPreInitialization()
        {
            // Select modules which support AutoInitialize
            var modules = Modules.Where(x => x.Module != null).Select(x => x.Module).Where(x => x.AutoInitialize);

            return modules;
        }


        /// <summary>
        /// Gets module entries suitable for Init. Only modules which have <see cref="ModuleEntry.AutoInitialize"/> set and are installed
        /// in the same version as their corresponding NuGet package are suitable.
        /// </summary>
        /// <returns>Enumeration of module entries.</returns>
        private static IEnumerable<ModuleEntry> GetModuleEntriesForAutoInitialization()
        {
            // Select modules which are either non-installable or suitable for init
            var modules = Modules.Where(x => x.Module != null && (!x.Module.IsInstallable || x.Module.IsInstalled)).Select(x => x.Module).Where(x => x.AutoInitialize);

            return modules;
        }


        /// <summary>
        /// Sets <see cref="ModuleEntry.IsInstalled"/> state in all installable modules.
        /// </summary>
        private static void SetInstallableModulesState()
        {
            ISet<string> omittedModuleNames = GetModuleNamesOmittedFromInitialization();
            if (omittedModuleNames != null)
            {
                // Mark installed installable modules
                foreach (var installableModule in Modules.Where(x => (x.Module != null) && x.Module.IsInstallable))
                {
                    installableModule.Module.IsInstalled = !omittedModuleNames.Contains(installableModule.Name);
                }
            }
        }


        /// <summary>
        /// Returns installable module names omitted from initialization or null if all modules are omitted.
        /// </summary>
        /// <returns>Installable module names omitted from initialization or null if all modules are omitted..</returns>
        private static ISet<string> GetModuleNamesOmittedFromInitialization()
        {
            try
            {
                // Get set of installable modules which are not installed correctly
                return Service.Resolve<IInstallableModulesInformationService>().GetModuleNamesOmittedFromInitialization();
            }
            catch (Exception ex)
            {
                CoreServices.EventLog.LogEvent("W", "ModuleEntryManager", "GetModuleNamesOmittedFromInitialization", String.Format("Names of omitted installable modules could not be retrieved. No installable modules will be initialized. {0}", ex));

                return null;
            }
        }

        #endregion


        #region "Application life-cycle methods"

        /// <summary>
        /// Pre-initializes modules.
        /// </summary>
        /// <remarks>
        /// PreInit applies to all modules which have <see cref="ModuleEntry.AutoInitialize"/> set. Even to those
        /// which are not installed.
        /// </remarks>
        internal static void PreInit()
        {
            TypeManager.PreInitializeTypes();

            var modules = GetModuleEntriesForAutoPreInitialization();
            foreach (var module in modules)
            {
                module.PreInit();
            }
        }


        /// <summary>
        /// Initializes modules.
        /// </summary>
        /// <remarks>
        /// Init applies to all modules which have <see cref="ModuleEntry.AutoInitialize"/> set and are
        /// either not installable or are installed in the same version as their NuGet package.
        /// </remarks>
        internal static void Init()
        {
            // Set installation state to all installable modules.
            SetInstallableModulesState();

            // Initialize types in all modules (installable modules have their installation state at this point).
            TypeManager.InitializeTypes();

            var modules = GetModuleEntriesForAutoInitialization();
            foreach (var module in modules)
            {
                module.Init();
            }

            DefaultEventLogService.LogBufferedEvents();
        }

        #endregion
    }
}