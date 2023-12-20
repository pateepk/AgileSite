using System;
using System.Linq;
using System.Text;

using CMS.DataEngine;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Translates macro rule instance to data query. Translation methods are hardcoded in implementations of <see cref="IMacroRuleInstanceTranslator"/> interface. 
    /// </summary>
    internal class MacroRuleInstanceToDataQueryTranslator : IMacroRuleInstanceToDataQueryTranslator
    {
        public ObjectQuery<ContactInfo> Translate(MacroRuleInstance macroRuleInstance)
        {
            if (macroRuleInstance == null)
            {
                throw new ArgumentNullException("macroRuleInstance");
            }

            if (!MacroRuleMetadataContainer.IsTranslatorAvailable(macroRuleInstance.MacroRuleName))
            {
                throw new InvalidOperationException("[MacroRuleInstanceToDataQueryTranslator.Translate]: Macro rule '" + macroRuleInstance.MacroRuleName + "' cannot be translated to DataQuery");
            }

            return MacroRuleMetadataContainer.GetMetadata(macroRuleInstance.MacroRuleName).Translator.Translate(macroRuleInstance);
        }


        public bool CanBeTranslated(string ruleName)
        {
            if (ruleName == null)
            {
                throw new ArgumentNullException("ruleName");
            }

            return MacroRuleMetadataContainer.IsTranslatorAvailable(ruleName);
        }
    }
}