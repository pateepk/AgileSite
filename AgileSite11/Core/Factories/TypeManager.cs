using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CMS.Core
{
    /// <summary>
    /// Manager for object factories
    /// </summary>
    public class TypeManager
    {
        #region "Variables"

        // Set of assemblies, that were already pre-initialized
        private static readonly HashSet<Assembly> mPreInitializedAssemblies = new HashSet<Assembly>();

        // Set of assemblies, that were already initialized
        private static readonly HashSet<Assembly> mInitializedAssemblies = new HashSet<Assembly>();

        // A set of assemblies that were compiled after the assembly discovery had finished.
        private static readonly HashSet<Assembly> mDynamicAssemblies = new HashSet<Assembly>();

        // Object for locking the context
        private static readonly object mLock = new object();
        
        private static readonly HashSet<Type> mRegisteredGenericTypes = new HashSet<Type>();

        private static IoCContainer mIoCContainer;
        private static object mIoCContainerInitializationLock = new object();

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets an inversion of control container which is used by the system to resolve service dependencies.
        /// </summary>
        /// <remarks>
        /// Service implementations are registered into the container during application initialization process.
        /// </remarks>
        /// <seealso cref="RegisterImplementationAttribute"/>
        /// <seealso cref="DefaultCoreServicesImplementations"/>
        internal static IoCContainer IoCContainer
        {
            get
            {
                if (mIoCContainer == null)
                {
                    lock(mIoCContainerInitializationLock)
                    {
                        if (mIoCContainer == null)
                        {
                            mIoCContainer = InitializeIoCContainer();
                        }
                    }
                }

                return mIoCContainer;
            }
        }


        /// <summary>
        /// If true, all types were already initialized.
        /// </summary>
        public static bool Initialized
        {
            get;
            private set;
        }


        /// <summary>
        /// If true, the types were already pre-initialized
        /// </summary>
        public static bool PreInitialized
        {
            get;
            private set;
        }


        private static bool PreInitializing
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Registers an assembly that was compiled after the assembly discovery had finished.
        /// </summary>
        /// <param name="assembly">The assembly to register.</param>
        internal static void RegisterDynamicAssembly(Assembly assembly)
        {
            lock (mLock)
            {
                mDynamicAssemblies.Add(assembly);
            }
        }


        /// <summary>
        /// Initializes an inversion of control container and registers core services into it.
        /// </summary>
        /// <seealso cref="DefaultCoreServicesImplementations"/>
        private static IoCContainer InitializeIoCContainer()
        {
            IoCContainer container = null;
            try
            {
                container = new IoCContainer();

                DefaultCoreServicesImplementations.Register(container);

                return container;
            }
            catch(Exception)
            {
                container?.Dispose();

                throw;
            }
        }

        #endregion


        #region "Type pre-initialization"

        /// <summary>
        /// PreInitializes all types with IPreInitAttribute attributes
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when a loop occurs in type pre-initialization.</exception>
        internal static void PreInitializeTypes()
        {
            if (!PreInitialized)
            {
                lock (mLock)
                {
                    if (!PreInitialized)
                    {
                        if (PreInitializing)
                        {
                            throw new InvalidOperationException("A loop occurred in type manager pre-initialization. This is likely a result of improper implementation of some attribute inheriting '" + typeof(IPreInitAttribute) +"'.");
                        }

                        PreInitializing = true;

                        foreach (var assembly in AssemblyDiscoveryHelper.GetAssemblies(discoverableOnly: true).Concat(mDynamicAssemblies))
                        {
                            PreInitializeTypes(assembly);
                        }
                                                
                        PreInitialized = true;
                    }
                }
            }
        }


        /// <summary>
        /// Pre-initializes types in the given assembly
        /// </summary>
        /// <param name="assembly">Assembly to Pre-initialize</param>
        internal static void PreInitializeTypes(Assembly assembly)
        {
            lock (mLock)
            {
                // Check if already initialized
                if (mPreInitializedAssemblies.Contains(assembly))
                {
                    return;
                }

                // Process all attributes - implementations are registered prior to execution of IPreInitAttribute.PreInit
                foreach (var registerImplementation in assembly.GetCustomAttributes<RegisterImplementationAttribute>())
                {
                    IoCContainer.RegisterImplementation(registerImplementation);
                }

                var attributes = assembly.GetCustomAttributes(typeof(IPreInitAttribute), false);
                foreach (IPreInitAttribute attribute in attributes)
                {
                    try
                    {
                        attribute.PreInit();
                    }
                    catch (Exception ex)
                    {
                        CoreServices.EventLog.LogException("TypeManager", "PREINIT", ex);
                    }
                }

                mPreInitializedAssemblies.Add(assembly);
            }
        }

        #endregion


        #region "Type initialization"

        /// <summary>
        /// Initializes all types with IInitAttribute attributes.
        /// Assemblies containing installable module which is not installed are omitted.
        /// </summary>
        internal static void InitializeTypes()
        {
            if (!Initialized)
            {
                lock (mLock)
                {
                    if (!Initialized)
                    {
                        if (!PreInitialized)
                        {
                            throw new InvalidOperationException("InitializeTypes cannot be called prior to application pre-initialization.");
                        }
                        
                        var omittedAssemblies = GetAssembliesOmittedFromTypeInitialization();

                        // Process all assemblies
                        foreach (var assembly in AssemblyDiscoveryHelper.GetAssemblies(discoverableOnly: true).Concat(mDynamicAssemblies))
                        {
                            // Skip assemblies which contain installable module which is not installed
                            if (omittedAssemblies.Contains(assembly))
                            {
                                continue;
                            }

                            InitializeTypes(assembly);
                        }

                        Initialized = true;
                    }
                }
            }
        }


        /// <summary>
        /// Initializes types in the given assembly
        /// </summary>
        /// <param name="assembly">Assembly to initialize</param>
        internal static void InitializeTypes(Assembly assembly)
        {
            lock (mLock)
            {
                // Check if already initialized
                if (mInitializedAssemblies.Contains(assembly))
                {
                    return;
                }

                var attributes = assembly.GetCustomAttributes(typeof(IInitAttribute), false);
                if (attributes.Length > 0)
                {
                    // Process all attributes
                    foreach (IInitAttribute attribute in attributes)
                    {
                        try
                        {
                            attribute.Init();
                        }
                        catch (Exception ex)
                        {
                            CoreServices.EventLog.LogException("TypeManager", "INIT", ex);
                        }
                    }
                }

                mInitializedAssemblies.Add(assembly);
            }
        }


        /// <summary>
        /// Gets assemblies to be omitted from type initialization.
        /// </summary>
        /// <returns>Set of omitted assemblies.</returns>
        /// <remarks>
        /// Assemblies containing installable modules which are not installed in the system (or which installation state 
        /// is not available at the moment) are omitted from type initialization.
        /// </remarks>
        private static ISet<Assembly> GetAssembliesOmittedFromTypeInitialization()
        {
            ISet<Assembly> omittedAssemblies = new HashSet<Assembly>();

            foreach (var module in ModuleEntryManager.Modules)
            {
                var moduleEntry = module.Module;

                // Skip virtual modules
                if (moduleEntry == null)
                {
                    continue;
                }

                // If assembly contains installable module which is not installed, omit it
                if (moduleEntry.IsInstallable && !moduleEntry.IsInstalled)
                {
                    omittedAssemblies.Add(moduleEntry.Assembly);
                }
            }

            return omittedAssemblies;
        }

        #endregion


        #region "Generic types"

        /// <summary>
        /// Registers a generic type.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Registered generic types are used for automatic application state reset within the automated tests.
        /// </para>
        /// <para>
        ///  Use this method in static constructor of a generic type.
        /// </para>
        /// </remarks>
        /// <param name="type">Type to register</param>
        public static void RegisterGenericType(Type type)
        {
            mRegisteredGenericTypes.Add(type);
        }


        /// <summary>
        /// Gets the list of registered generic types.
        /// </summary>
        internal static IEnumerable<Type> GetRegisteredGenericTypes()
        {
            return mRegisteredGenericTypes.ToList();
        }

        #endregion
    }
}
