using System;
using System.Collections.Generic;
using System.Linq;

using CMS.DataEngine;
using CMS.MacroEngine;

namespace CMS.ContactManagement
{
    internal class MacroRuleTreeToDataQueryTranslator : IMacroRuleTreeToDataQueryTranslator
    {
        private readonly IMacroRuleInstanceToDataQueryTranslator mInstanceTranslator;


        public MacroRuleTreeToDataQueryTranslator(IMacroRuleInstanceToDataQueryTranslator instanceTranslator)
        {
            if (instanceTranslator == null)
            {
                throw new ArgumentNullException("instanceTranslator");
            }

            mInstanceTranslator = instanceTranslator;
        }


        public ObjectQuery<ContactInfo> TranslateWithTransformation(MacroRuleTree macroRuleTreeRoot, Func<ObjectQuery<ContactInfo>, ObjectQuery<ContactInfo>> transformation)
        {
            if (macroRuleTreeRoot == null)
            {
                throw new ArgumentNullException("macroRuleTreeRoot");
            }
            if (transformation == null)
            {
                throw new ArgumentNullException("transformation");
            }

            if (macroRuleTreeRoot.RuleName != null)
            {
                var instance = new MacroRuleInstance()
                {
                    MacroRuleName = macroRuleTreeRoot.RuleName,
                    Parameters = macroRuleTreeRoot.Parameters,
                };

                return transformation(mInstanceTranslator.Translate(instance));
            }

            IEnumerable<ObjectQuery<ContactInfo>> childrenQueries = macroRuleTreeRoot.Children.Select(macroRule => TranslateWithTransformation(macroRule, transformation));

            // The first child has to be skipped when creating operators, because it has operator from the lower level
            IEnumerable<string> operators = macroRuleTreeRoot.Children.Skip(1).Select(tree =>
            {
                switch (tree.Operator)
                {
                    case "&&":
                        return SqlOperator.INTERSECT;
                    case "||":
                        return SqlOperator.UNION;
                    default:
                        throw new Exception("[MacroRuleTreeToDataQueryTranslator]: Unknown operator");
                }
            });
            
            return DataQuery.Combine(childrenQueries, operators, DatabaseSeparationHelper.OM_CONNECTION_STRING);
        }


        public bool CanBeTranslated(MacroRuleTree macroRuleTreeRoot)
        {
            if (macroRuleTreeRoot == null)
            {
                throw new ArgumentNullException("macroRuleTreeRoot");
            }

            if (macroRuleTreeRoot.RuleName != null)
            {
                return mInstanceTranslator.CanBeTranslated(macroRuleTreeRoot.RuleName);
            }

            return macroRuleTreeRoot.Children.All(CanBeTranslated);
        }
    }
}