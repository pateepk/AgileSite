namespace CMS.FormEngine
{
    /// <summary>
    /// Field macro resolving control types.
    /// </summary>
    public enum FormResolveTypeEnum
    {
        /// <summary>
        /// Disable macro resolving.
        /// </summary>
        None,

        /// <summary>
        /// Resolves all macros.
        /// </summary>
        AllFields,

        /// <summary>
        /// Resolves only macros in visible fields.
        /// </summary>
        Visible,

        /// <summary>
        /// Resolves only macros in hidden fields.
        /// </summary>
        Hidden,

        /// <summary>
        /// Resolves macros in widget's visible fields. Only for widgets!
        /// </summary>
        WidgetVisible
    }
}