namespace CMS.MacroEngine
{
    /// <summary>
    /// Interface for resolver objects.
    /// </summary>
    public interface IMacroResolver
    {
        /// <summary>
        /// Resolves all supported macro types in the given text within specified resolving context.
        /// </summary>
        /// <param name="text">Input text with macros to be resolved</param>
        /// <param name="settings">Macro context to be used for resolving (if null, context of the resolver is used)</param>
        string ResolveMacros(string text, MacroSettings settings = null);

        /// <summary>
        /// Resolves the data macro expression (expects expression without {% %} brackets).
        /// </summary>
        /// <param name="settings">Settings of the resolving process</param>
        EvaluationResult ResolveMacroExpression(ResolveExpressionSettings settings);

        /// <summary>
        /// Checks all the data sources for the value. Returns true if given data member was found within supported data sources.
        /// </summary>
        /// <param name="expression">Data member to look for</param>
        /// <param name="context">Evaluation context</param>
        EvaluationResult CheckDataSources(string expression, EvaluationContext context);

        /// <summary>
        /// Gets the object value at given index (this is called when indexer [(int)] is used in the expression).
        /// </summary>
        /// <param name="obj">Source object to get the index-th value from</param>
        /// <param name="index">Index of the item to get</param>
        /// <param name="context">Evaluation context</param>
        EvaluationResult GetObjectValue(object obj, int index, EvaluationContext context);

        /// <summary>
        /// Gets the object value of specified name.
        /// </summary>
        /// <param name="obj">Source object to get the index-th value from</param>
        /// <param name="columnName">Name of the value to get</param>
        /// <param name="context">Evaluation context</param>
        EvaluationResult GetObjectValue(object obj, string columnName, EvaluationContext context);
    }
}
