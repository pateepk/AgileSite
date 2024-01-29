using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

using CMS.Core;

namespace CMS.Base
{
    /// <summary>
    /// Helper to provide general access to assemblies and classes.
    /// </summary>
    public class ClassHelper : AbstractHelper<ClassHelper>
    {
        #region "Variables"

        /// <summary>
        /// AppCode constant.
        /// </summary>
        public const string ASSEMBLY_APPCODE = "App_Code";

        // Hash table of loaded assemblies.
        private static readonly StringSafeDictionary<Assembly> assemblies = new StringSafeDictionary<Assembly>();

        // Hash table of registered custom class factories
        private static readonly StringSafeDictionary<IObjectFactory> customClasses = new StringSafeDictionary<IObjectFactory>();

        // List of registered custom class names
        private static readonly List<string> customClassesList = new List<string>();

        // Hash table of classes specified by assembly name and ClassTypeSettings
        private static readonly StringSafeDictionary<List<string>> assemblyClasses = new StringSafeDictionary<List<string>>();

        // Hash table of filtered assemblies specified by assembly filter and ClassTypeSettings
        private static readonly StringSafeDictionary<List<string>> assembliesList = new StringSafeDictionary<List<string>>();

        /// <summary>
        /// Fires when the custom class is required.
        /// </summary>
        public static EventHandler<ClassEventArgs> OnGetCustomClass;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets default assembly for loading classes. Default assembly is used when the assembly given by a parameter is not found.
        /// </summary>
        public static Assembly DefaultAssembly
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Gets the dictionary key for the custom class
        /// </summary>
        /// <param name="assemblyName">Assembly name</param>
        /// <param name="className">Class name</param>
        private static string GetCustomClassKey(string assemblyName, string className)
        {
            if (IsCustomClass(assemblyName))
            {
                assemblyName = "";
            }

            string key = String.Format("{0}|{1}", assemblyName, className);

            return key;
        }


        /// <summary>
        /// Registers the class alias for backward compatibility
        /// </summary>
        /// <param name="assemblyName">Assembly name</param>
        /// <param name="className">Class name</param>
        public static void RegisterAliasFor<ObjectType>(string assemblyName, string className)
            where ObjectType : new()
        {
            RegisterCustomClass<ObjectType>(assemblyName, className);
        }


        /// <summary>
        /// Registers the custom class under the given assembly name and class name. Overrides the default one
        /// </summary>
        /// <param name="assemblyName">Assembly name</param>
        /// <param name="className">Class name</param>
        /// <param name="factory">Object factory to provide the class</param>
        public static void RegisterCustomClass(string assemblyName, string className, IObjectFactory factory)
        {
            // Get class key
            string key = GetCustomClassKey(assemblyName, className);

            // Register custom class
            customClasses[key] = factory;

            // Register custom class name
            customClassesList.Add(className);
        }


        /// <summary>
        /// Registers the custom App_Code class under the class name. Overrides the default one
        /// </summary>
        /// <param name="className">Class name</param>
        public static void RegisterCustomClass<ClassType>(string className)
            where ClassType : class, new()
        {
            var factory = new ObjectFactory<ClassType>();

            RegisterCustomClass(null, className, factory);
        }


        /// <summary>
        /// Registers the custom class under the given assembly name and class name. Overrides the default one
        /// </summary>
        /// <param name="assemblyName">Assembly name</param>
        /// <param name="className">Class name</param>
        public static void RegisterCustomClass<ClassType>(string assemblyName, string className)
            where ClassType : new()
        {
            var factory = new ObjectFactory<ClassType>();

            RegisterCustomClass(assemblyName, className, factory);
        }


        /// <summary>
        /// Loads the given assembly.
        /// </summary>
        /// <param name="assemblyName">Assembly name</param>
        public static Assembly GetAssembly(string assemblyName)
        {
            return HelperObject.GetAssemblyInternal(assemblyName);
        }


        /// <summary>
        /// Gets list of assembly names by given restrictions.
        /// </summary>
        /// <param name="assembliesFilter">File system filter for list assemblies</param>
        /// <param name="settings">Settings used for filter list of classes</param>
        public static List<string> GetAssemblyNames(string assembliesFilter, ClassTypeSettings settings = null)
        {
            return HelperObject.GetAssemblyNamesInternal(assembliesFilter, settings);
        }


        /// <summary>
        /// Gets the new object of given class.
        /// </summary>
        /// <param name="assemblyName">Assembly name</param>
        /// <param name="className">Full class name</param>
        public static Type GetClassType(string assemblyName, string className)
        {
            return HelperObject.GetClassTypeInternal(assemblyName, className);
        }


        /// <summary>
        /// Gets the new object of given class.
        /// </summary>
        /// <param name="assemblyName">Assembly name</param>
        /// <param name="className">Full class name</param>
        public static object GetClass(string assemblyName, string className)
        {
            return HelperObject.GetClassInternal(assemblyName, className);
        }


        /// <summary>
        /// Gets the new object of given class.
        /// </summary>
        /// <typeparam name="ReturnType">Return class type</typeparam>
        /// <param name="assemblyName">Assembly name</param>
        /// <param name="className">Full class name</param>
        public static ReturnType GetClass<ReturnType>(string assemblyName, string className)
        {
            return (ReturnType)GetClass(assemblyName, className);
        }


        /// <summary>
        /// Gets classes for given assembly name and filter settings
        /// </summary>
        /// <param name="assemblyName">Assembly name</param>
        /// <param name="settings">Settings used for filter list of classes</param>
        public static List<string> GetClasses(string assemblyName, ClassTypeSettings settings)
        {
            return HelperObject.GetClassesInternal(assemblyName, settings);
        }


        /// <summary>
        /// Gets the empty object of the given type
        /// </summary>
        /// <param name="objectType">Object type</param>
        public static object GetEmptyObject(Type objectType)
        {
            if (objectType != null)
            {
                // System object types
                if (objectType == typeof(string))
                {
                    return "";
                }
                else if (objectType == typeof(bool))
                {
                    return false;
                }
                else if (objectType == typeof(int))
                {
                    return 0;
                }
                else if (objectType == typeof(double))
                {
                    return 0.0d;
                }
                else if (objectType == typeof(float))
                {
                    return 0.0f;
                }
                else if (objectType == typeof(Guid))
                {
                    return Guid.Empty;
                }
                else if (objectType == typeof(DateTime))
                {
                    return DateTime.MinValue;
                }
                else if (objectType == typeof(byte[]))
                {
                    return new byte[0];
                }
                else
                {
                    try
                    {
                        // Try to make object with a constructor
                        return Activator.CreateInstance(objectType);
                    }
                    catch
                    {
                        return null;
                    }
                }
            }

            return null;
        }


        /// <summary>
        /// Gets the list of registered custom class names.
        /// </summary>
        public static List<string> GetCustomClassNames()
        {
            return HelperObject.GetCustomClassNamesInternal();
        }


        /// <summary>
        /// Indicates if given class is loaded.
        /// </summary>
        /// <param name="assemblyName">Assembly name</param>
        /// <param name="className">Full class name</param>
        public static bool ClassLoaded(string assemblyName, string className)
        {
            return HelperObject.ClassLoadedInternal(assemblyName, className);
        }


        /// <summary>
        /// Indicates if given assembly name represents custom class
        /// </summary>
        /// <param name="assemblyName">Assembly name</param>
        public static bool IsCustomClass(string assemblyName)
        {
            return (string.Equals(ASSEMBLY_APPCODE, assemblyName, StringComparison.OrdinalIgnoreCase));
        }


        /// <summary>
        /// Checks if given type is allowed based on base class name.
        /// </summary>
        /// <param name="type">Type of class</param>
        /// <param name="settings">Settings used for filter list of classes</param>
        public static bool IsAllowed(Type type, ClassTypeSettings settings)
        {
            foreach (Type baseType in settings.BaseTypes)
            {
                if (type.IsClass && (settings.CheckInterfaces(type) || ((baseType != null) && type.IsSubclassOf(baseType))))
                {
                    return true;
                }
            }

            return false;
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Indicates if given class is loaded.
        /// </summary>
        /// <param name="assemblyName">Assembly name</param>
        /// <param name="className">Full class name</param>
        protected virtual bool ClassLoadedInternal(string assemblyName, string className)
        {
            try
            {
                return (GetClassTypeInternal(assemblyName, className) != null);
            }
            catch (ClassNotLoadedException)
            {
                // Class is not loaded
            }

            return false;
        }


        /// <summary>
        /// Gets the list of registered custom class names.
        /// </summary>
        protected virtual List<string> GetCustomClassNamesInternal()
        {
            return customClassesList;
        }


        /// <summary>
        /// Loads the given assembly.
        /// </summary>
        /// <param name="assemblyName">Assembly name</param>
        protected virtual Assembly GetAssemblyInternal(string assemblyName)
        {
            try
            {
                // Try to get from cache
                var result = assemblies[assemblyName];
                if (result == null)
                {
                    // Load the assembly
                    result = Assembly.Load(assemblyName);

                    assemblies[assemblyName] = result;
                }

                return result;
            }
            catch (FileNotFoundException)
            {
                if (DefaultAssembly == null)
                {
                    throw;
                }
                else
                {
                    // Load from default assembly
                    return DefaultAssembly;
                }
            }
        }


        /// <summary>
        /// Gets list of assembly names by given restrictions. Returns assemblies that contains at least one class fulfilling restrictions.
        /// </summary>
        /// <param name="assembliesFilter">String to filter list of assembly names</param>
        /// <param name="settings">Settings used for filter classes</param>
        protected virtual List<string> GetAssemblyNamesInternal(string assembliesFilter, ClassTypeSettings settings)
        {
            bool filterItems = true;

            if (settings == null)
            {
                filterItems = false;

                settings = new ClassTypeSettings
                {
                    ShowInterfaces = true,
                    ShowEnumerations = true
                };
            }

            string cacheKey = settings.GetCacheKey(assembliesFilter);

            if (assembliesList.ContainsKey(cacheKey))
            {
                // Get assemblies list from cache
                return assembliesList[cacheKey];
            }

            // Get loaded assemblies
            var asms = AssemblyDiscoveryHelper.GetAssemblies(discoverableOnly: false);

            if (!String.IsNullOrEmpty(assembliesFilter))
            {
                asms = asms.Where(t => t.ManifestModule.Name.Contains(assembliesFilter));
            }

            var asmList = new List<string>();

            foreach (var assembly in asms)
            {
                string name = assembly.ManifestModule.Name.Replace(".dll", "");
                if (filterItems)
                {
                    var classes = GetClasses(name, settings);
                    if ((classes == null) || (classes.Count == 0))
                    {
                        continue;
                    }
                }

                asmList.Add(name);
            }

            // Store to cache
            assembliesList[settings.GetCacheKey(assembliesFilter)] = asmList;

            return asmList;
        }


        /// <summary>
        /// Gets the type of given class.
        /// </summary>
        /// <param name="assemblyName">Assembly name</param>
        /// <param name="className">Full class name</param>
        protected virtual Type GetClassTypeInternal(string assemblyName, string className)
        {
            object obj = null;

            // Try to get custom class
            if (GetCustomClass(assemblyName, className, ref obj))
            {
                if (obj != null)
                {
                    return obj.GetType();
                }
                else
                {
                    return null;
                }
            }

            // Get from regular assembly
            Assembly asm = GetAssembly(assemblyName);
            if (asm != null)
            {
                Type type = asm.GetType(className);
                return type;
            }

            return null;
        }


        /// <summary>
        /// Gets the new object of given class.
        /// </summary>
        /// <param name="assemblyName">Assembly name</param>
        /// <param name="className">Full class name</param>
        protected virtual object GetClassInternal(string assemblyName, string className)
        {
            object obj = null;

            // Try to get custom class
            if (GetCustomClass(assemblyName, className, ref obj))
            {
                return obj;
            }

            // Get from regular assembly
            var asm = GetAssembly(assemblyName);
            if (asm != null)
            {
                obj = asm.CreateInstance(className);
                return obj;
            }

            return null;
        }


        /// <summary>
        /// Gets classes for given assembly name and class restrictions settings.
        /// </summary>
        /// <param name="assemblyName">Assembly name</param>
        /// <param name="settings">Settings used for filter list of classes</param>
        protected virtual List<string> GetClassesInternal(string assemblyName, ClassTypeSettings settings)
        {
            string key = settings.GetCacheKey(assemblyName);

            if (assemblyClasses.ContainsKey(key))
            {
                // Get from hash table if key exists
                return assemblyClasses[key];
            }

            List<string> classes = null;

            var classTypes = new List<Tuple<string, Type>>();

            try
            {
                // App_Code classes
                if (assemblyName == ASSEMBLY_APPCODE)
                {
                    foreach (string className in GetCustomClassNames())
                    {
                        // Get all custom class types
                        try
                        {
                            Type type = GetClassType(assemblyName, className);

                            if (type != null)
                            {
                                classTypes.Add(new Tuple<string, Type>(className, type));
                            }
                        }
                        catch (ClassNotLoadedException)
                        {
                            // Do nothing, only skip failed custom class
                        }
                    }
                }
                else
                {
                    // Get assembly
                    Assembly assembly = GetAssembly(assemblyName);
                    if (assembly != null)
                    {
                        // Get list of classes from selected assembly
                        Type[] assemblyClassTypes;
                        try
                        {
                            assemblyClassTypes = assembly.GetTypes();
                        }
                        catch (ReflectionTypeLoadException exception)
                        {
                            assemblyClassTypes = exception.Types;
                        }
                        // The array might contain null values when there were exceptions loading the type information.
                        foreach (Type classType in assemblyClassTypes.Where(x => x != null))
                        {
                            classTypes.Add(new Tuple<string, Type>(classType.Namespace + "." + classType.Name, classType));
                        }
                    }
                }

                // Filter classes based on settings
                classes = classTypes.Where(t => GetClassCondition(t.Item2, settings)).Select(t => t.Item1).Where(s => !string.IsNullOrEmpty(s)).ToList();
            }
            catch
            {
                // Suppress error when not able to access the classes
            }

            // Sort class names
            if (classes != null)
            {
                classes.Sort();
            }

            assemblyClasses[key] = classes;

            return classes;
        }


        /// <summary>
        /// Attempts to get the custom class
        /// </summary>
        /// <param name="assemblyName">Assembly name</param>
        /// <param name="className">Class name</param>
        /// <param name="obj">Returns the class object</param>
        protected virtual bool GetCustomClass(string assemblyName, string className, ref object obj)
        {
            // First search explicitly registered classes
            var key = GetCustomClassKey(assemblyName, className);
            var factory = customClasses[key];
            if (factory != null)
            {
                obj = factory.CreateNewObject();

                return true;
            }

            // If the target is App_Code, get the class with handler
            if (IsCustomClass(assemblyName))
            {
                if (OnGetCustomClass != null)
                {
                    // Prepare the event arguments
                    var e = new ClassEventArgs
                    {
                        ClassName = className,
                        AssemblyName = assemblyName,
                    };

                    // Fire the event
                    OnGetCustomClass(null, e);

                    obj = e.Object;

                    return true;
                }

                throw new ClassNotLoadedException("Custom class '" + className + "' was not found. Please make sure you register it properly using RegisterCustomClass attribute, or that you provide it through OnGetCustomClass event.");
            }

            return false;
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Returns visibility condition for given class type.
        /// </summary>
        /// <param name="type">Class type</param>
        /// <param name="settings">Settings used for filter list of classes</param>
        private bool GetClassCondition(Type type, ClassTypeSettings settings)
        {
            return
                !type.IsGenericType &&
                !type.IsDefined(typeof(CompilerGeneratedAttribute), false) &&
                (type.DeclaringType == null || !type.DeclaringType.IsDefined(typeof(CompilerGeneratedAttribute), false)) &&
                (!type.IsAbstract || type.IsSealed || (type.IsInterface && settings.ShowInterfaces)) &&
                // Get only public classes (because of medium trust level)
                type.IsPublic &&
                // Interface
                (!type.IsInterface || settings.ShowInterfaces) &&
                // Enumeration
                (!type.IsEnum || settings.ShowEnumerations) &&
                // Classes
                (!type.IsClass || settings.ShowClasses) &&
                // Matches base class name
                (String.IsNullOrEmpty(settings.BaseClassNames) || (IsAllowed(type, settings))) &&
                // Checks whether class can be created automatically by system
                (!settings.CheckAutoCreation || TypeCanBeCreatedAutomatically(type));

        }


        /// <summary>
        /// Returns <c>true</c> if type is creatable automatically by <see cref="Activator.CreateInstance(Type)"/> method.
        /// </summary>
        private static bool TypeCanBeCreatedAutomatically(Type type)
        {
            try
            {
                return (type.GetConstructor(Type.EmptyTypes) != null);
            }
            catch
            {
                // possible reflection error is ignored and type is considered as not-creatable automatically by system
            }

            return false;
        }

        #endregion
    }
}