using System;

using CMS.Helpers;

namespace CMS.UIControls
{
    /// <summary>
    /// Field editor data mode.
    /// </summary>
    public enum FieldEditorModeEnum
    {
        /// <summary>
        /// Class form definition.
        /// </summary> 
        [EnumStringRepresentation("ClassFormDefinition")]
        ClassFormDefinition = 1,

        /// <summary>
        /// Webpart properties.
        /// </summary>
        [EnumStringRepresentation("WebPartProperties")]
        WebPartProperties = 2,

        /// <summary>
        /// System table.
        /// </summary>
        [EnumStringRepresentation("SystemTable")]
        SystemTable = 3,

        /// <summary>
        /// BizForm definition.
        /// </summary>
        [EnumStringRepresentation("BizFormDefinition")]
        BizFormDefinition = 4,

        /// <summary>
        /// General mode that uses XML definition only.
        /// </summary>
        [EnumStringRepresentation("General")]
        [EnumDefaultValue]
        General = 5,

        /// <summary>
        /// Custom table.
        /// </summary>
        [EnumStringRepresentation("CustomTable")]
        CustomTable = 7,

        /// <summary>
        /// Form controls.
        /// </summary>
        [EnumStringRepresentation("FormControls")]
        FormControls = 8,

        /// <summary>
        /// Widget properties.
        /// </summary>
        [EnumStringRepresentation("Widget")]
        Widget = 9,

        /// <summary>
        /// Alternative class form definition.
        /// </summary> 
        [EnumStringRepresentation("AlternativeClassFormDefinition")]
        AlternativeClassFormDefinition = 10,

        /// <summary>
        /// Alternative system table.
        /// </summary>
        [EnumStringRepresentation("AlternativeSystemTable")]
        AlternativeSystemTable = 11,

        /// <summary>
        /// Alternative BizForm definition.
        /// </summary>
        [EnumStringRepresentation("AlternativeBizFormDefinition")]
        AlternativeBizFormDefinition = 12,

        /// <summary>
        /// Alternative custom table.
        /// </summary>
        [EnumStringRepresentation("AlternativeCustomTable")]
        AlternativeCustomTable = 13,

        /// <summary>
        /// Alternative form control.
        /// </summary>
        [EnumStringRepresentation("InheritedFormControl")]
        InheritedFormControl = 14,

        /// <summary>
        /// Inherited webpart properties.
        /// </summary>
        [EnumStringRepresentation("InheritedWebPartProperties")]
        InheritedWebPartProperties = 15,

        /// <summary>
        /// System webpart properties.
        /// </summary>
        [EnumStringRepresentation("SystemWebPartProperties")]
        SystemWebPartProperties = 16,

        /// <summary>
        /// Page template properties.
        /// </summary>
        [EnumStringRepresentation("PageTemplateProperties")]
        PageTemplateProperties = 17,

        /// <summary>
        /// Process action properties (used in workflow and marketing automation actions).
        /// </summary>
        [EnumStringRepresentation("ProcessActions")]
        ProcessActions = 18,
    }
}