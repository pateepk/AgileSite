using CMS.DataEngine;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Defines methods needed to translate macro rule instance of one particular macro rule type to data query.
    /// </summary>
    public interface IMacroRuleInstanceTranslator
    {
        /// <summary>
        /// Translates one rule to object query returning contacts. Is used to speed up the process of recalculating contact groups.
        /// </summary>
        /// <param name="macroRuleInstance">Instance of the macro rule representing one line in the condition builder.</param>
        /// <returns>Object query for contacts that fit in this rule.</returns>
        ObjectQuery<ContactInfo> Translate(MacroRuleInstance macroRuleInstance);
    }
}