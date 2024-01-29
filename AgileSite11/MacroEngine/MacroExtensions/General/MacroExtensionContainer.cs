using System;
using System.Collections.Generic;

using CMS.Base;
using CMS.Core;

namespace CMS.MacroEngine
{
    /// <summary>
    /// Wrapper class to provide extensions to an arbitrary object. 
    /// It is a base class for ExtensionTypeContainer and MacroFieldContainer classes.
    /// </summary>
    public class MacroExtensionContainer<TContainer, TExtension> 
        where TExtension : MacroExtension 
        where TContainer : MacroExtensionContainer<TContainer, TExtension>, new()
    {
        #region "Variables"

        /// <summary>
        /// Collection of the Extensions registered.
        /// </summary>
        private readonly StringSafeDictionary<TExtension> mExtensions = new StringSafeDictionary<TExtension>();

        /// <summary>
        /// Indicates if extensions have been loaded / initialized
        /// </summary>
        private bool mExtensionsInitialized;

        /// <summary>
        /// Extensions by the given type [Type => Extensions]
        /// </summary>
        private static SafeDictionary<Type, List<TExtension>> mExtensionsByType = new SafeDictionary<Type, List<TExtension>>();

        /// <summary>
        /// Extensions by the given type and name [Type => Name => Extensions]
        /// </summary>
        private static TwoLevelDictionary<Type, string, MacroExtension> mExtensionsByTypeAndName = new TwoLevelDictionary<Type, string, MacroExtension>();

        /// <summary>
        /// Defines no extension in the dictionary of cached extensions
        /// </summary>
        private static MacroExtension NO_EXTENSION = new MacroExtension();

        /// <summary>
        /// Object for locking the context
        /// </summary>
        private static object mLockObject = new object();

        private static TContainer mInstance = new TContainer();

        #endregion


        #region "Properties"

        /// <summary>
        /// Container instance
        /// </summary>
        protected static TContainer Instance
        {
            get
            {
                return mInstance ?? (mInstance = new TContainer());
            }
        }


        /// <summary>
        /// Extensions dictionary
        /// </summary>
        protected StringSafeDictionary<TExtension> Extensions
        {
            get
            {
                EnsureExtensions();

                return mExtensions;
            }
        }

        /// <summary>
        /// Returns enumerable of all Extensions.
        /// </summary>
        protected IEnumerable<TExtension> RegisteredExtensions
        {
            get
            {
                var extensions = Extensions;

                return (extensions != null) ? extensions.TypedValues : null;
            }
        }


        /// <summary>
        /// Returns enumerable of all registered Extension names.
        /// </summary>
        protected IEnumerable<string> RegisteredExtensionNames
        {
            get
            {
                var extensions = Extensions;

                return (extensions != null) ? extensions.TypedKeys : null;
            }
        }


        /// <summary>
        /// Extensions by the given type [Type => Extensions]
        /// </summary>
        private static SafeDictionary<Type, List<TExtension>> ExtensionsByType
        {
            get
            {
                return mExtensionsByType ?? (mExtensionsByType = new SafeDictionary<Type, List<TExtension>>());
            }
        }


        /// <summary>
        /// Extensions by the given type and name [Type => Name => Extensions]
        /// </summary>
        private static TwoLevelDictionary<Type, string, MacroExtension> ExtensionsByTypeAndName
        {
            get
            {
                return mExtensionsByTypeAndName ?? (mExtensionsByTypeAndName = new TwoLevelDictionary<Type, string, MacroExtension>());
            }
        }
        
        #endregion
        

        #region "Extensions"

        /// <summary>
        /// Returns list of macro extension registered for specified object. Caches the result in the internal cache.
        /// Returns null if there is no such extension for given object.
        /// </summary>
        /// <param name="obj">Object to check</param>
        protected static IEnumerable<TExtension> GetExtensionsForObject(object obj)
        {
            if (obj == null)
            {
                return null;
            }

            var type = obj.GetType();

            // Try to get cached list
            var result = ExtensionsByType[type];
            if (result == null)
            {
                result = Instance.GetExtensions(type);

                ExtensionsByType[type] = result;
            }

            return result;
        }


        /// <summary>
        /// Gets the extensions for the given type. Does not cache the result.
        /// </summary>
        /// <param name="type">Type</param>
        protected virtual List<TExtension> GetExtensions(Type type)
        {
            // Build a new list of extensions for this type
            var result = new List<TExtension>();

            var extensions = Extension<TContainer>.GetExtensionsForType(type);
            if (extensions != null)
            {
                foreach (var ext in extensions)
                {
                    var item = ext.Value;

                    result.AddRange(item.RegisteredExtensions);
                }
            }

            return result;
        }


        /// <summary>
        /// Returns macro extension object of given name if registered for specified object. Caches the result in the internal cache.
        /// It loops through all extensions of given object type.
        /// Returns null if there is no such extension for given object.
        /// </summary>
        /// <param name="obj">Object to check</param>
        /// <param name="name">Name of the method</param>
        protected static TExtension GetExtensionForObject(object obj, string name)
        {
            if (obj == null)
            {
                return null;
            }

            var type = obj.GetType();

            // Try to get cached list
            var result = ExtensionsByTypeAndName[type, name];
            if (result == null)
            {
                result = Instance.GetExtension(type, name);

                // Cache the value
                ExtensionsByTypeAndName[type, name] = result ?? NO_EXTENSION;
            }
            else if (result == NO_EXTENSION)
            {
                result = null;
            }

            return (TExtension)result;
        }


        /// <summary>
        /// Returns the extension by type and name. Does not cache the result. Does not cache the result.
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="name">Member name</param>
        protected virtual MacroExtension GetExtension(Type type, string name)
        {
            MacroExtension result = null;

            var extensions = Extension<TContainer>.GetExtensionsForType(type);
            if (extensions != null)
            {
                foreach (var ext in extensions)
                {
                    try
                    {
                        var item = ext.Value;

                        var extension = item.GetExtension(name);
                        if (extension != null)
                        {
                            result = extension;
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException(String.Format("An error occured while loading macro extension '{0}' for type '{1}' of name '{2}' (MacroExtensionContainer<{3},{4}>)", 
                                ext, type.FullName, name, typeof (TContainer).FullName, typeof(TExtension).FullName), 
                            ex);
                    }
                }
            }

            return result;
        }


        /// <summary>
        /// Registers a Extension to the container.
        /// </summary>
        /// <param name="Extension">Extension to be registered</param>
        protected void RegisterExtension(TExtension Extension)
        {
            if (string.IsNullOrEmpty(Extension.Name))
            {
                throw new ArgumentException("[ExtensionTypeContainer.RegisterExtension]: Extension has to have a name assigned.");
            }

            mExtensions.Add(Extension.Name, Extension);
        }


        /// <summary>
        /// Returns a Extension of given name (return null if specified Extension does not exist).
        /// </summary>
        /// <param name="name">Extension name</param>
        protected TExtension GetExtension(string name)
        {
            return Extensions[name];
        }


        /// <summary>
        /// Ensures that the extensions for this type are loaded
        /// </summary>
        private void EnsureExtensions()
        {
            if (!mExtensionsInitialized)
            {
                lock (mLockObject)
                {
                    if (!mExtensionsInitialized)
                    {
                        // Register extensions
                        RegisterExtensions();
                        mExtensionsInitialized = true;
                    }
                }
            }

        }
        

        /// <summary>
        /// Registers extensions to the container. Override this to call RegisterExtension from within.
        /// </summary>
        protected virtual void RegisterExtensions()
        {
            // Do nothing by default
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Static constructor
        /// </summary>
        static MacroExtensionContainer()
        {
            TypeManager.RegisterGenericType(typeof(MacroExtensionContainer<TContainer, TExtension>));
        }


        /// <summary>
        /// Checks if the current context user is global admin, returns true if so, otherwise returns false
        /// </summary>
        /// <param name="context">Evaluation context</param>
        protected static bool CheckGlobalAdmin(EvaluationContext context)
        {
            // Skip check if security is not checked
            if (!context.CheckSecurity)
            {
                return true;
            }

            // Check security, allow debug only to global admin
            var user = context.User;
            if ((user == null) || !user.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin))
            {
                MacroDebug.LogSecurityCheckFailure(context.OriginalExpression, context.UserName, context.IdentityName, user?.UserName);
                return false;
            }

            return true;
        }

        #endregion
    }
}