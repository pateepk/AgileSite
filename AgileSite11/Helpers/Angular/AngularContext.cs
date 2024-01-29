using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CMS.Base;

namespace CMS.Helpers
{
    /// <summary>
    /// Class that manages registering angular modules for request.
    /// </summary>
    internal class AngularContext : AbstractContext<AngularContext>
    {
        #region "Variables"

        private IDictionary<string, object> mRegisteredModules;

        #endregion


        #region "Properties"

        /// <summary>
        /// Dictionary containing all registered angular modules for current request.
        /// Module id is stored as a key.
        /// Value represents all the parameters of the module.
        /// 
        /// Returns null if no module is registered.
        /// </summary>
        public static IDictionary<string, object> RegisteredModules
        {
            get
            {
                return Current.mRegisteredModules;
            }
            private set
            {
                Current.mRegisteredModules = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Renders JavaScript code to start a client-side angular module.
        /// </summary>
        /// <param name="moduleID">The full name of the client-side module to register.</param>
        /// <param name="parameters">An object that contains the named parameters to set for the module.</param>
        public static void RegisterAngularModule(string moduleID, object parameters = null)
        {
            if (string.IsNullOrEmpty(moduleID))
            {
                throw new ArgumentException("[AngularContext.RegisterAngularModule]: Module ID has to be defined.", "moduleID");
            }

            if (RegisteredModules == null)
            {
                RegisteredModules = new Dictionary<string, object>();
            }

            if (RegisteredModules.ContainsKey(moduleID))
            {
                throw new ArgumentException("[AngularContext.RegisterAngularModule]: Module with the same ID has already been registered.", "moduleID");
            }

            RegisteredModules.Add(moduleID, parameters);
        }

        #endregion
    }
}
