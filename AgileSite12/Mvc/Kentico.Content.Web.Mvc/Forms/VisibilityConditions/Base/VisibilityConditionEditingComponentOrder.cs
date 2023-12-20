namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Contains constants describing order of editable properties within <see cref="VisibilityCondition"/>.
    /// </summary>
    public static class VisibilityConditionEditingComponentOrder
    {
        /// <summary>
        /// Order of comparison type editing component. Returns <c>-100</c>.
        /// </summary>
        public const int COMPARISON_TYPE = -100;


        /// <summary>
        /// Order of compare to value editing component. Returns <c>-90</c>.
        /// </summary>
        public const int COMPARE_TO_VALUE = -90;
    }
}
