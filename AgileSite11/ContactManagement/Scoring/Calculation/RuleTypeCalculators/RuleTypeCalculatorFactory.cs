using System;

using CMS;
using CMS.ContactManagement;

[assembly: RegisterImplementation(typeof(IRuleTypeCalculatorFactory), typeof(RuleTypeCalculatorFactory), Priority = CMS.Core.RegistrationPriority.SystemDefault)]

namespace CMS.ContactManagement
{
    /// <summary>
    /// Factory class for IRuleTypeCalculator.
    /// </summary>
    internal class RuleTypeCalculatorFactory : IRuleTypeCalculatorFactory
    {
        /// <summary>
        /// Creates rule calculator which is able to recalculate rules of the given type.
        /// </summary>
        /// <param name="ruleType">Created calculator will be able to recalculate points of rules of this types</param>
        /// <returns>Created rule calculator</returns>
        public virtual IRuleTypeCalculator GetRuleCalculator(RuleTypeEnum ruleType)
        {
            switch (ruleType)
            {
                case RuleTypeEnum.Attribute:
                    return new AttributeRuleTypeCalculator();
                case RuleTypeEnum.Activity:
                    return new ActivityRuleTypeCalculator();
                case RuleTypeEnum.Macro:
                    return new MacroRuleTypeCalculator();
                default:
                    throw new NotSupportedException();
            }
        }
    }
}