using CMS.Helpers;

namespace CMS.Search
{
    /// <summary>
    /// Search filter mode enumeration.
    /// </summary>
    public enum SearchFilterModeEnum
    {
        /// <summary>
        /// Dropdown list.
        /// </summary>
        [EnumDefaultValue]
        [EnumStringRepresentation("dropdownlist")]
        DropdownList = 0,

        /// <summary>
        /// Checkbox.
        /// </summary>
        [EnumStringRepresentation("checkbox")]
        Checkbox = 1,

        /// <summary>
        /// Radion button.
        /// </summary>
        [EnumStringRepresentation("radiobutton")]
        RadioButton = 2,

        /// <summary>
        /// Text box
        /// </summary>
        [EnumStringRepresentation("textbox")]
        TextBox = 3
    }
}