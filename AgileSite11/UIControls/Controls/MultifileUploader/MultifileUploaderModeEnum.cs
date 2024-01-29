namespace CMS.UIControls
{
    /// <summary>
    /// Specifying multi-file uploader upload modes.
    /// </summary>
    public enum MultifileUploaderModeEnum
    {
        /// <summary>
        /// In this mode default upload grid is displayed and no restriction parameters are overridden.
        /// </summary>
        Grid = 0,


        /// <summary>
        /// In this mode only one button is displayed, and upload starts automatically after the selection of multiple files.
        /// </summary>
        DirectMultiple = 1,


        /// <summary>
        /// In this mode only one button is displayed, and upload starts automatically after the selection of a single file.
        /// </summary>
        DirectSingle = 2
    }
}