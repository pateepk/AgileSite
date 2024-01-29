using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using CMS.Core;

namespace CMS.Base
{
    /// <summary>
    /// Helper methods for creating extension objects from specific type
    /// </summary>
    public static class Extension
    {
        /// <summary>
        /// Object for locking the context. We lock with a single global lock for all extensions to avoid deadlocks when chaining through multiple generic classes.
        /// </summary>
        internal static object LockObject = new object();


        /// <summary>
        /// Creates a generic extension from the given type
        /// </summary>
        public static IGenericExtension CreateFromType(Type type)
        {
            Type extensionType = typeof(GenericExtension<>).MakeGenericType(type);
            IGenericExtension ext = (IGenericExtension)Activator.CreateInstance(extensionType);

            return ext;
        }


        /// <summary>
        /// Creates a generic property from the given type
        /// </summary>
        public static IGenericProperty CreatePropertyFromType(Type type)
        {
            Type extensionType = typeof(GenericProperty<>).MakeGenericType(type);
            IGenericProperty ext = (IGenericProperty)Activator.CreateInstance(extensionType);

            return ext;
        }
    }


    /// <summary>
    /// Storage for static extensions
    /// </summary>
    public class Extension<TExtension>
    {
        #region "Variables"

        // List of types that already registered their extension attributes
        private static readonly SafeDictionary<Type, bool> mRegisteredAttributes = new SafeDictionary<Type, bool>();

        // List of extension properties for types
        private static readonly SafeDictionary<Type, List<Lazy<TExtension>>> mTypeExtensions = new SafeDictionary<Type, List<Lazy<TExtension>>>();

        // List of extensions collected for specific type
        private static readonly SafeDictionary<Type, ReadOnlyCollection<Lazy<TExtension>>> mExtensionsCollectedForType = new SafeDictionary<Type, ReadOnlyCollection<Lazy<TExtension>>>();

        // List of all the extensions for all the types
        private static List<Lazy<TExtension>> mExtensions;

        // List of extension properties for types [Type -> [PropertyName -> Extension]]
        private static readonly TwoLevelDictionary<Type, string, GenericExtension<TExtension>> mTypeProperties = new TwoLevelDictionary<Type, string, GenericExtension<TExtension>>(false);

        // List of property results for specific type
        private static readonly TwoLevelDictionary<Type, string, GenericExtension<TExtension>> mPropertiesForType = new TwoLevelDictionary<Type, string, GenericExtension<TExtension>>(false);

        // List of static properties collected for specific type
        private static readonly TwoLevelDictionary<Type, string, GenericExtension<TExtension>> mPropertiesCollectedForType = new TwoLevelDictionary<Type, string, GenericExtension<TExtension>>();

        // List of extension static properties for types [Type -> [PropertyName -> Extension]]
        private static readonly TwoLevelDictionary<Type, string, GenericProperty<TExtension>> mTypeStaticProperties = new TwoLevelDictionary<Type, string, GenericProperty<TExtension>>(false);

        // List of property results for specific type
        private static readonly TwoLevelDictionary<Type, string, GenericProperty<TExtension>> mStaticPropertiesForType = new TwoLevelDictionary<Type, string, GenericProperty<TExtension>>(false);

        // List of static properties collected for specific type
        private static readonly TwoLevelDictionary<Type, string, GenericProperty<TExtension>> mStaticPropertiesCollectedForType = new TwoLevelDictionary<Type, string, GenericProperty<TExtension>>();

        #endregion


        #region "Properties"

        /// <summary>
        /// Adds the property to the given type
        /// </summary>
        /// <param name="type">Type to which register the property</param>
        /// <param name="propertyName">Property name</param>
        public static GenericExtension<TExtension> AddAsProperty(Type type, string propertyName)
        {
            return AddAsProperty(type, propertyName, null);
        }


        /// <summary>
        /// Adds the property to the given type
        /// </summary>
        /// <param name="type">Type to which register the property</param>
        /// <param name="propertyName">Property name</param>
        /// <param name="ext">Extension object</param>
        /// <exception cref="ArgumentException">Thrown when <see cref="IExtensible"/> can not be assigned to the type of <paramref name="type"/> parameter</exception>
        internal static GenericExtension<TExtension> AddAsProperty(Type type, string propertyName, GenericExtension<TExtension> ext)
        {
            if (!typeof(IExtensible).IsAssignableFrom(type))
            {
                throw new ArgumentException("Type '" + type.Name + "' does not implement interface IExtensible and cannot be extended with properties.");
            }

            var prop = mTypeProperties[type, propertyName];
            if (prop == null)
            {
                lock (Extension.LockObject)
                {
                    prop = mTypeProperties[type, propertyName];
                    if (prop == null)
                    {
                        prop = ext ?? new GenericExtension<TExtension>();

                        prop.Name = propertyName;
                        mTypeProperties[type, propertyName] = prop;

                        // Clear the cache of collected extensions
                        mPropertiesForType.Clear();

                        // Register the base types
                        var baseExtensions = GetBaseExtensions(prop);
                        if (baseExtensions != null)
                        {
                            foreach (var baseExtension in baseExtensions)
                            {
                                baseExtension.RegisterAsPropertyTo(type, propertyName);
                            }
                        }
                    }
                }
            }

            return prop;
        }


        /// <summary>
        /// Gets all extensions for the given object
        /// </summary>
        /// <param name="obj">Object to process</param>
        /// <param name="propertyName">Property name</param>
        public static GenericExtension<TExtension> GetPropertyForObject(object obj, string propertyName)
        {
            if (obj == null)
            {
                return null;
            }

            Type type = obj.GetType();

            return GetPropertyForType(type, propertyName);
        }


        /// <summary>
        /// Gets all extensions for the given object
        /// </summary>
        /// <param name="type">Type to search</param>
        /// <param name="propertyName">Property name</param>
        public static GenericExtension<TExtension> GetPropertyForType(Type type, string propertyName)
        {
            if (type == null)
            {
                return null;
            }


            var prop = mPropertiesForType[type, propertyName];
            if (prop == null)
            {
                lock (Extension.LockObject)
                {
                    prop = mPropertiesForType[type, propertyName];
                    if (prop == null)
                    {
                        prop = FindPropertyForType(type, propertyName);
                        mPropertiesForType[type, propertyName] = prop;
                    }
                }
            }

            return prop;
        }


        /// <summary>
        /// Adds the extension for the given type to the list of extensions
        /// </summary>
        /// <param name="properties">List of extensions</param>
        /// <param name="type">Type to add</param>
        private static void AddPropertiesForType(ref StringSafeDictionary<GenericExtension<TExtension>> properties, Type type)
        {
            EnsureRegisteredAttributes(type);

            // Get current extension
            var props = mTypeProperties[type];
            if (props != null)
            {
                // Ensure list for extensions
                if (properties == null)
                {
                    properties = new StringSafeDictionary<GenericExtension<TExtension>>();
                }

                // Add all found properties to the result
                foreach (var prop in props.TypedValues)
                {
                    properties[prop.Name] = prop;
                }
            }
        }


        /// <summary>
        /// Gets all static properties for the given object type
        /// </summary>
        /// <param name="type">Type to check</param>
        /// <param name="includeInheritedTypes">Include inherited types</param>
        /// <param name="includeInterfaces">Include interfaces</param>
        private static SafeDictionary<string, GenericExtension<TExtension>> CollectPropertiesForType(Type type, bool includeInheritedTypes = true, bool includeInterfaces = true)
        {
            Type objType = typeof(object);
            StringSafeDictionary<GenericExtension<TExtension>> properties = null;

            var processType = type;
            while (processType != null)
            {
                // Add the extension for type
                AddPropertiesForType(ref properties, processType);

                if ((processType == objType) || !includeInheritedTypes)
                {
                    break;
                }

                // Move to parent type
                processType = processType.BaseType;
            }

            // Add extensions for interfaces
            if (includeInterfaces)
            {
                var interfaces = type.GetInterfaces();

                foreach (var i in interfaces)
                {
                    AddPropertiesForType(ref properties, i);
                }
            }

            return properties;
        }


        /// <summary>
        /// Gets all extensions for the given object type
        /// </summary>
        /// <param name="type">Type to check</param>
        public static SafeDictionary<string, GenericExtension<TExtension>> GetPropertiesForType(Type type)
        {
            lock (Extension.LockObject)
            {
                var result = mPropertiesCollectedForType[type];
                if (result == null)
                {
                    result = CollectPropertiesForType(type) ?? new SafeDictionary<string, GenericExtension<TExtension>>();
                    mPropertiesCollectedForType[type] = result;
                }

                // No extensions found
                if (result.Count == 0)
                {
                    return null;
                }

                return result;
            }
        }


        /// <summary>
        /// Tries to find the property for the given type
        /// </summary>
        /// <param name="type">Type to search</param>
        /// <param name="propertyName">Property name</param>
        /// <param name="includeInheritedTypes">Include inherited types</param>
        /// <param name="includeInterfaces">Include interfaces</param>
        private static GenericExtension<TExtension> FindPropertyForType(Type type, string propertyName, bool includeInheritedTypes = true, bool includeInterfaces = true)
        {
            Type objType = typeof(object);

            var processType = type;
            while (processType != null)
            {
                var prop = GetPropertyForSpecificType(processType, propertyName);
                if (prop != null)
                {
                    return prop;
                }

                if ((processType == objType) || !includeInheritedTypes)
                {
                    break;
                }

                // Move to parent type
                processType = processType.BaseType;
            }

            // Add extensions for interfaces
            if (includeInterfaces)
            {
                var interfaces = type.GetInterfaces();

                foreach (var i in interfaces)
                {
                    var prop = GetPropertyForSpecificType(i, propertyName);
                    if (prop != null)
                    {
                        return prop;
                    }
                }
            }

            return null;
        }


        /// <summary>
        /// Tries to get property for specific type
        /// </summary>
        /// <param name="type">Type to check</param>
        /// <param name="propertyName">Property name</param>
        private static GenericExtension<TExtension> GetPropertyForSpecificType(Type type, string propertyName)
        {
            // Add the extension for type
            EnsureRegisteredAttributes(type);

            return mTypeProperties[type, propertyName];
        }

        #endregion


        #region "Static properties"

        /// <summary>
        /// Adds the static property to the given type
        /// </summary>
        /// <param name="type">Type to which register the property</param>
        /// <param name="propertyName">Property name</param>
        public static GenericProperty<TExtension> AddAsStaticProperty(Type type, string propertyName)
        {
            return AddAsStaticProperty(type, propertyName, null);
        }


        /// <summary>
        /// Adds the static property to the given type
        /// </summary>
        /// <param name="type">Type to which register the property</param>
        /// <param name="propertyName">Property name</param>
        /// <param name="ext">Extension object</param>
        internal static GenericProperty<TExtension> AddAsStaticProperty(Type type, string propertyName, GenericProperty<TExtension> ext)
        {
            var prop = mTypeStaticProperties[type, propertyName];
            if (prop == null)
            {
                lock (Extension.LockObject)
                {
                    prop = mTypeStaticProperties[type, propertyName];
                    if (prop == null)
                    {
                        prop = ext ?? new GenericProperty<TExtension>(propertyName);
                        mTypeStaticProperties[type, propertyName] = prop;

                        // Clear the cache of collected extensions
                        mStaticPropertiesForType.Clear();

                        // Register the base types
                        var baseExtensions = GetBaseProperties(prop);
                        if (baseExtensions != null)
                        {
                            foreach (var baseExtension in baseExtensions)
                            {
                                baseExtension.RegisterAsStaticPropertyTo(type, propertyName);
                            }
                        }
                    }
                }
            }

            return prop;
        }


        /// <summary>
        /// Gets the base property from the property
        /// </summary>
        /// <param name="ext">Extension</param>
        private static IEnumerable<IGenericProperty> GetBaseProperties(GenericProperty<TExtension> ext)
        {
            List<IGenericProperty> baseExtensions = new List<IGenericProperty>();

            var extType = typeof(TExtension);
            if (extType.IsValueType)
            {
                // Create base extension for object for value types
                var baseExtension = Extension.CreatePropertyFromType(typeof(object));
                baseExtension.ParentProperty = ext;

                baseExtensions.Add(baseExtension);
            }
            else
            {
                // Add base type
                if (!extType.IsValueType)
                {
                    var baseType = extType.BaseType;
                    if (baseType != null)
                    {
                        var baseExtension = Extension.CreatePropertyFromType(baseType);
                        baseExtension.ParentProperty = ext;

                        baseExtensions.Add(baseExtension);
                    }
                }

                // Add interfaces
                var interfaces = extType.GetInterfaces();

                foreach (var i in interfaces)
                {
                    var baseExtension = Extension.CreatePropertyFromType(i);
                    baseExtension.ParentProperty = ext;

                    baseExtensions.Add(baseExtension);
                }
            }

            return baseExtensions;
        }


        /// <summary>
        /// Tries to get property for specific type
        /// </summary>
        /// <param name="type">Type to check</param>
        /// <param name="propertyName">Property name</param>
        public static GenericProperty<TExtension> GetStaticPropertyForType(Type type, string propertyName)
        {
            if (type == null)
            {
                return null;
            }

            lock (Extension.LockObject)
            {
                var prop = mStaticPropertiesForType[type, propertyName];
                if (prop == null)
                {
                    lock (mStaticPropertiesForType)
                    {
                        prop = mStaticPropertiesForType[type, propertyName];
                        if (prop == null)
                        {
                            prop = FindStaticPropertyForType(type, propertyName);
                            mStaticPropertiesForType[type, propertyName] = prop;
                        }
                    }
                }

                return prop;
            }
        }


        /// <summary>
        /// Tries to find the static property for the given type
        /// </summary>
        /// <param name="type">Type to search</param>
        /// <param name="propertyName">Property name</param>
        /// <param name="includeInheritedTypes">Include inherited types</param>
        /// <param name="includeInterfaces">Include interfaces</param>
        private static GenericProperty<TExtension> FindStaticPropertyForType(Type type, string propertyName, bool includeInheritedTypes = true, bool includeInterfaces = true)
        {
            Type objType = typeof(object);

            var processType = type;
            while (processType != null)
            {
                var prop = GetStaticPropertyForSpecificType(processType, propertyName);
                if (prop != null)
                {
                    return prop;
                }

                if ((processType == objType) || !includeInheritedTypes)
                {
                    break;
                }

                // Move to parent type
                processType = processType.BaseType;
            }

            // Add extensions for interfaces
            if (includeInterfaces)
            {
                var interfaces = type.GetInterfaces();

                foreach (var i in interfaces)
                {
                    var prop = GetStaticPropertyForSpecificType(i, propertyName);
                    if (prop != null)
                    {
                        return prop;
                    }
                }
            }

            return null;
        }


        /// <summary>
        /// Tries to get property for specific type
        /// </summary>
        /// <param name="type">Type to check</param>
        /// <param name="propertyName">Property name</param>
        private static GenericProperty<TExtension> GetStaticPropertyForSpecificType(Type type, string propertyName)
        {
            // Add the extension for type
            EnsureRegisteredAttributes(type);

            return mTypeStaticProperties[type, propertyName];
        }


        /// <summary>
        /// Tries to get static properties for specific type
        /// </summary>
        /// <param name="obj">Object to check</param>
        public static SafeDictionary<string, GenericProperty<TExtension>> GetStaticPropertiesForObject(object obj)
        {
            if (obj == null)
            {
                return null;
            }

            var type = obj.GetType();

            return GetStaticPropertiesForType(type);
        }


        /// <summary>
        /// Tries to get static properties for specific type
        /// </summary>
        /// <param name="type">Type to check</param>
        public static SafeDictionary<string, GenericProperty<TExtension>> GetStaticPropertiesForSpecificType(Type type)
        {
            // Add the extension for type
            EnsureRegisteredAttributes(type);

            return mTypeStaticProperties[type];
        }


        /// <summary>
        /// Gets all extensions for the given object type
        /// </summary>
        /// <param name="type">Type to check</param>
        public static SafeDictionary<string, GenericProperty<TExtension>> GetStaticPropertiesForType(Type type)
        {
            lock (Extension.LockObject)
            {
                var result = mStaticPropertiesCollectedForType[type];
                if (result == null)
                {
                    result = CollectStaticPropertiesForType(type) ?? new SafeDictionary<string, GenericProperty<TExtension>>();

                    mStaticPropertiesCollectedForType[type] = result;
                }

                // No extensions found
                if (result.Count == 0)
                {
                    return null;
                }

                return result;
            }
        }


        /// <summary>
        /// Gets all static properties for the given object type
        /// </summary>
        /// <param name="type">Type to check</param>
        /// <param name="includeInheritedTypes">Include inherited types</param>
        /// <param name="includeInterfaces">Include interfaces</param>
        private static SafeDictionary<string, GenericProperty<TExtension>> CollectStaticPropertiesForType(Type type, bool includeInheritedTypes = true, bool includeInterfaces = true)
        {
            Type objType = typeof(object);
            StringSafeDictionary<GenericProperty<TExtension>> properties = null;

            var processType = type;
            while (processType != null)
            {
                // Add the extension for type
                AddStaticPropertiesForType(ref properties, processType);

                if ((processType == objType) || !includeInheritedTypes)
                {
                    break;
                }

                // Move to parent type
                processType = processType.BaseType;
            }

            // Add extensions for interfaces
            if (includeInterfaces)
            {
                var interfaces = type.GetInterfaces();

                foreach (var i in interfaces)
                {
                    AddStaticPropertiesForType(ref properties, i);
                }
            }

            return properties;
        }


        /// <summary>
        /// Adds the extension for the given type to the list of extensions
        /// </summary>
        /// <param name="properties">List of extensions</param>
        /// <param name="type">Type to add</param>
        private static void AddStaticPropertiesForType(ref StringSafeDictionary<GenericProperty<TExtension>> properties, Type type)
        {
            EnsureRegisteredAttributes(type);

            // Get current extension
            var props = mTypeStaticProperties[type];
            if (props != null)
            {
                // Ensure list for extensions
                if (properties == null)
                {
                    properties = new StringSafeDictionary<GenericProperty<TExtension>>();
                }

                // Add all found properties to the result
                foreach (var prop in props.TypedValues)
                {
                    properties[prop.Name] = prop;
                }
            }
        }

        #endregion


        #region "Extensions"

        /// <summary>
        /// Adds the extension to the given type
        /// </summary>
        /// <param name="type">Type to which register the extension</param>
        public static GenericExtension<TExtension> AddTo(Type type)
        {
            var ext = new GenericExtension<TExtension>();

            return AddTo(type, ext);
        }


        /// <summary>
        /// Adds the extension to the given type
        /// </summary>
        /// <param name="type">Type to which register the extension</param>
        /// <param name="ext">Extension to register</param>
        internal static GenericExtension<TExtension> AddTo(Type type, GenericExtension<TExtension> ext)
        {
            lock (Extension.LockObject)
            {
                var list = mTypeExtensions[type];
                if (list == null)
                {
                    list = new List<Lazy<TExtension>>();
                    mTypeExtensions[type] = list;
                }

                list.Add(ext.LazyInstance);

                // Clear the cache of collected extensions
                mExtensionsCollectedForType.Clear();
                mExtensions = null;

                // Register the base type
                var baseExtensions = GetBaseExtensions(ext);
                if (baseExtensions != null)
                {
                    foreach (var baseExtension in baseExtensions)
                    {
                        baseExtension.RegisterAsExtensionTo(type);
                    }
                }

                return ext;
            }
        }


        /// <summary>
        /// Gets the base property from the property
        /// </summary>
        /// <param name="ext">Extension</param>
        private static IEnumerable<IGenericExtension> GetBaseExtensions(GenericExtension<TExtension> ext)
        {
            List<IGenericExtension> baseExtensions = new List<IGenericExtension>();

            var extType = typeof(TExtension);
            if (extType.IsValueType)
            {
                // Create base extension for object for value types
                var baseExtension = Extension.CreateFromType(typeof(object));
                baseExtension.ParentExtension = ext;

                baseExtensions.Add(baseExtension);
            }
            else
            {
                // Add base type
                if (!extType.IsValueType)
                {
                    var baseType = extType.BaseType;
                    if (baseType != null)
                    {
                        var baseExtension = Extension.CreateFromType(baseType);
                        baseExtension.ParentExtension = ext;

                        baseExtensions.Add(baseExtension);
                    }
                }

                // Add interfaces
                var interfaces = extType.GetInterfaces();

                foreach (var i in interfaces)
                {
                    var baseExtension = Extension.CreateFromType(i);
                    baseExtension.ParentExtension = ext;

                    baseExtensions.Add(baseExtension);
                }
            }

            return baseExtensions;
        }


        /// <summary>
        /// Gets all extensions for the given object type
        /// </summary>
        public static ReadOnlyCollection<Lazy<TExtension>> GetExtensionsForType<ObjectType>()
        {
            return GetExtensionsForType(typeof(ObjectType));
        }


        /// <summary>
        /// Gets all extensions for the given object type
        /// </summary>
        /// <param name="type">Type to check</param>
        public static ReadOnlyCollection<Lazy<TExtension>> GetExtensionsForType(Type type)
        {
            lock (Extension.LockObject)
            {
                var result = mExtensionsCollectedForType[type];
                if (result == null)
                {
                    result = CollectExtensionsForType(type) ?? new List<Lazy<TExtension>>().AsReadOnly();
                    mExtensionsCollectedForType[type] = result;
                }

                // No extensions found
                if (result.Count == 0)
                {
                    return null;
                }

                return result;
            }
        }


        /// <summary>
        /// Gets all extensions for the given object type
        /// </summary>
        /// <param name="type">Type to check</param>
        /// <param name="includeInheritedTypes">Include inherited types</param>
        /// <param name="includeInterfaces">Include interfaces</param>
        private static ReadOnlyCollection<Lazy<TExtension>> CollectExtensionsForType(Type type, bool includeInheritedTypes = true, bool includeInterfaces = true)
        {
            Type objType = typeof(object);
            List<Lazy<TExtension>> extensions = null;

            var processType = type;
            while (processType != null)
            {
                // Add the extension for type
                AddExtensionsForType(ref extensions, processType);

                if ((processType == objType) || !includeInheritedTypes)
                {
                    break;
                }

                // Move to parent type
                processType = processType.BaseType;
            }

            // Add extensions for interfaces
            if (includeInterfaces)
            {
                var interfaces = type.GetInterfaces();

                foreach (var i in interfaces)
                {
                    AddExtensionsForType(ref extensions, i);
                }
            }

            return extensions != null ? extensions.AsReadOnly() : null;
        }


        /// <summary>
        /// Adds the extension for the given type to the list of extensions
        /// </summary>
        /// <param name="extensions">List of extensions</param>
        /// <param name="type">Type to add</param>
        private static void AddExtensionsForType(ref List<Lazy<TExtension>> extensions, Type type)
        {
            EnsureRegisteredAttributes(type);

            // Get current extension
            var ext = mTypeExtensions[type];
            if (ext != null)
            {
                // Ensure list for extensions
                if (extensions == null)
                {
                    extensions = new List<Lazy<TExtension>>();
                }

                extensions.AddRange(ext);
            }
        }

        #endregion


        #region "General methods"

        /// <summary>
        /// Static constructor
        /// </summary>
        static Extension()
        {
            TypeManager.RegisterGenericType(typeof(Extension<TExtension>));
        }
        

        /// <summary>
        /// Returns the merged list of all the extensions for all the types.
        /// </summary>
        public static IEnumerable<Lazy<TExtension>> GetExtensions()
        {
            lock (Extension.LockObject)
            {
                if (mExtensions == null)
                {
                    mExtensions = new List<Lazy<TExtension>>();

                    foreach (var extensions in mTypeExtensions.TypedValues)
                    {
                        mExtensions.AddRange(extensions);
                    }
                }
            }

            return mExtensions.ToArray();
        }


        /// <summary>
        /// Clears the registered extensions
        /// </summary>
        /// <remarks>
        /// This method is intended for tests only. Clearing the extensions while other threads
        /// collect the extensions may give unexpected results.
        /// </remarks>
        internal static void ClearExtensions()
        {
            lock (Extension.LockObject)
            {
                mRegisteredAttributes.Clear();

                mTypeProperties.Clear();
                mPropertiesForType.Clear();

                mTypeStaticProperties.Clear();
                mStaticPropertiesForType.Clear();

                mTypeExtensions.Clear();
                mExtensionsCollectedForType.Clear();
                mExtensions = null;
            }
        }


        /// <summary>
        /// Ensures that the attributes for the given type are registered within extensions
        /// </summary>
        /// <param name="type">Type to process</param>
        private static void EnsureRegisteredAttributes(Type type)
        {
            // Ensure attribute registration
            if (!mRegisteredAttributes[type])
            {
                lock (Extension.LockObject)
                {
                    if (!mRegisteredAttributes[type])
                    {
                        var attributes = type.GetCustomAttributes(typeof(ExtensionAttribute), false);

                        foreach (ExtensionAttribute attr in attributes)
                        {
                            attr.RegisterTo(type);
                        }

                        mRegisteredAttributes[type] = true;
                    }
                }
            }
        }

        #endregion
    }
}
