namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Provides method to evaluate widget variant which will be used for widget content rendering.
    /// </summary>
    internal interface IWidgetVariantEvaluator
    {
        /// <summary>
        /// Evaluates widget variants to select the one used for widget content rendering.
        /// </summary>
        /// <param name="widget">Widget configuration.</param>
        /// <returns>Widget variant to be used for widget content rendering.</returns>
        WidgetVariantConfiguration Evaluate(WidgetConfiguration widget);
    }
}
