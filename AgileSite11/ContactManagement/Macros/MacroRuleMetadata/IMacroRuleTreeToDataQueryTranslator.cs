using System;

using CMS.DataEngine;
using CMS.MacroEngine;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Defines methods for translating whole tree of macro rule instances into data query. Produced data query will return
    /// objects which will pass the macro.
    /// </summary>
    internal interface IMacroRuleTreeToDataQueryTranslator
    {
        /// <summary>
        /// Translates macro rule tree into a data query. Macro rule instances are translated into individual data queries
        /// and combined together using UNION and INTERSECT statements into single data query. A transformation function 
        /// specified in <paramref name="transformation"/> is applied to every individual data query converted from a macro 
        /// rule instance.  Produced data queries will return objects matching the condition of the macro rule tree. 
        /// Throws exception if any of the macro rules cannot be translated.
        /// </summary>
        /// <param name="macroRuleTreeRoot">Root element of the macro rule tree</param>
        /// <param name="transformation">
        /// Transformation function which will be applied to every data query part. Use this function to further
        /// restrict results by applying Where, to specify column which should be returned, etc.
        /// </param>
        /// <returns>DataQuery representing macro rule tree</returns>
        ObjectQuery<ContactInfo> TranslateWithTransformation(MacroRuleTree macroRuleTreeRoot, Func<ObjectQuery<ContactInfo>, ObjectQuery<ContactInfo>> transformation);


        /// <summary>
        /// Checks if macro rule tree can be translated into data query. Tree can be translated if all of its rules can be translated.
        /// </summary>
        /// <param name="macroRuleTreeRoot">Macro rule tree</param>
        /// <returns>True if tree can be translated into data query; false otherwise</returns>
        bool CanBeTranslated(MacroRuleTree macroRuleTreeRoot);
    }
}