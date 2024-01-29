using System;

using CMS.Base;
using CMS.DataEngine;
using CMS.MacroEngine;

namespace CMS.ContactManagement.Internal
{
    /// <summary>
    /// Defines methods needed to translate macro rule instance of one particular macro rule type to data query.
    /// </summary>
    public abstract class SingleMacroRuleInstanceTranslatorBase : IMacroRuleInstanceTranslator
    {
        /// <summary>
        /// Translates one rule to object query returning contacts. Is used to speed up the process of recalculating contact groups.
        /// </summary>
        /// <param name="macroRuleInstance">Instance of the macro rule representing one line in the condition builder.</param>
        /// <returns>Object query for contacts that fit in this rule.</returns>
        public ObjectQuery<ContactInfo> Translate(MacroRuleInstance macroRuleInstance)
        {
            if (macroRuleInstance == null)
            {
                throw new ArgumentNullException(nameof(macroRuleInstance));
            }

            return TranslateInternal(macroRuleInstance.Parameters);
        }


        /// <summary>
        /// Translates one rule to object query returning contacts. Is used to speed up the process of recalculating contact groups. 
        /// </summary>
        /// <param name="ruleParameters">Macro rule parameters representing one line in the condition builder.</param>
        /// <returns>Object query for contacts that fit in this rule.</returns>
        protected abstract ObjectQuery<ContactInfo> TranslateInternal(StringSafeDictionary<MacroRuleParameter> ruleParameters);
    }
}
