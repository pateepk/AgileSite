using CMS.Helpers;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Type of control to select product options.
    /// </summary>
    public enum OptionCategorySelectionTypeEnum
    {
        /// <summary>
        /// DropDownList.
        /// </summary>
        [EnumDefaultValue]
        [EnumStringRepresentation("DROPDOWN")]
        Dropdownlist = 0,

        /// <summary>
        /// RadioButtons in vertical layout.
        /// </summary>
        [EnumStringRepresentation("RADIOBUTTONVERTICAL")]
        RadioButtonsVertical = 1,

        /// <summary>
        /// RadioButtons in horizontal layout.
        /// </summary>
        [EnumStringRepresentation("RADIOBUTTONHORIZONTAL")]
        RadioButtonsHorizontal = 2,

        /// <summary>
        /// Checkboxes in vertical layout.
        /// </summary>
        [EnumStringRepresentation("CHECKBOXVERTICAL")]
        CheckBoxesVertical = 3,

        /// <summary>
        /// Checkboxes in horizontal layout.
        /// </summary>
        [EnumStringRepresentation("CHECKBOXHORIZONTAL")]
        CheckBoxesHorizontal = 4,

        /// <summary>
        /// Text box.
        /// </summary>
        [EnumStringRepresentation("TEXTBOX")]
        TextBox = 5,

        /// <summary>
        /// Text area.
        /// </summary>
        [EnumStringRepresentation("TEXTAREA")]
        TextArea = 6
    }
}