namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Contains constants describing order of editable properties within <see cref="FormComponentProperties"/>.
    /// </summary>
    public static class EditingComponentOrder
    {
        /// <summary>
        /// Order of label editing component. Returns <c>-120</c>.
        /// </summary>
        public const int LABEL = -120;


        /// <summary>
        /// Order of required flag editing component. Returns <c>-110</c>.
        /// </summary>
        public const int REQUIRED = -110;


        /// <summary>
        /// Order of smart field flag editing component. Returns <c>-100</c>.
        /// </summary>
        public const int SMART_FIELD = -100;


        /// <summary>
        /// Order of name editing component. Returns <c>-90</c>.
        /// </summary>
        public const int NAME = -90;


        /// <summary>
        /// Order of default value editing component. Returns <c>-80</c>.
        /// </summary>
        public const int DEFAULT_VALUE = -80;


        /// <summary>
        /// Order of explanation text editing component. Returns <c>-70</c>.
        /// </summary>
        public const int EXPLANATION_TEXT = -70;


        /// <summary>
        /// Order of tooltip editing component. Returns <c>-60</c>.
        /// </summary>
        public const int TOOLTIP = -60;
    }
}
