namespace CMS.Base.Web.UI
{
    ///<summary>Message type enumeration.</summary>
    public enum MessageTypeEnum : int
    {
        ///<summary>
        /// Information message.
        ///</summary>
        Information = 0,

        ///<summary>
        /// Confirmation message.
        ///</summary>
        Confirmation = 1,

        ///<summary>
        /// Warning message.
        ///</summary>
        Warning = 2,

        /// <summary>
        /// Error message.
        /// </summary>
        Error = 3,
    }
}