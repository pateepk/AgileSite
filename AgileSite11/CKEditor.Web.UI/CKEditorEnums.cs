namespace CMS.CKEditor.Web.UI
{
    /// <summary>
    /// Language direction enumeration.
    /// </summary>
    public enum LanguageDirection
    {
        /// <summary>
        /// Left to right.
        /// </summary>
        LeftToRight = 0,

        /// <summary>
        /// Right to left.
        /// </summary>
        RightToLeft = 1,

        /// <summary>
        /// Browser detection.
        /// </summary>
        Ui = 2
    }


    /// <summary>
    /// Enter mode enumeration.
    /// </summary>
    public enum EnterMode
    {
        /// <summary>
        /// Paragraph.
        /// </summary>
        P = 0,

        /// <summary>
        /// Break line.
        /// </summary>
        BR = 1,

        /// <summary>
        /// Div block.
        /// </summary>
        DIV = 2
    }


    /// <summary>
    /// Toolbar location enumeration.
    /// </summary>
    public enum ToolbarLocation
    {
        /// <summary>
        /// Top.
        /// </summary>
        Top = 0,

        /// <summary>
        /// Bottom.
        /// </summary>
        Bottom = 1
    }


    /// <summary>
    /// Resize direction enumeration.
    /// </summary>
    public enum ResizeDirection
    {
        /// <summary>
        /// Vertical.
        /// </summary>
        Vertical = 0,

        /// <summary>
        /// Horizontal.
        /// </summary>
        Horizontal = 1,

        /// <summary>
        /// Vertical and horizontal.
        /// </summary>
        Both = 2
    }


    /// <summary>
    /// Scayt more enumeration.
    /// </summary>
    public enum ScaytMoreSuggestions
    {
        /// <summary>
        /// Scayt more suggestions disabled.
        /// </summary>
        Off = 0,

        /// <summary>
        /// Scayt more suggestions enabled.
        /// </summary>
        On = 1
    }


    /// <summary>
    /// Dialog buttons order enumeration.
    /// </summary>
    public enum DialogButtonsOrder
    {
        /// <summary>
        /// Browser detection.
        /// </summary>
        OS = 0,

        /// <summary>
        /// Right to left.
        /// </summary>
        Rtl = 1,

        /// <summary>
        /// Left to right.
        /// </summary>
        Ltr = 2
    }
}