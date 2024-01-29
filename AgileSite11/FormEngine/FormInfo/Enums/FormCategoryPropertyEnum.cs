using CMS.Helpers;

namespace CMS.FormEngine
{
    /// <summary>
    /// Form category info property types.
    /// </summary>
    public enum FormCategoryPropertyEnum
    {
        /// <summary>
        /// Category caption.
        /// </summary>
        [EnumStringRepresentation("caption")]
        Caption,


        /// <summary>
        /// Indicates if category is collapsible.
        /// </summary>
        [EnumStringRepresentation("collapsible")]
        Collapsible,


        /// <summary>
        /// Indicates if category is collapsed by default.
        /// </summary>
        [EnumStringRepresentation("collapsedbydefault")]
        CollapsedByDefault,


        /// <summary>
        /// Indicates if category is visible.
        /// </summary>
        [EnumStringRepresentation("visible")]
        Visible
    }
}