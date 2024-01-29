using System.Collections.Generic;

namespace CMS.MacroEngine
{
    /// <summary>
    /// Base class for module resolver definitions.
    /// </summary>
    public class ResolverDefinition
    {
        /// <summary>
        /// Registers string values in the resolver (adds them to values which are already registered).
        /// Use this only for resolvers for visual components (the values of the properties will be empty strings).
        /// </summary>
        /// <param name="resolver">Resolver object</param>
        /// <param name="names">Names of the macros - values will be accessible by this names in the resolver</param>
        protected static void RegisterStringValues(MacroResolver resolver, IEnumerable<string> names)
        {
            if ((resolver != null) && (names != null))
            {
                foreach (string name in names)
                {
                    resolver.SetNamedSourceData(name, "", false);
                }
            }
        }
    }
}
