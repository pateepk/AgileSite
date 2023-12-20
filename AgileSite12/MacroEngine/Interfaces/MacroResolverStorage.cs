using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Base;

namespace CMS.MacroEngine
{
    /// <summary>
    /// Global storage of static resolvers for all modules.
    /// Extend this to store your module's resolver to be globally available (in e-mail templates, macro components, etc.)
    /// </summary>
    public class MacroResolverStorage
    {
        #region "Variables"

        /// <summary>
        /// Handler for receiving static resolver via its name.
        /// </summary>
        /// <param name="name">Name of the resolver</param>
        public delegate MacroResolver GetResolverHandler(string name);


        /// <summary>
        /// Handler called when resolver is being retrieved and was not found in the main storage.
        /// </summary>
        private static List<GetResolverHandler> mOnGetResolver;

        #endregion


        #region "Methods"

        /// <summary>
        /// Registers the macro resolver
        /// </summary>
        /// <param name="name">Resolver name</param>
        /// <param name="lambda">Lambda expression to initialize the resolver</param>
        public static void RegisterResolver(string name, Func<MacroResolver> lambda)
        {
            ExtendList<MacroResolverStorage, MacroResolver>.With(name).WithLazyInitialization(lambda);
        }


        /// <summary>
        /// Registers handler(s) called when resolver is being retrieved and was not found in the main storage.
        /// </summary>
        /// <param name="handlers">Handler(s) to be registered</param>
        public static void RegisterGetResolverHandler(params GetResolverHandler[] handlers)
        {
            if (mOnGetResolver == null)
            {
                mOnGetResolver = new List<GetResolverHandler>();
            }
            mOnGetResolver.AddRange(handlers);
        }


        /// <summary>
        /// Returns names of all registered resolvers.
        /// </summary>
        public static List<string> GetRegisteredResolvers()
        {
            var resolvers = typeof(MacroResolverStorage).GetStaticProperties<MacroResolver>();
            if (resolvers != null)
            {
                return resolvers.TypedValues.Select(prop => prop.Name).ToList();
            }
            return new List<string>();
        }


        /// <summary>
        /// Returns resolver of given name. Returns default resolver if the name was not found.
        /// </summary>
        /// <param name="name">Name of the resolver</param>
        public static MacroResolver GetRegisteredResolver(string name)
        {
            if (name == null)
            {
                // Return default resolver
                return MacroResolver.GetInstance();
            }

            var resolvers = typeof(MacroResolverStorage).GetStaticProperties<MacroResolver>();
            if (resolvers != null)
            {
                var resolver = resolvers[name];
                if (resolver != null)
                {
                    return resolver.Value;
                }
            }

            if (mOnGetResolver != null)
            {
                foreach (var getResolver in mOnGetResolver)
                {
                    var resolver = getResolver(name);
                    if (resolver != null)
                    {
                        return resolver;
                    }
                }
            }

            // Return default resolver
            return MacroResolver.GetInstance();
        }

        #endregion
    }
}
