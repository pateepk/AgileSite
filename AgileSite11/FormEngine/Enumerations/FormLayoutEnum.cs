using CMS.Helpers;

namespace CMS.FormEngine
{
    /// <summary>
    /// Form layout enumeration - for default form layout.
    /// </summary>
    public enum FormLayoutEnum
    {
        /// <summary>
        /// Single table layout - Single table, categories as rows.
        /// </summary>
        [EnumDefaultValue]
        [EnumStringRepresentation("Standard")]
        SingleTable,

        /// <summary>
        /// Tables layout - Each category has its own table.
        /// </summary>
        [EnumStringRepresentation("Tables")]
        TablePerCategory,

        /// <summary>
        /// Collapsible tables - Each category has its own collapsible fieldset.
        /// </summary>
        [EnumStringRepresentation("FieldSets")]
        FieldSets,

        /// <summary>
        /// Divs - Layout without tables.
        /// </summary>
        [EnumStringRepresentation("Divs")]
        Divs
    }
}