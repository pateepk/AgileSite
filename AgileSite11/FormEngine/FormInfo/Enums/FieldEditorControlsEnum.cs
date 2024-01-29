using CMS.Helpers;

namespace CMS.FormEngine
{
    /// <summary>
    /// Type of custom controls that can be selected from the control list in FieldEditor.
    /// </summary>
    public enum FieldEditorControlsEnum
    {
        /// <summary>
        /// No controls are displayed.
        /// </summary>
        [EnumStringRepresentation("None")]
        None = 0,

        /// <summary>
        /// All controls are displayed.
        /// </summary>
        [EnumStringRepresentation("All")]
        [EnumDefaultValue]
        All = 1,

        /// <summary>
        /// Controls are displayed according to the FieldEditor mode.
        /// </summary>
        [EnumStringRepresentation("ModeSelected")]
        ModeSelected = 2,

        /// <summary>
        /// Controls that are displayed in Bizforms.
        /// </summary>
        [EnumStringRepresentation("Bizforms")]
        Bizforms = 3,

        /// <summary>
        /// Controls that are displayed in document types.
        /// </summary>
        [EnumStringRepresentation("DocumentTypes")]
        DocumentTypes = 4,

        /// <summary>
        /// Controls that are displayed in system tables.
        /// </summary>
        [EnumStringRepresentation("SystemTables")]
        SystemTables = 5,

        /// <summary>
        /// Controls that are displayed in controls.
        /// </summary>
        [EnumStringRepresentation("Controls")]
        Controls = 6,

        /// <summary>
        /// Controls that are displayed in reports.
        /// </summary>
        [EnumStringRepresentation("Reports")]
        Reports = 7,

        /// <summary>
        /// Controls that are displayed in custom tables.
        /// </summary>
        [EnumStringRepresentation("CustomTables")]
        CustomTables = 8,

        /// <summary>
        /// Controls that are used for user visibility.
        /// </summary>
        [EnumStringRepresentation("Visibility")]
        Visibility = 9
    }
}