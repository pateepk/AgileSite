using System;

using CMS.DataEngine;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Defines methods needed to translate one instance of macro rule to the data query which when executed
    /// will return objects fulfilling macro rule condition.
    /// </summary>
    internal interface IMacroRuleInstanceToDataQueryTranslator
    {
        /// <summary>
        /// Translates macro rule with its parameters to data query. Created data query returns Contacts fulfilling 
        /// macro rule. Exception is thrown if rule cannot be translated into data query.
        /// </summary>
        /// <param name="macroRuleInstance">Macro rule instance to be translated</param>
        /// <returns>DataQuery representing macro rule</returns>
        ObjectQuery<ContactInfo> Translate(MacroRuleInstance macroRuleInstance);


        /// <summary>
        /// Checks whether macro rule with the passed code name can be translated to data query. If this method returns false, 
        /// calling <see cref="Translate"/> method with the same rule name will result into an exception being thrown.
        /// </summary>
        /// <param name="ruleName">Code name of the rule to be translated</param>
        /// <returns>True if it is possible to translate rule with the passed code name to data query; false otherwise</returns>
        bool CanBeTranslated(string ruleName);
    }
}