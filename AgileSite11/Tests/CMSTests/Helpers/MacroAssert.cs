using CMS.MacroEngine;

using NUnit.Framework;

namespace CMS.Tests
{
    /// <summary>
    ///  Custom macro assertions
    /// </summary>
    public static class MacroAssert
    {
        /// <summary>
        /// Resolves the macro condition using the selected resolver and compares the result with the expected value.
        /// </summary>
        /// <param name="expectedValue">Expected value</param>
        /// <param name="macro">Macro condition to resolve</param>
        /// <param name="resolver">MacroResolver to use</param>
        /// <param name="context">Evaluation context</param>
        /// <remarks>
        /// Fails if some error occurred during macro resolution  
        /// </remarks>
        public static void MacroReturnsExpectedValue(string expectedValue, string macro, MacroResolver resolver, EvaluationContext context = null)
        {
            string result = resolver.ResolveMacros(macro, context);

            CMSAssert.All(
            () => Assert.IsNull(resolver.LastError, "Macro resolved with the following error: " + resolver.LastError),
            () => Assert.AreEqual(expectedValue, result));
        }


        /// <summary>
        /// Resolves the data macro expression (expects expression without {% %} brackets) 
        /// using the selected resolver and compares the result with the expected value. 
        /// </summary>
        /// <param name="expectedValue">Expected value</param>
        /// <param name="resolver">MacroResolver to use</param>
        /// <param name="settings">Settings of the resolving process</param>
        /// <remarks>
        /// Fails if some error occurred during macro resolution  
        /// </remarks>
        public static void MacroReturnsExpectedValueWithResolveSettings(string expectedValue, MacroResolver resolver, ResolveExpressionSettings settings)
        {
            EvaluationResult result = resolver.ResolveMacroExpression(settings);

            CMSAssert.All(
            () => Assert.IsNull(resolver.LastError, "Macro resolved with the following error: " + resolver.LastError),
            () => Assert.AreEqual(expectedValue, result.ToString()));
        }

    }
}