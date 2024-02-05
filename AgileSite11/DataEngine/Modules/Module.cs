using System;

using CMS.Core;
using CMS.Base;

namespace CMS.DataEngine
{
    using CommandFunc = Func<object[], object>;
    using CommandsDictionary = StringSafeDictionary<Func<object[], object>>;
    
    /// <summary>
    /// Represents the standard module.
    /// </summary>
    public abstract class Module : ModuleEntry
    {
        #region "Variables"

        /// <summary>
        /// If true commands were already registered
        /// </summary>
        private bool mCommandsRegistered;

        /// <summary>
        /// Dictionary of the commands [objectType -> CommandFunc]
        /// </summary>
        private readonly CommandsDictionary mCommands = new CommandsDictionary();

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="metadata">Module metadata</param>
        /// <param name="isInstallable">Indicates if module is designed as installable.</param>
        protected Module(ModuleMetadata metadata, bool isInstallable = false)
            : base(metadata, isInstallable)
        {
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="moduleName">Module name</param>
        /// <param name="isInstallable">Indicates if module is designed as installable.</param>
        protected Module(string moduleName, bool isInstallable = false)
            : base(new ModuleMetadata(moduleName), isInstallable)
        {
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Clears resources allocated by module.
        /// </summary>
        /// <remarks>
        /// All provider hashtables are cleared in <see cref="ModuleManager.ClearHashtables"/> method call.
        /// Clear just provider non-related stuff in overridden methods.
        /// </remarks>
        /// <param name="logTasks">If true, web farm tasks are logged</param>
        internal void Clear(bool logTasks)
        {
            ClearHashtables(logTasks);
        }


        /// <summary>
        /// Clears the module hash tables.
        /// </summary>
        /// <param name="logTasks">If true, web farm tasks are logged</param>
        protected virtual void ClearHashtables(bool logTasks)
        {
            // Do nothing by default
        }


        /// <summary>
        /// Checks the license.
        /// </summary>
        /// <param name="domain">Domain name</param>
        /// <param name="feature">Feature</param>
        /// <param name="action">Action</param>
        [Obsolete("Use module specific method if available or change dependency for module you can't access directly.")]
        internal bool CheckLicense(string domain, FeatureEnum feature, ObjectActionEnum action)
        {
            return CheckLicenseInternal(domain, feature, action);
        }


        /// <summary>
        /// Checks the license.
        /// </summary>
        /// <param name="domain">Domain name</param>
        /// <param name="feature">Feature</param>
        /// <param name="action">Action</param>
        [Obsolete("Use module specific method if available or change dependency for module you can't access directly.")]
        protected virtual bool CheckLicenseInternal(string domain, FeatureEnum feature, ObjectActionEnum action)
        {
            // Do nothing by default
            return true;
        }

        #endregion


        #region "Public object type methods"

        /// <summary>
        /// Gets the object created from the given DataRow.
        /// </summary>
        /// <param name="objectType">Object type</param>
        public virtual BaseInfo GetObject(string objectType)
        {
            return null;
        }

        #endregion


        #region "Commands methods"

        /// <summary>
        /// Processes the specified command.
        /// </summary>
        /// <param name="commandName">Command name</param>
        /// <param name="parameters">Command parameters</param>
        public virtual object ProcessCommand(string commandName, object[] parameters)
        {
            EnsureCommands();

            // Try to get registered command
            CommandFunc func = mCommands[commandName];
            if (func != null)
            {
                return func(parameters);
            }

            // Command not found - throw exception
            throw new Exception("Command '" + commandName + "' not found in module '" + ModuleInfo.Name + "'.");
        }


        /// <summary>
        /// Ensures the object types to be registered
        /// </summary>
        private void EnsureCommands()
        {
            if (!mCommandsRegistered)
            {
                // Check if the object types are registered
                lock (this)
                {
                    if (!mCommandsRegistered)
                    {
                        // Register the object types
                        RegisterCommands();

                        mCommandsRegistered = true;
                    }
                }
            }
        }


        /// <summary>
        /// Registers the object type of this module
        /// </summary>
        protected virtual void RegisterCommands()
        {
            // Do nothing by default
        }


        /// <summary>
        /// Registers the given command
        /// </summary>
        /// <param name="name">Command name</param>
        /// <param name="func">Command function</param>
        protected void RegisterCommand(string name, CommandFunc func)
        {
            mCommands[name] = func;
        }


        /// <summary>
        /// Registers the module context
        /// </summary>
        /// <param name="name">Context name</param>
        protected static void RegisterContext<ContextType>(string name = null)
            where ContextType : IContext, new()
        {
            // Ensure automatic name
            if (name == null)
            {
                name = typeof(ContextType).Name;
            }

            // Register the context to the macro root
            Extend<IMacroRoot>.WithStaticProperty<ContextType>(name).AsSingleton();
            Extend<IContext>.WithStaticProperty<ContextType>(name).AsSingleton();
        }

        #endregion
    }
}