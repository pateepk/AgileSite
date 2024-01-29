using System;
using System.Linq;
using System.Text;

using CMS.Helpers;
using CMS.MacroEngine;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Cache for parsed macro conditions.
    /// </summary>
    public static class CachedMacroRuleTrees
    { 
        /// <summary>
        /// Parses serialized macro (for example in a contact group) to a <see cref="MacroRuleTree"/> and saves it to cache for subsequent calls.
        /// If macro rule tree was already parsed, returns it from cache.
        /// </summary>
        /// <param name="macroCondition">Macro condition</param>
        /// <returns>MacroRuleTree that is parsed from <paramref name="macroCondition"/> or taken from cache or null if it cannot be parsed</returns>
        /// <exception cref="ArgumentNullException"><paramref name="macroCondition"/> cannot be null</exception>
        public static MacroRuleTree GetParsedTree(string macroCondition)
        {
            if (macroCondition == null)
            {
                throw new ArgumentNullException("macroCondition");
            }

            return CacheHelper.Cache(() => MacroRuleTreeParser.TryParse(macroCondition), 
                new CacheSettings(20, "CachedMacroRuleTrees", macroCondition));
        }
    }
}
