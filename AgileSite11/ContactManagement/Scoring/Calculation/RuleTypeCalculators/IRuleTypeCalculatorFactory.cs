namespace CMS.ContactManagement
{
    internal interface IRuleTypeCalculatorFactory
    {
        /// <summary>
        /// Creates rule calculator which is able to recalculate rules of the given type.
        /// </summary>
        /// <param name="ruleType">Created calculator will be able to recalculate points of rules of this types</param>
        /// <returns>Created rule calculator</returns>
        IRuleTypeCalculator GetRuleCalculator(RuleTypeEnum ruleType);
    }
}