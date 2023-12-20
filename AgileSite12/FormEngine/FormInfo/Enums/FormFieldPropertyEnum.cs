using CMS.Helpers;

namespace CMS.FormEngine
{
    /// <summary>
    /// Form field info property types.
    /// </summary>
    public enum FormFieldPropertyEnum
    {
        /// <summary>
        /// Field default value.
        /// </summary>
        [EnumStringRepresentation("defaultvalue")]
        DefaultValue,


        /// <summary>
        /// Field caption.
        /// </summary>
        [EnumStringRepresentation("fieldcaption")]
        FieldCaption,


        /// <summary>
        /// Field description.
        /// </summary>
        [EnumStringRepresentation("fielddescription")]
        FieldDescription,


        /// <summary>
        /// Explanation text displayed bellow the field.
        /// </summary>
        [EnumStringRepresentation("explanationtext")]
        ExplanationText,


        /// <summary>
        /// Validation error message.
        /// </summary>
        [EnumStringRepresentation("validationerrormessage")]
        ValidationErrorMessage,


        /// <summary>
        /// Caption style.
        /// </summary>
        [EnumStringRepresentation("captionstyle")]
        CaptionStyle,


        /// <summary>
        /// Caption css class.
        /// </summary>
        [EnumStringRepresentation("captioncssclass")]
        CaptionCssClass,


        /// <summary>
        /// Caption cell css class.
        /// </summary>
        [EnumStringRepresentation("captioncellcssclass")]
        CaptionCellCssClass,


        /// <summary>
        /// Input control style.
        /// </summary>
        [EnumStringRepresentation("inputcontrolstyle")]
        InputControlStyle,


        /// <summary>
        /// Control css class.
        /// </summary>
        [EnumStringRepresentation("controlcssclass")]
        ControlCssClass,


        /// <summary>
        /// Control cell css class.
        /// </summary>
        [EnumStringRepresentation("controlcellcssclass")]
        ControlCellCssClass,


        /// <summary>
        /// Field css class.
        /// </summary>
        [EnumStringRepresentation("fieldcssclass")]
        FieldCssClass,


        /// <summary>
        /// Content before.
        /// </summary>
        [EnumStringRepresentation("contentbefore")]
        ContentBefore,


        /// <summary>
        /// Content after.
        /// </summary>
        [EnumStringRepresentation("contentafter")]
        ContentAfter,


        /// <summary>
        /// Visibility macro.
        /// </summary>
        [EnumStringRepresentation("visiblemacro")]
        VisibleMacro,


        /// <summary>
        /// Enabled macro.
        /// </summary>
        [EnumStringRepresentation("enabledmacro")]
        EnabledMacro,

        /// <summary>
        /// Smart field.
        /// </summary>
        [EnumStringRepresentation("smart")]
        Smart
    }
}